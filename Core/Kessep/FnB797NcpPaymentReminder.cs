// Program: FN_B797_NCP_PAYMENT_REMINDER, ID: 1902545933, model: 746.
// Short name: SWEF797B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B797_NCP_PAYMENT_REMINDER.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB797NcpPaymentReminder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B797_NCP_PAYMENT_REMINDER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB797NcpPaymentReminder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB797NcpPaymentReminder.
  /// </summary>
  public FnB797NcpPaymentReminder(IContext context, Import import, Export export)
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
    // 05/23/16  DDupree	CQ52085		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // In an effort to increase child support collections a yearly payment 
    // reminder letter informing all NCPs current monthly obligation, balance
    // owed as of end of the previous month, total payments for the last 12
    // months.
    // This program will be scheduled to run once a year and will produce the 
    // the letters that will be printed by the mainframe printer.
    // For each NCP, non-secondary current support and arrears debt balances 
    // will be calculated by court order.  To provide consistency, the OPAY
    // display action blocks will be used to calculate the balances per
    // obligation and then summarized by court order.  Separate letters will be
    // written for each NCP court order.
    // To document that the letter was sent to the NCP, a HIST record will be 
    // created for each case where the NCP is the currently active AP.
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
    local.Open.Action = "OPEN";
    local.Close.Action = "CLOSE";
    local.NumberOfLinesPerPage.Count = 57;
    local.NumberOfLinesPerHeader.Count = 13;

    // -------------------------------------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabReportSend.ReportNumber = 1;
    UseSpEabWriteDocument3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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

    local.RestartNcp.Number = "";
    local.TotalNumbOfNcpsWritten.Count = 0;
    local.TotalNumbOfLetters.Count = 0;
    local.EndDate.Date = local.ProgramProcessingInfo.ProcessDate;
    UseCabDate2TextWithHyphens1();
    local.Spaddr1.Street = "PO Box 497";
    local.Spaddr2.Street = "Topeka, KS  66601";
    local.Spaddr3.Street = "";

    // -------------------------------------------------------------------------------------
    // -- Determine the number of months in the collection period.
    // -------------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 3)))
    {
      // -- Default is 12 months.
      local.NumberOfMonths.Count = 12;
    }
    else
    {
      // -- Use value in first 3 characters of MPPI parameter.
      local.NumberOfMonths.Count =
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
    // -- Determine the first day of the month following the ppi processing 
    // date.
    // -- The NCP must have a debt due prior to this date.
    // -------------------------------------------------------------------------------------
    local.FirstDayOfMonth.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));
    local.EndDate.Date = AddDays(local.FirstDayOfMonth.Date, -1);
    local.Start.Date =
      AddMonths(local.FirstDayOfMonth.Date, -local.NumberOfMonths.Count);
    local.FirstDayOfMonth.Date = AddMonths(local.FirstDayOfMonth.Date, 1);

    // -------------------------------------------------------------------------------------
    // -- Determine Arrears Through Month and Current Month.
    // -------------------------------------------------------------------------------------
    switch(Month(local.ProgramProcessingInfo.ProcessDate))
    {
      case 1:
        local.CurrentMonth.Text11 = "January";

        break;
      case 2:
        local.CurrentMonth.Text11 = "February";

        break;
      case 3:
        local.CurrentMonth.Text11 = "March";

        break;
      case 4:
        local.CurrentMonth.Text11 = "April";

        break;
      case 5:
        local.CurrentMonth.Text11 = "May";

        break;
      case 6:
        local.CurrentMonth.Text11 = "June";

        break;
      case 7:
        local.CurrentMonth.Text11 = "July";

        break;
      case 8:
        local.CurrentMonth.Text11 = "August";

        break;
      case 9:
        local.CurrentMonth.Text11 = "September";

        break;
      case 10:
        local.CurrentMonth.Text11 = "September";

        break;
      case 11:
        local.CurrentMonth.Text11 = "November";

        break;
      case 12:
        local.CurrentMonth.Text11 = "December";

        break;
      default:
        break;
    }

    switch(Month(local.EndDate.Date))
    {
      case 1:
        local.ArrearsToMonth.Text11 = "January";

        break;
      case 2:
        local.ArrearsToMonth.Text11 = "February";

        break;
      case 3:
        local.ArrearsToMonth.Text11 = "March";

        break;
      case 4:
        local.ArrearsToMonth.Text11 = "April";

        break;
      case 5:
        local.ArrearsToMonth.Text11 = "May";

        break;
      case 6:
        local.ArrearsToMonth.Text11 = "June";

        break;
      case 7:
        local.ArrearsToMonth.Text11 = "July";

        break;
      case 8:
        local.ArrearsToMonth.Text11 = "August";

        break;
      case 9:
        local.ArrearsToMonth.Text11 = "September";

        break;
      case 10:
        local.ArrearsFromMonth.Text11 = "September";

        break;
      case 11:
        local.ArrearsToMonth.Text11 = "November";

        break;
      case 12:
        local.ArrearsToMonth.Text11 = "December";

        break;
      default:
        break;
    }

    switch(Month(local.Start.Date))
    {
      case 1:
        local.ArrearsFromMonth.Text11 = "January";

        break;
      case 2:
        local.ArrearsFromMonth.Text11 = "February";

        break;
      case 3:
        local.ArrearsFromMonth.Text11 = "March";

        break;
      case 4:
        local.ArrearsFromMonth.Text11 = "April";

        break;
      case 5:
        local.ArrearsFromMonth.Text11 = "May";

        break;
      case 6:
        local.ArrearsFromMonth.Text11 = "June";

        break;
      case 7:
        local.ArrearsFromMonth.Text11 = "July";

        break;
      case 8:
        local.ArrearsFromMonth.Text11 = "August";

        break;
      case 9:
        local.ArrearsFromMonth.Text11 = "September";

        break;
      case 10:
        local.ArrearsFromMonth.Text11 = "October";

        break;
      case 11:
        local.ArrearsFromMonth.Text11 = "November";

        break;
      case 12:
        local.ArrearsFromMonth.Text11 = "December";

        break;
      default:
        break;
    }

    local.Year.Year = Year(local.ProgramProcessingInfo.ProcessDate);
    local.Year.Month = Month(local.ProgramProcessingInfo.ProcessDate);
    local.Year.Day = Day(local.ProgramProcessingInfo.ProcessDate);
    local.CurentYr.Text4 = NumberToString(local.Year.Year, 12, 4);
    local.ToDay.Text2 = NumberToString(local.Year.Day, 14, 2);
    local.CurrentDateWorkArea.Text20 = TrimEnd(local.CurrentMonth.Text11) + " " +
      local.ToDay.Text2 + ", " + local.CurentYr.Text4;
    local.Year.Year = Year(local.EndDate.Date);
    local.Year.Month = Month(local.EndDate.Date);
    local.Year.Day = Day(local.EndDate.Date);
    local.CurentYr.Text4 = NumberToString(local.Year.Year, 12, 4);
    local.ToDay.Text2 = NumberToString(local.Year.Day, 14, 2);
    local.To.TextMonth = NumberToString(local.Year.Month, 14, 2);
    local.DateObliation.Text30 = TrimEnd(local.CurrentMonth.Text11) + " " + local
      .CurentYr.Text4;
    local.DateArrears.Text30 = "As of " + TrimEnd
      (local.ArrearsToMonth.Text11) + " " + local.ToDay.Text2 + ", " + local
      .CurentYr.Text4;
    local.Year.Year = Year(local.Start.Date);
    local.Year.Month = Month(local.Start.Date);
    local.Year.Day = Day(local.Start.Date);
    local.Previous.Text4 = NumberToString(local.Year.Year, 12, 4);
    local.FromDay.Text2 = NumberToString(local.Year.Day, 14, 2);
    local.From.TextMonth = NumberToString(local.Year.Month, 14, 2);
    local.CollectionPeriod.Text30 = TrimEnd(local.From.TextMonth) + "/" + local
      .FromDay.Text2 + "/" + local.Previous.Text4 + " - " + TrimEnd
      (local.To.TextMonth) + "/" + local.ToDay.Text2 + "/" + local
      .CurentYr.Text4;

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
            "Number of Months during collection period...." + NumberToString
            (local.NumberOfMonths.Count, 9, 7);

          break;
        case 3:
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail =
            "NCP Payment Collection Date Cutoff............................." +
            local.TextDate.Text10;

          break;
        case 5:
          UseCabDate2TextWithHyphens3();
          local.EabReportSend.RptDetail =
            "NCP payment collection date began.............................." +
            local.TextDate.Text10;

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "Current Support Month.........................................." +
            local.CurrentMonth.Text11;

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "Arrears Through Month.........................................." +
            local.ArrearsToMonth.Text11;

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
      if (!ReadCsePerson())
      {
        if (AsChar(local.LogExclusions.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Internal error reading NCP ";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        continue;
      }

      local.ActiveCaseFound.Flag = "";

      // -- Skip outgoing interstate cases.
      foreach(var item1 in ReadCase())
      {
        local.ActiveCaseFound.Flag = "Y";

        if (ReadInterstateRequest())
        {
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Outgoing interstate case";
              
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

      if (AsChar(local.ActiveCaseFound.Flag) != 'Y')
      {
        // -- Skip if NCP is not a active NCP on a case.
        if (AsChar(local.LogExclusions.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Not an active NCP on a case";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        continue;
      }

      if (ReadCsePersonAddress())
      {
        if (AsChar(entities.NcpCsePersonAddress.LocationType) == 'F')
        {
          // -- Skip NCPs with foreign addresses.
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Foreign Address";
              
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

      if (!entities.NcpCsePersonAddress.Populated)
      {
        // -- Skip if NCP has no verified address
        if (AsChar(local.LogExclusions.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - No Current Verified Address";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        continue;
      }

      UseSiGetCsePersonMailingAddr();

      if (ReadIncarceration())
      {
        if (!Lt(local.NullDateWorkArea.Date, entities.Incarceration.VerifiedDate))
          
        {
          // -- Skip if NCP has no inmate number but is an inmate
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Incarcerated but not verified";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          continue;
        }

        if (!Lt(local.ProgramProcessingInfo.ProcessDate,
          entities.Incarceration.StartDate) && !
          Lt(entities.Incarceration.EndDate,
          local.ProgramProcessingInfo.ProcessDate))
        {
          // this is a valid record so keep checking it
        }
        else
        {
          // -- Skip if NCP has no inmate number but is an inmate
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Incarcerated but not current";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          continue;
        }

        if (IsEmpty(entities.Incarceration.InmateNumber))
        {
          // -- Skip if NCP has no inmate number but is an inmate
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Incarcerated but no inmate number";
              
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

      // -- Get NCP name from adabase
      local.NcpCsePersonsWorkSet.Number = entities.NcpCsePerson.Number;
      UseSiReadCsePersonBatch();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...NCP " + entities
          .NcpCsePerson.Number + " " + local.ExitStateWorkArea.Message;
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
            local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - Error reading NCP in Adabas";
              
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

      local.Araddr1.Street = "";
      local.Araddr2.Street = "";
      local.Araddr3.Street = "";
      local.Araddr4.Street = "";
      local.Araddr5.Street = "";

      if (!IsEmpty(local.NcpCsePersonsWorkSet.MiddleInitial))
      {
        local.Arnm.FormattedName =
          TrimEnd(local.NcpCsePersonsWorkSet.FirstName) + " " + local
          .NcpCsePersonsWorkSet.MiddleInitial + " " + local
          .NcpCsePersonsWorkSet.LastName;
      }
      else
      {
        local.Arnm.FormattedName =
          TrimEnd(local.NcpCsePersonsWorkSet.FirstName) + " " + local
          .NcpCsePersonsWorkSet.LastName;
      }

      local.Araddr1.Street = local.NcpCsePersonAddress.Street1 ?? Spaces(30);

      if (!IsEmpty(local.NcpCsePersonAddress.Street2))
      {
        local.Araddr2.Street = local.NcpCsePersonAddress.Street2 ?? Spaces(30);

        if (!IsEmpty(local.NcpCsePersonAddress.Street3))
        {
          local.Araddr3.Street = local.NcpCsePersonAddress.Street3 ?? Spaces
            (30);

          if (!IsEmpty(local.NcpCsePersonAddress.Street4))
          {
            local.Araddr4.Street = local.NcpCsePersonAddress.Street4 ?? Spaces
              (30);

            if (!IsEmpty(local.NcpCsePersonAddress.Zip4))
            {
              local.Araddr5.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
                (local.NcpCsePersonAddress.State ?? "") + "  " + (
                  local.NcpCsePersonAddress.ZipCode ?? "") + "-" + (
                  local.NcpCsePersonAddress.Zip4 ?? "");
            }
            else
            {
              local.Araddr5.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
                (local.NcpCsePersonAddress.State ?? "") + " " + (
                  local.NcpCsePersonAddress.ZipCode ?? "");
            }
          }
          else if (!IsEmpty(local.NcpCsePersonAddress.Zip4))
          {
            local.Araddr4.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
              (local.NcpCsePersonAddress.State ?? "") + "  " + (
                local.NcpCsePersonAddress.ZipCode ?? "") + "-" + (
                local.NcpCsePersonAddress.Zip4 ?? "");
          }
          else
          {
            local.Araddr4.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
              (local.NcpCsePersonAddress.State ?? "") + " " + (
                local.NcpCsePersonAddress.ZipCode ?? "");
          }
        }
        else if (!IsEmpty(local.NcpCsePersonAddress.Zip4))
        {
          local.Araddr3.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
            (local.NcpCsePersonAddress.State ?? "") + "  " + (
              local.NcpCsePersonAddress.ZipCode ?? "") + "-" + (
              local.NcpCsePersonAddress.Zip4 ?? "");
        }
        else
        {
          local.Araddr3.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
            (local.NcpCsePersonAddress.State ?? "") + " " + (
              local.NcpCsePersonAddress.ZipCode ?? "");
        }
      }
      else if (!IsEmpty(local.NcpCsePersonAddress.Street3))
      {
        local.Araddr2.Street = local.NcpCsePersonAddress.Street3 ?? Spaces(30);

        if (!IsEmpty(local.NcpCsePersonAddress.Street4))
        {
          local.Araddr3.Street = local.NcpCsePersonAddress.Street4 ?? Spaces
            (30);

          if (!IsEmpty(local.NcpCsePersonAddress.Zip4))
          {
            local.Araddr4.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
              (local.NcpCsePersonAddress.State ?? "") + "  " + (
                local.NcpCsePersonAddress.ZipCode ?? "") + "-" + (
                local.NcpCsePersonAddress.Zip4 ?? "");
          }
          else
          {
            local.Araddr4.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
              (local.NcpCsePersonAddress.State ?? "") + " " + (
                local.NcpCsePersonAddress.ZipCode ?? "");
          }
        }
        else if (!IsEmpty(local.NcpCsePersonAddress.Zip4))
        {
          local.Araddr3.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
            (local.NcpCsePersonAddress.State ?? "") + "  " + (
              local.NcpCsePersonAddress.ZipCode ?? "") + "-" + (
              local.NcpCsePersonAddress.Zip4 ?? "");
        }
        else
        {
          local.Araddr3.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
            (local.NcpCsePersonAddress.State ?? "") + " " + (
              local.NcpCsePersonAddress.ZipCode ?? "");
        }
      }
      else if (!IsEmpty(local.NcpCsePersonAddress.Zip4))
      {
        local.Araddr2.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
          (local.NcpCsePersonAddress.State ?? "") + "  " + (
            local.NcpCsePersonAddress.ZipCode ?? "") + "-" + (
            local.NcpCsePersonAddress.Zip4 ?? "");
      }
      else
      {
        local.Araddr2.Street = TrimEnd(local.NcpCsePersonAddress.City) + ", " +
          (local.NcpCsePersonAddress.State ?? "") + " " + (
            local.NcpCsePersonAddress.ZipCode ?? "");
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
          local.EabReportSend.RptDetail = entities.NcpCsePerson.Number + " - NCP has only secondary debts";
            
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

        local.Pass.StandardNumber = "";
        local.TotalPaymentsCommon.TotalCurrency = 0;

        if (ReadLegalAction())
        {
          local.Pass.StandardNumber = entities.LegalAction.StandardNumber;
          UseOeDeterminePayments();
        }

        // -- Current Support Due
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.Summary.Item.SummaryCurrentOwed.
            TotalCurrency * 100), 15);
        UseEabConvertNumeric1();
        local.CurrentObligations.Text17 = "$" + Substring
          (local.EabConvertNumeric.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength,
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "), 22 -
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "));

        // -- Arrears Balance
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.Summary.Item.SummaryArrearsOwed.
            TotalCurrency * 100), 15);
        UseEabConvertNumeric1();
        local.TotalArrears.Text17 = "$" + Substring
          (local.EabConvertNumeric.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength,
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "), 22 -
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "));

        // -- Total Payments Made
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.TotalPaymentsCommon.TotalCurrency * 100),
          15);
        UseEabConvertNumeric1();
        local.TotalPaymentsWorkArea.Text17 = "$" + Substring
          (local.EabConvertNumeric.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength,
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "), 22 -
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "));
        local.EabFileHandling.Action = "WRITE";
        local.Common.Count = 0;
        local.PrintHeader.Index = -1;
        local.PrintHeader.Count = 0;

        do
        {
          ++local.PrintHeader.Index;
          local.PrintHeader.CheckSize();

          local.Print.RptDetail = "";

          switch(local.PrintHeader.Index + 1)
          {
            case 1:
              // --  Line 3   child support services unit
              local.Print.RptDetail = "                     " + "CHILD SUPPORT SERVICES ADMINISTRATION";
                

              break;
            case 2:
              // --  Line 4   sp address 1
              local.Print.RptDetail = "                     " + local
                .Spaddr1.Street;

              break;
            case 3:
              // --  Line 5  sp address 2 line
              local.Print.RptDetail = "                     " + local
                .Spaddr2.Street;

              break;
            case 4:
              // --  Line 6  sp address 3 line
              local.Print.RptDetail = "                     " + local
                .Spaddr3.Street;

              break;
            case 5:
              // --  Line 7 blank
              local.Print.RptDetail = "";

              break;
            case 6:
              // --  Line 8  spaces
              local.Print.RptDetail = "";

              break;
            case 7:
              // --  Line 9   ar name
              local.Print.RptDetail = "                     " + local
                .Arnm.FormattedName;

              break;
            case 8:
              // --  Line 10  ar address 1
              local.Print.RptDetail = "                     " + local
                .Araddr1.Street;

              break;
            case 9:
              // --  Line 11 ar address 2
              local.Print.RptDetail = "                     " + local
                .Araddr2.Street;

              break;
            case 10:
              // --  Line 12  ar address 3
              local.Print.RptDetail = "                     " + local
                .Araddr3.Street;

              break;
            case 11:
              // --  Line 13 ar address 4
              local.Print.RptDetail = "                     " + local
                .Araddr4.Street;

              break;
            case 12:
              // --  Line 14  ar address 5
              local.Print.RptDetail = "                     " + local
                .Araddr5.Street;

              break;
            case 13:
              // --  Line 256 spaces
              local.Print.RptDetail = "";

              break;
            default:
              break;
          }

          ++local.Common.Count;

          // -- Determine the action to send to the eab report writer. (i.e. do 
          // we need a page break)
          if (local.PrintHeader.Index == 0)
          {
            local.EabReportSend.Command = "NEWPAGE";
          }
          else if (local.Common.Count <= local.NumberOfLinesPerHeader.Count)
          {
            local.EabReportSend.Command = "DETAIL";
          }
          else
          {
            local.EabReportSend.Command = "NEWPAGE";
            local.Common.Count = 1;
          }

          local.EabReportSend.RptDetail = local.Print.RptDetail;
          local.EabReportSend.ReportNumber = 2;
          UseSpEabWriteDocument2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            return;
          }
        }
        while(local.PrintHeader.Index < 12);

        local.EabFileHandling.Action = "WRITE";
        local.Common.Count = 0;
        local.PrintReminder.Index = -1;
        local.PrintReminder.Count = 0;

        do
        {
          ++local.PrintReminder.Index;
          local.PrintReminder.CheckSize();

          local.Print.RptDetail = "";

          switch(local.PrintReminder.Index + 1)
          {
            case 1:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 2:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 3:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 4:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 5:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 6:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 7:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 8:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 9:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 10:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 11:
              // --  Line 1   child support services unit
              local.Print.RptDetail =
                "                     Kansas Child Support Payment Statement";

              break;
            case 12:
              // --  Line 2   blank
              local.Print.RptDetail = "";

              break;
            case 13:
              // --  Line 3  current date
              local.Print.RptDetail =
                "                                                           " +
                local.CurrentDateWorkArea.Text20;

              break;
            case 14:
              // --  Line 4  blank
              local.Print.RptDetail = "";

              break;
            case 15:
              // --  Line 5 blank
              local.Print.RptDetail = "";

              break;
            case 16:
              // --  Line 6   ar name
              local.Print.RptDetail = local.Arnm.FormattedName;

              break;
            case 17:
              // --  Line 7  ar address 1
              local.Print.RptDetail = local.Araddr1.Street;

              break;
            case 18:
              // --  Line 8 ar address 2
              local.Print.RptDetail = local.Araddr2.Street;

              break;
            case 19:
              // --  Line 9  ar address 3
              local.Print.RptDetail = local.Araddr3.Street;

              break;
            case 20:
              // --  Line 10 ar address 4
              local.Print.RptDetail = local.Araddr4.Street;

              break;
            case 21:
              // --  Line 11  ar address 5
              local.Print.RptDetail = local.Araddr5.Street;

              break;
            case 22:
              // --  Line 12  spaces
              local.Print.RptDetail = "";

              break;
            case 23:
              // --  Line 13  court order number
              local.Print.RptDetail = "Regarding Court Order Number  " + (
                local.Pass.StandardNumber ?? "");

              break;
            case 24:
              // --  Line 14 IS SPACES
              local.Print.RptDetail = "";

              break;
            case 25:
              // --  Line 15   columns heading
              local.Print.RptDetail =
                "Date                          Description                    Amount";
                

              break;
            case 26:
              // --  Line 16 current obligation
              local.Print.RptDetail = local.DateObliation.Text30 + "Current CS Monthly Obligation  " +
                local.CurrentObligations.Text17;

              break;
            case 27:
              // --  Line 17 arrears
              local.Print.RptDetail = local.DateArrears.Text30 + "Child Support Owed             " +
                local.TotalArrears.Text17;

              break;
            case 28:
              // --  Line 18 payments
              local.Print.RptDetail = local.CollectionPeriod.Text30 + "Child Support Collected        " +
                local.TotalPaymentsWorkArea.Text17;

              break;
            case 29:
              // --  Line 19 spaces
              local.Print.RptDetail = "";

              break;
            case 30:
              // --  Line 20 body 1
              local.Print.RptDetail =
                "Your Kansas Child Support Payment Statement is provided for your information.";
                

              break;
            case 31:
              // --  Line 21  body 2
              local.Print.RptDetail =
                "It is by court order, so if you have more than one Kansas child support case,";
                

              break;
            case 32:
              // --  Line 22  body 3
              local.Print.RptDetail = "you may receive additional statements.";

              break;
            case 33:
              // --  Line 23 body 4
              local.Print.RptDetail = "";

              break;
            case 34:
              // --  Line 24 body 5
              local.Print.RptDetail =
                "If you have child support owed, you can make your child support payment in";
                

              break;
            case 35:
              // --  Line 25 body 6
              local.Print.RptDetail = "several ways.";

              break;
            case 36:
              // --  Line 26 body 7
              local.Print.RptDetail = "";

              break;
            case 37:
              // --  Line 27 body 8
              local.Print.RptDetail =
                "If you are currently employed, please call us at 888-757-2445 to set up Income";
                

              break;
            case 38:
              // --  Line 28 body 9
              local.Print.RptDetail =
                "Withholding so your child support payments can regularly be taken out of your";
                

              break;
            case 39:
              // --  Line 29 body 10
              local.Print.RptDetail = "check by your employer.";

              break;
            case 40:
              // --  Line 29 body 11
              local.Print.RptDetail = "";

              break;
            case 41:
              // --  Line 30 body 12
              local.Print.RptDetail =
                "Child support payments can also be made by using a debit/credit card or direct";
                

              break;
            case 42:
              // --  Line 31 body 13
              local.Print.RptDetail =
                "transfer from your bank account.  Go to www.kspaycenter.com to sign up for KPC's";
                

              break;
            case 43:
              // --  Line 32 body 14
              local.Print.RptDetail =
                "electronic payment system (KPCpay) to make your child support payment online.";
                

              break;
            case 44:
              // --  Line 33 body 15
              local.Print.RptDetail = "";

              break;
            case 45:
              // --  Line 34  body 16
              local.Print.RptDetail =
                "You can also pay your support with cash by using PayNearMe or Moneygram. They";
                

              break;
            case 46:
              // --  Line 35 body 17
              local.Print.RptDetail =
                "are online at www.paynearme.com or www.moneygram.com or at stores near you,";
                

              break;
            case 47:
              // --  Line 36  body 18
              local.Print.RptDetail =
                "such as: 7-Eleven, Family Dollar, Walmart, CVS, QuikCash and many others.";
                

              break;
            case 48:
              // --  Line 37 body 19
              local.Print.RptDetail = "";

              break;
            case 49:
              // --  Line 38 body 20
              local.Print.RptDetail =
                "Checks can also be sent to: Kansas Payment Center, PO Box 758599, Topeka";
                

              break;
            case 50:
              // --  Line 39 body 21
              local.Print.RptDetail = "KS  66675-8599.";

              break;
            case 51:
              // --  Line 40 body 22
              local.Print.RptDetail = "";

              break;
            case 52:
              // --  Line 41 body 23
              local.Print.RptDetail =
                "To ensure you get credit for your child support payment, make sure to include";
                

              break;
            case 53:
              // --  Line 42 body 24
              local.Print.RptDetail =
                "your full name, court order number and Social Security number.";
                

              break;
            case 54:
              // --  Line 43 body 25
              local.Print.RptDetail = "";

              break;
            case 55:
              // --  Line 44 body 26
              local.Print.RptDetail =
                "If you have questions about your child support case or would like to update your";
                

              break;
            case 56:
              // --  Line 45 body 27
              local.Print.RptDetail =
                "address, cell phone or text information, please call us at 888-757-2445.";
                

              break;
            case 57:
              // --  Line 43 body 28
              local.Print.RptDetail = "";

              break;
            default:
              break;
          }

          ++local.Common.Count;

          // -- Determine the action to send to the eab report writer. (i.e. do 
          // we need a page break)
          if (local.PrintReminder.Index == 0)
          {
            local.EabReportSend.Command = "NEWPAGE";
          }
          else if (local.Common.Count <= local.NumberOfLinesPerPage.Count)
          {
            local.EabReportSend.Command = "DETAIL";
          }
          else
          {
            local.EabReportSend.Command = "NEWPAGE";
            local.Common.Count = 1;
          }

          local.EabReportSend.RptDetail = local.Print.RptDetail;
          local.EabReportSend.ReportNumber = 2;
          UseSpEabWriteDocument2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            return;
          }
        }
        while(local.PrintReminder.Index < 56);

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
        ++local.TotalNumbOfLetters.Count;
      }

      local.Summary.CheckIndex();

      // -- Increment the NCP count.
      ++local.TotalNumbOfNcpsWritten.Count;
      ++local.NumbOfNcpsSinceChckpnt.Count;

      // -- Create a HIST record on each CASE indicating the postcard was sent.
      foreach(var item1 in ReadCase())
      {
        local.Infrastructure.EventId = 48;
        local.Infrastructure.ReasonCode = "NCPPMTSTMNT";
        local.Infrastructure.Detail = "NCP Payment Statement";
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
            "Number of NCPs to Whom Statements Sent.............................." +
            NumberToString(local.TotalNumbOfNcpsWritten.Count, 9, 7);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Number of Statements sent out......................................." +
            NumberToString(local.TotalNumbOfLetters.Count, 9, 7);

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

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint.
    // ------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // --  Close the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseSpEabWriteDocument1();

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

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
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

    useImport.DateWorkArea.Date = local.EndDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.CurrentDateTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.EndDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens3()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Start.Date;

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

    useImport.CsePersonsWorkSet.Number = local.NcpCsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseFnDisplayObligationsByPayor()
  {
    var useImport = new FnDisplayObligationsByPayor.Import();
    var useExport = new FnDisplayObligationsByPayor.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = local.NcpCsePersonsWorkSet.Number;

    Call(FnDisplayObligationsByPayor.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
  }

  private void UseOeDeterminePayments()
  {
    var useImport = new OeDeterminePayments.Import();
    var useExport = new OeDeterminePayments.Export();

    useImport.Beginning.Date = local.Start.Date;
    useImport.EndDate.Date = local.EndDate.Date;
    useImport.LegalAction.StandardNumber = local.Pass.StandardNumber;
    useImport.CsePerson.Number = entities.NcpCsePerson.Number;

    Call(OeDeterminePayments.Execute, useImport, useExport);

    local.TotalPaymentsCommon.TotalCurrency =
      useExport.TotaledPayments.TotalCurrency;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = entities.NcpCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.NcpCsePersonAddress);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.NcpCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

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

  private void UseSpEabWriteDocument1()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpEabWriteDocument2()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.Assign(local.EabReportSend);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpEabWriteDocument3()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabReportSend.Assign(local.EabReportSend);
    useImport.EabFileHandling.Action = local.Open.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 3);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 4);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 5);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 6);
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
        db.SetDate(
          command, "dueDt", local.FirstDayOfMonth.Date.GetValueOrDefault());
        db.SetString(command, "numb", local.RestartNcp.Number);
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

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.InmateNumber = db.GetNullableString(reader, 3);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 4);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 5);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 6);
        entities.Incarceration.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Incarceration.Incarcerated = db.GetNullableString(reader, 8);
        entities.Incarceration.Populated = true;
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

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          local.Summary.Item.SummaryAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
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

    /// <summary>A PrintHeaderGroup group.</summary>
    [Serializable]
    public class PrintHeaderGroup
    {
      /// <summary>
      /// A value of PrintHeader1.
      /// </summary>
      [JsonPropertyName("printHeader1")]
      public EabReportSend PrintHeader1
      {
        get => printHeader1 ??= new();
        set => printHeader1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private EabReportSend printHeader1;
    }

    /// <summary>A PrintReminderGroup group.</summary>
    [Serializable]
    public class PrintReminderGroup
    {
      /// <summary>
      /// A value of GlocalReportDetailLineEoa.
      /// </summary>
      [JsonPropertyName("glocalReportDetailLineEoa")]
      public EabReportSend GlocalReportDetailLineEoa
      {
        get => glocalReportDetailLineEoa ??= new();
        set => glocalReportDetailLineEoa = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private EabReportSend glocalReportDetailLineEoa;
    }

    /// <summary>A PrintCasexfrGroup group.</summary>
    [Serializable]
    public class PrintCasexfrGroup
    {
      /// <summary>
      /// A value of GlocalReportDetailLineEoa1.
      /// </summary>
      [JsonPropertyName("glocalReportDetailLineEoa1")]
      public EabReportSend GlocalReportDetailLineEoa1
      {
        get => glocalReportDetailLineEoa1 ??= new();
        set => glocalReportDetailLineEoa1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private EabReportSend glocalReportDetailLineEoa1;
    }

    /// <summary>
    /// A value of CurrentMonth.
    /// </summary>
    [JsonPropertyName("currentMonth")]
    public WorkArea CurrentMonth
    {
      get => currentMonth ??= new();
      set => currentMonth = value;
    }

    /// <summary>
    /// A value of ActiveCaseFound.
    /// </summary>
    [JsonPropertyName("activeCaseFound")]
    public Common ActiveCaseFound
    {
      get => activeCaseFound ??= new();
      set => activeCaseFound = value;
    }

    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public WorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkAttributes To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkAttributes From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public TextWorkArea Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of CurentYr.
    /// </summary>
    [JsonPropertyName("curentYr")]
    public TextWorkArea CurentYr
    {
      get => curentYr ??= new();
      set => curentYr = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public DateWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of ToDay.
    /// </summary>
    [JsonPropertyName("toDay")]
    public TextWorkArea ToDay
    {
      get => toDay ??= new();
      set => toDay = value;
    }

    /// <summary>
    /// A value of FromDay.
    /// </summary>
    [JsonPropertyName("fromDay")]
    public TextWorkArea FromDay
    {
      get => fromDay ??= new();
      set => fromDay = value;
    }

    /// <summary>
    /// A value of CollectionPeriod.
    /// </summary>
    [JsonPropertyName("collectionPeriod")]
    public TextWorkArea CollectionPeriod
    {
      get => collectionPeriod ??= new();
      set => collectionPeriod = value;
    }

    /// <summary>
    /// A value of DateArrears.
    /// </summary>
    [JsonPropertyName("dateArrears")]
    public TextWorkArea DateArrears
    {
      get => dateArrears ??= new();
      set => dateArrears = value;
    }

    /// <summary>
    /// A value of DateObliation.
    /// </summary>
    [JsonPropertyName("dateObliation")]
    public TextWorkArea DateObliation
    {
      get => dateObliation ??= new();
      set => dateObliation = value;
    }

    /// <summary>
    /// A value of TotalPaymentsWorkArea.
    /// </summary>
    [JsonPropertyName("totalPaymentsWorkArea")]
    public WorkArea TotalPaymentsWorkArea
    {
      get => totalPaymentsWorkArea ??= new();
      set => totalPaymentsWorkArea = value;
    }

    /// <summary>
    /// A value of TotalArrears.
    /// </summary>
    [JsonPropertyName("totalArrears")]
    public WorkArea TotalArrears
    {
      get => totalArrears ??= new();
      set => totalArrears = value;
    }

    /// <summary>
    /// A value of CurrentObligations.
    /// </summary>
    [JsonPropertyName("currentObligations")]
    public WorkArea CurrentObligations
    {
      get => currentObligations ??= new();
      set => currentObligations = value;
    }

    /// <summary>
    /// A value of CurrentDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateTextWorkArea")]
    public TextWorkArea CurrentDateTextWorkArea
    {
      get => currentDateTextWorkArea ??= new();
      set => currentDateTextWorkArea = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of TotalPaymentsCommon.
    /// </summary>
    [JsonPropertyName("totalPaymentsCommon")]
    public Common TotalPaymentsCommon
    {
      get => totalPaymentsCommon ??= new();
      set => totalPaymentsCommon = value;
    }

    /// <summary>
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public LegalAction Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of TotalNumbOfLetters.
    /// </summary>
    [JsonPropertyName("totalNumbOfLetters")]
    public Common TotalNumbOfLetters
    {
      get => totalNumbOfLetters ??= new();
      set => totalNumbOfLetters = value;
    }

    /// <summary>
    /// A value of ArrearsToMonth.
    /// </summary>
    [JsonPropertyName("arrearsToMonth")]
    public WorkArea ArrearsToMonth
    {
      get => arrearsToMonth ??= new();
      set => arrearsToMonth = value;
    }

    /// <summary>
    /// A value of ArrearsFromMonth.
    /// </summary>
    [JsonPropertyName("arrearsFromMonth")]
    public WorkArea ArrearsFromMonth
    {
      get => arrearsFromMonth ??= new();
      set => arrearsFromMonth = value;
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
    /// A value of NumberOfMonths.
    /// </summary>
    [JsonPropertyName("numberOfMonths")]
    public Common NumberOfMonths
    {
      get => numberOfMonths ??= new();
      set => numberOfMonths = value;
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
    /// A value of NcpCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("ncpCsePersonsWorkSet")]
    public CsePersonsWorkSet NcpCsePersonsWorkSet
    {
      get => ncpCsePersonsWorkSet ??= new();
      set => ncpCsePersonsWorkSet = value;
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
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
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
    /// Gets a value of PrintHeader.
    /// </summary>
    [JsonIgnore]
    public Array<PrintHeaderGroup> PrintHeader => printHeader ??= new(
      PrintHeaderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrintHeader for json serialization.
    /// </summary>
    [JsonPropertyName("printHeader")]
    [Computed]
    public IList<PrintHeaderGroup> PrintHeader_Json
    {
      get => printHeader;
      set => PrintHeader.Assign(value);
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public EabReportSend Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of Spaddr1.
    /// </summary>
    [JsonPropertyName("spaddr1")]
    public AddressRecord Spaddr1
    {
      get => spaddr1 ??= new();
      set => spaddr1 = value;
    }

    /// <summary>
    /// A value of Spaddr2.
    /// </summary>
    [JsonPropertyName("spaddr2")]
    public AddressRecord Spaddr2
    {
      get => spaddr2 ??= new();
      set => spaddr2 = value;
    }

    /// <summary>
    /// A value of Spaddr3.
    /// </summary>
    [JsonPropertyName("spaddr3")]
    public AddressRecord Spaddr3
    {
      get => spaddr3 ??= new();
      set => spaddr3 = value;
    }

    /// <summary>
    /// A value of Arnm.
    /// </summary>
    [JsonPropertyName("arnm")]
    public CsePersonsWorkSet Arnm
    {
      get => arnm ??= new();
      set => arnm = value;
    }

    /// <summary>
    /// A value of Araddr1.
    /// </summary>
    [JsonPropertyName("araddr1")]
    public AddressRecord Araddr1
    {
      get => araddr1 ??= new();
      set => araddr1 = value;
    }

    /// <summary>
    /// A value of Araddr2.
    /// </summary>
    [JsonPropertyName("araddr2")]
    public AddressRecord Araddr2
    {
      get => araddr2 ??= new();
      set => araddr2 = value;
    }

    /// <summary>
    /// A value of Araddr3.
    /// </summary>
    [JsonPropertyName("araddr3")]
    public AddressRecord Araddr3
    {
      get => araddr3 ??= new();
      set => araddr3 = value;
    }

    /// <summary>
    /// A value of Araddr4.
    /// </summary>
    [JsonPropertyName("araddr4")]
    public AddressRecord Araddr4
    {
      get => araddr4 ??= new();
      set => araddr4 = value;
    }

    /// <summary>
    /// A value of Araddr5.
    /// </summary>
    [JsonPropertyName("araddr5")]
    public AddressRecord Araddr5
    {
      get => araddr5 ??= new();
      set => araddr5 = value;
    }

    /// <summary>
    /// A value of NumberOfLinesPerHeader.
    /// </summary>
    [JsonPropertyName("numberOfLinesPerHeader")]
    public Common NumberOfLinesPerHeader
    {
      get => numberOfLinesPerHeader ??= new();
      set => numberOfLinesPerHeader = value;
    }

    /// <summary>
    /// Gets a value of PrintReminder.
    /// </summary>
    [JsonIgnore]
    public Array<PrintReminderGroup> PrintReminder => printReminder ??= new(
      PrintReminderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrintReminder for json serialization.
    /// </summary>
    [JsonPropertyName("printReminder")]
    [Computed]
    public IList<PrintReminderGroup> PrintReminder_Json
    {
      get => printReminder;
      set => PrintReminder.Assign(value);
    }

    /// <summary>
    /// A value of NumberOfLinesPerPage.
    /// </summary>
    [JsonPropertyName("numberOfLinesPerPage")]
    public Common NumberOfLinesPerPage
    {
      get => numberOfLinesPerPage ??= new();
      set => numberOfLinesPerPage = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabFileHandling Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public EabFileHandling Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// Gets a value of PrintCasexfr.
    /// </summary>
    [JsonIgnore]
    public Array<PrintCasexfrGroup> PrintCasexfr => printCasexfr ??= new(
      PrintCasexfrGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrintCasexfr for json serialization.
    /// </summary>
    [JsonPropertyName("printCasexfr")]
    [Computed]
    public IList<PrintCasexfrGroup> PrintCasexfr_Json
    {
      get => printCasexfr;
      set => PrintCasexfr.Assign(value);
    }

    private WorkArea currentMonth;
    private Common activeCaseFound;
    private WorkArea currentDateWorkArea;
    private DateWorkAttributes to;
    private DateWorkAttributes from;
    private TextWorkArea previous;
    private TextWorkArea curentYr;
    private DateWorkArea year;
    private TextWorkArea toDay;
    private TextWorkArea fromDay;
    private TextWorkArea collectionPeriod;
    private TextWorkArea dateArrears;
    private TextWorkArea dateObliation;
    private WorkArea totalPaymentsWorkArea;
    private WorkArea totalArrears;
    private WorkArea currentObligations;
    private TextWorkArea currentDateTextWorkArea;
    private DateWorkArea endDate;
    private Common totalPaymentsCommon;
    private Common numberOfDays;
    private DateWorkArea start;
    private LegalAction pass;
    private Common logExclusions;
    private WorkArea workArea;
    private WorkArea textMessageFile;
    private EabConvertNumeric2 eabConvertNumeric;
    private WorkArea county;
    private Array<SummaryGroup> summary;
    private Array<LocalGroup> local1;
    private Common totalNumbOfLetters;
    private WorkArea arrearsToMonth;
    private WorkArea arrearsFromMonth;
    private Common numbOfNcpsSinceChckpnt;
    private CsePerson restartNcp;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Infrastructure nullInfrastructure;
    private Infrastructure infrastructure;
    private Common recordLength;
    private TextWorkArea textDate;
    private Common numberOfMonths;
    private DateWorkArea paymentCutoffDate;
    private CsePersonsWorkSet ncpCsePersonsWorkSet;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea firstDayOfMonth;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumbOfNcpsWritten;
    private CsePersonAddress ncpCsePersonAddress;
    private Array<PrintHeaderGroup> printHeader;
    private EabReportSend print;
    private AddressRecord spaddr1;
    private AddressRecord spaddr2;
    private AddressRecord spaddr3;
    private CsePersonsWorkSet arnm;
    private AddressRecord araddr1;
    private AddressRecord araddr2;
    private AddressRecord araddr3;
    private AddressRecord araddr4;
    private AddressRecord araddr5;
    private Common numberOfLinesPerHeader;
    private Array<PrintReminderGroup> printReminder;
    private Common numberOfLinesPerPage;
    private EabFileHandling open;
    private EabFileHandling close;
    private Array<PrintCasexfrGroup> printCasexfr;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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

    /// <summary>
    /// A value of NcpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("ncpCsePersonAddress")]
    public CsePersonAddress NcpCsePersonAddress
    {
      get => ncpCsePersonAddress ??= new();
      set => ncpCsePersonAddress = value;
    }

    private Incarceration incarceration;
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
    private CsePerson ncpCsePerson;
    private CsePerson supported2;
    private CsePersonAddress ncpCsePersonAddress;
  }
#endregion
}

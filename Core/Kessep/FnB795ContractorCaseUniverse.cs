// Program: FN_B795_CONTRACTOR_CASE_UNIVERSE, ID: 1902455708, model: 746.
// Short name: SWEF795B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B795_CONTRACTOR_CASE_UNIVERSE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB795ContractorCaseUniverse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B795_CONTRACTOR_CASE_UNIVERSE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB795ContractorCaseUniverse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB795ContractorCaseUniverse.
  /// </summary>
  public FnB795ContractorCaseUniverse(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 01/16/15  GVandy	CQ46307		Initial Development.
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
      // -- This could have resulted from not finding the MPPI or MPCR record.
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
      // 	001-002   Contractor Number
      // 	003-003   Blank
      // 	004-005   Judicial District
      // 	006-006   Blank
      // 	007-014   Worker ID
      // 	015-015   Blank
      // 	016-025   Last Case Number Processed
      // 	026-026   Blank
      // 	027-035   Total Number of Cases Processed
      // 	036-036   Blank
      // 	037-045   Number of Records Written to File Type 1
      // 	046-046   Blank
      // 	047-055   Number of Records Written to File Type 2
      // 	056-056   Blank
      // 	057-065   Number of Records Written to File Type 3a
      // 	066-066   Blank
      // 	067-075   Number of Records Written to File Type 3b
      // -------------------------------------------------------------------------------------
      local.Restart.ContractorNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 2);
      local.Restart.JudicialDistrict =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 4, 2);
      local.Restart.WorkerId =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 7, 8);
      local.Restart.CaseNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 16, 10);
      local.TotalNumbCasesProcessed.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 27, 9));
      local.FileType1Records.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 37, 9));
      local.FileType2Records.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 47, 9));
      local.FileType3ARecords.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 57, 9));
      local.FileType3BRecords.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 67, 9));

      // -- Log restart data to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 14; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Program restarting at ...";

            break;
          case 2:
            local.EabReportSend.RptDetail = "       Contractor Number..." + (
              local.Restart.ContractorNumber ?? "");

            break;
          case 3:
            local.EabReportSend.RptDetail = "       Judicial District..." + local
              .Restart.JudicialDistrict;

            break;
          case 4:
            local.EabReportSend.RptDetail = "       Worker ID..........." + local
              .Restart.WorkerId;

            break;
          case 5:
            local.EabReportSend.RptDetail = "       Case Number........." + local
              .Restart.CaseNumber;

            break;
          case 7:
            local.EabReportSend.RptDetail =
              "Number of Cases Previously Processed " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 27, 9);

            break;
          case 9:
            local.EabReportSend.RptDetail =
              "Number of File Type 1 Records Previously Written " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 37, 9);

            break;
          case 10:
            local.EabReportSend.RptDetail =
              "Number of File Type 2 Records Previously Written " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 47, 9);

            break;
          case 11:
            local.EabReportSend.RptDetail =
              "Number of File Type 3a Records Previously Written " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 57, 9);

            break;
          case 12:
            local.EabReportSend.RptDetail =
              "Number of File Type 3b Records Previously Written " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 67, 9);

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
      // --  Extend the Output Files.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        local.EabFileHandling.Action = "EXTEND";

        switch(local.Common.Count)
        {
          case 1:
            UseFnB795ProcessDataFormat5();

            break;
          case 2:
            UseFnB795ProcessDataFormat7();

            break;
          case 3:
            UseFnB795ProcessDataFormat9();

            break;
          default:
            break;
        }

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error Extending Data Format " + NumberToString
            (local.Common.Count, 15, 1) + " output file(s).  Return status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.Restart.ContractorNumber = "";
      local.Restart.JudicialDistrict = "";
      local.Restart.WorkerId = "";
      local.Restart.CaseNumber = "";
      local.TotalNumbCasesProcessed.Count = 0;
      local.FileType1Records.Count = 0;
      local.FileType2Records.Count = 0;
      local.FileType3ARecords.Count = 0;
      local.FileType3BRecords.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open all 3 Data Format Output Files.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        local.EabFileHandling.Action = "OPEN";

        // -- The program_processing_info processing_date is passed to and the 
        // file record count
        //    returned because a header record is written to the file in 
        // addition to opening the file.
        switch(local.Common.Count)
        {
          case 1:
            UseFnB795ProcessDataFormat4();

            break;
          case 2:
            UseFnB795ProcessDataFormat6();

            break;
          case 3:
            UseFnB795ProcessDataFormat8();

            break;
          default:
            break;
        }

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error Opening Data Format " + NumberToString
            (local.Common.Count, 15, 1) + " output file(s).  Return status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------
    // -- Determine report_month value for previous month.
    // -------------------------------------------------------------------------------------
    local.DateWorkArea.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -
      Day(local.ProgramProcessingInfo.ProcessDate));
    local.DashboardStagingPriority4.ReportMonth =
      Year(local.DateWorkArea.Date) * 100;
    local.DashboardStagingPriority4.
      ReportMonth += Month(local.DateWorkArea.Date);

    // -------------------------------------------------------------------------------------
    // -- Determine the maximum run number for the report_month.
    // -------------------------------------------------------------------------------------
    ReadDashboardStagingPriority1();

    // -------------------------------------------------------------------------------------
    // -- Log the Dashboard Pyramid Report Month and Run Number to the control 
    // report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Dashboard Pyramid Staging Table Report Month " + NumberToString
            (local.DashboardStagingPriority4.ReportMonth, 10, 6);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Dashboard Pyramid Staging Table Report Run Number " + NumberToString
            (local.DashboardStagingPriority4.RunNumber, 13, 3);

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

    local.NullTextDate.Text10 = "01-01-0001";
    local.NullCurrency.Text15 = "000000000000.00";

    // -- Create a local view with the default court order values.
    //    This view will be used to initialize each new entry added to the court
    // order group.
    local.NullCourtOrder.ContractorName = "";
    local.NullCourtOrder.JudicialDistrict = "";
    local.NullCourtOrder.CaseNumber = "";
    local.NullCourtOrder.NcpPersonNumber = "";
    local.NullCourtOrder.CoArrearsPaidFfytd = local.NullCurrency.Text15;
    local.NullCourtOrder.CoArrearsPaidInMonth = local.NullCurrency.Text15;
    local.NullCourtOrder.CoContemptHearingDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoContemptServiceDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoCourtOrderNumber = "";
    local.NullCourtOrder.CoCsCollectedFfytd = local.NullCurrency.Text15;
    local.NullCourtOrder.CoCsCollectedInMonth = local.NullCurrency.Text15;
    local.NullCourtOrder.CoCsDueFfytd = local.NullCurrency.Text15;
    local.NullCourtOrder.CoCsDueInMonth = local.NullCurrency.Text15;
    local.NullCourtOrder.CoDemandLetterCreatedDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoLastDsoPaymentDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoLastIClassActionTaken = "";
    local.NullCourtOrder.CoLastIClassCreatedDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoLastIClassFiledDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoLastIClassIwgl = "N";
    local.NullCourtOrder.CoLastPaymentAmount = local.NullCurrency.Text15;
    local.NullCourtOrder.CoLastPaymentDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoLastPaymentType = "";
    local.NullCourtOrder.CoMonthlyIwoWaAmount = local.NullCurrency.Text15;
    local.NullCourtOrder.CoPetitionCreatedDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoPetitionFiledDate = local.NullTextDate.Text10;
    local.NullCourtOrder.CoTotalArrearsAmountDue = local.NullCurrency.Text15;

    // -- Create a local view with the default court case/ncp/cp values.
    //    This view will be used to initialize case/ncp/cp values.
    local.NullCaseNcpCp.CaseOpenDate = local.NullTextDate.Text10;
    local.NullCaseNcpCp.CollectionRate = "0000000.0";
    local.NullCaseNcpCp.CpDateOfBirth = local.NullTextDate.Text10;
    local.NullCaseNcpCp.CuraAmount = local.NullCurrency.Text15;
    local.NullCaseNcpCp.CurrentSupportDue = local.NullCurrency.Text15;
    local.NullCaseNcpCp.CurrentSupportPaid = local.NullCurrency.Text15;
    local.NullCaseNcpCp.DateOfEmancipation = local.NullTextDate.Text10;
    local.NullCaseNcpCp.NcpDateOfBirth = local.NullTextDate.Text10;
    local.NullCaseNcpCp.NcpLocateDate = local.NullTextDate.Text10;
    local.NullCaseNcpCp.PaProgramEndDate = local.NullTextDate.Text10;
    local.NullCaseNcpCp.PendingCaseClosureDate = local.NullTextDate.Text10;
    local.NullCaseNcpCp.YoungestEmancipationDate = local.NullTextDate.Text10;
    local.NumbCasesSinceChckpnt.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Read each case in the Dashboard Pyramid Staging table for the previous
    // month.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority2())
    {
      // -- Extract all the data elements for this case.
      UseFnB795ProcessCase();

      // -- Error processing is done inside the called cabs.  Any exit state 
      // still set upon
      //    returning here will cause an abend.
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error returned from FN_B795_PROCESS_CASE...Case " + entities
          .DashboardStagingPriority4.CaseNumber + " " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Increment the Case count.
      ++local.TotalNumbCasesProcessed.Count;
      ++local.NumbCasesSinceChckpnt.Count;

      // -- File Commit and Checkpoint processing.
      if (local.NumbCasesSinceChckpnt.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // --  COMMIT changes to the Output Files.
        for(local.Common.Count = 1; local.Common.Count <= 3; ++
          local.Common.Count)
        {
          local.EabFileHandling.Action = "COMMIT";

          // -- The program_processing_info processing_date is passed to and the
          // file record count
          //    returned because a header record is written to the file in 
          // addition to opening the file.
          switch(local.Common.Count)
          {
            case 1:
              UseFnB795ProcessDataFormat5();

              break;
            case 2:
              UseFnB795ProcessDataFormat7();

              break;
            case 3:
              UseFnB795ProcessDataFormat9();

              break;
            default:
              break;
          }

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error Commiting Data Format " + NumberToString
              (local.Common.Count, 15, 1) + " output file(s).  Return status = " +
              local.EabFileHandling.Status;
            UseCabErrorReport2();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // -----------------------------------------
        // 	001-002   Contractor Number
        // 	003-003   Blank
        // 	004-005   Judicial District
        // 	006-006   Blank
        // 	007-014   Worker ID
        // 	015-015   Blank
        // 	016-025   Last Case Number Processed
        // 	026-026   Blank
        // 	027-035   Total Number of Cases Processed
        // 	036-036   Blank
        // 	037-045   Number of Records Written to File Type 1
        // 	046-046   Blank
        // 	047-055   Number of Records Written to File Type 2
        // 	056-056   Blank
        // 	057-065   Number of Records Written to File Type 3a
        // 	066-066   Blank
        // 	067-075   Number of Records Written to File Type 3b
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "";
        local.ProgramCheckpointRestart.RestartInfo =
          entities.DashboardStagingPriority4.ContractorNumber;
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 3) + entities
          .DashboardStagingPriority4.JudicialDistrict;
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 6) + entities
          .DashboardStagingPriority4.WorkerId;
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 15) + entities
          .DashboardStagingPriority4.CaseNumber;
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 26) + NumberToString
          (local.TotalNumbCasesProcessed.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 36) + NumberToString
          (local.FileType1Records.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 46) + NumberToString
          (local.FileType2Records.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 56) + NumberToString
          (local.FileType3ARecords.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 66) + NumberToString
          (local.FileType3BRecords.Count, 7, 9);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NumbCasesSinceChckpnt.Count = 0;
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Do a final Commit to the Output File(s).
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      local.EabFileHandling.Action = "COMMIT";

      // -- The program_processing_info processing_date is passed to and the 
      // file record count
      //    returned because a header record is written to the file in addition 
      // to opening the file.
      switch(local.Common.Count)
      {
        case 1:
          UseFnB795ProcessDataFormat5();

          break;
        case 2:
          UseFnB795ProcessDataFormat7();

          break;
        case 3:
          UseFnB795ProcessDataFormat9();

          break;
        default:
          break;
      }

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error on Final Commit of Data Format " + NumberToString
          (local.Common.Count, 15, 1) + " output file(s).  Return status = " + local
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
    // --  Close the Output File(s).
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      local.EabFileHandling.Action = "CLOSE";

      // -- The file record count is passed because the record count is written 
      // to a footer
      //    record in addition to closing the file.
      switch(local.Common.Count)
      {
        case 1:
          UseFnB795ProcessDataFormat5();

          break;
        case 2:
          UseFnB795ProcessDataFormat7();

          break;
        case 3:
          UseFnB795ProcessDataFormat9();

          break;
        default:
          break;
      }

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Closing Data Format " + NumberToString
          (local.Common.Count, 15, 1) + " output file(s).  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 8; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Total Number of Cases Processed.................." + NumberToString
            (local.TotalNumbCasesProcessed.Count, 7, 9);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total Number of File Type 1 Records Written......" + NumberToString
            (local.FileType1Records.Count, 7, 9);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Total Number of File Type 2 Records Written......" + NumberToString
            (local.FileType2Records.Count, 7, 9);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "Total Number of File Type 3a Records Written....." + NumberToString
            (local.FileType3ARecords.Count, 7, 9);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Total Number of File Type 3b Records Written....." + NumberToString
            (local.FileType3BRecords.Count, 7, 9);

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
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseFnB795ProcessCase()
  {
    var useImport = new FnB795ProcessCase.Import();
    var useExport = new FnB795ProcessCase.Export();

    useImport.DashboardStagingPriority4.Assign(
      entities.DashboardStagingPriority4);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.NullCourtOrder.Assign(local.NullCourtOrder);
    useImport.NullCaseNcpCp.Assign(local.NullCaseNcpCp);
    useExport.FileType1Records.Count = local.FileType1Records.Count;
    useExport.FileType2Records.Count = local.FileType2Records.Count;
    useExport.FileType3ARecords.Count = local.FileType3ARecords.Count;
    useExport.FileType3BRecords.Count = local.FileType3BRecords.Count;

    Call(FnB795ProcessCase.Execute, useImport, useExport);

    local.FileType1Records.Count = useExport.FileType1Records.Count;
    local.FileType2Records.Count = useExport.FileType2Records.Count;
    local.FileType3ARecords.Count = useExport.FileType3ARecords.Count;
    local.FileType3BRecords.Count = useExport.FileType3BRecords.Count;
  }

  private void UseFnB795ProcessDataFormat4()
  {
    var useImport = new FnB795ProcessDataFormat1.Import();
    var useExport = new FnB795ProcessDataFormat1.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.FileType1Records.Count = local.FileType1Records.Count;

    Call(FnB795ProcessDataFormat1.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FileType1Records.Count = useExport.FileType1Records.Count;
  }

  private void UseFnB795ProcessDataFormat5()
  {
    var useImport = new FnB795ProcessDataFormat1.Import();
    var useExport = new FnB795ProcessDataFormat1.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType1Records.Count = local.FileType1Records.Count;

    Call(FnB795ProcessDataFormat1.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FileType1Records.Count = useExport.FileType1Records.Count;
  }

  private void UseFnB795ProcessDataFormat6()
  {
    var useImport = new FnB795ProcessDataFormat2.Import();
    var useExport = new FnB795ProcessDataFormat2.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.FileType2Records.Count = local.FileType2Records.Count;

    Call(FnB795ProcessDataFormat2.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FileType2Records.Count = useExport.FileType2Records.Count;
  }

  private void UseFnB795ProcessDataFormat7()
  {
    var useImport = new FnB795ProcessDataFormat2.Import();
    var useExport = new FnB795ProcessDataFormat2.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType2Records.Count = local.FileType2Records.Count;

    Call(FnB795ProcessDataFormat2.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FileType2Records.Count = useExport.FileType2Records.Count;
  }

  private void UseFnB795ProcessDataFormat8()
  {
    var useImport = new FnB795ProcessDataFormat3.Import();
    var useExport = new FnB795ProcessDataFormat3.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.FileType3ARecords.Count = local.FileType3ARecords.Count;
    useImport.FileType3BRecords.Count = local.FileType3BRecords.Count;

    Call(FnB795ProcessDataFormat3.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FileType3ARecords.Count = useExport.FileType3ARecords.Count;
    local.FileType3BRecords.Count = useExport.FileType3BRecords.Count;
  }

  private void UseFnB795ProcessDataFormat9()
  {
    var useImport = new FnB795ProcessDataFormat3.Import();
    var useExport = new FnB795ProcessDataFormat3.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType3ARecords.Count = local.FileType3ARecords.Count;
    useImport.FileType3BRecords.Count = local.FileType3BRecords.Count;

    Call(FnB795ProcessDataFormat3.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FileType3ARecords.Count = useExport.FileType3ARecords.Count;
    local.FileType3BRecords.Count = useExport.FileType3BRecords.Count;
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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadDashboardStagingPriority1()
  {
    local.DashboardStagingPriority4.Populated = false;

    return Read("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", local.DashboardStagingPriority4.ReportMonth);
      },
      (db, reader) =>
      {
        local.DashboardStagingPriority4.RunNumber = db.GetInt32(reader, 0);
        local.DashboardStagingPriority4.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority2()
  {
    entities.DashboardStagingPriority4.Populated = false;

    return ReadEach("ReadDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", local.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", local.DashboardStagingPriority4.RunNumber);
        db.SetNullableString(
          command, "contractorNum", local.Restart.ContractorNumber ?? "");
        db.
          SetString(command, "judicialDistrict", local.Restart.JudicialDistrict);
          
        db.SetString(command, "workerId", local.Restart.WorkerId);
        db.SetString(command, "caseNumber", local.Restart.CaseNumber);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority4.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardStagingPriority4.RunNumber = db.GetInt32(reader, 1);
        entities.DashboardStagingPriority4.CaseNumber = db.GetString(reader, 2);
        entities.DashboardStagingPriority4.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority4.CurrentCsInd =
          db.GetNullableString(reader, 4);
        entities.DashboardStagingPriority4.OtherObgInd =
          db.GetNullableString(reader, 5);
        entities.DashboardStagingPriority4.CsDueAmt =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority4.CsCollectedAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority4.PayingCaseInd =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority4.PaternityEstInd =
          db.GetNullableString(reader, 9);
        entities.DashboardStagingPriority4.AddressVerInd =
          db.GetNullableString(reader, 10);
        entities.DashboardStagingPriority4.EmployerVerInd =
          db.GetNullableString(reader, 11);
        entities.DashboardStagingPriority4.WorkerId = db.GetString(reader, 12);
        entities.DashboardStagingPriority4.JudicialDistrict =
          db.GetString(reader, 13);
        entities.DashboardStagingPriority4.ContractorNumber =
          db.GetNullableString(reader, 14);
        entities.DashboardStagingPriority4.Populated = true;

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
    /// A value of NullCaseNcpCp.
    /// </summary>
    [JsonPropertyName("nullCaseNcpCp")]
    public ContractorCaseUniverse NullCaseNcpCp
    {
      get => nullCaseNcpCp ??= new();
      set => nullCaseNcpCp = value;
    }

    /// <summary>
    /// A value of NullCourtOrder.
    /// </summary>
    [JsonPropertyName("nullCourtOrder")]
    public ContractorCaseUniverse NullCourtOrder
    {
      get => nullCourtOrder ??= new();
      set => nullCourtOrder = value;
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
    /// A value of FileType3BRecords.
    /// </summary>
    [JsonPropertyName("fileType3BRecords")]
    public Common FileType3BRecords
    {
      get => fileType3BRecords ??= new();
      set => fileType3BRecords = value;
    }

    /// <summary>
    /// A value of FileType3ARecords.
    /// </summary>
    [JsonPropertyName("fileType3ARecords")]
    public Common FileType3ARecords
    {
      get => fileType3ARecords ??= new();
      set => fileType3ARecords = value;
    }

    /// <summary>
    /// A value of FileType2Records.
    /// </summary>
    [JsonPropertyName("fileType2Records")]
    public Common FileType2Records
    {
      get => fileType2Records ??= new();
      set => fileType2Records = value;
    }

    /// <summary>
    /// A value of FileType1Records.
    /// </summary>
    [JsonPropertyName("fileType1Records")]
    public Common FileType1Records
    {
      get => fileType1Records ??= new();
      set => fileType1Records = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public DashboardStagingPriority4 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority4.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority4")]
    public DashboardStagingPriority4 DashboardStagingPriority4
    {
      get => dashboardStagingPriority4 ??= new();
      set => dashboardStagingPriority4 = value;
    }

    /// <summary>
    /// A value of NumbCasesSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbCasesSinceChckpnt")]
    public Common NumbCasesSinceChckpnt
    {
      get => numbCasesSinceChckpnt ??= new();
      set => numbCasesSinceChckpnt = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of TotalNumbCasesProcessed.
    /// </summary>
    [JsonPropertyName("totalNumbCasesProcessed")]
    public Common TotalNumbCasesProcessed
    {
      get => totalNumbCasesProcessed ??= new();
      set => totalNumbCasesProcessed = value;
    }

    /// <summary>
    /// A value of NullCurrency.
    /// </summary>
    [JsonPropertyName("nullCurrency")]
    public WorkArea NullCurrency
    {
      get => nullCurrency ??= new();
      set => nullCurrency = value;
    }

    /// <summary>
    /// A value of NullTextDate.
    /// </summary>
    [JsonPropertyName("nullTextDate")]
    public TextWorkArea NullTextDate
    {
      get => nullTextDate ??= new();
      set => nullTextDate = value;
    }

    private ContractorCaseUniverse nullCaseNcpCp;
    private ContractorCaseUniverse nullCourtOrder;
    private DateWorkArea dateWorkArea;
    private Common fileType3BRecords;
    private Common fileType3ARecords;
    private Common fileType2Records;
    private Common fileType1Records;
    private DashboardStagingPriority4 restart;
    private DashboardStagingPriority4 dashboardStagingPriority4;
    private Common numbCasesSinceChckpnt;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumbCasesProcessed;
    private WorkArea nullCurrency;
    private TextWorkArea nullTextDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DashboardStagingPriority4.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority4")]
    public DashboardStagingPriority4 DashboardStagingPriority4
    {
      get => dashboardStagingPriority4 ??= new();
      set => dashboardStagingPriority4 = value;
    }

    private DashboardStagingPriority4 dashboardStagingPriority4;
  }
#endregion
}

// Program: FN_B721_BATCH_INITIALIZATION, ID: 371192566, model: 746.
// Short name: SWE02054
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB721BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721BatchInitialization.
  /// </summary>
  public FnB721BatchInitialization(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 12/02/03  GVandy	WR040134	Initial Development
    // 03/10/04  EShirk	PR198543	Added alt reporting period to the misc file 
    // processing.
    // 01/06/09  GVandy	CQ486		Add an audit trail to determine why part 1 and 
    // part 2 of the
    // 					OCSE34 report do not balance.
    // -----------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------
    // -- Read the PPI record.
    // -------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // -------------------------------------------------------------------------------------------
    // -- Open Error Report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProgramName = global.UserId;
    local.Open.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.  Had to 
      // open the error report before escaping to the PrAD.
      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Open Control Report.
    // -------------------------------------------------------------------------------------------
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Check for AUDIT parameter on the PPI record.
    // -------------------------------------------------------------------------------------------
    if (Find(local.ProgramProcessingInfo.ParameterList, "AUDIT") == 0)
    {
      export.Audit.Flag = "N";
    }
    else
    {
      export.Audit.Flag = "Y";

      // -- Log to the control report that audit is being performed.
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "AUDIT parameter is specified on the PPI record.  All amounts will be traced to the originating cash receipt detail.";
              

            break;
          case 2:
            local.EabReportSend.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }
      }
    }

    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------------
    // -- Determine if this is a restart.
    // -------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------------
      // -- We are restarting.
      // -------------------------------------------------------------------------------------------
      // -- Extract the restart line number and run date from the restart info.
      export.RestartLine.Text2 =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 2);
      export.Ocse34.CreatedTimestamp =
        Timestamp(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 11, 26));

      // -- Read the OCSE34 record to get reporting period dates...
      if (ReadOcse1())
      {
        export.ReportPeriodEndDate.Date =
          entities.Ocse34.ReportingPeriodEndDate;
        local.Ocse34.Assign(entities.Ocse34);
        MoveOcse34(entities.Ocse34, export.Ocse34);
      }
      else
      {
        ExitState = "FN0000_OCSE34_NF";

        return;
      }

      // -------------------------------------------------------------------------------------------
      // -- Write Restart info to the Control Report.
      // -------------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "RESTARTING AT LINE " + export
              .RestartLine.Text2;

            break;
          case 2:
            local.EabReportSend.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }
      }
    }
    else
    {
      // -------------------------------------------------------------------------------------------
      // -- We are not restarting.  Get parameter and adjustment amounts from 
      // the Misc file.
      // -------------------------------------------------------------------------------------------
      local.FileNumber.Count = 0;

      // -------------------------------------------------------------------------------------------
      // -- Open Misc input file (run parameters).
      // -------------------------------------------------------------------------------------------
      local.External.FileInstruction = "OPEN";
      UseFnB721ExtReadFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

        return;
      }

      do
      {
        // -------------------------------------------------------------------------------------------
        // -- Read each parameter/adjustment amount.
        // -------------------------------------------------------------------------------------------
        local.External.FileInstruction = "READ";
        UseFnB721ExtReadFile();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          ExitState = "OE0000_ERROR_READING_EXT_FILE";

          return;
        }

        switch(TrimEnd(Substring(local.External.TextLine80, 1, 2)))
        {
          case "01":
            // -- Process period begin date.
            local.Ocse34.ReportingPeriodBeginDate =
              StringToDate(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength, 4,
              4) + "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 9, 2) +
              "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 12, 2));
              

            break;
          case "02":
            // -- Process period end date.
            local.Ocse34.ReportingPeriodEndDate =
              StringToDate(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength, 4,
              4) + "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 9, 2) +
              "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 12, 2));
              

            break;
          case "03":
            // -- Adjustment amount.
            // -- Validate that the adjustment amount is in the format 99999999S
            if (Verify(Substring(
              local.External.TextLine80, External.TextLine80_MaxLength, 4, 8),
              "0123456789") == 0 && Verify
              (Substring(
                local.External.TextLine80, External.TextLine80_MaxLength, 12,
              1), " -+") == 0)
            {
              // -- Format is valid.  Convert to numeric.
              local.Ocse34.AdjustmentsAmount =
                (int?)StringToNumber(Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength, 4,
                8));

              if (CharAt(local.External.TextLine80, 12) == '-')
              {
                local.Ocse34.AdjustmentsAmount =
                  -local.Ocse34.AdjustmentsAmount.GetValueOrDefault();
              }
            }
            else
            {
              // -- Adjustment amount is not in a 99999999S format.
              local.EabReportSend.RptDetail =
                "Invalid format for adjustment amount " + Substring
                (local.External.TextLine80, External.TextLine80_MaxLength, 4, 9) +
                " in Misc file.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }

            break;
          case "04":
            // -- Adjustment text footer.
            local.Ocse34.AdjustFooterText =
              Substring(local.External.TextLine80, 4, 77);

            break;
          case "05":
            // -- Undistributed amount.
            // -- Validate that the undistributed amount is in the format 
            // 99999999S
            if (Verify(Substring(
              local.External.TextLine80, External.TextLine80_MaxLength, 4, 8),
              "0123456789") == 0 && Verify
              (Substring(
                local.External.TextLine80, External.TextLine80_MaxLength, 12,
              1), " -+") == 0)
            {
              // -- Format is valid.  Convert to numeric.
              local.Ocse34.UndistributedAmount =
                (int?)StringToNumber(Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength, 4,
                8));

              if (CharAt(local.External.TextLine80, 12) == '-')
              {
                local.Ocse34.UndistributedAmount =
                  -local.Ocse34.UndistributedAmount.GetValueOrDefault();
              }
            }
            else
            {
              // -- Undistributed amount is not in a 99999999S format.
              local.EabReportSend.RptDetail =
                "Invalid format for undistributed amount " + Substring
                (local.External.TextLine80, External.TextLine80_MaxLength, 4, 9) +
                " in Misc file.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }

            break;
          case "06":
            // -- Incentive pay amount.
            // -- Validate that the incentive pay amount is in the format 
            // 99999999S
            if (Verify(Substring(
              local.External.TextLine80, External.TextLine80_MaxLength, 4, 8),
              "0123456789") == 0 && Verify
              (Substring(
                local.External.TextLine80, External.TextLine80_MaxLength, 12,
              1), " -+") == 0)
            {
              // -- Format is valid.  Convert to numeric.
              local.Ocse34.IncentivePaymentAmount =
                (int?)StringToNumber(Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength, 4,
                8));

              if (CharAt(local.External.TextLine80, 12) == '-')
              {
                local.Ocse34.IncentivePaymentAmount =
                  -local.Ocse34.IncentivePaymentAmount.GetValueOrDefault();
              }
            }
            else
            {
              // -- Incentive pay amount is not in a 99999999S format.
              local.EabReportSend.RptDetail =
                "Invalid format for incentive pay amount " + Substring
                (local.External.TextLine80, External.TextLine80_MaxLength, 4, 9) +
                " in Misc file.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }

            break;
          case "07":
            // -- Process alt period begin date.
            local.Ocse34.AltReportingPeriodBeginDate =
              StringToDate(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength, 4,
              4) + "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 9, 2) +
              "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 12, 2));
              

            break;
          case "08":
            // -- Process alt period end date.
            local.Ocse34.AltReportingPeriodEndDate =
              StringToDate(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength, 4,
              4) + "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 9, 2) +
              "-" + Substring
              (local.External.TextLine80, External.TextLine80_MaxLength, 12, 2));
              

            break;
          case "99":
            // -- End of File marker.
            local.EndOfFileMarker.Flag = "Y";

            break;
          default:
            // -- Invalid record id.
            ExitState = "ACO_RE0000_INPUT_RECORD_TYPE_INV";

            return;
        }
      }
      while(AsChar(local.EndOfFileMarker.Flag) != 'Y');

      // -------------------------------------------------------------------------------------------
      // -- Close Misc input file.
      // -------------------------------------------------------------------------------------------
      local.External.FileInstruction = "CLOSE";
      UseFnB721ExtReadFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

        return;
      }

      // -------------------------------------------------------------------------------------------
      // -- Validate parameters from the Misc input file.
      // -------------------------------------------------------------------------------------------
      if (Equal(local.Ocse34.ReportingPeriodBeginDate, local.Null1.Date) || Equal
        (local.Ocse34.ReportingPeriodEndDate, local.Null1.Date))
      {
        // -- Must have begin and end dates.
        ExitState = "FN0000_REPORTING_PERIOD_REQUIRED";

        return;
      }

      if (Lt(local.Ocse34.ReportingPeriodEndDate,
        local.Ocse34.ReportingPeriodBeginDate))
      {
        ExitState = "ACO_NE0000_END_LESS_THAN_START";

        return;
      }

      if (Equal(local.Ocse34.AltReportingPeriodBeginDate, local.Null1.Date) || Equal
        (local.Ocse34.AltReportingPeriodEndDate, local.Null1.Date))
      {
        // -- Must have begin and end dates.
        ExitState = "FN0000_REPORTING_PERIOD_REQUIRED";

        return;
      }

      if (Lt(local.Ocse34.AltReportingPeriodEndDate,
        local.Ocse34.AltReportingPeriodBeginDate))
      {
        ExitState = "ACO_NE0000_END_LESS_THAN_START";

        return;
      }

      // -- Determine the quarter we are processing based on the reporting 
      // period end date.
      // The end date will always be within the quarter being processed.
      local.Ocse34.Period = Year(local.Ocse34.ReportingPeriodEndDate) * 100 + (
        Month(local.Ocse34.ReportingPeriodEndDate) - 1) / 3 + 1;

      // -- Create the OCSE34 record.
      if (ReadOcse2())
      {
        ExitState = "OCSE34_AE";

        return;
      }
      else
      {
        try
        {
          CreateOcse34();
          MoveOcse34(entities.Ocse34, export.Ocse34);
          export.ReportPeriodEndDate.Date =
            entities.Ocse34.ReportingPeriodEndDate;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OCSE34_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "OCSE34_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // -------------------------------------------------------------------------------------------
      // -- Take an initial Checkpoint
      // -- Store the line number, processing period, and timestamp of the 
      // OCSE34 record.
      // -------------------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      local.ProgramCheckpointRestart.RestartInd = "Y";
      local.ProgramCheckpointRestart.RestartInfo = "01 " + NumberToString
        (entities.Ocse34.Period, 10, 6);
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
        (Year(export.Ocse34.CreatedTimestamp), 12, 4);
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + "-";
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
        (Month(export.Ocse34.CreatedTimestamp), 14, 2);
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + "-";
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
        (Day(export.Ocse34.CreatedTimestamp), 14, 2);
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + "-";
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
        (Hour(export.Ocse34.CreatedTimestamp), 14, 2);
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + ".";
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
        (Minute(export.Ocse34.CreatedTimestamp), 14, 2);
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + ".";
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
        (Second(export.Ocse34.CreatedTimestamp), 14, 2);
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + ".";
      local.ProgramCheckpointRestart.RestartInfo =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
        (Microsecond(export.Ocse34.CreatedTimestamp), 10, 6);
      UseUpdateCheckpointRstAndCommit();
    }

    // -------------------------------------------------------------------------------------------
    // Set reporting period ending timestamp to Midnight.
    // Eg. 3rd qrtr start tmst - 2001-07-01-00.00.00.000000 and end tmst - 2001-
    // 09-30-23.59.59.999999
    // -------------------------------------------------------------------------------------------
    UseFnBuildTimestampFrmDateTime();
    export.ReportPeriodEndDate.Timestamp =
      AddMicroseconds(AddDays(export.ReportPeriodEndDate.Timestamp, 1), -1);

    // -- Set export ocse157_verification fiscal year and run number.
    export.Ocse157Verification.FiscalYear =
      (int?)StringToNumber(NumberToString(export.Ocse34.Period, 10, 4));
    export.Ocse157Verification.RunNumber =
      (int?)StringToNumber(NumberToString(export.Ocse34.Period, 14, 2));

    // -------------------------------------------------------------------------------------------
    // -- Write Parameters to the Control Report.
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 12; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "RUN PARAMETERS";

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            Substring(
              "Report Quarter...............................................",
            1, 35);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (entities.Ocse34.Period, 10, 6);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            Substring(
              "Report Period...............................................", 1,
            35);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (DateToInt(entities.Ocse34.ReportingPeriodBeginDate), 8, 8) + "-";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (DateToInt(export.ReportPeriodEndDate.Date), 8, 8) + " ";

          break;
        case 5:
          local.EabReportSend.RptDetail =
            Substring(
              "Alt Report Period...........................................", 1,
            35);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (DateToInt(local.Ocse34.AltReportingPeriodBeginDate), 8, 8) + "-";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (DateToInt(local.Ocse34.AltReportingPeriodEndDate), 8, 8) + " ";

          break;
        case 6:
          local.EabReportSend.RptDetail =
            Substring(
              "Adjustment Amount...........................................", 1,
            35);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (entities.Ocse34.AdjustmentsAmount.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.AdjustmentsAmount, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 7:
          local.EabReportSend.RptDetail =
            Substring(
              "Undistributed Amount........................................", 1,
            35);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (entities.Ocse34.UndistributedAmount.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.UndistributedAmount, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 8:
          local.EabReportSend.RptDetail =
            Substring(
              "Incentive Payment Amount........................................",
            1, 35);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (entities.Ocse34.IncentivePaymentAmount.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.IncentivePaymentAmount, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 9:
          local.EabReportSend.RptDetail =
            Substring(
              "Adjustment Footer Text........................................",
            1, 35);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + entities
            .Ocse34.AdjustFooterText;

          break;
        case 10:
          local.EabReportSend.RptDetail = "";

          break;
        case 11:
          local.EabReportSend.RptDetail = "CSE EXTRACT TOTALS";

          break;
        case 12:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveOcse34(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.Open, useImport.NeededToOpen);
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

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.Open, useImport.NeededToOpen);
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

  private void UseFnB721ExtReadFile()
  {
    var useImport = new FnB721ExtReadFile.Import();
    var useExport = new FnB721ExtReadFile.Export();

    useImport.FileNumber.Count = local.FileNumber.Count;
    useImport.External.FileInstruction = local.External.FileInstruction;
    MoveExternal(local.External, useExport.External);

    Call(FnB721ExtReadFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.ReportPeriodEndDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.ReportPeriodEndDate);
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

  private void CreateOcse34()
  {
    var period = local.Ocse34.Period;
    var adjustmentsAmount = local.Ocse34.AdjustmentsAmount.GetValueOrDefault();
    var undistributedAmount =
      local.Ocse34.UndistributedAmount.GetValueOrDefault();
    var incentivePaymentAmount =
      local.Ocse34.IncentivePaymentAmount.GetValueOrDefault();
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var reportingPeriodBeginDate = local.Ocse34.ReportingPeriodBeginDate;
    var reportingPeriodEndDate = local.Ocse34.ReportingPeriodEndDate;
    var adjustFooterText = local.Ocse34.AdjustFooterText ?? "";
    var altReportingPeriodBeginDate = local.Ocse34.AltReportingPeriodBeginDate;
    var altReportingPeriodEndDate = local.Ocse34.AltReportingPeriodEndDate;

    entities.Ocse34.Populated = false;
    Update("CreateOcse34",
      (db, command) =>
      {
        db.SetInt32(command, "period", period);
        db.SetInt32(command, "prevUndistAmt", 0);
        db.SetNullableInt32(command, "adjustmentsAmt", adjustmentsAmount);
        db.SetNullableInt32(command, "undistribAmt", undistributedAmount);
        db.SetNullableInt32(command, "incentPayAmt", incentivePaymentAmount);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDecimal(command, "fmapRate", 0M);
        db.SetNullableDate(command, "rptPrdBeginDt", reportingPeriodBeginDate);
        db.SetNullableDate(command, "rptPrdEndDt", reportingPeriodEndDate);
        db.SetNullableString(command, "adjFtrTxt", adjustFooterText);
        db.
          SetNullableDate(command, "altRptBeginDt", altReportingPeriodBeginDate);
          
        db.SetNullableDate(command, "altRptEndDt", altReportingPeriodEndDate);
      });

    entities.Ocse34.Period = period;
    entities.Ocse34.AdjustmentsAmount = adjustmentsAmount;
    entities.Ocse34.UndistributedAmount = undistributedAmount;
    entities.Ocse34.IncentivePaymentAmount = incentivePaymentAmount;
    entities.Ocse34.CreatedTimestamp = createdTimestamp;
    entities.Ocse34.CreatedBy = createdBy;
    entities.Ocse34.ReportingPeriodBeginDate = reportingPeriodBeginDate;
    entities.Ocse34.ReportingPeriodEndDate = reportingPeriodEndDate;
    entities.Ocse34.AdjustFooterText = adjustFooterText;
    entities.Ocse34.AltReportingPeriodBeginDate = altReportingPeriodBeginDate;
    entities.Ocse34.AltReportingPeriodEndDate = altReportingPeriodEndDate;
    entities.Ocse34.Populated = true;
  }

  private bool ReadOcse1()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.Ocse34.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.AdjustmentsAmount = db.GetNullableInt32(reader, 1);
        entities.Ocse34.UndistributedAmount = db.GetNullableInt32(reader, 2);
        entities.Ocse34.IncentivePaymentAmount = db.GetNullableInt32(reader, 3);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Ocse34.CreatedBy = db.GetString(reader, 5);
        entities.Ocse34.ReportingPeriodBeginDate =
          db.GetNullableDate(reader, 6);
        entities.Ocse34.ReportingPeriodEndDate = db.GetNullableDate(reader, 7);
        entities.Ocse34.AdjustFooterText = db.GetNullableString(reader, 8);
        entities.Ocse34.AltReportingPeriodBeginDate =
          db.GetNullableDate(reader, 9);
        entities.Ocse34.AltReportingPeriodEndDate =
          db.GetNullableDate(reader, 10);
        entities.Ocse34.Populated = true;
      });
  }

  private bool ReadOcse2()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse2",
      (db, command) =>
      {
        db.SetInt32(command, "period", local.Ocse34.Period);
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.AdjustmentsAmount = db.GetNullableInt32(reader, 1);
        entities.Ocse34.UndistributedAmount = db.GetNullableInt32(reader, 2);
        entities.Ocse34.IncentivePaymentAmount = db.GetNullableInt32(reader, 3);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Ocse34.CreatedBy = db.GetString(reader, 5);
        entities.Ocse34.ReportingPeriodBeginDate =
          db.GetNullableDate(reader, 6);
        entities.Ocse34.ReportingPeriodEndDate = db.GetNullableDate(reader, 7);
        entities.Ocse34.AdjustFooterText = db.GetNullableString(reader, 8);
        entities.Ocse34.AltReportingPeriodBeginDate =
          db.GetNullableDate(reader, 9);
        entities.Ocse34.AltReportingPeriodEndDate =
          db.GetNullableDate(reader, 10);
        entities.Ocse34.Populated = true;
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
    /// <summary>
    /// A value of RestartLine.
    /// </summary>
    [JsonPropertyName("restartLine")]
    public TextWorkArea RestartLine
    {
      get => restartLine ??= new();
      set => restartLine = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of ReportPeriodEndDate.
    /// </summary>
    [JsonPropertyName("reportPeriodEndDate")]
    public DateWorkArea ReportPeriodEndDate
    {
      get => reportPeriodEndDate ??= new();
      set => reportPeriodEndDate = value;
    }

    /// <summary>
    /// A value of Audit.
    /// </summary>
    [JsonPropertyName("audit")]
    public Common Audit
    {
      get => audit ??= new();
      set => audit = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private TextWorkArea restartLine;
    private Ocse34 ocse34;
    private DateWorkArea reportPeriodEndDate;
    private Common audit;
    private Ocse157Verification ocse157Verification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EndOfFileMarker.
    /// </summary>
    [JsonPropertyName("endOfFileMarker")]
    public Common EndOfFileMarker
    {
      get => endOfFileMarker ??= new();
      set => endOfFileMarker = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
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
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend open;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private Common endOfFileMarker;
    private Ocse34 ocse34;
    private External external;
    private Common fileNumber;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
  }
#endregion
}

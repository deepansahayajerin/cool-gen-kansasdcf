// Program: SP_B370_PURGE_ALERTS, ID: 371000827, model: 746.
// Short name: SWEP370B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B370_PURGE_ALERTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB370PurgeAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B370_PURGE_ALERTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB370PurgeAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB370PurgeAlerts.
  /// </summary>
  public SpB370PurgeAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // **                           M A I N T E N A N C E   L O G
    // **
    // ***************************************************************************************
    // *   Date   Developer  PR#/WR#   Description
    // 
    // *
    // *-------------------------------------------------------------------------------------*
    // * OCT 2000 SWSRCHF     000207   Initial development
    // *
    // 
    // This program purges the OSP
    // Alerts after writing them
    // *
    // 
    // to an extract file. The process
    // date from the PPI table
    // *
    // 
    // is used to calculate the purge
    // date range . The program
    // *
    // 
    // also accepts a parameter date
    // range from the PPI table.
    // *
    // *
    // 
    // Example 1: PPI Process date 10-
    // 01-2000
    // *
    // 
    // OSP Alerts purged for dates 08-
    // 01-2000 thru 08-31-2000
    // *
    // *
    // 
    // Example 2: PPI Parameter
    // 1001200010312000
    // *
    // 
    // OSP Alerts purge for dates 10-
    // 01-2000 thru 10-31-2000
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.WorkProgramProcessingInfo.Name = global.UserId;

    // *** READ the Program Processing Info table
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (!IsEmpty(local.WorkProgramProcessingInfo.ParameterList))
    {
      // ***
      // *** Parm range BEGIN date
      // ***
      local.BeginParmRange.DistributionDate =
        IntToDate((int)StringToNumber(Substring(
          local.WorkProgramProcessingInfo.ParameterList, 1, 8)));

      // ***
      // *** Parm range END date
      // ***
      local.EndParmRange.DistributionDate =
        IntToDate((int)StringToNumber(Substring(
          local.WorkProgramProcessingInfo.ParameterList, 9, 8)));
    }

    local.Run.Month = Month(local.WorkProgramProcessingInfo.ProcessDate);
    local.BeginRangeDateWorkArea.Year =
      Year(local.WorkProgramProcessingInfo.ProcessDate);
    local.EndRangeDateWorkArea.Year =
      Year(local.WorkProgramProcessingInfo.ProcessDate);

    switch(local.Run.Month)
    {
      case 1:
        // *** run month January
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("11");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.BeginRangeDateWorkArea.Year =
          Year(AddYears(local.WorkProgramProcessingInfo.ProcessDate, -1));
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("11");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("30");
        local.EndRangeDateWorkArea.Year =
          Year(AddYears(local.WorkProgramProcessingInfo.ProcessDate, -1));

        break;
      case 2:
        // *** run month February
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("12");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.BeginRangeDateWorkArea.Year =
          Year(AddYears(local.WorkProgramProcessingInfo.ProcessDate, -1));
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("12");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("31");
        local.EndRangeDateWorkArea.Year =
          Year(AddYears(local.WorkProgramProcessingInfo.ProcessDate, -1));

        break;
      case 3:
        // *** run month March
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("01");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("31");

        break;
      case 4:
        // *** run month April
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("02");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("02");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("28");

        break;
      case 5:
        // *** run month May
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("03");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("03");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("31");

        break;
      case 6:
        // *** run month June
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("04");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("04");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("30");

        break;
      case 7:
        // *** run month July
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("05");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("05");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("31");

        break;
      case 8:
        // *** run month August
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("06");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("06");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("30");

        break;
      case 9:
        // *** run month September
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("07");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("07");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("31");

        break;
      case 10:
        // *** run month October
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("08");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("08");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("31");

        break;
      case 11:
        // *** run month November
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("09");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("09");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("30");

        break;
      default:
        // *** run month December
        local.BeginRangeDateWorkArea.Month = (int)StringToNumber("10");
        local.BeginRangeDateWorkArea.Day = (int)StringToNumber("01");
        local.EndRangeDateWorkArea.Month = (int)StringToNumber("10");
        local.EndRangeDateWorkArea.Day = (int)StringToNumber("31");

        break;
    }

    local.BeginRangeDateWorkArea.TextDate =
      NumberToString(local.BeginRangeDateWorkArea.Year, 12, 4) + NumberToString
      (local.BeginRangeDateWorkArea.Month, 14, 2) + NumberToString
      (local.BeginRangeDateWorkArea.Day, 14, 2);

    // ***
    // *** range BEGIN date
    // ***
    local.BeginRangeOfficeServiceProviderAlert.DistributionDate =
      IntToDate((int)StringToNumber(local.BeginRangeDateWorkArea.TextDate));
    local.EndRangeDateWorkArea.TextDate =
      NumberToString(local.EndRangeDateWorkArea.Year, 12, 4) + NumberToString
      (local.EndRangeDateWorkArea.Month, 14, 2) + NumberToString
      (local.EndRangeDateWorkArea.Day, 14, 2);

    // ***
    // *** range END date
    // ***
    local.EndRangeOfficeServiceProviderAlert.DistributionDate =
      IntToDate((int)StringToNumber(local.EndRangeDateWorkArea.TextDate));
    local.WorkEabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.UserId;
    local.NeededToOpen.ProcessDate =
      local.WorkProgramProcessingInfo.ProcessDate;

    // *** OPEN the Error report
    UseCabErrorReport2();

    if (!Equal(local.WorkEabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.WorkEabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.UserId;
    local.NeededToOpen.ProcessDate =
      local.WorkProgramProcessingInfo.ProcessDate;

    // *** OPEN the Control report
    UseCabControlReport2();

    if (!Equal(local.WorkEabFileHandling.Status, "OK"))
    {
      local.WorkEabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control report";

      // *** WRITE to the Error report
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.WorkProgramCheckpointRestart.ProgramName = global.UserId;

    // *** Read the Checkpoint Restart table
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.WorkEabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered reading the Program Checkpoint Restart table.";

      // *** WRITE to the Error report
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.WorkProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // *** get the RESTART parameters
      local.RestartOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.WorkProgramCheckpointRestart.RestartInfo, 250, 12, 4));
      local.RestartServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.WorkProgramCheckpointRestart.RestartInfo, 250, 26, 5));
      local.RestartOfficeServiceProvider.EffectiveDate =
        IntToDate((int)StringToNumber(Substring(
          local.WorkProgramCheckpointRestart.RestartInfo, 250, 31, 8)));
      local.RestartOfficeServiceProvider.RoleCode =
        Substring(local.WorkProgramCheckpointRestart.RestartInfo, 39, 2);
      local.RestartOfficeServiceProviderAlert.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(
          local.WorkProgramCheckpointRestart.RestartInfo, 250, 47, 9));
      local.WorkEabFileHandling.Action = "EXTEND";
    }
    else
    {
      local.WorkEabFileHandling.Action = "OPEN";
    }

    // *** OPEN (no Restart) or OPEN EXTEND (Restart) the extract file
    UseEabPurgedAlertsExtract();

    if (!Equal(local.WorkEabFileHandling.Status, "OK"))
    {
      local.WorkEabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Extract file";

      // *** WRITE to the Error report
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.CheckpointControl.Count =
      local.WorkProgramCheckpointRestart.ReadFrequencyCount.
        GetValueOrDefault() + local
      .WorkProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault();

    // *** Initialize accumulators
    local.CommitControl.Count = 0;
    local.WorkProgramCheckpointRestart.CheckpointCount = 0;
    local.AlertsRead.Count = 0;
    local.AlertsPurged.Count = 0;
    local.AlertsWrittenToExtract.Count = 0;
    local.AlertsWithNoHistory.Count = 0;
    local.AlertsWithHistory.Count = 0;
    local.ManualAlerts.Count = 0;

    // ***
    // 
    // ***
    //  ***
    // 
    // ***
    // ***           Main processing LOOP begins here....           ***
    //  ***
    // 
    // ***
    //   ***
    // 
    // ***
    foreach(var item in ReadOfficeOfficeServiceProviderServiceProvider())
    {
      // ***
      // *** Initialize the views
      // ***
      local.WorkCsePersonsWorkSet.FormattedName =
        local.NullCsePersonsWorkSet.FormattedName;
      local.WorkOfficeServiceProvider.RoleCode =
        local.NullOfficeServiceProvider.RoleCode;
      MoveOfficeServiceProviderAlert1(local.NullOfficeServiceProviderAlert,
        local.WorkOfficeServiceProviderAlert);
      local.WorkInfrastructure.Assign(local.NullInfrastructure);
      MoveOffice(local.NullOffice, local.WorkOffice);

      // ***
      // *** Save the retrieved information
      // ***
      MoveOffice(entities.ExistingOffice, local.WorkOffice);
      local.WorkOfficeServiceProvider.RoleCode =
        entities.ExistingOfficeServiceProvider.RoleCode;
      local.WorkOfficeServiceProviderAlert.Assign(
        entities.ExistingOfficeServiceProviderAlert);

      // ***
      // *** Format the Service Provider name
      // ***
      local.WorkCsePersonsWorkSet.FormattedName =
        TrimEnd(entities.ExistingServiceProvider.LastName) + ", " + TrimEnd
        (entities.ExistingServiceProvider.FirstName) + " " + entities
        .ExistingServiceProvider.MiddleInitial;

      // ***
      // *** Increment accumulators
      // ***
      ++local.AlertsRead.Count;
      local.CommitControl.Count += 4;

      // ***
      // *** Initialize flag
      // ***
      local.BypassWritingToExtract.Flag = "N";

      // *** Does the OSP Alert have a history record??
      if (ReadInfrastructure())
      {
        // ***
        // *** System generated OSP Alert
        // ***
        // *** Save the retrieved information
        local.WorkInfrastructure.Assign(entities.ExistingInfrastructure);

        // ***
        // *** Increment accumulator
        // ***
        ++local.CommitControl.Count;
        ++local.AlertsWithHistory.Count;
      }
      else if (Equal(local.WorkOfficeServiceProviderAlert.TypeCode, "AUT"))
      {
        // ***
        // *** System generated OSP Alert
        // ***
        // *** Do NOT write this OSP Alert to the extract, set the bypass write 
        // flag to "Y"
        local.BypassWritingToExtract.Flag = "Y";

        // ***
        // *** Increment accumulators
        // ***
        ++local.AlertsWithNoHistory.Count;
      }
      else
      {
        // ***
        // *** Manual OSP Alert
        // ***
        // *** Increment accumulator
        ++local.ManualAlerts.Count;
      }

      if (AsChar(local.BypassWritingToExtract.Flag) == 'N')
      {
        local.WorkEabFileHandling.Action = "WRITE";

        // *** WRITE to the extract file
        UseEabPurgedAlertsExtract();

        if (!Equal(local.WorkEabFileHandling.Status, "OK"))
        {
          local.WorkEabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing to the Extract file";

          // *** WRITE to the Error report
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.AlertsWrittenToExtract.Count;
      }

      // ***
      // *** Delete the OSP Alert
      // ***
      DeleteOfficeServiceProviderAlert();

      // ***
      // *** Increment accumulators
      // ***
      ++local.CommitControl.Count;
      ++local.AlertsPurged.Count;

      if (local.CommitControl.Count < local.CheckpointControl.Count)
      {
        // *** Do NOT perform a DB2 Commit
        continue;
      }

      // ***
      // *** Perform a DB2 Commit
      // ***
      UseExtToDoACommit();

      // ***
      // *** Set the restart information
      // ***
      local.WorkProgramCheckpointRestart.ProgramName = global.UserId;
      local.WorkProgramCheckpointRestart.CheckpointCount =
        local.WorkProgramCheckpointRestart.CheckpointCount.GetValueOrDefault() +
        1;
      local.WorkProgramCheckpointRestart.RestartInd = "Y";
      local.WorkProgramCheckpointRestart.LastCheckpointTimestamp = Now();
      local.WorkDateWorkArea.TextDate =
        NumberToString(DateToInt(
          entities.ExistingOfficeServiceProvider.EffectiveDate), 8);
      local.WorkProgramCheckpointRestart.RestartInfo =
        NumberToString(entities.ExistingOffice.SystemGeneratedId, 15) + NumberToString
        (entities.ExistingServiceProvider.SystemGeneratedId, 15) + local
        .WorkDateWorkArea.TextDate + entities
        .ExistingOfficeServiceProvider.RoleCode + NumberToString
        (entities.ExistingOfficeServiceProviderAlert.SystemGeneratedIdentifier,
        15);

      // ***
      // *** Update the Checkpoint Restart table with the restart data
      // ***
      UseUpdatePgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.WorkEabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered updating the Program Checkpoint Restart table.";

        // *** WRITE to the Error report
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ***
      // *** Initialize the Commit count
      // ***
      local.CommitControl.Count = 0;
    }

    // ***
    // 
    // ***
    //  ***
    // 
    // ***
    // ***           Main processing LOOP ends here           ***
    //  ***
    // 
    // ***
    //   ***
    // 
    // ***
    // ***
    // *** Perform the final DB2 Commit
    // ***
    UseExtToDoACommit();
    local.WorkProgramCheckpointRestart.ProgramName = global.UserId;
    local.WorkProgramCheckpointRestart.RestartInd = "N";
    local.WorkProgramCheckpointRestart.RestartInfo = "";
    local.WorkProgramCheckpointRestart.CheckpointCount = 0;
    local.WorkProgramCheckpointRestart.LastCheckpointTimestamp = Now();

    // ***
    // *** Update the Checkpoint Restart table, remove the restart data
    // ***
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.WorkEabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered updating the Program Checkpoint Restart table.";

      // *** WRITE to the Error report
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.WorkEabFileHandling.Action = "CLOSE";

    // *** CLOSE the extract file
    UseEabPurgedAlertsExtract();

    if (!Equal(local.WorkEabFileHandling.Status, "OK"))
    {
      local.WorkEabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Extract file";

      // *** WRITE to the Error report
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.PrintLoopControl.Count = 1;

    // ***
    // *** Write the process (READ/PURGE) counts to the Control report
    // ***
    do
    {
      local.WorkEabFileHandling.Action = "WRITE";

      switch(local.PrintLoopControl.Count)
      {
        case 1:
          local.NeededToWrite.RptDetail = "Process date range  : " + NumberToString
            (local.BeginRangeDateWorkArea.Month, 14, 2) + "-" + NumberToString
            (local.BeginRangeDateWorkArea.Day, 14, 2) + "-" + NumberToString
            (local.BeginRangeDateWorkArea.Year, 12, 4);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " thru " + NumberToString
            (local.EndRangeDateWorkArea.Month, 14, 2) + "-" + NumberToString
            (local.EndRangeDateWorkArea.Day, 14, 2) + "-" + NumberToString
            (local.EndRangeDateWorkArea.Year, 12, 4);

          break;
        case 2:
          if (IsEmpty(local.WorkProgramProcessingInfo.ParameterList))
          {
            // ***
            // *** Increment the LOOP counter
            // ***
            ++local.PrintLoopControl.Count;

            continue;
          }

          local.NeededToWrite.RptDetail = "Parameter date range: " + NumberToString
            (DateToInt(local.BeginParmRange.DistributionDate), 12, 2) + "-" + NumberToString
            (DateToInt(local.BeginParmRange.DistributionDate), 14, 2) + "-" + NumberToString
            (DateToInt(local.BeginParmRange.DistributionDate), 8, 4);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " thru " + NumberToString
            (DateToInt(local.EndParmRange.DistributionDate), 12, 2) + "-" + NumberToString
            (DateToInt(local.EndParmRange.DistributionDate), 14, 2) + "-" + NumberToString
            (DateToInt(local.EndParmRange.DistributionDate), 8, 4);

          break;
        case 3:
          // *** write a blank line to the report
          local.NeededToWrite.RptDetail = "";

          break;
        case 4:
          if (local.AlertsRead.Count == 0)
          {
            local.NeededToWrite.RptDetail =
              "OSP Alerts read                          : 0";
          }
          else
          {
            local.Position.Flag = "N";
            local.Position.Count = 0;

            do
            {
              ++local.Position.Count;

              if (!Equal(NumberToString(
                local.AlertsRead.Count, local.Position.Count, 1), "0"))
              {
                local.Position.Flag = "Y";
              }
            }
            while(AsChar(local.Position.Flag) != 'Y');

            local.NeededToWrite.RptDetail =
              "OSP Alerts read                          : " + NumberToString
              (local.AlertsRead.Count, local.Position.Count, 15 - local
              .Position.Count + 1);
          }

          break;
        case 5:
          if (local.ManualAlerts.Count == 0)
          {
            local.NeededToWrite.RptDetail =
              "Manual OSP Alerts (MAN)                  : 0";
          }
          else
          {
            local.Position.Flag = "N";
            local.Position.Count = 0;

            do
            {
              ++local.Position.Count;

              if (!Equal(NumberToString(
                local.ManualAlerts.Count, local.Position.Count, 1), "0"))
              {
                local.Position.Flag = "Y";
              }
            }
            while(AsChar(local.Position.Flag) != 'Y');

            local.NeededToWrite.RptDetail =
              "Manual OSP Alerts (MAN)                  : " + NumberToString
              (local.ManualAlerts.Count, local.Position.Count, 15 - local
              .Position.Count + 1);
          }

          break;
        case 6:
          if (local.AlertsWithHistory.Count == 0)
          {
            local.NeededToWrite.RptDetail =
              "OSP Alerts (AUT) with a History record   : 0";
          }
          else
          {
            local.Position.Flag = "N";
            local.Position.Count = 0;

            do
            {
              ++local.Position.Count;

              if (!Equal(NumberToString(
                local.AlertsWithHistory.Count, local.Position.Count, 1), "0"))
              {
                local.Position.Flag = "Y";
              }
            }
            while(AsChar(local.Position.Flag) != 'Y');

            local.NeededToWrite.RptDetail =
              "OSP Alerts (AUT) with a History record   : " + NumberToString
              (local.AlertsWithHistory.Count, local.Position.Count, 15 - local
              .Position.Count + 1);
          }

          break;
        case 7:
          if (local.AlertsWrittenToExtract.Count == 0)
          {
            local.NeededToWrite.RptDetail =
              "Records written to the extract file      : 0";
          }
          else
          {
            local.Position.Flag = "N";
            local.Position.Count = 0;

            do
            {
              ++local.Position.Count;

              if (!Equal(NumberToString(
                local.AlertsWrittenToExtract.Count, local.Position.Count, 1),
                "0"))
              {
                local.Position.Flag = "Y";
              }
            }
            while(AsChar(local.Position.Flag) != 'Y');

            local.NeededToWrite.RptDetail =
              "Records written to the extract file      : " + NumberToString
              (local.AlertsWrittenToExtract.Count, local.Position.Count, 15 - local
              .Position.Count + 1);
          }

          break;
        case 8:
          if (local.AlertsWithNoHistory.Count == 0)
          {
            local.NeededToWrite.RptDetail =
              "OSP Alerts (AUT) without a History record: 0";
          }
          else
          {
            local.Position.Flag = "N";
            local.Position.Count = 0;

            do
            {
              ++local.Position.Count;

              if (!Equal(NumberToString(
                local.AlertsWithNoHistory.Count, local.Position.Count, 1),
                "0"))
              {
                local.Position.Flag = "Y";
              }
            }
            while(AsChar(local.Position.Flag) != 'Y');

            local.NeededToWrite.RptDetail =
              "OSP Alerts (AUT) without a History record: " + NumberToString
              (local.AlertsWithNoHistory.Count, local.Position.Count, 15 - local
              .Position.Count + 1);
          }

          break;
        default:
          if (local.AlertsPurged.Count == 0)
          {
            local.NeededToWrite.RptDetail =
              "Total OSP Alerts purged                  : 0";
          }
          else
          {
            local.Position.Flag = "N";
            local.Position.Count = 0;

            do
            {
              ++local.Position.Count;

              if (!Equal(NumberToString(
                local.AlertsPurged.Count, local.Position.Count, 1), "0"))
              {
                local.Position.Flag = "Y";
              }
            }
            while(AsChar(local.Position.Flag) != 'Y');

            local.NeededToWrite.RptDetail =
              "Total OSP Alerts purged                  : " + NumberToString
              (local.AlertsPurged.Count, local.Position.Count, 15 - local
              .Position.Count + 1);
          }

          break;
      }

      // *** WRITE to the Control report
      UseCabControlReport1();

      if (!Equal(local.WorkEabFileHandling.Status, "OK"))
      {
        local.WorkEabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing to the Control report";

        // *** WRITE to the Error report
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ***
      // *** Increment the LOOP counter
      // ***
      ++local.PrintLoopControl.Count;
    }
    while(local.PrintLoopControl.Count <= 9);

    local.WorkEabFileHandling.Action = "CLOSE";

    // *** CLOSE the Control report
    UseCabControlReport2();

    if (!Equal(local.WorkEabFileHandling.Status, "OK"))
    {
      local.WorkEabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Control report";

      // *** WRITE to the Error report
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.WorkEabFileHandling.Action = "CLOSE";

    // *** CLOSE the Error report
    UseCabErrorReport2();
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveOfficeServiceProviderAlert1(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Message = source.Message;
    target.DistributionDate = source.DistributionDate;
  }

  private static void MoveOfficeServiceProviderAlert2(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.Message = source.Message;
    target.DistributionDate = source.DistributionDate;
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

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.WorkEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.WorkEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.WorkEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.WorkEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.WorkEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.WorkEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.WorkEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.WorkEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabPurgedAlertsExtract()
  {
    var useImport = new EabPurgedAlertsExtract.Import();
    var useExport = new EabPurgedAlertsExtract.Export();

    MoveEabFileHandling(local.WorkEabFileHandling, useImport.EabFileHandling);
    useImport.CsePersonsWorkSet.FormattedName =
      local.WorkCsePersonsWorkSet.FormattedName;
    useImport.OfficeServiceProvider.RoleCode =
      local.WorkOfficeServiceProvider.RoleCode;
    useImport.Office.Name = local.WorkOffice.Name;
    MoveOfficeServiceProviderAlert2(local.WorkOfficeServiceProviderAlert,
      useImport.OfficeServiceProviderAlert);
    useImport.Infrastructure.Assign(local.WorkInfrastructure);
    MoveEabFileHandling(local.WorkEabFileHandling, useExport.EabFileHandling);

    Call(EabPurgedAlertsExtract.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.WorkEabFileHandling);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.Commit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Commit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.WorkProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.WorkProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.WorkProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.WorkProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.
      Assign(local.WorkProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.WorkProgramCheckpointRestart.
      Assign(useExport.ProgramCheckpointRestart);
  }

  private void DeleteOfficeServiceProviderAlert()
  {
    Update("DeleteOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ExistingOfficeServiceProviderAlert.
            SystemGeneratedIdentifier);
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProviderAlert.Populated);
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ExistingOfficeServiceProviderAlert.InfId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 3);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeOfficeServiceProviderServiceProvider()
  {
    entities.ExistingOfficeServiceProviderAlert.Populated = false;
    entities.ExistingOffice.Populated = false;
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "distributionDate1",
          local.BeginRangeOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetDate(
          command, "distributionDate2",
          local.EndRangeOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetDate(
          command, "distributionDate3",
          local.BeginParmRange.DistributionDate.GetValueOrDefault());
        db.SetDate(
          command, "distributionDate4",
          local.EndParmRange.DistributionDate.GetValueOrDefault());
        db.SetInt32(command, "officeId", local.RestartOffice.SystemGeneratedId);
        db.SetInt32(
          command, "servicePrvderId",
          local.RestartServiceProvider.SystemGeneratedId);
        db.SetNullableDate(
          command, "ospDate",
          local.RestartOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetNullableString(
          command, "ospCode", local.RestartOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "systemGeneratedI",
          local.RestartOfficeServiceProviderAlert.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingOfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingOfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 5);
        entities.ExistingOfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 5);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 6);
        entities.ExistingOfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 7);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 8);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 9);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 10);
        entities.ExistingOfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ExistingOfficeServiceProviderAlert.TypeCode =
          db.GetString(reader, 12);
        entities.ExistingOfficeServiceProviderAlert.Message =
          db.GetString(reader, 13);
        entities.ExistingOfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 14);
        entities.ExistingOfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 15);
        entities.ExistingOfficeServiceProviderAlert.Populated = true;
        entities.ExistingOffice.Populated = true;
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;

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
    /// A value of WorkDateWorkArea.
    /// </summary>
    [JsonPropertyName("workDateWorkArea")]
    public DateWorkArea WorkDateWorkArea
    {
      get => workDateWorkArea ??= new();
      set => workDateWorkArea = value;
    }

    /// <summary>
    /// A value of BeginRangeOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("beginRangeOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert BeginRangeOfficeServiceProviderAlert
    {
      get => beginRangeOfficeServiceProviderAlert ??= new();
      set => beginRangeOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of EndRangeOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("endRangeOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert EndRangeOfficeServiceProviderAlert
    {
      get => endRangeOfficeServiceProviderAlert ??= new();
      set => endRangeOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of BeginParmRange.
    /// </summary>
    [JsonPropertyName("beginParmRange")]
    public OfficeServiceProviderAlert BeginParmRange
    {
      get => beginParmRange ??= new();
      set => beginParmRange = value;
    }

    /// <summary>
    /// A value of EndParmRange.
    /// </summary>
    [JsonPropertyName("endParmRange")]
    public OfficeServiceProviderAlert EndParmRange
    {
      get => endParmRange ??= new();
      set => endParmRange = value;
    }

    /// <summary>
    /// A value of BeginRangeDateWorkArea.
    /// </summary>
    [JsonPropertyName("beginRangeDateWorkArea")]
    public DateWorkArea BeginRangeDateWorkArea
    {
      get => beginRangeDateWorkArea ??= new();
      set => beginRangeDateWorkArea = value;
    }

    /// <summary>
    /// A value of EndRangeDateWorkArea.
    /// </summary>
    [JsonPropertyName("endRangeDateWorkArea")]
    public DateWorkArea EndRangeDateWorkArea
    {
      get => endRangeDateWorkArea ??= new();
      set => endRangeDateWorkArea = value;
    }

    /// <summary>
    /// A value of Run.
    /// </summary>
    [JsonPropertyName("run")]
    public DateWorkArea Run
    {
      get => run ??= new();
      set => run = value;
    }

    /// <summary>
    /// A value of Purge.
    /// </summary>
    [JsonPropertyName("purge")]
    public OfficeServiceProviderAlert Purge
    {
      get => purge ??= new();
      set => purge = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of BypassWritingToExtract.
    /// </summary>
    [JsonPropertyName("bypassWritingToExtract")]
    public Common BypassWritingToExtract
    {
      get => bypassWritingToExtract ??= new();
      set => bypassWritingToExtract = value;
    }

    /// <summary>
    /// A value of AlertsWithHistory.
    /// </summary>
    [JsonPropertyName("alertsWithHistory")]
    public Common AlertsWithHistory
    {
      get => alertsWithHistory ??= new();
      set => alertsWithHistory = value;
    }

    /// <summary>
    /// A value of ManualAlerts.
    /// </summary>
    [JsonPropertyName("manualAlerts")]
    public Common ManualAlerts
    {
      get => manualAlerts ??= new();
      set => manualAlerts = value;
    }

    /// <summary>
    /// A value of AlertsWithNoHistory.
    /// </summary>
    [JsonPropertyName("alertsWithNoHistory")]
    public Common AlertsWithNoHistory
    {
      get => alertsWithNoHistory ??= new();
      set => alertsWithNoHistory = value;
    }

    /// <summary>
    /// A value of PrintLoopControl.
    /// </summary>
    [JsonPropertyName("printLoopControl")]
    public Common PrintLoopControl
    {
      get => printLoopControl ??= new();
      set => printLoopControl = value;
    }

    /// <summary>
    /// A value of CheckpointControl.
    /// </summary>
    [JsonPropertyName("checkpointControl")]
    public Common CheckpointControl
    {
      get => checkpointControl ??= new();
      set => checkpointControl = value;
    }

    /// <summary>
    /// A value of CommitControl.
    /// </summary>
    [JsonPropertyName("commitControl")]
    public Common CommitControl
    {
      get => commitControl ??= new();
      set => commitControl = value;
    }

    /// <summary>
    /// A value of AlertsWrittenToExtract.
    /// </summary>
    [JsonPropertyName("alertsWrittenToExtract")]
    public Common AlertsWrittenToExtract
    {
      get => alertsWrittenToExtract ??= new();
      set => alertsWrittenToExtract = value;
    }

    /// <summary>
    /// A value of AlertsRead.
    /// </summary>
    [JsonPropertyName("alertsRead")]
    public Common AlertsRead
    {
      get => alertsRead ??= new();
      set => alertsRead = value;
    }

    /// <summary>
    /// A value of AlertsPurged.
    /// </summary>
    [JsonPropertyName("alertsPurged")]
    public Common AlertsPurged
    {
      get => alertsPurged ??= new();
      set => alertsPurged = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("nullOfficeServiceProvider")]
    public OfficeServiceProvider NullOfficeServiceProvider
    {
      get => nullOfficeServiceProvider ??= new();
      set => nullOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NullOffice.
    /// </summary>
    [JsonPropertyName("nullOffice")]
    public Office NullOffice
    {
      get => nullOffice ??= new();
      set => nullOffice = value;
    }

    /// <summary>
    /// A value of NullOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("nullOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert NullOfficeServiceProviderAlert
    {
      get => nullOfficeServiceProviderAlert ??= new();
      set => nullOfficeServiceProviderAlert = value;
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
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public External Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of RestartOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("restartOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert RestartOfficeServiceProviderAlert
    {
      get => restartOfficeServiceProviderAlert ??= new();
      set => restartOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of RestartOffice.
    /// </summary>
    [JsonPropertyName("restartOffice")]
    public Office RestartOffice
    {
      get => restartOffice ??= new();
      set => restartOffice = value;
    }

    /// <summary>
    /// A value of RestartServiceProvider.
    /// </summary>
    [JsonPropertyName("restartServiceProvider")]
    public ServiceProvider RestartServiceProvider
    {
      get => restartServiceProvider ??= new();
      set => restartServiceProvider = value;
    }

    /// <summary>
    /// A value of RestartOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("restartOfficeServiceProvider")]
    public OfficeServiceProvider RestartOfficeServiceProvider
    {
      get => restartOfficeServiceProvider ??= new();
      set => restartOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of WorkEabFileHandling.
    /// </summary>
    [JsonPropertyName("workEabFileHandling")]
    public EabFileHandling WorkEabFileHandling
    {
      get => workEabFileHandling ??= new();
      set => workEabFileHandling = value;
    }

    /// <summary>
    /// A value of WorkProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("workProgramCheckpointRestart")]
    public ProgramCheckpointRestart WorkProgramCheckpointRestart
    {
      get => workProgramCheckpointRestart ??= new();
      set => workProgramCheckpointRestart = value;
    }

    /// <summary>
    /// A value of WorkProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("workProgramProcessingInfo")]
    public ProgramProcessingInfo WorkProgramProcessingInfo
    {
      get => workProgramProcessingInfo ??= new();
      set => workProgramProcessingInfo = value;
    }

    /// <summary>
    /// A value of WorkCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkCsePersonsWorkSet
    {
      get => workCsePersonsWorkSet ??= new();
      set => workCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of WorkOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("workOfficeServiceProvider")]
    public OfficeServiceProvider WorkOfficeServiceProvider
    {
      get => workOfficeServiceProvider ??= new();
      set => workOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of WorkOffice.
    /// </summary>
    [JsonPropertyName("workOffice")]
    public Office WorkOffice
    {
      get => workOffice ??= new();
      set => workOffice = value;
    }

    /// <summary>
    /// A value of WorkOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("workOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert WorkOfficeServiceProviderAlert
    {
      get => workOfficeServiceProviderAlert ??= new();
      set => workOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of WorkInfrastructure.
    /// </summary>
    [JsonPropertyName("workInfrastructure")]
    public Infrastructure WorkInfrastructure
    {
      get => workInfrastructure ??= new();
      set => workInfrastructure = value;
    }

    private DateWorkArea workDateWorkArea;
    private OfficeServiceProviderAlert beginRangeOfficeServiceProviderAlert;
    private OfficeServiceProviderAlert endRangeOfficeServiceProviderAlert;
    private OfficeServiceProviderAlert beginParmRange;
    private OfficeServiceProviderAlert endParmRange;
    private DateWorkArea beginRangeDateWorkArea;
    private DateWorkArea endRangeDateWorkArea;
    private DateWorkArea run;
    private OfficeServiceProviderAlert purge;
    private Common position;
    private Common bypassWritingToExtract;
    private Common alertsWithHistory;
    private Common manualAlerts;
    private Common alertsWithNoHistory;
    private Common printLoopControl;
    private Common checkpointControl;
    private Common commitControl;
    private Common alertsWrittenToExtract;
    private Common alertsRead;
    private Common alertsPurged;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private OfficeServiceProvider nullOfficeServiceProvider;
    private Office nullOffice;
    private OfficeServiceProviderAlert nullOfficeServiceProviderAlert;
    private Infrastructure nullInfrastructure;
    private External commit;
    private OfficeServiceProviderAlert restartOfficeServiceProviderAlert;
    private Office restartOffice;
    private ServiceProvider restartServiceProvider;
    private OfficeServiceProvider restartOfficeServiceProvider;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private EabFileHandling workEabFileHandling;
    private ProgramCheckpointRestart workProgramCheckpointRestart;
    private ProgramProcessingInfo workProgramProcessingInfo;
    private CsePersonsWorkSet workCsePersonsWorkSet;
    private OfficeServiceProvider workOfficeServiceProvider;
    private Office workOffice;
    private OfficeServiceProviderAlert workOfficeServiceProviderAlert;
    private Infrastructure workInfrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert ExistingOfficeServiceProviderAlert
    {
      get => existingOfficeServiceProviderAlert ??= new();
      set => existingOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    private Infrastructure existingInfrastructure;
    private OfficeServiceProviderAlert existingOfficeServiceProviderAlert;
    private Office existingOffice;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
  }
#endregion
}

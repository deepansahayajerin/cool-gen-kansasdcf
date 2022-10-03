// Program: LE_B579_DOL_IWO_GENERATION, ID: 945100649, model: 746.
// Short name: SWEL579B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B579_DOL_IWO_GENERATION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB579DolIwoGeneration: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B579_DOL_IWO_GENERATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB579DolIwoGeneration(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB579DolIwoGeneration.
  /// </summary>
  public LeB579DolIwoGeneration(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 05/14/12  GVandy            CQ33628    Initial Development
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // This program takes a notice file from Kansas Department of Labor (KDOL) 
    // containing
    // people for whom they would like an ORDIWO2B notice generated.  For each 
    // person
    // on this file who we certified on our most recent DOL UI certification we 
    // will
    // call the auto IWO action block to create these notices with DOL as the 
    // income
    // withholder.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ------------------------------------------------------------------------------
    // -- Read the PPI record.
    // ------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------------------
    // -- Read for restart info.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------------------
    // -- Open the error report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Open the control report.
    // ------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Control Report. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Pre-edit the KDOL file for record types, footer totals, etc.
    // ------------------------------------------------------------------------------
    UseLeB579PreEditDolFile();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------------------
    // -- Open the UI IWO Notice file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadKdolIwoNoticeInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening KDOL IWO Notice file. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ------------------------------------------------------------------------------
      // -- Extract the restart information.
      // ------------------------------------------------------------------------------
      // -- Restart info format
      //    Pos 001 - 009  Restart Record SSN
      //    Pos 010        Blank
      //    Pos 011 - 019  Restart Record Count
      //    Pos 020        Blank
      //    Pos 021 - 029  Matched NCP count
      //    Pos 030        Blank
      //    Pos 031 - 039  Error count
      //    Pos 040 - 040  Blank
      //    pos 041 - 048  KDOL File Creation Date
      local.RestartPerson.Ssn =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 9);
      local.RecordsRead.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 11, 9));
      local.MatchedNcp.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 21, 9));
      local.Error.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 31, 9));
      local.RestartKdolFileDate.TextDate =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 41, 8);

      // ------------------------------------------------------------------------------
      // -- Write restart info to control report.
      // ------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 9; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Program restarting.  Previous checkpoint info below.";

            break;
          case 2:
            local.EabReportSend.RptDetail = "        KDOL File SSN  :  " + local
              .RestartPerson.Ssn;

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "        Number Of KDOL Detail Records Read  :  " + NumberToString
              (local.RecordsRead.Count, 15);

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "        Number Of UI Certified NCP Matches  :  " + NumberToString
              (local.MatchedNcp.Count, 15);

            break;
          case 5:
            local.EabReportSend.RptDetail =
              "        Number Of UI Certified NCPs For Which Income Sources Could Not Be Created  :  " +
              NumberToString(local.Error.Count, 15);

            break;
          case 6:
            local.EabReportSend.RptDetail =
              "        KDOL File Creation Date  :  " + local
              .RestartKdolFileDate.TextDate;

            break;
          case 7:
            local.EabReportSend.RptDetail = "-- End of restart information  --";

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing restart info to the control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // ------------------------------------------------------------------------------
      // -- Re-position the KDOL file for restart.  (The restart record count is
      // the
      // -- number of Detail records read plus 1 to account for the header 
      // header record).
      // ------------------------------------------------------------------------------
      local.Common.Count = 1;

      for(var limit = local.RecordsRead.Count + 1; local.Common.Count <= limit; ++
        local.Common.Count)
      {
        local.EabFileHandling.Action = "READ";
        UseLeEabReadKdolIwoNoticeInfo1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          if (Equal(local.EabFileHandling.Status, "EF"))
          {
            local.EabReportSend.RptDetail =
              "End of file reached while repositioning for restart.  Restart info = " +
              local.EabFileHandling.Status;
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Error reading KDOL IWO Notice file for restart. Status = " + local
              .EabFileHandling.Status;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // -- Insure that the file creation date is the same as the checkpointed 
      // file creation date.
      if (!Equal(local.KdolFileCreationDate.TextDate,
        local.RestartKdolFileDate.TextDate))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "KDOL file date " + local
          .KdolFileCreationDate.TextDate + " does not match checkpointed file date " +
          local.RestartKdolFileDate.TextDate + ".  Checkpointed file did not complete processing!!!";
          
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Insure that the SSN at the restart position matches the checkpointed
      // SSN.
      if (!Equal(local.RestartPerson.Ssn,
        local.KdolUiInboundFile.NewClaimantRecord, 2, 9))
      {
        for(local.Common.Count = 1; local.Common.Count <= 8; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail =
                "SSN at restart position does not match checkpointed SSN.";

              break;
            case 2:
              local.EabReportSend.RptDetail = "     File SSN = " + Substring
                (local.KdolUiInboundFile.NewClaimantRecord,
                KdolUiInboundFile.NewClaimantRecord_MaxLength, 2, 9);

              break;
            case 3:
              local.EabReportSend.RptDetail = "     Checkpoint SSN = " + local
                .RestartPerson.Ssn;

              break;
            case 4:
              local.EabReportSend.RptDetail =
                "     KDOL File Record Number = " + NumberToString
                ((long)local.RecordsRead.Count + 1, 8, 8);

              break;
            default:
              local.EabReportSend.RptDetail = "";

              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      // ------------------------------------------------------------------------------
      // -- Insure the KDOL file creation date is greater than the creation date
      // of the
      // -- last file processed.
      // ------------------------------------------------------------------------------
      if (!Lt(Substring(local.ProgramProcessingInfo.ParameterList, 15, 8),
        local.KdolFileCreationDate.TextDate))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "KDOL file date " + local
          .KdolFileCreationDate.TextDate + " is less than or equal to the most recently processed file date " +
          Substring(local.ProgramProcessingInfo.ParameterList, 15, 8) + ".  File should have a more recent date.";
          
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Determine the last date that we certified for DOL UI withholding.
    // ------------------------------------------------------------------------------
    ReadDolUiWithholding2();

    // ------------------------------------------------------------------------------
    // -- Find the Department of Labor employer record.  (EIN=621444754)
    // ------------------------------------------------------------------------------
    if (!ReadEmployer())
    {
      ExitState = "LE0000_DOL_EMPLOYER_NOT_FOUND";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Extract parms and log to the control report.
    // --
    // --   PPI parms are in the format
    // --
    // --   9999 99999 XX
    // --
    // --   where positions  1 -  4 are the office number
    // --         positions  6 - 10 are the service provider number
    // --         positions 12 - 13 are the office service provider role code
    // --         positions 15 - 22 are the KDOL creation date of the last file 
    // processed
    // --         positions 24 - 27 are the central office default atty office 
    // number
    // --         positions 29 - 33 are the central office default atty service 
    // provider num
    // --         positions 35 - 36 are the central office default atty office 
    // service provider role code
    // --
    // ------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 13)))
    {
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "No special caseload for incoming interstate cases specified on the PPI record.";
            
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing PPI Parms to the control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.InterstateCaseloadOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 4));
      local.InterstateCaseloadServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 6, 5));
      local.InterstateCaseloadOfficeServiceProvider.RoleCode =
        Substring(local.ProgramProcessingInfo.ParameterList, 12, 2);

      // cq47223 Added the following parms being so the that the central office 
      // default attorney can bee passed in and assigned to legal action when it
      // is not a KS tribunal and there is not one already assigned.
      local.CentralOffDefaultAttyOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 24, 4));
      local.CentralOffDefaultAttyServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 29, 5));
      local.CentralOffDefaultAttyOfficeServiceProvider.RoleCode =
        Substring(local.ProgramProcessingInfo.ParameterList, 35, 2);

      for(local.Common.Count = 1; local.Common.Count <= 6; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "PPI special caseload for incoming interstate cases:";

            break;
          case 2:
            local.EabReportSend.RptDetail = "     Office: " + Substring
              (local.ProgramProcessingInfo.ParameterList, 1, 4);

            break;
          case 3:
            local.EabReportSend.RptDetail = "     Service Provider: " + Substring
              (local.ProgramProcessingInfo.ParameterList, 6, 5);

            break;
          case 4:
            local.EabReportSend.RptDetail = "     Role Code: " + Substring
              (local.ProgramProcessingInfo.ParameterList, 12, 2);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing PPI Parms to the control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Process each IWO Notice record from DOL triggering auto IWOs (ORDIWO2B
    // -- document) for each NCP we are currently certifying for UI IWO.
    // ------------------------------------------------------------------------------
    do
    {
      // ------------------------------------------------------------------------------
      // -- Read an IWO Notice record from DOL.
      // ------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseLeEabReadKdolIwoNoticeInfo1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        if (Equal(local.EabFileHandling.Status, "EF"))
        {
          continue;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading KDOL IWO Notice file. Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (CharAt(local.KdolUiInboundFile.NewClaimantRecord, 1) == '2')
      {
        // -- We already pre-edited the file and validated the record types and 
        // totals.  Just
        // -- process the type '2' (Detail) records here.  Continue.
      }
      else
      {
        continue;
      }

      ++local.RecordsRead.Count;
      ++local.RecordsSinceLastCommit.Count;

      // ------------------------------------------------------------------------------
      // -- Separate file record into local views.
      // ------------------------------------------------------------------------------
      local.CsePersonsWorkSet.Ssn =
        Substring(local.KdolUiInboundFile.NewClaimantRecord, 2, 9);

      // ------------------------------------------------------------------------------
      // -- Determine if we certified this person on our most recent DOL UI 
      // certification.
      // ------------------------------------------------------------------------------
      if (ReadDolUiWithholding1())
      {
        ++local.MatchedNcp.Count;
        local.CsePerson.Number = entities.DolUiWithholding.CsePersonNumber;

        // ------------------------------------------------------------------------------
        // -- We certified this person on our most recent DOL UI certification.
        // Create
        // -- DOL as an income source for this person and trigger the auto IWOs.
        // ------------------------------------------------------------------------------
        UseLeCreateUiEmpIncomeSource();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.Error.Count;
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Unable to create Income Source for person: " + local
            .CsePerson.Number + " Error: " + local.ExitStateWorkArea.Message;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      if (local.RecordsSinceLastCommit.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -- Restart info format
        //    Pos 001 - 009  Restart Record SSN
        //    Pos 010        Blank
        //    Pos 011 - 019  Restart Record Count
        //    Pos 020        Blank
        //    Pos 021 - 029  Matched NCP count
        //    Pos 030        Blank
        //    Pos 031 - 039  Error count
        //    Pos 040 - 040  Blank
        //    pos 041 - 048  KDOL File Creation Date
        local.ProgramCheckpointRestart.RestartInfo =
          local.CsePersonsWorkSet.Ssn + " " + NumberToString
          (local.RecordsRead.Count, 7, 9) + " " + NumberToString
          (local.MatchedNcp.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (local.Error.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + local
          .KdolFileCreationDate.TextDate;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.RecordsSinceLastCommit.Count = 0;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // ------------------------------------------------------------------------------
    // -- Log counts to the control report.
    // ------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Total Number Of KDOL Detail Records Read  :  " + NumberToString
            (local.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Total Number Of UI Certified NCP Matches  :  " + NumberToString
            (local.MatchedNcp.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total Number Of UI Certified NCPs For Which Income Sources Could Not Be Created  :  " +
            NumberToString(local.Error.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing totals to the control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Store the KDOL file creation date in Positions 15 - 22 of the PPI 
    // record.
    // -- On the next run we'll use this date to insure that file being 
    // processed was
    // -- created after this file.  This will insure we don't re-process a file 
    // or that
    // -- we get a wrong file on the FTP somehow.
    // ------------------------------------------------------------------------------
    local.ProgramProcessingInfo.ParameterList =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 14) + local
      .KdolFileCreationDate.TextDate + Substring
      (local.ProgramProcessingInfo.ParameterList, 23, 15);
    UseUpdateProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error adding KDOL file creation date to PPI record.  " + " ";
      UseCabErrorReport2();
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

    // ------------------------------------------------------------------------------
    // -- Close the IWO Notice file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadKdolIwoNoticeInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing KDOL IWO Notice file. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the control report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing control report.  Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the error report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

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

  private void UseLeB579PreEditDolFile()
  {
    var useImport = new LeB579PreEditDolFile.Import();
    var useExport = new LeB579PreEditDolFile.Export();

    Call(LeB579PreEditDolFile.Execute, useImport, useExport);

    local.KdolFileCreationDate.TextDate =
      useExport.KdolFileCreationDate.TextDate;
  }

  private void UseLeCreateUiEmpIncomeSource()
  {
    var useImport = new LeCreateUiEmpIncomeSource.Import();
    var useExport = new LeCreateUiEmpIncomeSource.Export();

    useImport.Employer.Identifier = entities.Employer.Identifier;
    useImport.InterstateCaseloadServiceProvider.SystemGeneratedId =
      local.InterstateCaseloadServiceProvider.SystemGeneratedId;
    useImport.InterstateCaseloadOfficeServiceProvider.RoleCode =
      local.InterstateCaseloadOfficeServiceProvider.RoleCode;
    useImport.InterstateCaseloadOffice.SystemGeneratedId =
      local.InterstateCaseloadOffice.SystemGeneratedId;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.CentralOffDefaultAttyServiceProvider.SystemGeneratedId =
      local.CentralOffDefaultAttyServiceProvider.SystemGeneratedId;
    useImport.CentralOffDefaultAttyOffice.SystemGeneratedId =
      local.CentralOffDefaultAttyOffice.SystemGeneratedId;
    useImport.CentralOffDefaultAttyOfficeServiceProvider.RoleCode =
      local.CentralOffDefaultAttyOfficeServiceProvider.RoleCode;

    Call(LeCreateUiEmpIncomeSource.Execute, useImport, useExport);
  }

  private void UseLeEabReadKdolIwoNoticeInfo1()
  {
    var useImport = new LeEabReadKdolIwoNoticeInfo.Import();
    var useExport = new LeEabReadKdolIwoNoticeInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.KdolUiInboundFile.NewClaimantRecord =
      local.KdolUiInboundFile.NewClaimantRecord;

    Call(LeEabReadKdolIwoNoticeInfo.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.KdolUiInboundFile.NewClaimantRecord =
      useExport.KdolUiInboundFile.NewClaimantRecord;
  }

  private void UseLeEabReadKdolIwoNoticeInfo2()
  {
    var useImport = new LeEabReadKdolIwoNoticeInfo.Import();
    var useExport = new LeEabReadKdolIwoNoticeInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadKdolIwoNoticeInfo.Execute, useImport, useExport);

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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadDolUiWithholding1()
  {
    entities.DolUiWithholding.Populated = false;

    return Read("ReadDolUiWithholding1",
      (db, command) =>
      {
        db.SetString(command, "ssn", local.CsePersonsWorkSet.Ssn);
        db.SetDate(
          command, "iwoCertDate",
          local.DolUiWithholding.WithholdingCertificationDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DolUiWithholding.CsePersonNumber = db.GetString(reader, 0);
        entities.DolUiWithholding.WithholdingCertificationDate =
          db.GetDate(reader, 1);
        entities.DolUiWithholding.StandardNumber = db.GetString(reader, 2);
        entities.DolUiWithholding.SocialSecurityNumber =
          db.GetString(reader, 3);
        entities.DolUiWithholding.Populated = true;
      });
  }

  private bool ReadDolUiWithholding2()
  {
    local.DolUiWithholding.Populated = false;

    return Read("ReadDolUiWithholding2",
      null,
      (db, reader) =>
      {
        local.DolUiWithholding.WithholdingCertificationDate =
          db.GetDate(reader, 0);
        local.DolUiWithholding.Populated = true;
      });
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      null,
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.Populated = true;
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
    /// A value of CentralOffDefaultAttyServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyServiceProvider")]
    public ServiceProvider CentralOffDefaultAttyServiceProvider
    {
      get => centralOffDefaultAttyServiceProvider ??= new();
      set => centralOffDefaultAttyServiceProvider = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOffice.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOffice")]
    public Office CentralOffDefaultAttyOffice
    {
      get => centralOffDefaultAttyOffice ??= new();
      set => centralOffDefaultAttyOffice = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOfficeServiceProvider")]
    public OfficeServiceProvider CentralOffDefaultAttyOfficeServiceProvider
    {
      get => centralOffDefaultAttyOfficeServiceProvider ??= new();
      set => centralOffDefaultAttyOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadServiceProvider")]
    public ServiceProvider InterstateCaseloadServiceProvider
    {
      get => interstateCaseloadServiceProvider ??= new();
      set => interstateCaseloadServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOfficeServiceProvider")]
    public OfficeServiceProvider InterstateCaseloadOfficeServiceProvider
    {
      get => interstateCaseloadOfficeServiceProvider ??= new();
      set => interstateCaseloadOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOffice.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOffice")]
    public Office InterstateCaseloadOffice
    {
      get => interstateCaseloadOffice ??= new();
      set => interstateCaseloadOffice = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of KdolUiInboundFile.
    /// </summary>
    [JsonPropertyName("kdolUiInboundFile")]
    public KdolUiInboundFile KdolUiInboundFile
    {
      get => kdolUiInboundFile ??= new();
      set => kdolUiInboundFile = value;
    }

    /// <summary>
    /// A value of MatchedNcp.
    /// </summary>
    [JsonPropertyName("matchedNcp")]
    public Common MatchedNcp
    {
      get => matchedNcp ??= new();
      set => matchedNcp = value;
    }

    /// <summary>
    /// A value of RestartPerson.
    /// </summary>
    [JsonPropertyName("restartPerson")]
    public CsePersonsWorkSet RestartPerson
    {
      get => restartPerson ??= new();
      set => restartPerson = value;
    }

    /// <summary>
    /// A value of RecordsSinceLastCommit.
    /// </summary>
    [JsonPropertyName("recordsSinceLastCommit")]
    public Common RecordsSinceLastCommit
    {
      get => recordsSinceLastCommit ??= new();
      set => recordsSinceLastCommit = value;
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
    /// A value of DolUiWithholding.
    /// </summary>
    [JsonPropertyName("dolUiWithholding")]
    public DolUiWithholding DolUiWithholding
    {
      get => dolUiWithholding ??= new();
      set => dolUiWithholding = value;
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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
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
    /// A value of RestartKdolFileDate.
    /// </summary>
    [JsonPropertyName("restartKdolFileDate")]
    public DateWorkArea RestartKdolFileDate
    {
      get => restartKdolFileDate ??= new();
      set => restartKdolFileDate = value;
    }

    /// <summary>
    /// A value of KdolFileCreationDate.
    /// </summary>
    [JsonPropertyName("kdolFileCreationDate")]
    public DateWorkArea KdolFileCreationDate
    {
      get => kdolFileCreationDate ??= new();
      set => kdolFileCreationDate = value;
    }

    private ServiceProvider centralOffDefaultAttyServiceProvider;
    private Office centralOffDefaultAttyOffice;
    private OfficeServiceProvider centralOffDefaultAttyOfficeServiceProvider;
    private ServiceProvider interstateCaseloadServiceProvider;
    private OfficeServiceProvider interstateCaseloadOfficeServiceProvider;
    private Office interstateCaseloadOffice;
    private Common error;
    private ExitStateWorkArea exitStateWorkArea;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private KdolUiInboundFile kdolUiInboundFile;
    private Common matchedNcp;
    private CsePersonsWorkSet restartPerson;
    private Common recordsSinceLastCommit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DolUiWithholding dolUiWithholding;
    private Common common;
    private Common recordsRead;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea restartKdolFileDate;
    private DateWorkArea kdolFileCreationDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of DolUiWithholding.
    /// </summary>
    [JsonPropertyName("dolUiWithholding")]
    public DolUiWithholding DolUiWithholding
    {
      get => dolUiWithholding ??= new();
      set => dolUiWithholding = value;
    }

    private Employer employer;
    private DolUiWithholding dolUiWithholding;
  }
#endregion
}

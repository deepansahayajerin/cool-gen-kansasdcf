// Program: LE_B577_KDOL_UI_IWO_MATCH, ID: 945091618, model: 746.
// Short name: SWEL577B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B577_KDOL_UI_IWO_MATCH.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB577KdolUiIwoMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B577_KDOL_UI_IWO_MATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB577KdolUiIwoMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB577KdolUiIwoMatch.
  /// </summary>
  public LeB577KdolUiIwoMatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 05/15/2012      RMathews   	Initial Creation - CQ#33628
    // 06/05/2012      RMathews        Modified to enforce Saturday extract date
    // 07/11/2012      RMathews        Modified read for max certification date
    // 03/05/2015      RMathews        Corrected exception handling in program
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWELB577";
    local.Restart.Flag = "N";

    // ---------------------------------------------------------------------------
    // Read program processing and checkpoint restart tables
    // ---------------------------------------------------------------------------
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!IsEmpty(local.ProgramCheckpointRestart.RestartInfo))
    {
      local.RestartSection.Text1 =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 1);
      local.RestartPerson.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 2, 10);
      local.RestartApRead.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 12, 15));
      local.Restart.Flag = "Y";
      local.StartRead.Number = local.RestartPerson.Number;
      local.NumberOfRecordsRead.Count = local.RestartApRead.Count;
    }
    else
    {
      local.StartRead.Number = "";
      local.NumberOfRecordsRead.Count = 0;
    }

    // ---------------------------------------------------------------------------
    // Open control and error reports
    // ---------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------------------------
    // Edit for missing parameter list, and parse interstate caseworker info, 
    // error
    // processing flag and optional extract override date
    // ---------------------------------------------------------------------------------
    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Job parameters are missing from PPI.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.InterstateOffice.SystemGeneratedId =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 1, 4));
    local.InterstateSpd.SystemGeneratedId =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 6, 5));
    local.InterstateRole.RoleCode =
      Substring(local.ProgramProcessingInfo.ParameterList, 12, 2);
    local.DisplayErrors.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 15, 1);
    local.Extract1.TextDate =
      Substring(local.ProgramProcessingInfo.ParameterList, 17, 8);

    // -----------------------------------------------------------------------------------
    // Shut down if any interstate caseworker parms are missing from PPI
    // -----------------------------------------------------------------------------------
    if (local.InterstateOffice.SystemGeneratedId <= 0 || local
      .InterstateSpd.SystemGeneratedId <= 0 || IsEmpty
      (local.InterstateRole.RoleCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Interstate caseworker parameters are missing from PPI.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------------------------
    // Check for override extract date and calculate next Saturday date if the
    // parameter is not supplied
    // ---------------------------------------------------------------------------------
    if (IsEmpty(local.Extract1.TextDate))
    {
      ReadDolUiWithholding3();

      if (AsChar(local.Restart.Flag) == 'Y')
      {
        // --- For restart, set extract date to the last date entries were added
        // to the table
        local.Extract1.Date = local.Max.WithholdingCertificationDate;
      }
      else
      {
        // --- Otherwise calculate the next Saturday date to use
        local.DayOfWeek.Text10 =
          DayOfWeek(local.Max.WithholdingCertificationDate);

        switch(TrimEnd(local.DayOfWeek.Text10))
        {
          case "SATURDAY":
            local.Extract1.Date =
              AddDays(local.Max.WithholdingCertificationDate, 7);

            break;
          case "SUNDAY":
            local.Extract1.Date =
              AddDays(local.Max.WithholdingCertificationDate, 6);

            break;
          case "MONDAY":
            local.Extract1.Date =
              AddDays(local.Max.WithholdingCertificationDate, 5);

            break;
          case "TUESDAY":
            local.Extract1.Date =
              AddDays(local.Max.WithholdingCertificationDate, 4);

            break;
          case "WEDNESDAY":
            local.Extract1.Date =
              AddDays(local.Max.WithholdingCertificationDate, 3);

            break;
          case "THURSDAY":
            local.Extract1.Date =
              AddDays(local.Max.WithholdingCertificationDate, 2);

            break;
          case "FRIDAY":
            local.Extract1.Date =
              AddDays(local.Max.WithholdingCertificationDate, 1);

            break;
          default:
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Unable to calculate next Saturday extract date";
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
        }
      }

      local.Extract1.TextDate =
        NumberToString(DateToInt(local.Extract1.Date), 8, 8);
    }
    else
    {
      local.Extract1.Date =
        IntToDate((int)StringToNumber(local.Extract1.TextDate));
    }

    // -----------------------------------------------------------------------------
    // Check DOL_UI_WITHHOLDING file for records with same process date. None 
    // should
    // exist unless program has been restarted.
    // -----------------------------------------------------------------------------
    if (AsChar(local.Restart.Flag) != 'Y')
    {
      if (ReadDolUiWithholding2())
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Records already exist on DOL_UI_WITHHOLDING file for extract date = " +
          local.Extract1.TextDate;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ---------------------------------------------------------------------------------
    // Main cursor - Section A - process NCP's with WA (arrears) and/or WC (
    // current) obligations
    // ---------------------------------------------------------------------------------
    if (AsChar(local.RestartSection.Text1) == 'A' || AsChar
      (local.Restart.Flag) != 'Y')
    {
      foreach(var item in ReadCsePerson())
      {
        ++local.NumberOfRecordsRead.Count;
        local.StartCsePersonsWorkSet.Number = entities.CsePerson.Number;
        local.CsePerson.Number = entities.CsePerson.Number;
        UseSiReadCsePersonBatch();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Abending: Error in si_read_cse_person_batch for person # " + local
            .StartCsePersonsWorkSet.Number + "  Exit state = " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // --- Log error if SSN is missing and error display is turned on
        if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
          (local.CsePersonsWorkSet.Ssn, "000000000"))
        {
          if (AsChar(local.DisplayErrors.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.CsePersonsWorkSet.Number + "-"
              + local.CsePersonsWorkSet.LastName + " is missing a valid SSN";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          continue;
        }

        // -----------------------------------------------------------------------------
        // Call modified auto IWO routine to certify NCP and return associated 
        // court
        // orders and amounts
        // 3-10-15 Client will be skipped and a message written to the error 
        // file when a
        // bad exit state is returned from this CAB.
        // -----------------------------------------------------------------------------
        UseLeAutomaticIwoForUi();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error in le_automatic_iwo_for_ui.  Following person # skipped: " +
            local.CsePersonsWorkSet.Number + "  Exit state = " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }

        local.Extract.Index = 0;

        for(var limit = local.Extract.Count; local.Extract.Index < limit; ++
          local.Extract.Index)
        {
          if (!local.Extract.CheckSize())
          {
            break;
          }

          // --------------------------------------------------------------------------------
          // Add record for each court order that meets criteria to 
          // DOL_UI_WITHHOLDING table
          // --------------------------------------------------------------------------------
          if (local.Extract.Item.Amounts.ArrearsAmountOwed == 0 && local
            .Extract.Item.Amounts.CurrentAmountOwed == 0)
          {
            // --- Do not write records to DOL_UI_IWO when both WA & WC amounts 
            // are zero
          }
          else
          {
            try
            {
              CreateDolUiWithholding();
              ++local.RestartCount.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Abend adding record to dol_ui_withholding. Record already exists for cse person number= " +
                    local.Extract.Item.Client.Number;
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.PermittedValueViolation:
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Abend adding record to dol_ui_withholding. Permitted value violation for cse person number= " +
                    local.Extract.Item.Client.Number;
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        local.Extract.CheckIndex();

        // -----------------------------------------------------------------------------
        // Check if error information is to be printed and generate detail error
        // list
        // if requested
        // -----------------------------------------------------------------------------
        if (AsChar(local.DisplayErrors.Flag) == 'Y')
        {
          for(local.ErrorMessages.Index = 0; local.ErrorMessages.Index < local
            .ErrorMessages.Count; ++local.ErrorMessages.Index)
          {
            if (!local.ErrorMessages.CheckSize())
            {
              break;
            }

            // -------------------------------------------------------------------------------
            // Write line to error report for each court order that did not pass
            // certification edits
            // -------------------------------------------------------------------------------
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.CsePersonsWorkSet.Number + "-"
              + local.CsePersonsWorkSet.LastName + "  " + (
                local.ErrorMessages.Item.ErrorCourtCase.StandardNumber ?? ""
              ) + "  " + TrimEnd(local.ErrorMessages.Item.ErrorType.Text50);
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          local.ErrorMessages.CheckIndex();
        }

        // ----------------------------------------------------------------------------------
        // Check to see if it's time for a commit
        // ----------------------------------------------------------------------------------
        if (local.RestartCount.Count > local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.ProgramCheckpointRestart.RestartInfo = "A" + local
            .StartCsePersonsWorkSet.Number + NumberToString
            (local.NumberOfRecordsRead.Count, 15);
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Abending because of error on checkpoint restart update for commit. Exit state message = " +
              local.ExitStateWorkArea.Message;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.RestartCount.Count = 0;
        }
      }
    }

    // ----------------------------------------------------------------------------
    // Section B - Read DOL_UI_WITHHOLDING by process date, looking and deleting
    // dup ssn
    // ----------------------------------------------------------------------------
    local.Ssn.Count = 0;
    local.Ssn.Index = -1;

    foreach(var item in ReadDolUiWithholding5())
    {
      if (Equal(local.TopAmountDolUiWithholding.SocialSecurityNumber,
        entities.DolUiWithholding.SocialSecurityNumber) && !
        Equal(local.TopAmountDolUiWithholding.CsePersonNumber,
        entities.DolUiWithholding.CsePersonNumber))
      {
        ++local.Ssn.Index;
        local.Ssn.CheckSize();

        local.Ssn.Update.Ssn1.Assign(entities.DolUiWithholding);
      }
      else if (local.Ssn.Count > 0)
      {
        local.Ssn.Index = 0;

        for(var limit = local.Ssn.Count; local.Ssn.Index < limit; ++
          local.Ssn.Index)
        {
          if (!local.Ssn.CheckSize())
          {
            break;
          }

          if (ReadDolUiWithholding1())
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.N2dRead.CsePersonNumber + "- court order number " +
              entities.N2dRead.StandardNumber + "  duplicate SSN";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            DeleteDolUiWithholding();
          }
        }

        local.Ssn.CheckIndex();

        for(local.Ssn.Index = 0; local.Ssn.Index < local.Ssn.Count; ++
          local.Ssn.Index)
        {
          if (!local.Ssn.CheckSize())
          {
            break;
          }

          local.Ssn.Update.Ssn1.Assign(local.Null1);
        }

        local.Ssn.CheckIndex();
        local.Ssn.Count = 0;
        local.Ssn.Index = -1;
        local.TopAmountDolUiWithholding.Assign(entities.DolUiWithholding);
        local.TopAmountCommon.TotalCurrency =
          entities.DolUiWithholding.WaAmount.GetValueOrDefault() + entities
          .DolUiWithholding.WcAmount.GetValueOrDefault();
      }
      else
      {
        local.TopAmountDolUiWithholding.Assign(entities.DolUiWithholding);
      }
    }

    // ----------------------------------------------------------------------------
    // Section C - Read DOL_UI_WITHHOLDING file for process date, format and 
    // write
    // one record per obligor on the KDOL file with current and arrears amounts
    // summarized
    // ----------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // Call external CAB to open the KDOL ouptput file
    // ---------------------------------------------------------------------------
    local.PassArea.FileInstruction = "OPEN";
    UseLeEabWriteKdolMatchFile4();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening KDOR output file. Return code = " + local
        .PassArea.TextReturnCode;
      UseCabErrorReport2();
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.PrevUi.Number = "";
    local.Total.TotalCurrency = 0;
    local.TotalCurrent.TotalCurrency = 0;
    local.TotalArrears.TotalCurrency = 0;
    local.KdolDetail.Count = 0;

    // --- Write header record to KDOL output file
    local.KdolRecType.Text1 = "1";
    local.EabExtract.TextDate = local.Extract1.TextDate;
    local.PassArea.FileInstruction = "WRITE";
    UseLeEabWriteKdolMatchFile1();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing header record to KDOL file. Return code = " + local
        .PassArea.TextReturnCode;
      UseCabErrorReport2();
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    foreach(var item in ReadDolUiWithholding4())
    {
      if (IsEmpty(local.PrevUi.Number))
      {
        // --- First time through, store values in hold area for comparison
        local.HoldDolUiWithholding.FirstName =
          entities.DolUiWithholding.FirstName;
        local.HoldDolUiWithholding.CsePersonNumber =
          entities.DolUiWithholding.CsePersonNumber;
        local.HoldDolUiWithholding.LastName =
          entities.DolUiWithholding.LastName;
        local.HoldDolUiWithholding.MaxWithholdingPercent =
          entities.DolUiWithholding.MaxWithholdingPercent;
        local.HoldDolUiWithholding.MiddleInitial =
          entities.DolUiWithholding.MiddleInitial;
        local.HoldDolUiWithholding.SocialSecurityNumber =
          entities.DolUiWithholding.SocialSecurityNumber;
        local.HoldDolUiWithholding.StandardNumber =
          entities.DolUiWithholding.StandardNumber;
        local.HoldDolUiWithholding.WaAmount =
          entities.DolUiWithholding.WaAmount;
        local.HoldDolUiWithholding.WcAmount =
          entities.DolUiWithholding.WcAmount;
        local.HoldDolUiWithholding.WithholdingCertificationDate =
          entities.DolUiWithholding.WithholdingCertificationDate;
        local.TotalCurrent.TotalCurrency += entities.DolUiWithholding.WcAmount.
          GetValueOrDefault();
        local.TotalArrears.TotalCurrency += entities.DolUiWithholding.WaAmount.
          GetValueOrDefault();
        local.PrevUi.Number = entities.DolUiWithholding.CsePersonNumber;
      }
      else if (Equal(local.HoldDolUiWithholding.CsePersonNumber,
        entities.DolUiWithholding.CsePersonNumber))
      {
        // --- Record read is for same person, just add to toal current & 
        // arrears amounts
        local.TotalCurrent.TotalCurrency += entities.DolUiWithholding.WcAmount.
          GetValueOrDefault();
        local.TotalArrears.TotalCurrency += entities.DolUiWithholding.WaAmount.
          GetValueOrDefault();
      }
      else
      {
        // --- Record read is for different person.  Format/write KDOL record 
        // from hold area
        local.KdolFile.Ssn = local.HoldDolUiWithholding.SocialSecurityNumber;
        local.KdolFile.LastName = local.HoldDolUiWithholding.LastName ?? Spaces
          (25);
        local.KdolFile.MiddleInitial =
          local.HoldDolUiWithholding.MiddleInitial ?? Spaces(1);
        local.KdolFile.FirstName = local.HoldDolUiWithholding.FirstName ?? Spaces
          (12);
        local.KdolFile.ClientNumber =
          local.HoldDolUiWithholding.CsePersonNumber + "    ";
        local.KdolFile.ExtractDate =
          NumberToString(DateToInt(local.Extract1.Date), 8, 8);
        local.KdolFile.Amount = local.TotalCurrent.TotalCurrency + local
          .TotalArrears.TotalCurrency;
        local.KdolFile.MaxPercent = "000";

        // --- Format KDOL output record and call external if debt greater than 
        // $0
        if (local.KdolFile.Amount == 0)
        {
          if (AsChar(local.DisplayErrors.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.CsePersonsWorkSet.Number + "-"
              + local.CsePersonsWorkSet.LastName + "has $0 total debt";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
        }
        else
        {
          local.KdolRecType.Text1 = "2";
          local.PassArea.FileInstruction = "WRITE";
          UseLeEabWriteKdolMatchFile3();

          if (!Equal(local.PassArea.TextReturnCode, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing detail to KDOL file. Return code = " + local
              .PassArea.TextReturnCode;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.KdolDetail.Count;
          local.Total.TotalCurrency = local.Total.TotalCurrency + local
            .TotalCurrent.TotalCurrency + local.TotalArrears.TotalCurrency;
        }

        // --- Reset amount totals and store current values in hold area
        local.TotalCurrent.TotalCurrency = 0;
        local.TotalArrears.TotalCurrency = 0;
        local.HoldDolUiWithholding.FirstName =
          entities.DolUiWithholding.FirstName;
        local.HoldDolUiWithholding.CsePersonNumber =
          entities.DolUiWithholding.CsePersonNumber;
        local.HoldDolUiWithholding.LastName =
          entities.DolUiWithholding.LastName;
        local.HoldDolUiWithholding.MaxWithholdingPercent =
          entities.DolUiWithholding.MaxWithholdingPercent;
        local.HoldDolUiWithholding.MiddleInitial =
          entities.DolUiWithholding.MiddleInitial;
        local.HoldDolUiWithholding.SocialSecurityNumber =
          entities.DolUiWithholding.SocialSecurityNumber;
        local.HoldDolUiWithholding.StandardNumber =
          entities.DolUiWithholding.StandardNumber;
        local.HoldDolUiWithholding.WaAmount =
          entities.DolUiWithholding.WaAmount;
        local.HoldDolUiWithholding.WcAmount =
          entities.DolUiWithholding.WcAmount;
        local.HoldDolUiWithholding.WithholdingCertificationDate =
          entities.DolUiWithholding.WithholdingCertificationDate;
        local.TotalCurrent.TotalCurrency += entities.DolUiWithholding.WcAmount.
          GetValueOrDefault();
        local.TotalArrears.TotalCurrency += entities.DolUiWithholding.WaAmount.
          GetValueOrDefault();
      }
    }

    // --- Format last KDOL output record and call external
    local.KdolFile.Ssn = local.HoldDolUiWithholding.SocialSecurityNumber;
    local.KdolFile.LastName = local.HoldDolUiWithholding.LastName ?? Spaces(25);
    local.KdolFile.MiddleInitial = local.HoldDolUiWithholding.MiddleInitial ?? Spaces
      (1);
    local.KdolFile.FirstName = local.HoldDolUiWithholding.FirstName ?? Spaces
      (12);
    local.KdolFile.ClientNumber = local.HoldDolUiWithholding.CsePersonNumber + "    ";
      
    local.KdolFile.ExtractDate =
      NumberToString(DateToInt(local.Extract1.Date), 8, 8);
    local.KdolFile.Amount = local.TotalCurrent.TotalCurrency + local
      .TotalArrears.TotalCurrency;
    local.KdolFile.MaxPercent = "000";

    if (local.KdolFile.Amount == 0)
    {
      if (AsChar(local.DisplayErrors.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.CsePersonsWorkSet.Number + "-" + local
          .CsePersonsWorkSet.LastName + "has $0 total debt";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.PassArea.FileInstruction = "WRITE";
      local.KdolRecType.Text1 = "2";
      UseLeEabWriteKdolMatchFile3();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing last detail record to KDOL file. Return code = " + local
          .PassArea.TextReturnCode;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.KdolDetail.Count;
      local.Total.TotalCurrency = local.Total.TotalCurrency + local
        .TotalCurrent.TotalCurrency + local.TotalArrears.TotalCurrency;
    }

    // --- Write footer record to KDOL output file
    local.PassArea.FileInstruction = "WRITE";
    local.KdolRecType.Text1 = "3";
    UseLeEabWriteKdolMatchFile2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing footer record to KDOL file. Return code = " + local
        .PassArea.TextReturnCode;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --- Close KDOL output file
    local.PassArea.FileInstruction = "CLOSE";
    UseLeEabWriteKdolMatchFile4();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing KDOL file. Return code = " + local
        .PassArea.TextReturnCode;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --- Clear restart info on checkpoint restart table
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Abending on final checkpoint restart update. Exit state message = " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --- Clear the extract date parm from the program processing info table
    local.ProgramProcessingInfo.ParameterList =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 15);
    UseUpdateProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Abending on final program processing info update. Exit state message = " +
        local.ExitStateWorkArea.Message;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------------------------
    // Write records to control report
    // ---------------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";

    do
    {
      switch(local.Sub.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Restart person number:  " + local
            .RestartPerson.Number;

          break;
        case 2:
          local.EabReportSend.RptDetail = "Process date:        " + NumberToString
            (DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Extract date:        " + NumberToString
            (DateToInt(local.Extract1.Date), 8, 8);

          break;
        case 4:
          local.EabReportSend.RptDetail = "Interstate provider parms: " + NumberToString
            (local.InterstateOffice.SystemGeneratedId, 12, 4) + " " + NumberToString
            (local.InterstateSpd.SystemGeneratedId, 11, 5) + " " + local
            .InterstateRole.RoleCode;

          break;
        case 5:
          local.EabReportSend.RptDetail = "Error display flag: " + local
            .DisplayErrors.Flag;

          break;
        case 6:
          break;
        case 7:
          break;
        case 8:
          local.EabReportSend.RptDetail =
            "Total number of AP records processed           :  " + NumberToString
            (local.NumberOfRecordsRead.Count, 15);

          break;
        case 9:
          break;
        case 10:
          local.EabReportSend.RptDetail =
            "Total number of KDOL detail records written    :  " + NumberToString
            (local.KdolDetail.Count, 15);

          break;
        case 11:
          break;
        case 12:
          local.EabReportSend.RptDetail =
            "Total dollar amount for KDOL file              :  " + NumberToString
            ((long)(local.Total.TotalCurrency * 100), 1, 13) + "." + NumberToString
            ((long)(local.Total.TotalCurrency * 100), 14, 2);

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writing control report. Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Sub.Count;
      local.EabReportSend.RptDetail = "";
    }
    while(local.Sub.Count <= 12);

    // ----------------------------------------------------------------------------------
    // Close report file and error file
    // ----------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing control report. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveErrorMessages(LeAutomaticIwoForUi.Export.
    ErrorMessagesGroup source, Local.ErrorMessagesGroup target)
  {
    target.ErrorCourtCase.StandardNumber = source.ErrorCourtCase.StandardNumber;
    target.ErrorType.Text50 = source.ErrorType.Text50;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveExtract(LeAutomaticIwoForUi.Export.
    ExtractGroup source, Local.ExtractGroup target)
  {
    target.MaxPct.MaxWithholdingPercent = source.MaxPct.MaxWithholdingPercent;
    target.CourtCase.StandardNumber = source.CourtCase.StandardNumber;
    MoveScreenOwedAmounts(source.Amounts, target.Amounts);
    target.Client.Assign(source.Client);
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private static void MoveScreenOwedAmounts(ScreenOwedAmounts source,
    ScreenOwedAmounts target)
  {
    target.CurrentAmountOwed = source.CurrentAmountOwed;
    target.ArrearsAmountOwed = source.ArrearsAmountOwed;
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

  private void UseLeAutomaticIwoForUi()
  {
    var useImport = new LeAutomaticIwoForUi.Import();
    var useExport = new LeAutomaticIwoForUi.Export();

    useImport.OfficeServiceProvider.RoleCode = local.InterstateRole.RoleCode;
    useImport.ServiceProvider.SystemGeneratedId =
      local.InterstateSpd.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      local.InterstateOffice.SystemGeneratedId;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(LeAutomaticIwoForUi.Execute, useImport, useExport);

    useExport.ErrorMessages.CopyTo(local.ErrorMessages, MoveErrorMessages);
    useExport.Extract.CopyTo(local.Extract, MoveExtract);
  }

  private void UseLeEabWriteKdolMatchFile1()
  {
    var useImport = new LeEabWriteKdolMatchFile.Import();
    var useExport = new LeEabWriteKdolMatchFile.Export();

    useImport.DateWorkArea.TextDate = local.EabExtract.TextDate;
    useImport.RecType.Text1 = local.KdolRecType.Text1;
    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(LeEabWriteKdolMatchFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseLeEabWriteKdolMatchFile2()
  {
    var useImport = new LeEabWriteKdolMatchFile.Import();
    var useExport = new LeEabWriteKdolMatchFile.Export();

    useImport.TotalAmount.TotalCurrency = local.Total.TotalCurrency;
    useImport.TotalCount.Count = local.KdolDetail.Count;
    useImport.RecType.Text1 = local.KdolRecType.Text1;
    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(LeEabWriteKdolMatchFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseLeEabWriteKdolMatchFile3()
  {
    var useImport = new LeEabWriteKdolMatchFile.Import();
    var useExport = new LeEabWriteKdolMatchFile.Export();

    useImport.RecType.Text1 = local.KdolRecType.Text1;
    useImport.KdolFile.Assign(local.KdolFile);
    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(LeEabWriteKdolMatchFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseLeEabWriteKdolMatchFile4()
  {
    var useImport = new LeEabWriteKdolMatchFile.Import();
    var useExport = new LeEabWriteKdolMatchFile.Export();

    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(LeEabWriteKdolMatchFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
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

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.StartCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
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

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);
  }

  private void CreateDolUiWithholding()
  {
    var csePersonNumber = local.Extract.Item.Client.Number;
    var withholdingCertificationDate = local.Extract1.Date;
    var standardNumber =
      Substring(local.Extract.Item.CourtCase.StandardNumber, 1, 12);
    var socialSecurityNumber = local.Extract.Item.Client.Ssn;
    var waAmount = local.Extract.Item.Amounts.ArrearsAmountOwed;
    var wcAmount = local.Extract.Item.Amounts.CurrentAmountOwed;
    var maxWithholdingPercent =
      local.Extract.Item.MaxPct.MaxWithholdingPercent.GetValueOrDefault();
    var firstName = local.Extract.Item.Client.FirstName;
    var lastName = local.Extract.Item.Client.LastName;
    var middleInitial = local.Extract.Item.Client.MiddleInitial;

    entities.DolUiWithholding.Populated = false;
    Update("CreateDolUiWithholding",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", csePersonNumber);
        db.SetDate(command, "iwoCertDate", withholdingCertificationDate);
        db.SetString(command, "standardNumber", standardNumber);
        db.SetString(command, "ssn", socialSecurityNumber);
        db.SetNullableDecimal(command, "waAmount", waAmount);
        db.SetNullableDecimal(command, "wcAmount", wcAmount);
        db.SetNullableInt32(command, "maxWithholdPct", maxWithholdingPercent);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "middleInitial", middleInitial);
      });

    entities.DolUiWithholding.CsePersonNumber = csePersonNumber;
    entities.DolUiWithholding.WithholdingCertificationDate =
      withholdingCertificationDate;
    entities.DolUiWithholding.StandardNumber = standardNumber;
    entities.DolUiWithholding.SocialSecurityNumber = socialSecurityNumber;
    entities.DolUiWithholding.WaAmount = waAmount;
    entities.DolUiWithholding.WcAmount = wcAmount;
    entities.DolUiWithholding.MaxWithholdingPercent = maxWithholdingPercent;
    entities.DolUiWithholding.FirstName = firstName;
    entities.DolUiWithholding.LastName = lastName;
    entities.DolUiWithholding.MiddleInitial = middleInitial;
    entities.DolUiWithholding.Populated = true;
  }

  private void DeleteDolUiWithholding()
  {
    Update("DeleteDolUiWithholding",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.N2dRead.CsePersonNumber);
        db.SetDate(
          command, "iwoCertDate",
          entities.N2dRead.WithholdingCertificationDate.GetValueOrDefault());
        db.
          SetString(command, "standardNumber", entities.N2dRead.StandardNumber);
          
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.StartRead.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadDolUiWithholding1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadDolUiWithholding1",
      (db, command) =>
      {
        db.SetDate(
          command, "iwoCertDate",
          local.Ssn.Item.Ssn1.WithholdingCertificationDate.GetValueOrDefault());
          
        db.SetString(
          command, "standardNumber", local.Ssn.Item.Ssn1.StandardNumber);
        db.SetString(command, "cspNumber", local.Ssn.Item.Ssn1.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.N2dRead.CsePersonNumber = db.GetString(reader, 0);
        entities.N2dRead.WithholdingCertificationDate = db.GetDate(reader, 1);
        entities.N2dRead.StandardNumber = db.GetString(reader, 2);
        entities.N2dRead.SocialSecurityNumber = db.GetString(reader, 3);
        entities.N2dRead.WaAmount = db.GetNullableDecimal(reader, 4);
        entities.N2dRead.WcAmount = db.GetNullableDecimal(reader, 5);
        entities.N2dRead.MaxWithholdingPercent = db.GetNullableInt32(reader, 6);
        entities.N2dRead.FirstName = db.GetNullableString(reader, 7);
        entities.N2dRead.LastName = db.GetNullableString(reader, 8);
        entities.N2dRead.MiddleInitial = db.GetNullableString(reader, 9);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadDolUiWithholding2()
  {
    entities.DolUiWithholding.Populated = false;

    return Read("ReadDolUiWithholding2",
      (db, command) =>
      {
        db.SetDate(
          command, "iwoCertDate", local.Extract1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DolUiWithholding.CsePersonNumber = db.GetString(reader, 0);
        entities.DolUiWithholding.WithholdingCertificationDate =
          db.GetDate(reader, 1);
        entities.DolUiWithholding.StandardNumber = db.GetString(reader, 2);
        entities.DolUiWithholding.SocialSecurityNumber =
          db.GetString(reader, 3);
        entities.DolUiWithholding.WaAmount = db.GetNullableDecimal(reader, 4);
        entities.DolUiWithholding.WcAmount = db.GetNullableDecimal(reader, 5);
        entities.DolUiWithholding.MaxWithholdingPercent =
          db.GetNullableInt32(reader, 6);
        entities.DolUiWithholding.FirstName = db.GetNullableString(reader, 7);
        entities.DolUiWithholding.LastName = db.GetNullableString(reader, 8);
        entities.DolUiWithholding.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.DolUiWithholding.Populated = true;
      });
  }

  private bool ReadDolUiWithholding3()
  {
    local.Max.Populated = false;

    return Read("ReadDolUiWithholding3",
      null,
      (db, reader) =>
      {
        local.Max.WithholdingCertificationDate = db.GetDate(reader, 0);
        local.Max.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDolUiWithholding4()
  {
    entities.DolUiWithholding.Populated = false;

    return ReadEach("ReadDolUiWithholding4",
      (db, command) =>
      {
        db.SetDate(
          command, "iwoCertDate", local.Extract1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DolUiWithholding.CsePersonNumber = db.GetString(reader, 0);
        entities.DolUiWithholding.WithholdingCertificationDate =
          db.GetDate(reader, 1);
        entities.DolUiWithholding.StandardNumber = db.GetString(reader, 2);
        entities.DolUiWithholding.SocialSecurityNumber =
          db.GetString(reader, 3);
        entities.DolUiWithholding.WaAmount = db.GetNullableDecimal(reader, 4);
        entities.DolUiWithholding.WcAmount = db.GetNullableDecimal(reader, 5);
        entities.DolUiWithholding.MaxWithholdingPercent =
          db.GetNullableInt32(reader, 6);
        entities.DolUiWithholding.FirstName = db.GetNullableString(reader, 7);
        entities.DolUiWithholding.LastName = db.GetNullableString(reader, 8);
        entities.DolUiWithholding.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.DolUiWithholding.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDolUiWithholding5()
  {
    entities.DolUiWithholding.Populated = false;

    return ReadEach("ReadDolUiWithholding5",
      (db, command) =>
      {
        db.SetDate(
          command, "iwoCertDate", local.Extract1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DolUiWithholding.CsePersonNumber = db.GetString(reader, 0);
        entities.DolUiWithholding.WithholdingCertificationDate =
          db.GetDate(reader, 1);
        entities.DolUiWithholding.StandardNumber = db.GetString(reader, 2);
        entities.DolUiWithholding.SocialSecurityNumber =
          db.GetString(reader, 3);
        entities.DolUiWithholding.WaAmount = db.GetNullableDecimal(reader, 4);
        entities.DolUiWithholding.WcAmount = db.GetNullableDecimal(reader, 5);
        entities.DolUiWithholding.MaxWithholdingPercent =
          db.GetNullableInt32(reader, 6);
        entities.DolUiWithholding.FirstName = db.GetNullableString(reader, 7);
        entities.DolUiWithholding.LastName = db.GetNullableString(reader, 8);
        entities.DolUiWithholding.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.DolUiWithholding.Populated = true;

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
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private DolUiWithholding dolUiWithholding;
    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SsnGroup group.</summary>
    [Serializable]
    public class SsnGroup
    {
      /// <summary>
      /// A value of Ssn1.
      /// </summary>
      [JsonPropertyName("ssn1")]
      public DolUiWithholding Ssn1
      {
        get => ssn1 ??= new();
        set => ssn1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private DolUiWithholding ssn1;
    }

    /// <summary>A ErrorMessagesGroup group.</summary>
    [Serializable]
    public class ErrorMessagesGroup
    {
      /// <summary>
      /// A value of ErrorCourtCase.
      /// </summary>
      [JsonPropertyName("errorCourtCase")]
      public LegalAction ErrorCourtCase
      {
        get => errorCourtCase ??= new();
        set => errorCourtCase = value;
      }

      /// <summary>
      /// A value of ErrorType.
      /// </summary>
      [JsonPropertyName("errorType")]
      public WorkArea ErrorType
      {
        get => errorType ??= new();
        set => errorType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private LegalAction errorCourtCase;
      private WorkArea errorType;
    }

    /// <summary>A ExtractGroup group.</summary>
    [Serializable]
    public class ExtractGroup
    {
      /// <summary>
      /// A value of MaxPct.
      /// </summary>
      [JsonPropertyName("maxPct")]
      public DolUiWithholding MaxPct
      {
        get => maxPct ??= new();
        set => maxPct = value;
      }

      /// <summary>
      /// A value of CourtCase.
      /// </summary>
      [JsonPropertyName("courtCase")]
      public LegalAction CourtCase
      {
        get => courtCase ??= new();
        set => courtCase = value;
      }

      /// <summary>
      /// A value of Amounts.
      /// </summary>
      [JsonPropertyName("amounts")]
      public ScreenOwedAmounts Amounts
      {
        get => amounts ??= new();
        set => amounts = value;
      }

      /// <summary>
      /// A value of Client.
      /// </summary>
      [JsonPropertyName("client")]
      public CsePersonsWorkSet Client
      {
        get => client ??= new();
        set => client = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DolUiWithholding maxPct;
      private LegalAction courtCase;
      private ScreenOwedAmounts amounts;
      private CsePersonsWorkSet client;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DolUiWithholding Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DolUiWithholding Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// Gets a value of Ssn.
    /// </summary>
    [JsonIgnore]
    public Array<SsnGroup> Ssn => ssn ??= new(SsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ssn for json serialization.
    /// </summary>
    [JsonPropertyName("ssn")]
    [Computed]
    public IList<SsnGroup> Ssn_Json
    {
      get => ssn;
      set => Ssn.Assign(value);
    }

    /// <summary>
    /// A value of TopAmountCommon.
    /// </summary>
    [JsonPropertyName("topAmountCommon")]
    public Common TopAmountCommon
    {
      get => topAmountCommon ??= new();
      set => topAmountCommon = value;
    }

    /// <summary>
    /// A value of TopAmountDolUiWithholding.
    /// </summary>
    [JsonPropertyName("topAmountDolUiWithholding")]
    public DolUiWithholding TopAmountDolUiWithholding
    {
      get => topAmountDolUiWithholding ??= new();
      set => topAmountDolUiWithholding = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DolUiWithholding Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of EabExtract.
    /// </summary>
    [JsonPropertyName("eabExtract")]
    public DateWorkArea EabExtract
    {
      get => eabExtract ??= new();
      set => eabExtract = value;
    }

    /// <summary>
    /// A value of DayOfWeek.
    /// </summary>
    [JsonPropertyName("dayOfWeek")]
    public WorkArea DayOfWeek
    {
      get => dayOfWeek ??= new();
      set => dayOfWeek = value;
    }

    /// <summary>
    /// A value of Extract1.
    /// </summary>
    [JsonPropertyName("extract1")]
    public DateWorkArea Extract1
    {
      get => extract1 ??= new();
      set => extract1 = value;
    }

    /// <summary>
    /// A value of KdolRecType.
    /// </summary>
    [JsonPropertyName("kdolRecType")]
    public TextWorkArea KdolRecType
    {
      get => kdolRecType ??= new();
      set => kdolRecType = value;
    }

    /// <summary>
    /// A value of InterstateRole.
    /// </summary>
    [JsonPropertyName("interstateRole")]
    public OfficeServiceProvider InterstateRole
    {
      get => interstateRole ??= new();
      set => interstateRole = value;
    }

    /// <summary>
    /// A value of InterstateSpd.
    /// </summary>
    [JsonPropertyName("interstateSpd")]
    public ServiceProvider InterstateSpd
    {
      get => interstateSpd ??= new();
      set => interstateSpd = value;
    }

    /// <summary>
    /// A value of InterstateOffice.
    /// </summary>
    [JsonPropertyName("interstateOffice")]
    public Office InterstateOffice
    {
      get => interstateOffice ??= new();
      set => interstateOffice = value;
    }

    /// <summary>
    /// A value of Sub.
    /// </summary>
    [JsonPropertyName("sub")]
    public Common Sub
    {
      get => sub ??= new();
      set => sub = value;
    }

    /// <summary>
    /// A value of KdolDetail.
    /// </summary>
    [JsonPropertyName("kdolDetail")]
    public Common KdolDetail
    {
      get => kdolDetail ??= new();
      set => kdolDetail = value;
    }

    /// <summary>
    /// A value of StartRead.
    /// </summary>
    [JsonPropertyName("startRead")]
    public CsePersonsWorkSet StartRead
    {
      get => startRead ??= new();
      set => startRead = value;
    }

    /// <summary>
    /// A value of HoldDolUiWithholding.
    /// </summary>
    [JsonPropertyName("holdDolUiWithholding")]
    public DolUiWithholding HoldDolUiWithholding
    {
      get => holdDolUiWithholding ??= new();
      set => holdDolUiWithholding = value;
    }

    /// <summary>
    /// A value of PrevUi.
    /// </summary>
    [JsonPropertyName("prevUi")]
    public CsePersonsWorkSet PrevUi
    {
      get => prevUi ??= new();
      set => prevUi = value;
    }

    /// <summary>
    /// A value of RestartCount.
    /// </summary>
    [JsonPropertyName("restartCount")]
    public Common RestartCount
    {
      get => restartCount ??= new();
      set => restartCount = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Common Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of RestartAmount.
    /// </summary>
    [JsonPropertyName("restartAmount")]
    public ScreenDueAmounts RestartAmount
    {
      get => restartAmount ??= new();
      set => restartAmount = value;
    }

    /// <summary>
    /// A value of RestartApProcessed.
    /// </summary>
    [JsonPropertyName("restartApProcessed")]
    public Common RestartApProcessed
    {
      get => restartApProcessed ??= new();
      set => restartApProcessed = value;
    }

    /// <summary>
    /// A value of RestartApRead.
    /// </summary>
    [JsonPropertyName("restartApRead")]
    public Common RestartApRead
    {
      get => restartApRead ??= new();
      set => restartApRead = value;
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
    /// A value of RestartSection.
    /// </summary>
    [JsonPropertyName("restartSection")]
    public TextWorkArea RestartSection
    {
      get => restartSection ??= new();
      set => restartSection = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of KdolFile.
    /// </summary>
    [JsonPropertyName("kdolFile")]
    public KdolFile KdolFile
    {
      get => kdolFile ??= new();
      set => kdolFile = value;
    }

    /// <summary>
    /// A value of TotalCurrent.
    /// </summary>
    [JsonPropertyName("totalCurrent")]
    public Common TotalCurrent
    {
      get => totalCurrent ??= new();
      set => totalCurrent = value;
    }

    /// <summary>
    /// A value of TotalArrears.
    /// </summary>
    [JsonPropertyName("totalArrears")]
    public Common TotalArrears
    {
      get => totalArrears ??= new();
      set => totalArrears = value;
    }

    /// <summary>
    /// Gets a value of ErrorMessages.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorMessagesGroup> ErrorMessages => errorMessages ??= new(
      ErrorMessagesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorMessages for json serialization.
    /// </summary>
    [JsonPropertyName("errorMessages")]
    [Computed]
    public IList<ErrorMessagesGroup> ErrorMessages_Json
    {
      get => errorMessages;
      set => ErrorMessages.Assign(value);
    }

    /// <summary>
    /// Gets a value of Extract.
    /// </summary>
    [JsonIgnore]
    public Array<ExtractGroup> Extract => extract ??= new(
      ExtractGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Extract for json serialization.
    /// </summary>
    [JsonPropertyName("extract")]
    [Computed]
    public IList<ExtractGroup> Extract_Json
    {
      get => extract;
      set => Extract.Assign(value);
    }

    /// <summary>
    /// A value of DisplayErrors.
    /// </summary>
    [JsonPropertyName("displayErrors")]
    public Common DisplayErrors
    {
      get => displayErrors ??= new();
      set => displayErrors = value;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CsePersonsWorkSet Clear
    {
      get => clear ??= new();
      set => clear = value;
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
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of StartCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startCsePersonsWorkSet")]
    public CsePersonsWorkSet StartCsePersonsWorkSet
    {
      get => startCsePersonsWorkSet ??= new();
      set => startCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NumberOfRecordsRead.
    /// </summary>
    [JsonPropertyName("numberOfRecordsRead")]
    public Common NumberOfRecordsRead
    {
      get => numberOfRecordsRead ??= new();
      set => numberOfRecordsRead = value;
    }

    /// <summary>
    /// A value of TotalNumberRecordsFound.
    /// </summary>
    [JsonPropertyName("totalNumberRecordsFound")]
    public Common TotalNumberRecordsFound
    {
      get => totalNumberRecordsFound ??= new();
      set => totalNumberRecordsFound = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of StartCommon.
    /// </summary>
    [JsonPropertyName("startCommon")]
    public Common StartCommon
    {
      get => startCommon ??= new();
      set => startCommon = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public TextWorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    private DolUiWithholding null1;
    private DolUiWithholding previous;
    private Array<SsnGroup> ssn;
    private Common topAmountCommon;
    private DolUiWithholding topAmountDolUiWithholding;
    private DolUiWithholding max;
    private DateWorkArea eabExtract;
    private WorkArea dayOfWeek;
    private DateWorkArea extract1;
    private TextWorkArea kdolRecType;
    private OfficeServiceProvider interstateRole;
    private ServiceProvider interstateSpd;
    private Office interstateOffice;
    private Common sub;
    private Common kdolDetail;
    private CsePersonsWorkSet startRead;
    private DolUiWithholding holdDolUiWithholding;
    private CsePersonsWorkSet prevUi;
    private Common restartCount;
    private Common total;
    private Common restart;
    private ScreenDueAmounts restartAmount;
    private Common restartApProcessed;
    private Common restartApRead;
    private CsePersonsWorkSet restartPerson;
    private TextWorkArea restartSection;
    private TextWorkArea textWorkArea;
    private KdolFile kdolFile;
    private Common totalCurrent;
    private Common totalArrears;
    private Array<ErrorMessagesGroup> errorMessages;
    private Array<ExtractGroup> extract;
    private Common displayErrors;
    private CsePerson csePerson;
    private CsePersonsWorkSet clear;
    private DateWorkArea dateWorkArea;
    private DateWorkArea startDate;
    private WorkArea workArea;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet startCsePersonsWorkSet;
    private Common numberOfRecordsRead;
    private Common totalNumberRecordsFound;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common startCommon;
    private TextWorkArea year;
    private TextWorkArea month;
    private TextWorkArea day;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public DolUiWithholding N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private DolUiWithholding n2dRead;
    private ObligationType obligationType;
    private DolUiWithholding dolUiWithholding;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private CsePerson csePerson;
  }
#endregion
}

// Program: FN_BFXD_CONVERT_HOUSEHOLD_MEMBRS, ID: 374445370, model: 746.
// Short name: SWEFFXDB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXD_CONVERT_HOUSEHOLD_MEMBRS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxdConvertHouseholdMembrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXD_CONVERT_HOUSEHOLD_MEMBRS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxdConvertHouseholdMembrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxdConvertHouseholdMembrs.
  /// </summary>
  public FnBfxdConvertHouseholdMembrs(IContext context, Import import,
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
    // --------------------------------------------------------------------------------
    // Initial Version :- SWSRKXD 04/25/2000
    // This utility will load IM_HH_MBR_MNTH_SUMM from IM_HH_MNTHLY_DEBIT_SUMM
    // 06/01/2000 - Change rules to determine member participation.
    // 6/5/2000 - skip duplicate cse_persons within same month.
    // 06/09/2000 - Also include EP and NP.
    // 08/05/2000 - Set file action to Write on return from 
    // batch_initialization.
    // --------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEFBFXD";
    UseFnBatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.RestartImHousehold.AeCaseNo =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 8);
      local.RestartImHouseholdMonthlyDebitSumma.Year =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 9, 4));
      local.RestartImHouseholdMonthlyDebitSumma.Month =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 13, 2));
      local.EabReportSend.RptDetail =
        "Program SWEFBFXD will be restarted. AE Case #:" + local
        .RestartImHousehold.AeCaseNo + ";  Restart Year= " + Substring
        (local.ProgramCheckpointRestart.RestartInfo, 250, 9, 4) + ";  Month=" +
        Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 13, 2);
      UseCabControlReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.FirstTime.Flag = "Y";

    foreach(var item in ReadImHouseholdMonthlyDebitSummaImHousehold())
    {
      if (!IsEmpty(local.TestHh.AeCaseNo) && Lt
        (entities.ImHousehold.AeCaseNo, local.TestHh.AeCaseNo))
      {
        continue;
      }

      if (!IsEmpty(local.TestHh.AeCaseNo) && Lt
        (local.TestHh.AeCaseNo, entities.ImHousehold.AeCaseNo))
      {
        break;
      }

      if (Equal(entities.ImHousehold.AeCaseNo, local.RestartImHousehold.AeCaseNo))
        
      {
        if (entities.ImHouseholdMonthlyDebitSumma.Year < local
          .RestartImHouseholdMonthlyDebitSumma.Year)
        {
          continue;
        }
        else if (entities.ImHouseholdMonthlyDebitSumma.Year == local
          .RestartImHouseholdMonthlyDebitSumma.Year)
        {
          if (entities.ImHouseholdMonthlyDebitSumma.Month <= local
            .RestartImHouseholdMonthlyDebitSumma.Month)
          {
            continue;
          }
        }
      }

      if (IsEmpty(local.DummyLoop.Flag))
      {
        if (entities.ImHouseholdMonthlyDebitSumma.Month == 0 || entities
          .ImHouseholdMonthlyDebitSumma.Year == 0)
        {
          ExitState = "FN0000_YEAR_OR_MONTH_IS_SET_TO_0";

          goto Test1;
        }

        if (entities.ImHouseholdMonthlyDebitSumma.GrantTotal == 0 && entities
          .ImHouseholdMonthlyDebitSumma.PassthruTotal == 0)
        {
          ExitState = "FN0000_GRANT_AND_PT_BOTH_ZERO";

          goto Test1;
        }

        ++local.NbrOfRecordsRead.Count;

        if (Equal(entities.ImHousehold.AeCaseNo, local.PrevImHousehold.AeCaseNo) &&
          entities.ImHouseholdMonthlyDebitSumma.Year == local
          .PrevImHouseholdMonthlyDebitSumma.Year && entities
          .ImHouseholdMonthlyDebitSumma.Month == local
          .PrevImHouseholdMonthlyDebitSumma.Month)
        {
          local.PrevImHouseholdMonthlyDebitSumma.GrantTotal += entities.
            ImHouseholdMonthlyDebitSumma.GrantTotal;
          local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal += entities.
            ImHouseholdMonthlyDebitSumma.PassthruTotal;

          continue;
        }

        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          local.FirstTime.Flag = "N";
          local.PrevImHousehold.AeCaseNo = entities.ImHousehold.AeCaseNo;
          local.PrevImHouseholdMonthlyDebitSumma.Assign(
            entities.ImHouseholdMonthlyDebitSumma);
          ++local.CommitCnt.Count;

          continue;
        }

        // -----------------------------------------------------
        // Now process the previous debit summary record.
        // -----------------------------------------------------
        // -----------------------------------------------------
        // Obtain currency on IM_Household. This is required to create
        // IM_Household_Mth_Debit_Summ below.
        // -----------------------------------------------------
        if (!ReadImHousehold())
        {
          ExitState = "IM_HOUSEHOLD_NF_RB";

          return;
        }

        local.FirstOfMonth.StartDate =
          IntToDate(local.PrevImHouseholdMonthlyDebitSumma.Year * 10000 + local
          .PrevImHouseholdMonthlyDebitSumma.Month * 100 + 1);
        local.LastOfMonth.StartDate =
          AddDays(AddMonths(local.FirstOfMonth.StartDate, 1), -1);
        local.NbrOfMbrsInHh.Count = 0;
        local.PiExists.Flag = "";

        // -----------------------------------------------------
        // Determine member participation.
        // A member will receive benefits if it was 'IN' during any day of
        // that month.
        // 06/09/2000 - Also include EP and NP.
        // -----------------------------------------------------
        local.PrevCsePerson.Number = "";

        local.Group.Index = 0;
        local.Group.Clear();

        foreach(var item1 in ReadCsePersonImHouseholdMember())
        {
          // -----------------------------------------------------
          // 6/5/2000 - skip duplicate cse_persons within same month.
          // -----------------------------------------------------
          if (Equal(entities.CsePerson.Number, local.PrevCsePerson.Number))
          {
            local.Group.Next();

            continue;
          }

          if (Equal(entities.ImHouseholdMember.Relationship, "PI"))
          {
            local.PiExists.Flag = "Y";
          }

          ++local.NbrOfMbrsInHh.Count;
          local.PrevCsePerson.Number = entities.CsePerson.Number;
          local.Group.Update.CsePerson.Number = entities.CsePerson.Number;
          local.Group.Update.ImHouseholdMember.Relationship =
            entities.ImHouseholdMember.Relationship;
          local.Group.Next();
        }

        if (local.NbrOfMbrsInHh.Count == 0)
        {
          ExitState = "OE0000_NO_MEMBER_FOUND_IN_HH";

          goto Test1;
        }

        if (AsChar(local.PiExists.Flag) == 'Y' && local.NbrOfMbrsInHh.Count == 1
          && local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal != 0)
        {
          ExitState = "OE0000_NO_MEMBER_TO_APPLY_PSTHRU";

          goto Test1;
        }

        local.BalanceGrant.TotalCurrency =
          local.PrevImHouseholdMonthlyDebitSumma.GrantTotal;
        local.BalancePassthru.TotalCurrency =
          local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal;
        local.CurrentCountInHh.Count = 1;

        // -----------------------------------------------------
        // Distribute amounts equally amongst participating household
        // members.
        // -----------------------------------------------------
        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          // ---------------------------------------------------------------
          // Due to division and rounding, last member needs special handling.
          // ---------------------------------------------------------------
          if (local.CurrentCountInHh.Count == local.NbrOfMbrsInHh.Count)
          {
            local.IncrementGrant.TotalCurrency =
              local.BalanceGrant.TotalCurrency;
            local.IncrementPassthru.TotalCurrency =
              local.BalancePassthru.TotalCurrency;
          }
          else
          {
            local.IncrementGrant.TotalCurrency =
              local.PrevImHouseholdMonthlyDebitSumma.GrantTotal / local
              .NbrOfMbrsInHh.Count;

            if (Equal(local.Group.Item.ImHouseholdMember.Relationship, "PI"))
            {
              local.IncrementPassthru.TotalCurrency = 0;
            }
            else if (AsChar(local.PiExists.Flag) == 'Y')
            {
              local.IncrementPassthru.TotalCurrency =
                local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal / (
                  (long)local.NbrOfMbrsInHh.Count - 1);
            }
            else
            {
              local.IncrementPassthru.TotalCurrency =
                local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal / local
                .NbrOfMbrsInHh.Count;
            }
          }

          ++local.CurrentCountInHh.Count;
          local.BalanceGrant.TotalCurrency -= local.IncrementGrant.
            TotalCurrency;
          local.BalancePassthru.TotalCurrency -= local.IncrementPassthru.
            TotalCurrency;

          if (local.IncrementGrant.TotalCurrency == 0 && local
            .IncrementPassthru.TotalCurrency == 0)
          {
            // ---------------------------------------------------------------
            // Since all amounts are 0, don't create MBR_MTH_SUMM record.
            // The only time you hit this scenario is when you have
            // non-zero Passthru_total and zero Grant_total for the same
            // month on MTH_DEBIT_SUMM.
            // Skip this record and write to error log as informational message
            // !.
            // ---------------------------------------------------------------
            ExitState = "FN0000_MSUM_RECORD_NOT_CREATED";
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.Status.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            local.EabReportSend.RptDetail = "CSE Person #:" + local
              .Group.Item.CsePerson.Number + "; AE Case #:" + local
              .PrevImHousehold.AeCaseNo;
            UseCabErrorReport2();
            local.EabReportSend.RptDetail = "Year:" + NumberToString
              (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4) + "; Month:" +
              NumberToString
              (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
            UseCabErrorReport2();

            if (!Equal(local.Status.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            continue;
          }

          if (!ReadCsePerson())
          {
            // ---------------------------------------------------------------
            // This is a fatal error!!! Write to the error report and ABEND.
            // ---------------------------------------------------------------
            ExitState = "FN000_CSE_PERSON_NF";
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.Status.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "CSE Person #:" + local
              .Group.Item.CsePerson.Number;
            UseCabErrorReport2();

            if (!Equal(local.Status.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            }

            return;
          }

          try
          {
            CreateImHouseholdMbrMnthlySum();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                // ---------------------------------------------------------------
                // This is a fatal error!!! Write to the error report and ABEND.
                // ---------------------------------------------------------------
                ExitState = "FN0000_IMHH_MBR_MNTH_SUMM_AE";
                UseEabExtractExitStateMessage();
                local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                local.EabReportSend.RptDetail = "CSE Person #:" + local
                  .Group.Item.CsePerson.Number + "; AE Case #:" + local
                  .PrevImHousehold.AeCaseNo;
                UseCabErrorReport2();
                local.EabReportSend.RptDetail = "Year:" + NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4) + "; Month:" +
                  NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                }

                break;
              case ErrorCode.PermittedValueViolation:
                // ---------------------------------------------------------------
                // This is a fatal error!!! Write to the error report and ABEND.
                // ---------------------------------------------------------------
                ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_PV_RB";
                UseEabExtractExitStateMessage();
                local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                local.EabReportSend.RptDetail = "CSE Person #:" + local
                  .Group.Item.CsePerson.Number + "; AE Case #:" + local
                  .PrevImHousehold.AeCaseNo;
                UseCabErrorReport2();
                local.EabReportSend.RptDetail = "Year:" + NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4) + "; Month:" +
                  NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                }

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.NbrOfCreates.Count;
        }
      }

Test1:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.NbrOfErroredRecords.Count;
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.EabReportSend.RptDetail = "AE Case # :" + local
          .PrevImHousehold.AeCaseNo + "; Year =" + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Month=" + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // -----------------------------------------------------
        // Check if commit count has been reached.
        // -----------------------------------------------------
      }
      else if (local.CommitCnt.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && AsChar
        (local.TestRun.Flag) != 'Y')
      {
        local.CommitCnt.Count = 0;

        if (!ReadProgramCheckpointRestart2())
        {
          ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

          return;
        }

        local.ProgramCheckpointRestart.RestartInfo =
          local.PrevImHousehold.AeCaseNo + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);

        try
        {
          UpdateProgramCheckpointRestart1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PROGRAM_CHECKPOINT_RESTART_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PROGRAM_CHECKPOINT_RESTART_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        UseExtToDoACommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }

        ++local.NbrOfCheckpoint.Count;
        local.EabReportSend.RptDetail = "Checkpoint #: " + NumberToString
          (local.NbrOfCheckpoint.Count, 10, 6);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " Time: ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (TimeToInt(TimeOfDay(Now())), 10, 6);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " AE Case #: ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local
          .PrevImHousehold.AeCaseNo;
        UseCabControlReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "*************Display for testing ******************";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail =
          "Display indicator is ON.  AE case #=  " + local
          .PrevImHousehold.AeCaseNo + "; Year= ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "Month=" + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      ++local.CommitCnt.Count;
      local.PrevImHousehold.AeCaseNo = entities.ImHousehold.AeCaseNo;
      local.PrevImHouseholdMonthlyDebitSumma.Assign(
        entities.ImHouseholdMonthlyDebitSumma);
    }

    // ------------------------------------------
    // Process the last record, if there was one.
    // ------------------------------------------
    if (!IsEmpty(local.PrevImHousehold.AeCaseNo))
    {
      if (!ReadImHousehold())
      {
        ExitState = "IM_HOUSEHOLD_NF_RB";

        return;
      }

      local.FirstOfMonth.StartDate =
        IntToDate(local.PrevImHouseholdMonthlyDebitSumma.Year * 10000 + local
        .PrevImHouseholdMonthlyDebitSumma.Month * 100 + 1);
      local.LastOfMonth.StartDate =
        AddDays(AddMonths(local.FirstOfMonth.StartDate, 1), -1);
      local.NbrOfMbrsInHh.Count = 0;
      local.PiExists.Flag = "";

      // -----------------------------------------------------
      // Determine member participation.
      // A member will receive benefits if it was 'IN' during any day of
      // that month.
      // 06/09/2000 - Also include EP and NP.
      // -----------------------------------------------------
      local.PrevCsePerson.Number = "";

      local.Group.Index = 0;
      local.Group.Clear();

      foreach(var item in ReadCsePersonImHouseholdMember())
      {
        // -----------------------------------------------------
        // 6/5/2000 - skip duplicate cse_persons within same month.
        // -----------------------------------------------------
        if (Equal(entities.CsePerson.Number, local.PrevCsePerson.Number))
        {
          local.Group.Next();

          continue;
        }

        if (Equal(entities.ImHouseholdMember.Relationship, "PI"))
        {
          local.PiExists.Flag = "Y";
        }

        ++local.NbrOfMbrsInHh.Count;
        local.PrevCsePerson.Number = entities.CsePerson.Number;
        local.Group.Update.CsePerson.Number = entities.CsePerson.Number;
        local.Group.Update.ImHouseholdMember.Relationship =
          entities.ImHouseholdMember.Relationship;
        local.Group.Next();
      }

      if (IsEmpty(local.DummyLoop.Flag))
      {
        if (local.NbrOfMbrsInHh.Count == 0)
        {
          ExitState = "OE0000_NO_MEMBER_FOUND_IN_HH";

          goto Test2;
        }

        if (AsChar(local.PiExists.Flag) == 'Y' && local.NbrOfMbrsInHh.Count == 1
          && local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal != 0)
        {
          ExitState = "OE0000_NO_MEMBER_TO_APPLY_PSTHRU";

          goto Test2;
        }

        local.BalanceGrant.TotalCurrency =
          local.PrevImHouseholdMonthlyDebitSumma.GrantTotal;
        local.BalancePassthru.TotalCurrency =
          local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal;
        local.CurrentCountInHh.Count = 1;

        // -----------------------------------------------------
        // Apply amt equally amongst participating household members.
        // -----------------------------------------------------
        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          // ---------------------------------------------------------------
          // Due to division and rounding, last member needs special handling.
          // ---------------------------------------------------------------
          if (local.CurrentCountInHh.Count == local.NbrOfMbrsInHh.Count)
          {
            local.IncrementGrant.TotalCurrency =
              local.BalanceGrant.TotalCurrency;
            local.IncrementPassthru.TotalCurrency =
              local.BalancePassthru.TotalCurrency;
          }
          else
          {
            local.IncrementGrant.TotalCurrency =
              local.PrevImHouseholdMonthlyDebitSumma.GrantTotal / local
              .NbrOfMbrsInHh.Count;

            if (Equal(local.Group.Item.ImHouseholdMember.Relationship, "PI"))
            {
              local.IncrementPassthru.TotalCurrency = 0;
            }
            else if (AsChar(local.PiExists.Flag) == 'Y')
            {
              local.IncrementPassthru.TotalCurrency =
                local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal / (
                  (long)local.NbrOfMbrsInHh.Count - 1);
            }
            else
            {
              local.IncrementPassthru.TotalCurrency =
                local.PrevImHouseholdMonthlyDebitSumma.PassthruTotal / local
                .NbrOfMbrsInHh.Count;
            }
          }

          ++local.CurrentCountInHh.Count;
          local.BalanceGrant.TotalCurrency -= local.IncrementGrant.
            TotalCurrency;
          local.BalancePassthru.TotalCurrency -= local.IncrementPassthru.
            TotalCurrency;

          if (!ReadCsePerson())
          {
            // ---------------------------------------------------------------
            // This is a fatal error!!! Write to the error report and ABEND.
            // ---------------------------------------------------------------
            ExitState = "FN000_CSE_PERSON_NF";
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.Status.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "CSE Person #:" + local
              .Group.Item.CsePerson.Number;
            UseCabErrorReport2();

            if (!Equal(local.Status.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            }

            return;
          }

          try
          {
            CreateImHouseholdMbrMnthlySum();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                // ---------------------------------------------------------------
                // This is a fatal error!!! Write to the error report and ABEND.
                // ---------------------------------------------------------------
                ExitState = "FN0000_IMHH_MBR_MNTH_SUMM_AE";
                UseEabExtractExitStateMessage();
                local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                local.EabReportSend.RptDetail = "CSE Person #:" + local
                  .Group.Item.CsePerson.Number + "; AE Case #:" + local
                  .PrevImHousehold.AeCaseNo;
                UseCabErrorReport2();
                local.EabReportSend.RptDetail = "Year:" + NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4) + "; Month:" +
                  NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                }

                break;
              case ErrorCode.PermittedValueViolation:
                // ---------------------------------------------------------------
                // This is a fatal error!!! Write to the error report and ABEND.
                // ---------------------------------------------------------------
                ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_PV_RB";
                UseEabExtractExitStateMessage();
                local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                local.EabReportSend.RptDetail = "CSE Person #:" + local
                  .Group.Item.CsePerson.Number + "; AE Case #:" + local
                  .PrevImHousehold.AeCaseNo;
                UseCabErrorReport2();
                local.EabReportSend.RptDetail = "Year:" + NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4) + "; Month:" +
                  NumberToString
                  (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
                UseCabErrorReport2();

                if (!Equal(local.Status.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                }

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.NbrOfCreates.Count;
        }
      }

Test2:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.NbrOfErroredRecords.Count;
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.EabReportSend.RptDetail = "AE Case # :" + local
          .PrevImHousehold.AeCaseNo + "; Year =" + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Month=" + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "*************Display for testing ******************";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail =
          "Display indicator is ON.  AE case #=  " + local
          .PrevImHousehold.AeCaseNo + "; Year= ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Year, 12, 4);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "Month=" + NumberToString
          (local.PrevImHouseholdMonthlyDebitSumma.Month, 14, 2);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    if (AsChar(local.TestRun.Flag) == 'Y')
    {
      ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    if (!IsEmpty(local.TestHh.AeCaseNo))
    {
      local.EabReportSend.RptDetail =
        "Test AE Case # was supplied by the PPI record. " + " ";
      UseCabControlReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "AE Case #:" + local.TestHh.AeCaseNo + " will be processed. No other records will be updated!";
        
      UseCabControlReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // *********************
    // Write control totals
    // *********************
    local.EabReportSend.RptDetail =
      "Number of IM Household Mthly Debit Summ records read :- " + NumberToString
      (local.NbrOfRecordsRead.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of records in error :- " + NumberToString
      (local.NbrOfErroredRecords.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      "Number of IM Household Mbr Mnth Summ records created :- " + NumberToString
      (local.NbrOfCreates.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of checkpoints taken :- " + NumberToString
      (local.NbrOfCheckpoint.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *********************
    // Reset restart flag
    // *********************
    if (!ReadProgramCheckpointRestart1())
    {
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

      return;
    }

    try
    {
      UpdateProgramCheckpointRestart2();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PROGRAM_CHECKPOINT_RESTART_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PROGRAM_CHECKPOINT_RESTART_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ****************
    // Close the files
    // ****************
    local.EabFileHandling.Action = "CLOSE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();
    UseCabErrorReport1();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnBatchInitialization()
  {
    var useImport = new FnBatchInitialization.Import();
    var useExport = new FnBatchInitialization.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(FnBatchInitialization.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    local.TestRun.Flag = useExport.TestRunInd.Flag;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.TestHh.AeCaseNo = useExport.TestHh.AeCaseNo;
  }

  private void CreateImHouseholdMbrMnthlySum()
  {
    var year = local.PrevImHouseholdMonthlyDebitSumma.Year;
    var month = local.PrevImHouseholdMonthlyDebitSumma.Month;
    var relationship = local.Group.Item.ImHouseholdMember.Relationship;
    var grantAmount = local.IncrementGrant.TotalCurrency;
    var grantMedicalAmount = 0M;
    var uraAmount =
      local.IncrementGrant.TotalCurrency +
      local.IncrementPassthru.TotalCurrency;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var imhAeCaseNo = entities.Prev.AeCaseNo;
    var cspNumber = entities.CsePerson.Number;

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("CreateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "year0", year);
        db.SetInt32(command, "month0", month);
        db.SetString(command, "relationship", relationship);
        db.SetNullableDecimal(command, "grantAmt", grantAmount);
        db.SetNullableDecimal(command, "grantMedAmt", grantMedicalAmount);
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetNullableDecimal(command, "uraMedicalAmount", grantMedicalAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber", cspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.Year = year;
    entities.ImHouseholdMbrMnthlySum.Month = month;
    entities.ImHouseholdMbrMnthlySum.Relationship = relationship;
    entities.ImHouseholdMbrMnthlySum.GrantAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount = grantMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ImHouseholdMbrMnthlySum.UraMedicalAmount = grantMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlySum.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = "";
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = null;
    entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlySum.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Group.Item.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonImHouseholdMember()
  {
    return ReadEach("ReadCsePersonImHouseholdMember",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.Prev.AeCaseNo);
        db.SetDate(
          command, "startDate",
          local.LastOfMonth.StartDate.GetValueOrDefault());
        db.SetDate(
          command, "endDate", local.FirstOfMonth.StartDate.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 0);
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 1);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 2);
        entities.ImHouseholdMember.EndDate = db.GetDate(reader, 3);
        entities.ImHouseholdMember.CreatedBy = db.GetString(reader, 4);
        entities.ImHouseholdMember.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.ImHouseholdMember.LastUpdatedBy = db.GetString(reader, 6);
        entities.ImHouseholdMember.LastUpdatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ImHouseholdMember.Relationship = db.GetString(reader, 8);
        entities.ImHouseholdMember.CseCaseNumber =
          db.GetNullableString(reader, 9);
        entities.ImHouseholdMember.AeParticipationCode =
          db.GetNullableString(reader, 10);
        entities.ImHouseholdMember.EndCollectionDate = db.GetDate(reader, 11);
        entities.CsePerson.Populated = true;
        entities.ImHouseholdMember.Populated = true;

        return true;
      });
  }

  private bool ReadImHousehold()
  {
    entities.Prev.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", local.PrevImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.Prev.AeCaseNo = db.GetString(reader, 0);
        entities.Prev.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMonthlyDebitSummaImHousehold()
  {
    entities.ImHousehold.Populated = false;
    entities.ImHouseholdMonthlyDebitSumma.Populated = false;

    return ReadEach("ReadImHouseholdMonthlyDebitSummaImHousehold",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", local.RestartImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMonthlyDebitSumma.SequenceNumber =
          db.GetInt32(reader, 0);
        entities.ImHouseholdMonthlyDebitSumma.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMonthlyDebitSumma.Year = db.GetInt32(reader, 2);
        entities.ImHouseholdMonthlyDebitSumma.GrantTotal =
          db.GetDecimal(reader, 3);
        entities.ImHouseholdMonthlyDebitSumma.PassthruTotal =
          db.GetDecimal(reader, 4);
        entities.ImHouseholdMonthlyDebitSumma.MedicalGrantTotal =
          db.GetDecimal(reader, 5);
        entities.ImHouseholdMonthlyDebitSumma.ImhAeCaseNo =
          db.GetString(reader, 6);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 6);
        entities.ImHousehold.Populated = true;
        entities.ImHouseholdMonthlyDebitSumma.Populated = true;

        return true;
      });
  }

  private bool ReadProgramCheckpointRestart1()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return Read("ReadProgramCheckpointRestart1",
      null,
      (db, reader) =>
      {
        entities.ProgramCheckpointRestart.ProgramName = db.GetString(reader, 0);
        entities.ProgramCheckpointRestart.UpdateFrequencyCount =
          db.GetNullableInt32(reader, 1);
        entities.ProgramCheckpointRestart.ReadFrequencyCount =
          db.GetNullableInt32(reader, 2);
        entities.ProgramCheckpointRestart.CheckpointCount =
          db.GetNullableInt32(reader, 3);
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ProgramCheckpointRestart.RestartInd =
          db.GetNullableString(reader, 5);
        entities.ProgramCheckpointRestart.RestartInfo =
          db.GetNullableString(reader, 6);
        entities.ProgramCheckpointRestart.Populated = true;
      });
  }

  private bool ReadProgramCheckpointRestart2()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return Read("ReadProgramCheckpointRestart2",
      null,
      (db, reader) =>
      {
        entities.ProgramCheckpointRestart.ProgramName = db.GetString(reader, 0);
        entities.ProgramCheckpointRestart.UpdateFrequencyCount =
          db.GetNullableInt32(reader, 1);
        entities.ProgramCheckpointRestart.ReadFrequencyCount =
          db.GetNullableInt32(reader, 2);
        entities.ProgramCheckpointRestart.CheckpointCount =
          db.GetNullableInt32(reader, 3);
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ProgramCheckpointRestart.RestartInd =
          db.GetNullableString(reader, 5);
        entities.ProgramCheckpointRestart.RestartInfo =
          db.GetNullableString(reader, 6);
        entities.ProgramCheckpointRestart.Populated = true;
      });
  }

  private void UpdateProgramCheckpointRestart1()
  {
    var lastCheckpointTimestamp = Now();
    var restartInd = "Y";
    var restartInfo = local.ProgramCheckpointRestart.RestartInfo ?? "";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", restartInfo);
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

    entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
      lastCheckpointTimestamp;
    entities.ProgramCheckpointRestart.RestartInd = restartInd;
    entities.ProgramCheckpointRestart.RestartInfo = restartInfo;
    entities.ProgramCheckpointRestart.Populated = true;
  }

  private void UpdateProgramCheckpointRestart2()
  {
    var lastCheckpointTimestamp = Now();
    var restartInd = "N";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", "");
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

    entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
      lastCheckpointTimestamp;
    entities.ProgramCheckpointRestart.RestartInd = restartInd;
    entities.ProgramCheckpointRestart.RestartInfo = "";
    entities.ProgramCheckpointRestart.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ImHouseholdMember.
      /// </summary>
      [JsonPropertyName("imHouseholdMember")]
      public ImHouseholdMember ImHouseholdMember
      {
        get => imHouseholdMember ??= new();
        set => imHouseholdMember = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private ImHouseholdMember imHouseholdMember;
      private CsePerson csePerson;
    }

    /// <summary>
    /// A value of PrevCsePerson.
    /// </summary>
    [JsonPropertyName("prevCsePerson")]
    public CsePerson PrevCsePerson
    {
      get => prevCsePerson ??= new();
      set => prevCsePerson = value;
    }

    /// <summary>
    /// A value of LastOfMonth.
    /// </summary>
    [JsonPropertyName("lastOfMonth")]
    public ImHouseholdMember LastOfMonth
    {
      get => lastOfMonth ??= new();
      set => lastOfMonth = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of PrevImHousehold.
    /// </summary>
    [JsonPropertyName("prevImHousehold")]
    public ImHousehold PrevImHousehold
    {
      get => prevImHousehold ??= new();
      set => prevImHousehold = value;
    }

    /// <summary>
    /// A value of PrevImHouseholdMonthlyDebitSumma.
    /// </summary>
    [JsonPropertyName("prevImHouseholdMonthlyDebitSumma")]
    public ImHouseholdMonthlyDebitSumma PrevImHouseholdMonthlyDebitSumma
    {
      get => prevImHouseholdMonthlyDebitSumma ??= new();
      set => prevImHouseholdMonthlyDebitSumma = value;
    }

    /// <summary>
    /// A value of NbrOfCreates.
    /// </summary>
    [JsonPropertyName("nbrOfCreates")]
    public Common NbrOfCreates
    {
      get => nbrOfCreates ??= new();
      set => nbrOfCreates = value;
    }

    /// <summary>
    /// A value of NbrOfCheckpoint.
    /// </summary>
    [JsonPropertyName("nbrOfCheckpoint")]
    public Common NbrOfCheckpoint
    {
      get => nbrOfCheckpoint ??= new();
      set => nbrOfCheckpoint = value;
    }

    /// <summary>
    /// A value of NbrOfErroredRecords.
    /// </summary>
    [JsonPropertyName("nbrOfErroredRecords")]
    public Common NbrOfErroredRecords
    {
      get => nbrOfErroredRecords ??= new();
      set => nbrOfErroredRecords = value;
    }

    /// <summary>
    /// A value of NbrOfRecordsRead.
    /// </summary>
    [JsonPropertyName("nbrOfRecordsRead")]
    public Common NbrOfRecordsRead
    {
      get => nbrOfRecordsRead ??= new();
      set => nbrOfRecordsRead = value;
    }

    /// <summary>
    /// A value of RestartImHouseholdMonthlyDebitSumma.
    /// </summary>
    [JsonPropertyName("restartImHouseholdMonthlyDebitSumma")]
    public ImHouseholdMonthlyDebitSumma RestartImHouseholdMonthlyDebitSumma
    {
      get => restartImHouseholdMonthlyDebitSumma ??= new();
      set => restartImHouseholdMonthlyDebitSumma = value;
    }

    /// <summary>
    /// A value of IncrementMedical.
    /// </summary>
    [JsonPropertyName("incrementMedical")]
    public Common IncrementMedical
    {
      get => incrementMedical ??= new();
      set => incrementMedical = value;
    }

    /// <summary>
    /// A value of BalanceMedical.
    /// </summary>
    [JsonPropertyName("balanceMedical")]
    public Common BalanceMedical
    {
      get => balanceMedical ??= new();
      set => balanceMedical = value;
    }

    /// <summary>
    /// A value of IncrementPassthru.
    /// </summary>
    [JsonPropertyName("incrementPassthru")]
    public Common IncrementPassthru
    {
      get => incrementPassthru ??= new();
      set => incrementPassthru = value;
    }

    /// <summary>
    /// A value of BalancePassthru.
    /// </summary>
    [JsonPropertyName("balancePassthru")]
    public Common BalancePassthru
    {
      get => balancePassthru ??= new();
      set => balancePassthru = value;
    }

    /// <summary>
    /// A value of PiExists.
    /// </summary>
    [JsonPropertyName("piExists")]
    public Common PiExists
    {
      get => piExists ??= new();
      set => piExists = value;
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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of TestRun.
    /// </summary>
    [JsonPropertyName("testRun")]
    public Common TestRun
    {
      get => testRun ??= new();
      set => testRun = value;
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
    /// A value of TestHh.
    /// </summary>
    [JsonPropertyName("testHh")]
    public ImHousehold TestHh
    {
      get => testHh ??= new();
      set => testHh = value;
    }

    /// <summary>
    /// A value of RestartImHousehold.
    /// </summary>
    [JsonPropertyName("restartImHousehold")]
    public ImHousehold RestartImHousehold
    {
      get => restartImHousehold ??= new();
      set => restartImHousehold = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
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
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public ImHouseholdMember FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
    }

    /// <summary>
    /// A value of NbrOfMbrsInHh.
    /// </summary>
    [JsonPropertyName("nbrOfMbrsInHh")]
    public Common NbrOfMbrsInHh
    {
      get => nbrOfMbrsInHh ??= new();
      set => nbrOfMbrsInHh = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of DummyLoop.
    /// </summary>
    [JsonPropertyName("dummyLoop")]
    public Common DummyLoop
    {
      get => dummyLoop ??= new();
      set => dummyLoop = value;
    }

    /// <summary>
    /// A value of CurrentCountInHh.
    /// </summary>
    [JsonPropertyName("currentCountInHh")]
    public Common CurrentCountInHh
    {
      get => currentCountInHh ??= new();
      set => currentCountInHh = value;
    }

    /// <summary>
    /// A value of IncrementGrant.
    /// </summary>
    [JsonPropertyName("incrementGrant")]
    public Common IncrementGrant
    {
      get => incrementGrant ??= new();
      set => incrementGrant = value;
    }

    /// <summary>
    /// A value of BalanceGrant.
    /// </summary>
    [JsonPropertyName("balanceGrant")]
    public Common BalanceGrant
    {
      get => balanceGrant ??= new();
      set => balanceGrant = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    private CsePerson prevCsePerson;
    private ImHouseholdMember lastOfMonth;
    private Common firstTime;
    private ImHousehold prevImHousehold;
    private ImHouseholdMonthlyDebitSumma prevImHouseholdMonthlyDebitSumma;
    private Common nbrOfCreates;
    private Common nbrOfCheckpoint;
    private Common nbrOfErroredRecords;
    private Common nbrOfRecordsRead;
    private ImHouseholdMonthlyDebitSumma restartImHouseholdMonthlyDebitSumma;
    private Common incrementMedical;
    private Common balanceMedical;
    private Common incrementPassthru;
    private Common balancePassthru;
    private Common piExists;
    private ProgramProcessingInfo programProcessingInfo;
    private Common displayInd;
    private Common testRun;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ImHousehold testHh;
    private ImHousehold restartImHousehold;
    private CsePerson restartCsePerson;
    private EabFileHandling eabFileHandling;
    private ImHouseholdMember firstOfMonth;
    private Common nbrOfMbrsInHh;
    private Array<GroupGroup> group;
    private Common dummyLoop;
    private Common currentCountInHh;
    private Common incrementGrant;
    private Common balanceGrant;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private Common commitCnt;
    private External forCommit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public ImHousehold Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMonthlyDebitSumma.
    /// </summary>
    [JsonPropertyName("imHouseholdMonthlyDebitSumma")]
    public ImHouseholdMonthlyDebitSumma ImHouseholdMonthlyDebitSumma
    {
      get => imHouseholdMonthlyDebitSumma ??= new();
      set => imHouseholdMonthlyDebitSumma = value;
    }

    /// <summary>
    /// A value of UraAdjustment.
    /// </summary>
    [JsonPropertyName("uraAdjustment")]
    public UraAdjustment UraAdjustment
    {
      get => uraAdjustment ??= new();
      set => uraAdjustment = value;
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
    /// A value of ImHouseholdMember.
    /// </summary>
    [JsonPropertyName("imHouseholdMember")]
    public ImHouseholdMember ImHouseholdMember
    {
      get => imHouseholdMember ??= new();
      set => imHouseholdMember = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ImHousehold Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of Starved.
    /// </summary>
    [JsonPropertyName("starved")]
    public CsePerson Starved
    {
      get => starved ??= new();
      set => starved = value;
    }

    private ImHousehold prev;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHousehold imHousehold;
    private ImHouseholdMonthlyDebitSumma imHouseholdMonthlyDebitSumma;
    private UraAdjustment uraAdjustment;
    private CsePerson csePerson;
    private ImHouseholdMember imHouseholdMember;
    private ImHousehold zdel;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson starved;
  }
#endregion
}

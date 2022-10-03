// Program: FN_BFXG_CONVERT_NEGATIVE_URA_ADJ, ID: 374474781, model: 746.
// Short name: SWEFFXGB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXG_CONVERT_NEGATIVE_URA_ADJ.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxgConvertNegativeUraAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXG_CONVERT_NEGATIVE_URA_ADJ program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxgConvertNegativeUraAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxgConvertNegativeUraAdj.
  /// </summary>
  public FnBfxgConvertNegativeUraAdj(IContext context, Import import,
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
    // -----------------------------------------------------------
    // Initial Version :- SWSRKXD 08/15/2000
    // This utility was copied from BFXE to convert negative URA
    // Adjustments.
    // -----------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEFBFXG";
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
      local.RestartCsePerson.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 9, 10);

      // ---------------------------------------------------
      // input format for datenum YYYYMMDD
      // ----------------------------------------------------
      local.RestartImHouseholdMember.StartDate =
        IntToDate((int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 19, 8)));
      local.RestartUraAdjustment.Identifier =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 27, 6));
      local.EabReportSend.RptDetail =
        "Program SWEFBFXG will be restarted. AE Case #:" + local
        .RestartImHousehold.AeCaseNo + ";  Restart CSE Person #: " + Substring
        (local.ProgramCheckpointRestart.RestartInfo, 250, 9, 10) + ";  Start Date:" +
        Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 19, 10);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "; Adj #:" +
        Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 29, 6);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    foreach(var item in ReadUraAdjustmentImHouseholdMemberImHousehold())
    {
      if (Equal(entities.ImHousehold.AeCaseNo, local.RestartImHousehold.AeCaseNo))
        
      {
        if (Lt(entities.CsePerson.Number, local.RestartCsePerson.Number))
        {
          continue;
        }
        else if (Equal(entities.CsePerson.Number, local.RestartCsePerson.Number))
          
        {
          if (Lt(entities.ImHouseholdMember.StartDate,
            local.RestartImHouseholdMember.StartDate))
          {
            continue;
          }
          else if (Equal(entities.ImHouseholdMember.StartDate,
            local.RestartImHouseholdMember.StartDate))
          {
            if (entities.UraAdjustment.Identifier <= local
              .RestartUraAdjustment.Identifier)
            {
              continue;
            }
          }
        }
      }

      if (!IsEmpty(local.TestHh.AeCaseNo))
      {
        if (Lt(local.TestHh.AeCaseNo, entities.ImHousehold.AeCaseNo))
        {
          break;
        }

        if (!Equal(entities.ImHousehold.AeCaseNo, local.TestHh.AeCaseNo))
        {
          continue;
        }
      }

      ++local.NbrOfRecordsRead.Count;
      local.NbrOfRecordsRead.TotalCurrency += entities.UraAdjustment.
        AdjHouseholdUra.GetValueOrDefault();

      if (IsEmpty(local.DummyLoop.Flag))
      {
        if (entities.UraAdjustment.AdjMonth == 8 && entities
          .UraAdjustment.AdjYear == 1999)
        {
          // -------------------------------------------------------
          // Adj amount is < 0. We are dealing with the lump sum
          // adjustment created by conversion in 08/99
          // --------------------------------------------------------
          // -------------------------------------------------------------
          // Calculate 'total Household' URA as sum(grant_amount)
          // ------------------------------------------------------------
          ReadImHouseholdMbrMnthlySum1();

          if (local.TotalHhUra.TotalCurrency == 0)
          {
            ExitState = "FN0000_TOTAL_HH_URA_IS_0";

            goto Test;
          }

          // -------------------------------------------------------------
          // Build a GV of all members that participated in HH and
          // corresponding URA amounts.
          // ------------------------------------------------------------
          local.PrevCsePerson.Number = "";

          local.Group.Index = 0;
          local.Group.Clear();

          foreach(var item1 in ReadCsePerson())
          {
            if (Equal(entities.Member.Number, local.PrevCsePerson.Number))
            {
              local.Group.Next();

              continue;
            }

            local.PrevCsePerson.Number = entities.Member.Number;
            local.Group.Update.CsePerson.Number = entities.Member.Number;

            // ----------------------------------------------
            // Calculate total 'member' URA
            // ----------------------------------------------
            ReadImHouseholdMbrMnthlySum2();
            local.Group.Next();
          }

          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            // -------------------------------------------------------------
            // Calculate member's share of Adj as % of total HH URA.
            // ------------------------------------------------------------
            local.MembersShareOfAdj.TotalCurrency =
              Math.Round(
                local.Group.Item.ImHouseholdMbrMnthlySum.UraAmount.
                GetValueOrDefault() * entities
              .UraAdjustment.AdjHouseholdUra.GetValueOrDefault() /
              local.TotalHhUra.TotalCurrency, 2, MidpointRounding.AwayFromZero);
              

            // -------------------------------------------------------------
            // Now apply it to member's URA. Oldest first.
            // ------------------------------------------------------------
            foreach(var item1 in ReadImHouseholdMbrMnthlySum3())
            {
              if (!Lt(-
                local.MembersShareOfAdj.TotalCurrency,
                entities.ImHouseholdMbrMnthlySum.UraAmount))
              {
                local.MembersShareOfAdj.TotalCurrency += entities.
                  ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();

                try
                {
                  CreateImHouseholdMbrMnthlyAdj1();
                  ++local.NbrOfUraAdjCreated.Count;
                  local.NbrOfUraAdjCreated.TotalCurrency += entities.
                    ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      // ---------------------------------------------------------------
                      // Fatal Error
                      // ---------------------------------------------------------------
                      ExitState = "FN0000_IM_HOUSEHOLD_MMBR_ADJ_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }

                UpdateImHouseholdMbrMnthlySum();
              }
              else
              {
                try
                {
                  CreateImHouseholdMbrMnthlyAdj2();
                  ++local.NbrOfUraAdjCreated.Count;
                  local.NbrOfUraAdjCreated.TotalCurrency += entities.
                    ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      // ---------------------------------------------------------------
                      // Fatal Error
                      // ---------------------------------------------------------------
                      ExitState = "FN0000_IM_HOUSEHOLD_MMBR_ADJ_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }

                UpdateImHouseholdMbrMnthlySum();

                // ---------------------------------------------------------------
                // All URA for this member has been exhausted. Now process
                // the next member.
                // ---------------------------------------------------------------
                local.MembersShareOfAdj.TotalCurrency = 0;

                goto Next;
              }
            }

Next:
            ;
          }

          if (local.TotalHhUra.TotalCurrency < -
            entities.UraAdjustment.AdjHouseholdUra.GetValueOrDefault())
          {
            ExitState = "FN0000_ADJ_AMT_EXCEDS_TOT_HH_URA";

            goto Test;
          }
        }

        if (entities.UraAdjustment.AdjMonth != 8 || entities
          .UraAdjustment.AdjYear != 1999)
        {
          ExitState = "FN0000_NEG_ADJ_FOR_NON_08_99";
        }
      }

Test:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.NbrOfErroredRecords.Count;
        local.NbrOfErroredRecords.TotalCurrency += entities.UraAdjustment.
          AdjHouseholdUra.GetValueOrDefault();
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.EabReportSend.RptDetail = "AE Case # :" + entities
          .ImHousehold.AeCaseNo;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; CSE person #:" + entities
          .CsePerson.Number;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Mbr Start date:" + NumberToString
          (DateToInt(entities.ImHouseholdMember.StartDate), 8, 8);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Adj #:" + NumberToString
          (entities.UraAdjustment.Identifier, 12, 4);
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "Adj Year =" + NumberToString
          (entities.UraAdjustment.AdjYear, 12, 4);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Adj Month:" + NumberToString
          (entities.UraAdjustment.AdjMonth, 14, 2);

        if (Lt(entities.UraAdjustment.AdjHouseholdUra, 0))
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "; Adj Amount: -$" + NumberToString
            ((long)(entities.UraAdjustment.AdjHouseholdUra.GetValueOrDefault() *
            100), 5, 11);
        }
        else
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "; Adj Amount: $" + NumberToString
            ((long)(entities.UraAdjustment.AdjHouseholdUra.GetValueOrDefault() *
            100), 5, 11);
        }

        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // -----------------------------------------
      // Check if commit count has been reached.
      // -----------------------------------------
      if (local.CommitCnt.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && AsChar
        (local.TestRun.Flag) != 'Y')
      {
        local.CommitCnt.Count = 0;

        if (!ReadProgramCheckpointRestart())
        {
          ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

          return;
        }

        local.ProgramCheckpointRestart.RestartInfo =
          entities.ImHousehold.AeCaseNo + entities.CsePerson.Number + NumberToString
          (DateToInt(entities.ImHouseholdMember.StartDate), 8, 8) + NumberToString
          (entities.UraAdjustment.Identifier, 10, 6);

        try
        {
          UpdateProgramCheckpointRestart1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
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
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .ImHousehold.AeCaseNo;
        UseCabControlReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      ++local.CommitCnt.Count;

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "*************Display for testing ******************";
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail =
          "Display Indicator is on. AE Case # :" + entities
          .ImHousehold.AeCaseNo + entities.CsePerson.Number + "; URA Adj #:" + NumberToString
          (entities.UraAdjustment.Identifier, 15);
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "Adjustment amount:" + NumberToString
          ((long)entities.UraAdjustment.AdjHouseholdUra.GetValueOrDefault(), 5,
          11);
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport1();

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

    // *********************
    // Write control totals
    // *********************
    local.EabReportSend.RptDetail =
      Substring(
        "Total number of records read........................................",
      1, 45) + NumberToString(local.NbrOfRecordsRead.Count, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (local.NbrOfRecordsRead.TotalCurrency < 0)
    {
      local.EabReportSend.RptDetail =
        Substring(
          "Total Amount of records read...................................................",
        1, 45) + "-$" + NumberToString
        ((long)(local.NbrOfRecordsRead.TotalCurrency * 100), 1, 15);
    }
    else
    {
      local.EabReportSend.RptDetail =
        Substring(
          "Total Amount of records read...................................................",
        1, 45) + "$" + NumberToString
        ((long)(local.NbrOfRecordsRead.TotalCurrency * 100), 1, 15);
    }

    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Total number of records in error................................................",
      1, 45) + NumberToString(local.NbrOfErroredRecords.Count, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (local.NbrOfErroredRecords.TotalCurrency < 0)
    {
      local.EabReportSend.RptDetail =
        Substring(
          "Total amount of records in error..................................................",
        1, 45) + "-$" + NumberToString
        ((long)(local.NbrOfErroredRecords.TotalCurrency * 100), 1, 15);
    }
    else
    {
      local.EabReportSend.RptDetail =
        Substring(
          "Total amount of records in error.......................................",
        1, 45) + "$" + NumberToString
        ((long)(local.NbrOfErroredRecords.TotalCurrency * 100), 1, 15);
    }

    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Total number of records created..................................................",
      1, 45) + NumberToString(local.NbrOfUraAdjCreated.Count, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (local.NbrOfUraAdjCreated.TotalCurrency < 0)
    {
      local.EabReportSend.RptDetail =
        Substring(
          "Total amount of records created................................................",
        1, 45) + "-$" + NumberToString
        ((long)(local.NbrOfUraAdjCreated.TotalCurrency * 100), 1, 15);
    }
    else
    {
      local.EabReportSend.RptDetail =
        Substring(
          "Total amount of records created................................................",
        1, 45) + "$" + NumberToString
        ((long)(local.NbrOfUraAdjCreated.TotalCurrency * 100), 1, 15);
    }

    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Number of checkpoints taken...................................................",
      1, 45) + NumberToString(local.NbrOfCheckpoint.Count, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *********************
    // Reset restart flag.
    // *********************
    if (!ReadProgramCheckpointRestart())
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
    UseCabControlReport2();
    UseCabErrorReport2();
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

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
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

    local.TestHh.AeCaseNo = useExport.TestHh.AeCaseNo;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    local.TestRun.Flag = useExport.TestRunInd.Flag;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void CreateImHouseholdMbrMnthlyAdj1()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var type1 = "A";
    var adjustmentAmount =
      -entities.ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
    var levelAppliedTo = "H";
    var createdBy = global.UserId;
    var createdTmst = Now();
    var imhAeCaseNo = entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo;
    var cspNumber = entities.ImHouseholdMbrMnthlySum.CspNumber;
    var imsMonth = entities.ImHouseholdMbrMnthlySum.Month;
    var imsYear = entities.ImHouseholdMbrMnthlySum.Year;
    var adjustmentReason = "C";

    entities.ImHouseholdMbrMnthlyAdj.Populated = false;
    Update("CreateImHouseholdMbrMnthlyAdj1",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "adjustmentAmt", adjustmentAmount);
        db.SetString(command, "levelAppliedTo", levelAppliedTo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "imsMonth", imsMonth);
        db.SetInt32(command, "imsYear", imsYear);
        db.SetString(command, "adjustmentReason", adjustmentReason);
      });

    entities.ImHouseholdMbrMnthlyAdj.Type1 = type1;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount = adjustmentAmount;
    entities.ImHouseholdMbrMnthlyAdj.LevelAppliedTo = levelAppliedTo;
    entities.ImHouseholdMbrMnthlyAdj.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlyAdj.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlyAdj.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlyAdj.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlyAdj.ImsMonth = imsMonth;
    entities.ImHouseholdMbrMnthlyAdj.ImsYear = imsYear;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentReason = adjustmentReason;
    entities.ImHouseholdMbrMnthlyAdj.Populated = true;
  }

  private void CreateImHouseholdMbrMnthlyAdj2()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var type1 = "A";
    var adjustmentAmount = local.MembersShareOfAdj.TotalCurrency;
    var levelAppliedTo = "H";
    var createdBy = global.UserId;
    var createdTmst = Now();
    var imhAeCaseNo = entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo;
    var cspNumber = entities.ImHouseholdMbrMnthlySum.CspNumber;
    var imsMonth = entities.ImHouseholdMbrMnthlySum.Month;
    var imsYear = entities.ImHouseholdMbrMnthlySum.Year;
    var adjustmentReason = "C";

    entities.ImHouseholdMbrMnthlyAdj.Populated = false;
    Update("CreateImHouseholdMbrMnthlyAdj2",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "adjustmentAmt", adjustmentAmount);
        db.SetString(command, "levelAppliedTo", levelAppliedTo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "imsMonth", imsMonth);
        db.SetInt32(command, "imsYear", imsYear);
        db.SetString(command, "adjustmentReason", adjustmentReason);
      });

    entities.ImHouseholdMbrMnthlyAdj.Type1 = type1;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount = adjustmentAmount;
    entities.ImHouseholdMbrMnthlyAdj.LevelAppliedTo = levelAppliedTo;
    entities.ImHouseholdMbrMnthlyAdj.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlyAdj.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlyAdj.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlyAdj.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlyAdj.ImsMonth = imsMonth;
    entities.ImHouseholdMbrMnthlyAdj.ImsYear = imsYear;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentReason = adjustmentReason;
    entities.ImHouseholdMbrMnthlyAdj.Populated = true;
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", entities.UraAdjustment.AdjYear);
        db.SetInt32(command, "month0", entities.UraAdjustment.AdjMonth);
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.Member.Number = db.GetString(reader, 0);
        entities.Member.Populated = true;

        return true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum1()
  {
    return Read("ReadImHouseholdMbrMnthlySum1",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", entities.UraAdjustment.AdjYear);
        db.SetInt32(command, "month0", entities.UraAdjustment.AdjMonth);
      },
      (db, reader) =>
      {
        local.TotalHhUra.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadImHouseholdMbrMnthlySum2()
  {
    local.Group.Item.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum2",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetString(command, "cspNumber", entities.Member.Number);
        db.SetInt32(command, "year0", entities.UraAdjustment.AdjYear);
        db.SetInt32(command, "month0", entities.UraAdjustment.AdjMonth);
      },
      (db, reader) =>
      {
        local.Group.Update.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 0);
        local.Group.Item.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySum3()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySum3",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetString(command, "cspNumber", local.Group.Item.CsePerson.Number);
        db.SetInt32(command, "year0", entities.UraAdjustment.AdjYear);
        db.SetInt32(command, "month0", entities.UraAdjustment.AdjMonth);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ImHouseholdMbrMnthlySum.CreatedBy = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 11);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 12);
        entities.ImHouseholdMbrMnthlySum.Populated = true;

        return true;
      });
  }

  private bool ReadProgramCheckpointRestart()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return Read("ReadProgramCheckpointRestart",
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

  private IEnumerable<bool> ReadUraAdjustmentImHouseholdMemberImHousehold()
  {
    entities.UraAdjustment.Populated = false;
    entities.ImHouseholdMember.Populated = false;
    entities.ImHousehold.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadUraAdjustmentImHouseholdMemberImHousehold",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", local.RestartImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.UraAdjustment.Identifier = db.GetInt32(reader, 0);
        entities.UraAdjustment.AdjYear = db.GetInt32(reader, 1);
        entities.UraAdjustment.AdjMonth = db.GetInt32(reader, 2);
        entities.UraAdjustment.AdjAdcGrant = db.GetNullableDecimal(reader, 3);
        entities.UraAdjustment.AdjPassthru = db.GetNullableDecimal(reader, 4);
        entities.UraAdjustment.AdjMedAssistance =
          db.GetNullableDecimal(reader, 5);
        entities.UraAdjustment.AdjFcGrant = db.GetNullableDecimal(reader, 6);
        entities.UraAdjustment.AdjHouseholdUra =
          db.GetNullableDecimal(reader, 7);
        entities.UraAdjustment.AdjReason = db.GetNullableString(reader, 8);
        entities.UraAdjustment.CreatedBy = db.GetString(reader, 9);
        entities.UraAdjustment.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.UraAdjustment.CspNumber = db.GetString(reader, 11);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 11);
        entities.CsePerson.Number = db.GetString(reader, 11);
        entities.CsePerson.Number = db.GetString(reader, 11);
        entities.UraAdjustment.ImhAeCaseNo = db.GetString(reader, 12);
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 12);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 12);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 12);
        entities.UraAdjustment.IhmStartDate = db.GetDate(reader, 13);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 13);
        entities.UraAdjustment.AdjHholdMedicalUra =
          db.GetNullableDecimal(reader, 14);
        entities.UraAdjustment.Populated = true;
        entities.ImHouseholdMember.Populated = true;
        entities.ImHousehold.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var uraAmount =
      entities.ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault() +
      entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "year0", entities.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", entities.ImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
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
      /// A value of ImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("imHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
      {
        get => imHouseholdMbrMnthlySum ??= new();
        set => imHouseholdMbrMnthlySum = value;
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

      private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
      private CsePerson csePerson;
    }

    /// <summary>
    /// A value of TreatAsPositiveAdj.
    /// </summary>
    [JsonPropertyName("treatAsPositiveAdj")]
    public Common TreatAsPositiveAdj
    {
      get => treatAsPositiveAdj ??= new();
      set => treatAsPositiveAdj = value;
    }

    /// <summary>
    /// A value of MembersShareOfAdj.
    /// </summary>
    [JsonPropertyName("membersShareOfAdj")]
    public Common MembersShareOfAdj
    {
      get => membersShareOfAdj ??= new();
      set => membersShareOfAdj = value;
    }

    /// <summary>
    /// A value of TotalMemberUra.
    /// </summary>
    [JsonPropertyName("totalMemberUra")]
    public Common TotalMemberUra
    {
      get => totalMemberUra ??= new();
      set => totalMemberUra = value;
    }

    /// <summary>
    /// A value of TotalHhUra.
    /// </summary>
    [JsonPropertyName("totalHhUra")]
    public Common TotalHhUra
    {
      get => totalHhUra ??= new();
      set => totalHhUra = value;
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
    /// A value of RestartUraAdjustment.
    /// </summary>
    [JsonPropertyName("restartUraAdjustment")]
    public UraAdjustment RestartUraAdjustment
    {
      get => restartUraAdjustment ??= new();
      set => restartUraAdjustment = value;
    }

    /// <summary>
    /// A value of RestartImHouseholdMember.
    /// </summary>
    [JsonPropertyName("restartImHouseholdMember")]
    public ImHouseholdMember RestartImHouseholdMember
    {
      get => restartImHouseholdMember ??= new();
      set => restartImHouseholdMember = value;
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
    /// A value of DummyLoop.
    /// </summary>
    [JsonPropertyName("dummyLoop")]
    public Common DummyLoop
    {
      get => dummyLoop ??= new();
      set => dummyLoop = value;
    }

    /// <summary>
    /// A value of IncrementHura.
    /// </summary>
    [JsonPropertyName("incrementHura")]
    public Common IncrementHura
    {
      get => incrementHura ??= new();
      set => incrementHura = value;
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
    /// A value of BalanceHura.
    /// </summary>
    [JsonPropertyName("balanceHura")]
    public Common BalanceHura
    {
      get => balanceHura ??= new();
      set => balanceHura = value;
    }

    /// <summary>
    /// A value of NbrOfUraAdjCreated.
    /// </summary>
    [JsonPropertyName("nbrOfUraAdjCreated")]
    public Common NbrOfUraAdjCreated
    {
      get => nbrOfUraAdjCreated ??= new();
      set => nbrOfUraAdjCreated = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Medical.
    /// </summary>
    [JsonPropertyName("medical")]
    public Common Medical
    {
      get => medical ??= new();
      set => medical = value;
    }

    /// <summary>
    /// A value of Grant.
    /// </summary>
    [JsonPropertyName("grant")]
    public Common Grant
    {
      get => grant ??= new();
      set => grant = value;
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
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public ImHouseholdMember FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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
    /// A value of LastOfMonth.
    /// </summary>
    [JsonPropertyName("lastOfMonth")]
    public ImHouseholdMember LastOfMonth
    {
      get => lastOfMonth ??= new();
      set => lastOfMonth = value;
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
    /// A value of NbrOfCheckpoint.
    /// </summary>
    [JsonPropertyName("nbrOfCheckpoint")]
    public Common NbrOfCheckpoint
    {
      get => nbrOfCheckpoint ??= new();
      set => nbrOfCheckpoint = value;
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
    /// A value of NbrOfErroredRecords.
    /// </summary>
    [JsonPropertyName("nbrOfErroredRecords")]
    public Common NbrOfErroredRecords
    {
      get => nbrOfErroredRecords ??= new();
      set => nbrOfErroredRecords = value;
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

    private Common treatAsPositiveAdj;
    private Common membersShareOfAdj;
    private Common totalMemberUra;
    private Common totalHhUra;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private UraAdjustment restartUraAdjustment;
    private ImHouseholdMember restartImHouseholdMember;
    private ImHousehold restartImHousehold;
    private CsePerson restartCsePerson;
    private Common dummyLoop;
    private Common incrementHura;
    private Common currentCountInHh;
    private Common balanceHura;
    private Common nbrOfUraAdjCreated;
    private ImHousehold testHh;
    private Common displayInd;
    private Common testRun;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private Common medical;
    private Common grant;
    private Common nbrOfMbrsInHh;
    private ImHouseholdMember firstOfMonth;
    private Array<GroupGroup> group;
    private Common commitCnt;
    private External forCommit;
    private EabFileHandling eabFileHandling;
    private EabFileHandling status;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private ImHouseholdMember lastOfMonth;
    private Common nbrOfRecordsRead;
    private Common nbrOfCheckpoint;
    private ImHousehold prevImHousehold;
    private Common nbrOfErroredRecords;
    private CsePerson prevCsePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Member.
    /// </summary>
    [JsonPropertyName("member")]
    public CsePerson Member
    {
      get => member ??= new();
      set => member = value;
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
    /// A value of Asd.
    /// </summary>
    [JsonPropertyName("asd")]
    public ImHouseholdMember Asd
    {
      get => asd ??= new();
      set => asd = value;
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
    /// A value of ImHouseholdMember.
    /// </summary>
    [JsonPropertyName("imHouseholdMember")]
    public ImHouseholdMember ImHouseholdMember
    {
      get => imHouseholdMember ??= new();
      set => imHouseholdMember = value;
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
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson member;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ImHouseholdMember asd;
    private ImHouseholdMonthlyDebitSumma imHouseholdMonthlyDebitSumma;
    private UraAdjustment uraAdjustment;
    private ImHouseholdMember imHouseholdMember;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private ImHousehold imHousehold;
    private CsePerson csePerson;
  }
#endregion
}

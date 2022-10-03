// Program: FN_BFXJ_DRA_FEE_INITIALIZATION, ID: 371343384, model: 746.
// Short name: SWEFFXJB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_BFXJ_DRA_FEE_INITIALIZATION.
/// </para>
/// <para>
/// Supplement to the OCSE396a report which identifies the number of non-TAF 
/// cases that qualified for the $25 Deficit Reduction Act fee during the
/// quarter.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxjDraFeeInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXJ_DRA_FEE_INITIALIZATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxjDraFeeInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxjDraFeeInitialization.
  /// </summary>
  public FnBfxjDraFeeInitialization(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // -------------------------------------------------------------------------
    // 03/17/2008  GVandy	CQ296		Initial Coding
    // -----------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -----------------------------------------------------------------------------------------------
    // Retrieve the PPI info.
    // -----------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.TranCode;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Control Report
    // -----------------------------------------------------------------------------------------------
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Get the DB2 commit frequency counts and determine if we are restarting.
    // -----------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -----------------------------------------------------------------------------------------------
      // Extract checkpoint/restart info...
      //   Disbursement Process Date	columns  1 - 10
      //   Obligee CSP Number		columns 11 - 20
      //   Disbursement Reference #	columns 21 - 34
      // -----------------------------------------------------------------------------------------------
      local.RestartDisbursementTransaction.ProcessDate =
        StringToDate(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 10));
      local.RestartObligee.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 10);
      local.RestartDisbursementTransaction.ReferenceNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 21, 14);

      // -- Set the restart timestamp to the end of the restart day.
      local.RestartDateWorkArea.Timestamp =
        AddMicroseconds(AddDays(
          Timestamp(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 10)), 1), -1);

      // -- Write the restart info to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.NeededToWrite.RptDetail =
              "Process Restarting from:  Disbursement Process Date " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 1, 10);

            break;
          case 2:
            local.NeededToWrite.RptDetail =
              "                          Obligee CSP Number " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 11, 10);

            break;
          case 3:
            local.NeededToWrite.RptDetail =
              "                          Disbursement Reference # " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 21, 14);

            break;
          case 4:
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing Restart Info to control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Extract parameters from the PPI record.
    //   Display Flag			column   1
    //   Disbursement Start Date    	columns  2 - 11
    //   Disbursement End Date		columns 12 - 21
    // -----------------------------------------------------------------------------------------------
    if (CharAt(local.ProgramProcessingInfo.ParameterList, 1) == 'Y')
    {
      local.Display.Flag = "Y";
    }

    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 2, 10)))
    {
      local.Start.Date = new DateTime(2007, 10, 1);
    }
    else
    {
      local.Start.Date =
        StringToDate(Substring(local.ProgramProcessingInfo.ParameterList, 2, 10));
        
    }

    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 12, 10)))
    {
      local.End.Date = local.ProgramProcessingInfo.ProcessDate;
    }
    else
    {
      local.End.Date =
        StringToDate(
          Substring(local.ProgramProcessingInfo.ParameterList, 12, 10));
    }

    // -- Convert start and end date to text values (i.e. format YYYY-MM-DD).
    local.StartDate.Text10 = UseCabDate2TextWithHyphens3();
    local.EndDate.Text10 = UseCabDate2TextWithHyphens2();

    // -- Create starting and ending timestamp values.
    local.Start.Timestamp = Timestamp(local.StartDate.Text10);
    local.End.Timestamp =
      AddMicroseconds(AddDays(Timestamp(local.EndDate.Text10), 1), -1);

    // -- Write parameter info to the control report.
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          if (AsChar(local.Display.Flag) == 'Y')
          {
            local.NeededToWrite.RptDetail =
              "Display Flag = Y on the PPI record.";
          }
          else
          {
            local.NeededToWrite.RptDetail =
              "Display Flag not set on the PPI record.";
          }

          break;
        case 2:
          local.NeededToWrite.RptDetail = "";

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "Starting Disbursement Process Date " + local.StartDate.Text10;

          break;
        case 4:
          local.NeededToWrite.RptDetail =
            "Ending Disbursement Process Date " + local.EndDate.Text10;

          break;
        case 5:
          local.NeededToWrite.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing parameter info to control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -- Load all recovery obligations created during the period into a group 
    // view for later processing.
    local.Recovery.Index = -1;

    foreach(var item in ReadObligation())
    {
      if (!ReadCsePersonCsePersonAccountObligationType())
      {
        continue;
      }

      if (local.Recovery.Index + 1 >= Local.RecoveryGroup.Capacity)
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Recovery group view is full.  Expand the group view size.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Recovery.Index;
      local.Recovery.CheckSize();

      local.Recovery.Update.GlocalRecoveryCsePerson.Number =
        entities.RecoveryCsePerson.Number;
      local.Recovery.Update.GlocalRecoveryCsePersonAccount.Type1 =
        entities.RecoveryObligor.Type1;
      MoveObligation(entities.RecoveryObligation,
        local.Recovery.Update.GlocalRecoveryObligation);
      local.Recovery.Update.GlocalRecoveryObligationType.
        SystemGeneratedIdentifier =
          entities.RecoveryObligationType.SystemGeneratedIdentifier;
      ++local.TotalNumberRecoveryObs.Count;
    }

    if (local.Recovery.Count > 0)
    {
      local.Recovery.Index = 0;
      local.Recovery.CheckSize();
    }

    // -----------------------------------------------------------------------------------------------
    // Read each disbursement debit processed between the start and end date
    // (e.g. between 10-01-2007 and the current date).
    // -----------------------------------------------------------------------------------------------
    foreach(var item in ReadDisbursementTransactionCsePerson())
    {
      if (Equal(local.PreviousObligee.Number, entities.Obligee2.Number) && Equal
        (local.PreviousDebit.ProcessDate, entities.DistinctDebit.ProcessDate) &&
        Equal
        (local.PreviousDebit.ReferenceNumber,
        entities.DistinctDebit.ReferenceNumber))
      {
        continue;
      }
      else
      {
        local.PreviousObligee.Number = entities.Obligee2.Number;
        MoveDisbursementTransaction2(entities.DistinctDebit, local.PreviousDebit);
          
      }

      ++local.NumberOfReads.Count;
      ++local.TotalNumberRecordsRead.Count;

      if (local.Recovery.Count > 0)
      {
        // -- Process all recovery obligations that were created on or before 
        // the disbursement process date.
        while(!Lt(entities.DistinctDebit.ProcessDate,
          Date(local.Recovery.Item.GlocalRecoveryObligation.CreatedTmst)))
        {
          UseFnDraFeeProcessRecoveryObg();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -- Log message to error report and abort.
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "AR " + local
              .Recovery.Item.GlocalRecoveryCsePerson.Number + "  Recovery Obligation Sys Gen ID " +
              NumberToString
              (local.Recovery.Item.GlocalRecoveryObligation.
                SystemGeneratedIdentifier, 13, 3) + "...." + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (local.Group.Item.GlocalActualRecovery.Amount != 0 && AsChar
              (local.Group.Item.GlocalTafIndicator.Flag) == 'N')
            {
              ++local.NumRecovObDraEligible.Count;

              break;
            }
          }

          if (AsChar(local.Display.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";

            if (local.Group.IsEmpty)
            {
              // -- Log the AR number, Recovery Date, and Recovery Amount.
              local.NeededToWrite.RptDetail = "Recovery - AR # " + local
                .Recovery.Item.GlocalRecoveryCsePerson.Number;
              local.DateWorkArea.Date =
                Date(local.Recovery.Item.GlocalRecoveryObligation.CreatedTmst);
              local.WorkArea.Text10 = UseCabFormatDate();
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " Date " + local
                .WorkArea.Text10;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " Recovery Amount " + NumberToString
                ((long)(local.Recovery1.Amount * 100), 5, 11);

              if (local.Recovery1.Amount < 0)
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "-";
              }

              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " AP # " + "n/a";
              UseCabControlReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Error encountered writing Disbursement Transaction info to control report.";
                  
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }
            else
            {
              for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
                local.Group.Index)
              {
                // -- Log the AR number, Recovery Date, Recovery Amount, AP 
                // number, TAF indicator, and amount of recovery allocated to
                // the AP.
                local.NeededToWrite.RptDetail = "Recovery - AR # " + local
                  .Recovery.Item.GlocalRecoveryCsePerson.Number;
                local.DateWorkArea.Date =
                  Date(local.Recovery.Item.GlocalRecoveryObligation.CreatedTmst);
                  
                local.WorkArea.Text10 = UseCabFormatDate();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Date " + local
                  .WorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Recovery Amount " +
                  NumberToString((long)(local.Recovery1.Amount * 100), 5, 11);

                if (local.Recovery1.Amount < 0)
                {
                  local.NeededToWrite.RptDetail =
                    TrimEnd(local.NeededToWrite.RptDetail) + "-";
                }

                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " AP # " + local
                  .Group.Item.G.Number;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " TAF " + local
                  .Group.Item.GlocalTafIndicator.Flag;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Amount Allocated to AP " +
                  NumberToString
                  ((long)(local.Group.Item.GlocalActualRecovery.Amount * 100),
                  5, 11);

                if (local.Group.Item.GlocalActualRecovery.Amount < 0)
                {
                  local.NeededToWrite.RptDetail =
                    TrimEnd(local.NeededToWrite.RptDetail) + "-";
                }

                UseCabControlReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.NeededToWrite.RptDetail =
                    "Error encountered writing Disbursement Transaction info to control report.";
                    
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
            }
          }

          if (local.Recovery.Index + 1 < local.Recovery.Count)
          {
            ++local.Recovery.Index;
            local.Recovery.CheckSize();
          }
          else
          {
            local.Recovery.Count = 0;

            break;
          }
        }
      }

      // -- Process the disbursement, determine if it qualifies to be included 
      // in the Disbursement Summary non_taf_amount for the AR/AP combination.
      UseFnDraFeeProcessDisbursement();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Log message to error report and abort.
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Obligee " + entities.Obligee2.Number;
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " Obligor " + local
          .Obligor.Number;
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " Ref # " + entities
          .DistinctDebit.ReferenceNumber;
        local.DateWorkArea.Date = entities.DistinctDebit.ProcessDate;
        local.WorkArea.Text10 = UseCabFormatDate();
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " Disb Date " + local
          .WorkArea.Text10;
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + "...." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Keep track of the number of disbursements that were counted as 
      // non_taf_amounts and eligible for the DRA Fee.
      if (AsChar(local.DisbCountedForDraFee.Flag) == 'Y')
      {
        ++local.NumberDisbDraEligible.Count;
      }

      if (AsChar(local.Display.Flag) == 'Y')
      {
        // -- Log the obligee number, obligor number, reference number, 
        // disbursement process date, amount,
        //    TAF indicator and whether DRA Eligible to the control report.
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Obligee " + local.Obligee.Number;
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " Obligor " + local
          .Obligor.Number;
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " Ref # " + (
            local.Debit.ReferenceNumber ?? "");
        local.DateWorkArea.Date = local.Debit.ProcessDate;
        local.WorkArea.Text10 = UseCabFormatDate();
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " Disb Date " + local
          .WorkArea.Text10;
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " Amount " + NumberToString
          ((long)(local.Debit.Amount * 100), 15);

        if (local.Debit.Amount < 0)
        {
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + "-";
        }

        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " DRA Eligible " + local
          .DisbCountedForDraFee.Flag;
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " TAF " + local
          .TafIndicator.Flag;
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing Disbursement Transaction info to control report.";
            
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // --  Check for commit point.
      if (local.NumberOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // --  Update program checkpoint restart with current commit position.
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -----------------------------------------------------------------------------------------------
        // Save checkpoint/restart info...
        //   Disbursement Process Date	columns  1 - 10
        //   Obligee CSP Number		columns 11 - 20
        //   Disbursement Reference #	columns 21 - 34
        // -----------------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInfo = "";
        local.DateWorkArea.Date = entities.DistinctDebit.ProcessDate;
        local.WorkArea.Text10 = UseCabDate2TextWithHyphens1();
        local.ProgramCheckpointRestart.RestartInfo = local.WorkArea.Text10;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + entities
          .Obligee2.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + entities
          .DistinctDebit.ReferenceNumber;
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Update of checkpoint restart failed.  Restart Info " + (
              local.ProgramCheckpointRestart.RestartInfo ?? "");
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Commit Failed for Checkpoint Info " + (
              local.ProgramCheckpointRestart.RestartInfo ?? "");
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NumberOfReads.Count = 0;
      }
    }

    // -- Process any remaining recovery debts.
    if (local.Recovery.Count > 0)
    {
      while(local.Recovery.Index < local.Recovery.Count)
      {
        UseFnDraFeeProcessRecoveryObg();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Log message to error report and abort.
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "AR " + local
            .Recovery.Item.GlocalRecoveryCsePerson.Number + "  Recovery Obligation Sys Gen ID " +
            NumberToString
            (local.Recovery.Item.GlocalRecoveryObligation.
              SystemGeneratedIdentifier, 13, 3) + "...." + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (local.Group.Item.GlocalActualRecovery.Amount != 0 && AsChar
            (local.Group.Item.GlocalTafIndicator.Flag) == 'N')
          {
            ++local.NumRecovObDraEligible.Count;

            break;
          }
        }

        if (AsChar(local.Display.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";

          if (local.Group.IsEmpty)
          {
            // -- Log the AR number, Recovery Date, and Recovery Amount.
            local.NeededToWrite.RptDetail = "Recovery - AR # " + local
              .Recovery.Item.GlocalRecoveryCsePerson.Number;
            local.DateWorkArea.Date =
              Date(local.Recovery.Item.GlocalRecoveryObligation.CreatedTmst);
            local.WorkArea.Text10 = UseCabFormatDate();
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + " Date " + local
              .WorkArea.Text10;
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + " Recovery Amount " + NumberToString
              ((long)(local.Recovery1.Amount * 100), 5, 11);

            if (local.Recovery1.Amount < 0)
            {
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "-";
            }

            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + " AP # " + "n/a";
            UseCabControlReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Error encountered writing Disbursement Transaction info to control report.";
                
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
          else
          {
            for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
              local.Group.Index)
            {
              // -- Log the AR number, Recovery Date, Recovery Amount, AP 
              // number, TAF indicator, and amount of recovery allocated to the
              // AP.
              local.NeededToWrite.RptDetail = "Recovery - AR # " + local
                .Recovery.Item.GlocalRecoveryCsePerson.Number;
              local.DateWorkArea.Date =
                Date(local.Recovery.Item.GlocalRecoveryObligation.CreatedTmst);
              local.WorkArea.Text10 = UseCabFormatDate();
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " Date " + local
                .WorkArea.Text10;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " Recovery Amount " + NumberToString
                ((long)(local.Recovery1.Amount * 100), 5, 11);

              if (local.Recovery1.Amount < 0)
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "-";
              }

              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " AP # " + local
                .Group.Item.G.Number;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " TAF " + local
                .Group.Item.GlocalTafIndicator.Flag;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " Amount Allocated to AP " +
                NumberToString
                ((long)(local.Group.Item.GlocalActualRecovery.Amount * 100), 5,
                11);

              if (local.Group.Item.GlocalActualRecovery.Amount < 0)
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "-";
              }

              UseCabControlReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Error encountered writing Disbursement Transaction info to control report.";
                  
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }
          }
        }

        if (local.Recovery.Index + 1 < local.Recovery.Count)
        {
          ++local.Recovery.Index;
          local.Recovery.CheckSize();
        }
        else
        {
          local.Recovery.Count = 0;

          break;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Log the total number of Disbursement Transaction records read and the 
    // number that were
    // DRA eligible to the control report.
    // -----------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 7; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.NeededToWrite.RptDetail = "";

          break;
        case 2:
          local.NeededToWrite.RptDetail =
            "Total Number of Disbursement Transaction Records Read...." + NumberToString
            (local.TotalNumberRecordsRead.Count, 6, 10);

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "Number of Disbursement Transactions DRA Eligible........." + NumberToString
            (local.NumberDisbDraEligible.Count, 6, 10);

          break;
        case 4:
          local.NeededToWrite.RptDetail = "";

          break;
        case 5:
          local.NeededToWrite.RptDetail =
            "Total Number of Recovery Obligations Read................" + NumberToString
            (local.TotalNumberRecoveryObs.Count, 6, 10);

          break;
        case 6:
          local.NeededToWrite.RptDetail =
            "Number of Recovery Obligations DRA Eligible.............." + NumberToString
            (local.NumRecovObDraEligible.Count, 6, 10);

          break;
        case 7:
          local.NeededToWrite.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing Totals to control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Take a final checkpoint.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.CheckpointCount = -1;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Final update of checkpoint restart table failed.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Control Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveDisbursementTransaction1(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveDisbursementTransaction2(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveGroup(FnDraFeeProcessRecoveryObg.Export.
    GroupGroup source, Local.GroupGroup target)
  {
    target.G.Number = source.G.Number;
    target.GlocalTafIndicator.Flag = source.GexportTafIndicator.Flag;
    target.GlocalPotentialRecovery.Amount =
      source.GexportPotentialRecovery.Amount;
    target.GlocalActualRecovery.Amount = source.GexportActualRecovery.Amount;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTmst = source.CreatedTmst;
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
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private string UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text10;
  }

  private string UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.End.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text10;
  }

  private string UseCabDate2TextWithHyphens3()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Start.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text10;
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
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private string UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnDraFeeProcessDisbursement()
  {
    var useImport = new FnDraFeeProcessDisbursement.Import();
    var useExport = new FnDraFeeProcessDisbursement.Export();

    useImport.Obligee.Number = entities.Obligee2.Number;
    useImport.Debit.Assign(entities.DistinctDebit);

    Call(FnDraFeeProcessDisbursement.Execute, useImport, useExport);

    local.TafIndicator.Flag = useExport.TafIndicator.Flag;
    local.DisbCountedForDraFee.Flag = useExport.DisbCountedForDraFee.Flag;
    MoveDisbursementTransaction1(useExport.Debit, local.Debit);
    local.Obligor.Number = useExport.Obligor.Number;
    local.Obligee.Number = useExport.Obligee.Number;
  }

  private void UseFnDraFeeProcessRecoveryObg()
  {
    var useImport = new FnDraFeeProcessRecoveryObg.Import();
    var useExport = new FnDraFeeProcessRecoveryObg.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      local.Recovery.Item.GlocalRecoveryObligationType.
        SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      local.Recovery.Item.GlocalRecoveryObligation.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 =
      local.Recovery.Item.GlocalRecoveryCsePersonAccount.Type1;
    useImport.Ar.Number = local.Recovery.Item.GlocalRecoveryCsePerson.Number;

    Call(FnDraFeeProcessRecoveryObg.Execute, useImport, useExport);

    local.Recovery1.Amount = useExport.Recovery.Amount;
    useExport.Group.CopyTo(local.Group, MoveGroup);
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadCsePersonCsePersonAccountObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.RecoveryObligation.Populated);
    entities.RecoveryCsePerson.Populated = false;
    entities.RecoveryObligor.Populated = false;
    entities.RecoveryObligationType.Populated = false;

    return Read("ReadCsePersonCsePersonAccountObligationType",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", entities.RecoveryObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.RecoveryObligation.CpaType);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.RecoveryObligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.RecoveryCsePerson.Number = db.GetString(reader, 0);
        entities.RecoveryCsePerson.Type1 = db.GetString(reader, 1);
        entities.RecoveryObligor.CspNumber = db.GetString(reader, 2);
        entities.RecoveryObligor.Type1 = db.GetString(reader, 3);
        entities.RecoveryObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.RecoveryCsePerson.Populated = true;
        entities.RecoveryObligor.Populated = true;
        entities.RecoveryObligationType.Populated = true;
        CheckValid<CsePerson>("Type1", entities.RecoveryCsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.RecoveryObligor.Type1);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionCsePerson()
  {
    entities.DistinctDebit.Populated = false;
    entities.Obligee2.Populated = false;

    return ReadEach("ReadDisbursementTransactionCsePerson",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.Start.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate",
          local.RestartDisbursementTransaction.ProcessDate.GetValueOrDefault());
          
        db.SetString(command, "numb", local.RestartObligee.Number);
        db.SetNullableString(
          command, "referenceNumber",
          local.RestartDisbursementTransaction.ReferenceNumber ?? "");
      },
      (db, reader) =>
      {
        entities.DistinctDebit.CpaType = db.GetString(reader, 0);
        entities.DistinctDebit.CspNumber = db.GetString(reader, 1);
        entities.DistinctDebit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DistinctDebit.Type1 = db.GetString(reader, 3);
        entities.DistinctDebit.ProcessDate = db.GetNullableDate(reader, 4);
        entities.DistinctDebit.DbtGeneratedId = db.GetNullableInt32(reader, 5);
        entities.DistinctDebit.ReferenceNumber =
          db.GetNullableString(reader, 6);
        entities.Obligee2.Number = db.GetString(reader, 7);
        entities.Obligee2.Type1 = db.GetString(reader, 8);
        entities.DistinctDebit.Populated = true;
        entities.Obligee2.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DistinctDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DistinctDebit.Type1);
        CheckValid<CsePerson>("Type1", entities.Obligee2.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.RecoveryObligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", local.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.End.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst",
          local.RestartDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.RecoveryObligation.CpaType = db.GetString(reader, 0);
        entities.RecoveryObligation.CspNumber = db.GetString(reader, 1);
        entities.RecoveryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.RecoveryObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.RecoveryObligation.CreatedTmst = db.GetDateTime(reader, 4);
        entities.RecoveryObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.RecoveryObligation.CpaType);

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePerson G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GlocalTafIndicator.
      /// </summary>
      [JsonPropertyName("glocalTafIndicator")]
      public Common GlocalTafIndicator
      {
        get => glocalTafIndicator ??= new();
        set => glocalTafIndicator = value;
      }

      /// <summary>
      /// A value of GlocalPotentialRecovery.
      /// </summary>
      [JsonPropertyName("glocalPotentialRecovery")]
      public DisbursementTransaction GlocalPotentialRecovery
      {
        get => glocalPotentialRecovery ??= new();
        set => glocalPotentialRecovery = value;
      }

      /// <summary>
      /// A value of GlocalActualRecovery.
      /// </summary>
      [JsonPropertyName("glocalActualRecovery")]
      public DisbursementTransaction GlocalActualRecovery
      {
        get => glocalActualRecovery ??= new();
        set => glocalActualRecovery = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePerson g;
      private Common glocalTafIndicator;
      private DisbursementTransaction glocalPotentialRecovery;
      private DisbursementTransaction glocalActualRecovery;
    }

    /// <summary>A RecoveryGroup group.</summary>
    [Serializable]
    public class RecoveryGroup
    {
      /// <summary>
      /// A value of GlocalRecoveryCsePerson.
      /// </summary>
      [JsonPropertyName("glocalRecoveryCsePerson")]
      public CsePerson GlocalRecoveryCsePerson
      {
        get => glocalRecoveryCsePerson ??= new();
        set => glocalRecoveryCsePerson = value;
      }

      /// <summary>
      /// A value of GlocalRecoveryCsePersonAccount.
      /// </summary>
      [JsonPropertyName("glocalRecoveryCsePersonAccount")]
      public CsePersonAccount GlocalRecoveryCsePersonAccount
      {
        get => glocalRecoveryCsePersonAccount ??= new();
        set => glocalRecoveryCsePersonAccount = value;
      }

      /// <summary>
      /// A value of GlocalRecoveryObligation.
      /// </summary>
      [JsonPropertyName("glocalRecoveryObligation")]
      public Obligation GlocalRecoveryObligation
      {
        get => glocalRecoveryObligation ??= new();
        set => glocalRecoveryObligation = value;
      }

      /// <summary>
      /// A value of GlocalRecoveryObligationType.
      /// </summary>
      [JsonPropertyName("glocalRecoveryObligationType")]
      public ObligationType GlocalRecoveryObligationType
      {
        get => glocalRecoveryObligationType ??= new();
        set => glocalRecoveryObligationType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private CsePerson glocalRecoveryCsePerson;
      private CsePersonAccount glocalRecoveryCsePersonAccount;
      private Obligation glocalRecoveryObligation;
      private ObligationType glocalRecoveryObligationType;
    }

    /// <summary>
    /// A value of TotalNumberRecoveryObs.
    /// </summary>
    [JsonPropertyName("totalNumberRecoveryObs")]
    public Common TotalNumberRecoveryObs
    {
      get => totalNumberRecoveryObs ??= new();
      set => totalNumberRecoveryObs = value;
    }

    /// <summary>
    /// A value of NumRecovObDraEligible.
    /// </summary>
    [JsonPropertyName("numRecovObDraEligible")]
    public Common NumRecovObDraEligible
    {
      get => numRecovObDraEligible ??= new();
      set => numRecovObDraEligible = value;
    }

    /// <summary>
    /// A value of TafIndicator.
    /// </summary>
    [JsonPropertyName("tafIndicator")]
    public Common TafIndicator
    {
      get => tafIndicator ??= new();
      set => tafIndicator = value;
    }

    /// <summary>
    /// A value of Recovery1.
    /// </summary>
    [JsonPropertyName("recovery1")]
    public ObligationTransaction Recovery1
    {
      get => recovery1 ??= new();
      set => recovery1 = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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
    /// A value of RestartDateWorkArea.
    /// </summary>
    [JsonPropertyName("restartDateWorkArea")]
    public DateWorkArea RestartDateWorkArea
    {
      get => restartDateWorkArea ??= new();
      set => restartDateWorkArea = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public WorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public WorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// Gets a value of Recovery.
    /// </summary>
    [JsonIgnore]
    public Array<RecoveryGroup> Recovery => recovery ??= new(
      RecoveryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Recovery for json serialization.
    /// </summary>
    [JsonPropertyName("recovery")]
    [Computed]
    public IList<RecoveryGroup> Recovery_Json
    {
      get => recovery;
      set => Recovery.Assign(value);
    }

    /// <summary>
    /// A value of NumberDisbDraEligible.
    /// </summary>
    [JsonPropertyName("numberDisbDraEligible")]
    public Common NumberDisbDraEligible
    {
      get => numberDisbDraEligible ??= new();
      set => numberDisbDraEligible = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of DisbCountedForDraFee.
    /// </summary>
    [JsonPropertyName("disbCountedForDraFee")]
    public Common DisbCountedForDraFee
    {
      get => disbCountedForDraFee ??= new();
      set => disbCountedForDraFee = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of RestartDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("restartDisbursementTransaction")]
    public DisbursementTransaction RestartDisbursementTransaction
    {
      get => restartDisbursementTransaction ??= new();
      set => restartDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of RestartObligee.
    /// </summary>
    [JsonPropertyName("restartObligee")]
    public CsePerson RestartObligee
    {
      get => restartObligee ??= new();
      set => restartObligee = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of TotalNumberRecordsRead.
    /// </summary>
    [JsonPropertyName("totalNumberRecordsRead")]
    public Common TotalNumberRecordsRead
    {
      get => totalNumberRecordsRead ??= new();
      set => totalNumberRecordsRead = value;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
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
    /// A value of PreviousObligee.
    /// </summary>
    [JsonPropertyName("previousObligee")]
    public CsePerson PreviousObligee
    {
      get => previousObligee ??= new();
      set => previousObligee = value;
    }

    /// <summary>
    /// A value of PreviousDebit.
    /// </summary>
    [JsonPropertyName("previousDebit")]
    public DisbursementTransaction PreviousDebit
    {
      get => previousDebit ??= new();
      set => previousDebit = value;
    }

    private Common totalNumberRecoveryObs;
    private Common numRecovObDraEligible;
    private Common tafIndicator;
    private ObligationTransaction recovery1;
    private DateWorkArea dateWorkArea;
    private WorkArea workArea;
    private Array<GroupGroup> group;
    private DateWorkArea restartDateWorkArea;
    private DateWorkArea end;
    private DateWorkArea start;
    private WorkArea endDate;
    private WorkArea startDate;
    private Array<RecoveryGroup> recovery;
    private Common numberDisbDraEligible;
    private CsePerson obligee;
    private CsePerson obligor;
    private DisbursementTransaction debit;
    private Common disbCountedForDraFee;
    private ExitStateWorkArea exitStateWorkArea;
    private Common common;
    private DisbursementTransaction restartDisbursementTransaction;
    private CsePerson restartObligee;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common numberOfReads;
    private Common totalNumberRecordsRead;
    private Common display;
    private External passArea;
    private CsePerson previousObligee;
    private DisbursementTransaction previousDebit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DistinctDebit.
    /// </summary>
    [JsonPropertyName("distinctDebit")]
    public DisbursementTransaction DistinctDebit
    {
      get => distinctDebit ??= new();
      set => distinctDebit = value;
    }

    /// <summary>
    /// A value of RecoveryCsePerson.
    /// </summary>
    [JsonPropertyName("recoveryCsePerson")]
    public CsePerson RecoveryCsePerson
    {
      get => recoveryCsePerson ??= new();
      set => recoveryCsePerson = value;
    }

    /// <summary>
    /// A value of RecoveryObligor.
    /// </summary>
    [JsonPropertyName("recoveryObligor")]
    public CsePersonAccount RecoveryObligor
    {
      get => recoveryObligor ??= new();
      set => recoveryObligor = value;
    }

    /// <summary>
    /// A value of RecoveryObligationType.
    /// </summary>
    [JsonPropertyName("recoveryObligationType")]
    public ObligationType RecoveryObligationType
    {
      get => recoveryObligationType ??= new();
      set => recoveryObligationType = value;
    }

    /// <summary>
    /// A value of RecoveryObligation.
    /// </summary>
    [JsonPropertyName("recoveryObligation")]
    public Obligation RecoveryObligation
    {
      get => recoveryObligation ??= new();
      set => recoveryObligation = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    private DisbursementTransaction distinctDebit;
    private CsePerson recoveryCsePerson;
    private CsePersonAccount recoveryObligor;
    private ObligationType recoveryObligationType;
    private Obligation recoveryObligation;
    private DisbursementType disbursementType;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
  }
#endregion
}

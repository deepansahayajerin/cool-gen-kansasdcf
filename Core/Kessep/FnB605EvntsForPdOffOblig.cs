// Program: FN_B605_EVNTS_FOR_PD_OFF_OBLIG, ID: 372449854, model: 746.
// Short name: SWEF605B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B605_EVNTS_FOR_PD_OFF_OBLIG.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB605EvntsForPdOffOblig: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B605_EVNTS_FOR_PD_OFF_OBLIG program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB605EvntsForPdOffOblig(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB605EvntsForPdOffOblig.
  /// </summary>
  public FnB605EvntsForPdOffOblig(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	  Request #	Description
    // --------  ---------	  ---------	
    // -------------------------------------------------------------------------------------------
    // ??/??/??  ??????			Initial Development
    // 04/28/97  Ty Hill			Change Current_date
    // 06/07/99  PATHY				Procedure was rewritten
    // 07/27/99  Srini Ganji			Code changed to process all records reading for 
    // AP or AR case units for CSE Person.
    // 08/25/99  Srini Ganji	  PR#234	Create an Infrastructure record for each 
    // case unit/case associated to the
    // 					obligation that is paid in full
    // 08/10/00  Madhu Kumar			End dating the obligation assignment when the 
    // obligation reaches zero (ie when the
    // 					obligation is deactivated).
    // 01/19/01  Madhu Kumar	  PR # 106748	Infrastructure record must have a 
    // case number.
    // 02/19/01  Vithal Madhira  WR# 000243	Eliminate the alert when the 
    // obligation paid off if other obligations are still active
    // 					for the same court order.  I changed the code to bypass the  
    // creation of infrastructure
    // 					record except in case of reactivated obligation. Call the '
    // Fn_cab_raise_event_paid_off'
    // 					CAB only if an obligation is reactivated.
    // 02/21/01  Vithal Madhira  PR# 114066	When the obligation is created the '
    // Last_obligation_event' is set to 'SPACES'. If the
    // 					obligation is not paid off before running this batch,  SPACES will 
    // be updated to 'A' (active).
    // 					With this PR we will update 'SPACES' to 'D' (Deactivated) only if 
    // the obligation is paid off.
    // 					S--->A will not happen now. S--->D will happen. If obligation is 
    // resurrected,   update D to 'A'
    // 					(Reactivated),  call 'Fn_cab_raise_event_paid_off'  and  create 
    // infrastructure
    // 					record with reason_code 'FNOBGREACT' and  raise the event.
    // 02/23/01  Vithal Madhira  PR# 114578	Alert and History records need to be
    // created when recovery obligations are paid off.
    // 04/05/06  GVandy	  PR#261670	Generate events for non recovery obligations
    // and include processing from SRRUN200.
    // -----------------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.MaxForDiscontinueDate.Date = UseCabSetMaximumDiscontinueDate();
    local.ObligRecsRead.Count = 0;
    local.Activated.Count = 0;
    local.Deactivated.Count = 0;
    local.Reactivated.Count = 0;
    local.InfraRecsCreated.Count = 0;
    local.ErrorRecs.Count = 0;
    local.Commit.Count = 0;
    local.NoOfObligationsUpdated.Count = 0;
    local.NoOfObsWCollsToProt.Count = 0;
    local.CreateObCollProtHist.Flag = "Y";
    local.ObligCollProtectionHist.ReasonText =
      "AUTO COLLECTION PROTECTION - OBLIGATION PAID IN FULL";

    // ------------------------------------------------------------------------------------------------------------
    // Get the RUN Paramateres for this program
    // ------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      local.ParamProcessDate.Date = local.ProgramProcessingInfo.ProcessDate;
    }
    else
    {
      local.ParamProcessDate.Date =
        IntToDate((int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 8)));
      local.Param.Number =
        Substring(local.ProgramProcessingInfo.ParameterList, 10, 10);
    }

    local.Processing.Date = local.ProgramProcessingInfo.ProcessDate;

    // ------------------------------------------------------------------------------------------------------------
    // Open ERROR Report, DD Name = RPT99
    // ------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------------------------------------
    // Open CONTROL Report, DD Name = RPT98
    // ------------------------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error opening Control Report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------------------------------------
    // Get the DB2 commit frequency counts.
    // ------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Checkpoint Restart not found";
      UseCabErrorReport1();
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

      return;
    }

    // ------------------------------------------------------------------------------------------------------------
    // Write Report Headers
    // ------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";

    if (!IsEmpty(local.Param.Number))
    {
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail = "Report for Obligor #: " + local
            .Param.Number;
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Control Report.";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
      }
    }

    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "CSE PERSON #    OBG TYPE    Reason";

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "-----------     --------    ---------------------------------------------------";
            

          break;
        default:
          break;
      }

      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------------------------------------
    // Main logic starts here...
    // ------------------------------------------------------------------------------------------------------------
    // -- Read each debt detail whose status has changed since the last run.
    foreach(var item in ReadCsePersonObligationObligationType())
    {
      ExitState = "ACO_NN0000_ALL_OK";

      if (!IsEmpty(local.Param.Number))
      {
        if (!Equal(local.Param.Number, entities.Obligor1.Number))
        {
          break;
        }
      }

      // -- Skip Voluntary obligations (obligations_type sys_gen_id = 16)
      if (entities.ObligationType.SystemGeneratedIdentifier == 16)
      {
        continue;
      }

      ++local.ObligRecsRead.Count;
      UseFnDetermineObligationDbtStat();

      switch(AsChar(local.ObligationPaidOffInd.Flag))
      {
        case 'Y':
          if (AsChar(entities.Obligation.LastObligationEvent) == 'D')
          {
            // -- No change in the obligation state (i.e. there is no active 
            // debt and the last obligation event is "D"eactive)  Skip this
            // obligation.
            continue;
          }

          // ---------------------------------------------------------
          // --  The obligation is newly deactivated.
          // ---------------------------------------------------------
          local.NewObligEvent.Flag = "D";
          ++local.Deactivated.Count;

          if (AsChar(entities.ObligationType.Classification) == 'R' || AsChar
            (entities.ObligationType.Classification) == 'F')
          {
            // -- Discontinue active worker assignment to the obligation.
            foreach(var item1 in ReadObligationAssignment())
            {
              try
              {
                UpdateObligationAssignment();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ++local.ErrorRecs.Count;
                    local.EabReportSend.RptDetail = entities.Obligor1.Number + "      " +
                      entities.ObligationType.Code + " Not Unique Violation updating Obligation Assignment.";
                      
                    UseCabErrorReport1();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_4_BATCH";

                      return;
                    }

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ++local.ErrorRecs.Count;
                    local.EabReportSend.RptDetail = entities.Obligor1.Number + "      " +
                      entities.ObligationType.Code + " PV Violation updating Obligation Assignment.";
                      
                    UseCabErrorReport1();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_4_BATCH";

                      return;
                    }

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          if (AsChar(entities.Obligation.PrimarySecondaryCode) != 'S')
          {
            UseFnProtectCollectionsForOblig();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = "ABENDING!!  Obligor # " + entities
                .Obligor1.Number + "  Error protecting collections... " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }

            if (AsChar(local.CollsFndToProtect.Flag) == 'Y')
            {
              ++local.NoOfObsWCollsToProt.Count;
            }
          }

          break;
        case 'N':
          if (AsChar(entities.Obligation.LastObligationEvent) == 'A')
          {
            // -- No change in the obligation state (i.e. there is active debt 
            // and the last obligation event is "A"ctive)  Skip this obligation.
            continue;
          }

          // ---------------------------------------------------------
          // --  The obligation is newly activated or reactivated
          // ---------------------------------------------------------
          local.NewObligEvent.Flag = "A";

          if (AsChar(entities.Obligation.LastObligationEvent) == 'D')
          {
            // --  This is a reactivate.
            ++local.Reactivated.Count;
          }
          else
          {
            // --  This is an initial activate.
            ++local.Activated.Count;
          }

          break;
        default:
          break;
      }

      // --  Raise events for all deactivates and reactivates.
      // --  Events are not raised for initial activates since that is done from
      // OACC, ONAC, OREC, OFEE, etc.
      if (AsChar(local.NewObligEvent.Flag) == 'D' || AsChar
        (local.NewObligEvent.Flag) == 'A' && AsChar
        (entities.Obligation.LastObligationEvent) == 'D')
      {
        local.Infrastructure.EventId = 47;
        local.Infrastructure.UserId = global.UserId;
        local.Infrastructure.BusinessObjectCd = "OBL";
        local.Infrastructure.CsePersonNumber = entities.Obligor1.Number;
        local.Infrastructure.SituationNumber = 0;
        UseFnCabRaiseEventPaidOff();
        local.InfraRecsCreated.Count += local.TempInfraRecsCreated.Count;

        if (AsChar(local.ErrorFound.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = entities.Obligor1.Number + "      " +
            entities.ObligationType.Code;

          switch(TrimEnd(local.ErrorFound.ActionEntry))
          {
            case "03":
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "   Event details not found";
                

              break;
            case "04":
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "   Obligation was not found";
                

              break;
            case "09":
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "   Infrastructure record not found";
                

              break;
            case "10":
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "   Infrastructure Id = 0";
                

              break;
            default:
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "   Unknown error";

              break;
          }

          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_4_BATCH";

            return;
          }

          ++local.ErrorRecs.Count;

          continue;
        }

        if (AsChar(local.NewObligEvent.Flag) == 'D' && AsChar
          (entities.ObligationType.Classification) != 'R' && AsChar
          (entities.ObligationType.Classification) != 'F')
        {
          // --  Raise event 7/240 (FNALLOBGPD) when the last non recovery/non 
          // fee debt for the case unit is retired.
          UseFnCheckIfLastDebt4CaseUnit();
          local.InfraRecsCreated.Count += local.TempInfraRecsCreated.Count;

          if (AsChar(local.ErrorFound.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = entities.Obligor1.Number + "      " +
              entities.ObligationType.Code;

            if (Equal(local.ErrorFound.ActionEntry, "03"))
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "   Event details not found";
                
            }
            else
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "   Unknown error";
            }

            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }

            ++local.ErrorRecs.Count;

            continue;
          }
        }
      }

      try
      {
        UpdateObligation();
        ++local.NoOfObligationsUpdated.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGATION_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGATION_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ------------------------------------------------------------------------------------------------------------
      // Check to Commit database
      // ------------------------------------------------------------------------------------------------------------
      ++local.Commit.Count;

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // ------------------------------------------------------------------------------------------------------------
        // Call an external that does a DB2 commit using a Cobol program.
        // ------------------------------------------------------------------------------------------------------------
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.Commit.Count = 0;
      }
    }

    // ------------------------------------------------------------------------------------------------------------
    // The job completed sucessfully, the Paramlist of Program processing 
    // information is updated with Process date.
    // ------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.ParameterList =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8);

    if (ReadProgramProcessingInfo1())
    {
      try
      {
        UpdateProgramProcessingInfo();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_PROCESSING_INFO_NU_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_PROCESSING_INFO_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    for(local.Common.Count = 1; local.Common.Count <= 8; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF OBLIGATIONS READ                              : " + NumberToString
            (local.ObligRecsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF INFRASTRUCTURE RECORDS CREATED                : " + NumberToString
            (local.InfraRecsCreated.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF OBLIGATIONS UPDATED                           : " + NumberToString
            (local.NoOfObligationsUpdated.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF RECORDS ACTIVATED                             : " + NumberToString
            (local.Activated.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF RECORDS DEACTIVATED                           : " + NumberToString
            (local.Deactivated.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF RECORDS REACTIVATED                           : " + NumberToString
            (local.Reactivated.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF ERROR  RECORDS CREATED                        : " + NumberToString
            (local.ErrorRecs.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "TOTAL NO. OF OBLIGATIONS WITH COLLECTIONS PROTECTED        : " + NumberToString
            (local.NoOfObsWCollsToProt.Count, 15);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report..";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        break;
      }
    }

    // ------------------------------------------------------------------------------------------------------------
    // Close CONTROL Report, DD Name = RPT98
    // ------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    // ------------------------------------------------------------------------------------------------------------
    // Close ERROR Report, DD Name = RPT99
    // ------------------------------------------------------------------------------------------------------------
    if (local.ErrorRecs.Count <= 0)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "------------- No Error Records for this run -------------";
      UseCabErrorReport1();
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";

    // ------------------------------------------------------------------------------------------------------------
    // End of Batch Job
    // ------------------------------------------------------------------------------------------------------------
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.ActionEntry = source.ActionEntry;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.LastObligationEvent = source.LastObligationEvent;
    target.CpaType = source.CpaType;
    target.CspNumber = source.CspNumber;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private void UseFnCabRaiseEventPaidOff()
  {
    var useImport = new FnCabRaiseEventPaidOff.Import();
    var useExport = new FnCabRaiseEventPaidOff.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.Action.Flag = local.NewObligEvent.Flag;
    useImport.Processing.Date = local.Processing.Date;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(FnCabRaiseEventPaidOff.Execute, useImport, useExport);

    local.TempInfraRecsCreated.Count = useExport.NoOfInfraRecsCreated.Count;
    MoveCommon(useExport.ErrorFound, local.ErrorFound);
  }

  private void UseFnCheckIfLastDebt4CaseUnit()
  {
    var useImport = new FnCheckIfLastDebt4CaseUnit.Import();
    var useExport = new FnCheckIfLastDebt4CaseUnit.Export();

    useImport.Process.Date = local.Processing.Date;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligor.Number = entities.Obligor1.Number;
    useImport.LastRunDate.Date = local.ParamProcessDate.Date;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;

    Call(FnCheckIfLastDebt4CaseUnit.Execute, useImport, useExport);

    local.TempInfraRecsCreated.Count = useExport.NoOfInfraRecsCreated.Count;
    MoveCommon(useExport.ErrorFound, local.ErrorFound);
  }

  private void UseFnDetermineObligationDbtStat()
  {
    var useImport = new FnDetermineObligationDbtStat.Import();
    var useExport = new FnDetermineObligationDbtStat.Export();

    useImport.Process.Date = local.Processing.Date;
    useImport.Persistant.Assign(entities.Obligation);

    Call(FnDetermineObligationDbtStat.Execute, useImport, useExport);

    local.ObligationPaidOffInd.Flag = useExport.PaidOffInd.Flag;
  }

  private void UseFnProtectCollectionsForOblig()
  {
    var useImport = new FnProtectCollectionsForOblig.Import();
    var useExport = new FnProtectCollectionsForOblig.Export();

    useImport.Persistent.Assign(entities.Obligation);
    useImport.ObligCollProtectionHist.ReasonText =
      local.ObligCollProtectionHist.ReasonText;
    useImport.CreateObCollProtHist.Flag = local.CreateObCollProtHist.Flag;

    Call(FnProtectCollectionsForOblig.Execute, useImport, useExport);

    MoveObligation(useImport.Persistent, entities.Obligation);
    local.CollsFndToProtect.Flag = useExport.CollsFndToProtect.Flag;
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

  private IEnumerable<bool> ReadCsePersonObligationObligationType()
  {
    entities.Obligor1.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadCsePersonObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Param.Number);
        db.SetDate(
          command, "effectiveDt",
          local.ParamProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 0);
        entities.Obligation.CpaType = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 6);
        entities.Obligation.LastObligationEvent =
          db.GetNullableString(reader, 7);
        entities.ObligationType.Code = db.GetString(reader, 8);
        entities.ObligationType.Classification = db.GetString(reader, 9);
        entities.Obligor1.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationAssignment.Populated = false;

    return ReadEach("ReadObligationAssignment",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "discontinueDate", date);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ObligationAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ObligationAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 5);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 6);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 7);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 8);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 10);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 11);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 12);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);

        return true;
      });
  }

  private bool ReadProgramProcessingInfo1()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = Now();
    var lastObligationEvent = local.NewObligEvent.Flag;

    entities.Obligation.Populated = false;
    Update("UpdateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetNullableString(command, "lastObligEvent", lastObligationEvent);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.LastUpdatedBy = lastUpdatedBy;
    entities.Obligation.LastUpdateTmst = lastUpdateTmst;
    entities.Obligation.LastObligationEvent = lastObligationEvent;
    entities.Obligation.Populated = true;
  }

  private void UpdateObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationAssignment.Populated);

    var discontinueDate = Now().Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ObligationAssignment.Populated = false;
    Update("UpdateObligationAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ObligationAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.ObligationAssignment.SpdId);
        db.SetInt32(command, "offId", entities.ObligationAssignment.OffId);
        db.SetString(command, "ospCode", entities.ObligationAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.SetInt32(command, "otyId", entities.ObligationAssignment.OtyId);
        db.SetString(command, "cpaType", entities.ObligationAssignment.CpaType);
        db.SetString(command, "cspNo", entities.ObligationAssignment.CspNo);
        db.SetInt32(command, "obgId", entities.ObligationAssignment.ObgId);
      });

    entities.ObligationAssignment.DiscontinueDate = discontinueDate;
    entities.ObligationAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ObligationAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ObligationAssignment.Populated = true;
  }

  private void UpdateProgramProcessingInfo()
  {
    var parameterList = local.ProgramProcessingInfo.ParameterList ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of TempInfraRecsCreated.
    /// </summary>
    [JsonPropertyName("tempInfraRecsCreated")]
    public Common TempInfraRecsCreated
    {
      get => tempInfraRecsCreated ??= new();
      set => tempInfraRecsCreated = value;
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
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of ObligRecsRead.
    /// </summary>
    [JsonPropertyName("obligRecsRead")]
    public Common ObligRecsRead
    {
      get => obligRecsRead ??= new();
      set => obligRecsRead = value;
    }

    /// <summary>
    /// A value of InfraRecsCreated.
    /// </summary>
    [JsonPropertyName("infraRecsCreated")]
    public Common InfraRecsCreated
    {
      get => infraRecsCreated ??= new();
      set => infraRecsCreated = value;
    }

    /// <summary>
    /// A value of ErrorRecs.
    /// </summary>
    [JsonPropertyName("errorRecs")]
    public Common ErrorRecs
    {
      get => errorRecs ??= new();
      set => errorRecs = value;
    }

    /// <summary>
    /// A value of Activated.
    /// </summary>
    [JsonPropertyName("activated")]
    public Common Activated
    {
      get => activated ??= new();
      set => activated = value;
    }

    /// <summary>
    /// A value of Deactivated.
    /// </summary>
    [JsonPropertyName("deactivated")]
    public Common Deactivated
    {
      get => deactivated ??= new();
      set => deactivated = value;
    }

    /// <summary>
    /// A value of Reactivated.
    /// </summary>
    [JsonPropertyName("reactivated")]
    public Common Reactivated
    {
      get => reactivated ??= new();
      set => reactivated = value;
    }

    /// <summary>
    /// A value of NoOfObsWCollsToProt.
    /// </summary>
    [JsonPropertyName("noOfObsWCollsToProt")]
    public Common NoOfObsWCollsToProt
    {
      get => noOfObsWCollsToProt ??= new();
      set => noOfObsWCollsToProt = value;
    }

    /// <summary>
    /// A value of NoOfObligationsUpdated.
    /// </summary>
    [JsonPropertyName("noOfObligationsUpdated")]
    public Common NoOfObligationsUpdated
    {
      get => noOfObligationsUpdated ??= new();
      set => noOfObligationsUpdated = value;
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
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of CreateObCollProtHist.
    /// </summary>
    [JsonPropertyName("createObCollProtHist")]
    public Common CreateObCollProtHist
    {
      get => createObCollProtHist ??= new();
      set => createObCollProtHist = value;
    }

    /// <summary>
    /// A value of CollsFndToProtect.
    /// </summary>
    [JsonPropertyName("collsFndToProtect")]
    public Common CollsFndToProtect
    {
      get => collsFndToProtect ??= new();
      set => collsFndToProtect = value;
    }

    /// <summary>
    /// A value of MaxForDiscontinueDate.
    /// </summary>
    [JsonPropertyName("maxForDiscontinueDate")]
    public DateWorkArea MaxForDiscontinueDate
    {
      get => maxForDiscontinueDate ??= new();
      set => maxForDiscontinueDate = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of ParamProcessDate.
    /// </summary>
    [JsonPropertyName("paramProcessDate")]
    public DateWorkArea ParamProcessDate
    {
      get => paramProcessDate ??= new();
      set => paramProcessDate = value;
    }

    /// <summary>
    /// A value of NewObligEvent.
    /// </summary>
    [JsonPropertyName("newObligEvent")]
    public Common NewObligEvent
    {
      get => newObligEvent ??= new();
      set => newObligEvent = value;
    }

    /// <summary>
    /// A value of Processing.
    /// </summary>
    [JsonPropertyName("processing")]
    public DateWorkArea Processing
    {
      get => processing ??= new();
      set => processing = value;
    }

    /// <summary>
    /// A value of ObligationPaidOffInd.
    /// </summary>
    [JsonPropertyName("obligationPaidOffInd")]
    public Common ObligationPaidOffInd
    {
      get => obligationPaidOffInd ??= new();
      set => obligationPaidOffInd = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of Param.
    /// </summary>
    [JsonPropertyName("param")]
    public CsePerson Param
    {
      get => param ??= new();
      set => param = value;
    }

    private Common tempInfraRecsCreated;
    private ExitStateWorkArea exitStateWorkArea;
    private Common commit;
    private Common obligRecsRead;
    private Common infraRecsCreated;
    private Common errorRecs;
    private Common activated;
    private Common deactivated;
    private Common reactivated;
    private Common noOfObsWCollsToProt;
    private Common noOfObligationsUpdated;
    private Common common;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Common createObCollProtHist;
    private Common collsFndToProtect;
    private DateWorkArea maxForDiscontinueDate;
    private Common errorFound;
    private DateWorkArea paramProcessDate;
    private Common newObligEvent;
    private DateWorkArea processing;
    private Common obligationPaidOffInd;
    private EabReportSend eabReportSend;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private CsePerson param;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private Obligation obligation;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private ObligationTransaction debt;
    private ObligationAssignment obligationAssignment;
  }
#endregion
}

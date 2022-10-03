// Program: SP_B302_DATE_MONITOR, ID: 372054589, model: 746.
// Short name: SWEP302B
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
/// A program: SP_B302_DATE_MONITOR.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB302DateMonitor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B302_DATE_MONITOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB302DateMonitor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB302DateMonitor.
  /// </summary>
  public SpB302DateMonitor(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // ø Date		Developer	Request #      Description ø
    // øææææææææææææææææææææææææææææææææææææææææææææææææææææææææææø
    // ø 20Dec 96      Alan Samuels                   Initial Dev ø
    // ø
    // 
    // ø
    // ø 21Mar 97      Regan Welborn          Changed Emancipationø
    // ø
    // 
    // ø
    // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    // -----------------------------------------------------------------------------------------------------------------
    // 12 May 97     J.Rookard     Clarify date logic, correct Infrastructure 
    // detail attribute population, insert
    // "when not found" error capture logic for Read Event, Event Detail.
    // 26Jan99 Crook Incorporated use of CAB_CONTROL_REPORT and CAB_ERROR_REPORT
    // and ACO_NN0000_ABEND_FOR_BATCH for abort exit state
    // 08/09/2001	SWSRRPM
    // PR # 124177 => For all event 6's, infrastructure records
    // should only be created per court order b/c the alert goes out
    // to the attorney assigned to the legal action.  This will prevent
    // them from being created on each case/AP/case unit.
    // -----------------------------------------------------------------------------------------------------------------
    // 12/27/2010  RMathews    CQ23999 Modified to update PPI parameter list for
    // SWEPB302 with last program process date
    // -----------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.CurrentDateWorkArea.Date = Now().Date;

    // ±æææææææææææææææææææææææææææææææææææææææææææÉ
    // ø Get the run parameters for this program.  ø
    // ø If the span between the two dates returnedø
    // ø is greater than one day, the job will be  ø
    // ø run for each day between the two dates,   ø
    // ø as well as the most recent date.          ø
    // þæææææææææææææææææææææææææææææææææææææææææææÊ
    local.CurrentProgramProcessingInfo.Name = global.UserId;
    UseReadPgmProcessInfoMultiDay();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.CurrentProgramProcessingInfo.Assign(local.Ending);

    // *****************************************************************
    // * Open the ERROR RPT. DDNAME=RPT99.                             *
    // ********************************************
    // Crook  27Jan99  ***
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.Ending.Name;
    local.NeededToOpen.ProcessDate = local.Ending.ProcessDate;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Open the CONTROL RPT. DDNAME=RPT98.                           *
    // ********************************************
    // Crook  27Jan99  ***
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Get Checkpoint restart information                            *
    // ********************************************
    // Crook  27Jan99  ***
    // ±æææææææææææææææææææææææææææææææææææææææææææÉ
    // ø Get the DB2 commit frequency counts.      ø
    // þæææææææææææææææææææææææææææææææææææææææææææÊ
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Restarted at " + Substring
          (local.ProgramCheckpointRestart.RestartInfo, 250, 1, 2);
        UseCabErrorReport3();
      }
    }
    else
    {
      // *****************************************************************
      // * Write a line to he ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered reading the Checkpoint Restart Information.";
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ProgramCheckpointRestart.CheckpointCount = 0;

    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // øRecords are processed by entity type for each date.       ø
    // øCommits are performed at the end of each entity type.     ø
    // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // øRead entity types based on restart value.                 ø
    // øThe event_id is 18 or 6 for all Infrastructure records
    // to ø
    // 
    // øbe created.
    // ø
    // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    // *****************************************************************
    // * Process Military Expected Discharge Date                      *
    // ********************************************
    // Crook  28Jan99  ***
    if (!Lt("02", Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
      2)))
    {
      if (Lt(local.Initialize.DenormDate, local.Starting.ProcessDate) && Lt
        (local.Starting.ProcessDate, AddDays(local.Ending.ProcessDate, -1)))
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.Starting.ProcessDate, 1);
      }
      else
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          local.Ending.ProcessDate;
      }

      while(!Lt(local.Ending.ProcessDate,
        local.CurrentProgramProcessingInfo.ProcessDate))
      {
        // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
        // øThe following timestamp value is used to compare against  ø
        // øa timestamp value of several of the entity types to be
        // ø
        // 
        // øread.  It represents 00:00:01 of the processing date.
        // ø
        // ø
        // 
        // ø
        // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
        local.PassToDateWorkArea.Date =
          local.CurrentProgramProcessingInfo.ProcessDate;
        UseCabDate2TextWithHyphens();
        local.ProcessDate.IefTimestamp =
          Timestamp(local.ReturnFromTextWorkArea.Text10);
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 18;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.BusinessObjectCd = "MIL";
        local.PassToInfrastructure.ReasonCode = "DMRLSEMIL";

        if (ReadEventEventDetail6())
        {
          foreach(var item in ReadMilitaryServiceCsePerson())
          {
            local.PassToDateWorkArea.Date =
              entities.MilitaryService.ExpectedDischargeDate;
            UseCabDate2TextWithHyphens();
            ++local.LcontrolTotNumbRecsRead.Count;
            local.PassToInfrastructure.CsePersonNumber =
              entities.CsePerson.Number;
            local.PassToInfrastructure.DenormDate =
              entities.MilitaryService.EffectiveDate;
            local.PassToInfrastructure.Detail = "CSE Person " + entities
              .CsePerson.Number + " expected to be discharged from military on " +
              local.ReturnFromTextWorkArea.Text10 + ".";
            local.PassToInfrastructure.SituationNumber = 0;

            foreach(var item1 in ReadCaseCaseUnit())
            {
              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.CaseUnitNumber =
                entities.CaseUnit.CuNumber;
              UseSpCabCreateInfrastructure();

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øIncrement counters and detect errors.		   	   ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.HoldForSafeKeeping.SystemGeneratedIdentifier =
                local.ProgramError.SystemGeneratedIdentifier;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // ø
              // 
              // ø
              // øFor non-critical errors you may write an error record to  ø
              // øthe program error entity type.                            ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.LcontrolTotNumbErrRecs.Count;
                UseEabExtractExitStateMessage();
                local.ProgramError.ProgramError1 =
                  local.ExitStateWorkArea.Message;

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øKEY INFO : This attrib contains info to identify the      ø
                // øtransaction record that caused the error. String contains:ø
                // ø MILITARY SERVICE EFFECTIVE DATE; CSE PERSON NUMBER       ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                local.DateWorkArea.TextDate =
                  NumberToString(DateToInt(
                    entities.MilitaryService.EffectiveDate), 8);
                local.ProgramError.KeyInfo =
                  "MILITARY SERVICE EFFECTIVE DATE " + local
                  .DateWorkArea.TextDate + "; CSE PERSON NUMBER " + entities
                  .CsePerson.Number;
                local.ProgramError.SystemGeneratedIdentifier =
                  local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
                ExitState = "ACO_NN0000_ALL_OK";

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øThis AB creates an error record.                          ø
                // øInput is a Persistent Program Run as well as a local view ø
                // øof Program Run in case currency is lost. Local Program_   ø
                // øProcessing_Info is also needed to reread Program Run.     ø
                // øProgram Error contains information about the current errorø
                // øas follows:
                // 
                // ø
                // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
                // ø   Key Info   : String identifying info for record in err ø
                // ø   Program Err: Set this to the extracted exit state msg  ø
                // ø
                // 
                // ø
                // øWARNING: Set the above attribs only. At the time of       ø
                // øwriting this template, there were a few superflous attribsø
                // øthat need to be deleted (from PROGRAM ERR)                ø
                // ø
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                // *****************************************************************
                // * Write a line to the ERROR RPT
                // 
                // *
                // *  replaces USE create_program_error above.
                // *
                // ********************************************
                // Crook  28Jan99  ***
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // ********************************************
                // Crook  28Jan99  ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  NumberToString(local.ProgramError.SystemGeneratedIdentifier,
                  13, 3);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
                local.PassToDateWorkArea.Date =
                  local.CurrentProgramProcessingInfo.ProcessDate;
                UseCabDate2TextWithHyphens();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.BusinessObjectCd;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.EventId, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.ReasonCode;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",MSE.Expectd_Dischrg_Dt="
                  ;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
                  (DateToInt(entities.MilitaryService.EffectiveDate), 8, 8);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSP.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CsePersonNumber, 10,
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"));
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.NeededToWrite.RptDetail = "    CAS.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CaseNumber, 10,
                  Verify(local.PassToInfrastructure.CaseNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CaseNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSU.Num=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                    GetValueOrDefault(), 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.Work.RptDetail = " " + (
                  local.ProgramError.ProgramError1 ?? "");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .Work.RptDetail;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              else
              {
                ++local.LcontrolTotNumbInfraCreated.Count;

                // ******End of EXIT STATE <> ALL_OK
              }
            }
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          ++local.LcontrolTotNumbErrRecs.Count;
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event18, Event Detail Reason Code DMRLSEMIL was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        if (Equal(local.CurrentProgramProcessingInfo.ProcessDate,
          local.Ending.ProcessDate))
        {
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øUpdate 1. number of checkpoints and the last checkpoint   ø
          // ø       2. last checkpoint time                            ø
          // ø       3. Set restart indicator to YES                    ø
          // ø       4. Restart Information                             ø
          // øAlso, return the checkpoint frequency count  in case they ø
          // øhave been changed since the last read.                    ø
          // øCAB increments checkpoint counter.                        ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øRestart info for this entity type is 03 to indicate       ø
          // øcompletion of MILITARY SERVICE.                           ø
          // ø
          // 
          // ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          local.ProgramCheckpointRestart.RestartInfo = "03";
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****************************************************************
            // * Write a line to he ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered updating the Program Checkpoint Restart Information.";
              
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ±æææææææææææææææææææææææÉ
          // øExternal DB2 commit.   ø
          // þæææææææææææææææææææææææÊ
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            // *****************************************************************
            // * Write a line to the ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered trying to COMMIT.";
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****End of Control Total Processing
        }

        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.CurrentProgramProcessingInfo.ProcessDate, 1);
      }
    }

    // *****************************************************************
    // * Process Hearing Conducted Date
    // 
    // *
    // ********************************************
    // Crook  28Jan99  ***
    if (!Lt("03", Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
      2)))
    {
      if (Lt(local.Initialize.DenormDate, local.Starting.ProcessDate) && Lt
        (local.Starting.ProcessDate, AddDays(local.Ending.ProcessDate, -1)))
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.Starting.ProcessDate, 1);
      }
      else
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          local.Ending.ProcessDate;
      }

      while(!Lt(local.Ending.ProcessDate,
        local.CurrentProgramProcessingInfo.ProcessDate))
      {
        // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
        // øThe following timestamp value is used to compare against  ø
        // øa timestamp value of several of the entity types to be
        // ø
        // 
        // øread.  It represents 00:00:01 of the processing date.
        // ø
        // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
        local.PassToDateWorkArea.Date =
          local.CurrentProgramProcessingInfo.ProcessDate;
        UseCabDate2TextWithHyphens();
        local.ProcessDate.IefTimestamp =
          Timestamp(local.ReturnFromTextWorkArea.Text10);
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 6;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.BusinessObjectCd = "LEA";
        local.PassToInfrastructure.ReasonCode = "DMHEARNGJ";

        if (ReadEventEventDetail7())
        {
          foreach(var item in ReadHearingLegalAction())
          {
            ++local.LcontrolTotNumbRecsRead.Count;
            local.PassToInfrastructure.DenormNumeric12 =
              entities.LegalAction.Identifier;
            local.PassToInfrastructure.SituationNumber = 0;

            foreach(var item1 in ReadCaseCaseRoleCaseUnitCsePersonLegalActionCaseRole())
              
            {
              if (Equal(entities.LegalAction.CourtCaseNumber,
                local.OneCourtOrder.CourtCaseNumber) && entities
                .LegalAction.Identifier == local.OneCourtOrder.Identifier)
              {
                goto ReadEach1;
              }
              else
              {
                local.OneCourtOrder.CourtCaseNumber =
                  entities.LegalAction.CourtCaseNumber;
                local.OneCourtOrder.Identifier =
                  entities.LegalAction.Identifier;
              }

              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              local.PassToInfrastructure.CsePersonNumber =
                entities.CsePerson.Number;
              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.CaseUnitNumber =
                entities.CaseUnit.CuNumber;
              local.PassToInfrastructure.Detail = "Court Case:" + TrimEnd
                (entities.LegalAction.CourtCaseNumber) + " Hearing held for AP " +
                entities.CsePerson.Number;
              local.PassToDateWorkArea.Date = entities.Hearing.ConductedDate;
              UseCabDate2TextWithHyphens();
              local.PassToInfrastructure.Detail =
                TrimEnd(local.PassToInfrastructure.Detail) + " on " + local
                .ReturnFromTextWorkArea.Text10;
              UseSpCabCreateInfrastructure();

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øIncrement counters and detect errors.		   	   ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.HoldForSafeKeeping.SystemGeneratedIdentifier =
                local.ProgramError.SystemGeneratedIdentifier;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // ø
              // 
              // ø
              // øFor non-critical errors you may write an error record to  ø
              // øthe program error entity type.                            ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.LcontrolTotNumbErrRecs.Count;
                UseEabExtractExitStateMessage();
                local.ProgramError.ProgramError1 =
                  local.ExitStateWorkArea.Message;

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øKEY INFO : This attrib contains info to identify the      ø
                // øtransaction record that caused the error. String contains:ø
                // øHEARING ID; LEGAL ACTION ID
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                local.ProgramError.KeyInfo = "HEARING ID " + NumberToString
                  (entities.Hearing.SystemGeneratedIdentifier, 15) + "; LEGAL ACTION ID " +
                  NumberToString(entities.LegalAction.Identifier, 15);
                local.ProgramError.SystemGeneratedIdentifier =
                  local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
                ExitState = "ACO_NN0000_ALL_OK";

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øThis AB creates an error record.                          ø
                // øInput is a Persistent Program Run as well as a local view ø
                // øof Program Run in case currency is lost. Local Program_   ø
                // øProcessing_Info is also needed to reread Program Run.     ø
                // øProgram Error contains information about the current errorø
                // øas follows:
                // 
                // ø
                // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
                // ø   Key Info   : String identifying info for record in err ø
                // ø   Program Err: Set this to the extracted exit state msg  ø
                // ø
                // 
                // ø
                // øWARNING: Set the above attribs only. At the time of       ø
                // øwriting this template, there were a few superflous attribsø
                // øthat need to be deleted (from PROGRAM ERR)                ø
                // ø
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                // *****************************************************************
                // * Write a line to the ERROR RPT
                // 
                // *
                // *  replaces USE create_program_error above.
                // *
                // ********************************************
                // Crook  28Jan99  ***
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // ********************************************
                // Crook  28Jan99  ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  NumberToString(local.ProgramError.SystemGeneratedIdentifier,
                  13, 3);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
                local.PassToDateWorkArea.Date =
                  local.CurrentProgramProcessingInfo.ProcessDate;
                UseCabDate2TextWithHyphens();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.BusinessObjectCd;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.EventId, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.ReasonCode;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",HRG.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.Hearing.SystemGeneratedIdentifier, 6,
                  10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",LGA.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.LegalAction.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",LGA.Court_Case=";
                local.TextWorkArea.Text10 =
                  entities.LegalAction.CourtCaseNumber ?? Spaces(10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.NeededToWrite.RptDetail = "    CSP.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CsePersonNumber, 10,
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CAS.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CaseNumber, 10,
                  Verify(local.PassToInfrastructure.CaseNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CaseNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSU.Num=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                    GetValueOrDefault(), 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.CaseRole.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Type=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + entities
                  .CaseRole.Type1;
                local.Work.RptDetail = " " + (
                  local.ProgramError.ProgramError1 ?? "");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .Work.RptDetail;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              else
              {
                ++local.LcontrolTotNumbInfraCreated.Count;

                // ******End of EXIT STATE <> ALL_OK
              }
            }

ReadEach1:
            ;
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          ++local.LcontrolTotNumbErrRecs.Count;
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event 6, Event Detail Reason Code DMHEARNGJ was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        if (Equal(local.CurrentProgramProcessingInfo.ProcessDate,
          local.Ending.ProcessDate))
        {
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øUpdate 1. number of checkpoints and the last checkpoint   ø
          // ø       2. last checkpoint time                            ø
          // ø       3. Set restart indicator to YES                    ø
          // ø       4. Restart Information                             ø
          // øAlso, return the checkpoint frequency count  in case they ø
          // øhave been changed since the last read.                    ø
          // øCAB increments checkpoint counter.                        ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øRestart info for this entity type is 04 to indicate       ø
          // øcompletion of HEARING.
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          local.ProgramCheckpointRestart.RestartInfo = "04";
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****************************************************************
            // * Write a line to he ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered updating the Program Checkpoint Restart Information.";
              
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ±æææææææææææææææææææææææÉ
          // øExternal DB2 commit.   ø
          // þæææææææææææææææææææææææÊ
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****End of Control Total Processing
        }

        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.CurrentProgramProcessingInfo.ProcessDate, 1);
      }
    }

    // *****************************************************************
    // * Process Service Process Service Request Date                  *
    // ********************************************
    // Crook  28Jan99  ***
    if (!Lt("04", Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
      2)))
    {
      if (Lt(local.Initialize.DenormDate, local.Starting.ProcessDate) && Lt
        (local.Starting.ProcessDate, AddDays(local.Ending.ProcessDate, -1)))
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.Starting.ProcessDate, 1);
      }
      else
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          local.Ending.ProcessDate;
      }

      local.OneCourtOrder.CourtCaseNumber = "";
      local.OneCourtOrder.Identifier = 0;

      while(!Lt(local.Ending.ProcessDate,
        local.CurrentProgramProcessingInfo.ProcessDate))
      {
        // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
        // øThe following timestamp value is used to compare against  ø
        // øa timestamp value of several of the entity types to be
        // ø
        // 
        // øread.  It represents 00:00:01 of the processing date.
        // ø
        // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
        local.PassToDateWorkArea.Date =
          local.CurrentProgramProcessingInfo.ProcessDate;
        UseCabDate2TextWithHyphens();
        local.ProcessDate.IefTimestamp =
          Timestamp(local.ReturnFromTextWorkArea.Text10);
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 6;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.BusinessObjectCd = "LEA";
        local.PassToInfrastructure.ReasonCode = "SRVREQSUB";

        if (ReadEventEventDetail8())
        {
          foreach(var item in ReadServiceProcessLegalAction())
          {
            ++local.LcontrolTotNumbRecsRead.Count;
            local.PassToInfrastructure.DenormNumeric12 =
              entities.LegalAction.Identifier;
            local.PassToInfrastructure.SituationNumber = 0;

            foreach(var item1 in ReadCaseCaseUnitCsePersonLegalActionCaseRole())
            {
              if (Equal(entities.LegalAction.CourtCaseNumber,
                local.OneCourtOrder.CourtCaseNumber) && entities
                .LegalAction.Identifier == local.OneCourtOrder.Identifier)
              {
                goto ReadEach2;
              }
              else
              {
                local.OneCourtOrder.CourtCaseNumber =
                  entities.LegalAction.CourtCaseNumber;
                local.OneCourtOrder.Identifier =
                  entities.LegalAction.Identifier;
              }

              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              local.PassToInfrastructure.CsePersonNumber =
                entities.CsePerson.Number;
              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.CaseUnitNumber =
                entities.CaseUnit.CuNumber;
              local.PassToDateWorkArea.Date =
                entities.ServiceProcess.ServiceRequestDate;
              UseCabDate2TextWithHyphens();
              local.PassToInfrastructure.Detail =
                "Service Request for court case " + TrimEnd
                (entities.LegalAction.CourtCaseNumber) + " to be processed on " +
                local.ReturnFromTextWorkArea.Text10;
              UseSpCabCreateInfrastructure();

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øIncrement counters and detect errors.		   	   ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.HoldForSafeKeeping.SystemGeneratedIdentifier =
                local.ProgramError.SystemGeneratedIdentifier;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // ø
              // 
              // ø
              // øFor non-critical errors you may write an error record to  ø
              // øthe program error entity type.                            ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.LcontrolTotNumbErrRecs.Count;
                UseEabExtractExitStateMessage();
                local.ProgramError.ProgramError1 =
                  local.ExitStateWorkArea.Message;

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øKEY INFO : This attrib contains info to identify the      ø
                // øtransaction record that caused the error. String contains:ø
                // øSERVICE PROCESS ID; LEGAL ACTION ID                       ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                local.ProgramError.KeyInfo = "SERVICE PROCESS ID " + NumberToString
                  (entities.ServiceProcess.Identifier, 15) + "; LEGAL ACTION ID " +
                  NumberToString(entities.LegalAction.Identifier, 15);
                local.ProgramError.SystemGeneratedIdentifier =
                  local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
                ExitState = "ACO_NN0000_ALL_OK";

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øThis AB creates an error record.                          ø
                // øInput is a Persistent Program Run as well as a local view ø
                // øof Program Run in case currency is lost. Local Program_   ø
                // øProcessing_Info is also needed to reread Program Run.     ø
                // øProgram Error contains information about the current errorø
                // øas follows:
                // 
                // ø
                // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
                // ø   Key Info   : String identifying info for record in err ø
                // ø   Program Err: Set this to the extracted exit state msg  ø
                // ø
                // 
                // ø
                // øWARNING: Set the above attribs only. At the time of       ø
                // øwriting this template, there were a few superflous attribsø
                // øthat need to be deleted (from PROGRAM ERR)                ø
                // ø
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                // *****************************************************************
                // * Write a line to the ERROR RPT
                // 
                // *
                // *  replaces USE create_program_error above.
                // *
                // ********************************************
                // Crook  28Jan99  ***
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // ********************************************
                // Crook  28Jan99  ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  NumberToString(local.ProgramError.SystemGeneratedIdentifier,
                  13, 3);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
                local.PassToDateWorkArea.Date =
                  local.CurrentProgramProcessingInfo.ProcessDate;
                UseCabDate2TextWithHyphens();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.BusinessObjectCd;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.EventId, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.ReasonCode;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",SPR.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.ServiceProcess.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",LGA.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.LegalAction.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",LGA.Court_Case=";
                local.TextWorkArea.Text10 =
                  entities.LegalAction.CourtCaseNumber ?? Spaces(10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.NeededToWrite.RptDetail = "    CSP.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CsePersonNumber, 10,
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CAS.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CaseNumber, 10,
                  Verify(local.PassToInfrastructure.CaseNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CaseNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSU.Num=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                    GetValueOrDefault(), 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.CaseRole.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Type=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + entities
                  .CaseRole.Type1;
                local.Work.RptDetail = " " + (
                  local.ProgramError.ProgramError1 ?? "");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .Work.RptDetail;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              else
              {
                ++local.LcontrolTotNumbInfraCreated.Count;

                // ******End of EXIT STATE <> ALL_OK
              }
            }

ReadEach2:
            ;
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ++local.LcontrolTotNumbErrRecs.Count;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event 6, Event Detail Reason Code SRVREQSUB was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        if (Equal(local.CurrentProgramProcessingInfo.ProcessDate,
          local.Ending.ProcessDate))
        {
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øUpdate 1. number of checkpoints and the last checkpoint   ø
          // ø       2. last checkpoint time                            ø
          // ø       3. Set restart indicator to YES                    ø
          // ø       4. Restart Information                             ø
          // øAlso, return the checkpoint frequency count  in case they ø
          // øhave been changed since the last read.                    ø
          // øCAB increments checkpoint counter.                        ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øRestart info for this entity type is 05 to indicate       ø
          // øcompletion of SERVICE PROCESS.                            ø
          // øææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          local.ProgramCheckpointRestart.RestartInfo = "05";
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****************************************************************
            // * Write a line to he ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered updating the Program Checkpoint Restart Information.";
              
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ±æææææææææææææææææææææææÉ
          // øExternal DB2 commit.   ø
          // þæææææææææææææææææææææææÊ
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****End of Control Total Processing
        }

        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.CurrentProgramProcessingInfo.ProcessDate, 1);
      }
    }

    // *****************************************************************
    // * Process Bankruptcy Expected Discharge Date                    *
    // ********************************************
    // Crook  28Jan99  ***
    if (!Lt("05", Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
      2)))
    {
      if (Lt(local.Initialize.DenormDate, local.Starting.ProcessDate) && Lt
        (local.Starting.ProcessDate, AddDays(local.Ending.ProcessDate, -1)))
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.Starting.ProcessDate, 1);
      }
      else
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          local.Ending.ProcessDate;
      }

      while(!Lt(local.Ending.ProcessDate,
        local.CurrentProgramProcessingInfo.ProcessDate))
      {
        // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
        // øThe following timestamp value is used to compare against  ø
        // øa timestamp value of several of the entity types to be
        // ø
        // 
        // øread.  It represents 00:00:01 of the processing date.
        // ø
        // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
        local.PassToDateWorkArea.Date =
          local.CurrentProgramProcessingInfo.ProcessDate;
        UseCabDate2TextWithHyphens();
        local.ProcessDate.IefTimestamp =
          Timestamp(local.ReturnFromTextWorkArea.Text10);
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 18;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.BusinessObjectCd = "BKR";
        local.PassToInfrastructure.ReasonCode = "DMBKRPDSCH";

        if (ReadEventEventDetail1())
        {
          foreach(var item in ReadBankruptcyCsePerson())
          {
            // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
            // øCHANGE NOTE:  RVW Mar 21 97.  Changed reference           ø
            // ø  From Bankruptcy Discharge Date to Expected Bankruptcy   ø
            // ø  Discharge Date.
            // 
            // ø
            // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
            ++local.LcontrolTotNumbRecsRead.Count;
            local.PassToInfrastructure.CsePersonNumber =
              entities.CsePerson.Number;
            local.PassToInfrastructure.DenormNumeric12 =
              entities.Bankruptcy.Identifier;
            local.PassToInfrastructure.SituationNumber = 0;

            foreach(var item1 in ReadCaseCaseUnit())
            {
              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.CaseUnitNumber =
                entities.CaseUnit.CuNumber;
              local.PassToDateWorkArea.Date =
                entities.Bankruptcy.ExpectedBkrpDischargeDate;
              UseCabDate2TextWithHyphens();
              local.PassToInfrastructure.Detail =
                "Bankruptcy expected to be discharged on " + local
                .ReturnFromTextWorkArea.Text10;
              UseSpCabCreateInfrastructure();

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øIncrement counters and detect errors.		   	   ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.HoldForSafeKeeping.SystemGeneratedIdentifier =
                local.ProgramError.SystemGeneratedIdentifier;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // ø
              // 
              // ø
              // øFor non-critical errors you may write an error record to  ø
              // øthe program error entity type.                            ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.LcontrolTotNumbErrRecs.Count;
                UseEabExtractExitStateMessage();
                local.ProgramError.ProgramError1 =
                  local.ExitStateWorkArea.Message;

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øKEY INFO : This attrib contains info to identify the      ø
                // øtransaction record that caused the error. String contains:ø
                // ø BANKRUPTCY IDENTIFIER; CSE PERSON NUMBER                 ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                local.ProgramError.KeyInfo = "BANKRUPTCY ID " + NumberToString
                  (entities.Bankruptcy.Identifier, 15) + "; CSE PERSON NUMBER " +
                  entities.CsePerson.Number;
                local.ProgramError.KeyInfo = "CSE Person Number " + local
                  .CsePersonsWorkSet.Number;
                local.ProgramError.SystemGeneratedIdentifier =
                  local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
                ExitState = "ACO_NN0000_ALL_OK";

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øThis AB creates an error record.                          ø
                // øInput is a Persistent Program Run as well as a local view ø
                // øof Program Run in case currency is lost. Local Program_   ø
                // øProcessing_Info is also needed to reread Program Run.     ø
                // øProgram Error contains information about the current errorø
                // øas follows:
                // 
                // ø
                // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
                // ø   Key Info   : String identifying info for record in err ø
                // ø   Program Err: Set this to the extracted exit state msg  ø
                // ø
                // 
                // ø
                // øWARNING: Set the above attribs only. At the time of       ø
                // øwriting this template, there were a few superflous attribsø
                // øthat need to be deleted (from PROGRAM ERR)                ø
                // ø
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                // *****************************************************************
                // * Write a line to the ERROR RPT
                // 
                // *
                // *  replaces USE create_program_error above.
                // *
                // ********************************************
                // Crook  28Jan99  ***
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // ********************************************
                // Crook  28Jan99  ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  NumberToString(local.ProgramError.SystemGeneratedIdentifier,
                  13, 3);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
                local.PassToDateWorkArea.Date =
                  local.CurrentProgramProcessingInfo.ProcessDate;
                UseCabDate2TextWithHyphens();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.BusinessObjectCd;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.EventId, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.ReasonCode;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",BKR.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.Bankruptcy.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSP.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CsePersonNumber, 10,
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CAS.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CaseNumber, 10,
                  Verify(local.PassToInfrastructure.CaseNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CaseNumber, "0"));
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.NeededToWrite.RptDetail = "    CSU.Num=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                    GetValueOrDefault(), 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.CaseRole.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Type=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + entities
                  .CaseRole.Type1;
                local.Work.RptDetail = " " + (
                  local.ProgramError.ProgramError1 ?? "");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .Work.RptDetail;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              else
              {
                ++local.LcontrolTotNumbInfraCreated.Count;

                // ******End of EXIT STATE <> ALL_OK
              }
            }
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ++local.LcontrolTotNumbErrRecs.Count;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event 18, Event Detail Reason Code DMBKRPDSCH was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        if (Equal(local.CurrentProgramProcessingInfo.ProcessDate,
          local.Ending.ProcessDate))
        {
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øUpdate 1. number of checkpoints and the last checkpoint   ø
          // ø       2. last checkpoint time                            ø
          // ø       3. Set restart indicator to YES                    ø
          // ø       4. Restart Information                             ø
          // øAlso, return the checkpoint frequency count  in case they ø
          // øhave been changed since the last read.                    ø
          // øCAB increments checkpoint counter.                        ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øRestart info for this entity type is 06 to indicate       ø
          // øcompletion of BANKRUPTCY.
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          local.ProgramCheckpointRestart.RestartInfo = "06";
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****************************************************************
            // * Write a line to he ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered updating the Program Checkpoint Restart Information.";
              
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ±æææææææææææææææææææææææÉ
          // øExternal DB2 commit.   ø
          // þæææææææææææææææææææææææÊ
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****End of Control Total Processing
        }

        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.CurrentProgramProcessingInfo.ProcessDate, 1);
      }
    }

    // *****************************************************************
    // * Process Case Role Date of Emancipation                        *
    // ********************************************
    // Crook  28Jan99  ***
    if (!Lt("06", Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
      2)))
    {
      if (Lt(local.Initialize.DenormDate, local.Starting.ProcessDate) && Lt
        (local.Starting.ProcessDate, AddDays(local.Ending.ProcessDate, -1)))
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.Starting.ProcessDate, 1);
      }
      else
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          local.Ending.ProcessDate;
      }

      while(!Lt(local.Ending.ProcessDate,
        local.CurrentProgramProcessingInfo.ProcessDate))
      {
        // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
        // øThe following timestamp value is used to compare against  ø
        // øa timestamp value of several of the entity types to be    ø
        // øread.  It represents 00:00:01 of the processing date.     ø
        // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
        local.PassToDateWorkArea.Date =
          local.CurrentProgramProcessingInfo.ProcessDate;
        UseCabDate2TextWithHyphens();
        local.ProcessDate.IefTimestamp =
          Timestamp(local.ReturnFromTextWorkArea.Text10);
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 18;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.ReasonCode = "DMEMANCIP";

        if (ReadEventEventDetail3())
        {
          foreach(var item in ReadCaseRoleCsePerson())
          {
            local.PassToDateWorkArea.Date =
              entities.CaseRole.DateOfEmancipation;
            UseCabDate2TextWithHyphens();
            ++local.LcontrolTotNumbRecsRead.Count;
            local.PassToInfrastructure.CsePersonNumber =
              entities.CsePerson.Number;
            local.PassToInfrastructure.SituationNumber = 0;
            local.PassToInfrastructure.Detail = "CSE Person " + entities
              .CsePerson.Number + " expected to emancipate on " + local
              .ReturnFromTextWorkArea.Text10 + ".";

            foreach(var item1 in ReadCase())
            {
              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.BusinessObjectCd = "CAS";

              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              if (ReadCaseUnit())
              {
                local.PassToInfrastructure.BusinessObjectCd = "CAU";
                local.PassToInfrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
              }
              else
              {
                local.PassToInfrastructure.CaseUnitNumber = 0;
              }

              UseSpCabCreateInfrastructure();

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øIncrement counters and detect errors.		   	   ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.HoldForSafeKeeping.SystemGeneratedIdentifier =
                local.ProgramError.SystemGeneratedIdentifier;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // ø
              // 
              // ø
              // øFor non-critical errors you may write an error record to  ø
              // øthe program error entity type.                            ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.LcontrolTotNumbErrRecs.Count;
                UseEabExtractExitStateMessage();
                local.ProgramError.ProgramError1 =
                  local.ExitStateWorkArea.Message;

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øKEY INFO : This attrib contains info to identify the      ø
                // øtransaction record that caused the error. String contains:ø
                // øCASE ROLE TYPE,ID; CASE NUMBER; CSE PERSON NUMBER         ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                local.ProgramError.KeyInfo = "CASE ROLE TYPE " + entities
                  .CaseRole.Type1 + "; CASE ROLE ID " + NumberToString
                  (entities.CaseRole.Identifier, 15) + "; CASE NUMBER " + entities
                  .Case1.Number + "; CSE PERSON NUMBER " + entities
                  .CsePerson.Number;
                local.ProgramError.SystemGeneratedIdentifier =
                  local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
                ExitState = "ACO_NN0000_ALL_OK";

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øThis AB creates an error record.                          ø
                // øInput is a Persistent Program Run as well as a local view ø
                // øof Program Run in case currency is lost. Local Program_   ø
                // øProcessing_Info is also needed to reread Program Run.     ø
                // øProgram Error contains information about the current errorø
                // øas follows:
                // 
                // ø
                // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
                // ø   Key Info   : String identifying info for record in err ø
                // ø   Program Err: Set this to the extracted exit state msg  ø
                // ø
                // 
                // ø
                // øWARNING: Set the above attribs only. At the time of       ø
                // øwriting this template, there were a few superflous attribsø
                // øthat need to be deleted (from PROGRAM ERR)                ø
                // ø
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                // *****************************************************************
                // * Write a line to the ERROR RPT
                // 
                // *
                // *  replaces USE create_program_error above.
                // *
                // ********************************************
                // Crook  28Jan99  ***
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // ********************************************
                // Crook  28Jan99  ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  NumberToString(local.ProgramError.SystemGeneratedIdentifier,
                  13, 3);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
                local.PassToDateWorkArea.Date =
                  local.CurrentProgramProcessingInfo.ProcessDate;
                UseCabDate2TextWithHyphens();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.BusinessObjectCd;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.EventId, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.ReasonCode;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.CaseRole.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Type=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + entities
                  .CaseRole.Type1;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSP.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CsePersonNumber, 10,
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CAS.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CaseNumber, 10,
                  Verify(local.PassToInfrastructure.CaseNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CaseNumber, "0"));
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.NeededToWrite.RptDetail = "    CSU.Num=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                    GetValueOrDefault(), 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.CaseRole.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Type=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + entities
                  .CaseRole.Type1;
                local.Work.RptDetail = " " + (
                  local.ProgramError.ProgramError1 ?? "");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .Work.RptDetail;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              else
              {
                ++local.LcontrolTotNumbInfraCreated.Count;

                // ******End of EXIT STATE <> ALL_OK
              }
            }
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ++local.LcontrolTotNumbErrRecs.Count;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event 18, Event Detail Reason Code DMEMANCIP was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        // +
        // -----------------------------------------------------------
        // +
        // +The below listed block added to generate DMEMANCIP45 alerts+
        // +this is new emancipation worker alert will be generated 45 +
        // +days in advance.  This is in additona to current worker    +
        // +Alert (being generated one week in advance).               +
        // +CQ66150 Ticket - Change Start
        // 
        // +
        // +
        // -----------------------------------------------------------
        // +
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 18;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.ReasonCode = "DMEMANCIP45";

        if (ReadEventEventDetail4())
        {
          foreach(var item in ReadCaseRoleCsePerson())
          {
            local.PassToDateWorkArea.Date =
              entities.CaseRole.DateOfEmancipation;
            UseCabDate2TextWithHyphens();
            ++local.LcontrolTotNumbRecsRead.Count;
            local.PassToInfrastructure.CsePersonNumber =
              entities.CsePerson.Number;
            local.PassToInfrastructure.SituationNumber = 0;
            local.PassToInfrastructure.Detail = "CSE Person " + entities
              .CsePerson.Number + " expected to emancipate on " + local
              .ReturnFromTextWorkArea.Text10 + ".";

            foreach(var item1 in ReadCase())
            {
              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.BusinessObjectCd = "CAS";

              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              if (ReadCaseUnit())
              {
                local.PassToInfrastructure.BusinessObjectCd = "CAU";
                local.PassToInfrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
              }
              else
              {
                local.PassToInfrastructure.CaseUnitNumber = 0;
              }

              UseSpCabCreateInfrastructure();

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øIncrement counters and detect errors.		   	   ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.HoldForSafeKeeping.SystemGeneratedIdentifier =
                local.ProgramError.SystemGeneratedIdentifier;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // ø
              // 
              // ø
              // øFor non-critical errors you may write an error record to  ø
              // øthe program error entity type.                            ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.LcontrolTotNumbErrRecs.Count;
                UseEabExtractExitStateMessage();
                local.ProgramError.ProgramError1 =
                  local.ExitStateWorkArea.Message;

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øKEY INFO : This attrib contains info to identify the      ø
                // øtransaction record that caused the error. String contains:ø
                // øCASE ROLE TYPE,ID; CASE NUMBER; CSE PERSON NUMBER         ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                local.ProgramError.KeyInfo = "CASE ROLE TYPE " + entities
                  .CaseRole.Type1 + "; CASE ROLE ID " + NumberToString
                  (entities.CaseRole.Identifier, 15) + "; CASE NUMBER " + entities
                  .Case1.Number + "; CSE PERSON NUMBER " + entities
                  .CsePerson.Number;
                local.ProgramError.SystemGeneratedIdentifier =
                  local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
                ExitState = "ACO_NN0000_ALL_OK";

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øThis AB creates an error record.                          ø
                // øInput is a Persistent Program Run as well as a local view ø
                // øof Program Run in case currency is lost. Local Program_   ø
                // øProcessing_Info is also needed to reread Program Run.     ø
                // øProgram Error contains information about the current errorø
                // øas follows:
                // 
                // ø
                // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
                // ø   Key Info   : String identifying info for record in err ø
                // ø   Program Err: Set this to the extracted exit state msg  ø
                // ø
                // 
                // ø
                // øWARNING: Set the above attribs only. At the time of       ø
                // øwriting this template, there were a few superflous attribsø
                // øthat need to be deleted (from PROGRAM ERR)                ø
                // ø
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                // *****************************************************************
                // * Write a line to the ERROR RPT
                // 
                // *
                // *  replaces USE create_program_error above.
                // *
                // ********************************************
                // Crook  28Jan99  ***
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // ********************************************
                // Crook  28Jan99  ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  NumberToString(local.ProgramError.SystemGeneratedIdentifier,
                  13, 3);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
                local.PassToDateWorkArea.Date =
                  local.CurrentProgramProcessingInfo.ProcessDate;
                UseCabDate2TextWithHyphens();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.BusinessObjectCd;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.EventId, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.ReasonCode;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.CaseRole.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Type=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + entities
                  .CaseRole.Type1;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSP.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CsePersonNumber, 10,
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CAS.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CaseNumber, 10,
                  Verify(local.PassToInfrastructure.CaseNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CaseNumber, "0"));
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.NeededToWrite.RptDetail = "    CSU.Num=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                    GetValueOrDefault(), 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.CaseRole.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CRO.Type=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + entities
                  .CaseRole.Type1;
                local.Work.RptDetail = " " + (
                  local.ProgramError.ProgramError1 ?? "");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .Work.RptDetail;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              else
              {
                ++local.LcontrolTotNumbInfraCreated.Count;

                // ******End of EXIT STATE <> ALL_OK
              }
            }
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ++local.LcontrolTotNumbErrRecs.Count;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event 18, Event Detail Reason Code DMEMANCIP was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        // +
        // -----------------------------------------------------------
        // +
        // +CQ66150 Ticket - Change End
        // 
        // +
        // +
        // -----------------------------------------------------------
        // +
        if (Equal(local.CurrentProgramProcessingInfo.ProcessDate,
          local.Ending.ProcessDate))
        {
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øUpdate 1. number of checkpoints and the last checkpoint   ø
          // ø       2. last checkpoint time                            ø
          // ø       3. Set restart indicator to YES                    ø
          // ø       4. Restart Information                             ø
          // øAlso, return the checkpoint frequency count  in case they ø
          // øhave been changed since the last read.                    ø
          // øCAB increments checkpoint counter.                        ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øRestart info for this entity type is 07 to indicate       ø
          // øcompletion of CASE ROLE.
          // 
          // ø
          // ø
          // 
          // ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          local.ProgramCheckpointRestart.RestartInfo = "07";
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****************************************************************
            // * Write a line to he ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered updating the Program Checkpoint Restart Information.";
              
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ±æææææææææææææææææææææææÉ
          // øExternal DB2 commit.   ø
          // þæææææææææææææææææææææææÊ
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****End of Control Total Processing
        }

        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.CurrentProgramProcessingInfo.ProcessDate, 1);
      }
    }

    // *****************************************************************
    // * Process Genetic Test Test Result Received Date                *
    // ********************************************
    // Crook  28Jan99  ***
    if (!Lt("07", Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
      2)))
    {
      if (Lt(local.Initialize.DenormDate, local.Starting.ProcessDate) && Lt
        (local.Starting.ProcessDate, AddDays(local.Ending.ProcessDate, -1)))
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.Starting.ProcessDate, 1);
      }
      else
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          local.Ending.ProcessDate;
      }

      while(!Lt(local.Ending.ProcessDate,
        local.CurrentProgramProcessingInfo.ProcessDate))
      {
        // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
        // øThe following timestamp value is used to compare against  ø
        // øa timestamp value of several of the entity types to be
        // ø
        // 
        // øread.  It represents 00:00:01 of the processing date.
        // ø
        // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
        local.PassToDateWorkArea.Date =
          local.CurrentProgramProcessingInfo.ProcessDate;
        UseCabDate2TextWithHyphens();
        local.ProcessDate.IefTimestamp =
          Timestamp(local.ReturnFromTextWorkArea.Text10);
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 18;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.BusinessObjectCd = "GNT";
        local.PassToInfrastructure.ReasonCode = "DMCONTSTGT";

        if (ReadEventEventDetail2())
        {
          local.GeneticTest.TestResultReceivedDate =
            AddDays(local.CurrentProgramProcessingInfo.ProcessDate, (-
            entities.EventDetail.DateMonitorDays).GetValueOrDefault());

          foreach(var item in ReadGeneticTest())
          {
            ++local.LcontrolTotNumbRecsRead.Count;
            local.PassToInfrastructure.SituationNumber = 0;

            // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
            // øRead case unit/case where mother is AR, father is AP, and ø
            // øchild is CH.
            // ø
            // 
            // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
            if (ReadCaseRoleCaseUnitCaseCsePersonCaseRoleCsePerson2())
            {
              // Continue
            }
            else
            {
              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øRead case unit/case where mother is AP, and child is CH.  ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (ReadCaseRoleCaseUnitCaseCsePersonCaseRoleCsePerson1())
              {
                if (ReadInterstateRequest())
                {
                  local.PassToInfrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.PassToInfrastructure.InitiatingStateCode = "KS";
                }

                local.PassToInfrastructure.DenormNumeric12 =
                  entities.GeneticTest.TestNumber;
                local.PassToInfrastructure.CsePersonNumber =
                  entities.ExistingMotherCsePerson.Number;
                local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
                local.PassToInfrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
                local.PassToInfrastructure.SituationNumber = 0;
                local.PassToDateWorkArea.Date =
                  entities.GeneticTest.TestResultReceivedDate;
                UseCabDate2TextWithHyphens();
                local.PassToInfrastructure.Detail = "GnTst " + NumberToString
                  (entities.GeneticTest.TestNumber, 15) + " CH" + entities
                  .ExistingChildCsePerson.Number + " MO" + entities
                  .ExistingMotherCsePerson.Number + " FA" + entities
                  .ExistingFatherCsePerson.Number + " rslts rcvd " + local
                  .ReturnFromTextWorkArea.Text10;
                UseSpCabCreateInfrastructure();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                  // øIncrement counter for new infrastructure created above.   
                  // ø
                  // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                  ++local.LcontrolTotNumbInfraCreated.Count;

                  // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                  // øRead case unit/case where father is AP, and child is CH.  
                  // ø
                  // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                  if (ReadCaseUnitCaseCaseRoleCsePersonCaseRoleCsePerson())
                  {
                    // Continue
                  }
                  else
                  {
                    ExitState = "CASE_ROLE_NF";
                  }
                }
              }
              else
              {
                ExitState = "CASE_ROLE_NF";
              }
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              local.PassToInfrastructure.DenormNumeric12 =
                entities.GeneticTest.TestNumber;
              local.PassToInfrastructure.CsePersonNumber =
                entities.ExistingFatherCsePerson.Number;
              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.CaseUnitNumber =
                entities.CaseUnit.CuNumber;
              local.PassToDateWorkArea.Date =
                entities.GeneticTest.TestResultReceivedDate;
              local.PassToInfrastructure.SituationNumber = 0;
              UseCabDate2TextWithHyphens();
              local.PassToInfrastructure.Detail = "GnTst " + NumberToString
                (entities.GeneticTest.TestNumber, 15) + " CH" + entities
                .ExistingChildCsePerson.Number + " MO" + entities
                .ExistingMotherCsePerson.Number + " FA" + entities
                .ExistingFatherCsePerson.Number + " rslts rcvd " + local
                .ReturnFromTextWorkArea.Text10;
              UseSpCabCreateInfrastructure();
            }

            // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
            // øIncrement counters and detect errors.		   	   ø
            // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
            local.HoldForSafeKeeping.SystemGeneratedIdentifier =
              local.ProgramError.SystemGeneratedIdentifier;

            // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
            // ø
            // 
            // ø
            // øFor non-critical errors you may write an error record to  ø
            // øthe program error entity type.                            ø
            // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ++local.LcontrolTotNumbErrRecs.Count;
              UseEabExtractExitStateMessage();
              local.ProgramError.ProgramError1 =
                local.ExitStateWorkArea.Message;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øKEY INFO : This attrib contains info to identify the      ø
              // øtransaction record that caused the error. String contains:ø
              // øGENETIC TEST TEST NUMBER
              // 
              // ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.ProgramError.KeyInfo = "GENETIC TEST TEST NUMBER " + NumberToString
                (entities.GeneticTest.TestNumber, 15);
              local.ProgramError.SystemGeneratedIdentifier =
                local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
              ExitState = "ACO_NN0000_ALL_OK";

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øThis AB creates an error record.                          ø
              // øInput is a Persistent Program Run as well as a local view ø
              // øof Program Run in case currency is lost. Local Program_   ø
              // øProcessing_Info is also needed to reread Program Run.     ø
              // øProgram Error contains information about the current errorø
              // øas follows:
              // 
              // ø
              // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
              // ø   Key Info   : String identifying info for record in err ø
              // ø   Program Err: Set this to the extracted exit state msg  ø
              // ø
              // 
              // ø
              // øWARNING: Set the above attribs only. At the time of       ø
              // øwriting this template, there were a few superflous attribsø
              // øthat need to be deleted (from PROGRAM ERR)                ø
              // ø
              // 
              // ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              // *****************************************************************
              // * Write a line to the ERROR RPT
              // 
              // *
              // *  replaces USE create_program_error above.
              // *
              // ********************************************
              // Crook  28Jan99  ***
              // *****************************************************************
              // * Write a line to the ERROR RPT.
              // 
              // *
              // ********************************************
              // Crook  28Jan99  ***
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13,
                3);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
              local.PassToDateWorkArea.Date =
                local.CurrentProgramProcessingInfo.ProcessDate;
              UseCabDate2TextWithHyphens();
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + local
                .ReturnFromTextWorkArea.Text10;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + local
                .PassToInfrastructure.BusinessObjectCd;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
              local.TextWorkArea.Text10 =
                NumberToString(local.PassToInfrastructure.EventId, 6, 10);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + local
                .PassToInfrastructure.ReasonCode;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",GTE.Tst_Num=";
              local.TextWorkArea.Text10 =
                NumberToString(entities.GeneticTest.TestNumber, 6, 10);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",GTE.Tst_Rslt_Rec=";
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + local
                .ReturnFromTextWorkArea.Text10;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",CAS.Num=";
              local.TextWorkArea.Text10 =
                local.PassToInfrastructure.CaseNumber ?? Spaces(10);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.NeededToWrite.RptDetail = "    CSU.Num=";
              local.TextWorkArea.Text10 =
                NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                  GetValueOrDefault(), 6, 10);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",CH_CRO.Id=";
              local.TextWorkArea.Text10 =
                NumberToString(entities.ExistingChildCaseRole.Identifier, 6, 10);
                
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",CH_CSP.Num=";
              local.TextWorkArea.Text10 =
                entities.ExistingChildCsePerson.Number;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",MO_CRO.Id=";
              local.TextWorkArea.Text10 =
                NumberToString(entities.ExistingMotherCaseRole.Identifier, 6, 10);
                
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",MO_CSP.Num=";
              local.TextWorkArea.Text10 =
                entities.ExistingMotherCsePerson.Number;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",FA_CRO.Id=";
              local.TextWorkArea.Text10 =
                NumberToString(entities.ExistingFatherCaseRole.Identifier, 6, 10);
                
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + ",FA_CSP.Num=";
              local.TextWorkArea.Text10 =
                entities.ExistingFatherCsePerson.Number;
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + Substring
                (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                Verify(local.TextWorkArea.Text10, "0"), 11 -
                Verify(local.TextWorkArea.Text10, "0"));
              local.Work.RptDetail = " " + (
                local.ProgramError.ProgramError1 ?? "");
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }
            else
            {
              ++local.LcontrolTotNumbInfraCreated.Count;

              // ******End of EXIT STATE <> ALL_OK
            }
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ++local.LcontrolTotNumbErrRecs.Count;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event 18, Event Detail Reason Code DMCONTSTGT was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        if (Equal(local.CurrentProgramProcessingInfo.ProcessDate,
          local.Ending.ProcessDate))
        {
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øUpdate 1. number of checkpoints and the last checkpoint   ø
          // ø       2. last checkpoint time                            ø
          // ø       3. Set restart indicator to YES                    ø
          // ø       4. Restart Information                             ø
          // øAlso, return the checkpoint frequency count  in case they ø
          // øhave been changed since the last read.                    ø
          // øCAB increments checkpoint counter.                        ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øRestart info for this entity type is 08 to indicate       ø
          // øcompletion of PERSON GENETIC TEST.                        ø
          // ø
          // 
          // ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          local.ProgramCheckpointRestart.RestartInfo = "08";
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****************************************************************
            // * Write a line to he ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered updating the Program Checkpoint Restart Information.";
              
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ±æææææææææææææææææææææææÉ
          // øExternal DB2 commit.   ø
          // þæææææææææææææææææææææææÊ
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****End of Control Total Processing
        }

        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.CurrentProgramProcessingInfo.ProcessDate, 1);
      }
    }

    // *****************************************************************
    // * Process Incarceration Parole Eligibility Date                 *
    // ********************************************
    // Crook  28Jan99  ***
    if (!Lt("08", Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
      2)))
    {
      if (Lt(local.Initialize.DenormDate, local.Starting.ProcessDate) && Lt
        (local.Starting.ProcessDate, AddDays(local.Ending.ProcessDate, -1)))
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.Starting.ProcessDate, 1);
      }
      else
      {
        local.CurrentProgramProcessingInfo.ProcessDate =
          local.Ending.ProcessDate;
      }

      while(!Lt(local.Ending.ProcessDate,
        local.CurrentProgramProcessingInfo.ProcessDate))
      {
        // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
        // øThe following timestamp value is used to compare against  ø
        // øa timestamp value of several of the entity types to be
        // ø
        // 
        // øread.  It represents 00:00:01 of the processing date.
        // ø
        // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
        local.PassToDateWorkArea.Date =
          local.CurrentProgramProcessingInfo.ProcessDate;
        UseCabDate2TextWithHyphens();
        local.ProcessDate.IefTimestamp =
          Timestamp(local.ReturnFromTextWorkArea.Text10);
        local.PassToInfrastructure.Assign(local.Initialize);
        local.PassToInfrastructure.EventId = 18;
        local.PassToInfrastructure.ProcessStatus = "Q";
        local.PassToInfrastructure.UserId = "SWEPB302";
        local.PassToInfrastructure.BusinessObjectCd = "INC";

        if (ReadEventEventDetail5())
        {
          foreach(var item in ReadIncarcerationCsePerson())
          {
            ++local.LcontrolTotNumbRecsRead.Count;

            if (AsChar(entities.Incarceration.Type1) == 'J')
            {
              local.PassToInfrastructure.ReasonCode = "DMPAROLJAIL";
            }
            else if (AsChar(entities.Incarceration.Type1) == 'P')
            {
              local.PassToInfrastructure.ReasonCode = "DMPAROLPRIS";
            }
            else
            {
              continue;
            }

            local.PassToInfrastructure.CsePersonNumber =
              entities.CsePerson.Number;
            local.PassToInfrastructure.DenormNumeric12 =
              entities.Incarceration.Identifier;
            local.PassToInfrastructure.SituationNumber = 0;

            foreach(var item1 in ReadCaseCaseUnit())
            {
              if (ReadInterstateRequest())
              {
                local.PassToInfrastructure.InitiatingStateCode = "OS";
              }
              else
              {
                local.PassToInfrastructure.InitiatingStateCode = "KS";
              }

              local.PassToInfrastructure.CaseNumber = entities.Case1.Number;
              local.PassToInfrastructure.CaseUnitNumber =
                entities.CaseUnit.CuNumber;
              local.PassToDateWorkArea.Date =
                entities.Incarceration.VerifiedDate;
              UseCabDate2TextWithHyphens();
              local.PassToInfrastructure.Detail = "Incarceration, type " + entities
                .Incarceration.Type1 + ", begun on " + local
                .ReturnFromTextWorkArea.Text10 + ", eligible to end on ";
              local.PassToDateWorkArea.Date =
                entities.Incarceration.ParoleEligibilityDate;
              UseCabDate2TextWithHyphens();
              local.PassToInfrastructure.Detail =
                TrimEnd(local.PassToInfrastructure.Detail) + " " + local
                .ReturnFromTextWorkArea.Text10;
              UseSpCabCreateInfrastructure();

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // øIncrement counters and detect errors.		   	   ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              local.HoldForSafeKeeping.SystemGeneratedIdentifier =
                local.ProgramError.SystemGeneratedIdentifier;

              // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
              // ø
              // 
              // ø
              // øFor non-critical errors you may write an error record to  ø
              // øthe program error entity type.                            ø
              // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.LcontrolTotNumbErrRecs.Count;
                UseEabExtractExitStateMessage();
                local.ProgramError.ProgramError1 =
                  local.ExitStateWorkArea.Message;

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øKEY INFO : This attrib contains info to identify the      ø
                // øtransaction record that caused the error. String contains:ø
                // ø PAROLE INCARCERATION IDENTIFIER; CSE PERSON NUMBER       ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                local.ProgramError.KeyInfo = "PAROLE INCARCERATION ID " + NumberToString
                  (entities.Incarceration.Identifier, 15) + "; CSE PERSON NUMBER " +
                  entities.CsePerson.Number;
                local.ProgramError.SystemGeneratedIdentifier =
                  local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
                ExitState = "ACO_NN0000_ALL_OK";

                // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
                // øThis AB creates an error record.                          ø
                // øInput is a Persistent Program Run as well as a local view ø
                // øof Program Run in case currency is lost. Local Program_   ø
                // øProcessing_Info is also needed to reread Program Run.     ø
                // øProgram Error contains information about the current errorø
                // øas follows:
                // 
                // ø
                // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
                // ø   Key Info   : String identifying info for record in err ø
                // ø   Program Err: Set this to the extracted exit state msg  ø
                // ø
                // 
                // ø
                // øWARNING: Set the above attribs only. At the time of       ø
                // øwriting this template, there were a few superflous attribsø
                // øthat need to be deleted (from PROGRAM ERR)                ø
                // ø
                // 
                // ø
                // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
                // *****************************************************************
                // * Write a line to the ERROR RPT
                // 
                // *
                // *  replaces USE create_program_error above.
                // *
                // ********************************************
                // Crook  28Jan99  ***
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // ********************************************
                // Crook  28Jan99  ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  NumberToString(local.ProgramError.SystemGeneratedIdentifier,
                  13, 3);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " Process_Date=";
                local.PassToDateWorkArea.Date =
                  local.CurrentProgramProcessingInfo.ProcessDate;
                UseCabDate2TextWithHyphens();
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + " INF.BO=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.BusinessObjectCd;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVE.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.EventId, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",EVD.Rsn_Cd=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .PassToInfrastructure.ReasonCode;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",INC.Id=";
                local.TextWorkArea.Text10 =
                  NumberToString(entities.Incarceration.Identifier, 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",INC.Vrfd=";
                local.TextWorkArea.Text10 = local.ReturnFromTextWorkArea.Text10;
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CSP.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CsePersonNumber, 10,
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CsePersonNumber, "0"));
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + ",CAS.Num=";
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.PassToInfrastructure.CaseNumber, 10,
                  Verify(local.PassToInfrastructure.CaseNumber, "0"), 11 -
                  Verify(local.PassToInfrastructure.CaseNumber, "0"));
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.NeededToWrite.RptDetail = "    CSU.Num=";
                local.TextWorkArea.Text10 =
                  NumberToString(local.PassToInfrastructure.CaseUnitNumber.
                    GetValueOrDefault(), 6, 10);
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + Substring
                  (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
                  Verify(local.TextWorkArea.Text10, "0"), 11 -
                  Verify(local.TextWorkArea.Text10, "0"));
                local.Work.RptDetail = " " + (
                  local.ProgramError.ProgramError1 ?? "");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + local
                  .Work.RptDetail;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              else
              {
                ++local.LcontrolTotNumbInfraCreated.Count;

                // ******End of EXIT STATE <> ALL_OK
              }
            }
          }
        }
        else
        {
          local.CurrentProgramProcessingInfo.ProcessDate =
            local.Ending.ProcessDate;
          ++local.LcontrolTotNumbErrRecs.Count;
          ExitState = "SP0000_EVENT_DETAIL_NF";
          UseEabExtractExitStateMessage();
          local.ProgramError.ProgramError1 = local.ExitStateWorkArea.Message;
          local.ProgramError.KeyInfo =
            "Event 18, Event Detail Reason Code DMPAROLJAIL was not found.";
          local.ProgramError.SystemGeneratedIdentifier =
            local.HoldForSafeKeeping.SystemGeneratedIdentifier + 1;
          ExitState = "ACO_NN0000_ALL_OK";

          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øThis AB creates an error record.                          ø
          // øInput is a Persistent Program Run as well as a local view ø
          // øof Program Run in case currency is lost. Local Program_   ø
          // øProcessing_Info is also needed to reread Program Run.     ø
          // øProgram Error contains information about the current errorø
          // øas follows:
          // 
          // ø
          // ø   Sys Gen Id : Increment by 1 (see above stmt)           ø
          // ø   Key Info   : String identifying info for record in err ø
          // ø   Program Err: Set this to the extracted exit state msg  ø
          // ø
          // 
          // ø
          // øWARNING: Set the above attribs only. At the time of       ø
          // øwriting this template, there were a few superflous attribsø
          // øthat need to be deleted (from PROGRAM ERR)                ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // *****************************************************************
          // * Write a line to the ERROR RPT
          // 
          // *
          // *  replaces USE create_program_error above.                     *
          // ********************************************
          // Crook  28Jan99  ***
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // ********************************************
          // Crook  28Jan99  ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            NumberToString(local.ProgramError.SystemGeneratedIdentifier, 13, 3);
            
          local.Work.RptDetail = " " + (local.ProgramError.KeyInfo ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          local.Work.RptDetail = " " + (local.ProgramError.ProgramError1 ?? "");
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + local.Work.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        if (Equal(local.CurrentProgramProcessingInfo.ProcessDate,
          local.Ending.ProcessDate))
        {
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øUpdate 1. number of checkpoints and the last checkpoint   ø
          // ø       2. last checkpoint time                            ø
          // ø       3. Set restart indicator to YES                    ø
          // ø       4. Restart Information                             ø
          // øAlso, return the checkpoint frequency count  in case they ø
          // øhave been changed since the last read.                    ø
          // øCAB increments checkpoint counter.                        ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
          // øRestart info for this entity type is 09 to indicate       ø
          // øcompletion of PAROLE INCARCERATION.                       ø
          // ø
          // 
          // ø
          // ø
          // 
          // ø
          // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
          local.ProgramCheckpointRestart.RestartInfo = "09";
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****************************************************************
            // * Write a line to he ERROR RPT.
            // 
            // *
            // ********************************************
            // Crook  27Jan99  ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered updating the Program Checkpoint Restart Information.";
              
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ±æææææææææææææææææææææææÉ
          // øExternal DB2 commit.   ø
          // þæææææææææææææææææææææææÊ
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****End of Control Total Processing
        }

        local.CurrentProgramProcessingInfo.ProcessDate =
          AddDays(local.CurrentProgramProcessingInfo.ProcessDate, 1);
      }
    }

    // CQ23999  Following code added to store last process date in PPI parameter
    // list for SWEPB302
    local.Ending.ParameterList =
      NumberToString(DateToInt(local.Ending.ProcessDate), 8, 8);
    UseUpdateProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "Write";
      local.NeededToWrite.RptDetail =
        "Error encountered updating Program Processing Info parameter list";
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // øSet restart indicator to NO because we successfully ended ø
    // øthis program
    // 
    // ø
    // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdatePgmCheckpointRestart1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****************************************************************
      // * Write a line to he ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered updating the Program Checkpoint Restart Information.";
        
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Write Starting Processing Date to CONTROL RPT. DDNAME=RPT98.  *
    // ********************************************
    // Crook  27Jan99  ***
    local.PassToDateWorkArea.Date = AddDays(local.Starting.ProcessDate, 1);
    UseCabDate2TextWithHyphens();
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "Starting Processing Date - " + local
      .ReturnFromTextWorkArea.Text10;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered writing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Write Ending Processing Date to CONTROL RPT. DDNAME=RPT98.    *
    // ********************************************
    // Crook  27Jan99  ***
    local.PassToDateWorkArea.Date = local.Ending.ProcessDate;
    UseCabDate2TextWithHyphens();
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "Ending Processing Date - - " + local
      .ReturnFromTextWorkArea.Text10;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered writing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Write Totals to the CONTROL RPT. DDNAME=RPT98.                *
    // ********************************************
    // Crook  27Jan99  ***
    // *****************************************************************
    // * Write Reads to the CONTROL RPT. DDNAME=RPT98.                 *
    // ********************************************
    // Crook  27Jan99  ***
    local.EabFileHandling.Action = "WRITE";

    if (local.LcontrolTotNumbRecsRead.Count > 0)
    {
      local.TextWorkArea.Text10 =
        NumberToString(local.LcontrolTotNumbRecsRead.Count, 6, 10);
      local.NeededToWrite.RptDetail =
        "Total Number of Infrastructure Date Monitor Reads - - " + Substring
        (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
        Verify(local.TextWorkArea.Text10, "0"), 11 -
        Verify(local.TextWorkArea.Text10, "0"));
    }
    else
    {
      local.NeededToWrite.RptDetail =
        "Total Number of Infrastructure Date Monitor Reads - - 0";
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered writing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Write Created to the CONTROL RPT. DDNAME=RPT98.               *
    // ********************************************
    // Crook  27Jan99  ***
    local.EabFileHandling.Action = "WRITE";

    if (local.LcontrolTotNumbInfraCreated.Count > 0)
    {
      local.TextWorkArea.Text10 =
        NumberToString(local.LcontrolTotNumbInfraCreated.Count, 6, 10);
      local.NeededToWrite.RptDetail =
        "Total Number of Infrastructure Date Monitor Creates - " + Substring
        (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
        Verify(local.TextWorkArea.Text10, "0"), 11 -
        Verify(local.TextWorkArea.Text10, "0"));
    }
    else
    {
      local.NeededToWrite.RptDetail =
        "Total Number of Infrastructure Date Monitor Creates - 0";
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered writing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Write Errors to the CONTROL RPT. DDNAME=RPT98.                *
    // ********************************************
    // Crook  27Jan99  ***
    local.EabFileHandling.Action = "WRITE";

    if (local.LcontrolTotNumbErrRecs.Count > 0)
    {
      local.TextWorkArea.Text10 =
        NumberToString(local.LcontrolTotNumbErrRecs.Count, 6, 10);
      local.NeededToWrite.RptDetail =
        "Total Number of Infrastructure Date Monitor Errors  - " + Substring
        (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
        Verify(local.TextWorkArea.Text10, "0"), 11 -
        Verify(local.TextWorkArea.Text10, "0"));
    }
    else
    {
      local.NeededToWrite.RptDetail =
        "Total Number of Infrastructure Date Monitor Errors  - 0";
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered writing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Close the CONTROL RPT. DDNAME=RPT98.                           *
    // ********************************************
    // Crook  27Jan99  ***
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // ********************************************
      // Crook  27Jan99  ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // ********************************************
    // Crook  27Jan99  ***
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

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.Detail = source.Detail;
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
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

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

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.PassToDateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.ReturnFromTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private void UseReadPgmProcessInfoMultiDay()
  {
    var useImport = new ReadPgmProcessInfoMultiDay.Import();
    var useExport = new ReadPgmProcessInfoMultiDay.Export();

    useImport.ProgramProcessingInfo.Name =
      local.CurrentProgramProcessingInfo.Name;

    Call(ReadPgmProcessInfoMultiDay.Execute, useImport, useExport);

    local.Ending.Assign(useExport.Ending);
    local.Starting.ProcessDate = useExport.Starting.ProcessDate;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.PassToInfrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.PassToInfrastructure);
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.Ending);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.Ending.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadBankruptcyCsePerson()
  {
    entities.Bankruptcy.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadBankruptcyCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dateMonitorDays",
          entities.EventDetail.DateMonitorDays.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Bankruptcy.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Bankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.Bankruptcy.BankruptcyFilingDate = db.GetDate(reader, 2);
        entities.Bankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 3);
        entities.Bankruptcy.BankruptcyConfirmationDate =
          db.GetNullableDate(reader, 4);
        entities.Bankruptcy.LastUpdatedTimestamp = db.GetDateTime(reader, 5);
        entities.Bankruptcy.DischargeDateModInd =
          db.GetNullableString(reader, 6);
        entities.Bankruptcy.ExpectedBkrpDischargeDate =
          db.GetNullableDate(reader, 7);
        entities.Bankruptcy.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCaseCaseRoleCaseUnitCsePersonLegalActionCaseRole()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CaseUnit.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRoleCaseUnitCsePersonLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "startDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CsePerson.Number = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 9);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 10);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 11);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 12);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 13);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 14);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 15);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 16);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CaseUnit.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnitCsePersonLegalActionCaseRole()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CaseUnit.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadCaseCaseUnitCsePersonLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "startDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.CsePerson.Number = db.GetString(reader, 7);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 7);
        entities.CaseRole.CspNumber = db.GetString(reader, 7);
        entities.CaseRole.CspNumber = db.GetString(reader, 7);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 8);
        entities.CaseRole.CasNumber = db.GetString(reader, 8);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 9);
        entities.CaseRole.Type1 = db.GetString(reader, 9);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 10);
        entities.CaseRole.Identifier = db.GetInt32(reader, 10);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 11);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 12);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 13);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 14);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 15);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CaseUnit.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCaseRoleCaseUnitCaseCsePersonCaseRoleCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;
    entities.ExistingChildCaseRole.Populated = false;
    entities.ExistingChildCsePerson.Populated = false;
    entities.ExistingMotherCaseRole.Populated = false;
    entities.ExistingMotherCsePerson.Populated = false;

    return Read("ReadCaseRoleCaseUnitCaseCsePersonCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "casMNumber", entities.GeneticTest.CasMNumber ?? "");
        db.SetNullableString(
          command, "cspMNumber", entities.GeneticTest.CspMNumber ?? "");
        db.SetNullableString(
          command, "croMType", entities.GeneticTest.CroMType ?? "");
        db.SetNullableInt32(
          command, "croMIdentifier",
          entities.GeneticTest.CroMIdentifier.GetValueOrDefault());
        db.SetNullableString(
          command, "casNumber", entities.GeneticTest.CasNumber ?? "");
        db.SetNullableString(
          command, "cspNumber", entities.GeneticTest.CspNumber ?? "");
        db.SetNullableString(
          command, "croType", entities.GeneticTest.CroType ?? "");
        db.SetNullableInt32(
          command, "croIdentifier",
          entities.GeneticTest.CroIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMotherCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingMotherCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingMotherCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingMotherCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingMotherCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 4);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 5);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.Case1.Number = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 8);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 9);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 10);
        entities.ExistingChildCaseRole.CasNumber = db.GetString(reader, 11);
        entities.ExistingChildCaseRole.CspNumber = db.GetString(reader, 12);
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 12);
        entities.ExistingChildCaseRole.Type1 = db.GetString(reader, 13);
        entities.ExistingChildCaseRole.Identifier = db.GetInt32(reader, 14);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        entities.ExistingChildCaseRole.Populated = true;
        entities.ExistingChildCsePerson.Populated = true;
        entities.ExistingMotherCaseRole.Populated = true;
        entities.ExistingMotherCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingMotherCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ExistingChildCaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCaseUnitCaseCsePersonCaseRoleCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;
    entities.ExistingChildCaseRole.Populated = false;
    entities.ExistingChildCsePerson.Populated = false;
    entities.ExistingFatherCaseRole.Populated = false;
    entities.ExistingFatherCsePerson.Populated = false;
    entities.ExistingMotherCaseRole.Populated = false;
    entities.ExistingMotherCsePerson.Populated = false;

    return Read("ReadCaseRoleCaseUnitCaseCsePersonCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "casMNumber", entities.GeneticTest.CasMNumber ?? "");
        db.SetNullableString(
          command, "cspMNumber", entities.GeneticTest.CspMNumber ?? "");
        db.SetNullableString(
          command, "croMType", entities.GeneticTest.CroMType ?? "");
        db.SetNullableInt32(
          command, "croMIdentifier",
          entities.GeneticTest.CroMIdentifier.GetValueOrDefault());
        db.SetNullableString(
          command, "casNumber", entities.GeneticTest.CasNumber ?? "");
        db.SetNullableString(
          command, "cspNumber", entities.GeneticTest.CspNumber ?? "");
        db.SetNullableString(
          command, "croType", entities.GeneticTest.CroType ?? "");
        db.SetNullableInt32(
          command, "croIdentifier",
          entities.GeneticTest.CroIdentifier.GetValueOrDefault());
        db.SetNullableString(
          command, "casANumber", entities.GeneticTest.CasANumber ?? "");
        db.SetNullableString(
          command, "cspANumber", entities.GeneticTest.CspANumber ?? "");
        db.SetNullableString(
          command, "croAType", entities.GeneticTest.CroAType ?? "");
        db.SetNullableInt32(
          command, "croAIdentifier",
          entities.GeneticTest.CroAIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMotherCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingMotherCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingMotherCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingMotherCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingMotherCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 4);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 5);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.Case1.Number = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 8);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 9);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 10);
        entities.ExistingChildCaseRole.CasNumber = db.GetString(reader, 11);
        entities.ExistingChildCaseRole.CspNumber = db.GetString(reader, 12);
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 12);
        entities.ExistingChildCaseRole.Type1 = db.GetString(reader, 13);
        entities.ExistingChildCaseRole.Identifier = db.GetInt32(reader, 14);
        entities.ExistingFatherCaseRole.CasNumber = db.GetString(reader, 15);
        entities.ExistingFatherCaseRole.CspNumber = db.GetString(reader, 16);
        entities.ExistingFatherCsePerson.Number = db.GetString(reader, 16);
        entities.ExistingFatherCaseRole.Type1 = db.GetString(reader, 17);
        entities.ExistingFatherCaseRole.Identifier = db.GetInt32(reader, 18);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        entities.ExistingChildCaseRole.Populated = true;
        entities.ExistingChildCsePerson.Populated = true;
        entities.ExistingFatherCaseRole.Populated = true;
        entities.ExistingFatherCsePerson.Populated = true;
        entities.ExistingMotherCaseRole.Populated = true;
        entities.ExistingMotherCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingMotherCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ExistingChildCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ExistingFatherCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "dateMonitorDays",
          entities.EventDetail.DateMonitorDays.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

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
        db.SetNullableString(command, "cspNoChild", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 1);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCaseUnitCaseCaseRoleCsePersonCaseRoleCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;
    entities.ExistingChildCaseRole.Populated = false;
    entities.ExistingChildCsePerson.Populated = false;
    entities.ExistingFatherCaseRole.Populated = false;
    entities.ExistingFatherCsePerson.Populated = false;

    return Read("ReadCaseUnitCaseCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "casNumber", entities.GeneticTest.CasNumber ?? "");
        db.SetNullableString(
          command, "cspNumber", entities.GeneticTest.CspNumber ?? "");
        db.SetNullableString(
          command, "croType", entities.GeneticTest.CroType ?? "");
        db.SetNullableInt32(
          command, "croIdentifier",
          entities.GeneticTest.CroIdentifier.GetValueOrDefault());
        db.SetNullableString(
          command, "casANumber", entities.GeneticTest.CasANumber ?? "");
        db.SetNullableString(
          command, "cspANumber", entities.GeneticTest.CspANumber ?? "");
        db.SetNullableString(
          command, "croAType", entities.GeneticTest.CroAType ?? "");
        db.SetNullableInt32(
          command, "croAIdentifier",
          entities.GeneticTest.CroAIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 1);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.ExistingChildCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ExistingChildCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 8);
        entities.ExistingChildCaseRole.Type1 = db.GetString(reader, 9);
        entities.ExistingChildCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ExistingFatherCaseRole.CasNumber = db.GetString(reader, 11);
        entities.ExistingFatherCaseRole.CspNumber = db.GetString(reader, 12);
        entities.ExistingFatherCsePerson.Number = db.GetString(reader, 12);
        entities.ExistingFatherCaseRole.Type1 = db.GetString(reader, 13);
        entities.ExistingFatherCaseRole.Identifier = db.GetInt32(reader, 14);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        entities.ExistingChildCaseRole.Populated = true;
        entities.ExistingChildCsePerson.Populated = true;
        entities.ExistingFatherCaseRole.Populated = true;
        entities.ExistingFatherCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChildCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ExistingFatherCaseRole.Type1);
      });
  }

  private bool ReadEventEventDetail1()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail1",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail2()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail2",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail3()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail3",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail4()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail4",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail5()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail5",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail6()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail6",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail7()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail7",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail8()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail8",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 5);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadGeneticTest()
  {
    entities.GeneticTest.Populated = false;

    return ReadEach("ReadGeneticTest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "resultRcvdDate",
          local.GeneticTest.TestResultReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.TestResultReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.GeneticTest.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 2);
        entities.GeneticTest.CasNumber = db.GetNullableString(reader, 3);
        entities.GeneticTest.CspNumber = db.GetNullableString(reader, 4);
        entities.GeneticTest.CroType = db.GetNullableString(reader, 5);
        entities.GeneticTest.CroIdentifier = db.GetNullableInt32(reader, 6);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 7);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 8);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 9);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 10);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 11);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 12);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 13);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 14);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.GeneticTest.CroType);
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);

        return true;
      });
  }

  private IEnumerable<bool> ReadHearingLegalAction()
  {
    entities.Hearing.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadHearingLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dateMonitorDays",
          entities.EventDetail.DateMonitorDays.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Identifier = db.GetInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.Type1 = db.GetNullableString(reader, 3);
        entities.Hearing.LastUpdatedTstamp = db.GetNullableDateTime(reader, 4);
        entities.Hearing.Outcome = db.GetNullableString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.Hearing.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncarcerationCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Incarceration.Populated = false;

    return ReadEach("ReadIncarcerationCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dateMonitorDays",
          entities.EventDetail.DateMonitorDays.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.ParoleEligibilityDate =
          db.GetNullableDate(reader, 3);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 4);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 5);
        entities.CsePerson.Populated = true;
        entities.Incarceration.Populated = true;

        return true;
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
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMilitaryServiceCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.MilitaryService.Populated = false;

    return ReadEach("ReadMilitaryServiceCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dateMonitorDays",
          entities.EventDetail.DateMonitorDays.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.MilitaryService.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.MilitaryService.ExpectedDischargeDate =
          db.GetNullableDate(reader, 2);
        entities.CsePerson.Populated = true;
        entities.MilitaryService.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProcessLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.ServiceProcess.Populated = false;

    return ReadEach("ReadServiceProcessLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dateMonitorDays",
          entities.EventDetail.DateMonitorDays.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate",
          local.CurrentProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(
          command, "serviceDate",
          local.NullServiceProcess.ServiceDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.ServiceRequestDate = db.GetDate(reader, 1);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 2);
        entities.ServiceProcess.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.Populated = true;
        entities.ServiceProcess.Populated = true;

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
    /// A value of AbortProgramIndicator.
    /// </summary>
    [JsonPropertyName("abortProgramIndicator")]
    public Common AbortProgramIndicator
    {
      get => abortProgramIndicator ??= new();
      set => abortProgramIndicator = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of CheckpointNumbOfReads.
    /// </summary>
    [JsonPropertyName("checkpointNumbOfReads")]
    public Common CheckpointNumbOfReads
    {
      get => checkpointNumbOfReads ??= new();
      set => checkpointNumbOfReads = value;
    }

    /// <summary>
    /// A value of CheckpointRestart.
    /// </summary>
    [JsonPropertyName("checkpointRestart")]
    public CsePersonAddress CheckpointRestart
    {
      get => checkpointRestart ??= new();
      set => checkpointRestart = value;
    }

    /// <summary>
    /// A value of LcontrolTotNumbErrRecs.
    /// </summary>
    [JsonPropertyName("lcontrolTotNumbErrRecs")]
    public Common LcontrolTotNumbErrRecs
    {
      get => lcontrolTotNumbErrRecs ??= new();
      set => lcontrolTotNumbErrRecs = value;
    }

    /// <summary>
    /// A value of LcontrolTotNumbInfraCreated.
    /// </summary>
    [JsonPropertyName("lcontrolTotNumbInfraCreated")]
    public Common LcontrolTotNumbInfraCreated
    {
      get => lcontrolTotNumbInfraCreated ??= new();
      set => lcontrolTotNumbInfraCreated = value;
    }

    /// <summary>
    /// A value of LcontrolTotNumbRecsRead.
    /// </summary>
    [JsonPropertyName("lcontrolTotNumbRecsRead")]
    public Common LcontrolTotNumbRecsRead
    {
      get => lcontrolTotNumbRecsRead ??= new();
      set => lcontrolTotNumbRecsRead = value;
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
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("currentProgramProcessingInfo")]
    public ProgramProcessingInfo CurrentProgramProcessingInfo
    {
      get => currentProgramProcessingInfo ??= new();
      set => currentProgramProcessingInfo = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public ProgramProcessingInfo Ending
    {
      get => ending ??= new();
      set => ending = value;
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
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    /// <summary>
    /// A value of HoldForSafeKeeping.
    /// </summary>
    [JsonPropertyName("holdForSafeKeeping")]
    public ProgramError HoldForSafeKeeping
    {
      get => holdForSafeKeeping ??= new();
      set => holdForSafeKeeping = value;
    }

    /// <summary>
    /// A value of IncarcerationType.
    /// </summary>
    [JsonPropertyName("incarcerationType")]
    public TextWorkArea IncarcerationType
    {
      get => incarcerationType ??= new();
      set => incarcerationType = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public Infrastructure Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of NullServiceProcess.
    /// </summary>
    [JsonPropertyName("nullServiceProcess")]
    public ServiceProcess NullServiceProcess
    {
      get => nullServiceProcess ??= new();
      set => nullServiceProcess = value;
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
    /// A value of PassToDateWorkArea.
    /// </summary>
    [JsonPropertyName("passToDateWorkArea")]
    public DateWorkArea PassToDateWorkArea
    {
      get => passToDateWorkArea ??= new();
      set => passToDateWorkArea = value;
    }

    /// <summary>
    /// A value of PassToInfrastructure.
    /// </summary>
    [JsonPropertyName("passToInfrastructure")]
    public Infrastructure PassToInfrastructure
    {
      get => passToInfrastructure ??= new();
      set => passToInfrastructure = value;
    }

    /// <summary>
    /// A value of PersonGeneticTest.
    /// </summary>
    [JsonPropertyName("personGeneticTest")]
    public PersonGeneticTest PersonGeneticTest
    {
      get => personGeneticTest ??= new();
      set => personGeneticTest = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public BatchTimestampWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of ProcessOption.
    /// </summary>
    [JsonPropertyName("processOption")]
    public Common ProcessOption
    {
      get => processOption ??= new();
      set => processOption = value;
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
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    /// <summary>
    /// A value of RecordsCreated.
    /// </summary>
    [JsonPropertyName("recordsCreated")]
    public Common RecordsCreated
    {
      get => recordsCreated ??= new();
      set => recordsCreated = value;
    }

    /// <summary>
    /// A value of ReturnFromControlTable.
    /// </summary>
    [JsonPropertyName("returnFromControlTable")]
    public ControlTable ReturnFromControlTable
    {
      get => returnFromControlTable ??= new();
      set => returnFromControlTable = value;
    }

    /// <summary>
    /// A value of ReturnFromInfrastructure.
    /// </summary>
    [JsonPropertyName("returnFromInfrastructure")]
    public Infrastructure ReturnFromInfrastructure
    {
      get => returnFromInfrastructure ??= new();
      set => returnFromInfrastructure = value;
    }

    /// <summary>
    /// A value of ReturnFromTextWorkArea.
    /// </summary>
    [JsonPropertyName("returnFromTextWorkArea")]
    public TextWorkArea ReturnFromTextWorkArea
    {
      get => returnFromTextWorkArea ??= new();
      set => returnFromTextWorkArea = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ProgramProcessingInfo Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public EabReportSend Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of OneCourtOrder.
    /// </summary>
    [JsonPropertyName("oneCourtOrder")]
    public LegalAction OneCourtOrder
    {
      get => oneCourtOrder ??= new();
      set => oneCourtOrder = value;
    }

    private Common abortProgramIndicator;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common checkpointNumbOfReads;
    private CsePersonAddress checkpointRestart;
    private Common lcontrolTotNumbErrRecs;
    private Common lcontrolTotNumbInfraCreated;
    private Common lcontrolTotNumbRecsRead;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea currentDateWorkArea;
    private ProgramProcessingInfo currentProgramProcessingInfo;
    private DateWorkArea dateWorkArea;
    private Document document;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo ending;
    private ExitStateWorkArea exitStateWorkArea;
    private GeneticTest geneticTest;
    private Hearing hearing;
    private ProgramError holdForSafeKeeping;
    private TextWorkArea incarcerationType;
    private Infrastructure initialize;
    private DateWorkArea initialized;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private DateWorkArea nullDateWorkArea;
    private ServiceProcess nullServiceProcess;
    private External passArea;
    private DateWorkArea passToDateWorkArea;
    private Infrastructure passToInfrastructure;
    private PersonGeneticTest personGeneticTest;
    private BatchTimestampWorkArea processDate;
    private Common processOption;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramError programError;
    private Common recordsCreated;
    private ControlTable returnFromControlTable;
    private Infrastructure returnFromInfrastructure;
    private TextWorkArea returnFromTextWorkArea;
    private ServiceProcess serviceProcess;
    private ProgramProcessingInfo starting;
    private TextWorkArea textWorkArea;
    private EabReportSend work;
    private LegalAction oneCourtOrder;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of ExistingChildCaseRole.
    /// </summary>
    [JsonPropertyName("existingChildCaseRole")]
    public CaseRole ExistingChildCaseRole
    {
      get => existingChildCaseRole ??= new();
      set => existingChildCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingChildCsePerson.
    /// </summary>
    [JsonPropertyName("existingChildCsePerson")]
    public CsePerson ExistingChildCsePerson
    {
      get => existingChildCsePerson ??= new();
      set => existingChildCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingFatherCaseRole.
    /// </summary>
    [JsonPropertyName("existingFatherCaseRole")]
    public CaseRole ExistingFatherCaseRole
    {
      get => existingFatherCaseRole ??= new();
      set => existingFatherCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingFatherCsePerson.
    /// </summary>
    [JsonPropertyName("existingFatherCsePerson")]
    public CsePerson ExistingFatherCsePerson
    {
      get => existingFatherCsePerson ??= new();
      set => existingFatherCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingMotherCaseRole.
    /// </summary>
    [JsonPropertyName("existingMotherCaseRole")]
    public CaseRole ExistingMotherCaseRole
    {
      get => existingMotherCaseRole ??= new();
      set => existingMotherCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingMotherCsePerson.
    /// </summary>
    [JsonPropertyName("existingMotherCsePerson")]
    public CsePerson ExistingMotherCsePerson
    {
      get => existingMotherCsePerson ??= new();
      set => existingMotherCsePerson = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    /// <summary>
    /// A value of PersonGeneticTest.
    /// </summary>
    [JsonPropertyName("personGeneticTest")]
    public PersonGeneticTest PersonGeneticTest
    {
      get => personGeneticTest ??= new();
      set => personGeneticTest = value;
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
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    private Bankruptcy bankruptcy;
    private Case1 case1;
    private CaseRole caseRole;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private Event1 event1;
    private EventDetail eventDetail;
    private CaseRole existingChildCaseRole;
    private CsePerson existingChildCsePerson;
    private CaseRole existingFatherCaseRole;
    private CsePerson existingFatherCsePerson;
    private CaseRole existingMotherCaseRole;
    private CsePerson existingMotherCsePerson;
    private GeneticTest geneticTest;
    private Hearing hearing;
    private Incarceration incarceration;
    private InterstateRequest interstateRequest;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson;
    private MilitaryService militaryService;
    private PersonGeneticTest personGeneticTest;
    private ProgramProcessingInfo programProcessingInfo;
    private ServiceProcess serviceProcess;
  }
#endregion
}

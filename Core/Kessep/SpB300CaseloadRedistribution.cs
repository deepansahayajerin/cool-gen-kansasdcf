// Program: SP_B300_CASELOAD_REDISTRIBUTION, ID: 372571436, model: 746.
// Short name: SWEP300B
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
/// A program: SP_B300_CASELOAD_REDISTRIBUTION.
/// </para>
/// <para>
/// This batch procedure is used to process the caseload redistribution.  There 
/// are two phase to this process.  First, statistics are run for the new
/// Caseload Assignments(this is for Caseload Assignments where the Assignment
/// Indicator = P).  Second, Caseloads are redistributed using the new Caseload
/// Assignments.  Then at the end of the procedure a report is produced to
/// display the statistics of either phase of the process.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB300CaseloadRedistribution: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B300_CASELOAD_REDISTRIBUTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB300CaseloadRedistribution(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB300CaseloadRedistribution.
  /// </summary>
  public SpB300CaseloadRedistribution(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	    Developer	PR#	Description
    // -------------------------------------------------------------------
    // 07/01/95  J. Kemp - MTW		Initial Development
    // 01/14/97  G. Lofton - MTW	Rework - replace task and plan task with case 
    // assignment, added reassignment logic for case unit function assignment
    // and monitored activity assignmnt.
    // 02-17-98  J. Rookard            enhance error reporting.  Note that prior
    // to this date and after the G.Lofton mod date various changes have been
    // made to main cab called by this program to support additional
    // functionality required by the client.
    // 2-23-98   J. Rookard            in support of performance tuning modify 
    // assoc case to coord cab to reduce calls to action block that determines
    // case level program.  modify stats action block to enhance read statement
    // for monthly obligor summary.
    // 2-24-98   J. Rookard            remove extraneous adds to the read count.
    // Only add to the count for a read on case.
    // 03-31-99    SWSRKEH         Incorported the use of CAB_Control_Report and
    //                             CAB_Error_Report
    // 06/03/99	PMcElderry	Added CSEnet functionality
    // 08/25/99  	M Ramirez	516		Pass in infrastructure
    //                            reference_date to 
    // sp_create_document_infrastruct for
    // 						CASETXFR document so that it will be
    // 						printed when then assignment is
    // 						effective.
    // 09/17/99    M Ramirez H00073450   Don't send the document if
    //                                   
    // the AR is not a client
    // 09/28/99    SWSRCHF   H00073420   Expanded code to allow the commit
    //                                   
    // point to be table driven.
    // 09/28/99    SWSRCHF               Disabled the generation of
    //                                   
    // INFRASTRUCTURE records
    //                                   
    // per SME and SME manager
    // 10/20/99    SWSRCHF   H00077181   Changed the code so that User's
    //                                   
    // not found on OFCD, who have there
    //                                   
    // OVERRIDE indicator equal "Y" are
    //                                   
    // not reassigned
    // 11/16/99    SWSRCHF   H00077567   Modified the code to always
    //                                   
    // create Document Infrastructure
    //                                   
    // records
    // 12/07/99    SWSRCHF   H00078633   Changed the code to stop the
    //                                   
    // generation of transfer letters
    //                                   
    // for arrears only cases
    // 12/20/99    SWSRCHF   H00083161   Changed the code to reassign
    //                                   
    // Case Unit and Monitored Activity
    //                                   
    // records for ARREARS only cases
    // 01/25/00    SWSRCHF   H00082899   Reassign ALERT's and DMON's
    // 08/01/00    SWSRCHF   H00097425   Check the discontinue date for MAX date
    // on read of
    //                                   
    // the NEW Office Service Provider
    // 01/04/01    SWSRCHF    000265     Expand Last Name from 6 to 11 
    // characters
    //                                   
    // for OFCA and OFCD
    // 01/08/02    SWSRPDP    134430     Changed to use the Same Function CAB as
    //                                   
    // CASL/CADS/CASE/B713/B707
    // 03/25/08    SWCCRXS    WR20258C   Reassign Interstate
    //                        /CQ102     Case when associated Kansas Case is 
    // reassigned.
    // 12/17/2008  SWCOAMX    CQ#8420    Add the batch processing OK exit state.
    // 11/15/2010  SWCCRXS(RAJ) CQ#23296  Modifed to display the ES message 
    // before calling rollback action block
    //                                    
    // to identify the problem location.
    // 11/17/2011  GVandy	CQ30161    1) Do not trigger CASETXFR letters.  The 
    // letters will now be
    // 				   triggered by new batch program SP_B703_CASETXFR_GENERATION.
    // 				   2) Always raise CASEXFR event.
    // 08/16/2013  GVandy	CQ38147    Replace assignments by county with 
    // assignments by tribunal.
    // 				   Also remove first initial from alpha assignments.
    // 11/04/2013  GVandy      CQ41845	   Change assignment processing order to 
    // priority,
    //                                    
    // program, tribunal, function, and
    // alpha.
    // 12/12/2013  GVandy      CQ42036	   Change to read OFCA rules using 
    // current date instead
    // 				   of processing date.
    // -------------------------------------------------------------------
    //  END of M A I N T E N A N C E   L O G
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramControlTotal.SystemGeneratedIdentifier = 0;

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // 12/12/2013 GVandy CQ42036  Change to read OFCA rules using current date 
    // instead of processing date.
    if (!IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 10)))
    {
      // -- Allow a parameter list overide of the date.  This is to facilitate 
      // testing.  It
      // should not be necessary to use this feature for production.  We don't 
      // want to use the
      // PPI Process Date for this because it was causing issues in production 
      // if the job
      // was run but the PPI process date was not set to the current date.
      local.Current.Date =
        StringToDate(Substring(local.ProgramProcessingInfo.ParameterList, 1, 10));
        
    }
    else
    {
      // -- Production should always use current date.
      local.Current.Date = Now().Date;
    }

    local.DisplayDate.Text10 =
      NumberToString(DateToInt(local.Current.Date), 10);
    local.DisplayDate.Text10 =
      Substring(local.DisplayDate.Text10, TextWorkArea.Text10_MaxLength, 7, 2) +
      "-" + Substring
      (local.DisplayDate.Text10, TextWorkArea.Text10_MaxLength, 9, 2) + "-" + Substring
      (local.DisplayDate.Text10, TextWorkArea.Text10_MaxLength, 3, 4);
    local.CurrentPlus1.Date = AddDays(local.Current.Date, 1);

    // *****************************************************************
    // * Setup of batch error handling
    // 
    // *
    // *****************************************************************
    // *****************************************************************
    // * Open the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.Current.Date;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Open the CONTROL RPT. DDNAME=RPT98.                           *
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.Current.Date;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * End of Batch error handling setup                             *
    // *****************************************************************
    // ***** Get the DB2 commit frequency counts.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered reading the Checkpoint Restart DB2 Table.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **** Retrieve restart office system generated id from restart table ****
    local.ChkpntRestartKey.SystemGeneratedId =
      (int)StringToNumber(Substring(
        local.ProgramCheckpointRestart.RestartInfo, 250, 1, 4));

    // **** OPEN REPORT DSN (DDNAME = RPTCSLD1) ****
    local.Send.Parm1 = "OF";
    local.Send.Parm2 = "";
    UseEabSpCaseloadDistributionRpt3();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the report output file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **** OPEN REPORT DSN (DDNAME = RPTCSLD1) ****
    UseEabCsldTest2();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the intermediate output file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **** END OF REPORT DSN OPEN PROCESS ****
    // ***** Process the selected records in groups based upon       ****
    // ***** the a logical unit of work.  Do a DB2 commit at the end ****
    // ***** of each group.
    // 
    // ****
    local.ChkpntNumbReads.Count = 0;
    local.ChkpntNumbUpdates.Count = 0;
    local.ChkpntNumbCreates.Count = 0;
    local.ChkpntNumbDeletes.Count = 0;
    local.Commit.Count = 0;

    // ************************************************************
    // Begin Main Process Loop
    // ************************************************************
    foreach(var item in ReadOffice())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      local.TotCaseErrors.Count = 0;

      // ************************************************************
      // Load all Office Caseload Assignments into the group view so
      // that stats can be accumulated.
      // ************************************************************
      // *** Initialise the group view (ARRAY)
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        local.Local1.Update.ServiceProvider.Assign(local.InitServiceProvider);
        MoveOfficeServiceProvider(local.InitOfficeServiceProvider,
          local.Local1.Update.OfficeServiceProvider);
        MoveOfficeCaseloadAssignment2(local.InitOfficeCaseloadAssignment,
          local.Local1.Update.OfficeCaseloadAssignment);
        local.Local1.Update.Program.Code = local.InitProgram.Code;
        local.Local1.Update.Tribunal.Identifier = local.InitTribunal.Identifier;
        MoveTextWorkArea(local.InitBegAlpha, local.Local1.Update.BegAlpha);
        MoveTextWorkArea(local.InitEndAlpha, local.Local1.Update.EndAlpha);
        local.Local1.Update.Ovrd.Count = local.InitOvrd.Count;
        local.Local1.Update.Case1.Count = local.InitCase.Count;
        local.Local1.Update.CaseCnt.Count = local.InitCaseCnt.Count;
      }

      local.Local1.CheckIndex();
      local.Local1.Index = -1;
      local.Local1.Count = 0;
      local.Use.AssignmentIndicator = "";

      // ************************************************************
      // Process only Office Caseload Assignments where the
      // Assignment Indicator  is 'P' (preview) or 'R'
      // (redistribute).  An Assignment Indicator of 'S' indicates
      // that the Case Assignments have been thru a Preview process
      // and are waiting on a Confirm action in the screen OFCA.
      // ************************************************************
      // @@@
      // -- 11/04/2013 GVandy CQ41845 Change assignment processing order to 
      // priority, program,
      //    tribunal, function, and alpha.  Original code is commented out 
      // below.
      foreach(var item1 in ReadOfficeCaseloadAssignmentOfficeServiceProvider())
      {
        if (local.Local1.Index == -1)
        {
          local.Use.AssignmentIndicator =
            entities.OfficeCaseloadAssignment.AssignmentIndicator;
        }

        if (AsChar(entities.OfficeCaseloadAssignment.AssignmentIndicator) != AsChar
          (local.Use.AssignmentIndicator))
        {
          // ************************************************************
          // Cannot have records with both assignment indicator values
          // ************************************************************
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *****************************************************************
          // *Write control numbers to the CONTROL RPT. DDNAME=RPT98.        *
          // *****************************************************************
          local.Counter.Count = 0;

          do
          {
            ++local.Counter.Count;

            switch(local.Counter.Count)
            {
              case 1:
                // *Write the error message to the CONTROL RPT. DDNAME=RPT98.
                // *
                local.NeededToWrite.RptDetail = "Office :" + NumberToString
                  (entities.Office.SystemGeneratedId, 15) + " has 'P' and 'R' Assignment Indicators, cannot process.";
                  

                break;
              case 2:
                local.NeededToWrite.RptDetail = "";

                break;
              default:
                continue;
            }

            UseCabControlReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****************************************************************
              // * Write a line to the ERROR RPT.
              // 
              // *
              // *****************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Error encountered writing to the Control Report.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
          while(local.Counter.Count != 2);

          goto ReadEach2;
        }

        if (local.Local1.Index + 1 == Local.LocalGroup.Capacity)
        {
          // *****************************************************************
          // *Write Message to the CONTROL RPT. DDNAME=RPT98.        *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.Counter.Count = 0;

          do
          {
            ++local.Counter.Count;

            switch(local.Counter.Count)
            {
              case 1:
                local.NeededToWrite.RptDetail =
                  "Maximum number of CaseLoad Redistribution Profiles reached for office: " +
                  NumberToString(entities.Office.SystemGeneratedId, 12, 4);

                break;
              case 2:
                // *Write Blank line to the CONTROL RPT. DDNAME=RPT98.        *
                local.NeededToWrite.RptDetail = "";

                break;
              default:
                continue;
            }

            UseCabControlReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****************************************************************
              // * Write a line to the ERROR RPT.
              // 
              // *
              // *****************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Error encountered writing to the Control Report.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
          while(local.Counter.Count != 2);

          goto ReadEach3;
        }

        ++local.Local1.Index;
        local.Local1.CheckSize();

        MoveOfficeCaseloadAssignment1(entities.OfficeCaseloadAssignment,
          local.Local1.Update.OfficeCaseloadAssignment);
        local.Local1.Update.ServiceProvider.Assign(entities.NewServiceProvider);
        MoveOfficeServiceProvider(entities.NewOfficeServiceProvider,
          local.Local1.Update.OfficeServiceProvider);

        if (ReadTribunal())
        {
          local.Local1.Update.Tribunal.Identifier =
            entities.Tribunal.Identifier;
        }
        else
        {
          local.Local1.Update.Tribunal.Identifier =
            local.BlankTribunal.Identifier;
        }

        if (ReadProgram())
        {
          local.Local1.Update.Program.Code = entities.Program.Code;
        }
        else
        {
          local.Local1.Update.Program.Code = local.BlankProgram.Code;
        }
      }

      // -- Sort assignment by Priority, Program, Tribunal, Function, and Alpha.
      //    A value in one of these sort criteria displays before blanks.
      local.I.Count = 1;

      for(var limit = local.Local1.Count; local.I.Count <= limit; ++
        local.I.Count)
      {
        local.Local1.Index = local.I.Count - 1;
        local.Local1.CheckSize();

        local.Compare.CompareOfficeCaseloadAssignment.Assign(
          local.Local1.Item.OfficeCaseloadAssignment);
        MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
          local.Compare.CompareOfficeServiceProvider);
        local.Compare.CompareProgram.Code = local.Local1.Item.Program.Code;
        local.Compare.CompareServiceProvider.Assign(
          local.Local1.Item.ServiceProvider);
        local.Compare.CompareTribunal.Identifier =
          local.Local1.Item.Tribunal.Identifier;
        local.J.Count = local.I.Count + 1;

        for(var limit1 = local.Local1.Count; local.J.Count <= limit1; ++
          local.J.Count)
        {
          local.Local1.Index = local.J.Count - 1;
          local.Local1.CheckSize();

          local.Swap1.Flag = "N";

          if (AsChar(local.Swap1.Flag) == 'N')
          {
            // -- Priority is the first sort criteria.
            if (local.Local1.Item.OfficeCaseloadAssignment.Priority < local
              .Compare.CompareOfficeCaseloadAssignment.Priority)
            {
              local.Swap1.Flag = "Y";

              goto Test1;
            }
            else if (local.Local1.Item.OfficeCaseloadAssignment.Priority > local
              .Compare.CompareOfficeCaseloadAssignment.Priority)
            {
              continue;
            }

            // -- Program is the second sort criteria.
            if (IsEmpty(local.Local1.Item.Program.Code))
            {
              if (!IsEmpty(local.Compare.CompareProgram.Code))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.Program.Code,
              local.Compare.CompareProgram.Code) || IsEmpty
              (local.Compare.CompareProgram.Code))
            {
              local.Swap1.Flag = "Y";

              goto Test1;
            }
            else if (Lt(local.Compare.CompareProgram.Code,
              local.Local1.Item.Program.Code))
            {
              continue;
            }

            // -- Tribunal is the third sort criteria.
            if (local.Local1.Item.Tribunal.Identifier == 0)
            {
              if (local.Compare.CompareTribunal.Identifier != 0)
              {
                continue;
              }
            }
            else if (local.Local1.Item.Tribunal.Identifier < local
              .Compare.CompareTribunal.Identifier || local
              .Compare.CompareTribunal.Identifier == 0)
            {
              local.Swap1.Flag = "Y";

              goto Test1;
            }
            else if (local.Local1.Item.Tribunal.Identifier > local
              .Compare.CompareTribunal.Identifier)
            {
              continue;
            }

            // -- Function is the fourth sort criteria.
            if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.Function))
            {
              if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
                Function))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.Function,
              local.Compare.CompareOfficeCaseloadAssignment.Function) || IsEmpty
              (local.Compare.CompareOfficeCaseloadAssignment.Function))
            {
              local.Swap1.Flag = "Y";

              goto Test1;
            }
            else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.Function,
              local.Local1.Item.OfficeCaseloadAssignment.Function))
            {
              continue;
            }

            // -- Beginning Alpha is the fifth sort criteria.
            if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha))
              
            {
              if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
                BeginingAlpha))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.
              BeginingAlpha,
              local.Compare.CompareOfficeCaseloadAssignment.BeginingAlpha) || IsEmpty
              (local.Compare.CompareOfficeCaseloadAssignment.BeginingAlpha))
            {
              local.Swap1.Flag = "Y";

              goto Test1;
            }
            else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.
              BeginingAlpha,
              local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha))
            {
              continue;
            }

            // -- Ending Alpha is the sixth sort criteria.
            if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha))
            {
              if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
                EndingAlpha))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha,
              local.Compare.CompareOfficeCaseloadAssignment.EndingAlpha) || IsEmpty
              (local.Compare.CompareOfficeCaseloadAssignment.EndingAlpha))
            {
              local.Swap1.Flag = "Y";
            }
            else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.
              EndingAlpha,
              local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha))
            {
              continue;
            }
          }

Test1:

          if (AsChar(local.Swap1.Flag) == 'N')
          {
            continue;
          }

          local.Swap.SwapOfficeCaseloadAssignment.Assign(
            local.Local1.Item.OfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
            local.Swap.SwapOfficeServiceProvider);
          local.Swap.SwapProgram.Code = local.Local1.Item.Program.Code;
          local.Swap.SwapServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          local.Swap.SwapTribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
          local.Local1.Update.OfficeCaseloadAssignment.Assign(
            local.Compare.CompareOfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Compare.CompareOfficeServiceProvider,
            local.Local1.Update.OfficeServiceProvider);
          local.Local1.Update.Program.Code = local.Compare.CompareProgram.Code;
          local.Local1.Update.ServiceProvider.Assign(
            local.Compare.CompareServiceProvider);
          local.Local1.Update.Tribunal.Identifier =
            local.Compare.CompareTribunal.Identifier;

          local.Local1.Index = local.I.Count - 1;
          local.Local1.CheckSize();

          local.Local1.Update.OfficeCaseloadAssignment.Assign(
            local.Swap.SwapOfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Swap.SwapOfficeServiceProvider,
            local.Local1.Update.OfficeServiceProvider);
          local.Local1.Update.Program.Code = local.Swap.SwapProgram.Code;
          local.Local1.Update.ServiceProvider.Assign(
            local.Swap.SwapServiceProvider);
          local.Local1.Update.Tribunal.Identifier =
            local.Swap.SwapTribunal.Identifier;
          local.Compare.CompareOfficeCaseloadAssignment.Assign(
            local.Local1.Item.OfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
            local.Compare.CompareOfficeServiceProvider);
          local.Compare.CompareProgram.Code = local.Local1.Item.Program.Code;
          local.Compare.CompareServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          local.Compare.CompareTribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
        }
      }

      // -- Set beginning and ending alpha group values for consistency with 
      // prior coding.
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        // *** Work request 00265
        // *** 01/04/01 swsrchf
        // *** start
        local.Local1.Update.BegAlpha.Text30 =
          local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha + "";
        local.Local1.Update.EndAlpha.Text30 =
          local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha + "";

        // *** end
        // *** 01/04/01 swsrchf
        // *** Work request 00265
      }

      local.Local1.CheckIndex();

      if (IsEmpty(local.Use.AssignmentIndicator))
      {
        // ************************************************************
        // No new Office Caseload Assignments were found for this
        // Office - Go get the NEXT office
        // ************************************************************
        continue;
      }

      local.RedistRequest.Flag = "Y";

      if (ReadOfficeAddress1())
      {
        local.OfficeAddress.Assign(entities.OfficeAddress);
      }
      else if (ReadOfficeAddress2())
      {
        local.OfficeAddress.Assign(entities.OfficeAddress);
      }
      else
      {
        local.OfficeAddress.Assign(local.BlankOfficeAddress);
      }

      // ************************************************************
      // Begin process of associating and reassigning Cases to the
      // new Office Caseload Assignments and Office Service
      // Providers within the current office.
      // ************************************************************
      local.TotOffcErrors.Count = 0;
      local.TotCasesInOffc.Count = 0;
      local.TotOvrdInOffc.Count = 0;
      local.TotReassignCnt.Count = 0;
      local.TotCaseNotReasgnd.Count = 0;
      local.TotInfrastrCnt.Count = 0;
      local.TotDocInfraCnt.Count = 0;

      // *** Problem report H00078633
      // *** 12/09/99 SWSRCHF
      local.TotArrearsOnly.Count = 0;
      local.TotMonActErrors.Count = 0;
      local.TotCuErrors.Count = 0;
      local.TotCaseErrors.Count = 0;

      // *** Problem Report H00077181
      // *** 10/20/99 SWSRCHF
      local.AddedToGroupView.Flag = "N";

      // *** Problem report H00097425
      // *** 08/01/00 SWSRCHF
      // *** changed check for the Case Assignment to
      // *** discontinue date equal to MAX date '2099-12-31'
      foreach(var item1 in ReadCaseCaseAssignmentServiceProviderOfficeServiceProvider())
        
      {
        if (Lt(local.Current.Date, entities.OldCaseAssignment.EffectiveDate))
        {
          if (Equal(entities.Case1.Number, local.PrevCase.Number))
          {
            continue;
          }

          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "Case Number:  " + entities
            .Case1.Number + " not included in re-assignment process for office: " +
            NumberToString(entities.Office.SystemGeneratedId, 12, 4);
          UseCabErrorReport2();
          ++local.TotCaseErrors.Count;

          continue;
        }

        MoveCase1(entities.Case1, local.CaseCase);
        ++local.TotCasesInOffc.Count;

        // **** Check for case assignments overrides ****
        if (AsChar(entities.OldCaseAssignment.OverrideInd) == 'Y')
        {
          ++local.TotOvrdInOffc.Count;

          // ****Increment SP's counts for report ****
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            if (local.Local1.Item.ServiceProvider.SystemGeneratedId == entities
              .OldServiceProvider.SystemGeneratedId)
            {
              local.Local1.Update.Case1.Count =
                local.Local1.Item.Case1.Count + 1;
              local.Local1.Update.Ovrd.Count = local.Local1.Item.Ovrd.Count + 1;

              goto ReadEach1;
            }
          }

          local.Local1.CheckIndex();

          // *** Problem report H00077181
          // *** 10/20/99 SWSRCHF
          // *** start
          foreach(var item2 in ReadServiceProviderOfficeServiceProvider())
          {
            local.Local1.Index = local.Local1.Count;
            local.Local1.CheckSize();

            if (local.Local1.Index + 1 >= Local.LocalGroup.Capacity)
            {
              local.EabFileHandling.Action = "WRITE";
              local.Counter.Count = 0;

              do
              {
                ++local.Counter.Count;

                switch(local.Counter.Count)
                {
                  case 1:
                    if (local.Local1.Index + 1 == Local.LocalGroup.Capacity)
                    {
                      local.NeededToWrite.RptDetail =
                        "Maximum number of CaseLoad Redistribution Profiles reached for office: " +
                        NumberToString
                        (entities.Office.SystemGeneratedId, 12, 4);
                    }
                    else
                    {
                      local.NeededToWrite.RptDetail =
                        "Maximum number of CaseLoad Redistribution Profiles surpassed for office: " +
                        NumberToString
                        (entities.Office.SystemGeneratedId, 12, 4);
                    }

                    break;
                  case 2:
                    // *Write Blank line to the CONTROL RPT. DDNAME=RPT98.
                    // *
                    local.NeededToWrite.RptDetail = "";

                    break;
                  default:
                    continue;
                }

                UseCabControlReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  // *****************************************************************
                  // * Write a line to the ERROR RPT.
                  // 
                  // *
                  // *****************************************************************
                  local.EabFileHandling.Action = "WRITE";
                  local.NeededToWrite.RptDetail =
                    "Error encountered writing to the Control Report.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              while(local.Counter.Count != 2);

              if (local.Local1.Index >= Local.LocalGroup.Capacity)
              {
                goto ReadEach1;
              }
            }

            local.Local1.Update.ServiceProvider.Assign(
              entities.OverriddenServiceProvider);
            MoveOfficeServiceProvider(entities.OverriddenOfficeServiceProvider,
              local.Local1.Update.OfficeServiceProvider);
            local.Local1.Update.OfficeCaseloadAssignment.Priority = 0;
            local.AddedToGroupView.Flag = "Y";
            local.Local1.Update.Case1.Count = local.Local1.Item.Case1.Count + 1;
            local.Local1.Update.Ovrd.Count = local.Local1.Item.Ovrd.Count + 1;

            goto ReadEach1;
          }

          // *** end
          // *** Problem report H00077181
          // *** 10/20/99 SWSRCHF
        }

        // **** Determine Case Program Type ****
        local.CaseProgram.Code = "";
        UseSiReadCaseProgramType();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          local.CaseProgram.Code = "";
          ExitState = "ACO_NN0000_ALL_OK";
        }

        // **** Determine Case Function ****
        // ************************************************************
        // Determine the major Case Function
        // ************************************************************
        local.CaseFuncWorkSet.FuncText3 = "";

        // 01/08/02    SWSRPDP    134430     Changed to use the Same Function 
        // CAB as
        //                                   
        // CASL/CADS/CASE/B713/B707
        UseSiCabReturnCaseFunction();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          local.CaseFuncWorkSet.FuncText3 = "";
          ExitState = "ACO_NN0000_ALL_OK";
        }

        // **** Determine Case Name & County ****
        UseSpCabDetrmnCaseNameTribunal();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          local.CaseTribunal2.Identifier = 0;
          local.CaseName.Text30 = "A          A";
          ExitState = "ACO_NN0000_ALL_OK";
        }

        // **** Do the Re-Assignment ****
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // **** If not an exact match then the CASE is assigned to Priority 5 
          // (the bit bucket) ****
          local.UseBitBucket.Flag = "";

          // **** Do the Re-Assignment ****
          // **** Program Codes are Priority 1 assignments ****
          if (!IsEmpty(local.CaseProgram.Code))
          {
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 1)
              {
                continue;
              }

              if (Equal(local.CaseProgram.Code, local.Local1.Item.Program.Code))
              {
                // *** Work request 00265
                // *** 01/04/01 swsrchf
                // *** changed text_8 to text_30
                if (local.Local1.Item.Tribunal.Identifier == local
                  .CaseTribunal2.Identifier && Equal
                  (local.Local1.Item.OfficeCaseloadAssignment.Function,
                  local.CaseFuncWorkSet.FuncText3) && !
                  Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30) &&
                  !
                  Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
                {
                  local.Local1.Update.Case1.Count =
                    local.Local1.Item.Case1.Count + 1;

                  goto Test2;

                  // *** Work request 00265
                  // *** 01/04/01 swsrchf
                  // *** changed text_8 to text_30
                }
                else if (local.Local1.Item.Tribunal.Identifier == 0 && Equal
                  (local.Local1.Item.OfficeCaseloadAssignment.Function,
                  local.CaseFuncWorkSet.FuncText3) && !
                  Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30) &&
                  !
                  Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
                {
                  local.Local1.Update.Case1.Count =
                    local.Local1.Item.Case1.Count + 1;

                  goto Test2;

                  // *** Work request 00265
                  // *** 01/04/01 swsrchf
                  // *** changed text_8 to text_30
                }
                else if (local.Local1.Item.Tribunal.Identifier == local
                  .CaseTribunal2.Identifier && IsEmpty
                  (local.Local1.Item.OfficeCaseloadAssignment.Function) && !
                  Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30) &&
                  !
                  Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
                {
                  local.Local1.Update.Case1.Count =
                    local.Local1.Item.Case1.Count + 1;

                  goto Test2;

                  // *** Work request 00265
                  // *** 01/04/01 swsrchf
                  // *** changed text_8 to text_30
                }
                else if (local.Local1.Item.Tribunal.Identifier == 0 && IsEmpty
                  (local.Local1.Item.OfficeCaseloadAssignment.Function) && !
                  Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30) &&
                  !
                  Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
                {
                  local.Local1.Update.Case1.Count =
                    local.Local1.Item.Case1.Count + 1;

                  goto Test2;
                }
              }
            }

            local.Local1.CheckIndex();
          }

          if (IsEmpty(local.UseBitBucket.Flag))
          {
            // **** Function Codes are Priority 2 assignments ****
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 2)
              {
                continue;
              }

              if (Equal(local.CaseFuncWorkSet.FuncText3,
                local.Local1.Item.OfficeCaseloadAssignment.Function))
              {
                // *** Work request 00265
                // *** 01/04/01 swsrchf
                // *** changed text_8 to text_30
                if (local.Local1.Item.Tribunal.Identifier == local
                  .CaseTribunal2.Identifier && !
                  Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30) &&
                  !
                  Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
                {
                  local.Local1.Update.Case1.Count =
                    local.Local1.Item.Case1.Count + 1;

                  goto Test2;

                  // *** Work request 00265
                  // *** 01/04/01 swsrchf
                  // *** changed text_8 to text_30
                }
                else if (local.Local1.Item.Tribunal.Identifier == 0 && !
                  Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30) &&
                  !
                  Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
                {
                  local.Local1.Update.Case1.Count =
                    local.Local1.Item.Case1.Count + 1;

                  goto Test2;
                }
              }
            }

            local.Local1.CheckIndex();
          }

          if (IsEmpty(local.UseBitBucket.Flag))
          {
            // **** County Codes are Priority 3 assignments ****
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 3)
              {
                continue;
              }

              if (local.Local1.Item.Tribunal.Identifier == local
                .CaseTribunal2.Identifier)
              {
                // *** Work request 00265
                // *** 01/04/01 swsrchf
                // *** changed text_8 to text_30
                if (!Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30)
                  && !
                  Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
                {
                  local.Local1.Update.Case1.Count =
                    local.Local1.Item.Case1.Count + 1;

                  goto Test2;
                }
              }
            }

            local.Local1.CheckIndex();
          }

          if (IsEmpty(local.UseBitBucket.Flag))
          {
            // **** Alpha Codes are Priority 4 assignments ****
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 4)
              {
                continue;
              }

              // *** Work request 00265
              // *** 01/04/01 swsrchf
              // *** changed text_8 to text_30
              if (!Lt(local.CaseName.Text30, local.Local1.Item.BegAlpha.Text30) &&
                !Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
              {
                local.Local1.Update.Case1.Count =
                  local.Local1.Item.Case1.Count + 1;

                goto Test2;
              }
            }

            local.Local1.CheckIndex();
          }

          // **** Alpha Codes are Priority 5 assignments ****
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 5)
            {
              continue;
            }

            local.Local1.Update.Case1.Count = local.Local1.Item.Case1.Count + 1;

            goto Test2;
          }

          local.Local1.CheckIndex();
          local.Local1.Update.Case1.Count = local.Local1.Item.Case1.Count + 1;

          // **** Issue error message - could not find a SP to assign case ****
          local.EabFileHandling.Action = "WRITE";

          // ****  Concat office, case, current SP and proposed new SP in error 
          // message ****
          local.NeededToWrite.RptDetail = "Could not reassign case number " + entities
            .Case1.Number + " from SP " + entities.OldServiceProvider.UserId + " in office:";
            

          // *** Problem report H00073420
          // *** 09/28/99 SWSRCHF
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " " + NumberToString
            (entities.Office.SystemGeneratedId, 12, 4);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " " + entities
            .NewServiceProvider.UserId + " in office " + NumberToString
            (entities.Office.SystemGeneratedId, 15);
          UseCabErrorReport2();
          ++local.TotCaseErrors.Count;
          ++local.TotOffcErrors.Count;

          continue;
        }

Test2:

        // **** Add check for 'P' of 'R' assgn Ind ****
        // **** If 'P' don't do the re-assignments ****
        // **** just provide the status report     ****
        if (AsChar(local.Use.AssignmentIndicator) == 'P')
        {
          continue;
        }

        // ****IF SP is the same - do not reassign ****
        if (entities.OldServiceProvider.SystemGeneratedId == local
          .Local1.Item.ServiceProvider.SystemGeneratedId)
        {
          ++local.TotCaseNotReasgnd.Count;

          // ****Do not end date the Assignment if SPs are the same ****
          continue;
        }

        // *** Problem report H00082899
        // *** 01/25/00 SWSRCHF
        // *** start
        MoveServiceProvider(local.Local1.Item.ServiceProvider,
          local.NewServiceProvider);
        MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
          local.NewOfficeServiceProvider);

        // *** end
        // *** 01/25/00 SWSRCHF
        // *** Problem report H00082899
        // *** Problem report H00097425
        // *** 08/01/00 SWSRCHF
        // *** added check for the Office Service Provider
        // *** discontinue date equal to MAX date '2099-12-31'
        ReadOfficeServiceProvider();

        // **** Re-assign Case ****
        UseSpCabReassignCaseBatch();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.ChkpntNumbUpdates.Count;
          ++local.ChkpntNumbCreates.Count;
          ++local.TotReassignCnt.Count;
        }
        else
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";

          // ****  Concat office, case, current SP and proposed new SP in error 
          // message ****
          local.NeededToWrite.RptDetail = "Could not reassign case number " + entities
            .Case1.Number + " from SP " + entities.OldServiceProvider.UserId + " to ";
            
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + entities
            .NewServiceProvider.UserId + " in office " + NumberToString
            (entities.Office.SystemGeneratedId, 15);
          UseCabErrorReport2();

          // *** CQ#23296 Changes Begin Here ***
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = "ES: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // *** CQ#23296 Changes End   Here ***
          // *** CQ#8420 Changes Begin Here ***
          UseCabErrorReport2();

          // *** CQ#8420 Changes End   Here ***
          ++local.TotCaseErrors.Count;
          ++local.TotOffcErrors.Count;
          UseEabRollbackSql();

          continue;
        }

        // 03/25/2008    SWCCRXS    CQ102 Part-C Changes Starts Here.
        UseSpReassignIntCaseBatch();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.ChkpntNumbUpdates.Count;
          ++local.ChkpntNumbCreates.Count;
        }
        else
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";

          // ****  Concat office, case, current SP and proposed new SP in error 
          // message ****
          local.NeededToWrite.RptDetail =
            "Could not reassign interstate case number " + entities
            .Case1.Number + " from SP " + entities.OldServiceProvider.UserId + " to ";
            
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + entities
            .NewServiceProvider.UserId + " in office " + NumberToString
            (entities.Office.SystemGeneratedId, 15);
          UseCabErrorReport2();

          // *** CQ#23296 Changes Begin Here ***
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = "ES: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // *** CQ#23296 Changes End   Here ***
          // *** CQ#8420 Changes Begin Here ***
          ExitState = "ACO_NN0000_ALL_OK";

          // *** CQ#8420 Changes End   Here ***
        }

        // 03/25/2008    SWCCRXS    CQ102 Part-C Changes Ends Here.
        // --------------------
        // CSEnet functionality
        // --------------------
        if (ReadInterstateRequest2())
        {
          local.ScreenIdentification.Command = "ASIN";
          UseSiCreateAutoCsenetTrans();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";

            // ****  Concat office, case, current SP and proposed new SP in 
            // error message
            local.NeededToWrite.RptDetail =
              "CSENET ERROR:  Could not create the CSEnet transaction for Case number " +
              entities.Case1.Number + " from SP " + entities
              .OldServiceProvider.UserId + " to ";
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + entities
              .NewServiceProvider.UserId + " in office " + NumberToString
              (entities.Office.SystemGeneratedId, 15);
            UseCabErrorReport2();

            // *** CQ#23296 Changes Begin Here ***
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.NeededToWrite.RptDetail = "ES: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            // *** CQ#23296 Changes End   Here ***
            // *** CQ#8420 Changes Begin Here ***
            UseCabErrorReport2();

            // *** CQ#8420 Changes End   Here ***
            UseEabRollbackSql();
            ++local.TotOffcErrors.Count;
            ++local.TotCuErrors.Count;

            continue;
          }
        }
        else
        {
          // ----------------------
          // No processing required
          // ----------------------
        }

        // *** Problem report H00078633
        // *** 12/07/99 SWSRCHF
        // *** start
        if (IsEmpty(local.CaseProgram.Code))
        {
          ++local.TotArrearsOnly.Count;

          // *** Problem report H00083161
          // *** 12/20/99 SWSRCHF
        }

        // *** end
        // *** 12/07/99 SWSRCHF
        // *** Problem report H00078633
        // 11/17/2011 GVandy  CQ30161 Raise the CASEXFR event for all cases.
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.EventId = 5;
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.UserId = global.UserId;
          local.Infrastructure.ReasonCode = "CASEXFR";
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.ReferenceDate = local.Current.Date;
          local.Infrastructure.Detail = "Case has been transfered from :";
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
            .OldServiceProvider.UserId + " to " + local
            .Local1.Item.ServiceProvider.UserId;

          if (ReadInterstateRequest1())
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }

          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.TotInfrastrCnt.Count;
            ++local.ChkpntNumbCreates.Count;
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";

            // ****  Concat office, case, current SP and proposed new SP in 
            // error message ****
            local.NeededToWrite.RptDetail =
              "Could not create the infrastructure record for the reassignment of " +
              entities.Case1.Number + " from SP " + entities
              .OldServiceProvider.UserId + " to ";
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + entities
              .NewServiceProvider.UserId + " in office " + NumberToString
              (entities.Office.SystemGeneratedId, 15);
            UseCabErrorReport2();

            // *** CQ#23296 Changes Begin Here ***
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.NeededToWrite.RptDetail = "ES: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            // *** CQ#23296 Changes End   Here ***
            // *** CQ#8420 Changes Begin Here ***
            UseCabErrorReport2();

            // *** CQ#8420 Changes End   Here ***
            ++local.TotOffcErrors.Count;
            UseEabRollbackSql();

            continue;
          }
        }

        // 11/17/2011 GVandy  CQ30161 Do not trigger CASETXFR letters.  The 
        // letters will now be
        // triggered by new batch program SP_B703_CASETXFR_GENERATION.
        // **** Re-assign Case Unit ****
        UseSpCabReassignCaseUnitBatch();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";

          // ****  Concat office, case, current SP and proposed new SP in error 
          // message ****
          local.NeededToWrite.RptDetail =
            "Could not reassignment the case units for Case number " + entities
            .Case1.Number + " from SP " + entities.OldServiceProvider.UserId + " to ";
            
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + entities
            .NewServiceProvider.UserId + " in office " + NumberToString
            (entities.Office.SystemGeneratedId, 15);
          UseCabErrorReport2();

          // *** CQ#23296 Changes Begin Here ***
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = "ES: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // *** CQ#23296 Changes End   Here ***
          // *** CQ#8420 Changes Begin Here ***
          UseCabErrorReport2();

          // *** CQ#8420 Changes End   Here ***
          UseEabRollbackSql();
          ++local.TotOffcErrors.Count;
          ++local.TotCuErrors.Count;

          continue;
        }

        // **** Re-assign Monitored Activity Assignment ****
        UseSpCabReassignMonActsBatch();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";

          // ****  Concat office, case, current SP and proposed new SP in error 
          // message ****
          local.NeededToWrite.RptDetail =
            "Could not reassignment the Monitored Activities for Case number " +
            entities.Case1.Number + " from SP " + entities
            .OldServiceProvider.UserId + " to ";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + entities
            .NewServiceProvider.UserId + " in office " + NumberToString
            (entities.Office.SystemGeneratedId, 15);
          UseCabErrorReport2();

          // *** CQ#23296 Changes Begin Here ***
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = "ES: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // *** CQ#23296 Changes End   Here ***
          // *** CQ#8420 Changes Begin Here ***
          UseCabErrorReport2();

          // *** CQ#8420 Changes End   Here ***
          UseEabRollbackSql();
          ++local.TotOffcErrors.Count;
          ++local.TotMonActErrors.Count;

          continue;
        }

        // *** Problem report H00082899
        // *** 01/25/00 SWSRCHF
        // *** start
        // *** Re-assign ALERT's and DMON's
        UseSpCabB300ReassignBatch();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";

          // ****  Concat office, case, current SP and proposed new SP in error 
          // message ****
          local.NeededToWrite.RptDetail =
            "Could not reassignment the Office Service Provider Alert or Monitored Document for Case number " +
            entities.Case1.Number + " from SP " + entities
            .OldServiceProvider.UserId + " to ";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + entities
            .NewServiceProvider.UserId + " in office " + NumberToString
            (entities.Office.SystemGeneratedId, 15);
          UseCabErrorReport2();

          // *** CQ#23296 Changes Begin Here ***
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = "ES: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // *** CQ#23296 Changes End   Here ***
          // *** CQ#8420 Changes Begin Here ***
          UseCabErrorReport2();

          // *** CQ#8420 Changes End   Here ***
          UseEabRollbackSql();
          ++local.TotOffcErrors.Count;
          ++local.TotAlertErrors.Count;

          continue;
        }

        // *** end
        // *** 01/25/00 SWSRCHF
        // *** Problem report H00082899
        // *** Problem report H00073420
        // *** 09/28/99 SWSRCHF
        // *** start
        ++local.Commit.Count;

        if (local.Commit.Count >= local
          .ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault())
        {
          // ***** Call an external that does a DB2 commit using a Cobol 
          // program.
          UseExtToDoACommit();
          local.Commit.Count = 0;
        }

        // *** end
        // *** 09/28/99 SWSRCHF
        // *** Problem report H00073420
        // **** Write REPORT DSN (DDNAME = EABCSLT) ****
        local.Send.Parm1 = "GR";
        local.Send.Parm2 = "";
        local.Send.SubreportCode = "MAIN";
        ReadServiceProvider();

        if (local.CaseTribunal2.Identifier > 0)
        {
          local.CaseTribunal1.Text4 =
            NumberToString(local.CaseTribunal2.Identifier, 12, 4);
        }
        else
        {
          local.CaseTribunal1.Text4 = "";
        }

        UseEabCsldTest1();

        if (!IsEmpty(local.Return1.Parm1))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered opening the intermediate output file.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

ReadEach1:
        ;
      }

      // **** Write the stats out ****
      // **** Re-init the array and counters ****
      // Reinitialize the local group and the local stat override case count.
      local.Send.Parm1 = "GR";
      local.Send.Parm2 = "";
      local.Send.SubreportCode = "MAIN";

      if (AsChar(local.Use.AssignmentIndicator) == 'P')
      {
        local.RptHeading.Text30 = "-    PREVIEWS    -";
      }
      else
      {
        local.RptHeading.Text30 = "- REDISTRIBUTION -";
      }

      if (!IsEmpty(local.OfficeAddress.Zip) && !
        IsEmpty(local.OfficeAddress.Zip4))
      {
        local.Zip10.Text10 = (local.OfficeAddress.Zip ?? "") + "-" + (
          local.OfficeAddress.Zip4 ?? "");
      }
      else
      {
        local.Zip10.Text10 = local.OfficeAddress.Zip ?? Spaces(10);
      }

      local.RptCityStZip.Text30 = TrimEnd(local.OfficeAddress.City) + ", " + local
        .OfficeAddress.StateProvince + " " + local.Zip10.Text10;
      local.StatOverrideCaseCnt.Count = 0;

      // -- 11/04/2013 GVandy CQ41845 Change assignment processing order to 
      // priority, program,
      // -- tribunal, function, and alpha.
      // --
      // --   Need to always sort the group due to the new processing sort 
      // order.  This will
      // --   re-order the group so the stats are written in the correct order 
      // for the
      // --   Natural program which creates the report.
      UseSpCabSortArray();
      local.Local1.Index = 0;

      for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        // *** Problem report H00077181
        // *** 10/20/99 SWSRCHF
        // *** start
        // *** end
        // *** 10/20/99 SWSRCHF
        // *** Problem report H00077181
        local.RptSpFormatted.FormattedName =
          TrimEnd(local.Local1.Item.ServiceProvider.LastName) + "," + local
          .Local1.Item.ServiceProvider.FirstName;
        local.RptTribunal2.Identifier = local.Local1.Item.Tribunal.Identifier;
        MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
          local.RptOfficeServiceProvider);
        local.RptProgram.Code = local.Local1.Item.Program.Code;
        local.RptSpCaseNbr.Count = local.Local1.Item.Case1.Count;
        local.RptSpOvrd.Count = local.Local1.Item.Ovrd.Count;
        local.RptServiceProvider.SystemGeneratedId =
          local.Local1.Item.ServiceProvider.SystemGeneratedId;
        local.RptOfficeCaseloadAssignment.Assign(
          local.Local1.Item.OfficeCaseloadAssignment);

        if (local.RptTribunal2.Identifier > 0)
        {
          local.RptTribunal1.Text4 =
            NumberToString(local.RptTribunal2.Identifier, 12, 4);
        }
        else
        {
          local.RptTribunal1.Text4 = "";
        }

        UseEabSpCaseloadDistributionRpt1();

        if (!IsEmpty(local.Return1.Parm1))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing to the report output file.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.Local1.Update.Tribunal.Identifier = 0;
        local.Local1.Update.Program.Code = "";
        local.Local1.Update.OfficeServiceProvider.EffectiveDate =
          local.Initialized.Date;
        local.Local1.Update.OfficeServiceProvider.RoleCode = "";
        local.Local1.Update.ServiceProvider.FirstName = "";
        local.Local1.Update.ServiceProvider.LastName = "";
        local.Local1.Update.ServiceProvider.SystemGeneratedId = 0;
        local.Local1.Update.ServiceProvider.UserId = "";
        local.Local1.Update.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          0;
        local.Local1.Update.OfficeCaseloadAssignment.BeginingAlpha = "";
        local.Local1.Update.OfficeCaseloadAssignment.EndingAlpha = "";
        local.Local1.Update.OfficeCaseloadAssignment.Function = "";
        local.Local1.Update.OfficeCaseloadAssignment.Priority = 0;
        local.Local1.Update.Case1.Count = 0;
        local.Local1.Update.Ovrd.Count = 0;
      }

      local.Local1.CheckIndex();

      // ****Change the Assignment indicators ****
      if (AsChar(local.Use.AssignmentIndicator) == 'P')
      {
        // ************************************************************
        // This is a Preview of the Case Assignments. Obtain Statistics
        // but do not Redistribute. Get next Case and Case Assignment.
        // ************************************************************
        local.Reset.AssignmentIndicator = "S";
      }
      else
      {
        local.Reset.AssignmentIndicator = "A";

        foreach(var item1 in ReadOfficeCaseloadAssignment2())
        {
          DeleteOfficeCaseloadAssignment();
          ++local.ChkpntNumbDeletes.Count;
        }
      }

      // ************************************************************
      // Update current Caseload Assignments to new Assignment
      // Indicator.
      // ************************************************************
      foreach(var item1 in ReadOfficeCaseloadAssignment1())
      {
        try
        {
          UpdateOfficeCaseloadAssignment();
          ++local.ChkpntNumbUpdates.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_OFFC_CASELOAD_ASSGN_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_OFFC_CASELOAD_ASSGN_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (IsExitState("SP0000_OFFC_CASELOAD_ASSGN_NU") || IsExitState
          ("SP0000_OFFC_CASELOAD_ASSGN_PV"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered updating the Office Caseload assignment for office: " +
            NumberToString(entities.Office.SystemGeneratedId, 15);
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // ****Check for a commit point ****
      if (local.ChkpntNumbCreates.Count > 0 || local.ChkpntNumbUpdates.Count > 0
        )
      {
        // **** Write checkpoint info to file ****
        // ************************************************************
        // Set restart indicator to no because we have successfully
        // finished processing
        // ************************************************************
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(entities.Office.SystemGeneratedId, 12, 4);
        UseUpdatePgmCheckpointRestart();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing to the Checkpoint Restart DB2 Table.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.PrevCase.Number = entities.Case1.Number;
        UseExtToDoACommit();
      }

      // *****************************************************************
      // *Write control numbers to the CONTROL RPT. DDNAME=RPT98.        *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Counter.Count = 0;

      do
      {
        ++local.Counter.Count;

        switch(local.Counter.Count)
        {
          case 1:
            // *Write Office Totals to the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail = "OFFICE: " + NumberToString
              (entities.Office.SystemGeneratedId, 12, 4) + " " + entities
              .Office.Name + "   TOTAL Cases: " + NumberToString
              (local.TotCasesInOffc.Count, 10, 6);

            break;
          case 2:
            // *Write Office Total reassigns to the CONTROL RPT. DDNAME=RPT98.
            // *
            local.NeededToWrite.RptDetail =
              "                                   TOTAL CASES REASSIGNED: " + NumberToString
              (local.TotReassignCnt.Count, 10, 6);

            break;
          case 3:
            // *Write Office Total No Change to the CONTROL RPT. DDNAME=RPT98.
            // *
            local.NeededToWrite.RptDetail =
              "                                  TOTAL CASES NOT CHANGED: " + NumberToString
              (local.TotCaseNotReasgnd.Count, 10, 6);

            break;
          case 4:
            // *WriteOffice Total overrides to the CONTROL RPT. DDNAME=RPT98.
            // *
            local.NeededToWrite.RptDetail =
              "                                     TOTAL CASE OVERRIDES: " + NumberToString
              (local.TotOvrdInOffc.Count, 10, 6);

            break;
          case 5:
            // *WriteOffice Total Case errors to the CONTROL RPT. DDNAME=RPT98.
            // *
            local.NeededToWrite.RptDetail =
              "                                        TOTAL CASE ERRORS: " + NumberToString
              (local.TotOffcErrors.Count, 10, 6);

            break;
          case 6:
            // *Write Blank line to the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail = "";

            break;
          case 7:
            // *Write Office Total Case reassigns errors to the CONTROL RPT. 
            // DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "                           TOTAL CASE REASSIGNMENT ERRORS: " + NumberToString
              (local.TotCaseErrors.Count, 10, 6);

            break;
          case 8:
            // *Write Office Total Case_Unit reassigns errors to the CONTROL 
            // RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "                      TOTAL CASE UNIT REASSIGNMENT ERRORS: " + NumberToString
              (local.TotCuErrors.Count, 10, 6);

            break;
          case 9:
            // *Write Office Total Case monitored activity reassigns errors to 
            // the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "                          TOTAL MONITORED ACTIVITY ERRORS: " + NumberToString
              (local.TotMonActErrors.Count, 10, 6);

            break;
          case 10:
            // *Write Office Total Case office service provider alert reassigns 
            // errors to the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "               TOTAL OFFICE SERVICE PROVIDER ALERT ERRORS: " + NumberToString
              (local.TotMonActErrors.Count, 10, 6);

            break;
          case 11:
            // *Write Office Total Case Monitored Document reassigns errors to 
            // the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "                          TOTAL MONITORED DOCUMENT ERRORS: " + NumberToString
              (local.TotMonActErrors.Count, 10, 6);

            break;
          case 12:
            // *Write Blank line to the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail = "";

            break;
          case 13:
            // *Write Office Total Infrastructure records created to the CONTROL
            // RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "                TOTAL CASE INFRASTRUCTURE RECORDS CREATED: " + NumberToString
              (local.TotInfrastrCnt.Count, 10, 6);

            break;
          case 14:
            // *Write Office Total Document Infrastructure records created to 
            // the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "       TOTAL CASE DOCUMENT INFRASTRUCTURE RECORDS CREATED: " + NumberToString
              (local.TotDocInfraCnt.Count, 10, 6);

            break;
          case 15:
            // *Write Office Total Cases Reassigned  NO Document Infrastructure 
            // record created to the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail =
              "       TOTAL CASES REASSIGNED NO DOCUMENT RECORDS CREATED: " + NumberToString
              (local.TotArrearsOnly.Count, 10, 6);

            break;
          case 16:
            // *Write Blank line to the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            continue;
        }

        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing to the Control Report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      while(local.Counter.Count != 16);

      // ************************************************************
      // Set restart indicator to Yes
      // ************************************************************
      local.ProgramCheckpointRestart.RestartInd = "Y";
      local.ProgramCheckpointRestart.RestartInfo =
        NumberToString(entities.Office.SystemGeneratedId, 12, 4);
      UseUpdatePgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ok, continue processing
      }
      else
      {
        return;
      }

ReadEach2:
      ;
    }

ReadEach3:

    // **** Check for a no caseload requested situation ****
    if (IsEmpty(local.RedistRequest.Flag))
    {
      // *****************************************************************
      // *Write Message to the CONTROL RPT. DDNAME=RPT98.        *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Counter.Count = 0;

      do
      {
        ++local.Counter.Count;

        switch(local.Counter.Count)
        {
          case 1:
            local.NeededToWrite.RptDetail =
              "No caseload Preview or Redistribution request found for process date: " +
              local.DisplayDate.Text10;

            break;
          case 2:
            // *Write Blank line to the CONTROL RPT. DDNAME=RPT98.        *
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            continue;
        }

        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing to the Control Report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      while(local.Counter.Count != 2);

      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    // ************************************************************
    // Set restart indicator to no because we have successfully
    // finished processing
    // ************************************************************
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // **** START OF REPORT DSN CLOSE PROCESS ****
    if (!IsExitState("ACO_NI0000_PROCESSING_COMPLETE"))
    {
      local.Send.Parm1 = "CF";
      local.Send.Parm2 = "";
      UseEabSpCaseloadDistributionRpt2();

      if (!IsEmpty(local.Return1.Parm1))
      {
        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered closing the Caseload Reassignment Report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseEabCsldTest2();

      if (!IsEmpty(local.Return1.Parm1))
      {
        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered closing the intermediate output file.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // **** END OF REPORT DSN CLOSE PROCESS ****
    }

    // *****************************************************************
    // * Close the CONTROL RPT. DDNAME=RPT98.                          *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCaseAssignment(CaseAssignment source,
    CaseAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveGroupToLocal1(SpCabSortArray.Export.GroupGroup source,
    Local.LocalGroup target)
  {
    target.ServiceProvider.Assign(source.ServiceProvider);
    MoveOfficeServiceProvider(source.OfficeServiceProvider,
      target.OfficeServiceProvider);
    target.OfficeCaseloadAssignment.Assign(source.OfficeCaseloadAssignment);
    target.Program.Code = source.Program.Code;
    target.Tribunal.Identifier = source.Tribunal.Identifier;
    MoveTextWorkArea(source.BegAlpha, target.BegAlpha);
    MoveTextWorkArea(source.EndAlpha, target.EndAlpha);
    target.Ovrd.Count = source.Ovrd.Count;
    target.Case1.Count = source.Case1.Count;
    target.CaseCnt.Count = source.CaseCnt.Count;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveLocal1ToGroup(Local.LocalGroup source,
    SpCabSortArray.Import.GroupGroup target)
  {
    target.ServiceProvider.Assign(source.ServiceProvider);
    MoveOfficeServiceProvider(source.OfficeServiceProvider,
      target.OfficeServiceProvider);
    target.OfficeCaseloadAssignment.Assign(source.OfficeCaseloadAssignment);
    target.Program.Code = source.Program.Code;
    target.Tribunal.Identifier = source.Tribunal.Identifier;
    MoveTextWorkArea(source.BegAlpha, target.BegAlpha);
    MoveTextWorkArea(source.EndAlpha, target.EndAlpha);
    target.Ovrd.Count = source.Ovrd.Count;
    target.Case1.Count = source.Case1.Count;
    target.CaseCnt.Count = source.CaseCnt.Count;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveOfficeAddress(OfficeAddress source,
    OfficeAddress target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
  }

  private static void MoveOfficeCaseloadAssignment1(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.AssignmentIndicator = source.AssignmentIndicator;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EndingAlpha = source.EndingAlpha;
    target.BeginingAlpha = source.BeginingAlpha;
    target.Priority = source.Priority;
    target.Function = source.Function;
    target.AssignmentType = source.AssignmentType;
  }

  private static void MoveOfficeCaseloadAssignment2(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EndingAlpha = source.EndingAlpha;
    target.BeginingAlpha = source.BeginingAlpha;
    target.Priority = source.Priority;
    target.Function = source.Function;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
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

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text8 = source.Text8;
    target.Text30 = source.Text30;
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
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Initialized.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabCsldTest1()
  {
    var useImport = new EabCsldTest.Import();
    var useExport = new EabCsldTest.Export();

    useImport.ReportParms.Assign(local.Send);
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.Case1.Number = entities.Case1.Number;
    useImport.Program.Code = local.CaseProgram.Code;
    useImport.CaseFuncWorkSet.FuncText3 = local.CaseFuncWorkSet.FuncText3;
    useImport.TextWorkArea.Text8 = local.CaseName.Text8;
    useImport.OldSp.UserId = entities.OldServiceProvider.UserId;
    useImport.NewSp.UserId = entities.NewServiceProvider.UserId;
    useImport.Tribunal.Text4 = local.CaseTribunal1.Text4;
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabCsldTest.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabCsldTest2()
  {
    var useImport = new EabCsldTest.Import();
    var useExport = new EabCsldTest.Export();

    useImport.ReportParms.Assign(local.Send);
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabCsldTest.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseEabSpCaseloadDistributionRpt1()
  {
    var useImport = new EabSpCaseloadDistributionRpt.Import();
    var useExport = new EabSpCaseloadDistributionRpt.Export();

    MoveReportParms(local.Send, useImport.ReportParms);
    useImport.RptHeading.Text30 = local.RptHeading.Text30;
    MoveOffice(entities.Office, useImport.Office);
    MoveOfficeAddress(entities.OfficeAddress, useImport.OfficeAddress);
    useImport.RptCityStZip.Text30 = local.RptCityStZip.Text30;
    useImport.DateWorkArea.Date = local.Current.Date;
    useImport.ServiceProvider.SystemGeneratedId =
      local.RptServiceProvider.SystemGeneratedId;
    useImport.CsePersonsWorkSet.FormattedName =
      local.RptSpFormatted.FormattedName;
    useImport.OfficeServiceProvider.RoleCode =
      local.RptOfficeServiceProvider.RoleCode;
    useImport.Program.Code = local.RptProgram.Code;
    useImport.OfficeCaseloadAssignment.
      Assign(local.RptOfficeCaseloadAssignment);
    useImport.Cases.Count = local.RptSpCaseNbr.Count;
    useImport.Ovrd.Count = local.RptSpOvrd.Count;
    useImport.Tribunal.Text4 = local.RptTribunal1.Text4;
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabSpCaseloadDistributionRpt.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabSpCaseloadDistributionRpt2()
  {
    var useImport = new EabSpCaseloadDistributionRpt.Import();
    var useExport = new EabSpCaseloadDistributionRpt.Export();

    MoveReportParms(local.Send, useImport.ReportParms);
    MoveOffice(entities.Office, useImport.Office);
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabSpCaseloadDistributionRpt.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabSpCaseloadDistributionRpt3()
  {
    var useImport = new EabSpCaseloadDistributionRpt.Import();
    var useExport = new EabSpCaseloadDistributionRpt.Export();

    MoveReportParms(local.Send, useImport.ReportParms);
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabSpCaseloadDistributionRpt.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
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

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    local.CaseFuncWorkSet.FuncText3 = useExport.CaseFuncWorkSet.FuncText3;
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    MoveCase1(entities.Case1, useImport.Case1);
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    local.CaseProgram.Code = useExport.Program.Code;
  }

  private void UseSpCabB300ReassignBatch()
  {
    var useImport = new SpCabB300ReassignBatch.Import();
    var useExport = new SpCabB300ReassignBatch.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.Persistent.Assign(entities.Case1);
    useImport.Old.UserId = entities.OldServiceProvider.UserId;
    useImport.PersistentOld.Assign(entities.OldOfficeServiceProvider);
    MoveServiceProvider(local.NewServiceProvider, useImport.NewServiceProvider);
    MoveOfficeServiceProvider(local.NewOfficeServiceProvider,
      useImport.NewOfficeServiceProvider);
    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.ChkpntNumbDeletes.Count = local.ChkpntNumbDeletes.Count;

    Call(SpCabB300ReassignBatch.Execute, useImport, useExport);

    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbDeletes.Count = useExport.ChkpntNumbDeletes.Count;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabDetrmnCaseNameTribunal()
  {
    var useImport = new SpCabDetrmnCaseNameTribunal.Import();
    var useExport = new SpCabDetrmnCaseNameTribunal.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Case1.Number = entities.Case1.Number;

    Call(SpCabDetrmnCaseNameTribunal.Execute, useImport, useExport);

    MoveTextWorkArea(useExport.TextWorkArea, local.CaseName);
    local.CaseTribunal2.Identifier = useExport.Tribunal.Identifier;
  }

  private void UseSpCabReassignCaseBatch()
  {
    var useImport = new SpCabReassignCaseBatch.Import();
    var useExport = new SpCabReassignCaseBatch.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;
    useImport.CurrentDatePlus1.Date = local.CurrentPlus1.Date;
    useImport.CurrentDate.Date = local.Current.Date;
    useImport.Office.Assign(entities.Office);
    useImport.New1.Assign(entities.NewOfficeServiceProvider);
    useImport.Current.Assign(entities.OldCaseAssignment);
    useImport.Persistent.Assign(entities.Case1);

    Call(SpCabReassignCaseBatch.Execute, useImport, useExport);

    MoveOfficeServiceProvider(useImport.New1, entities.NewOfficeServiceProvider);
      
    MoveCaseAssignment(useImport.Current, entities.OldCaseAssignment);
    MoveCase1(useImport.Persistent, entities.Case1);
  }

  private void UseSpCabReassignCaseUnitBatch()
  {
    var useImport = new SpCabReassignCaseUnitBatch.Import();
    var useExport = new SpCabReassignCaseUnitBatch.Export();

    useImport.Create.Count = local.ChkpntNumbCreates.Count;
    useImport.Update.Count = local.ChkpntNumbUpdates.Count;
    useImport.CurrentDateWorkArea.Date = local.Current.Date;
    useImport.CurrentDatePlus1.Date = local.CurrentPlus1.Date;
    useImport.Max.Date = local.Max.Date;
    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;
    useImport.Case1.Assign(entities.Case1);
    useImport.CurrentServiceProvider.Assign(entities.OldServiceProvider);
    useImport.New1.Assign(entities.NewOfficeServiceProvider);

    Call(SpCabReassignCaseUnitBatch.Execute, useImport, useExport);

    MoveServiceProvider(useImport.CurrentServiceProvider,
      entities.OldServiceProvider);
    MoveOfficeServiceProvider(useImport.New1, entities.NewOfficeServiceProvider);
      
    local.ChkpntNumbCreates.Count = useExport.Create.Count;
    local.ChkpntNumbUpdates.Count = useExport.Update.Count;
  }

  private void UseSpCabReassignMonActsBatch()
  {
    var useImport = new SpCabReassignMonActsBatch.Import();
    var useExport = new SpCabReassignMonActsBatch.Export();

    useImport.Update.Count = local.ChkpntNumbUpdates.Count;
    useImport.Create.Count = local.ChkpntNumbCreates.Count;
    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;
    useImport.Case1.Assign(entities.Case1);
    useImport.CurrentServiceProvider.Assign(entities.OldServiceProvider);
    useImport.New1.Assign(entities.NewOfficeServiceProvider);
    useImport.CurrentDateWorkArea.Date = local.Current.Date;
    useImport.CurrentDatePlus1.Date = local.CurrentPlus1.Date;
    useImport.Max.Date = local.Max.Date;

    Call(SpCabReassignMonActsBatch.Execute, useImport, useExport);

    MoveOfficeServiceProvider(useImport.New1, entities.NewOfficeServiceProvider);
      
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
  }

  private void UseSpCabSortArray()
  {
    var useImport = new SpCabSortArray.Import();
    var useExport = new SpCabSortArray.Export();

    local.Local1.CopyTo(useImport.Group, MoveLocal1ToGroup);

    Call(SpCabSortArray.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Local1, MoveGroupToLocal1);
  }

  private void UseSpReassignIntCaseBatch()
  {
    var useImport = new SpReassignIntCaseBatch.Import();
    var useExport = new SpReassignIntCaseBatch.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;
    useImport.CurrentPlusOne.Date = local.CurrentPlus1.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.NewOffice.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.Case1.Number = local.CaseCase.Number;
    useImport.NewOfficeServiceProvider.RoleCode =
      entities.NewOfficeServiceProvider.RoleCode;
    MoveServiceProvider(local.NewServiceProvider, useImport.NewServiceProvider);

    Call(SpReassignIntCaseBatch.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void DeleteOfficeCaseloadAssignment()
  {
    Update("DeleteOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OldOfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });
  }

  private IEnumerable<bool>
    ReadCaseCaseAssignmentServiceProviderOfficeServiceProvider()
  {
    entities.Case1.Populated = false;
    entities.OldCaseAssignment.Populated = false;
    entities.OldServiceProvider.Populated = false;
    entities.OldOfficeServiceProvider.Populated = false;

    return ReadEach(
      "ReadCaseCaseAssignmentServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.OldCaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.OldCaseAssignment.ReasonCode = db.GetString(reader, 2);
        entities.OldCaseAssignment.OverrideInd = db.GetString(reader, 3);
        entities.OldCaseAssignment.EffectiveDate = db.GetDate(reader, 4);
        entities.OldCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OldCaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.OldCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OldCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.OldCaseAssignment.SpdId = db.GetInt32(reader, 9);
        entities.OldOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 9);
        entities.OldCaseAssignment.OffId = db.GetInt32(reader, 10);
        entities.OldOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 10);
        entities.OldCaseAssignment.OspCode = db.GetString(reader, 11);
        entities.OldOfficeServiceProvider.RoleCode = db.GetString(reader, 11);
        entities.OldCaseAssignment.OspDate = db.GetDate(reader, 12);
        entities.OldOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 12);
        entities.OldServiceProvider.SystemGeneratedId = db.GetInt32(reader, 13);
        entities.OldServiceProvider.UserId = db.GetString(reader, 14);
        entities.Case1.Populated = true;
        entities.OldCaseAssignment.Populated = true;
        entities.OldServiceProvider.Populated = true;
        entities.OldOfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.Office.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", local.ChkpntRestartKey.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.EffectiveDate = db.GetDate(reader, 2);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeAddress1()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress1",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeAddress2()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress2",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment1()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetString(command, "assignmentInd", local.Use.AssignmentIndicator);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 9);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 10);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 11);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 12);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 15);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 17);
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment2()
  {
    entities.OldOfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OldOfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OldOfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 1);
        entities.OldOfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 2);
        entities.OldOfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 3);
        entities.OldOfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 4);
        entities.OldOfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignmentOfficeServiceProvider()
  {
    entities.OfficeCaseloadAssignment.Populated = false;
    entities.NewServiceProvider.Populated = false;
    entities.NewOfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 9);
        entities.NewOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 9);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 10);
        entities.NewOfficeServiceProvider.RoleCode = db.GetString(reader, 10);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 11);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 12);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 15);
        entities.NewOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 15);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.NewOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 16);
        entities.NewServiceProvider.SystemGeneratedId = db.GetInt32(reader, 16);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 17);
        entities.NewOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 18);
        entities.NewServiceProvider.UserId = db.GetString(reader, 19);
        entities.NewServiceProvider.LastName = db.GetString(reader, 20);
        entities.NewServiceProvider.FirstName = db.GetString(reader, 21);
        entities.OfficeCaseloadAssignment.Populated = true;
        entities.NewServiceProvider.Populated = true;
        entities.NewOfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetString(
          command, "roleCode",
          local.Local1.Item.OfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "spdGeneratedId",
          local.Local1.Item.ServiceProvider.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.NewOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.NewOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.NewOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId",
          entities.OfficeCaseloadAssignment.PrgGeneratedId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.NewServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          local.Local1.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewServiceProvider.UserId = db.GetString(reader, 1);
        entities.NewServiceProvider.LastName = db.GetString(reader, 2);
        entities.NewServiceProvider.FirstName = db.GetString(reader, 3);
        entities.NewServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProvider()
  {
    entities.OverriddenServiceProvider.Populated = false;
    entities.OverriddenOfficeServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.OldServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OverriddenServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.OverriddenOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.OverriddenServiceProvider.UserId = db.GetString(reader, 1);
        entities.OverriddenServiceProvider.LastName = db.GetString(reader, 2);
        entities.OverriddenServiceProvider.FirstName = db.GetString(reader, 3);
        entities.OverriddenOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.OverriddenOfficeServiceProvider.RoleCode =
          db.GetString(reader, 5);
        entities.OverriddenOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 6);
        entities.OverriddenServiceProvider.Populated = true;
        entities.OverriddenOfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.OfficeCaseloadAssignment.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
      });
  }

  private void UpdateOfficeCaseloadAssignment()
  {
    var effectiveDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var assignmentIndicator = local.Reset.AssignmentIndicator;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("UpdateOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatedTstamp);
        db.SetString(command, "assignmentInd", assignmentIndicator);
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.EffectiveDate = effectiveDate;
    entities.OfficeCaseloadAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.OfficeCaseloadAssignment.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.OfficeCaseloadAssignment.AssignmentIndicator = assignmentIndicator;
    entities.OfficeCaseloadAssignment.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
  public class Local: IInitializable
  {
    /// <summary>A SwapGroup group.</summary>
    [Serializable]
    public class SwapGroup
    {
      /// <summary>
      /// A value of SwapOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("swapOfficeServiceProvider")]
      public OfficeServiceProvider SwapOfficeServiceProvider
      {
        get => swapOfficeServiceProvider ??= new();
        set => swapOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of SwapProgram.
      /// </summary>
      [JsonPropertyName("swapProgram")]
      public Program SwapProgram
      {
        get => swapProgram ??= new();
        set => swapProgram = value;
      }

      /// <summary>
      /// A value of SwapTribunal.
      /// </summary>
      [JsonPropertyName("swapTribunal")]
      public Tribunal SwapTribunal
      {
        get => swapTribunal ??= new();
        set => swapTribunal = value;
      }

      /// <summary>
      /// A value of SwapServiceProvider.
      /// </summary>
      [JsonPropertyName("swapServiceProvider")]
      public ServiceProvider SwapServiceProvider
      {
        get => swapServiceProvider ??= new();
        set => swapServiceProvider = value;
      }

      /// <summary>
      /// A value of SwapOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("swapOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment SwapOfficeCaseloadAssignment
      {
        get => swapOfficeCaseloadAssignment ??= new();
        set => swapOfficeCaseloadAssignment = value;
      }

      private OfficeServiceProvider swapOfficeServiceProvider;
      private Program swapProgram;
      private Tribunal swapTribunal;
      private ServiceProvider swapServiceProvider;
      private OfficeCaseloadAssignment swapOfficeCaseloadAssignment;
    }

    /// <summary>A CompareGroup group.</summary>
    [Serializable]
    public class CompareGroup
    {
      /// <summary>
      /// A value of CompareOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("compareOfficeServiceProvider")]
      public OfficeServiceProvider CompareOfficeServiceProvider
      {
        get => compareOfficeServiceProvider ??= new();
        set => compareOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of CompareProgram.
      /// </summary>
      [JsonPropertyName("compareProgram")]
      public Program CompareProgram
      {
        get => compareProgram ??= new();
        set => compareProgram = value;
      }

      /// <summary>
      /// A value of CompareTribunal.
      /// </summary>
      [JsonPropertyName("compareTribunal")]
      public Tribunal CompareTribunal
      {
        get => compareTribunal ??= new();
        set => compareTribunal = value;
      }

      /// <summary>
      /// A value of CompareServiceProvider.
      /// </summary>
      [JsonPropertyName("compareServiceProvider")]
      public ServiceProvider CompareServiceProvider
      {
        get => compareServiceProvider ??= new();
        set => compareServiceProvider = value;
      }

      /// <summary>
      /// A value of CompareOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("compareOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment CompareOfficeCaseloadAssignment
      {
        get => compareOfficeCaseloadAssignment ??= new();
        set => compareOfficeCaseloadAssignment = value;
      }

      private OfficeServiceProvider compareOfficeServiceProvider;
      private Program compareProgram;
      private Tribunal compareTribunal;
      private ServiceProvider compareServiceProvider;
      private OfficeCaseloadAssignment compareOfficeCaseloadAssignment;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of BegAlpha.
      /// </summary>
      [JsonPropertyName("begAlpha")]
      public TextWorkArea BegAlpha
      {
        get => begAlpha ??= new();
        set => begAlpha = value;
      }

      /// <summary>
      /// A value of EndAlpha.
      /// </summary>
      [JsonPropertyName("endAlpha")]
      public TextWorkArea EndAlpha
      {
        get => endAlpha ??= new();
        set => endAlpha = value;
      }

      /// <summary>
      /// A value of Ovrd.
      /// </summary>
      [JsonPropertyName("ovrd")]
      public Common Ovrd
      {
        get => ovrd ??= new();
        set => ovrd = value;
      }

      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Common Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of CaseCnt.
      /// </summary>
      [JsonPropertyName("caseCnt")]
      public Common CaseCnt
      {
        get => caseCnt ??= new();
        set => caseCnt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private ServiceProvider serviceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
      private Program program;
      private Tribunal tribunal;
      private TextWorkArea begAlpha;
      private TextWorkArea endAlpha;
      private Common ovrd;
      private Common case1;
      private Common caseCnt;
    }

    /// <summary>
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public Common J
    {
      get => j ??= new();
      set => j = value;
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
    }

    /// <summary>
    /// Gets a value of Swap.
    /// </summary>
    [JsonPropertyName("swap")]
    public SwapGroup Swap
    {
      get => swap ?? (swap = new());
      set => swap = value;
    }

    /// <summary>
    /// Gets a value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public CompareGroup Compare
    {
      get => compare ?? (compare = new());
      set => compare = value;
    }

    /// <summary>
    /// A value of Swap1.
    /// </summary>
    [JsonPropertyName("swap1")]
    public Common Swap1
    {
      get => swap1 ??= new();
      set => swap1 = value;
    }

    /// <summary>
    /// A value of RptTribunal1.
    /// </summary>
    [JsonPropertyName("rptTribunal1")]
    public TextWorkArea RptTribunal1
    {
      get => rptTribunal1 ??= new();
      set => rptTribunal1 = value;
    }

    /// <summary>
    /// A value of CaseTribunal1.
    /// </summary>
    [JsonPropertyName("caseTribunal1")]
    public TextWorkArea CaseTribunal1
    {
      get => caseTribunal1 ??= new();
      set => caseTribunal1 = value;
    }

    /// <summary>
    /// A value of BlankTribunal.
    /// </summary>
    [JsonPropertyName("blankTribunal")]
    public Tribunal BlankTribunal
    {
      get => blankTribunal ??= new();
      set => blankTribunal = value;
    }

    /// <summary>
    /// A value of CaseTribunal2.
    /// </summary>
    [JsonPropertyName("caseTribunal2")]
    public Tribunal CaseTribunal2
    {
      get => caseTribunal2 ??= new();
      set => caseTribunal2 = value;
    }

    /// <summary>
    /// A value of RptTribunal2.
    /// </summary>
    [JsonPropertyName("rptTribunal2")]
    public Tribunal RptTribunal2
    {
      get => rptTribunal2 ??= new();
      set => rptTribunal2 = value;
    }

    /// <summary>
    /// A value of InitTribunal.
    /// </summary>
    [JsonPropertyName("initTribunal")]
    public Tribunal InitTribunal
    {
      get => initTribunal ??= new();
      set => initTribunal = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of AddedToGroupView.
    /// </summary>
    [JsonPropertyName("addedToGroupView")]
    public Common AddedToGroupView
    {
      get => addedToGroupView ??= new();
      set => addedToGroupView = value;
    }

    /// <summary>
    /// A value of InitServiceProvider.
    /// </summary>
    [JsonPropertyName("initServiceProvider")]
    public ServiceProvider InitServiceProvider
    {
      get => initServiceProvider ??= new();
      set => initServiceProvider = value;
    }

    /// <summary>
    /// A value of InitOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("initOfficeServiceProvider")]
    public OfficeServiceProvider InitOfficeServiceProvider
    {
      get => initOfficeServiceProvider ??= new();
      set => initOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of InitOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("initOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment InitOfficeCaseloadAssignment
    {
      get => initOfficeCaseloadAssignment ??= new();
      set => initOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of InitProgram.
    /// </summary>
    [JsonPropertyName("initProgram")]
    public Program InitProgram
    {
      get => initProgram ??= new();
      set => initProgram = value;
    }

    /// <summary>
    /// A value of InitBegAlpha.
    /// </summary>
    [JsonPropertyName("initBegAlpha")]
    public TextWorkArea InitBegAlpha
    {
      get => initBegAlpha ??= new();
      set => initBegAlpha = value;
    }

    /// <summary>
    /// A value of InitEndAlpha.
    /// </summary>
    [JsonPropertyName("initEndAlpha")]
    public TextWorkArea InitEndAlpha
    {
      get => initEndAlpha ??= new();
      set => initEndAlpha = value;
    }

    /// <summary>
    /// A value of InitOvrd.
    /// </summary>
    [JsonPropertyName("initOvrd")]
    public Common InitOvrd
    {
      get => initOvrd ??= new();
      set => initOvrd = value;
    }

    /// <summary>
    /// A value of InitCase.
    /// </summary>
    [JsonPropertyName("initCase")]
    public Common InitCase
    {
      get => initCase ??= new();
      set => initCase = value;
    }

    /// <summary>
    /// A value of InitCaseCnt.
    /// </summary>
    [JsonPropertyName("initCaseCnt")]
    public Common InitCaseCnt
    {
      get => initCaseCnt ??= new();
      set => initCaseCnt = value;
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
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
    }

    /// <summary>
    /// A value of RedistRequest.
    /// </summary>
    [JsonPropertyName("redistRequest")]
    public Common RedistRequest
    {
      get => redistRequest ??= new();
      set => redistRequest = value;
    }

    /// <summary>
    /// A value of DisplayDate.
    /// </summary>
    [JsonPropertyName("displayDate")]
    public TextWorkArea DisplayDate
    {
      get => displayDate ??= new();
      set => displayDate = value;
    }

    /// <summary>
    /// A value of CaseCase.
    /// </summary>
    [JsonPropertyName("caseCase")]
    public Case1 CaseCase
    {
      get => caseCase ??= new();
      set => caseCase = value;
    }

    /// <summary>
    /// A value of TotArrearsOnly.
    /// </summary>
    [JsonPropertyName("totArrearsOnly")]
    public Common TotArrearsOnly
    {
      get => totArrearsOnly ??= new();
      set => totArrearsOnly = value;
    }

    /// <summary>
    /// A value of TotOffcErrors.
    /// </summary>
    [JsonPropertyName("totOffcErrors")]
    public Common TotOffcErrors
    {
      get => totOffcErrors ??= new();
      set => totOffcErrors = value;
    }

    /// <summary>
    /// A value of TotCasesInOffc.
    /// </summary>
    [JsonPropertyName("totCasesInOffc")]
    public Common TotCasesInOffc
    {
      get => totCasesInOffc ??= new();
      set => totCasesInOffc = value;
    }

    /// <summary>
    /// A value of TotReassignCnt.
    /// </summary>
    [JsonPropertyName("totReassignCnt")]
    public Common TotReassignCnt
    {
      get => totReassignCnt ??= new();
      set => totReassignCnt = value;
    }

    /// <summary>
    /// A value of TotCaseNotReasgnd.
    /// </summary>
    [JsonPropertyName("totCaseNotReasgnd")]
    public Common TotCaseNotReasgnd
    {
      get => totCaseNotReasgnd ??= new();
      set => totCaseNotReasgnd = value;
    }

    /// <summary>
    /// A value of TotOvrdInOffc.
    /// </summary>
    [JsonPropertyName("totOvrdInOffc")]
    public Common TotOvrdInOffc
    {
      get => totOvrdInOffc ??= new();
      set => totOvrdInOffc = value;
    }

    /// <summary>
    /// A value of TotInfrastrCnt.
    /// </summary>
    [JsonPropertyName("totInfrastrCnt")]
    public Common TotInfrastrCnt
    {
      get => totInfrastrCnt ??= new();
      set => totInfrastrCnt = value;
    }

    /// <summary>
    /// A value of TotDocInfraCnt.
    /// </summary>
    [JsonPropertyName("totDocInfraCnt")]
    public Common TotDocInfraCnt
    {
      get => totDocInfraCnt ??= new();
      set => totDocInfraCnt = value;
    }

    /// <summary>
    /// A value of TotCaseErrors.
    /// </summary>
    [JsonPropertyName("totCaseErrors")]
    public Common TotCaseErrors
    {
      get => totCaseErrors ??= new();
      set => totCaseErrors = value;
    }

    /// <summary>
    /// A value of TotCuErrors.
    /// </summary>
    [JsonPropertyName("totCuErrors")]
    public Common TotCuErrors
    {
      get => totCuErrors ??= new();
      set => totCuErrors = value;
    }

    /// <summary>
    /// A value of TotMonActErrors.
    /// </summary>
    [JsonPropertyName("totMonActErrors")]
    public Common TotMonActErrors
    {
      get => totMonActErrors ??= new();
      set => totMonActErrors = value;
    }

    /// <summary>
    /// A value of TotAlertErrors.
    /// </summary>
    [JsonPropertyName("totAlertErrors")]
    public Common TotAlertErrors
    {
      get => totAlertErrors ??= new();
      set => totAlertErrors = value;
    }

    /// <summary>
    /// A value of TotDmonErrors.
    /// </summary>
    [JsonPropertyName("totDmonErrors")]
    public Common TotDmonErrors
    {
      get => totDmonErrors ??= new();
      set => totDmonErrors = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of Zip10.
    /// </summary>
    [JsonPropertyName("zip10")]
    public TextWorkArea Zip10
    {
      get => zip10 ??= new();
      set => zip10 = value;
    }

    /// <summary>
    /// A value of RptCityStZip.
    /// </summary>
    [JsonPropertyName("rptCityStZip")]
    public TextWorkArea RptCityStZip
    {
      get => rptCityStZip ??= new();
      set => rptCityStZip = value;
    }

    /// <summary>
    /// A value of RptHeading.
    /// </summary>
    [JsonPropertyName("rptHeading")]
    public TextWorkArea RptHeading
    {
      get => rptHeading ??= new();
      set => rptHeading = value;
    }

    /// <summary>
    /// A value of RptOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("rptOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment RptOfficeCaseloadAssignment
    {
      get => rptOfficeCaseloadAssignment ??= new();
      set => rptOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of RptSpOvrd.
    /// </summary>
    [JsonPropertyName("rptSpOvrd")]
    public Common RptSpOvrd
    {
      get => rptSpOvrd ??= new();
      set => rptSpOvrd = value;
    }

    /// <summary>
    /// A value of RptSpCaseNbr.
    /// </summary>
    [JsonPropertyName("rptSpCaseNbr")]
    public Common RptSpCaseNbr
    {
      get => rptSpCaseNbr ??= new();
      set => rptSpCaseNbr = value;
    }

    /// <summary>
    /// A value of RptOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("rptOfficeServiceProvider")]
    public OfficeServiceProvider RptOfficeServiceProvider
    {
      get => rptOfficeServiceProvider ??= new();
      set => rptOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of RptProgram.
    /// </summary>
    [JsonPropertyName("rptProgram")]
    public Program RptProgram
    {
      get => rptProgram ??= new();
      set => rptProgram = value;
    }

    /// <summary>
    /// A value of RptServiceProvider.
    /// </summary>
    [JsonPropertyName("rptServiceProvider")]
    public ServiceProvider RptServiceProvider
    {
      get => rptServiceProvider ??= new();
      set => rptServiceProvider = value;
    }

    /// <summary>
    /// A value of RptSpFormatted.
    /// </summary>
    [JsonPropertyName("rptSpFormatted")]
    public CsePersonsWorkSet RptSpFormatted
    {
      get => rptSpFormatted ??= new();
      set => rptSpFormatted = value;
    }

    /// <summary>
    /// A value of TestFlag.
    /// </summary>
    [JsonPropertyName("testFlag")]
    public Common TestFlag
    {
      get => testFlag ??= new();
      set => testFlag = value;
    }

    /// <summary>
    /// A value of CurrentPlus1.
    /// </summary>
    [JsonPropertyName("currentPlus1")]
    public DateWorkArea CurrentPlus1
    {
      get => currentPlus1 ??= new();
      set => currentPlus1 = value;
    }

    /// <summary>
    /// A value of CaseName.
    /// </summary>
    [JsonPropertyName("caseName")]
    public TextWorkArea CaseName
    {
      get => caseName ??= new();
      set => caseName = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of CaseProgram.
    /// </summary>
    [JsonPropertyName("caseProgram")]
    public Program CaseProgram
    {
      get => caseProgram ??= new();
      set => caseProgram = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ChkpntRestartKey.
    /// </summary>
    [JsonPropertyName("chkpntRestartKey")]
    public Office ChkpntRestartKey
    {
      get => chkpntRestartKey ??= new();
      set => chkpntRestartKey = value;
    }

    /// <summary>
    /// A value of Use.
    /// </summary>
    [JsonPropertyName("use")]
    public OfficeCaseloadAssignment Use
    {
      get => use ??= new();
      set => use = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of BlankOfficeAddress.
    /// </summary>
    [JsonPropertyName("blankOfficeAddress")]
    public OfficeAddress BlankOfficeAddress
    {
      get => blankOfficeAddress ??= new();
      set => blankOfficeAddress = value;
    }

    /// <summary>
    /// A value of BlankServiceProvider.
    /// </summary>
    [JsonPropertyName("blankServiceProvider")]
    public ServiceProvider BlankServiceProvider
    {
      get => blankServiceProvider ??= new();
      set => blankServiceProvider = value;
    }

    /// <summary>
    /// A value of BlankOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("blankOfficeServiceProvider")]
    public OfficeServiceProvider BlankOfficeServiceProvider
    {
      get => blankOfficeServiceProvider ??= new();
      set => blankOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of BlankProgram.
    /// </summary>
    [JsonPropertyName("blankProgram")]
    public Program BlankProgram
    {
      get => blankProgram ??= new();
      set => blankProgram = value;
    }

    /// <summary>
    /// A value of Reset.
    /// </summary>
    [JsonPropertyName("reset")]
    public OfficeCaseloadAssignment Reset
    {
      get => reset ??= new();
      set => reset = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of ChkpntNumbReads.
    /// </summary>
    [JsonPropertyName("chkpntNumbReads")]
    public Common ChkpntNumbReads
    {
      get => chkpntNumbReads ??= new();
      set => chkpntNumbReads = value;
    }

    /// <summary>
    /// A value of ChkpntNumbUpdates.
    /// </summary>
    [JsonPropertyName("chkpntNumbUpdates")]
    public Common ChkpntNumbUpdates
    {
      get => chkpntNumbUpdates ??= new();
      set => chkpntNumbUpdates = value;
    }

    /// <summary>
    /// A value of ChkpntNumbCreates.
    /// </summary>
    [JsonPropertyName("chkpntNumbCreates")]
    public Common ChkpntNumbCreates
    {
      get => chkpntNumbCreates ??= new();
      set => chkpntNumbCreates = value;
    }

    /// <summary>
    /// A value of ChkpntNumbDeletes.
    /// </summary>
    [JsonPropertyName("chkpntNumbDeletes")]
    public Common ChkpntNumbDeletes
    {
      get => chkpntNumbDeletes ??= new();
      set => chkpntNumbDeletes = value;
    }

    /// <summary>
    /// A value of TotalNumbReads.
    /// </summary>
    [JsonPropertyName("totalNumbReads")]
    public Common TotalNumbReads
    {
      get => totalNumbReads ??= new();
      set => totalNumbReads = value;
    }

    /// <summary>
    /// A value of TotalNumbUpdates.
    /// </summary>
    [JsonPropertyName("totalNumbUpdates")]
    public Common TotalNumbUpdates
    {
      get => totalNumbUpdates ??= new();
      set => totalNumbUpdates = value;
    }

    /// <summary>
    /// A value of TotalNumbCreates.
    /// </summary>
    [JsonPropertyName("totalNumbCreates")]
    public Common TotalNumbCreates
    {
      get => totalNumbCreates ??= new();
      set => totalNumbCreates = value;
    }

    /// <summary>
    /// A value of TotalNumbDeletes.
    /// </summary>
    [JsonPropertyName("totalNumbDeletes")]
    public Common TotalNumbDeletes
    {
      get => totalNumbDeletes ??= new();
      set => totalNumbDeletes = value;
    }

    /// <summary>
    /// A value of StatOverrideCaseCnt.
    /// </summary>
    [JsonPropertyName("statOverrideCaseCnt")]
    public Common StatOverrideCaseCnt
    {
      get => statOverrideCaseCnt ??= new();
      set => statOverrideCaseCnt = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
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
    /// A value of AbortPgmInd.
    /// </summary>
    [JsonPropertyName("abortPgmInd")]
    public Common AbortPgmInd
    {
      get => abortPgmInd ??= new();
      set => abortPgmInd = value;
    }

    /// <summary>
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
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
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of UseBitBucket.
    /// </summary>
    [JsonPropertyName("useBitBucket")]
    public Common UseBitBucket
    {
      get => useBitBucket ??= new();
      set => useBitBucket = value;
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
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public ReportParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public ReportParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
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

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      j = null;
      i = null;
      swap = null;
      compare = null;
      swap1 = null;
      rptTribunal1 = null;
      caseTribunal1 = null;
      blankTribunal = null;
      caseTribunal2 = null;
      rptTribunal2 = null;
      initTribunal = null;
      newServiceProvider = null;
      newOfficeServiceProvider = null;
      addedToGroupView = null;
      initServiceProvider = null;
      initOfficeServiceProvider = null;
      initOfficeCaseloadAssignment = null;
      initProgram = null;
      initBegAlpha = null;
      initEndAlpha = null;
      initOvrd = null;
      initCase = null;
      initCaseCnt = null;
      commit = null;
      screenIdentification = null;
      redistRequest = null;
      displayDate = null;
      totArrearsOnly = null;
      totOffcErrors = null;
      totCasesInOffc = null;
      totReassignCnt = null;
      totCaseNotReasgnd = null;
      totOvrdInOffc = null;
      totInfrastrCnt = null;
      totDocInfraCnt = null;
      totCaseErrors = null;
      totCuErrors = null;
      totMonActErrors = null;
      totAlertErrors = null;
      totDmonErrors = null;
      counter = null;
      zip10 = null;
      rptCityStZip = null;
      rptHeading = null;
      rptOfficeCaseloadAssignment = null;
      rptSpOvrd = null;
      rptSpCaseNbr = null;
      rptOfficeServiceProvider = null;
      rptProgram = null;
      rptServiceProvider = null;
      rptSpFormatted = null;
      testFlag = null;
      currentPlus1 = null;
      caseName = null;
      infrastructure = null;
      outgoingDocument = null;
      document = null;
      spDocKey = null;
      caseProgram = null;
      max = null;
      initialized = null;
      current = null;
      chkpntRestartKey = null;
      use = null;
      officeAddress = null;
      blankOfficeAddress = null;
      blankServiceProvider = null;
      blankOfficeServiceProvider = null;
      blankProgram = null;
      reset = null;
      local1 = null;
      chkpntNumbReads = null;
      chkpntNumbUpdates = null;
      chkpntNumbCreates = null;
      chkpntNumbDeletes = null;
      totalNumbReads = null;
      totalNumbUpdates = null;
      totalNumbCreates = null;
      totalNumbDeletes = null;
      statOverrideCaseCnt = null;
      programProcessingInfo = null;
      programRun = null;
      programCheckpointRestart = null;
      programError = null;
      abortPgmInd = null;
      programControlTotal = null;
      passArea = null;
      caseFuncWorkSet = null;
      useBitBucket = null;
      eabFileHandling = null;
      neededToOpen = null;
      neededToWrite = null;
      send = null;
      return1 = null;
      exitStateWorkArea = null;
    }

    private Common j;
    private Common i;
    private SwapGroup swap;
    private CompareGroup compare;
    private Common swap1;
    private TextWorkArea rptTribunal1;
    private TextWorkArea caseTribunal1;
    private Tribunal blankTribunal;
    private Tribunal caseTribunal2;
    private Tribunal rptTribunal2;
    private Tribunal initTribunal;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private Common addedToGroupView;
    private ServiceProvider initServiceProvider;
    private OfficeServiceProvider initOfficeServiceProvider;
    private OfficeCaseloadAssignment initOfficeCaseloadAssignment;
    private Program initProgram;
    private TextWorkArea initBegAlpha;
    private TextWorkArea initEndAlpha;
    private Common initOvrd;
    private Common initCase;
    private Common initCaseCnt;
    private Common commit;
    private Common screenIdentification;
    private Case1 prevCase;
    private Common redistRequest;
    private TextWorkArea displayDate;
    private Case1 caseCase;
    private Common totArrearsOnly;
    private Common totOffcErrors;
    private Common totCasesInOffc;
    private Common totReassignCnt;
    private Common totCaseNotReasgnd;
    private Common totOvrdInOffc;
    private Common totInfrastrCnt;
    private Common totDocInfraCnt;
    private Common totCaseErrors;
    private Common totCuErrors;
    private Common totMonActErrors;
    private Common totAlertErrors;
    private Common totDmonErrors;
    private Common counter;
    private TextWorkArea zip10;
    private TextWorkArea rptCityStZip;
    private TextWorkArea rptHeading;
    private OfficeCaseloadAssignment rptOfficeCaseloadAssignment;
    private Common rptSpOvrd;
    private Common rptSpCaseNbr;
    private OfficeServiceProvider rptOfficeServiceProvider;
    private Program rptProgram;
    private ServiceProvider rptServiceProvider;
    private CsePersonsWorkSet rptSpFormatted;
    private Common testFlag;
    private DateWorkArea currentPlus1;
    private TextWorkArea caseName;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private Document document;
    private SpDocKey spDocKey;
    private Program caseProgram;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private DateWorkArea current;
    private Office chkpntRestartKey;
    private OfficeCaseloadAssignment use;
    private OfficeAddress officeAddress;
    private OfficeAddress blankOfficeAddress;
    private ServiceProvider blankServiceProvider;
    private OfficeServiceProvider blankOfficeServiceProvider;
    private Program blankProgram;
    private OfficeCaseloadAssignment reset;
    private Array<LocalGroup> local1;
    private Common chkpntNumbReads;
    private Common chkpntNumbUpdates;
    private Common chkpntNumbCreates;
    private Common chkpntNumbDeletes;
    private Common totalNumbReads;
    private Common totalNumbUpdates;
    private Common totalNumbCreates;
    private Common totalNumbDeletes;
    private Common statOverrideCaseCnt;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramError programError;
    private Common abortPgmInd;
    private ProgramControlTotal programControlTotal;
    private External passArea;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common useBitBucket;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private ReportParms send;
    private ReportParms return1;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of OverriddenServiceProvider.
    /// </summary>
    [JsonPropertyName("overriddenServiceProvider")]
    public ServiceProvider OverriddenServiceProvider
    {
      get => overriddenServiceProvider ??= new();
      set => overriddenServiceProvider = value;
    }

    /// <summary>
    /// A value of OverriddenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("overriddenOfficeServiceProvider")]
    public OfficeServiceProvider OverriddenOfficeServiceProvider
    {
      get => overriddenOfficeServiceProvider ??= new();
      set => overriddenOfficeServiceProvider = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// A value of OldCaseAssignment.
    /// </summary>
    [JsonPropertyName("oldCaseAssignment")]
    public CaseAssignment OldCaseAssignment
    {
      get => oldCaseAssignment ??= new();
      set => oldCaseAssignment = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of OldOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("oldOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment OldOfficeCaseloadAssignment
    {
      get => oldOfficeCaseloadAssignment ??= new();
      set => oldOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of OldServiceProvider.
    /// </summary>
    [JsonPropertyName("oldServiceProvider")]
    public ServiceProvider OldServiceProvider
    {
      get => oldServiceProvider ??= new();
      set => oldServiceProvider = value;
    }

    /// <summary>
    /// A value of OldOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oldOfficeServiceProvider")]
    public OfficeServiceProvider OldOfficeServiceProvider
    {
      get => oldOfficeServiceProvider ??= new();
      set => oldOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private Tribunal tribunal;
    private ServiceProvider overriddenServiceProvider;
    private OfficeServiceProvider overriddenOfficeServiceProvider;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Office office;
    private OfficeAddress officeAddress;
    private Case1 case1;
    private CaseAssignment oldCaseAssignment;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private Program program;
    private OfficeCaseloadAssignment oldOfficeCaseloadAssignment;
    private ServiceProvider oldServiceProvider;
    private OfficeServiceProvider oldOfficeServiceProvider;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private InterstateRequest interstateRequest;
  }
#endregion
}

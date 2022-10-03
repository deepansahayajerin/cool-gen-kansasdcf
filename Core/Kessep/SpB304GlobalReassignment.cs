// Program: SP_B304_GLOBAL_REASSIGNMENT, ID: 372783453, model: 746.
// Short name: SWEP304B
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
/// A program: SP_B304_GLOBAL_REASSIGNMENT.
/// </para>
/// <para>
/// RESP:  Service Plan
/// This batch procedure is used to process occurrences of Global_Reassignment, 
/// which reassign a type of assignable business object (such as Case or
/// Legal_Action) from one Office Service Provider to another Office Service
/// Provider.  Jack Rookard, MTW 02-12-1998
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB304GlobalReassignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B304_GLOBAL_REASSIGNMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB304GlobalReassignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB304GlobalReassignment.
  /// </summary>
  public SpB304GlobalReassignment(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // -------------------------------------------------------------------
    // Date	  Developer     Problem #       Description
    // -------------------------------------------------------------------
    // 02/02/98  J. Rookard - MTW	        Initial Development
    // -------------------------------------------------------------------
    // 09/30/99  SWSRCHF  	H00073391       Expanded the code to allow the
    //                                         
    // commit point to be table driven
    // -------------------------------------------------------------------
    // 11/09/99  SWSRCHF  	H00079318       Added 2 new Exit State's to the
    //                                         
    // Case of Command
    // -------------------------------------------------------------------
    // 11/09/99  SWSRCHF  	H00079038       Added 1 new Exit State to the
    //                                         
    // Case of Command.
    //                                         
    // On the OTHERWISE, use EAB to extract
    //                                         
    // the message from the ExitState
    // -------------------------------------------------------------------
    // 11/23/99  SWSRCHF  	H00080696       Moved the re-initialization of the
    //                                         
    // local counters to the end of the
    //                                         
    // 'READ EACH' loop
    // -------------------------------------------------------------------
    // 01/25/00  SWSRCHF  	H00082899       Reassign ALERT's and DMON's
    // -------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------------------------------
    // 06/20/2007	Raj S		PR 267661 	Modified to add process logic to re-assign 
    // only the
    //                                                 
    // interstate cases created through GBOR screen by
    //                                                 
    // selecting business object code as 'INC' (
    // interstate
    //                                                 
    // case).
    // ------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** Set the global variables.                             *****
    local.ProgramControlTotal.SystemGeneratedIdentifier = 0;
    local.ProgramProcessingInfo.Name = global.UserId;
    local.Current.Date = Now().Date;

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
    // ***** Get the run parameters for this program.              *****
    UseReadProgramProcessingInfo();

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
        "Error encountered opening the PPI table.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // To facilitate testing, the following logic sets the local current date 
    // work area date to either:
    // 1. If the processing info date is blank, set the local current date to 
    // the system current date.
    // 2. If the processing info date is max date (2099-12-31), set the local 
    // current date to the system current date.
    // 3. Otherwise, set the local current date to the processing info date.
    if (Equal(local.ProgramProcessingInfo.ProcessDate,
      local.InitializedDateWorkArea.Date))
    {
      local.Current.Date = Now().Date;
    }
    else if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Max.Date))
    {
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = local.ProgramProcessingInfo.ProcessDate;
    }

    // ***** Get the DB2 commit frequency counts.
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
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

    // *****************************************************************
    // *  Restart logic
    // 
    // *
    // *****************************************************************
    local.ProgramCheckpointRestart.CheckpointCount = 0;

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // *  Extract the last completed data              *
      local.RestrtOffce.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 4));
      local.RestrtSp.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 6, 5));
      local.RestrtGbor.BusinessObjectCode =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 12, 3);
      local.BatchTimestampWorkArea.TextTimestamp =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 16, 26);
      UseLeCabConvertTimestamp();
      local.ChkpntRestartKey.CreatedTimestamp =
        local.BatchTimestampWorkArea.IefTimestamp;
    }
    else
    {
      local.ChkpntRestartKey.CreatedTimestamp =
        local.InitializedGlobalReassignment.CreatedTimestamp;
    }

    // ***** Process the selected records in groups based upon the
    // commit frequencies.  Do a DB2 commit at the end of each
    // group.
    local.ChkpntNumbReads.Count = 0;
    local.ChkpntNumbUpdates.Count = 0;
    local.ChkpntNumbCreates.Count = 0;

    // ************************************************************
    // Begin Main Process Loop
    // ************************************************************
    foreach(var item in ReadGlobalReassignmentOfficeOfficeServiceProvider())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      local.PostRollback.CreatedTimestamp =
        entities.GlobalReassignment.CreatedTimestamp;
      local.Request.Flag = "Y";
      ++local.ChkpntNumbReads.Count;
      local.BatchTimestampWorkArea.IefTimestamp =
        entities.GlobalReassignment.CreatedTimestamp;
      local.BatchTimestampWorkArea.TextTimestamp = "";

      switch(TrimEnd(entities.GlobalReassignment.BusinessObjectCode))
      {
        case "CAS":
          // *** 01/25/2000 SWSRCHF
          // *** problem report H00082899
          // *** start
          // *** Re-assign ALERT's and DMON's
          UseSpCabB304ReassignBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          // *** end
          // *** 01/25/2000 SWSRCHF
          // *** problem report H00082899
          UseSpCabGlobalReassignCas();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          break;
        case "INC":
          // **************************************************************************************
          // Who: Raj When: 06/20/2007 Ref: PR 267661 New Interstate BOC(INC) 
          // changes  Start here
          // **************************************************************************************
          UseSpCabGlobalReassignIntCas();

          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // **************************************************************************************
          // Who: Raj When: 06/20/2007 Ref: PR 267661 New Interstate BOC(INC) 
          // changes  Ends here
          // **************************************************************************************
          break;
        case "LEA":
          UseSpCabGlobalReassignLea();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          break;
        case "LRF":
          UseSpCabGlobalReassignLrf();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          break;
        case "OBL":
          UseSpCabGlobalReassignObl();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          break;
        case "PAR":
          UseSpCabGlobalReassignPar();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          break;
        case "ADA":
          UseSpCabGlobalReassignAda();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          break;
        case "PYR":
          UseSpCabGlobalReassignPyr();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          break;
        default:
          local.EabFileHandling.Action = "WRITE";

          // **** Write error message out to the error file ****
          local.NeededToWrite.RptDetail =
            " is an invalid business object in Office:" + NumberToString
            (entities.ReassignFromOffice.SystemGeneratedId, 12, 4);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " for Service Provider: " +
            NumberToString
            (entities.ReassignFromOffice.SystemGeneratedId, 11, 5);
          local.NeededToWrite.RptDetail =
            entities.GlobalReassignment.BusinessObjectCode + local
            .NeededToWrite.RptDetail;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          try
          {
            UpdateGlobalReassignment1();

            continue;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "SP0000_GLOBAL_REASSIGNMENT_AE";

                continue;
              case ErrorCode.PermittedValueViolation:
                ExitState = "SP0000_GLOBAL_REASSIGNMENT_PV";

                continue;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.BatchTimestampWorkArea.IefTimestamp = Now();

        try
        {
          UpdateGlobalReassignment2();

          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
          // *** start
          // ***
          // *** do a final commit, as the processing will probably
          // *** be in-between commit points
          // ***
          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            // ***
            // *** A bad return has been received from the
            // *** external program performing the DB2 COMMIT
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while trying to do a DB2 commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          // *** end
          // *** 09/30/1999 SWSRCHF
          // *** problem report H00073391
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_GLOBAL_REASSIGNMENT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              // Not possible.  No permitted values on this entity.
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (IsExitState("SP0000_GLOBAL_REASSIGNMENT_AE"))
      {
        local.ProgramError.ProgramError1 = "GRAE";
        local.NeededToWrite.RptDetail =
          "Global Reassignment already exists.  Database error.";
      }
      else if (IsExitState("SP0000_GLOBAL_REASSIGNMENT_NU"))
      {
        local.ProgramError.ProgramError1 = "GRNU";
        local.NeededToWrite.RptDetail =
          "Global Reassignment not unique.  Database error.";
      }
      else if (IsExitState("SP0000_GLOBAL_REASSIGNMENT_NF"))
      {
        local.ProgramError.ProgramError1 = "GRNF";
        local.NeededToWrite.RptDetail =
          "Global Reassignment not found.  Database error.";
      }
      else if (IsExitState("OFFICE_SERVICE_PROVIDER_NF"))
      {
        local.ProgramError.ProgramError1 = "OSPNF";
        local.NeededToWrite.RptDetail = "Office Service Provider not found.";
      }
      else if (IsExitState("ADMIN_APPEAL_ASSIGNMENT_NU"))
      {
        // *** Problem report H00079318
        // *** 11/09/99 SWSRCHF
        // *** start
        local.ProgramError.ProgramError1 = "AAANU";
        local.NeededToWrite.RptDetail =
          "Administrative Appeal Aassignment not unique.  Database error.";

        // *** end
        // *** 11/09/99 SWSRCHF
        // *** Problem report H00079318
      }
      else if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
      {
        // *** Problem report H00079038
        // *** 11/09/99 SWSRCHF
        // *** start
        local.ProgramError.ProgramError1 = "PPCNF";
        local.NeededToWrite.RptDetail =
          "No Person Programs are currently active for this case";

        // *** end
        // *** 11/09/99 SWSRCHF
        // *** Problem report H00079038
      }
      else if (IsExitState("SP0000_INFRASTRUCTURE_NU"))
      {
        // *** Problem report H00082899
        // *** 01/25/00 SWSRCHF
        // *** start
        local.ProgramError.ProgramError1 = "INFNU";
        local.NeededToWrite.RptDetail = "Infrastructure not unique";

        // *** end
        // *** 01/25/00 SWSRCHF
        // *** Problem report H00082899
      }
      else if (IsExitState("SP0000_INFRASTRUCTURE_PV"))
      {
        // *** Problem report H00082899
        // *** 01/25/00 SWSRCHF
        // *** start
        local.ProgramError.ProgramError1 = "INFPV";
        local.NeededToWrite.RptDetail =
          "Infrastructure permitted value violation";

        // *** end
        // *** 01/25/00 SWSRCHF
        // *** Problem report H00082899
      }
      else if (IsExitState("SP0000_OSP_ALERT_AE"))
      {
        // *** Problem report H00082899
        // *** 01/25/00 SWSRCHF
        // *** start
        local.ProgramError.ProgramError1 = "OSPAAE";
        local.NeededToWrite.RptDetail =
          "Office Service Provider Alert already exists";

        // *** end
        // *** 01/25/00 SWSRCHF
        // *** Problem report H00082899
      }
      else if (IsExitState("SP0000_OSP_ALERT_PV"))
      {
        // *** Problem report H00082899
        // *** 01/25/00 SWSRCHF
        // *** start
        local.ProgramError.ProgramError1 = "OSPAPV";
        local.NeededToWrite.RptDetail =
          "Office Service Provider Alert permitted value violation";

        // *** end
        // *** 01/25/00 SWSRCHF
        // *** Problem report H00082899
      }
      else if (IsExitState("SP0000_OFFICE_SERVICE_PROVIDR_NF"))
      {
        // *** Problem report H00079318
        // *** 11/09/99 SWSRCHF
        // *** start
        local.ProgramError.ProgramError1 = "OSPNF";
        local.NeededToWrite.RptDetail = "Office Service Provider not found.";

        // *** end
        // *** 11/09/99 SWSRCHF
        // *** Problem report H00079318
      }
      else if (IsExitState("SERVICE_PROVIDER_NF"))
      {
        local.ProgramError.ProgramError1 = "SPNF";
        local.NeededToWrite.RptDetail = "Service Provider not found.";
      }
      else if (IsExitState("OFFICE_NF"))
      {
        local.ProgramError.ProgramError1 = "OFFNF";
        local.NeededToWrite.RptDetail = "Office not found. Database error.";
      }
      else if (IsExitState("CASE_ASSIGNMENT_NU"))
      {
        local.ProgramError.ProgramError1 = "CASANU";
        local.NeededToWrite.RptDetail =
          "Case Assignment not unique.  Database error.";
      }
      else if (IsExitState("SP0000_CASE_ASSIGNMENT_AE"))
      {
        local.ProgramError.ProgramError1 = "CASAAE";
        local.NeededToWrite.RptDetail =
          "Case Assignment already exists.  Database error.";
      }
      else if (IsExitState("CASE_UNIT_FUNCTION_ASSIGNMT_NU"))
      {
        local.ProgramError.ProgramError1 = "CASUFANU";
        local.NeededToWrite.RptDetail =
          "Case Unit Function Assgnmnt not unique.  Database error.";
      }
      else if (IsExitState("LEGAL_ACTION_ASSIGNMENT_NU"))
      {
        local.ProgramError.ProgramError1 = "LEAANU";
        local.NeededToWrite.RptDetail =
          "Legal Action Assignment already exists.  Database error.";
      }
      else if (IsExitState("SP0000_LEGAL_ACTION_ASSIGN_AE"))
      {
        local.ProgramError.ProgramError1 = "LEAAAE";
        local.NeededToWrite.RptDetail =
          "Legal Action Assignment already exists.  Database error.";
      }
      else if (IsExitState("LEGAL_REFERRAL_ASSIGNMENT_NU"))
      {
        local.ProgramError.ProgramError1 = "LRFNU";
        local.NeededToWrite.RptDetail =
          "Legal Referral Assignment already exists.  Database error.";
      }
      else if (IsExitState("OBLIGATION_ASSIGNMENT_NU"))
      {
        local.ProgramError.ProgramError1 = "OBLNU";
        local.NeededToWrite.RptDetail =
          "Obligation Assignment already exists.  Database error.";
      }
      else if (IsExitState("PA_REFERRAL_ASSIGNMENT_NU"))
      {
        local.ProgramError.ProgramError1 = "PARNU";
        local.NeededToWrite.RptDetail =
          "PA Referral Assignment already exists.  Database error.";
      }
      else if (IsExitState("SP0000_MONITORED_ACT_ASSGN_AE"))
      {
        local.ProgramError.ProgramError1 = "MAAAE";
        local.NeededToWrite.RptDetail =
          "Monitored Activity Assignment already exists.  Database error.";
      }
      else if (IsExitState("SP0000_MONITORED_ACT_ASSGN_NU"))
      {
        local.ProgramError.ProgramError1 = "MAANU";
        local.NeededToWrite.RptDetail =
          "Monitored Activity Assignment not unique.  Database error.";
      }
      else if (IsExitState("INFRASTRUCTURE_NF"))
      {
        local.ProgramError.ProgramError1 = "INFNF";
        local.NeededToWrite.RptDetail =
          "Infrastructure not found.  Database error.";
      }
      else if (IsExitState("SI0000_INTERSTAT_REQ_HIST_NU"))
      {
        local.ProgramError.ProgramError1 = "IRHNU";
        local.NeededToWrite.RptDetail =
          "Interstate request history is not unique. Database error.";
      }
      else if (IsExitState("SI0000_INTERSTAT_REQ_HIST_PV"))
      {
        local.ProgramError.ProgramError1 = "IRHPV";
        local.NeededToWrite.RptDetail =
          "Interstate request history permitted value violation. Database error.";
          
      }
      else if (IsExitState("SI0000_INTERSTATE_TRANS_ERROR_RB"))
      {
        local.ProgramError.ProgramError1 = "ITCNS";
        local.NeededToWrite.RptDetail =
          "Interstate transaction was not created successfully. Database error.";
          
      }
      else if (IsExitState("SI0000_INTERSTATE_CASE_AE_RB"))
      {
        local.ProgramError.ProgramError1 = "ICAE";
        local.NeededToWrite.RptDetail =
          "nterstate case already exists. Database error.";
      }
      else if (IsExitState("SI0000_INTERSTATE_CASE_PV_RB"))
      {
        local.ProgramError.ProgramError1 = "ICPV";
        local.NeededToWrite.RptDetail =
          "Interstate case permitted value violation. Database error.";
      }
      else if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        local.ProgramError.ProgramError1 = "OTRWSE";
        local.NeededToWrite.RptDetail = "Unplanned exitstate error.";

        // *** Problem report H00079038
        // *** 11/09/99 SWSRCHF
        // *** start
        UseEabExtractExitStateMessage();

        if (!IsEmpty(local.ExitStateWorkArea.Message))
        {
          local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
        }

        // *** end
        // *** 11/09/99 SWSRCHF
        // *** Problem report H00079038
      }

      if (!IsEmpty(local.ProgramError.ProgramError1))
      {
        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // ok, continue processing
        local.ProgramError.Code = "";
        local.ProgramError.ProgramError1 = "";
        local.ProgramError.KeyInfo = "";

        if (local.ChkpntNumbCreates.Count > 0 || local
          .ChkpntNumbUpdates.Count > 0)
        {
          UseEabRollbackSql();

          // *** Problem report H00079038
          // *** 11/09/99 SWSRCHF
        }

        // *** Problem report H00079038
        // *** 11/09/99 SWSRCHF
        // *** start
        if (AsChar(local.SoftError1.Flag) == 'Y')
        {
          for(local.SoftError.Index = 0; local.SoftError.Index < local
            .SoftError.Count; ++local.SoftError.Index)
          {
            if (!local.SoftError.CheckSize())
            {
              break;
            }

            // *****************************************************************
            // * Write a line to the ERROR RPT.
            // 
            // *
            // *****************************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              local.SoftError.Item.Detail.KeyInfo ?? Spaces(132);
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          local.SoftError.CheckIndex();

          for(local.SoftError.Index = 0; local.SoftError.Index < local
            .SoftError.Count; ++local.SoftError.Index)
          {
            if (!local.SoftError.CheckSize())
            {
              break;
            }

            local.SoftError.Update.Detail.Code = "";
            local.SoftError.Update.Detail.KeyInfo = "";
            local.SoftError.Update.Detail.ProgramError1 = "";
          }

          local.SoftError.CheckIndex();
          local.SoftError1.Flag = "N";
          local.ProgramError.Code = "";
          local.ProgramError.ProgramError1 = "";
          local.ProgramError.KeyInfo = "";
        }

        // *** end
        // *** 11/09/99 SWSRCHF
        // *** Problem report H00079038
        continue;
      }

      if (AsChar(local.SoftError1.Flag) == 'Y')
      {
        for(local.SoftError.Index = 0; local.SoftError.Index < local
          .SoftError.Count; ++local.SoftError.Index)
        {
          if (!local.SoftError.CheckSize())
          {
            break;
          }

          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            local.SoftError.Item.Detail.KeyInfo ?? Spaces(132);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        local.SoftError.CheckIndex();

        for(local.SoftError.Index = 0; local.SoftError.Index < local
          .SoftError.Count; ++local.SoftError.Index)
        {
          if (!local.SoftError.CheckSize())
          {
            break;
          }

          local.SoftError.Update.Detail.Code = "";
          local.SoftError.Update.Detail.KeyInfo = "";
          local.SoftError.Update.Detail.ProgramError1 = "";
        }

        local.SoftError.CheckIndex();
        local.SoftError1.Flag = "N";
        local.ProgramError.Code = "";
        local.ProgramError.ProgramError1 = "";
        local.ProgramError.KeyInfo = "";
      }

      // ************************************************************
      // Check number of creates and updates to determine if a commit
      // and checkpoint update is required.
      // ************************************************************
      if (local.ChkpntNumbCreates.Count > 0 || local.ChkpntNumbUpdates.Count > 0
        )
      {
        // ************************************************************
        // Call an external that does a DB2 commit using a Cobol
        // program.
        // ************************************************************
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered while trying to do a DB2 commit.";
          UseCabErrorReport2();
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        // ************************************************************
        // Write out the control totals to rpt98
        // ************************************************************
        local.Counter.Count = 0;
        local.EabFileHandling.Action = "WRITE";

        do
        {
          ++local.Counter.Count;

          switch(local.Counter.Count)
          {
            case 1:
              // ****  Setup the header ****
              local.NeededToWrite.RptDetail =
                "Frm_Offc Frm_OSP To_Offc To_OSP Bus_Obj";

              break;
            case 2:
              // ****  Setup the sub-header ****
              local.NeededToWrite.RptDetail =
                NumberToString(entities.ReassignFromOffice.SystemGeneratedId,
                12, 4) + "      " + NumberToString
                (entities.ReassignFromServiceProvider.SystemGeneratedId, 11, 5);
                
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "   " + NumberToString
                (entities.ReassignToOffice.SystemGeneratedId, 12, 4);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "    " + NumberToString
                (entities.ReassignToServiceProvider.SystemGeneratedId, 11, 5);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "  " + entities
                .GlobalReassignment.BusinessObjectCode;

              break;
            case 3:
              // ****  Blank Line ****
              local.NeededToWrite.RptDetail = "";

              break;
            case 4:
              // ****  Setup detail line 1 - Monitored Activities created ****
              local.NeededToWrite.RptDetail =
                "                  Total ReAssigned Monitored Activities = " + NumberToString
                (entities.GlobalReassignment.MonCount.GetValueOrDefault(), 10, 6);
                

              break;
            case 5:
              // ****  Setup detail line 2 - Bus-obj created ****
              local.NeededToWrite.RptDetail =
                "                      Total ReAssigned Business Objects = " + NumberToString
                (entities.GlobalReassignment.BoCount.GetValueOrDefault(), 10, 6);
                

              break;
            case 6:
              // ****  Setup detail line 3 - records created should = bus-Objs(
              // detail line 2) + MONA (detail line 1) ****
              local.NeededToWrite.RptDetail =
                "                                  Total Objects Created = " + NumberToString
                (local.ChkpntNumbCreates.Count, 10, 6);

              break;
            case 7:
              // ****  Setup detail line 4 - Records Updated should be the same 
              // as records created (detail line 3) ****
              local.NeededToWrite.RptDetail =
                "                       Total Business Objects End-Dated = " + NumberToString
                (local.ChkpntNumbUpdates.Count, 10, 6);

              break;
            case 8:
              // ****  Blank Line ****
              local.NeededToWrite.RptDetail = "";

              break;
            default:
              break;
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
        while(local.Counter.Count != 8);
      }

      // *** Problem report H00080696
      // *** 11/23/99 SWSRCHF
      // *** Re-initialize the Global Reassignment counters.
      local.BusObjCount.Count = 0;
      local.MonActCount.Count = 0;
      local.ChkpntNumbReads.Count = 0;
      local.ChkpntNumbUpdates.Count = 0;
      local.ChkpntNumbCreates.Count = 0;
    }

    // ************************************************************
    // CHECK FOR A NO REQUEST SITUATION
    // ************************************************************
    if (AsChar(local.Request.Flag) != 'Y')
    {
      // ************************************************************
      // Write out the control totals to rpt98
      // ************************************************************
      local.Counter.Count = 0;
      local.EabFileHandling.Action = "WRITE";

      do
      {
        ++local.Counter.Count;

        switch(local.Counter.Count)
        {
          case 1:
            // ****  WRITE BLANK LINE TO THE CONTROL FILE ****
            local.NeededToWrite.RptDetail = "";

            break;
          case 2:
            // ****  WRITE 'NO GBOR REQUESTS' MESSAGE TO CONTROL FILE  ****
            local.NeededToWrite.RptDetail =
              "   NO GBOR REASSIGNMENT REQUESTS FOUND";

            break;
          case 3:
            // ****  WRITE BLANK LINE TO THE CONTROL FILE ****
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            break;
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
      while(local.Counter.Count != 3);
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

    // *****************************************************************
    // * End of  Close of CONTROL RPT. DDNAME=RPT98                    *
    // * & ERROR RPT. DDNAME = RPT99.
    // 
    // *
    // *****************************************************************
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveError1(SpCabGlobalReassignIntCas.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveError2(SpCabGlobalReassignCas.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveError3(SpCabGlobalReassignPyr.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveError4(SpCabGlobalReassignAda.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveError5(SpCabGlobalReassignPar.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveError6(SpCabGlobalReassignObl.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveError7(SpCabGlobalReassignLrf.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveError8(SpCabGlobalReassignLea.Export.
    ErrorGroup source, Local.SoftErrorGroup target)
  {
    target.Detail.Assign(source.Detail);
  }

  private static void MoveGlobalReassignment1(GlobalReassignment source,
    GlobalReassignment target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ProcessDate = source.ProcessDate;
    target.StatusFlag = source.StatusFlag;
    target.OverrideFlag = source.OverrideFlag;
    target.BusinessObjectCode = source.BusinessObjectCode;
    target.AssignmentReasonCode = source.AssignmentReasonCode;
    target.BoCount = source.BoCount;
    target.MonCount = source.MonCount;
  }

  private static void MoveGlobalReassignment2(GlobalReassignment source,
    GlobalReassignment target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.OverrideFlag = source.OverrideFlag;
    target.AssignmentReasonCode = source.AssignmentReasonCode;
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

    useImport.DateWorkArea.Date = local.InitializedDateWorkArea.Date;

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

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
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

  private void UseSpCabB304ReassignBatch()
  {
    var useImport = new SpCabB304ReassignBatch.Import();
    var useExport = new SpCabB304ReassignBatch.Export();

    MoveGlobalReassignment2(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.MaxDate.Date = local.Max.Date;
    useImport.CurrentDate.Date = local.Current.Date;
    useImport.PersistentNewOffice.Assign(entities.ReassignToOffice);
    MoveOfficeServiceProvider(entities.ReassignToOfficeServiceProvider,
      useImport.New1);
    useImport.PersistentNewServiceProvider.Assign(
      entities.ReassignToServiceProvider);
    useImport.PersistentOld.Assign(entities.ReassignFromOfficeServiceProvider);
    useImport.Old.UserId = entities.ReassignFromServiceProvider.UserId;
    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.ChkpntNumbDeletes.Count = local.ChkpntNumbDeletes.Count;

    Call(SpCabB304ReassignBatch.Execute, useImport, useExport);

    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbDeletes.Count = useExport.ChkpntNumbDeletes.Count;
  }

  private void UseSpCabGlobalReassignAda()
  {
    var useImport = new SpCabGlobalReassignAda.Import();
    var useExport = new SpCabGlobalReassignAda.Export();

    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignAda.Execute, useImport, useExport);

    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError4);
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpCabGlobalReassignCas()
  {
    var useImport = new SpCabGlobalReassignCas.Import();
    var useExport = new SpCabGlobalReassignCas.Export();

    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignCas.Execute, useImport, useExport);

    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError2);
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpCabGlobalReassignIntCas()
  {
    var useImport = new SpCabGlobalReassignIntCas.Import();
    var useExport = new SpCabGlobalReassignIntCas.Export();

    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignIntCas.Execute, useImport, useExport);

    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError1);
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
  }

  private void UseSpCabGlobalReassignLea()
  {
    var useImport = new SpCabGlobalReassignLea.Import();
    var useExport = new SpCabGlobalReassignLea.Export();

    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignLea.Execute, useImport, useExport);

    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError8);
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpCabGlobalReassignLrf()
  {
    var useImport = new SpCabGlobalReassignLrf.Import();
    var useExport = new SpCabGlobalReassignLrf.Export();

    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignLrf.Execute, useImport, useExport);

    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError7);
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpCabGlobalReassignObl()
  {
    var useImport = new SpCabGlobalReassignObl.Import();
    var useExport = new SpCabGlobalReassignObl.Export();

    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignObl.Execute, useImport, useExport);

    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError6);
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpCabGlobalReassignPar()
  {
    var useImport = new SpCabGlobalReassignPar.Import();
    var useExport = new SpCabGlobalReassignPar.Export();

    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignPar.Execute, useImport, useExport);

    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError5);
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpCabGlobalReassignPyr()
  {
    var useImport = new SpCabGlobalReassignPyr.Import();
    var useExport = new SpCabGlobalReassignPyr.Export();

    useImport.ChkpntNumbUpdates.Count = local.ChkpntNumbUpdates.Count;
    useImport.ChkpntNumbCreates.Count = local.ChkpntNumbCreates.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.DateWorkArea.Date = local.Current.Date;
    MoveGlobalReassignment1(entities.GlobalReassignment,
      useImport.GlobalReassignment);
    useImport.ProgramCheckpointRestart.CheckpointCount =
      local.ProgramCheckpointRestart.CheckpointCount;

    Call(SpCabGlobalReassignPyr.Execute, useImport, useExport);

    local.ChkpntNumbCreates.Count = useExport.ChkpntNumbCreates.Count;
    local.ChkpntNumbUpdates.Count = useExport.ChkpntNumbUpdates.Count;
    local.SoftError1.Flag = useExport.SoftError.Flag;
    useExport.Error.CopyTo(local.SoftError, MoveError3);
    local.BusObjCount.Count = useExport.BusObjCount.Count;
    local.MonActCount.Count = useExport.MonActCount.Count;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private IEnumerable<bool> ReadGlobalReassignmentOfficeOfficeServiceProvider()
  {
    entities.ReassignToOffice.Populated = false;
    entities.ReassignToOfficeServiceProvider.Populated = false;
    entities.ReassignToServiceProvider.Populated = false;
    entities.ReassignFromOffice.Populated = false;
    entities.ReassignFromOfficeServiceProvider.Populated = false;
    entities.ReassignFromServiceProvider.Populated = false;
    entities.GlobalReassignment.Populated = false;

    return ReadEach("ReadGlobalReassignmentOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", local.RestrtOffce.SystemGeneratedId);
        db.SetString(
          command, "businessObjCode", local.RestrtGbor.BusinessObjectCode);
        db.SetDateTime(
          command, "createdTimestamp",
          local.ChkpntRestartKey.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "processDate", local.Current.Date.GetValueOrDefault());
        db.
          SetInt32(command, "servicePrvderId", local.RestrtSp.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.GlobalReassignment.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.GlobalReassignment.CreatedBy = db.GetString(reader, 1);
        entities.GlobalReassignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.GlobalReassignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.GlobalReassignment.ProcessDate = db.GetDate(reader, 4);
        entities.GlobalReassignment.StatusFlag = db.GetString(reader, 5);
        entities.GlobalReassignment.OverrideFlag = db.GetString(reader, 6);
        entities.GlobalReassignment.BusinessObjectCode =
          db.GetString(reader, 7);
        entities.GlobalReassignment.AssignmentReasonCode =
          db.GetString(reader, 8);
        entities.GlobalReassignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.ReassignToOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 9);
        entities.GlobalReassignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.ReassignToOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 10);
        entities.GlobalReassignment.OspRoleCode =
          db.GetNullableString(reader, 11);
        entities.ReassignToOfficeServiceProvider.RoleCode =
          db.GetString(reader, 11);
        entities.GlobalReassignment.OspEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.ReassignToOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 12);
        entities.GlobalReassignment.SpdGeneratedId1 =
          db.GetNullableInt32(reader, 13);
        entities.ReassignFromOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 13);
        entities.GlobalReassignment.OffGeneratedId1 =
          db.GetNullableInt32(reader, 14);
        entities.ReassignFromOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 14);
        entities.GlobalReassignment.OspRoleCod =
          db.GetNullableString(reader, 15);
        entities.ReassignFromOfficeServiceProvider.RoleCode =
          db.GetString(reader, 15);
        entities.GlobalReassignment.OspEffectiveDat =
          db.GetNullableDate(reader, 16);
        entities.ReassignFromOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 16);
        entities.GlobalReassignment.BoCount = db.GetNullableInt32(reader, 17);
        entities.GlobalReassignment.MonCount = db.GetNullableInt32(reader, 18);
        entities.ReassignFromOffice.SystemGeneratedId = db.GetInt32(reader, 19);
        entities.ReassignFromOffice.OffOffice = db.GetNullableInt32(reader, 20);
        entities.ReassignFromServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 21);
        entities.ReassignFromServiceProvider.UserId = db.GetString(reader, 22);
        entities.ReassignToOffice.SystemGeneratedId = db.GetInt32(reader, 23);
        entities.ReassignToOffice.OffOffice = db.GetNullableInt32(reader, 24);
        entities.ReassignToServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 25);
        entities.ReassignToServiceProvider.UserId = db.GetString(reader, 26);
        entities.ReassignToOffice.Populated = true;
        entities.ReassignToOfficeServiceProvider.Populated = true;
        entities.ReassignToServiceProvider.Populated = true;
        entities.ReassignFromOffice.Populated = true;
        entities.ReassignFromOfficeServiceProvider.Populated = true;
        entities.ReassignFromServiceProvider.Populated = true;
        entities.GlobalReassignment.Populated = true;

        return true;
      });
  }

  private void UpdateGlobalReassignment1()
  {
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();
    var statusFlag = "E";

    entities.GlobalReassignment.Populated = false;
    Update("UpdateGlobalReassignment1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "statusFlag", statusFlag);
        db.SetNullableInt32(command, "boCount", 0);
        db.SetNullableInt32(command, "monCount", 0);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.GlobalReassignment.CreatedTimestamp.GetValueOrDefault());
      });

    entities.GlobalReassignment.LastUpdatedBy = lastUpdatedBy;
    entities.GlobalReassignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.GlobalReassignment.StatusFlag = statusFlag;
    entities.GlobalReassignment.BoCount = 0;
    entities.GlobalReassignment.MonCount = 0;
    entities.GlobalReassignment.Populated = true;
  }

  private void UpdateGlobalReassignment2()
  {
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.BatchTimestampWorkArea.IefTimestamp;
    var statusFlag = "P";
    var boCount = local.BusObjCount.Count;
    var monCount = local.MonActCount.Count;

    entities.GlobalReassignment.Populated = false;
    Update("UpdateGlobalReassignment2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "statusFlag", statusFlag);
        db.SetNullableInt32(command, "boCount", boCount);
        db.SetNullableInt32(command, "monCount", monCount);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.GlobalReassignment.CreatedTimestamp.GetValueOrDefault());
      });

    entities.GlobalReassignment.LastUpdatedBy = lastUpdatedBy;
    entities.GlobalReassignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.GlobalReassignment.StatusFlag = statusFlag;
    entities.GlobalReassignment.BoCount = boCount;
    entities.GlobalReassignment.MonCount = monCount;
    entities.GlobalReassignment.Populated = true;
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
    /// <summary>A SoftErrorGroup group.</summary>
    [Serializable]
    public class SoftErrorGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ProgramError Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private ProgramError detail;
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
    /// A value of Request.
    /// </summary>
    [JsonPropertyName("request")]
    public Common Request
    {
      get => request ??= new();
      set => request = value;
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
    /// A value of RestrtGbor.
    /// </summary>
    [JsonPropertyName("restrtGbor")]
    public GlobalReassignment RestrtGbor
    {
      get => restrtGbor ??= new();
      set => restrtGbor = value;
    }

    /// <summary>
    /// A value of RestrtOffce.
    /// </summary>
    [JsonPropertyName("restrtOffce")]
    public Office RestrtOffce
    {
      get => restrtOffce ??= new();
      set => restrtOffce = value;
    }

    /// <summary>
    /// A value of RestrtSp.
    /// </summary>
    [JsonPropertyName("restrtSp")]
    public ServiceProvider RestrtSp
    {
      get => restrtSp ??= new();
      set => restrtSp = value;
    }

    /// <summary>
    /// A value of PostRollback.
    /// </summary>
    [JsonPropertyName("postRollback")]
    public GlobalReassignment PostRollback
    {
      get => postRollback ??= new();
      set => postRollback = value;
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
    /// A value of SoftError1.
    /// </summary>
    [JsonPropertyName("softError1")]
    public Common SoftError1
    {
      get => softError1 ??= new();
      set => softError1 = value;
    }

    /// <summary>
    /// Gets a value of SoftError.
    /// </summary>
    [JsonIgnore]
    public Array<SoftErrorGroup> SoftError => softError ??= new(
      SoftErrorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SoftError for json serialization.
    /// </summary>
    [JsonPropertyName("softError")]
    [Computed]
    public IList<SoftErrorGroup> SoftError_Json
    {
      get => softError;
      set => SoftError.Assign(value);
    }

    /// <summary>
    /// A value of RollbackPgmInd.
    /// </summary>
    [JsonPropertyName("rollbackPgmInd")]
    public Common RollbackPgmInd
    {
      get => rollbackPgmInd ??= new();
      set => rollbackPgmInd = value;
    }

    /// <summary>
    /// A value of MonActCount.
    /// </summary>
    [JsonPropertyName("monActCount")]
    public Common MonActCount
    {
      get => monActCount ??= new();
      set => monActCount = value;
    }

    /// <summary>
    /// A value of BusObjCount.
    /// </summary>
    [JsonPropertyName("busObjCount")]
    public Common BusObjCount
    {
      get => busObjCount ??= new();
      set => busObjCount = value;
    }

    /// <summary>
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
    }

    /// <summary>
    /// A value of InitializedGlobalReassignment.
    /// </summary>
    [JsonPropertyName("initializedGlobalReassignment")]
    public GlobalReassignment InitializedGlobalReassignment
    {
      get => initializedGlobalReassignment ??= new();
      set => initializedGlobalReassignment = value;
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
    /// A value of HoldOffice.
    /// </summary>
    [JsonPropertyName("holdOffice")]
    public WorkArea HoldOffice
    {
      get => holdOffice ??= new();
      set => holdOffice = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
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
    public GlobalReassignment ChkpntRestartKey
    {
      get => chkpntRestartKey ??= new();
      set => chkpntRestartKey = value;
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

    private ExitStateWorkArea exitStateWorkArea;
    private Common request;
    private Common counter;
    private GlobalReassignment restrtGbor;
    private Office restrtOffce;
    private ServiceProvider restrtSp;
    private GlobalReassignment postRollback;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common softError1;
    private Array<SoftErrorGroup> softError;
    private Common rollbackPgmInd;
    private Common monActCount;
    private Common busObjCount;
    private GlobalReassignment globalReassignment;
    private GlobalReassignment initializedGlobalReassignment;
    private DateWorkArea max;
    private WorkArea holdOffice;
    private DateWorkArea initializedDateWorkArea;
    private DateWorkArea current;
    private GlobalReassignment chkpntRestartKey;
    private External external;
    private Common chkpntNumbReads;
    private Common chkpntNumbUpdates;
    private Common chkpntNumbCreates;
    private Common chkpntNumbDeletes;
    private Common totalNumbReads;
    private Common totalNumbUpdates;
    private Common totalNumbCreates;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramError programError;
    private Common abortPgmInd;
    private ProgramControlTotal programControlTotal;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReassignToOffice.
    /// </summary>
    [JsonPropertyName("reassignToOffice")]
    public Office ReassignToOffice
    {
      get => reassignToOffice ??= new();
      set => reassignToOffice = value;
    }

    /// <summary>
    /// A value of ReassignToOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("reassignToOfficeServiceProvider")]
    public OfficeServiceProvider ReassignToOfficeServiceProvider
    {
      get => reassignToOfficeServiceProvider ??= new();
      set => reassignToOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ReassignToServiceProvider.
    /// </summary>
    [JsonPropertyName("reassignToServiceProvider")]
    public ServiceProvider ReassignToServiceProvider
    {
      get => reassignToServiceProvider ??= new();
      set => reassignToServiceProvider = value;
    }

    /// <summary>
    /// A value of ReassignFromOffice.
    /// </summary>
    [JsonPropertyName("reassignFromOffice")]
    public Office ReassignFromOffice
    {
      get => reassignFromOffice ??= new();
      set => reassignFromOffice = value;
    }

    /// <summary>
    /// A value of ReassignFromOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("reassignFromOfficeServiceProvider")]
    public OfficeServiceProvider ReassignFromOfficeServiceProvider
    {
      get => reassignFromOfficeServiceProvider ??= new();
      set => reassignFromOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ReassignFromServiceProvider.
    /// </summary>
    [JsonPropertyName("reassignFromServiceProvider")]
    public ServiceProvider ReassignFromServiceProvider
    {
      get => reassignFromServiceProvider ??= new();
      set => reassignFromServiceProvider = value;
    }

    /// <summary>
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
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

    private Office reassignToOffice;
    private OfficeServiceProvider reassignToOfficeServiceProvider;
    private ServiceProvider reassignToServiceProvider;
    private Office reassignFromOffice;
    private OfficeServiceProvider reassignFromOfficeServiceProvider;
    private ServiceProvider reassignFromServiceProvider;
    private GlobalReassignment globalReassignment;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

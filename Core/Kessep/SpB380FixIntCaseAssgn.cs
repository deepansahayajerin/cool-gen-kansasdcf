// Program: SP_B380_FIX_INT_CASE_ASSGN, ID: 371401290, model: 746.
// Short name: SWEP380B
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
/// A program: SP_B380_FIX_INT_CASE_ASSGN.
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
public partial class SpB380FixIntCaseAssgn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B380_FIX_INT_CASE_ASSGN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB380FixIntCaseAssgn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB380FixIntCaseAssgn.
  /// </summary>
  public SpB380FixIntCaseAssgn(IContext context, Import import, Export export):
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
    // 08/30/02  M. Lachowicz		Initial Development
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
      if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 10)))
      {
        ExitState = "ACO0000_INVALID_RESTART_PARM";

        return;
      }

      local.RunType.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 12, 1);

      if (AsChar(local.RunType.Flag) != 'U')
      {
        local.RunType.Flag = "R";
      }
    }
    else
    {
      return;
    }

    // Test note added....
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // To facilitate testing, the following logic sets the local current date 
    // work area date to either:
    // 1. If the processing info date is blank, set the local current date to 
    // the system current date.
    // 2. If the processing info date is max date (2099-12-31), set the local 
    // current date to the system current date.
    // 3. Otherwise, set the local current date to the program processing info 
    // date.
    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Initialized.Date))
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
    if (AsChar(local.RunType.Flag) == 'U')
    {
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      UseReadPgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ok, continue processing
        // **** Retrieve restart Case Number from restart table ****
        if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
        {
          local.ChkpntRestartKeyCase.Number =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
        }
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
    foreach(var item in ReadCaseCaseAssignment())
    {
      local.InterstateOfficeFlag.Flag = "";
      local.InterstateOverridFlag.Flag = "";
      local.ErrorFlag.Flag = "";
      local.ErrorMessage.Text30 = "";

      if (!ReadOfficeServiceProviderOfficeServiceProvider())
      {
        local.ErrorFlag.Flag = "Y";
        local.ErrorMessage.Text30 = "**No Valid Servicer Provider**";
        local.InterstateCaseText.Text12 = entities.Case1.Number;
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Case : " + entities.Case1.Number + ",      Case Number:" +
          local.InterstateCaseText.Text12 + ", Old Provider : " + "NONE    " + ", New Provider : " +
          "NONE    " + local.ErrorMessage.Text30;
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

        continue;
      }

      local.InterstateCase.KsCaseId = entities.Case1.Number;

      foreach(var item1 in ReadInterstateCaseInterstateCaseAssignment())
      {
        local.ErrorFlag.Flag = "";
        local.ErrorMessage.Text30 = "";
        local.InterstateCaseText.Text12 =
          NumberToString(entities.InterstateCase.TransSerialNumber, 4, 12);

        if (AsChar(entities.InterstateCaseAssignment.OverrideInd) == 'Y')
        {
          local.InterstateOverridFlag.Flag = "Y";
          local.ErrorFlag.Flag = "Y";
          local.ErrorMessage.Text30 = "**Override Flag has been set**";
        }

        if (ReadOfficeOfficeServiceProviderServiceProvider())
        {
          if (entities.InterstateCaseOffice.SystemGeneratedId == 21 || entities
            .InterstateCaseOffice.SystemGeneratedId == 45)
          {
            local.InterstateOfficeFlag.Flag = "Y";
            local.ErrorFlag.Flag = "Y";
            local.ErrorMessage.Text30 = "**Interstate Office Case**";
          }

          if (entities.InterstateCaseOffice.SystemGeneratedId == entities
            .CaseOffice.SystemGeneratedId && entities
            .InterstateCaseServiceProvider.SystemGeneratedId == entities
            .CaseServiceProvider.SystemGeneratedId)
          {
            continue;
          }

          ExitState = "ACO_NN0000_ALL_OK";

          if (AsChar(local.RunType.Flag) == 'U' && IsEmpty
            (local.ErrorFlag.Flag))
          {
            MoveInterstateCaseAssignment(entities.InterstateCaseAssignment,
              local.InterstateCaseAssignment);

            try
            {
              UpdateInterstateCaseAssignment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SI0000_IN_CASE_ASS_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "SP0000_ICASE_ASSGMNT_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            try
            {
              CreateInterstateCaseAssignment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SI0000_IN_CASE_ASS_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "SP0000_ICASE_ASSGMNT_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            local.ErrorMessage.Text30 = "**Interstate Case Reassigned**";

            if (local.Commit.Count >= local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                entities.Case1.Number;
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

              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                // *****************************************************************
                // * Write a line to the ERROR RPT.
                // 
                // *
                // *****************************************************************
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Error in External to do a commit for: " + NumberToString
                  (local.PassArea.NumericReturnCode, 1, 15);
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.Commit.Count = 0;
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ++local.ChkpntNumbCreates.Count;
              ++local.Commit.Count;
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail = "Case : " + entities
                .Case1.Number + ", Interstate Case :" + local
                .InterstateCaseText.Text12 + ", Old Provider : " + entities
                .InterstateCaseServiceProvider.UserId + ", New Provider : " + entities
                .CaseServiceProvider.UserId + "*** Eroor while Re-assigning ***";
                
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

              continue;
            }
          }
          else if (AsChar(local.ErrorFlag.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "Case : " + entities
              .Case1.Number + ", Interstate Case :" + local
              .InterstateCaseText.Text12 + ", Old Provider : " + entities
              .InterstateCaseServiceProvider.UserId + ", New Provider : " + entities
              .CaseServiceProvider.UserId + local.ErrorMessage.Text30;
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

            continue;
          }
          else
          {
            local.ErrorMessage.Text30 = "*Int. Case will be Reassigned*";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "Case : " + entities
              .Case1.Number + ", Interstate Case :" + local
              .InterstateCaseText.Text12 + ", Old Provider : " + entities
              .InterstateCaseServiceProvider.UserId + ", New Provider : " + entities
              .CaseServiceProvider.UserId + local.ErrorMessage.Text30;
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
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Program could not reassign provider for Case : " + entities
              .Case1.Number + ", Interstate Case :" + local
              .InterstateCaseText.Text12 + ", Old Provider : " + entities
              .InterstateCaseServiceProvider.UserId + ", New Provider : " + entities
              .CaseServiceProvider.UserId;
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

          // *****************************************************************
          // *Write control numbers to the CONTROL RPT. DDNAME=RPT98.        *
          // *****************************************************************
        }
        else
        {
          continue;
        }
      }
    }

    // ************************************************************
    // Set restart indicator to no because we have successfully
    // finished processing
    // ************************************************************
    if (AsChar(local.RunType.Flag) == 'U')
    {
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.CheckpointCount = 0;
      UseUpdatePgmCheckpointRestart();

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
          "Successful End of job, but error in update checkpoint restart.  Exitstate msg is: " +
          " ";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
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

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInterstateCaseAssignment(
    InterstateCaseAssignment source, InterstateCaseAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
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

    useImport.DateWorkArea.Date = local.Initialized.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void CreateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.CaseOfficeServiceProvider.Populated);

    var reasonCode = local.InterstateCaseAssignment.ReasonCode;
    var overrideInd = local.InterstateCaseAssignment.OverrideInd;
    var effectiveDate = local.CurrentPlus1.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTimestamp = Now();
    var spdId = entities.CaseOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.CaseOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.CaseOfficeServiceProvider.RoleCode;
    var ospDate = entities.CaseOfficeServiceProvider.EffectiveDate;
    var icsDate = entities.InterstateCase.TransactionDate;
    var icsNo = entities.InterstateCase.TransSerialNumber;

    entities.NewInterstateCaseAssignment.Populated = false;
    Update("CreateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetDate(command, "icsDate", icsDate);
        db.SetInt64(command, "icsNo", icsNo);
      });

    entities.NewInterstateCaseAssignment.ReasonCode = reasonCode;
    entities.NewInterstateCaseAssignment.OverrideInd = overrideInd;
    entities.NewInterstateCaseAssignment.EffectiveDate = effectiveDate;
    entities.NewInterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.NewInterstateCaseAssignment.CreatedBy = createdBy;
    entities.NewInterstateCaseAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewInterstateCaseAssignment.LastUpdatedBy = "";
    entities.NewInterstateCaseAssignment.LastUpdatedTimestamp = null;
    entities.NewInterstateCaseAssignment.SpdId = spdId;
    entities.NewInterstateCaseAssignment.OffId = offId;
    entities.NewInterstateCaseAssignment.OspCode = ospCode;
    entities.NewInterstateCaseAssignment.OspDate = ospDate;
    entities.NewInterstateCaseAssignment.IcsDate = icsDate;
    entities.NewInterstateCaseAssignment.IcsNo = icsNo;
    entities.NewInterstateCaseAssignment.Populated = true;
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", local.ChkpntRestartKeyCase.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 1);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseInterstateCaseAssignment()
  {
    entities.InterstateCase.Populated = false;
    entities.InterstateCaseAssignment.Populated = false;

    return ReadEach("ReadInterstateCaseInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ksCaseId", local.InterstateCase.KsCaseId ?? "");
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 1);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 2);
        entities.InterstateCaseAssignment.ReasonCode = db.GetString(reader, 3);
        entities.InterstateCaseAssignment.OverrideInd = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.InterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 9);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 10);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 11);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 12);
        entities.InterstateCase.Populated = true;
        entities.InterstateCaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);
    entities.InterstateCaseServiceProvider.Populated = false;
    entities.InterstateCaseOffice.Populated = false;
    entities.InterstateCaseOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.InterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.InterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.InterstateCaseAssignment.SpdId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCaseOffice.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateCaseOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateCaseOffice.OffOffice =
          db.GetNullableInt32(reader, 1);
        entities.InterstateCaseOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 2);
        entities.InterstateCaseServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 2);
        entities.InterstateCaseOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.InterstateCaseOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.InterstateCaseOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateCaseServiceProvider.UserId = db.GetString(reader, 6);
        entities.InterstateCaseServiceProvider.Populated = true;
        entities.InterstateCaseOffice.Populated = true;
        entities.InterstateCaseOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.CaseServiceProvider.Populated = false;
    entities.CaseOffice.Populated = false;
    entities.CaseOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.CaseServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.CaseOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.CaseOffice.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.CaseOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.CaseOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.CaseOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CaseOffice.OffOffice = db.GetNullableInt32(reader, 5);
        entities.CaseServiceProvider.UserId = db.GetString(reader, 6);
        entities.CaseServiceProvider.Populated = true;
        entities.CaseOffice.Populated = true;
        entities.CaseOfficeServiceProvider.Populated = true;
      });
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    entities.InterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.InterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.InterstateCaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.InterstateCaseAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.InterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.InterstateCaseAssignment.IcsDate.GetValueOrDefault());
        db.SetInt64(command, "icsNo", entities.InterstateCaseAssignment.IcsNo);
      });

    entities.InterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.InterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateCaseAssignment.Populated = true;
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
    /// <summary>
    /// A value of ChkpntRestartKeyCase.
    /// </summary>
    [JsonPropertyName("chkpntRestartKeyCase")]
    public Case1 ChkpntRestartKeyCase
    {
      get => chkpntRestartKeyCase ??= new();
      set => chkpntRestartKeyCase = value;
    }

    /// <summary>
    /// A value of InterstateCaseText.
    /// </summary>
    [JsonPropertyName("interstateCaseText")]
    public TextWorkArea InterstateCaseText
    {
      get => interstateCaseText ??= new();
      set => interstateCaseText = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of DisplayDate.
    /// </summary>
    [JsonPropertyName("displayDate")]
    public TextWorkArea DisplayDate
    {
      get => displayDate ??= new();
      set => displayDate = value;
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
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
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
    /// A value of ChkpntRestartKeyOffice.
    /// </summary>
    [JsonPropertyName("chkpntRestartKeyOffice")]
    public Office ChkpntRestartKeyOffice
    {
      get => chkpntRestartKeyOffice ??= new();
      set => chkpntRestartKeyOffice = value;
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
    /// A value of RunType.
    /// </summary>
    [JsonPropertyName("runType")]
    public Common RunType
    {
      get => runType ??= new();
      set => runType = value;
    }

    /// <summary>
    /// A value of ErrorFlag.
    /// </summary>
    [JsonPropertyName("errorFlag")]
    public Common ErrorFlag
    {
      get => errorFlag ??= new();
      set => errorFlag = value;
    }

    /// <summary>
    /// A value of InterstateOverridFlag.
    /// </summary>
    [JsonPropertyName("interstateOverridFlag")]
    public Common InterstateOverridFlag
    {
      get => interstateOverridFlag ??= new();
      set => interstateOverridFlag = value;
    }

    /// <summary>
    /// A value of InterstateOfficeFlag.
    /// </summary>
    [JsonPropertyName("interstateOfficeFlag")]
    public Common InterstateOfficeFlag
    {
      get => interstateOfficeFlag ??= new();
      set => interstateOfficeFlag = value;
    }

    /// <summary>
    /// A value of ErrorMessage.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public TextWorkArea ErrorMessage
    {
      get => errorMessage ??= new();
      set => errorMessage = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      chkpntRestartKeyCase = null;
      interstateCaseText = null;
      interstateCase = null;
      newServiceProvider = null;
      newOfficeServiceProvider = null;
      commit = null;
      screenIdentification = null;
      displayDate = null;
      counter = null;
      rptHeading = null;
      currentPlus1 = null;
      caseName = null;
      infrastructure = null;
      max = null;
      initialized = null;
      current = null;
      chkpntRestartKeyOffice = null;
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
      runType = null;
      errorFlag = null;
      interstateOverridFlag = null;
      interstateOfficeFlag = null;
      errorMessage = null;
      interstateCaseAssignment = null;
    }

    private Case1 chkpntRestartKeyCase;
    private TextWorkArea interstateCaseText;
    private InterstateCase interstateCase;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private Common commit;
    private Common screenIdentification;
    private TextWorkArea displayDate;
    private Case1 case1;
    private Common counter;
    private TextWorkArea rptHeading;
    private DateWorkArea currentPlus1;
    private TextWorkArea caseName;
    private Infrastructure infrastructure;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private DateWorkArea current;
    private Office chkpntRestartKeyOffice;
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
    private Common runType;
    private Common errorFlag;
    private Common interstateOverridFlag;
    private Common interstateOfficeFlag;
    private TextWorkArea errorMessage;
    private InterstateCaseAssignment interstateCaseAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CaseServiceProvider.
    /// </summary>
    [JsonPropertyName("caseServiceProvider")]
    public ServiceProvider CaseServiceProvider
    {
      get => caseServiceProvider ??= new();
      set => caseServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseOffice.
    /// </summary>
    [JsonPropertyName("caseOffice")]
    public Office CaseOffice
    {
      get => caseOffice ??= new();
      set => caseOffice = value;
    }

    /// <summary>
    /// A value of CaseOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("caseOfficeServiceProvider")]
    public OfficeServiceProvider CaseOfficeServiceProvider
    {
      get => caseOfficeServiceProvider ??= new();
      set => caseOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseServiceProvider")]
    public ServiceProvider InterstateCaseServiceProvider
    {
      get => interstateCaseServiceProvider ??= new();
      set => interstateCaseServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseOffice.
    /// </summary>
    [JsonPropertyName("interstateCaseOffice")]
    public Office InterstateCaseOffice
    {
      get => interstateCaseOffice ??= new();
      set => interstateCaseOffice = value;
    }

    /// <summary>
    /// A value of InterstateCaseOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseOfficeServiceProvider")]
    public OfficeServiceProvider InterstateCaseOfficeServiceProvider
    {
      get => interstateCaseOfficeServiceProvider ??= new();
      set => interstateCaseOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of NewInterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("newInterstateCaseAssignment")]
    public InterstateCaseAssignment NewInterstateCaseAssignment
    {
      get => newInterstateCaseAssignment ??= new();
      set => newInterstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    private InterstateCase interstateCase;
    private ServiceProvider caseServiceProvider;
    private Office caseOffice;
    private OfficeServiceProvider caseOfficeServiceProvider;
    private ServiceProvider interstateCaseServiceProvider;
    private Office interstateCaseOffice;
    private OfficeServiceProvider interstateCaseOfficeServiceProvider;
    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCaseAssignment newInterstateCaseAssignment;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private OfficeServiceProvider newOfficeServiceProvider;
  }
#endregion
}

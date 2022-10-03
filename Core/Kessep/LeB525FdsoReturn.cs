// Program: LE_B525_FDSO_RETURN, ID: 372668040, model: 746.
// Short name: SWEL525B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B525_FDSO_RETURN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB525FdsoReturn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B525_FDSO_RETURN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB525FdsoReturn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB525FdsoReturn.
  /// </summary>
  public LeB525FdsoReturn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************************************
    // ******                      M A I N T E N A N C E   L O G
    // ******
    // ************************************************************************************
    // *DATE		PR/WR		DEVELOPER		DESCRIPTION
    // ***********************************************************************************
    // 02/14/2002	PR138562	ESHIRK
    // Added restart and commit logic.  Cleaned up views / dead code etc.
    // 07/25/2007	PR313068	GVandy
    // Send transactions per case type (i.e. adc verses non-adc).
    ExitState = "ACO_NN0000_ALL_OK";

    // *********************************************************************
    // **  Retrieve PPI Information
    // *********************************************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // *********************************************************************
    // **  Retrieve Checkpoint Restart Information.
    // *********************************************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    // ***********************************************************************
    // **   Open Error Report
    // ***********************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.UserId;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // ***********************************************************************
    // **   Open Control Report.
    // ***********************************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // ***********************************************************************
    // **   Open Federal Reject File.
    // ***********************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabFileHandling.Status = "";
    UseEabFdsoReadReturnH();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error opening federal reject file.";
      UseCabErrorReport1();

      return;
    }

    // ***********************************************************************
    // **   Prime Read Federal Reject File.
    // ***********************************************************************
    local.EabFileHandling.Action = "READ";
    local.EabFileHandling.Status = "";
    UseEabFdsoReadReturnH();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error on the prime read of the federal reject file.";
      UseCabErrorReport1();

      return;
    }

    local.ReadCount.Count = 0;

    // ***********************************************************************
    // **  Check for Restart.  Read one record past last commit point and then 
    // proceed into main process loop.
    // ***********************************************************************
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.RejectMatch.Flag = "N";
      local.Restart.CaseNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.Restart.CaseType =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 1);
      local.Restart.TanfCode =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 12, 1);

      do
      {
        if (Equal(local.Restart.CaseNumber, export.FdsoReturnH.CaseNumber) && AsChar
          (local.Restart.CaseType) == AsChar
          (export.FdsoReturnH.TransactionType) && AsChar
          (local.Restart.TanfCode) == AsChar(export.FdsoReturnH.CaseTypeInd))
        {
          local.RejectMatch.Flag = "Y";
        }

        local.EabFileHandling.Action = "READ";
        local.EabFileHandling.Status = "";
        UseEabFdsoReadReturnH();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error on the restart read of the reject file.";
          UseCabErrorReport1();

          return;
        }
      }
      while(AsChar(local.RejectMatch.Flag) != 'Y');
    }

    // ***********************************************************************
    // **     M A I N   P R O C E S S   L O O P
    // ***********************************************************************
    do
    {
      // ***********************************************************************
      // **   Stamp error report with AP being processed.
      // ***********************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "";
      UseCabErrorReport2();
      local.NeededToWrite.RptDetail = "SSN - " + NumberToString
        (export.FdsoReturnH.Ssn, 7, 9) + "   CSP NUMBER - " + export
        .FdsoReturnH.CaseNumber;
      UseCabErrorReport2();
      local.Message.Errcode1 = "";

      // ***********************************************************************
      // **   Process reject records sent from the feds.
      // ***********************************************************************
      if (!Equal(export.FdsoReturnH.Errcode1, "00") && !
        IsEmpty(export.FdsoReturnH.Errcode1))
      {
        local.Message.Errcode1 = export.FdsoReturnH.Errcode1;
        UseCabFdsoReturnMessage();

        if (Equal(export.FdsoReturnH.Errcode1, "12"))
        {
          local.CsePerson.Number = export.FdsoReturnH.CaseNumber;

          foreach(var item in ReadFederalDebtSetoffCsePersonObligor())
          {
            ++local.ReadCount.Count;

            if (!IsEmpty(entities.ExistingFederalDebtSetoff.
              TtypeDDeleteCertification))
            {
              goto Test1;
            }
            else if (Equal(entities.ExistingFederalDebtSetoff.AdcAmount, 0) && Equal
              (entities.ExistingFederalDebtSetoff.NonAdcAmount, 0))
            {
              goto Test1;
            }

            // ----------------------------------------------
            // Create a new record with same information
            // and send it again as 'Add'
            // -----------------------------------------------
            ReadAdministrativeAction();

            try
            {
              CreateFederalDebtSetoff2();

              goto Test1;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  local.EabFileHandling.Action = "WRITE";
                  local.NeededToWrite.RptDetail =
                    "Error while creating new FDSO record, under Error Code 12,Already exists, CSE Person :  " +
                    export.FdsoReturnH.CaseNumber;
                  UseCabErrorReport2();

                  goto Test2;
                case ErrorCode.PermittedValueViolation:
                  local.EabFileHandling.Action = "WRITE";
                  local.NeededToWrite.RptDetail =
                    "Error while creating new FDSO record, under Error Code 12,Permitted Value violation, CSE Person :  " +
                    export.FdsoReturnH.CaseNumber;
                  UseCabErrorReport1();
                  ExitState = "ACO_NN0000_ABEND_4_BATCH";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

Test1:

        if (Equal(export.FdsoReturnH.Errcode1, "17"))
        {
          // --------------------------------------------------
          // According to new requirement, for last name
          // does not match, Fed will be sending first 4 bytes
          // of last name so set first 4 bytes of last name to this new field.
          // --------------------------------------------------
          local.CsePerson.Number = export.FdsoReturnH.CaseNumber;

          foreach(var item in ReadFederalDebtSetoffCsePersonObligor())
          {
            ++local.ReadCount.Count;

            if (!IsEmpty(export.FdsoReturnH.FedReturnedLastName))
            {
              // ----------------------------------------------
              // Create a new record with same information.
              // -----------------------------------------------
              ReadAdministrativeAction();

              // --------------------------------------------------
              // PR # 00120632
              // The Last Name in following CREATE statement
              // has been changed to set the exact last name send by Feds.
              // (4 bytes or less than 4 bytes, whatever send by Feds).
              // --------------------------------------------------
              try
              {
                CreateFederalDebtSetoff1();

                goto Test2;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Error while creating new FDSO record, under Error Code 17,Already exists, CSE Person :  " +
                      export.FdsoReturnH.CaseNumber;
                    UseCabErrorReport2();

                    goto Test2;
                  case ErrorCode.PermittedValueViolation:
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Error while creating new FDSO record, under Error Code 17,Permitted Value violation, CSE Person :  " +
                      export.FdsoReturnH.CaseNumber;
                    UseCabErrorReport1();
                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

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
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Last Name not provided by Fed for Error code 17, Person No : " +
                export.FdsoReturnH.CaseNumber;
              UseCabErrorReport2();

              goto Test2;
            }
          }
        }

        if (Equal(export.FdsoReturnH.Errcode1, "38"))
        {
          // --------------------------------------------------
          // If last record sent is 'D' delete, read next.
          // If last record sent is other than delete then send another Add.
          // --------------------------------------------------
          local.CsePerson.Number = export.FdsoReturnH.CaseNumber;

          foreach(var item in ReadFederalDebtSetoffCsePersonObligor())
          {
            ++local.ReadCount.Count;

            if (!IsEmpty(entities.ExistingFederalDebtSetoff.
              TtypeDDeleteCertification))
            {
              goto Test2;
            }
            else
            {
              if (Equal(entities.ExistingFederalDebtSetoff.AdcAmount, 0) && Equal
                (entities.ExistingFederalDebtSetoff.NonAdcAmount, 0))
              {
                goto Test2;
              }

              // ----------------------------------------------
              // Create a new record with same information
              // and send it again as 'Add'
              // -----------------------------------------------
              ReadAdministrativeAction();

              try
              {
                CreateFederalDebtSetoff2();

                goto Test2;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Error while creating new FDSO record, under Error Code 38,Already exists, CSE Person :  " +
                      export.FdsoReturnH.CaseNumber;
                    UseCabErrorReport2();

                    goto Test2;
                  case ErrorCode.PermittedValueViolation:
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Error while creating new FDSO record, under Error Code 38,Permitted Value violation, CSE Person :  " +
                      export.FdsoReturnH.CaseNumber;
                    UseCabErrorReport1();
                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
      }

Test2:

      if (!Equal(export.FdsoReturnH.Errcode2, "00") && !
        IsEmpty(export.FdsoReturnH.Errcode2))
      {
        local.Message.Errcode1 = export.FdsoReturnH.Errcode2;
        UseCabFdsoReturnMessage();
      }

      if (!Equal(export.FdsoReturnH.Errcode3, "00") && !
        IsEmpty(export.FdsoReturnH.Errcode3))
      {
        local.Message.Errcode1 = export.FdsoReturnH.Errcode3;
        UseCabFdsoReturnMessage();
      }

      if (!Equal(export.FdsoReturnH.Errcode4, "00") && !
        IsEmpty(export.FdsoReturnH.Errcode4))
      {
        local.Message.Errcode1 = export.FdsoReturnH.Errcode4;
        UseCabFdsoReturnMessage();
      }

      if (!Equal(export.FdsoReturnH.Errcode5, "00") && !
        IsEmpty(export.FdsoReturnH.Errcode5))
      {
        local.Message.Errcode1 = export.FdsoReturnH.Errcode5;
        UseCabFdsoReturnMessage();
      }

      if (!Equal(export.FdsoReturnH.Errcode6, "00") && !
        IsEmpty(export.FdsoReturnH.Errcode6))
      {
        local.Message.Errcode1 = export.FdsoReturnH.Errcode6;
        UseCabFdsoReturnMessage();
      }

      // ***********************************************************************
      // **   Check for commit point.
      // ***********************************************************************
      if (local.ReadCount.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        ExitState = "ACO_NN0000_ALL_OK";
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          export.FdsoReturnH.CaseNumber;
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(export.FdsoReturnH.CaseNumber,
          FdsoReturnH.CaseNumber_MaxLength, 1, 10) + export
          .FdsoReturnH.TransactionType + export.FdsoReturnH.CaseTypeInd;
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Update failed of checkpoint restart table.";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "Commit failed at AP number : " + export
            .FdsoReturnH.CaseNumber;
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.ReadCount.Count = 0;

        // ***********************************************************************
        // **   Stamp error report with commit point.
        // ***********************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Commit point performed at AP number : " + export
          .FdsoReturnH.CaseNumber;
        UseCabErrorReport2();
      }

      // ***********************************************************************
      // **   Read Federal Reject File.
      // ***********************************************************************
      local.EabFileHandling.Action = "READ";
      UseEabFdsoReadReturnH();

      if (Equal(export.FdsoReturnHTotal.Control, "CTL"))
      {
        break;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "C"));

    // ***********************************************************************
    // **   Final Commit Point.
    // ***********************************************************************
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Final commit failed at AP number : " + export
        .FdsoReturnH.CaseNumber;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***********************************************************************
    // **   Initialize program checkpoint restart information.
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramCheckpointRestart.RestartInd = "";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.CheckpointCount = -1;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Final update failed of checkpoint restart table.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***********************************************************************
    // **   Print control totals from reject file.
    // ***********************************************************************
    local.EabFileHandling.Action = "WRITE";
    export.FdsoReturnH.CaseNumber =
      NumberToString(export.FdsoReturnHTotal.TanfAccepted, 15);
    local.NeededToWrite.RptDetail = "TOTAL     TANF ACCEPTED - " + export
      .FdsoReturnH.CaseNumber;
    UseCabControlReport2();
    export.FdsoReturnH.CaseNumber =
      NumberToString(export.FdsoReturnHTotal.TanfRejected, 15);
    local.NeededToWrite.RptDetail = "TOTAL     TANF REJECTED - " + export
      .FdsoReturnH.CaseNumber;
    UseCabControlReport2();
    export.FdsoReturnH.CaseNumber =
      NumberToString(export.FdsoReturnHTotal.NontanfAccepted, 15);
    local.NeededToWrite.RptDetail = "TOTAL NON-TANF ACCEPTED - " + export
      .FdsoReturnH.CaseNumber;
    UseCabControlReport2();
    export.FdsoReturnH.CaseNumber =
      NumberToString(export.FdsoReturnHTotal.NontanfRejected, 15);
    local.NeededToWrite.RptDetail = "TOTAL NON-TANF REJECTED - " + export
      .FdsoReturnH.CaseNumber;
    UseCabControlReport2();

    // ***********************************************************************
    // **   Close error and control reports.
    // ***********************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    UseCabControlReport1();
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

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFdsoReturnMessage()
  {
    var useImport = new CabFdsoReturnMessage.Import();
    var useExport = new CabFdsoReturnMessage.Export();

    useImport.FdsoReturnH.Errcode1 = local.Message.Errcode1;

    Call(CabFdsoReturnMessage.Execute, useImport, useExport);

    local.NeededToWrite.RptDetail = useExport.EabReportSend.RptDetail;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabFdsoReadReturnH()
  {
    var useImport = new EabFdsoReadReturnH.Import();
    var useExport = new EabFdsoReadReturnH.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.FdsoReturnHTotal.Assign(export.FdsoReturnHTotal);
    useExport.FdsoReturnH.Assign(export.FdsoReturnH);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabFdsoReadReturnH.Execute, useImport, useExport);

    export.FdsoReturnHTotal.Assign(useExport.FdsoReturnHTotal);
    export.FdsoReturnH.Assign(useExport.FdsoReturnH);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
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

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void CreateFederalDebtSetoff1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligor.Populated);

    var cpaType = entities.ExistingObligor.Type1;
    var cspNumber = entities.ExistingObligor.CspNumber;
    var type1 = entities.ExistingFederalDebtSetoff.Type1;
    var takenDate = local.ProgramProcessingInfo.ProcessDate;
    var aatType = entities.ExistingAdministrativeAction.Type1;
    var originalAmount = entities.ExistingFederalDebtSetoff.OriginalAmount;
    var currentAmount = entities.ExistingFederalDebtSetoff.CurrentAmount;
    var currentAmountDate =
      entities.ExistingFederalDebtSetoff.CurrentAmountDate;
    var createdBy = global.TranCode;
    var createdTstamp = Now();
    var adcAmount = entities.ExistingFederalDebtSetoff.AdcAmount;
    var nonAdcAmount = entities.ExistingFederalDebtSetoff.NonAdcAmount;
    var dateSent = local.Null1.ProcessDate;
    var etypeAdministrativeOffset =
      entities.ExistingFederalDebtSetoff.EtypeAdministrativeOffset;
    var localCode = entities.ExistingFederalDebtSetoff.LocalCode;
    var ssn = entities.ExistingFederalDebtSetoff.Ssn;
    var caseNumber = entities.ExistingFederalDebtSetoff.CaseNumber;
    var lastName = export.FdsoReturnH.FedReturnedLastName;
    var firstName = entities.ExistingFederalDebtSetoff.FirstName;
    var amountOwed = entities.ExistingFederalDebtSetoff.AmountOwed;
    var ttypeAAddNewCase = entities.ExistingFederalDebtSetoff.TtypeAAddNewCase;
    var caseType = entities.ExistingFederalDebtSetoff.CaseType;
    var processYear = entities.ExistingFederalDebtSetoff.ProcessYear;
    var tanfCode = entities.ExistingFederalDebtSetoff.TanfCode;
    var ttypeDDeleteCertification =
      entities.ExistingFederalDebtSetoff.TtypeDDeleteCertification;
    var ttypeLChangeSubmittingState =
      entities.ExistingFederalDebtSetoff.TtypeLChangeSubmittingState;
    var ttypeMModifyAmount =
      entities.ExistingFederalDebtSetoff.TtypeMModifyAmount;
    var ttypeRModifyExclusion =
      entities.ExistingFederalDebtSetoff.TtypeRModifyExclusion;
    var ttypeBNameChange = entities.ExistingFederalDebtSetoff.TtypeBNameChange;
    var ttypeZAddressChange =
      entities.ExistingFederalDebtSetoff.TtypeZAddressChange;
    var etypeFederalRetirement =
      entities.ExistingFederalDebtSetoff.EtypeFederalRetirement;
    var etypeFederalSalary =
      entities.ExistingFederalDebtSetoff.EtypeFederalSalary;
    var etypeTaxRefund = entities.ExistingFederalDebtSetoff.EtypeTaxRefund;
    var etypeVendorPaymentOrMisc =
      entities.ExistingFederalDebtSetoff.EtypeVendorPaymentOrMisc;
    var etypePassportDenial =
      entities.ExistingFederalDebtSetoff.EtypePassportDenial;
    var etypeFinancialInstitution =
      entities.ExistingFederalDebtSetoff.EtypeFinancialInstitution;
    var etypeAdmBankrupt = entities.ExistingFederalDebtSetoff.EtypeAdmBankrupt;
    var addressStreet1 = entities.ExistingFederalDebtSetoff.AddressStreet1;
    var addressStreet2 = entities.ExistingFederalDebtSetoff.AddressStreet2;
    var addressCity = entities.ExistingFederalDebtSetoff.AddressCity;
    var addressState = entities.ExistingFederalDebtSetoff.AddressState;
    var addressZip = entities.ExistingFederalDebtSetoff.AddressZip;

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.New1.Populated = false;
    Update("CreateFederalDebtSetoff1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableString(command, "aatType", aatType);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableDate(command, "currentAmtDt", currentAmountDate);
        db.SetNullableDate(command, "decertifiedDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableDecimal(command, "recoveryAmt", 0M);
        db.SetNullableDecimal(command, "adcAmt", adcAmount);
        db.SetNullableDecimal(command, "nonAdcAmt", nonAdcAmount);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", "");
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetNullableString(
          command, "etypeAdminOffset", etypeAdministrativeOffset);
        db.SetNullableString(command, "localCode", localCode);
        db.SetInt32(command, "ssn", ssn);
        db.SetString(command, "caseNumber", caseNumber);
        db.SetString(command, "lastName", lastName);
        db.SetString(command, "firstName", firstName);
        db.SetInt32(command, "amountOwed", amountOwed);
        db.SetNullableString(command, "ttypeAddNewCase", ttypeAAddNewCase);
        db.SetString(command, "caseType", caseType);
        db.SetNullableString(command, "transferState", "");
        db.SetNullableInt32(command, "localForTransfer", 0);
        db.SetNullableInt32(command, "processYear", processYear);
        db.SetString(command, "tanfCode", tanfCode);
        db.SetNullableString(
          command, "ttypeDeleteCert", ttypeDDeleteCertification);
        db.SetNullableString(
          command, "ttypeChngSubSt", ttypeLChangeSubmittingState);
        db.SetNullableString(command, "ttypeModifyAmnt", ttypeMModifyAmount);
        db.SetNullableString(command, "ttypeModifyExcl", ttypeRModifyExclusion);
        db.SetNullableString(command, "ttypeStatePymnt", "");
        db.SetNullableString(command, "ttypeNameChange", ttypeBNameChange);
        db.SetNullableString(command, "ttypeAddressChg", ttypeZAddressChange);
        db.
          SetNullableString(command, "etypeFedRetrmnt", etypeFederalRetirement);
          
        db.SetNullableString(command, "etypeFedSalary", etypeFederalSalary);
        db.SetNullableString(command, "etypeTaxRefund", etypeTaxRefund);
        db.SetNullableString(
          command, "etypeVndrPymntM", etypeVendorPaymentOrMisc);
        db.SetNullableString(command, "etypePsprtDenial", etypePassportDenial);
        db.
          SetNullableString(command, "etypeFinInst", etypeFinancialInstitution);
          
        db.SetNullableString(command, "returnStatus", "");
        db.SetNullableDate(command, "returnStatusDate", dateSent);
        db.SetNullableString(command, "etypeAdmBankrupt", etypeAdmBankrupt);
        db.SetNullableString(command, "decertifyReason", "");
        db.SetNullableString(command, "addressStreet1", addressStreet1);
        db.SetNullableString(command, "addressStreet2", addressStreet2);
        db.SetNullableString(command, "addressCity", addressCity);
        db.SetNullableString(command, "addressState", addressState);
        db.SetNullableString(command, "addressZip", addressZip);
      });

    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = cspNumber;
    entities.New1.Type1 = type1;
    entities.New1.TakenDate = takenDate;
    entities.New1.AatType = aatType;
    entities.New1.OriginalAmount = originalAmount;
    entities.New1.CurrentAmount = currentAmount;
    entities.New1.CurrentAmountDate = currentAmountDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTstamp = createdTstamp;
    entities.New1.AdcAmount = adcAmount;
    entities.New1.NonAdcAmount = nonAdcAmount;
    entities.New1.DateSent = dateSent;
    entities.New1.EtypeAdministrativeOffset = etypeAdministrativeOffset;
    entities.New1.LocalCode = localCode;
    entities.New1.Ssn = ssn;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.LastName = lastName;
    entities.New1.FirstName = firstName;
    entities.New1.AmountOwed = amountOwed;
    entities.New1.TtypeAAddNewCase = ttypeAAddNewCase;
    entities.New1.CaseType = caseType;
    entities.New1.ProcessYear = processYear;
    entities.New1.TanfCode = tanfCode;
    entities.New1.TtypeDDeleteCertification = ttypeDDeleteCertification;
    entities.New1.TtypeLChangeSubmittingState = ttypeLChangeSubmittingState;
    entities.New1.TtypeMModifyAmount = ttypeMModifyAmount;
    entities.New1.TtypeRModifyExclusion = ttypeRModifyExclusion;
    entities.New1.TtypeBNameChange = ttypeBNameChange;
    entities.New1.TtypeZAddressChange = ttypeZAddressChange;
    entities.New1.EtypeFederalRetirement = etypeFederalRetirement;
    entities.New1.EtypeFederalSalary = etypeFederalSalary;
    entities.New1.EtypeTaxRefund = etypeTaxRefund;
    entities.New1.EtypeVendorPaymentOrMisc = etypeVendorPaymentOrMisc;
    entities.New1.EtypePassportDenial = etypePassportDenial;
    entities.New1.EtypeFinancialInstitution = etypeFinancialInstitution;
    entities.New1.ReturnStatus = "";
    entities.New1.ReturnStatusDate = dateSent;
    entities.New1.EtypeAdmBankrupt = etypeAdmBankrupt;
    entities.New1.AddressStreet1 = addressStreet1;
    entities.New1.AddressStreet2 = addressStreet2;
    entities.New1.AddressCity = addressCity;
    entities.New1.AddressState = addressState;
    entities.New1.AddressZip = addressZip;
    entities.New1.Populated = true;
  }

  private void CreateFederalDebtSetoff2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligor.Populated);

    var cpaType = entities.ExistingObligor.Type1;
    var cspNumber = entities.ExistingObligor.CspNumber;
    var type1 = entities.ExistingFederalDebtSetoff.Type1;
    var takenDate = local.ProgramProcessingInfo.ProcessDate;
    var aatType = entities.ExistingAdministrativeAction.Type1;
    var originalAmount = entities.ExistingFederalDebtSetoff.OriginalAmount;
    var currentAmount = entities.ExistingFederalDebtSetoff.CurrentAmount;
    var currentAmountDate =
      entities.ExistingFederalDebtSetoff.CurrentAmountDate;
    var createdBy = global.TranCode;
    var createdTstamp = Now();
    var adcAmount = entities.ExistingFederalDebtSetoff.AdcAmount;
    var nonAdcAmount = entities.ExistingFederalDebtSetoff.NonAdcAmount;
    var dateSent = local.Null1.ProcessDate;
    var etypeAdministrativeOffset =
      entities.ExistingFederalDebtSetoff.EtypeAdministrativeOffset;
    var localCode = entities.ExistingFederalDebtSetoff.LocalCode;
    var ssn = entities.ExistingFederalDebtSetoff.Ssn;
    var caseNumber = entities.ExistingFederalDebtSetoff.CaseNumber;
    var lastName = entities.ExistingFederalDebtSetoff.LastName;
    var firstName = entities.ExistingFederalDebtSetoff.FirstName;
    var amountOwed = entities.ExistingFederalDebtSetoff.AmountOwed;
    var ttypeAAddNewCase = export.FdsoReturnH.CaseTypeInd;
    var caseType = entities.ExistingFederalDebtSetoff.CaseType;
    var processYear = entities.ExistingFederalDebtSetoff.ProcessYear;
    var tanfCode = entities.ExistingFederalDebtSetoff.TanfCode;
    var etypeFederalRetirement =
      entities.ExistingFederalDebtSetoff.EtypeFederalRetirement;
    var etypeFederalSalary =
      entities.ExistingFederalDebtSetoff.EtypeFederalSalary;
    var etypeTaxRefund = entities.ExistingFederalDebtSetoff.EtypeTaxRefund;
    var etypeVendorPaymentOrMisc =
      entities.ExistingFederalDebtSetoff.EtypeVendorPaymentOrMisc;
    var etypePassportDenial =
      entities.ExistingFederalDebtSetoff.EtypePassportDenial;
    var etypeFinancialInstitution =
      entities.ExistingFederalDebtSetoff.EtypeFinancialInstitution;
    var etypeAdmBankrupt = entities.ExistingFederalDebtSetoff.EtypeAdmBankrupt;
    var addressStreet1 = entities.ExistingFederalDebtSetoff.AddressStreet1;
    var addressStreet2 = entities.ExistingFederalDebtSetoff.AddressStreet2;
    var addressCity = entities.ExistingFederalDebtSetoff.AddressCity;
    var addressState = entities.ExistingFederalDebtSetoff.AddressState;
    var addressZip = entities.ExistingFederalDebtSetoff.AddressZip;

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.New1.Populated = false;
    Update("CreateFederalDebtSetoff2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableString(command, "aatType", aatType);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableDate(command, "currentAmtDt", currentAmountDate);
        db.SetNullableDate(command, "decertifiedDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableDecimal(command, "recoveryAmt", 0M);
        db.SetNullableDecimal(command, "adcAmt", adcAmount);
        db.SetNullableDecimal(command, "nonAdcAmt", nonAdcAmount);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", "");
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetNullableString(
          command, "etypeAdminOffset", etypeAdministrativeOffset);
        db.SetNullableString(command, "localCode", localCode);
        db.SetInt32(command, "ssn", ssn);
        db.SetString(command, "caseNumber", caseNumber);
        db.SetString(command, "lastName", lastName);
        db.SetString(command, "firstName", firstName);
        db.SetInt32(command, "amountOwed", amountOwed);
        db.SetNullableString(command, "ttypeAddNewCase", ttypeAAddNewCase);
        db.SetString(command, "caseType", caseType);
        db.SetNullableString(command, "transferState", "");
        db.SetNullableInt32(command, "localForTransfer", 0);
        db.SetNullableInt32(command, "processYear", processYear);
        db.SetString(command, "tanfCode", tanfCode);
        db.SetNullableString(command, "ttypeDeleteCert", "");
        db.SetNullableString(command, "ttypeChngSubSt", "");
        db.SetNullableString(command, "ttypeModifyAmnt", "");
        db.SetNullableString(command, "ttypeModifyExcl", "");
        db.SetNullableString(command, "ttypeStatePymnt", "");
        db.SetNullableString(command, "ttypeNameChange", "");
        db.SetNullableString(command, "ttypeAddressChg", "");
        db.
          SetNullableString(command, "etypeFedRetrmnt", etypeFederalRetirement);
          
        db.SetNullableString(command, "etypeFedSalary", etypeFederalSalary);
        db.SetNullableString(command, "etypeTaxRefund", etypeTaxRefund);
        db.SetNullableString(
          command, "etypeVndrPymntM", etypeVendorPaymentOrMisc);
        db.SetNullableString(command, "etypePsprtDenial", etypePassportDenial);
        db.
          SetNullableString(command, "etypeFinInst", etypeFinancialInstitution);
          
        db.SetNullableString(command, "returnStatus", "");
        db.SetNullableDate(command, "returnStatusDate", dateSent);
        db.SetNullableString(command, "etypeAdmBankrupt", etypeAdmBankrupt);
        db.SetNullableString(command, "decertifyReason", "");
        db.SetNullableString(command, "addressStreet1", addressStreet1);
        db.SetNullableString(command, "addressStreet2", addressStreet2);
        db.SetNullableString(command, "addressCity", addressCity);
        db.SetNullableString(command, "addressState", addressState);
        db.SetNullableString(command, "addressZip", addressZip);
      });

    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = cspNumber;
    entities.New1.Type1 = type1;
    entities.New1.TakenDate = takenDate;
    entities.New1.AatType = aatType;
    entities.New1.OriginalAmount = originalAmount;
    entities.New1.CurrentAmount = currentAmount;
    entities.New1.CurrentAmountDate = currentAmountDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTstamp = createdTstamp;
    entities.New1.AdcAmount = adcAmount;
    entities.New1.NonAdcAmount = nonAdcAmount;
    entities.New1.DateSent = dateSent;
    entities.New1.EtypeAdministrativeOffset = etypeAdministrativeOffset;
    entities.New1.LocalCode = localCode;
    entities.New1.Ssn = ssn;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.LastName = lastName;
    entities.New1.FirstName = firstName;
    entities.New1.AmountOwed = amountOwed;
    entities.New1.TtypeAAddNewCase = ttypeAAddNewCase;
    entities.New1.CaseType = caseType;
    entities.New1.ProcessYear = processYear;
    entities.New1.TanfCode = tanfCode;
    entities.New1.TtypeDDeleteCertification = "";
    entities.New1.TtypeLChangeSubmittingState = "";
    entities.New1.TtypeMModifyAmount = "";
    entities.New1.TtypeRModifyExclusion = "";
    entities.New1.TtypeBNameChange = "";
    entities.New1.TtypeZAddressChange = "";
    entities.New1.EtypeFederalRetirement = etypeFederalRetirement;
    entities.New1.EtypeFederalSalary = etypeFederalSalary;
    entities.New1.EtypeTaxRefund = etypeTaxRefund;
    entities.New1.EtypeVendorPaymentOrMisc = etypeVendorPaymentOrMisc;
    entities.New1.EtypePassportDenial = etypePassportDenial;
    entities.New1.EtypeFinancialInstitution = etypeFinancialInstitution;
    entities.New1.ReturnStatus = "";
    entities.New1.ReturnStatusDate = dateSent;
    entities.New1.EtypeAdmBankrupt = etypeAdmBankrupt;
    entities.New1.AddressStreet1 = addressStreet1;
    entities.New1.AddressStreet2 = addressStreet2;
    entities.New1.AddressCity = addressCity;
    entities.New1.AddressState = addressState;
    entities.New1.AddressZip = addressZip;
    entities.New1.Populated = true;
  }

  private bool ReadAdministrativeAction()
  {
    entities.ExistingAdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      null,
      (db, reader) =>
      {
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFederalDebtSetoffCsePersonObligor()
  {
    entities.ExistingObligor.Populated = false;
    entities.ExistingCsePerson.Populated = false;
    entities.ExistingFederalDebtSetoff.Populated = false;

    return ReadEach("ReadFederalDebtSetoffCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePerson.Number);
        db.SetString(command, "caseNumber", export.FdsoReturnH.CaseNumber);
        db.SetInt32(command, "ssn", export.FdsoReturnH.Ssn);
      },
      (db, reader) =>
      {
        entities.ExistingFederalDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.ExistingObligor.Type1 = db.GetString(reader, 0);
        entities.ExistingFederalDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.ExistingCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingObligor.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligor.CspNumber = db.GetString(reader, 1);
        entities.ExistingFederalDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.ExistingFederalDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.ExistingFederalDebtSetoff.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingFederalDebtSetoff.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingFederalDebtSetoff.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingFederalDebtSetoff.CreatedBy = db.GetString(reader, 7);
        entities.ExistingFederalDebtSetoff.CreatedTstamp =
          db.GetDateTime(reader, 8);
        entities.ExistingFederalDebtSetoff.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ExistingFederalDebtSetoff.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.ExistingFederalDebtSetoff.AdcAmount =
          db.GetNullableDecimal(reader, 11);
        entities.ExistingFederalDebtSetoff.NonAdcAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingFederalDebtSetoff.DateSent =
          db.GetNullableDate(reader, 13);
        entities.ExistingFederalDebtSetoff.EtypeAdministrativeOffset =
          db.GetNullableString(reader, 14);
        entities.ExistingFederalDebtSetoff.LocalCode =
          db.GetNullableString(reader, 15);
        entities.ExistingFederalDebtSetoff.Ssn = db.GetInt32(reader, 16);
        entities.ExistingFederalDebtSetoff.CaseNumber =
          db.GetString(reader, 17);
        entities.ExistingFederalDebtSetoff.LastName = db.GetString(reader, 18);
        entities.ExistingFederalDebtSetoff.FirstName = db.GetString(reader, 19);
        entities.ExistingFederalDebtSetoff.AmountOwed = db.GetInt32(reader, 20);
        entities.ExistingFederalDebtSetoff.TtypeAAddNewCase =
          db.GetNullableString(reader, 21);
        entities.ExistingFederalDebtSetoff.CaseType = db.GetString(reader, 22);
        entities.ExistingFederalDebtSetoff.ProcessYear =
          db.GetNullableInt32(reader, 23);
        entities.ExistingFederalDebtSetoff.TanfCode = db.GetString(reader, 24);
        entities.ExistingFederalDebtSetoff.TtypeDDeleteCertification =
          db.GetNullableString(reader, 25);
        entities.ExistingFederalDebtSetoff.TtypeLChangeSubmittingState =
          db.GetNullableString(reader, 26);
        entities.ExistingFederalDebtSetoff.TtypeMModifyAmount =
          db.GetNullableString(reader, 27);
        entities.ExistingFederalDebtSetoff.TtypeRModifyExclusion =
          db.GetNullableString(reader, 28);
        entities.ExistingFederalDebtSetoff.TtypeBNameChange =
          db.GetNullableString(reader, 29);
        entities.ExistingFederalDebtSetoff.TtypeZAddressChange =
          db.GetNullableString(reader, 30);
        entities.ExistingFederalDebtSetoff.EtypeFederalRetirement =
          db.GetNullableString(reader, 31);
        entities.ExistingFederalDebtSetoff.EtypeFederalSalary =
          db.GetNullableString(reader, 32);
        entities.ExistingFederalDebtSetoff.EtypeTaxRefund =
          db.GetNullableString(reader, 33);
        entities.ExistingFederalDebtSetoff.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 34);
        entities.ExistingFederalDebtSetoff.EtypePassportDenial =
          db.GetNullableString(reader, 35);
        entities.ExistingFederalDebtSetoff.EtypeFinancialInstitution =
          db.GetNullableString(reader, 36);
        entities.ExistingFederalDebtSetoff.ReturnStatus =
          db.GetNullableString(reader, 37);
        entities.ExistingFederalDebtSetoff.ReturnStatusDate =
          db.GetNullableDate(reader, 38);
        entities.ExistingFederalDebtSetoff.EtypeAdmBankrupt =
          db.GetNullableString(reader, 39);
        entities.ExistingFederalDebtSetoff.AddressStreet1 =
          db.GetNullableString(reader, 40);
        entities.ExistingFederalDebtSetoff.AddressStreet2 =
          db.GetNullableString(reader, 41);
        entities.ExistingFederalDebtSetoff.AddressCity =
          db.GetNullableString(reader, 42);
        entities.ExistingFederalDebtSetoff.AddressState =
          db.GetNullableString(reader, 43);
        entities.ExistingFederalDebtSetoff.AddressZip =
          db.GetNullableString(reader, 44);
        entities.ExistingObligor.Populated = true;
        entities.ExistingCsePerson.Populated = true;
        entities.ExistingFederalDebtSetoff.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingFederalDebtSetoff.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.ExistingObligor.Type1);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingFederalDebtSetoff.Type1);

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
    /// <summary>
    /// A value of FdsoReturnHTotal.
    /// </summary>
    [JsonPropertyName("fdsoReturnHTotal")]
    public FdsoReturnHTotal FdsoReturnHTotal
    {
      get => fdsoReturnHTotal ??= new();
      set => fdsoReturnHTotal = value;
    }

    /// <summary>
    /// A value of FdsoReturnH.
    /// </summary>
    [JsonPropertyName("fdsoReturnH")]
    public FdsoReturnH FdsoReturnH
    {
      get => fdsoReturnH ??= new();
      set => fdsoReturnH = value;
    }

    private FdsoReturnHTotal fdsoReturnHTotal;
    private FdsoReturnH fdsoReturnH;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RejectMatch.
    /// </summary>
    [JsonPropertyName("rejectMatch")]
    public Common RejectMatch
    {
      get => rejectMatch ??= new();
      set => rejectMatch = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public AdministrativeActCertification Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public EabReportSend Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
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
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public FdsoReturnH Message
    {
      get => message ??= new();
      set => message = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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

    private Common rejectMatch;
    private AdministrativeActCertification restart;
    private External passArea;
    private EabReportSend null1;
    private Common readCount;
    private CsePerson csePerson;
    private FdsoReturnH message;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public AdministrativeActCertification New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingAdministrativeAction")]
    public AdministrativeAction ExistingAdministrativeAction
    {
      get => existingAdministrativeAction ??= new();
      set => existingAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingFederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("existingFederalDebtSetoff")]
    public AdministrativeActCertification ExistingFederalDebtSetoff
    {
      get => existingFederalDebtSetoff ??= new();
      set => existingFederalDebtSetoff = value;
    }

    private AdministrativeActCertification new1;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingObligor;
    private CsePerson existingCsePerson;
    private AdministrativeActCertification existingFederalDebtSetoff;
  }
#endregion
}

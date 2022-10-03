// Program: FN_BF17_MED_JUDGMENT_RETIREMENT, ID: 371359136, model: 746.
// Short name: SWEFF17B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF17_MED_JUDGMENT_RETIREMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBf17MedJudgmentRetirement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF17_MED_JUDGMENT_RETIREMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf17MedJudgmentRetirement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf17MedJudgmentRetirement.
  /// </summary>
  public FnBf17MedJudgmentRetirement(IContext context, Import import,
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
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // 09-22-2008  SWCOAMX - Arun      Initial Development
    // 01-08-2009  SWCOAMX - Arun      CQ#8678 Added Obligation Id to the output
    // file.
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

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

      local.ParameterProcess.Date =
        StringToDate(Substring(local.ProgramProcessingInfo.ParameterList, 1, 10));
        

      if (Lt(new DateTime(2008, 6, 2), local.ParameterProcess.Date))
      {
        ExitState = "ACO0000_INVALID_RESTART_PARM";

        return;
      }

      // Run Type 'R' for Read and 'U' for Update
      local.RunType.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 11, 1);

      if (AsChar(local.RunType.Flag) != 'U')
      {
        local.RunType.Flag = "R";
      }

      local.Current.Date = local.ProgramProcessingInfo.ProcessDate;

      // ***** Convert the parameter process date to timestamp.
      UseFnBuildTimestampFrmDateTime2();

      // ***** Convert the batch date to timestamp.
      UseFnBuildTimestampFrmDateTime1();
    }
    else
    {
      // : Abort exitstate already set.
      return;
    }

    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        // Extract restart info here, if needed
        local.RestartProcess.IefTimestamp =
          Timestamp(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 1, 26));
        local.TotalObligationProcessed.Count =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 28, 7));
        local.TotalDebtDtlProcessed.Count =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 36, 7));
      }
    }
    else
    {
      // : Abort exitstate already set.
      return;
    }

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
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *****************************************************************
    // * End of Batch error handling setup                             *
    // *****************************************************************
    // : Open Conrtol Report file
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Open the output file
    local.EabFileHandling.Action = "OPEN";
    UseFnEabSwexfw18RetireMjWrite2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Extract File.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // <========== GET MAX DATE ==========>
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // *********** Main Processing Logic Starts Here **********
    foreach(var item in ReadObligation())
    {
      // : Read Medical Judgment obligations
      if (AsChar(entities.Obligation.OrderTypeCode) == 'I')
      {
        continue;
      }

      if (ReadCsePerson())
      {
        local.FileNcpInfo.Number = entities.Obligor1.Number;

        // : Retrieve Obligor Person Name from Adabas
        UseCabReadAdabasPersonBatch2();

        if (IsExitState("ADABAS_UNAVAILABLE_RB"))
        {
          // <===========Write error message to error report ========>
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "ADABAS Unavailable .";
          UseCabErrorReport3();
          ExitState = "LE0000_ADABAS_UNAVAILABLE_ABORT";

          return;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ok, continue processing
        }
        else
        {
          local.FileNcpInfo.FirstName = "N/F";
          local.FileNcpInfo.LastName = "N/F";
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGOR_NF_RB";

        break;
      }

      local.FileObligCreatedDate.Date = Date(entities.Obligation.CreatedTmst);

      // *** CQ#8678 Changes Start Here ***
      local.FileObligationId.SystemGeneratedIdentifier =
        entities.Obligation.SystemGeneratedIdentifier;

      // *** CQ#8678 Changes End   Here ***
      if (ReadLegalAction())
      {
        local.FileCourtOrderNumber.StandardNumber =
          entities.LegalAction.StandardNumber;
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF_RB";

        break;
      }

      // : Flag to Identify new obligation is processed
      local.ProcessingNewObligation.Flag = "Y";

      foreach(var item1 in ReadDebtDetailDebtCsePerson())
      {
        if (Equal(entities.DebtDetail.PreconversionProgramCode, "NA"))
        {
          continue;
        }

        if (entities.DebtDetail.BalanceDueAmt <= 0)
        {
          continue;
        }

        if (AsChar(local.ProcessingNewObligation.Flag) == 'Y')
        {
          local.ProcessingNewObligation.Flag = "N";
          ++local.TotalObligationProcessed.Count;
        }

        local.FileCourtOrderAmt.Amount = entities.Debt.Amount;
        local.FileDueDate.DueDt = entities.DebtDetail.DueDt;
        local.FileBalDueAtRetiredDt.BalanceDueAmt =
          entities.DebtDetail.BalanceDueAmt;
        local.FileCovered.CoveredPrdStartDt =
          entities.DebtDetail.CoveredPrdStartDt;
        local.FileCovered.CoveredPrdEndDt = entities.DebtDetail.CoveredPrdEndDt;

        if (AsChar(local.RunType.Flag) == 'U')
        {
          // : Create Debt Adjustment.
          if (!ReadLegalActionPerson())
          {
            // ***---  OK; optional relationship
          }

          if (entities.LegalActionPerson.Populated)
          {
            for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
              local.RetryLoop.Count)
            {
              try
              {
                CreateDebtAdjustment1();

                goto Test1;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    if (local.RetryLoop.Count < 5)
                    {
                      // *** Try again ***
                    }
                    else
                    {
                      ExitState = "FN0000_DEBT_ADJUSTMENT_AE_RB";

                      goto ReadEach;
                    }

                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else
          {
            for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
              local.RetryLoop.Count)
            {
              try
              {
                CreateDebtAdjustment2();

                goto Test1;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    if (local.RetryLoop.Count < 5)
                    {
                      // *** Try again ***
                    }
                    else
                    {
                      ExitState = "FN0000_DEBT_ADJUSTMENT_AE_RB";

                      goto ReadEach;
                    }

                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

Test1:

          // : Create Obligation transaction relation.
          if (ReadObligationTransactionRlnRsn())
          {
            // ok, continue processing
          }
          else
          {
            ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF_RB";

            goto ReadEach;
          }

          for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
            local.RetryLoop.Count)
          {
            try
            {
              CreateObligationTransactionRln();

              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.RetryLoop.Count < 5)
                  {
                    // *** Try again ***
                  }
                  else
                  {
                    ExitState = "FN0000_OBLIG_TRANS_RLN_AE_RB";

                    goto ReadEach;
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // : Update Debt Detail.
          try
          {
            UpdateDebtDetail();

            // : Updated was successful
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0214_DEBT_DETAIL_NU_RB";

                goto ReadEach;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0218_DEBT_DETAIL_PV_RB";

                goto ReadEach;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // : Update Debt Detail Status History.
          local.DebtDetailStatusHistory.ReasonTxt =
            "DA is balance owed at time of statewide MJ deactivation.";

          if (ReadDebtDetailStatusHistory())
          {
            try
            {
              UpdateDebtDetailStatusHistory();

              for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
                local.RetryLoop.Count)
              {
                try
                {
                  CreateDebtDetailStatusHistory();

                  goto Test2;
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      if (local.RetryLoop.Count < 5)
                      {
                        continue;
                      }
                      else
                      {
                        ExitState = "FN0000_DEBT_DETL_STAT_HIST_AE_R";

                        goto ReadEach;
                      }

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_DEBT_DETL_STAT_HIST_PV_RB";

                      goto ReadEach;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_DEBT_DETL_STAT_HIST_NU_RB";

                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_DEBT_DETL_STAT_HIST_PV_RB";

                  goto ReadEach;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            ExitState = "FN0000_DEBT_DETL_STAT_HIST_NF_RB";

            goto ReadEach;
          }
        }

Test2:

        // : Get the Case number for the AP/CH
        local.FileCaseNumber.Number = "";
        local.CaseOpen.Flag = "N";
        local.ApActive.Flag = "N";
        local.ChildActive.Flag = "N";
        local.FileRemarks.Text30 = "";

        if (ReadCaseAbsentParentChild())
        {
          local.CaseOpen.Flag = "Y";
          local.FileCaseNumber.Number = entities.CaseInfo.Number;

          if (!Lt(entities.CaseRoleAbsentParent.EndDate, local.Current.Date))
          {
            local.ApActive.Flag = "Y";
          }

          if (!Lt(entities.CaseRoleChild.EndDate, local.Current.Date))
          {
            local.ChildActive.Flag = "Y";
          }
        }

        if (AsChar(local.CaseOpen.Flag) == 'N')
        {
          local.FileRemarks.Text30 = "Case closed.";
        }
        else if (AsChar(local.ApActive.Flag) == 'N')
        {
          local.FileRemarks.Text30 = "AP inactive.";
        }
        else if (AsChar(local.ChildActive.Flag) == 'N')
        {
          local.FileRemarks.Text30 = "Supported Person inactive.";
        }

        // : Get the Office Id and the Service Provider Name for the case.
        local.FileOfficeInfo.SystemGeneratedId = 0;
        local.FileOfficeInfo.Name = "";
        local.FileSpName.FirstName = "";
        local.FileSpName.LastName = "";

        if (AsChar(local.CaseOpen.Flag) == 'Y')
        {
          if (ReadCaseAssignment())
          {
            if (ReadServiceProviderOffice())
            {
              local.FileOfficeInfo.SystemGeneratedId =
                entities.Office.SystemGeneratedId;
              local.FileOfficeInfo.Name = entities.Office.Name;
              local.FileSpName.FirstName = entities.ServiceProvider.FirstName;
              local.FileSpName.LastName = entities.ServiceProvider.LastName;
            }
            else
            {
              local.FileOfficeInfo.Name = "N/F";
              local.FileSpName.FirstName = "N/F";
              local.FileSpName.LastName = "N/F";
            }
          }
        }

        // : Retrieve Supported Person Name from Adabas
        local.FileSupportedInfo.Number = entities.Supported1.Number;
        UseCabReadAdabasPersonBatch1();

        if (IsExitState("ADABAS_UNAVAILABLE_RB"))
        {
          // <===========Write error message to error report ========>
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "ADABAS Unavailable .";
          UseCabErrorReport3();
          ExitState = "LE0000_ADABAS_UNAVAILABLE_ABORT";

          return;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ok, continue processing
        }
        else
        {
          // *******************************************
          // Unknown error response returned from adabas .
          // *******************************************
          local.FileSupportedInfo.FirstName = "N/F";
          local.FileSupportedInfo.LastName = "N/F";
          ExitState = "ACO_NN0000_ALL_OK";
        }

        ++local.TotalDebtDtlProcessed.Count;

        // : Write to sequential file.
        local.EabFileHandling.Action = "WRITE";

        // *** CQ#8678 Changes Start Here ***
        local.FileTypeOfProcess.Text1 = "E";

        // *** CQ#8678 Changes End   Here ***
        UseFnEabSwexfw18RetireMjWrite1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_AE0000_BATCH_ABEND";

          return;
        }

        // *** Commit if it's time
        if (local.ProcessCountToCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.ProgramCheckpointRestart.ProgramName = global.UserId;
          local.RestartProcess.IefTimestamp = entities.Obligation.CreatedTmst;
          UseLeCabConvertTimestamp();
          local.ProgramCheckpointRestart.RestartInfo =
            local.RestartProcess.TextTimestamp;
          local.ProgramCheckpointRestart.RestartInfo =
            local.RestartProcess.TextTimestamp + " " + NumberToString
            (local.TotalObligationProcessed.Count, 9, 7) + " " + NumberToString
            (local.TotalDebtDtlProcessed.Count, 9, 7);
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.NeededToWrite.RptDetail =
              "Error in update checkpoint restart.  Exitstate msg is: " + local
              .ExitStateWorkArea.Message;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport3();
            ExitState = "ACO_AE0000_BATCH_ABEND";

            return;
          }

          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error in External to do a commit for: " + local
              .NeededToWrite.RptDetail;
            UseCabErrorReport3();
            ExitState = "ACO_AE0000_BATCH_ABEND";

            return;
          }

          local.ProcessCountToCommit.Count = 0;
        }

        ++local.ProcessCountToCommit.Count;
      }
    }

ReadEach:

    // *********** Main Processing Logic Ends Here **********
    // END OF PROCESSING
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("FN0000_OBLIGOR_NF_RB"))
      {
        local.NeededToWrite.RptDetail =
          "Obligor not found for obligation : " + NumberToString
          (entities.Obligation.SystemGeneratedIdentifier, 7, 9);
      }
      else if (IsExitState("LEGAL_ACTION_NF_RB"))
      {
        local.NeededToWrite.RptDetail =
          "Legal Action not found for obligation : " + NumberToString
          (entities.Obligation.SystemGeneratedIdentifier, 7, 9);
      }
      else if (IsExitState("FN0000_DEBT_ADJUSTMENT_AE_RB"))
      {
        local.NeededToWrite.RptDetail = "Debt adjustment already exists.";
      }
      else if (IsExitState("FN0000_OBLIG_TRANS_RLN_RSN_NF_RB"))
      {
        local.NeededToWrite.RptDetail =
          "Obligation transaction rln rsn not found for : " + NumberToString
          (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier, 7, 9);
          
      }
      else if (IsExitState("FN0000_OBLIG_TRANS_RLN_AE_RB"))
      {
        local.NeededToWrite.RptDetail =
          "Obligation transaction rln already exists.";
      }
      else if (IsExitState("FN0214_DEBT_DETAIL_NU_RB"))
      {
        local.NeededToWrite.RptDetail = "Debt adjustment not unique.";
      }
      else if (IsExitState("FN0218_DEBT_DETAIL_PV_RB"))
      {
        local.NeededToWrite.RptDetail = "Debt adjustment permitted violation.";
      }
      else if (IsExitState("FN0000_DEBT_DETL_STAT_HIST_AE_R"))
      {
        local.NeededToWrite.RptDetail =
          "Debt adjustment status history already exists.";
      }
      else if (IsExitState("FN0000_DEBT_DETL_STAT_HIST_PV_RB"))
      {
        local.NeededToWrite.RptDetail =
          "Debt adjustment status history permitted violation.";
      }
      else if (IsExitState("FN0000_DEBT_DETL_STAT_HIST_NU_RB"))
      {
        local.NeededToWrite.RptDetail =
          "Debt adjustment status history not unique.";
      }
      else if (IsExitState("FN0000_DEBT_DETL_STAT_HIST_NF_RB"))
      {
        local.NeededToWrite.RptDetail =
          "Debt adjustment status history not found.";
      }
      else
      {
        local.NeededToWrite.RptDetail =
          "Missed an exit state to display. Please add it for the display.";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport3();
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Successful end of job, so update checkpoint restart.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail =
        "Successful End of job, but error in update checkpoint restart.  Exitstate msg is: " +
        local.ExitStateWorkArea.Message;
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // **** START OF REPORT DSN CLOSE PROCESS ****
    // : Close the output file
    local.EabFileHandling.Action = "CLOSE";
    UseFnEabSwexfw18RetireMjWrite2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Medical Judgment Retirement file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // **** END OF REPORT DSN CLOSE PROCESS ****
    // : Write Control Totals
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail =
      "Total Obligations Processed              :";
    local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " " +
      NumberToString(local.TotalObligationProcessed.Count, 9, 7);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.RunType.Flag) == 'U')
    {
      local.NeededToWrite.RptDetail =
        "Total Debt Details Processed and Updated :";
    }
    else
    {
      local.NeededToWrite.RptDetail =
        "Total Debt Details Processed             :";
    }

    local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " " +
      NumberToString(local.TotalDebtDtlProcessed.Count, 9, 7);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Close Control Report file
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
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
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

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

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void UseCabReadAdabasPersonBatch1()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.FileSupportedInfo.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.FileSupportedInfo.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPersonBatch2()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.FileNcpInfo.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.FileNcpInfo.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Initialized.Date;

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

  private void UseFnBuildTimestampFrmDateTime1()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.Current);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.ParameterProcess.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.ParameterProcess);
  }

  private void UseFnEabSwexfw18RetireMjWrite1()
  {
    var useImport = new FnEabSwexfw18RetireMjWrite.Import();
    var useExport = new FnEabSwexfw18RetireMjWrite.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NcpInfo.Assign(local.FileNcpInfo);
    useImport.ObligCreatedDate.Date = local.FileObligCreatedDate.Date;
    useImport.CourtOrderNumber.StandardNumber =
      local.FileCourtOrderNumber.StandardNumber;
    useImport.CourtOrderAmt.Amount = local.FileCourtOrderAmt.Amount;
    MoveDebtDetail(local.FileCovered, useImport.Covered);
    useImport.DueDate.DueDt = local.FileDueDate.DueDt;
    useImport.BalDueAtRetiredDt.BalanceDueAmt =
      local.FileBalDueAtRetiredDt.BalanceDueAmt;
    useImport.SupportedInfo.Assign(local.FileSupportedInfo);
    useImport.CaseNbr.Number = local.FileCaseNumber.Number;
    MoveServiceProvider(local.FileSpName, useImport.SpName);
    MoveOffice(local.FileOfficeInfo, useImport.OfficeInfo);
    useImport.Remarks.Text30 = local.FileRemarks.Text30;
    useImport.TypeOfProcess.Text1 = local.FileTypeOfProcess.Text1;
    useImport.ObligationId.SystemGeneratedIdentifier =
      local.FileObligationId.SystemGeneratedIdentifier;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnEabSwexfw18RetireMjWrite.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnEabSwexfw18RetireMjWrite2()
  {
    var useImport = new FnEabSwexfw18RetireMjWrite.Import();
    var useExport = new FnEabSwexfw18RetireMjWrite.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnEabSwexfw18RetireMjWrite.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.RestartProcess,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.RestartProcess);
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

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void CreateDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "DA";
    var amount = entities.DebtDetail.BalanceDueAmt;
    var debtAdjustmentType = "D";
    var debtAdjustmentDt = local.Current.Date;
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTmst = local.Current.Timestamp;
    var cpaSupType =
      GetImplicitValue<ObligationTransaction, string>("CpaSupType");
    var otyType = entities.Obligation.DtyGeneratedId;
    var debtAdjCollAdjProcReqInd = "Y";
    var lapId = entities.LegalActionPerson.Identifier;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtAdjustmentType", debtAdjustmentType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
      debtAdjCollAdjProcReqInd);
    entities.DebtAdjustment.Populated = false;
    Update("CreateDebtAdjustment1",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(command, "debtAdjTyp", debtAdjustmentType);
        db.SetDate(command, "debAdjDt", debtAdjustmentDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", "");
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetDate(command, "debtAdjProcDate", debtAdjustmentDt);
        db.SetString(command, "daCaProcReqInd", debtAdjCollAdjProcReqInd);
        db.SetNullableDate(command, "daCaProcDt", default(DateTime));
        db.SetString(command, "rsnCd", "");
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetNullableString(
          command, "reverseClctnsInd", debtAdjCollAdjProcReqInd);
      });

    entities.DebtAdjustment.ObgGeneratedId = obgGeneratedId;
    entities.DebtAdjustment.CspNumber = cspNumber;
    entities.DebtAdjustment.CpaType = cpaType;
    entities.DebtAdjustment.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtAdjustment.Type1 = type1;
    entities.DebtAdjustment.Amount = amount;
    entities.DebtAdjustment.DebtAdjustmentType = debtAdjustmentType;
    entities.DebtAdjustment.DebtAdjustmentDt = debtAdjustmentDt;
    entities.DebtAdjustment.CreatedBy = createdBy;
    entities.DebtAdjustment.CreatedTmst = createdTmst;
    entities.DebtAdjustment.CspSupNumber = null;
    entities.DebtAdjustment.CpaSupType = cpaSupType;
    entities.DebtAdjustment.OtyType = otyType;
    entities.DebtAdjustment.DebtAdjustmentProcessDate = debtAdjustmentDt;
    entities.DebtAdjustment.DebtAdjCollAdjProcReqInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.LapId = lapId;
    entities.DebtAdjustment.ReverseCollectionsInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.Populated = true;
  }

  private void CreateDebtAdjustment2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "DA";
    var amount = entities.DebtDetail.BalanceDueAmt;
    var debtAdjustmentType = "D";
    var debtAdjustmentDt = local.Current.Date;
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTmst = local.Current.Timestamp;
    var cpaSupType =
      GetImplicitValue<ObligationTransaction, string>("CpaSupType");
    var otyType = entities.Obligation.DtyGeneratedId;
    var debtAdjCollAdjProcReqInd = "Y";

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtAdjustmentType", debtAdjustmentType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
      debtAdjCollAdjProcReqInd);
    entities.DebtAdjustment.Populated = false;
    Update("CreateDebtAdjustment2",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(command, "debtAdjTyp", debtAdjustmentType);
        db.SetDate(command, "debAdjDt", debtAdjustmentDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", "");
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetDate(command, "debtAdjProcDate", debtAdjustmentDt);
        db.SetString(command, "daCaProcReqInd", debtAdjCollAdjProcReqInd);
        db.SetNullableDate(command, "daCaProcDt", default(DateTime));
        db.SetString(command, "rsnCd", "");
        db.SetNullableString(
          command, "reverseClctnsInd", debtAdjCollAdjProcReqInd);
      });

    entities.DebtAdjustment.ObgGeneratedId = obgGeneratedId;
    entities.DebtAdjustment.CspNumber = cspNumber;
    entities.DebtAdjustment.CpaType = cpaType;
    entities.DebtAdjustment.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtAdjustment.Type1 = type1;
    entities.DebtAdjustment.Amount = amount;
    entities.DebtAdjustment.DebtAdjustmentType = debtAdjustmentType;
    entities.DebtAdjustment.DebtAdjustmentDt = debtAdjustmentDt;
    entities.DebtAdjustment.CreatedBy = createdBy;
    entities.DebtAdjustment.CreatedTmst = createdTmst;
    entities.DebtAdjustment.CspSupNumber = null;
    entities.DebtAdjustment.CpaSupType = cpaSupType;
    entities.DebtAdjustment.OtyType = otyType;
    entities.DebtAdjustment.DebtAdjustmentProcessDate = debtAdjustmentDt;
    entities.DebtAdjustment.DebtAdjCollAdjProcReqInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.LapId = null;
    entities.DebtAdjustment.ReverseCollectionsInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.Populated = true;
  }

  private void CreateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDt = local.Current.Date;
    var discontinueDt = local.Max.Date;
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTmst = local.Current.Timestamp;
    var otrType = entities.DebtDetail.OtrType;
    var otrId = entities.DebtDetail.OtrGeneratedId;
    var cpaType = entities.DebtDetail.CpaType;
    var cspNumber = entities.DebtDetail.CspNumber;
    var obgId = entities.DebtDetail.ObgGeneratedId;
    var code = "D";
    var otyType = entities.DebtDetail.OtyType;
    var reasonTxt = local.DebtDetailStatusHistory.ReasonTxt ?? "";

    CheckValid<DebtDetailStatusHistory>("OtrType", otrType);
    CheckValid<DebtDetailStatusHistory>("CpaType", cpaType);
    entities.DebtDetailStatusHistory.Populated = false;
    Update("CreateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnStatHstId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "obTrnStCd", code);
        db.SetInt32(command, "otyType", otyType);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
      });

    entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtDetailStatusHistory.EffectiveDt = effectiveDt;
    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.CreatedBy = createdBy;
    entities.DebtDetailStatusHistory.CreatedTmst = createdTmst;
    entities.DebtDetailStatusHistory.OtrType = otrType;
    entities.DebtDetailStatusHistory.OtrId = otrId;
    entities.DebtDetailStatusHistory.CpaType = cpaType;
    entities.DebtDetailStatusHistory.CspNumber = cspNumber;
    entities.DebtDetailStatusHistory.ObgId = obgId;
    entities.DebtDetailStatusHistory.Code = code;
    entities.DebtDetailStatusHistory.OtyType = otyType;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
  }

  private void CreateObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    System.Diagnostics.Debug.Assert(entities.DebtAdjustment.Populated);

    var onrGeneratedId =
      entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    var otrType = entities.DebtAdjustment.Type1;
    var otrGeneratedId = entities.DebtAdjustment.SystemGeneratedIdentifier;
    var cpaType = entities.DebtAdjustment.CpaType;
    var cspNumber = entities.DebtAdjustment.CspNumber;
    var obgGeneratedId = entities.DebtAdjustment.ObgGeneratedId;
    var otrPType = entities.Debt.Type1;
    var otrPGeneratedId = entities.Debt.SystemGeneratedIdentifier;
    var cpaPType = entities.Debt.CpaType;
    var cspPNumber = entities.Debt.CspNumber;
    var obgPGeneratedId = entities.Debt.ObgGeneratedId;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTmst = local.Current.Timestamp;
    var otyTypePrimary = entities.Debt.OtyType;
    var otyTypeSecondary = entities.DebtAdjustment.OtyType;
    var description =
      "DA is balance owed at time of statewide MJ deactivation.";

    CheckValid<ObligationTransactionRln>("OtrType", otrType);
    CheckValid<ObligationTransactionRln>("CpaType", cpaType);
    CheckValid<ObligationTransactionRln>("OtrPType", otrPType);
    CheckValid<ObligationTransactionRln>("CpaPType", cpaPType);
    entities.ObligationTransactionRln.Populated = false;
    Update("CreateObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "onrGeneratedId", onrGeneratedId);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "otrPType", otrPType);
        db.SetInt32(command, "otrPGeneratedId", otrPGeneratedId);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetInt32(command, "obgPGeneratedId", obgPGeneratedId);
        db.SetInt32(command, "obTrnRlnId", systemGeneratedIdentifier);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "otyTypePrimary", otyTypePrimary);
        db.SetInt32(command, "otyTypeSecondary", otyTypeSecondary);
        db.SetNullableString(command, "obTrnRlnDsc", description);
      });

    entities.ObligationTransactionRln.OnrGeneratedId = onrGeneratedId;
    entities.ObligationTransactionRln.OtrType = otrType;
    entities.ObligationTransactionRln.OtrGeneratedId = otrGeneratedId;
    entities.ObligationTransactionRln.CpaType = cpaType;
    entities.ObligationTransactionRln.CspNumber = cspNumber;
    entities.ObligationTransactionRln.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransactionRln.OtrPType = otrPType;
    entities.ObligationTransactionRln.OtrPGeneratedId = otrPGeneratedId;
    entities.ObligationTransactionRln.CpaPType = cpaPType;
    entities.ObligationTransactionRln.CspPNumber = cspPNumber;
    entities.ObligationTransactionRln.ObgPGeneratedId = obgPGeneratedId;
    entities.ObligationTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransactionRln.CreatedBy = createdBy;
    entities.ObligationTransactionRln.CreatedTmst = createdTmst;
    entities.ObligationTransactionRln.OtyTypePrimary = otyTypePrimary;
    entities.ObligationTransactionRln.OtyTypeSecondary = otyTypeSecondary;
    entities.ObligationTransactionRln.Description = description;
    entities.ObligationTransactionRln.Populated = true;
  }

  private bool ReadCaseAbsentParentChild()
  {
    entities.CaseRoleChild.Populated = false;
    entities.CaseRoleAbsentParent.Populated = false;
    entities.CaseInfo.Populated = false;

    return Read("ReadCaseAbsentParentChild",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Obligor1.Number);
        db.SetString(command, "cspNumber2", entities.Supported1.Number);
      },
      (db, reader) =>
      {
        entities.CaseInfo.Number = db.GetString(reader, 0);
        entities.CaseRoleAbsentParent.CasNumber = db.GetString(reader, 0);
        entities.CaseRoleChild.CasNumber = db.GetString(reader, 0);
        entities.CaseInfo.Status = db.GetNullableString(reader, 1);
        entities.CaseRoleAbsentParent.CspNumber = db.GetString(reader, 2);
        entities.CaseRoleAbsentParent.Type1 = db.GetString(reader, 3);
        entities.CaseRoleAbsentParent.Identifier = db.GetInt32(reader, 4);
        entities.CaseRoleAbsentParent.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRoleAbsentParent.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRoleChild.CspNumber = db.GetString(reader, 7);
        entities.CaseRoleChild.Type1 = db.GetString(reader, 8);
        entities.CaseRoleChild.Identifier = db.GetInt32(reader, 9);
        entities.CaseRoleChild.StartDate = db.GetNullableDate(reader, 10);
        entities.CaseRoleChild.EndDate = db.GetNullableDate(reader, 11);
        entities.CaseRoleChild.Populated = true;
        entities.CaseRoleAbsentParent.Populated = true;
        entities.CaseInfo.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRoleAbsentParent.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRoleChild.Type1);
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", local.FileCaseNumber.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OspCode = db.GetString(reader, 4);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.CaseAssignment.CasNo = db.GetString(reader, 6);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Obligor1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor1.Type1 = db.GetString(reader, 1);
        entities.Obligor1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Obligor1.Type1);
      });
  }

  private IEnumerable<bool> ReadDebtDetailDebtCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Supported1.Populated = false;
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailDebtCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 11);
        entities.DebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.Debt.Amount = db.GetDecimal(reader, 14);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 16);
        entities.Debt.LapId = db.GetNullableInt32(reader, 17);
        entities.Supported1.Number = db.GetString(reader, 18);
        entities.Supported1.Type1 = db.GetString(reader, 19);
        entities.Supported1.Populated = true;
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<CsePerson>("Type1", entities.Supported1.Type1);

        return true;
      });
  }

  private bool ReadDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(command, "obgId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(command, "otrId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
        db.SetNullableDate(
          command, "discontinueDt", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.CreatedBy = db.GetString(reader, 3);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 6);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 8);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 9);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 10);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 12);
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "laPersonId", entities.Debt.LapId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          local.RestartProcess.IefTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          local.ParameterProcess.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      null,
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private bool ReadServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(command, "servicePrvderId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "officeId", entities.CaseAssignment.OffId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.Office.Name = db.GetString(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private void UpdateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var balanceDueAmt = 0M;
    var retiredDt = local.Current.Date;
    var lastUpdatedTmst = local.Current.Timestamp;
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;

    entities.DebtDetail.Populated = false;
    Update("UpdateDebtDetail",
      (db, command) =>
      {
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDate(command, "retiredDt", retiredDt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      });

    entities.DebtDetail.BalanceDueAmt = balanceDueAmt;
    entities.DebtDetail.RetiredDt = retiredDt;
    entities.DebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.DebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.DebtDetail.Populated = true;
  }

  private void UpdateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetailStatusHistory.Populated);

    var discontinueDt = local.Current.Date;
    var reasonTxt = local.DebtDetailStatusHistory.ReasonTxt ?? "";

    entities.DebtDetailStatusHistory.Populated = false;
    Update("UpdateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
        db.SetInt32(
          command, "obTrnStatHstId",
          entities.DebtDetailStatusHistory.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType", entities.DebtDetailStatusHistory.OtrType);
        db.SetInt32(command, "otrId", entities.DebtDetailStatusHistory.OtrId);
        db.SetString(
          command, "cpaType", entities.DebtDetailStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber", entities.DebtDetailStatusHistory.CspNumber);
        db.SetInt32(command, "obgId", entities.DebtDetailStatusHistory.ObgId);
        db.
          SetString(command, "obTrnStCd", entities.DebtDetailStatusHistory.Code);
          
        db.
          SetInt32(command, "otyType", entities.DebtDetailStatusHistory.OtyType);
          
      });

    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
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
    /// A value of ProcessingNewObligation.
    /// </summary>
    [JsonPropertyName("processingNewObligation")]
    public Common ProcessingNewObligation
    {
      get => processingNewObligation ??= new();
      set => processingNewObligation = value;
    }

    /// <summary>
    /// A value of TotalDebtDtlProcessed.
    /// </summary>
    [JsonPropertyName("totalDebtDtlProcessed")]
    public Common TotalDebtDtlProcessed
    {
      get => totalDebtDtlProcessed ??= new();
      set => totalDebtDtlProcessed = value;
    }

    /// <summary>
    /// A value of TotalObligationProcessed.
    /// </summary>
    [JsonPropertyName("totalObligationProcessed")]
    public Common TotalObligationProcessed
    {
      get => totalObligationProcessed ??= new();
      set => totalObligationProcessed = value;
    }

    /// <summary>
    /// A value of ChildActive.
    /// </summary>
    [JsonPropertyName("childActive")]
    public Common ChildActive
    {
      get => childActive ??= new();
      set => childActive = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of FileRemarks.
    /// </summary>
    [JsonPropertyName("fileRemarks")]
    public TextWorkArea FileRemarks
    {
      get => fileRemarks ??= new();
      set => fileRemarks = value;
    }

    /// <summary>
    /// A value of FileOfficeInfo.
    /// </summary>
    [JsonPropertyName("fileOfficeInfo")]
    public Office FileOfficeInfo
    {
      get => fileOfficeInfo ??= new();
      set => fileOfficeInfo = value;
    }

    /// <summary>
    /// A value of FileSpName.
    /// </summary>
    [JsonPropertyName("fileSpName")]
    public ServiceProvider FileSpName
    {
      get => fileSpName ??= new();
      set => fileSpName = value;
    }

    /// <summary>
    /// A value of FileCaseNumber.
    /// </summary>
    [JsonPropertyName("fileCaseNumber")]
    public Case1 FileCaseNumber
    {
      get => fileCaseNumber ??= new();
      set => fileCaseNumber = value;
    }

    /// <summary>
    /// A value of FileSupportedInfo.
    /// </summary>
    [JsonPropertyName("fileSupportedInfo")]
    public CsePersonsWorkSet FileSupportedInfo
    {
      get => fileSupportedInfo ??= new();
      set => fileSupportedInfo = value;
    }

    /// <summary>
    /// A value of FileBalDueAtRetiredDt.
    /// </summary>
    [JsonPropertyName("fileBalDueAtRetiredDt")]
    public DebtDetail FileBalDueAtRetiredDt
    {
      get => fileBalDueAtRetiredDt ??= new();
      set => fileBalDueAtRetiredDt = value;
    }

    /// <summary>
    /// A value of FileDueDate.
    /// </summary>
    [JsonPropertyName("fileDueDate")]
    public DebtDetail FileDueDate
    {
      get => fileDueDate ??= new();
      set => fileDueDate = value;
    }

    /// <summary>
    /// A value of FileCovered.
    /// </summary>
    [JsonPropertyName("fileCovered")]
    public DebtDetail FileCovered
    {
      get => fileCovered ??= new();
      set => fileCovered = value;
    }

    /// <summary>
    /// A value of FileCourtOrderAmt.
    /// </summary>
    [JsonPropertyName("fileCourtOrderAmt")]
    public ObligationTransaction FileCourtOrderAmt
    {
      get => fileCourtOrderAmt ??= new();
      set => fileCourtOrderAmt = value;
    }

    /// <summary>
    /// A value of FileCourtOrderNumber.
    /// </summary>
    [JsonPropertyName("fileCourtOrderNumber")]
    public LegalAction FileCourtOrderNumber
    {
      get => fileCourtOrderNumber ??= new();
      set => fileCourtOrderNumber = value;
    }

    /// <summary>
    /// A value of FileObligCreatedDate.
    /// </summary>
    [JsonPropertyName("fileObligCreatedDate")]
    public DateWorkArea FileObligCreatedDate
    {
      get => fileObligCreatedDate ??= new();
      set => fileObligCreatedDate = value;
    }

    /// <summary>
    /// A value of FileNcpInfo.
    /// </summary>
    [JsonPropertyName("fileNcpInfo")]
    public CsePersonsWorkSet FileNcpInfo
    {
      get => fileNcpInfo ??= new();
      set => fileNcpInfo = value;
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
    /// A value of RunType.
    /// </summary>
    [JsonPropertyName("runType")]
    public Common RunType
    {
      get => runType ??= new();
      set => runType = value;
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
    /// A value of RestartProcess.
    /// </summary>
    [JsonPropertyName("restartProcess")]
    public BatchTimestampWorkArea RestartProcess
    {
      get => restartProcess ??= new();
      set => restartProcess = value;
    }

    /// <summary>
    /// A value of ParameterProcess.
    /// </summary>
    [JsonPropertyName("parameterProcess")]
    public DateWorkArea ParameterProcess
    {
      get => parameterProcess ??= new();
      set => parameterProcess = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of ProcessCountToCommit.
    /// </summary>
    [JsonPropertyName("processCountToCommit")]
    public Common ProcessCountToCommit
    {
      get => processCountToCommit ??= new();
      set => processCountToCommit = value;
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
    /// A value of RetryLoop.
    /// </summary>
    [JsonPropertyName("retryLoop")]
    public Common RetryLoop
    {
      get => retryLoop ??= new();
      set => retryLoop = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of FileTypeOfProcess.
    /// </summary>
    [JsonPropertyName("fileTypeOfProcess")]
    public TextWorkArea FileTypeOfProcess
    {
      get => fileTypeOfProcess ??= new();
      set => fileTypeOfProcess = value;
    }

    /// <summary>
    /// A value of FileObligationId.
    /// </summary>
    [JsonPropertyName("fileObligationId")]
    public Obligation FileObligationId
    {
      get => fileObligationId ??= new();
      set => fileObligationId = value;
    }

    /// <summary>
    /// A value of FileCreatedBy.
    /// </summary>
    [JsonPropertyName("fileCreatedBy")]
    public DebtDetailStatusHistory FileCreatedBy
    {
      get => fileCreatedBy ??= new();
      set => fileCreatedBy = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      processingNewObligation = null;
      totalDebtDtlProcessed = null;
      totalObligationProcessed = null;
      childActive = null;
      apActive = null;
      caseOpen = null;
      fileRemarks = null;
      fileOfficeInfo = null;
      fileSpName = null;
      fileCaseNumber = null;
      fileSupportedInfo = null;
      fileBalDueAtRetiredDt = null;
      fileDueDate = null;
      fileCovered = null;
      fileCourtOrderAmt = null;
      fileCourtOrderNumber = null;
      fileObligCreatedDate = null;
      fileNcpInfo = null;
      current = null;
      runType = null;
      abendData = null;
      restartProcess = null;
      parameterProcess = null;
      exitStateWorkArea = null;
      programProcessingInfo = null;
      max = null;
      initialized = null;
      eabFileHandling = null;
      neededToOpen = null;
      neededToWrite = null;
      processCountToCommit = null;
      passArea = null;
      programCheckpointRestart = null;
      retryLoop = null;
      null1 = null;
      fileTypeOfProcess = null;
      fileObligationId = null;
      fileCreatedBy = null;
    }

    private Common processingNewObligation;
    private Common totalDebtDtlProcessed;
    private Common totalObligationProcessed;
    private Common childActive;
    private Common apActive;
    private Common caseOpen;
    private TextWorkArea fileRemarks;
    private Office fileOfficeInfo;
    private ServiceProvider fileSpName;
    private Case1 fileCaseNumber;
    private CsePersonsWorkSet fileSupportedInfo;
    private DebtDetail fileBalDueAtRetiredDt;
    private DebtDetail fileDueDate;
    private DebtDetail fileCovered;
    private ObligationTransaction fileCourtOrderAmt;
    private LegalAction fileCourtOrderNumber;
    private DateWorkArea fileObligCreatedDate;
    private CsePersonsWorkSet fileNcpInfo;
    private DateWorkArea current;
    private Common runType;
    private AbendData abendData;
    private BatchTimestampWorkArea restartProcess;
    private DateWorkArea parameterProcess;
    private ExitStateWorkArea exitStateWorkArea;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private Common processCountToCommit;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common retryLoop;
    private DateWorkArea null1;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private TextWorkArea fileTypeOfProcess;
    private Obligation fileObligationId;
    private DebtDetailStatusHistory fileCreatedBy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of CaseRoleChild.
    /// </summary>
    [JsonPropertyName("caseRoleChild")]
    public CaseRole CaseRoleChild
    {
      get => caseRoleChild ??= new();
      set => caseRoleChild = value;
    }

    /// <summary>
    /// A value of CaseRoleAbsentParent.
    /// </summary>
    [JsonPropertyName("caseRoleAbsentParent")]
    public CaseRole CaseRoleAbsentParent
    {
      get => caseRoleAbsentParent ??= new();
      set => caseRoleAbsentParent = value;
    }

    /// <summary>
    /// A value of CaseInfo.
    /// </summary>
    [JsonPropertyName("caseInfo")]
    public Case1 CaseInfo
    {
      get => caseInfo ??= new();
      set => caseInfo = value;
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
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private CaseRole caseRoleChild;
    private CaseRole caseRoleAbsentParent;
    private Case1 caseInfo;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private CsePerson supported1;
    private CsePersonAccount supported2;
    private ObligationTransaction debt;
    private ObligationType obligationType;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private LegalActionPerson legalActionPerson;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransactionRln obligationTransactionRln;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private LegalAction legalAction;
  }
#endregion
}

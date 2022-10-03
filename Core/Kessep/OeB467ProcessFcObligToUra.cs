// Program: OE_B467_PROCESS_FC_OBLIG_TO_URA, ID: 374473787, model: 746.
// Short name: SWEE467B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B467_PROCESS_FC_OBLIG_TO_URA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB467ProcessFcObligToUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B467_PROCESS_FC_OBLIG_TO_URA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB467ProcessFcObligToUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB467ProcessFcObligToUra.
  /// </summary>
  public OeB467ProcessFcObligToUra(IContext context, Import import,
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
    // ****************************************************************
    // 06-29-00  WK 000206  Fangman - New program to derive FC URA data from 
    // debt details.
    // 10-02-00 PR 104223  A Doty - Corrected a problem with handling P/S and 
    // Joint & Several obligations.
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.AbendCheckLoop.Flag = "Y";
    local.ErrorLoop.Flag = "Y";
    local.AdabasExternalAction.Flag = "F";
    local.HardcodedAccruing.Classification = "A";
    local.Send.Action = "WRITE";
    local.Collection.Date = Now().Date;
    UseOeB467Housekeeping();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (ReadControlTable())
      {
        MoveControlTable(entities.FcAeCaseNumber, local.FcAeCaseNumber);
      }
      else
      {
        ExitState = "CONTROL_TABLE_FC_AE_CASE_NBR_NF";
      }
    }
    else
    {
      // **** Continue & error out  ****
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend1.RptDetail = "Abort: " + " - " + local
        .ExitStateMessage.Message;
      UseCabErrorReport2();

      if (!Equal(local.Receive.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (AsChar(local.AbendCheckLoop.Flag) == 'Y')
    {
      // **** The sort on due date is not necessary but helps with verifing 
      // results  ****
      foreach(var item in ReadDebtDetailDebtObligationObligationTypeCsePerson())
      {
        // ****  Skip medical obligations - B466 will process these  ****
        if (entities.ObligationType.SystemGeneratedIdentifier == 3 || entities
          .ObligationType.SystemGeneratedIdentifier == 10 || entities
          .ObligationType.SystemGeneratedIdentifier == 11)
        {
          continue;
        }

        ++local.CountsAndAmounts.NbrOfDebtDtlsRead.Count;
        local.IoDoneForThisDebtDtl.Flag = "N";

        if (AsChar(local.ErrorLoop.Flag) == 'Y')
        {
          // : PR 104223 - Skip all Secondary Obligations.
          //   Go ahead and set the processed date on Debt.
          if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
          {
            ++local.CountsAndAmounts.NbrOfNonFcDebtDtls.Count;
          }
          else
          {
            UseFnDeterminePgmForDebtDetail();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test1;
            }

            if (Equal(local.Program.Code, "FC"))
            {
              if (entities.Debt.Amount > 0)
              {
                local.FcAmt.TotalCurrency = entities.Debt.Amount;
              }
              else
              {
                local.FcAmt.TotalCurrency = entities.DebtDetail.BalanceDueAmt;
              }

              foreach(var item1 in ReadDebtAdjustment())
              {
                if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
                {
                  local.FcAmt.TotalCurrency += entities.DebtAdjustment.Amount;
                }
                else
                {
                  local.FcAmt.TotalCurrency -= entities.DebtAdjustment.Amount;
                }
              }

              ++local.CountsAndAmounts.NbrOfFcDebtDtls.Count;
              local.CountsAndAmounts.AmtOfFcDebtDtlsRead.TotalCurrency += local.
                FcAmt.TotalCurrency;

              if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
              {
                UseOeB467DisplayDebtDtlInfo2();
              }

              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                // ****  Accruing obligations use debt detail due date  ****
                local.ForRead.Year = Year(entities.DebtDetail.DueDt);
                local.ForRead.Month = Month(entities.DebtDetail.DueDt);
                local.OnFcDate.Date = entities.DebtDetail.DueDt;
              }
              else
              {
                // ****  Non-accruing obligations use covered period start date
                // ****
                local.ForRead.Year =
                  Year(entities.DebtDetail.CoveredPrdStartDt);
                local.ForRead.Month =
                  Month(entities.DebtDetail.CoveredPrdStartDt);
                local.OnFcDate.Date = entities.DebtDetail.CoveredPrdStartDt;
              }

              // ****  Check AE to get the AE Case Number if it exists.   ****
              UseOeEabGetAeCaseNbrForPers1();

              if (AsChar(local.AdabasExternalAction.Flag) == 'F')
              {
                local.AdabasExternalAction.Flag = "";
              }

              if (Equal(local.ExecResults.Text5, "ABEND"))
              {
                local.EabReportSend1.RptDetail = "Abort in adabas external " + " - " +
                  local.ExecResults.Text80;
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                goto Test2;
              }

              if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
              {
                if (Equal(local.FromAeImHousehold.FirstBenefitDate,
                  local.Initialized.Date))
                {
                  local.ReturnedDateFromAe.Text2 = "";
                }
                else
                {
                  local.ReturnedDateFromAe.Text2 = " *";
                }

                local.EabReportSend1.RptDetail = "Returned from AE: " + local
                  .FromAeImHousehold.AeCaseNo + local
                  .ReturnedDateFromAe.Text2 + NumberToString
                  (DateToInt(local.FromAeImHousehold.FirstBenefitDate), 8, 8) +
                  "  " + local.FromAeImHouseholdMbrMnthlySum.Relationship + "."
                  ;
                UseCabErrorReport2();
              }

              if (IsEmpty(local.FromAeImHousehold.AeCaseNo))
              {
                if (ReadImHousehold1())
                {
                  // Continue
                }
                else
                {
                  // ****  Derive the next case number to create   ****
                  ++local.FcAeCaseNumber.LastUsedNumber;
                  local.ForCreate.AeCaseNo = "F" + NumberToString
                    (local.FcAeCaseNumber.LastUsedNumber, 9, 7);

                  try
                  {
                    CreateImHousehold1();
                    local.IoDoneForThisDebtDtl.Flag = "Y";
                    ++local.CountsAndAmounts.NbrOfImHhCreated.Count;
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "IM_HOUSEHOLD_AE";

                        goto Test1;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "IM_HOUSEHOLD_PV";

                        goto Test1;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
              }
              else if (ReadImHousehold2())
              {
                // Continue
              }
              else
              {
                // ****  Get the 1st benefit date if it was not returned from AE
                // ****
                if (Equal(local.FromAeImHousehold.FirstBenefitDate,
                  local.Initialized.Date))
                {
                  local.FromAeImHousehold.FirstBenefitDate =
                    IntToDate(local.ForRead.Year * 10000 + local
                    .ForRead.Month * 100 + 1);
                }

                try
                {
                  CreateImHousehold2();
                  local.IoDoneForThisDebtDtl.Flag = "Y";
                  ++local.CountsAndAmounts.NbrOfImHhCreated.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "IM_HOUSEHOLD_AE";

                      goto Test1;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "IM_HOUSEHOLD_PV";

                      goto Test1;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              // ****  Check to see if the monthly sum already exists   ****
              // : PR 104223 - Added support for Joint & Several.
              //   Process both sides of the J & S obligation and divide the 
              // amount by 2.
              //   That way each side of the J & S obligation will contribute 50
              // % of the amount for the Grant.
              if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
              {
                local.FcAmt.TotalCurrency =
                  Math.Round(
                    local.FcAmt.TotalCurrency /
                  2, 2, MidpointRounding.AwayFromZero);
              }

              if (ReadImHouseholdMbrMnthlySum())
              {
                try
                {
                  UpdateImHouseholdMbrMnthlySum();
                  ++local.CountsAndAmounts.NbrOfMoUrasUpdated.Count;
                  local.CountsAndAmounts.AmtOfMoUrasUpdated.
                    TotalCurrency += local.FcAmt.TotalCurrency;

                  if (AsChar(local.SetTriggers.Flag) == 'Y')
                  {
                    UseOeSetUraTrigger();
                  }
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_NU";

                      if (AsChar(local.IoDoneForThisDebtDtl.Flag) == 'Y')
                      {
                        goto Test2;
                      }
                      else
                      {
                        goto Test1;
                      }

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_PV";

                      if (AsChar(local.IoDoneForThisDebtDtl.Flag) == 'Y')
                      {
                        goto Test2;
                      }
                      else
                      {
                        goto Test1;
                      }

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
              else
              {
                try
                {
                  CreateImHouseholdMbrMnthlySum();
                  ++local.CountsAndAmounts.NbrOfMoUrasCreated.Count;
                  local.CountsAndAmounts.AmtOfMoUrasCreated.
                    TotalCurrency += local.FcAmt.TotalCurrency;

                  if (AsChar(local.SetTriggers.Flag) == 'Y')
                  {
                    UseOeSetUraTrigger();
                  }
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_AE";

                      if (AsChar(local.IoDoneForThisDebtDtl.Flag) == 'Y')
                      {
                        goto Test2;
                      }
                      else
                      {
                        goto Test1;
                      }

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_PV";

                      if (AsChar(local.IoDoneForThisDebtDtl.Flag) == 'Y')
                      {
                        goto Test2;
                      }
                      else
                      {
                        goto Test1;
                      }

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              local.IoDoneForThisDebtDtl.Flag = "Y";
            }
            else
            {
              ++local.CountsAndAmounts.NbrOfNonFcDebtDtls.Count;
            }
          }

          // **** Update the debt so that we do not process it again in the 
          // future.  ****
          try
          {
            UpdateDebt();
            ++local.CountsAndAmounts.NbrOfDebtDtlsUpdated.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DEBT_NU";

                if (AsChar(local.IoDoneForThisDebtDtl.Flag) == 'Y')
                {
                  goto Test2;
                }
                else
                {
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DEBT_PV";

                if (AsChar(local.IoDoneForThisDebtDtl.Flag) == 'Y')
                {
                  goto Test2;
                }
                else
                {
                }

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

Test1:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseOeB467DisplayDebtDtlInfo1();
          UseEabExtractExitStateMessage();
          local.EabReportSend1.RptDetail = "Error - Debt Dtl skipped: " + " - " +
            local.ExitStateMessage.Message;
          UseCabErrorReport1();

          if (!Equal(local.Receive.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ALL_OK";
          ++local.CountsAndAmounts.NbrOfErrors.Count;
          local.CountsAndAmounts.AmtOfErrors.TotalCurrency += local.FcAmt.
            TotalCurrency;
        }

        if (local.RecsProcessedSinceCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.RecsProcessedSinceCommit.Count = 0;

          if (AsChar(local.Test.TestRunInd.Flag) != 'Y')
          {
            try
            {
              UpdateControlTable();

              // Continue
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CONTROL_TABLE_VALUE_NU";

                  goto Test2;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CONTROL_TABLE_PV";

                  goto Test2;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              goto Test2;
            }
          }

          UseOeB467AccumulateTotals();
        }
        else
        {
          ++local.RecsProcessedSinceCommit.Count;
        }
      }
    }

Test2:

    // ****************************************************************
    // Commit any records not already committed.
    // ****************************************************************
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.Test.TestRunInd.Flag) != 'Y')
      {
        try
        {
          UpdateControlTable();

          // Continue
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CONTROL_TABLE_VALUE_NU";

              goto Test3;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CONTROL_TABLE_PV";

              goto Test3;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          goto Test3;
        }
      }

      UseOeB467AccumulateTotals();
    }

Test3:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.Test.TestRunInd.Flag) == 'Y')
      {
        ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
      }
      else
      {
        ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
      }
    }
    else
    {
      UseOeB467DisplayDebtDtlInfo1();
      UseEabExtractExitStateMessage();
      local.EabReportSend1.RptDetail = "Abort: " + " - " + local
        .ExitStateMessage.Message;
      UseCabErrorReport2();

      if (!Equal(local.Receive.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    UseOeB467PrintTotals();
    local.Send.Action = "CLOSE";
    local.EabReportSend1.RptDetail = "";
    UseCabControlReport();
    UseCabErrorReport3();
    local.AdabasExternalAction.Flag = "L";
    UseOeEabGetAeCaseNbrForPers2();
  }

  private static void MoveControlTable(ControlTable source, ControlTable target)
  {
    target.Identifier = source.Identifier;
    target.LastUsedNumber = source.LastUsedNumber;
  }

  private static void MoveCountsAndAmounts1(OeB467AccumulateTotals.Export.
    CountsAndAmountsGroup source, Local.CountsAndAmountsGroup target)
  {
    target.NbrOfDebtDtlsRead.Count = source.NbrOfDebtDtlsRead.Count;
    target.NbrOfDebtDtlsUpdated.Count = source.NbrOfDebtDtlsUpdated.Count;
    target.NbrOfFcDebtDtls.Count = source.NbrOfFcDebtDtls.Count;
    target.NbrOfNonFcDebtDtls.Count = source.NbrOfNonFcDebtDtls.Count;
    target.NbrOfImHhCreated.Count = source.NbrOfImHhCreated.Count;
    target.NbrOfMoUrasCreated.Count = source.NbrOfMoUrasCreated.Count;
    target.NbrOfMoUrasUpdated.Count = source.NbrOfMoUrasUpdated.Count;
    target.NbrOfErrors.Count = source.NbrOfErrors.Count;
    target.AmtOfFcDebtDtlsRead.TotalCurrency =
      source.AmtOfFcDebtDtlsRead.TotalCurrency;
    target.AmtOfMoUrasCreated.TotalCurrency =
      source.AmtOfMoUrasCreated.TotalCurrency;
    target.AmtOfMoUrasUpdated.TotalCurrency =
      source.AmtOfMoUrasUpdated.TotalCurrency;
    target.AmtOfErrors.TotalCurrency = source.AmtOfErrors.TotalCurrency;
  }

  private static void MoveCountsAndAmounts2(OeB467PrintTotals.Export.
    CountsAndAmountsGroup source, Local.TotCountsAndAmountsGroup target)
  {
    target.TotNbrOfDebtDtlsRead.Count = source.NbrOfDebtDtlsRead.Count;
    target.TotNbrOfDebtDtlsUpdated.Count = source.NbrOfDebtDtlsUpdated.Count;
    target.TotNbrOfFcDebtDtls.Count = source.NbrOfFcDebtDtls.Count;
    target.TotNbrOfNonFcDebtDtls.Count = source.NbrOfNonFcDebtDtls.Count;
    target.TotNbrOfImHhCreated.Count = source.NbrOfImHhCreated.Count;
    target.TotNbrOfMoUrasCreated.Count = source.NbrOfMoUrasCreated.Count;
    target.TotNbrOfMoUrasUpdated.Count = source.NbrOfMoUrasUpdated.Count;
    target.TotNbrOfErrors.Count = source.NbrOfErrors.Count;
    target.TotAmtOfFcDebtDtlsRead.TotalCurrency =
      source.AmtOfFcDebtDtlsRead.TotalCurrency;
    target.TotAmtOfMoUrasCreated.TotalCurrency =
      source.AmtOfMoUrasCreated.TotalCurrency;
    target.TotMatOfMoUrasUpdated.TotalCurrency =
      source.AmtOfMoUrasUpdated.TotalCurrency;
    target.TotAmtOfErrors.TotalCurrency = source.AmtOfErrors.TotalCurrency;
  }

  private static void MoveCountsAndAmounts3(Local.CountsAndAmountsGroup source,
    OeB467AccumulateTotals.Import.CountsAndAmountsGroup target)
  {
    target.NbrOfDebtDtlsRead.Count = source.NbrOfDebtDtlsRead.Count;
    target.NbrOfDebtDtlsUpdated.Count = source.NbrOfDebtDtlsUpdated.Count;
    target.NbrOfFcDebtDtls.Count = source.NbrOfFcDebtDtls.Count;
    target.NbrOfNonFcDebtDtls.Count = source.NbrOfNonFcDebtDtls.Count;
    target.NbrOfImHhCreated.Count = source.NbrOfImHhCreated.Count;
    target.NbrOfMoUrasCreated.Count = source.NbrOfMoUrasCreated.Count;
    target.NbrOfMoUrasUpdated.Count = source.NbrOfMoUrasUpdated.Count;
    target.NbrOfErrors.Count = source.NbrOfErrors.Count;
    target.AmtOfFcDebtDtlsRead.TotalCurrency =
      source.AmtOfFcDebtDtlsRead.TotalCurrency;
    target.AmtOfMoUrasCreated.TotalCurrency =
      source.AmtOfMoUrasCreated.TotalCurrency;
    target.AmtOfMoUrasUpdated.TotalCurrency =
      source.AmtOfMoUrasUpdated.TotalCurrency;
    target.AmtOfErrors.TotalCurrency = source.AmtOfErrors.TotalCurrency;
  }

  private static void MoveDebtDetail1(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.RetiredDt = source.RetiredDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveDebtDetail2(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveImHousehold(ImHousehold source, ImHousehold target)
  {
    target.AeCaseNo = source.AeCaseNo;
    target.FirstBenefitDate = source.FirstBenefitDate;
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveTest(OeB467Housekeeping.Export.TestGroup source,
    Local.TestGroup target)
  {
    target.TestRunInd.Flag = source.TestRunInd.Flag;
    target.TestDisplayInd.Flag = source.TestDisplayInd.Flag;
    target.TestFirstSup.Number = source.TestFirstSup.Number;
    target.TestLastSup.Number = source.TestLastSup.Number;
  }

  private static void MoveTotCountsAndAmounts1(OeB467AccumulateTotals.Export.
    TotCountsAndAmountsGroup source, Local.TotCountsAndAmountsGroup target)
  {
    target.TotNbrOfDebtDtlsRead.Count = source.TotNbrOfDebtDtlsRead.Count;
    target.TotNbrOfDebtDtlsUpdated.Count = source.TotNbrOfDebtDtlsUpdated.Count;
    target.TotNbrOfFcDebtDtls.Count = source.TotNbrOfFcDebtDtls.Count;
    target.TotNbrOfNonFcDebtDtls.Count = source.TotNbrOfNonFcDebtDtls.Count;
    target.TotNbrOfImHhCreated.Count = source.TotNbrOfImHhCreated.Count;
    target.TotNbrOfMoUrasCreated.Count = source.TotNbrOfMoUrasCreated.Count;
    target.TotNbrOfMoUrasUpdated.Count = source.TotNbrOfMoUrasUpdated.Count;
    target.TotNbrOfErrors.Count = source.TotNbrOfErrors.Count;
    target.TotAmtOfFcDebtDtlsRead.TotalCurrency =
      source.TotAmtOfFcDebtDtlsRead.TotalCurrency;
    target.TotAmtOfMoUrasCreated.TotalCurrency =
      source.TotAmtOfMoUrasCreated.TotalCurrency;
    target.TotMatOfMoUrasUpdated.TotalCurrency =
      source.TotAmtOfMoUrasUpdated.TotalCurrency;
    target.TotAmtOfErrors.TotalCurrency = source.TotAmtOfErrors.TotalCurrency;
  }

  private static void MoveTotCountsAndAmounts2(Local.
    TotCountsAndAmountsGroup source,
    OeB467AccumulateTotals.Export.TotCountsAndAmountsGroup target)
  {
    target.TotNbrOfDebtDtlsRead.Count = source.TotNbrOfDebtDtlsRead.Count;
    target.TotNbrOfDebtDtlsUpdated.Count = source.TotNbrOfDebtDtlsUpdated.Count;
    target.TotNbrOfFcDebtDtls.Count = source.TotNbrOfFcDebtDtls.Count;
    target.TotNbrOfNonFcDebtDtls.Count = source.TotNbrOfNonFcDebtDtls.Count;
    target.TotNbrOfImHhCreated.Count = source.TotNbrOfImHhCreated.Count;
    target.TotNbrOfMoUrasCreated.Count = source.TotNbrOfMoUrasCreated.Count;
    target.TotNbrOfMoUrasUpdated.Count = source.TotNbrOfMoUrasUpdated.Count;
    target.TotNbrOfErrors.Count = source.TotNbrOfErrors.Count;
    target.TotAmtOfFcDebtDtlsRead.TotalCurrency =
      source.TotAmtOfFcDebtDtlsRead.TotalCurrency;
    target.TotAmtOfMoUrasCreated.TotalCurrency =
      source.TotAmtOfMoUrasCreated.TotalCurrency;
    target.TotAmtOfMoUrasUpdated.TotalCurrency =
      source.TotMatOfMoUrasUpdated.TotalCurrency;
    target.TotAmtOfErrors.TotalCurrency = source.TotAmtOfErrors.TotalCurrency;
  }

  private static void MoveTotCountsAndAmounts3(Local.
    TotCountsAndAmountsGroup source,
    OeB467PrintTotals.Export.CountsAndAmountsGroup target)
  {
    target.NbrOfDebtDtlsRead.Count = source.TotNbrOfDebtDtlsRead.Count;
    target.NbrOfDebtDtlsUpdated.Count = source.TotNbrOfDebtDtlsUpdated.Count;
    target.NbrOfFcDebtDtls.Count = source.TotNbrOfFcDebtDtls.Count;
    target.NbrOfNonFcDebtDtls.Count = source.TotNbrOfNonFcDebtDtls.Count;
    target.NbrOfImHhCreated.Count = source.TotNbrOfImHhCreated.Count;
    target.NbrOfMoUrasCreated.Count = source.TotNbrOfMoUrasCreated.Count;
    target.NbrOfMoUrasUpdated.Count = source.TotNbrOfMoUrasUpdated.Count;
    target.NbrOfErrors.Count = source.TotNbrOfErrors.Count;
    target.AmtOfFcDebtDtlsRead.TotalCurrency =
      source.TotAmtOfFcDebtDtlsRead.TotalCurrency;
    target.AmtOfMoUrasCreated.TotalCurrency =
      source.TotAmtOfMoUrasCreated.TotalCurrency;
    target.AmtOfMoUrasUpdated.TotalCurrency =
      source.TotMatOfMoUrasUpdated.TotalCurrency;
    target.AmtOfErrors.TotalCurrency = source.TotAmtOfErrors.TotalCurrency;
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text80 = source.Text80;
    target.Text5 = source.Text5;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Send.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend1.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Send.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend1.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Receive.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Send.Action;

    useImport.NeededToWrite.RptDetail = local.EabReportSend1.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Receive.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Send.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend1.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateMessage.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateMessage.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveDebtDetail2(entities.DebtDetail, useImport.DebtDetail);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = entities.Supported.Number;
    useImport.Collection.Date = local.Collection.Date;
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccruing.Classification;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseOeB467AccumulateTotals()
  {
    var useImport = new OeB467AccumulateTotals.Import();
    var useExport = new OeB467AccumulateTotals.Export();

    MoveCountsAndAmounts3(local.CountsAndAmounts, useImport.CountsAndAmounts);
    MoveTotCountsAndAmounts2(local.TotCountsAndAmounts,
      useExport.TotCountsAndAmounts);

    Call(OeB467AccumulateTotals.Execute, useImport, useExport);

    MoveCountsAndAmounts1(useExport.CountsAndAmounts, local.CountsAndAmounts);
    MoveTotCountsAndAmounts1(useExport.TotCountsAndAmounts,
      local.TotCountsAndAmounts);
  }

  private void UseOeB467DisplayDebtDtlInfo1()
  {
    var useImport = new OeB467DisplayDebtDtlInfo.Import();
    var useExport = new OeB467DisplayDebtDtlInfo.Export();

    useImport.PerDebtDetail.Assign(entities.DebtDetail);
    useImport.PerDebt.Assign(entities.Debt);
    useImport.PerObligation.Assign(entities.Obligation);
    useImport.PerObligationType.Assign(entities.ObligationType);
    useImport.PerObligor.Assign(entities.ObligorCsePerson);
    useImport.PerSupported.Assign(entities.Supported);

    Call(OeB467DisplayDebtDtlInfo.Execute, useImport, useExport);

    MoveDebtDetail1(useImport.PerDebtDetail, entities.DebtDetail);
    MoveObligationTransaction(useImport.PerDebt, entities.Debt);
    entities.Obligation.SystemGeneratedIdentifier =
      useImport.PerObligation.SystemGeneratedIdentifier;
    MoveObligationType(useImport.PerObligationType, entities.ObligationType);
    entities.ObligorCsePerson.Number = useImport.PerObligor.Number;
    entities.Supported.Number = useImport.PerSupported.Number;
  }

  private void UseOeB467DisplayDebtDtlInfo2()
  {
    var useImport = new OeB467DisplayDebtDtlInfo.Import();
    var useExport = new OeB467DisplayDebtDtlInfo.Export();

    useImport.PerDebtDetail.Assign(entities.DebtDetail);
    useImport.PerDebt.Assign(entities.Debt);
    useImport.PerObligation.Assign(entities.Obligation);
    useImport.PerObligationType.Assign(entities.ObligationType);
    useImport.PerObligor.Assign(entities.ObligorCsePerson);
    useImport.PerSupported.Assign(entities.Supported);
    useImport.FcAmt.TotalCurrency = local.FcAmt.TotalCurrency;
    useImport.Program.Code = local.Program.Code;

    Call(OeB467DisplayDebtDtlInfo.Execute, useImport, useExport);

    MoveDebtDetail1(useImport.PerDebtDetail, entities.DebtDetail);
    MoveObligationTransaction(useImport.PerDebt, entities.Debt);
    entities.Obligation.SystemGeneratedIdentifier =
      useImport.PerObligation.SystemGeneratedIdentifier;
    MoveObligationType(useImport.PerObligationType, entities.ObligationType);
    entities.ObligorCsePerson.Number = useImport.PerObligor.Number;
    entities.Supported.Number = useImport.PerSupported.Number;
  }

  private void UseOeB467Housekeeping()
  {
    var useImport = new OeB467Housekeeping.Import();
    var useExport = new OeB467Housekeeping.Export();

    Call(OeB467Housekeeping.Execute, useImport, useExport);

    local.SetTriggers.Flag = useExport.SetTriggers.Flag;
    MoveTest(useExport.Test, local.Test);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseOeB467PrintTotals()
  {
    var useImport = new OeB467PrintTotals.Import();
    var useExport = new OeB467PrintTotals.Export();

    MoveTotCountsAndAmounts3(local.TotCountsAndAmounts,
      useExport.CountsAndAmounts);

    Call(OeB467PrintTotals.Execute, useImport, useExport);

    MoveCountsAndAmounts2(useExport.CountsAndAmounts, local.TotCountsAndAmounts);
      
  }

  private void UseOeEabGetAeCaseNbrForPers1()
  {
    var useImport = new OeEabGetAeCaseNbrForPers.Import();
    var useExport = new OeEabGetAeCaseNbrForPers.Export();

    useImport.CsePerson.Number = entities.Supported.Number;
    useImport.AdabasExternalAction.Flag = local.AdabasExternalAction.Flag;
    useImport.OnFcDate.Date = local.OnFcDate.Date;
    MoveWorkArea(local.ExecResults, useExport.ExecResults);
    MoveImHousehold(local.FromAeImHousehold, useExport.ImHousehold);
    useExport.ImHouseholdMbrMnthlySum.Relationship =
      local.FromAeImHouseholdMbrMnthlySum.Relationship;

    Call(OeEabGetAeCaseNbrForPers.Execute, useImport, useExport);

    MoveWorkArea(useExport.ExecResults, local.ExecResults);
    MoveImHousehold(useExport.ImHousehold, local.FromAeImHousehold);
    local.FromAeImHouseholdMbrMnthlySum.Relationship =
      useExport.ImHouseholdMbrMnthlySum.Relationship;
  }

  private void UseOeEabGetAeCaseNbrForPers2()
  {
    var useImport = new OeEabGetAeCaseNbrForPers.Import();
    var useExport = new OeEabGetAeCaseNbrForPers.Export();

    useImport.AdabasExternalAction.Flag = local.AdabasExternalAction.Flag;

    Call(OeEabGetAeCaseNbrForPers.Execute, useImport, useExport);
  }

  private void UseOeSetUraTrigger()
  {
    var useImport = new OeSetUraTrigger.Import();
    var useExport = new OeSetUraTrigger.Export();

    useImport.ImHousehold.AeCaseNo = entities.ImHousehold.AeCaseNo;
    MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum,
      useImport.ImHouseholdMbrMnthlySum);

    Call(OeSetUraTrigger.Execute, useImport, useExport);
  }

  private void CreateImHousehold1()
  {
    var aeCaseNo = local.ForCreate.AeCaseNo;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var firstBenefitDate = IntToDate(local.ForRead.Year * 10000 + local
      .ForRead.Month * 100 + 1);

    entities.ImHousehold.Populated = false;
    Update("CreateImHousehold1",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", aeCaseNo);
        db.SetNullableInt32(command, "householdSize", 0);
        db.SetString(command, "caseStatus", "");
        db.SetDate(command, "statusDate", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTimes", default(DateTime));
        db.SetNullableDate(command, "firstBenDate", firstBenefitDate);
        db.SetNullableString(command, "type", "");
      });

    entities.ImHousehold.AeCaseNo = aeCaseNo;
    entities.ImHousehold.CreatedBy = createdBy;
    entities.ImHousehold.CreatedTimestamp = createdTimestamp;
    entities.ImHousehold.FirstBenefitDate = firstBenefitDate;
    entities.ImHousehold.Populated = true;
  }

  private void CreateImHousehold2()
  {
    var aeCaseNo = local.FromAeImHousehold.AeCaseNo;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var firstBenefitDate = local.FromAeImHousehold.FirstBenefitDate;

    entities.ImHousehold.Populated = false;
    Update("CreateImHousehold2",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", aeCaseNo);
        db.SetNullableInt32(command, "householdSize", 0);
        db.SetString(command, "caseStatus", "");
        db.SetDate(command, "statusDate", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTimes", default(DateTime));
        db.SetNullableDate(command, "firstBenDate", firstBenefitDate);
        db.SetNullableString(command, "type", "");
      });

    entities.ImHousehold.AeCaseNo = aeCaseNo;
    entities.ImHousehold.CreatedBy = createdBy;
    entities.ImHousehold.CreatedTimestamp = createdTimestamp;
    entities.ImHousehold.FirstBenefitDate = firstBenefitDate;
    entities.ImHousehold.Populated = true;
  }

  private void CreateImHouseholdMbrMnthlySum()
  {
    var year = local.ForRead.Year;
    var month = local.ForRead.Month;
    var relationship = local.FromAeImHouseholdMbrMnthlySum.Relationship;
    var grantAmount = local.FcAmt.TotalCurrency;
    var grantMedicalAmount = 0M;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var imhAeCaseNo = entities.ImHousehold.AeCaseNo;
    var cspNumber = entities.Supported.Number;

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("CreateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "year0", year);
        db.SetInt32(command, "month0", month);
        db.SetString(command, "relationship", relationship);
        db.SetNullableDecimal(command, "grantAmt", grantAmount);
        db.SetNullableDecimal(command, "grantMedAmt", grantMedicalAmount);
        db.SetNullableDecimal(command, "uraAmount", grantAmount);
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
    entities.ImHouseholdMbrMnthlySum.UraAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.UraMedicalAmount = grantMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlySum.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = "";
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = null;
    entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlySum.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
  }

  private bool ReadControlTable()
  {
    entities.FcAeCaseNumber.Populated = false;

    return Read("ReadControlTable",
      null,
      (db, reader) =>
      {
        entities.FcAeCaseNumber.Identifier = db.GetString(reader, 0);
        entities.FcAeCaseNumber.LastUsedNumber = db.GetInt32(reader, 1);
        entities.FcAeCaseNumber.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 7);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 8);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 9);
        entities.DebtAdjustment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDebtDetailDebtObligationObligationTypeCsePerson()
  {
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.ObligorCsePerson.Populated = false;
    entities.Supported.Populated = false;

    return ReadEach("ReadDebtDetailDebtObligationObligationTypeCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "uraUpdProcDate",
          local.Initialized.Date.GetValueOrDefault());
        db.SetString(command, "number1", local.Test.TestFirstSup.Number);
        db.SetString(command, "number2", local.Test.TestLastSup.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 4);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 11);
        entities.Debt.Amount = db.GetDecimal(reader, 12);
        entities.Debt.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.Debt.LastUpdatedTmst = db.GetNullableDateTime(reader, 14);
        entities.Debt.DebtType = db.GetString(reader, 15);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 16);
        entities.Supported.Number = db.GetString(reader, 16);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 17);
        entities.Debt.UraUpdateProcDate = db.GetNullableDate(reader, 18);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 19);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.ObligorCsePerson.Populated = true;
        entities.Supported.Populated = true;

        return true;
      });
  }

  private bool ReadImHousehold1()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Supported.Number);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.CreatedBy = db.GetString(reader, 1);
        entities.ImHousehold.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ImHousehold.FirstBenefitDate = db.GetNullableDate(reader, 3);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadImHousehold2()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold2",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", local.FromAeImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.CreatedBy = db.GetString(reader, 1);
        entities.ImHousehold.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ImHousehold.FirstBenefitDate = db.GetNullableDate(reader, 3);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Supported.Number);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", local.ForRead.Year);
        db.SetInt32(command, "month0", local.ForRead.Month);
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
      });
  }

  private void UpdateControlTable()
  {
    var lastUsedNumber = local.FcAeCaseNumber.LastUsedNumber;

    entities.FcAeCaseNumber.Populated = false;
    Update("UpdateControlTable",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(command, "cntlTblId", entities.FcAeCaseNumber.Identifier);
      });

    entities.FcAeCaseNumber.LastUsedNumber = lastUsedNumber;
    entities.FcAeCaseNumber.Populated = true;
  }

  private void UpdateDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var uraUpdateProcDate = local.ProgramProcessingInfo.ProcessDate;

    entities.Debt.Populated = false;
    Update("UpdateDebt",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "uraUpdProcDate", uraUpdateProcDate);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.
          SetInt32(command, "obTrnId", entities.Debt.SystemGeneratedIdentifier);
          
        db.SetString(command, "obTrnTyp", entities.Debt.Type1);
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
      });

    entities.Debt.LastUpdatedBy = lastUpdatedBy;
    entities.Debt.LastUpdatedTmst = lastUpdatedTmst;
    entities.Debt.UraUpdateProcDate = uraUpdateProcDate;
    entities.Debt.Populated = true;
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var grantAmount =
      entities.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault() +
      local.FcAmt.TotalCurrency;
    var uraAmount =
      entities.ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault() +
      local.FcAmt.TotalCurrency;

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "grantAmt", grantAmount);
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetInt32(command, "year0", entities.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", entities.ImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.GrantAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
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
    /// <summary>A TestGroup group.</summary>
    [Serializable]
    public class TestGroup
    {
      /// <summary>
      /// A value of TestRunInd.
      /// </summary>
      [JsonPropertyName("testRunInd")]
      public Common TestRunInd
      {
        get => testRunInd ??= new();
        set => testRunInd = value;
      }

      /// <summary>
      /// A value of TestDisplayInd.
      /// </summary>
      [JsonPropertyName("testDisplayInd")]
      public Common TestDisplayInd
      {
        get => testDisplayInd ??= new();
        set => testDisplayInd = value;
      }

      /// <summary>
      /// A value of TestFirstSup.
      /// </summary>
      [JsonPropertyName("testFirstSup")]
      public CsePerson TestFirstSup
      {
        get => testFirstSup ??= new();
        set => testFirstSup = value;
      }

      /// <summary>
      /// A value of TestLastSup.
      /// </summary>
      [JsonPropertyName("testLastSup")]
      public CsePerson TestLastSup
      {
        get => testLastSup ??= new();
        set => testLastSup = value;
      }

      private Common testRunInd;
      private Common testDisplayInd;
      private CsePerson testFirstSup;
      private CsePerson testLastSup;
    }

    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsRead")]
      public Common NbrOfDebtDtlsRead
      {
        get => nbrOfDebtDtlsRead ??= new();
        set => nbrOfDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of NbrOfDebtDtlsUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsUpdated")]
      public Common NbrOfDebtDtlsUpdated
      {
        get => nbrOfDebtDtlsUpdated ??= new();
        set => nbrOfDebtDtlsUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfFcDebtDtls")]
      public Common NbrOfFcDebtDtls
      {
        get => nbrOfFcDebtDtls ??= new();
        set => nbrOfFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfNonFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfNonFcDebtDtls")]
      public Common NbrOfNonFcDebtDtls
      {
        get => nbrOfNonFcDebtDtls ??= new();
        set => nbrOfNonFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfImHhCreated.
      /// </summary>
      [JsonPropertyName("nbrOfImHhCreated")]
      public Common NbrOfImHhCreated
      {
        get => nbrOfImHhCreated ??= new();
        set => nbrOfImHhCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasCreated")]
      public Common NbrOfMoUrasCreated
      {
        get => nbrOfMoUrasCreated ??= new();
        set => nbrOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasUpdated")]
      public Common NbrOfMoUrasUpdated
      {
        get => nbrOfMoUrasUpdated ??= new();
        set => nbrOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfErrors.
      /// </summary>
      [JsonPropertyName("nbrOfErrors")]
      public Common NbrOfErrors
      {
        get => nbrOfErrors ??= new();
        set => nbrOfErrors = value;
      }

      /// <summary>
      /// A value of AmtOfFcDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("amtOfFcDebtDtlsRead")]
      public Common AmtOfFcDebtDtlsRead
      {
        get => amtOfFcDebtDtlsRead ??= new();
        set => amtOfFcDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasCreated")]
      public Common AmtOfMoUrasCreated
      {
        get => amtOfMoUrasCreated ??= new();
        set => amtOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasUpdated")]
      public Common AmtOfMoUrasUpdated
      {
        get => amtOfMoUrasUpdated ??= new();
        set => amtOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of AmtOfErrors.
      /// </summary>
      [JsonPropertyName("amtOfErrors")]
      public Common AmtOfErrors
      {
        get => amtOfErrors ??= new();
        set => amtOfErrors = value;
      }

      private Common nbrOfDebtDtlsRead;
      private Common nbrOfDebtDtlsUpdated;
      private Common nbrOfFcDebtDtls;
      private Common nbrOfNonFcDebtDtls;
      private Common nbrOfImHhCreated;
      private Common nbrOfMoUrasCreated;
      private Common nbrOfMoUrasUpdated;
      private Common nbrOfErrors;
      private Common amtOfFcDebtDtlsRead;
      private Common amtOfMoUrasCreated;
      private Common amtOfMoUrasUpdated;
      private Common amtOfErrors;
    }

    /// <summary>A TotCountsAndAmountsGroup group.</summary>
    [Serializable]
    public class TotCountsAndAmountsGroup
    {
      /// <summary>
      /// A value of TotNbrOfDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("totNbrOfDebtDtlsRead")]
      public Common TotNbrOfDebtDtlsRead
      {
        get => totNbrOfDebtDtlsRead ??= new();
        set => totNbrOfDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of TotNbrOfDebtDtlsUpdated.
      /// </summary>
      [JsonPropertyName("totNbrOfDebtDtlsUpdated")]
      public Common TotNbrOfDebtDtlsUpdated
      {
        get => totNbrOfDebtDtlsUpdated ??= new();
        set => totNbrOfDebtDtlsUpdated = value;
      }

      /// <summary>
      /// A value of TotNbrOfFcDebtDtls.
      /// </summary>
      [JsonPropertyName("totNbrOfFcDebtDtls")]
      public Common TotNbrOfFcDebtDtls
      {
        get => totNbrOfFcDebtDtls ??= new();
        set => totNbrOfFcDebtDtls = value;
      }

      /// <summary>
      /// A value of TotNbrOfNonFcDebtDtls.
      /// </summary>
      [JsonPropertyName("totNbrOfNonFcDebtDtls")]
      public Common TotNbrOfNonFcDebtDtls
      {
        get => totNbrOfNonFcDebtDtls ??= new();
        set => totNbrOfNonFcDebtDtls = value;
      }

      /// <summary>
      /// A value of TotNbrOfImHhCreated.
      /// </summary>
      [JsonPropertyName("totNbrOfImHhCreated")]
      public Common TotNbrOfImHhCreated
      {
        get => totNbrOfImHhCreated ??= new();
        set => totNbrOfImHhCreated = value;
      }

      /// <summary>
      /// A value of TotNbrOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("totNbrOfMoUrasCreated")]
      public Common TotNbrOfMoUrasCreated
      {
        get => totNbrOfMoUrasCreated ??= new();
        set => totNbrOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of TotNbrOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("totNbrOfMoUrasUpdated")]
      public Common TotNbrOfMoUrasUpdated
      {
        get => totNbrOfMoUrasUpdated ??= new();
        set => totNbrOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of TotNbrOfErrors.
      /// </summary>
      [JsonPropertyName("totNbrOfErrors")]
      public Common TotNbrOfErrors
      {
        get => totNbrOfErrors ??= new();
        set => totNbrOfErrors = value;
      }

      /// <summary>
      /// A value of TotAmtOfFcDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("totAmtOfFcDebtDtlsRead")]
      public Common TotAmtOfFcDebtDtlsRead
      {
        get => totAmtOfFcDebtDtlsRead ??= new();
        set => totAmtOfFcDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of TotAmtOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("totAmtOfMoUrasCreated")]
      public Common TotAmtOfMoUrasCreated
      {
        get => totAmtOfMoUrasCreated ??= new();
        set => totAmtOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of TotMatOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("totMatOfMoUrasUpdated")]
      public Common TotMatOfMoUrasUpdated
      {
        get => totMatOfMoUrasUpdated ??= new();
        set => totMatOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of TotAmtOfErrors.
      /// </summary>
      [JsonPropertyName("totAmtOfErrors")]
      public Common TotAmtOfErrors
      {
        get => totAmtOfErrors ??= new();
        set => totAmtOfErrors = value;
      }

      private Common totNbrOfDebtDtlsRead;
      private Common totNbrOfDebtDtlsUpdated;
      private Common totNbrOfFcDebtDtls;
      private Common totNbrOfNonFcDebtDtls;
      private Common totNbrOfImHhCreated;
      private Common totNbrOfMoUrasCreated;
      private Common totNbrOfMoUrasUpdated;
      private Common totNbrOfErrors;
      private Common totAmtOfFcDebtDtlsRead;
      private Common totAmtOfMoUrasCreated;
      private Common totMatOfMoUrasUpdated;
      private Common totAmtOfErrors;
    }

    /// <summary>
    /// A value of ReturnedDateFromAe.
    /// </summary>
    [JsonPropertyName("returnedDateFromAe")]
    public WorkArea ReturnedDateFromAe
    {
      get => returnedDateFromAe ??= new();
      set => returnedDateFromAe = value;
    }

    /// <summary>
    /// A value of SetTriggers.
    /// </summary>
    [JsonPropertyName("setTriggers")]
    public Common SetTriggers
    {
      get => setTriggers ??= new();
      set => setTriggers = value;
    }

    /// <summary>
    /// A value of ExecResults.
    /// </summary>
    [JsonPropertyName("execResults")]
    public WorkArea ExecResults
    {
      get => execResults ??= new();
      set => execResults = value;
    }

    /// <summary>
    /// A value of AdabasExternalAction.
    /// </summary>
    [JsonPropertyName("adabasExternalAction")]
    public Common AdabasExternalAction
    {
      get => adabasExternalAction ??= new();
      set => adabasExternalAction = value;
    }

    /// <summary>
    /// A value of OnFcDate.
    /// </summary>
    [JsonPropertyName("onFcDate")]
    public DateWorkArea OnFcDate
    {
      get => onFcDate ??= new();
      set => onFcDate = value;
    }

    /// <summary>
    /// A value of FcAmt.
    /// </summary>
    [JsonPropertyName("fcAmt")]
    public Common FcAmt
    {
      get => fcAmt ??= new();
      set => fcAmt = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public ImHousehold ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// A value of RecsProcessedSinceCommit.
    /// </summary>
    [JsonPropertyName("recsProcessedSinceCommit")]
    public Common RecsProcessedSinceCommit
    {
      get => recsProcessedSinceCommit ??= new();
      set => recsProcessedSinceCommit = value;
    }

    /// <summary>
    /// A value of IoDoneForThisDebtDtl.
    /// </summary>
    [JsonPropertyName("ioDoneForThisDebtDtl")]
    public Common IoDoneForThisDebtDtl
    {
      get => ioDoneForThisDebtDtl ??= new();
      set => ioDoneForThisDebtDtl = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of FcAeCaseNumber.
    /// </summary>
    [JsonPropertyName("fcAeCaseNumber")]
    public ControlTable FcAeCaseNumber
    {
      get => fcAeCaseNumber ??= new();
      set => fcAeCaseNumber = value;
    }

    /// <summary>
    /// A value of ForRead.
    /// </summary>
    [JsonPropertyName("forRead")]
    public ImHouseholdMbrMnthlySum ForRead
    {
      get => forRead ??= new();
      set => forRead = value;
    }

    /// <summary>
    /// A value of FromAeImHousehold.
    /// </summary>
    [JsonPropertyName("fromAeImHousehold")]
    public ImHousehold FromAeImHousehold
    {
      get => fromAeImHousehold ??= new();
      set => fromAeImHousehold = value;
    }

    /// <summary>
    /// A value of FromAeImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("fromAeImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum FromAeImHouseholdMbrMnthlySum
    {
      get => fromAeImHouseholdMbrMnthlySum ??= new();
      set => fromAeImHouseholdMbrMnthlySum = value;
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
    /// Gets a value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public TestGroup Test
    {
      get => test ?? (test = new());
      set => test = value;
    }

    /// <summary>
    /// A value of FdsoInfrastructure.
    /// </summary>
    [JsonPropertyName("fdsoInfrastructure")]
    public Infrastructure FdsoInfrastructure
    {
      get => fdsoInfrastructure ??= new();
      set => fdsoInfrastructure = value;
    }

    /// <summary>
    /// A value of FdsoDocument.
    /// </summary>
    [JsonPropertyName("fdsoDocument")]
    public Document FdsoDocument
    {
      get => fdsoDocument ??= new();
      set => fdsoDocument = value;
    }

    /// <summary>
    /// A value of FdsoLetter.
    /// </summary>
    [JsonPropertyName("fdsoLetter")]
    public SpDocKey FdsoLetter
    {
      get => fdsoLetter ??= new();
      set => fdsoLetter = value;
    }

    /// <summary>
    /// A value of CountErrorsForAbend.
    /// </summary>
    [JsonPropertyName("countErrorsForAbend")]
    public Common CountErrorsForAbend
    {
      get => countErrorsForAbend ??= new();
      set => countErrorsForAbend = value;
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
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
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
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
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
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public EabFileHandling Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of Receive.
    /// </summary>
    [JsonPropertyName("receive")]
    public EabFileHandling Receive
    {
      get => receive ??= new();
      set => receive = value;
    }

    /// <summary>
    /// A value of EabReportSend1.
    /// </summary>
    [JsonPropertyName("eabReportSend1")]
    public EabReportSend EabReportSend1
    {
      get => eabReportSend1 ??= new();
      set => eabReportSend1 = value;
    }

    /// <summary>
    /// A value of ExitStateMessage.
    /// </summary>
    [JsonPropertyName("exitStateMessage")]
    public ExitStateWorkArea ExitStateMessage
    {
      get => exitStateMessage ??= new();
      set => exitStateMessage = value;
    }

    /// <summary>
    /// Gets a value of CountsAndAmounts.
    /// </summary>
    [JsonPropertyName("countsAndAmounts")]
    public CountsAndAmountsGroup CountsAndAmounts
    {
      get => countsAndAmounts ?? (countsAndAmounts = new());
      set => countsAndAmounts = value;
    }

    /// <summary>
    /// Gets a value of TotCountsAndAmounts.
    /// </summary>
    [JsonPropertyName("totCountsAndAmounts")]
    public TotCountsAndAmountsGroup TotCountsAndAmounts
    {
      get => totCountsAndAmounts ?? (totCountsAndAmounts = new());
      set => totCountsAndAmounts = value;
    }

    /// <summary>
    /// A value of AbendCheckLoop.
    /// </summary>
    [JsonPropertyName("abendCheckLoop")]
    public Common AbendCheckLoop
    {
      get => abendCheckLoop ??= new();
      set => abendCheckLoop = value;
    }

    /// <summary>
    /// A value of ErrorLoop.
    /// </summary>
    [JsonPropertyName("errorLoop")]
    public Common ErrorLoop
    {
      get => errorLoop ??= new();
      set => errorLoop = value;
    }

    /// <summary>
    /// A value of EabReportSend2.
    /// </summary>
    [JsonPropertyName("eabReportSend2")]
    public EabReportSend EabReportSend2
    {
      get => eabReportSend2 ??= new();
      set => eabReportSend2 = value;
    }

    private WorkArea returnedDateFromAe;
    private Common setTriggers;
    private WorkArea execResults;
    private Common adabasExternalAction;
    private DateWorkArea onFcDate;
    private Common fcAmt;
    private ImHousehold forCreate;
    private Common recsProcessedSinceCommit;
    private Common ioDoneForThisDebtDtl;
    private DateWorkArea collection;
    private Program program;
    private ControlTable fcAeCaseNumber;
    private ImHouseholdMbrMnthlySum forRead;
    private ImHousehold fromAeImHousehold;
    private ImHouseholdMbrMnthlySum fromAeImHouseholdMbrMnthlySum;
    private DateWorkArea initialized;
    private TestGroup test;
    private Infrastructure fdsoInfrastructure;
    private Document fdsoDocument;
    private SpDocKey fdsoLetter;
    private Common countErrorsForAbend;
    private Common processCountToCommit;
    private ObligationType hardcodedAccruing;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common numberOfUpdates;
    private External passArea;
    private EabFileHandling send;
    private EabFileHandling receive;
    private EabReportSend eabReportSend1;
    private ExitStateWorkArea exitStateMessage;
    private CountsAndAmountsGroup countsAndAmounts;
    private TotCountsAndAmountsGroup totCountsAndAmounts;
    private Common abendCheckLoop;
    private Common errorLoop;
    private EabReportSend eabReportSend2;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePersonAccount SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
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
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of FcAeCaseNumber.
    /// </summary>
    [JsonPropertyName("fcAeCaseNumber")]
    public ControlTable FcAeCaseNumber
    {
      get => fcAeCaseNumber ??= new();
      set => fcAeCaseNumber = value;
    }

    private ObligationTransactionRln obligationTransactionRln;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction debtAdjustment;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson supported;
    private CsePersonAccount supportedPerson;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ControlTable fcAeCaseNumber;
  }
#endregion
}

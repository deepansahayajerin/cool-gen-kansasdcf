// Program: FN_COMPUTE_SUMMARY_TOTALS_DTL_2, ID: 1902558078, model: 746.
// Short name: SWE03759
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_COMPUTE_SUMMARY_TOTALS_DTL_2.
/// </summary>
[Serializable]
public partial class FnComputeSummaryTotalsDtl2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMPUTE_SUMMARY_TOTALS_DTL_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnComputeSummaryTotalsDtl2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnComputeSummaryTotalsDtl2.
  /// </summary>
  public FnComputeSummaryTotalsDtl2(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------
    // CQ# 50344  JHarden  7/21/2016  Show AJ, MJ, and 718B Debts on Screen. 
    // Change MD arrears to MS arrears, add Spousal arrears (SAJ), and Cost of
    // Raising Child (CRCH). The amounts in all of the fields should be totaled
    // for the Total Pay Off field.
    // PR# 75972      VITHAL MADHIRA      01/20/2000
    // Modified th SORT order in READ EACH COLLECTION ..... statement below. 
    // Presently it SORTs by  ascending OBLIGATION_TYPE
    // System_Generated_Identifier. Due to this the 'Amt Last Pymt' and 'Date
    // Last Payment' fields on QCOL screen is displaying  incorrect data. To
    // correct this problem,  I changed the SORT to ascending COLLECTION '
    // Collection_Date'.
    // -------------------------------------------------------------------------------------
    if (IsEmpty(import.Obligor.Number))
    {
      ExitState = "FN0000_CSE_PERSON_NOT_PASSED";

      return;
    }

    export.ScreenOwedAmountsDtl.IncomingInterstateObExists = "N";
    UseFnHardcodedCashReceipting();
    UseFnHardcodedDebtDistribution();
    local.Process.Date = Now().Date;
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();
    UseCabFirstAndLastDateOfMonth();

    if (import.Filter.SystemGeneratedIdentifier == 0)
    {
      local.LowLimit.SystemGeneratedIdentifier = 0;
      local.HighLimit.SystemGeneratedIdentifier = 999;
      local.OmitSecondaryObligInd.Flag = "Y";
    }
    else
    {
      local.LowLimit.SystemGeneratedIdentifier =
        import.Filter.SystemGeneratedIdentifier;
      local.HighLimit.SystemGeneratedIdentifier =
        import.Filter.SystemGeneratedIdentifier;
      local.OmitSecondaryObligInd.Flag = "N";
    }

    foreach(var item in ReadObligationTypeObligationDebtDebtDetail())
    {
      if (AsChar(local.OmitSecondaryObligInd.Flag) == 'Y')
      {
        if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
          (local.HardcodedSecondary.PrimarySecondaryCode))
        {
          continue;
        }
      }

      if (import.FilterByIdObligationType.SystemGeneratedIdentifier != 0)
      {
        if (import.FilterByIdObligationType.SystemGeneratedIdentifier != entities
          .ExistingObligationType.SystemGeneratedIdentifier)
        {
          continue;
        }
      }

      if (!IsEmpty(import.FilterByClass.Classification))
      {
        if (AsChar(import.FilterByClass.Classification) != AsChar
          (entities.ExistingObligationType.Classification))
        {
          continue;
        }
      }

      if (import.FilterByIdLegalAction.Identifier != 0)
      {
        if (!ReadLegalAction1())
        {
          continue;
        }
      }
      else if (!IsEmpty(import.FilterByStdNo.StandardNumber))
      {
        if (!ReadLegalAction2())
        {
          continue;
        }
      }

      if (AsChar(entities.ExistingObligation.OrderTypeCode) == 'I')
      {
        export.ScreenOwedAmountsDtl.IncomingInterstateObExists = "Y";
      }

      if (AsChar(entities.ExistingObligationType.SupportedPersonReqInd) == 'Y')
      {
        if (!ReadCsePerson())
        {
          ExitState = "FN0000_SUPP_PERSON_NF";

          return;
        }
      }

      if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == 'J')
      {
        export.MultiJoint.Text1 = "J";
      }

      // CQ 50344
      if (AsChar(entities.ExistingObligationType.Classification) == 'A')
      {
        if (!Lt(entities.ExistingDebtDetail.DueDt, local.ProcessMonthBegin.Date))
          
        {
          if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
            .HardcodedCsType.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.CsCurrDue += entities.ExistingDebt.
              Amount;
            export.ScreenOwedAmountsDtl.CsCurrOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
          }
          else if (entities.ExistingObligationType.SystemGeneratedIdentifier ==
            local.HardcodedSpType.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.SpCurrDue += entities.ExistingDebt.
              Amount;
            export.ScreenOwedAmountsDtl.SpCurrOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
          }
          else if (entities.ExistingObligationType.SystemGeneratedIdentifier ==
            local.HardcodedMsType.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.MsCurrDue += entities.ExistingDebt.
              Amount;
            export.ScreenOwedAmountsDtl.MsCurrOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
          }
          else if (entities.ExistingObligationType.SystemGeneratedIdentifier ==
            local.HardcodedMcType.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.McCurrDue += entities.ExistingDebt.
              Amount;
            export.ScreenOwedAmountsDtl.McCurrOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
          }

          export.ScreenOwedAmountsDtl.TotalCurrDue += entities.ExistingDebt.
            Amount;
          export.ScreenOwedAmountsDtl.TotalCurrOwed += entities.
            ExistingDebtDetail.BalanceDueAmt;
          export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed += entities.
            ExistingDebtDetail.BalanceDueAmt;
        }
        else
        {
          if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
            .HardcodedCsType.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.CsCurrArrears += entities.
              ExistingDebtDetail.BalanceDueAmt;
          }
          else if (entities.ExistingObligationType.SystemGeneratedIdentifier ==
            local.HardcodedSpType.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.SpCurrArrears += entities.
              ExistingDebtDetail.BalanceDueAmt;
          }
          else if (entities.ExistingObligationType.SystemGeneratedIdentifier ==
            local.HardcodedMsType.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.MsCurrArrears += entities.
              ExistingDebtDetail.BalanceDueAmt;
          }
          else
          {
            continue;
          }

          UseFnDeterminePgmForDebtDetail();

          switch(TrimEnd(local.Program.Code))
          {
            case "AF":
              switch(TrimEnd(local.DprProgram.ProgramState))
              {
                case "PA":
                  export.ScreenOwedAmountsDtl.AfPaArrearsOwed += entities.
                    ExistingDebtDetail.BalanceDueAmt;
                  export.ScreenOwedAmountsDtl.AfPaInterestOwed += entities.
                    ExistingDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault();

                  break;
                case "TA":
                  export.ScreenOwedAmountsDtl.AfTaArrearsOwed += entities.
                    ExistingDebtDetail.BalanceDueAmt;
                  export.ScreenOwedAmountsDtl.AfTaInterestOwed += entities.
                    ExistingDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault();

                  break;
                case "CA":
                  export.ScreenOwedAmountsDtl.AfCaArrearsOwed += entities.
                    ExistingDebtDetail.BalanceDueAmt;
                  export.ScreenOwedAmountsDtl.AfCaInterestOwed += entities.
                    ExistingDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault();

                  break;
                default:
                  break;
              }

              break;
            case "AFI":
              export.ScreenOwedAmountsDtl.AfiArrearsOwed += entities.
                ExistingDebtDetail.BalanceDueAmt;
              export.ScreenOwedAmountsDtl.AfiInterestOwed += entities.
                ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

              break;
            case "FC":
              if (Equal(local.DprProgram.ProgramState, "PA"))
              {
                export.ScreenOwedAmountsDtl.FcPaArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.FcPaInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
              }
              else
              {
              }

              break;
            case "FCI":
              export.ScreenOwedAmountsDtl.FciArrearsOwed += entities.
                ExistingDebtDetail.BalanceDueAmt;
              export.ScreenOwedAmountsDtl.FciInterestOwed += entities.
                ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

              break;
            case "NA":
              switch(TrimEnd(local.DprProgram.ProgramState))
              {
                case "NA":
                  export.ScreenOwedAmountsDtl.NaNaArrearsOwed += entities.
                    ExistingDebtDetail.BalanceDueAmt;
                  export.ScreenOwedAmountsDtl.NaNaInterestOwed += entities.
                    ExistingDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault();

                  break;
                case "UP":
                  export.ScreenOwedAmountsDtl.NaUpArrearsOwed += entities.
                    ExistingDebtDetail.BalanceDueAmt;
                  export.ScreenOwedAmountsDtl.NaUpInterestOwed += entities.
                    ExistingDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault();

                  break;
                case "UD":
                  export.ScreenOwedAmountsDtl.NaUdArrearsOwed += entities.
                    ExistingDebtDetail.BalanceDueAmt;
                  export.ScreenOwedAmountsDtl.NaUdInterestOwed += entities.
                    ExistingDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault();

                  break;
                case "CA":
                  export.ScreenOwedAmountsDtl.NaCaArrearsOwed += entities.
                    ExistingDebtDetail.BalanceDueAmt;
                  export.ScreenOwedAmountsDtl.NaCaInterestOwed += entities.
                    ExistingDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault();

                  break;
                default:
                  break;
              }

              break;
            case "NAI":
              export.ScreenOwedAmountsDtl.NaiArrearsOwed += entities.
                ExistingDebtDetail.BalanceDueAmt;
              export.ScreenOwedAmountsDtl.NaiInterestOwed += entities.
                ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

              break;
            case "NF":
              export.ScreenOwedAmountsDtl.NfArrearsOwed += entities.
                ExistingDebtDetail.BalanceDueAmt;
              export.ScreenOwedAmountsDtl.NfInterestOwed += entities.
                ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

              break;
            case "NC":
              export.ScreenOwedAmountsDtl.NcArrearsOwed += entities.
                ExistingDebtDetail.BalanceDueAmt;
              export.ScreenOwedAmountsDtl.NcInterestOwed += entities.
                ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

              break;
            default:
              break;
          }

          export.ScreenOwedAmountsDtl.TotalArrearsOwed += entities.
            ExistingDebtDetail.BalanceDueAmt;
          export.ScreenOwedAmountsDtl.TotalInterestOwed += entities.
            ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed =
            export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed + entities
            .ExistingDebtDetail.BalanceDueAmt + entities
            .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        }

        // "M" = MJ, Medical Judgement
        // "N" = IJ, Interest Judgement / AJ, Arrears Judgement / CRCH, Cost of 
        // Raising Child / SAJ, Spousal Arrears Judgement /
        //       718B, 718B Judgement / WA, Withholding Arrears
      }
      else if (AsChar(entities.ExistingObligationType.Classification) == 'M'
        || AsChar(entities.ExistingObligationType.Classification) == 'N')
      {
        // CQ 50344 add AJ, MJ, and 718B to screen
        if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
          .MedicalJudgement.SystemGeneratedIdentifier)
        {
          export.MedicalJudgement.TotalCurrency += entities.ExistingDebtDetail.
            BalanceDueAmt;
        }
        else if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
          .ArrearsJudgement.SystemGeneratedIdentifier)
        {
          export.ArrearsJudgement.TotalCurrency += entities.ExistingDebtDetail.
            BalanceDueAmt;
        }
        else if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
          .Local718BUraJudgement.SystemGeneratedIdentifier)
        {
          export.Export718BJudgement.TotalCurrency += entities.
            ExistingDebtDetail.BalanceDueAmt;
        }
        else if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
          .SpousalArrearsJudgement.SystemGeneratedIdentifier)
        {
          export.SpousalArrears.TotalCurrency += entities.ExistingDebtDetail.
            BalanceDueAmt;
        }
        else if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
          .OtCostOfRasingChild.SystemGeneratedIdentifier)
        {
          export.CostOfRaisingChild.TotalCurrency += entities.
            ExistingDebtDetail.BalanceDueAmt;
        }

        UseFnDeterminePgmForDebtDetail();

        switch(TrimEnd(local.Program.Code))
        {
          case "AF":
            switch(TrimEnd(local.DprProgram.ProgramState))
            {
              case "PA":
                export.ScreenOwedAmountsDtl.AfPaArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.AfPaInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

                break;
              case "TA":
                export.ScreenOwedAmountsDtl.AfTaArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.AfTaInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

                break;
              case "CA":
                export.ScreenOwedAmountsDtl.AfCaArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.AfCaInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

                break;
              default:
                break;
            }

            break;
          case "AFI":
            export.ScreenOwedAmountsDtl.AfiArrearsOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
            export.ScreenOwedAmountsDtl.AfiInterestOwed += entities.
              ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

            break;
          case "FC":
            if (Equal(local.DprProgram.ProgramState, "PA"))
            {
              export.ScreenOwedAmountsDtl.FcPaArrearsOwed += entities.
                ExistingDebtDetail.BalanceDueAmt;
              export.ScreenOwedAmountsDtl.FcPaInterestOwed += entities.
                ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
            }
            else
            {
            }

            break;
          case "FCI":
            export.ScreenOwedAmountsDtl.FciArrearsOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
            export.ScreenOwedAmountsDtl.FciInterestOwed += entities.
              ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

            break;
          case "NA":
            switch(TrimEnd(local.DprProgram.ProgramState))
            {
              case "NA":
                export.ScreenOwedAmountsDtl.NaNaArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.NaNaInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

                break;
              case "UP":
                export.ScreenOwedAmountsDtl.NaUpArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.NaUpInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

                break;
              case "UD":
                export.ScreenOwedAmountsDtl.NaUdArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.NaUdInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

                break;
              case "CA":
                export.ScreenOwedAmountsDtl.NaCaArrearsOwed += entities.
                  ExistingDebtDetail.BalanceDueAmt;
                export.ScreenOwedAmountsDtl.NaCaInterestOwed += entities.
                  ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

                break;
              default:
                break;
            }

            break;
          case "NAI":
            export.ScreenOwedAmountsDtl.NaiArrearsOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
            export.ScreenOwedAmountsDtl.NaiInterestOwed += entities.
              ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

            break;
          case "NF":
            export.ScreenOwedAmountsDtl.NfArrearsOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
            export.ScreenOwedAmountsDtl.NfInterestOwed += entities.
              ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

            break;
          case "NC":
            export.ScreenOwedAmountsDtl.NcArrearsOwed += entities.
              ExistingDebtDetail.BalanceDueAmt;
            export.ScreenOwedAmountsDtl.NcInterestOwed += entities.
              ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

            break;
          default:
            break;
        }

        export.ScreenOwedAmountsDtl.TotalArrearsOwed += entities.
          ExistingDebtDetail.BalanceDueAmt;
        export.ScreenOwedAmountsDtl.TotalInterestOwed += entities.
          ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed =
          export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed + entities
          .ExistingDebtDetail.BalanceDueAmt + entities
          .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

        if (entities.ExistingObligation.SystemGeneratedIdentifier != local
          .Hold.SystemGeneratedIdentifier)
        {
          local.Hold.SystemGeneratedIdentifier =
            entities.ExistingObligation.SystemGeneratedIdentifier;

          if (ReadObligationPaymentSchedule())
          {
            local.Tmp1.TotalCurrency =
              entities.ExistingObligationPaymentSchedule.Amount.
                GetValueOrDefault();
            UseFnCalculateMonthlyAmountDue();
            export.ScreenOwedAmountsDtl.PeriodicPymntDue += local.Tmp1.
              TotalCurrency;
            ReadCollection();
            export.ScreenOwedAmountsDtl.PeriodicPymntColl += local.Tmp1.
              TotalCurrency;
          }
          else
          {
            // : No Payment Schedule Found - Continue Processing.
          }
        }
      }
      else if (AsChar(entities.ExistingObligationType.Classification) == 'R')
      {
        export.ScreenOwedAmountsDtl.RecoveryArrearsOwed += entities.
          ExistingDebtDetail.BalanceDueAmt;
        export.ScreenOwedAmountsDtl.TotalArrearsOwed += entities.
          ExistingDebtDetail.BalanceDueAmt;
        export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed =
          export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed + entities
          .ExistingDebtDetail.BalanceDueAmt + entities
          .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

        if (entities.ExistingObligation.SystemGeneratedIdentifier != local
          .Hold.SystemGeneratedIdentifier)
        {
          local.Hold.SystemGeneratedIdentifier =
            entities.ExistingObligation.SystemGeneratedIdentifier;

          if (ReadObligationPaymentSchedule())
          {
            local.Tmp1.TotalCurrency =
              entities.ExistingObligationPaymentSchedule.Amount.
                GetValueOrDefault();
            UseFnCalculateMonthlyAmountDue();
            export.ScreenOwedAmountsDtl.PeriodicPymntDue += local.Tmp1.
              TotalCurrency;
            ReadCollection();
            export.ScreenOwedAmountsDtl.PeriodicPymntColl += local.Tmp1.
              TotalCurrency;
          }
          else
          {
            // : No Payment Schedule Found - Continue Processing.
          }
        }
      }
      else if (AsChar(entities.ExistingObligationType.Classification) == 'F')
      {
        export.ScreenOwedAmountsDtl.FeesArrearsOwed += entities.
          ExistingDebtDetail.BalanceDueAmt;
        export.ScreenOwedAmountsDtl.FeesInterestOwed += entities.
          ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        export.ScreenOwedAmountsDtl.TotalArrearsOwed += entities.
          ExistingDebtDetail.BalanceDueAmt;
        export.ScreenOwedAmountsDtl.TotalInterestOwed += entities.
          ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed =
          export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed + entities
          .ExistingDebtDetail.BalanceDueAmt + entities
          .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

        if (entities.ExistingObligation.SystemGeneratedIdentifier != local
          .Hold.SystemGeneratedIdentifier)
        {
          local.Hold.SystemGeneratedIdentifier =
            entities.ExistingObligation.SystemGeneratedIdentifier;

          if (ReadObligationPaymentSchedule())
          {
            local.Tmp1.TotalCurrency =
              entities.ExistingObligationPaymentSchedule.Amount.
                GetValueOrDefault();
            UseFnCalculateMonthlyAmountDue();
            export.ScreenOwedAmountsDtl.PeriodicPymntDue += local.Tmp1.
              TotalCurrency;
            ReadCollection();
            export.ScreenOwedAmountsDtl.PeriodicPymntColl += local.Tmp1.
              TotalCurrency;
          }
          else
          {
            // : No Payment Schedule Found - Continue Processing.
          }
        }
      }
    }

    if (export.ScreenOwedAmountsDtl.PeriodicPymntColl > export
      .ScreenOwedAmountsDtl.PeriodicPymntDue)
    {
      export.ScreenOwedAmountsDtl.PeriodicPymntColl =
        export.ScreenOwedAmountsDtl.PeriodicPymntDue;
    }

    export.ScreenOwedAmountsDtl.PeriodicPymntOwed =
      export.ScreenOwedAmountsDtl.PeriodicPymntDue - export
      .ScreenOwedAmountsDtl.PeriodicPymntColl;

    if (AsChar(import.OmitCollectionDtlsInd.Flag) != 'Y')
    {
      // ---------------------------------------------------------------------------
      // Per PR# 75972 the SORT order is changed to ascending COLLECTION '
      // Collection Date'.
      //                                                     
      // --- Vithal (01-20-2000)
      // --------------------------------------------------------------------------
      foreach(var item in ReadCollectionObligationTypeObligation())
      {
        if (AsChar(local.OmitSecondaryObligInd.Flag) == 'Y')
        {
          if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
            (local.HardcodedSecondary.PrimarySecondaryCode))
          {
            continue;
          }
        }

        if (import.FilterByIdObligationType.SystemGeneratedIdentifier != 0)
        {
          if (import.FilterByIdObligationType.SystemGeneratedIdentifier != entities
            .ExistingObligationType.SystemGeneratedIdentifier)
          {
            continue;
          }
        }

        if (!IsEmpty(import.FilterByClass.Classification))
        {
          if (AsChar(import.FilterByClass.Classification) != AsChar
            (entities.ExistingObligationType.Classification))
          {
            continue;
          }
        }

        if (import.FilterByIdLegalAction.Identifier != 0)
        {
          if (!ReadLegalAction1())
          {
            continue;
          }
        }
        else if (!IsEmpty(import.FilterByStdNo.StandardNumber))
        {
          if (!ReadLegalAction2())
          {
            continue;
          }
        }

        // : Skip any Collection that was applied to a J & S Obligation where 
        // the Payor is not the Obligor.
        if (AsChar(entities.ExistingCollection.ConcurrentInd) == 'Y')
        {
          if (!ReadCashReceiptDetail())
          {
            continue;
          }
        }

        if (Lt(export.ScreenOwedAmountsDtl.LastCollDt,
          entities.ExistingCollection.CollectionDt))
        {
          export.ScreenOwedAmountsDtl.LastCollDt =
            entities.ExistingCollection.CollectionDt;
          export.ScreenOwedAmountsDtl.LastCollAmt =
            entities.ExistingCollection.Amount;
        }
        else if (Equal(entities.ExistingCollection.CollectionDt,
          export.ScreenOwedAmountsDtl.LastCollDt))
        {
          export.ScreenOwedAmountsDtl.LastCollAmt += entities.
            ExistingCollection.Amount;
        }

        if (AsChar(entities.ExistingCollection.AppliedToCode) == 'C')
        {
          if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
            .HardcodedVoluntary.SystemGeneratedIdentifier)
          {
            export.ScreenOwedAmountsDtl.TotalVoluntaryColl += entities.
              ExistingCollection.Amount;
          }
          else if (!Lt(entities.ExistingCollection.CollectionDt,
            local.ProcessMonthBegin.Date) && !
            Lt(local.ProcessMonthEnd.Date,
            entities.ExistingCollection.CollectionDt))
          {
            if (AsChar(entities.ExistingCollection.AppliedToFuture) == 'Y')
            {
              export.ScreenOwedAmountsDtl.FutureColl += entities.
                ExistingCollection.Amount;
            }
            else
            {
              if (entities.ExistingObligationType.SystemGeneratedIdentifier == local
                .HardcodedCsType.SystemGeneratedIdentifier)
              {
                export.ScreenOwedAmountsDtl.CsCurrColl += entities.
                  ExistingCollection.Amount;
              }
              else if (entities.ExistingObligationType.
                SystemGeneratedIdentifier == local
                .HardcodedSpType.SystemGeneratedIdentifier)
              {
                export.ScreenOwedAmountsDtl.SpCurrColl += entities.
                  ExistingCollection.Amount;
              }
              else if (entities.ExistingObligationType.
                SystemGeneratedIdentifier == local
                .HardcodedMsType.SystemGeneratedIdentifier)
              {
                export.ScreenOwedAmountsDtl.MsCurrColl += entities.
                  ExistingCollection.Amount;
              }
              else if (entities.ExistingObligationType.
                SystemGeneratedIdentifier == local
                .HardcodedMcType.SystemGeneratedIdentifier)
              {
                export.ScreenOwedAmountsDtl.McCurrColl += entities.
                  ExistingCollection.Amount;
              }

              export.ScreenOwedAmountsDtl.TotalCurrColl += entities.
                ExistingCollection.Amount;
            }
          }
          else
          {
            switch(TrimEnd(entities.ExistingCollection.ProgramAppliedTo))
            {
              case "AF":
                switch(TrimEnd(entities.ExistingCollection.DistPgmStateAppldTo))
                {
                  case "PA":
                    export.ScreenOwedAmountsDtl.AfPaArrearCollected += entities.
                      ExistingCollection.Amount;

                    break;
                  case "TA":
                    export.ScreenOwedAmountsDtl.AfTaArrearCollected += entities.
                      ExistingCollection.Amount;

                    break;
                  case "CA":
                    export.ScreenOwedAmountsDtl.AfCaArrearCollected += entities.
                      ExistingCollection.Amount;

                    break;
                  default:
                    break;
                }

                break;
              case "AFI":
                export.ScreenOwedAmountsDtl.AfiArrearsColl += entities.
                  ExistingCollection.Amount;

                break;
              case "FC":
                if (Equal(entities.ExistingCollection.DistPgmStateAppldTo, "PA"))
                  
                {
                  export.ScreenOwedAmountsDtl.FcPaArrearCollected += entities.
                    ExistingCollection.Amount;
                }
                else
                {
                }

                break;
              case "FCI":
                export.ScreenOwedAmountsDtl.FciArrearsColl += entities.
                  ExistingCollection.Amount;

                break;
              case "NA":
                switch(TrimEnd(entities.ExistingCollection.DistPgmStateAppldTo))
                {
                  case "NA":
                    export.ScreenOwedAmountsDtl.NaNaArrearCollected += entities.
                      ExistingCollection.Amount;

                    break;
                  case "UP":
                    export.ScreenOwedAmountsDtl.NaUpArrearCollected += entities.
                      ExistingCollection.Amount;

                    break;
                  case "UD":
                    export.ScreenOwedAmountsDtl.NaUdArrearCollected += entities.
                      ExistingCollection.Amount;

                    break;
                  case "CA":
                    export.ScreenOwedAmountsDtl.NaCaArrearCollected += entities.
                      ExistingCollection.Amount;

                    break;
                  default:
                    break;
                }

                break;
              case "NAI":
                export.ScreenOwedAmountsDtl.NaiArrearsColl += entities.
                  ExistingCollection.Amount;

                break;
              case "NF":
                export.ScreenOwedAmountsDtl.NfArrearsColl += entities.
                  ExistingCollection.Amount;

                break;
              case "NC":
                export.ScreenOwedAmountsDtl.NcArrearsColl += entities.
                  ExistingCollection.Amount;

                break;
              default:
                break;
            }

            export.ScreenOwedAmountsDtl.TotalArrearsColl += entities.
              ExistingCollection.Amount;
          }
        }
        else if (AsChar(entities.ExistingCollection.AppliedToCode) == 'A')
        {
          switch(TrimEnd(entities.ExistingCollection.ProgramAppliedTo))
          {
            case "AF":
              switch(TrimEnd(entities.ExistingCollection.DistPgmStateAppldTo))
              {
                case "PA":
                  export.ScreenOwedAmountsDtl.AfPaArrearCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "TA":
                  export.ScreenOwedAmountsDtl.AfTaArrearCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "CA":
                  export.ScreenOwedAmountsDtl.AfCaArrearCollected += entities.
                    ExistingCollection.Amount;

                  break;
                default:
                  break;
              }

              break;
            case "AFI":
              export.ScreenOwedAmountsDtl.AfiArrearsColl += entities.
                ExistingCollection.Amount;

              break;
            case "FC":
              if (Equal(entities.ExistingCollection.DistPgmStateAppldTo, "PA"))
              {
                export.ScreenOwedAmountsDtl.FcPaArrearCollected += entities.
                  ExistingCollection.Amount;
              }
              else
              {
              }

              break;
            case "FCI":
              export.ScreenOwedAmountsDtl.FciArrearsColl += entities.
                ExistingCollection.Amount;

              break;
            case "NA":
              switch(TrimEnd(entities.ExistingCollection.DistPgmStateAppldTo))
              {
                case "NA":
                  export.ScreenOwedAmountsDtl.NaNaArrearCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "UP":
                  export.ScreenOwedAmountsDtl.NaUpArrearCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "UD":
                  export.ScreenOwedAmountsDtl.NaUdArrearCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "CA":
                  export.ScreenOwedAmountsDtl.NaCaArrearCollected += entities.
                    ExistingCollection.Amount;

                  break;
                default:
                  break;
              }

              break;
            case "NAI":
              export.ScreenOwedAmountsDtl.NaiArrearsColl += entities.
                ExistingCollection.Amount;

              break;
            case "NF":
              export.ScreenOwedAmountsDtl.NfArrearsColl += entities.
                ExistingCollection.Amount;

              break;
            case "NC":
              export.ScreenOwedAmountsDtl.NcArrearsColl += entities.
                ExistingCollection.Amount;

              break;
            default:
              switch(AsChar(entities.ExistingObligationType.Classification))
              {
                case 'R':
                  export.ScreenOwedAmountsDtl.RecoveryArrearsColl += entities.
                    ExistingCollection.Amount;

                  break;
                case 'F':
                  export.ScreenOwedAmountsDtl.FeesArrearsColl += entities.
                    ExistingCollection.Amount;

                  break;
                default:
                  break;
              }

              break;
          }

          export.ScreenOwedAmountsDtl.TotalArrearsColl += entities.
            ExistingCollection.Amount;
        }
        else if (AsChar(entities.ExistingCollection.AppliedToCode) == 'I')
        {
          switch(TrimEnd(entities.ExistingCollection.ProgramAppliedTo))
          {
            case "AF":
              switch(TrimEnd(entities.ExistingCollection.DistPgmStateAppldTo))
              {
                case "PA":
                  export.ScreenOwedAmountsDtl.AfPaInterestCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "TA":
                  export.ScreenOwedAmountsDtl.AfTaInterestCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "CA":
                  export.ScreenOwedAmountsDtl.AfCaInterestCollected += entities.
                    ExistingCollection.Amount;

                  break;
                default:
                  break;
              }

              break;
            case "AFI":
              export.ScreenOwedAmountsDtl.AfiInterestColl += entities.
                ExistingCollection.Amount;

              break;
            case "FC":
              if (Equal(entities.ExistingCollection.DistPgmStateAppldTo, "PA"))
              {
                export.ScreenOwedAmountsDtl.FcPaInterestCollected += entities.
                  ExistingCollection.Amount;
              }
              else
              {
              }

              break;
            case "FCI":
              export.ScreenOwedAmountsDtl.FciInterestColl += entities.
                ExistingCollection.Amount;

              break;
            case "NA":
              switch(TrimEnd(entities.ExistingCollection.DistPgmStateAppldTo))
              {
                case "NA":
                  export.ScreenOwedAmountsDtl.NaNaInterestCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "UP":
                  export.ScreenOwedAmountsDtl.NaUpInterestCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "UD":
                  export.ScreenOwedAmountsDtl.NaUdInterestCollected += entities.
                    ExistingCollection.Amount;

                  break;
                case "CA":
                  export.ScreenOwedAmountsDtl.NaCaInterestCollected += entities.
                    ExistingCollection.Amount;

                  break;
                default:
                  break;
              }

              break;
            case "NAI":
              export.ScreenOwedAmountsDtl.NaiInterestColl += entities.
                ExistingCollection.Amount;

              break;
            case "NF":
              export.ScreenOwedAmountsDtl.NfInterestColl += entities.
                ExistingCollection.Amount;

              break;
            case "NC":
              export.ScreenOwedAmountsDtl.NcInterestColl += entities.
                ExistingCollection.Amount;

              break;
            default:
              if (AsChar(entities.ExistingObligationType.Classification) == 'F')
              {
                export.ScreenOwedAmountsDtl.FeesInterestColl += entities.
                  ExistingCollection.Amount;
              }
              else
              {
              }

              break;
          }

          export.ScreenOwedAmountsDtl.TotalInterestColl += entities.
            ExistingCollection.Amount;
        }
        else if (AsChar(entities.ExistingCollection.AppliedToCode) == 'G')
        {
          export.ScreenOwedAmountsDtl.GiftColl += entities.ExistingCollection.
            Amount;
        }

        export.ScreenOwedAmountsDtl.TotalCurrArrIntColl += entities.
          ExistingCollection.Amount;
      }
    }

    if (AsChar(import.OmitUndistAmtInd.Flag) != 'Y')
    {
      foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus())
      {
        if (entities.ExistingCashReceiptDetailStatus.
          SystemGeneratedIdentifier == local
          .HardcodedAdjusted.SystemGeneratedIdentifier)
        {
          continue;
        }

        if (!IsEmpty(import.FilterByStdNo.StandardNumber))
        {
          if (!Equal(entities.ExistingCashReceiptDetail.CourtOrderNumber,
            import.FilterByStdNo.StandardNumber))
          {
            continue;
          }
        }

        export.ScreenOwedAmountsDtl.UndistributedAmt += entities.
          ExistingCashReceiptDetail.CollectionAmount - entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault() - entities
          .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptDetailBalanceAdjCashReceiptDetail())
        {
          export.ScreenOwedAmountsDtl.UndistributedAmt -= entities.Adjustment.
            CollectionAmount;
        }
      }
    }

    if (AsChar(import.OmitUnprocTrnCheckInd.Flag) != 'Y')
    {
      UseFnChkUnprocessTranPrsnCase();
      export.ScreenOwedAmountsDtl.ErrorInformationLine =
        local.ScreenOwedAmounts.ErrorInformationLine;
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.ProcessMonthBegin.Date = useExport.First.Date;
    local.ProcessMonthEnd.Date = useExport.Last.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCalculateMonthlyAmountDue()
  {
    var useImport = new FnCalculateMonthlyAmountDue.Import();
    var useExport = new FnCalculateMonthlyAmountDue.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ExistingObligationPaymentSchedule);
    useImport.PeriodAmountDue.TotalCurrency = local.Tmp1.TotalCurrency;
    useImport.Period.Date = local.Process.Date;

    Call(FnCalculateMonthlyAmountDue.Execute, useImport, useExport);

    local.Tmp1.TotalCurrency = useExport.MonthlyDue.TotalCurrency;
  }

  private void UseFnChkUnprocessTranPrsnCase()
  {
    var useImport = new FnChkUnprocessTranPrsnCase.Import();
    var useExport = new FnChkUnprocessTranPrsnCase.Export();

    useImport.Obligor.Number = import.Obligor.Number;
    useImport.FilterObligation.SystemGeneratedIdentifier =
      import.Filter.SystemGeneratedIdentifier;
    useImport.FilterObligationType.SystemGeneratedIdentifier =
      import.FilterByIdObligationType.SystemGeneratedIdentifier;
    useImport.FilterBy.StandardNumber = import.FilterByStdNo.StandardNumber;
    useImport.OmitCrdInd.Flag = import.IncludeCrdInd.Flag;

    Call(FnChkUnprocessTranPrsnCase.Execute, useImport, useExport);

    local.ScreenOwedAmounts.ErrorInformationLine =
      useExport.ScreenOwedAmounts.ErrorInformationLine;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.ExistingSupportedKeyOnly.Number;
    useImport.Obligation.OrderTypeCode =
      entities.ExistingObligation.OrderTypeCode;
    MoveObligationType(entities.ExistingObligationType, useImport.ObligationType);
      
    MoveDebtDetail(entities.ExistingDebtDetail, useImport.DebtDetail);
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccruingClass.Classification;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      useExport.OtSpousalArrearsJudgement.SystemGeneratedIdentifier;
    local.OtCostOfRasingChild.SystemGeneratedIdentifier =
      useExport.OtCostOfRasingChild.SystemGeneratedIdentifier;
    local.Local718BUraJudgement.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.ArrearsJudgement.SystemGeneratedIdentifier =
      useExport.OtArrearsJudgement.SystemGeneratedIdentifier;
    local.MedicalJudgement.SystemGeneratedIdentifier =
      useExport.OtMedicalJudgement.SystemGeneratedIdentifier;
    local.HardcodedVoluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
    local.HardcodedMsType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupport.SystemGeneratedIdentifier;
    local.HardcodedMcType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupportForCash.SystemGeneratedIdentifier;
    local.HardcodedCsType.SystemGeneratedIdentifier =
      useExport.OtChildSupport.SystemGeneratedIdentifier;
    local.HardcodedSpType.SystemGeneratedIdentifier =
      useExport.OtSpousalSupport.SystemGeneratedIdentifier;
    local.HardcodedAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedSecondary.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.ExistingCollection.CrdId);
        db.
          SetInt32(command, "crvIdentifier", entities.ExistingCollection.CrvId);
          
        db.
          SetInt32(command, "cstIdentifier", entities.ExistingCollection.CstId);
          
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCollection.CrtType);
        db.SetNullableString(command, "oblgorPrsnNbr", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 6);
        entities.ExistingCashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.ExistingCashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailBalanceAdjCashReceiptDetail()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.Adjustment.Populated = false;
    entities.ExistingCashReceiptDetailBalanceAdj.Populated = false;

    return ReadEach("ReadCashReceiptDetailBalanceAdjCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.Adjustment.SequentialIdentifier = db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.Adjustment.CrvIdentifier = db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.Adjustment.CstIdentifier = db.GetInt32(reader, 6);
        entities.ExistingCashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.Adjustment.CrtIdentifier = db.GetInt32(reader, 7);
        entities.ExistingCashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.ExistingCashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingCashReceiptDetailBalanceAdj.Description =
          db.GetNullableString(reader, 10);
        entities.Adjustment.AdjustmentInd = db.GetNullableString(reader, 11);
        entities.Adjustment.CollectionAmount = db.GetDecimal(reader, 12);
        entities.Adjustment.Populated = true;
        entities.ExistingCashReceiptDetailBalanceAdj.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus()
  {
    entities.ExistingCashReceiptDetail.Populated = false;
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.Obligor.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaxDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 6);
        entities.ExistingCashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.ExistingCashReceiptDetail.Populated = true;
        entities.ExistingCashReceiptDetailStatus.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.ExistingCashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);

    return Read("ReadCollection",
      (db, command) =>
      {
        db.
          SetInt32(command, "otyId", entities.ExistingObligation.DtyGeneratedId);
          
        db.SetInt32(
          command, "obgId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetDate(
          command, "date1", local.ProcessMonthBegin.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.ProcessMonthEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.Tmp1.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTypeObligation()
  {
    entities.ExistingCollection.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligationType.Populated = false;

    return ReadEach("ReadCollectionObligationTypeObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LowLimit.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HighLimit.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AppliedToCode = db.GetString(reader, 1);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 2);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 4);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 5);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 6);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 8);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 9);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 10);
        entities.ExistingCollection.CpaType = db.GetString(reader, 11);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 12);
        entities.ExistingCollection.OtrType = db.GetString(reader, 13);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 17);
        entities.ExistingCollection.AppliedToFuture = db.GetString(reader, 18);
        entities.ExistingCollection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 19);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 20);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 21);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 22);
        entities.ExistingObligation.CpaType = db.GetString(reader, 23);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 24);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 25);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 26);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 27);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 28);
        entities.ExistingCollection.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligationType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.ExistingCollection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToFuture",
          entities.ExistingCollection.AppliedToFuture);
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingObligation.OrderTypeCode);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.ExistingSupportedKeyOnly.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingDebt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSupportedKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingSupportedKeyOnly.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId1",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
        db.SetInt32(
          command, "legalActionId2", import.FilterByIdLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.FilterByStdNo.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "obgCspNumber", entities.ExistingObligation.CspNumber);
        db.
          SetString(command, "obgCpaType", entities.ExistingObligation.CpaType);
          
        db.SetDate(command, "startDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligationPaymentSchedule.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ExistingObligationPaymentSchedule.ObgCpaType =
          db.GetString(reader, 3);
        entities.ExistingObligationPaymentSchedule.StartDt =
          db.GetDate(reader, 4);
        entities.ExistingObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ExistingObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ExistingObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ExistingObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ExistingObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ExistingObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ExistingObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ExistingObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ExistingObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ExistingObligationPaymentSchedule.PeriodInd);
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligationDebtDebtDetail()
  {
    entities.ExistingDebt.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligationType.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadObligationTypeObligationDebtDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LowLimit.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HighLimit.SystemGeneratedIdentifier);
        db.SetDate(
          command, "dueDt", local.ProcessMonthEnd.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null2.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 0);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 1);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 2);
        entities.ExistingObligation.CpaType = db.GetString(reader, 3);
        entities.ExistingDebt.CpaType = db.GetString(reader, 3);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 3);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 4);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 4);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 4);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 6);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 7);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 8);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 9);
        entities.ExistingDebt.Type1 = db.GetString(reader, 10);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 10);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 11);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 12);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 13);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 14);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 17);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 18);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 19);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 20);
        entities.ExistingDebt.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingObligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);

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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Obligation Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of FilterByIdObligationType.
    /// </summary>
    [JsonPropertyName("filterByIdObligationType")]
    public ObligationType FilterByIdObligationType
    {
      get => filterByIdObligationType ??= new();
      set => filterByIdObligationType = value;
    }

    /// <summary>
    /// A value of FilterByClass.
    /// </summary>
    [JsonPropertyName("filterByClass")]
    public ObligationType FilterByClass
    {
      get => filterByClass ??= new();
      set => filterByClass = value;
    }

    /// <summary>
    /// A value of FilterByIdLegalAction.
    /// </summary>
    [JsonPropertyName("filterByIdLegalAction")]
    public LegalAction FilterByIdLegalAction
    {
      get => filterByIdLegalAction ??= new();
      set => filterByIdLegalAction = value;
    }

    /// <summary>
    /// A value of FilterByStdNo.
    /// </summary>
    [JsonPropertyName("filterByStdNo")]
    public LegalAction FilterByStdNo
    {
      get => filterByStdNo ??= new();
      set => filterByStdNo = value;
    }

    /// <summary>
    /// A value of IncludeCrdInd.
    /// </summary>
    [JsonPropertyName("includeCrdInd")]
    public Common IncludeCrdInd
    {
      get => includeCrdInd ??= new();
      set => includeCrdInd = value;
    }

    /// <summary>
    /// A value of OmitCollectionDtlsInd.
    /// </summary>
    [JsonPropertyName("omitCollectionDtlsInd")]
    public Common OmitCollectionDtlsInd
    {
      get => omitCollectionDtlsInd ??= new();
      set => omitCollectionDtlsInd = value;
    }

    /// <summary>
    /// A value of OmitUndistAmtInd.
    /// </summary>
    [JsonPropertyName("omitUndistAmtInd")]
    public Common OmitUndistAmtInd
    {
      get => omitUndistAmtInd ??= new();
      set => omitUndistAmtInd = value;
    }

    /// <summary>
    /// A value of OmitUnprocTrnCheckInd.
    /// </summary>
    [JsonPropertyName("omitUnprocTrnCheckInd")]
    public Common OmitUnprocTrnCheckInd
    {
      get => omitUnprocTrnCheckInd ??= new();
      set => omitUnprocTrnCheckInd = value;
    }

    /// <summary>
    /// A value of DeleteMe.
    /// </summary>
    [JsonPropertyName("deleteMe")]
    public DateWorkArea DeleteMe
    {
      get => deleteMe ??= new();
      set => deleteMe = value;
    }

    private CsePerson obligor;
    private Obligation filter;
    private ObligationType filterByIdObligationType;
    private ObligationType filterByClass;
    private LegalAction filterByIdLegalAction;
    private LegalAction filterByStdNo;
    private Common includeCrdInd;
    private Common omitCollectionDtlsInd;
    private Common omitUndistAmtInd;
    private Common omitUnprocTrnCheckInd;
    private DateWorkArea deleteMe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SpousalArrears.
    /// </summary>
    [JsonPropertyName("spousalArrears")]
    public Common SpousalArrears
    {
      get => spousalArrears ??= new();
      set => spousalArrears = value;
    }

    /// <summary>
    /// A value of CostOfRaisingChild.
    /// </summary>
    [JsonPropertyName("costOfRaisingChild")]
    public Common CostOfRaisingChild
    {
      get => costOfRaisingChild ??= new();
      set => costOfRaisingChild = value;
    }

    /// <summary>
    /// A value of Export718BJudgement.
    /// </summary>
    [JsonPropertyName("export718BJudgement")]
    public Common Export718BJudgement
    {
      get => export718BJudgement ??= new();
      set => export718BJudgement = value;
    }

    /// <summary>
    /// A value of ArrearsJudgement.
    /// </summary>
    [JsonPropertyName("arrearsJudgement")]
    public Common ArrearsJudgement
    {
      get => arrearsJudgement ??= new();
      set => arrearsJudgement = value;
    }

    /// <summary>
    /// A value of MedicalJudgement.
    /// </summary>
    [JsonPropertyName("medicalJudgement")]
    public Common MedicalJudgement
    {
      get => medicalJudgement ??= new();
      set => medicalJudgement = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("screenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ScreenOwedAmountsDtl
    {
      get => screenOwedAmountsDtl ??= new();
      set => screenOwedAmountsDtl = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public Common DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    /// <summary>
    /// A value of MultiJoint.
    /// </summary>
    [JsonPropertyName("multiJoint")]
    public TextWorkArea MultiJoint
    {
      get => multiJoint ??= new();
      set => multiJoint = value;
    }

    private Common spousalArrears;
    private Common costOfRaisingChild;
    private Common export718BJudgement;
    private Common arrearsJudgement;
    private Common medicalJudgement;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private Common delMe;
    private TextWorkArea multiJoint;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PgmHistGroup group.</summary>
    [Serializable]
    public class PgmHistGroup
    {
      /// <summary>
      /// A value of PgmHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("pgmHistSuppPrsn")]
      public CsePerson PgmHistSuppPrsn
      {
        get => pgmHistSuppPrsn ??= new();
        set => pgmHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of PgmHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<PgmHistDtlGroup> PgmHistDtl => pgmHistDtl ??= new(
        PgmHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of PgmHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("pgmHistDtl")]
      [Computed]
      public IList<PgmHistDtlGroup> PgmHistDtl_Json
      {
        get => pgmHistDtl;
        set => PgmHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson pgmHistSuppPrsn;
      private Array<PgmHistDtlGroup> pgmHistDtl;
    }

    /// <summary>A PgmHistDtlGroup group.</summary>
    [Serializable]
    public class PgmHistDtlGroup
    {
      /// <summary>
      /// A value of PgmHistDtlProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlProgram")]
      public Program PgmHistDtlProgram
      {
        get => pgmHistDtlProgram ??= new();
        set => pgmHistDtlProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlPersonProgram")]
      public PersonProgram PgmHistDtlPersonProgram
      {
        get => pgmHistDtlPersonProgram ??= new();
        set => pgmHistDtlPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlProgram;
      private PersonProgram pgmHistDtlPersonProgram;
    }

    /// <summary>A TmpGroup group.</summary>
    [Serializable]
    public class TmpGroup
    {
      /// <summary>
      /// A value of TmpProgram.
      /// </summary>
      [JsonPropertyName("tmpProgram")]
      public Program TmpProgram
      {
        get => tmpProgram ??= new();
        set => tmpProgram = value;
      }

      /// <summary>
      /// A value of TmpPersonProgram.
      /// </summary>
      [JsonPropertyName("tmpPersonProgram")]
      public PersonProgram TmpPersonProgram
      {
        get => tmpPersonProgram ??= new();
        set => tmpPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program tmpProgram;
      private PersonProgram tmpPersonProgram;
    }

    /// <summary>A NullGroup group.</summary>
    [Serializable]
    public class NullGroup
    {
      /// <summary>
      /// A value of Null2.
      /// </summary>
      [JsonPropertyName("null2")]
      public TextWorkArea Null2
      {
        get => null2 ??= new();
        set => null2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private TextWorkArea null2;
    }

    /// <summary>
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of OtCostOfRasingChild.
    /// </summary>
    [JsonPropertyName("otCostOfRasingChild")]
    public ObligationType OtCostOfRasingChild
    {
      get => otCostOfRasingChild ??= new();
      set => otCostOfRasingChild = value;
    }

    /// <summary>
    /// A value of Local718BUraJudgement.
    /// </summary>
    [JsonPropertyName("local718BUraJudgement")]
    public ObligationType Local718BUraJudgement
    {
      get => local718BUraJudgement ??= new();
      set => local718BUraJudgement = value;
    }

    /// <summary>
    /// A value of ArrearsJudgement.
    /// </summary>
    [JsonPropertyName("arrearsJudgement")]
    public ObligationType ArrearsJudgement
    {
      get => arrearsJudgement ??= new();
      set => arrearsJudgement = value;
    }

    /// <summary>
    /// A value of MedicalJudgement.
    /// </summary>
    [JsonPropertyName("medicalJudgement")]
    public ObligationType MedicalJudgement
    {
      get => medicalJudgement ??= new();
      set => medicalJudgement = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public Obligation Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of Tmp1.
    /// </summary>
    [JsonPropertyName("tmp1")]
    public Common Tmp1
    {
      get => tmp1 ??= new();
      set => tmp1 = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// Gets a value of PgmHist.
    /// </summary>
    [JsonIgnore]
    public Array<PgmHistGroup> PgmHist =>
      pgmHist ??= new(PgmHistGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmHist for json serialization.
    /// </summary>
    [JsonPropertyName("pgmHist")]
    [Computed]
    public IList<PgmHistGroup> PgmHist_Json
    {
      get => pgmHist;
      set => PgmHist.Assign(value);
    }

    /// <summary>
    /// Gets a value of Tmp.
    /// </summary>
    [JsonIgnore]
    public Array<TmpGroup> Tmp => tmp ??= new(TmpGroup.Capacity);

    /// <summary>
    /// Gets a value of Tmp for json serialization.
    /// </summary>
    [JsonPropertyName("tmp")]
    [Computed]
    public IList<TmpGroup> Tmp_Json
    {
      get => tmp;
      set => Tmp.Assign(value);
    }

    /// <summary>
    /// Gets a value of Null1.
    /// </summary>
    [JsonIgnore]
    public Array<NullGroup> Null1 => null1 ??= new(NullGroup.Capacity);

    /// <summary>
    /// Gets a value of Null1 for json serialization.
    /// </summary>
    [JsonPropertyName("null1")]
    [Computed]
    public IList<NullGroup> Null1_Json
    {
      get => null1;
      set => Null1.Assign(value);
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of Null2.
    /// </summary>
    [JsonPropertyName("null2")]
    public DateWorkArea Null2
    {
      get => null2 ??= new();
      set => null2 = value;
    }

    /// <summary>
    /// A value of ProcessMonthBegin.
    /// </summary>
    [JsonPropertyName("processMonthBegin")]
    public DateWorkArea ProcessMonthBegin
    {
      get => processMonthBegin ??= new();
      set => processMonthBegin = value;
    }

    /// <summary>
    /// A value of ProcessMonthEnd.
    /// </summary>
    [JsonPropertyName("processMonthEnd")]
    public DateWorkArea ProcessMonthEnd
    {
      get => processMonthEnd ??= new();
      set => processMonthEnd = value;
    }

    /// <summary>
    /// A value of OmitSecondaryObligInd.
    /// </summary>
    [JsonPropertyName("omitSecondaryObligInd")]
    public Common OmitSecondaryObligInd
    {
      get => omitSecondaryObligInd ??= new();
      set => omitSecondaryObligInd = value;
    }

    /// <summary>
    /// A value of LowLimit.
    /// </summary>
    [JsonPropertyName("lowLimit")]
    public Obligation LowLimit
    {
      get => lowLimit ??= new();
      set => lowLimit = value;
    }

    /// <summary>
    /// A value of HighLimit.
    /// </summary>
    [JsonPropertyName("highLimit")]
    public Obligation HighLimit
    {
      get => highLimit ??= new();
      set => highLimit = value;
    }

    /// <summary>
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
    }

    /// <summary>
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of HardcodedCsType.
    /// </summary>
    [JsonPropertyName("hardcodedCsType")]
    public ObligationType HardcodedCsType
    {
      get => hardcodedCsType ??= new();
      set => hardcodedCsType = value;
    }

    /// <summary>
    /// A value of HardcodedSpType.
    /// </summary>
    [JsonPropertyName("hardcodedSpType")]
    public ObligationType HardcodedSpType
    {
      get => hardcodedSpType ??= new();
      set => hardcodedSpType = value;
    }

    /// <summary>
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNa.
    /// </summary>
    [JsonPropertyName("hardcodedNa")]
    public Program HardcodedNa
    {
      get => hardcodedNa ??= new();
      set => hardcodedNa = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    /// <summary>
    /// A value of HardcodedAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedAdjusted")]
    public CashReceiptDetailStatus HardcodedAdjusted
    {
      get => hardcodedAdjusted ??= new();
      set => hardcodedAdjusted = value;
    }

    /// <summary>
    /// A value of DelMe1.
    /// </summary>
    [JsonPropertyName("delMe1")]
    public ObligationType DelMe1
    {
      get => delMe1 ??= new();
      set => delMe1 = value;
    }

    private ObligationType spousalArrearsJudgement;
    private ObligationType otCostOfRasingChild;
    private ObligationType local718BUraJudgement;
    private ObligationType arrearsJudgement;
    private ObligationType medicalJudgement;
    private DprProgram dprProgram;
    private Obligation hold;
    private Common tmp1;
    private ScreenOwedAmounts screenOwedAmounts;
    private Array<PgmHistGroup> pgmHist;
    private Array<TmpGroup> tmp;
    private Array<NullGroup> null1;
    private Program program;
    private DateWorkArea process;
    private DateWorkArea null2;
    private DateWorkArea processMonthBegin;
    private DateWorkArea processMonthEnd;
    private Common omitSecondaryObligInd;
    private Obligation lowLimit;
    private Obligation highLimit;
    private DateWorkArea maxDiscontinue;
    private ObligationType hardcodedVoluntary;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMcType;
    private ObligationType hardcodedCsType;
    private ObligationType hardcodedSpType;
    private ObligationType hardcodedAccruingClass;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNa;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private Obligation hardcodedSecondary;
    private CashReceiptDetailStatus hardcodedAdjusted;
    private ObligationType delMe1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingSupportedKeyOnly.
    /// </summary>
    [JsonPropertyName("existingSupportedKeyOnly")]
    public CsePerson ExistingSupportedKeyOnly
    {
      get => existingSupportedKeyOnly ??= new();
      set => existingSupportedKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlySupported.
    /// </summary>
    [JsonPropertyName("existingKeyOnlySupported")]
    public CsePersonAccount ExistingKeyOnlySupported
    {
      get => existingKeyOnlySupported ??= new();
      set => existingKeyOnlySupported = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyDebt.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyDebt")]
    public ObligationTransaction ExistingKeyOnlyDebt
    {
      get => existingKeyOnlyDebt ??= new();
      set => existingKeyOnlyDebt = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePersonAccount ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public CashReceiptDetail Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj ExistingCashReceiptDetailBalanceAdj
    {
      get => existingCashReceiptDetailBalanceAdj ??= new();
      set => existingCashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of ExistingObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("existingObligationPaymentSchedule")]
    public ObligationPaymentSchedule ExistingObligationPaymentSchedule
    {
      get => existingObligationPaymentSchedule ??= new();
      set => existingObligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ExistingCashReceiptDetailStatus
    {
      get => existingCashReceiptDetailStatus ??= new();
      set => existingCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of DelMeCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("delMeCashReceiptDetail")]
    public CashReceiptDetail DelMeCashReceiptDetail
    {
      get => delMeCashReceiptDetail ??= new();
      set => delMeCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of DelMeObligationType.
    /// </summary>
    [JsonPropertyName("delMeObligationType")]
    public ObligationType DelMeObligationType
    {
      get => delMeObligationType ??= new();
      set => delMeObligationType = value;
    }

    private Collection existingCollection;
    private CashReceiptDetail existingCashReceiptDetail;
    private CsePerson existingSupportedKeyOnly;
    private CsePersonAccount existingKeyOnlySupported;
    private ObligationTransaction existingDebt;
    private LegalAction existingLegalAction;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private DebtDetail existingDebtDetail;
    private ObligationTransaction existingKeyOnlyDebt;
    private CsePersonAccount existingKeyOnlyObligor;
    private CsePerson existingObligor;
    private CashReceiptDetail adjustment;
    private CashReceiptDetailBalanceAdj existingCashReceiptDetailBalanceAdj;
    private ObligationPaymentSchedule existingObligationPaymentSchedule;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private CashReceiptDetail delMeCashReceiptDetail;
    private ObligationType delMeObligationType;
  }
#endregion
}

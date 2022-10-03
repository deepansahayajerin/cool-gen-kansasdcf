// Program: FN_OSUM_DSPLY_OBLIGATION_SUMMARY, ID: 374431671, model: 746.
// Short name: SWEOSUMP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_OSUM_DSPLY_OBLIGATION_SUMMARY.
/// </para>
/// <para>
/// Resp:Finance
/// Display Obligation_Summary.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOsumDsplyObligationSummary: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OSUM_DSPLY_OBLIGATION_SUMMARY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOsumDsplyObligationSummary(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOsumDsplyObligationSummary.
  /// </summary>
  public FnOsumDsplyObligationSummary(IContext context, Import import,
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
    // *** Maureen Brown November, 1998:
    //  - Fixed flow problems
    //  - Removed code that sets fields that are not displayed on screen.
    //  - Cleaned up views
    //  - Added Interstate Indicator to screen
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (!import.Group.IsEmpty)
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GarrOweAmount.AverageCurrency =
          import.Group.Item.GarrOweAmount.AverageCurrency;
        export.Group.Update.GintOweAmount.AverageCurrency =
          export.Group.Item.GintOweAmount.AverageCurrency;
        export.Group.Update.GarrCollAmount.AverageCurrency =
          import.Group.Item.GarrCollAmount.AverageCurrency;
        export.Group.Update.GintCollAmount.AverageCurrency =
          import.Group.Item.GintCollAmount.AverageCurrency;
        export.Group.Update.GarrOweTitle.Text5 =
          import.Group.Item.GarrOweTitle.Text5;
        export.Group.Update.GintOweTitle.Text5 =
          import.Group.Item.GintOweTitle.Text5;
        export.Group.Update.GarrCollTitle.Text5 =
          import.Group.Item.GarrCollTitle.Text5;
        export.Group.Update.GintCollTitle.Text5 =
          import.Group.Item.GintCollTitle.Text5;
      }

      import.Group.CheckIndex();
    }

    // ***** HARDCODE AREA *****
    // : Set CSE Person Account Type to Obligor.
    // : Set Monthly Financial Summary Type to Obligor Account.
    // : Set Obligation Transaction Type to DEBT.
    // : Set Obligation Transaction Debt Type to ACCRUING INSTRUCTIONS.
    // : Set Obligation Transaction Debt Type to DEBT DETAIL.
    // : Set Obligation Type Classification to CURRENT SUPPORT.
    // : Set Obligation Type Classification to Health Insurance Coverage.
    UseFnHardcodedDebtDistribution();

    // ***** SET-UP AREA *****
    MoveScreenOwedAmountsDtl2(import.ScreenOwedAmountsDtl,
      export.ScreenOwedAmountsDtl);
    export.AmtPrompt.Text1 = import.AmtPrompt.Text1;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.Obligation.Assign(import.Obligation);
    export.ObligationType.Assign(import.ObligationType);
    export.TotalOwedToDate.TotalCurrency = import.TotalOwedToDate.TotalCurrency;
    export.TotalCollectedToDate.TotalCurrency =
      import.TotalCollectedToDate.TotalCurrency;
    export.ObligationPaymentSchedule.Assign(import.ObligationPaymentSchedule);
    export.FrequencyWorkSet.Assign(import.FrequencyWorkSet);

    // ***** Left Padding Cse_Person Number ******
    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.PadCsePersonNumber.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.PadCsePersonNumber.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETDISP"))
    {
      // : Coming from OCTO
      export.CsePerson.Number = import.OctoFromList.Number;
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RTFRMLNK"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
      // *** Command will be "rtfrmlnk" upon return from link to LCDA or CRUC.
      return;
    }
    else if (Equal(global.Command, "LIST") || Equal(global.Command, "LCDA"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // *****  MAIN-LINE AREA *****
    switch(TrimEnd(global.Command))
    {
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "OE0068_SCROLLED_BEY_LAST_PAGE";

        break;
      case "LIST":
        switch(AsChar(import.AmtPrompt.Text1))
        {
          case 'S':
            export.AmtPrompt.Text1 = "+";
            export.CsePerson.Number = export.CsePersonsWorkSet.Number;
            ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";

            break;
          case '+':
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case ' ':
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          default:
            var field = GetField(export.AmtPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "DISPLAY":
        UseFnVerifyMmyyyy();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (ReadCsePerson())
        {
          if (AsChar(entities.CsePerson.Type1) == 'C')
          {
            UseCabReadAdabasPerson();
            UseSiFormatCsePersonName();
          }
          else
          {
            export.CsePersonsWorkSet.FormattedName =
              entities.CsePerson.OrganizationName ?? Spaces(33);
          }
        }
        else
        {
          export.CsePersonsWorkSet.FormattedName = "";
          ExitState = "CSE_PERSON_NF";

          return;
        }

        if (ReadCsePersonAccount())
        {
          // : Obligor CSE Person Account successfully retreived - Continue 
          // Processing.
        }
        else
        {
          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 = GetField(export.CsePersonsWorkSet, "formattedName");

          field2.Error = true;

          ExitState = "CSE_PERSON_NOT_AN_OBLIGOR";

          return;
        }

        if (ReadObligation())
        {
          export.Obligation.Assign(entities.Obligation);

          if (ReadObligationType())
          {
            export.ObligationType.Assign(entities.ObligationType);
          }
          else
          {
            ExitState = "FN0000_OBLIG_TYPE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          return;
        }

        // : Get frequency amount and monthly amount owed for accruing 
        // obligations
        //   and non-accruing obligations with payment schedules.
        if (ReadObligationPaymentSchedule())
        {
          local.Found.Flag = "Y";
        }

        if (AsChar(local.Found.Flag) == 'Y')
        {
          export.ObligationPaymentSchedule.Assign(
            entities.ObligationPaymentSchedule);
          UseFnSetFrequencyTextField();

          // *** Calculate Obligation amount due ***
          if (AsChar(entities.ObligationType.Classification) == 'A')
          {
            export.ObligationPaymentSchedule.Amount = 0;
            local.WorkCommon.Count = 0;

            foreach(var item in ReadDebt())
            {
              local.PaymentSchedule.TotalCurrency = entities.Debt.Amount + local
                .PaymentSchedule.TotalCurrency;
              export.ObligationPaymentSchedule.Amount = entities.Debt.Amount + export
                .ObligationPaymentSchedule.Amount.GetValueOrDefault();
              ++local.WorkCommon.Count;
            }

            if (local.WorkCommon.Count == 0)
            {
              ExitState = "CO0000_ACCRUAL_INSTRUCTN_NF";

              return;
            }
          }
          else
          {
            local.PaymentSchedule.TotalCurrency =
              entities.ObligationPaymentSchedule.Amount.GetValueOrDefault();
          }

          UseFnCalculateMonthlyAmountDue();
          export.ObligAmount.TotalCurrency =
            local.PaymentSchedule.TotalCurrency;
        }
        else if (AsChar(entities.ObligationType.Classification) == 'A')
        {
          ExitState = "FN0000_PAYMENT_SCHEDULE_NF";

          return;
        }
        else
        {
          // : This is ok.  Most non-accruing obligations do not have payment 
          // schedules.
        }

        if (ReadLegalAction())
        {
          MoveLegalAction(entities.LegalAction, export.LegalAction);
        }
        else
        {
          // : OK, Not all Obligations have Court Orders.
        }

        UseFnComputeSummaryTotalsDtl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.TotalCollectedToDate.TotalCurrency =
          export.ScreenOwedAmountsDtl.TotalCurrColl + export
          .ScreenOwedAmountsDtl.TotalArrearsColl + export
          .ScreenOwedAmountsDtl.TotalInterestColl + export
          .ScreenOwedAmountsDtl.FutureColl;
        export.TotalOwedToDate.TotalCurrency =
          export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed;
        UseFnDisplayProgramAmountsDtl();
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LCDA":
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
        ExitState = "ECO_LNK_TO_LCDA";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroup(FnDisplayProgramAmountsDtl.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.GarrOweTitle.Text5 = source.GarrOweTitle.Text5;
    target.GarrOweAmount.AverageCurrency = source.GarrOweAmount.AverageCurrency;
    target.GintOweTitle.Text5 = source.GintOweTitle.Text5;
    target.GintOweAmount.AverageCurrency = source.GintOweAmount.AverageCurrency;
    target.GarrCollTitle.Text5 = source.GarrCollTitle.Text5;
    target.GarrCollAmount.AverageCurrency =
      source.GarrCollAmount.AverageCurrency;
    target.GintCollTitle.Text5 = source.GintCollTitle.Text5;
    target.GintCollAmount.AverageCurrency =
      source.GintCollAmount.AverageCurrency;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveScreenOwedAmountsDtl1(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.CsCurrOwed = source.CsCurrOwed;
    target.CsCurrColl = source.CsCurrColl;
    target.SpCurrOwed = source.SpCurrOwed;
    target.SpCurrColl = source.SpCurrColl;
    target.MsCurrOwed = source.MsCurrOwed;
    target.MsCurrColl = source.MsCurrColl;
    target.McCurrOwed = source.McCurrOwed;
    target.McCurrColl = source.McCurrColl;
    target.TotalCurrDue = source.TotalCurrDue;
    target.TotalCurrOwed = source.TotalCurrOwed;
    target.TotalCurrColl = source.TotalCurrColl;
    target.PeriodicPymntDue = source.PeriodicPymntDue;
    target.PeriodicPymntOwed = source.PeriodicPymntOwed;
    target.PeriodicPymntColl = source.PeriodicPymntColl;
    target.AfiArrearsOwed = source.AfiArrearsOwed;
    target.AfiArrearsColl = source.AfiArrearsColl;
    target.AfiInterestOwed = source.AfiInterestOwed;
    target.AfiInterestColl = source.AfiInterestColl;
    target.FciArrearsOwed = source.FciArrearsOwed;
    target.FciArrearsColl = source.FciArrearsColl;
    target.FciInterestOwed = source.FciInterestOwed;
    target.FciInterestColl = source.FciInterestColl;
    target.NaiArrearsOwed = source.NaiArrearsOwed;
    target.NaiArrearsColl = source.NaiArrearsColl;
    target.NaiInterestOwed = source.NaiInterestOwed;
    target.NaiInterestColl = source.NaiInterestColl;
    target.NfArrearsOwed = source.NfArrearsOwed;
    target.NfArrearsColl = source.NfArrearsColl;
    target.NfInterestOwed = source.NfInterestOwed;
    target.NfInterestColl = source.NfInterestColl;
    target.NcArrearsOwed = source.NcArrearsOwed;
    target.NcArrearsColl = source.NcArrearsColl;
    target.NcInterestOwed = source.NcInterestOwed;
    target.NcInterestColl = source.NcInterestColl;
    target.FeesArrearsOwed = source.FeesArrearsOwed;
    target.FeesArrearsColl = source.FeesArrearsColl;
    target.FeesInterestOwed = source.FeesInterestOwed;
    target.FeesInterestColl = source.FeesInterestColl;
    target.RecoveryArrearsOwed = source.RecoveryArrearsOwed;
    target.RecoveryArrearsColl = source.RecoveryArrearsColl;
    target.FutureColl = source.FutureColl;
    target.GiftColl = source.GiftColl;
    target.TotalArrearsOwed = source.TotalArrearsOwed;
    target.TotalArrearsColl = source.TotalArrearsColl;
    target.TotalInterestOwed = source.TotalInterestOwed;
    target.TotalInterestColl = source.TotalInterestColl;
    target.TotalCurrArrIntOwed = source.TotalCurrArrIntOwed;
    target.TotalCurrArrIntColl = source.TotalCurrArrIntColl;
    target.TotalVoluntaryColl = source.TotalVoluntaryColl;
    target.UndistributedAmt = source.UndistributedAmt;
    target.IncomingInterstateObExists = source.IncomingInterstateObExists;
    target.LastCollAmt = source.LastCollAmt;
    target.ErrorInformationLine = source.ErrorInformationLine;
    target.NaNaArrearsOwed = source.NaNaArrearsOwed;
    target.NaUpArrearsOwed = source.NaUpArrearsOwed;
    target.NaUdArrearsOwed = source.NaUdArrearsOwed;
    target.NaCaArrearsOwed = source.NaCaArrearsOwed;
    target.AfPaArrearsOwed = source.AfPaArrearsOwed;
    target.AfTaArrearsOwed = source.AfTaArrearsOwed;
    target.AfCaArrearsOwed = source.AfCaArrearsOwed;
    target.FcPaArrearsOwed = source.FcPaArrearsOwed;
    target.FcTaArrearsOwed = source.FcTaArrearsOwed;
    target.FcCaArrearsOwed = source.FcCaArrearsOwed;
    target.NaNaInterestOwed = source.NaNaInterestOwed;
    target.NaUpInterestOwed = source.NaUpInterestOwed;
    target.NaUdInterestOwed = source.NaUdInterestOwed;
    target.NaCaInterestOwed = source.NaCaInterestOwed;
    target.AfPaInterestOwed = source.AfPaInterestOwed;
    target.AfTaInterestOwed = source.AfTaInterestOwed;
    target.AfCaInterestOwed = source.AfCaInterestOwed;
    target.FcPaInterestOwed = source.FcPaInterestOwed;
    target.FcTaInterestOwed = source.FcTaInterestOwed;
    target.FcCaInterestOwed = source.FcCaInterestOwed;
    target.NaNaArrearCollected = source.NaNaArrearCollected;
    target.NaUpArrearCollected = source.NaUpArrearCollected;
    target.NaUdArrearCollected = source.NaUdArrearCollected;
    target.NaCaArrearCollected = source.NaCaArrearCollected;
    target.AfPaArrearCollected = source.AfPaArrearCollected;
    target.AfTaArrearCollected = source.AfTaArrearCollected;
    target.AfCaArrearCollected = source.AfCaArrearCollected;
    target.FcPaArrearCollected = source.FcPaArrearCollected;
    target.FcTaArrearCollected = source.FcTaArrearCollected;
    target.FcCaArrearCollected = source.FcCaArrearCollected;
    target.NaNaInterestCollected = source.NaNaInterestCollected;
    target.NaUpInterestCollected = source.NaUpInterestCollected;
    target.NaUdInterestCollected = source.NaUdInterestCollected;
    target.NaCaInterestCollected = source.NaCaInterestCollected;
    target.AfPaInterestCollected = source.AfPaInterestCollected;
    target.AfTaInterestCollected = source.AfTaInterestCollected;
    target.AfCaInterestCollected = source.AfCaInterestCollected;
    target.FcPaInterestCollected = source.FcPaInterestCollected;
    target.FcTaInterestCollected = source.FcTaInterestCollected;
    target.FcCaInterestCollected = source.FcCaInterestCollected;
  }

  private static void MoveScreenOwedAmountsDtl2(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.TotalCurrDue = source.TotalCurrDue;
    target.TotalCurrOwed = source.TotalCurrOwed;
    target.TotalCurrColl = source.TotalCurrColl;
    target.PeriodicPymntDue = source.PeriodicPymntDue;
    target.PeriodicPymntOwed = source.PeriodicPymntOwed;
    target.PeriodicPymntColl = source.PeriodicPymntColl;
    target.AfiArrearsOwed = source.AfiArrearsOwed;
    target.AfiArrearsColl = source.AfiArrearsColl;
    target.AfiInterestOwed = source.AfiInterestOwed;
    target.AfiInterestColl = source.AfiInterestColl;
    target.FciArrearsOwed = source.FciArrearsOwed;
    target.FciArrearsColl = source.FciArrearsColl;
    target.FciInterestOwed = source.FciInterestOwed;
    target.FciInterestColl = source.FciInterestColl;
    target.NaiArrearsOwed = source.NaiArrearsOwed;
    target.NaiArrearsColl = source.NaiArrearsColl;
    target.NaiInterestOwed = source.NaiInterestOwed;
    target.NaiInterestColl = source.NaiInterestColl;
    target.NfArrearsOwed = source.NfArrearsOwed;
    target.NfArrearsColl = source.NfArrearsColl;
    target.NfInterestOwed = source.NfInterestOwed;
    target.NfInterestColl = source.NfInterestColl;
    target.NcArrearsOwed = source.NcArrearsOwed;
    target.NcArrearsColl = source.NcArrearsColl;
    target.NcInterestOwed = source.NcInterestOwed;
    target.NcInterestColl = source.NcInterestColl;
    target.FeesArrearsOwed = source.FeesArrearsOwed;
    target.FeesArrearsColl = source.FeesArrearsColl;
    target.FeesInterestOwed = source.FeesInterestOwed;
    target.FeesInterestColl = source.FeesInterestColl;
    target.RecoveryArrearsOwed = source.RecoveryArrearsOwed;
    target.RecoveryArrearsColl = source.RecoveryArrearsColl;
    target.FutureColl = source.FutureColl;
    target.GiftColl = source.GiftColl;
    target.TotalArrearsOwed = source.TotalArrearsOwed;
    target.TotalArrearsColl = source.TotalArrearsColl;
    target.TotalInterestOwed = source.TotalInterestOwed;
    target.TotalInterestColl = source.TotalInterestColl;
    target.TotalCurrArrIntOwed = source.TotalCurrArrIntOwed;
    target.TotalCurrArrIntColl = source.TotalCurrArrIntColl;
    target.TotalVoluntaryColl = source.TotalVoluntaryColl;
    target.UndistributedAmt = source.UndistributedAmt;
    target.ErrorInformationLine = source.ErrorInformationLine;
    target.NaNaArrearsOwed = source.NaNaArrearsOwed;
    target.AfPaArrearsOwed = source.AfPaArrearsOwed;
    target.FcPaArrearsOwed = source.FcPaArrearsOwed;
    target.NaNaInterestOwed = source.NaNaInterestOwed;
    target.AfPaInterestOwed = source.AfPaInterestOwed;
    target.FcPaInterestOwed = source.FcPaInterestOwed;
    target.NaNaArrearCollected = source.NaNaArrearCollected;
    target.AfPaArrearCollected = source.AfPaArrearCollected;
    target.FcPaArrearCollected = source.FcPaArrearCollected;
    target.NaNaInterestCollected = source.NaNaInterestCollected;
    target.AfPaInterestCollected = source.AfPaInterestCollected;
    target.FcPaInterestCollected = source.FcPaInterestCollected;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.PadCsePersonNumber.Text10;
    useExport.TextWorkArea.Text10 = local.PadCsePersonNumber.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.PadCsePersonNumber.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCalculateMonthlyAmountDue()
  {
    var useImport = new FnCalculateMonthlyAmountDue.Import();
    var useExport = new FnCalculateMonthlyAmountDue.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);
    useImport.PeriodAmountDue.TotalCurrency =
      local.PaymentSchedule.TotalCurrency;

    Call(FnCalculateMonthlyAmountDue.Execute, useImport, useExport);

    local.PaymentSchedule.TotalCurrency = useExport.MonthlyDue.TotalCurrency;
  }

  private void UseFnComputeSummaryTotalsDtl()
  {
    var useImport = new FnComputeSummaryTotalsDtl.Import();
    var useExport = new FnComputeSummaryTotalsDtl.Export();

    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.Filter.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotalsDtl.Execute, useImport, useExport);

    MoveScreenOwedAmountsDtl1(useExport.ScreenOwedAmountsDtl,
      export.ScreenOwedAmountsDtl);
  }

  private void UseFnDisplayProgramAmountsDtl()
  {
    var useImport = new FnDisplayProgramAmountsDtl.Import();
    var useExport = new FnDisplayProgramAmountsDtl.Export();

    MoveScreenOwedAmountsDtl1(export.ScreenOwedAmountsDtl,
      useImport.ScreenOwedAmountsDtl);

    Call(FnDisplayProgramAmountsDtl.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligorType.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeActiveStatus.Code = useExport.DdshActiveStatus.Code;
    local.HardcodeObligationType.Type1 = useExport.MosObligation.Type1;
    local.HardcodeDebt.Type1 = useExport.OtrnTDebt.Type1;
    MoveObligationTransaction(useExport.OtrnDtAccrualInstructions,
      local.HardcodeAccruingInstructions);
    MoveObligationTransaction(useExport.OtrnDtDebtDetail,
      local.HardcodeDebtDetail);
    local.HardcodeCurrSupClassification.Classification =
      useExport.OtCAccruingClassification.Classification;
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.FrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
  }

  private void UseFnVerifyMmyyyy()
  {
    var useImport = new FnVerifyMmyyyy.Import();
    var useExport = new FnVerifyMmyyyy.Export();

    Call(FnVerifyMmyyyy.Execute, useImport, useExport);

    local.WorkYearMonth.YearMonth = useExport.YearMonth.YearMonth;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    MoveLegalAction(export.LegalAction, useImport.LegalAction);
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", local.HardcodeObligorType.Type1);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;

    return ReadEach("ReadDebt",
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
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.DebtType = db.GetString(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
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
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.LastCollAmt = db.GetNullableDecimal(reader, 5);
        entities.Obligation.LastCollDt = db.GetNullableDate(reader, 6);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 7);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GarrOweTitle.
      /// </summary>
      [JsonPropertyName("garrOweTitle")]
      public WorkArea GarrOweTitle
      {
        get => garrOweTitle ??= new();
        set => garrOweTitle = value;
      }

      /// <summary>
      /// A value of GarrOweAmount.
      /// </summary>
      [JsonPropertyName("garrOweAmount")]
      public Common GarrOweAmount
      {
        get => garrOweAmount ??= new();
        set => garrOweAmount = value;
      }

      /// <summary>
      /// A value of GintOweTitle.
      /// </summary>
      [JsonPropertyName("gintOweTitle")]
      public WorkArea GintOweTitle
      {
        get => gintOweTitle ??= new();
        set => gintOweTitle = value;
      }

      /// <summary>
      /// A value of GintOweAmount.
      /// </summary>
      [JsonPropertyName("gintOweAmount")]
      public Common GintOweAmount
      {
        get => gintOweAmount ??= new();
        set => gintOweAmount = value;
      }

      /// <summary>
      /// A value of GarrCollTitle.
      /// </summary>
      [JsonPropertyName("garrCollTitle")]
      public WorkArea GarrCollTitle
      {
        get => garrCollTitle ??= new();
        set => garrCollTitle = value;
      }

      /// <summary>
      /// A value of GarrCollAmount.
      /// </summary>
      [JsonPropertyName("garrCollAmount")]
      public Common GarrCollAmount
      {
        get => garrCollAmount ??= new();
        set => garrCollAmount = value;
      }

      /// <summary>
      /// A value of GintCollTitle.
      /// </summary>
      [JsonPropertyName("gintCollTitle")]
      public WorkArea GintCollTitle
      {
        get => gintCollTitle ??= new();
        set => gintCollTitle = value;
      }

      /// <summary>
      /// A value of GintCollAmount.
      /// </summary>
      [JsonPropertyName("gintCollAmount")]
      public Common GintCollAmount
      {
        get => gintCollAmount ??= new();
        set => gintCollAmount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea garrOweTitle;
      private Common garrOweAmount;
      private WorkArea gintOweTitle;
      private Common gintOweAmount;
      private WorkArea garrCollTitle;
      private Common garrCollAmount;
      private WorkArea gintCollTitle;
      private Common gintCollAmount;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of OctoFromList.
    /// </summary>
    [JsonPropertyName("octoFromList")]
    public CsePerson OctoFromList
    {
      get => octoFromList ??= new();
      set => octoFromList = value;
    }

    /// <summary>
    /// A value of ObligAmount.
    /// </summary>
    [JsonPropertyName("obligAmount")]
    public Common ObligAmount
    {
      get => obligAmount ??= new();
      set => obligAmount = value;
    }

    /// <summary>
    /// A value of TotalOwedToDate.
    /// </summary>
    [JsonPropertyName("totalOwedToDate")]
    public Common TotalOwedToDate
    {
      get => totalOwedToDate ??= new();
      set => totalOwedToDate = value;
    }

    /// <summary>
    /// A value of TotalCollectedToDate.
    /// </summary>
    [JsonPropertyName("totalCollectedToDate")]
    public Common TotalCollectedToDate
    {
      get => totalCollectedToDate ??= new();
      set => totalCollectedToDate = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of AmtPrompt.
    /// </summary>
    [JsonPropertyName("amtPrompt")]
    public TextWorkArea AmtPrompt
    {
      get => amtPrompt ??= new();
      set => amtPrompt = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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

    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FrequencyWorkSet frequencyWorkSet;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePerson octoFromList;
    private Common obligAmount;
    private Common totalOwedToDate;
    private Common totalCollectedToDate;
    private NextTranInfo hidden;
    private Standard standard;
    private TextWorkArea amtPrompt;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GarrOweTitle.
      /// </summary>
      [JsonPropertyName("garrOweTitle")]
      public WorkArea GarrOweTitle
      {
        get => garrOweTitle ??= new();
        set => garrOweTitle = value;
      }

      /// <summary>
      /// A value of GarrOweAmount.
      /// </summary>
      [JsonPropertyName("garrOweAmount")]
      public Common GarrOweAmount
      {
        get => garrOweAmount ??= new();
        set => garrOweAmount = value;
      }

      /// <summary>
      /// A value of GintOweTitle.
      /// </summary>
      [JsonPropertyName("gintOweTitle")]
      public WorkArea GintOweTitle
      {
        get => gintOweTitle ??= new();
        set => gintOweTitle = value;
      }

      /// <summary>
      /// A value of GintOweAmount.
      /// </summary>
      [JsonPropertyName("gintOweAmount")]
      public Common GintOweAmount
      {
        get => gintOweAmount ??= new();
        set => gintOweAmount = value;
      }

      /// <summary>
      /// A value of GarrCollTitle.
      /// </summary>
      [JsonPropertyName("garrCollTitle")]
      public WorkArea GarrCollTitle
      {
        get => garrCollTitle ??= new();
        set => garrCollTitle = value;
      }

      /// <summary>
      /// A value of GarrCollAmount.
      /// </summary>
      [JsonPropertyName("garrCollAmount")]
      public Common GarrCollAmount
      {
        get => garrCollAmount ??= new();
        set => garrCollAmount = value;
      }

      /// <summary>
      /// A value of GintCollTitle.
      /// </summary>
      [JsonPropertyName("gintCollTitle")]
      public WorkArea GintCollTitle
      {
        get => gintCollTitle ??= new();
        set => gintCollTitle = value;
      }

      /// <summary>
      /// A value of GintCollAmount.
      /// </summary>
      [JsonPropertyName("gintCollAmount")]
      public Common GintCollAmount
      {
        get => gintCollAmount ??= new();
        set => gintCollAmount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea garrOweTitle;
      private Common garrOweAmount;
      private WorkArea gintOweTitle;
      private Common gintOweAmount;
      private WorkArea garrCollTitle;
      private Common garrCollAmount;
      private WorkArea gintCollTitle;
      private Common gintCollAmount;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ObligAmount.
    /// </summary>
    [JsonPropertyName("obligAmount")]
    public Common ObligAmount
    {
      get => obligAmount ??= new();
      set => obligAmount = value;
    }

    /// <summary>
    /// A value of TotalOwedToDate.
    /// </summary>
    [JsonPropertyName("totalOwedToDate")]
    public Common TotalOwedToDate
    {
      get => totalOwedToDate ??= new();
      set => totalOwedToDate = value;
    }

    /// <summary>
    /// A value of TotalCollectedToDate.
    /// </summary>
    [JsonPropertyName("totalCollectedToDate")]
    public Common TotalCollectedToDate
    {
      get => totalCollectedToDate ??= new();
      set => totalCollectedToDate = value;
    }

    /// <summary>
    /// A value of AmtPrompt.
    /// </summary>
    [JsonPropertyName("amtPrompt")]
    public TextWorkArea AmtPrompt
    {
      get => amtPrompt ??= new();
      set => amtPrompt = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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

    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private FrequencyWorkSet frequencyWorkSet;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Common obligAmount;
    private Common totalOwedToDate;
    private Common totalCollectedToDate;
    private TextWorkArea amtPrompt;
    private NextTranInfo hidden;
    private Standard standard;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of WorkCommon.
    /// </summary>
    [JsonPropertyName("workCommon")]
    public Common WorkCommon
    {
      get => workCommon ??= new();
      set => workCommon = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of PaymentSchedule.
    /// </summary>
    [JsonPropertyName("paymentSchedule")]
    public Common PaymentSchedule
    {
      get => paymentSchedule ??= new();
      set => paymentSchedule = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public DateWorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
    }

    /// <summary>
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public DateWorkArea FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
    }

    /// <summary>
    /// A value of WorkYearMonth.
    /// </summary>
    [JsonPropertyName("workYearMonth")]
    public DateWorkArea WorkYearMonth
    {
      get => workYearMonth ??= new();
      set => workYearMonth = value;
    }

    /// <summary>
    /// A value of ReqestedObTranDebtType.
    /// </summary>
    [JsonPropertyName("reqestedObTranDebtType")]
    public ObligationTransaction ReqestedObTranDebtType
    {
      get => reqestedObTranDebtType ??= new();
      set => reqestedObTranDebtType = value;
    }

    /// <summary>
    /// A value of WorkCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkCsePersonsWorkSet
    {
      get => workCsePersonsWorkSet ??= new();
      set => workCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HardcodeObligorType.
    /// </summary>
    [JsonPropertyName("hardcodeObligorType")]
    public CsePersonAccount HardcodeObligorType
    {
      get => hardcodeObligorType ??= new();
      set => hardcodeObligorType = value;
    }

    /// <summary>
    /// A value of HardcodeActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodeActiveStatus")]
    public DebtDetailStatusHistory HardcodeActiveStatus
    {
      get => hardcodeActiveStatus ??= new();
      set => hardcodeActiveStatus = value;
    }

    /// <summary>
    /// A value of HardcodeObligationType.
    /// </summary>
    [JsonPropertyName("hardcodeObligationType")]
    public MonthlyObligorSummary HardcodeObligationType
    {
      get => hardcodeObligationType ??= new();
      set => hardcodeObligationType = value;
    }

    /// <summary>
    /// A value of HardcodeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDebt")]
    public ObligationTransaction HardcodeDebt
    {
      get => hardcodeDebt ??= new();
      set => hardcodeDebt = value;
    }

    /// <summary>
    /// A value of HardcodeAccruingInstructions.
    /// </summary>
    [JsonPropertyName("hardcodeAccruingInstructions")]
    public ObligationTransaction HardcodeAccruingInstructions
    {
      get => hardcodeAccruingInstructions ??= new();
      set => hardcodeAccruingInstructions = value;
    }

    /// <summary>
    /// A value of HardcodeDebtDetail.
    /// </summary>
    [JsonPropertyName("hardcodeDebtDetail")]
    public ObligationTransaction HardcodeDebtDetail
    {
      get => hardcodeDebtDetail ??= new();
      set => hardcodeDebtDetail = value;
    }

    /// <summary>
    /// A value of HardcodeCurrSupClassification.
    /// </summary>
    [JsonPropertyName("hardcodeCurrSupClassification")]
    public ObligationType HardcodeCurrSupClassification
    {
      get => hardcodeCurrSupClassification ??= new();
      set => hardcodeCurrSupClassification = value;
    }

    /// <summary>
    /// A value of PadCsePersonNumber.
    /// </summary>
    [JsonPropertyName("padCsePersonNumber")]
    public TextWorkArea PadCsePersonNumber
    {
      get => padCsePersonNumber ??= new();
      set => padCsePersonNumber = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Common found;
    private Common workCommon;
    private CsePerson csePerson;
    private DateWorkArea current;
    private Common paymentSchedule;
    private DateWorkArea textDate;
    private DateWorkArea firstOfMonth;
    private DateWorkArea workYearMonth;
    private ObligationTransaction reqestedObTranDebtType;
    private CsePersonsWorkSet workCsePersonsWorkSet;
    private CsePersonAccount hardcodeObligorType;
    private DebtDetailStatusHistory hardcodeActiveStatus;
    private MonthlyObligorSummary hardcodeObligationType;
    private ObligationTransaction hardcodeDebt;
    private ObligationTransaction hardcodeAccruingInstructions;
    private ObligationTransaction hardcodeDebtDetail;
    private ObligationType hardcodeCurrSupClassification;
    private TextWorkArea padCsePersonNumber;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
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

    private ObligationTransaction debt;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationType obligationType;
    private MonthlyObligorSummary monthlyObligorSummary;
    private LegalAction legalAction;
  }
#endregion
}

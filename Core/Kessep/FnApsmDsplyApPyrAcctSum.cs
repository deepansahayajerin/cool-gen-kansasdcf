// Program: FN_APSM_DSPLY_AP_PYR_ACCT_SUM, ID: 374430372, model: 746.
// Short name: SWEAPSMP
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
/// A program: FN_APSM_DSPLY_AP_PYR_ACCT_SUM.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnApsmDsplyApPyrAcctSum: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_APSM_DSPLY_AP_PYR_ACCT_SUM program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnApsmDsplyApPyrAcctSum(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnApsmDsplyApPyrAcctSum.
  /// </summary>
  public FnApsmDsplyApPyrAcctSum(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************** MAINTENANCE LOG ****************************
    // AUTHOR    	 DATE       CHG REQ# DESCRIPTION
    // Madhu Kumar   Jan ,  2001 pr#109274  -  Corrected the validations for the
    // court order and AP/Payor associated with that court order .
    // Maureen Brown Dec, 2000 pr#106234 - Next Tran updates.
    // Madhu Kumar       09/14/2000
    // Enhancement on this screen  to display information based
    // 
    // on Ap/payor # as well as the Court order.
    // December, 1998 Maureen Brown:
    //      - FDSO is being replaced with Treasury Offset.  Changed the "F" 
    // literal
    //        to "T", and removed the old "T" from the screen.
    //      - Cleaned up views, and removed code for fields no longer displayed 
    // on
    //        the screen.
    // 12/10/10   RMathews     CQ22192        Screen change to expand 
    // undistributed amount from 5.2 to 6.2
    // ******************* END MAINTENANCE LOG **********************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ***** HARDCODE AREA *****
    // : Set CSE Person Account Type to Obligor.
    // : Set Monthly Financial Summary Type to Obligor Account.
    UseFnHardcodedDebtDistribution();

    // ***** SET-UP AREA *****
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;

    if (Equal(global.Command, "RETNAME"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
      if (!IsEmpty(import.FromNameList.Number))
      {
        export.CsePersonsWorkSet.Assign(import.FromNameList);
      }

      global.Command = "DISPLAY";
    }
    else if (Equal(global.Command, "RETCASE"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
      if (!IsEmpty(import.FromListSelect.StandardNumber))
      {
        export.LegalAction.StandardNumber =
          import.FromListSelect.StandardNumber;
      }

      global.Command = "DISPLAY";
    }
    else if (Equal(global.Command, "RTFRMLNK"))
    {
      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      MoveScreenOwedAmountsDtl1(import.ScreenOwedAmountsDtl,
        export.ScreenOwedAmountsDtl);
      export.PromptForCourtOrder.SelectChar =
        import.PromptForCourtOrder.SelectChar;
      export.MultiJoint.Text1 = import.MultiJoint.Text1;
      export.AmtPrompt.Text1 = import.AmtPrompt.Text1;
      export.NextTransaction.Command = import.NextTransaction.Command;
      export.PromptForCsePerson.SelectChar =
        import.PromptForCsePerson.SelectChar;
      export.TotalOfAllCollections.TotalCurrency =
        import.TotalOfAllCollections.TotalCurrency;
      export.TotCurrAndPerDue.TotalCurrency =
        import.TotCurrAndPerDue.TotalCurrency;
      MoveAdministrativeActCertification(import.CertifiedFdso,
        export.CertifiedFdso);
      MoveAdministrativeActCertification(import.CertifiedSdso,
        export.CertifiedSdso);

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
    }

    // ***** Left Padding Cse_Person Number ******
    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.PadCsePersonNumber.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.PadCsePersonNumber.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      if (!Equal(export.CsePersonsWorkSet.Number, export.Hidden.CsePersonNumber) &&
        !IsEmpty(export.CsePersonsWorkSet.Number))
      {
        // : MBrown Dec, 2000 pr#106234 - reset next tran, since person number 
        // has changed.
        export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
        export.Hidden.CsePersonNumberAp = export.CsePersonsWorkSet.Number;
        export.Hidden.CsePersonNumberObligor = export.CsePersonsWorkSet.Number;
        export.Hidden.StandardCrtOrdNumber =
          export.LegalAction.StandardNumber ?? "";
        export.Hidden.CsePersonNumberObligee = "";
        export.Hidden.CaseNumber = "";
        export.Hidden.CourtCaseNumber = "";
        export.Hidden.CourtOrderNumber = "";
        export.Hidden.CourtCaseNumber = "";
        export.Hidden.ObligationId = 0;
        export.Hidden.MiscNum1 = 0;
        export.Hidden.MiscNum2 = 0;
        export.Hidden.InfrastructureId = 0;
        export.Hidden.LegalActionIdentifier = 0;
      }

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

      if (IsEmpty(export.Hidden.CsePersonNumber) && IsEmpty
        (export.Hidden.StandardCrtOrdNumber))
      {
        return;
      }
      else
      {
        export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
        export.LegalAction.StandardNumber =
          export.Hidden.StandardCrtOrdNumber ?? "";
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "RETNAME") || Equal
      (global.Command, "RTFRMLNK") || Equal(global.Command, "RETCASE"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else if (Equal(global.Command, "OPAY") || Equal(global.Command, "LCDA"))
    {
      // : Bypass security - flowing to another screen.
    }
    else
    {
      // *****
      // Added data level security 10/21/96.
      // *****
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "OE0068_SCROLLED_BEY_LAST_PAGE";

        break;
      case "DISPLAY":
        if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
          (export.LegalAction.StandardNumber))
        {
          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 = GetField(export.LegalAction, "standardNumber");

          field2.Error = true;

          export.CsePersonsWorkSet.FormattedName = "";
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        if (!IsEmpty(export.LegalAction.StandardNumber))
        {
          if (!ReadLegalAction())
          {
            ExitState = "LEGAL_ACTION_NF";

            return;
          }

          if (!IsEmpty(export.CsePersonsWorkSet.Number))
          {
            // ******************************************************
            // This is just a validation to check if the obligor has
            // some obligations for the current case .
            // ******************************************************
            if (ReadObligorCsePerson1())
            {
              // ******************************************************
              // The obligor has some obligations for the current case.
              // ******************************************************
            }
            else
            {
              export.CsePersonsWorkSet.FormattedName = "";
              ExitState = "OE_CSE_PERSON_NOT_FOR_THIS_CASE";

              return;
            }
          }

          local.NoOfCsePersonsInCase.Count = 0;

          foreach(var item in ReadObligorCsePerson2())
          {
            ++local.NoOfCsePersonsInCase.Count;
          }

          if (local.NoOfCsePersonsInCase.Count > 1)
          {
            export.MultiJoint.Text1 = "M";
          }
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseFnVerifyMmyyyy();

          // ******************************************************
          //     Compute the summary for the court order .
          // ******************************************************
          UseFnComputeTotalsForCtOrder();

          if (!IsEmpty(local.MultiJoint.Text1))
          {
            export.MultiJoint.Text1 = local.MultiJoint.Text1;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          export.ScreenOwedAmountsDtl.MsCurrOwed += export.ScreenOwedAmountsDtl.
            McCurrOwed;
          export.TotCurrAndPerDue.TotalCurrency =
            export.ScreenOwedAmountsDtl.TotalCurrOwed + export
            .ScreenOwedAmountsDtl.TotalArrearsOwed + export
            .ScreenOwedAmountsDtl.TotalInterestOwed;
          export.TotalOfAllCollections.TotalCurrency =
            export.ScreenOwedAmountsDtl.TotalCurrColl + export
            .ScreenOwedAmountsDtl.TotalArrearsColl + export
            .ScreenOwedAmountsDtl.TotalInterestColl + export
            .ScreenOwedAmountsDtl.FutureColl;
          UseFnDisplayProgramAmountsDtl();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

            foreach(var item in ReadObligation3())
            {
              if (AsChar(entities.Obligation.OrderTypeCode) == 'I')
              {
                export.ScreenOwedAmountsDtl.IncomingInterstateObExists = "I";
              }
            }
          }

          if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
            if (!IsEmpty(export.ScreenOwedAmountsDtl.ErrorInformationLine))
            {
              var field =
                GetField(export.ScreenOwedAmountsDtl, "errorInformationLine");

              field.Color = "red";
              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
          }
        }
        else
        {
          if (ReadCsePerson())
          {
            if (AsChar(entities.CsePerson.Type1) == 'C')
            {
              UseEabReadCsePerson();
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
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }

          local.CsePerson.Number = export.CsePersonsWorkSet.Number;

          if (ReadObligor())
          {
            // +++++++++++++++++++++++++++++++++++++++++++++++++++
            // Code to find the Certified amount of Collection
            //   R.B.M / 06-17-1996
            // +++++++++++++++++++++++++++++++++++++++++++++++++++
            if (ReadAdministrativeActCertification2())
            {
              if (!Equal(entities.AdministrativeActCertification.
                DecertifiedDate, local.Zero.Date))
              {
                goto Read1;
              }

              MoveAdministrativeActCertification(entities.
                AdministrativeActCertification, export.CertifiedSdso);
            }

Read1:

            if (ReadAdministrativeActCertification1())
            {
              if (!Equal(entities.AdministrativeActCertification.
                DecertifiedDate, local.Zero.Date))
              {
                goto Read2;
              }

              MoveAdministrativeActCertification(entities.
                AdministrativeActCertification, export.CertifiedFdso);
            }
          }
          else
          {
            // : Obligor Not Found
            ExitState = "CSE_PERSON_NOT_OBLIGOR";

            return;
          }

Read2:

          // : Get current MMYYYY for read of Monthly Obligor Summary.
          UseFnVerifyMmyyyy();
          UseFnComputeSummaryTotalsDtl();

          if (!IsEmpty(local.MultiJoint.Text1))
          {
            export.MultiJoint.Text1 = local.MultiJoint.Text1;
          }

          export.ScreenOwedAmountsDtl.MsCurrOwed += export.ScreenOwedAmountsDtl.
            McCurrOwed;
          export.TotCurrAndPerDue.TotalCurrency =
            export.ScreenOwedAmountsDtl.TotalCurrOwed + export
            .ScreenOwedAmountsDtl.TotalArrearsOwed + export
            .ScreenOwedAmountsDtl.TotalInterestOwed;
          export.TotalOfAllCollections.TotalCurrency =
            export.ScreenOwedAmountsDtl.TotalCurrColl + export
            .ScreenOwedAmountsDtl.TotalArrearsColl + export
            .ScreenOwedAmountsDtl.TotalInterestColl + export
            .ScreenOwedAmountsDtl.FutureColl;
          UseFnDisplayProgramAmountsDtl();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

            if (IsEmpty(export.LegalAction.StandardNumber))
            {
              foreach(var item in ReadObligation2())
              {
                if (AsChar(entities.Obligation.OrderTypeCode) == 'I')
                {
                  export.ScreenOwedAmountsDtl.IncomingInterstateObExists = "I";

                  break;
                }
              }
            }
            else
            {
              foreach(var item in ReadObligation1())
              {
                if (AsChar(entities.Obligation.OrderTypeCode) == 'I')
                {
                  export.ScreenOwedAmountsDtl.IncomingInterstateObExists = "I";

                  break;
                }
              }
            }
          }

          if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
            if (!IsEmpty(export.ScreenOwedAmountsDtl.ErrorInformationLine))
            {
              var field =
                GetField(export.ScreenOwedAmountsDtl, "errorInformationLine");

              field.Color = "red";
              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
          }
        }

        break;
      case "LIST":
        if (AsChar(export.PromptForCsePerson.SelectChar) == 'S')
        {
          local.PromptCount.Count = 1;
        }
        else if (IsEmpty(export.PromptForCsePerson.SelectChar) || AsChar
          (export.PromptForCsePerson.SelectChar) == '+')
        {
        }
        else
        {
          var field = GetField(export.PromptForCsePerson, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        if (AsChar(export.AmtPrompt.Text1) == 'S')
        {
          ++local.PromptCount.Count;
        }
        else if (IsEmpty(export.AmtPrompt.Text1) || AsChar
          (export.AmtPrompt.Text1) == '+')
        {
        }
        else
        {
          var field = GetField(export.AmtPrompt, "text1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        if (AsChar(export.PromptForCourtOrder.SelectChar) == 'S')
        {
          ++local.PromptCount.Count;
        }
        else if (IsEmpty(export.PromptForCourtOrder.SelectChar) || AsChar
          (export.PromptForCourtOrder.SelectChar) == '+')
        {
        }
        else
        {
          var field = GetField(export.PromptForCourtOrder, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            if (AsChar(export.PromptForCsePerson.SelectChar) == 'S')
            {
              if (IsEmpty(export.LegalAction.StandardNumber))
              {
                ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
              }
              else
              {
                export.ShowDeactivateForOcto.SelectChar = "Y";
                ExitState = "ECO_LNK_TO_OCTO";
              }

              export.PromptForCsePerson.SelectChar = "+";
            }
            else if (AsChar(export.PromptForCourtOrder.SelectChar) == 'S')
            {
              if (IsEmpty(export.CsePersonsWorkSet.Number))
              {
                ExitState = "ECO_LNK_TO_LACN";
              }
              else
              {
                ExitState = "ECO_LNK_TO_LST_LGL_ACT_BY_CSE_P";
              }

              export.PromptForCourtOrder.SelectChar = "+";
            }
            else
            {
              export.AmtPrompt.Text1 = "+";
              ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";
              export.CsePerson.Number = export.CsePersonsWorkSet.Number;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "OPAY":
        ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

        break;
      case "LCDA":
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
        ExitState = "ECO_LNK_TO_LCDA";

        break;
      case "":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.OriginalAmount = source.OriginalAmount;
    target.CurrentAmount = source.CurrentAmount;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
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
    target.TotalCurrOwed = source.TotalCurrOwed;
    target.TotalCurrColl = source.TotalCurrColl;
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

  private static void MoveScreenOwedAmountsDtl2(ScreenOwedAmountsDtl source,
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
    target.TotalCurrOwed = source.TotalCurrOwed;
    target.TotalCurrColl = source.TotalCurrColl;
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
    target.NaNaInterestOwed = source.NaNaInterestOwed;
    target.NaUpInterestOwed = source.NaUpInterestOwed;
    target.NaUdInterestOwed = source.NaUdInterestOwed;
    target.NaCaInterestOwed = source.NaCaInterestOwed;
    target.AfPaInterestOwed = source.AfPaInterestOwed;
    target.AfTaInterestOwed = source.AfTaInterestOwed;
    target.AfCaInterestOwed = source.AfCaInterestOwed;
    target.FcPaInterestOwed = source.FcPaInterestOwed;
    target.NaNaArrearCollected = source.NaNaArrearCollected;
    target.NaUpArrearCollected = source.NaUpArrearCollected;
    target.NaUdArrearCollected = source.NaUdArrearCollected;
    target.NaCaArrearCollected = source.NaCaArrearCollected;
    target.AfPaArrearCollected = source.AfPaArrearCollected;
    target.AfTaArrearCollected = source.AfTaArrearCollected;
    target.AfCaArrearCollected = source.AfCaArrearCollected;
    target.FcPaArrearCollected = source.FcPaArrearCollected;
    target.NaNaInterestCollected = source.NaNaInterestCollected;
    target.NaUpInterestCollected = source.NaUpInterestCollected;
    target.NaUdInterestCollected = source.NaUdInterestCollected;
    target.NaCaInterestCollected = source.NaCaInterestCollected;
    target.AfPaInterestCollected = source.AfPaInterestCollected;
    target.AfTaInterestCollected = source.AfTaInterestCollected;
    target.AfCaInterestCollected = source.AfCaInterestCollected;
    target.FcPaInterestCollected = source.FcPaInterestCollected;
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

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      

    Call(EabReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void UseFnComputeSummaryTotalsDtl()
  {
    var useImport = new FnComputeSummaryTotalsDtl.Import();
    var useExport = new FnComputeSummaryTotalsDtl.Export();

    useImport.FilterByStdNo.StandardNumber = export.LegalAction.StandardNumber;
    useImport.Obligor.Number = entities.CsePerson.Number;

    Call(FnComputeSummaryTotalsDtl.Execute, useImport, useExport);

    local.MultiJoint.Text1 = useExport.MultiJoint.Text1;
    MoveScreenOwedAmountsDtl2(useExport.ScreenOwedAmountsDtl,
      export.ScreenOwedAmountsDtl);
  }

  private void UseFnComputeTotalsForCtOrder()
  {
    var useImport = new FnComputeTotalsForCtOrder.Import();
    var useExport = new FnComputeTotalsForCtOrder.Export();

    useImport.FilterByStdNo.StandardNumber = export.LegalAction.StandardNumber;

    Call(FnComputeTotalsForCtOrder.Execute, useImport, useExport);

    local.MultiJoint.Text1 = useExport.MultiJoint.Text1;
    MoveScreenOwedAmountsDtl2(useExport.ScreenOwedAmountsDtl,
      export.ScreenOwedAmountsDtl);
    MoveAdministrativeActCertification(useExport.ExpotCertifiedFdso,
      export.CertifiedFdso);
    MoveAdministrativeActCertification(useExport.ExpotCertifiedSdso,
      export.CertifiedSdso);
  }

  private void UseFnDisplayProgramAmountsDtl()
  {
    var useImport = new FnDisplayProgramAmountsDtl.Import();
    var useExport = new FnDisplayProgramAmountsDtl.Export();

    MoveScreenOwedAmountsDtl2(export.ScreenOwedAmountsDtl,
      useImport.ScreenOwedAmountsDtl);

    Call(FnDisplayProgramAmountsDtl.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligorAccountType.Type1 =
      useExport.MosCsePersonAccount.Type1;
    local.HardcodeObligorType.Type1 = useExport.CpaObligor.Type1;
  }

  private void UseFnVerifyMmyyyy()
  {
    var useImport = new FnVerifyMmyyyy.Import();
    var useExport = new FnVerifyMmyyyy.Export();

    Call(FnVerifyMmyyyy.Execute, useImport, useExport);

    local.ProcessYearMonth.YearMonth = useExport.YearMonth.YearMonth;
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

    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);
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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

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

  private bool ReadAdministrativeActCertification1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.AdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.AdministrativeActCertification.RecoveryAmount =
          db.GetNullableDecimal(reader, 8);
        entities.AdministrativeActCertification.ChildSupportRelatedAmount =
          db.GetDecimal(reader, 9);
        entities.AdministrativeActCertification.AdcAmount =
          db.GetNullableDecimal(reader, 10);
        entities.AdministrativeActCertification.NonAdcAmount =
          db.GetNullableDecimal(reader, 11);
        entities.AdministrativeActCertification.InjuredSpouseDate =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 14);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAdministrativeActCertification2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.AdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.AdministrativeActCertification.RecoveryAmount =
          db.GetNullableDecimal(reader, 8);
        entities.AdministrativeActCertification.ChildSupportRelatedAmount =
          db.GetDecimal(reader, 9);
        entities.AdministrativeActCertification.AdcAmount =
          db.GetNullableDecimal(reader, 10);
        entities.AdministrativeActCertification.NonAdcAmount =
          db.GetNullableDecimal(reader, 11);
        entities.AdministrativeActCertification.InjuredSpouseDate =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 14);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
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

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
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

  private IEnumerable<bool> ReadObligation2()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
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

  private IEnumerable<bool> ReadObligation3()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
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

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 2);
        entities.Obligor.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 3);
        entities.Obligor.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 4);
        entities.Obligor.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 5);
        entities.Obligor.AsOfDtRecBal = db.GetNullableDecimal(reader, 6);
        entities.Obligor.AsOfDtTotFeeBal = db.GetNullableDecimal(reader, 7);
        entities.Obligor.AsOfDtTotFeeIntBal = db.GetNullableDecimal(reader, 8);
        entities.Obligor.AsOfDtTotBalCurrArr = db.GetNullableDecimal(reader, 9);
        entities.Obligor.TillDtCsCollCurr = db.GetNullableDecimal(reader, 10);
        entities.Obligor.TillDtSpCollCurr = db.GetNullableDecimal(reader, 11);
        entities.Obligor.TillDtMsCollCurr = db.GetNullableDecimal(reader, 12);
        entities.Obligor.TillDtNadArrColl = db.GetNullableDecimal(reader, 13);
        entities.Obligor.TillDtNadIntColl = db.GetNullableDecimal(reader, 14);
        entities.Obligor.TillDtAdcArrColl = db.GetNullableDecimal(reader, 15);
        entities.Obligor.TillDtAdcIntColl = db.GetNullableDecimal(reader, 16);
        entities.Obligor.AsOfDtTotRecColl = db.GetNullableDecimal(reader, 17);
        entities.Obligor.AsOfDtTotFeeColl = db.GetNullableDecimal(reader, 18);
        entities.Obligor.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 19);
        entities.Obligor.LastCollAmt = db.GetNullableDecimal(reader, 20);
        entities.Obligor.AsOfDtNfArrBal = db.GetNullableDecimal(reader, 21);
        entities.Obligor.AsOfDtNfIntBal = db.GetNullableDecimal(reader, 22);
        entities.Obligor.AsOfDtFcArrBal = db.GetNullableDecimal(reader, 23);
        entities.Obligor.AsOfDtFcIntBal = db.GetNullableDecimal(reader, 24);
        entities.Obligor.TillDtNfArrColl = db.GetNullableDecimal(reader, 25);
        entities.Obligor.TillDtNfIntColl = db.GetNullableDecimal(reader, 26);
        entities.Obligor.TillDtFcArrColl = db.GetNullableDecimal(reader, 27);
        entities.Obligor.TillDtFcIntColl = db.GetNullableDecimal(reader, 28);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private bool ReadObligorCsePerson1()
  {
    entities.Obligor.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadObligorCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 2);
        entities.Obligor.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 3);
        entities.Obligor.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 4);
        entities.Obligor.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 5);
        entities.Obligor.AsOfDtRecBal = db.GetNullableDecimal(reader, 6);
        entities.Obligor.AsOfDtTotFeeBal = db.GetNullableDecimal(reader, 7);
        entities.Obligor.AsOfDtTotFeeIntBal = db.GetNullableDecimal(reader, 8);
        entities.Obligor.AsOfDtTotBalCurrArr = db.GetNullableDecimal(reader, 9);
        entities.Obligor.TillDtCsCollCurr = db.GetNullableDecimal(reader, 10);
        entities.Obligor.TillDtSpCollCurr = db.GetNullableDecimal(reader, 11);
        entities.Obligor.TillDtMsCollCurr = db.GetNullableDecimal(reader, 12);
        entities.Obligor.TillDtNadArrColl = db.GetNullableDecimal(reader, 13);
        entities.Obligor.TillDtNadIntColl = db.GetNullableDecimal(reader, 14);
        entities.Obligor.TillDtAdcArrColl = db.GetNullableDecimal(reader, 15);
        entities.Obligor.TillDtAdcIntColl = db.GetNullableDecimal(reader, 16);
        entities.Obligor.AsOfDtTotRecColl = db.GetNullableDecimal(reader, 17);
        entities.Obligor.AsOfDtTotFeeColl = db.GetNullableDecimal(reader, 18);
        entities.Obligor.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 19);
        entities.Obligor.LastCollAmt = db.GetNullableDecimal(reader, 20);
        entities.Obligor.AsOfDtNfArrBal = db.GetNullableDecimal(reader, 21);
        entities.Obligor.AsOfDtNfIntBal = db.GetNullableDecimal(reader, 22);
        entities.Obligor.AsOfDtFcArrBal = db.GetNullableDecimal(reader, 23);
        entities.Obligor.AsOfDtFcIntBal = db.GetNullableDecimal(reader, 24);
        entities.Obligor.TillDtNfArrColl = db.GetNullableDecimal(reader, 25);
        entities.Obligor.TillDtNfIntColl = db.GetNullableDecimal(reader, 26);
        entities.Obligor.TillDtFcArrColl = db.GetNullableDecimal(reader, 27);
        entities.Obligor.TillDtFcIntColl = db.GetNullableDecimal(reader, 28);
        entities.CsePerson.Type1 = db.GetString(reader, 29);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 30);
        entities.Obligor.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadObligorCsePerson2()
  {
    entities.Obligor.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadObligorCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 2);
        entities.Obligor.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 3);
        entities.Obligor.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 4);
        entities.Obligor.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 5);
        entities.Obligor.AsOfDtRecBal = db.GetNullableDecimal(reader, 6);
        entities.Obligor.AsOfDtTotFeeBal = db.GetNullableDecimal(reader, 7);
        entities.Obligor.AsOfDtTotFeeIntBal = db.GetNullableDecimal(reader, 8);
        entities.Obligor.AsOfDtTotBalCurrArr = db.GetNullableDecimal(reader, 9);
        entities.Obligor.TillDtCsCollCurr = db.GetNullableDecimal(reader, 10);
        entities.Obligor.TillDtSpCollCurr = db.GetNullableDecimal(reader, 11);
        entities.Obligor.TillDtMsCollCurr = db.GetNullableDecimal(reader, 12);
        entities.Obligor.TillDtNadArrColl = db.GetNullableDecimal(reader, 13);
        entities.Obligor.TillDtNadIntColl = db.GetNullableDecimal(reader, 14);
        entities.Obligor.TillDtAdcArrColl = db.GetNullableDecimal(reader, 15);
        entities.Obligor.TillDtAdcIntColl = db.GetNullableDecimal(reader, 16);
        entities.Obligor.AsOfDtTotRecColl = db.GetNullableDecimal(reader, 17);
        entities.Obligor.AsOfDtTotFeeColl = db.GetNullableDecimal(reader, 18);
        entities.Obligor.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 19);
        entities.Obligor.LastCollAmt = db.GetNullableDecimal(reader, 20);
        entities.Obligor.AsOfDtNfArrBal = db.GetNullableDecimal(reader, 21);
        entities.Obligor.AsOfDtNfIntBal = db.GetNullableDecimal(reader, 22);
        entities.Obligor.AsOfDtFcArrBal = db.GetNullableDecimal(reader, 23);
        entities.Obligor.AsOfDtFcIntBal = db.GetNullableDecimal(reader, 24);
        entities.Obligor.TillDtNfArrColl = db.GetNullableDecimal(reader, 25);
        entities.Obligor.TillDtNfIntColl = db.GetNullableDecimal(reader, 26);
        entities.Obligor.TillDtFcArrColl = db.GetNullableDecimal(reader, 27);
        entities.Obligor.TillDtFcIntColl = db.GetNullableDecimal(reader, 28);
        entities.CsePerson.Type1 = db.GetString(reader, 29);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 30);
        entities.Obligor.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of TotalOfAllCollections.
    /// </summary>
    [JsonPropertyName("totalOfAllCollections")]
    public Common TotalOfAllCollections
    {
      get => totalOfAllCollections ??= new();
      set => totalOfAllCollections = value;
    }

    /// <summary>
    /// A value of TotCurrAndPerDue.
    /// </summary>
    [JsonPropertyName("totCurrAndPerDue")]
    public Common TotCurrAndPerDue
    {
      get => totCurrAndPerDue ??= new();
      set => totCurrAndPerDue = value;
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
    /// A value of CertifiedFdso.
    /// </summary>
    [JsonPropertyName("certifiedFdso")]
    public AdministrativeActCertification CertifiedFdso
    {
      get => certifiedFdso ??= new();
      set => certifiedFdso = value;
    }

    /// <summary>
    /// A value of CertifiedSdso.
    /// </summary>
    [JsonPropertyName("certifiedSdso")]
    public AdministrativeActCertification CertifiedSdso
    {
      get => certifiedSdso ??= new();
      set => certifiedSdso = value;
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
    /// A value of PromptForCsePerson.
    /// </summary>
    [JsonPropertyName("promptForCsePerson")]
    public Common PromptForCsePerson
    {
      get => promptForCsePerson ??= new();
      set => promptForCsePerson = value;
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
    /// A value of FromNameList.
    /// </summary>
    [JsonPropertyName("fromNameList")]
    public CsePersonsWorkSet FromNameList
    {
      get => fromNameList ??= new();
      set => fromNameList = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
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
    /// A value of ScrollLess.
    /// </summary>
    [JsonPropertyName("scrollLess")]
    public WorkArea ScrollLess
    {
      get => scrollLess ??= new();
      set => scrollLess = value;
    }

    /// <summary>
    /// A value of ScrollMore.
    /// </summary>
    [JsonPropertyName("scrollMore")]
    public WorkArea ScrollMore
    {
      get => scrollMore ??= new();
      set => scrollMore = value;
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
    /// A value of FromListSelect.
    /// </summary>
    [JsonPropertyName("fromListSelect")]
    public LegalAction FromListSelect
    {
      get => fromListSelect ??= new();
      set => fromListSelect = value;
    }

    /// <summary>
    /// A value of PromptForCourtOrder.
    /// </summary>
    [JsonPropertyName("promptForCourtOrder")]
    public Common PromptForCourtOrder
    {
      get => promptForCourtOrder ??= new();
      set => promptForCourtOrder = value;
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

    private Common totalOfAllCollections;
    private Common totCurrAndPerDue;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private AdministrativeActCertification certifiedFdso;
    private AdministrativeActCertification certifiedSdso;
    private TextWorkArea amtPrompt;
    private Common promptForCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet fromNameList;
    private Common nextTransaction;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea scrollLess;
    private WorkArea scrollMore;
    private Array<GroupGroup> group;
    private LegalAction legalAction;
    private LegalAction fromListSelect;
    private Common promptForCourtOrder;
    private TextWorkArea multiJoint;
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
    /// A value of ShowDeactivateForOcto.
    /// </summary>
    [JsonPropertyName("showDeactivateForOcto")]
    public Common ShowDeactivateForOcto
    {
      get => showDeactivateForOcto ??= new();
      set => showDeactivateForOcto = value;
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
    /// A value of TotCurrAndPerDue.
    /// </summary>
    [JsonPropertyName("totCurrAndPerDue")]
    public Common TotCurrAndPerDue
    {
      get => totCurrAndPerDue ??= new();
      set => totCurrAndPerDue = value;
    }

    /// <summary>
    /// A value of TotalOfAllCollections.
    /// </summary>
    [JsonPropertyName("totalOfAllCollections")]
    public Common TotalOfAllCollections
    {
      get => totalOfAllCollections ??= new();
      set => totalOfAllCollections = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CertifiedFdso.
    /// </summary>
    [JsonPropertyName("certifiedFdso")]
    public AdministrativeActCertification CertifiedFdso
    {
      get => certifiedFdso ??= new();
      set => certifiedFdso = value;
    }

    /// <summary>
    /// A value of CertifiedSdso.
    /// </summary>
    [JsonPropertyName("certifiedSdso")]
    public AdministrativeActCertification CertifiedSdso
    {
      get => certifiedSdso ??= new();
      set => certifiedSdso = value;
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
    /// A value of PromptForCsePerson.
    /// </summary>
    [JsonPropertyName("promptForCsePerson")]
    public Common PromptForCsePerson
    {
      get => promptForCsePerson ??= new();
      set => promptForCsePerson = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
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
    /// A value of ScrollLess.
    /// </summary>
    [JsonPropertyName("scrollLess")]
    public WorkArea ScrollLess
    {
      get => scrollLess ??= new();
      set => scrollLess = value;
    }

    /// <summary>
    /// A value of ScrollMore.
    /// </summary>
    [JsonPropertyName("scrollMore")]
    public WorkArea ScrollMore
    {
      get => scrollMore ??= new();
      set => scrollMore = value;
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
    /// A value of PromptForCourtOrder.
    /// </summary>
    [JsonPropertyName("promptForCourtOrder")]
    public Common PromptForCourtOrder
    {
      get => promptForCourtOrder ??= new();
      set => promptForCourtOrder = value;
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

    private Common showDeactivateForOcto;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private Common totCurrAndPerDue;
    private Common totalOfAllCollections;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeActCertification certifiedFdso;
    private AdministrativeActCertification certifiedSdso;
    private TextWorkArea amtPrompt;
    private Common promptForCsePerson;
    private Common nextTransaction;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea scrollLess;
    private WorkArea scrollMore;
    private Array<GroupGroup> group;
    private LegalAction legalAction;
    private Common promptForCourtOrder;
    private TextWorkArea multiJoint;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MultiJoint.
    /// </summary>
    [JsonPropertyName("multiJoint")]
    public TextWorkArea MultiJoint
    {
      get => multiJoint ??= new();
      set => multiJoint = value;
    }

    /// <summary>
    /// A value of NoOfCsePersonsInCase.
    /// </summary>
    [JsonPropertyName("noOfCsePersonsInCase")]
    public Common NoOfCsePersonsInCase
    {
      get => noOfCsePersonsInCase ??= new();
      set => noOfCsePersonsInCase = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Function.
    /// </summary>
    [JsonPropertyName("function")]
    public DateWorkArea Function
    {
      get => function ??= new();
      set => function = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ProcessYearMonth.
    /// </summary>
    [JsonPropertyName("processYearMonth")]
    public DateWorkArea ProcessYearMonth
    {
      get => processYearMonth ??= new();
      set => processYearMonth = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePersonsWorkSet Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of HardcodeObligorAccountType.
    /// </summary>
    [JsonPropertyName("hardcodeObligorAccountType")]
    public MonthlyObligorSummary HardcodeObligorAccountType
    {
      get => hardcodeObligorAccountType ??= new();
      set => hardcodeObligorAccountType = value;
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
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

    private TextWorkArea multiJoint;
    private Common noOfCsePersonsInCase;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private DateWorkArea zero;
    private DateWorkArea function;
    private TextWorkArea padCsePersonNumber;
    private CsePerson csePerson;
    private DateWorkArea processYearMonth;
    private CsePersonsWorkSet work;
    private MonthlyObligorSummary hardcodeObligorAccountType;
    private CsePersonAccount hardcodeObligorType;
    private Common promptCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private MonthlyObligorSummary monthlyObligorSummary;
    private Obligation obligation;
    private AdministrativeActCertification administrativeActCertification;
  }
#endregion
}

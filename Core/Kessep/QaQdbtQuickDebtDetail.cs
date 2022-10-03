// Program: QA_QDBT_QUICK_DEBT_DETAIL, ID: 372659340, model: 746.
// Short name: SWEQDBTP
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
/// A program: QA_QDBT_QUICK_DEBT_DETAIL.
/// </para>
/// <para>
/// RESP:   Quality Assurance.
/// QA Quick Reference AP Data Screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQdbtQuickDebtDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QDBT_QUICK_DEBT_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQdbtQuickDebtDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQdbtQuickDebtDetail.
  /// </summary>
  public QaQdbtQuickDebtDetail(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------
    //                      Maintenance Log
    // Date       Developer                Description
    // 02/11/1999   JF Caillouet             Initial Development
    // 03/01/1999   JF Caillouet             Clean up
    // 10/08/1999   JF Caillouet             Recoded IWO
    // --------------------------------------------------------
    // -----------------------------------------------------------------------------------
    // 01/12/2000            PR# 82737       Vithal Madhira(SRG)
    //     'Pd Last Mo' field on QDBT displays incorrect amount.
    // 07/11/2000            PR# 96358       Vithal Madhira
    // Fixed the code and now user will not be forced to go to LACS if the case 
    // has two legal actions with same 'standard number'.
    // 07/12/2000            PR# 94339       Vithal Madhira
    // Fixed the code. Now, the 'last_payment date' is SET to '
    // cash_receipt_detail  collection_ date'  instead of 'collection
    // collectio_date'
    //  07/13/2000           PR# 94164           Vithal Madhira
    // Fixed the code. Now  screen will display only the related court_order for
    // which AP is an OBLIGOR.
    // ---------------------------------------------------------------------------------
    // -------------------------------------------------------------------
    // WR# 000254       11/20/2000           Vithal Madhira
    // New Flag is added to show if disbursements are suppressed  for AR.
    // ----------------------------------------------------------------------
    // 11/17/00 M.Lachowicz      WR 298. Create header
    //                           information for screens.
    // -----------------------------------------------
    // 03/28/01 M.Brown      PR 115907  Remove URA information
    // (First Benefit Date, Grant Amount, Grant number, Total Grant amount).
    // -----------------------------------------------
    // 03/28/01 M.Brown      PR 116895  Added view matching for the header on
    // transfers between QA screens.
    // -----------------------------------------------
    // : M. Brown, PR # 122212 - a fix I did previously was incorrect, so I put 
    // it back the way it was.
    // WR20202. 12-12-01 by L. Bachura. This WR provides that the  QDBT screen 
    // display info for closed case. All info will display except case program.
    // Per Karen buchelle it is not necessary to display it. The change is
    // effected by deleting the comparison of the discontinue date to the local
    // current date in the display section of the code. The deleted statement is
    // "and desired XX discontinue date is > local_current date work area date
    // ". This check was to verify that the discontinue date was 2099-12-31.
    // -----------------------------------------------
    // 2/12/02  WR 000235  Fangman - PSUM redesign.  Changed one column name in 
    // Monthly table.  The field did not appear to be in used since prior to the
    // PRWORA changes.
    // -----------------------------------------------
    // PR139464 on 2-26-02. Installed logic to cehck to ensure that the AR 
    // selected for display has an open end date on role when the case is
    // active. Lbachura.
    // --------------------------------------------------------------
    // 11/2002         SWSRKXD
    // Fix screen help Id.
    // --------------------------------------------------------------
    // -------------------------------------------------------------------------------
    // 10/15/07  G. Pan   CQ488
    // Added logic to read Infrastructure to get the created_timestamp in order 
    // to set the last_modification_review_date.
    // The date currently used (before code change) was from Legal_Action and 
    // was incorrect.  Should be the same date
    // as displayed on CRIN Last Mod Review field (from Infrastructure).
    // 02/27/08  G. Pan   CQ3259
    // Found a production problem while doing CQ488 acceptance test - If user 
    // presses PF2 a 2nd time or PF15, Pf16, PF18
    // and PF21 Last Mod Rvw Dt and/or Court Ord Date fields has disappeared.
    // program change - Commented out two statements not to initialize 
    // filed_date (Court Ord Date) and last_modification
    // _review_date (Last Mod Rvw Dt).
    // 05/29/08  G. Pan   CQ3259  Set "Y" to PF19_from_QCOLflag to fix when PF19
    // returs from QCOL the filed_date
    //                    & last_modification_review_date disappeared problem.
    // 08/05/10  J Huss  CQ# 407  Added checks for decertification dates and 
    // exemptions to FDSO lookup.
    // -------------------------------------------------------------------------------
    // 07/18/16  J.Harden    CQ50344  Show AJ, MJ, and 718B debts on screen and 
    // remove fees and charges. Also decided to add MS arrears instead of MD
    // arrears, add Spousal arrears (SAJ) and Cost of Raising Child (CRCH).
    // Amounts in all fields should be totaled for the Total Pay Off field.
    // 
    // -------------------------------------------------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_XFR_TO_MENU";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    UseCabSetMaximumDiscontinueDate();

    // ---  Move Section  --
    export.HiddenPf19FromQcol.Flag = import.HiddenPf19FromQcol.Flag;
    MoveCase2(import.Case1, export.Case1);
    MoveCaseFuncWorkSet(import.CaseFuncWorkSet, export.CaseFuncWorkSet);
    MoveCodeValue(import.CaseCloseRsn, export.CaseCloseRsn);
    export.Program.Code = import.Program.Code;
    MoveServiceProvider(import.ServiceProvider, export.ServiceProvider);
    export.Standard.Assign(import.Standard);
    MoveCaseRole1(import.ApCaseRole, export.ApCaseRole);
    MoveCsePerson3(import.ApClient, export.ApClient);
    MoveCsePerson4(import.ArCsePerson, export.ArCsePerson);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.ArCsePersonsWorkSet.Assign(import.ArCsePersonsWorkSet);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.MultipleAps.Flag = import.MultipleAps.Flag;
    export.ApMultiCases.Flag = import.ApMultiCases.Flag;
    export.Next.Number = import.Next.Number;
    export.OtherChilderen.Flag = import.OtherChildren.Flag;
    export.ArMultiCases.Flag = import.ArMultiCases.Flag;
    export.AltSsnAlias.Text30 = import.AltSsnAlias.Text30;
    MoveOffice(import.Office, export.Office);
    export.ApMultiCasesPrompt.Flag = import.ApMultiCasesPrompt.Flag;
    export.ProgCodeDescription.Description =
      import.ProgCodeDescription.Description;
    export.ArMultiCasesPrompt.Flag = import.ArMultiCasesPrompt.Flag;
    MoveCsePersonsWorkSet3(import.ComnLink, export.ComnLink);

    // 11/17/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/17/00 M.L End
    // ---  Debt section  --
    export.SelectCourtOrder.Assign(import.SelectCourtOrder);
    MoveCommon(import.CourtOrderPrompt, export.CourtOrderPrompt);
    export.MonthlyObligeeSummary.Assign(import.MonthlyObligeeSummary);
    export.LastModDate.Date = import.LastModDate.Date;
    export.CurrPdLastMo.NumericalDollarValue =
      import.CurrPdLastMo.NumericalDollarValue;
    export.PdLastMonth.NumericalDollarValue =
      import.PdLastMonth.NumericalDollarValue;
    export.MultiCourtCase.Flag = import.MultiCourtCase.Flag;
    MoveScreenOwedAmountsDtl3(import.Amts, export.Amts);
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    MoveDateWorkArea(import.GrantDate, export.GrantDate);
    export.CredAdministrativeActCertification.Assign(
      import.CredAdministrativeActCertification);
    export.CredCommon.Flag = import.CredCommon.Flag;
    export.Fdso.Assign(import.Fdso);
    export.Sdso.Assign(import.Sdso);
    MoveCreditReportingAction(import.CreditReportingAction,
      export.CreditReportingAction);
    MoveAdministrativeActCertification(import.Treas, export.Treas);
    export.Iwo.Assign(import.Iwo);
    export.LastPymnt.Date = import.LastPymnt.Date;
    export.IwoPymnt.Flag = import.IwoPymnt.Flag;
    export.IwoCurrent.TotalCurrency = import.IwoCurrent.TotalCurrency;
    export.IwoArrears.TotalCurrency = import.IwoArrears.TotalCurrency;
    export.PdLastMonth.NumericalDollarValue =
      import.PdLastMonth.NumericalDollarValue;
    export.ArRecov.TotalCurrency = import.ArRecov.TotalCurrency;
    export.FlowFromLacs.Assign(import.FlowFromLacs);
    MoveLegalAction3(import.HiddenLegalAction, export.HiddenLegalAction);
    MoveImHousehold(import.ImHousehold, export.ImHousehold);
    export.Ura.Assign(import.Ura);
    MoveOeUraSummary(import.UraCum, export.UraCum);
    export.Med.NumericalDollarValue = import.Med.NumericalDollarValue;

    // CQ 50344
    export.ArrearsJudgement.TotalCurrency = import.SpousalArrears.TotalCurrency;
    export.ArrearsJudgement.TotalCurrency =
      import.CostOfRaisingChild.TotalCurrency;
    export.MedicalJudgement.TotalCurrency =
      import.MedicalJudgement.TotalCurrency;
    export.ArrearsJudgement.TotalCurrency =
      import.ArrearsJudgement.TotalCurrency;
    export.Export718BJudgement.TotalCurrency =
      import.Import718BJudgement.TotalCurrency;

    // ------------------------------------------------------------------------------------
    //        Per WR# 000254, the following code is added.
    //                                                                  
    // Vithal(11/20/2000)
    // ------------------------------------------------------------------------------------
    export.DisbSupp.Flag = import.DisbSupp.Flag;

    if (!IsEmpty(export.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Case1.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        export.ApCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.ApCsePersonsWorkSet.Number =
        export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
      export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
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

    local.Current.Date = Now().Date;
    local.Current.Month = Month(local.Current.Date);
    local.Current.Year = Year(local.Current.Date);
    local.Current.YearMonth = local.Current.Year * 100 + local.Current.Month;

    // ***  Set Prompt Screen Characteristics  ***
    if (AsChar(export.ApMultiCases.Flag) == 'Y')
    {
      var field = GetField(export.ApMultiCasesPrompt, "flag");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.ArMultiCases.Flag) == 'Y')
    {
      var field = GetField(export.ArMultiCasesPrompt, "flag");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.MultipleAps.Flag) == 'Y')
    {
      var field = GetField(export.ApPrompt, "selectChar");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.MultiCourtCase.Flag) == 'Y')
    {
      var field = GetField(export.CourtOrderPrompt, "flag");

      field.Color = "green";
      field.Protected = false;
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        break;
      case "RETCOMN":
        if (AsChar(export.ArMultiCasesPrompt.Flag) == 'S')
        {
          if (IsEmpty(export.Next.Number))
          {
            export.Next.Number = export.Case1.Number;
          }
          else
          {
            export.ArCsePersonsWorkSet.Assign(export.ComnLink);
          }
        }
        else if (AsChar(export.ApMultiCasesPrompt.Flag) == 'S')
        {
          if (IsEmpty(export.Next.Number))
          {
            export.Next.Number = export.Case1.Number;
          }
          else
          {
            export.ApCsePersonsWorkSet.Assign(export.ComnLink);
          }
        }

        global.Command = "DISPLAY";

        break;
      case "RETCOMP":
        var field = GetField(export.ApPrompt, "selectChar");

        field.Color = "green";
        field.Protected = false;

        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
        break;
      case "RETLACS":
        MoveLegalAction2(import.FlowFromLacs, export.SelectCourtOrder);
        export.CourtOrderPrompt.Flag = "";
        global.Command = "DISPLAY";

        break;
      case "LIST":
        // Each prompt should have the following IF Statement type
        switch(AsChar(export.ApPrompt.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field1 = GetField(export.ApPrompt, "selectChar");

            field1.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.ApMultiCasesPrompt.Flag))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            MoveCsePersonsWorkSet2(export.ApCsePersonsWorkSet, export.ComnLink);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field1 = GetField(export.ApMultiCasesPrompt, "flag");

            field1.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.ArMultiCasesPrompt.Flag))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            MoveCsePersonsWorkSet2(export.ApCsePersonsWorkSet, export.ComnLink);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field1 = GetField(export.ArMultiCasesPrompt, "flag");

            field1.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.CourtOrderPrompt.Flag))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Filter.Classification = "J";
            ExitState = "ECO_LNK_TO_LACS";

            break;
          default:
            var field1 = GetField(export.CourtOrderPrompt, "flag");

            field1.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            // Each prompt should have the following IF Statement type
            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.ApMultiCasesPrompt.Flag) == 'S')
            {
              var field1 = GetField(export.ApMultiCasesPrompt, "flag");

              field1.Error = true;
            }

            if (AsChar(export.ArMultiCasesPrompt.Flag) == 'S')
            {
              var field1 = GetField(export.ArMultiCasesPrompt, "flag");

              field1.Error = true;
            }

            // ---  Next Tran and Security Logic  --
            if (AsChar(export.ArMultiCasesPrompt.Flag) == 'S')
            {
              var field1 = GetField(export.ArMultiCasesPrompt, "flag");

              field1.Error = true;
            }

            if (AsChar(export.CourtOrderPrompt.Flag) == 'S')
            {
              var field1 = GetField(export.CourtOrderPrompt, "flag");

              field1.Error = true;
            }

            break;
        }

        break;
      case "PVSCR":
        ExitState = "ECO_XFR_TO_PREV";

        break;
      case "NXSCR":
        ExitState = "ECO_XFR_TO_NEXT_SCRN";

        break;
      case "COMP":
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        break;
      case "HIST":
        export.HistFlow.CaseNumber = export.Next.Number;
        export.HistFlow.CsePersonNumber = export.ApClient.Number;
        ExitState = "ECO_LNK_TO_HIST";

        break;
      case "RETLINK":
        if (!Equal(export.Next.Number, import.HistFlow.CaseNumber))
        {
          export.Next.Number = import.HistFlow.CaseNumber ?? Spaces(10);
          export.ApClient.Number = import.HistFlow.CsePersonNumber ?? Spaces
            (10);
          global.Command = "DISPLAY";
        }

        break;
      case "OCTO":
        ExitState = "ECO_LNK_TO_OCTO";

        break;
      case "OPAY":
        MoveLegalAction3(export.SelectCourtOrder, export.HiddenLegalAction);
        ExitState = "ECO_LNK_LST_OPAY_OBLG_BY_AP";

        break;
      case "PEPR":
        ExitState = "ECO_LNK_TO_PEPR";

        break;
      case "POFF":
        ExitState = "ECO_LNK_TO_POFF";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETOPAY":
        if (IsEmpty(import.SelectCourtOrder.StandardNumber))
        {
          export.SelectCourtOrder.StandardNumber =
            export.HiddenLegalAction.StandardNumber ?? "";
        }

        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.ApPrompt.SelectChar = "";
      export.ArMultiCasesPrompt.Flag = "";
      export.ApMultiCasesPrompt.Flag = "";
      export.CourtOrderPrompt.Flag = "";

      if (IsEmpty(export.Next.Number) || !
        Equal(export.Next.Number, export.Case1.Number))
      {
        export.MultiCourtCase.Flag = "";
        export.ServiceProvider.LastName = "";
        export.ServiceProvider.FirstName = "";
        export.Office.Name = "";
        export.Office.TypeCode = "";
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseFuncWorkSet.FuncDate = local.NullDateWorkArea.Date;
        export.CaseUnitFunctionAssignmt.Function = "";
        export.Sdso.CurrentAmount = 0;
        export.Fdso.CurrentAmount = 0;
        export.CreditReportingAction.CseActionCode = "";
        export.CreditReportingAction.CraTransDate = local.NullDateWorkArea.Date;
        export.DisbSupp.Flag = "";
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Cdvalue = "";
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseUnitFunctionAssignmt.Function = "";
        export.AltSsnAlias.Text30 = "";
        export.ApCsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
        export.ArCsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
        MoveCsePerson2(local.NullCsePerson, export.ApClient);
        export.Case1.Assign(local.NullCase);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;
        export.ApMultiCases.Flag = "";
        export.ArMultiCases.Flag = "";
        export.SelectCourtOrder.StandardNumber = "";
        export.FlowFromLacs.CourtCaseNumber = "";
        export.FlowFromLacs.StandardNumber = "";
        export.SelectCourtOrder.LastModificationReviewDate =
          local.NullDateWorkArea.Date;
        export.SelectCourtOrder.FiledDate = local.NullDateWorkArea.Date;
      }

      // *** Debt Section ***
      // *** Moved this out of the Case Protected area ****
      // -------------------------------------------------------------------------------
      // 02/27/08  G. Pan   CQ3259  commented out following two statements so 
      // the
      //                    Court Ord Date and Last Mod Rvw Dt will not 
      // disappear
      //                    when  press PF2 a 2nd time.
      // -------------------------------------------------------------------------------
      export.LastModDate.Date = local.NullDateWorkArea.Date;
      export.CredCommon.Flag = "";
      export.IwoPymnt.Flag = "";
      export.LastPymnt.Date = local.NullDateWorkArea.Date;
      export.Sdso.RecoveryAmount = 0;
      export.ArRecov.TotalCurrency = 0;
      export.ScreenOwedAmounts.TotalAmountOwed = 0;
      export.PdLastMonth.NumericalDollarValue = 0;
      export.CurrPdLastMo.NumericalDollarValue = 0;
      export.Amts.TotalInterestOwed = 0;
      export.AdministrativeActCertification.Type1 = "";
      export.MonthlyObligeeSummary.DisbursementsSuppressed = 0;
      MoveScreenOwedAmountsDtl1(local.NullAmts, export.Amts);
      export.CredAdministrativeActCertification.DateSent =
        local.NullDateWorkArea.Date;
      export.Fdso.AmountOwed = 0;
      export.Treas.AmountOwed = 0;
      export.GrantDate.Date = local.NullDateWorkArea.Date;
      export.Iwo.EffectiveDate = local.NullDateWorkArea.Date;
      export.Iwo.OrderType = "";
      export.IwoArrears.TotalCurrency = 0;
      export.IwoCurrent.TotalCurrency = 0;
      export.Ura.Grant = 0;
      export.UraCum.Ura = 0;
      export.UraCum.Grant = 0;
      export.ImHousehold.FirstBenefitDate = local.NullDateWorkArea.Date;
      export.Med.NumericalDollarValue = 0;

      if (IsEmpty(export.Next.Number))
      {
        ExitState = "CASE_NUMBER_REQUIRED";

        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      if (!Equal(export.Next.Number, export.Case1.Number))
      {
        UseCabZeroFillNumber();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }

        export.Case1.Number = export.Next.Number;
        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }

        // ***********************************************************
        // ***  CASE DETAILS
        // 
        // ***
        // ***********************************************************
        // *** READ CASE  ***
        local.CaseOpen.Flag = "N";

        if (ReadCase2())
        {
          MoveCase3(entities.Case1, export.Case1);

          if (AsChar(export.Case1.Status) == 'O')
          {
            export.Case1.StatusDate = local.NullDateWorkArea.Date;
            local.CaseOpen.Flag = "Y";
          }
        }
        else
        {
          ExitState = "CASE_NF";

          return;
        }

        if (!IsEmpty(export.Case1.ClosureReason))
        {
          if (ReadCodeValue1())
          {
            export.CaseCloseRsn.Description = entities.CodeValue.Description;
          }
          else
          {
            var field = GetField(export.CaseCloseRsn, "description");

            field.Error = true;
          }
        }
        else
        {
          export.CaseCloseRsn.Cdvalue = "";
        }

        // ***********************************************************
        // ***  AR DETAILS
        // 
        // ***
        // ***********************************************************
        // *** Find AR  ***
        if (AsChar(local.CaseOpen.Flag) == 'Y')
        {
          if (ReadCsePerson2())
          {
            export.ArCsePersonsWorkSet.Number = entities.CsePerson.Number;
            export.ArCsePerson.AeCaseNumber = entities.CsePerson.AeCaseNumber;
            local.AeCaseNumber.AeCaseNo = entities.CsePerson.AeCaseNumber ?? Spaces
              (8);
          }
          else
          {
            ExitState = "CASE_ROLE_AR_NF";

            return;
          }
        }

        if (AsChar(local.CaseOpen.Flag) == 'N')
        {
          if (ReadCsePerson3())
          {
            export.ArCsePersonsWorkSet.Number = entities.CsePerson.Number;
            export.ArCsePerson.AeCaseNumber = entities.CsePerson.AeCaseNumber;
            local.AeCaseNumber.AeCaseNo = entities.CsePerson.AeCaseNumber ?? Spaces
              (8);
          }
          else
          {
            ExitState = "CASE_ROLE_AR_NF";

            return;
          }
        }

        // *** AR on more than one case?  ***
        if (ReadCase1())
        {
          export.ArMultiCases.Flag = "Y";

          var field = GetField(export.ArMultiCasesPrompt, "flag");

          field.Color = "green";
          field.Protected = false;
        }
        else
        {
          export.ArMultiCases.Flag = "N";

          var field = GetField(export.ArMultiCasesPrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;
        }

        // *** Get AR details  ***
        UseSiReadCsePerson();

        if (!IsEmpty(local.AbendData.Type1) || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(local.AbendData.Type1))
          {
            var field = GetField(export.ArCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "CO0000_AR_NF";
          }

          return;
        }

        // *** Determine multiple AP's     ***
        export.MultipleAps.Flag = "";

        foreach(var item in ReadCsePerson4())
        {
          if (IsEmpty(export.MultipleAps.Flag))
          {
            export.MultipleAps.Flag = "N";
            export.ApCsePersonsWorkSet.Number = entities.CsePerson.Number;
          }
          else
          {
            export.MultipleAps.Flag = "Y";
            export.ApCsePersonsWorkSet.Number = "";

            break;
          }
        }

        if (IsEmpty(export.MultipleAps.Flag))
        {
          ExitState = "AP_FOR_CASE_NF";

          return;
        }
      }

      // ***********************************************************
      // ***  AP DETAILS
      // 
      // ***
      // ***********************************************************
      if (AsChar(export.MultipleAps.Flag) == 'Y')
      {
        if (IsEmpty(import.FromComp.Number) && IsEmpty
          (export.ApCsePersonsWorkSet.Number))
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        var field = GetField(export.ApPrompt, "selectChar");

        field.Color = "green";
        field.Protected = false;
      }

      if (!Equal(import.FromComp.Number, export.ApCsePersonsWorkSet.Number) || !
        IsEmpty(import.FromComp.Number))
      {
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.AltSsnAlias.Text30 = "";
        MoveCsePerson2(local.NullCsePerson, export.ApClient);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;

        if (!IsEmpty(import.FromComp.Number))
        {
          export.ApCsePersonsWorkSet.Number = import.FromComp.Number;
        }

        // *** Get AP details  ***
        UseSiReadApCaseRoleDetails();

        // PR140816 on March 26,2002. Intalled the follwoing logic so that the 
        // inacitve AP is not displayed on the screen. Lbachura
        // 8-14-02 PR146446 code installed in below. LJB
        local.ApOpen.Flag = "N";

        if (AsChar(export.MultipleAps.Flag) == 'Y')
        {
          if (ReadCsePerson1())
          {
            local.ApOpen.Flag = "Y";
          }
        }

        if (AsChar(export.MultipleAps.Flag) == 'N' && !
          Lt(Now().Date, export.ApCaseRole.EndDate))
        {
          if (ReadCsePerson1())
          {
            local.ApOpen.Flag = "Y";
          }
        }

        if (!Lt(Now().Date, export.ApCaseRole.EndDate) && AsChar
          (local.ApOpen.Flag) == 'N')
        {
          export.ApCsePersonsWorkSet.FormattedName = "";
          export.ApCsePersonsWorkSet.Number = "";
          export.ApCsePersonsWorkSet.Ssn = "";
          export.ApMultiCases.Flag = "";

          var field = GetField(export.ApMultiCasesPrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;

          ExitState = "OE0000_AP_INACTIVE_ON_THIS_CASE";

          return;
        }

        if (!IsEmpty(local.AbendData.Type1) || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(local.AbendData.Type1))
          {
            var field = GetField(export.ApCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "CO0000_AP_NF";
          }

          return;
        }

        // *** AP on more than one case?  ***
        if (AsChar(export.ApMultiCases.Flag) == 'Y')
        {
          var field = GetField(export.ApMultiCasesPrompt, "flag");

          field.Color = "green";
          field.Protected = false;
        }
        else
        {
          var field = GetField(export.ApMultiCasesPrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;
        }

        UseSiAltsBuildAliasAndSsn();

        if (AsChar(local.ArAltOccur.Flag) == 'Y' || AsChar
          (local.ApAltOccur.Flag) == 'Y')
        {
          export.AltSsnAlias.Text30 = "Alt SSN/Alias";
        }
        else
        {
          export.AltSsnAlias.Text30 = "";
        }
      }

      UseSiCadsReadCaseDetails();

      if (!IsEmpty(export.Program.Code))
      {
        if (ReadCodeValue2())
        {
          export.ProgCodeDescription.Description =
            entities.CodeValue.Description;
        }
        else
        {
          var field = GetField(export.ProgCodeDescription, "description");

          field.Error = true;
        }
      }
      else
      {
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
      }

      // *** Set blank SSN's to spaces  ***
      if (Equal(export.ApCsePersonsWorkSet.Ssn, "000000000"))
      {
        export.ApCsePersonsWorkSet.Ssn = "";
      }

      if (Equal(export.ArCsePersonsWorkSet.Ssn, "000000000"))
      {
        export.ArCsePersonsWorkSet.Ssn = "";
      }

      // ------------------------------------------------------------------------------------
      //        Per WR# 000254, the following code is added  to SET the 
      // disbursement suppressed flag.
      //                                                                  
      // Vithal(11/20/2000)
      // ------------------------------------------------------------------------------------
      if (ReadDisbursementStatusHistory())
      {
        export.DisbSupp.Flag = "Y";
      }
      else
      {
        export.DisbSupp.Flag = "N";
      }

      // *****************************************************************
      // ***  New Debt Section  ****
      // *****************************************************************
      export.ArRecov.TotalCurrency = 0;
      export.PdLastMonth.NumericalDollarValue = 0;
      export.CurrPdLastMo.NumericalDollarValue = 0;
      export.IwoArrears.TotalCurrency = 0;
      export.IwoCurrent.TotalCurrency = 0;

      if (IsEmpty(export.MultiCourtCase.Flag) || AsChar
        (export.HiddenPf19FromQcol.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------------------------------------------------
        // 06/03/08 CQ3259 added new coded.  When PF19 returns from
        // QCOL, move space to multi_court_case flag so it will always read
        // legal_action and find the correct record.
        // -------------------------------------------------------------------------------------------------------------------
        if (AsChar(export.HiddenPf19FromQcol.Flag) == 'Y')
        {
          if (AsChar(export.MultiCourtCase.Flag) == 'Y')
          {
            export.HiddenLegalAction.StandardNumber =
              export.FlowFromLacs.StandardNumber ?? "";
          }
        }

        // -----------------------------------------------------------------------------
        // PR# 96358             Vithal Madhira       07/11/2000
        // Added "SORTED BY DESCENDING legal_action standard_number "  to the 
        // below READ EACH. This code will fix the problem and will not force
        // the user to go to LACS to select Legal Action.
        // ---------------------------------------------------------------------------------
        foreach(var item in ReadLegalAction3())
        {
          if (Equal(entities.LegalAction.StandardNumber,
            export.HiddenLegalAction.StandardNumber))
          {
            MoveLegalAction1(entities.LegalAction, export.SelectCourtOrder);
            MoveLegalAction3(entities.LegalAction, export.HiddenLegalAction);

            // 06/03/08 CQ3259 commented out redundant code.
            continue;
          }

          if (IsEmpty(export.MultiCourtCase.Flag) || AsChar
            (export.HiddenPf19FromQcol.Flag) == 'Y')
          {
            // 06/04/08 CQ3259 when PF19 returns from QCOL for multiple court 
            // order case don't do this move statement.  It should continue read
            // until find the matched court order number. The export_hidden
            // legal_action contains correct standard number (court order number
            // ). MOVE statement will overlay the correct statard number.
            if (AsChar(export.HiddenPf19FromQcol.Flag) == 'Y' && AsChar
              (export.MultiCourtCase.Flag) == 'Y')
            {
            }
            else
            {
              export.MultiCourtCase.Flag = "N";
              MoveLegalAction1(entities.LegalAction, export.SelectCourtOrder);
              MoveLegalAction3(entities.LegalAction, export.HiddenLegalAction);

              // 06/03/08 CQ3259 commented out redundant code.
            }
          }
          else
          {
            export.MultiCourtCase.Flag = "Y";

            // 06/03/08 CQ3259 when PF19 returns from QCOL, don't set the 
            // standard number to space so Court Order field will not be blank
            // on QDBT.
            if (AsChar(export.HiddenPf19FromQcol.Flag) == 'Y' && !
              IsEmpty(export.SelectCourtOrder.StandardNumber))
            {
            }
            else
            {
              export.SelectCourtOrder.StandardNumber = "";
            }

            break;
          }
        }

        // -------------------------------------------------------------------------------
        // 10/15/07  G. Pan   CQ488  Added Read infrastructure statements to 
        // display last_modification_review_date
        //                    for the field 'Last Mod Rvw Dt'.
        // -------------------------------------------------------------------------------
        if (ReadInfrastructure())
        {
          export.SelectCourtOrder.LastModificationReviewDate =
            Date(entities.Infrastructure.CreatedTimestamp);
        }

        // -------------------------------------------------------------------------------
        // 12/03/07  G. Pan   CQ488  If this date were stored as max date (
        // 12312099),
        //                    convert it to zeros and don't display it on the 
        // screen.
        // -------------------------------------------------------------------------------
        if (Equal(export.SelectCourtOrder.LastModificationReviewDate,
          local.Max.Date))
        {
          export.SelectCourtOrder.LastModificationReviewDate = null;
        }

        if (AsChar(export.MultiCourtCase.Flag) == 'Y')
        {
          if (IsEmpty(import.FlowFromLacs.StandardNumber) && IsEmpty
            (export.SelectCourtOrder.StandardNumber))
          {
            export.Filter.Classification = "J";
            ExitState = "ECO_LNK_TO_LACS";

            return;
          }
        }

        if (IsEmpty(export.SelectCourtOrder.StandardNumber))
        {
          var field = GetField(export.CourtOrderPrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;

          ExitState = "FN0000_COURT_ORDER_NF";

          return;
        }
      }

      if (AsChar(export.MultiCourtCase.Flag) == 'Y')
      {
        var field = GetField(export.CourtOrderPrompt, "flag");

        field.Color = "green";
        field.Protected = false;
      }
      else
      {
        var field = GetField(export.CourtOrderPrompt, "flag");

        field.Color = "cyan";
        field.Protected = true;
      }

      if (ReadMonthlyObligeeSummaryCsePersonAccount())
      {
        local.GrantTemp.YearMonth = entities.MonthlyObligeeSummary.Year * 100;
        local.GrantTemp.YearMonth += entities.MonthlyObligeeSummary.Month;
        export.GrantDate.Date = IntToDate(local.GrantTemp.YearMonth * 100 + 1);
        export.MonthlyObligeeSummary.DisbursementsSuppressed =
          entities.MonthlyObligeeSummary.DisbursementsSuppressed;
      }

      local.OmitColDtls.Flag = "N";
      UseFnComputeSummaryTotalsDtl2();
      export.Med.NumericalDollarValue = export.Amts.MsCurrArrears;

      // CQ 50344
      UseFnComputeSummaryTotals2();
      local.ShowInactive.SelectChar = "N";

      // ***  Read for Last Date Court Order was Modified  ***
      if (ReadLegalAction2())
      {
        export.LastModDate.Date = entities.LegalAction.FiledDate;
      }

      // *** Admin Act Cert - FDSO/SDSO/CRED/TREAS ***
      // -------------------------------------------------------------------------
      // Per PR# 98441 : The following code is written to fix this PR.
      //                                                             
      // -- Vithal (07/28/00)
      // ------------------------------------------------------------------------
      if (ReadCreditReportingAction())
      {
        export.CreditReportingAction.CseActionCode =
          entities.CreditReportingAction.CseActionCode;
        export.CreditReportingAction.CraTransDate =
          entities.CreditReportingAction.CraTransDate;
      }

      // Find the most recent FDSO record.
      if (ReadAdministrativeActCertification1())
      {
        // The most recent FDSO record is for decertification.  Do not display 
        // any value.
        if (Lt(local.NullDateWorkArea.Date,
          entities.AdministrativeActCertification.DecertifiedDate))
        {
          goto Read1;
        }

        // Set the FDSO amount now, but it may be reset to 0 later if all 
        // obligations are exempted.
        export.Fdso.CurrentAmount =
          entities.AdministrativeActCertification.CurrentAmount;

        // Find all of the obligations that are ordered to the AP that are 
        // associated with
        // legal actions having the selected standard number.
        foreach(var item in ReadObligation())
        {
          // Check to see if the obligation has been exempted.
          if (!ReadObligationAdmActionExemption())
          {
            // At least one obligation has been found that has not been 
            // exempted.
            // Escape, leaving the FDSO amount populated.
            goto Read1;
          }
        }

        // Either all related obligations have been exempted or there are no 
        // obligations at all.
        // Set FDSO amount to 0 so it does not display on the screen.
        export.Fdso.CurrentAmount = 0;
      }

Read1:

      foreach(var item in ReadAdministrativeActCertification2())
      {
        local.CheckSdso.Date =
          AddDays(entities.AdministrativeActCertification.CurrentAmountDate, 7);
          

        if (Lt(local.Current.Date, local.CheckSdso.Date))
        {
          export.Sdso.CurrentAmount =
            entities.AdministrativeActCertification.CurrentAmount;

          break;
        }
        else
        {
          export.Sdso.CurrentAmount = 0;

          break;
        }
      }

      // -------------------------------------------------------------------------
      // Per PR# 98441 : The following code is commented as it is setting values
      // incorrectly. The above code is written instead.
      //                                                             
      // -- Vithal (07/28/00)
      // ------------------------------------------------------------------------
      // ****************************
      // *** Calculate Last Month ***
      // ****************************
      local.LastMonth.Month = local.Current.Month - 1;
      local.LastMonth.Year = local.Current.Year;

      if (local.LastMonth.Month < 1)
      {
        local.LastMonth.Month = 12;
        --local.LastMonth.Year;
      }

      local.NextMonth.Month = local.Current.Month + 1;
      local.NextMonth.Year = local.Current.Year;

      if (local.NextMonth.Month > 12)
      {
        local.NextMonth.Month = 1;
        ++local.NextMonth.Year;
      }

      local.NextMonth.Date = IntToDate(local.NextMonth.Year * 10000 + local
        .NextMonth.Month * 100 + 1);
      local.FirstOfCurrent.Date = IntToDate(local.Current.Year * 10000 + local
        .Current.Month * 100 + 1);

      // : M. Brown, March 30, 2001 - changed this read to qualify on legal 
      // action standard number instead of it being in a loop that reads each
      // legal action using standard number as a key.
      foreach(var item in ReadCollection2())
      {
        // -----------------------------------------------------------------------------
        // Per PR# 82737 the following code is commented and rewritten. The 'Pd 
        // Last Mo' field displays incorrect amount. The code is rewritten below
        // after commented code.
        //                                                      
        // Vithal Madhira( 01/12/2000)
        // --------------------------------------------------------------------------
        local.Collection.Month = Month(entities.Collection.CollectionDt);
        local.Collection.Year = Year(entities.Collection.CollectionDt);

        if (local.Collection.Month == local.LastMonth.Month && local
          .Collection.Year == local.LastMonth.Year)
        {
          export.PdLastMonth.NumericalDollarValue += entities.Collection.Amount;

          if (AsChar(entities.Collection.AppliedToCode) == 'C')
          {
            export.CurrPdLastMo.NumericalDollarValue += entities.Collection.
              Amount;
          }
        }
      }

      // : M. Brown, PR # 122212
      if (ReadCollection1())
      {
        if (ReadCashReceiptDetailCollectionType())
        {
          export.IwoPymnt.Flag = "Y";
        }
        else
        {
          export.IwoPymnt.Flag = "N";
        }
      }

      if (ReadCashReceiptDetail())
      {
        export.LastPymnt.Date = entities.CashReceiptDetail.CollectionDate;
      }

      // *** IWO SECTION ***
      if (ReadLegalAction1())
      {
        // *** Read for Legal Action Detail  10081999 ***
        export.IwoCurrent.TotalCurrency = 0;
        export.IwoArrears.TotalCurrency = 0;

        if (Equal(entities.LegalAction.ActionTaken, "IWONOTKT") || Equal
          (entities.LegalAction.ActionTaken, "IWOTERM"))
        {
          // -------------------------------------------------------------------
          //                    IWO terminated.
          // -------------------------------------------------------------------
          goto Read2;
        }
        else if (Equal(entities.LegalAction.ActionTaken, "IWO") || Equal
          (entities.LegalAction.ActionTaken, "IWOMODO") || Equal
          (entities.LegalAction.ActionTaken, "IWONOTKM") || Equal
          (entities.LegalAction.ActionTaken, "IWONOTKS") || Equal
          (entities.LegalAction.ActionTaken, "NOIIWON") || Equal
          (entities.LegalAction.ActionTaken, "ORDIWO2"))
        {
          // -------------------------------------------------------------------
          //     In case of only the above 'action_taken' situations, check the 
          // IWO amounts for each legal_action_detail associated to obligation
          // types  "WC " or " WA"  and the AP is the associated obligor for the
          // legal_action_detail.
          // -------------------------------------------------------------------
          foreach(var item in ReadLegalActionDetailObligationType())
          {
            export.Iwo.EffectiveDate = entities.LegalActionDetail.EffectiveDate;

            if (Equal(entities.ObligationType.Code, "WC"))
            {
              export.IwoCurrent.TotalCurrency += entities.LegalActionDetail.
                CurrentAmount.GetValueOrDefault();
            }

            if (Equal(entities.ObligationType.Code, "WA"))
            {
              export.IwoArrears.TotalCurrency += entities.LegalActionDetail.
                ArrearsAmount.GetValueOrDefault();
            }
          }

          if (export.IwoArrears.TotalCurrency > 0 && export
            .IwoCurrent.TotalCurrency > 0)
          {
            export.Iwo.OrderType = "WB";
          }
          else if (export.IwoArrears.TotalCurrency > 0)
          {
            export.Iwo.OrderType = "WA";
          }
          else if (export.IwoCurrent.TotalCurrency > 0)
          {
            export.Iwo.OrderType = "WC";
          }
        }
      }

Read2:

      foreach(var item in ReadDebtDetail())
      {
        export.ArRecov.TotalCurrency += entities.DebtDetail.BalanceDueAmt;
      }

      if (AsChar(export.Case1.Status) == 'O' && IsExitState
        ("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (AsChar(export.Case1.Status) == 'O' && IsExitState
        ("INVALID_PROCESSING_MONTH"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (AsChar(export.Case1.Status) == 'O' && IsExitState("IM_HOUSEHOLD_NF"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (AsChar(export.Case1.Status) == 'C')
      {
        ExitState = "ACO_NI0000_DISPLAY_CLOSED_CASE";
      }
    }
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.Type1 = source.Type1;
    target.AmountOwed = source.AmountOwed;
  }

  private static void MoveCase2(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.OfficeIdentifier = source.OfficeIdentifier;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
    target.ClosureLetterDate = source.ClosureLetterDate;
  }

  private static void MoveCase3(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
  }

  private static void MoveCaseFuncWorkSet(CaseFuncWorkSet source,
    CaseFuncWorkSet target)
  {
    target.FuncText3 = source.FuncText3;
    target.FuncDate = source.FuncDate;
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.AbsenceReasonCode = source.AbsenceReasonCode;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCreditReportingAction(CreditReportingAction source,
    CreditReportingAction target)
  {
    target.CseActionCode = source.CseActionCode;
    target.CraTransDate = source.CraTransDate;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.OtherNumber = source.OtherNumber;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.AeCaseNumber = source.AeCaseNumber;
    target.OtherNumber = source.OtherNumber;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
  }

  private static void MoveCsePerson3(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.AeCaseNumber = source.AeCaseNumber;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
  }

  private static void MoveCsePerson4(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.AeCaseNumber = source.AeCaseNumber;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet5(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.YearMonth = source.YearMonth;
  }

  private static void MoveImHousehold(ImHousehold source, ImHousehold target)
  {
    target.AeCaseNo = source.AeCaseNo;
    target.FirstBenefitDate = source.FirstBenefitDate;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.LastModificationReviewDate = source.LastModificationReviewDate;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveOeUraSummary(OeUraSummary source, OeUraSummary target)
  {
    target.Grant = source.Grant;
    target.Ura = source.Ura;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveScreenOwedAmountsDtl1(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.CsCurrDue = source.CsCurrDue;
    target.CsCurrArrears = source.CsCurrArrears;
    target.SpCurrDue = source.SpCurrDue;
    target.SpCurrArrears = source.SpCurrArrears;
    target.MsCurrDue = source.MsCurrDue;
    target.MsCurrArrears = source.MsCurrArrears;
    target.McCurrDue = source.McCurrDue;
    target.McCurrArrears = source.McCurrArrears;
  }

  private static void MoveScreenOwedAmountsDtl2(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.CsCurrDue = source.CsCurrDue;
    target.CsCurrArrears = source.CsCurrArrears;
    target.SpCurrDue = source.SpCurrDue;
    target.SpCurrArrears = source.SpCurrArrears;
    target.MsCurrDue = source.MsCurrDue;
    target.MsCurrArrears = source.MsCurrArrears;
    target.McCurrDue = source.McCurrDue;
    target.McCurrArrears = source.McCurrArrears;
    target.TotalCurrDue = source.TotalCurrDue;
    target.TotalCurrColl = source.TotalCurrColl;
    target.FeesArrearsOwed = source.FeesArrearsOwed;
    target.TotalInterestOwed = source.TotalInterestOwed;
  }

  private static void MoveScreenOwedAmountsDtl3(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.CsCurrDue = source.CsCurrDue;
    target.CsCurrArrears = source.CsCurrArrears;
    target.SpCurrDue = source.SpCurrDue;
    target.SpCurrArrears = source.SpCurrArrears;
    target.MsCurrDue = source.MsCurrDue;
    target.MsCurrArrears = source.MsCurrArrears;
    target.McCurrDue = source.McCurrDue;
    target.McCurrArrears = source.McCurrArrears;
    target.TotalCurrColl = source.TotalCurrColl;
    target.FeesArrearsOwed = source.FeesArrearsOwed;
    target.TotalInterestOwed = source.TotalInterestOwed;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseFnComputeSummaryTotals2()
  {
    var useImport = new FnComputeSummaryTotals2.Import();
    var useExport = new FnComputeSummaryTotals2.Export();

    useImport.Obligor.Number = export.ApClient.Number;
    useImport.FilterByStdNo.StandardNumber =
      export.SelectCourtOrder.StandardNumber;

    Call(FnComputeSummaryTotals2.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnComputeSummaryTotalsDtl2()
  {
    var useImport = new FnComputeSummaryTotalsDtl2.Import();
    var useExport = new FnComputeSummaryTotalsDtl2.Export();

    useImport.Obligor.Number = export.ApClient.Number;
    useImport.FilterByStdNo.StandardNumber =
      export.SelectCourtOrder.StandardNumber;
    useImport.OmitCollectionDtlsInd.Flag = local.OmitColDtls.Flag;

    Call(FnComputeSummaryTotalsDtl2.Execute, useImport, useExport);

    export.SpousalArrears.TotalCurrency =
      useExport.SpousalArrears.TotalCurrency;
    export.CostOfRaisingChild.TotalCurrency =
      useExport.CostOfRaisingChild.TotalCurrency;
    export.Export718BJudgement.TotalCurrency =
      useExport.Export718BJudgement.TotalCurrency;
    export.ArrearsJudgement.TotalCurrency =
      useExport.ArrearsJudgement.TotalCurrency;
    export.MedicalJudgement.TotalCurrency =
      useExport.MedicalJudgement.TotalCurrency;
    MoveScreenOwedAmountsDtl2(useExport.ScreenOwedAmountsDtl, export.Amts);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    useImport.Case1.Number = export.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiAltsBuildAliasAndSsn()
  {
    var useImport = new SiAltsBuildAliasAndSsn.Import();
    var useExport = new SiAltsBuildAliasAndSsn.Export();

    MoveCsePersonsWorkSet1(export.ApCsePersonsWorkSet, useImport.Ap1);
    MoveCsePersonsWorkSet1(export.ArCsePersonsWorkSet, useImport.Ar1);

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    local.ApAltOccur.Flag = useExport.ApOccur.Flag;
    local.ArAltOccur.Flag = useExport.ArOccur.Flag;
  }

  private void UseSiCadsReadCaseDetails()
  {
    var useImport = new SiCadsReadCaseDetails.Import();
    var useExport = new SiCadsReadCaseDetails.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiCadsReadCaseDetails.Execute, useImport, useExport);

    export.Program.Code = useExport.Program.Code;
    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, export.CaseFuncWorkSet);
  }

  private void UseSiReadApCaseRoleDetails()
  {
    var useImport = new SiReadApCaseRoleDetails.Import();
    var useExport = new SiReadApCaseRoleDetails.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiReadApCaseRoleDetails.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    MoveCaseRole2(useExport.ApCaseRole, export.ApCaseRole);
    export.OtherChilderen.Flag = useExport.OtherChildren.Flag;
    export.ApMultiCases.Flag = useExport.MultipleCases.Flag;
    MoveCsePerson1(useExport.ApCsePerson, export.ApClient);
    MoveCsePersonsWorkSet5(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    MoveCsePersonsWorkSet4(useExport.CsePersonsWorkSet,
      export.ArCsePersonsWorkSet);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private bool ReadAdministrativeActCertification1()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApClient.Number);
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
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 9);
        entities.AdministrativeActCertification.ApRespReceivedData =
          db.GetNullableDate(reader, 10);
        entities.AdministrativeActCertification.DateStayed =
          db.GetNullableDate(reader, 11);
        entities.AdministrativeActCertification.DateStayReleased =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeActCertification.HighestAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AdministrativeActCertification.AmountOwed =
          db.GetInt32(reader, 14);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 15);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private IEnumerable<bool> ReadAdministrativeActCertification2()
  {
    entities.AdministrativeActCertification.Populated = false;

    return ReadEach("ReadAdministrativeActCertification2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApClient.Number);
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
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 9);
        entities.AdministrativeActCertification.ApRespReceivedData =
          db.GetNullableDate(reader, 10);
        entities.AdministrativeActCertification.DateStayed =
          db.GetNullableDate(reader, 11);
        entities.AdministrativeActCertification.DateStayReleased =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeActCertification.HighestAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AdministrativeActCertification.AmountOwed =
          db.GetInt32(reader, 14);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 15);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);

        return true;
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "numb", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Next.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 7);
        entities.CollectionType.Code = db.GetString(reader, 8);
        entities.CollectionType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCodeValue1()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "closureReason", export.Case1.ClosureReason ?? "");
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue2",
      (db, command) =>
      {
        db.SetString(command, "code", export.Program.Code);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCollection1()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 17);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 17);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadCreditReportingAction()
  {
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApClient.Number);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;
        CheckValid<CreditReportingAction>("CpaType",
          entities.CreditReportingAction.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.CreditReportingAction.AacType);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "numb", export.ApCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadDisbursementStatusHistory()
  {
    entities.DisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.Function = db.GetNullableString(reader, 9);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.ActionTaken = db.GetString(reader, 3);
        entities.LegalAction.Type1 = db.GetString(reader, 4);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.ActionTaken = db.GetString(reader, 3);
        entities.LegalAction.Type1 = db.GetString(reader, 4);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "cspNumber", export.ApClient.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.ActionTaken = db.GetString(reader, 3);
        entities.LegalAction.Type1 = db.GetString(reader, 4);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(
          command, "cspNumber", export.ApCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 7);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 8);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ObligationType.Code = db.GetString(reader, 9);
        entities.ObligationType.Classification = db.GetString(reader, 10);
        entities.LegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadMonthlyObligeeSummaryCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;
    entities.MonthlyObligeeSummary.Populated = false;

    return Read("ReadMonthlyObligeeSummaryCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspSNumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.MonthlyObligeeSummary.Year = db.GetInt32(reader, 0);
        entities.MonthlyObligeeSummary.Month = db.GetInt32(reader, 1);
        entities.MonthlyObligeeSummary.DisbursementsSuppressed =
          db.GetNullableDecimal(reader, 2);
        entities.MonthlyObligeeSummary.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.MonthlyObligeeSummary.CpaSType = db.GetString(reader, 4);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 4);
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 5);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 5);
        entities.CsePersonAccount.Populated = true;
        entities.MonthlyObligeeSummary.Populated = true;
        CheckValid<MonthlyObligeeSummary>("CpaSType",
          entities.MonthlyObligeeSummary.CpaSType);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.SelectCourtOrder.StandardNumber ?? "");
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private bool ReadObligationAdmActionExemption()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption",
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
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
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
    /// A value of MedicalJudgement.
    /// </summary>
    [JsonPropertyName("medicalJudgement")]
    public Common MedicalJudgement
    {
      get => medicalJudgement ??= new();
      set => medicalJudgement = value;
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
    /// A value of Import718BJudgement.
    /// </summary>
    [JsonPropertyName("import718BJudgement")]
    public Common Import718BJudgement
    {
      get => import718BJudgement ??= new();
      set => import718BJudgement = value;
    }

    /// <summary>
    /// A value of Med.
    /// </summary>
    [JsonPropertyName("med")]
    public FinanceWorkAttributes Med
    {
      get => med ??= new();
      set => med = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of UraCum.
    /// </summary>
    [JsonPropertyName("uraCum")]
    public OeUraSummary UraCum
    {
      get => uraCum ??= new();
      set => uraCum = value;
    }

    /// <summary>
    /// A value of HistFlow.
    /// </summary>
    [JsonPropertyName("histFlow")]
    public Infrastructure HistFlow
    {
      get => histFlow ??= new();
      set => histFlow = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of FlowFromLacs.
    /// </summary>
    [JsonPropertyName("flowFromLacs")]
    public LegalAction FlowFromLacs
    {
      get => flowFromLacs ??= new();
      set => flowFromLacs = value;
    }

    /// <summary>
    /// A value of ArRecov.
    /// </summary>
    [JsonPropertyName("arRecov")]
    public Common ArRecov
    {
      get => arRecov ??= new();
      set => arRecov = value;
    }

    /// <summary>
    /// A value of IwoArrears.
    /// </summary>
    [JsonPropertyName("iwoArrears")]
    public Common IwoArrears
    {
      get => iwoArrears ??= new();
      set => iwoArrears = value;
    }

    /// <summary>
    /// A value of IwoCurrent.
    /// </summary>
    [JsonPropertyName("iwoCurrent")]
    public Common IwoCurrent
    {
      get => iwoCurrent ??= new();
      set => iwoCurrent = value;
    }

    /// <summary>
    /// A value of IwoPymnt.
    /// </summary>
    [JsonPropertyName("iwoPymnt")]
    public Common IwoPymnt
    {
      get => iwoPymnt ??= new();
      set => iwoPymnt = value;
    }

    /// <summary>
    /// A value of LastPymnt.
    /// </summary>
    [JsonPropertyName("lastPymnt")]
    public DateWorkArea LastPymnt
    {
      get => lastPymnt ??= new();
      set => lastPymnt = value;
    }

    /// <summary>
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public LegalActionIncomeSource Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
    }

    /// <summary>
    /// A value of CredCommon.
    /// </summary>
    [JsonPropertyName("credCommon")]
    public Common CredCommon
    {
      get => credCommon ??= new();
      set => credCommon = value;
    }

    /// <summary>
    /// A value of Treas.
    /// </summary>
    [JsonPropertyName("treas")]
    public AdministrativeActCertification Treas
    {
      get => treas ??= new();
      set => treas = value;
    }

    /// <summary>
    /// A value of CredAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("credAdministrativeActCertification")]
    public AdministrativeActCertification CredAdministrativeActCertification
    {
      get => credAdministrativeActCertification ??= new();
      set => credAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of Sdso.
    /// </summary>
    [JsonPropertyName("sdso")]
    public AdministrativeActCertification Sdso
    {
      get => sdso ??= new();
      set => sdso = value;
    }

    /// <summary>
    /// A value of Fdso.
    /// </summary>
    [JsonPropertyName("fdso")]
    public AdministrativeActCertification Fdso
    {
      get => fdso ??= new();
      set => fdso = value;
    }

    /// <summary>
    /// A value of GrantDate.
    /// </summary>
    [JsonPropertyName("grantDate")]
    public DateWorkArea GrantDate
    {
      get => grantDate ??= new();
      set => grantDate = value;
    }

    /// <summary>
    /// A value of MultiCourtCase.
    /// </summary>
    [JsonPropertyName("multiCourtCase")]
    public Common MultiCourtCase
    {
      get => multiCourtCase ??= new();
      set => multiCourtCase = value;
    }

    /// <summary>
    /// A value of PdLastMonth.
    /// </summary>
    [JsonPropertyName("pdLastMonth")]
    public FinanceWorkAttributes PdLastMonth
    {
      get => pdLastMonth ??= new();
      set => pdLastMonth = value;
    }

    /// <summary>
    /// A value of CurrPdLastMo.
    /// </summary>
    [JsonPropertyName("currPdLastMo")]
    public FinanceWorkAttributes CurrPdLastMo
    {
      get => currPdLastMo ??= new();
      set => currPdLastMo = value;
    }

    /// <summary>
    /// A value of LastModDate.
    /// </summary>
    [JsonPropertyName("lastModDate")]
    public DateWorkArea LastModDate
    {
      get => lastModDate ??= new();
      set => lastModDate = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of TotalPayoff.
    /// </summary>
    [JsonPropertyName("totalPayoff")]
    public Common TotalPayoff
    {
      get => totalPayoff ??= new();
      set => totalPayoff = value;
    }

    /// <summary>
    /// A value of Ura.
    /// </summary>
    [JsonPropertyName("ura")]
    public OeUraSummary Ura
    {
      get => ura ??= new();
      set => ura = value;
    }

    /// <summary>
    /// A value of Amts.
    /// </summary>
    [JsonPropertyName("amts")]
    public ScreenOwedAmountsDtl Amts
    {
      get => amts ??= new();
      set => amts = value;
    }

    /// <summary>
    /// A value of SelectCourtOrder.
    /// </summary>
    [JsonPropertyName("selectCourtOrder")]
    public LegalAction SelectCourtOrder
    {
      get => selectCourtOrder ??= new();
      set => selectCourtOrder = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public Common CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of FromComp.
    /// </summary>
    [JsonPropertyName("fromComp")]
    public CsePersonsWorkSet FromComp
    {
      get => fromComp ??= new();
      set => fromComp = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePersonsWorkSet Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ComnLink.
    /// </summary>
    [JsonPropertyName("comnLink")]
    public CsePersonsWorkSet ComnLink
    {
      get => comnLink ??= new();
      set => comnLink = value;
    }

    /// <summary>
    /// A value of ProgCodeDescription.
    /// </summary>
    [JsonPropertyName("progCodeDescription")]
    public CodeValue ProgCodeDescription
    {
      get => progCodeDescription ??= new();
      set => progCodeDescription = value;
    }

    /// <summary>
    /// A value of ArMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("arMultiCasesPrompt")]
    public Common ArMultiCasesPrompt
    {
      get => arMultiCasesPrompt ??= new();
      set => arMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ApMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("apMultiCasesPrompt")]
    public Common ApMultiCasesPrompt
    {
      get => apMultiCasesPrompt ??= new();
      set => apMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ApAltOccur.
    /// </summary>
    [JsonPropertyName("apAltOccur")]
    public Common ApAltOccur
    {
      get => apAltOccur ??= new();
      set => apAltOccur = value;
    }

    /// <summary>
    /// A value of ArAltOccur.
    /// </summary>
    [JsonPropertyName("arAltOccur")]
    public Common ArAltOccur
    {
      get => arAltOccur ??= new();
      set => arAltOccur = value;
    }

    /// <summary>
    /// A value of ArMultiCases.
    /// </summary>
    [JsonPropertyName("arMultiCases")]
    public Common ArMultiCases
    {
      get => arMultiCases ??= new();
      set => arMultiCases = value;
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
    /// A value of ArOtherCases.
    /// </summary>
    [JsonPropertyName("arOtherCases")]
    public Common ArOtherCases
    {
      get => arOtherCases ??= new();
      set => arOtherCases = value;
    }

    /// <summary>
    /// A value of ApOtherCases.
    /// </summary>
    [JsonPropertyName("apOtherCases")]
    public Common ApOtherCases
    {
      get => apOtherCases ??= new();
      set => apOtherCases = value;
    }

    /// <summary>
    /// A value of AltSsnAlias.
    /// </summary>
    [JsonPropertyName("altSsnAlias")]
    public TextWorkArea AltSsnAlias
    {
      get => altSsnAlias ??= new();
      set => altSsnAlias = value;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of OtherChildren.
    /// </summary>
    [JsonPropertyName("otherChildren")]
    public Common OtherChildren
    {
      get => otherChildren ??= new();
      set => otherChildren = value;
    }

    /// <summary>
    /// A value of ApMultiCases.
    /// </summary>
    [JsonPropertyName("apMultiCases")]
    public Common ApMultiCases
    {
      get => apMultiCases ??= new();
      set => apMultiCases = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApClient.
    /// </summary>
    [JsonPropertyName("apClient")]
    public CsePerson ApClient
    {
      get => apClient ??= new();
      set => apClient = value;
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
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of CaseCloseRsn.
    /// </summary>
    [JsonPropertyName("caseCloseRsn")]
    public CodeValue CaseCloseRsn
    {
      get => caseCloseRsn ??= new();
      set => caseCloseRsn = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
    }

    /// <summary>
    /// A value of DisbSupp.
    /// </summary>
    [JsonPropertyName("disbSupp")]
    public Common DisbSupp
    {
      get => disbSupp ??= new();
      set => disbSupp = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of HiddenPf19FromQcol.
    /// </summary>
    [JsonPropertyName("hiddenPf19FromQcol")]
    public Common HiddenPf19FromQcol
    {
      get => hiddenPf19FromQcol ??= new();
      set => hiddenPf19FromQcol = value;
    }

    private Common spousalArrears;
    private Common costOfRaisingChild;
    private Common medicalJudgement;
    private Common arrearsJudgement;
    private Common import718BJudgement;
    private FinanceWorkAttributes med;
    private CsePerson arCsePerson;
    private ImHousehold imHousehold;
    private OeUraSummary uraCum;
    private Infrastructure histFlow;
    private LegalAction hiddenLegalAction;
    private ScreenOwedAmounts screenOwedAmounts;
    private LegalAction flowFromLacs;
    private Common arRecov;
    private Common iwoArrears;
    private Common iwoCurrent;
    private Common iwoPymnt;
    private DateWorkArea lastPymnt;
    private LegalActionIncomeSource iwo;
    private Common credCommon;
    private AdministrativeActCertification treas;
    private AdministrativeActCertification credAdministrativeActCertification;
    private AdministrativeActCertification sdso;
    private AdministrativeActCertification fdso;
    private DateWorkArea grantDate;
    private Common multiCourtCase;
    private FinanceWorkAttributes pdLastMonth;
    private FinanceWorkAttributes currPdLastMo;
    private DateWorkArea lastModDate;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private Common totalPayoff;
    private OeUraSummary ura;
    private ScreenOwedAmountsDtl amts;
    private LegalAction selectCourtOrder;
    private Collection collection;
    private Common courtOrderPrompt;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private CsePersonsWorkSet fromComp;
    private CsePersonsWorkSet ch;
    private CaseRole arCaseRole;
    private CsePersonsWorkSet comnLink;
    private CodeValue progCodeDescription;
    private Common arMultiCasesPrompt;
    private Common apMultiCasesPrompt;
    private Common apAltOccur;
    private Common arAltOccur;
    private Common arMultiCases;
    private Office office;
    private Common arOtherCases;
    private Common apOtherCases;
    private TextWorkArea altSsnAlias;
    private Standard standard;
    private Common multipleAps;
    private Common uci;
    private Common otherChildren;
    private Common apMultiCases;
    private CaseRole apCaseRole;
    private CsePerson apClient;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apPrompt;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private ServiceProvider serviceProvider;
    private CodeValue caseCloseRsn;
    private Case1 case1;
    private Case1 next;
    private CreditReportingAction creditReportingAction;
    private Common disbSupp;
    private WorkArea headerLine;
    private Common hiddenPf19FromQcol;
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
    /// A value of MedicalJudgement.
    /// </summary>
    [JsonPropertyName("medicalJudgement")]
    public Common MedicalJudgement
    {
      get => medicalJudgement ??= new();
      set => medicalJudgement = value;
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
    /// A value of Export718BJudgement.
    /// </summary>
    [JsonPropertyName("export718BJudgement")]
    public Common Export718BJudgement
    {
      get => export718BJudgement ??= new();
      set => export718BJudgement = value;
    }

    /// <summary>
    /// A value of Med.
    /// </summary>
    [JsonPropertyName("med")]
    public FinanceWorkAttributes Med
    {
      get => med ??= new();
      set => med = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of UraCum.
    /// </summary>
    [JsonPropertyName("uraCum")]
    public OeUraSummary UraCum
    {
      get => uraCum ??= new();
      set => uraCum = value;
    }

    /// <summary>
    /// A value of HistFlow.
    /// </summary>
    [JsonPropertyName("histFlow")]
    public Infrastructure HistFlow
    {
      get => histFlow ??= new();
      set => histFlow = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// A value of FlowFromLacs.
    /// </summary>
    [JsonPropertyName("flowFromLacs")]
    public LegalAction FlowFromLacs
    {
      get => flowFromLacs ??= new();
      set => flowFromLacs = value;
    }

    /// <summary>
    /// A value of ArRecov.
    /// </summary>
    [JsonPropertyName("arRecov")]
    public Common ArRecov
    {
      get => arRecov ??= new();
      set => arRecov = value;
    }

    /// <summary>
    /// A value of IwoArrears.
    /// </summary>
    [JsonPropertyName("iwoArrears")]
    public Common IwoArrears
    {
      get => iwoArrears ??= new();
      set => iwoArrears = value;
    }

    /// <summary>
    /// A value of IwoCurrent.
    /// </summary>
    [JsonPropertyName("iwoCurrent")]
    public Common IwoCurrent
    {
      get => iwoCurrent ??= new();
      set => iwoCurrent = value;
    }

    /// <summary>
    /// A value of IwoPymnt.
    /// </summary>
    [JsonPropertyName("iwoPymnt")]
    public Common IwoPymnt
    {
      get => iwoPymnt ??= new();
      set => iwoPymnt = value;
    }

    /// <summary>
    /// A value of LastPymnt.
    /// </summary>
    [JsonPropertyName("lastPymnt")]
    public DateWorkArea LastPymnt
    {
      get => lastPymnt ??= new();
      set => lastPymnt = value;
    }

    /// <summary>
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public LegalActionIncomeSource Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
    }

    /// <summary>
    /// A value of CredCommon.
    /// </summary>
    [JsonPropertyName("credCommon")]
    public Common CredCommon
    {
      get => credCommon ??= new();
      set => credCommon = value;
    }

    /// <summary>
    /// A value of Treas.
    /// </summary>
    [JsonPropertyName("treas")]
    public AdministrativeActCertification Treas
    {
      get => treas ??= new();
      set => treas = value;
    }

    /// <summary>
    /// A value of CredAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("credAdministrativeActCertification")]
    public AdministrativeActCertification CredAdministrativeActCertification
    {
      get => credAdministrativeActCertification ??= new();
      set => credAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of Sdso.
    /// </summary>
    [JsonPropertyName("sdso")]
    public AdministrativeActCertification Sdso
    {
      get => sdso ??= new();
      set => sdso = value;
    }

    /// <summary>
    /// A value of Fdso.
    /// </summary>
    [JsonPropertyName("fdso")]
    public AdministrativeActCertification Fdso
    {
      get => fdso ??= new();
      set => fdso = value;
    }

    /// <summary>
    /// A value of MultiCourtCase.
    /// </summary>
    [JsonPropertyName("multiCourtCase")]
    public Common MultiCourtCase
    {
      get => multiCourtCase ??= new();
      set => multiCourtCase = value;
    }

    /// <summary>
    /// A value of CurrPdLastMo.
    /// </summary>
    [JsonPropertyName("currPdLastMo")]
    public FinanceWorkAttributes CurrPdLastMo
    {
      get => currPdLastMo ??= new();
      set => currPdLastMo = value;
    }

    /// <summary>
    /// A value of PdLastMonth.
    /// </summary>
    [JsonPropertyName("pdLastMonth")]
    public FinanceWorkAttributes PdLastMonth
    {
      get => pdLastMonth ??= new();
      set => pdLastMonth = value;
    }

    /// <summary>
    /// A value of Ura.
    /// </summary>
    [JsonPropertyName("ura")]
    public OeUraSummary Ura
    {
      get => ura ??= new();
      set => ura = value;
    }

    /// <summary>
    /// A value of LastModDate.
    /// </summary>
    [JsonPropertyName("lastModDate")]
    public DateWorkArea LastModDate
    {
      get => lastModDate ??= new();
      set => lastModDate = value;
    }

    /// <summary>
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public Common CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
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

    /// <summary>
    /// A value of GrantDate.
    /// </summary>
    [JsonPropertyName("grantDate")]
    public DateWorkArea GrantDate
    {
      get => grantDate ??= new();
      set => grantDate = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of TotalPayoff.
    /// </summary>
    [JsonPropertyName("totalPayoff")]
    public Common TotalPayoff
    {
      get => totalPayoff ??= new();
      set => totalPayoff = value;
    }

    /// <summary>
    /// A value of Amts.
    /// </summary>
    [JsonPropertyName("amts")]
    public ScreenOwedAmountsDtl Amts
    {
      get => amts ??= new();
      set => amts = value;
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
    /// A value of SelectCourtOrder.
    /// </summary>
    [JsonPropertyName("selectCourtOrder")]
    public LegalAction SelectCourtOrder
    {
      get => selectCourtOrder ??= new();
      set => selectCourtOrder = value;
    }

    /// <summary>
    /// A value of ComnLink.
    /// </summary>
    [JsonPropertyName("comnLink")]
    public CsePersonsWorkSet ComnLink
    {
      get => comnLink ??= new();
      set => comnLink = value;
    }

    /// <summary>
    /// A value of ProgCodeDescription.
    /// </summary>
    [JsonPropertyName("progCodeDescription")]
    public CodeValue ProgCodeDescription
    {
      get => progCodeDescription ??= new();
      set => progCodeDescription = value;
    }

    /// <summary>
    /// A value of ArMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("arMultiCasesPrompt")]
    public Common ArMultiCasesPrompt
    {
      get => arMultiCasesPrompt ??= new();
      set => arMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ApMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("apMultiCasesPrompt")]
    public Common ApMultiCasesPrompt
    {
      get => apMultiCasesPrompt ??= new();
      set => apMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ArMultiCases.
    /// </summary>
    [JsonPropertyName("arMultiCases")]
    public Common ArMultiCases
    {
      get => arMultiCases ??= new();
      set => arMultiCases = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of AltSsnAlias.
    /// </summary>
    [JsonPropertyName("altSsnAlias")]
    public TextWorkArea AltSsnAlias
    {
      get => altSsnAlias ??= new();
      set => altSsnAlias = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of OtherChilderen.
    /// </summary>
    [JsonPropertyName("otherChilderen")]
    public Common OtherChilderen
    {
      get => otherChilderen ??= new();
      set => otherChilderen = value;
    }

    /// <summary>
    /// A value of ApMultiCases.
    /// </summary>
    [JsonPropertyName("apMultiCases")]
    public Common ApMultiCases
    {
      get => apMultiCases ??= new();
      set => apMultiCases = value;
    }

    /// <summary>
    /// A value of ApClient.
    /// </summary>
    [JsonPropertyName("apClient")]
    public CsePerson ApClient
    {
      get => apClient ??= new();
      set => apClient = value;
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
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CaseCloseRsn.
    /// </summary>
    [JsonPropertyName("caseCloseRsn")]
    public CodeValue CaseCloseRsn
    {
      get => caseCloseRsn ??= new();
      set => caseCloseRsn = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HhBenDate.
    /// </summary>
    [JsonPropertyName("hhBenDate")]
    public ImHousehold HhBenDate
    {
      get => hhBenDate ??= new();
      set => hhBenDate = value;
    }

    /// <summary>
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
    }

    /// <summary>
    /// A value of DisbSupp.
    /// </summary>
    [JsonPropertyName("disbSupp")]
    public Common DisbSupp
    {
      get => disbSupp ??= new();
      set => disbSupp = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of HiddenPf19FromQcol.
    /// </summary>
    [JsonPropertyName("hiddenPf19FromQcol")]
    public Common HiddenPf19FromQcol
    {
      get => hiddenPf19FromQcol ??= new();
      set => hiddenPf19FromQcol = value;
    }

    private Common spousalArrears;
    private Common costOfRaisingChild;
    private Common medicalJudgement;
    private Common arrearsJudgement;
    private Common export718BJudgement;
    private FinanceWorkAttributes med;
    private CsePerson arCsePerson;
    private ImHousehold imHousehold;
    private OeUraSummary uraCum;
    private Infrastructure histFlow;
    private LegalAction filter;
    private LegalAction hiddenLegalAction;
    private LegalAction flowFromLacs;
    private Common arRecov;
    private Common iwoArrears;
    private Common iwoCurrent;
    private Common iwoPymnt;
    private DateWorkArea lastPymnt;
    private LegalActionIncomeSource iwo;
    private Common credCommon;
    private AdministrativeActCertification treas;
    private AdministrativeActCertification credAdministrativeActCertification;
    private AdministrativeActCertification sdso;
    private AdministrativeActCertification fdso;
    private Common multiCourtCase;
    private FinanceWorkAttributes currPdLastMo;
    private FinanceWorkAttributes pdLastMonth;
    private OeUraSummary ura;
    private DateWorkArea lastModDate;
    private Common courtOrderPrompt;
    private AdministrativeActCertification administrativeActCertification;
    private DateWorkArea grantDate;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private Common totalPayoff;
    private ScreenOwedAmountsDtl amts;
    private ScreenOwedAmounts screenOwedAmounts;
    private LegalAction selectCourtOrder;
    private CsePersonsWorkSet comnLink;
    private CodeValue progCodeDescription;
    private Common arMultiCasesPrompt;
    private Common apMultiCasesPrompt;
    private Common arMultiCases;
    private CaseRole apCaseRole;
    private TextWorkArea altSsnAlias;
    private Common multipleAps;
    private Common otherChilderen;
    private Common apMultiCases;
    private CsePerson apClient;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apPrompt;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private Office office;
    private ServiceProvider serviceProvider;
    private CodeValue caseCloseRsn;
    private Case1 case1;
    private Case1 next;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private ImHousehold hhBenDate;
    private CreditReportingAction creditReportingAction;
    private Common disbSupp;
    private WorkArea headerLine;
    private Common hiddenPf19FromQcol;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of ApOpen.
    /// </summary>
    [JsonPropertyName("apOpen")]
    public Common ApOpen
    {
      get => apOpen ??= new();
      set => apOpen = value;
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
    /// A value of OmitColDtls.
    /// </summary>
    [JsonPropertyName("omitColDtls")]
    public Common OmitColDtls
    {
      get => omitColDtls ??= new();
      set => omitColDtls = value;
    }

    /// <summary>
    /// A value of NullAmts.
    /// </summary>
    [JsonPropertyName("nullAmts")]
    public ScreenOwedAmountsDtl NullAmts
    {
      get => nullAmts ??= new();
      set => nullAmts = value;
    }

    /// <summary>
    /// A value of FirstOfCurrent.
    /// </summary>
    [JsonPropertyName("firstOfCurrent")]
    public DateWorkArea FirstOfCurrent
    {
      get => firstOfCurrent ??= new();
      set => firstOfCurrent = value;
    }

    /// <summary>
    /// A value of NextMonth.
    /// </summary>
    [JsonPropertyName("nextMonth")]
    public DateWorkArea NextMonth
    {
      get => nextMonth ??= new();
      set => nextMonth = value;
    }

    /// <summary>
    /// A value of LastMonth.
    /// </summary>
    [JsonPropertyName("lastMonth")]
    public DateWorkArea LastMonth
    {
      get => lastMonth ??= new();
      set => lastMonth = value;
    }

    /// <summary>
    /// A value of MulitpleCourtNo.
    /// </summary>
    [JsonPropertyName("mulitpleCourtNo")]
    public Common MulitpleCourtNo
    {
      get => mulitpleCourtNo ??= new();
      set => mulitpleCourtNo = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of AeCaseNumber.
    /// </summary>
    [JsonPropertyName("aeCaseNumber")]
    public ImHousehold AeCaseNumber
    {
      get => aeCaseNumber ??= new();
      set => aeCaseNumber = value;
    }

    /// <summary>
    /// A value of AeNumber.
    /// </summary>
    [JsonPropertyName("aeNumber")]
    public CsePerson AeNumber
    {
      get => aeNumber ??= new();
      set => aeNumber = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of GrantTemp.
    /// </summary>
    [JsonPropertyName("grantTemp")]
    public DateWorkArea GrantTemp
    {
      get => grantTemp ??= new();
      set => grantTemp = value;
    }

    /// <summary>
    /// A value of GrantYear.
    /// </summary>
    [JsonPropertyName("grantYear")]
    public DateWorkArea GrantYear
    {
      get => grantYear ??= new();
      set => grantYear = value;
    }

    /// <summary>
    /// A value of GrantMonth.
    /// </summary>
    [JsonPropertyName("grantMonth")]
    public DateWorkArea GrantMonth
    {
      get => grantMonth ??= new();
      set => grantMonth = value;
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
    /// A value of FilterByIdObligationType.
    /// </summary>
    [JsonPropertyName("filterByIdObligationType")]
    public ObligationType FilterByIdObligationType
    {
      get => filterByIdObligationType ??= new();
      set => filterByIdObligationType = value;
    }

    /// <summary>
    /// A value of ShowInactive.
    /// </summary>
    [JsonPropertyName("showInactive")]
    public Common ShowInactive
    {
      get => showInactive ??= new();
      set => showInactive = value;
    }

    /// <summary>
    /// A value of ApAltOccur.
    /// </summary>
    [JsonPropertyName("apAltOccur")]
    public Common ApAltOccur
    {
      get => apAltOccur ??= new();
      set => apAltOccur = value;
    }

    /// <summary>
    /// A value of ArAltOccur.
    /// </summary>
    [JsonPropertyName("arAltOccur")]
    public Common ArAltOccur
    {
      get => arAltOccur ??= new();
      set => arAltOccur = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of NullCase.
    /// </summary>
    [JsonPropertyName("nullCase")]
    public Case1 NullCase
    {
      get => nullCase ??= new();
      set => nullCase = value;
    }

    /// <summary>
    /// A value of NullCsePerson.
    /// </summary>
    [JsonPropertyName("nullCsePerson")]
    public CsePerson NullCsePerson
    {
      get => nullCsePerson ??= new();
      set => nullCsePerson = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
    }

    /// <summary>
    /// A value of NoAps.
    /// </summary>
    [JsonPropertyName("noAps")]
    public Common NoAps
    {
      get => noAps ??= new();
      set => noAps = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CheckSdso.
    /// </summary>
    [JsonPropertyName("checkSdso")]
    public DateWorkArea CheckSdso
    {
      get => checkSdso ??= new();
      set => checkSdso = value;
    }

    private TextWorkArea date;
    private Common apOpen;
    private Common caseOpen;
    private Common omitColDtls;
    private ScreenOwedAmountsDtl nullAmts;
    private DateWorkArea firstOfCurrent;
    private DateWorkArea nextMonth;
    private DateWorkArea lastMonth;
    private Common mulitpleCourtNo;
    private ObligationTransaction obligationTransaction;
    private ImHousehold aeCaseNumber;
    private CsePerson aeNumber;
    private TextWorkArea year;
    private DateWorkArea grantTemp;
    private DateWorkArea grantYear;
    private DateWorkArea grantMonth;
    private ObligationType filterByClass;
    private LegalAction filterByIdLegalAction;
    private ObligationType filterByIdObligationType;
    private Common showInactive;
    private Common apAltOccur;
    private Common arAltOccur;
    private Common multipleAps;
    private Case1 nullCase;
    private CsePerson nullCsePerson;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea max;
    private Common invalid;
    private Common noAps;
    private AbendData abendData;
    private DateWorkArea collection;
    private DateWorkArea checkSdso;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    private AdministrativeAction administrativeAction;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private Infrastructure infrastructure;
    private LegalActionDetail legalActionDetail;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private CsePersonAccount csePersonAccount;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private LegalActionCaseRole legalActionCaseRole;
    private Tribunal tribunal;
    private AdministrativeActCertification administrativeActCertification;
    private Collection collection;
    private LegalAction legalAction;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private Code code;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private CreditReportingAction creditReportingAction;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
    private LegalActionPerson legalActionPerson;
    private LaPersonLaCaseRole laPersonLaCaseRole;
  }
#endregion
}

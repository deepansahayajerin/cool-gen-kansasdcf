// Program: QA_QCOL_QUICK_COLLECTION_DETAIL, ID: 372656930, model: 746.
// Short name: SWEQCOLP
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
/// A program: QA_QCOL_QUICK_COLLECTION_DETAIL.
/// </para>
/// <para>
/// RESP:   Quality Assurance.
/// QA Quick Reference AP Data Screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQcolQuickCollectionDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QCOL_QUICK_COLLECTION_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQcolQuickCollectionDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQcolQuickCollectionDetail.
  /// </summary>
  public QaQcolQuickCollectionDetail(IContext context, Import import,
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
    // --------------------------------------------------------
    //                      Maintenance Log
    // Date       Developer                Description
    // 02/11/1999   JF Caillouet             Initial Development
    // 03/01/1999   JF Caillouet             Clean up
    // 09/14/1999   JF Caillouet             Corrected Legal Action Display PR#
    // 73278
    // --------------------------------------------------------
    // ---------------------------------------------------------------------------------
    // PR# 86423            Vithal Madhira      01-27-2000
    // The code and screen  are  modified . According to SME the 'SDSO', 'FDSO' 
    // and 'Tres. Offset Recd.'  amounts on QCOL must appear exactly as on PAYR
    // (ie.  Collection amount and date).
    // PR# 94164           Vithal Madhira      07/13/2000
    // Fixed the code. Now  screen will display only the related court_order for
    // which AP is an OBLIGOR.
    //  PR# 96358       Vithal Madhira         07/13/2000
    // Fixed the code and now user will not be forced to go to LACS if the case 
    // has two legal actions with same 'standard number'.
    // -------------------------------------------------------------------------------
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // *********************************************
    // WR20202. 12-12-01 by L. Bachura. This WR provides that the QCOL screen 
    // display info for a closed case. All info will display except case
    // program. Per Karen Buchelle,it is not necessary to display it. The change
    // is effected by deleting the comparison of the discontinue date to the
    // local current date in the display section of the code.  The deleted
    // statment is  "and desired XX discontinue date if > local-current_date
    // work area date". This check was to verify that the discontinue date was
    // 2099-12-31.
    // PR139464. 2-26-02. Added logic so that AR with open role date would be 
    // the one selected when more than one AR on the case. LBachura
    // PR141927 on 4-2-02. Changed the exit state on the dialog flow on the QCOL
    // screen so that when there is multiple AP's the flow is automatic to the
    // COMP screen.,
    // 11/2002     SWSRKXD   Fix screen help Id.
    // -------------------------------------------------------------------------------
    // 05/29/08  G. Pan   CQ3259  added ief_supplied flag for PF19_from_QCOL.
    // -------------------------------------------------------------------------------
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
    MoveCaseRole(import.ApCaseRole, export.ApCaseRole);
    export.ApClient.Assign(import.ApClient);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.Ar.Assign(import.ArCsePersonsWorkSet);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.MultipleAps.Flag = import.MultipleAps.Flag;
    export.ApMultiCases.Flag = import.ApMultiCases.Flag;
    export.Next.Number = import.Next.Number;
    export.OtherChilderen.Flag = import.OtherChildren.Flag;
    export.ArMultiCases.Flag = import.ArMultiCases.Flag;
    MoveCsePersonsWorkSet5(import.FromComp, export.FromComp);
    export.AltSsnAlias.Text30 = import.AltSsnAlias.Text30;
    MoveOffice(import.Office, export.Office);
    export.ApMultiCasesPrompt.Flag = import.ApMultiCasesPrompt.Flag;
    export.ProgCodeDescription.Description =
      import.ProgCodeDescription.Description;
    export.ArMultiCasesPrompt.Flag = import.ArMultiCasesPrompt.Flag;
    export.ComnLink.Assign(import.ComnLink);

    // ***  Collection Section ***
    export.SelectCourtOrder.Assign(import.SelectCourtOrder);
    MoveLegalAction3(import.HiddenLegalAction, export.HiddenLegalAction);
    export.CourtOrderPrompt.Flag = import.CourtOrderPrompt.Flag;
    export.FlowFromLacs.Assign(import.FlowFromLacs);
    export.Amts.Assign(import.Amts);
    export.AmtLastPay.TotalCurrency = import.AmtLastPay.TotalCurrency;
    export.CurrPdLastMo.NumericalDollarValue =
      import.CurrPdLastMo.NumericalDollarValue;
    export.LastPymnt.Date = import.LastPymnt.Date;
    export.PdLastMo.NumericalDollarValue = import.PdLastMo.NumericalDollarValue;
    MoveCollection(import.Fdso, export.Fdso);
    MoveCollection(import.Sdso, export.Sdso);
    MoveCollection(import.TreasuryOffset, export.TreasuryOffset);
    export.ArrsPdCurrMo.NumericalDollarValue =
      import.ArrsPdCurrMo.NumericalDollarValue;
    export.ArrsPdLastMo.NumericalDollarValue =
      import.ArrsPdLastMo.NumericalDollarValue;
    export.FeesPdCurrMo.NumericalDollarValue =
      import.FeesPdCurrMo.NumericalDollarValue;
    export.FeesPdLastMo.NumericalDollarValue =
      import.FeesPdLastMo.NumericalDollarValue;
    export.MedPdCurrMo.NumericalDollarValue =
      import.MedPdCurrMo.NumericalDollarValue;
    export.MedPdLastMo.NumericalDollarValue =
      import.MedPdLastMo.NumericalDollarValue;
    export.InterestPdCurrMo.NumericalDollarValue =
      import.InterestPdCurrMo.NumericalDollarValue;
    export.InterestPdLastMo.NumericalDollarValue =
      import.InterestPdLastMo.NumericalDollarValue;
    export.FeesPaid.NumericalDollarValue = import.FeesPaid.NumericalDollarValue;
    export.ArDisbSupp.Number = import.ArDisbSupp.Number;
    export.DisbSuppressionStatusHistory.Assign(
      import.DisbSuppressionStatusHistory);
    export.MultiCourtCase.Flag = import.MultiCourtCase.Flag;

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    // ---  Next Tran and Security Logic  --
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
    local.Current.Month = Now().Date.Month;
    local.Current.Year = Now().Date.Year;

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

    if (AsChar(export.CourtOrderPrompt.Flag) == 'Y')
    {
      var field = GetField(export.ApPrompt, "selectChar");

      field.Color = "green";
      field.Protected = false;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETLACS":
        MoveLegalAction2(import.FlowFromLacs, export.SelectCourtOrder);
        export.CourtOrderPrompt.Flag = "";
        global.Command = "DISPLAY";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

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
            export.Ar.Assign(export.ComnLink);
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

        if (IsEmpty(import.FromComp.Number))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field1 = GetField(export.ApCsePersonsWorkSet, "number");

          field1.Error = true;

          return;
        }

        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
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
            MoveCsePersonsWorkSet3(export.Ar, export.ComnLink);
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
            if (AsChar(export.ApMultiCasesPrompt.Flag) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.ArMultiCasesPrompt.Flag) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.CourtOrderPrompt.Flag) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            break;
        }

        break;
      case "PVSCR":
        // -------------------------------------------------------------------------------
        // 05/29/08  G. Pan   CQ3259  Set "Y" to PF19_from_QCOLflag to fix 
        // filed_date &
        //                    last_modification_review_date disappeared problem 
        // on QDBT.
        // -------------------------------------------------------------------------------
        export.HiddenPf19FromQcol.Flag = "Y";
        ExitState = "ECO_XFR_TO_PREV";

        break;
      case "APSM":
        ExitState = "ECO_LNK_TO_APSM";

        break;
      case "PAYR":
        export.ImportSelCourtOrderNo.CourtOrderNumber =
          export.SelectCourtOrder.StandardNumber ?? "";
        ExitState = "ECO_XFR_TO_PAYR";

        break;
      case "REIP":
        ExitState = "ECO_XFER_TO_REIP";

        break;
      case "PSUP":
        export.ArDisbSupp.Number = export.Ar.Number;
        ExitState = "ECO_LNK_TO_PSUP";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "NXSCR":
        ExitState = "ECO_XFR_TO_NEXT_SCRN";

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
      export.HiddenPf19FromQcol.Flag = "";

      if (IsEmpty(export.Next.Number) || !
        Equal(export.Next.Number, export.Case1.Number))
      {
        // *** Clear fields before a display ***
        export.ServiceProvider.LastName = "";
        export.ServiceProvider.FirstName = "";
        export.Office.Name = "";
        export.Office.TypeCode = "";
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseFuncWorkSet.FuncDate = local.NullDateWorkArea.Date;
        export.CaseUnitFunctionAssignmt.Function = "";
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Cdvalue = "";
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseUnitFunctionAssignmt.Function = "";
        export.AltSsnAlias.Text30 = "";
        export.ApCsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
        export.Ar.Assign(local.NullCsePersonsWorkSet);
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.Case1.Assign(local.NullCase);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;
        export.ApMultiCases.Flag = "";
        export.ArMultiCases.Flag = "";

        // *** Collection section  ***
        export.SelectCourtOrder.StandardNumber = "";
        export.MultiCourtCase.Flag = "";
        export.FlowFromLacs.CourtCaseNumber = "";
        export.FlowFromLacs.StandardNumber = "";
      }

      // *** Collection section  ***
      // *** Move this out of the Case Protected section ***
      export.CurrPdLastMo.NumericalDollarValue = 0;
      export.AmtLastPay.TotalCurrency = 0;
      export.LastPymnt.Date = local.NullDateWorkArea.Date;
      export.Fdso.Amount = 0;
      export.Fdso.CollectionDt = local.NullDateWorkArea.Date;
      export.Sdso.Amount = 0;
      export.Sdso.CollectionDt = local.NullDateWorkArea.Date;
      export.TreasuryOffset.Amount = 0;
      export.TreasuryOffset.CollectionDt = local.NullDateWorkArea.Date;
      export.Amts.CsCurrColl = 0;
      export.Amts.SpCurrColl = 0;
      export.Amts.MsCurrColl = 0;
      export.Amts.TotalCurrColl = 0;
      export.Amts.LastCollAmt = 0;
      export.Amts.LastCollDt = local.NullDateWorkArea.Date;
      export.FeesPaid.NumericalDollarValue = 0;
      export.FeesPdCurrMo.NumericalDollarValue = 0;
      export.FeesPdLastMo.NumericalDollarValue = 0;
      export.InterestPdCurrMo.NumericalDollarValue = 0;
      export.InterestPdLastMo.NumericalDollarValue = 0;
      export.PdLastMo.NumericalDollarValue = 0;
      export.ArrsPdCurrMo.NumericalDollarValue = 0;
      export.ArrsPdLastMo.NumericalDollarValue = 0;
      export.MedPdCurrMo.NumericalDollarValue = 0;
      export.MedPdLastMo.NumericalDollarValue = 0;
      export.InterestPdCurrMo.NumericalDollarValue = 0;
      export.InterestPdLastMo.NumericalDollarValue = 0;
      export.DisbSuppressionStatusHistory.Type1 = "";
      export.DisbSuppressionStatusHistory.ReasonText =
        Spaces(DisbSuppressionStatusHistory.ReasonText_MaxLength);

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
            var field = GetField(export.CaseCloseRsn, "cdvalue");

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
            export.Ar.Number = entities.CsePerson.Number;
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
            export.Ar.Number = entities.CsePerson.Number;
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
            var field = GetField(export.Ar, "number");

            field.Error = true;

            ExitState = "CO0000_AR_NF";
          }

          return;
        }

        // *** Determine multiple AP's     ***
        export.MultipleAps.Flag = "";
        local.ApCounter.Count = 0;

        foreach(var item in ReadCsePerson4())
        {
          ++local.ApCounter.Count;

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
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;

        if (!IsEmpty(import.FromComp.Number))
        {
          export.ApCsePersonsWorkSet.Number = export.FromComp.Number;
        }

        // *** Get AP details  ***
        UseSiReadApCaseRoleDetails();

        // PR140816 on 4-2-02. Installed the following logic to prevent the 
        // display of inactive AP's.
        // Modified with PR146644 on 8-15-02 to fix mulit aps on an open case 
        // display issue. LBachura
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

          field.Color = "green";
          field.Protected = false;

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

      if (Equal(export.Ar.Ssn, "000000000"))
      {
        export.Ar.Ssn = "";
      }

      // *****************************************************************
      // ***        New Collection Section         ***
      // *****************************************************************
      if (IsEmpty(export.MultiCourtCase.Flag))
      {
        // -----------------------------------------------------------------------------------
        // PR# 94164, 96358    :   Changed the READ EACH to read '
        // cse_person_account'   where AP is of  TYPE obligor (R)
        //                                                         
        // Vithal (07/13/2000)
        // -------------------------------------------------------------------------------------
        foreach(var item in ReadLegalAction2())
        {
          if (Equal(entities.LegalAction.StandardNumber,
            export.SelectCourtOrder.StandardNumber))
          {
            MoveLegalAction1(entities.LegalAction, export.SelectCourtOrder);

            continue;
          }

          if (IsEmpty(export.MultiCourtCase.Flag))
          {
            export.MultiCourtCase.Flag = "N";
            MoveLegalAction1(entities.LegalAction, export.SelectCourtOrder);
          }
          else
          {
            export.MultiCourtCase.Flag = "Y";
            export.SelectCourtOrder.StandardNumber = "";

            break;
          }
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

      local.OmitColDtls.Flag = "N";
      UseFnComputeSummaryTotalsDtl();
      local.ShowInactive.SelectChar = "N";

      // *** Calc Next Month ***
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

      // ******************************
      // *** New Collection Section ***
      // *********************************************************************************************
      // *** 09/14/1999 - Added Test for Redisplay on Legal Action Infor and 
      // Financial Calculations,
      // *********************************************************************************************
      if (Equal(export.HiddenLegalAction.StandardNumber,
        export.SelectCourtOrder.StandardNumber))
      {
        // *** If the same Legal Action Std No. is used skip Computations 
        // already done. ***
      }
      else
      {
        foreach(var item in ReadLegalAction1())
        {
          export.HiddenLegalAction.StandardNumber =
            entities.LegalAction.StandardNumber;

          foreach(var item1 in ReadCollection())
          {
            // ***Current Month ***
            local.Collection.Month = Month(entities.Collection2.CollectionDt);
            local.Collection.Year = Year(entities.Collection2.CollectionDt);

            if (local.Collection.Month == local.Current.Month && local
              .Collection.Year == local.Current.Year)
            {
              switch(AsChar(entities.Collection2.AppliedToCode))
              {
                case 'A':
                  export.ArrsPdCurrMo.NumericalDollarValue += entities.
                    Collection2.Amount;

                  break;
                case 'I':
                  export.InterestPdCurrMo.NumericalDollarValue += entities.
                    Collection2.Amount;

                  break;
                default:
                  break;
              }

              if (ReadObligationType())
              {
                switch(AsChar(entities.ObligationType.Classification))
                {
                  case 'F':
                    export.FeesPdCurrMo.NumericalDollarValue += entities.
                      Collection2.Amount;
                    export.FeesPaid.NumericalDollarValue += entities.
                      Collection2.Amount;

                    break;
                  case 'M':
                    if (entities.ObligationType.SystemGeneratedIdentifier == 3
                      || entities.ObligationType.SystemGeneratedIdentifier == 19
                      )
                    {
                      export.MedPdCurrMo.NumericalDollarValue += entities.
                        Collection2.Amount;
                    }

                    break;
                  default:
                    break;
                }
              }
            }
            else if (local.Collection.Month == local.LastMonth.Month && local
              .Collection.Year == local.LastMonth.Year)
            {
              export.PdLastMo.NumericalDollarValue += entities.Collection2.
                Amount;

              // *** Last Month ***
              switch(AsChar(entities.Collection2.AppliedToCode))
              {
                case 'C':
                  export.CurrPdLastMo.NumericalDollarValue += entities.
                    Collection2.Amount;

                  break;
                case 'A':
                  export.ArrsPdLastMo.NumericalDollarValue += entities.
                    Collection2.Amount;

                  break;
                case 'I':
                  export.InterestPdLastMo.NumericalDollarValue += entities.
                    Collection2.Amount;

                  break;
                default:
                  break;
              }

              if (ReadObligationType())
              {
                switch(AsChar(entities.ObligationType.Classification))
                {
                  case 'F':
                    export.FeesPdLastMo.NumericalDollarValue += entities.
                      Collection2.Amount;
                    export.FeesPaid.NumericalDollarValue += entities.
                      Collection2.Amount;

                    break;
                  case 'M':
                    if (entities.ObligationType.SystemGeneratedIdentifier == 3
                      || entities.ObligationType.SystemGeneratedIdentifier == 19
                      )
                    {
                      export.MedPdLastMo.NumericalDollarValue += entities.
                        Collection2.Amount;
                    }

                    break;
                  default:
                    break;
                }
              }
            }
          }
        }

        // ---------------------------------------------------------------------------------
        // PR# 86423            Vithal Madhira      01-27-2000    The code and 
        // screen  are  modified . According to SME the 'SDSO', 'FDSO'  and '
        // Tres. Offset Recd'  amounts on QCOL must appear exactly as  on PAYR (
        // ie. Collection amount and date for COLLECTION_TYPE 'S' or 'K' or 'U'
        // or 'R',  'F',  & 'T or Y or Z' ). The following code is commented and
        // new code is written below.
        // -------------------------------------------------------------------------------
        if (ReadCashReceiptDetailCollectionType2())
        {
          export.Fdso.Amount = entities.CashReceiptDetail.CollectionAmount;
          export.Fdso.CollectionDt = entities.CashReceiptDetail.CollectionDate;
        }

        if (export.Fdso.Amount == 0 && Equal
          (export.Fdso.CollectionDt, local.NullDateWorkArea.Date))
        {
          if (ReadCashReceiptDetailCollectionType1())
          {
            export.Fdso.Amount = entities.CashReceiptDetail.CollectionAmount;
            export.Fdso.CollectionDt =
              entities.CashReceiptDetail.CollectionDate;
          }
        }

        if (ReadCashReceiptDetailCollectionType6())
        {
          export.Sdso.Amount = entities.CashReceiptDetail.CollectionAmount;
          export.Sdso.CollectionDt = entities.CashReceiptDetail.CollectionDate;
        }

        if (export.Sdso.Amount == 0 && Equal
          (export.Sdso.CollectionDt, local.NullDateWorkArea.Date))
        {
          if (ReadCashReceiptDetailCollectionType5())
          {
            export.Sdso.Amount = entities.CashReceiptDetail.CollectionAmount;
            export.Sdso.CollectionDt =
              entities.CashReceiptDetail.CollectionDate;
          }
        }

        if (ReadCashReceiptDetailCollectionType4())
        {
          export.TreasuryOffset.Amount =
            entities.CashReceiptDetail.CollectionAmount;
          export.TreasuryOffset.CollectionDt =
            entities.CashReceiptDetail.CollectionDate;
        }

        if (export.TreasuryOffset.Amount == 0 && Equal
          (export.TreasuryOffset.CollectionDt, local.NullDateWorkArea.Date))
        {
          if (ReadCashReceiptDetailCollectionType3())
          {
            export.TreasuryOffset.Amount =
              entities.CashReceiptDetail.CollectionAmount;
            export.TreasuryOffset.CollectionDt =
              entities.CashReceiptDetail.CollectionDate;
          }
        }

        local.ArDisbSupp.Type1 = "E";
        UseFnReadPersonDisbSuppression();
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
        ("FN0000_NO_ACTIVE_OR_FUTURE_SUPPR"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (AsChar(export.Case1.Status) == 'C')
      {
        ExitState = "ACO_NI0000_DISPLAY_CLOSED_CASE";
      }
    }
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

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.NumberOfChildren = source.NumberOfChildren;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.CollectionDt = source.CollectionDt;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
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
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet5(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet6(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveScreenOwedAmountsDtl(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.CsCurrDue = source.CsCurrDue;
    target.CsCurrOwed = source.CsCurrOwed;
    target.CsCurrArrears = source.CsCurrArrears;
    target.CsCurrColl = source.CsCurrColl;
    target.SpCurrDue = source.SpCurrDue;
    target.SpCurrOwed = source.SpCurrOwed;
    target.SpCurrArrears = source.SpCurrArrears;
    target.SpCurrColl = source.SpCurrColl;
    target.MsCurrDue = source.MsCurrDue;
    target.MsCurrOwed = source.MsCurrOwed;
    target.MsCurrColl = source.MsCurrColl;
    target.MsCurrArrears = source.MsCurrArrears;
    target.McCurrDue = source.McCurrDue;
    target.McCurrOwed = source.McCurrOwed;
    target.McCurrArrears = source.McCurrArrears;
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
    target.LastCollDt = source.LastCollDt;
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

  private void UseFnComputeSummaryTotalsDtl()
  {
    var useImport = new FnComputeSummaryTotalsDtl.Import();
    var useExport = new FnComputeSummaryTotalsDtl.Export();

    useImport.OmitCollectionDtlsInd.Flag = local.OmitColDtls.Flag;
    useImport.Obligor.Number = export.ApClient.Number;
    useImport.FilterByStdNo.StandardNumber =
      export.SelectCourtOrder.StandardNumber;

    Call(FnComputeSummaryTotalsDtl.Execute, useImport, useExport);

    MoveScreenOwedAmountsDtl(useExport.ScreenOwedAmountsDtl, export.Amts);
  }

  private void UseFnReadPersonDisbSuppression()
  {
    var useImport = new FnReadPersonDisbSuppression.Import();
    var useExport = new FnReadPersonDisbSuppression.Export();

    useImport.CsePerson.Number = import.ArDisbSupp.Number;
    useImport.CsePersonAccount.Type1 = local.ArDisbSupp.Type1;

    Call(FnReadPersonDisbSuppression.Execute, useImport, useExport);

    export.DisbSuppressionStatusHistory.Assign(
      useExport.DisbSuppressionStatusHistory);
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
    MoveCsePersonsWorkSet1(export.Ar, useImport.Ar1);

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    local.ApAltOccur.Flag = useExport.ApOccur.Flag;
    local.ArAltOccur.Flag = useExport.ArOccur.Flag;
  }

  private void UseSiCadsReadCaseDetails()
  {
    var useImport = new SiCadsReadCaseDetails.Import();
    var useExport = new SiCadsReadCaseDetails.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Ar.Number = export.Ar.Number;
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
    useImport.Ar.Number = export.Ar.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiReadApCaseRoleDetails.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.ApCaseRole.Assign(useExport.ApCaseRole);
    export.OtherChilderen.Flag = useExport.OtherChildren.Flag;
    export.ApMultiCases.Flag = useExport.MultipleCases.Flag;
    MoveCsePerson(useExport.ApCsePerson, export.ApClient);
    MoveCsePersonsWorkSet6(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet4(useExport.CsePersonsWorkSet, export.Ar);
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

  private bool ReadCashReceiptDetailCollectionType1()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber",
          export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 11);
        entities.CollectionType.Code = db.GetString(reader, 12);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType2()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorSsn", export.ApCsePersonsWorkSet.Ssn);
        db.SetNullableString(
          command, "oblgorPrsnNbr", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 11);
        entities.CollectionType.Code = db.GetString(reader, 12);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType3()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber",
          export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 11);
        entities.CollectionType.Code = db.GetString(reader, 12);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType4()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorSsn", export.ApCsePersonsWorkSet.Ssn);
        db.SetNullableString(
          command, "oblgorPrsnNbr", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 11);
        entities.CollectionType.Code = db.GetString(reader, 12);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType5()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber",
          export.SelectCourtOrder.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 11);
        entities.CollectionType.Code = db.GetString(reader, 12);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType6()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType6",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorSsn", export.ApCsePersonsWorkSet.Ssn);
        db.SetNullableString(
          command, "oblgorPrsnNbr", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 11);
        entities.CollectionType.Code = db.GetString(reader, 12);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;
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

  private IEnumerable<bool> ReadCollection()
  {
    entities.Collection2.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Collection2.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection2.AppliedToCode = db.GetString(reader, 1);
        entities.Collection2.CollectionDt = db.GetDate(reader, 2);
        entities.Collection2.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection2.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection2.CrtType = db.GetInt32(reader, 5);
        entities.Collection2.CstId = db.GetInt32(reader, 6);
        entities.Collection2.CrvId = db.GetInt32(reader, 7);
        entities.Collection2.CrdId = db.GetInt32(reader, 8);
        entities.Collection2.ObgId = db.GetInt32(reader, 9);
        entities.Collection2.CspNumber = db.GetString(reader, 10);
        entities.Collection2.CpaType = db.GetString(reader, 11);
        entities.Collection2.OtrId = db.GetInt32(reader, 12);
        entities.Collection2.OtrType = db.GetString(reader, 13);
        entities.Collection2.OtyId = db.GetInt32(reader, 14);
        entities.Collection2.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection2.Amount = db.GetDecimal(reader, 16);
        entities.Collection2.CourtOrderAppliedTo =
          db.GetNullableString(reader, 17);
        entities.Collection2.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection2.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection2.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection2.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection2.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection2.OtrType);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "numb", export.ApClient.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
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
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
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
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
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
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction1",
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

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
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

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection2.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Collection2.OtyId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
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
    /// A value of MultiCourtCase.
    /// </summary>
    [JsonPropertyName("multiCourtCase")]
    public Common MultiCourtCase
    {
      get => multiCourtCase ??= new();
      set => multiCourtCase = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of ArDisbSupp.
    /// </summary>
    [JsonPropertyName("arDisbSupp")]
    public CsePerson ArDisbSupp
    {
      get => arDisbSupp ??= new();
      set => arDisbSupp = value;
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
    /// A value of FeesPaid.
    /// </summary>
    [JsonPropertyName("feesPaid")]
    public FinanceWorkAttributes FeesPaid
    {
      get => feesPaid ??= new();
      set => feesPaid = value;
    }

    /// <summary>
    /// A value of MedPdCurrMo.
    /// </summary>
    [JsonPropertyName("medPdCurrMo")]
    public FinanceWorkAttributes MedPdCurrMo
    {
      get => medPdCurrMo ??= new();
      set => medPdCurrMo = value;
    }

    /// <summary>
    /// A value of MedPdLastMo.
    /// </summary>
    [JsonPropertyName("medPdLastMo")]
    public FinanceWorkAttributes MedPdLastMo
    {
      get => medPdLastMo ??= new();
      set => medPdLastMo = value;
    }

    /// <summary>
    /// A value of FeesPdCurrMo.
    /// </summary>
    [JsonPropertyName("feesPdCurrMo")]
    public FinanceWorkAttributes FeesPdCurrMo
    {
      get => feesPdCurrMo ??= new();
      set => feesPdCurrMo = value;
    }

    /// <summary>
    /// A value of FeesPdLastMo.
    /// </summary>
    [JsonPropertyName("feesPdLastMo")]
    public FinanceWorkAttributes FeesPdLastMo
    {
      get => feesPdLastMo ??= new();
      set => feesPdLastMo = value;
    }

    /// <summary>
    /// A value of InterestPdLastMo.
    /// </summary>
    [JsonPropertyName("interestPdLastMo")]
    public FinanceWorkAttributes InterestPdLastMo
    {
      get => interestPdLastMo ??= new();
      set => interestPdLastMo = value;
    }

    /// <summary>
    /// A value of InterestPdCurrMo.
    /// </summary>
    [JsonPropertyName("interestPdCurrMo")]
    public FinanceWorkAttributes InterestPdCurrMo
    {
      get => interestPdCurrMo ??= new();
      set => interestPdCurrMo = value;
    }

    /// <summary>
    /// A value of ArrsPdLastMo.
    /// </summary>
    [JsonPropertyName("arrsPdLastMo")]
    public FinanceWorkAttributes ArrsPdLastMo
    {
      get => arrsPdLastMo ??= new();
      set => arrsPdLastMo = value;
    }

    /// <summary>
    /// A value of ArrsPdCurrMo.
    /// </summary>
    [JsonPropertyName("arrsPdCurrMo")]
    public FinanceWorkAttributes ArrsPdCurrMo
    {
      get => arrsPdCurrMo ??= new();
      set => arrsPdCurrMo = value;
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
    /// A value of AmtLastPay.
    /// </summary>
    [JsonPropertyName("amtLastPay")]
    public Common AmtLastPay
    {
      get => amtLastPay ??= new();
      set => amtLastPay = value;
    }

    /// <summary>
    /// A value of PdLastMo.
    /// </summary>
    [JsonPropertyName("pdLastMo")]
    public FinanceWorkAttributes PdLastMo
    {
      get => pdLastMo ??= new();
      set => pdLastMo = value;
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
    /// A value of LastPymnt.
    /// </summary>
    [JsonPropertyName("lastPymnt")]
    public DateWorkArea LastPymnt
    {
      get => lastPymnt ??= new();
      set => lastPymnt = value;
    }

    /// <summary>
    /// A value of TreasuryOffset.
    /// </summary>
    [JsonPropertyName("treasuryOffset")]
    public Collection TreasuryOffset
    {
      get => treasuryOffset ??= new();
      set => treasuryOffset = value;
    }

    /// <summary>
    /// A value of Sdso.
    /// </summary>
    [JsonPropertyName("sdso")]
    public Collection Sdso
    {
      get => sdso ??= new();
      set => sdso = value;
    }

    /// <summary>
    /// A value of Fdso.
    /// </summary>
    [JsonPropertyName("fdso")]
    public Collection Fdso
    {
      get => fdso ??= new();
      set => fdso = value;
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
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
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
    /// A value of SelectCourtOrder.
    /// </summary>
    [JsonPropertyName("selectCourtOrder")]
    public LegalAction SelectCourtOrder
    {
      get => selectCourtOrder ??= new();
      set => selectCourtOrder = value;
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

    private LegalAction hiddenLegalAction;
    private Common multiCourtCase;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson arDisbSupp;
    private Common courtOrderPrompt;
    private FinanceWorkAttributes feesPaid;
    private FinanceWorkAttributes medPdCurrMo;
    private FinanceWorkAttributes medPdLastMo;
    private FinanceWorkAttributes feesPdCurrMo;
    private FinanceWorkAttributes feesPdLastMo;
    private FinanceWorkAttributes interestPdLastMo;
    private FinanceWorkAttributes interestPdCurrMo;
    private FinanceWorkAttributes arrsPdLastMo;
    private FinanceWorkAttributes arrsPdCurrMo;
    private ScreenOwedAmountsDtl amts;
    private Common amtLastPay;
    private FinanceWorkAttributes pdLastMo;
    private FinanceWorkAttributes currPdLastMo;
    private DateWorkArea lastPymnt;
    private Collection treasuryOffset;
    private Collection sdso;
    private Collection fdso;
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
    private Security2 security;
    private LegalAction flowFromLacs;
    private LegalAction selectCourtOrder;
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
    /// A value of FromComp.
    /// </summary>
    [JsonPropertyName("fromComp")]
    public CsePersonsWorkSet FromComp
    {
      get => fromComp ??= new();
      set => fromComp = value;
    }

    /// <summary>
    /// A value of ArDisbSupp.
    /// </summary>
    [JsonPropertyName("arDisbSupp")]
    public CsePerson ArDisbSupp
    {
      get => arDisbSupp ??= new();
      set => arDisbSupp = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of ImportSelCourtOrderNo.
    /// </summary>
    [JsonPropertyName("importSelCourtOrderNo")]
    public CashReceiptDetail ImportSelCourtOrderNo
    {
      get => importSelCourtOrderNo ??= new();
      set => importSelCourtOrderNo = value;
    }

    /// <summary>
    /// A value of CourtCaseClass.
    /// </summary>
    [JsonPropertyName("courtCaseClass")]
    public Standard CourtCaseClass
    {
      get => courtCaseClass ??= new();
      set => courtCaseClass = value;
    }

    /// <summary>
    /// A value of CourtCaseNumberOnly.
    /// </summary>
    [JsonPropertyName("courtCaseNumberOnly")]
    public WorkArea CourtCaseNumberOnly
    {
      get => courtCaseNumberOnly ??= new();
      set => courtCaseNumberOnly = value;
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
    /// A value of FeesPaid.
    /// </summary>
    [JsonPropertyName("feesPaid")]
    public FinanceWorkAttributes FeesPaid
    {
      get => feesPaid ??= new();
      set => feesPaid = value;
    }

    /// <summary>
    /// A value of MedPdCurrMo.
    /// </summary>
    [JsonPropertyName("medPdCurrMo")]
    public FinanceWorkAttributes MedPdCurrMo
    {
      get => medPdCurrMo ??= new();
      set => medPdCurrMo = value;
    }

    /// <summary>
    /// A value of MedPdLastMo.
    /// </summary>
    [JsonPropertyName("medPdLastMo")]
    public FinanceWorkAttributes MedPdLastMo
    {
      get => medPdLastMo ??= new();
      set => medPdLastMo = value;
    }

    /// <summary>
    /// A value of FeesPdCurrMo.
    /// </summary>
    [JsonPropertyName("feesPdCurrMo")]
    public FinanceWorkAttributes FeesPdCurrMo
    {
      get => feesPdCurrMo ??= new();
      set => feesPdCurrMo = value;
    }

    /// <summary>
    /// A value of FeesPdLastMo.
    /// </summary>
    [JsonPropertyName("feesPdLastMo")]
    public FinanceWorkAttributes FeesPdLastMo
    {
      get => feesPdLastMo ??= new();
      set => feesPdLastMo = value;
    }

    /// <summary>
    /// A value of InterestPdLastMo.
    /// </summary>
    [JsonPropertyName("interestPdLastMo")]
    public FinanceWorkAttributes InterestPdLastMo
    {
      get => interestPdLastMo ??= new();
      set => interestPdLastMo = value;
    }

    /// <summary>
    /// A value of InterestPdCurrMo.
    /// </summary>
    [JsonPropertyName("interestPdCurrMo")]
    public FinanceWorkAttributes InterestPdCurrMo
    {
      get => interestPdCurrMo ??= new();
      set => interestPdCurrMo = value;
    }

    /// <summary>
    /// A value of ArrsPdLastMo.
    /// </summary>
    [JsonPropertyName("arrsPdLastMo")]
    public FinanceWorkAttributes ArrsPdLastMo
    {
      get => arrsPdLastMo ??= new();
      set => arrsPdLastMo = value;
    }

    /// <summary>
    /// A value of ArrsPdCurrMo.
    /// </summary>
    [JsonPropertyName("arrsPdCurrMo")]
    public FinanceWorkAttributes ArrsPdCurrMo
    {
      get => arrsPdCurrMo ??= new();
      set => arrsPdCurrMo = value;
    }

    /// <summary>
    /// A value of TreasuryOffset.
    /// </summary>
    [JsonPropertyName("treasuryOffset")]
    public Collection TreasuryOffset
    {
      get => treasuryOffset ??= new();
      set => treasuryOffset = value;
    }

    /// <summary>
    /// A value of Sdso.
    /// </summary>
    [JsonPropertyName("sdso")]
    public Collection Sdso
    {
      get => sdso ??= new();
      set => sdso = value;
    }

    /// <summary>
    /// A value of Fdso.
    /// </summary>
    [JsonPropertyName("fdso")]
    public Collection Fdso
    {
      get => fdso ??= new();
      set => fdso = value;
    }

    /// <summary>
    /// A value of AmtLastPay.
    /// </summary>
    [JsonPropertyName("amtLastPay")]
    public Common AmtLastPay
    {
      get => amtLastPay ??= new();
      set => amtLastPay = value;
    }

    /// <summary>
    /// A value of PdLastMo.
    /// </summary>
    [JsonPropertyName("pdLastMo")]
    public FinanceWorkAttributes PdLastMo
    {
      get => pdLastMo ??= new();
      set => pdLastMo = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Cred.
    /// </summary>
    [JsonPropertyName("cred")]
    public Common Cred
    {
      get => cred ??= new();
      set => cred = value;
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
    /// A value of SelectCourtOrder.
    /// </summary>
    [JsonPropertyName("selectCourtOrder")]
    public LegalAction SelectCourtOrder
    {
      get => selectCourtOrder ??= new();
      set => selectCourtOrder = value;
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
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of LastPymnt.
    /// </summary>
    [JsonPropertyName("lastPymnt")]
    public DateWorkArea LastPymnt
    {
      get => lastPymnt ??= new();
      set => lastPymnt = value;
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

    private CsePersonsWorkSet fromComp;
    private CsePerson arDisbSupp;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private LegalAction filter;
    private CashReceiptDetail importSelCourtOrderNo;
    private Standard courtCaseClass;
    private WorkArea courtCaseNumberOnly;
    private LegalAction flowFromLacs;
    private FinanceWorkAttributes feesPaid;
    private FinanceWorkAttributes medPdCurrMo;
    private FinanceWorkAttributes medPdLastMo;
    private FinanceWorkAttributes feesPdCurrMo;
    private FinanceWorkAttributes feesPdLastMo;
    private FinanceWorkAttributes interestPdLastMo;
    private FinanceWorkAttributes interestPdCurrMo;
    private FinanceWorkAttributes arrsPdLastMo;
    private FinanceWorkAttributes arrsPdCurrMo;
    private Collection treasuryOffset;
    private Collection sdso;
    private Collection fdso;
    private Common amtLastPay;
    private FinanceWorkAttributes pdLastMo;
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
    private CsePersonsWorkSet ar;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private Office office;
    private ServiceProvider serviceProvider;
    private CodeValue caseCloseRsn;
    private Case1 case1;
    private Case1 next;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Common cred;
    private Common multiCourtCase;
    private LegalAction selectCourtOrder;
    private Common courtOrderPrompt;
    private ScreenOwedAmountsDtl amts;
    private ScreenOwedAmounts screenOwedAmounts;
    private LegalAction hiddenLegalAction;
    private FinanceWorkAttributes currPdLastMo;
    private DateWorkArea lastPymnt;
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
    /// A value of ApOpen.
    /// </summary>
    [JsonPropertyName("apOpen")]
    public Common ApOpen
    {
      get => apOpen ??= new();
      set => apOpen = value;
    }

    /// <summary>
    /// A value of ApCounter.
    /// </summary>
    [JsonPropertyName("apCounter")]
    public Common ApCounter
    {
      get => apCounter ??= new();
      set => apCounter = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of ArDisbSupp.
    /// </summary>
    [JsonPropertyName("arDisbSupp")]
    public CsePersonAccount ArDisbSupp
    {
      get => arDisbSupp ??= new();
      set => arDisbSupp = value;
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
    /// A value of OmitColDtls.
    /// </summary>
    [JsonPropertyName("omitColDtls")]
    public Common OmitColDtls
    {
      get => omitColDtls ??= new();
      set => omitColDtls = value;
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
    /// A value of LastMonth.
    /// </summary>
    [JsonPropertyName("lastMonth")]
    public DateWorkArea LastMonth
    {
      get => lastMonth ??= new();
      set => lastMonth = value;
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
    /// A value of FirstOfCurrent.
    /// </summary>
    [JsonPropertyName("firstOfCurrent")]
    public DateWorkArea FirstOfCurrent
    {
      get => firstOfCurrent ??= new();
      set => firstOfCurrent = value;
    }

    private Common apOpen;
    private Common apCounter;
    private Common caseOpen;
    private DateWorkArea collection;
    private CsePersonAccount arDisbSupp;
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
    private Common omitColDtls;
    private Common showInactive;
    private DateWorkArea lastMonth;
    private DateWorkArea nextMonth;
    private DateWorkArea firstOfCurrent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Collection1.
    /// </summary>
    [JsonPropertyName("collection1")]
    public LegalAction Collection1
    {
      get => collection1 ??= new();
      set => collection1 = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Collection2.
    /// </summary>
    [JsonPropertyName("collection2")]
    public Collection Collection2
    {
      get => collection2 ??= new();
      set => collection2 = value;
    }

    private LegalAction collection1;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private ObligationType obligationType;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private Code code;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private Tribunal tribunal;
    private AdministrativeActCertification administrativeActCertification;
    private CsePersonAccount csePersonAccount;
    private Collection collection2;
  }
#endregion
}

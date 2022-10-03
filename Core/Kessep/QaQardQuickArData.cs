// Program: QA_QARD_QUICK_AR_DATA, ID: 372230554, model: 746.
// Short name: SWEQARDP
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
/// A program: QA_QARD_QUICK_AR_DATA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQardQuickArData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QARD_QUICK_AR_DATA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQardQuickArData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQardQuickArData.
  /// </summary>
  public QaQardQuickArData(IContext context, Import import, Export export):
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
    // 12/03/98   JF Caillouet             Initial Development
    // --------------------------------------------------------
    // --------------------------------------------------------------------------------
    // PR# 85315         Vithal Madhira        01-21-2000    Added logic to 
    // check the 'EXP_GRP_HIDDEN' group view size so that the screen will not
    // abend.
    // ---------------------------------------------------------------------------------
    // 04/06/00 W.Campbell          Modified the view matching
    //                              for the call to SECURITY so that
    //                              it gets passed the next case number
    //                              instead of the case number.
    //                              Work done on WR# 00162
    //                              for PRWORA - Family Violence.
    // --------------------------------------------------------
    // -------------------------------------------------------------------
    // PR# 92293  04/24/2000       Vithal Madhira         The screen is supposed
    // to display the current referral status (like the screen LGRQ). Presently
    // the screen is reading legal_referral sorted by DESC 'status_date'. On
    // LGRQ  legal_referral  is read by DESC 'Identifier'. So to fix the problem
    // on QARD the Legal_Referral  will be READ sorted by DESC 'Identifier'.
    // ----------------------------------------------------------------------
    // -------------------------------------------------------------------
    // WR# 000254       11/20/2000           Vithal Madhira
    // New Flag is added to show if disbursements are suppressed  for AR.
    // ----------------------------------------------------------------------
    // 11/17/00 M.Lachowicz      WR 298. Create header
    //                           information for screens.
    // -----------------------------------------------
    // 03/28/01 M.Brown      PR 115907  Remove URA information
    // (First Benefit Date, and Grant Amount).
    // -----------------------------------------------
    // 03/28/01 M.Brown      PR 116895  Added view matching for the header on
    // transfers between QA screens.
    // -----------------------------------------------
    // 11/14/2001  V.Madhira   PR# 121249   Family Violence Fix.
    // -----------------------------------------------------------------
    // WR20202. 12-19-2001 by L. Bachura. This WR provides that the QARD screen 
    // display info if the case is closed. All info that displays when the case
    // is open will also display when the case is closed except case program.
    // Per Karen Buchelle, it is ok to not display the case program.The change
    // is effected by deleting the comparison of the role discontinue date to
    // the local current date in the display section of the code . The deleted
    // statement is "and desired xx discontinue date is > local current date
    // work area date." This check was to verify the disontinue date was 2099-12
    // -31.
    // PR139464. 2-26-2002. Add local flag for case open and changed AR read so 
    // that can get the current AR when more than one AR on a case. LBachura
    // PR139724. 3-26-02. Install change to show current non-coop status.
    // PR140346 Remove TAF dates 3-26-02. Lbachura
    // PR140816. 3-26-02 Install logic to not display AP information when the AP
    // is inactive on the case.
    // -------------------------------------------------------------------------------------
    // 05/07/2002      Vithal Madhira               PR# 144481
    // IV-A case number doesn't display on QARD.
    // On ARDS the 'AE Case Number' attribute from  Client (AR) will be 
    // displayed on the screen. On QARD this field is displayed from 'IM
    // Household'. SME wants the AE case number from Client to be displayed on
    // QARD.
    // Fixed the screen display. Now the IV-A case number value will be 
    // displayed from  Client (AR) instead of 'IM Household'.
    // 10/28/02 K.Doshi           Fix screen Help.
    // --------------------------------------------------------
    // PR158234. Install change to links to make the screens flow correctly for 
    // PF19 and pf20. 12-20-02. LJB
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Month = Month(local.Current.Date);
    local.Current.Year = Year(local.Current.Date);
    local.Current.YearMonth = local.Current.Year * 100 + local.Current.Month;
    UseCabSetMaximumDiscontinueDate();

    // ---  Move Section  --
    export.Case1.Assign(import.Case1);
    MoveCaseFuncWorkSet(import.CaseFuncWorkSet, export.CaseFuncWorkSet);
    export.ClsRsn.Description = import.ClsRsn.Description;
    export.Program.Code = import.Program.Code;
    MoveServiceProvider(import.ServiceProvider, export.ServiceProvider);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCsePersonAddress2(import.ArCsePersonAddress, export.ArCsePersonAddress);
    export.ApCsePersonsWorkSet.Assign(import.Ap);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.Hidden1.Assign(import.Hidden1);
    export.ApMultiCases.Flag = import.ApMultiCases.Flag;
    export.Next.Number = import.Next.Number;
    export.NonCoopRsn.Description = import.NonCoopRsn.Description;
    export.PaMedService.Description = import.PaMedService.Description;
    export.ProgCodeDescription.Description =
      import.ProgCodeDescription.Description;
    export.HiddenMultipleAps.Flag = import.HiddenMultipleAps.Flag;
    export.ApSelected.Assign(import.ApSelected);

    // 11/17/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/17/00 M.L End
    // ***  Move AR data    ***
    export.ArCsePerson.Assign(import.ArCsePerson);
    export.ArCsePersonsWorkSet.Assign(import.ArCsePersonsWorkSet);
    export.ArMultiCases.Flag = import.ArMultiCases.Flag;
    export.AltSsnAlias.Text30 = import.AltSsnAlias.Text30;
    MoveOffice(import.Office, export.Office);
    export.ArOtherCasePrompt.Flag = import.ArOtherCasePrompt.Flag;
    export.ApOtherCasePrompt.Flag = import.ApOtherCasePrompt.Flag;
    export.ComnLink.Assign(import.ComnLink);

    // ------------------------------------------------------------------------------------
    //        Per WR# 000254, the following code is added.
    //                                                                  
    // Vithal(11/20/2000)
    // ------------------------------------------------------------------------------------
    export.DisbSupp.Flag = import.DisbSupp.Flag;

    // ***  Move CHILD data    ***
    export.Ch.Assign(import.Ch);
    export.ChOtherCases.Flag = import.ChOtherCases.Flag;
    export.ChOtherCh.Flag = import.ChOtherCh.Flag;
    export.ChOtherChPrompt.SelectChar = import.ChOtherChPrompt.SelectChar;
    export.ChOtherCasesPrompt.SelectChar = import.ChOtherCasesPrompt.SelectChar;
    export.ChFlow.Number = import.ChFlow.Number;
    export.ChProcessed.Flag = import.ChProcessed.Flag;

    // ***  New QARD Moves  ***
    export.GoodCause.Code = import.GoodCause.Code;
    MoveNonCooperation(import.NonCooperation, export.NonCooperation);
    export.Employed.Flag = import.Employed.Flag;
    export.ImHousehold.AeCaseNo = import.ImHousehold.AeCaseNo;
    export.Cum.Grant = import.Cum.Grant;

    // ***  Move Page and Page Keys AND Legal Referral Group  ***
    export.CurrentPage.Count = import.CurrentPage.Count;
    export.MoreIndicator.ScrollingMessage =
      import.MoreIndicator.ScrollingMessage;
    export.ReferredTo.Text33 = import.ReferredTo.Text33;
    export.LegalRefStatusDescpt.Description =
      import.LegalRefStatusDescpt.Description;
    export.LegalReferral.Assign(import.LegalReferral);

    for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
      import.Hidden.Index)
    {
      if (!import.Hidden.CheckSize())
      {
        break;
      }

      export.Hidden.Index = import.Hidden.Index;
      export.Hidden.CheckSize();

      export.Hidden.Update.GexpLegalRefStatus.Description =
        import.Hidden.Item.GimpLegalRefStatus.Description;
      export.Hidden.Update.G.Assign(import.Hidden.Item.G);
      export.Hidden.Update.GexpReferredTo.Text33 =
        import.Hidden.Item.GimpReferredTo.Text33;
    }

    import.Hidden.CheckIndex();

    // ---  Next Tran and Security Logic  --
    if (!IsEmpty(export.Standard.NextTransaction))
    {
      export.Hidden1.CaseNumber = export.Case1.Number;
      export.Hidden1.CsePersonNumber = export.ApCsePersonsWorkSet.Number;
      export.Hidden1.CsePersonNumberAp = export.ApCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.ApCsePersonsWorkSet.Number = export.Hidden1.CsePersonNumberAp ?? Spaces
        (10);
      export.Next.Number = export.Hidden1.CaseNumber ?? Spaces(10);

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // --------------------------------------------------------
      // 04/06/00 W.Campbell -  Changed view
      // matching from case to next_case.
      // Work done on WR# 00162
      // for PRWORA - Family Violence.
      // --------------------------------------------------------
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // ***  Set Prompt Screen Characteristics  ***
    if (AsChar(export.ApMultiCases.Flag) == 'Y')
    {
      var field = GetField(export.ApOtherCasePrompt, "flag");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.ArMultiCases.Flag) == 'Y')
    {
      var field = GetField(export.ArOtherCasePrompt, "flag");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.MultipleAps.Flag) == 'Y')
    {
      var field = GetField(export.ApPrompt, "selectChar");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.ChOtherCases.Flag) == 'Y')
    {
      var field = GetField(export.ChOtherCasesPrompt, "selectChar");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.ChOtherCh.Flag) == 'Y')
    {
      var field = GetField(export.ChOtherChPrompt, "selectChar");

      field.Color = "green";
      field.Protected = false;
    }

    switch(TrimEnd(global.Command))
    {
      case "NEXT":
        export.Hidden.Index = export.CurrentPage.Count;
        export.Hidden.CheckSize();

        if (export.Hidden.Item.G.Identifier <= 0)
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        if (export.CurrentPage.Count >= Export.HiddenGroup.Capacity)
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        ++export.CurrentPage.Count;
        MoveLegalReferral(export.Hidden.Item.G, export.LegalReferral);
        export.LegalRefStatusDescpt.Description =
          export.Hidden.Item.GexpLegalRefStatus.Description;
        export.ReferredTo.Text33 = export.Hidden.Item.GexpReferredTo.Text33;

        if (export.CurrentPage.Count < export.Hidden.Count)
        {
          export.MoreIndicator.ScrollingMessage = "More - +";
        }
        else
        {
          export.MoreIndicator.ScrollingMessage = "More -";
        }

        break;
      case "PREV":
        if (export.CurrentPage.Count <= 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.CurrentPage.Count;

        export.Hidden.Index = export.CurrentPage.Count - 1;
        export.Hidden.CheckSize();

        MoveLegalReferral(export.Hidden.Item.G, export.LegalReferral);
        export.LegalRefStatusDescpt.Description =
          export.Hidden.Item.GexpLegalRefStatus.Description;
        export.ReferredTo.Text33 = export.Hidden.Item.GexpReferredTo.Text33;

        if (export.CurrentPage.Count > 1)
        {
          export.MoreIndicator.ScrollingMessage = "More - +";
        }
        else
        {
          export.MoreIndicator.ScrollingMessage = "More   +";
        }

        break;
      case "RETCOMN":
        if (AsChar(export.ArOtherCasePrompt.Flag) == 'S')
        {
          if (IsEmpty(export.Next.Number))
          {
            export.Next.Number = export.Case1.Number;
          }
          else
          {
            MoveCsePersonsWorkSet2(export.ComnLink, export.ArCsePersonsWorkSet);
          }
        }
        else if (AsChar(export.ChOtherCasesPrompt.SelectChar) == 'S')
        {
          if (IsEmpty(export.Next.Number))
          {
            export.Next.Number = export.Case1.Number;
          }
          else
          {
            export.Ch.Assign(export.ComnLink);
          }
        }
        else if (AsChar(export.ApOtherCasePrompt.Flag) == 'S')
        {
          if (IsEmpty(export.Next.Number))
          {
            export.Next.Number = export.Case1.Number;
          }
          else
          {
            export.Ch.Assign(export.ComnLink);
          }
        }

        global.Command = "DISPLAY";

        break;
      case "RETCOMP":
        if (IsEmpty(export.Next.Number))
        {
          export.Next.Number = export.Case1.Number;
        }

        if (AsChar(export.ChOtherChPrompt.SelectChar) == 'S')
        {
          UseSiReadCsePerson3();
        }
        else if (AsChar(export.ApPrompt.SelectChar) == 'S' || !
          Equal(export.ApCsePersonsWorkSet.Number, import.FromComp.Number))
        {
          MoveCsePersonsWorkSet6(import.FromComp, export.ApSelected);

          var field = GetField(export.ApPrompt, "selectChar");

          field.Color = "green";
          field.Protected = false;
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
            var field = GetField(export.ApPrompt, "selectChar");

            field.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ***   AP Other Cases   ***
        switch(AsChar(export.ApOtherCasePrompt.Flag))
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
            var field = GetField(export.ApOtherCasePrompt, "flag");

            field.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ***   AR Other Cases   ***
        switch(AsChar(export.ArOtherCasePrompt.Flag))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            MoveCsePersonsWorkSet2(export.ArCsePersonsWorkSet, export.ComnLink);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field = GetField(export.ArOtherCasePrompt, "flag");

            field.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ***   CH Other Cases   ***
        switch(AsChar(export.ChOtherChPrompt.SelectChar))
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
            var field = GetField(export.ChOtherChPrompt, "selectChar");

            field.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.ChOtherCasesPrompt.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.ComnLink.Assign(export.Ch);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field = GetField(export.ChOtherCasesPrompt, "selectChar");

            field.Error = true;

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
              var field = GetField(export.ApPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.ApOtherCasePrompt.Flag) == 'S')
            {
              var field = GetField(export.ApOtherCasePrompt, "flag");

              field.Error = true;
            }

            if (AsChar(export.ArOtherCasePrompt.Flag) == 'S')
            {
              var field = GetField(export.ArOtherCasePrompt, "flag");

              field.Error = true;
            }

            if (AsChar(export.ChOtherCasesPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ChOtherCasesPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.ChOtherChPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ChOtherChPrompt, "selectChar");

              field.Error = true;
            }

            break;
        }

        break;
      case "MARH":
        ExitState = "ECO_XFR_TO_MARH";

        break;
      case "LGRQ":
        ExitState = "ECO_XFR_MENU_TO_LGRQ";

        break;
      case "ALTS":
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        break;
      case "PEPR":
        ExitState = "ECO_LNK_TO_PEPR";

        break;
      case "PVSCR":
        ExitState = "ECO_XFR_TO_PREV";

        break;
      case "NXSCR":
        ExitState = "ECO_XFR_TO_NEXT_SCRN";

        break;
      case "ARDS":
        ExitState = "ECO_XFR_TO_ARDS";

        break;
      case "URAH":
        ExitState = "ECO_LNK_TO_URAH_HOUSEHOLD_URA";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.ApPrompt.SelectChar = "";
      export.ArOtherCasePrompt.Flag = "";
      export.ApOtherCasePrompt.Flag = "";
      export.ChOtherCasesPrompt.SelectChar = "";
      export.ChOtherChPrompt.SelectChar = "";

      if (IsEmpty(export.Next.Number) || !
        Equal(export.Next.Number, export.Case1.Number))
      {
        // *** Set Displayed fields to blank  ****
        export.Office.Name = "";
        export.Office.TypeCode = "";
        export.ServiceProvider.LastName = "";
        export.ServiceProvider.FirstName = "";
        export.Cum.Grant = 0;
        export.Employed.Flag = "";
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseFuncWorkSet.FuncDate = local.NullDateWorkArea.Date;
        export.CaseUnitFunctionAssignmt.Function = "";
        export.Case1.InterstateCaseId = "";
        export.Case1.DuplicateCaseIndicator = "";
        export.Case1.ExpeditedPaternityInd = "";
        export.DesigPayee.Flag = "";
        export.DisbSupp.Flag = "";
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseUnitFunctionAssignmt.Function = "";
        export.AltSsnAlias.Text30 = "";
        export.NonCoopRsn.Description = Spaces(CodeValue.Description_MaxLength);
        export.NonCooperation.Code = "";
        export.PaMedService.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.GoodCause.Code = "";
        export.PaMedService.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.ClsRsn.Description = Spaces(CodeValue.Description_MaxLength);
        MoveCase1(local.NullCase, export.Case1);
        export.ApCsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
        export.ArCsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
        export.Ch.Assign(local.NullCsePersonsWorkSet);
        MoveCsePerson(local.NullCsePerson, export.ArCsePerson);
        export.ArCsePersonAddress.Assign(local.NullCsePersonAddress);
        MoveCsePersonsWorkSet4(local.NullCsePersonsWorkSet, export.ApSelected);
        export.ApMultiCases.Flag = "";
        export.ArMultiCases.Flag = "";
        export.ChOtherCases.Flag = "";
        export.ChOtherCh.Flag = "";
        export.ChProcessed.Flag = "";

        // ***  Clear the Legal repeating grp  ***
        export.LegalReferral.ReferralReason1 = "";
        export.LegalReferral.ReferralReason2 = "";
        export.LegalReferral.ReferralReason3 = "";
        export.LegalReferral.ReferralReason4 = "";
        export.LegalReferral.ReferralReason5 = "";
        export.ImHousehold.AeCaseNo = "";
        export.Case1.AdcCloseDate = local.NullDateWorkArea.Date;
        export.Case1.AdcOpenDate = local.NullDateWorkArea.Date;
        export.LegalReferral.Status = "";
        export.ReferredTo.Text33 = "";
        export.LegalReferral.ReferredByUserId = "";
        export.LegalReferral.StatusDate = local.NullDateWorkArea.Date;
        export.LegalReferral.ReferralDate = local.NullDateWorkArea.Date;
        export.LegalRefStatusDescpt.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.MoreIndicator.ScrollingMessage = "";
      }

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
          export.Case1.Assign(entities.Case1);

          if (AsChar(export.Case1.Status) == 'O')
          {
            export.Case1.StatusDate = local.NullDateWorkArea.Date;
            local.CaseOpen.Flag = "Y";
          }

          // --------------------------------------------------------------------
          // Per PR# 110100 , the interstate ind. will be set using the same 
          // logic ( ie. SI_CADS_READ_CASE_DETAILS)  used in CADS  to set '
          // Interstate'  fields.
          // ---------------------------------------------------------------------
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
            export.ClsRsn.Description = entities.CodeValue.Description;
          }
          else
          {
            export.ClsRsn.Description = Spaces(CodeValue.Description_MaxLength);

            var field = GetField(export.ClsRsn, "description");

            field.Error = true;
          }
        }
        else
        {
          export.ClsRsn.Description = Spaces(CodeValue.Description_MaxLength);
        }

        if (!IsEmpty(export.Case1.PaMedicalService))
        {
          if (ReadCodeValue3())
          {
            export.PaMedService.Description = entities.CodeValue.Description;
          }
          else
          {
            export.PaMedService.Description =
              Spaces(CodeValue.Description_MaxLength);

            var field = GetField(export.PaMedService, "description");

            field.Error = true;
          }
        }
        else
        {
          export.PaMedService.Description =
            Spaces(CodeValue.Description_MaxLength);
        }

        // ***********************************************************
        // ***  AR DETAILS
        // 
        // ***
        // ***********************************************************
        // *** Find AR  ***
        // : M. Brown, March, 2001 - changed read to use current case.
        if (AsChar(local.CaseOpen.Flag) == 'Y')
        {
          if (ReadCsePerson3())
          {
            export.ArCsePersonsWorkSet.Number = entities.CsePerson.Number;
            export.ArCsePerson.AeCaseNumber = entities.CsePerson.AeCaseNumber;
          }
          else
          {
            ExitState = "CASE_ROLE_AR_NF";

            return;
          }
        }

        if (AsChar(local.CaseOpen.Flag) == 'N')
        {
          if (ReadCsePerson5())
          {
            export.ArCsePersonsWorkSet.Number = entities.CsePerson.Number;
            export.ArCsePerson.AeCaseNumber = entities.CsePerson.AeCaseNumber;
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

          var field = GetField(export.ArOtherCasePrompt, "flag");

          field.Color = "green";
          field.Protected = false;
        }
        else
        {
          export.ArMultiCases.Flag = "N";

          var field = GetField(export.ArOtherCasePrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;
        }

        // *** Get AR details  ***
        UseSiReadCsePerson2();

        if (!IsEmpty(local.AbendData.Type1) || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(local.AbendData.Type1))
          {
            ExitState = "CO0000_AR_NF";
          }

          var field = GetField(export.ArCsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        UseSiGetCsePersonMailingAddr();

        // ***  Read AR Address  ***
        export.Employed.Flag = "N";

        if (ReadIncomeSource())
        {
          export.Employed.Flag = "Y";
        }

        // *** Determine multiple AP's     ***
        export.HiddenMultipleAps.Flag = "";

        foreach(var item in ReadCsePerson6())
        {
          if (IsEmpty(export.HiddenMultipleAps.Flag))
          {
            export.HiddenMultipleAps.Flag = "N";
            export.ApCsePersonsWorkSet.Number = entities.CsePerson.Number;
          }
          else
          {
            export.HiddenMultipleAps.Flag = "Y";
            export.ApCsePersonsWorkSet.Number = "";

            break;
          }
        }

        if (IsEmpty(export.HiddenMultipleAps.Flag))
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
      if (AsChar(export.HiddenMultipleAps.Flag) == 'Y')
      {
        if (IsEmpty(export.ApSelected.Number) && IsEmpty
          (export.ApCsePersonsWorkSet.Number))
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        var field = GetField(export.ApPrompt, "selectChar");

        field.Color = "green";
        field.Protected = false;
      }

      if (!Equal(export.ApSelected.Number, export.ApCsePersonsWorkSet.Number))
      {
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.AltSsnAlias.Text30 = "";

        if (!IsEmpty(export.ApSelected.Number))
        {
          export.ApCsePersonsWorkSet.Number = export.ApSelected.Number;
        }

        // *** Get AP details  ***
        UseSiReadApCaseRoleDetails();

        // PR140816. 3-26-02 Install the following logic to not display AP 
        // information when the AP is inactive on the case.
        // Modified the logic for PR146446 on 8-13-02 LBachura
        local.ApOpen.Flag = "N";

        if (AsChar(export.HiddenMultipleAps.Flag) == 'Y')
        {
          if (ReadCsePerson1())
          {
            local.ApOpen.Flag = "Y";
          }
        }

        if (AsChar(export.HiddenMultipleAps.Flag) == 'N' && !
          Lt(Now().Date, export.ApCaseRole.EndDate))
        {
          if (ReadCsePerson1())
          {
            local.ApOpen.Flag = "Y";
          }
        }

        // End PR146446 logic. Lbachura
        if (!Lt(Now().Date, export.ApCaseRole.EndDate) && AsChar
          (local.ApOpen.Flag) == 'N')
        {
          export.ApCsePersonsWorkSet.FormattedName = "";
          export.ApCsePersonsWorkSet.Number = "";
          export.ApCsePersonsWorkSet.Ssn = "";
          export.ApMultiCases.Flag = "";

          var field = GetField(export.ApOtherCasePrompt, "flag");

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
            ExitState = "CO0000_AP_NF";
          }

          var field = GetField(export.ApCsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        // *** AP on more than one case?  ***
        if (AsChar(export.ApMultiCases.Flag) == 'Y')
        {
          var field = GetField(export.ApOtherCasePrompt, "flag");

          field.Color = "green";
          field.Protected = false;
        }
        else
        {
          var field = GetField(export.ApOtherCasePrompt, "flag");

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

      // --------------------------------------------------------------------
      // Per PR# 110100 , the interstate ind. will be set using the same logic (
      // ie. SI_CADS_READ_CASE_DETAILS)  used in CADS  to set ' Interstate'
      // fields. Check if 'Group_Local_Interstate'  is populated. If so, set the
      // indicator.
      // ---------------------------------------------------------------------
      export.Case1.InterstateCaseId = "N";

      if (!local.Interstate.IsEmpty)
      {
        export.Case1.InterstateCaseId = "Y";
      }

      if (!IsEmpty(export.Program.Code))
      {
        if (ReadCodeValue2())
        {
          export.ProgCodeDescription.Description =
            entities.CodeValue.Description;
        }
        else
        {
          export.ProgCodeDescription.Description =
            Spaces(CodeValue.Description_MaxLength);

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

      // **************************
      // ***   Child section   ****
      // **************************
      if (AsChar(export.ChProcessed.Flag) != 'Y')
      {
        UseSiRetrieveChildForCase();

        // ***  Read Child CSE Person Number  ***
        if (AsChar(export.ChOtherCh.Flag) == 'Y')
        {
          var field = GetField(export.ChOtherChPrompt, "selectChar");

          field.Color = "green";
          field.Protected = false;

          if (ReadCsePerson4())
          {
            local.Child.Number = entities.CsePerson.Number;

            goto Test;
          }

          if (ReadCsePerson2())
          {
            local.Child.Number = entities.CsePerson.Number;
          }
        }
        else
        {
          export.ChOtherCh.Flag = "N";

          var field = GetField(export.ChOtherChPrompt, "selectChar");

          field.Color = "cyan";
          field.Protected = true;

          local.Child.Number = export.Ch.Number;
        }

Test:

        UseSiReadCsePerson1();

        if (!IsEmpty(local.AbendData.Type1) || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(local.AbendData.Type1))
          {
            ExitState = "CO0000_AP_NF";
          }

          var field = GetField(export.Ch, "number");

          field.Error = true;

          return;
        }

        if (IsEmpty(export.Ch.Number))
        {
          ExitState = "CASE_ROLE_CHILD_NF";
        }

        // ***   Read for Other CH Cases   ***
        if (ReadCaseCaseRole())
        {
          export.ChOtherCases.Flag = "Y";

          var field = GetField(export.ChOtherCasesPrompt, "selectChar");

          field.Color = "green";
          field.Protected = false;
        }
        else
        {
          export.ChOtherCases.Flag = "N";

          var field = GetField(export.ChOtherCasesPrompt, "selectChar");

          field.Color = "cyan";
          field.Protected = true;
        }

        export.ChProcessed.Flag = "Y";
      }

      // **********************
      // *** AR Stuff Again ***
      // **********************
      // ***  Read AR Address  ***
      // ***  Required when transferring from QINC or QCHD ****
      UseSiReadCsePerson2();
      UseSiGetCsePersonMailingAddr();

      if (ReadGoodCause())
      {
        export.GoodCause.Code = entities.GoodCause.Code;
      }

      // PR 139724. March 25, 2002. LBachura. Changed discontinue date in the 
      // read for non-cooperation to create date, because CADS does not load the
      // discontinue date. Frther from testing, it appears that each non
      // cooperation is mutually exclusive and non-cooperation status's don't
      // overlap
      if (ReadNonCooperation())
      {
        export.NonCooperation.Code = entities.NonCooperation.Code;
        export.NonCooperation.Reason = entities.NonCooperation.Reason;
      }

      if (!IsEmpty(export.NonCooperation.Reason))
      {
        if (ReadCodeValue4())
        {
          export.NonCoopRsn.Description = entities.CodeValue.Description;
        }
        else
        {
          export.NonCoopRsn.Description =
            Spaces(CodeValue.Description_MaxLength);

          var field = GetField(export.NonCoopRsn, "description");

          field.Error = true;
        }
      }
      else
      {
        export.NonCoopRsn.Description = Spaces(CodeValue.Description_MaxLength);
      }

      // ***  Legal Referral Scroll Group  ***
      export.Hidden.Index = -1;

      // -------------------------------------------------------------------
      // PR# 92293  04/24/2000       Vithal Madhira         The screen is 
      // supposed to display the current referral status (like the screen LGRQ).
      // Presently the screen is reading legal_referral sorted by DESC '
      // status_date'. On LGRQ  legal_referral  is read by DESC 'Identifier'. So
      // to fix the problem on QARD the Legal_Referral  will be READ sorted by
      // DESC 'Identifier'.
      // ----------------------------------------------------------------------
      // start PR160748 and wr20331
      foreach(var item in ReadLegalReferral())
      {
        ++export.Hidden.Index;
        export.Hidden.CheckSize();

        // --------------------------------------------------------------------------------
        // PR# 85315         Vithal Madhira        01-21-2000    Added logic to 
        // check the 'EXP_GRP_HIDDEN' group view size so that the screen will
        // not abend.
        // ---------------------------------------------------------------------------------
        if (export.Hidden.Count >= Export.HiddenGroup.Capacity)
        {
          break;
        }

        // ***  Read Legal Referral SRVC PRVDR  ***
        if (ReadServiceProvider())
        {
          export.Hidden.Update.GexpReferredTo.Text33 =
            TrimEnd(entities.ServiceProvider.LastName) + ", " + TrimEnd
            (entities.ServiceProvider.FirstName) + " " + entities
            .ServiceProvider.MiddleInitial;
        }

        if (!IsEmpty(entities.LegalReferral.Status))
        {
          if (ReadCodeValue5())
          {
            export.Hidden.Update.GexpLegalRefStatus.Description =
              entities.CodeValue.Description;
          }
          else
          {
            export.Hidden.Update.GexpLegalRefStatus.Description =
              Spaces(CodeValue.Description_MaxLength);
          }
        }
        else
        {
          export.Hidden.Update.GexpLegalRefStatus.Description =
            Spaces(CodeValue.Description_MaxLength);
        }

        // **  MOVE TO G HIDDEN   **
        export.Hidden.Update.G.Assign(entities.LegalReferral);
      }

      // The statmetn right beelow this will need to be changed back to active 
      // or deleted if the code within these notes works for the is pr and wr.
      // LJB PR160748 and wr20331
      export.CurrentPage.Count = 1;

      export.Hidden.Index = export.CurrentPage.Count - 1;
      export.Hidden.CheckSize();

      export.LegalRefStatusDescpt.Description =
        export.Hidden.Item.GexpLegalRefStatus.Description;
      MoveLegalReferral(export.Hidden.Item.G, export.LegalReferral);
      export.ReferredTo.Text33 = export.Hidden.Item.GexpReferredTo.Text33;

      // *** Only should happen when nothing found  ***
      if (export.Hidden.Index == -1)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }

      if (export.Hidden.Index < 1)
      {
        export.MoreIndicator.ScrollingMessage = "More";
      }
      else
      {
        export.MoreIndicator.ScrollingMessage = "More   +";
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
    }

    // -----------------------------------------------------------------------
    // 11/14/2001  V.Madhira   PR# 121249   Family Violence Fix.
    // ------------------------------------------------------------------------
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -----------------------------------------------------------------------
      // Call the SECURITY CAB to check for Family_Violence.
      // Since this is a person (AR) driven screen, pass the Cse_Person number 
      // only to the CAB.
      // ------------------------------------------------------------------------
      UseScSecurityCheckForFv();

      if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
      {
        export.ArCsePersonAddress.Assign(local.NullCsePersonAddress);
        export.ArCsePerson.HomePhone = 0;
        export.ArCsePerson.HomePhoneAreaCode = 0;
        export.ArCsePerson.WorkPhone = 0;
        export.ArCsePerson.WorkPhoneAreaCode = 0;
        export.ArCsePerson.WorkPhoneExt = "";
        export.Employed.Flag = "";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("IM_HOUSEHOLD_NF"))
    {
      ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
    }

    if (AsChar(export.Case1.Status) == 'C')
    {
      ExitState = "ACO_NI0000_DISPLAY_CLOSED_CASE";
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
    target.PaMedicalService = source.PaMedicalService;
    target.InterstateCaseId = source.InterstateCaseId;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
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
    target.Type1 = source.Type1;
    target.EndDate = source.EndDate;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.AeCaseNumber = source.AeCaseNumber;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
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
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet5(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet6(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet7(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInterstate(SiCadsReadCaseDetails.Export.
    InterstateGroup source, Local.InterstateGroup target)
  {
    target.Interstate1.Text4 = source.Interstate1.Text4;
  }

  private static void MoveLegalReferral(LegalReferral source,
    LegalReferral target)
  {
    target.ReferredByUserId = source.ReferredByUserId;
    target.ReferralDate = source.ReferralDate;
    target.StatusDate = source.StatusDate;
    target.ReferralReason1 = source.ReferralReason1;
    target.ReferralReason2 = source.ReferralReason2;
    target.ReferralReason3 = source.ReferralReason3;
    target.ReferralReason5 = source.ReferralReason5;
    target.ReferralReason4 = source.ReferralReason4;
    target.Status = source.Status;
  }

  private static void MoveNonCooperation(NonCooperation source,
    NonCooperation target)
  {
    target.Code = source.Code;
    target.Reason = source.Reason;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
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

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden1.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden1);

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

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
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

    useExport.Interstate.CopyTo(local.Interstate, MoveInterstate);
    export.Program.Code = useExport.Program.Code;
    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, export.CaseFuncWorkSet);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = export.ArCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, export.ArCsePersonAddress);
      
  }

  private void UseSiReadApCaseRoleDetails()
  {
    var useImport = new SiReadApCaseRoleDetails.Import();
    var useExport = new SiReadApCaseRoleDetails.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiReadApCaseRoleDetails.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet7(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
    export.ApMultiCases.Flag = useExport.MultipleCases.Flag;
    MoveCaseRole(useExport.ApCaseRole, export.ApCaseRole);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Child.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet5(useExport.CsePersonsWorkSet, export.Ch);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePerson(useExport.CsePerson, export.ArCsePerson);
    MoveCsePersonsWorkSet3(useExport.CsePersonsWorkSet,
      export.ArCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.ChFlow.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet5(useExport.CsePersonsWorkSet, export.Ch);
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

  private void UseSiRetrieveChildForCase()
  {
    var useImport = new SiRetrieveChildForCase.Import();
    var useExport = new SiRetrieveChildForCase.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiRetrieveChildForCase.Execute, useImport, useExport);

    export.ChOtherCh.Flag = useExport.MultipleChildren.Flag;
    export.Ch.Number = useExport.Child.Number;
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
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 5);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 6);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 7);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 8);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 9);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 10);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 11);
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
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 5);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 6);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 7);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 8);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 9);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 10);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 11);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ch.Number);
        db.SetString(command, "numb", export.Next.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 5);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 6);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 7);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 8);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 9);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 10);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 11);
        entities.CaseRole.CspNumber = db.GetString(reader, 12);
        entities.CaseRole.Type1 = db.GetString(reader, 13);
        entities.CaseRole.Identifier = db.GetInt32(reader, 14);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 15);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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

  private bool ReadCodeValue3()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "paMedicalService", export.Case1.PaMedicalService ?? "");
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

  private bool ReadCodeValue4()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "reason", export.NonCooperation.Reason ?? "");
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

  private bool ReadCodeValue5()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "status", entities.LegalReferral.Status ?? "");
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
          command, "endDate", local.Current.Date.GetValueOrDefault());
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
        db.SetString(command, "casNumber", entities.Case1.Number);
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

  private bool ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson4",
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

  private bool ReadCsePerson5()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
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

  private IEnumerable<bool> ReadCsePerson6()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson6",
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

  private bool ReadGoodCause()
  {
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "cspINumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 3);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 4);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 10);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private bool ReadNonCooperation()
  {
    entities.NonCooperation.Populated = false;

    return Read("ReadNonCooperation",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.NonCooperation.Code = db.GetNullableString(reader, 0);
        entities.NonCooperation.Reason = db.GetNullableString(reader, 1);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 2);
        entities.NonCooperation.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.NonCooperation.CasNumber = db.GetString(reader, 5);
        entities.NonCooperation.CspNumber = db.GetString(reader, 6);
        entities.NonCooperation.CroType = db.GetString(reader, 7);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 8);
        entities.NonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of GimpLegalRefStatus.
      /// </summary>
      [JsonPropertyName("gimpLegalRefStatus")]
      public CodeValue GimpLegalRefStatus
      {
        get => gimpLegalRefStatus ??= new();
        set => gimpLegalRefStatus = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public LegalReferral G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GimpReferredTo.
      /// </summary>
      [JsonPropertyName("gimpReferredTo")]
      public WorkArea GimpReferredTo
      {
        get => gimpReferredTo ??= new();
        set => gimpReferredTo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private CodeValue gimpLegalRefStatus;
      private LegalReferral g;
      private WorkArea gimpReferredTo;
    }

    /// <summary>
    /// A value of Cum.
    /// </summary>
    [JsonPropertyName("cum")]
    public OeUraSummary Cum
    {
      get => cum ??= new();
      set => cum = value;
    }

    /// <summary>
    /// A value of ChProcessed.
    /// </summary>
    [JsonPropertyName("chProcessed")]
    public Common ChProcessed
    {
      get => chProcessed ??= new();
      set => chProcessed = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public CsePersonsWorkSet ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
    }

    /// <summary>
    /// A value of ChFlow.
    /// </summary>
    [JsonPropertyName("chFlow")]
    public CsePersonsWorkSet ChFlow
    {
      get => chFlow ??= new();
      set => chFlow = value;
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
    /// A value of ReferredTo.
    /// </summary>
    [JsonPropertyName("referredTo")]
    public WorkArea ReferredTo
    {
      get => referredTo ??= new();
      set => referredTo = value;
    }

    /// <summary>
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
    }

    /// <summary>
    /// A value of MoreIndicator.
    /// </summary>
    [JsonPropertyName("moreIndicator")]
    public Standard MoreIndicator
    {
      get => moreIndicator ??= new();
      set => moreIndicator = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of PaMedService.
    /// </summary>
    [JsonPropertyName("paMedService")]
    public CodeValue PaMedService
    {
      get => paMedService ??= new();
      set => paMedService = value;
    }

    /// <summary>
    /// A value of NonCoopRsn.
    /// </summary>
    [JsonPropertyName("nonCoopRsn")]
    public CodeValue NonCoopRsn
    {
      get => nonCoopRsn ??= new();
      set => nonCoopRsn = value;
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
    /// A value of Employed.
    /// </summary>
    [JsonPropertyName("employed")]
    public Common Employed
    {
      get => employed ??= new();
      set => employed = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public Common DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of LegalRefStatusDescpt.
    /// </summary>
    [JsonPropertyName("legalRefStatusDescpt")]
    public CodeValue LegalRefStatusDescpt
    {
      get => legalRefStatusDescpt ??= new();
      set => legalRefStatusDescpt = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of ChOtherChPrompt.
    /// </summary>
    [JsonPropertyName("chOtherChPrompt")]
    public Common ChOtherChPrompt
    {
      get => chOtherChPrompt ??= new();
      set => chOtherChPrompt = value;
    }

    /// <summary>
    /// A value of ChOtherCh.
    /// </summary>
    [JsonPropertyName("chOtherCh")]
    public Common ChOtherCh
    {
      get => chOtherCh ??= new();
      set => chOtherCh = value;
    }

    /// <summary>
    /// A value of ChOtherCasesPrompt.
    /// </summary>
    [JsonPropertyName("chOtherCasesPrompt")]
    public Common ChOtherCasesPrompt
    {
      get => chOtherCasesPrompt ??= new();
      set => chOtherCasesPrompt = value;
    }

    /// <summary>
    /// A value of ChOtherCases.
    /// </summary>
    [JsonPropertyName("chOtherCases")]
    public Common ChOtherCases
    {
      get => chOtherCases ??= new();
      set => chOtherCases = value;
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
    /// A value of ArOtherCasePrompt.
    /// </summary>
    [JsonPropertyName("arOtherCasePrompt")]
    public Common ArOtherCasePrompt
    {
      get => arOtherCasePrompt ??= new();
      set => arOtherCasePrompt = value;
    }

    /// <summary>
    /// A value of ApOtherCasePrompt.
    /// </summary>
    [JsonPropertyName("apOtherCasePrompt")]
    public Common ApOtherCasePrompt
    {
      get => apOtherCasePrompt ??= new();
      set => apOtherCasePrompt = value;
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
    /// A value of HiddenMultipleAps.
    /// </summary>
    [JsonPropertyName("hiddenMultipleAps")]
    public Common HiddenMultipleAps
    {
      get => hiddenMultipleAps ??= new();
      set => hiddenMultipleAps = value;
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
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
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
    /// A value of Hidden1.
    /// </summary>
    [JsonPropertyName("hidden1")]
    public NextTranInfo Hidden1
    {
      get => hidden1 ??= new();
      set => hidden1 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of ClsRsn.
    /// </summary>
    [JsonPropertyName("clsRsn")]
    public CodeValue ClsRsn
    {
      get => clsRsn ??= new();
      set => clsRsn = value;
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

    private OeUraSummary cum;
    private Common chProcessed;
    private CsePersonsWorkSet apSelected;
    private CsePersonsWorkSet chFlow;
    private ImHousehold imHousehold;
    private WorkArea referredTo;
    private Common currentPage;
    private Standard moreIndicator;
    private Array<HiddenGroup> hidden;
    private CodeValue paMedService;
    private CodeValue nonCoopRsn;
    private CsePersonsWorkSet fromComp;
    private Common employed;
    private NonCooperation nonCooperation;
    private GoodCause goodCause;
    private Common desigPayee;
    private CodeValue legalRefStatusDescpt;
    private LegalReferral legalReferral;
    private CsePerson arCsePerson;
    private Common chOtherChPrompt;
    private Common chOtherCh;
    private Common chOtherCasesPrompt;
    private Common chOtherCases;
    private CsePersonsWorkSet ch;
    private CsePersonsWorkSet comnLink;
    private CodeValue progCodeDescription;
    private Common arOtherCasePrompt;
    private Common apOtherCasePrompt;
    private Common arMultiCases;
    private Office office;
    private Common apOtherCases;
    private TextWorkArea altSsnAlias;
    private Common hiddenMultipleAps;
    private Common apMultiCases;
    private CsePersonAddress arCsePersonAddress;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apPrompt;
    private NextTranInfo hidden1;
    private Standard standard;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private ServiceProvider serviceProvider;
    private CodeValue clsRsn;
    private Case1 case1;
    private Case1 next;
    private Common disbSupp;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of GexpLegalRefStatus.
      /// </summary>
      [JsonPropertyName("gexpLegalRefStatus")]
      public CodeValue GexpLegalRefStatus
      {
        get => gexpLegalRefStatus ??= new();
        set => gexpLegalRefStatus = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public LegalReferral G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexpReferredTo.
      /// </summary>
      [JsonPropertyName("gexpReferredTo")]
      public WorkArea GexpReferredTo
      {
        get => gexpReferredTo ??= new();
        set => gexpReferredTo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private CodeValue gexpLegalRefStatus;
      private LegalReferral g;
      private WorkArea gexpReferredTo;
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
    /// A value of Cum.
    /// </summary>
    [JsonPropertyName("cum")]
    public OeUraSummary Cum
    {
      get => cum ??= new();
      set => cum = value;
    }

    /// <summary>
    /// A value of ChProcessed.
    /// </summary>
    [JsonPropertyName("chProcessed")]
    public Common ChProcessed
    {
      get => chProcessed ??= new();
      set => chProcessed = value;
    }

    /// <summary>
    /// A value of ChFlow.
    /// </summary>
    [JsonPropertyName("chFlow")]
    public CsePersonsWorkSet ChFlow
    {
      get => chFlow ??= new();
      set => chFlow = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public CsePersonsWorkSet ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
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
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
    }

    /// <summary>
    /// A value of MoreIndicator.
    /// </summary>
    [JsonPropertyName("moreIndicator")]
    public Standard MoreIndicator
    {
      get => moreIndicator ??= new();
      set => moreIndicator = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of PaMedService.
    /// </summary>
    [JsonPropertyName("paMedService")]
    public CodeValue PaMedService
    {
      get => paMedService ??= new();
      set => paMedService = value;
    }

    /// <summary>
    /// A value of NonCoopRsn.
    /// </summary>
    [JsonPropertyName("nonCoopRsn")]
    public CodeValue NonCoopRsn
    {
      get => nonCoopRsn ??= new();
      set => nonCoopRsn = value;
    }

    /// <summary>
    /// A value of Employed.
    /// </summary>
    [JsonPropertyName("employed")]
    public Common Employed
    {
      get => employed ??= new();
      set => employed = value;
    }

    /// <summary>
    /// A value of ReferredTo.
    /// </summary>
    [JsonPropertyName("referredTo")]
    public WorkArea ReferredTo
    {
      get => referredTo ??= new();
      set => referredTo = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public Common DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of LegalRefStatusDescpt.
    /// </summary>
    [JsonPropertyName("legalRefStatusDescpt")]
    public CodeValue LegalRefStatusDescpt
    {
      get => legalRefStatusDescpt ??= new();
      set => legalRefStatusDescpt = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of ChOtherChPrompt.
    /// </summary>
    [JsonPropertyName("chOtherChPrompt")]
    public Common ChOtherChPrompt
    {
      get => chOtherChPrompt ??= new();
      set => chOtherChPrompt = value;
    }

    /// <summary>
    /// A value of ChOtherCh.
    /// </summary>
    [JsonPropertyName("chOtherCh")]
    public Common ChOtherCh
    {
      get => chOtherCh ??= new();
      set => chOtherCh = value;
    }

    /// <summary>
    /// A value of ChOtherCasesPrompt.
    /// </summary>
    [JsonPropertyName("chOtherCasesPrompt")]
    public Common ChOtherCasesPrompt
    {
      get => chOtherCasesPrompt ??= new();
      set => chOtherCasesPrompt = value;
    }

    /// <summary>
    /// A value of ChOtherCases.
    /// </summary>
    [JsonPropertyName("chOtherCases")]
    public Common ChOtherCases
    {
      get => chOtherCases ??= new();
      set => chOtherCases = value;
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
    /// A value of ArOtherCasePrompt.
    /// </summary>
    [JsonPropertyName("arOtherCasePrompt")]
    public Common ArOtherCasePrompt
    {
      get => arOtherCasePrompt ??= new();
      set => arOtherCasePrompt = value;
    }

    /// <summary>
    /// A value of ApOtherCasePrompt.
    /// </summary>
    [JsonPropertyName("apOtherCasePrompt")]
    public Common ApOtherCasePrompt
    {
      get => apOtherCasePrompt ??= new();
      set => apOtherCasePrompt = value;
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
    /// A value of AltSsnAlias.
    /// </summary>
    [JsonPropertyName("altSsnAlias")]
    public TextWorkArea AltSsnAlias
    {
      get => altSsnAlias ??= new();
      set => altSsnAlias = value;
    }

    /// <summary>
    /// A value of HiddenMultipleAps.
    /// </summary>
    [JsonPropertyName("hiddenMultipleAps")]
    public Common HiddenMultipleAps
    {
      get => hiddenMultipleAps ??= new();
      set => hiddenMultipleAps = value;
    }

    /// <summary>
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
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
    /// A value of ClsRsn.
    /// </summary>
    [JsonPropertyName("clsRsn")]
    public CodeValue ClsRsn
    {
      get => clsRsn ??= new();
      set => clsRsn = value;
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
    /// A value of Hidden1.
    /// </summary>
    [JsonPropertyName("hidden1")]
    public NextTranInfo Hidden1
    {
      get => hidden1 ??= new();
      set => hidden1 = value;
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
    /// A value of ApMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("apMultiCasesPrompt")]
    public Common ApMultiCasesPrompt
    {
      get => apMultiCasesPrompt ??= new();
      set => apMultiCasesPrompt = value;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
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

    private CaseRole apCaseRole;
    private OeUraSummary cum;
    private Common chProcessed;
    private CsePersonsWorkSet chFlow;
    private CsePersonsWorkSet apSelected;
    private ImHousehold imHousehold;
    private Common currentPage;
    private Standard moreIndicator;
    private Array<HiddenGroup> hidden;
    private CodeValue paMedService;
    private CodeValue nonCoopRsn;
    private Common employed;
    private WorkArea referredTo;
    private NonCooperation nonCooperation;
    private GoodCause goodCause;
    private Common desigPayee;
    private CodeValue legalRefStatusDescpt;
    private LegalReferral legalReferral;
    private CsePerson arCsePerson;
    private Common chOtherChPrompt;
    private Common chOtherCh;
    private Common chOtherCasesPrompt;
    private Common chOtherCases;
    private CsePersonsWorkSet ch;
    private CsePersonsWorkSet comnLink;
    private CodeValue progCodeDescription;
    private Common arOtherCasePrompt;
    private Common apOtherCasePrompt;
    private Common arMultiCases;
    private TextWorkArea altSsnAlias;
    private Common hiddenMultipleAps;
    private CsePersonAddress arCsePersonAddress;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apPrompt;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private Office office;
    private ServiceProvider serviceProvider;
    private CodeValue clsRsn;
    private Case1 case1;
    private Case1 next;
    private Standard standard;
    private NextTranInfo hidden1;
    private Common apMultiCases;
    private Common apMultiCasesPrompt;
    private Common arMultiCasesPrompt;
    private Common multipleAps;
    private Common disbSupp;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A InterstateGroup group.</summary>
    [Serializable]
    public class InterstateGroup
    {
      /// <summary>
      /// A value of Interstate1.
      /// </summary>
      [JsonPropertyName("interstate1")]
      public TextWorkArea Interstate1
      {
        get => interstate1 ??= new();
        set => interstate1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private TextWorkArea interstate1;
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
    /// A value of ApInactive.
    /// </summary>
    [JsonPropertyName("apInactive")]
    public Common ApInactive
    {
      get => apInactive ??= new();
      set => apInactive = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of NullCsePersonAddress.
    /// </summary>
    [JsonPropertyName("nullCsePersonAddress")]
    public CsePersonAddress NullCsePersonAddress
    {
      get => nullCsePersonAddress ??= new();
      set => nullCsePersonAddress = value;
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
    /// A value of NullCase.
    /// </summary>
    [JsonPropertyName("nullCase")]
    public Case1 NullCase
    {
      get => nullCase ??= new();
      set => nullCase = value;
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
    /// Gets a value of Interstate.
    /// </summary>
    [JsonIgnore]
    public Array<InterstateGroup> Interstate => interstate ??= new(
      InterstateGroup.Capacity);

    /// <summary>
    /// Gets a value of Interstate for json serialization.
    /// </summary>
    [JsonPropertyName("interstate")]
    [Computed]
    public IList<InterstateGroup> Interstate_Json
    {
      get => interstate;
      set => Interstate.Assign(value);
    }

    private Common apOpen;
    private Common apInactive;
    private Common caseOpen;
    private CsePersonsWorkSet child;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea max;
    private Common invalid;
    private AbendData abendData;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private CsePersonAddress nullCsePersonAddress;
    private CsePerson nullCsePerson;
    private Case1 nullCase;
    private DateWorkArea current;
    private Common apAltOccur;
    private Common arAltOccur;
    private Array<InterstateGroup> interstate;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private InterstateCase interstateCase;
    private IncomeSource incomeSource;
    private Employer employer;
    private CsePersonAccount csePersonAccount;
    private ImHousehold imHousehold;
    private LegalReferralAssignment legalReferralAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private LegalReferral legalReferral;
    private NonCooperation nonCooperation;
    private GoodCause goodCause;
    private CsePersonAddress csePersonAddress;
    private Code code;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
  }
#endregion
}

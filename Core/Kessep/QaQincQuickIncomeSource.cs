// Program: QA_QINC_QUICK_INCOME_SOURCE, ID: 372262065, model: 746.
// Short name: SWEQINCP
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
/// A program: QA_QINC_QUICK_INCOME_SOURCE.
/// </para>
/// <para>
/// RESP:   Quality Assurance.
/// QA Quick Reference AP Data Screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQincQuickIncomeSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QINC_QUICK_INCOME_SOURCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQincQuickIncomeSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQincQuickIncomeSource.
  /// </summary>
  public QaQincQuickIncomeSource(IContext context, Import import, Export export):
    
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
    // Date                     Developer                Description
    // 12/25/98             JF Caillouet             Initial Development
    // 02/15/2000         Vithal Madhira            PR# 88212
    // 04/05/00 W.Campbell          Moved an ESCAPE
    //                              stmt and added a COMMAND
    //                              is DISPLAY stmt.  Also added a
    //                              set stmt and modified the view
    //                              matching for the call to the
    //                              SECURITY cab so that it gets
    //                              passed the case number
    //                              and AP cse person number
    //                              correctly on entering this
    //                              Pstep via NEXTTRAN.
    //                              Work done on WR# 00162
    //                              for PRWORA - Family Violence.
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // 11/14/2001  V.Madhira   PR# 121249   Family Violence Fix.
    // WR20202 12-19-2001 by L. Bachura. This WR provides that the QINC screen 
    // displays information when the case is closed. It should display the same
    // information that is displayed when the case is open except the case
    // program stuff. Per Karen Buchelle, it is not necessary to display case
    // program for a closed case.  The change is effected by deleting the logic
    // that compares the case role discontinue date to the local current date in
    // hhe display case section of the code. The deleted statment is  "and
    // desired case role end date is > the local current work area date". The
    // deleted check was to verify that the discontinue date was 2099-12-31.
    // PR139464. 2-26-02. Add logic so that AR with open case role is the ar 
    // seleceted when more than one AR on a case.
    // 11/2002         SWSRKXD
    // Fix screen help Id.
    // --------------------------------------------------------------
    // PR158234. Change links to make screen flow. 12-20-02. LJB
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
    UseCabSetMaximumDiscontinueDate();

    // ---  Move Section  --
    MoveCase2(import.Case1, export.Case1);
    MoveCaseFuncWorkSet(import.CaseFuncWorkSet, export.CaseFuncWorkSet);
    MoveCodeValue(import.CaseCloseRsn, export.CaseCloseRsn);
    export.Program.Code = import.Program.Code;
    MoveServiceProvider(import.ServiceProvider, export.ServiceProvider);
    export.Standard.Assign(import.Standard);
    MoveCaseRole(import.ApCaseRole, export.ApCaseRole);
    export.ApClient.Assign(import.ApClient);
    export.ApCsePersonAddress.Assign(import.ApCsePersonAddress);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.Ar.Assign(import.ArCsePersonsWorkSet);
    export.Bankruptcy.Flag = import.Bankruptcy.Flag;
    export.Hidden.Assign(import.Hidden);
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
    local.ArAltOccur.Flag = import.ArAltOccur.Flag;
    export.ArMultiCasesPrompt.Flag = import.ArMultiCasesPrompt.Flag;
    export.ComnLink.Assign(import.ComnLink);
    export.IncomeSourceIndicator.Type1 = import.IncomeSourceIndicator.Type1;
    export.IncomeSrcCodeValue.Cdvalue = import.IncomeSrcCodeValue.Cdvalue;
    export.IncomeSourceTypePrompt.SelectChar =
      import.IncomeSourceTypePrompt.SelectChar;

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    export.IncomeSource.Index = 0;
    export.IncomeSource.Clear();

    for(import.IncomeSource.Index = 0; import.IncomeSource.Index < import
      .IncomeSource.Count; ++import.IncomeSource.Index)
    {
      if (export.IncomeSource.IsFull)
      {
        break;
      }

      MoveIncomeSource2(import.IncomeSource.Item.GincomeSource,
        export.IncomeSource.Update.GincomeSource);
      MoveEmployer1(import.IncomeSource.Item.Gemployer,
        export.IncomeSource.Update.Gemployer);
      MovePersonIncomeHistory(import.IncomeSource.Item.GpersonIncomeHistory,
        export.IncomeSource.Update.GpersonIncomeHistory);
      export.IncomeSource.Update.GnonEmployIncomeSourceAddress.Assign(
        import.IncomeSource.Item.GnonEmployIncomeSourceAddress);
      MoveIncomeSourceContact(import.IncomeSource.Item.GincomeSourceContact,
        export.IncomeSource.Update.GincomeSourceContact);
      export.IncomeSource.Update.GexpReturn.Description =
        import.IncomeSource.Item.GimpReturn.Description;
      export.G.ResourceNo =
        import.IncomeSource.Item.GcsePersonResource.ResourceNo;
      export.IncomeSource.Update.GexpOtherIncomeSrc.Description =
        import.IncomeSource.Item.GimpOtherIncomeSrc.Description;
      export.IncomeSource.Next();
    }

    for(import.PageKeys.Index = 0; import.PageKeys.Index < import
      .PageKeys.Count; ++import.PageKeys.Index)
    {
      if (!import.PageKeys.CheckSize())
      {
        break;
      }

      export.PageKeys.Index = import.PageKeys.Index;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.PageKey.Assign(import.PageKeys.Item.PageKey);
    }

    import.PageKeys.CheckIndex();

    // ---  Next Tran and Security Logic  --
    if (!IsEmpty(export.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.ApCsePersonsWorkSet.Number;
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
      export.ApCsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
        (10);
      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        // --------------------------------------------------------
        // 04/05/00 W.Campbell - Moved the
        // following ESCAPE stmt to inside
        // this ELSE from just below it.
        // Work done on WR# 00162
        // for PRWORA - Family Violence.
        // --------------------------------------------------------
        return;
      }

      // --------------------------------------------------------
      // 04/05/00 W.Campbell - Added the
      // following COMMAND is DISPLAY.
      // Work done on WR# 00162
      // for PRWORA - Family Violence.
      // --------------------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // --------------------------------------------------------
      // 04/05/00 W.Campbell - Added view matching
      // for the call to SECURITY so that it
      // passes the AP cse person number
      // into the security cab.  Also, changed view
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

    local.Current.Date = Now().Date;

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

    switch(TrimEnd(global.Command))
    {
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
      case "RETCDVL":
        export.IncomeSourceTypePrompt.SelectChar = "";
        export.IncomeSourceIndicator.Type1 = export.IncomeSrcCodeValue.Cdvalue;
        global.Command = "DISPLAY";

        break;
      case "RETCOMP":
        var field = GetField(export.ApPrompt, "selectChar");

        field.Color = "green";
        field.Protected = false;

        global.Command = "DISPLAY";

        break;
      default:
        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "TYPE":
        if (AsChar(export.IncomeSourceIndicator.Type1) == 'R')
        {
          // *** Link to RESL  ***
          ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
        }
        else if (AsChar(export.IncomeSourceIndicator.Type1) == 'M')
        {
          ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
        }
        else if (AsChar(export.IncomeSourceIndicator.Type1) == 'E')
        {
          ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
        }
        else if (AsChar(export.IncomeSourceIndicator.Type1) == 'O')
        {
          // *** Link to INCL  ***
          ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
        }
        else if (IsEmpty(export.IncomeSourceIndicator.Type1))
        {
          ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
        }
        else
        {
          var field = GetField(export.IncomeSourceIndicator, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_ENTER_VALID_INCOME_SO";
        }

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "DISPLAY":
        export.Standard.PageNumber = 1;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.PageKey.EndDt = local.Max.Date;

        break;
      case "LIST":
        // *** Each prompt should have the following IF Statement type  ***
        switch(AsChar(export.IncomeSourceTypePrompt.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.IncomeSrcCode.CodeName = "INCOME SOURCE TYPE";
            ExitState = "ECO_LNK_TO_CODE_VALUES";

            break;
          default:
            var field = GetField(export.IncomeSourceTypePrompt, "selectChar");

            field.Error = true;

            ++local.Invalid.Count;

            break;
        }

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
            var field = GetField(export.ApMultiCasesPrompt, "flag");

            field.Error = true;

            ++local.Invalid.Count;

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
            var field = GetField(export.ArMultiCasesPrompt, "flag");

            field.Error = true;

            ++local.Invalid.Count;

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

            break;
        }

        break;
      case "PVSCR":
        ExitState = "ZD_ECO_XFR_TO_PREV_SCRN";

        break;
      case "NXSCR":
        ExitState = "ECO_XFR_TO_NEXT_SCRN";

        break;
      case "1099":
        ExitState = "ECO_LNK_TO_DISPLAY_1099";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

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
      export.IncomeSourceTypePrompt.SelectChar = "";

      if (IsEmpty(export.Next.Number) || !
        Equal(export.Next.Number, export.Case1.Number))
      {
        export.Office.Name = "";
        export.Office.TypeCode = "";
        export.ServiceProvider.LastName = "";
        export.ServiceProvider.FirstName = "";
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
        export.ApCsePersonAddress.Assign(local.NullCsePersonAddress);
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.Case1.Assign(local.NullCase);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;
        export.ApMultiCases.Flag = "";
        export.ArMultiCases.Flag = "";
        export.IncomeSourceIndicator.Type1 = "";

        // *** Blank out all of the repeating group view for income source ***
        export.IncomeSource.Index = 0;
        export.IncomeSource.Clear();

        for(local.NullIncomeSource.Index = 0; local.NullIncomeSource.Index < local
          .NullIncomeSource.Count; ++local.NullIncomeSource.Index)
        {
          if (export.IncomeSource.IsFull)
          {
            break;
          }

          MoveEmployer1(local.NullIncomeSource.Item.GlocalNullEmployer,
            export.IncomeSource.Update.Gemployer);
          MovePersonIncomeHistory(local.NullIncomeSource.Item.
            GlocalNullPersonIncomeHistory,
            export.IncomeSource.Update.GpersonIncomeHistory);
          export.IncomeSource.Update.GnonEmployIncomeSourceAddress.Assign(
            local.NullIncomeSource.Item.GlocalNullNonEmployIncomeSourceAddress);
            
          MoveIncomeSourceContact(local.NullIncomeSource.Item.
            GlocalNullIncomeSourceContact,
            export.IncomeSource.Update.GincomeSourceContact);
          MoveIncomeSource2(local.NullIncomeSource.Item.GlocalNullIncomeSource,
            export.IncomeSource.Update.GincomeSource);
          export.IncomeSource.Update.GexpReturn.Description =
            local.NullIncomeSource.Item.GlocalNullCodeValue.Description;
          export.IncomeSource.Next();
        }
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

        if (AsChar(local.CaseOpen.Flag) == 'N')
        {
          if (ReadCsePerson4())
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

        foreach(var item in ReadCsePerson5())
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
        export.ApCsePersonAddress.Assign(local.NullCsePersonAddress);
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;

        if (!IsEmpty(import.FromComp.Number))
        {
          export.ApCsePersonsWorkSet.Number = import.FromComp.Number;
        }

        // *** Get AP details  ***
        UseSiReadApCaseRoleDetails();

        // PR140816. March 26, 2002 by Lbachura. Installed the following para. 
        // of logic to provide that the AP is not displayed when the AP is
        // inactive.
        // Modified the logic for PR146446 on 8-13-02. Lbachura
        local.ApOpen.Flag = "N";

        if (AsChar(export.MultipleAps.Flag) == 'Y')
        {
          if (ReadCsePerson2())
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

          // *** Blank out all of the repeating group view for income source ***
          export.IncomeSource.Index = 0;
          export.IncomeSource.Clear();

          for(local.NullIncomeSource.Index = 0; local.NullIncomeSource.Index < local
            .NullIncomeSource.Count; ++local.NullIncomeSource.Index)
          {
            if (export.IncomeSource.IsFull)
            {
              break;
            }

            MoveEmployer1(local.NullIncomeSource.Item.GlocalNullEmployer,
              export.IncomeSource.Update.Gemployer);
            MovePersonIncomeHistory(local.NullIncomeSource.Item.
              GlocalNullPersonIncomeHistory,
              export.IncomeSource.Update.GpersonIncomeHistory);
            export.IncomeSource.Update.GnonEmployIncomeSourceAddress.Assign(
              local.NullIncomeSource.Item.
                GlocalNullNonEmployIncomeSourceAddress);
            MoveIncomeSourceContact(local.NullIncomeSource.Item.
              GlocalNullIncomeSourceContact,
              export.IncomeSource.Update.GincomeSourceContact);
            MoveIncomeSource2(local.NullIncomeSource.Item.
              GlocalNullIncomeSource, export.IncomeSource.Update.GincomeSource);
              
            export.IncomeSource.Update.GexpReturn.Description =
              local.NullIncomeSource.Item.GlocalNullCodeValue.Description;
            export.IncomeSource.Next();
          }

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

        if (ReadCsePersonAddress())
        {
          export.ApCsePersonAddress.Assign(entities.CsePersonAddress);

          if (Equal(export.ApCsePersonAddress.EndDate, local.Max.Date))
          {
            export.ApCsePersonAddress.EndDate = local.NullDateWorkArea.Date;
          }

          if (Equal(export.ApCsePersonAddress.VerifiedDate, local.Max.Date))
          {
            export.ApCsePersonAddress.VerifiedDate =
              local.NullDateWorkArea.Date;
          }
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

      // **********************************************
      // ***   Income Source Information Section   ****
      // **********************************************
      UseSiReadIncomeSourceList();
      ++export.Standard.PageNumber;

      ++export.PageKeys.Index;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.PageKey.Assign(local.PageKey);
      local.NoOfIncSource.Count = local.IncomeSource.Count;

      local.IncomeSource.Index = 0;
      local.IncomeSource.CheckSize();

      local.Local1.Index = 0;
      local.Local1.Clear();

      while(!local.Local1.IsFull)
      {
        if (local.Local1.IsFull)
        {
          break;
        }

        local.Local1.Update.GcsePersonResource.ResourceNo =
          local.IncomeSource.Item.GcsePersonResource.ResourceNo;
        local.Local1.Update.Gemployer.Assign(local.IncomeSource.Item.Gemployer);
        MovePersonIncomeHistory(local.IncomeSource.Item.GpersonIncomeHistory,
          local.Local1.Update.GpersonIncomeHistory);
        local.Local1.Update.GnonEmployIncomeSourceAddress.Assign(
          local.IncomeSource.Item.GnonEmployIncomeSourceAddress);
        local.Local1.Update.GlocalDetailCommon.SelectChar =
          local.IncomeSource.Item.Gcommon.SelectChar;
        local.Local1.Update.GlocalDetailIncomeSource.Assign(
          local.IncomeSource.Item.GincomeSource);
        local.Local1.Update.GlocalDetailMnthlyIncm.TotalCurrency =
          local.IncomeSource.Item.GlocDetailMnthlyIncome.TotalCurrency;

        ++local.IncomeSource.Index;
        local.IncomeSource.CheckSize();

        if (local.IncomeSource.Count > local.NoOfIncSource.Count)
        {
          local.Local1.Next();

          break;
        }

        local.Local1.Next();
      }

      export.IncomeSource.Index = 0;
      export.IncomeSource.Clear();

      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (export.IncomeSource.IsFull)
        {
          break;
        }

        MoveIncomeSource3(local.Local1.Item.GlocalDetailIncomeSource,
          export.IncomeSource.Update.GincomeSource);
        export.IncomeSource.Update.GpersonIncomeHistory.IncomeAmt =
          local.Local1.Item.GlocalDetailMnthlyIncm.TotalCurrency;

        if (export.IncomeSource.Item.GpersonIncomeHistory.IncomeAmt.
          GetValueOrDefault() > 0)
        {
          export.IncomeSource.Update.GpersonIncomeHistory.Freq = "M";
        }

        UseSiReadIncomeSourceDetails();

        // ***  Get the Descripiton for the Employer Return Code  ***
        if (!IsEmpty(export.IncomeSource.Item.GincomeSource.ReturnCd))
        {
          switch(AsChar(export.IncomeSource.Item.GincomeSource.Type1))
          {
            case 'E':
              local.Code.CodeName = "EMPLOYMENT RETURN";

              break;
            case 'M':
              local.Code.CodeName = "MILITARY RETURN";

              break;
            case 'O':
              local.Code.CodeName = "OTHER RETURN";

              break;
            default:
              break;
          }

          local.CodeValue.Cdvalue =
            export.IncomeSource.Item.GincomeSource.ReturnCd ?? Spaces(10);
          UseCabGetCodeValueDescription2();
        }
        else
        {
          export.IncomeSource.Update.GexpReturn.Description =
            Spaces(CodeValue.Description_MaxLength);
        }

        if (!IsEmpty(export.IncomeSource.Item.GincomeSource.Code))
        {
          local.CodeValue.Cdvalue =
            export.IncomeSource.Item.GincomeSource.Code ?? Spaces(10);
          local.Code.CodeName = "INCOME SOURCE OTHER INCOME";
          UseCabGetCodeValueDescription1();
        }

        export.IncomeSource.Next();
      }

      // -----------------------------------------------------------------------
      // 11/14/2001  V.Madhira   PR# 121249   Family Violence Fix.
      // ------------------------------------------------------------------------
      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -----------------------------------------------------------------------
        // Call the SECURITY CAB to check for Family_Violence.
        // Since this is a person (AP) driven screen, pass the Cse_Person number
        // only to the CAB.
        // ------------------------------------------------------------------------
        UseScSecurityCheckForFv();

        if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
        {
          export.ApCsePersonAddress.Assign(local.NullCsePersonAddress);
          export.ApClient.HomePhone = 0;
          export.ApClient.HomePhoneAreaCode = 0;
          export.ApClient.WorkPhone = 0;
          export.ApClient.WorkPhoneAreaCode = 0;
          export.ApClient.WorkPhoneExt = "";
          export.IncomeSourceTypePrompt.SelectChar = "";
          export.IncomeSourceIndicator.Type1 = "";

          // *** Blank out all of the repeating group view for income source ***
          export.IncomeSource.Index = 0;
          export.IncomeSource.Clear();

          for(local.NullIncomeSource.Index = 0; local.NullIncomeSource.Index < local
            .NullIncomeSource.Count; ++local.NullIncomeSource.Index)
          {
            if (export.IncomeSource.IsFull)
            {
              break;
            }

            MoveEmployer1(local.NullIncomeSource.Item.GlocalNullEmployer,
              export.IncomeSource.Update.Gemployer);
            MovePersonIncomeHistory(local.NullIncomeSource.Item.
              GlocalNullPersonIncomeHistory,
              export.IncomeSource.Update.GpersonIncomeHistory);
            export.IncomeSource.Update.GnonEmployIncomeSourceAddress.Assign(
              local.NullIncomeSource.Item.
                GlocalNullNonEmployIncomeSourceAddress);
            MoveIncomeSourceContact(local.NullIncomeSource.Item.
              GlocalNullIncomeSourceContact,
              export.IncomeSource.Update.GincomeSourceContact);
            MoveIncomeSource2(local.NullIncomeSource.Item.
              GlocalNullIncomeSource, export.IncomeSource.Update.GincomeSource);
              
            export.IncomeSource.Update.GexpReturn.Description =
              local.NullIncomeSource.Item.GlocalNullCodeValue.Description;
            export.IncomeSource.Next();
          }

          return;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
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

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
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
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEmployer1(Employer source, Employer target)
  {
    target.Name = source.Name;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveEmployer2(Employer source, Employer target)
  {
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveExport1ToIncomeSource(SiReadIncomeSourceList.Export.
    ExportGroup source, Local.IncomeSourceGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.Gcommon.SelectChar = source.DetailCommon.SelectChar;
    MoveIncomeSource3(source.DetailIncomeSource, target.GincomeSource);
    target.GlocDetailMnthlyIncome.TotalCurrency =
      source.DetailMnthlyIncm.TotalCurrency;
  }

  private static void MoveIncomeSource1(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.SentDt = source.SentDt;
    target.SendTo = source.SendTo;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.Note = source.Note;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
    target.Code = source.Code;
  }

  private static void MoveIncomeSource2(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.SentDt = source.SentDt;
    target.SendTo = source.SendTo;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
    target.Code = source.Code;
    target.MilitaryCode = source.MilitaryCode;
  }

  private static void MoveIncomeSource3(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.Name = source.Name;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSource4(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSourceContact(IncomeSourceContact source,
    IncomeSourceContact target)
  {
    target.ExtensionNo = source.ExtensionNo;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MovePersonIncomeHistory(PersonIncomeHistory source,
    PersonIncomeHistory target)
  {
    target.IncomeAmt = source.IncomeAmt;
    target.Freq = source.Freq;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private void UseCabGetCodeValueDescription1()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.IncomeSource.Update.GexpOtherIncomeSrc.Description =
      useExport.CodeValue.Description;
  }

  private void UseCabGetCodeValueDescription2()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.IncomeSource.Update.GexpReturn.Description =
      useExport.CodeValue.Description;
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

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
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
    export.Uci.Flag = useExport.Uci.Flag;
    export.Military.Flag = useExport.Military.Flag;
    export.Incarceration.Flag = useExport.Incarceration.Flag;
    export.Bankruptcy.Flag = useExport.Bankruptcy.Flag;
    export.OtherChilderen.Flag = useExport.OtherChildren.Flag;
    export.ApMultiCases.Flag = useExport.MultipleCases.Flag;
    MoveCsePerson(useExport.ApCsePerson, export.ApClient);
    MoveCsePersonsWorkSet5(useExport.CsePersonsWorkSet,
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

  private void UseSiReadIncomeSourceDetails()
  {
    var useImport = new SiReadIncomeSourceDetails.Import();
    var useExport = new SiReadIncomeSourceDetails.Export();

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier =
      export.IncomeSource.Item.GincomeSource.Identifier;

    Call(SiReadIncomeSourceDetails.Execute, useImport, useExport);

    export.G.ResourceNo = useExport.CsePersonResource.ResourceNo;
    export.IncomeSource.Update.GnonEmployIncomeSourceAddress.Assign(
      useExport.NonEmployIncomeSourceAddress);
    MoveEmployer2(useExport.Employer, export.IncomeSource.Update.Gemployer);
    MoveIncomeSource1(useExport.IncomeSource,
      export.IncomeSource.Update.GincomeSource);
    export.IncomeSource.Update.GincomeSourceContact.Assign(useExport.Phone);
  }

  private void UseSiReadIncomeSourceList()
  {
    var useImport = new SiReadIncomeSourceList.Import();
    var useExport = new SiReadIncomeSourceList.Export();

    useImport.Search.Type1 = export.IncomeSourceIndicator.Type1;
    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Standard.PageNumber = export.Standard.PageNumber;
    MoveIncomeSource4(export.PageKeys.Item.PageKey, useImport.Page);

    Call(SiReadIncomeSourceList.Execute, useImport, useExport);

    local.PageKey.Assign(useExport.Page);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(local.IncomeSource, MoveExport1ToIncomeSource);
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

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "numb", export.ApCsePersonsWorkSet.Number);
        db.SetNullableDate(command, "startDate", date);
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
        var date = Now().Date;

        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "numb", export.ApClient.Number);
        db.SetNullableDate(command, "startDate", date);
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
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson5()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson5",
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

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApClient.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 13);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
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
    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public IncomeSource PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private IncomeSource pageKey;
    }

    /// <summary>A IncomeSourceGroup group.</summary>
    [Serializable]
    public class IncomeSourceGroup
    {
      /// <summary>
      /// A value of GimpOtherIncomeSrc.
      /// </summary>
      [JsonPropertyName("gimpOtherIncomeSrc")]
      public CodeValue GimpOtherIncomeSrc
      {
        get => gimpOtherIncomeSrc ??= new();
        set => gimpOtherIncomeSrc = value;
      }

      /// <summary>
      /// A value of GcsePersonResource.
      /// </summary>
      [JsonPropertyName("gcsePersonResource")]
      public CsePersonResource GcsePersonResource
      {
        get => gcsePersonResource ??= new();
        set => gcsePersonResource = value;
      }

      /// <summary>
      /// A value of GnonEmployIncomeSourceAddress.
      /// </summary>
      [JsonPropertyName("gnonEmployIncomeSourceAddress")]
      public NonEmployIncomeSourceAddress GnonEmployIncomeSourceAddress
      {
        get => gnonEmployIncomeSourceAddress ??= new();
        set => gnonEmployIncomeSourceAddress = value;
      }

      /// <summary>
      /// A value of GpersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("gpersonIncomeHistory")]
      public PersonIncomeHistory GpersonIncomeHistory
      {
        get => gpersonIncomeHistory ??= new();
        set => gpersonIncomeHistory = value;
      }

      /// <summary>
      /// A value of Gemployer.
      /// </summary>
      [JsonPropertyName("gemployer")]
      public Employer Gemployer
      {
        get => gemployer ??= new();
        set => gemployer = value;
      }

      /// <summary>
      /// A value of GincomeSourceContact.
      /// </summary>
      [JsonPropertyName("gincomeSourceContact")]
      public IncomeSourceContact GincomeSourceContact
      {
        get => gincomeSourceContact ??= new();
        set => gincomeSourceContact = value;
      }

      /// <summary>
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GimpReturn.
      /// </summary>
      [JsonPropertyName("gimpReturn")]
      public CodeValue GimpReturn
      {
        get => gimpReturn ??= new();
        set => gimpReturn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private CodeValue gimpOtherIncomeSrc;
      private CsePersonResource gcsePersonResource;
      private NonEmployIncomeSourceAddress gnonEmployIncomeSourceAddress;
      private PersonIncomeHistory gpersonIncomeHistory;
      private Employer gemployer;
      private IncomeSourceContact gincomeSourceContact;
      private IncomeSource gincomeSource;
      private CodeValue gimpReturn;
    }

    /// <summary>
    /// A value of IncomeSrcCodeValue.
    /// </summary>
    [JsonPropertyName("incomeSrcCodeValue")]
    public CodeValue IncomeSrcCodeValue
    {
      get => incomeSrcCodeValue ??= new();
      set => incomeSrcCodeValue = value;
    }

    /// <summary>
    /// A value of IncomeSrcCode.
    /// </summary>
    [JsonPropertyName("incomeSrcCode")]
    public Code IncomeSrcCode
    {
      get => incomeSrcCode ??= new();
      set => incomeSrcCode = value;
    }

    /// <summary>
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of IncomeSourceTypePrompt.
    /// </summary>
    [JsonPropertyName("incomeSourceTypePrompt")]
    public Common IncomeSourceTypePrompt
    {
      get => incomeSourceTypePrompt ??= new();
      set => incomeSourceTypePrompt = value;
    }

    /// <summary>
    /// A value of IncomeSourceIndicator.
    /// </summary>
    [JsonPropertyName("incomeSourceIndicator")]
    public IncomeSource IncomeSourceIndicator
    {
      get => incomeSourceIndicator ??= new();
      set => incomeSourceIndicator = value;
    }

    /// <summary>
    /// Gets a value of IncomeSource.
    /// </summary>
    [JsonIgnore]
    public Array<IncomeSourceGroup> IncomeSource => incomeSource ??= new(
      IncomeSourceGroup.Capacity);

    /// <summary>
    /// Gets a value of IncomeSource for json serialization.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    [Computed]
    public IList<IncomeSourceGroup> IncomeSource_Json
    {
      get => incomeSource;
      set => IncomeSource.Assign(value);
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
    /// A value of MaritalStatDescription.
    /// </summary>
    [JsonPropertyName("maritalStatDescription")]
    public CodeValue MaritalStatDescription
    {
      get => maritalStatDescription ??= new();
      set => maritalStatDescription = value;
    }

    /// <summary>
    /// A value of MaritalStatus.
    /// </summary>
    [JsonPropertyName("maritalStatus")]
    public CodeValue MaritalStatus
    {
      get => maritalStatus ??= new();
      set => maritalStatus = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
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
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
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
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private CodeValue incomeSrcCodeValue;
    private Code incomeSrcCode;
    private Array<PageKeysGroup> pageKeys;
    private Common incomeSourceTypePrompt;
    private IncomeSource incomeSourceIndicator;
    private Array<IncomeSourceGroup> incomeSource;
    private CsePersonsWorkSet fromComp;
    private CsePersonsWorkSet ch;
    private CaseRole arCaseRole;
    private CodeValue maritalStatDescription;
    private CodeValue maritalStatus;
    private PersonPrivateAttorney personPrivateAttorney;
    private FplsLocateRequest fplsLocateRequest;
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
    private Common military;
    private Common incarceration;
    private Common bankruptcy;
    private Common otherChildren;
    private Common apMultiCases;
    private CaseRole apCaseRole;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePerson apClient;
    private CsePersonAddress apCsePersonAddress;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apPrompt;
    private NextTranInfo hidden;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private ServiceProvider serviceProvider;
    private CodeValue caseCloseRsn;
    private Case1 case1;
    private Case1 next;
    private Security2 security;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public IncomeSource PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private IncomeSource pageKey;
    }

    /// <summary>A IncomeSourceGroup group.</summary>
    [Serializable]
    public class IncomeSourceGroup
    {
      /// <summary>
      /// A value of GexpOtherIncomeSrc.
      /// </summary>
      [JsonPropertyName("gexpOtherIncomeSrc")]
      public CodeValue GexpOtherIncomeSrc
      {
        get => gexpOtherIncomeSrc ??= new();
        set => gexpOtherIncomeSrc = value;
      }

      /// <summary>
      /// A value of GnonEmployIncomeSourceAddress.
      /// </summary>
      [JsonPropertyName("gnonEmployIncomeSourceAddress")]
      public NonEmployIncomeSourceAddress GnonEmployIncomeSourceAddress
      {
        get => gnonEmployIncomeSourceAddress ??= new();
        set => gnonEmployIncomeSourceAddress = value;
      }

      /// <summary>
      /// A value of GpersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("gpersonIncomeHistory")]
      public PersonIncomeHistory GpersonIncomeHistory
      {
        get => gpersonIncomeHistory ??= new();
        set => gpersonIncomeHistory = value;
      }

      /// <summary>
      /// A value of Gemployer.
      /// </summary>
      [JsonPropertyName("gemployer")]
      public Employer Gemployer
      {
        get => gemployer ??= new();
        set => gemployer = value;
      }

      /// <summary>
      /// A value of GincomeSourceContact.
      /// </summary>
      [JsonPropertyName("gincomeSourceContact")]
      public IncomeSourceContact GincomeSourceContact
      {
        get => gincomeSourceContact ??= new();
        set => gincomeSourceContact = value;
      }

      /// <summary>
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GexpReturn.
      /// </summary>
      [JsonPropertyName("gexpReturn")]
      public CodeValue GexpReturn
      {
        get => gexpReturn ??= new();
        set => gexpReturn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private CodeValue gexpOtherIncomeSrc;
      private NonEmployIncomeSourceAddress gnonEmployIncomeSourceAddress;
      private PersonIncomeHistory gpersonIncomeHistory;
      private Employer gemployer;
      private IncomeSourceContact gincomeSourceContact;
      private IncomeSource gincomeSource;
      private CodeValue gexpReturn;
    }

    /// <summary>
    /// A value of SearchResl.
    /// </summary>
    [JsonPropertyName("searchResl")]
    public CsePerson SearchResl
    {
      get => searchResl ??= new();
      set => searchResl = value;
    }

    /// <summary>
    /// A value of IncomeSrcCodeValue.
    /// </summary>
    [JsonPropertyName("incomeSrcCodeValue")]
    public CodeValue IncomeSrcCodeValue
    {
      get => incomeSrcCodeValue ??= new();
      set => incomeSrcCodeValue = value;
    }

    /// <summary>
    /// A value of IncomeSrcCode.
    /// </summary>
    [JsonPropertyName("incomeSrcCode")]
    public Code IncomeSrcCode
    {
      get => incomeSrcCode ??= new();
      set => incomeSrcCode = value;
    }

    /// <summary>
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of IncomeSourceTypePrompt.
    /// </summary>
    [JsonPropertyName("incomeSourceTypePrompt")]
    public Common IncomeSourceTypePrompt
    {
      get => incomeSourceTypePrompt ??= new();
      set => incomeSourceTypePrompt = value;
    }

    /// <summary>
    /// A value of IncomeSourceIndicator.
    /// </summary>
    [JsonPropertyName("incomeSourceIndicator")]
    public IncomeSource IncomeSourceIndicator
    {
      get => incomeSourceIndicator ??= new();
      set => incomeSourceIndicator = value;
    }

    /// <summary>
    /// Gets a value of IncomeSource.
    /// </summary>
    [JsonIgnore]
    public Array<IncomeSourceGroup> IncomeSource => incomeSource ??= new(
      IncomeSourceGroup.Capacity);

    /// <summary>
    /// Gets a value of IncomeSource for json serialization.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    [Computed]
    public IList<IncomeSourceGroup> IncomeSource_Json
    {
      get => incomeSource;
      set => IncomeSource.Assign(value);
    }

    /// <summary>
    /// A value of G.
    /// </summary>
    [JsonPropertyName("g")]
    public CsePersonResource G
    {
      get => g ??= new();
      set => g = value;
    }

    /// <summary>
    /// A value of MaritalStatDescription.
    /// </summary>
    [JsonPropertyName("maritalStatDescription")]
    public CodeValue MaritalStatDescription
    {
      get => maritalStatDescription ??= new();
      set => maritalStatDescription = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
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
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
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
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private CsePerson searchResl;
    private CodeValue incomeSrcCodeValue;
    private Code incomeSrcCode;
    private Array<PageKeysGroup> pageKeys;
    private Common incomeSourceTypePrompt;
    private IncomeSource incomeSourceIndicator;
    private Array<IncomeSourceGroup> incomeSource;
    private CsePersonResource g;
    private CodeValue maritalStatDescription;
    private PersonPrivateAttorney personPrivateAttorney;
    private FplsLocateRequest fplsLocateRequest;
    private CsePersonsWorkSet comnLink;
    private CodeValue progCodeDescription;
    private Common arMultiCasesPrompt;
    private Common apMultiCasesPrompt;
    private Common arMultiCases;
    private CaseRole apCaseRole;
    private TextWorkArea altSsnAlias;
    private Common multipleAps;
    private Common uci;
    private Common military;
    private Common incarceration;
    private Common bankruptcy;
    private Common otherChilderen;
    private Common apMultiCases;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePerson apClient;
    private CsePersonAddress apCsePersonAddress;
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
    private NextTranInfo hidden;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A NullIncomeSourceGroup group.</summary>
    [Serializable]
    public class NullIncomeSourceGroup
    {
      /// <summary>
      /// A value of GlocalNullNonEmployIncomeSourceAddress.
      /// </summary>
      [JsonPropertyName("glocalNullNonEmployIncomeSourceAddress")]
      public NonEmployIncomeSourceAddress GlocalNullNonEmployIncomeSourceAddress
      {
        get => glocalNullNonEmployIncomeSourceAddress ??= new();
        set => glocalNullNonEmployIncomeSourceAddress = value;
      }

      /// <summary>
      /// A value of GlocalNullPersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("glocalNullPersonIncomeHistory")]
      public PersonIncomeHistory GlocalNullPersonIncomeHistory
      {
        get => glocalNullPersonIncomeHistory ??= new();
        set => glocalNullPersonIncomeHistory = value;
      }

      /// <summary>
      /// A value of GlocalNullEmployer.
      /// </summary>
      [JsonPropertyName("glocalNullEmployer")]
      public Employer GlocalNullEmployer
      {
        get => glocalNullEmployer ??= new();
        set => glocalNullEmployer = value;
      }

      /// <summary>
      /// A value of GlocalNullIncomeSourceContact.
      /// </summary>
      [JsonPropertyName("glocalNullIncomeSourceContact")]
      public IncomeSourceContact GlocalNullIncomeSourceContact
      {
        get => glocalNullIncomeSourceContact ??= new();
        set => glocalNullIncomeSourceContact = value;
      }

      /// <summary>
      /// A value of GlocalNullIncomeSource.
      /// </summary>
      [JsonPropertyName("glocalNullIncomeSource")]
      public IncomeSource GlocalNullIncomeSource
      {
        get => glocalNullIncomeSource ??= new();
        set => glocalNullIncomeSource = value;
      }

      /// <summary>
      /// A value of GlocalNullCodeValue.
      /// </summary>
      [JsonPropertyName("glocalNullCodeValue")]
      public CodeValue GlocalNullCodeValue
      {
        get => glocalNullCodeValue ??= new();
        set => glocalNullCodeValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private NonEmployIncomeSourceAddress glocalNullNonEmployIncomeSourceAddress;
        
      private PersonIncomeHistory glocalNullPersonIncomeHistory;
      private Employer glocalNullEmployer;
      private IncomeSourceContact glocalNullIncomeSourceContact;
      private IncomeSource glocalNullIncomeSource;
      private CodeValue glocalNullCodeValue;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of GnonEmployIncomeSourceAddress.
      /// </summary>
      [JsonPropertyName("gnonEmployIncomeSourceAddress")]
      public NonEmployIncomeSourceAddress GnonEmployIncomeSourceAddress
      {
        get => gnonEmployIncomeSourceAddress ??= new();
        set => gnonEmployIncomeSourceAddress = value;
      }

      /// <summary>
      /// A value of GpersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("gpersonIncomeHistory")]
      public PersonIncomeHistory GpersonIncomeHistory
      {
        get => gpersonIncomeHistory ??= new();
        set => gpersonIncomeHistory = value;
      }

      /// <summary>
      /// A value of Gemployer.
      /// </summary>
      [JsonPropertyName("gemployer")]
      public Employer Gemployer
      {
        get => gemployer ??= new();
        set => gemployer = value;
      }

      /// <summary>
      /// A value of GcsePersonResource.
      /// </summary>
      [JsonPropertyName("gcsePersonResource")]
      public CsePersonResource GcsePersonResource
      {
        get => gcsePersonResource ??= new();
        set => gcsePersonResource = value;
      }

      /// <summary>
      /// A value of GlocalDetailCommon.
      /// </summary>
      [JsonPropertyName("glocalDetailCommon")]
      public Common GlocalDetailCommon
      {
        get => glocalDetailCommon ??= new();
        set => glocalDetailCommon = value;
      }

      /// <summary>
      /// A value of GlocalDetailIncomeSource.
      /// </summary>
      [JsonPropertyName("glocalDetailIncomeSource")]
      public IncomeSource GlocalDetailIncomeSource
      {
        get => glocalDetailIncomeSource ??= new();
        set => glocalDetailIncomeSource = value;
      }

      /// <summary>
      /// A value of GlocalDetailMnthlyIncm.
      /// </summary>
      [JsonPropertyName("glocalDetailMnthlyIncm")]
      public Common GlocalDetailMnthlyIncm
      {
        get => glocalDetailMnthlyIncm ??= new();
        set => glocalDetailMnthlyIncm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private NonEmployIncomeSourceAddress gnonEmployIncomeSourceAddress;
      private PersonIncomeHistory gpersonIncomeHistory;
      private Employer gemployer;
      private CsePersonResource gcsePersonResource;
      private Common glocalDetailCommon;
      private IncomeSource glocalDetailIncomeSource;
      private Common glocalDetailMnthlyIncm;
    }

    /// <summary>A IncomeSourceGroup group.</summary>
    [Serializable]
    public class IncomeSourceGroup
    {
      /// <summary>
      /// A value of GcsePersonResource.
      /// </summary>
      [JsonPropertyName("gcsePersonResource")]
      public CsePersonResource GcsePersonResource
      {
        get => gcsePersonResource ??= new();
        set => gcsePersonResource = value;
      }

      /// <summary>
      /// A value of Gemployer.
      /// </summary>
      [JsonPropertyName("gemployer")]
      public Employer Gemployer
      {
        get => gemployer ??= new();
        set => gemployer = value;
      }

      /// <summary>
      /// A value of GpersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("gpersonIncomeHistory")]
      public PersonIncomeHistory GpersonIncomeHistory
      {
        get => gpersonIncomeHistory ??= new();
        set => gpersonIncomeHistory = value;
      }

      /// <summary>
      /// A value of GnonEmployIncomeSourceAddress.
      /// </summary>
      [JsonPropertyName("gnonEmployIncomeSourceAddress")]
      public NonEmployIncomeSourceAddress GnonEmployIncomeSourceAddress
      {
        get => gnonEmployIncomeSourceAddress ??= new();
        set => gnonEmployIncomeSourceAddress = value;
      }

      /// <summary>
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GlocDetailMnthlyIncome.
      /// </summary>
      [JsonPropertyName("glocDetailMnthlyIncome")]
      public Common GlocDetailMnthlyIncome
      {
        get => glocDetailMnthlyIncome ??= new();
        set => glocDetailMnthlyIncome = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private CsePersonResource gcsePersonResource;
      private Employer gemployer;
      private PersonIncomeHistory gpersonIncomeHistory;
      private NonEmployIncomeSourceAddress gnonEmployIncomeSourceAddress;
      private Common gcommon;
      private IncomeSource gincomeSource;
      private Common glocDetailMnthlyIncome;
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
    /// Gets a value of NullIncomeSource.
    /// </summary>
    [JsonIgnore]
    public Array<NullIncomeSourceGroup> NullIncomeSource =>
      nullIncomeSource ??= new(NullIncomeSourceGroup.Capacity);

    /// <summary>
    /// Gets a value of NullIncomeSource for json serialization.
    /// </summary>
    [JsonPropertyName("nullIncomeSource")]
    [Computed]
    public IList<NullIncomeSourceGroup> NullIncomeSource_Json
    {
      get => nullIncomeSource;
      set => NullIncomeSource.Assign(value);
    }

    /// <summary>
    /// A value of PageKey.
    /// </summary>
    [JsonPropertyName("pageKey")]
    public IncomeSource PageKey
    {
      get => pageKey ??= new();
      set => pageKey = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of NullCsePersonAddress.
    /// </summary>
    [JsonPropertyName("nullCsePersonAddress")]
    public CsePersonAddress NullCsePersonAddress
    {
      get => nullCsePersonAddress ??= new();
      set => nullCsePersonAddress = value;
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
    /// Gets a value of IncomeSource.
    /// </summary>
    [JsonIgnore]
    public Array<IncomeSourceGroup> IncomeSource => incomeSource ??= new(
      IncomeSourceGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomeSource for json serialization.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    [Computed]
    public IList<IncomeSourceGroup> IncomeSource_Json
    {
      get => incomeSource;
      set => IncomeSource.Assign(value);
    }

    /// <summary>
    /// A value of NoOfIncSource.
    /// </summary>
    [JsonPropertyName("noOfIncSource")]
    public Common NoOfIncSource
    {
      get => noOfIncSource ??= new();
      set => noOfIncSource = value;
    }

    private Common apOpen;
    private Common caseOpen;
    private Code code;
    private CodeValue codeValue;
    private Array<NullIncomeSourceGroup> nullIncomeSource;
    private IncomeSource pageKey;
    private Array<LocalGroup> local1;
    private Common apAltOccur;
    private Common arAltOccur;
    private Common multipleAps;
    private Case1 nullCase;
    private CsePerson nullCsePerson;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private CsePersonAddress nullCsePersonAddress;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea max;
    private Common invalid;
    private Common noAps;
    private AbendData abendData;
    private Array<IncomeSourceGroup> incomeSource;
    private Common noOfIncSource;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of IncomeNonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("incomeNonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress IncomeNonEmployIncomeSourceAddress
    {
      get => incomeNonEmployIncomeSourceAddress ??= new();
      set => incomeNonEmployIncomeSourceAddress = value;
    }

    /// <summary>
    /// A value of IncomeIncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeIncomeSourceContact")]
    public IncomeSourceContact IncomeIncomeSourceContact
    {
      get => incomeIncomeSourceContact ??= new();
      set => incomeIncomeSourceContact = value;
    }

    /// <summary>
    /// A value of IncomeCodeValue.
    /// </summary>
    [JsonPropertyName("incomeCodeValue")]
    public CodeValue IncomeCodeValue
    {
      get => incomeCodeValue ??= new();
      set => incomeCodeValue = value;
    }

    /// <summary>
    /// A value of IncomeIncomeSource.
    /// </summary>
    [JsonPropertyName("incomeIncomeSource")]
    public IncomeSource IncomeIncomeSource
    {
      get => incomeIncomeSource ??= new();
      set => incomeIncomeSource = value;
    }

    /// <summary>
    /// A value of IncomeEmployerAddress.
    /// </summary>
    [JsonPropertyName("incomeEmployerAddress")]
    public EmployerAddress IncomeEmployerAddress
    {
      get => incomeEmployerAddress ??= new();
      set => incomeEmployerAddress = value;
    }

    /// <summary>
    /// A value of IncomeEmployer.
    /// </summary>
    [JsonPropertyName("incomeEmployer")]
    public Employer IncomeEmployer
    {
      get => incomeEmployer ??= new();
      set => incomeEmployer = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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

    private CsePersonResource csePersonResource;
    private NonEmployIncomeSourceAddress incomeNonEmployIncomeSourceAddress;
    private IncomeSourceContact incomeIncomeSourceContact;
    private CodeValue incomeCodeValue;
    private IncomeSource incomeIncomeSource;
    private EmployerAddress incomeEmployerAddress;
    private Employer incomeEmployer;
    private CaseRole child;
    private HealthInsuranceViability healthInsuranceViability;
    private PersonPrivateAttorney personPrivateAttorney;
    private CsePersonAddress csePersonAddress;
    private Code code;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}

// Program: QA_QCHD_QUICK_CHILD_DATA, ID: 372233287, model: 746.
// Short name: SWEQCHDP
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
/// A program: QA_QCHD_QUICK_CHILD_DATA.
/// </para>
/// <para>
/// RESP:   Quality Assurance.
/// QA Quick Reference AP Data Screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQchdQuickChildData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QCHD_QUICK_CHILD_DATA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQchdQuickChildData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQchdQuickChildData.
  /// </summary>
  public QaQchdQuickChildData(IContext context, Import import, Export export):
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
    // 01/18/98   JF Caillouet             Initial Development
    // 04/11/2000    Vithal Madhira    PR# 92743.
    // Added new flow to HIPL (PF21). Now the flow to HICP (PF21) will no longer
    // exists.
    // --------------------------------------------------------
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // WR20202 installed to have closed case displayed on the screen. 12-5-01. 
    // L. Bachura.
    // PR139464 on 2-26-2002. Installed local view case open flag and logic to 
    // change read to pick up ar with open role date when more than one ar on a
    // case. LBachura
    // 10/28/02 K.Doshi         Fix screen Help.
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
    UseCabSetMaximumDiscontinueDate();

    // ---  Move Section  --
    MoveCase2(import.Case1, export.Case1);
    MoveCaseFuncWorkSet(import.CaseFuncWorkSet, export.CaseFuncWorkSet);
    MoveCodeValue(import.CaseCloseRsn, export.CaseCloseRsn);
    MoveHealthInsuranceViability(import.HealthInsuranceViability,
      export.HealthInsuranceViability);
    export.Program.Code = import.Program.Code;
    export.ProgCodeDescription.Description =
      import.ProgCodeDescription.Description;
    MoveServiceProvider(import.ServiceProvider, export.ServiceProvider);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCaseRole1(import.ApCaseRole, export.ApCaseRole);
    export.ApClient.Assign(import.ApClient);
    export.ApCsePersonAddress.Assign(import.ApCsePersonAddress);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.Ar.Assign(import.ArCsePersonsWorkSet);
    export.Bankruptcy.Flag = import.Bankruptcy.Flag;
    export.Hidden.Assign(import.Hidden);
    export.Incarceration.Flag = import.Incarceration.Flag;
    export.Military.Flag = import.Military.Flag;
    export.MultipleAps.Flag = import.MultipleAps.Flag;
    export.ApMultiCases.Flag = import.ApMultiCases.Flag;
    export.Next.Number = import.Next.Number;
    export.OtherChilderen.Flag = import.OtherChildren.Flag;
    export.Uci.Flag = import.Uci.Flag;
    export.ArMultiCases.Flag = import.ArMultiCases.Flag;
    export.AltSsnAlias.Text30 = import.AltSsnAlias.Text30;
    MoveOffice(import.Office, export.Office);
    export.ApMultiCasesPrompt.Flag = import.ApMultiCasesPrompt.Flag;
    local.ArAltOccur.Flag = import.ArAltOccur.Flag;
    export.ArMultiCasesPrompt.Flag = import.ArMultiCasesPrompt.Flag;
    export.ComnLink.Assign(import.ComnLink);
    MoveCsePersonsWorkSet8(import.FromComp, export.FromComp);

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    export.Child.Index = 0;
    export.Child.Clear();

    for(import.Child.Index = 0; import.Child.Index < import.Child.Count; ++
      import.Child.Index)
    {
      if (export.Child.IsFull)
      {
        break;
      }

      export.Child.Update.GcaseRole.Assign(import.Child.Item.GcaseRole);
      export.Child.Update.Gcommon.SelectChar =
        import.Child.Item.Gcommon.SelectChar;
      export.Child.Update.Gprogram.Code = import.Child.Item.Gprogram.Code;
      export.Child.Update.GexportCh.Assign(import.Child.Item.GimportCh);
      export.Child.Update.GexportStatus.Text1 =
        import.Child.Item.GimportStatus.Text1;
      export.Child.Update.GexportAbsenceRsn.Description =
        import.Child.Item.GimportAbsenceRsn.Description;
      export.Child.Update.GexportRelToAr.Description =
        import.Child.Item.GimportRelToAr.Description;
      export.Child.Update.GexportChHealthInsuInd.Flag =
        import.Child.Item.GimportChHealthInsuInd.Flag;
      export.Child.Next();
    }

    MoveStandard(import.HiddenPage, export.HiddenPage);

    for(import.PageKeys.Index = 0; import.PageKeys.Index < import
      .PageKeys.Count; ++import.PageKeys.Index)
    {
      if (!import.PageKeys.CheckSize())
      {
        break;
      }

      export.PageKeys.Index = import.PageKeys.Index;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.GexportPageKeysCsePersonsWorkSet.Number =
        import.PageKeys.Item.GimportPageKeysCsePersonsWorkSet.Number;
      export.PageKeys.Update.GexportPageKeysWorkArea.Text1 =
        import.PageKeys.Item.GimportPageKeysWorkArea.Text1;
    }

    import.PageKeys.CheckIndex();

    // ---  Next Tran and Security Logic  --
    if (!IsEmpty(export.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Next.Number;
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.ApCsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
        UseCabZeroFillNumber();
      }

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

    // ***   Test for Selection Characters   ***
    for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
      export.Child.Index)
    {
      switch(AsChar(export.Child.Item.Gcommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.Select.Count;
          export.SelectCh.Number = export.Child.Item.GexportCh.Number;
          export.SelectedChild.Number = export.Child.Item.GexportCh.Number;

          break;
        default:
          var field = GetField(export.Child.Item.Gcommon, "selectChar");

          field.Error = true;

          ++local.Select.Count;
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (local.Select.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
        export.Child.Index)
      {
        if (!IsEmpty(export.Child.Item.Gcommon.SelectChar))
        {
          var field = GetField(export.Child.Item.Gcommon, "selectChar");

          field.Error = true;
        }
      }
    }
    else if (local.Select.Count < 1)
    {
      if (Equal(global.Command, "CHDS") || Equal(global.Command, "CURA") || Equal
        (global.Command, "PEPR") || Equal(global.Command, "HIPL"))
      {
        ExitState = "ACO_NE0000_SELECTION_REQUIRED";

        for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
          export.Child.Index)
        {
          var field = GetField(export.Child.Item.Gcommon, "selectChar");

          field.Error = true;

          break;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
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
      case "LIST":
        // Each prompt should have the following IF Statement type
        switch(AsChar(export.ApPrompt.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Select.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field1 = GetField(export.ApPrompt, "selectChar");

            field1.Error = true;

            ++local.Select.Count;
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
            ++local.Select.Count;
            MoveCsePersonsWorkSet2(export.ApCsePersonsWorkSet, export.ComnLink);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field1 = GetField(export.ApMultiCasesPrompt, "flag");

            field1.Error = true;

            ++local.Select.Count;
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
            ++local.Select.Count;
            MoveCsePersonsWorkSet3(export.Ar, export.ComnLink);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field1 = GetField(export.ArMultiCasesPrompt, "flag");

            field1.Error = true;

            ++local.Select.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(local.Select.Count)
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

            break;
        }

        break;
      case "NEXT":
        if (export.HiddenPage.PageNumber >= Export.PageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          return;
        }

        export.PageKeys.Index = export.HiddenPage.PageNumber;
        export.PageKeys.CheckSize();

        if (IsEmpty(export.PageKeys.Item.GexportPageKeysCsePersonsWorkSet.Number)
          || IsEmpty(export.PageKeys.Item.GexportPageKeysWorkArea.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenPage.PageNumber;

        break;
      case "PREV":
        if (export.HiddenPage.PageNumber <= 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenPage.PageNumber;

        break;
      case "DISPLAY":
        export.HiddenPage.PageNumber = 1;
        export.PageKeys.Count = 0;

        break;
      case "ALTS":
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        break;
      case "CURA":
        ExitState = "ECO_LNK_TO_CURA";

        break;
      case "CHDS":
        ExitState = "ECO_LNK_TO_CHDS";

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
      case "HIPL":
        ExitState = "ECO_LNK_TO_LIST_INSURANCE_COVERA";

        break;
      case "ROLE":
        ExitState = "ECO_LNK_TO_ROLE";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      export.ApPrompt.SelectChar = "";
      export.ArMultiCasesPrompt.Flag = "";
      export.ApMultiCasesPrompt.Flag = "";
      export.ApMultiCases.Flag = "";

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
        export.ApMultiCases.Flag = "";
        export.ArMultiCases.Flag = "";
        export.Military.Flag = "";
        export.Uci.Flag = "";
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.MaritalStatDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Cdvalue = "";
        export.CaseUnitFunctionAssignmt.Function = "";
        export.AltSsnAlias.Text30 = "";
        export.HealthInsuranceViability.HinsViableInd = "";
        export.ApCsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
        export.Ar.Assign(local.NullCsePersonsWorkSet);
        MoveCsePersonAddress(local.NullCsePersonAddress,
          export.ApCsePersonAddress);
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.Case1.Assign(local.NullCase);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;

        for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
          export.Child.Index)
        {
          export.Child.Update.GexportCh.Assign(local.NullCsePersonsWorkSet);
          export.Child.Update.GcaseRole.Assign(local.GlocalNull);
          export.Child.Update.Gprogram.Code = "";
          export.Child.Update.GexportRelToAr.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.Child.Update.GexportStatus.Text1 = "";
          export.Child.Update.Gcommon.SelectChar = "";
          export.Child.Update.GexportAbsenceRsn.Description =
            Spaces(CodeValue.Description_MaxLength);
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
          if (ReadCodeValue2())
          {
            export.CaseCloseRsn.Description = entities.CodeValue.Description;
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
        UseSiReadCsePerson1();

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
      local.MultiAps.Flag = "N";

      if (AsChar(export.MultipleAps.Flag) == 'Y')
      {
        local.MultiAps.Flag = "Y";

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
        export.Military.Flag = "";
        export.Uci.Flag = "";
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.MaritalStatDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.AltSsnAlias.Text30 = "";
        export.HealthInsuranceViability.HinsViableInd = "";
        MoveCsePersonAddress(local.NullCsePersonAddress,
          export.ApCsePersonAddress);
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;

        if (!IsEmpty(import.FromComp.Number))
        {
          export.ApCsePersonsWorkSet.Number = import.FromComp.Number;
        }

        // *** Get AP details  ***
        UseSiReadApCaseRoleDetails();

        // PR140816. March 26, 2002. Installed logic below to prevent the 
        // display of AP information if the AP is inactive on the case.
        // LBachura.
        // PR146446 installed on 8-14-02 to change this logic. Lbachura
        export.LocalApOpen.Flag = "N";

        if (AsChar(local.MultiAps.Flag) == 'Y')
        {
          if (ReadCsePerson1())
          {
            export.LocalApOpen.Flag = "Y";
          }
        }

        if (AsChar(local.MultiAps.Flag) == 'N' && !
          Lt(Now().Date, export.ApCaseRole.EndDate))
        {
          if (ReadCsePerson1())
          {
            export.LocalApOpen.Flag = "Y";
          }
        }

        if (!Lt(Now().Date, export.ApCaseRole.EndDate) && AsChar
          (export.LocalApOpen.Flag) == 'N')
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

        if (ReadCsePersonAddress())
        {
          MoveCsePersonAddress(entities.CsePersonAddress,
            export.ApCsePersonAddress);

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
        if (ReadCodeValue3())
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

      // ***********************
      // ***  Child Section  ***
      // ***********************
      local.CaseRole.Type1 = "CH";

      export.PageKeys.Index = export.HiddenPage.PageNumber - 1;
      export.PageKeys.CheckSize();

      local.PageKeysWorkArea.Text1 =
        export.PageKeys.Item.GexportPageKeysWorkArea.Text1;
      local.PageKeysCsePersonsWorkSet.Number =
        export.PageKeys.Item.GexportPageKeysCsePersonsWorkSet.Number;
      UseSiReadSpecificCaseRoles();

      export.PageKeys.Index = export.HiddenPage.PageNumber;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.GexportPageKeysWorkArea.Text1 =
        local.PageKeysWorkArea.Text1;
      export.PageKeys.Update.GexportPageKeysCsePersonsWorkSet.Number =
        local.PageKeysCsePersonsWorkSet.Number;

      export.Child.Index = 0;
      export.Child.Clear();

      for(local.Child.Index = 0; local.Child.Index < local.Child.Count; ++
        local.Child.Index)
      {
        if (export.Child.IsFull)
        {
          break;
        }

        export.Child.Update.Gcommon.SelectChar = "";

        if (AsChar(local.Child.Item.GlocalChildWorkArea.Text1) == 'A')
        {
          ReadCaseRole3();
        }
        else if (AsChar(local.Child.Item.GlocalChildWorkArea.Text1) == 'P')
        {
          ReadCaseRole1();
        }
        else
        {
          ReadCaseRole2();
        }

        // *** person prog **
        if (ReadPersonProgramProgram())
        {
          export.Child.Update.Gprogram.Code = entities.Program.Code;
        }

        UseSiReadCsePerson2();
        MoveCsePersonsWorkSet7(local.Child.Item.GlocalChildCsePersonsWorkSet,
          export.Child.Update.GexportCh);
        MoveCaseRole2(entities.CaseRole, export.Child.Update.GcaseRole);
        export.Child.Update.GexportStatus.Text1 =
          local.Child.Item.GlocalChildWorkArea.Text1;

        if (Equal(export.Child.Item.GexportCh.Ssn, "000000000"))
        {
          export.Child.Update.GexportCh.Ssn = "";
        }

        if (!IsEmpty(export.Child.Item.GcaseRole.AbsenceReasonCode))
        {
          if (ReadCodeValue1())
          {
            export.Child.Update.GexportAbsenceRsn.Description =
              entities.CodeValue.Description;
          }
          else
          {
            var field =
              GetField(export.Child.Item.GexportAbsenceRsn, "description");

            field.Error = true;
          }
        }
        else
        {
          export.Child.Update.GexportAbsenceRsn.Description =
            Spaces(CodeValue.Description_MaxLength);
        }

        if (!IsEmpty(export.Child.Item.GcaseRole.RelToAr))
        {
          if (ReadCodeValue4())
          {
            export.Child.Update.GexportRelToAr.Description =
              entities.CodeValue.Description;
          }
          else
          {
            var field =
              GetField(export.Child.Item.GexportRelToAr, "description");

            field.Error = true;
          }
        }
        else
        {
          export.Child.Update.GexportRelToAr.Description =
            Spaces(CodeValue.Description_MaxLength);
        }

        // ---------------------------------------------------------------------------------------
        // WR#000160: The Paternity_Establishment_Indicator was moved from '
        // Case_Role' to 'CSE_Person'. We are displaying this  value on the
        // screen using the attribute "FLAG" in 'G_Export_ Ch
        // CSE_Person_Work_Set'.  We will READ for 'CSE_Person' with 'CASE_ROLE'
        // type "CH" on the present CASE. Then SET the value of '
        // Paternity_Establishment_Indicator' in 'CSE_Person'   to "FLAG".
        // ---------------------------------------------------------------------------------------
        if (ReadCsePerson4())
        {
          export.Child.Update.GexportCh.Flag =
            entities.ChildCsePerson.PaternityEstablishedIndicator ?? Spaces(1);
        }

        // ---------------------------------------------------------------------------
        // PR# 92743   :   The ' Hlth Ins '  flag is set to 'Health Insurance 
        // Indicator' of CASE_ROLE. But this attribute is not getting updated
        // whenever the info is added/updated about child health insurance to
        // the system. Now the ' Personal_Health_Insurace ' (table)   for the
        // CHILD will be read and then the value of ' Hlth Ins '  will be set to
        // Y/N accordingly.
        // ----------------------------------------------------------------------------------
        if (ReadPersonalHealthInsurance())
        {
          export.Child.Update.GexportChHealthInsuInd.Flag = "Y";
        }
        else
        {
          export.Child.Update.GexportChHealthInsuInd.Flag = "N";
        }

        export.Child.Next();
      }

      // ***  After the Party is Over  ***
      if (IsExitState("ACO_NN0000_ALL_OK") && AsChar(export.Case1.Status) == 'O'
        )
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (IsExitState("ACO_NN0000_ALL_OK") && AsChar(export.Case1.Status) == 'C'
        )
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

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
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

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.AbsenceReasonCode = source.AbsenceReasonCode;
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.DateOfEmancipation = source.DateOfEmancipation;
    target.RelToAr = source.RelToAr;
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

  private static void MoveCsePersonAddress(CsePersonAddress source,
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
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet5(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet6(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet7(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet8(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet9(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToChild(SiReadSpecificCaseRoles.Export.
    ExportGroup source, Local.ChildGroup target)
  {
    target.GlocalChildCommon.SelectChar = source.DetailCommon.SelectChar;
    target.GlocalChildWorkArea.Text1 = source.DetailWorkArea.Text1;
    MoveCsePersonsWorkSet6(source.DetailCsePersonsWorkSet,
      target.GlocalChildCsePersonsWorkSet);
  }

  private static void MoveHealthInsuranceViability(
    HealthInsuranceViability source, HealthInsuranceViability target)
  {
    target.Identifier = source.Identifier;
    target.HinsViableInd = source.HinsViableInd;
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
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

    useImport.Case1.Number = export.Next.Number;

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
    export.Uci.Flag = useExport.Uci.Flag;
    export.Military.Flag = useExport.Military.Flag;
    export.Incarceration.Flag = useExport.Incarceration.Flag;
    export.Bankruptcy.Flag = useExport.Bankruptcy.Flag;
    export.OtherChilderen.Flag = useExport.OtherChildren.Flag;
    export.ApMultiCases.Flag = useExport.MultipleCases.Flag;
    MoveCsePerson(useExport.ApCsePerson, export.ApClient);
    MoveCsePersonsWorkSet9(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet5(useExport.CsePersonsWorkSet, export.Ar);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      local.Child.Item.GlocalChildCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet4(useExport.CsePersonsWorkSet,
      export.Child.Update.GexportCh);
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

  private void UseSiReadSpecificCaseRoles()
  {
    var useImport = new SiReadSpecificCaseRoles.Import();
    var useExport = new SiReadSpecificCaseRoles.Export();

    useImport.PageKey.Number = local.PageKeysCsePersonsWorkSet.Number;
    useImport.PageKeyStatus.Text1 = local.PageKeysWorkArea.Text1;
    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.Standard.PageNumber = export.HiddenPage.PageNumber;
    useImport.Case1.Number = export.Next.Number;

    Call(SiReadSpecificCaseRoles.Execute, useImport, useExport);

    local.PageKeysCsePersonsWorkSet.Number = useExport.NextPage.Number;
    local.PageKeysWorkArea.Text1 = useExport.NextPageStatus.Text1;
    useExport.Export1.CopyTo(local.Child, MoveExport1ToChild);
    export.HiddenPage.ScrollingMessage = useExport.Standard.ScrollingMessage;
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

  private bool ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetString(
          command, "cspNumber",
          local.Child.Item.GlocalChildCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 8);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 11);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 12);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 13);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetString(
          command, "cspNumber",
          local.Child.Item.GlocalChildCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 8);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 11);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 12);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 13);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetString(
          command, "cspNumber",
          local.Child.Item.GlocalChildCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 8);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 11);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 12);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 13);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private bool ReadCodeValue1()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "absenceReasonCode",
          export.Child.Item.GcaseRole.AbsenceReasonCode ?? "");
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

  private bool ReadCodeValue3()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue3",
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

  private bool ReadCodeValue4()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "relToAr", export.Child.Item.GcaseRole.RelToAr ?? "");
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

  private bool ReadCsePerson4()
  {
    entities.ChildCsePerson.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "numb", export.Child.Item.GexportCh.Number);
      },
      (db, reader) =>
      {
        entities.ChildCsePerson.Number = db.GetString(reader, 0);
        entities.ChildCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChildCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 2);
        entities.ChildCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildCsePerson.Type1);
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

  private bool ReadPersonProgramProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          local.Child.Item.GlocalChildCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 6);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.Program.Code = db.GetString(reader, 7);
        entities.Program.EffectiveDate = db.GetDate(reader, 8);
        entities.Program.DiscontinueDate = db.GetDate(reader, 9);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.ChildPersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Child.Item.GexportCh.Number);
        db.SetNullableDate(
          command, "coverBeginDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ChildPersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.ChildPersonalHealthInsurance.CspNumber =
          db.GetString(reader, 1);
        entities.ChildPersonalHealthInsurance.AlertFlagInsuranceExistsInd =
          db.GetNullableString(reader, 2);
        entities.ChildPersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 3);
        entities.ChildPersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 4);
        entities.ChildPersonalHealthInsurance.Populated = true;
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
      /// A value of GimportPageKeysCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gimportPageKeysCsePersonsWorkSet")]
      public CsePersonsWorkSet GimportPageKeysCsePersonsWorkSet
      {
        get => gimportPageKeysCsePersonsWorkSet ??= new();
        set => gimportPageKeysCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GimportPageKeysWorkArea.
      /// </summary>
      [JsonPropertyName("gimportPageKeysWorkArea")]
      public WorkArea GimportPageKeysWorkArea
      {
        get => gimportPageKeysWorkArea ??= new();
        set => gimportPageKeysWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet gimportPageKeysCsePersonsWorkSet;
      private WorkArea gimportPageKeysWorkArea;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of GimportStatus.
      /// </summary>
      [JsonPropertyName("gimportStatus")]
      public WorkArea GimportStatus
      {
        get => gimportStatus ??= new();
        set => gimportStatus = value;
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
      /// A value of GimportCh.
      /// </summary>
      [JsonPropertyName("gimportCh")]
      public CsePersonsWorkSet GimportCh
      {
        get => gimportCh ??= new();
        set => gimportCh = value;
      }

      /// <summary>
      /// A value of GcaseRole.
      /// </summary>
      [JsonPropertyName("gcaseRole")]
      public CaseRole GcaseRole
      {
        get => gcaseRole ??= new();
        set => gcaseRole = value;
      }

      /// <summary>
      /// A value of Gprogram.
      /// </summary>
      [JsonPropertyName("gprogram")]
      public Program Gprogram
      {
        get => gprogram ??= new();
        set => gprogram = value;
      }

      /// <summary>
      /// A value of GimportAbsenceRsn.
      /// </summary>
      [JsonPropertyName("gimportAbsenceRsn")]
      public CodeValue GimportAbsenceRsn
      {
        get => gimportAbsenceRsn ??= new();
        set => gimportAbsenceRsn = value;
      }

      /// <summary>
      /// A value of GimportRelToAr.
      /// </summary>
      [JsonPropertyName("gimportRelToAr")]
      public CodeValue GimportRelToAr
      {
        get => gimportRelToAr ??= new();
        set => gimportRelToAr = value;
      }

      /// <summary>
      /// A value of GimportChHealthInsuInd.
      /// </summary>
      [JsonPropertyName("gimportChHealthInsuInd")]
      public Common GimportChHealthInsuInd
      {
        get => gimportChHealthInsuInd ??= new();
        set => gimportChHealthInsuInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private WorkArea gimportStatus;
      private Common gcommon;
      private CsePersonsWorkSet gimportCh;
      private CaseRole gcaseRole;
      private Program gprogram;
      private CodeValue gimportAbsenceRsn;
      private CodeValue gimportRelToAr;
      private Common gimportChHealthInsuInd;
    }

    /// <summary>
    /// A value of SelectCh.
    /// </summary>
    [JsonPropertyName("selectCh")]
    public CsePersonsWorkSet SelectCh
    {
      get => selectCh ??= new();
      set => selectCh = value;
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
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
    }

    /// <summary>
    /// A value of HiddenPage.
    /// </summary>
    [JsonPropertyName("hiddenPage")]
    public Standard HiddenPage
    {
      get => hiddenPage ??= new();
      set => hiddenPage = value;
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

    private CsePersonsWorkSet selectCh;
    private Array<PageKeysGroup> pageKeys;
    private Array<ChildGroup> child;
    private Standard hiddenPage;
    private HealthInsuranceViability healthInsuranceViability;
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
      /// A value of GexportPageKeysCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gexportPageKeysCsePersonsWorkSet")]
      public CsePersonsWorkSet GexportPageKeysCsePersonsWorkSet
      {
        get => gexportPageKeysCsePersonsWorkSet ??= new();
        set => gexportPageKeysCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GexportPageKeysWorkArea.
      /// </summary>
      [JsonPropertyName("gexportPageKeysWorkArea")]
      public WorkArea GexportPageKeysWorkArea
      {
        get => gexportPageKeysWorkArea ??= new();
        set => gexportPageKeysWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet gexportPageKeysCsePersonsWorkSet;
      private WorkArea gexportPageKeysWorkArea;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of GexportStatus.
      /// </summary>
      [JsonPropertyName("gexportStatus")]
      public WorkArea GexportStatus
      {
        get => gexportStatus ??= new();
        set => gexportStatus = value;
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
      /// A value of GexportCh.
      /// </summary>
      [JsonPropertyName("gexportCh")]
      public CsePersonsWorkSet GexportCh
      {
        get => gexportCh ??= new();
        set => gexportCh = value;
      }

      /// <summary>
      /// A value of GcaseRole.
      /// </summary>
      [JsonPropertyName("gcaseRole")]
      public CaseRole GcaseRole
      {
        get => gcaseRole ??= new();
        set => gcaseRole = value;
      }

      /// <summary>
      /// A value of Gprogram.
      /// </summary>
      [JsonPropertyName("gprogram")]
      public Program Gprogram
      {
        get => gprogram ??= new();
        set => gprogram = value;
      }

      /// <summary>
      /// A value of GexportAbsenceRsn.
      /// </summary>
      [JsonPropertyName("gexportAbsenceRsn")]
      public CodeValue GexportAbsenceRsn
      {
        get => gexportAbsenceRsn ??= new();
        set => gexportAbsenceRsn = value;
      }

      /// <summary>
      /// A value of GexportRelToAr.
      /// </summary>
      [JsonPropertyName("gexportRelToAr")]
      public CodeValue GexportRelToAr
      {
        get => gexportRelToAr ??= new();
        set => gexportRelToAr = value;
      }

      /// <summary>
      /// A value of GexportChHealthInsuInd.
      /// </summary>
      [JsonPropertyName("gexportChHealthInsuInd")]
      public Common GexportChHealthInsuInd
      {
        get => gexportChHealthInsuInd ??= new();
        set => gexportChHealthInsuInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private WorkArea gexportStatus;
      private Common gcommon;
      private CsePersonsWorkSet gexportCh;
      private CaseRole gcaseRole;
      private Program gprogram;
      private CodeValue gexportAbsenceRsn;
      private CodeValue gexportRelToAr;
      private Common gexportChHealthInsuInd;
    }

    /// <summary>
    /// A value of LocalApOpen.
    /// </summary>
    [JsonPropertyName("localApOpen")]
    public Common LocalApOpen
    {
      get => localApOpen ??= new();
      set => localApOpen = value;
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
    /// A value of SelectedChild.
    /// </summary>
    [JsonPropertyName("selectedChild")]
    public CsePerson SelectedChild
    {
      get => selectedChild ??= new();
      set => selectedChild = value;
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
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
    }

    /// <summary>
    /// A value of HiddenPage.
    /// </summary>
    [JsonPropertyName("hiddenPage")]
    public Standard HiddenPage
    {
      get => hiddenPage ??= new();
      set => hiddenPage = value;
    }

    /// <summary>
    /// A value of SelectCh.
    /// </summary>
    [JsonPropertyName("selectCh")]
    public CsePersonsWorkSet SelectCh
    {
      get => selectCh ??= new();
      set => selectCh = value;
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
    /// A value of ChOtherCasesPrompt.
    /// </summary>
    [JsonPropertyName("chOtherCasesPrompt")]
    public Common ChOtherCasesPrompt
    {
      get => chOtherCasesPrompt ??= new();
      set => chOtherCasesPrompt = value;
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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Common localApOpen;
    private CsePersonsWorkSet fromComp;
    private CsePerson selectedChild;
    private Array<PageKeysGroup> pageKeys;
    private Array<ChildGroup> child;
    private Standard hiddenPage;
    private CsePersonsWorkSet selectCh;
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
    private Common chOtherCasesPrompt;
    private Common chOtherChPrompt;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of GlocalChildCommon.
      /// </summary>
      [JsonPropertyName("glocalChildCommon")]
      public Common GlocalChildCommon
      {
        get => glocalChildCommon ??= new();
        set => glocalChildCommon = value;
      }

      /// <summary>
      /// A value of GlocalChildWorkArea.
      /// </summary>
      [JsonPropertyName("glocalChildWorkArea")]
      public WorkArea GlocalChildWorkArea
      {
        get => glocalChildWorkArea ??= new();
        set => glocalChildWorkArea = value;
      }

      /// <summary>
      /// A value of GlocalChildCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("glocalChildCsePersonsWorkSet")]
      public CsePersonsWorkSet GlocalChildCsePersonsWorkSet
      {
        get => glocalChildCsePersonsWorkSet ??= new();
        set => glocalChildCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common glocalChildCommon;
      private WorkArea glocalChildWorkArea;
      private CsePersonsWorkSet glocalChildCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of MultiAps.
    /// </summary>
    [JsonPropertyName("multiAps")]
    public Common MultiAps
    {
      get => multiAps ??= new();
      set => multiAps = value;
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
    /// A value of GlocalNull.
    /// </summary>
    [JsonPropertyName("glocalNull")]
    public CaseRole GlocalNull
    {
      get => glocalNull ??= new();
      set => glocalNull = value;
    }

    /// <summary>
    /// A value of PageKeysCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("pageKeysCsePersonsWorkSet")]
    public CsePersonsWorkSet PageKeysCsePersonsWorkSet
    {
      get => pageKeysCsePersonsWorkSet ??= new();
      set => pageKeysCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PageKeysWorkArea.
    /// </summary>
    [JsonPropertyName("pageKeysWorkArea")]
    public WorkArea PageKeysWorkArea
    {
      get => pageKeysWorkArea ??= new();
      set => pageKeysWorkArea = value;
    }

    /// <summary>
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
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

    private Common multiAps;
    private Common caseOpen;
    private CaseRole glocalNull;
    private CsePersonsWorkSet pageKeysCsePersonsWorkSet;
    private WorkArea pageKeysWorkArea;
    private Array<ChildGroup> child;
    private CaseRole caseRole;
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
    private Common select;
    private Common noAps;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CaseRole Child1
    {
      get => child1 ??= new();
      set => child1 = value;
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
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of ChildHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("childHealthInsuranceCoverage")]
    public HealthInsuranceCoverage ChildHealthInsuranceCoverage
    {
      get => childHealthInsuranceCoverage ??= new();
      set => childHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of ChildPersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("childPersonalHealthInsurance")]
    public PersonalHealthInsurance ChildPersonalHealthInsurance
    {
      get => childPersonalHealthInsurance ??= new();
      set => childPersonalHealthInsurance = value;
    }

    /// <summary>
    /// A value of ChildHlthInsurance.
    /// </summary>
    [JsonPropertyName("childHlthInsurance")]
    public CsePerson ChildHlthInsurance
    {
      get => childHlthInsurance ??= new();
      set => childHlthInsurance = value;
    }

    private PersonProgram personProgram;
    private Program program;
    private CaseRole child1;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePersonAddress csePersonAddress;
    private Code code;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson childCsePerson;
    private HealthInsuranceCoverage childHealthInsuranceCoverage;
    private PersonalHealthInsurance childPersonalHealthInsurance;
    private CsePerson childHlthInsurance;
  }
#endregion
}

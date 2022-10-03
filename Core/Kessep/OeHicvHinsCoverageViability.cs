// Program: OE_HICV_HINS_COVERAGE_VIABILITY, ID: 371850192, model: 746.
// Short name: SWEHICVP
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
/// A program: OE_HICV_HINS_COVERAGE_VIABILITY.
/// </para>
/// <para>
/// Resp: OBLGEST
/// This procedure(PRAD) displays all of the information that facilitates a 
/// determination of viability to seek Health Insurance coverage obligation and
/// allows ADD, UPDATE, and DELETE capability.
/// This Procedure accepts a CSE_PERSON as input and displays scrolling for all 
/// children involved.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHicvHinsCoverageViability: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICV_HINS_COVERAGE_VIABILITY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicvHinsCoverageViability(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicvHinsCoverageViability.
  /// </summary>
  public OeHicvHinsCoverageViability(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE 		DESCRIPTION
    // Rebecca Grimes	01/02/95	Initial Code
    // Govinderaj 	05/17/95	Enhancement
    // Sid		02/01/95	Rework/Complete
    // T.O.Redmond	02/15/96	Retrofit
    // T.O.Redmond	03/15/96	Complete Logic for Notes and Income Monthly Amount. 
    // Correct Logic for Next and Previous Child.
    // R. Marchman     10/14/96        Retrofit with data level security.
    // S Chowdhary	06/16/97	Display closed cases.
    // ******** END MAINTENANCE LOG ****************
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // *********************************************
    // ****************************************************************************************
    // G. Pan     11/27/2007   CQ469
    // 			Added a new Group View (100) for group_import_active_child
    //                         and group_export_active_child with three 
    // attributes -
    //                         1. id from case_role
    //                         2. number from child cse_person
    //                         3. flag for active/inactive child.
    //                         Pass this group View when calls 
    // oe_hicv_display_hins_cov_viablty.
    //                         Added work view for import_child_count and 
    // export_child_count.
    //                         Added Identifier attribute to child entity view.
    //                         Added logic for CHPV (previous) and CHNx (next) 
    // before calling
    //                         display action block.
    // *****************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.Case1.Number = import.Case1.Number;
      export.ApCsePerson.Number = import.ApCsePerson.Number;
      MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet,
        export.ApCsePersonsWorkSet);
      export.ArCsePerson.Number = import.ArCsePerson.Number;
      export.ArCsePersonsWorkSet.FormattedName =
        import.ArCsePersonsWorkSet.FormattedName;
      export.Next.Number = import.Next.Number;
      export.ServiceProvider.LastName = import.ServiceProvider.LastName;
      MoveOffice(import.Office, export.Office);

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "RETOPAY"))
    {
      global.Command = "DISPLAY";
    }

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    export.Case1.Number = import.Case1.Number;
    export.ApCsePerson.Number = import.ApCsePerson.Number;
    MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet, export.ApCsePersonsWorkSet);
      
    export.ArCsePerson.Number = import.ArCsePerson.Number;
    export.ArCsePersonsWorkSet.FormattedName =
      import.ArCsePersonsWorkSet.FormattedName;
    export.ChildCsePerson.Number = import.ChildCsePerson.Number;
    export.ChildCsePersonsWorkSet.FormattedName =
      import.ChildCsePersonsWorkSet.FormattedName;
    export.Child.Assign(import.Child);
    export.HealthInsuranceViability.Assign(import.HealthInsuranceViability);
    export.LegalAction.CourtCaseNumber = import.LegalAction.CourtCaseNumber;
    MoveLegalActionDetail2(import.HinsCovOblg, export.HinsCovOblg);
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    export.HiddenPrevious.Number = import.HiddenPrevious.Number;
    export.Prompt.SelectChar = import.Prompt.SelectChar;
    export.Hap.Number = import.Hap.Number;
    export.MonthlyIncome.TotalCurrency = import.MonthlyIncome.TotalCurrency;
    export.Next.Number = import.Next.Number;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.ActiveChild1.Flag = import.ActiveChild1.Flag;
    export.NoOfChildrenNeedIns.Count = import.NoOfChildrenNeedIns.Count;

    // ****************************************************************************************
    // G. Pan     11/27/2007   CQ469
    // ****************************************************************************************
    export.ChildCount.Count = import.ChildCount.Count;

    for(import.ActiveChild.Index = 0; import.ActiveChild.Index < import
      .ActiveChild.Count; ++import.ActiveChild.Index)
    {
      if (!import.ActiveChild.CheckSize())
      {
        break;
      }

      export.ActiveChild.Index = import.ActiveChild.Index;
      export.ActiveChild.CheckSize();

      export.ActiveChild.Update.ActiveChildId.Identifier =
        import.ActiveChild.Item.ActiveChildId.Identifier;
      export.ActiveChild.Update.ActiveChildNum.Number =
        import.ActiveChild.Item.ActiveChildNum.Number;
      export.ActiveChild.Update.ActiveChild1.Flag =
        import.ActiveChild.Item.ActiveChild1.Flag;
    }

    import.ActiveChild.CheckIndex();
    UseOeCabSetMnemonics();

    if (!IsEmpty(export.Case1.Number))
    {
      local.TextWorkArea.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.Next.Number))
    {
      local.TextWorkArea.Text10 = export.Next.Number;
      UseEabPadLeftWithZeros();
      export.Next.Number = local.TextWorkArea.Text10;
    }

    // ************************************************
    // *Set Hidden Previous for first time display    *
    // ************************************************
    if (IsEmpty(export.HiddenPrevious.Number) && Equal
      (global.Command, "DISPLAY"))
    {
      export.HiddenPrevious.Number = export.Case1.Number;
      export.HiddenPrevUserAction.Command = global.Command;
    }

    if (!import.ChildProgram.IsEmpty)
    {
      export.ChildProgram.Index = 0;
      export.ChildProgram.Clear();

      for(import.ChildProgram.Index = 0; import.ChildProgram.Index < import
        .ChildProgram.Count; ++import.ChildProgram.Index)
      {
        if (export.ChildProgram.IsFull)
        {
          break;
        }

        export.ChildProgram.Update.Detail.Code =
          import.ChildProgram.Item.Detail.Code;
        export.ChildProgram.Next();
      }
    }

    if (!import.AvailHins.IsEmpty)
    {
      export.AvailHins.Index = 0;
      export.AvailHins.Clear();

      for(import.AvailHins.Index = 0; import.AvailHins.Index < import
        .AvailHins.Count; ++import.AvailHins.Index)
      {
        if (export.AvailHins.IsFull)
        {
          break;
        }

        export.AvailHins.Update.DetailAvailHinsHealthInsuranceCoverage.Assign(
          import.AvailHins.Item.DetailAvailHinsHealthInsuranceCoverage);
        export.AvailHins.Update.DetailAvailHinsPersonalHealthInsurance.Assign(
          import.AvailHins.Item.DetailAvailHinsPersonalHealthInsurance);
        export.AvailHins.Update.DetailPolicyHoldr.Number =
          import.AvailHins.Item.DetailPolicyHoldr.Number;
        export.AvailHins.Next();
      }
    }

    if (!import.CurrentOblg.IsEmpty)
    {
      export.CurrentOblg.Index = 0;
      export.CurrentOblg.Clear();

      for(import.CurrentOblg.Index = 0; import.CurrentOblg.Index < import
        .CurrentOblg.Count; ++import.CurrentOblg.Index)
      {
        if (export.CurrentOblg.IsFull)
        {
          break;
        }

        export.CurrentOblg.Update.DetailCurrOblgLegalActionDetail.Assign(
          import.CurrentOblg.Item.DetailCurrOblgLegalActionDetail);
        export.CurrentOblg.Update.DetailCurrOblgObligationType.Code =
          import.CurrentOblg.Item.DetailCurrOblgObligationType.Code;
        export.CurrentOblg.Next();
      }
    }

    export.Notes.Index = -1;

    for(import.Notes.Index = 0; import.Notes.Index < import.Notes.Count; ++
      import.Notes.Index)
    {
      ++export.Notes.Index;
      export.Notes.CheckSize();

      export.Notes.Update.Detail.Identifier =
        import.Notes.Item.Detail.Identifier;
      export.Notes.Update.Detail.Note = import.Notes.Item.Detail.Note ?? "";
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      if (IsEmpty(export.Case1.Number))
      {
        export.Hidden.CaseNumber = export.Next.Number;
      }
      else
      {
        export.Hidden.CaseNumber = export.Case1.Number;
        export.Hidden.CourtCaseNumber = export.LegalAction.CourtCaseNumber ?? ""
          ;
        export.Hidden.CsePersonNumber = export.ApCsePerson.Number;
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
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      if (!IsEmpty(import.SelectedAp.Number))
      {
        export.ApCsePerson.Number = import.SelectedAp.Number;
        MoveCsePersonsWorkSet(import.SelectedAp, export.ApCsePersonsWorkSet);
      }

      export.Prompt.SelectChar = "";
      local.Common.Flag = "Y";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }

      if (AsChar(export.ActiveChild1.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_INACTIVE_CHILD";

        return;
      }
    }

    if (Equal(global.Command, "CHPV") || Equal(global.Command, "CHNX") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      local.UserAction.Command = global.Command;

      if (!Equal(export.Case1.Number, export.HiddenPrevious.Number))
      {
        export.ChildCsePerson.Number = "";
        export.ApCsePerson.Number = "";
      }

      // ***************************************************************
      // Clear all view before displaying
      // ***************************************************************
      if (Equal(global.Command, "DISPLAY"))
      {
        MoveOffice(local.RefreshOffice, export.Office);
        export.ServiceProvider.LastName = local.RefreshServiceProvider.LastName;
        MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
        export.ArCsePersonsWorkSet.FormattedName =
          local.RefreshCsePersonsWorkSet.FormattedName;
        export.ChildCsePersonsWorkSet.FormattedName =
          local.RefreshCsePersonsWorkSet.FormattedName;
        export.CsePerson.Number = local.RefreshCsePerson.Number;
        export.ArCsePerson.Number = local.RefreshCsePerson.Number;
        export.ChildCsePerson.Number = local.RefreshCsePerson.Number;
        export.Hap.Number = local.RefreshCsePerson.Number;
        export.ActiveChild1.Flag = local.RefreshCommon.Flag;
        export.CaseOpen.Flag = local.RefreshCommon.Flag;
        export.CourtOrderOblPrompt.SelectChar = local.RefreshCommon.SelectChar;
        export.HiddenDisplayPerformed.Flag = local.RefreshCommon.Flag;
        export.HiddenPrevUserAction.Command = local.RefreshCommon.Command;
        export.MonthlyIncome.TotalCurrency = local.RefreshCommon.TotalCurrency;
        export.NoOfChildrenNeedIns.Count = local.RefreshCommon.Count;
        export.Prompt.SelectChar = local.RefreshCommon.SelectChar;

        for(export.ChildProgram.Index = 0; export.ChildProgram.Index < export
          .ChildProgram.Count; ++export.ChildProgram.Index)
        {
          export.ChildProgram.Update.Detail.Code = local.RefreshProgram.Code;
        }

        MoveCaseRole(local.RefreshChild, export.Child);
        export.LegalAction.CourtCaseNumber =
          local.RefreshLegalAction.CourtCaseNumber;
        MoveLegalActionDetail2(local.RefreshLegalActionDetail,
          export.HinsCovOblg);

        for(export.AvailHins.Index = 0; export.AvailHins.Index < export
          .AvailHins.Count; ++export.AvailHins.Index)
        {
          export.AvailHins.Update.DetailPolicyHoldr.Number =
            local.RefreshCsePerson.Number;
          MoveHealthInsuranceCoverage(local.RefreshHealthInsuranceCoverage,
            export.AvailHins.Update.DetailAvailHinsHealthInsuranceCoverage);
          export.AvailHins.Update.DetailAvailHinsPersonalHealthInsurance.Assign(
            local.RefreshPersonalHealthInsurance);
        }

        for(export.CurrentOblg.Index = 0; export.CurrentOblg.Index < export
          .CurrentOblg.Count; ++export.CurrentOblg.Index)
        {
          export.CurrentOblg.Update.DetailCurrOblgObligationType.Code =
            local.RefreshObligationType.Code;
          MoveLegalActionDetail1(local.RefreshLegalActionDetail,
            export.CurrentOblg.Update.DetailCurrOblgLegalActionDetail);
        }

        export.HealthInsuranceViability.Assign(
          local.RefreshHealthInsuranceViability);
        export.Notes.Count = 0;
      }

      if (IsEmpty(export.Case1.Number))
      {
        if (IsEmpty(export.Next.Number))
        {
          ExitState = "CASE_NUMBER_REQUIRED";

          var field = GetField(export.Case1, "number");

          field.Error = true;

          return;
        }

        export.Case1.Number = export.Next.Number;
      }

      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        return;
      }

      // ****************************************************************************************
      // G. Pan     11/27/2007   CQ469
      // ****************************************************************************************
      if (Equal(global.Command, "CHPV"))
      {
        if (export.ChildCount.Count <= 1)
        {
          ExitState = "OE0119_NO_MORE_CHILD";

          goto Test;
        }

        --export.ChildCount.Count;

        export.ActiveChild.Index = export.ChildCount.Count - 1;
        export.ActiveChild.CheckSize();

        export.ChildCsePerson.Number =
          export.ActiveChild.Item.ActiveChildNum.Number;
        export.Child.Identifier =
          export.ActiveChild.Item.ActiveChildId.Identifier;
      }

      if (Equal(global.Command, "CHNX"))
      {
        if (export.ChildCount.Count >= import.ActiveChild.Count)
        {
          ExitState = "OE0119_NO_MORE_CHILD";

          goto Test;
        }

        ++export.ChildCount.Count;

        export.ActiveChild.Index = export.ChildCount.Count - 1;
        export.ActiveChild.CheckSize();

        export.ChildCsePerson.Number =
          export.ActiveChild.Item.ActiveChildNum.Number;
        export.Child.Identifier =
          export.ActiveChild.Item.ActiveChildId.Identifier;
      }

      // ****************************************************************************************
      // G. Pan     11/27/2007   CQ469  Ended
      // ****************************************************************************************
      UseOeHicvDisplayHinsCovViablty();

      if (IsExitState("CASE_NF"))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        return;
      }

      if (IsExitState("CO0000_ABSENT_PARENT_NF"))
      {
        var field = GetField(export.ApCsePerson, "number");

        field.Error = true;

        return;
      }

      if (IsExitState("ECO_LNK_TO_CASE_COMPOSITION"))
      {
        export.HiddenPrevious.Number = export.Case1.Number;

        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(export.HinsCovOblg.EndDate, local.MaxDate.ExpirationDate))
        {
          export.HinsCovOblg.EndDate = null;
        }

        export.CourtOrderOblPrompt.SelectChar = "";

        for(export.CurrentOblg.Index = 0; export.CurrentOblg.Index < export
          .CurrentOblg.Count; ++export.CurrentOblg.Index)
        {
          ++local.ObligationCount.Count;
        }

        if (local.ObligationCount.Count > 6)
        {
          export.CourtOrderOblPrompt.SelectChar = "+";
        }

        for(export.AvailHins.Index = 0; export.AvailHins.Index < export
          .AvailHins.Count; ++export.AvailHins.Index)
        {
          if (Equal(export.AvailHins.Item.
            DetailAvailHinsHealthInsuranceCoverage.PolicyExpirationDate,
            local.MaxDate.ExpirationDate))
          {
            export.AvailHins.Update.DetailAvailHinsHealthInsuranceCoverage.
              PolicyExpirationDate = null;
          }

          if (Equal(export.AvailHins.Item.
            DetailAvailHinsPersonalHealthInsurance.CoverageEndDate,
            local.MaxDate.ExpirationDate))
          {
            export.AvailHins.Update.DetailAvailHinsPersonalHealthInsurance.
              CoverageEndDate = null;
          }
        }

        export.HiddenPrevious.Number = export.Case1.Number;
        export.Next.Number = export.Case1.Number;
        export.Hap.Number = export.ApCsePerson.Number;
        export.HiddenPrevUserAction.Command = global.Command;
        export.HiddenDisplayPerformed.Flag = "Y";
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        if (AsChar(export.ActiveChild1.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_INACTIVE_CHILD";
        }

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }

        if (AsChar(local.Common.Flag) == 'Y')
        {
          var field =
            GetField(export.HealthInsuranceViability, "hinsViableInd");

          field.Protected = false;
          field.Focused = true;
        }

        return;
      }
      else
      {
        export.HiddenDisplayPerformed.Flag = "N";
      }
    }

Test:

    if (Equal(global.Command, "RETOPAY") || Equal(global.Command, "CHPV") || Equal
      (global.Command, "CHNX") || Equal(global.Command, "OPAY"))
    {
      // Action Level Security bypasses PREV and NEXT
      // We are using CHPV and CHNX in the same mode.
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

    // ************************************************
    // *Action has already been taken for the commands*
    // *Exit, Return, Signoff, Display, CHPV, and CHNX*
    // ************************************************
    if (Equal(global.Command, "CHPV") || Equal(global.Command, "CHNX") || Equal
      (global.Command, "SIGNOFF") || Equal(global.Command, "EXIT") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "RETURN") || Equal
      (global.Command, "RETOPAY"))
    {
      export.HiddenPrevUserAction.Command = global.Command;
      export.HiddenPrevious.Number = export.Case1.Number;

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (AsChar(export.Prompt.SelectChar) != 'S')
        {
          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";

          return;
        }

        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        break;
      case "OPAY":
        export.ApCsePersonsWorkSet.Number = export.ApCsePerson.Number;
        ExitState = "ECO_LNK_TO_LST_OBLIGATION_DETAIL";

        return;
      case "ADD":
        if (!Equal(export.Case1.Number, export.HiddenPrevious.Number) || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y')
        {
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "OE0120_DISP_HINS_VIAB_BEF_ADD";

          return;
        }

        if (IsEmpty(export.ApCsePerson.Number))
        {
          var field = GetField(export.ApCsePerson, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NO_REQUIRED";
        }

        if (AsChar(export.HealthInsuranceViability.HinsViableInd) != 'Y' && AsChar
          (export.HealthInsuranceViability.HinsViableInd) != 'N' && !
          IsEmpty(export.HealthInsuranceViability.HinsViableInd))
        {
          var field =
            GetField(export.HealthInsuranceViability, "hinsViableInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0121_INVALID_HINS_VIABILITY";
          }
        }

        if (IsEmpty(export.HealthInsuranceViability.HinsViableInd))
        {
          var field =
            GetField(export.HealthInsuranceViability, "hinsViableInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        local.Common.Flag = "N";

        for(export.Notes.Index = 0; export.Notes.Index < export.Notes.Count; ++
          export.Notes.Index)
        {
          if (!export.Notes.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Notes.Item.Detail.Note))
          {
            local.Common.Flag = "Y";

            break;
          }
        }

        export.Notes.CheckIndex();

        if (AsChar(local.Common.Flag) == 'N')
        {
          export.Notes.Count = Export.NotesGroup.Capacity;

          for(export.Notes.Index = 0; export.Notes.Index < export.Notes.Count; ++
            export.Notes.Index)
          {
            if (!export.Notes.CheckSize())
            {
              break;
            }

            var field = GetField(export.Notes.Item.Detail, "note");

            field.Error = true;
          }

          export.Notes.CheckIndex();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeHicvCreateHinsCovViablty();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        if (!Equal(export.Case1.Number, export.HiddenPrevious.Number) || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        if (!Equal(export.ApCsePerson.Number, export.Hap.Number))
        {
          var field = GetField(export.ApCsePerson, "number");

          field.Error = true;

          ExitState = "PERSON_HAS_CHANGED_MUST_DISPLAY";

          return;
        }

        if (AsChar(export.HealthInsuranceViability.HinsViableInd) != 'Y' && AsChar
          (export.HealthInsuranceViability.HinsViableInd) != 'N' && !
          IsEmpty(export.HealthInsuranceViability.HinsViableInd))
        {
          var field =
            GetField(export.HealthInsuranceViability, "hinsViableInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0121_INVALID_HINS_VIABILITY";
          }
        }

        if (IsEmpty(export.HealthInsuranceViability.HinsViableInd))
        {
          var field =
            GetField(export.HealthInsuranceViability, "hinsViableInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        local.Common.Flag = "N";

        for(export.Notes.Index = 0; export.Notes.Index < export.Notes.Count; ++
          export.Notes.Index)
        {
          if (!export.Notes.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Notes.Item.Detail.Note))
          {
            local.Common.Flag = "Y";

            break;
          }
        }

        export.Notes.CheckIndex();

        if (AsChar(local.Common.Flag) == 'N')
        {
          export.Notes.Count = Export.NotesGroup.Capacity;

          for(export.Notes.Index = 0; export.Notes.Index < export.Notes.Count; ++
            export.Notes.Index)
          {
            if (!export.Notes.CheckSize())
            {
              break;
            }

            var field = GetField(export.Notes.Item.Detail, "note");

            field.Error = true;
          }

          export.Notes.CheckIndex();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeHicvUpdateHinsCovViablty();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          if (IsExitState("OE0121_INVALID_HINS_VIABILITY"))
          {
            var field =
              GetField(export.HealthInsuranceViability, "hinsViableInd");

            field.Error = true;

            return;
          }
        }
        else
        {
          export.Notes.Index = -1;

          for(import.Notes.Index = 0; import.Notes.Index < import.Notes.Count; ++
            import.Notes.Index)
          {
            ++export.Notes.Index;
            export.Notes.CheckSize();

            export.Notes.Update.Detail.Identifier =
              import.Notes.Item.Detail.Identifier;
            export.Notes.Update.Detail.Identifier =
              import.Notes.Item.Detail.Identifier;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        if (!Equal(export.Case1.Number, export.HiddenPrevious.Number) || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        UseOeHicvDeleteHinsCovViablty();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
        else
        {
          export.HealthInsuranceViability.Assign(
            local.RefreshHealthInsuranceViability);
          export.Notes.Count = 0;
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    export.HiddenPrevUserAction.Command = global.Command;
    export.HiddenPrevious.Number = export.Case1.Number;
    export.Hap.Number = export.ApCsePerson.Number;
  }

  private static void MoveActiveChild1(OeHicvDisplayHinsCovViablty.Export.
    ActiveChildGroup source, Export.ActiveChildGroup target)
  {
    target.ActiveChildId.Identifier = source.ActiveChildId.Identifier;
    target.ActiveChildNum.Number = source.ActiveChildNum.Number;
    target.ActiveChild1.Flag = source.ActiveChild1.Flag;
  }

  private static void MoveActiveChild2(Export.ActiveChildGroup source,
    OeHicvDisplayHinsCovViablty.Import.ActiveChildGroup target)
  {
    target.ActiveChildId.Identifier = source.ActiveChildId.Identifier;
    target.ActiveChildNum.Number = source.ActiveChildNum.Number;
    target.ActiveChild1.Flag = source.ActiveChild1.Flag;
  }

  private static void MoveAvailHins(OeHicvDisplayHinsCovViablty.Export.
    AvailHinsGroup source, Export.AvailHinsGroup target)
  {
    target.DetailPolicyHoldr.Number = source.DetailPolicyHoldr.Number;
    target.DetailAvailHinsPersonalHealthInsurance.Assign(
      source.DetailAvailHinsPersonalHealthInsurance);
    target.DetailAvailHinsHealthInsuranceCoverage.Assign(
      source.DetailAvailHinsHealthInsuranceCoverage);
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.PriorMedicalSupport = source.PriorMedicalSupport;
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.DateOfEmancipation = source.DateOfEmancipation;
    target.Over18AndInSchool = source.Over18AndInSchool;
  }

  private static void MoveChildsProgramToChildProgram(
    OeHicvDisplayHinsCovViablty.Export.ChildsProgramGroup source,
    Export.ChildProgramGroup target)
  {
    target.Detail.Code = source.DetailChilds.Code;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCurrentOblg(OeHicvDisplayHinsCovViablty.Export.
    CurrentOblgGroup source, Export.CurrentOblgGroup target)
  {
    target.DetailCurrOblgObligationType.Code =
      source.DetailCurrentOblgObligationType.Code;
    target.DetailCurrOblgLegalActionDetail.Assign(
      source.DetailCurrentOblgLegalActionDetail);
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.PolicyPaidByCsePersonInd = source.PolicyPaidByCsePersonInd;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
  }

  private static void MoveHealthInsuranceViability(
    HealthInsuranceViability source, HealthInsuranceViability target)
  {
    target.Identifier = source.Identifier;
    target.HinsViableInd = source.HinsViableInd;
  }

  private static void MoveHicvNoteToNotes1(OeHicvDisplayHinsCovViablty.Export.
    HicvNoteGroup source, Export.NotesGroup target)
  {
    target.Detail.Note = source.Detail.Note;
  }

  private static void MoveHicvNoteToNotes2(OeHicvDeleteHinsCovViablty.Export.
    HicvNoteGroup source, Export.NotesGroup target)
  {
    MoveHinsViabNote(source.Detail, target.Detail);
  }

  private static void MoveHinsViabNote(HinsViabNote source, HinsViabNote target)
  {
    target.Identifier = source.Identifier;
    target.Note = source.Note;
  }

  private static void MoveLegalActionDetail1(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.Number = source.Number;
    target.DetailType = source.DetailType;
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
    target.CurrentAmount = source.CurrentAmount;
  }

  private static void MoveLegalActionDetail2(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
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

  private static void MoveNotesToHicvNote(Export.NotesGroup source,
    OeHicvDeleteHinsCovViablty.Import.HicvNoteGroup target)
  {
    MoveHinsViabNote(source.Detail, target.Detail);
  }

  private static void MoveNotesToImport2(Export.NotesGroup source,
    OeHicvUpdateHinsCovViablty.Import.ImportGroup target)
  {
    MoveHinsViabNote(source.Detail, target.HinsViabNote);
  }

  private static void MoveNotesToImport3(Export.NotesGroup source,
    OeHicvCreateHinsCovViablty.Import.ImportGroup target)
  {
    MoveHinsViabNote(source.Detail, target.HinsViabNote);
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseOeHicvCreateHinsCovViablty()
  {
    var useImport = new OeHicvCreateHinsCovViablty.Import();
    var useExport = new OeHicvCreateHinsCovViablty.Export();

    useImport.Ap.Number = import.ApCsePerson.Number;
    useImport.New1.HinsViableInd =
      export.HealthInsuranceViability.HinsViableInd;
    useImport.Child1.Number = export.ChildCsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Child2.StartDate = export.Child.StartDate;
    export.Notes.CopyTo(useImport.Import1, MoveNotesToImport3);

    Call(OeHicvCreateHinsCovViablty.Execute, useImport, useExport);

    export.HealthInsuranceViability.Assign(useExport.HealthInsuranceViability);
  }

  private void UseOeHicvDeleteHinsCovViablty()
  {
    var useImport = new OeHicvDeleteHinsCovViablty.Import();
    var useExport = new OeHicvDeleteHinsCovViablty.Export();

    export.Notes.CopyTo(useImport.HicvNote, MoveNotesToHicvNote);
    MoveHealthInsuranceViability(export.HealthInsuranceViability,
      useImport.HealthInsuranceViability);
    useImport.Child1.Number = export.ChildCsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Child2.StartDate = export.Child.StartDate;
    useImport.Ap.Number = export.ApCsePerson.Number;

    Call(OeHicvDeleteHinsCovViablty.Execute, useImport, useExport);

    export.HealthInsuranceViability.Assign(useExport.HealthInsuranceViability);
    useExport.HicvNote.CopyTo(export.Notes, MoveHicvNoteToNotes2);
  }

  private void UseOeHicvDisplayHinsCovViablty()
  {
    var useImport = new OeHicvDisplayHinsCovViablty.Import();
    var useExport = new OeHicvDisplayHinsCovViablty.Export();

    useImport.ChildCount.Count = export.ChildCount.Count;
    export.ActiveChild.CopyTo(useImport.ActiveChild, MoveActiveChild2);
    useImport.ChildCaseRole.Identifier = export.Child.Identifier;
    useImport.Ap.Number = export.ApCsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.ChildCsePerson.Number = export.ChildCsePerson.Number;
    useImport.UserAction.Command = local.UserAction.Command;

    Call(OeHicvDisplayHinsCovViablty.Execute, useImport, useExport);

    export.ChildCount.Count = useExport.ChildCount.Count;
    useExport.ActiveChild.CopyTo(export.ActiveChild, MoveActiveChild1);
    export.ActiveChild1.Flag = useExport.ActiveChild1.Flag;
    export.MonthlyIncome.TotalCurrency =
      useExport.MonthlyTotalIncome.TotalCurrency;
    useExport.HicvNote.CopyTo(export.Notes, MoveHicvNoteToNotes1);
    MoveLegalActionDetail2(useExport.HinsCovOblgLegalActionDetail,
      export.HinsCovOblg);
    export.NoOfChildrenNeedIns.Count = useExport.ChildrenNeedingCoverage.Count;
    export.ArCsePersonsWorkSet.FormattedName =
      useExport.ArCsePersonsWorkSet.FormattedName;
    export.ArCsePerson.Number = useExport.ArCsePerson.Number;
    MoveCsePersonsWorkSet(useExport.ApCsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
    export.ApCsePerson.Number = useExport.ApCsePerson.Number;
    export.Case1.Number = useExport.Case1.Number;
    export.ChildCsePersonsWorkSet.FormattedName =
      useExport.ChildCsePersonsWorkSet.FormattedName;
    export.ChildCsePerson.Number = useExport.ChildCsePerson.Number;
    MoveCaseRole(useExport.Child, export.Child);
    export.HealthInsuranceViability.Assign(useExport.HealthInsuranceViability);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    export.LegalAction.CourtCaseNumber =
      useExport.HinsCovOblgLegalAction.CourtCaseNumber;
    useExport.ChildsProgram.CopyTo(
      export.ChildProgram, MoveChildsProgramToChildProgram);
    useExport.AvailHins.CopyTo(export.AvailHins, MoveAvailHins);
    useExport.CurrentOblg.CopyTo(export.CurrentOblg, MoveCurrentOblg);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseOeHicvUpdateHinsCovViablty()
  {
    var useImport = new OeHicvUpdateHinsCovViablty.Import();
    var useExport = new OeHicvUpdateHinsCovViablty.Export();

    useImport.Ap.Number = export.ApCsePerson.Number;
    MoveHealthInsuranceViability(export.HealthInsuranceViability,
      useImport.HealthInsuranceViability);
    useImport.Child1.Number = export.ChildCsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Child2.StartDate = export.Child.StartDate;
    export.Notes.CopyTo(useImport.Import1, MoveNotesToImport2);

    Call(OeHicvUpdateHinsCovViablty.Execute, useImport, useExport);

    export.HealthInsuranceViability.Assign(useExport.HealthInsuranceViability);
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

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.LegalAction.CourtCaseNumber = export.LegalAction.CourtCaseNumber;
    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.ApCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A ActiveChildGroup group.</summary>
    [Serializable]
    public class ActiveChildGroup
    {
      /// <summary>
      /// A value of ActiveChildId.
      /// </summary>
      [JsonPropertyName("activeChildId")]
      public CaseRole ActiveChildId
      {
        get => activeChildId ??= new();
        set => activeChildId = value;
      }

      /// <summary>
      /// A value of ActiveChildNum.
      /// </summary>
      [JsonPropertyName("activeChildNum")]
      public CsePersonsWorkSet ActiveChildNum
      {
        get => activeChildNum ??= new();
        set => activeChildNum = value;
      }

      /// <summary>
      /// A value of ActiveChild1.
      /// </summary>
      [JsonPropertyName("activeChild1")]
      public Common ActiveChild1
      {
        get => activeChild1 ??= new();
        set => activeChild1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CaseRole activeChildId;
      private CsePersonsWorkSet activeChildNum;
      private Common activeChild1;
    }

    /// <summary>A CurrentOblgGroup group.</summary>
    [Serializable]
    public class CurrentOblgGroup
    {
      /// <summary>
      /// A value of DetailCurrOblgObligationType.
      /// </summary>
      [JsonPropertyName("detailCurrOblgObligationType")]
      public ObligationType DetailCurrOblgObligationType
      {
        get => detailCurrOblgObligationType ??= new();
        set => detailCurrOblgObligationType = value;
      }

      /// <summary>
      /// A value of DetailCurrOblgLegalActionDetail.
      /// </summary>
      [JsonPropertyName("detailCurrOblgLegalActionDetail")]
      public LegalActionDetail DetailCurrOblgLegalActionDetail
      {
        get => detailCurrOblgLegalActionDetail ??= new();
        set => detailCurrOblgLegalActionDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private ObligationType detailCurrOblgObligationType;
      private LegalActionDetail detailCurrOblgLegalActionDetail;
    }

    /// <summary>A AvailHinsGroup group.</summary>
    [Serializable]
    public class AvailHinsGroup
    {
      /// <summary>
      /// A value of DetailPolicyHoldr.
      /// </summary>
      [JsonPropertyName("detailPolicyHoldr")]
      public CsePerson DetailPolicyHoldr
      {
        get => detailPolicyHoldr ??= new();
        set => detailPolicyHoldr = value;
      }

      /// <summary>
      /// A value of DetailAvailHinsPersonalHealthInsurance.
      /// </summary>
      [JsonPropertyName("detailAvailHinsPersonalHealthInsurance")]
      public PersonalHealthInsurance DetailAvailHinsPersonalHealthInsurance
      {
        get => detailAvailHinsPersonalHealthInsurance ??= new();
        set => detailAvailHinsPersonalHealthInsurance = value;
      }

      /// <summary>
      /// A value of DetailAvailHinsHealthInsuranceCoverage.
      /// </summary>
      [JsonPropertyName("detailAvailHinsHealthInsuranceCoverage")]
      public HealthInsuranceCoverage DetailAvailHinsHealthInsuranceCoverage
      {
        get => detailAvailHinsHealthInsuranceCoverage ??= new();
        set => detailAvailHinsHealthInsuranceCoverage = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePerson detailPolicyHoldr;
      private PersonalHealthInsurance detailAvailHinsPersonalHealthInsurance;
      private HealthInsuranceCoverage detailAvailHinsHealthInsuranceCoverage;
    }

    /// <summary>A ChildProgramGroup group.</summary>
    [Serializable]
    public class ChildProgramGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Program Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Program detail;
    }

    /// <summary>A NotesGroup group.</summary>
    [Serializable]
    public class NotesGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public HinsViabNote Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private HinsViabNote detail;
    }

    /// <summary>
    /// A value of ChildCount.
    /// </summary>
    [JsonPropertyName("childCount")]
    public Common ChildCount
    {
      get => childCount ??= new();
      set => childCount = value;
    }

    /// <summary>
    /// Gets a value of ActiveChild.
    /// </summary>
    [JsonIgnore]
    public Array<ActiveChildGroup> ActiveChild => activeChild ??= new(
      ActiveChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ActiveChild for json serialization.
    /// </summary>
    [JsonPropertyName("activeChild")]
    [Computed]
    public IList<ActiveChildGroup> ActiveChild_Json
    {
      get => activeChild;
      set => ActiveChild.Assign(value);
    }

    /// <summary>
    /// A value of Hap.
    /// </summary>
    [JsonPropertyName("hap")]
    public CsePerson Hap
    {
      get => hap ??= new();
      set => hap = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
    }

    /// <summary>
    /// A value of MonthlyIncome.
    /// </summary>
    [JsonPropertyName("monthlyIncome")]
    public Common MonthlyIncome
    {
      get => monthlyIncome ??= new();
      set => monthlyIncome = value;
    }

    /// <summary>
    /// A value of CourtOrderOblPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderOblPrompt")]
    public Common CourtOrderOblPrompt
    {
      get => courtOrderOblPrompt ??= new();
      set => courtOrderOblPrompt = value;
    }

    /// <summary>
    /// A value of HinsCovOblg.
    /// </summary>
    [JsonPropertyName("hinsCovOblg")]
    public LegalActionDetail HinsCovOblg
    {
      get => hinsCovOblg ??= new();
      set => hinsCovOblg = value;
    }

    /// <summary>
    /// A value of NoOfChildrenNeedIns.
    /// </summary>
    [JsonPropertyName("noOfChildrenNeedIns")]
    public Common NoOfChildrenNeedIns
    {
      get => noOfChildrenNeedIns ??= new();
      set => noOfChildrenNeedIns = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public Case1 HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of ChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("childCsePersonsWorkSet")]
    public CsePersonsWorkSet ChildCsePersonsWorkSet
    {
      get => childCsePersonsWorkSet ??= new();
      set => childCsePersonsWorkSet = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// Gets a value of CurrentOblg.
    /// </summary>
    [JsonIgnore]
    public Array<CurrentOblgGroup> CurrentOblg => currentOblg ??= new(
      CurrentOblgGroup.Capacity);

    /// <summary>
    /// Gets a value of CurrentOblg for json serialization.
    /// </summary>
    [JsonPropertyName("currentOblg")]
    [Computed]
    public IList<CurrentOblgGroup> CurrentOblg_Json
    {
      get => currentOblg;
      set => CurrentOblg.Assign(value);
    }

    /// <summary>
    /// Gets a value of AvailHins.
    /// </summary>
    [JsonIgnore]
    public Array<AvailHinsGroup> AvailHins => availHins ??= new(
      AvailHinsGroup.Capacity);

    /// <summary>
    /// Gets a value of AvailHins for json serialization.
    /// </summary>
    [JsonPropertyName("availHins")]
    [Computed]
    public IList<AvailHinsGroup> AvailHins_Json
    {
      get => availHins;
      set => AvailHins.Assign(value);
    }

    /// <summary>
    /// Gets a value of ChildProgram.
    /// </summary>
    [JsonIgnore]
    public Array<ChildProgramGroup> ChildProgram => childProgram ??= new(
      ChildProgramGroup.Capacity);

    /// <summary>
    /// Gets a value of ChildProgram for json serialization.
    /// </summary>
    [JsonPropertyName("childProgram")]
    [Computed]
    public IList<ChildProgramGroup> ChildProgram_Json
    {
      get => childProgram;
      set => ChildProgram.Assign(value);
    }

    /// <summary>
    /// Gets a value of Notes.
    /// </summary>
    [JsonIgnore]
    public Array<NotesGroup> Notes => notes ??= new(NotesGroup.Capacity);

    /// <summary>
    /// Gets a value of Notes for json serialization.
    /// </summary>
    [JsonPropertyName("notes")]
    [Computed]
    public IList<NotesGroup> Notes_Json
    {
      get => notes;
      set => Notes.Assign(value);
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ActiveChild1.
    /// </summary>
    [JsonPropertyName("activeChild1")]
    public Common ActiveChild1
    {
      get => activeChild1 ??= new();
      set => activeChild1 = value;
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

    private Common childCount;
    private Array<ActiveChildGroup> activeChild;
    private CsePerson hap;
    private Common prompt;
    private CsePersonsWorkSet selectedAp;
    private Common monthlyIncome;
    private Common courtOrderOblPrompt;
    private LegalActionDetail hinsCovOblg;
    private Common noOfChildrenNeedIns;
    private Common hiddenDisplayPerformed;
    private Case1 hiddenPrevious;
    private Common hiddenPrevUserAction;
    private LegalAction legalAction;
    private ScrollingAttributes scrollingAttributes;
    private HealthInsuranceViability healthInsuranceViability;
    private CaseRole child;
    private CsePerson childCsePerson;
    private CsePersonsWorkSet childCsePersonsWorkSet;
    private Case1 case1;
    private CsePerson apCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson arCsePerson;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Array<CurrentOblgGroup> currentOblg;
    private Array<AvailHinsGroup> availHins;
    private Array<ChildProgramGroup> childProgram;
    private Array<NotesGroup> notes;
    private NextTranInfo hidden;
    private Standard standard;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private Common activeChild1;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ActiveChildGroup group.</summary>
    [Serializable]
    public class ActiveChildGroup
    {
      /// <summary>
      /// A value of ActiveChildId.
      /// </summary>
      [JsonPropertyName("activeChildId")]
      public CaseRole ActiveChildId
      {
        get => activeChildId ??= new();
        set => activeChildId = value;
      }

      /// <summary>
      /// A value of ActiveChildNum.
      /// </summary>
      [JsonPropertyName("activeChildNum")]
      public CsePersonsWorkSet ActiveChildNum
      {
        get => activeChildNum ??= new();
        set => activeChildNum = value;
      }

      /// <summary>
      /// A value of ActiveChild1.
      /// </summary>
      [JsonPropertyName("activeChild1")]
      public Common ActiveChild1
      {
        get => activeChild1 ??= new();
        set => activeChild1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CaseRole activeChildId;
      private CsePersonsWorkSet activeChildNum;
      private Common activeChild1;
    }

    /// <summary>A CurrentOblgGroup group.</summary>
    [Serializable]
    public class CurrentOblgGroup
    {
      /// <summary>
      /// A value of DetailCurrOblgObligationType.
      /// </summary>
      [JsonPropertyName("detailCurrOblgObligationType")]
      public ObligationType DetailCurrOblgObligationType
      {
        get => detailCurrOblgObligationType ??= new();
        set => detailCurrOblgObligationType = value;
      }

      /// <summary>
      /// A value of DetailCurrOblgLegalActionDetail.
      /// </summary>
      [JsonPropertyName("detailCurrOblgLegalActionDetail")]
      public LegalActionDetail DetailCurrOblgLegalActionDetail
      {
        get => detailCurrOblgLegalActionDetail ??= new();
        set => detailCurrOblgLegalActionDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private ObligationType detailCurrOblgObligationType;
      private LegalActionDetail detailCurrOblgLegalActionDetail;
    }

    /// <summary>A AvailHinsGroup group.</summary>
    [Serializable]
    public class AvailHinsGroup
    {
      /// <summary>
      /// A value of DetailPolicyHoldr.
      /// </summary>
      [JsonPropertyName("detailPolicyHoldr")]
      public CsePerson DetailPolicyHoldr
      {
        get => detailPolicyHoldr ??= new();
        set => detailPolicyHoldr = value;
      }

      /// <summary>
      /// A value of DetailAvailHinsPersonalHealthInsurance.
      /// </summary>
      [JsonPropertyName("detailAvailHinsPersonalHealthInsurance")]
      public PersonalHealthInsurance DetailAvailHinsPersonalHealthInsurance
      {
        get => detailAvailHinsPersonalHealthInsurance ??= new();
        set => detailAvailHinsPersonalHealthInsurance = value;
      }

      /// <summary>
      /// A value of DetailAvailHinsHealthInsuranceCoverage.
      /// </summary>
      [JsonPropertyName("detailAvailHinsHealthInsuranceCoverage")]
      public HealthInsuranceCoverage DetailAvailHinsHealthInsuranceCoverage
      {
        get => detailAvailHinsHealthInsuranceCoverage ??= new();
        set => detailAvailHinsHealthInsuranceCoverage = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePerson detailPolicyHoldr;
      private PersonalHealthInsurance detailAvailHinsPersonalHealthInsurance;
      private HealthInsuranceCoverage detailAvailHinsHealthInsuranceCoverage;
    }

    /// <summary>A ChildProgramGroup group.</summary>
    [Serializable]
    public class ChildProgramGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Program Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Program detail;
    }

    /// <summary>A NotesGroup group.</summary>
    [Serializable]
    public class NotesGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public HinsViabNote Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private HinsViabNote detail;
    }

    /// <summary>
    /// A value of ChildCount.
    /// </summary>
    [JsonPropertyName("childCount")]
    public Common ChildCount
    {
      get => childCount ??= new();
      set => childCount = value;
    }

    /// <summary>
    /// Gets a value of ActiveChild.
    /// </summary>
    [JsonIgnore]
    public Array<ActiveChildGroup> ActiveChild => activeChild ??= new(
      ActiveChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ActiveChild for json serialization.
    /// </summary>
    [JsonPropertyName("activeChild")]
    [Computed]
    public IList<ActiveChildGroup> ActiveChild_Json
    {
      get => activeChild;
      set => ActiveChild.Assign(value);
    }

    /// <summary>
    /// A value of Hap.
    /// </summary>
    [JsonPropertyName("hap")]
    public CsePerson Hap
    {
      get => hap ??= new();
      set => hap = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of MonthlyIncome.
    /// </summary>
    [JsonPropertyName("monthlyIncome")]
    public Common MonthlyIncome
    {
      get => monthlyIncome ??= new();
      set => monthlyIncome = value;
    }

    /// <summary>
    /// A value of CourtOrderOblPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderOblPrompt")]
    public Common CourtOrderOblPrompt
    {
      get => courtOrderOblPrompt ??= new();
      set => courtOrderOblPrompt = value;
    }

    /// <summary>
    /// A value of HinsCovOblg.
    /// </summary>
    [JsonPropertyName("hinsCovOblg")]
    public LegalActionDetail HinsCovOblg
    {
      get => hinsCovOblg ??= new();
      set => hinsCovOblg = value;
    }

    /// <summary>
    /// A value of NoOfChildrenNeedIns.
    /// </summary>
    [JsonPropertyName("noOfChildrenNeedIns")]
    public Common NoOfChildrenNeedIns
    {
      get => noOfChildrenNeedIns ??= new();
      set => noOfChildrenNeedIns = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public Case1 HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of ChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("childCsePersonsWorkSet")]
    public CsePersonsWorkSet ChildCsePersonsWorkSet
    {
      get => childCsePersonsWorkSet ??= new();
      set => childCsePersonsWorkSet = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// Gets a value of CurrentOblg.
    /// </summary>
    [JsonIgnore]
    public Array<CurrentOblgGroup> CurrentOblg => currentOblg ??= new(
      CurrentOblgGroup.Capacity);

    /// <summary>
    /// Gets a value of CurrentOblg for json serialization.
    /// </summary>
    [JsonPropertyName("currentOblg")]
    [Computed]
    public IList<CurrentOblgGroup> CurrentOblg_Json
    {
      get => currentOblg;
      set => CurrentOblg.Assign(value);
    }

    /// <summary>
    /// Gets a value of AvailHins.
    /// </summary>
    [JsonIgnore]
    public Array<AvailHinsGroup> AvailHins => availHins ??= new(
      AvailHinsGroup.Capacity);

    /// <summary>
    /// Gets a value of AvailHins for json serialization.
    /// </summary>
    [JsonPropertyName("availHins")]
    [Computed]
    public IList<AvailHinsGroup> AvailHins_Json
    {
      get => availHins;
      set => AvailHins.Assign(value);
    }

    /// <summary>
    /// Gets a value of ChildProgram.
    /// </summary>
    [JsonIgnore]
    public Array<ChildProgramGroup> ChildProgram => childProgram ??= new(
      ChildProgramGroup.Capacity);

    /// <summary>
    /// Gets a value of ChildProgram for json serialization.
    /// </summary>
    [JsonPropertyName("childProgram")]
    [Computed]
    public IList<ChildProgramGroup> ChildProgram_Json
    {
      get => childProgram;
      set => ChildProgram.Assign(value);
    }

    /// <summary>
    /// Gets a value of Notes.
    /// </summary>
    [JsonIgnore]
    public Array<NotesGroup> Notes => notes ??= new(NotesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Notes for json serialization.
    /// </summary>
    [JsonPropertyName("notes")]
    [Computed]
    public IList<NotesGroup> Notes_Json
    {
      get => notes;
      set => Notes.Assign(value);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ActiveChild1.
    /// </summary>
    [JsonPropertyName("activeChild1")]
    public Common ActiveChild1
    {
      get => activeChild1 ??= new();
      set => activeChild1 = value;
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

    private Common childCount;
    private Array<ActiveChildGroup> activeChild;
    private CsePerson hap;
    private Common prompt;
    private Common monthlyIncome;
    private Common courtOrderOblPrompt;
    private LegalActionDetail hinsCovOblg;
    private Common noOfChildrenNeedIns;
    private Common hiddenDisplayPerformed;
    private Case1 hiddenPrevious;
    private Common hiddenPrevUserAction;
    private LegalAction legalAction;
    private ScrollingAttributes scrollingAttributes;
    private HealthInsuranceViability healthInsuranceViability;
    private CaseRole child;
    private CsePerson childCsePerson;
    private CsePersonsWorkSet childCsePersonsWorkSet;
    private Case1 case1;
    private CsePerson apCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson arCsePerson;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Array<CurrentOblgGroup> currentOblg;
    private Array<AvailHinsGroup> availHins;
    private Array<ChildProgramGroup> childProgram;
    private Array<NotesGroup> notes;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson csePerson;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private Common activeChild1;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of ObligationCount.
    /// </summary>
    [JsonPropertyName("obligationCount")]
    public Common ObligationCount
    {
      get => obligationCount ??= new();
      set => obligationCount = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of RefreshOffice.
    /// </summary>
    [JsonPropertyName("refreshOffice")]
    public Office RefreshOffice
    {
      get => refreshOffice ??= new();
      set => refreshOffice = value;
    }

    /// <summary>
    /// A value of RefreshServiceProvider.
    /// </summary>
    [JsonPropertyName("refreshServiceProvider")]
    public ServiceProvider RefreshServiceProvider
    {
      get => refreshServiceProvider ??= new();
      set => refreshServiceProvider = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of RefreshCsePerson.
    /// </summary>
    [JsonPropertyName("refreshCsePerson")]
    public CsePerson RefreshCsePerson
    {
      get => refreshCsePerson ??= new();
      set => refreshCsePerson = value;
    }

    /// <summary>
    /// A value of RefreshCommon.
    /// </summary>
    [JsonPropertyName("refreshCommon")]
    public Common RefreshCommon
    {
      get => refreshCommon ??= new();
      set => refreshCommon = value;
    }

    /// <summary>
    /// A value of RefreshProgram.
    /// </summary>
    [JsonPropertyName("refreshProgram")]
    public Program RefreshProgram
    {
      get => refreshProgram ??= new();
      set => refreshProgram = value;
    }

    /// <summary>
    /// A value of RefreshChild.
    /// </summary>
    [JsonPropertyName("refreshChild")]
    public CaseRole RefreshChild
    {
      get => refreshChild ??= new();
      set => refreshChild = value;
    }

    /// <summary>
    /// A value of RefreshLegalAction.
    /// </summary>
    [JsonPropertyName("refreshLegalAction")]
    public LegalAction RefreshLegalAction
    {
      get => refreshLegalAction ??= new();
      set => refreshLegalAction = value;
    }

    /// <summary>
    /// A value of RefreshLegalActionDetail.
    /// </summary>
    [JsonPropertyName("refreshLegalActionDetail")]
    public LegalActionDetail RefreshLegalActionDetail
    {
      get => refreshLegalActionDetail ??= new();
      set => refreshLegalActionDetail = value;
    }

    /// <summary>
    /// A value of RefreshPersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("refreshPersonalHealthInsurance")]
    public PersonalHealthInsurance RefreshPersonalHealthInsurance
    {
      get => refreshPersonalHealthInsurance ??= new();
      set => refreshPersonalHealthInsurance = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCoverage")]
    public HealthInsuranceCoverage RefreshHealthInsuranceCoverage
    {
      get => refreshHealthInsuranceCoverage ??= new();
      set => refreshHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of RefreshObligationType.
    /// </summary>
    [JsonPropertyName("refreshObligationType")]
    public ObligationType RefreshObligationType
    {
      get => refreshObligationType ??= new();
      set => refreshObligationType = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceViability")]
    public HealthInsuranceViability RefreshHealthInsuranceViability
    {
      get => refreshHealthInsuranceViability ??= new();
      set => refreshHealthInsuranceViability = value;
    }

    /// <summary>
    /// A value of RefreshHinsViabNote.
    /// </summary>
    [JsonPropertyName("refreshHinsViabNote")]
    public HinsViabNote RefreshHinsViabNote
    {
      get => refreshHinsViabNote ??= new();
      set => refreshHinsViabNote = value;
    }

    private Common common;
    private Code maxDate;
    private Common obligationCount;
    private Common userAction;
    private TextWorkArea textWorkArea;
    private Office refreshOffice;
    private ServiceProvider refreshServiceProvider;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private CsePerson refreshCsePerson;
    private Common refreshCommon;
    private Program refreshProgram;
    private CaseRole refreshChild;
    private LegalAction refreshLegalAction;
    private LegalActionDetail refreshLegalActionDetail;
    private PersonalHealthInsurance refreshPersonalHealthInsurance;
    private HealthInsuranceCoverage refreshHealthInsuranceCoverage;
    private ObligationType refreshObligationType;
    private HealthInsuranceViability refreshHealthInsuranceViability;
    private HinsViabNote refreshHinsViabNote;
  }
#endregion
}

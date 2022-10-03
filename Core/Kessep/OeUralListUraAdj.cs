// Program: OE_URAL_LIST_URA_ADJ, ID: 374465628, model: 746.
// Short name: SWEURALP
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
/// A program: OE_URAL_LIST_URA_ADJ.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeUralListUraAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAL_LIST_URA_ADJ program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUralListUraAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUralListUraAdj.
  /// </summary>
  public OeUralListUraAdj(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************** MAINTENANCE LOG **********************
    //  AUTHOR         DATE         CHG REQ#       DESCRIPTION
    // Madhu Kumar   05/10/00                     Initial Code.
    // Fangman          08/08/00                 Corrected sort order, cleaned 
    // up views.
    // *************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

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

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ***************************************************************
    //          Move Import to Exports , enterable fields .
    // ***************************************************************
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.ImHousehold.AeCaseNo = import.ImHousehold.AeCaseNo;
    export.CsePersonCsePerson.Assign(import.CsePerson1);
    export.ImHouseholdMbrMnthlySum.Relationship =
      import.ImHouseholdMbrMnthlySum.Relationship;
    export.CsePersonStandard.PromptField = import.CsePerson2.PromptField;
    export.ListAdjsForAllMbrs.Flag = import.ListAdjsForAllMbrs.Flag;
    export.FromYearNMonth.Assign(import.FromYearNMonth);
    export.ToYearNMonth.Assign(import.ToYearNMonth);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Selected.Number))
    {
      MoveCsePersonsWorkSet(import.Selected, export.CsePersonsWorkSet);
    }

    if (IsEmpty(export.CsePersonStandard.PromptField))
    {
      export.CsePersonStandard.PromptField = "+";
    }

    if (!IsEmpty(import.CsePerson1.Number))
    {
      // *************************************************************
      //           If we enter the screen with a cse person
      //     number,the workset export view needs to be
      //     populated with that.
      // *************************************************************
      export.CsePersonsWorkSet.Number = import.CsePerson1.Number;
    }

    if (IsEmpty(export.CsePersonsWorkSet.Number) && !
      IsEmpty(import.ImHousehold.AeCaseNo))
    {
      if (IsEmpty(export.ListAdjsForAllMbrs.Flag))
      {
        export.ListAdjsForAllMbrs.Flag = "Y";
      }
    }

    if (!IsEmpty(export.CsePersonsWorkSet.Number) && !
      IsEmpty(import.ImHousehold.AeCaseNo))
    {
      if (IsEmpty(export.ListAdjsForAllMbrs.Flag))
      {
        export.ListAdjsForAllMbrs.Flag = "N";
      }
    }

    if (!import.Group.IsEmpty && !Equal(global.Command, "DISPLAY"))
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        MoveCsePersonsWorkSet(import.Group.Item.GcsePersonsWorkSet,
          export.Group.Update.GcsePersonsWorkSet);
        export.Group.Update.Gcommon.SelectChar =
          import.Group.Item.Gcommon.SelectChar;
        export.Group.Update.GimHouseholdMbrMnthlyAdj.Assign(
          import.Group.Item.GimHouseholdMbrMnthlyAdj);
        MoveImHouseholdMbrMnthlySum(import.Group.Item.GimHouseholdMbrMnthlySum,
          export.Group.Update.GimHouseholdMbrMnthlySum);
        export.Group.Update.GadjustmentYrMnth.YearMonth =
          import.Group.Item.GadjustmentYrMnth.YearMonth;
        export.Group.Next();
      }
    }

    // ------- Pad AE case number left with 0's
    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.ForPadding.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros1();
      export.CsePersonsWorkSet.Number = local.ForPadding.Text10;
    }

    // Check --- for NEXTRAN
    if (!IsEmpty(import.Standard.NextTransaction))
    {
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
      ExitState = "ACO_NN0000_ALL_OK";
      ExitState = "OE0000_AE_CASE_NUMBER_MANDATORY";

      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (!IsEmpty(export.ImHousehold.AeCaseNo))
    {
      local.TextWorkArea.Text10 = export.ImHousehold.AeCaseNo;
      UseEabPadLeftWithZeros2();

      if (TextWorkArea.Text10_MaxLength > 9)
      {
        export.ImHousehold.AeCaseNo =
          Substring(local.TextWorkArea.Text10, 3, 8);
      }
      else
      {
        export.ImHousehold.AeCaseNo = local.TextWorkArea.Text10;
      }
    }

    if (Equal(global.Command, "IMHH") || Equal(global.Command, "URAH") || Equal
      (global.Command, "URAC") || Equal(global.Command, "CURA") || Equal
      (global.Command, "URAA") || Equal(global.Command, "UCOL") || Equal
      (global.Command, "UHMM"))
    {
      local.NumberOfEntriesSelected.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        switch(AsChar(export.Group.Item.Gcommon.SelectChar))
        {
          case 's':
            ++local.NumberOfEntriesSelected.Count;

            if (local.NumberOfEntriesSelected.Count > 1)
            {
              ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN3";

              return;
            }

            if (local.NumberOfEntriesSelected.Count == 1)
            {
              export.SelectedImHouseholdMbrMnthlySum.Month =
                export.Group.Item.GimHouseholdMbrMnthlySum.Month;
              export.SelectedImHouseholdMbrMnthlySum.Year =
                export.Group.Item.GimHouseholdMbrMnthlySum.Year;
              export.SelectedCsePersonsWorkSet.Number =
                export.Group.Item.GcsePersonsWorkSet.Number;
              export.SelectedCsePerson.Number =
                export.Group.Item.GcsePersonsWorkSet.Number;
              export.SelectedImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
            }

            break;
          case ' ':
            break;
          case 'S':
            ++local.NumberOfEntriesSelected.Count;

            if (local.NumberOfEntriesSelected.Count > 1)
            {
              ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN3";

              return;
            }

            if (local.NumberOfEntriesSelected.Count == 1)
            {
              export.SelectedImHouseholdMbrMnthlySum.Month =
                export.Group.Item.GimHouseholdMbrMnthlySum.Month;
              export.SelectedImHouseholdMbrMnthlySum.Year =
                export.Group.Item.GimHouseholdMbrMnthlySum.Year;
              export.SelectedCsePersonsWorkSet.Number =
                export.Group.Item.GcsePersonsWorkSet.Number;
              export.SelectedCsePerson.Number =
                export.Group.Item.GcsePersonsWorkSet.Number;
              export.SelectedImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
            }

            break;
          default:
            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            var field = GetField(export.Group.Item.Gcommon, "selectChar");

            field.Error = true;

            return;
        }
      }

      if (local.NumberOfEntriesSelected.Count == 0)
      {
        export.SelectedCsePersonsWorkSet.Number =
          export.CsePersonsWorkSet.Number;
        export.SelectedImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
        export.SelectedCsePerson.Number =
          export.SelectedCsePersonsWorkSet.Number;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (AsChar(export.CsePersonStandard.PromptField) == 'S' || AsChar
          (export.CsePersonStandard.PromptField) == 's')
        {
          export.CsePersonStandard.PromptField = "";
          ExitState = "ECO_LNK_TO_NAME";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          var field = GetField(export.CsePersonStandard, "promptField");

          field.Error = true;
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        export.FromYearNMonth.YearMonth = export.FromYearNMonth.Year * 100 + export
          .FromYearNMonth.Month;
        export.ToYearNMonth.YearMonth = export.ToYearNMonth.Year * 100 + export
          .ToYearNMonth.Month;

        if (export.ToYearNMonth.YearMonth == 0 && export
          .FromYearNMonth.YearMonth != 0)
        {
          export.ToYearNMonth.Year = Now().Date.Year;
          export.ToYearNMonth.Month = Now().Date.Month;
          export.ToYearNMonth.YearMonth = DateToInt(Now().Date) / 100;
        }

        if (export.ToYearNMonth.YearMonth == 0 && export
          .FromYearNMonth.YearMonth == 0)
        {
          // ********************************************************
          //    This just removes the day part of the date
          //  and leaves  the month and year.
          // ********************************************************
          export.FromYearNMonth.Year = Now().Date.Year - 5;
          export.FromYearNMonth.Month = Now().Date.Month;
          export.FromYearNMonth.YearMonth = export.FromYearNMonth.Year * 100 + export
            .FromYearNMonth.Month;
          export.ToYearNMonth.Year = Now().Date.Year;
          export.ToYearNMonth.Month = Now().Date.Month;
          export.ToYearNMonth.YearMonth = DateToInt(Now().Date) / 100;
        }

        if (export.ToYearNMonth.Month < 1 || export.ToYearNMonth.Month > 12)
        {
          local.ForErrMsg.Text1 = "T";

          var field = GetField(export.ToYearNMonth, "month");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;
        }
        else if (import.ToYearNMonth.Month == 0 && import.ToYearNMonth.Year > 0
          || export.ToYearNMonth.Year == 0 && export.ToYearNMonth.Month > 0)
        {
          local.ForErrMsg.Text1 = "1";

          var field1 = GetField(export.ToYearNMonth, "month");

          field1.Color = "red";
          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.ToYearNMonth, "year");

          field2.Color = "red";
          field2.Protected = false;
          field2.Focused = true;
        }

        if (export.FromYearNMonth.Month < 1 || export.FromYearNMonth.Month > 12)
        {
          local.ForErrMsg.Text1 = "F";

          var field = GetField(export.FromYearNMonth, "month");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;
        }
        else if (import.FromYearNMonth.Month == 0 && import
          .FromYearNMonth.Year > 0 || import.FromYearNMonth.Year == 0 && import
          .FromYearNMonth.Month > 0)
        {
          local.ForErrMsg.Text1 = "2";

          var field1 = GetField(export.FromYearNMonth, "month");

          field1.Color = "red";
          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.FromYearNMonth, "year");

          field2.Color = "red";
          field2.Protected = false;
          field2.Focused = true;
        }

        if (AsChar(export.ListAdjsForAllMbrs.Flag) == 'Y')
        {
        }
        else if (AsChar(export.ListAdjsForAllMbrs.Flag) == 'N')
        {
          if (IsEmpty(export.CsePersonsWorkSet.Number))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Color = "red";
            field.Protected = false;
            field.Focused = true;

            local.ForErrMsg.Text1 = "N";
          }
        }
        else
        {
          var field = GetField(export.ListAdjsForAllMbrs, "flag");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          local.ForErrMsg.Text1 = "L";
        }

        if (IsEmpty(export.ImHousehold.AeCaseNo))
        {
          var field = GetField(export.ImHousehold, "aeCaseNo");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          local.ForErrMsg.Text1 = "A";
        }

        switch(AsChar(local.ForErrMsg.Text1))
        {
          case 'A':
            ExitState = "OE0000_AE_NUMBER_MANDATORY";

            return;
          case 'F':
            ExitState = "OE0000_INVALID_MONTH";

            return;
          case '2':
            ExitState = "OE0000_MM_YY_REQUIRED";

            return;
          case 'T':
            ExitState = "OE0000_INVALID_MONTH";

            return;
          case '1':
            ExitState = "OE0000_MM_YY_REQUIRED";

            return;
          case 'L':
            ExitState = "OE0000_INCORRECT_VALUE_FOR_LADJ";

            return;
          case 'N':
            ExitState = "OE0000_CSE_PER_NBR_REQUIRED";

            return;
          default:
            break;
        }

        if (export.FromYearNMonth.YearMonth > export.ToYearNMonth.YearMonth)
        {
          ExitState = "OE0000_FROM_DATE_SHOULD_BE_LESS";

          return;
        }

        if (export.ToYearNMonth.YearMonth != 0 && export
          .FromYearNMonth.YearMonth == 0)
        {
          ExitState = "OE0000_LU_FROM_DATE_NOT_ENTERED";

          return;
        }

        if (AsChar(export.ListAdjsForAllMbrs.Flag) == 'N')
        {
          if (export.ToYearNMonth.YearMonth != 0 && export
            .FromYearNMonth.YearMonth != 0)
          {
            UseOeLuraGetAdjPersonCaseDt();
          }
          else
          {
            UseOeLuraGetAdjPersonNCase();
          }
        }
        else if (export.ToYearNMonth.YearMonth != 0 && export
          .FromYearNMonth.YearMonth != 0)
        {
          UseOeLuraGetAdjForCaseNDates();
        }
        else
        {
          UseOeLuraGetAdjForCase();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "OE0000_URAS_DISPLAY_SUCCESSFULLY";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "UHMM":
        ExitState = "ECO_LNK_TO_UHMM";

        break;
      case "URAH":
        ExitState = "ECO_LNK_TO_URAH";

        break;
      case "URAC":
        ExitState = "ECO_LNK_TO_URAC";

        break;
      case "CURA":
        ExitState = "ECO_LNK_TO_CURA";

        break;
      case "URAA":
        if (IsEmpty(export.SelectedCsePerson.Number))
        {
          ExitState = "OE0000_NO_PERSON_NO_SELECTED";

          return;
        }

        ExitState = "ECO_LNK_TO_URAA";

        break;
      case "UCOL":
        ExitState = "ECO_LNK_TO_UCOL";

        break;
      case "IMHH":
        ExitState = "ECO_LNK_TO_IMHH";

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

  private static void MoveGroup1(OeLuraGetAdjForCase.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.GadjustmentYrMnth.YearMonth = source.GadjustmentYrMnth.YearMonth;
    target.Gcommon.SelectChar = source.Gcommon.SelectChar;
    MoveImHouseholdMbrMnthlySum(source.GimHouseholdMbrMnthlySum,
      target.GimHouseholdMbrMnthlySum);
    target.GimHouseholdMbrMnthlyAdj.Assign(source.GimHouseholdMbrMnthlyAdj);
    MoveCsePersonsWorkSet(source.GcsePersonsWorkSet, target.GcsePersonsWorkSet);
  }

  private static void MoveGroup2(OeLuraGetAdjForCaseNDates.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.GadjustmentYrMnth.YearMonth = source.GadjustmentYrMnth.YearMonth;
    target.Gcommon.SelectChar = source.Gcommon.SelectChar;
    MoveImHouseholdMbrMnthlySum(source.GimHouseholdMbrMnthlySum,
      target.GimHouseholdMbrMnthlySum);
    target.GimHouseholdMbrMnthlyAdj.Assign(source.GimHouseholdMbrMnthlyAdj);
    MoveCsePersonsWorkSet(source.GcsePersonsWorkSet, target.GcsePersonsWorkSet);
  }

  private static void MoveGroup3(OeLuraGetAdjPersonCaseDt.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.GadjustmentYrMnth.YearMonth = source.GadjustmentYrMnth.YearMonth;
    target.Gcommon.SelectChar = source.Gcommon.SelectChar;
    MoveImHouseholdMbrMnthlySum(source.GimHouseholdMbrMnthlySum,
      target.GimHouseholdMbrMnthlySum);
    target.GimHouseholdMbrMnthlyAdj.Assign(source.GimHouseholdMbrMnthlyAdj);
    MoveCsePersonsWorkSet(source.GcsePersonsWorkSet, target.GcsePersonsWorkSet);
  }

  private static void MoveGroup4(OeLuraGetAdjPersonNCase.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.GadjustmentYrMnth.YearMonth = source.GadjustmentYrMnth.YearMonth;
    target.Gcommon.SelectChar = source.Gcommon.SelectChar;
    MoveImHouseholdMbrMnthlySum(source.GimHouseholdMbrMnthlySum,
      target.GimHouseholdMbrMnthlySum);
    target.GimHouseholdMbrMnthlyAdj.Assign(source.GimHouseholdMbrMnthlyAdj);
    MoveCsePersonsWorkSet(source.GcsePersonsWorkSet, target.GcsePersonsWorkSet);
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private void UseEabPadLeftWithZeros1()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ForPadding.Text10;
    useExport.TextWorkArea.Text10 = local.ForPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ForPadding.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabPadLeftWithZeros2()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeLuraGetAdjForCase()
  {
    var useImport = new OeLuraGetAdjForCase.Import();
    var useExport = new OeLuraGetAdjForCase.Export();

    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useImport.ImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
    useImport.ImHouseholdMbrMnthlySum.Relationship =
      export.ImHouseholdMbrMnthlySum.Relationship;

    Call(OeLuraGetAdjForCase.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup1);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.ImHouseholdMbrMnthlySum.Relationship =
      useExport.ImHouseholdMbrMnthlySum.Relationship;
  }

  private void UseOeLuraGetAdjForCaseNDates()
  {
    var useImport = new OeLuraGetAdjForCaseNDates.Import();
    var useExport = new OeLuraGetAdjForCaseNDates.Export();

    useImport.ToYearNMonth.Assign(export.ToYearNMonth);
    useImport.FromYearNMonth.Assign(export.FromYearNMonth);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useImport.ImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
    useImport.ImHouseholdMbrMnthlySum.Relationship =
      export.ImHouseholdMbrMnthlySum.Relationship;

    Call(OeLuraGetAdjForCaseNDates.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup2);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.ImHouseholdMbrMnthlySum.Relationship =
      useExport.ImHouseholdMbrMnthlySum.Relationship;
  }

  private void UseOeLuraGetAdjPersonCaseDt()
  {
    var useImport = new OeLuraGetAdjPersonCaseDt.Import();
    var useExport = new OeLuraGetAdjPersonCaseDt.Export();

    useImport.To.Assign(export.ToYearNMonth);
    useImport.From.Assign(export.FromYearNMonth);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useImport.ImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
    useImport.ImHouseholdMbrMnthlySum.Relationship =
      export.ImHouseholdMbrMnthlySum.Relationship;

    Call(OeLuraGetAdjPersonCaseDt.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup3);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.ImHouseholdMbrMnthlySum.Relationship =
      useExport.ImHouseholdMbrMnthlySum.Relationship;
  }

  private void UseOeLuraGetAdjPersonNCase()
  {
    var useImport = new OeLuraGetAdjPersonNCase.Import();
    var useExport = new OeLuraGetAdjPersonNCase.Export();

    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useImport.ImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
    useImport.ImHouseholdMbrMnthlySum.Relationship =
      export.ImHouseholdMbrMnthlySum.Relationship;

    Call(OeLuraGetAdjPersonNCase.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.SelectedCsePersonsWorkSet);
    useExport.Group.CopyTo(export.Group, MoveGroup4);
    export.ImHouseholdMbrMnthlySum.Relationship =
      useExport.ImHouseholdMbrMnthlySum.Relationship;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

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

    useImport.CsePerson.Number = import.CsePerson1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GadjustmentYrMnth.
      /// </summary>
      [JsonPropertyName("gadjustmentYrMnth")]
      public DateWorkArea GadjustmentYrMnth
      {
        get => gadjustmentYrMnth ??= new();
        set => gadjustmentYrMnth = value;
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
      /// A value of GimHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum GimHouseholdMbrMnthlySum
      {
        get => gimHouseholdMbrMnthlySum ??= new();
        set => gimHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of GimHouseholdMbrMnthlyAdj.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlyAdj")]
      public ImHouseholdMbrMnthlyAdj GimHouseholdMbrMnthlyAdj
      {
        get => gimHouseholdMbrMnthlyAdj ??= new();
        set => gimHouseholdMbrMnthlyAdj = value;
      }

      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private DateWorkArea gadjustmentYrMnth;
      private Common gcommon;
      private ImHouseholdMbrMnthlySum gimHouseholdMbrMnthlySum;
      private ImHouseholdMbrMnthlyAdj gimHouseholdMbrMnthlyAdj;
      private CsePersonsWorkSet gcsePersonsWorkSet;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of CsePerson1.
    /// </summary>
    [JsonPropertyName("csePerson1")]
    public CsePerson CsePerson1
    {
      get => csePerson1 ??= new();
      set => csePerson1 = value;
    }

    /// <summary>
    /// A value of ListAdjsForAllMbrs.
    /// </summary>
    [JsonPropertyName("listAdjsForAllMbrs")]
    public Common ListAdjsForAllMbrs
    {
      get => listAdjsForAllMbrs ??= new();
      set => listAdjsForAllMbrs = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of ToYearNMonth.
    /// </summary>
    [JsonPropertyName("toYearNMonth")]
    public DateWorkArea ToYearNMonth
    {
      get => toYearNMonth ??= new();
      set => toYearNMonth = value;
    }

    /// <summary>
    /// A value of FromYearNMonth.
    /// </summary>
    [JsonPropertyName("fromYearNMonth")]
    public DateWorkArea FromYearNMonth
    {
      get => fromYearNMonth ??= new();
      set => fromYearNMonth = value;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of CsePerson2.
    /// </summary>
    [JsonPropertyName("csePerson2")]
    public Standard CsePerson2
    {
      get => csePerson2 ??= new();
      set => csePerson2 = value;
    }

    private CsePersonsWorkSet selected;
    private CsePerson csePerson1;
    private Common listAdjsForAllMbrs;
    private Array<GroupGroup> group;
    private DateWorkArea toYearNMonth;
    private DateWorkArea fromYearNMonth;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ImHousehold imHousehold;
    private Standard standard;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private Standard csePerson2;
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
      /// A value of GadjustmentYrMnth.
      /// </summary>
      [JsonPropertyName("gadjustmentYrMnth")]
      public DateWorkArea GadjustmentYrMnth
      {
        get => gadjustmentYrMnth ??= new();
        set => gadjustmentYrMnth = value;
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
      /// A value of GimHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum GimHouseholdMbrMnthlySum
      {
        get => gimHouseholdMbrMnthlySum ??= new();
        set => gimHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of GimHouseholdMbrMnthlyAdj.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlyAdj")]
      public ImHouseholdMbrMnthlyAdj GimHouseholdMbrMnthlyAdj
      {
        get => gimHouseholdMbrMnthlyAdj ??= new();
        set => gimHouseholdMbrMnthlyAdj = value;
      }

      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private DateWorkArea gadjustmentYrMnth;
      private Common gcommon;
      private ImHouseholdMbrMnthlySum gimHouseholdMbrMnthlySum;
      private ImHouseholdMbrMnthlyAdj gimHouseholdMbrMnthlyAdj;
      private CsePersonsWorkSet gcsePersonsWorkSet;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of CsePersonCsePerson.
    /// </summary>
    [JsonPropertyName("csePersonCsePerson")]
    public CsePerson CsePersonCsePerson
    {
      get => csePersonCsePerson ??= new();
      set => csePersonCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("selectedImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum SelectedImHouseholdMbrMnthlySum
    {
      get => selectedImHouseholdMbrMnthlySum ??= new();
      set => selectedImHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedImHousehold.
    /// </summary>
    [JsonPropertyName("selectedImHousehold")]
    public ImHousehold SelectedImHousehold
    {
      get => selectedImHousehold ??= new();
      set => selectedImHousehold = value;
    }

    /// <summary>
    /// A value of ListAdjsForAllMbrs.
    /// </summary>
    [JsonPropertyName("listAdjsForAllMbrs")]
    public Common ListAdjsForAllMbrs
    {
      get => listAdjsForAllMbrs ??= new();
      set => listAdjsForAllMbrs = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of ToYearNMonth.
    /// </summary>
    [JsonPropertyName("toYearNMonth")]
    public DateWorkArea ToYearNMonth
    {
      get => toYearNMonth ??= new();
      set => toYearNMonth = value;
    }

    /// <summary>
    /// A value of FromYearNMonth.
    /// </summary>
    [JsonPropertyName("fromYearNMonth")]
    public DateWorkArea FromYearNMonth
    {
      get => fromYearNMonth ??= new();
      set => fromYearNMonth = value;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of CsePersonStandard.
    /// </summary>
    [JsonPropertyName("csePersonStandard")]
    public Standard CsePersonStandard
    {
      get => csePersonStandard ??= new();
      set => csePersonStandard = value;
    }

    private CsePerson selectedCsePerson;
    private CsePerson csePersonCsePerson;
    private ImHouseholdMbrMnthlySum selectedImHouseholdMbrMnthlySum;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private ImHousehold selectedImHousehold;
    private Common listAdjsForAllMbrs;
    private Case1 case1;
    private Array<GroupGroup> group;
    private DateWorkArea toYearNMonth;
    private DateWorkArea fromYearNMonth;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ImHousehold imHousehold;
    private Standard standard;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private Standard csePersonStandard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForErrMsg.
    /// </summary>
    [JsonPropertyName("forErrMsg")]
    public TextWorkArea ForErrMsg
    {
      get => forErrMsg ??= new();
      set => forErrMsg = value;
    }

    /// <summary>
    /// A value of ForPadding.
    /// </summary>
    [JsonPropertyName("forPadding")]
    public TextWorkArea ForPadding
    {
      get => forPadding ??= new();
      set => forPadding = value;
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
    /// A value of ForCompOfMnthlyAdj.
    /// </summary>
    [JsonPropertyName("forCompOfMnthlyAdj")]
    public DateWorkArea ForCompOfMnthlyAdj
    {
      get => forCompOfMnthlyAdj ??= new();
      set => forCompOfMnthlyAdj = value;
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

    /// <summary>
    /// A value of NumberOfEntriesSelected.
    /// </summary>
    [JsonPropertyName("numberOfEntriesSelected")]
    public Common NumberOfEntriesSelected
    {
      get => numberOfEntriesSelected ??= new();
      set => numberOfEntriesSelected = value;
    }

    private TextWorkArea forErrMsg;
    private TextWorkArea forPadding;
    private TextWorkArea textWorkArea;
    private DateWorkArea forCompOfMnthlyAdj;
    private NextTranInfo nextTranInfo;
    private Common numberOfEntriesSelected;
  }
#endregion
}

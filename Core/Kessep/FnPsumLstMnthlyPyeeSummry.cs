// Program: FN_PSUM_LST_MNTHLY_PYEE_SUMMRY, ID: 372522891, model: 746.
// Short name: SWEPSUMP
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
/// A program: FN_PSUM_LST_MNTHLY_PYEE_SUMMRY.
/// </para>
/// <para>
/// RESP: FINMNGMNT
/// This procedure will list the monthly totals for particular categories for a 
/// payee for specific months.
/// The occurence was set to 132 (for 11 years) instead of 240 (for 20 years) to
/// keep the total export view from exceed the limit 31K.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPsumLstMnthlyPyeeSummry: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PSUM_LST_MNTHLY_PYEE_SUMMRY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPsumLstMnthlyPyeeSummry(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPsumLstMnthlyPyeeSummry.
  /// </summary>
  public FnPsumLstMnthlyPyeeSummry(IContext context, Import import,
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
    // *********************************************
    // Procedure : List Monthly AR/Payee Summary
    // Screen-id : PSUM
    // Primary Developer   : Unknown
    // Subsequent Development by : R.B.Mohapatra, MTW
    // Change Log :
    // -----------
    // 1. 02/05/1996   Change in the Procedure Logic
    // 2. 02/20/1996   Added Entity View CSE_PERSON
    //                 in both IMPORT/EXPORT for DLG
    //                 flow data-sent compatibility.
    // R. Marchman
    // 1. 12/10/96	Add new security and next tran
    // A. Phadnis
    // 1. 01/27/98     Added flow to URAH.
    // 2. 01/27/98     Added a call to action block to get the suppressed 
    // amount.
    // N.Engoor
    // 1. 03/04/99     Added flow to EFTL screen.
    // 2. 03/04/99     Added three new PF KEYS on the screen -
    // PF17 - EFTL, PF18 - PREV YEAR, PF19 - NEXT YEAR.
    // SWSRKXD 11/22/99 PR80750
    // Changed dialog flow to PACC to pass both start and end
    // dates. Also change code last day of current month as
    // End_date and first day of previous month as Start_date
    // SWSRKXD 05/24/2000 Work Order #164
    // - Excess URA changes for PRWORA.
    // - Replace ZDEL_IM_HOUSEHOLD_MBR with IM_HH_MBR_MTHLY_SUMM
    // - Replace the logic to clear RGV with a single USE statement
    // *********************************************
    // *********************************************
    // Fangman  01/24/2001 Work Order #235
    // - PSUM redesign - Add new columns & correct any problems with current 
    // data capture/calculation.   Added code to "restore" the AR # to the
    // screen when the name is not found in ADABASE.
    // *********************************************
    // *********************************************
    // Fangman  04/17/2002 PR 143744
    // - PF8 Next scroll feature on PSUM - took out error exit state & replaced 
    // it with the normal warning exit state.
    // *********************************************
    // *********************************************
    // Fangman  05/08/2002 PR 146227
    // - Changed fields on screen to show negative numbers for each amount field
    // *********************************************
    // *********************************************
    // K Cole	05/24/02	PR#143744
    // - Changed scrolling to explicit
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    local.Common.Count = 0;
    local.Error.Count = 0;
    local.DateWorkArea.Date = Now().Date;

    // **** Move all IMPORTs to EXPORTs ****
    MoveCommon(import.PromptPayee, export.PromptPayee);
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    MoveMonthlyObligeeSummary1(import.Start, export.Start);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MoveStandard(import.Scrolling, export.Scrolling);

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      MoveMonthlyObligeeSummary1(import.HiddenPageKeys.Item.
        GimportHiddenPageKey,
        export.HiddenPageKeys.Update.GexportHiddenPageKey);
    }

    import.HiddenPageKeys.CheckIndex();
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);

    // *** Left Padding CSE_PERSON NUMBER with Zeros ***
    local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
    UseEabPadLeftWithZeros();
    export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    export.CsePerson.Number = export.CsePersonsWorkSet.Number;

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        MoveMonthlyObligeeSummary1(import.HiddenPageKeys.Item.
          GimportHiddenPageKey,
          export.HiddenPageKeys.Update.GexportHiddenPageKey);
        export.CsePersonsWorkSet.Number = export.HiddenCsePersonsWorkSet.Number;
      }
      else
      {
        var field = GetField(export.Start, "month");

        field.Protected = false;
        field.Focused = true;
      }

      export.CsePersonsWorkSet.FormattedName = "";
      export.PromptPayee.SelectChar = "";
    }
    else if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = -1;

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;
        MoveMonthlyObligeeSummary2(import.Import1.Item.MonthlyObligeeSummary,
          export.Export1.Update.MonthlyObligeeSummary);
        export.Export1.Update.Yy.Text2 = import.Import1.Item.Yy.Text2;
      }
    }

    if (Equal(global.Command, "RETURAH"))
    {
      return;
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
      {
        MoveMonthlyObligeeSummary1(export.Export1.Item.MonthlyObligeeSummary,
          local.Flow);
        ++local.Common.Count;
      }
      else if (IsEmpty(export.Export1.Item.Common.SelectChar))
      {
        // --- Do nothing
      }
      else
      {
        // --- User selected other than S
        ++local.Error.Count;
      }
    }

    export.Export1.CheckIndex();

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (local.Common.Count > 1)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }

      if (local.Error.Count > 0)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
      }
    }

    export.Export1.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ------------------
      // This is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction.
      // ------------------
      export.HiddenNextTranInfo.CsePersonNumberObligee =
        import.CsePersonsWorkSet.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        import.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------
      // This is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ---------------------
      UseScCabNextTranGet();

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligee))
      {
        export.CsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumberObligee ?? Spaces(10);
      }
      else
      {
        export.CsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      }

      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------
    // If Starting Month & Year are not entered, then default to the current  
    // month and year.
    // ---------------------
    if (export.Start.Month == 0)
    {
      local.DateValidation.Month = Month(local.DateWorkArea.Date);

      if (local.DateValidation.Month == 0)
      {
        export.Start.Month = 12;
        export.Start.Year = Year(local.DateWorkArea.Date) - 1;
      }
      else
      {
        export.Start.Month = local.DateValidation.Month;
      }
    }
    else if (export.Start.Month > 12 || export.Start.Month < 1)
    {
      var field = GetField(export.Start, "month");

      field.Error = true;

      ExitState = "INVALID_MONTH_ENTERED";

      return;
    }

    if (export.Start.Year == 0)
    {
      local.DateValidation.Year = Year(local.DateWorkArea.Date);
      export.Start.Year = Year(local.DateWorkArea.Date);
    }

    // ------------------------------------------------------------
    //                    Main CASE OF COMMAND.
    // ------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "EFTL":
        if (!IsEmpty(export.CsePerson.Number) && IsEmpty
          (export.CsePersonsWorkSet.FormattedName))
        {
          ExitState = "FN0000_CLEAR_SCREEN_BEFORE_FLOW";

          return;
        }

        ExitState = "ECO_LNK_TO_LST_EFTS";

        return;
      case "URAH":
        if (!IsEmpty(export.CsePerson.Number) && IsEmpty
          (export.CsePersonsWorkSet.FormattedName))
        {
          ExitState = "FN0000_CLEAR_SCREEN_BEFORE_FLOW";

          return;
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          // ------------------------------------------------------
          // SWSRKXD - 5/24/2000
          // Replace set statements to clear RGV with USE statement below.
          // ---------------------------------------------------------
          UseFnPsumClearViews();
          ExitState = "FN0000_AR_CSE_PERSON_NO_REQD";

          return;
        }

        foreach(var item in ReadImHouseholdImHouseholdMbrMnthlySum())
        {
          export.ImHousehold.AeCaseNo = entities.ImHousehold.AeCaseNo;
        }

        ExitState = "ECO_LNK_TO_URAH_HOUSEHOLD_URA";

        break;
      case "LIST":
        // Link to EAB to get AR/Payee from ADABASE
        switch(AsChar(export.PromptPayee.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          default:
            var field1 = GetField(export.PromptPayee, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        var field = GetField(export.PromptPayee, "selectChar");

        field.Error = true;

        break;
      case "DISPLAY":
        UseFnPsumClearViews();
        export.Scrolling.PageNumber = 1;

        export.HiddenPageKeys.Index = 0;
        export.HiddenPageKeys.CheckSize();

        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";

        // Initializing first occurance of year in group to 9999 because the 
        // read sorts the data descending.
        export.HiddenPageKeys.Update.GexportHiddenPageKey.Year = 9999;

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          export.CsePersonsWorkSet.FormattedName = "";
          ExitState = "FN0000_AR_CSE_PERSON_NO_REQD";

          return;
        }

        if (ReadCsePerson())
        {
          export.HiddenCsePersonsWorkSet.Number =
            export.CsePersonsWorkSet.Number;
        }
        else
        {
          // ------------------------------------
          // KD - 2/11/99
          // Highlight field in error
          // ------------------------------------
          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          export.CsePersonsWorkSet.FormattedName = "";

          // ------------------------------------------------------
          // SWSRKXD - 5/24/2000
          // Replace set statements to clear RGV with USE statement below.
          // ---------------------------------------------------------
          ExitState = "CSE_PERSON_NF";

          return;
        }

        if (!ReadCsePersonAccount())
        {
          export.CsePersonsWorkSet.FormattedName = "";

          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          ExitState = "FN0000_CSE_PERSON_NOT_A_PAYEE";

          return;
        }

        local.Hold.Number = export.CsePersonsWorkSet.Number;
        UseSiReadCsePerson();

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          export.CsePersonsWorkSet.Number = local.Hold.Number;
        }

        local.PageStartKey.Year = 9999;
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "NEXT":
        if (export.Scrolling.PageNumber == Export.HiddenPageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          return;
        }

        if (IsEmpty(export.ScrollingAttributes.PlusFlag))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        UseFnPsumClearViews();
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";

        // Set variable so read can start at correct place for explicit 
        // scrolling. Read handled below.
        ++export.Scrolling.PageNumber;

        export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
        export.HiddenPageKeys.CheckSize();

        MoveMonthlyObligeeSummary1(export.HiddenPageKeys.Item.
          GexportHiddenPageKey, local.PageStartKey);

        break;
      case "PREV":
        if (export.Scrolling.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        UseFnPsumClearViews();
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";

        // Set variable so read can start at correct place for explicit 
        // scrolling. Read handled below.
        --export.Scrolling.PageNumber;

        export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
        export.HiddenPageKeys.CheckSize();

        MoveMonthlyObligeeSummary1(export.HiddenPageKeys.Item.
          GexportHiddenPageKey, local.PageStartKey);

        break;
      case "PACC":
        // ----------------------------------------------------
        // Pass the cse_person_number and Start and End Dates to
        // List Payee Account screen.
        // ----------------------------------------------------
        if (!IsEmpty(export.CsePerson.Number) && IsEmpty
          (export.CsePersonsWorkSet.FormattedName))
        {
          ExitState = "FN0000_CLEAR_SCREEN_BEFORE_FLOW";

          return;
        }

        if (local.Common.Count == 1)
        {
          // ----------------------------------------------------------
          // SWSRKXD - 11/22/99 PR80750
          // Pass last day of current month as End_date and first day of
          // previous month as Start_date.
          // ----------------------------------------------------------
          export.FlowStart.Date = AddMonths(IntToDate(local.Flow.Year * 10000
            + local.Flow.Month * 100 + 1), -1);
          export.FlowEnd.Date = AddDays(AddMonths(IntToDate(local.Flow.Year * 10000
            + local.Flow.Month * 100 + 1), 1), -1);

          if (Lt(Now().Date, export.FlowEnd.Date))
          {
            export.FlowEnd.Date = Now().Date;
          }
        }
        else
        {
          export.FlowStart.Date = null;
          export.FlowEnd.Date = null;
        }

        ExitState = "ECO_LNK_TO_LST_PAYEE_ACCT";

        break;
      case "WARR":
        if (!IsEmpty(export.CsePerson.Number) && IsEmpty
          (export.CsePersonsWorkSet.FormattedName))
        {
          ExitState = "FN0000_CLEAR_SCREEN_BEFORE_FLOW";

          return;
        }

        // -----------------------------------------------------
        // Link to WARR screen, passing the Cse_person Number,
        // Name and Starting date.
        // ------------------------------------------------------
        if (local.Common.Count == 1)
        {
          export.FlowStart.Date = IntToDate(local.Flow.Year * 10000 + local
            .Flow.Month * 100 + 1);
        }
        else
        {
          export.FlowStart.Date = null;
        }

        ExitState = "ECO_LNK_TO_LST_WARRANTS";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // The following was added for explicit scrolling. K Cole
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      export.Export1.Index = -1;

      foreach(var item in ReadMonthlyObligeeSummary())
      {
        if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
        {
          break;
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.MonthlyObligeeSummary.Assign(
          entities.MonthlyObligeeSummary);
        export.Export1.Update.Yy.Text2 =
          NumberToString(entities.MonthlyObligeeSummary.Year, 14, 2);
      }

      // Set exit state and MORE indicator
      if (export.Scrolling.PageNumber > 1)
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }

      if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
      {
        export.ScrollingAttributes.PlusFlag = "+";

        ++export.HiddenPageKeys.Index;
        export.HiddenPageKeys.CheckSize();

        MoveMonthlyObligeeSummary1(export.Export1.Item.MonthlyObligeeSummary,
          export.HiddenPageKeys.Update.GexportHiddenPageKey);
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(FnPsumClearViews.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
    target.Yy.Text2 = source.Yy.Text2;
  }

  private static void MoveMonthlyObligeeSummary1(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private static void MoveMonthlyObligeeSummary2(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.DisbursementsSuppressed = source.DisbursementsSuppressed;
    target.RecapturedAmt = source.RecapturedAmt;
    target.PassthruAmount = source.PassthruAmount;
    target.CollectionsAmount = source.CollectionsAmount;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
    target.FeeAmount = source.FeeAmount;
    target.AdcReimbursedAmount = source.AdcReimbursedAmount;
    target.TotExcessUraAmt = source.TotExcessUraAmt;
    target.NumberOfCollections = source.NumberOfCollections;
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

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
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

  private void UseFnPsumClearViews()
  {
    var useImport = new FnPsumClearViews.Import();
    var useExport = new FnPsumClearViews.Export();

    Call(FnPsumClearViews.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
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
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.Obligee.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligee.CspNumber = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 1);
        entities.Obligee.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee.Type1);
      });
  }

  private IEnumerable<bool> ReadImHouseholdImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.ImHousehold.Populated = false;

    return ReadEach("ReadImHouseholdImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.CaseStatus = db.GetString(reader, 1);
        entities.ImHousehold.StatusDate = db.GetDate(reader, 2);
        entities.ImHousehold.CreatedBy = db.GetString(reader, 3);
        entities.ImHousehold.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.ImHousehold.LastUpdatedBy = db.GetString(reader, 5);
        entities.ImHousehold.LastUpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.ImHousehold.FirstBenefitDate = db.GetNullableDate(reader, 7);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 8);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 9);
        entities.ImHouseholdMbrMnthlySum.CreatedTmst =
          db.GetDateTime(reader, 10);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 11);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.ImHousehold.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonthlyObligeeSummary()
  {
    entities.MonthlyObligeeSummary.Populated = false;

    return ReadEach("ReadMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer1", local.PageStartKey.Year);
        db.SetInt32(command, "mnth1", local.PageStartKey.Month);
        db.SetInt32(command, "yer2", export.Start.Year);
        db.SetInt32(command, "mnth2", export.Start.Month);
        db.SetString(command, "cspSNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.MonthlyObligeeSummary.Year = db.GetInt32(reader, 0);
        entities.MonthlyObligeeSummary.Month = db.GetInt32(reader, 1);
        entities.MonthlyObligeeSummary.DisbursementsSuppressed =
          db.GetNullableDecimal(reader, 2);
        entities.MonthlyObligeeSummary.RecapturedAmt =
          db.GetNullableDecimal(reader, 3);
        entities.MonthlyObligeeSummary.PassthruAmount =
          db.GetDecimal(reader, 4);
        entities.MonthlyObligeeSummary.CreatedBy = db.GetString(reader, 5);
        entities.MonthlyObligeeSummary.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.MonthlyObligeeSummary.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.MonthlyObligeeSummary.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.MonthlyObligeeSummary.AdcReimbursedAmount =
          db.GetNullableDecimal(reader, 9);
        entities.MonthlyObligeeSummary.CpaSType = db.GetString(reader, 10);
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 11);
        entities.MonthlyObligeeSummary.CollectionsAmount =
          db.GetNullableDecimal(reader, 12);
        entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
          db.GetNullableDecimal(reader, 13);
        entities.MonthlyObligeeSummary.FeeAmount =
          db.GetNullableDecimal(reader, 14);
        entities.MonthlyObligeeSummary.TotExcessUraAmt =
          db.GetNullableDecimal(reader, 15);
        entities.MonthlyObligeeSummary.NumberOfCollections =
          db.GetNullableInt32(reader, 16);
        entities.MonthlyObligeeSummary.Populated = true;
        CheckValid<MonthlyObligeeSummary>("CpaSType",
          entities.MonthlyObligeeSummary.CpaSType);

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
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportHiddenPageKey.
      /// </summary>
      [JsonPropertyName("gimportHiddenPageKey")]
      public MonthlyObligeeSummary GimportHiddenPageKey
      {
        get => gimportHiddenPageKey ??= new();
        set => gimportHiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private MonthlyObligeeSummary gimportHiddenPageKey;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
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
      /// A value of MonthlyObligeeSummary.
      /// </summary>
      [JsonPropertyName("monthlyObligeeSummary")]
      public MonthlyObligeeSummary MonthlyObligeeSummary
      {
        get => monthlyObligeeSummary ??= new();
        set => monthlyObligeeSummary = value;
      }

      /// <summary>
      /// A value of Yy.
      /// </summary>
      [JsonPropertyName("yy")]
      public TextWorkArea Yy
      {
        get => yy ??= new();
        set => yy = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common common;
      private MonthlyObligeeSummary monthlyObligeeSummary;
      private TextWorkArea yy;
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
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    /// A value of PromptPayee.
    /// </summary>
    [JsonPropertyName("promptPayee")]
    public Common PromptPayee
    {
      get => promptPayee ??= new();
      set => promptPayee = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public MonthlyObligeeSummary Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private ScrollingAttributes scrollingAttributes;
    private Standard scrolling;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptPayee;
    private MonthlyObligeeSummary start;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportHiddenPageKey.
      /// </summary>
      [JsonPropertyName("gexportHiddenPageKey")]
      public MonthlyObligeeSummary GexportHiddenPageKey
      {
        get => gexportHiddenPageKey ??= new();
        set => gexportHiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private MonthlyObligeeSummary gexportHiddenPageKey;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
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
      /// A value of MonthlyObligeeSummary.
      /// </summary>
      [JsonPropertyName("monthlyObligeeSummary")]
      public MonthlyObligeeSummary MonthlyObligeeSummary
      {
        get => monthlyObligeeSummary ??= new();
        set => monthlyObligeeSummary = value;
      }

      /// <summary>
      /// A value of Yy.
      /// </summary>
      [JsonPropertyName("yy")]
      public TextWorkArea Yy
      {
        get => yy ??= new();
        set => yy = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common common;
      private MonthlyObligeeSummary monthlyObligeeSummary;
      private TextWorkArea yy;
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
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of FlowEnd.
    /// </summary>
    [JsonPropertyName("flowEnd")]
    public DateWorkArea FlowEnd
    {
      get => flowEnd ??= new();
      set => flowEnd = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of PromptPayee.
    /// </summary>
    [JsonPropertyName("promptPayee")]
    public Common PromptPayee
    {
      get => promptPayee ??= new();
      set => promptPayee = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public MonthlyObligeeSummary Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of FlowStart.
    /// </summary>
    [JsonPropertyName("flowStart")]
    public DateWorkArea FlowStart
    {
      get => flowStart ??= new();
      set => flowStart = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private ScrollingAttributes scrollingAttributes;
    private Standard scrolling;
    private DateWorkArea flowEnd;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Case1 case1;
    private ImHousehold imHousehold;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptPayee;
    private MonthlyObligeeSummary start;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<ExportGroup> export1;
    private DateWorkArea flowStart;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PageStartKey.
    /// </summary>
    [JsonPropertyName("pageStartKey")]
    public MonthlyObligeeSummary PageStartKey
    {
      get => pageStartKey ??= new();
      set => pageStartKey = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public CsePersonsWorkSet Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public MonthlyObligeeSummary Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of DateToPass.
    /// </summary>
    [JsonPropertyName("dateToPass")]
    public DateWorkArea DateToPass
    {
      get => dateToPass ??= new();
      set => dateToPass = value;
    }

    /// <summary>
    /// A value of DateValidation.
    /// </summary>
    [JsonPropertyName("dateValidation")]
    public DateValidation DateValidation
    {
      get => dateValidation ??= new();
      set => dateValidation = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private MonthlyObligeeSummary pageStartKey;
    private CsePersonsWorkSet hold;
    private DateWorkArea convert;
    private Common error;
    private DateWorkArea dateWorkArea;
    private TextWorkArea textWorkArea;
    private MonthlyObligeeSummary flow;
    private DateWorkArea dateToPass;
    private DateValidation dateValidation;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHousehold imHousehold;
    private CsePersonAccount obligee;
    private CsePerson csePerson;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }
#endregion
}

// Program: OE_UCOL_LIST_URA_COLLECTIONS, ID: 374450924, model: 746.
// Short name: SWEUCOLP
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
/// A program: OE_UCOL_LIST_URA_COLLECTIONS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeUcolListUraCollections: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UCOL_LIST_URA_COLLECTIONS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUcolListUraCollections(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUcolListUraCollections.
  /// </summary>
  public OeUcolListUraCollections(IContext context, Import import, Export export)
    :
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
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // Maureen Brown    May, 2000         Initial Code
    // Mark Ashworth    Feb, 2002         PR138462, Added collection filter
    // K.Doshi          10-02-02          Fix screen help Id.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ****************************************************************
    // set current date to a local view
    // ****************************************************************
    local.Current.Date = Now().Date;

    // : Set default for the date range.  It will be current date - 5 years if '
    // to'
    //   date was not entered, or 'to' date -5 years.
    local.Default1.Date = Now().Date.AddYears(-5);
    local.DefaultYearsToSubtract.Count = 5;

    // -----------------------------------------------------------
    // Move imports to exports
    // -----------------------------------------------------------
    export.SearchCollFrom.Date = import.SearchCollFrom.Date;
    export.SearchCollTo.Date = import.SearchCollTo.Date;
    export.SearchImHousehold.AeCaseNo = import.SearchImHousehold.AeCaseNo;
    MoveCsePersonsWorkSet(import.SearchMemberCsePersonsWorkSet,
      export.SearchMember);
    MoveCsePersonsWorkSet(import.SearchObligor, export.SearchObligor);
    export.SearchLegalAction.StandardNumber =
      import.SearchLegalAction.StandardNumber;
    export.SearchCase.Number = import.SearchCase.Number;

    if (!IsEmpty(import.SearchMemberCsePerson.Number) && IsEmpty
      (import.SearchMemberCsePersonsWorkSet.Number) && IsEmpty
      (import.PreviousMember.Number))
    {
      // : Most screens pass UCOL CSE Person Workset, but PATM sends CSE person,
      //  so populate the workset if it is empty and CSE person is not.
      export.SearchMember.Number = import.SearchMemberCsePerson.Number;
    }

    export.PreviousMember.Number = export.SearchMember.Number;
    export.CourtOrderPrompt.SelectChar = import.CourtOrderPrompt.SelectChar;
    export.MemberPrompt.SelectChar = import.MemberPrompt.SelectChar;
    export.ToMonthyear.YearMonth = import.ToMonthyear.YearMonth;
    export.FromMonthyear.YearMonth = import.FromMonthyear.YearMonth;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.MemberPrompt.SelectChar = import.MemberPrompt.SelectChar;
    export.ObligorPrompt.SelectChar = import.ObligorPrompt.SelectChar;
    export.CourtOrderPrompt.SelectChar = import.CourtOrderPrompt.SelectChar;
    export.CasePrompt.SelectChar = import.CasePrompt.SelectChar;

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (!import.Import1.IsEmpty)
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.Common.SelectChar =
            import.Import1.Item.Common.SelectChar;
          export.Export1.Update.Case1.Number = import.Import1.Item.Case1.Number;
          export.Export1.Update.Collection.
            Assign(import.Import1.Item.Collection);
          export.Export1.Update.CollectionType.Code =
            import.Import1.Item.CollectionType.Code;
          export.Export1.Update.ImHousehold.AeCaseNo =
            import.Import1.Item.ImHousehold.AeCaseNo;
          MoveCsePersonsWorkSet(import.Import1.Item.Member,
            export.Export1.Update.Member);
          MoveCsePersonsWorkSet(import.Import1.Item.Obligor,
            export.Export1.Update.Obligor);
          MoveCsePersonsWorkSet(import.Import1.Item.Supported,
            export.Export1.Update.Supported);
          MoveUraCollectionApplication(import.Import1.Item.
            UraCollectionApplication,
            export.Export1.Update.UraCollectionApplication);
          MoveImHouseholdMbrMnthlySum(import.Import1.Item.
            ImHouseholdMbrMnthlySum,
            export.Export1.Update.ImHouseholdMbrMnthlySum);
          export.Export1.Next();
        }
      }

      // : Check for invalid prompt character entered when command not prompt.
      if (!Equal(global.Command, "LIST"))
      {
        export.CasePrompt.SelectChar = "+";
        export.CourtOrderPrompt.SelectChar = "+";
        export.MemberPrompt.SelectChar = "+";
        export.ObligorPrompt.SelectChar = "+";
      }

      // : Fields passed to other screens on links are the same as those entered
      // as search criteria.  Save search criteria entered by user, so that it
      // can be correctly re-populated on return flow from a link.
      MoveCsePersonsWorkSet(export.SearchObligor, export.SaveObligor);
      export.SaveCase.Number = export.SearchCase.Number;
      export.SaveImHousehold.AeCaseNo = export.SearchImHousehold.AeCaseNo;
      export.SaveLegalAction.StandardNumber =
        export.SearchLegalAction.StandardNumber;
      MoveCsePersonsWorkSet(export.SearchMember, export.SaveMember);

      // : Check selection.
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          continue;
        }
        else if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          ++local.SelectCount.Count;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.SelectCount.Count > 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        return;
      }
    }

    // -----------------------------------------------------------------------
    //  FLOWS IN - SET DATES
    //  If import dates in other formats are set, we know we are coming
    //  in from another screen, since these attributes are not mapped
    //  to this screen.
    // -----------------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // : URAL and URAA pass dates as 2 numeric fields.
      //   Set up date fields as one MMYYYY numeric field for use in
      //   this program.
      if (import.PassToDateWorkArea.Month == 0 && import
        .PassToDateWorkArea.Year == 0)
      {
      }
      else
      {
        export.ToMonthyear.YearMonth = import.PassToDateWorkArea.Month * 10000
          + import.PassToDateWorkArea.Year;
      }

      if (import.PassFromDateWorkArea.Month == 0 && import
        .PassFromDateWorkArea.Year == 0)
      {
      }
      else
      {
        export.FromMonthyear.YearMonth = import.PassFromDateWorkArea.Month * 10000
          + import.PassFromDateWorkArea.Year;
      }

      // : IMHH, URAH, URAC and CURA pass dates in text format.
      //   Set up date fields as one MMYYYY numeric field for use in
      //   this program.
      if (!IsEmpty(import.PassToDateWorkAttributes.TextMonthYear))
      {
        export.ToMonthyear.YearMonth =
          (int)StringToNumber(import.PassToDateWorkAttributes.TextMonthYear);
      }

      if (!IsEmpty(import.PassFromDateWorkAttributes.TextMonthYear))
      {
        export.FromMonthyear.YearMonth =
          (int)StringToNumber(import.PassFromDateWorkAttributes.TextMonthYear);
      }
    }

    // ---------------------------------------------
    //  FLOWS OUT - SET UP VIEWS
    // ---------------------------------------------
    // : Set up data to pass to other IMHH, URAH, URAC, CURA.  Date needs to be 
    // passed to these 4 screens in a MMYYYY text format.  If a row is selected,
    // send the info in that row instead of the info on the screen header.
    if (Equal(global.Command, "IMHH") || Equal(global.Command, "URAH") || Equal
      (global.Command, "URAC") || Equal(global.Command, "CURA"))
    {
      export.PassFromDateWorkAttributes.TextMonthYear =
        NumberToString(export.FromMonthyear.YearMonth, 6);
      export.PassToDateWorkAttributes.TextMonthYear =
        NumberToString(export.ToMonthyear.YearMonth, 6);

      if (local.SelectCount.Count == 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            export.SearchImHousehold.AeCaseNo =
              export.Export1.Item.ImHousehold.AeCaseNo;
            export.SearchLegalAction.StandardNumber =
              export.Export1.Item.Collection.CourtOrderAppliedTo ?? "";
            export.SearchCase.Number = export.Export1.Item.Case1.Number;
            MoveCsePersonsWorkSet(export.Export1.Item.Member,
              export.SearchMember);

            goto Test1;
          }
        }
      }
    }

Test1:

    if (Equal(global.Command, "URAA"))
    {
      if (local.SelectCount.Count == 0)
      {
        ExitState = "OE0000_SELECTION_MUST_BE_MADE";

        return;
      }
    }

    // : Set up data to pass to URAL and URAA.  Date needs to be passed to these
    // 2 screens in MM YYYY numeric two fields.  If a row is selected, send the
    // info in that row instead of the info on the screen header.
    if (Equal(global.Command, "URAL") || Equal(global.Command, "URAA"))
    {
      export.PassFromDateWorkArea.Month = export.FromMonthyear.YearMonth / 10000
        ;
      export.PassFromDateWorkArea.Year = export.FromMonthyear.YearMonth - export
        .PassFromDateWorkArea.Month * 10000;
      export.PassToDateWorkArea.Month = export.ToMonthyear.YearMonth / 10000;
      export.PassToDateWorkArea.Year = export.ToMonthyear.YearMonth - export
        .PassFromDateWorkArea.Month * 10000;

      if (local.SelectCount.Count == 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            export.SearchImHousehold.AeCaseNo =
              export.Export1.Item.ImHousehold.AeCaseNo;
            export.SearchLegalAction.StandardNumber =
              export.Export1.Item.Collection.CourtOrderAppliedTo ?? "";
            export.SearchCase.Number = export.Export1.Item.Case1.Number;
            export.PassMember.Number = export.Export1.Item.Member.Number;
            MoveCsePersonsWorkSet(export.Export1.Item.Member,
              export.SearchMember);

            goto Test2;
          }
        }
      }

      export.PassMember.Number = export.SearchMember.Number;
    }

Test2:

    // : Set up data to pass to UHMM.  If a row is selected, send AE case in 
    // that row instead of the one on the screen header.
    if (Equal(global.Command, "UHMM"))
    {
      if (local.SelectCount.Count == 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            export.SearchImHousehold.AeCaseNo =
              export.Export1.Item.ImHousehold.AeCaseNo;

            goto Test3;
          }
        }
      }
    }

Test3:

    // : Set up data to pass to PART.  If a row is selected, obligor number in 
    // that row instead of the one on the screen header.
    if (Equal(global.Command, "PART"))
    {
      if (local.SelectCount.Count == 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            MoveCsePersonsWorkSet(export.Export1.Item.Obligor,
              export.PassObligor);

            goto Test4;
          }
        }
      }
      else
      {
        MoveCsePersonsWorkSet(export.SearchObligor, export.PassObligor);
      }
    }

Test4:

    // -----------------------------------------------------------
    // NEXTRAN
    // -----------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      return;

      // ----------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // ----------------------------------------------------------
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      local.NextTranInfo.CsePersonNumber = export.SearchObligor.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "RETNAME"))
    {
      if (AsChar(import.MemberPrompt.SelectChar) == 'S')
      {
        MoveCsePersonsWorkSet(import.PassedFromName, export.SearchMember);
        export.MemberPrompt.SelectChar = "+";
      }
      else
      {
        MoveCsePersonsWorkSet(import.PassedFromName, export.SearchObligor);
        export.ObligorPrompt.SelectChar = "+";
      }

      return;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        MoveCsePersonsWorkSet(import.SaveObligor, export.SearchObligor);
        export.SearchCase.Number = import.SaveCase.Number;
        export.SearchImHousehold.AeCaseNo = import.SaveImHousehold.AeCaseNo;
        export.SearchLegalAction.StandardNumber =
          import.SaveLegalAction.StandardNumber;
        MoveCsePersonsWorkSet(import.SaveMember, export.SearchMember);
        export.Export1.Update.Common.SelectChar = "";
      }

      return;
    }

    if (Equal(global.Command, "RETCOMN"))
    {
      export.CasePrompt.SelectChar = "+";

      return;
    }

    if (Equal(global.Command, "RETLACS"))
    {
      export.CourtOrderPrompt.SelectChar = "+";

      return;
    }

    // -----------------------------------------------------------
    // CHECK SECURITY
    // -----------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.SearchCase.Number) && IsEmpty
          (export.SearchImHousehold.AeCaseNo) && IsEmpty
          (export.SearchLegalAction.StandardNumber) && IsEmpty
          (export.SearchMember.Number) && IsEmpty(export.SearchObligor.Number))
        {
          var field1 = GetField(export.SearchCase, "number");

          field1.Error = true;

          var field2 = GetField(export.SearchImHousehold, "aeCaseNo");

          field2.Error = true;

          var field3 = GetField(export.SearchLegalAction, "standardNumber");

          field3.Error = true;

          var field4 = GetField(export.SearchMember, "number");

          field4.Error = true;

          var field5 = GetField(export.SearchObligor, "number");

          field5.Error = true;

          ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

          return;
        }

        if (!IsEmpty(export.SearchMember.Number))
        {
          local.TextWorkArea.Text10 = export.SearchMember.Number;
          UseEabPadLeftWithZeros();
          export.SearchMember.Number = local.TextWorkArea.Text10;
        }

        if (!IsEmpty(export.SearchObligor.Number))
        {
          local.TextWorkArea.Text10 = export.SearchObligor.Number;
          UseEabPadLeftWithZeros();
          export.SearchObligor.Number = local.TextWorkArea.Text10;
        }

        if (!IsEmpty(export.SearchCase.Number))
        {
          local.TextWorkArea.Text10 = export.SearchCase.Number;
          UseEabPadLeftWithZeros();
          export.SearchCase.Number = local.TextWorkArea.Text10;
        }

        if (export.FromMonthyear.YearMonth == 0)
        {
          if (export.ToMonthyear.YearMonth == 0)
          {
            // : Neither date has been entered.
            export.FromMonthyear.YearMonth = Month(local.Default1.Date) * 10000
              + Year(local.Default1.Date);
            export.ToMonthyear.YearMonth = Month(local.Current.Date) * 10000 + Year
              (local.Current.Date);
          }
          else
          {
            // : To date was entered, from date was not.  Default from date to '
            // to' date - 5 years.
            // : Check for invalid 'to' month before using this date to set '
            // from'.
            local.Month.Count = export.ToMonthyear.YearMonth / 10000;

            if (local.Month.Count < 1 || local.Month.Count > 12)
            {
              ExitState = "INVALID_MONTH_ENTERED";

              var field = GetField(export.ToMonthyear, "yearMonth");

              field.Error = true;

              return;
            }

            export.FromMonthyear.YearMonth = export.ToMonthyear.YearMonth - local
              .DefaultYearsToSubtract.Count;
          }
        }
        else if (export.ToMonthyear.YearMonth == 0)
        {
          // : If no 'to month/year' entered, default it to current month/year.
          export.ToMonthyear.YearMonth = Month(local.Current.Date) * 10000 + Year
            (local.Current.Date);
        }
        else
        {
          // : Both dates have been entered - check for valid month.
          // : Check for invalid 'from' month.
          local.Month.Count = export.FromMonthyear.YearMonth / 10000;

          if (local.Month.Count < 1 || local.Month.Count > 12)
          {
            ExitState = "INVALID_MONTH_ENTERED";

            var field = GetField(export.FromMonthyear, "yearMonth");

            field.Error = true;
          }

          // : Check for invalid 'to' month.
          local.Month.Count = export.ToMonthyear.YearMonth / 10000;

          if (local.Month.Count < 1 || local.Month.Count > 12)
          {
            ExitState = "INVALID_MONTH_ENTERED";

            var field = GetField(export.ToMonthyear, "yearMonth");

            field.Error = true;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        local.From.Month = export.FromMonthyear.YearMonth / 10000;
        local.From.Year = export.FromMonthyear.YearMonth - local.From.Month * 10000
          ;
        local.To.Month = export.ToMonthyear.YearMonth / 10000;
        local.To.Year = export.ToMonthyear.YearMonth - local.To.Month * 10000;

        // : Need to set a local to and from date in YYYYMM format in order to 
        // compare dates during data retrieval.  (unfortunately we have to put
        // it as MMYYYY on the screen).
        local.From.YearMonth = local.From.Year * 100 + local.From.Month;
        local.To.YearMonth = local.To.Year * 100 + local.To.Month;

        // : Check for from month/year greater than to month/year.
        if (local.From.YearMonth > local.To.YearMonth)
        {
          var field1 = GetField(export.FromMonthyear, "yearMonth");

          field1.Error = true;

          var field2 = GetField(export.ToMonthyear, "yearMonth");

          field2.Error = true;

          ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

          return;
        }

        // mca
        // -------------------------------------------------
        // 2-27-02
        // Mark Ashworth    Feb, 2002         PR138462, Added collection filter.
        // Xcopied code from Payr.
        // -----------------------------------------------------------
        // mca
        // -------------------------------------------------
        // 2-27-02
        // *** From and To Dates ***
        // When blank, default From Date to first day of prev month
        // and To Date to last day of current month.
        // When From Date is supplied, it will be reset to first day of entered 
        // month.
        // When To Date is supplied, it will be reset to last day of
        // entered month.
        // -----------------------------------------------------------
        if (Equal(export.SearchCollFrom.Date, local.NullValue.Date))
        {
        }
        else
        {
          UseCabFirstAndLastDateOfMonth1();
        }

        if (Equal(export.SearchCollTo.Date, local.NullValue.Date))
        {
          UseCabFirstAndLastDateOfMonth3();
        }
        else
        {
          UseCabFirstAndLastDateOfMonth2();
        }

        // *** Validate Dates ***
        if (Lt(export.SearchCollTo.Date, export.SearchCollFrom.Date))
        {
          ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

          var field1 = GetField(export.SearchCollFrom, "date");

          field1.Error = true;

          var field2 = GetField(export.SearchCollTo, "date");

          field2.Error = true;

          return;
        }

        UseOeUcolRetrieveUraCollections();

        if (IsExitState("IM_HOUSEHOLD_NF"))
        {
          var field = GetField(export.SearchImHousehold, "aeCaseNo");

          field.Error = true;
        }
        else if (IsExitState("FN0000_OBLIGOR_CSE_PERSON_NF"))
        {
          var field = GetField(export.SearchObligor, "number");

          field.Error = true;
        }
        else if (IsExitState("CASE_NF"))
        {
          var field = GetField(export.SearchCase, "number");

          field.Error = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.SearchMember, "number");

          field.Error = true;
        }
        else
        {
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
          }
          else if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else
        {
        }

        break;
      case "LIST":
        if (IsEmpty(export.CasePrompt.SelectChar) || AsChar
          (export.CasePrompt.SelectChar) == '+')
        {
        }
        else if (AsChar(export.CasePrompt.SelectChar) == 'S')
        {
          ++local.PromptCount.Count;
        }
        else
        {
          var field = GetField(export.CasePrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        if (IsEmpty(export.CourtOrderPrompt.SelectChar) || AsChar
          (export.CourtOrderPrompt.SelectChar) == '+')
        {
        }
        else if (AsChar(export.CourtOrderPrompt.SelectChar) == 'S')
        {
          ++local.PromptCount.Count;
        }
        else
        {
          var field = GetField(export.CourtOrderPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        if (IsEmpty(export.MemberPrompt.SelectChar) || AsChar
          (export.MemberPrompt.SelectChar) == '+')
        {
        }
        else if (AsChar(export.MemberPrompt.SelectChar) == 'S')
        {
          ++local.PromptCount.Count;
        }
        else
        {
          var field = GetField(export.MemberPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        if (IsEmpty(export.ObligorPrompt.SelectChar) || AsChar
          (export.ObligorPrompt.SelectChar) == '+')
        {
        }
        else if (AsChar(export.ObligorPrompt.SelectChar) == 'S')
        {
          ++local.PromptCount.Count;
        }
        else
        {
          var field = GetField(export.ObligorPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.PromptCount.Count == 0)
        {
          var field1 = GetField(export.ObligorPrompt, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.MemberPrompt, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.CasePrompt, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.CourtOrderPrompt, "selectChar");

          field4.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }

        if (local.PromptCount.Count > 1)
        {
          if (IsEmpty(export.ObligorPrompt.SelectChar) || AsChar
            (export.ObligorPrompt.SelectChar) == '+')
          {
          }
          else
          {
            var field = GetField(export.ObligorPrompt, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.MemberPrompt.SelectChar) || AsChar
            (export.MemberPrompt.SelectChar) == '+')
          {
          }
          else
          {
            var field = GetField(export.MemberPrompt, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.CasePrompt.SelectChar) || AsChar
            (export.CasePrompt.SelectChar) == '+')
          {
          }
          else
          {
            var field = GetField(export.CasePrompt, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.CourtOrderPrompt.SelectChar) || AsChar
            (export.CourtOrderPrompt.SelectChar) == '+')
          {
          }
          else
          {
            var field = GetField(export.CourtOrderPrompt, "selectChar");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.ObligorPrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

          return;
        }

        if (AsChar(export.MemberPrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

          return;
        }

        if (AsChar(export.CasePrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_COMN";

          return;
        }

        if (AsChar(export.CourtOrderPrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_LST_LEG_ACT_BY_CSE_PERSN";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "CURA":
        ExitState = "ECO_LNK_TO_CURA";

        break;
      case "URAH":
        ExitState = "ECO_LNK_TO_URAH";

        break;
      case "URAC":
        ExitState = "ECO_LNK_TO_URAC";

        break;
      case "IMHH":
        ExitState = "ECO_LNK_TO_IMHH";

        break;
      case "MURA":
        ExitState = "ECO_LNK_TO_MURA";

        break;
      case "URAA":
        ExitState = "ECO_LNK_TO_URAA";

        break;
      case "URAL":
        ExitState = "ECO_LNK_TO_URAL";

        break;
      case "PART":
        ExitState = "ECO_LNK_TO_CASE_PARTICIPATION";

        break;
      case "UHMM":
        ExitState = "ECO_LNK_TO_UHMM";

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

  private static void MoveExport1(OeUcolRetrieveUraCollections.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.Collection.Assign(source.Collection);
    MoveUraCollectionApplication(source.UraCollectionApplication,
      target.UraCollectionApplication);
    target.CollectionType.Code = source.CollectionType.Code;
    target.ImHousehold.AeCaseNo = source.ImHousehold.AeCaseNo;
    target.Case1.Number = source.Case1.Number;
    MoveCsePersonsWorkSet(source.Obligor, target.Obligor);
    MoveCsePersonsWorkSet(source.Member, target.Member);
    MoveCsePersonsWorkSet(source.Supported, target.Supported);
    MoveImHouseholdMbrMnthlySum(source.ImHouseholdMbrMnthlySum,
      target.ImHouseholdMbrMnthlySum);
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private static void MoveUraCollectionApplication(
    UraCollectionApplication source, UraCollectionApplication target)
  {
    target.CollectionAmountApplied = source.CollectionAmountApplied;
    target.Type1 = source.Type1;
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = import.SearchCollFrom.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchCollFrom.Date = useExport.First.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = import.SearchCollTo.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchCollTo.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth3()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchCollTo.Date = useExport.Last.Date;
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

  private void UseOeUcolRetrieveUraCollections()
  {
    var useImport = new OeUcolRetrieveUraCollections.Import();
    var useExport = new OeUcolRetrieveUraCollections.Export();

    useImport.SearchCollFrom.Date = export.SearchCollFrom.Date;
    useImport.SearchCollTo.Date = export.SearchCollTo.Date;
    useImport.From.Assign(local.From);
    useImport.To.Assign(local.To);
    useImport.Case1.Number = export.SearchCase.Number;
    useImport.ImHousehold.AeCaseNo = export.SearchImHousehold.AeCaseNo;
    useImport.Member.Number = export.SearchMember.Number;
    useImport.Obligor.Number = export.SearchObligor.Number;
    useImport.LegalAction.StandardNumber =
      export.SearchLegalAction.StandardNumber;
    useImport.FromMonthyear.YearMonth = export.FromMonthyear.YearMonth;
    useImport.ToMonthyear.YearMonth = export.ToMonthyear.YearMonth;

    Call(OeUcolRetrieveUraCollections.Execute, useImport, useExport);

    export.SearchImHousehold.AeCaseNo = useExport.ImHousehold.AeCaseNo;
    MoveCsePersonsWorkSet(useExport.Member, export.SearchMember);
    MoveCsePersonsWorkSet(useExport.Obligor, export.SearchObligor);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(local.NextTranInfo);
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

    useImport.CsePersonsWorkSet.Number = export.SearchObligor.Number;

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
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>
      /// A value of UraCollectionApplication.
      /// </summary>
      [JsonPropertyName("uraCollectionApplication")]
      public UraCollectionApplication UraCollectionApplication
      {
        get => uraCollectionApplication ??= new();
        set => uraCollectionApplication = value;
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
      /// A value of ImHousehold.
      /// </summary>
      [JsonPropertyName("imHousehold")]
      public ImHousehold ImHousehold
      {
        get => imHousehold ??= new();
        set => imHousehold = value;
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
      /// A value of Obligor.
      /// </summary>
      [JsonPropertyName("obligor")]
      public CsePersonsWorkSet Obligor
      {
        get => obligor ??= new();
        set => obligor = value;
      }

      /// <summary>
      /// A value of Member.
      /// </summary>
      [JsonPropertyName("member")]
      public CsePersonsWorkSet Member
      {
        get => member ??= new();
        set => member = value;
      }

      /// <summary>
      /// A value of Supported.
      /// </summary>
      [JsonPropertyName("supported")]
      public CsePersonsWorkSet Supported
      {
        get => supported ??= new();
        set => supported = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private Common common;
      private Collection collection;
      private UraCollectionApplication uraCollectionApplication;
      private CollectionType collectionType;
      private ImHousehold imHousehold;
      private Case1 case1;
      private CsePersonsWorkSet obligor;
      private CsePersonsWorkSet member;
      private CsePersonsWorkSet supported;
      private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchImHousehold.
    /// </summary>
    [JsonPropertyName("searchImHousehold")]
    public ImHousehold SearchImHousehold
    {
      get => searchImHousehold ??= new();
      set => searchImHousehold = value;
    }

    /// <summary>
    /// A value of SearchMemberCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchMemberCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchMemberCsePersonsWorkSet
    {
      get => searchMemberCsePersonsWorkSet ??= new();
      set => searchMemberCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchObligor.
    /// </summary>
    [JsonPropertyName("searchObligor")]
    public CsePersonsWorkSet SearchObligor
    {
      get => searchObligor ??= new();
      set => searchObligor = value;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    /// <summary>
    /// A value of MemberPrompt.
    /// </summary>
    [JsonPropertyName("memberPrompt")]
    public Common MemberPrompt
    {
      get => memberPrompt ??= new();
      set => memberPrompt = value;
    }

    /// <summary>
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
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
    /// A value of PreviousMember.
    /// </summary>
    [JsonPropertyName("previousMember")]
    public CsePersonsWorkSet PreviousMember
    {
      get => previousMember ??= new();
      set => previousMember = value;
    }

    /// <summary>
    /// A value of CasePrompt.
    /// </summary>
    [JsonPropertyName("casePrompt")]
    public Common CasePrompt
    {
      get => casePrompt ??= new();
      set => casePrompt = value;
    }

    /// <summary>
    /// A value of FromMonthyear.
    /// </summary>
    [JsonPropertyName("fromMonthyear")]
    public DateWorkArea FromMonthyear
    {
      get => fromMonthyear ??= new();
      set => fromMonthyear = value;
    }

    /// <summary>
    /// A value of ToMonthyear.
    /// </summary>
    [JsonPropertyName("toMonthyear")]
    public DateWorkArea ToMonthyear
    {
      get => toMonthyear ??= new();
      set => toMonthyear = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PassFromDateWorkArea.
    /// </summary>
    [JsonPropertyName("passFromDateWorkArea")]
    public DateWorkArea PassFromDateWorkArea
    {
      get => passFromDateWorkArea ??= new();
      set => passFromDateWorkArea = value;
    }

    /// <summary>
    /// A value of PassFromDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("passFromDateWorkAttributes")]
    public DateWorkAttributes PassFromDateWorkAttributes
    {
      get => passFromDateWorkAttributes ??= new();
      set => passFromDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of PassToDateWorkArea.
    /// </summary>
    [JsonPropertyName("passToDateWorkArea")]
    public DateWorkArea PassToDateWorkArea
    {
      get => passToDateWorkArea ??= new();
      set => passToDateWorkArea = value;
    }

    /// <summary>
    /// A value of PassToDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("passToDateWorkAttributes")]
    public DateWorkAttributes PassToDateWorkAttributes
    {
      get => passToDateWorkAttributes ??= new();
      set => passToDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of PassedFromName.
    /// </summary>
    [JsonPropertyName("passedFromName")]
    public CsePersonsWorkSet PassedFromName
    {
      get => passedFromName ??= new();
      set => passedFromName = value;
    }

    /// <summary>
    /// A value of SaveLegalAction.
    /// </summary>
    [JsonPropertyName("saveLegalAction")]
    public LegalAction SaveLegalAction
    {
      get => saveLegalAction ??= new();
      set => saveLegalAction = value;
    }

    /// <summary>
    /// A value of SaveObligor.
    /// </summary>
    [JsonPropertyName("saveObligor")]
    public CsePersonsWorkSet SaveObligor
    {
      get => saveObligor ??= new();
      set => saveObligor = value;
    }

    /// <summary>
    /// A value of SaveCase.
    /// </summary>
    [JsonPropertyName("saveCase")]
    public Case1 SaveCase
    {
      get => saveCase ??= new();
      set => saveCase = value;
    }

    /// <summary>
    /// A value of SaveImHousehold.
    /// </summary>
    [JsonPropertyName("saveImHousehold")]
    public ImHousehold SaveImHousehold
    {
      get => saveImHousehold ??= new();
      set => saveImHousehold = value;
    }

    /// <summary>
    /// A value of SaveMember.
    /// </summary>
    [JsonPropertyName("saveMember")]
    public CsePersonsWorkSet SaveMember
    {
      get => saveMember ??= new();
      set => saveMember = value;
    }

    /// <summary>
    /// A value of SearchMemberCsePerson.
    /// </summary>
    [JsonPropertyName("searchMemberCsePerson")]
    public CsePerson SearchMemberCsePerson
    {
      get => searchMemberCsePerson ??= new();
      set => searchMemberCsePerson = value;
    }

    /// <summary>
    /// A value of ZdimportSearchObligor.
    /// </summary>
    [JsonPropertyName("zdimportSearchObligor")]
    public CsePerson ZdimportSearchObligor
    {
      get => zdimportSearchObligor ??= new();
      set => zdimportSearchObligor = value;
    }

    /// <summary>
    /// A value of SearchCollFrom.
    /// </summary>
    [JsonPropertyName("searchCollFrom")]
    public DateWorkArea SearchCollFrom
    {
      get => searchCollFrom ??= new();
      set => searchCollFrom = value;
    }

    /// <summary>
    /// A value of SearchCollTo.
    /// </summary>
    [JsonPropertyName("searchCollTo")]
    public DateWorkArea SearchCollTo
    {
      get => searchCollTo ??= new();
      set => searchCollTo = value;
    }

    private Case1 searchCase;
    private ImHousehold searchImHousehold;
    private CsePersonsWorkSet searchMemberCsePersonsWorkSet;
    private CsePersonsWorkSet searchObligor;
    private LegalAction searchLegalAction;
    private Common memberPrompt;
    private Common obligorPrompt;
    private Common courtOrderPrompt;
    private CsePersonsWorkSet previousMember;
    private Common casePrompt;
    private DateWorkArea fromMonthyear;
    private DateWorkArea toMonthyear;
    private Array<ImportGroup> import1;
    private Standard standard;
    private DateWorkArea passFromDateWorkArea;
    private DateWorkAttributes passFromDateWorkAttributes;
    private DateWorkArea passToDateWorkArea;
    private DateWorkAttributes passToDateWorkAttributes;
    private CsePersonsWorkSet passedFromName;
    private LegalAction saveLegalAction;
    private CsePersonsWorkSet saveObligor;
    private Case1 saveCase;
    private ImHousehold saveImHousehold;
    private CsePersonsWorkSet saveMember;
    private CsePerson searchMemberCsePerson;
    private CsePerson zdimportSearchObligor;
    private DateWorkArea searchCollFrom;
    private DateWorkArea searchCollTo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>
      /// A value of UraCollectionApplication.
      /// </summary>
      [JsonPropertyName("uraCollectionApplication")]
      public UraCollectionApplication UraCollectionApplication
      {
        get => uraCollectionApplication ??= new();
        set => uraCollectionApplication = value;
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
      /// A value of ImHousehold.
      /// </summary>
      [JsonPropertyName("imHousehold")]
      public ImHousehold ImHousehold
      {
        get => imHousehold ??= new();
        set => imHousehold = value;
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
      /// A value of Obligor.
      /// </summary>
      [JsonPropertyName("obligor")]
      public CsePersonsWorkSet Obligor
      {
        get => obligor ??= new();
        set => obligor = value;
      }

      /// <summary>
      /// A value of Member.
      /// </summary>
      [JsonPropertyName("member")]
      public CsePersonsWorkSet Member
      {
        get => member ??= new();
        set => member = value;
      }

      /// <summary>
      /// A value of Supported.
      /// </summary>
      [JsonPropertyName("supported")]
      public CsePersonsWorkSet Supported
      {
        get => supported ??= new();
        set => supported = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private Common common;
      private Collection collection;
      private UraCollectionApplication uraCollectionApplication;
      private CollectionType collectionType;
      private ImHousehold imHousehold;
      private Case1 case1;
      private CsePersonsWorkSet obligor;
      private CsePersonsWorkSet member;
      private CsePersonsWorkSet supported;
      private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchImHousehold.
    /// </summary>
    [JsonPropertyName("searchImHousehold")]
    public ImHousehold SearchImHousehold
    {
      get => searchImHousehold ??= new();
      set => searchImHousehold = value;
    }

    /// <summary>
    /// A value of MemberPrompt.
    /// </summary>
    [JsonPropertyName("memberPrompt")]
    public Common MemberPrompt
    {
      get => memberPrompt ??= new();
      set => memberPrompt = value;
    }

    /// <summary>
    /// A value of SearchMember.
    /// </summary>
    [JsonPropertyName("searchMember")]
    public CsePersonsWorkSet SearchMember
    {
      get => searchMember ??= new();
      set => searchMember = value;
    }

    /// <summary>
    /// A value of SearchObligor.
    /// </summary>
    [JsonPropertyName("searchObligor")]
    public CsePersonsWorkSet SearchObligor
    {
      get => searchObligor ??= new();
      set => searchObligor = value;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    /// <summary>
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
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
    /// A value of CasePrompt.
    /// </summary>
    [JsonPropertyName("casePrompt")]
    public Common CasePrompt
    {
      get => casePrompt ??= new();
      set => casePrompt = value;
    }

    /// <summary>
    /// A value of PreviousMember.
    /// </summary>
    [JsonPropertyName("previousMember")]
    public CsePersonsWorkSet PreviousMember
    {
      get => previousMember ??= new();
      set => previousMember = value;
    }

    /// <summary>
    /// A value of FromMonthyear.
    /// </summary>
    [JsonPropertyName("fromMonthyear")]
    public DateWorkArea FromMonthyear
    {
      get => fromMonthyear ??= new();
      set => fromMonthyear = value;
    }

    /// <summary>
    /// A value of ToMonthyear.
    /// </summary>
    [JsonPropertyName("toMonthyear")]
    public DateWorkArea ToMonthyear
    {
      get => toMonthyear ??= new();
      set => toMonthyear = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PassFromDateWorkArea.
    /// </summary>
    [JsonPropertyName("passFromDateWorkArea")]
    public DateWorkArea PassFromDateWorkArea
    {
      get => passFromDateWorkArea ??= new();
      set => passFromDateWorkArea = value;
    }

    /// <summary>
    /// A value of PassFromDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("passFromDateWorkAttributes")]
    public DateWorkAttributes PassFromDateWorkAttributes
    {
      get => passFromDateWorkAttributes ??= new();
      set => passFromDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of PassToDateWorkArea.
    /// </summary>
    [JsonPropertyName("passToDateWorkArea")]
    public DateWorkArea PassToDateWorkArea
    {
      get => passToDateWorkArea ??= new();
      set => passToDateWorkArea = value;
    }

    /// <summary>
    /// A value of PassToDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("passToDateWorkAttributes")]
    public DateWorkAttributes PassToDateWorkAttributes
    {
      get => passToDateWorkAttributes ??= new();
      set => passToDateWorkAttributes = value;
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
    /// A value of PassObligor.
    /// </summary>
    [JsonPropertyName("passObligor")]
    public CsePersonsWorkSet PassObligor
    {
      get => passObligor ??= new();
      set => passObligor = value;
    }

    /// <summary>
    /// A value of SaveLegalAction.
    /// </summary>
    [JsonPropertyName("saveLegalAction")]
    public LegalAction SaveLegalAction
    {
      get => saveLegalAction ??= new();
      set => saveLegalAction = value;
    }

    /// <summary>
    /// A value of SaveObligor.
    /// </summary>
    [JsonPropertyName("saveObligor")]
    public CsePersonsWorkSet SaveObligor
    {
      get => saveObligor ??= new();
      set => saveObligor = value;
    }

    /// <summary>
    /// A value of SaveCase.
    /// </summary>
    [JsonPropertyName("saveCase")]
    public Case1 SaveCase
    {
      get => saveCase ??= new();
      set => saveCase = value;
    }

    /// <summary>
    /// A value of SaveImHousehold.
    /// </summary>
    [JsonPropertyName("saveImHousehold")]
    public ImHousehold SaveImHousehold
    {
      get => saveImHousehold ??= new();
      set => saveImHousehold = value;
    }

    /// <summary>
    /// A value of SaveMember.
    /// </summary>
    [JsonPropertyName("saveMember")]
    public CsePersonsWorkSet SaveMember
    {
      get => saveMember ??= new();
      set => saveMember = value;
    }

    /// <summary>
    /// A value of PassMember.
    /// </summary>
    [JsonPropertyName("passMember")]
    public CsePerson PassMember
    {
      get => passMember ??= new();
      set => passMember = value;
    }

    /// <summary>
    /// A value of SearchCollFrom.
    /// </summary>
    [JsonPropertyName("searchCollFrom")]
    public DateWorkArea SearchCollFrom
    {
      get => searchCollFrom ??= new();
      set => searchCollFrom = value;
    }

    /// <summary>
    /// A value of SearchCollTo.
    /// </summary>
    [JsonPropertyName("searchCollTo")]
    public DateWorkArea SearchCollTo
    {
      get => searchCollTo ??= new();
      set => searchCollTo = value;
    }

    private Case1 searchCase;
    private ImHousehold searchImHousehold;
    private Common memberPrompt;
    private CsePersonsWorkSet searchMember;
    private CsePersonsWorkSet searchObligor;
    private LegalAction searchLegalAction;
    private Common obligorPrompt;
    private Common courtOrderPrompt;
    private Common casePrompt;
    private CsePersonsWorkSet previousMember;
    private DateWorkArea fromMonthyear;
    private DateWorkArea toMonthyear;
    private Array<ExportGroup> export1;
    private Standard standard;
    private DateWorkArea passFromDateWorkArea;
    private DateWorkAttributes passFromDateWorkAttributes;
    private DateWorkArea passToDateWorkArea;
    private DateWorkAttributes passToDateWorkAttributes;
    private Case1 case1;
    private CsePersonsWorkSet passObligor;
    private LegalAction saveLegalAction;
    private CsePersonsWorkSet saveObligor;
    private Case1 saveCase;
    private ImHousehold saveImHousehold;
    private CsePersonsWorkSet saveMember;
    private CsePerson passMember;
    private DateWorkArea searchCollFrom;
    private DateWorkArea searchCollTo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DefaultYearsToSubtract.
    /// </summary>
    [JsonPropertyName("defaultYearsToSubtract")]
    public Common DefaultYearsToSubtract
    {
      get => defaultYearsToSubtract ??= new();
      set => defaultYearsToSubtract = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public DateWorkArea Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of SelectCount.
    /// </summary>
    [JsonPropertyName("selectCount")]
    public Common SelectCount
    {
      get => selectCount ??= new();
      set => selectCount = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of NullValue.
    /// </summary>
    [JsonPropertyName("nullValue")]
    public DateWorkArea NullValue
    {
      get => nullValue ??= new();
      set => nullValue = value;
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
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public Common Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    private Common defaultYearsToSubtract;
    private DateWorkArea default1;
    private Common selectCount;
    private Common promptCount;
    private TextWorkArea textWorkArea;
    private DateWorkArea current;
    private DateWorkArea nullValue;
    private NextTranInfo nextTranInfo;
    private Common month;
    private DateWorkArea from;
    private DateWorkArea to;
  }
#endregion
}

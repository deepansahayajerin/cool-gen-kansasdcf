// Program: OE_BSSN_BAD_SSN_MAINTENANCE, ID: 371153581, model: 746.
// Short name: SWEBSSNP
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
/// A program: OE_BSSN_BAD_SSN_MAINTENANCE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step action block lists the Obligor
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeBssnBadSsnMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BSSN_BAD_SSN_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBssnBadSsnMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBssnBadSsnMaintenance.
  /// </summary>
  public OeBssnBadSsnMaintenance(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    // Date			Developer	Request #
    // 04-23-2009		DDupree
    // Initial development
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      // transfer to PERM screen (Person Management Menu)
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      // -------------
      // begin group F
      // -------------
      UseScCabSignoff();

      return;

      // -------------
      // end   group F
      // -------------
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      return;
    }

    if (Equal(global.Command, "DONE"))
    {
      if (Equal(import.SortCsePersonsWorkSet.Number, import.Flow.Number) && !
        IsEmpty(import.SortCsePersonsWorkSet.Number))
      {
        export.SortCsePersonsWorkSet.Assign(import.SortCsePersonsWorkSet);
      }
      else if (!IsEmpty(import.Flow.Number))
      {
        export.SortCsePersonsWorkSet.Number = import.Flow.Number;
      }
      else
      {
        return;
      }

      global.Command = "DISPLAY";
    }
    else
    {
      export.SortCsePersonsWorkSet.Assign(import.SortCsePersonsWorkSet);
    }

    export.MoreLessScroll.Text3 = import.MoreLessScroll.Text3;
    export.PageCount.Count = import.PageCount.Count;
    MoveCsePersonsWorkSet1(import.HiddenPrev, export.HiddenPrev);
    export.PromptCsePersonNumber.SelectChar =
      import.PromptCsePersonNumber.SelectChar;
    export.SortSsnWorkArea.Assign(import.SortSsnWorkArea);
    export.From.Date = import.From.Date;
    export.To.Date = import.To.Date;
    export.FromPrevious.Date = import.FromPrevious.Date;
    export.ToPrevious.Date = import.ToPrevious.Date;
    export.FirstPassAdd.Flag = import.FirstAddPass.Flag;
    local.BlankStartDate.Date = new DateTime(1, 1, 1);
    local.Current.Date = Now().Date;

    if (!IsEmpty(import.SortCsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = import.SortCsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.SortCsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }
    else
    {
      export.SortCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // -------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // -------------------------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.SortCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    if (!IsEmpty(import.SortSsnWorkArea.SsnTextPart1) && !
      IsEmpty(import.SortSsnWorkArea.SsnTextPart2) && !
      IsEmpty(import.SortSsnWorkArea.SsnTextPart3))
    {
      if (Verify(import.SortSsnWorkArea.SsnTextPart1, "0123456789") > 0)
      {
        var field = GetField(export.SortSsnWorkArea, "ssnTextPart1");

        field.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      if (Length(TrimEnd(import.SortSsnWorkArea.SsnTextPart1)) < 3)
      {
        var field = GetField(export.SortSsnWorkArea, "ssnTextPart1");

        field.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      if (Verify(import.SortSsnWorkArea.SsnTextPart2, "0123456789") > 0)
      {
        var field = GetField(export.SortSsnWorkArea, "ssnTextPart2");

        field.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      if (Length(TrimEnd(import.SortSsnWorkArea.SsnTextPart2)) < 2)
      {
        var field = GetField(export.SortSsnWorkArea, "ssnTextPart2");

        field.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      if (Verify(import.SortSsnWorkArea.SsnTextPart3, "0123456789") > 0)
      {
        var field = GetField(export.SortSsnWorkArea, "ssnTextPart3");

        field.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      if (Length(TrimEnd(import.SortSsnWorkArea.SsnTextPart3)) < 4)
      {
        var field = GetField(export.SortSsnWorkArea, "ssnTextPart3");

        field.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      if (Equal(export.SortSsnWorkArea.SsnTextPart1, "000") && Equal
        (import.SortSsnWorkArea.SsnTextPart2, "00") && Equal
        (import.SortSsnWorkArea.SsnTextPart3, "0000"))
      {
        var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

        field1.Error = true;

        var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

        field2.Error = true;

        var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

        field3.Error = true;

        ExitState = "OE_INVALID_SSN";

        return;
      }

      if (Equal(export.SortSsnWorkArea.SsnTextPart1, "666") && Equal
        (import.SortSsnWorkArea.SsnTextPart2, "66") && Equal
        (import.SortSsnWorkArea.SsnTextPart3, "6666"))
      {
        var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

        field1.Error = true;

        var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

        field2.Error = true;

        var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

        field3.Error = true;

        ExitState = "OE_INVALID_SSN";

        return;
      }

      if (Equal(export.SortSsnWorkArea.SsnTextPart1, "999") && Equal
        (import.SortSsnWorkArea.SsnTextPart2, "99") && Equal
        (import.SortSsnWorkArea.SsnTextPart3, "9999"))
      {
        var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

        field1.Error = true;

        var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

        field2.Error = true;

        var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

        field3.Error = true;

        ExitState = "OE_INVALID_SSN";

        return;
      }

      export.SortCsePersonsWorkSet.Ssn = import.SortSsnWorkArea.SsnTextPart1 + import
        .SortSsnWorkArea.SsnTextPart2 + import.SortSsnWorkArea.SsnTextPart3;
    }
    else
    {
      if (!IsEmpty(import.SortSsnWorkArea.SsnTextPart1) || !
        IsEmpty(import.SortSsnWorkArea.SsnTextPart2) || !
        IsEmpty(import.SortSsnWorkArea.SsnTextPart3))
      {
        if (IsEmpty(import.SortSsnWorkArea.SsnTextPart1))
        {
          var field = GetField(export.SortSsnWorkArea, "ssnTextPart1");

          field.Error = true;
        }

        if (IsEmpty(import.SortSsnWorkArea.SsnTextPart2))
        {
          var field = GetField(export.SortSsnWorkArea, "ssnTextPart2");

          field.Error = true;
        }

        if (IsEmpty(import.SortSsnWorkArea.SsnTextPart3))
        {
          var field = GetField(export.SortSsnWorkArea, "ssnTextPart3");

          field.Error = true;
        }

        ExitState = "LE0000_SSN_HAS_LT_9_CHARS";

        return;
      }

      export.SortCsePersonsWorkSet.Ssn = "";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.SortCsePersonsWorkSet.Ssn = import.SortSsnWorkArea.SsnTextPart1 + import
        .SortSsnWorkArea.SsnTextPart2 + import.SortSsnWorkArea.SsnTextPart3;

      // this will be a group view based on the new table  - invalid ssn
      if (!import.Display.IsEmpty)
      {
        export.Display.Index = -1;
        export.Display.Count = 0;

        for(import.Display.Index = 0; import.Display.Index < import
          .Display.Count; ++import.Display.Index)
        {
          if (!import.Display.CheckSize())
          {
            break;
          }

          ++export.Display.Index;
          export.Display.CheckSize();

          export.Display.Update.CsePersonsWorkSet.Assign(
            import.Display.Item.CsePersonsWorkSet);
          MoveInvalidSsn(import.Display.Item.InvalidSsn,
            export.Display.Update.InvalidSsn);
          export.Display.Update.DateWorkArea.Date =
            import.Display.Item.DateWorkArea.Date;
          export.Display.Update.Common.SelectChar =
            import.Display.Item.Common.SelectChar;
          export.Display.Update.SsnWorkArea.Assign(
            import.Display.Item.SsnWorkArea);
        }

        import.Display.CheckIndex();
      }

      if (!import.Paging.IsEmpty)
      {
        export.Paging.Index = -1;
        export.Paging.Count = 0;

        for(import.Paging.Index = 0; import.Paging.Index < import.Paging.Count; ++
          import.Paging.Index)
        {
          if (!import.Paging.CheckSize())
          {
            break;
          }

          ++export.Paging.Index;
          export.Paging.CheckSize();

          MoveCsePersonsWorkSet3(import.Paging.Item.PageCsePersonsWorkSet,
            export.Paging.Update.PageCsePersonsWorkSet);
          export.Paging.Update.PageInvalidSsn.Ssn =
            import.Paging.Item.PageInvalidSsn.Ssn;
          export.Paging.Update.PageDateWorkArea.Date =
            import.Paging.Item.PageDateWorkArea.Date;
          export.Paging.Update.PageFrom.Date = import.Paging.Item.PageFrom.Date;
          export.Paging.Update.PageTo.Date = import.Paging.Item.PageTo.Date;
        }

        import.Paging.CheckIndex();
      }
    }
    else
    {
      for(export.Display.Index = 0; export.Display.Index < export
        .Display.Count; ++export.Display.Index)
      {
        if (!export.Display.CheckSize())
        {
          break;
        }

        export.Display.Update.CsePersonsWorkSet.Assign(
          local.ClearCsePersonsWorkSet);
        MoveInvalidSsn(local.ClearInvalidSsn, export.Display.Update.InvalidSsn);
        export.Display.Update.DateWorkArea.Date = local.ClearDateWorkArea.Date;
        export.Display.Update.Common.SelectChar = local.ClearCommon.SelectChar;
        export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
      }

      export.Display.CheckIndex();
      export.Display.Index = -1;
      export.Display.Count = 0;

      for(export.Paging.Index = 0; export.Paging.Index < export.Paging.Count; ++
        export.Paging.Index)
      {
        if (!export.Paging.CheckSize())
        {
          break;
        }

        MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
          export.Paging.Update.PageCsePersonsWorkSet);
        export.Paging.Update.PageInvalidSsn.Ssn = local.ClearInvalidSsn.Ssn;
        export.Paging.Update.PageDateWorkArea.Date =
          local.ClearDateWorkArea.Date;
      }

      export.Paging.CheckIndex();
      export.Paging.Index = -1;
      export.Paging.Count = 0;
      export.FirstPassAdd.Flag = "";
      export.PromptCsePersonNumber.SelectChar = "";
      export.FromPrevious.Date = local.ClearDateWorkArea.Date;
      export.ToPrevious.Date = local.ClearDateWorkArea.Date;
      MoveCsePersonsWorkSet1(local.ClearCsePersonsWorkSet, export.HiddenPrev);
      export.SortCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
      export.SortCsePersonsWorkSet.Number = local.TextWorkArea.Text10;

      if (!IsEmpty(import.SortSsnWorkArea.SsnTextPart1) && !
        IsEmpty(import.SortSsnWorkArea.SsnTextPart2) && !
        IsEmpty(import.SortSsnWorkArea.SsnTextPart3))
      {
        export.SortCsePersonsWorkSet.Ssn =
          import.SortSsnWorkArea.SsnTextPart1 + import
          .SortSsnWorkArea.SsnTextPart2 + import.SortSsnWorkArea.SsnTextPart3;
      }

      export.PageCount.Count = 0;
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ----------------------------------------------------------
    // The following statements must be placed after MOVE imports
    // to exports
    // ----------------------------------------------------------
    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsEmpty(import.SortCsePersonsWorkSet.Number))
      {
        UseSiReadCsePerson2();

        if (IsEmpty(local.ReadCsePersonsWorkSet.Number))
        {
          export.SortCsePersonsWorkSet.Assign(import.SortCsePersonsWorkSet);
          ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";

          var field = GetField(export.SortCsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        export.SortCsePersonsWorkSet.FormattedName =
          local.ReadCsePersonsWorkSet.FormattedName;
      }
      else
      {
      }

      local.SsnWorkArea.SsnNum9 =
        (int)StringToNumber(export.SortCsePersonsWorkSet.Ssn);

      ++export.Paging.Index;
      export.Paging.CheckSize();

      export.Paging.Update.PageCsePersonsWorkSet.Number =
        export.SortCsePersonsWorkSet.Number;
      export.Paging.Update.PageInvalidSsn.Ssn = local.SsnWorkArea.SsnNum9;
      export.Paging.Update.PageFrom.Date = export.From.Date;
      export.Paging.Update.PageTo.Date = export.To.Date;
      export.Paging.Update.PageDateWorkArea.Date = new DateTime(1, 1, 1);
      MoveCsePersonsWorkSet1(local.Initialised, export.HiddenPrev);
      export.MoreLessScroll.Text3 = "";

      if (!IsEmpty(export.SortCsePersonsWorkSet.Number))
      {
        if (ReadInvalidSsn2())
        {
          // ok proceed
        }
        else if (ReadCsePerson())
        {
          ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";
        }
        else
        {
          ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";

          var field = GetField(export.SortCsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }
      }
      else if (local.SsnWorkArea.SsnNum9 > 0)
      {
        if (ReadInvalidSsn3())
        {
          // ok proceed
        }
        else
        {
          ExitState = "FN0000_NO_RECORDS_FOUND";

          var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          return;
        }
      }
    }

    if (!Equal(global.Command, "LIST"))
    {
      export.PromptCsePersonNumber.SelectChar = "";
    }

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // the only prompt will be for the ap person number - goes to the NAME 
        // screen.
        if (IsEmpty(export.PromptCsePersonNumber.SelectChar))
        {
          var field = GetField(export.PromptCsePersonNumber, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }

        if (!IsEmpty(export.PromptCsePersonNumber.SelectChar) && AsChar
          (export.PromptCsePersonNumber.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptCsePersonNumber, "selectChar");

          field.Error = true;

          return;
        }

        if (!export.Display.IsEmpty)
        {
          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Display.Item.Common.SelectChar))
            {
              var field = GetField(export.Display.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "INVALID_SELECTION_FOR_COMMAND";
            }
          }

          export.Display.CheckIndex();

          if (IsExitState("INVALID_SELECTION_FOR_COMMAND"))
          {
            return;
          }
        }

        // prompt for ap person number - send to NAME screen
        if (AsChar(export.PromptCsePersonNumber.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_SELECT_PERSON";
        }

        break;
      case "DISPLAY":
        MoveCsePersonsWorkSet1(export.SortCsePersonsWorkSet, local.Saved);
        export.From.Date = import.From.Date;
        export.To.Date = import.To.Date;

        if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
          (local.BlankStartDate.Date, import.To.Date))
        {
          // This is to populate the export 'to' or 'from' dates with the date 
          // of the one that has
          // been entered when only one date has been entered.
          if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            // this is  ok no need to move anything since both date ranges were 
            // populated
          }
          else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
            Lt(local.BlankStartDate.Date, import.To.Date))
          {
            export.To.Date = import.From.Date;
          }
          else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            export.From.Date = import.To.Date;
          }
        }

        if (!IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn))
        {
          // getting all invalid ssns for a single cse person
          foreach(var item in ReadInvalidSsnCsePerson3())
          {
            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "A";

            if (!IsEmpty(local.Ap.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ap.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;
            }
            else if (!IsEmpty(local.Ar.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ar.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ar.Number;
            }
            else
            {
              // this is an error
            }

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+";
              ++export.PageCount.Count;

              ++export.Paging.Index;
              export.Paging.CheckSize();

              export.Paging.Update.PageCsePersonsWorkSet.Number =
                export.Display.Item.CsePersonsWorkSet.Number;
              export.Paging.Update.PageDateWorkArea.Date =
                export.Display.Item.DateWorkArea.Date;
              export.Paging.Update.PageInvalidSsn.Ssn =
                export.Display.Item.InvalidSsn.Ssn;
              export.Paging.Update.PageFrom.Date = export.From.Date;
              export.Paging.Update.PageTo.Date = export.To.Date;
              export.Paging.Update.PageCsePersonsWorkSet.Flag = "A";

              goto Test1;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field = GetField(export.SortCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";

            return;
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && !
          IsEmpty(export.SortCsePersonsWorkSet.Ssn))
        {
          local.SsnWorkArea.SsnNum9 =
            (int)StringToNumber(export.SortCsePersonsWorkSet.Ssn);

          foreach(var item in ReadCsePersonInvalidSsn3())
          {
            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            local.Saved.Number = entities.CsePerson.Number;
            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "B";

            if (!IsEmpty(local.Ap.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ap.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;
            }
            else if (!IsEmpty(local.Ar.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ar.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ar.Number;
            }
            else
            {
              // this is an error
            }

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+";
              ++export.PageCount.Count;

              ++export.Paging.Index;
              export.Paging.CheckSize();

              export.Paging.Update.PageCsePersonsWorkSet.Number =
                export.Display.Item.CsePersonsWorkSet.Number;
              export.Paging.Update.PageDateWorkArea.Date =
                export.Display.Item.DateWorkArea.Date;
              export.Paging.Update.PageInvalidSsn.Ssn =
                export.Display.Item.InvalidSsn.Ssn;
              export.Paging.Update.PageFrom.Date = export.From.Date;
              export.Paging.Update.PageTo.Date = export.To.Date;
              export.Paging.Update.PageCsePersonsWorkSet.Flag = "B";

              goto Test1;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

            field1.Error = true;

            var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

            field2.Error = true;

            var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

            field3.Error = true;

            ExitState = "FN0000_NO_RECORDS_FOUND";

            return;
          }
        }
        else if (!IsEmpty(export.SortCsePersonsWorkSet.Number) && !
          IsEmpty(export.SortCsePersonsWorkSet.Ssn))
        {
          // we are reading by the cse person number and the ssn
          local.SsnWorkArea.SsnNum9 =
            (int)StringToNumber(export.SortCsePersonsWorkSet.Ssn);

          if (ReadCsePersonInvalidSsn1())
          {
            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            UseSiReadCsePerson1();

            if (!IsEmpty(local.Ap.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ap.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;
            }
            else if (!IsEmpty(local.Ar.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ar.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ar.Number;
            }
            else
            {
              // this is an error
            }
          }
          else
          {
            ExitState = "FN0000_NO_RECORDS_FOUND";

            var field1 = GetField(export.SortCsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.SortCsePersonsWorkSet, "ssn");

            field2.Error = true;

            return;
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn) && (
            Lt(local.BlankStartDate.Date, import.From.Date) || Lt
          (local.BlankStartDate.Date, import.To.Date)))
        {
          // checking to see if it is in the date range that was entered if 
          // there is one
          local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

          if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(export.To.Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.To.Date;
          }
          else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
            Lt(local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.From.Date;
          }
          else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.To.Date;
            local.End.Date = import.To.Date;
          }

          local.End.Year = Year(local.Begin.Date);
          local.End.Month = Month(local.Begin.Date);
          local.End.Day = Day(local.Begin.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.Begin.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);
          local.End.Date = AddDays(local.End.Date, 1);
          local.End.Year = Year(local.End.Date);
          local.End.Month = Month(local.End.Date);
          local.End.Day = Day(local.End.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.End.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);

          foreach(var item in ReadCsePersonInvalidSsn5())
          {
            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "C";

            if (!IsEmpty(local.Ap.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ap.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;
            }
            else if (!IsEmpty(local.Ar.Number))
            {
              export.Display.Update.CsePersonsWorkSet.FormattedName =
                local.Ar.FormattedName;
              export.Display.Update.CsePersonsWorkSet.Number = local.Ar.Number;
            }
            else
            {
              // this is an error
            }

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+";
              ++export.PageCount.Count;

              ++export.Paging.Index;
              export.Paging.CheckSize();

              export.Paging.Update.PageCsePersonsWorkSet.Number =
                export.Display.Item.CsePersonsWorkSet.Number;
              export.Paging.Update.PageDateWorkArea.Date =
                export.Display.Item.DateWorkArea.Date;
              export.Paging.Update.PageInvalidSsn.Ssn =
                export.Display.Item.InvalidSsn.Ssn;
              export.Paging.Update.PageFrom.Date = export.From.Date;
              export.Paging.Update.PageTo.Date = export.To.Date;
              export.Paging.Update.PageCsePersonsWorkSet.Flag = "C";

              goto Test1;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field1 = GetField(export.From, "date");

            field1.Error = true;

            var field2 = GetField(export.To, "date");

            field2.Error = true;

            ExitState = "FN0000_NO_RECORDS_FOUND";

            return;
          }
        }
        else
        {
          // this is an error, must have a ap person number or ssn or either a 
          // from date or to date
          ExitState = "MISSING_REQUIRED_DATA";

          var field1 = GetField(export.SortCsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

          field2.Error = true;

          var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

          field3.Error = true;

          var field4 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

          field4.Error = true;

          var field5 = GetField(export.From, "date");

          field5.Error = true;

          var field6 = GetField(export.To, "date");

          field6.Error = true;

          return;
        }

Test1:

        export.ToPrevious.Date = export.To.Date;
        export.FromPrevious.Date = export.From.Date;
        MoveCsePersonsWorkSet1(export.SortCsePersonsWorkSet, export.HiddenPrev);
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "ADD":
        // is this a valid cse person
        local.Saved.Number = export.SortCsePersonsWorkSet.Number;
        UseSiReadCsePerson1();

        if (IsEmpty(local.Ap.Number))
        {
          export.SortCsePersonsWorkSet.FormattedName =
            import.SortCsePersonsWorkSet.FormattedName;
          ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";

          var field = GetField(export.SortCsePersonsWorkSet, "number");

          field.Error = true;

          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            export.Display.Update.CsePersonsWorkSet.Assign(
              local.ClearCsePersonsWorkSet);
            MoveInvalidSsn(local.ClearInvalidSsn,
              export.Display.Update.InvalidSsn);
            export.Display.Update.DateWorkArea.Date =
              local.ClearDateWorkArea.Date;
            export.Display.Update.Common.SelectChar =
              local.ClearCommon.SelectChar;
            export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
          }

          export.Display.CheckIndex();
          export.Display.Index = -1;
          export.Display.Count = 0;

          for(export.Paging.Index = 0; export.Paging.Index < export
            .Paging.Count; ++export.Paging.Index)
          {
            if (!export.Paging.CheckSize())
            {
              break;
            }

            MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
              export.Paging.Update.PageCsePersonsWorkSet);
            export.Paging.Update.PageInvalidSsn.Ssn = local.ClearInvalidSsn.Ssn;
            export.Paging.Update.PageDateWorkArea.Date =
              local.ClearDateWorkArea.Date;
          }

          export.Paging.CheckIndex();
          export.Paging.Index = -1;
          export.Paging.Count = 0;
          export.MoreLessScroll.Text3 = "";
          export.PageCount.Count = 0;

          return;
        }

        if (!Equal(export.SortCsePersonsWorkSet.Number, export.HiddenPrev.Number)
          || !Equal(export.SortCsePersonsWorkSet.Ssn, export.HiddenPrev.Ssn))
        {
          export.FirstPassAdd.Flag = "";
        }

        export.SortCsePersonsWorkSet.FormattedName = local.Ap.FormattedName;

        // is this ssn a correctly formated ssn
        if (IsEmpty(import.SortSsnWorkArea.SsnTextPart1) || IsEmpty
          (import.SortSsnWorkArea.SsnTextPart2) || IsEmpty
          (import.SortSsnWorkArea.SsnTextPart3))
        {
          if (IsEmpty(import.SortSsnWorkArea.SsnTextPart1))
          {
            var field = GetField(export.SortSsnWorkArea, "ssnTextPart1");

            field.Error = true;
          }

          if (IsEmpty(import.SortSsnWorkArea.SsnTextPart2))
          {
            var field = GetField(export.SortSsnWorkArea, "ssnTextPart2");

            field.Error = true;
          }

          if (IsEmpty(import.SortSsnWorkArea.SsnTextPart3))
          {
            var field = GetField(export.SortSsnWorkArea, "ssnTextPart3");

            field.Error = true;
          }

          ExitState = "LE0000_SSN_HAS_LT_9_CHARS";

          return;
        }

        if (IsEmpty(export.SortCsePersonsWorkSet.Ssn))
        {
          var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          ExitState = "LE0000_SSN_HAS_LT_9_CHARS";

          return;
        }

        // we are reading by the cse person number and the ssn
        local.SsnWorkArea.SsnNum9 =
          (int)StringToNumber(export.SortCsePersonsWorkSet.Ssn);

        if (ReadCsePersonInvalidSsn1())
        {
          // already have a record so we can not add another record
          // first clear the display
          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            export.Display.Update.CsePersonsWorkSet.Assign(
              local.ClearCsePersonsWorkSet);
            MoveInvalidSsn(local.ClearInvalidSsn,
              export.Display.Update.InvalidSsn);
            export.Display.Update.DateWorkArea.Date =
              local.ClearDateWorkArea.Date;
            export.Display.Update.Common.SelectChar =
              local.ClearCommon.SelectChar;
            export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
          }

          export.Display.CheckIndex();
          export.Display.Index = -1;
          export.Display.Count = 0;

          for(export.Paging.Index = 0; export.Paging.Index < export
            .Paging.Count; ++export.Paging.Index)
          {
            if (!export.Paging.CheckSize())
            {
              break;
            }

            MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
              export.Paging.Update.PageCsePersonsWorkSet);
            export.Paging.Update.PageInvalidSsn.Ssn = local.ClearInvalidSsn.Ssn;
            export.Paging.Update.PageDateWorkArea.Date =
              local.ClearDateWorkArea.Date;
          }

          export.Paging.CheckIndex();
          export.Paging.Index = -1;
          export.Paging.Count = 0;
          export.MoreLessScroll.Text3 = "";
          export.PageCount.Count = 0;

          // now display the existing record
          ++export.Display.Index;
          export.Display.CheckSize();

          export.Display.Update.InvalidSsn.CreatedBy =
            entities.InvalidSsn.CreatedBy;
          export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
          local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;
          local.SsnWorkArea.ConvertOption = "1";
          UseCabSsnConvertNumToText();
          ExitState = "ACO_NN0000_ALL_OK";
          export.Display.Update.SsnWorkArea.SsnTextPart1 =
            Substring(local.SsnWorkArea.SsnText9, 1, 3);
          export.Display.Update.SsnWorkArea.SsnTextPart2 =
            Substring(local.SsnWorkArea.SsnText9, 4, 2);
          export.Display.Update.SsnWorkArea.SsnTextPart3 =
            Substring(local.SsnWorkArea.SsnText9, 6, 4);
          export.Display.Update.DateWorkArea.Date =
            Date(entities.InvalidSsn.CreatedTstamp);
          UseSiReadCsePerson1();
          export.Display.Update.CsePersonsWorkSet.FormattedName =
            local.Ap.FormattedName;
          export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;
          export.Display.Update.CsePersonsWorkSet.FormattedName =
            local.Ap.FormattedName;

          var field1 = GetField(export.Display.Item.Common, "selectChar");

          field1.Error = true;

          var field2 =
            GetField(export.Display.Item.CsePersonsWorkSet, "number");

          field2.Error = true;

          var field3 =
            GetField(export.Display.Item.CsePersonsWorkSet, "formattedName");

          field3.Error = true;

          var field4 =
            GetField(export.Display.Item.SsnWorkArea, "ssnTextPart1");

          field4.Error = true;

          var field5 =
            GetField(export.Display.Item.SsnWorkArea, "ssnTextPart2");

          field5.Error = true;

          var field6 =
            GetField(export.Display.Item.SsnWorkArea, "ssnTextPart3");

          field6.Error = true;

          var field7 = GetField(export.Display.Item.InvalidSsn, "createdBy");

          field7.Error = true;

          var field8 = GetField(export.Display.Item.DateWorkArea, "date");

          field8.Error = true;

          export.SortCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
          MoveCsePersonsWorkSet1(local.ClearCsePersonsWorkSet, export.HiddenPrev);
            
          export.To.Date = local.ClearDateWorkArea.Date;
          export.ToPrevious.Date = local.ClearDateWorkArea.Date;
          export.From.Date = local.ClearDateWorkArea.Date;
          export.FromPrevious.Date = local.ClearDateWorkArea.Date;
          export.PromptCsePersonNumber.SelectChar =
            local.ClearCommon.SelectChar;
          export.SortSsnWorkArea.Assign(local.ClearSsnWorkArea);
          ExitState = "RECORD_AE";

          return;
        }
        else
        {
          // no record found so we can proceed in adding a record
        }

        if (AsChar(local.Ae.Flag) == 'O')
        {
          if (Equal(local.Ap.Ssn, export.SortCsePersonsWorkSet.Ssn))
          {
            if (AsChar(export.FirstPassAdd.Flag) != 'Y')
            {
              ExitState = "PERSON_INVALID_SSN_KNOWN_AE";
              MoveCsePersonsWorkSet1(export.SortCsePersonsWorkSet,
                export.HiddenPrev);

              var field1 = GetField(export.SortCsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

              field2.Error = true;

              var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

              field3.Error = true;

              var field4 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

              field4.Error = true;

              export.FirstPassAdd.Flag = "Y";

              for(export.Display.Index = 0; export.Display.Index < export
                .Display.Count; ++export.Display.Index)
              {
                if (!export.Display.CheckSize())
                {
                  break;
                }

                export.Display.Update.CsePersonsWorkSet.Assign(
                  local.ClearCsePersonsWorkSet);
                MoveInvalidSsn(local.ClearInvalidSsn,
                  export.Display.Update.InvalidSsn);
                export.Display.Update.DateWorkArea.Date =
                  local.ClearDateWorkArea.Date;
                export.Display.Update.Common.SelectChar =
                  local.ClearCommon.SelectChar;
                export.Display.Update.SsnWorkArea.
                  Assign(local.ClearSsnWorkArea);
              }

              export.Display.CheckIndex();
              export.Display.Index = -1;
              export.Display.Count = 0;

              for(export.Paging.Index = 0; export.Paging.Index < export
                .Paging.Count; ++export.Paging.Index)
              {
                if (!export.Paging.CheckSize())
                {
                  break;
                }

                MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                  export.Paging.Update.PageCsePersonsWorkSet);
                export.Paging.Update.PageInvalidSsn.Ssn =
                  local.ClearInvalidSsn.Ssn;
                export.Paging.Update.PageDateWorkArea.Date =
                  local.ClearDateWorkArea.Date;
              }

              export.Paging.CheckIndex();
              export.Paging.Index = -1;
              export.Paging.Count = 0;
              export.MoreLessScroll.Text3 = "";
              export.PageCount.Count = 0;

              return;
            }
          }
          else
          {
            local.Saved.Number = export.SortCsePersonsWorkSet.Number;

            if (Lt(new DateTime(1, 1, 1), local.BlankStartDate.Date))
            {
              UseEabReadAliasBatch2();
            }

            UseEabReadAlias2();

            switch(AsChar(local.AbendData.Type1))
            {
              case ' ':
                // normal, no errors
                break;
              case 'A':
                // Unsuccessful ADABAS Read Occurred.
                switch(TrimEnd(local.AbendData.AdabasResponseCd))
                {
                  case "0113":
                    ExitState = "ACO_ADABAS_PERSON_NF_113";

                    return;
                  case "0148":
                    ExitState = "ACO_ADABAS_UNAVAILABLE";

                    return;
                  default:
                    // this is ok since there was not any alias ssns
                    break;
                }

                break;
              case 'C':
                // CICS action Failed. A reason code should be interpreted.
                if (IsEmpty(local.AbendData.CicsResponseCd))
                {
                }
                else
                {
                  ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

                  return;
                }

                break;
              default:
                ExitState = "ADABAS_INVALID_RETURN_CODE";

                return;
            }

            // we are checking alternative ssns
            for(local.Aliases.Index = 0; local.Aliases.Index < local
              .Aliases.Count; ++local.Aliases.Index)
            {
              if (!local.Aliases.CheckSize())
              {
                break;
              }

              if (Equal(export.SortCsePersonsWorkSet.Ssn,
                local.Aliases.Item.CsePersonsWorkSet.Ssn))
              {
                if (AsChar(local.Aliases.Item.Ae.Flag) == 'Y' || AsChar
                  (local.Aliases.Item.Kscares.Flag) == 'Y' || AsChar
                  (local.Aliases.Item.Kanpay.Flag) == 'Y')
                {
                  if (AsChar(import.FirstAddPass.Flag) != 'Y')
                  {
                    ExitState = "PERSON_INVALID_SSN_KNOWN_AE";

                    var field1 =
                      GetField(export.SortCsePersonsWorkSet, "number");

                    field1.Error = true;

                    var field2 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart1");

                    field2.Error = true;

                    var field3 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart2");

                    field3.Error = true;

                    var field4 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart3");

                    field4.Error = true;

                    export.FirstPassAdd.Flag = "Y";

                    for(export.Display.Index = 0; export.Display.Index < export
                      .Display.Count; ++export.Display.Index)
                    {
                      if (!export.Display.CheckSize())
                      {
                        break;
                      }

                      export.Display.Update.CsePersonsWorkSet.Assign(
                        local.ClearCsePersonsWorkSet);
                      MoveInvalidSsn(local.ClearInvalidSsn,
                        export.Display.Update.InvalidSsn);
                      export.Display.Update.DateWorkArea.Date =
                        local.ClearDateWorkArea.Date;
                      export.Display.Update.Common.SelectChar =
                        local.ClearCommon.SelectChar;
                      export.Display.Update.SsnWorkArea.Assign(
                        local.ClearSsnWorkArea);
                    }

                    export.Display.CheckIndex();
                    export.Display.Index = -1;
                    export.Display.Count = 0;

                    for(export.Paging.Index = 0; export.Paging.Index < export
                      .Paging.Count; ++export.Paging.Index)
                    {
                      if (!export.Paging.CheckSize())
                      {
                        break;
                      }

                      MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                        export.Paging.Update.PageCsePersonsWorkSet);
                      export.Paging.Update.PageInvalidSsn.Ssn =
                        local.ClearInvalidSsn.Ssn;
                      export.Paging.Update.PageDateWorkArea.Date =
                        local.ClearDateWorkArea.Date;
                    }

                    export.Paging.CheckIndex();
                    export.Paging.Index = -1;
                    export.Paging.Count = 0;
                    export.MoreLessScroll.Text3 = "";
                    export.PageCount.Count = 0;

                    return;
                  }
                }
                else
                {
                  ExitState = "SSN_AND_PERSON_ALREADY_EXIST_CSE";

                  var field1 = GetField(export.SortCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

                  field2.Error = true;

                  var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

                  field3.Error = true;

                  var field4 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

                  field4.Error = true;

                  for(export.Display.Index = 0; export.Display.Index < export
                    .Display.Count; ++export.Display.Index)
                  {
                    if (!export.Display.CheckSize())
                    {
                      break;
                    }

                    export.Display.Update.CsePersonsWorkSet.Assign(
                      local.ClearCsePersonsWorkSet);
                    MoveInvalidSsn(local.ClearInvalidSsn,
                      export.Display.Update.InvalidSsn);
                    export.Display.Update.DateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                    export.Display.Update.Common.SelectChar =
                      local.ClearCommon.SelectChar;
                    export.Display.Update.SsnWorkArea.Assign(
                      local.ClearSsnWorkArea);
                  }

                  export.Display.CheckIndex();
                  export.Display.Index = -1;
                  export.Display.Count = 0;

                  for(export.Paging.Index = 0; export.Paging.Index < export
                    .Paging.Count; ++export.Paging.Index)
                  {
                    if (!export.Paging.CheckSize())
                    {
                      break;
                    }

                    MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                      export.Paging.Update.PageCsePersonsWorkSet);
                    export.Paging.Update.PageInvalidSsn.Ssn =
                      local.ClearInvalidSsn.Ssn;
                    export.Paging.Update.PageDateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                  }

                  export.Paging.CheckIndex();
                  export.Paging.Index = -1;
                  export.Paging.Count = 0;
                  export.MoreLessScroll.Text3 = "";
                  export.PageCount.Count = 0;

                  return;
                }
              }
            }

            local.Aliases.CheckIndex();

            while(!IsEmpty(local.NextKeyAp.UniqueKey))
            {
              // we will keep looping through alternative ssns until we find one
              // that matches or we run out of alternative ssn for this person
              if (Lt(new DateTime(1, 1, 1), local.BlankStartDate.Date))
              {
                UseEabReadAliasBatch1();
              }

              UseEabReadAlias2();

              switch(AsChar(local.AbendData.Type1))
              {
                case ' ':
                  // normal, no errors
                  break;
                case 'A':
                  // Unsuccessful ADABAS Read Occurred.
                  switch(TrimEnd(local.AbendData.AdabasResponseCd))
                  {
                    case "0113":
                      ExitState = "ACO_ADABAS_PERSON_NF_113";

                      return;
                    case "0148":
                      ExitState = "ACO_ADABAS_UNAVAILABLE";

                      return;
                    default:
                      // this is ok since there was not any alias ssns
                      ExitState = "ADABAS_READ_UNSUCCESSFUL";

                      break;
                  }

                  break;
                case 'C':
                  // CICS action Failed. A reason code should be interpreted.
                  if (IsEmpty(local.AbendData.CicsResponseCd))
                  {
                  }
                  else
                  {
                    ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

                    return;
                  }

                  break;
                default:
                  ExitState = "ADABAS_INVALID_RETURN_CODE";

                  return;
              }

              for(local.Aliases.Index = 0; local.Aliases.Index < local
                .Aliases.Count; ++local.Aliases.Index)
              {
                if (!local.Aliases.CheckSize())
                {
                  break;
                }

                if (Equal(export.SortCsePersonsWorkSet.Ssn,
                  local.Aliases.Item.CsePersonsWorkSet.Ssn))
                {
                  if (AsChar(local.Aliases.Item.Ae.Flag) == 'Y' || AsChar
                    (local.Aliases.Item.Kscares.Flag) == 'Y' || AsChar
                    (local.Aliases.Item.Kanpay.Flag) == 'Y')
                  {
                    if (AsChar(import.FirstAddPass.Flag) != 'Y')
                    {
                      ExitState = "PERSON_INVALID_SSN_KNOWN_AE";

                      var field1 =
                        GetField(export.SortCsePersonsWorkSet, "number");

                      field1.Error = true;

                      var field2 =
                        GetField(export.SortSsnWorkArea, "ssnTextPart1");

                      field2.Error = true;

                      var field3 =
                        GetField(export.SortSsnWorkArea, "ssnTextPart2");

                      field3.Error = true;

                      var field4 =
                        GetField(export.SortSsnWorkArea, "ssnTextPart3");

                      field4.Error = true;

                      export.FirstPassAdd.Flag = "Y";

                      for(export.Display.Index = 0; export.Display.Index < export
                        .Display.Count; ++export.Display.Index)
                      {
                        if (!export.Display.CheckSize())
                        {
                          break;
                        }

                        export.Display.Update.CsePersonsWorkSet.Assign(
                          local.ClearCsePersonsWorkSet);
                        MoveInvalidSsn(local.ClearInvalidSsn,
                          export.Display.Update.InvalidSsn);
                        export.Display.Update.DateWorkArea.Date =
                          local.ClearDateWorkArea.Date;
                        export.Display.Update.Common.SelectChar =
                          local.ClearCommon.SelectChar;
                        export.Display.Update.SsnWorkArea.Assign(
                          local.ClearSsnWorkArea);
                      }

                      export.Display.CheckIndex();
                      export.Display.Index = -1;
                      export.Display.Count = 0;

                      for(export.Paging.Index = 0; export.Paging.Index < export
                        .Paging.Count; ++export.Paging.Index)
                      {
                        if (!export.Paging.CheckSize())
                        {
                          break;
                        }

                        MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                          export.Paging.Update.PageCsePersonsWorkSet);
                        export.Paging.Update.PageInvalidSsn.Ssn =
                          local.ClearInvalidSsn.Ssn;
                        export.Paging.Update.PageDateWorkArea.Date =
                          local.ClearDateWorkArea.Date;
                      }

                      export.Paging.CheckIndex();
                      export.Paging.Index = -1;
                      export.Paging.Count = 0;
                      export.MoreLessScroll.Text3 = "";
                      export.PageCount.Count = 0;

                      return;
                    }
                    else
                    {
                      goto Test2;
                    }
                  }
                  else
                  {
                    ExitState = "SSN_AND_PERSON_ALREADY_EXIST_CSE";

                    var field1 =
                      GetField(export.SortCsePersonsWorkSet, "number");

                    field1.Error = true;

                    var field2 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart1");

                    field2.Error = true;

                    var field3 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart2");

                    field3.Error = true;

                    var field4 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart3");

                    field4.Error = true;

                    for(export.Display.Index = 0; export.Display.Index < export
                      .Display.Count; ++export.Display.Index)
                    {
                      if (!export.Display.CheckSize())
                      {
                        break;
                      }

                      export.Display.Update.CsePersonsWorkSet.Assign(
                        local.ClearCsePersonsWorkSet);
                      MoveInvalidSsn(local.ClearInvalidSsn,
                        export.Display.Update.InvalidSsn);
                      export.Display.Update.DateWorkArea.Date =
                        local.ClearDateWorkArea.Date;
                      export.Display.Update.Common.SelectChar =
                        local.ClearCommon.SelectChar;
                      export.Display.Update.SsnWorkArea.Assign(
                        local.ClearSsnWorkArea);
                    }

                    export.Display.CheckIndex();
                    export.Display.Index = -1;
                    export.Display.Count = 0;

                    for(export.Paging.Index = 0; export.Paging.Index < export
                      .Paging.Count; ++export.Paging.Index)
                    {
                      if (!export.Paging.CheckSize())
                      {
                        break;
                      }

                      MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                        export.Paging.Update.PageCsePersonsWorkSet);
                      export.Paging.Update.PageInvalidSsn.Ssn =
                        local.ClearInvalidSsn.Ssn;
                      export.Paging.Update.PageDateWorkArea.Date =
                        local.ClearDateWorkArea.Date;
                    }

                    export.Paging.CheckIndex();
                    export.Paging.Index = -1;
                    export.Paging.Count = 0;
                    export.MoreLessScroll.Text3 = "";
                    export.PageCount.Count = 0;

                    return;
                  }
                }
              }

              local.Aliases.CheckIndex();
            }
          }
        }
        else if (Equal(local.Ap.Ssn, export.SortCsePersonsWorkSet.Ssn))
        {
          ExitState = "SSN_AND_PERSON_ALREADY_EXIST_CSE";

          var field1 = GetField(export.SortCsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

          field2.Error = true;

          var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

          field3.Error = true;

          var field4 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

          field4.Error = true;

          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            export.Display.Update.CsePersonsWorkSet.Assign(
              local.ClearCsePersonsWorkSet);
            MoveInvalidSsn(local.ClearInvalidSsn,
              export.Display.Update.InvalidSsn);
            export.Display.Update.DateWorkArea.Date =
              local.ClearDateWorkArea.Date;
            export.Display.Update.Common.SelectChar =
              local.ClearCommon.SelectChar;
            export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
          }

          export.Display.CheckIndex();
          export.Display.Index = -1;
          export.Display.Count = 0;

          for(export.Paging.Index = 0; export.Paging.Index < export
            .Paging.Count; ++export.Paging.Index)
          {
            if (!export.Paging.CheckSize())
            {
              break;
            }

            MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
              export.Paging.Update.PageCsePersonsWorkSet);
            export.Paging.Update.PageInvalidSsn.Ssn = local.ClearInvalidSsn.Ssn;
            export.Paging.Update.PageDateWorkArea.Date =
              local.ClearDateWorkArea.Date;
          }

          export.Paging.CheckIndex();
          export.Paging.Index = -1;
          export.Paging.Count = 0;
          export.MoreLessScroll.Text3 = "";
          export.PageCount.Count = 0;

          return;
        }
        else
        {
          // we are checking alternative ssns
          local.Saved.Number = export.SortCsePersonsWorkSet.Number;

          if (Lt(new DateTime(1, 1, 1), local.BlankStartDate.Date))
          {
            UseEabReadAliasBatch2();
          }

          UseEabReadAlias2();

          switch(AsChar(local.AbendData.Type1))
          {
            case ' ':
              // normal, no errors
              break;
            case 'A':
              // Unsuccessful ADABAS Read Occurred.
              switch(TrimEnd(local.AbendData.AdabasResponseCd))
              {
                case "0113":
                  ExitState = "ACO_ADABAS_PERSON_NF_113";

                  return;
                case "0148":
                  ExitState = "ACO_ADABAS_UNAVAILABLE";

                  return;
                default:
                  // this is ok since there was not any alias ssns
                  break;
              }

              break;
            case 'C':
              // CICS action Failed. A reason code should be interpreted.
              if (IsEmpty(local.AbendData.CicsResponseCd))
              {
              }
              else
              {
                ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

                return;
              }

              break;
            default:
              ExitState = "ADABAS_INVALID_RETURN_CODE";

              return;
          }

          for(local.Aliases.Index = 0; local.Aliases.Index < local
            .Aliases.Count; ++local.Aliases.Index)
          {
            if (!local.Aliases.CheckSize())
            {
              break;
            }

            if (Equal(export.SortCsePersonsWorkSet.Ssn,
              local.Aliases.Item.CsePersonsWorkSet.Ssn))
            {
              if (AsChar(local.Aliases.Item.Ae.Flag) == 'Y' || AsChar
                (local.Aliases.Item.Kscares.Flag) == 'Y' || AsChar
                (local.Aliases.Item.Kanpay.Flag) == 'Y')
              {
                if (AsChar(import.FirstAddPass.Flag) != 'Y')
                {
                  ExitState = "PERSON_INVALID_SSN_KNOWN_AE";

                  var field1 = GetField(export.SortCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

                  field2.Error = true;

                  var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

                  field3.Error = true;

                  var field4 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

                  field4.Error = true;

                  export.FirstPassAdd.Flag = "Y";

                  for(export.Display.Index = 0; export.Display.Index < export
                    .Display.Count; ++export.Display.Index)
                  {
                    if (!export.Display.CheckSize())
                    {
                      break;
                    }

                    export.Display.Update.CsePersonsWorkSet.Assign(
                      local.ClearCsePersonsWorkSet);
                    MoveInvalidSsn(local.ClearInvalidSsn,
                      export.Display.Update.InvalidSsn);
                    export.Display.Update.DateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                    export.Display.Update.Common.SelectChar =
                      local.ClearCommon.SelectChar;
                    export.Display.Update.SsnWorkArea.Assign(
                      local.ClearSsnWorkArea);
                  }

                  export.Display.CheckIndex();
                  export.Display.Index = -1;
                  export.Display.Count = 0;

                  for(export.Paging.Index = 0; export.Paging.Index < export
                    .Paging.Count; ++export.Paging.Index)
                  {
                    if (!export.Paging.CheckSize())
                    {
                      break;
                    }

                    MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                      export.Paging.Update.PageCsePersonsWorkSet);
                    export.Paging.Update.PageInvalidSsn.Ssn =
                      local.ClearInvalidSsn.Ssn;
                    export.Paging.Update.PageDateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                  }

                  export.Paging.CheckIndex();
                  export.Paging.Index = -1;
                  export.Paging.Count = 0;
                  export.MoreLessScroll.Text3 = "";
                  export.PageCount.Count = 0;

                  return;
                }
              }
              else
              {
                ExitState = "SSN_AND_PERSON_ALREADY_EXIST_CSE";

                var field1 = GetField(export.SortCsePersonsWorkSet, "number");

                field1.Error = true;

                var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

                field2.Error = true;

                var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

                field3.Error = true;

                var field4 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

                field4.Error = true;

                for(export.Display.Index = 0; export.Display.Index < export
                  .Display.Count; ++export.Display.Index)
                {
                  if (!export.Display.CheckSize())
                  {
                    break;
                  }

                  export.Display.Update.CsePersonsWorkSet.Assign(
                    local.ClearCsePersonsWorkSet);
                  MoveInvalidSsn(local.ClearInvalidSsn,
                    export.Display.Update.InvalidSsn);
                  export.Display.Update.DateWorkArea.Date =
                    local.ClearDateWorkArea.Date;
                  export.Display.Update.Common.SelectChar =
                    local.ClearCommon.SelectChar;
                  export.Display.Update.SsnWorkArea.Assign(
                    local.ClearSsnWorkArea);
                }

                export.Display.CheckIndex();
                export.Display.Index = -1;
                export.Display.Count = 0;

                for(export.Paging.Index = 0; export.Paging.Index < export
                  .Paging.Count; ++export.Paging.Index)
                {
                  if (!export.Paging.CheckSize())
                  {
                    break;
                  }

                  MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                    export.Paging.Update.PageCsePersonsWorkSet);
                  export.Paging.Update.PageInvalidSsn.Ssn =
                    local.ClearInvalidSsn.Ssn;
                  export.Paging.Update.PageDateWorkArea.Date =
                    local.ClearDateWorkArea.Date;
                }

                export.Paging.CheckIndex();
                export.Paging.Index = -1;
                export.Paging.Count = 0;
                export.MoreLessScroll.Text3 = "";
                export.PageCount.Count = 0;

                return;
              }
            }
          }

          local.Aliases.CheckIndex();

          while(!IsEmpty(local.NextKeyAp.UniqueKey))
          {
            if (Lt(new DateTime(1, 1, 1), local.BlankStartDate.Date))
            {
              UseEabReadAliasBatch1();
            }

            UseEabReadAlias1();

            switch(AsChar(local.AbendData.Type1))
            {
              case ' ':
                // normal, no errors
                break;
              case 'A':
                // Unsuccessful ADABAS Read Occurred.
                switch(TrimEnd(local.AbendData.AdabasResponseCd))
                {
                  case "0113":
                    ExitState = "ACO_ADABAS_PERSON_NF_113";

                    return;
                  case "0148":
                    ExitState = "ACO_ADABAS_UNAVAILABLE";

                    return;
                  default:
                    // this is ok since there was not any alias ssns
                    break;
                }

                break;
              case 'C':
                // CICS action Failed. A reason code should be interpreted.
                if (IsEmpty(local.AbendData.CicsResponseCd))
                {
                }
                else
                {
                  ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

                  return;
                }

                break;
              default:
                ExitState = "ADABAS_INVALID_RETURN_CODE";

                return;
            }

            // we will keep looping through alternative ssns until we find one 
            // that matches or we run out of alternative ssn for this person
            for(local.Aliases.Index = 0; local.Aliases.Index < local
              .Aliases.Count; ++local.Aliases.Index)
            {
              if (!local.Aliases.CheckSize())
              {
                break;
              }

              if (Equal(export.SortCsePersonsWorkSet.Ssn,
                local.Aliases.Item.CsePersonsWorkSet.Ssn))
              {
                if (AsChar(local.Aliases.Item.Ae.Flag) == 'Y' || AsChar
                  (local.Aliases.Item.Kscares.Flag) == 'Y' || AsChar
                  (local.Aliases.Item.Kanpay.Flag) == 'Y')
                {
                  if (AsChar(import.FirstAddPass.Flag) != 'Y')
                  {
                    ExitState = "PERSON_INVALID_SSN_KNOWN_AE";

                    var field1 =
                      GetField(export.SortCsePersonsWorkSet, "number");

                    field1.Error = true;

                    var field2 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart1");

                    field2.Error = true;

                    var field3 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart2");

                    field3.Error = true;

                    var field4 =
                      GetField(export.SortSsnWorkArea, "ssnTextPart3");

                    field4.Error = true;

                    export.FirstPassAdd.Flag = "Y";

                    for(export.Display.Index = 0; export.Display.Index < export
                      .Display.Count; ++export.Display.Index)
                    {
                      if (!export.Display.CheckSize())
                      {
                        break;
                      }

                      export.Display.Update.CsePersonsWorkSet.Assign(
                        local.ClearCsePersonsWorkSet);
                      MoveInvalidSsn(local.ClearInvalidSsn,
                        export.Display.Update.InvalidSsn);
                      export.Display.Update.DateWorkArea.Date =
                        local.ClearDateWorkArea.Date;
                      export.Display.Update.Common.SelectChar =
                        local.ClearCommon.SelectChar;
                      export.Display.Update.SsnWorkArea.Assign(
                        local.ClearSsnWorkArea);
                    }

                    export.Display.CheckIndex();
                    export.Display.Index = -1;
                    export.Display.Count = 0;

                    for(export.Paging.Index = 0; export.Paging.Index < export
                      .Paging.Count; ++export.Paging.Index)
                    {
                      if (!export.Paging.CheckSize())
                      {
                        break;
                      }

                      MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                        export.Paging.Update.PageCsePersonsWorkSet);
                      export.Paging.Update.PageInvalidSsn.Ssn =
                        local.ClearInvalidSsn.Ssn;
                      export.Paging.Update.PageDateWorkArea.Date =
                        local.ClearDateWorkArea.Date;
                    }

                    export.Paging.CheckIndex();
                    export.Paging.Index = -1;
                    export.Paging.Count = 0;
                    export.MoreLessScroll.Text3 = "";
                    export.PageCount.Count = 0;

                    return;
                  }
                  else
                  {
                    goto Test2;
                  }
                }
                else
                {
                  ExitState = "SSN_AND_PERSON_ALREADY_EXIST_CSE";

                  var field1 = GetField(export.SortCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

                  field2.Error = true;

                  var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

                  field3.Error = true;

                  var field4 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

                  field4.Error = true;

                  for(export.Display.Index = 0; export.Display.Index < export
                    .Display.Count; ++export.Display.Index)
                  {
                    if (!export.Display.CheckSize())
                    {
                      break;
                    }

                    export.Display.Update.CsePersonsWorkSet.Assign(
                      local.ClearCsePersonsWorkSet);
                    MoveInvalidSsn(local.ClearInvalidSsn,
                      export.Display.Update.InvalidSsn);
                    export.Display.Update.DateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                    export.Display.Update.Common.SelectChar =
                      local.ClearCommon.SelectChar;
                    export.Display.Update.SsnWorkArea.Assign(
                      local.ClearSsnWorkArea);
                  }

                  export.Display.CheckIndex();
                  export.Display.Index = -1;
                  export.Display.Count = 0;

                  for(export.Paging.Index = 0; export.Paging.Index < export
                    .Paging.Count; ++export.Paging.Index)
                  {
                    if (!export.Paging.CheckSize())
                    {
                      break;
                    }

                    MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                      export.Paging.Update.PageCsePersonsWorkSet);
                    export.Paging.Update.PageInvalidSsn.Ssn =
                      local.ClearInvalidSsn.Ssn;
                    export.Paging.Update.PageDateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                  }

                  export.Paging.CheckIndex();
                  export.Paging.Index = -1;
                  export.Paging.Count = 0;
                  export.MoreLessScroll.Text3 = "";
                  export.PageCount.Count = 0;

                  return;
                }
              }
            }

            local.Aliases.CheckIndex();
          }
        }

Test2:

        // if the record has made it to this point then it is time to add a 
        // record
        if (ReadCsePerson())
        {
          try
          {
            CreateInvalidSsn();

            for(export.Display.Index = 0; export.Display.Index < export
              .Display.Count; ++export.Display.Index)
            {
              if (!export.Display.CheckSize())
              {
                break;
              }

              export.Display.Update.CsePersonsWorkSet.Assign(
                local.ClearCsePersonsWorkSet);
              MoveInvalidSsn(local.ClearInvalidSsn,
                export.Display.Update.InvalidSsn);
              export.Display.Update.DateWorkArea.Date =
                local.ClearDateWorkArea.Date;
              export.Display.Update.Common.SelectChar =
                local.ClearCommon.SelectChar;
              export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
            }

            export.Display.CheckIndex();
            export.Display.Index = -1;
            export.Display.Count = 0;

            for(export.Paging.Index = 0; export.Paging.Index < export
              .Paging.Count; ++export.Paging.Index)
            {
              if (!export.Paging.CheckSize())
              {
                break;
              }

              MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                export.Paging.Update.PageCsePersonsWorkSet);
              export.Paging.Update.PageInvalidSsn.Ssn =
                local.ClearInvalidSsn.Ssn;
              export.Paging.Update.PageDateWorkArea.Date =
                local.ClearDateWorkArea.Date;
            }

            export.Paging.CheckIndex();
            export.Paging.Index = -1;
            export.Paging.Count = 0;
            export.MoreLessScroll.Text3 = "";
            export.PageCount.Count = 0;

            // now display the existing record
            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = local.SsnWorkArea.SsnNum9;
            local.SsnWorkArea.ConvertOption = "1";
            UseCabSsnConvertNumToText();
            ExitState = "ACO_NN0000_ALL_OK";
            export.Display.Update.SsnWorkArea.SsnTextPart1 =
              Substring(local.SsnWorkArea.SsnText9, 1, 3);
            export.Display.Update.SsnWorkArea.SsnTextPart2 =
              Substring(local.SsnWorkArea.SsnText9, 4, 2);
            export.Display.Update.SsnWorkArea.SsnTextPart3 =
              Substring(local.SsnWorkArea.SsnText9, 6, 4);
            export.Display.Update.DateWorkArea.Date = Now().Date;
            local.Saved.Number = export.SortCsePersonsWorkSet.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (ReadInvalidSsn1())
                {
                  // already have a record so we can not add another record
                  // first clear the display
                  ExitState = "RECORD_AE";

                  for(export.Display.Index = 0; export.Display.Index < export
                    .Display.Count; ++export.Display.Index)
                  {
                    if (!export.Display.CheckSize())
                    {
                      break;
                    }

                    export.Display.Update.CsePersonsWorkSet.Assign(
                      local.ClearCsePersonsWorkSet);
                    MoveInvalidSsn(local.ClearInvalidSsn,
                      export.Display.Update.InvalidSsn);
                    export.Display.Update.DateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                    export.Display.Update.Common.SelectChar =
                      local.ClearCommon.SelectChar;
                    export.Display.Update.SsnWorkArea.Assign(
                      local.ClearSsnWorkArea);
                  }

                  export.Display.CheckIndex();
                  export.Display.Index = -1;
                  export.Display.Count = 0;

                  for(export.Paging.Index = 0; export.Paging.Index < export
                    .Paging.Count; ++export.Paging.Index)
                  {
                    if (!export.Paging.CheckSize())
                    {
                      break;
                    }

                    MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
                      export.Paging.Update.PageCsePersonsWorkSet);
                    export.Paging.Update.PageInvalidSsn.Ssn =
                      local.ClearInvalidSsn.Ssn;
                    export.Paging.Update.PageDateWorkArea.Date =
                      local.ClearDateWorkArea.Date;
                  }

                  export.Paging.CheckIndex();
                  export.Paging.Index = -1;
                  export.Paging.Count = 0;
                  export.PageCount.Count = 0;
                  export.MoreLessScroll.Text3 = "";

                  // now display the existing record
                  ++export.Display.Index;
                  export.Display.CheckSize();

                  export.Display.Update.InvalidSsn.CreatedBy =
                    entities.InvalidSsn.CreatedBy;
                  export.Display.Update.InvalidSsn.Ssn =
                    entities.InvalidSsn.Ssn;
                  local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

                  if (local.SsnWorkArea.SsnNum9 > 0)
                  {
                    local.SsnWorkArea.ConvertOption = "1";
                    UseCabSsnConvertNumToText();
                    ExitState = "ACO_NN0000_ALL_OK";
                    export.Display.Update.SsnWorkArea.SsnTextPart1 =
                      Substring(local.SsnWorkArea.SsnText9, 1, 3);
                    export.Display.Update.SsnWorkArea.SsnTextPart2 =
                      Substring(local.SsnWorkArea.SsnText9, 4, 2);
                    export.Display.Update.SsnWorkArea.SsnTextPart3 =
                      Substring(local.SsnWorkArea.SsnText9, 6, 4);
                  }

                  export.Display.Update.DateWorkArea.Date =
                    Date(entities.InvalidSsn.CreatedTstamp);
                  UseSiReadCsePerson1();
                  export.Display.Update.CsePersonsWorkSet.FormattedName =
                    local.Ap.FormattedName;
                  export.Display.Update.CsePersonsWorkSet.Number =
                    local.Ap.Number;

                  var field5 =
                    GetField(export.Display.Item.Common, "selectChar");

                  field5.Error = true;

                  var field6 =
                    GetField(export.Display.Item.CsePersonsWorkSet, "number");

                  field6.Error = true;

                  var field7 =
                    GetField(export.Display.Item.CsePersonsWorkSet,
                    "formattedName");

                  field7.Error = true;

                  var field8 =
                    GetField(export.Display.Item.SsnWorkArea, "ssnTextPart1");

                  field8.Error = true;

                  var field9 =
                    GetField(export.Display.Item.SsnWorkArea, "ssnTextPart2");

                  field9.Error = true;

                  var field10 =
                    GetField(export.Display.Item.SsnWorkArea, "ssnTextPart3");

                  field10.Error = true;

                  var field11 =
                    GetField(export.Display.Item.DateWorkArea, "date");

                  field11.Error = true;

                  var field12 =
                    GetField(export.Display.Item.InvalidSsn, "createdBy");

                  field12.Error = true;

                  ExitState = "RECORD_AE";

                  return;
                }
                else
                {
                  // this should never happend since the whole point of the 
                  // current read on was to
                  // show that the invlaid ssn record already exists.
                  var field5 =
                    GetField(export.Display.Item.CsePersonsWorkSet, "number");

                  field5.Error = true;

                  var field6 =
                    GetField(export.Display.Item.SsnWorkArea, "ssnTextPart1");

                  field6.Error = true;

                  var field7 =
                    GetField(export.Display.Item.SsnWorkArea, "ssnTextPart2");

                  field7.Error = true;

                  var field8 =
                    GetField(export.Display.Item.SsnWorkArea, "ssnTextPart3");

                  field8.Error = true;

                  ExitState = "INVALID_SSN_NF";

                  return;
                }

                break;
              case ErrorCode.PermittedValueViolation:
                var field1 =
                  GetField(export.Display.Item.CsePersonsWorkSet, "number");

                field1.Error = true;

                var field2 =
                  GetField(export.Display.Item.SsnWorkArea, "ssnTextPart1");

                field2.Error = true;

                var field3 =
                  GetField(export.Display.Item.SsnWorkArea, "ssnTextPart2");

                field3.Error = true;

                var field4 =
                  GetField(export.Display.Item.SsnWorkArea, "ssnTextPart3");

                field4.Error = true;

                ExitState = "INVALID_SSN_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          var field = GetField(export.SortCsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";

          return;
        }

        MoveCsePersonsWorkSet1(export.SortCsePersonsWorkSet, local.Saved);
        MoveCsePersonsWorkSet1(export.SortCsePersonsWorkSet, export.HiddenPrev);
        export.SortCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
        export.SortSsnWorkArea.Assign(local.ClearSsnWorkArea);
        export.From.Date = local.ClearDateWorkArea.Date;
        export.To.Date = local.ClearDateWorkArea.Date;
        export.FromPrevious.Date = local.ClearDateWorkArea.Date;
        export.ToPrevious.Date = local.ClearDateWorkArea.Date;
        ExitState = "ACO_NI0000_ADD_SUCCESSFUL";

        break;
      case "DELETE":
        if (!Equal(export.SortCsePersonsWorkSet.Number, export.HiddenPrev.Number)
          || !
          Equal(export.SortCsePersonsWorkSet.Ssn, export.HiddenPrev.Ssn) || !
          Equal(export.From.Date, export.FromPrevious.Date) || !
          Equal(export.To.Date, import.ToPrevious.Date))
        {
          var field1 =
            GetField(export.Display.Item.CsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 =
            GetField(export.Display.Item.SsnWorkArea, "ssnTextPart1");

          field2.Error = true;

          var field3 =
            GetField(export.Display.Item.SsnWorkArea, "ssnTextPart2");

          field3.Error = true;

          var field4 =
            GetField(export.Display.Item.SsnWorkArea, "ssnTextPart3");

          field4.Error = true;

          var field5 = GetField(export.From, "date");

          field5.Error = true;

          var field6 = GetField(export.To, "date");

          field6.Error = true;

          ExitState = "SORT_CRITERIA_CHANGED_REDISPLAY";

          return;
        }

        local.RecordSelected.Flag = "";

        if (!export.Display.IsEmpty)
        {
          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Display.Item.Common.SelectChar) && AsChar
              (export.Display.Item.Common.SelectChar) != 'S')
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              var field = GetField(export.Display.Item.Common, "selectChar");

              field.Error = true;

              return;
            }

            if (AsChar(export.Display.Item.Common.SelectChar) == 'S')
            {
              local.RecordSelected.Flag = "Y";

              if (ReadInvalidSsnCsePerson1())
              {
                DeleteInvalidSsn();
                ExitState = "DELETED_SUCCESSFULLY_FROM_BSSN";
              }
              else
              {
                var field1 =
                  GetField(export.Display.Item.CsePersonsWorkSet, "number");

                field1.Error = true;

                var field2 =
                  GetField(export.Display.Item.SsnWorkArea, "ssnTextPart1");

                field2.Error = true;

                var field3 =
                  GetField(export.Display.Item.SsnWorkArea, "ssnTextPart2");

                field3.Error = true;

                var field4 =
                  GetField(export.Display.Item.SsnWorkArea, "ssnTextPart3");

                field4.Error = true;

                ExitState = "INVALID_SSN_NF";

                return;
              }
            }
          }

          export.Display.CheckIndex();

          if (IsEmpty(local.RecordSelected.Flag))
          {
            for(export.Display.Index = 0; export.Display.Index < export
              .Display.Count; ++export.Display.Index)
            {
              if (!export.Display.CheckSize())
              {
                break;
              }

              var field = GetField(export.Display.Item.Common, "selectChar");

              field.Error = true;
            }

            export.Display.CheckIndex();
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            return;
          }
        }
        else
        {
          ExitState = "NO_RECORDS_TO_SELECT_FOR_COMMAND";

          return;
        }

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          export.Display.Update.CsePersonsWorkSet.Assign(
            local.ClearCsePersonsWorkSet);
          MoveInvalidSsn(local.ClearInvalidSsn, export.Display.Update.InvalidSsn);
            
          export.Display.Update.DateWorkArea.Date =
            local.ClearDateWorkArea.Date;
          export.Display.Update.Common.SelectChar =
            local.ClearCommon.SelectChar;
          export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
        }

        export.Display.CheckIndex();
        export.Display.Index = -1;
        export.Display.Count = 0;

        for(export.Paging.Index = 0; export.Paging.Index < export.Paging.Count; ++
          export.Paging.Index)
        {
          if (!export.Paging.CheckSize())
          {
            break;
          }

          MoveCsePersonsWorkSet3(local.ClearCsePersonsWorkSet,
            export.Paging.Update.PageCsePersonsWorkSet);
          export.Paging.Update.PageInvalidSsn.Ssn = local.ClearInvalidSsn.Ssn;
          export.Paging.Update.PageDateWorkArea.Date =
            local.ClearDateWorkArea.Date;
        }

        export.Paging.CheckIndex();
        export.Paging.Index = -1;
        export.Paging.Count = 0;
        export.PageCount.Count = 0;

        if (!IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn))
        {
          ExitState = "ACO_NN0000_ALL_OK";

          // getting all invalid ssns for a single cse person
          foreach(var item in ReadInvalidSsnCsePerson3())
          {
            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "A";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+";
              ++export.PageCount.Count;

              ++export.Paging.Index;
              export.Paging.CheckSize();

              export.Paging.Update.PageCsePersonsWorkSet.Number =
                export.Display.Item.CsePersonsWorkSet.Number;
              export.Paging.Update.PageDateWorkArea.Date =
                export.Display.Item.DateWorkArea.Date;
              export.Paging.Update.PageInvalidSsn.Ssn =
                export.Display.Item.InvalidSsn.Ssn;
              export.Paging.Update.PageFrom.Date = export.From.Date;
              export.Paging.Update.PageTo.Date = export.To.Date;
              export.Paging.Update.PageCsePersonsWorkSet.Flag = "A";

              return;
            }
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && !
          IsEmpty(export.SortCsePersonsWorkSet.Ssn))
        {
          local.SsnWorkArea.SsnNum9 =
            (int)StringToNumber(export.SortCsePersonsWorkSet.Ssn);

          foreach(var item in ReadCsePersonInvalidSsn3())
          {
            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.ConvertOption = "1";

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            local.Saved.Number = entities.CsePerson.Number;
            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "B";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+";
              ++export.PageCount.Count;

              ++export.Paging.Index;
              export.Paging.CheckSize();

              export.Paging.Update.PageCsePersonsWorkSet.Number =
                export.Display.Item.CsePersonsWorkSet.Number;
              export.Paging.Update.PageDateWorkArea.Date =
                export.Display.Item.DateWorkArea.Date;
              export.Paging.Update.PageInvalidSsn.Ssn =
                export.Display.Item.InvalidSsn.Ssn;
              export.Paging.Update.PageFrom.Date = export.From.Date;
              export.Paging.Update.PageTo.Date = export.To.Date;
              export.Paging.Update.PageCsePersonsWorkSet.Flag = "B";

              return;
            }
          }
        }
        else if (!IsEmpty(export.SortCsePersonsWorkSet.Number) && !
          IsEmpty(export.SortCsePersonsWorkSet.Ssn))
        {
          // we are reading by the cse person number and the ssn
          local.SsnWorkArea.SsnNum9 =
            (int)StringToNumber(export.SortCsePersonsWorkSet.Ssn);

          if (ReadCsePersonInvalidSsn1())
          {
            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn) && (
            Lt(local.BlankStartDate.Date, export.From.Date) || Lt
          (local.BlankStartDate.Date, export.To.Date)))
        {
          // checking to see if it is in the date range that was entered if 
          // there is one
          local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

          if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(export.To.Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.To.Date;
          }
          else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
            Lt(local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.From.Date;
          }
          else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.To.Date;
            local.End.Date = import.To.Date;
          }

          local.End.Year = Year(local.Begin.Date);
          local.End.Month = Month(local.Begin.Date);
          local.End.Day = Day(local.Begin.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.Begin.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);
          local.End.Date = AddDays(local.End.Date, 1);
          local.End.Year = Year(local.End.Date);
          local.End.Month = Month(local.End.Date);
          local.End.Day = Day(local.End.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.End.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);

          foreach(var item in ReadCsePersonInvalidSsn5())
          {
            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "C";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+";
              ++export.PageCount.Count;

              ++export.Paging.Index;
              export.Paging.CheckSize();

              export.Paging.Update.PageCsePersonsWorkSet.Number =
                export.Display.Item.CsePersonsWorkSet.Number;
              export.Paging.Update.PageDateWorkArea.Date =
                export.Display.Item.DateWorkArea.Date;
              export.Paging.Update.PageInvalidSsn.Ssn =
                export.Display.Item.InvalidSsn.Ssn;
              export.Paging.Update.PageFrom.Date = export.From.Date;
              export.Paging.Update.PageTo.Date = export.To.Date;
              export.Paging.Update.PageCsePersonsWorkSet.Flag = "C";

              return;
            }
          }
        }
        else
        {
          // this is an error, must have a ap person number or ssn or either a 
          // from date or to date
        }

        break;
      case "PREV":
        if (!Equal(export.SortCsePersonsWorkSet.Number, export.HiddenPrev.Number))
          
        {
          var field = GetField(export.SortCsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (!Equal(export.SortCsePersonsWorkSet.Ssn, export.HiddenPrev.Ssn))
        {
          var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (!Equal(export.From.Date, export.FromPrevious.Date))
        {
          var field = GetField(export.From, "date");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (!Equal(export.To.Date, export.ToPrevious.Date))
        {
          var field = GetField(export.To, "date");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (import.Paging.Count <= 1)
        {
          ExitState = "THERE_NO_PREVIOUS_RECORD_DISPLAY";

          return;
        }
        else
        {
          if (export.PageCount.Count == 1)
          {
            ExitState = "THERE_NO_PREVIOUS_RECORD_DISPLAY";

            return;
          }

          --export.PageCount.Count;

          for(import.Paging.Index = 0; import.Paging.Index < import
            .Paging.Count; ++import.Paging.Index)
          {
            if (!import.Paging.CheckSize())
            {
              break;
            }

            if (export.PageCount.Count == import.Paging.Index + 1)
            {
              local.ReadCsePersonsWorkSet.Number =
                import.Paging.Item.PageCsePersonsWorkSet.Number;
              local.ReadInvalidSsn.Ssn = import.Paging.Item.PageInvalidSsn.Ssn;
              local.BeginRead.Date = import.Paging.Item.PageFrom.Date;
              local.EndRead.Date = import.Paging.Item.PageTo.Date;

              break;
            }
          }

          import.Paging.CheckIndex();
        }

        if (export.PageCount.Count == 1)
        {
          export.MoreLessScroll.Text3 = "+";
        }
        else if (export.PageCount.Count < import.Paging.Count && export
          .PageCount.Count > 1)
        {
          export.MoreLessScroll.Text3 = "+ -";
        }

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          export.Display.Update.CsePersonsWorkSet.Assign(
            local.ClearCsePersonsWorkSet);
          MoveInvalidSsn(local.ClearInvalidSsn, export.Display.Update.InvalidSsn);
            
          export.Display.Update.DateWorkArea.Date =
            local.ClearDateWorkArea.Date;
          export.Display.Update.Common.SelectChar =
            local.ClearCommon.SelectChar;
          export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
        }

        export.Display.CheckIndex();
        export.Display.Index = -1;
        export.Display.Count = 0;

        if (!IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn))
        {
          // getting all invalid ssns for a single cse person
          foreach(var item in ReadInvalidSsnCsePerson2())
          {
            local.ReadInvalidSsn.Ssn = entities.InvalidSsn.Ssn;

            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "A";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              goto Test3;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field = GetField(export.SortCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";

            return;
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && !
          IsEmpty(export.SortCsePersonsWorkSet.Ssn))
        {
          foreach(var item in ReadCsePersonInvalidSsn2())
          {
            local.ReadCsePersonsWorkSet.Number = entities.CsePerson.Number;

            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            local.Saved.Number = entities.CsePerson.Number;
            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "B";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              return;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

            field1.Error = true;

            var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

            field2.Error = true;

            var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

            field3.Error = true;

            ExitState = "FN0000_NO_RECORDS_FOUND";

            return;
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn) && (
            Lt(local.BlankStartDate.Date, export.From.Date) || Lt
          (local.BlankStartDate.Date, export.To.Date)))
        {
          // checking to see if it is in the date range that was entered if 
          // there is one
          local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

          if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(export.To.Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.To.Date;
          }
          else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
            Lt(local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.From.Date;
          }
          else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.To.Date;
            local.End.Date = import.To.Date;
          }

          local.End.Year = Year(local.Begin.Date);
          local.End.Month = Month(local.Begin.Date);
          local.End.Day = Day(local.Begin.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.Begin.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);
          local.End.Date = AddDays(local.End.Date, 1);
          local.End.Year = Year(local.End.Date);
          local.End.Month = Month(local.End.Date);
          local.End.Day = Day(local.End.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.End.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);

          foreach(var item in ReadCsePersonInvalidSsn4())
          {
            local.ReadCsePersonsWorkSet.Number = entities.CsePerson.Number;
            local.ReadInvalidSsn.Ssn = entities.InvalidSsn.Ssn;

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "C";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              goto Test3;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field1 = GetField(export.From, "date");

            field1.Error = true;

            var field2 = GetField(export.To, "date");

            field2.Error = true;

            ExitState = "FN0000_NO_RECORDS_FOUND";

            return;
          }
        }

Test3:

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "NEXT":
        if (!Equal(export.SortCsePersonsWorkSet.Number, export.HiddenPrev.Number))
          
        {
          var field = GetField(export.SortCsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (!Equal(export.SortCsePersonsWorkSet.Ssn, export.HiddenPrev.Ssn))
        {
          var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (!Equal(export.From.Date, export.FromPrevious.Date))
        {
          var field = GetField(export.From, "date");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (!Equal(export.To.Date, export.ToPrevious.Date))
        {
          var field = GetField(export.To, "date");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (import.Paging.Count <= 1)
        {
          ExitState = "THERE_NO_MORE_RECORDS_TO_DISPLAY";

          return;
        }
        else
        {
          if (import.PageCount.Count == import.Paging.Count)
          {
            ExitState = "THERE_NO_MORE_RECORDS_TO_DISPLAY";

            return;
          }

          ++export.PageCount.Count;

          for(import.Paging.Index = 0; import.Paging.Index < import
            .Paging.Count; ++import.Paging.Index)
          {
            if (!import.Paging.CheckSize())
            {
              break;
            }

            if (export.PageCount.Count == import.Paging.Index + 1)
            {
              local.ReadCsePersonsWorkSet.Number =
                import.Paging.Item.PageCsePersonsWorkSet.Number;
              local.ReadInvalidSsn.Ssn = import.Paging.Item.PageInvalidSsn.Ssn;
              local.BeginRead.Date = import.Paging.Item.PageFrom.Date;
              local.EndRead.Date = import.Paging.Item.PageTo.Date;

              break;
            }
          }

          import.Paging.CheckIndex();
        }

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          export.Display.Update.CsePersonsWorkSet.Assign(
            local.ClearCsePersonsWorkSet);
          MoveInvalidSsn(local.ClearInvalidSsn, export.Display.Update.InvalidSsn);
            
          export.Display.Update.DateWorkArea.Date =
            local.ClearDateWorkArea.Date;
          export.Display.Update.Common.SelectChar =
            local.ClearCommon.SelectChar;
          export.Display.Update.SsnWorkArea.Assign(local.ClearSsnWorkArea);
        }

        export.Display.CheckIndex();
        export.Display.Index = -1;
        export.Display.Count = 0;

        if (!IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn))
        {
          // getting all invalid ssns for a single cse person
          foreach(var item in ReadInvalidSsnCsePerson2())
          {
            local.ReadInvalidSsn.Ssn = entities.InvalidSsn.Ssn;

            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "A";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+ -";

              if (export.PageCount.Count == export.Paging.Count)
              {
                ++export.Paging.Index;
                export.Paging.CheckSize();

                export.Paging.Update.PageCsePersonsWorkSet.Number =
                  export.Display.Item.CsePersonsWorkSet.Number;
                export.Paging.Update.PageDateWorkArea.Date =
                  export.Display.Item.DateWorkArea.Date;
                export.Paging.Update.PageInvalidSsn.Ssn =
                  export.Display.Item.InvalidSsn.Ssn;
                export.Paging.Update.PageFrom.Date = export.From.Date;
                export.Paging.Update.PageTo.Date = export.To.Date;
                export.Paging.Update.PageCsePersonsWorkSet.Flag = "A";
              }

              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

              goto Test4;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field = GetField(export.SortCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";

            return;
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && !
          IsEmpty(export.SortCsePersonsWorkSet.Ssn))
        {
          foreach(var item in ReadCsePersonInvalidSsn2())
          {
            local.ReadCsePersonsWorkSet.Number = entities.CsePerson.Number;

            if (Lt(local.BlankStartDate.Date, import.From.Date) || Lt
              (local.BlankStartDate.Date, import.To.Date))
            {
              // checking to see if it is in the date range that was entered if 
              // there is one
              local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

              if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (!Lt(local.CheckDate.Date, import.From.Date) && !
                  Lt(import.To.Date, local.CheckDate.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
                Lt(local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.From.Date))
                {
                }
                else
                {
                  continue;
                }
              }
              else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
                (local.BlankStartDate.Date, import.To.Date))
              {
                if (Equal(local.CheckDate.Date, import.To.Date))
                {
                }
                else
                {
                  continue;
                }
              }
            }

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            local.Saved.Number = entities.CsePerson.Number;
            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "B";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+ -";

              if (export.PageCount.Count == export.Paging.Count)
              {
                ++export.Paging.Index;
                export.Paging.CheckSize();

                export.Paging.Update.PageCsePersonsWorkSet.Number =
                  export.Display.Item.CsePersonsWorkSet.Number;
                export.Paging.Update.PageDateWorkArea.Date =
                  export.Display.Item.DateWorkArea.Date;
                export.Paging.Update.PageInvalidSsn.Ssn =
                  export.Display.Item.InvalidSsn.Ssn;
                export.Paging.Update.PageFrom.Date = export.From.Date;
                export.Paging.Update.PageTo.Date = export.To.Date;
                export.Paging.Update.PageCsePersonsWorkSet.Flag = "B";
              }

              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

              goto Test4;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field1 = GetField(export.SortSsnWorkArea, "ssnTextPart1");

            field1.Error = true;

            var field2 = GetField(export.SortSsnWorkArea, "ssnTextPart2");

            field2.Error = true;

            var field3 = GetField(export.SortSsnWorkArea, "ssnTextPart3");

            field3.Error = true;

            ExitState = "FN0000_NO_RECORDS_FOUND";

            return;
          }
        }
        else if (IsEmpty(export.SortCsePersonsWorkSet.Number) && IsEmpty
          (export.SortCsePersonsWorkSet.Ssn) && (
            Lt(local.BlankStartDate.Date, export.From.Date) || Lt
          (local.BlankStartDate.Date, export.To.Date)))
        {
          // checking to see if it is in the date range that was entered if 
          // there is one
          local.CheckDate.Date = Date(entities.InvalidSsn.CreatedTstamp);

          if (Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            if (Lt(export.To.Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.To.Date;
          }
          else if (Lt(local.BlankStartDate.Date, import.From.Date) && !
            Lt(local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.From.Date))
            {
              var field = GetField(export.From, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.From.Date;
            local.End.Date = import.From.Date;
          }
          else if (!Lt(local.BlankStartDate.Date, import.From.Date) && Lt
            (local.BlankStartDate.Date, import.To.Date))
          {
            if (Lt(Now().Date, import.To.Date))
            {
              var field = GetField(export.To, "date");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURRENT_DATE";

              return;
            }

            local.Begin.Date = import.To.Date;
            local.End.Date = import.To.Date;
          }

          local.End.Year = Year(local.Begin.Date);
          local.End.Month = Month(local.Begin.Date);
          local.End.Day = Day(local.Begin.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.Begin.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);
          local.End.Date = AddDays(local.End.Date, 1);
          local.End.Year = Year(local.End.Date);
          local.End.Month = Month(local.End.Date);
          local.End.Day = Day(local.End.Date);
          local.Year.Text4 = NumberToString(local.End.Year, 12, 4);
          local.Month.Text2 = NumberToString(local.End.Month, 14, 2);
          local.Day.Text2 = NumberToString(local.End.Day, 14, 2);
          local.End.Timestamp = Timestamp(local.Year.Text4 + "-" + local
            .Month.Text2 + "-" + local.Day.Text2);

          foreach(var item in ReadCsePersonInvalidSsn4())
          {
            local.ReadCsePersonsWorkSet.Number = entities.CsePerson.Number;
            local.ReadInvalidSsn.Ssn = entities.InvalidSsn.Ssn;

            ++export.Display.Index;
            export.Display.CheckSize();

            export.Display.Update.InvalidSsn.CreatedBy =
              entities.InvalidSsn.CreatedBy;
            export.Display.Update.InvalidSsn.Ssn = entities.InvalidSsn.Ssn;
            local.SsnWorkArea.SsnNum9 = entities.InvalidSsn.Ssn;

            if (local.SsnWorkArea.SsnNum9 > 0)
            {
              local.SsnWorkArea.ConvertOption = "1";
              UseCabSsnConvertNumToText();
              ExitState = "ACO_NN0000_ALL_OK";
              export.Display.Update.SsnWorkArea.SsnTextPart1 =
                Substring(local.SsnWorkArea.SsnText9, 1, 3);
              export.Display.Update.SsnWorkArea.SsnTextPart2 =
                Substring(local.SsnWorkArea.SsnText9, 4, 2);
              export.Display.Update.SsnWorkArea.SsnTextPart3 =
                Substring(local.SsnWorkArea.SsnText9, 6, 4);
            }

            export.Display.Update.DateWorkArea.Date =
              Date(entities.InvalidSsn.CreatedTstamp);
            local.Saved.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();
            export.Display.Update.CsePersonsWorkSet.Flag = "C";
            export.Display.Update.CsePersonsWorkSet.FormattedName =
              local.Ap.FormattedName;
            export.Display.Update.CsePersonsWorkSet.Number = local.Ap.Number;

            if (export.Display.Index + 1 == Export.DisplayGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+ -";

              if (export.PageCount.Count == export.Paging.Count)
              {
                ++export.Paging.Index;
                export.Paging.CheckSize();

                export.Paging.Update.PageCsePersonsWorkSet.Number =
                  export.Display.Item.CsePersonsWorkSet.Number;
                export.Paging.Update.PageDateWorkArea.Date =
                  export.Display.Item.DateWorkArea.Date;
                export.Paging.Update.PageInvalidSsn.Ssn =
                  export.Display.Item.InvalidSsn.Ssn;
                export.Paging.Update.PageFrom.Date = export.From.Date;
                export.Paging.Update.PageTo.Date = export.To.Date;
                export.Paging.Update.PageCsePersonsWorkSet.Flag = "C";
              }

              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

              goto Test4;
            }
          }

          if (export.Display.IsEmpty)
          {
            var field1 = GetField(export.From, "date");

            field1.Error = true;

            var field2 = GetField(export.To, "date");

            field2.Error = true;

            ExitState = "FN0000_NO_RECORDS_FOUND";

            return;
          }
        }

Test4:

        if (export.PageCount.Count == 1 && export.Paging.Count > 1)
        {
          export.MoreLessScroll.Text3 = "+";
        }
        else if (export.PageCount.Count < export.Paging.Count && export
          .PageCount.Count > 1)
        {
          export.MoreLessScroll.Text3 = "+ -";
        }
        else if (export.PageCount.Count == export.Paging.Count && export
          .PageCount.Count > 1)
        {
          export.MoreLessScroll.Text3 = "  -";
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveAliases1(Local.AliasesGroup source,
    EabReadAlias.Export.AliasesGroup target)
  {
    target.G.Assign(source.CsePersonsWorkSet);
    target.Gkscares.Flag = source.Kscares.Flag;
    target.Gkanpay.Flag = source.Kanpay.Flag;
    target.Gcse.Flag = source.Cse.Flag;
    target.Gae.Flag = source.Ae.Flag;
  }

  private static void MoveAliases2(Local.AliasesGroup source,
    EabReadAliasBatch.Export.AliasesGroup target)
  {
    target.G.Assign(source.CsePersonsWorkSet);
    target.Gkscares.Flag = source.Kscares.Flag;
    target.Gkanpay.Flag = source.Kanpay.Flag;
    target.Gcse.Flag = source.Cse.Flag;
    target.Gae.Flag = source.Ae.Flag;
  }

  private static void MoveAliases3(EabReadAlias.Export.AliasesGroup source,
    Local.AliasesGroup target)
  {
    target.CsePersonsWorkSet.Assign(source.G);
    target.Kscares.Flag = source.Gkscares.Flag;
    target.Kanpay.Flag = source.Gkanpay.Flag;
    target.Cse.Flag = source.Gcse.Flag;
    target.Ae.Flag = source.Gae.Flag;
  }

  private static void MoveAliases4(EabReadAliasBatch.Export.AliasesGroup source,
    Local.AliasesGroup target)
  {
    target.CsePersonsWorkSet.Assign(source.G);
    target.Kscares.Flag = source.Gkscares.Flag;
    target.Kanpay.Flag = source.Gkanpay.Flag;
    target.Cse.Flag = source.Gcse.Flag;
    target.Ae.Flag = source.Gae.Flag;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Flag = source.Flag;
  }

  private static void MoveInvalidSsn(InvalidSsn source, InvalidSsn target)
  {
    target.Ssn = source.Ssn;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveSsnWorkArea(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNum9 = source.SsnNum9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea(useExport.SsnWorkArea, local.SsnWorkArea);
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

  private void UseEabReadAlias1()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    useImport.CsePersonsWorkSet.UniqueKey = local.NextKeyAp.UniqueKey;
    useExport.AbendData.Assign(local.AbendData);
    local.Aliases.CopyTo(useExport.Aliases, MoveAliases1);
    useExport.NextKey.UniqueKey = local.NextKeyAp.UniqueKey;

    Call(EabReadAlias.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.Aliases.CopyTo(local.Aliases, MoveAliases3);
    local.NextKeyAp.UniqueKey = useExport.NextKey.UniqueKey;
  }

  private void UseEabReadAlias2()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    useImport.CsePersonsWorkSet.Number = local.Saved.Number;
    useExport.AbendData.Assign(local.AbendData);
    local.Aliases.CopyTo(useExport.Aliases, MoveAliases1);
    useExport.NextKey.UniqueKey = local.NextKeyAp.UniqueKey;

    Call(EabReadAlias.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.Aliases.CopyTo(local.Aliases, MoveAliases3);
    local.NextKeyAp.UniqueKey = useExport.NextKey.UniqueKey;
  }

  private void UseEabReadAliasBatch1()
  {
    var useImport = new EabReadAliasBatch.Import();
    var useExport = new EabReadAliasBatch.Export();

    useImport.CsePersonsWorkSet.UniqueKey = local.NextKeyAp.UniqueKey;
    useExport.AbendData.Assign(local.AbendData);
    local.Aliases.CopyTo(useExport.Aliases, MoveAliases2);
    useExport.NextKey.UniqueKey = local.NextKeyAp.UniqueKey;

    Call(EabReadAliasBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.Aliases.CopyTo(local.Aliases, MoveAliases4);
    local.NextKeyAp.UniqueKey = useExport.NextKey.UniqueKey;
  }

  private void UseEabReadAliasBatch2()
  {
    var useImport = new EabReadAliasBatch.Import();
    var useExport = new EabReadAliasBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Saved.Number;
    useExport.AbendData.Assign(local.AbendData);
    local.Aliases.CopyTo(useExport.Aliases, MoveAliases2);
    useExport.NextKey.UniqueKey = local.NextKeyAp.UniqueKey;

    Call(EabReadAliasBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.Aliases.CopyTo(local.Aliases, MoveAliases4);
    local.NextKeyAp.UniqueKey = useExport.NextKey.UniqueKey;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Saved.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Ae.Flag = useExport.Ae.Flag;
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, local.Ap);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.SortCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReadCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateInvalidSsn()
  {
    var cspNumber = entities.CsePerson.Number;
    var ssn = local.SsnWorkArea.SsnNum9;
    var createdBy = global.UserId;
    var createdTstamp = Now();

    entities.InvalidSsn.Populated = false;
    Update("CreateInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "ssn", ssn);
        db.SetNullableDate(command, "fcrSentDate", null);
        db.SetNullableDate(command, "nextCheckDate", null);
        db.SetNullableString(command, "fcrProcessInd", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
      });

    entities.InvalidSsn.CspNumber = cspNumber;
    entities.InvalidSsn.Ssn = ssn;
    entities.InvalidSsn.FcrSentDate = null;
    entities.InvalidSsn.NextCheckDate = null;
    entities.InvalidSsn.FcrProcessInd = "";
    entities.InvalidSsn.CreatedBy = createdBy;
    entities.InvalidSsn.CreatedTstamp = createdTstamp;
    entities.InvalidSsn.Populated = true;
  }

  private void DeleteInvalidSsn()
  {
    Update("DeleteInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.InvalidSsn.CspNumber);
        db.SetInt32(command, "ssn", entities.InvalidSsn.Ssn);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SortCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonInvalidSsn1()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonInvalidSsn1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SortCsePersonsWorkSet.Number);
        db.SetInt32(command, "ssn", local.SsnWorkArea.SsnNum9);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 4);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 5);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 6);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 7);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 8);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonInvalidSsn2()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonInvalidSsn2",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.ReadInvalidSsn.Ssn);
        db.SetString(command, "numb", local.ReadCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 4);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 5);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 6);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 7);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 8);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonInvalidSsn3()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonInvalidSsn3",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.SsnWorkArea.SsnNum9);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 4);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 5);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 6);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 7);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 8);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonInvalidSsn4()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonInvalidSsn4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp1", local.Begin.Timestamp.GetValueOrDefault());
          
        db.SetDateTime(
          command, "createdTstamp2", local.End.Timestamp.GetValueOrDefault());
        db.SetString(command, "numb", local.ReadCsePersonsWorkSet.Number);
        db.SetInt32(command, "ssn", local.ReadInvalidSsn.Ssn);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 4);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 5);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 6);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 7);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 8);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonInvalidSsn5()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonInvalidSsn5",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp1", local.Begin.Timestamp.GetValueOrDefault());
          
        db.SetDateTime(
          command, "createdTstamp2", local.End.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 4);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 5);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 6);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 7);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 8);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadInvalidSsn1()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn1",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.SsnWorkArea.SsnNum9);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 3);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 4);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 5);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.InvalidSsn.Populated = true;
      });
  }

  private bool ReadInvalidSsn2()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.SortCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 3);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 4);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 5);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.InvalidSsn.Populated = true;
      });
  }

  private bool ReadInvalidSsn3()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn3",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.SsnWorkArea.SsnNum9);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 3);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 4);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 5);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.InvalidSsn.Populated = true;
      });
  }

  private bool ReadInvalidSsnCsePerson1()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadInvalidSsnCsePerson1",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", export.Display.Item.InvalidSsn.Ssn);
        db.SetString(
          command, "numb", export.Display.Item.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 3);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 4);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 5);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadInvalidSsnCsePerson2()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadInvalidSsnCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SortCsePersonsWorkSet.Number);
        db.SetInt32(command, "ssn", local.ReadInvalidSsn.Ssn);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 3);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 4);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 5);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadInvalidSsnCsePerson3()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadInvalidSsnCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SortCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 3);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 4);
        entities.InvalidSsn.CreatedBy = db.GetString(reader, 5);
        entities.InvalidSsn.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
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
    /// <summary>A DisplayGroup group.</summary>
    [Serializable]
    public class DisplayGroup
    {
      /// <summary>
      /// A value of SsnWorkArea.
      /// </summary>
      [JsonPropertyName("ssnWorkArea")]
      public SsnWorkArea SsnWorkArea
      {
        get => ssnWorkArea ??= new();
        set => ssnWorkArea = value;
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

      /// <summary>
      /// A value of InvalidSsn.
      /// </summary>
      [JsonPropertyName("invalidSsn")]
      public InvalidSsn InvalidSsn
      {
        get => invalidSsn ??= new();
        set => invalidSsn = value;
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
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private SsnWorkArea ssnWorkArea;
      private Common common;
      private InvalidSsn invalidSsn;
      private CsePersonsWorkSet csePersonsWorkSet;
      private DateWorkArea dateWorkArea;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of PageInvalidSsn.
      /// </summary>
      [JsonPropertyName("pageInvalidSsn")]
      public InvalidSsn PageInvalidSsn
      {
        get => pageInvalidSsn ??= new();
        set => pageInvalidSsn = value;
      }

      /// <summary>
      /// A value of PageCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("pageCsePersonsWorkSet")]
      public CsePersonsWorkSet PageCsePersonsWorkSet
      {
        get => pageCsePersonsWorkSet ??= new();
        set => pageCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of PageDateWorkArea.
      /// </summary>
      [JsonPropertyName("pageDateWorkArea")]
      public DateWorkArea PageDateWorkArea
      {
        get => pageDateWorkArea ??= new();
        set => pageDateWorkArea = value;
      }

      /// <summary>
      /// A value of PageTo.
      /// </summary>
      [JsonPropertyName("pageTo")]
      public DateWorkArea PageTo
      {
        get => pageTo ??= new();
        set => pageTo = value;
      }

      /// <summary>
      /// A value of PageFrom.
      /// </summary>
      [JsonPropertyName("pageFrom")]
      public DateWorkArea PageFrom
      {
        get => pageFrom ??= new();
        set => pageFrom = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private InvalidSsn pageInvalidSsn;
      private CsePersonsWorkSet pageCsePersonsWorkSet;
      private DateWorkArea pageDateWorkArea;
      private DateWorkArea pageTo;
      private DateWorkArea pageFrom;
    }

    /// <summary>
    /// A value of FromPrevious.
    /// </summary>
    [JsonPropertyName("fromPrevious")]
    public DateWorkArea FromPrevious
    {
      get => fromPrevious ??= new();
      set => fromPrevious = value;
    }

    /// <summary>
    /// A value of ToPrevious.
    /// </summary>
    [JsonPropertyName("toPrevious")]
    public DateWorkArea ToPrevious
    {
      get => toPrevious ??= new();
      set => toPrevious = value;
    }

    /// <summary>
    /// A value of FirstAddPass.
    /// </summary>
    [JsonPropertyName("firstAddPass")]
    public Common FirstAddPass
    {
      get => firstAddPass ??= new();
      set => firstAddPass = value;
    }

    /// <summary>
    /// A value of SortSsnWorkArea.
    /// </summary>
    [JsonPropertyName("sortSsnWorkArea")]
    public SsnWorkArea SortSsnWorkArea
    {
      get => sortSsnWorkArea ??= new();
      set => sortSsnWorkArea = value;
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

    /// <summary>
    /// Gets a value of Display.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayGroup> Display => display ??= new(
      DisplayGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Display for json serialization.
    /// </summary>
    [JsonPropertyName("display")]
    [Computed]
    public IList<DisplayGroup> Display_Json
    {
      get => display;
      set => Display.Assign(value);
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public CsePersonsWorkSet Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of PageCount.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public Common PageCount
    {
      get => pageCount ??= new();
      set => pageCount = value;
    }

    /// <summary>
    /// Gets a value of Paging.
    /// </summary>
    [JsonIgnore]
    public Array<PagingGroup> Paging => paging ??= new(PagingGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Paging for json serialization.
    /// </summary>
    [JsonPropertyName("paging")]
    [Computed]
    public IList<PagingGroup> Paging_Json
    {
      get => paging;
      set => Paging.Assign(value);
    }

    /// <summary>
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
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
    /// A value of SortCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("sortCsePersonsWorkSet")]
    public CsePersonsWorkSet SortCsePersonsWorkSet
    {
      get => sortCsePersonsWorkSet ??= new();
      set => sortCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MoreLessScroll.
    /// </summary>
    [JsonPropertyName("moreLessScroll")]
    public WorkArea MoreLessScroll
    {
      get => moreLessScroll ??= new();
      set => moreLessScroll = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    private DateWorkArea fromPrevious;
    private DateWorkArea toPrevious;
    private Common firstAddPass;
    private SsnWorkArea sortSsnWorkArea;
    private DateWorkArea from;
    private DateWorkArea to;
    private Array<DisplayGroup> display;
    private CsePersonsWorkSet flow;
    private Common pageCount;
    private Array<PagingGroup> paging;
    private Common promptCsePersonNumber;
    private Standard standard;
    private CsePersonsWorkSet sortCsePersonsWorkSet;
    private WorkArea moreLessScroll;
    private CsePersonsWorkSet hiddenPrev;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DisplayGroup group.</summary>
    [Serializable]
    public class DisplayGroup
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of SsnWorkArea.
      /// </summary>
      [JsonPropertyName("ssnWorkArea")]
      public SsnWorkArea SsnWorkArea
      {
        get => ssnWorkArea ??= new();
        set => ssnWorkArea = value;
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
      /// A value of InvalidSsn.
      /// </summary>
      [JsonPropertyName("invalidSsn")]
      public InvalidSsn InvalidSsn
      {
        get => invalidSsn ??= new();
        set => invalidSsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Common common;
      private CsePersonsWorkSet csePersonsWorkSet;
      private SsnWorkArea ssnWorkArea;
      private DateWorkArea dateWorkArea;
      private InvalidSsn invalidSsn;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of PageTo.
      /// </summary>
      [JsonPropertyName("pageTo")]
      public DateWorkArea PageTo
      {
        get => pageTo ??= new();
        set => pageTo = value;
      }

      /// <summary>
      /// A value of PageFrom.
      /// </summary>
      [JsonPropertyName("pageFrom")]
      public DateWorkArea PageFrom
      {
        get => pageFrom ??= new();
        set => pageFrom = value;
      }

      /// <summary>
      /// A value of PageInvalidSsn.
      /// </summary>
      [JsonPropertyName("pageInvalidSsn")]
      public InvalidSsn PageInvalidSsn
      {
        get => pageInvalidSsn ??= new();
        set => pageInvalidSsn = value;
      }

      /// <summary>
      /// A value of PageCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("pageCsePersonsWorkSet")]
      public CsePersonsWorkSet PageCsePersonsWorkSet
      {
        get => pageCsePersonsWorkSet ??= new();
        set => pageCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of PageDateWorkArea.
      /// </summary>
      [JsonPropertyName("pageDateWorkArea")]
      public DateWorkArea PageDateWorkArea
      {
        get => pageDateWorkArea ??= new();
        set => pageDateWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DateWorkArea pageTo;
      private DateWorkArea pageFrom;
      private InvalidSsn pageInvalidSsn;
      private CsePersonsWorkSet pageCsePersonsWorkSet;
      private DateWorkArea pageDateWorkArea;
    }

    /// <summary>
    /// A value of FromPrevious.
    /// </summary>
    [JsonPropertyName("fromPrevious")]
    public DateWorkArea FromPrevious
    {
      get => fromPrevious ??= new();
      set => fromPrevious = value;
    }

    /// <summary>
    /// A value of ToPrevious.
    /// </summary>
    [JsonPropertyName("toPrevious")]
    public DateWorkArea ToPrevious
    {
      get => toPrevious ??= new();
      set => toPrevious = value;
    }

    /// <summary>
    /// A value of FirstPassAdd.
    /// </summary>
    [JsonPropertyName("firstPassAdd")]
    public Common FirstPassAdd
    {
      get => firstPassAdd ??= new();
      set => firstPassAdd = value;
    }

    /// <summary>
    /// A value of SortSsnWorkArea.
    /// </summary>
    [JsonPropertyName("sortSsnWorkArea")]
    public SsnWorkArea SortSsnWorkArea
    {
      get => sortSsnWorkArea ??= new();
      set => sortSsnWorkArea = value;
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

    /// <summary>
    /// Gets a value of Display.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayGroup> Display => display ??= new(
      DisplayGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Display for json serialization.
    /// </summary>
    [JsonPropertyName("display")]
    [Computed]
    public IList<DisplayGroup> Display_Json
    {
      get => display;
      set => Display.Assign(value);
    }

    /// <summary>
    /// A value of PageCount.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public Common PageCount
    {
      get => pageCount ??= new();
      set => pageCount = value;
    }

    /// <summary>
    /// Gets a value of Paging.
    /// </summary>
    [JsonIgnore]
    public Array<PagingGroup> Paging => paging ??= new(PagingGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Paging for json serialization.
    /// </summary>
    [JsonPropertyName("paging")]
    [Computed]
    public IList<PagingGroup> Paging_Json
    {
      get => paging;
      set => Paging.Assign(value);
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
    /// A value of SortCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("sortCsePersonsWorkSet")]
    public CsePersonsWorkSet SortCsePersonsWorkSet
    {
      get => sortCsePersonsWorkSet ??= new();
      set => sortCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
    }

    /// <summary>
    /// A value of MoreLessScroll.
    /// </summary>
    [JsonPropertyName("moreLessScroll")]
    public WorkArea MoreLessScroll
    {
      get => moreLessScroll ??= new();
      set => moreLessScroll = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    private DateWorkArea fromPrevious;
    private DateWorkArea toPrevious;
    private Common firstPassAdd;
    private SsnWorkArea sortSsnWorkArea;
    private DateWorkArea from;
    private DateWorkArea to;
    private Array<DisplayGroup> display;
    private Common pageCount;
    private Array<PagingGroup> paging;
    private Standard standard;
    private CsePersonsWorkSet sortCsePersonsWorkSet;
    private Common promptCsePersonNumber;
    private WorkArea moreLessScroll;
    private CsePersonsWorkSet hiddenPrev;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AliasesGroup group.</summary>
    [Serializable]
    public class AliasesGroup
    {
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
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet csePersonsWorkSet;
      private Common kscares;
      private Common kanpay;
      private Common cse;
      private Common ae;
    }

    /// <summary>A ApdGroup group.</summary>
    [Serializable]
    public class ApdGroup
    {
      /// <summary>
      /// A value of ApdCommon.
      /// </summary>
      [JsonPropertyName("apdCommon")]
      public Common ApdCommon
      {
        get => apdCommon ??= new();
        set => apdCommon = value;
      }

      /// <summary>
      /// A value of ApdCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("apdCsePersonsWorkSet")]
      public CsePersonsWorkSet ApdCsePersonsWorkSet
      {
        get => apdCsePersonsWorkSet ??= new();
        set => apdCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ApSsn3D.
      /// </summary>
      [JsonPropertyName("apSsn3D")]
      public WorkArea ApSsn3D
      {
        get => apSsn3D ??= new();
        set => apSsn3D = value;
      }

      /// <summary>
      /// A value of ApSsn2D.
      /// </summary>
      [JsonPropertyName("apSsn2D")]
      public WorkArea ApSsn2D
      {
        get => apSsn2D ??= new();
        set => apSsn2D = value;
      }

      /// <summary>
      /// A value of ApSsn4D.
      /// </summary>
      [JsonPropertyName("apSsn4D")]
      public WorkArea ApSsn4D
      {
        get => apSsn4D ??= new();
        set => apSsn4D = value;
      }

      /// <summary>
      /// A value of ApKscaresd.
      /// </summary>
      [JsonPropertyName("apKscaresd")]
      public Common ApKscaresd
      {
        get => apKscaresd ??= new();
        set => apKscaresd = value;
      }

      /// <summary>
      /// A value of ApKanpayd.
      /// </summary>
      [JsonPropertyName("apKanpayd")]
      public Common ApKanpayd
      {
        get => apKanpayd ??= new();
        set => apKanpayd = value;
      }

      /// <summary>
      /// A value of ApCsed.
      /// </summary>
      [JsonPropertyName("apCsed")]
      public Common ApCsed
      {
        get => apCsed ??= new();
        set => apCsed = value;
      }

      /// <summary>
      /// A value of ApAed.
      /// </summary>
      [JsonPropertyName("apAed")]
      public Common ApAed
      {
        get => apAed ??= new();
        set => apAed = value;
      }

      /// <summary>
      /// A value of ApFad.
      /// </summary>
      [JsonPropertyName("apFad")]
      public Common ApFad
      {
        get => apFad ??= new();
        set => apFad = value;
      }

      /// <summary>
      /// A value of ApDbOccd.
      /// </summary>
      [JsonPropertyName("apDbOccd")]
      public Common ApDbOccd
      {
        get => apDbOccd ??= new();
        set => apDbOccd = value;
      }

      /// <summary>
      /// A value of ApActiveOnKscaresd.
      /// </summary>
      [JsonPropertyName("apActiveOnKscaresd")]
      public Common ApActiveOnKscaresd
      {
        get => apActiveOnKscaresd ??= new();
        set => apActiveOnKscaresd = value;
      }

      /// <summary>
      /// A value of ApActiveOnKankapd.
      /// </summary>
      [JsonPropertyName("apActiveOnKankapd")]
      public Common ApActiveOnKankapd
      {
        get => apActiveOnKankapd ??= new();
        set => apActiveOnKankapd = value;
      }

      /// <summary>
      /// A value of ApActiveOnCsed.
      /// </summary>
      [JsonPropertyName("apActiveOnCsed")]
      public Common ApActiveOnCsed
      {
        get => apActiveOnCsed ??= new();
        set => apActiveOnCsed = value;
      }

      /// <summary>
      /// A value of ApActiveOnAed.
      /// </summary>
      [JsonPropertyName("apActiveOnAed")]
      public Common ApActiveOnAed
      {
        get => apActiveOnAed ??= new();
        set => apActiveOnAed = value;
      }

      /// <summary>
      /// A value of ApActiveOnFad.
      /// </summary>
      [JsonPropertyName("apActiveOnFad")]
      public Common ApActiveOnFad
      {
        get => apActiveOnFad ??= new();
        set => apActiveOnFad = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common apdCommon;
      private CsePersonsWorkSet apdCsePersonsWorkSet;
      private WorkArea apSsn3D;
      private WorkArea apSsn2D;
      private WorkArea apSsn4D;
      private Common apKscaresd;
      private Common apKanpayd;
      private Common apCsed;
      private Common apAed;
      private Common apFad;
      private Common apDbOccd;
      private Common apActiveOnKscaresd;
      private Common apActiveOnKankapd;
      private Common apActiveOnCsed;
      private Common apActiveOnAed;
      private Common apActiveOnFad;
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
    /// Gets a value of Aliases.
    /// </summary>
    [JsonIgnore]
    public Array<AliasesGroup> Aliases => aliases ??= new(
      AliasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Aliases for json serialization.
    /// </summary>
    [JsonPropertyName("aliases")]
    [Computed]
    public IList<AliasesGroup> Aliases_Json
    {
      get => aliases;
      set => Aliases.Assign(value);
    }

    /// <summary>
    /// A value of UseBatch.
    /// </summary>
    [JsonPropertyName("useBatch")]
    public Common UseBatch
    {
      get => useBatch ??= new();
      set => useBatch = value;
    }

    /// <summary>
    /// A value of ReadInvalidSsn.
    /// </summary>
    [JsonPropertyName("readInvalidSsn")]
    public InvalidSsn ReadInvalidSsn
    {
      get => readInvalidSsn ??= new();
      set => readInvalidSsn = value;
    }

    /// <summary>
    /// A value of EndRead.
    /// </summary>
    [JsonPropertyName("endRead")]
    public DateWorkArea EndRead
    {
      get => endRead ??= new();
      set => endRead = value;
    }

    /// <summary>
    /// A value of BeginRead.
    /// </summary>
    [JsonPropertyName("beginRead")]
    public DateWorkArea BeginRead
    {
      get => beginRead ??= new();
      set => beginRead = value;
    }

    /// <summary>
    /// A value of RecordSelected.
    /// </summary>
    [JsonPropertyName("recordSelected")]
    public Common RecordSelected
    {
      get => recordSelected ??= new();
      set => recordSelected = value;
    }

    /// <summary>
    /// A value of NextKeyAp.
    /// </summary>
    [JsonPropertyName("nextKeyAp")]
    public CsePersonsWorkSet NextKeyAp
    {
      get => nextKeyAp ??= new();
      set => nextKeyAp = value;
    }

    /// <summary>
    /// Gets a value of Apd.
    /// </summary>
    [JsonIgnore]
    public Array<ApdGroup> Apd => apd ??= new(ApdGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Apd for json serialization.
    /// </summary>
    [JsonPropertyName("apd")]
    [Computed]
    public IList<ApdGroup> Apd_Json
    {
      get => apd;
      set => Apd.Assign(value);
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of ReadCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("readCsePersonsWorkSet")]
    public CsePersonsWorkSet ReadCsePersonsWorkSet
    {
      get => readCsePersonsWorkSet ??= new();
      set => readCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ClearSsnWorkArea.
    /// </summary>
    [JsonPropertyName("clearSsnWorkArea")]
    public SsnWorkArea ClearSsnWorkArea
    {
      get => clearSsnWorkArea ??= new();
      set => clearSsnWorkArea = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public WorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public WorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public WorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Begin.
    /// </summary>
    [JsonPropertyName("begin")]
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
    }

    /// <summary>
    /// A value of CheckDate.
    /// </summary>
    [JsonPropertyName("checkDate")]
    public DateWorkArea CheckDate
    {
      get => checkDate ??= new();
      set => checkDate = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ClearCommon.
    /// </summary>
    [JsonPropertyName("clearCommon")]
    public Common ClearCommon
    {
      get => clearCommon ??= new();
      set => clearCommon = value;
    }

    /// <summary>
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ClearDateWorkArea.
    /// </summary>
    [JsonPropertyName("clearDateWorkArea")]
    public DateWorkArea ClearDateWorkArea
    {
      get => clearDateWorkArea ??= new();
      set => clearDateWorkArea = value;
    }

    /// <summary>
    /// A value of ClearInvalidSsn.
    /// </summary>
    [JsonPropertyName("clearInvalidSsn")]
    public InvalidSsn ClearInvalidSsn
    {
      get => clearInvalidSsn ??= new();
      set => clearInvalidSsn = value;
    }

    /// <summary>
    /// A value of PageCounter.
    /// </summary>
    [JsonPropertyName("pageCounter")]
    public Common PageCounter
    {
      get => pageCounter ??= new();
      set => pageCounter = value;
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
    /// A value of BlankStartDate.
    /// </summary>
    [JsonPropertyName("blankStartDate")]
    public DateWorkArea BlankStartDate
    {
      get => blankStartDate ??= new();
      set => blankStartDate = value;
    }

    /// <summary>
    /// A value of CountExmp.
    /// </summary>
    [JsonPropertyName("countExmp")]
    public Common CountExmp
    {
      get => countExmp ??= new();
      set => countExmp = value;
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
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public CsePersonsWorkSet Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public CsePersonsWorkSet InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public CsePersonsWorkSet Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    private AbendData abendData;
    private Array<AliasesGroup> aliases;
    private Common useBatch;
    private InvalidSsn readInvalidSsn;
    private DateWorkArea endRead;
    private DateWorkArea beginRead;
    private Common recordSelected;
    private CsePersonsWorkSet nextKeyAp;
    private Array<ApdGroup> apd;
    private Common ae;
    private CsePersonsWorkSet readCsePersonsWorkSet;
    private SsnWorkArea clearSsnWorkArea;
    private WorkArea month;
    private WorkArea day;
    private WorkArea year;
    private DateWorkArea end;
    private DateWorkArea begin;
    private DateWorkArea checkDate;
    private SsnWorkArea ssnWorkArea;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Common clearCommon;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
    private DateWorkArea clearDateWorkArea;
    private InvalidSsn clearInvalidSsn;
    private Common pageCounter;
    private DateWorkArea current;
    private DateWorkArea blankStartDate;
    private Common countExmp;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet saved;
    private CsePersonsWorkSet initialisedToSpaces;
    private CsePerson csePerson;
    private Common selected;
    private NextTranInfo nextTranInfo;
    private CsePersonsWorkSet initialised;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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

    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
  }
#endregion
}

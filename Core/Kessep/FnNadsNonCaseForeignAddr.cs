// Program: FN_NADS_NON_CASE_FOREIGN_ADDR, ID: 372252153, model: 746.
// Short name: SWENADSQ
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
/// A program: FN_NADS_NON_CASE_FOREIGN_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes
/// addresses for APs and ARs.  It also allows requests for postmaster letters 
/// and updates any information pertaining to these letters.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnNadsNonCaseForeignAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_NADS_NON_CASE_FOREIGN_ADDR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnNadsNonCaseForeignAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnNadsNonCaseForeignAddr.
  /// </summary>
  public FnNadsNonCaseForeignAddr(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    // Date	  Author		Reason
    // ------------------------------------------------------------
    // 01/04/99  M. Brown  Copied this procedure from FADS, and made
    //  changes as per non-case requirements.
    // 06/12/01  M. Lachowicz  Do not allow verified and end date greater
    //                         than 6 months from today.  WR 283 A.
    // 08-12-02  K Doshi         PR149011     Fix screen Help Id.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeCabSetMnemonics();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MoveStandard(import.HiddenStandard, export.HiddenStandard);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Current.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // 06/12/01 M.L Start
    local.TodayPlusSixMonths.Date = AddMonths(local.Current.Date, 6);

    // 06/12/01 M.L End
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    if (!Equal(global.Command, "DISPLAY"))
    {
      export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
      export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
      MoveCsePersonAddress2(import.Last, export.Last);
      export.ScrollMinus.Text1 = import.ScrollMinus.Text1;
      export.ScrollPlus.Text1 = import.ScrollPlus.Text1;
      export.ProtectFields.Flag = import.ProtectFields.Flag;
      export.Fips.Flag = import.Fips.Flag;

      if (!import.Import1.IsEmpty)
      {
        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.Sel.SelectChar =
            import.Import1.Item.Sel.SelectChar;
          MoveCsePersonAddress1(import.Import1.Item.CsePersonAddress,
            export.Export1.Update.CsePersonAddress);
          export.Export1.Update.PromptCountry.SelectChar =
            import.Import1.Item.PromptCountry.SelectChar;
          export.Export1.Update.PromptEndCode.SelectChar =
            import.Import1.Item.PromptEndCode.SelectChar;
          export.Export1.Update.PromptSourceCode.SelectChar =
            import.Import1.Item.PromptSourceCode.SelectChar;
          export.Export1.Update.Hidden.Assign(import.Import1.Item.Hidden);
        }

        import.Import1.CheckIndex();
      }

      export.HpromptLineNo.Count = import.HpromptLineNo.Count;

      if (!import.HiddenPageKeys.IsEmpty)
      {
        for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
          .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
        {
          if (!import.HiddenPageKeys.CheckSize())
          {
            break;
          }

          export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          MoveCsePersonAddress2(import.HiddenPageKeys.Item.HiddenPageKey,
            export.HiddenPageKeys.Update.HiddenPageKey);
        }

        import.HiddenPageKeys.CheckIndex();
      }
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
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
      return;

      // : Commented this out, because at this time, we do not want to preserve 
      // the key info.  This is because we want to avoid the situation where the
      // user enters a new person number on screens where this key info is not
      // preserved, and then when they come back here, the old person number
      // from this screen is displayed again. (This is as per Pam Vickers and
      // Marilyn Gasperich.)
      UseScCabNextTranGet();
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "RETCDVL") || Equal(global.Command, "PRMPTRET"))
    {
    }
    else if (Equal(global.Command, "RETNAME"))
    {
      if (Equal(export.HiddenCsePerson.Number, export.CsePersonsWorkSet.Number))
      {
        // : Nothing selected in NAME - no processing required, but need
        //   to execute protection logic at end of prad.
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ---------------------------------------------
    // When the control is returned from a promting
    // screen, populate the data appropriately.
    // ---------------------------------------------
    if (Equal(global.Command, "PRMPTRET"))
    {
      // : Coming back from one of the prompt fields.
      export.Export1.Index = export.HpromptLineNo.Count - 1;
      export.Export1.CheckSize();

      if (AsChar(export.Export1.Item.PromptCountry.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.Export1.Update.CsePersonAddress.Country =
            import.Selected.Cdvalue;
        }

        var field = GetField(export.Export1.Item.CsePersonAddress, "country");

        field.Protected = false;
        field.Focused = true;
      }
      else if (AsChar(export.Export1.Item.PromptEndCode.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.Export1.Update.CsePersonAddress.EndCode =
            import.Selected.Cdvalue;
        }

        var field = GetField(export.Export1.Item.CsePersonAddress, "endCode");

        field.Protected = false;
        field.Focused = true;
      }
      else if (AsChar(export.Export1.Item.PromptSourceCode.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.Export1.Update.CsePersonAddress.Source =
            import.Selected.Cdvalue;
        }

        var field = GetField(export.Export1.Item.CsePersonAddress, "source");

        field.Protected = false;
        field.Focused = true;
      }

      export.HpromptLineNo.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        export.Export1.Update.PromptCountry.SelectChar = "";
        export.Export1.Update.PromptEndCode.SelectChar = "";
        export.Export1.Update.PromptSourceCode.SelectChar = "";
      }

      export.Export1.CheckIndex();

      return;
    }

    if (!Equal(export.HiddenCsePerson.Number, export.CsePersonsWorkSet.Number))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Number = export.HiddenCsePerson.Number;
      }
      else if (!Equal(global.Command, "DISPLAY"))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        if (AsChar(export.ProtectFields.Flag) != 'Y')
        {
          // : If protect flag is on, we need to execute protection logic at the
          // end
          //  of the prad.
          return;
        }
      }
    }

    // ---------------------------------------------
    // Do not allow scrolling when a selection has
    // been made.
    // ---------------------------------------------
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.Sel.SelectChar))
      {
        case '*':
          break;
        case 'S':
          ++local.Select.Count;

          if (local.Select.Count > 1)
          {
            var field1 = GetField(export.Export1.Item.Sel, "selectChar");

            field1.Error = true;
          }

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Export1.Item.Sel, "selectChar");

          field.Error = true;

          local.Error.Count = local.Select.Count + 1;

          break;
      }
    }

    export.Export1.CheckIndex();

    if (local.Error.Count > 0)
    {
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

      return;
    }

    if (local.Select.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    // : Scrolling is not allowed if something is selected.
    if ((local.Select.Count > 0 || local.Error.Count > 0) && (
      Equal(global.Command, "PREV") || Equal(global.Command, "NEXT")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (import.HiddenStandard.PageNumber == 0 || Equal
      (global.Command, "DISPLAY"))
    {
      export.HiddenStandard.PageNumber = 1;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "NEXT":
        if (import.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          break;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        if (IsEmpty(import.ScrollPlus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        ++export.HiddenStandard.PageNumber;
        MoveCsePersonAddress2(export.HiddenPageKeys.Item.HiddenPageKey,
          export.Last);
        UseSiListForeignAddresses();

        if (export.Export1.IsFull)
        {
          export.ScrollPlus.Text1 = "+";

          ++export.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          export.Export1.Index = Export.ExportGroup.Capacity - 2;
          export.Export1.CheckSize();

          MoveCsePersonAddress2(export.Export1.Item.CsePersonAddress,
            export.HiddenPageKeys.Update.HiddenPageKey);
        }
        else
        {
          export.ScrollPlus.Text1 = "";
        }

        export.ScrollMinus.Text1 = "-";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
            local.Maximum.Date))
          {
            export.Export1.Update.CsePersonAddress.EndDate = local.Zero.Date;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "PREV":
        if (IsEmpty(export.ScrollMinus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.HiddenStandard.PageNumber;

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
        export.HiddenPageKeys.CheckSize();

        MoveCsePersonAddress2(export.HiddenPageKeys.Item.HiddenPageKey,
          export.Last);
        UseSiListForeignAddresses();

        if (export.HiddenStandard.PageNumber > 1)
        {
          export.ScrollMinus.Text1 = "-";
        }
        else
        {
          export.ScrollMinus.Text1 = "";
        }

        export.ScrollPlus.Text1 = "+";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
            local.Maximum.Date))
          {
            export.Export1.Update.CsePersonAddress.EndDate = local.Zero.Date;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "LIST":
        // : Check CSE person prompt.
        if (AsChar(export.CsePersonPrompt.SelectChar) == 'S')
        {
          export.CsePersonPrompt.SelectChar = "";
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

          return;
        }
        else if (AsChar(export.CsePersonPrompt.SelectChar) == '+' || IsEmpty
          (export.CsePersonPrompt.SelectChar))
        {
        }
        else
        {
          var field = GetField(export.CsePersonPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (AsChar(export.ProtectFields.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";

          break;
        }

        // : Find the first row selected in the group view.  Keep a counter of 
        // number of prompts selected in the row.  It will be checked outside of
        // this loop.  A previous edit ensures that only one row is selected.
        local.Select.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            local.RowSelected.Flag = "Y";

            if (AsChar(export.Export1.Item.PromptCountry.SelectChar) == 'S')
            {
              local.Select.Count = 1;
            }
            else if (IsEmpty(export.Export1.Item.PromptCountry.SelectChar) || AsChar
              (export.Export1.Item.PromptCountry.SelectChar) == '+')
            {
              // *** OK ***
            }
            else
            {
              var field =
                GetField(export.Export1.Item.PromptCountry, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (AsChar(export.Export1.Item.PromptEndCode.SelectChar) == 'S')
            {
              ++local.Select.Count;
            }
            else if (IsEmpty(export.Export1.Item.PromptEndCode.SelectChar) || AsChar
              (export.Export1.Item.PromptEndCode.SelectChar) == '+')
            {
              // *** OK ***
            }
            else
            {
              var field =
                GetField(export.Export1.Item.PromptEndCode, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (AsChar(export.Export1.Item.PromptSourceCode.SelectChar) == 'S')
            {
              ++local.Select.Count;
            }
            else if (IsEmpty(export.Export1.Item.PromptSourceCode.SelectChar) ||
              AsChar(export.Export1.Item.PromptSourceCode.SelectChar) == '+')
            {
              // *** OK ***
            }
            else
            {
              var field =
                GetField(export.Export1.Item.PromptSourceCode, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (local.Select.Count == 0)
            {
              // : A selection was made on a line, but none of the
              //  promptable fields were selected.
              var field1 =
                GetField(export.Export1.Item.PromptSourceCode, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.PromptEndCode, "selectChar");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.PromptCountry, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

              return;
            }
            else if (local.Select.Count > 1)
            {
              // : Multiple prompts were selected.
              if (AsChar(export.Export1.Item.PromptSourceCode.SelectChar) == 'S'
                )
              {
                var field =
                  GetField(export.Export1.Item.PromptSourceCode, "selectChar");

                field.Error = true;
              }

              if (AsChar(export.Export1.Item.PromptEndCode.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Export1.Item.PromptEndCode, "selectChar");

                field.Error = true;
              }

              if (AsChar(export.Export1.Item.PromptCountry.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Export1.Item.PromptCountry, "selectChar");

                field.Error = true;
              }

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
            }

            // : Escape after locating the first row selected.  A previous edit 
            // ensures
            //   that only one row is selected.
            break;
          }
        }

        export.Export1.CheckIndex();

        if (AsChar(local.RowSelected.Flag) == 'Y')
        {
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          ++export.HpromptLineNo.Count;

          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.PromptCountry.SelectChar) == 'S')
            {
              export.Prompt.CodeName = local.Country.CodeName;
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }

            if (AsChar(export.Export1.Item.PromptEndCode.SelectChar) == 'S')
            {
              export.Prompt.CodeName = local.AddressEndCode.CodeName;
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }

            if (AsChar(export.Export1.Item.PromptSourceCode.SelectChar) == 'S')
            {
              export.Prompt.CodeName = local.AddressSource.CodeName;
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }
          }
        }

        export.Export1.CheckIndex();

        break;
      case "DISPLAY":
        export.ProtectFields.Flag = "N";
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;

        if (ReadCsePerson())
        {
          if (AsChar(entities.CsePerson.Type1) == 'O')
          {
            export.CsePersonsWorkSet.FormattedName =
              entities.CsePerson.OrganizationName ?? Spaces(33);

            if (ReadFips())
            {
              // : The system does not currently handle foreign fips addresses.
              //   If it ever does, only display would be allowed on this 
              // screen.
              export.HiddenCsePerson.Number = export.CsePersonsWorkSet.Number;
              export.ProtectFields.Flag = "Y";
              export.Fips.Flag = "Y";
              ExitState = "FN0000_NO_FIPS_ON_NADS";

              break;
            }
            else
            {
              // OK
            }
          }
          else
          {
            UseCabReadAdabasPerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            UseSiFormatCsePersonName();

            if (AsChar(local.CseFlag.Flag) != 'N')
            {
              // : This is not a Non Case person - fields need to be protected.
              export.ProtectFields.Flag = "Y";
            }
          }
        }
        else
        {
          ExitState = "CSE_PERSON_NF";

          return;
        }

        export.HiddenCsePerson.Number = export.CsePersonsWorkSet.Number;
        export.Last.Identifier = Now();
        export.Last.EndDate = local.Maximum.Date;
        UseSiListForeignAddresses();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }
        else
        {
          return;
        }

        export.ScrollMinus.Text1 = "";
        export.HiddenStandard.PageNumber = 1;

        if (export.Export1.IsFull)
        {
          export.ScrollPlus.Text1 = "+";

          export.Export1.Index = 0;
          export.Export1.CheckSize();

          export.HiddenPageKeys.Index = 0;
          export.HiddenPageKeys.CheckSize();

          MoveCsePersonAddress2(export.Export1.Item.CsePersonAddress,
            export.HiddenPageKeys.Update.HiddenPageKey);

          ++export.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          MoveCsePersonAddress2(export.Last,
            export.HiddenPageKeys.Update.HiddenPageKey);
        }
        else
        {
          export.ScrollPlus.Text1 = "";
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
            local.Maximum.Date))
          {
            export.Export1.Update.CsePersonAddress.EndDate = local.Zero.Date;
            export.Export1.Update.Hidden.EndDate = local.Zero.Date;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "CREATE":
        // ---------------------------------------------
        //          C R E A T E   P R O C E S S I N G
        // --------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Escape to field protection logic.
          break;
        }

        // : Check the field protection flag.
        if (AsChar(export.ProtectFields.Flag) == 'Y')
        {
          // : Set a different message if fips.
          if (AsChar(export.Fips.Flag) == 'Y')
          {
            ExitState = "FN0000_ORG_HAS_FIPS_CANT_ADD_ADR";
          }
          else
          {
            ExitState = "FN0000_PERSON_IS_CASE_RELATED";
          }

          // : Escape to field protection logic.
          break;
        }

        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            export.Export1.Update.PromptCountry.SelectChar = "";
            export.Export1.Update.PromptEndCode.SelectChar = "";
            export.Export1.Update.PromptSourceCode.SelectChar = "";

            if (IsEmpty(export.Export1.Item.CsePersonAddress.Country))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "country");

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (IsEmpty(export.Export1.Item.CsePersonAddress.City))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "city");

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (IsEmpty(export.Export1.Item.CsePersonAddress.Street1))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "street1");

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.Export1.Update.CsePersonAddress.Type1 = "M";

            // : Edit COUNTRY
            local.Verify.Cdvalue =
              export.Export1.Item.CsePersonAddress.Country ?? Spaces(10);
            UseCabValidateCodeValue1();

            if (AsChar(local.ReturnCode.Flag) == 'N')
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "country");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
            }

            // : Edit SOURCE
            if (!IsEmpty(export.Export1.Item.CsePersonAddress.Source))
            {
              local.Verify.Cdvalue =
                export.Export1.Item.CsePersonAddress.Source ?? Spaces(10);
              UseCabValidateCodeValue2();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.CsePersonAddress, "source");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // : Edit VERIFIED DATE.
            if (Equal(export.Export1.Item.CsePersonAddress.VerifiedDate,
              local.Zero.Date))
            {
              export.Export1.Update.CsePersonAddress.VerifiedDate =
                local.Current.Date;
            }
            else if (Lt(export.Export1.Item.CsePersonAddress.VerifiedDate,
              local.Current.Date))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "verifiedDate");

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              return;
            }

            // 06/12/01 M.L Start
            if (Lt(local.TodayPlusSixMonths.Date,
              export.Export1.Item.CsePersonAddress.VerifiedDate))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "verifiedDate");

              field.Error = true;

              ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

              return;
            }

            // 06/12/01 M.L End
            // : Edit END CODE
            if (!IsEmpty(export.Export1.Item.CsePersonAddress.EndCode))
            {
              if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
                local.Zero.Date))
              {
                // : For add, the end date must be entered with the end code.
                //   (for update, we default end date to current date if end 
                // code entered).
                var field =
                  GetField(export.Export1.Item.CsePersonAddress, "endDate");

                field.Error = true;

                ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                return;
              }

              local.Verify.Cdvalue =
                export.Export1.Item.CsePersonAddress.EndCode ?? Spaces(10);
              UseCabValidateCodeValue3();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.CsePersonAddress, "endCode");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

                return;
              }
            }
            else
            {
              // ---------------------------------------------
              // 01/04/99 W.Campbell - Logic added to make
              // sure END CODE is entered with END DATE.
              // Work done on IDCR454.
              // ---------------------------------------------
              if (!Equal(export.Export1.Item.CsePersonAddress.EndDate,
                local.Zero.Date))
              {
                var field1 =
                  GetField(export.Export1.Item.CsePersonAddress, "endCode");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.CsePersonAddress, "endDate");

                field2.Error = true;

                ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                return;
              }
            }

            if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
              local.Zero.Date))
            {
              export.Export1.Update.CsePersonAddress.EndDate =
                local.Maximum.Date;
            }
            else if (!Lt(Now().Date,
              export.Export1.Item.CsePersonAddress.EndDate))
            {
              // : End date must be in the future for add function.
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "endDate");

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_MUST_BE_FUTURE";

              return;
            }

            // 06/12/01 M.L Start
            if (Lt(local.TodayPlusSixMonths.Date,
              export.Export1.Item.CsePersonAddress.EndDate) && Lt
              (export.Export1.Item.CsePersonAddress.EndDate, local.Maximum.Date))
              
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "endDate");

              field.Error = true;

              ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

              return;
            }

            // 06/12/01 M.L End
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // *** Create Address ***
            UseSiCreatePersonForeignAddress();

            if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
              local.Maximum.Date))
            {
              export.Export1.Update.CsePersonAddress.EndDate = local.Zero.Date;
            }

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              // : CSENET action block call.
              local.CsePerson.Number = export.CsePersonsWorkSet.Number;
              ExitState = "ACO_NN0000_ALL_OK";
              local.ScreenId.Command = "NADS";
              UseSiCreateAutoCsenetTrans();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              export.Export1.Update.Sel.SelectChar = "*";
              MoveCsePersonAddress3(export.Export1.Item.CsePersonAddress,
                export.Export1.Update.Hidden);
            }

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "UPDATE":
        // ---------------------------------------------
        //       U P D A T E   P R O C E S S I N G
        // --------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Escape to field protection logic.
          break;
        }

        // : Check the field protection flag.
        if (AsChar(export.ProtectFields.Flag) == 'Y')
        {
          // : Set a different message if fips.
          if (AsChar(export.Fips.Flag) == 'Y')
          {
            ExitState = "FN0000_ORG_HAS_FIPS_CANT_UPD_ADR";
          }
          else
          {
            ExitState = "FN0000_PERSON_IS_CASE_RELATED";
          }

          // : Escape to field protection logic.
          break;
        }

        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            export.Export1.Update.PromptCountry.SelectChar = "";
            export.Export1.Update.PromptEndCode.SelectChar = "";
            export.Export1.Update.PromptSourceCode.SelectChar = "";

            if (Equal(export.Export1.Item.CsePersonAddress.Identifier,
              local.Zero.Timestamp))
            {
              var field = GetField(export.Export1.Item.Sel, "selectChar");

              field.Error = true;

              ExitState = "FN0000_ADDRESS_DOES_NOT_EXIST";

              return;
            }

            if (IsEmpty(export.Export1.Item.CsePersonAddress.Country))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "country");

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (IsEmpty(export.Export1.Item.CsePersonAddress.City))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "city");

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (IsEmpty(export.Export1.Item.CsePersonAddress.Street1))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "street1");

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // : Edit COUNTRY
            if (!IsEmpty(export.Export1.Item.CsePersonAddress.Country))
            {
              local.Verify.Cdvalue =
                export.Export1.Item.CsePersonAddress.Country ?? Spaces(10);
              UseCabValidateCodeValue1();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.CsePersonAddress, "country");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }
            }

            // : Edit SOURCE
            if (!IsEmpty(export.Export1.Item.CsePersonAddress.Source))
            {
              local.Verify.Cdvalue =
                export.Export1.Item.CsePersonAddress.Source ?? Spaces(10);
              UseCabValidateCodeValue2();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.CsePersonAddress, "source");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // : Edit VERIFIED DATE
            if (Equal(export.Export1.Item.CsePersonAddress.VerifiedDate,
              local.Zero.Date))
            {
              export.Export1.Update.CsePersonAddress.VerifiedDate =
                local.Current.Date;
            }
            else if (Lt(export.Export1.Item.CsePersonAddress.VerifiedDate,
              local.Current.Date) && !
              Equal(export.Export1.Item.CsePersonAddress.VerifiedDate,
              export.Export1.Item.Hidden.VerifiedDate))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "verifiedDate");

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              return;
            }

            // 06/12/01 M.L Start
            if (Lt(local.TodayPlusSixMonths.Date,
              export.Export1.Item.CsePersonAddress.VerifiedDate))
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "verifiedDate");

              field.Error = true;

              ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

              return;
            }

            // 06/12/01 M.L End
            // : Edit END CODE
            if (!IsEmpty(export.Export1.Item.CsePersonAddress.EndCode))
            {
              // : Default end date to current date if end code is entered.
              if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
                local.Zero.Date))
              {
                export.Export1.Update.CsePersonAddress.EndDate =
                  local.Current.Date;
              }

              local.Verify.Cdvalue =
                export.Export1.Item.CsePersonAddress.EndCode ?? Spaces(10);
              UseCabValidateCodeValue3();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.CsePersonAddress, "endCode");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

                return;
              }
            }
            else
            {
              // ---------------------------------------------
              // 01/04/99 W.Campbell - Logic added to make
              // sure END CODE is entered with END DATE.
              // Work done on IDCR454.
              // ---------------------------------------------
              if (!Equal(export.Export1.Item.CsePersonAddress.EndDate,
                local.Zero.Date))
              {
                var field1 =
                  GetField(export.Export1.Item.CsePersonAddress, "endCode");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.CsePersonAddress, "endDate");

                field2.Error = true;

                ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                return;
              }
            }

            if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
              local.Zero.Date))
            {
              export.Export1.Update.CsePersonAddress.EndDate =
                local.Maximum.Date;
            }

            // 06/12/01 M.L Start
            if (Lt(local.TodayPlusSixMonths.Date,
              export.Export1.Item.CsePersonAddress.EndDate) && Lt
              (export.Export1.Item.CsePersonAddress.EndDate, local.Maximum.Date))
              
            {
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "endDate");

              field.Error = true;

              ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

              return;
            }

            // 06/12/01 M.L End
            if (Lt(export.Export1.Item.CsePersonAddress.EndDate, Now().Date) &&
              !
              Equal(export.Export1.Item.CsePersonAddress.EndDate,
              export.Export1.Item.Hidden.EndDate))
            {
              // : End date must be greater than or equal to current date.
              var field =
                GetField(export.Export1.Item.CsePersonAddress, "endDate");

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              return;
            }

            local.CsePerson.Number = export.CsePersonsWorkSet.Number;
            export.HiddenCsePerson.Number = local.CsePerson.Number;

            // *** Perform Update ***
            UseSiUpdatePersonForeignAddress();

            if (Equal(export.Export1.Item.CsePersonAddress.EndDate,
              local.Maximum.Date))
            {
              export.Export1.Update.CsePersonAddress.EndDate = local.Zero.Date;
            }

            // : (Update cab sets exitstate to update successful).
            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
              ("ACO_NN0000_ALL_OK"))
            {
              // : CSENET action block call.
              ExitState = "ACO_NN0000_ALL_OK";
              local.CsePerson.Number = export.CsePersonsWorkSet.Number;
              local.ScreenId.Command = "NADS";
              UseSiCreateAutoCsenetTrans();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
              MoveCsePersonAddress3(export.Export1.Item.CsePersonAddress,
                export.Export1.Update.Hidden);
              export.Export1.Update.Sel.SelectChar = "*";
            }

            return;
          }
        }

        export.Export1.CheckIndex();

        // : If we dropped through to here, then nothing was selected for 
        // update.
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // *****     Field Protection Logic     ******
    if (AsChar(export.ProtectFields.Flag) == 'Y')
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        var field1 = GetField(export.Export1.Item.CsePersonAddress, "city");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.CsePersonAddress, "country");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.CsePersonAddress, "endCode");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.CsePersonAddress, "endDate");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.CsePersonAddress, "postalCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.CsePersonAddress, "province");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Export1.Item.CsePersonAddress, "source");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.Export1.Item.CsePersonAddress, "street1");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.Export1.Item.CsePersonAddress, "street2");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.Export1.Item.CsePersonAddress, "street3");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.Export1.Item.CsePersonAddress, "street4");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.Export1.Item.CsePersonAddress, "type1");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 =
          GetField(export.Export1.Item.CsePersonAddress, "verifiedDate");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 =
          GetField(export.Export1.Item.CsePersonAddress, "workerId");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.Export1.Item.PromptCountry, "selectChar");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.Export1.Item.PromptEndCode, "selectChar");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 =
          GetField(export.Export1.Item.PromptSourceCode, "selectChar");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.Export1.Item.Sel, "selectChar");

        field18.Color = "cyan";
        field18.Protected = true;
      }

      export.Export1.CheckIndex();
    }

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.Identifier = source.Identifier;
    target.EndDate = source.EndDate;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroupToExport1(SiListForeignAddresses.Export.
    GroupGroup source, Export.ExportGroup target)
  {
    target.Hidden.Assign(source.Ghet);
    target.PromptCountry.SelectChar = source.GpromptCountry.SelectChar;
    target.PromptSourceCode.SelectChar = source.GpromptSourceCode.SelectChar;
    target.PromptEndCode.SelectChar = source.GpromptEndCode.SelectChar;
    target.PromptReturnCode.SelectChar = source.GpromptReturnCode.SelectChar;
    target.Sel.SelectChar = source.GdetailCommon.SelectChar;
    MoveCsePersonAddress1(source.GdetailCsePersonAddress,
      target.CsePersonAddress);
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CseFlag.Flag = useExport.Cse.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Country.CodeName;
    useImport.CodeValue.Cdvalue = local.Verify.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.Verify.Cdvalue;
    useImport.Code.CodeName = local.AddressSource.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.Verify.Cdvalue;
    useImport.Code.CodeName = local.AddressEndCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.Country.CodeName = useExport.Country.CodeName;
    local.AddressSource.CodeName = useExport.AddressSource.CodeName;
    local.AddressEndCode.CodeName = useExport.AddressEndCode.CodeName;
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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ScreenIdentification.Command = local.ScreenId.Command;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreatePersonForeignAddress()
  {
    var useImport = new SiCreatePersonForeignAddress.Import();
    var useExport = new SiCreatePersonForeignAddress.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveCsePersonAddress1(export.Export1.Item.CsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiCreatePersonForeignAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress,
      export.Export1.Update.CsePersonAddress);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiListForeignAddresses()
  {
    var useImport = new SiListForeignAddresses.Import();
    var useExport = new SiListForeignAddresses.Export();

    useImport.Search.Number = export.CsePersonsWorkSet.Number;
    MoveCsePersonAddress2(export.Last, useImport.LastAddr);

    Call(SiListForeignAddresses.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Export1, MoveGroupToExport1);
    MoveCsePersonAddress2(useExport.LastAddr, export.Last);
  }

  private void UseSiUpdatePersonForeignAddress()
  {
    var useImport = new SiUpdatePersonForeignAddress.Import();
    var useExport = new SiUpdatePersonForeignAddress.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveCsePersonAddress1(export.Export1.Item.CsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiUpdatePersonForeignAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress,
      export.Export1.Update.CsePersonAddress);
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
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 20);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 21);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 22);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 23);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 24);
        entities.CsePerson.CreatedBy = db.GetString(reader, 25);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 26);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 27);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 28);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 30);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 31);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 32);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 33);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 34);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 35);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 36);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 37);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CspNumber = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CsePersonAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of PromptCountry.
      /// </summary>
      [JsonPropertyName("promptCountry")]
      public Common PromptCountry
      {
        get => promptCountry ??= new();
        set => promptCountry = value;
      }

      /// <summary>
      /// A value of PromptSourceCode.
      /// </summary>
      [JsonPropertyName("promptSourceCode")]
      public Common PromptSourceCode
      {
        get => promptSourceCode ??= new();
        set => promptSourceCode = value;
      }

      /// <summary>
      /// A value of PromptReturnCode.
      /// </summary>
      [JsonPropertyName("promptReturnCode")]
      public Common PromptReturnCode
      {
        get => promptReturnCode ??= new();
        set => promptReturnCode = value;
      }

      /// <summary>
      /// A value of PromptEndCode.
      /// </summary>
      [JsonPropertyName("promptEndCode")]
      public Common PromptEndCode
      {
        get => promptEndCode ??= new();
        set => promptEndCode = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonAddress hidden;
      private Common promptCountry;
      private Common promptSourceCode;
      private Common promptReturnCode;
      private Common promptEndCode;
      private Common sel;
      private CsePersonAddress csePersonAddress;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public CsePersonAddress HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonAddress hiddenPageKey;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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
    /// A value of ProtectFields.
    /// </summary>
    [JsonPropertyName("protectFields")]
    public Common ProtectFields
    {
      get => protectFields ??= new();
      set => protectFields = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Common Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
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
    /// A value of HpromptLineNo.
    /// </summary>
    [JsonPropertyName("hpromptLineNo")]
    public Common HpromptLineNo
    {
      get => hpromptLineNo ??= new();
      set => hpromptLineNo = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CsePersonAddress Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ScrollMinus.
    /// </summary>
    [JsonPropertyName("scrollMinus")]
    public WorkArea ScrollMinus
    {
      get => scrollMinus ??= new();
      set => scrollMinus = value;
    }

    /// <summary>
    /// A value of ScrollPlus.
    /// </summary>
    [JsonPropertyName("scrollPlus")]
    public WorkArea ScrollPlus
    {
      get => scrollPlus ??= new();
      set => scrollPlus = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
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
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
    }

    private Array<ImportGroup> import1;
    private Common protectFields;
    private Common fips;
    private CsePerson hiddenCsePerson;
    private Standard standard;
    private Common hpromptLineNo;
    private CodeValue selected;
    private CsePersonAddress last;
    private WorkArea scrollMinus;
    private WorkArea scrollPlus;
    private Common csePersonPrompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private CsePersonAddress lastAddr;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CsePersonAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of PromptCountry.
      /// </summary>
      [JsonPropertyName("promptCountry")]
      public Common PromptCountry
      {
        get => promptCountry ??= new();
        set => promptCountry = value;
      }

      /// <summary>
      /// A value of PromptSourceCode.
      /// </summary>
      [JsonPropertyName("promptSourceCode")]
      public Common PromptSourceCode
      {
        get => promptSourceCode ??= new();
        set => promptSourceCode = value;
      }

      /// <summary>
      /// A value of PromptEndCode.
      /// </summary>
      [JsonPropertyName("promptEndCode")]
      public Common PromptEndCode
      {
        get => promptEndCode ??= new();
        set => promptEndCode = value;
      }

      /// <summary>
      /// A value of PromptReturnCode.
      /// </summary>
      [JsonPropertyName("promptReturnCode")]
      public Common PromptReturnCode
      {
        get => promptReturnCode ??= new();
        set => promptReturnCode = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonAddress hidden;
      private Common promptCountry;
      private Common promptSourceCode;
      private Common promptEndCode;
      private Common promptReturnCode;
      private Common sel;
      private CsePersonAddress csePersonAddress;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public CsePersonAddress HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonAddress hiddenPageKey;
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
    /// A value of ProtectFields.
    /// </summary>
    [JsonPropertyName("protectFields")]
    public Common ProtectFields
    {
      get => protectFields ??= new();
      set => protectFields = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Common Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
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
    /// A value of HpromptLineNo.
    /// </summary>
    [JsonPropertyName("hpromptLineNo")]
    public Common HpromptLineNo
    {
      get => hpromptLineNo ??= new();
      set => hpromptLineNo = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
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
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of ScrollPlus.
    /// </summary>
    [JsonPropertyName("scrollPlus")]
    public WorkArea ScrollPlus
    {
      get => scrollPlus ??= new();
      set => scrollPlus = value;
    }

    /// <summary>
    /// A value of ScrollMinus.
    /// </summary>
    [JsonPropertyName("scrollMinus")]
    public WorkArea ScrollMinus
    {
      get => scrollMinus ??= new();
      set => scrollMinus = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CsePersonAddress Last
    {
      get => last ??= new();
      set => last = value;
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
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
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

    private Array<ExportGroup> export1;
    private Common protectFields;
    private Common fips;
    private CsePerson hiddenCsePerson;
    private Standard standard;
    private Common hpromptLineNo;
    private Code prompt;
    private Common csePersonPrompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard hiddenStandard;
    private WorkArea scrollPlus;
    private WorkArea scrollMinus;
    private CsePersonAddress last;
    private NextTranInfo hiddenNextTranInfo;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private CsePersonAddress lastAddr;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TodayPlusSixMonths.
    /// </summary>
    [JsonPropertyName("todayPlusSixMonths")]
    public DateWorkArea TodayPlusSixMonths
    {
      get => todayPlusSixMonths ??= new();
      set => todayPlusSixMonths = value;
    }

    /// <summary>
    /// A value of RowSelected.
    /// </summary>
    [JsonPropertyName("rowSelected")]
    public Common RowSelected
    {
      get => rowSelected ??= new();
      set => rowSelected = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public Code Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public CodeValue Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of AddressSource.
    /// </summary>
    [JsonPropertyName("addressSource")]
    public Code AddressSource
    {
      get => addressSource ??= new();
      set => addressSource = value;
    }

    /// <summary>
    /// A value of AddressEndCode.
    /// </summary>
    [JsonPropertyName("addressEndCode")]
    public Code AddressEndCode
    {
      get => addressEndCode ??= new();
      set => addressEndCode = value;
    }

    /// <summary>
    /// A value of AddressVerifiedCode.
    /// </summary>
    [JsonPropertyName("addressVerifiedCode")]
    public Code AddressVerifiedCode
    {
      get => addressVerifiedCode ??= new();
      set => addressVerifiedCode = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of CseFlag.
    /// </summary>
    [JsonPropertyName("cseFlag")]
    public Common CseFlag
    {
      get => cseFlag ??= new();
      set => cseFlag = value;
    }

    /// <summary>
    /// A value of DetailText1.
    /// </summary>
    [JsonPropertyName("detailText1")]
    public WorkArea DetailText1
    {
      get => detailText1 ??= new();
      set => detailText1 = value;
    }

    /// <summary>
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of ScreenId.
    /// </summary>
    [JsonPropertyName("screenId")]
    public Common ScreenId
    {
      get => screenId ??= new();
      set => screenId = value;
    }

    private DateWorkArea todayPlusSixMonths;
    private Common rowSelected;
    private DateWorkArea zero;
    private Code country;
    private Common returnCode;
    private CodeValue verify;
    private Code addressSource;
    private Code addressEndCode;
    private Code addressVerifiedCode;
    private Common error;
    private Common select;
    private Common cseFlag;
    private WorkArea detailText1;
    private TextWorkArea detailText10;
    private CsePerson csePerson;
    private DateWorkArea current;
    private TextWorkArea textWorkArea;
    private DateWorkArea maximum;
    private Common screenId;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private Fips fips;
  }
#endregion
}

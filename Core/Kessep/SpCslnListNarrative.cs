// Program: SP_CSLN_LIST_NARRATIVE, ID: 370955625, model: 746.
// Short name: SWECSLNP
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
/// A program: SP_CSLN_LIST_NARRATIVE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCslnListNarrative: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CSLN_LIST_NARRATIVE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCslnListNarrative(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCslnListNarrative.
  /// </summary>
  public SpCslnListNarrative(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************
    // Developer : Sree Veettil
    // Date      : 06-16-2000
    // 09/20/00  SWSRCHF H00104020 Removed SUBSTR statements and replaced with 
    // SET statements
    //                             Set the READ attribute to 'Select Only' for 
    // singleton reads
    //                             Set the READ EACH attribute to 'Uncommitted/
    // Browse'
    // 09/25/00  SWSRCHF H00104086 Removed check on READ EACH for End date = '
    // 2099-12-31' and
    //                             added SORT descending on End date
    // 09/28/00  SWSRCHF H00104562 Removed code setting the USER_ID before 
    // flowing to NATE
    // 01/31/01  SWSRCHF I00111937 Do not display AP name, when the case is OPEN
    // and the
    //                             AP role has been end dated
    // 09/05/02  SWSRKXD PR149011  Fix Screen Help.
    // ******************************************************************
    // *****************************************************************************************
    // 05/20/08   LSS   PR320860 CQ1168  &  PR295785 CQ550
    //                  Modified to initialize page details when selection 
    // criteria has
    //                  changed in order to fix missing data.
    // *****************************************************************************************
    // *****************************************************************************************
    // 05/04/11  T. Pierce  CQ19554
    //                  Modified to allow "LR" and "MR" as valid external event 
    // codes.
    // *****************************************************************************************
    // *****************************************************************************************
    // 09/18/15  GVandy  CQ47981
    //                  Change default Show All indicator from "N" to "Y".
    // *****************************************************************************************
    // 09/27/16   JHarden    CQ 50345   CSLN sreen - filter by all event types.
    // 
    // ****************************************************************************
    switch(TrimEnd(global.Command))
    {
      case "CLEAR":
        export.ShowAll.Flag = "Y";
        export.HeaderStart.Timestamp = Now().AddMonths(-1);
        export.HeaderLast.Timestamp = Now();
        ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NN0000_ALL_OK";

        break;
    }

    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CurrentPage.Count = import.CurrentPage.Count;
    export.HeaderInfrastructure.Assign(import.HeaderInfrastructure);
    export.HeaderCase.Number = import.HeaderCase.Number;
    export.HeaderCsePersonsWorkSet.Assign(import.HeaderCsePersonsWorkSet);
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    MoveDateWorkArea(import.HeaderStart, export.HeaderStart);
    MoveDateWorkArea(import.HeaderLast, export.HeaderLast);
    export.ShowAll.Flag = import.ShowAll.Flag;
    export.Prompt.SelectChar = import.Prompt.SelectChar;
    MoveStandard(import.Scroll, export.Scroll);
    export.FromEvls.Assign(import.FromEvls);
    MoveInfrastructure3(import.ExternalEvent, export.ExternalEvent);
    local.Max.Date = new DateTime(2099, 12, 31);

    if (!import.Group.IsEmpty)
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.Infrastructure.Assign(
          import.Group.Item.Infrastructure);
        export.Group.Update.DateWorkArea.Timestamp =
          import.Group.Item.DateWorkArea.Timestamp;
        export.Group.Update.Display.NarrativeText =
          import.Group.Item.Display.NarrativeText;
        export.Group.Update.NarInd.Flag = import.Group.Item.NarInd.Flag;

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          MoveInfrastructure1(export.Group.Item.Infrastructure, export.Selected);
            
          local.Selected.CreatedTimestamp = export.Selected.CreatedTimestamp;
          ++local.Common.Count;
          local.WorkCommon.Subscript = export.Group.Index + 1;
        }
        else if (!IsEmpty(export.Group.Item.Common.SelectChar) && AsChar
          (export.Group.Item.Common.SelectChar) != '_')
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
      }

      import.Group.CheckIndex();

      if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            if (AsChar(export.Group.Item.Common.SelectChar) == 'S' || IsEmpty
              (export.Group.Item.Common.SelectChar) || AsChar
              (export.Group.Item.Common.SelectChar) == '_')
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Protected = true;
            }
          }
        }

        export.Group.CheckIndex();

        return;
      }
      else if (local.WorkCommon.Subscript != 0)
      {
        export.Group.Index = local.WorkCommon.Subscript;
        export.Group.CheckSize();

        if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
        {
          export.Selected.UserId = "CSLN";
        }
      }
    }

    // -----------------------------------------------------------------------------------------------------------
    // LSS   05/20/2008   PR320860 CQ1168  &  PR295785 CQ550
    //       Added if statement for COMMAND not equal to DISPLAY so that the 
    // previous
    //       key values will not be used unless the command value is other than 
    // Display.
    // -----------------------------------------------------------------------------------------------------------
    if (!Equal(global.Command, "DISPLAY"))
    {
      if (!import.HiddenKeys.IsEmpty)
      {
        for(import.HiddenKeys.Index = 0; import.HiddenKeys.Index < import
          .HiddenKeys.Count; ++import.HiddenKeys.Index)
        {
          if (!import.HiddenKeys.CheckSize())
          {
            break;
          }

          export.HiddenKeys.Index = import.HiddenKeys.Index;
          export.HiddenKeys.CheckSize();

          export.HiddenKeys.Update.GkeyInfrastructure.Assign(
            import.HiddenKeys.Item.Gkey);
          export.HiddenKeys.Update.GkeyNarrativeDetail.Assign(
            import.HiddenKeys.Item.ImpoertGKey);
        }

        import.HiddenKeys.CheckIndex();
      }
    }

    if (local.Common.Count > 1)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
        {
          var field1 = GetField(export.Group.Item.Display, "narrativeText");

          field1.Color = "white";
          field1.Protected = true;

          var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

          field2.Color = "white";
          field2.Protected = true;

          var field3 = GetField(export.Group.Item.Common, "selectChar");

          field3.Protected = true;
        }
      }

      export.Group.CheckIndex();
      ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTION";

      return;
    }

    if (Equal(Date(export.HeaderStart.Timestamp), local.BlankDateWorkArea.Date))
    {
      export.HeaderStart.Timestamp = Now().AddMonths(-1);
    }

    if (Equal(Date(export.HeaderLast.Timestamp), local.BlankDateWorkArea.Date))
    {
      export.HeaderLast.Timestamp = Now();
    }

    local.WorkBatchTimestampWorkArea.TextDateYyyy =
      NumberToString(Year(export.HeaderLast.Timestamp), 4);
    local.WorkBatchTimestampWorkArea.TextDateMm =
      NumberToString(Month(export.HeaderLast.Timestamp), 2);
    local.WorkBatchTimestampWorkArea.TestDateDd =
      NumberToString(Day(export.HeaderLast.Timestamp), 2);
    local.WorkBatchTimestampWorkArea.TextTimestamp =
      local.WorkBatchTimestampWorkArea.TextDateYyyy + "-" + local
      .WorkBatchTimestampWorkArea.TextDateMm + "-" + local
      .WorkBatchTimestampWorkArea.TestDateDd + "-23.59.59.999999";
    export.HeaderLast.Timestamp =
      Timestamp(local.WorkBatchTimestampWorkArea.TextTimestamp);

    if (IsEmpty(export.ShowAll.Flag))
    {
      export.ShowAll.Flag = "Y";
    }

    // ************************************************
    // The cursor should be in the case number field.
    // ************************************************
    if (!IsEmpty(export.HeaderCase.Number))
    {
      if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
      {
        goto Test1;
      }

      local.WorkTextWorkArea.Text10 = export.HeaderCase.Number;
      UseEabPadLeftWithZeros();
      export.HeaderCase.Number = local.WorkTextWorkArea.Text10;

      // ****************************************************************
      // Get the AR anme and the AP name to be displayed on the screen.
      // ****************************************************************
      if (ReadCase())
      {
        export.Ap.FormattedName = "";
        export.Ap.Number = "";
        export.Ar.FormattedName = "";
        export.Ar.Number = "";
      }
      else
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field3 = GetField(export.Group.Item.Display, "narrativeText");

            field3.Color = "white";
            field3.Protected = true;

            var field4 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field4.Color = "white";
            field4.Protected = true;

            var field5 = GetField(export.Group.Item.Common, "selectChar");

            field5.Protected = true;
          }
        }

        export.Group.CheckIndex();

        var field1 = GetField(export.HeaderCase, "number");

        field1.Error = true;

        var field2 = GetField(export.HeaderCase, "number");

        field2.Color = "red";
        field2.Protected = false;
        field2.Focused = true;

        ExitState = "CASE_NF";

        return;
      }

      local.NotFound.Flag = "N";

      // *** Probem report H00104086
      // *** 09/25/00 SWSRCHF
      // *** Removed check on READ EACH for End date = '2099-12-31' and
      // *** added SORT descending on End date
      foreach(var item in ReadCaseRole1())
      {
        // *** Problem report I00111937
        // *** 01/31/01 swsrchf
        // *** start
        if (AsChar(entities.ExistingCase.Status) == 'O' && !
          Equal(entities.ExistingCaseRole.EndDate, local.Max.Date))
        {
          break;
        }

        // *** end
        // *** 01/31/01 swsrchf
        // *** Problem report I00111937
        if (ReadCsePerson())
        {
          local.WorkCsePersonsWorkSet.Number =
            entities.ExistingCsePerson.Number;
          local.ErrOnAdabasUnavailable.Flag = "Y";
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
              {
                var field1 =
                  GetField(export.Group.Item.Display, "narrativeText");

                field1.Color = "white";
                field1.Protected = true;

                var field2 =
                  GetField(export.Group.Item.DateWorkArea, "timestamp");

                field2.Color = "white";
                field2.Protected = true;

                var field3 = GetField(export.Group.Item.Common, "selectChar");

                field3.Protected = true;
              }
            }

            export.Group.CheckIndex();

            return;
          }

          export.Ap.FormattedName = local.WorkCsePersonsWorkSet.FormattedName;
          export.Ap.Number = local.WorkCsePersonsWorkSet.Number;

          break;
        }
        else
        {
          local.NotFound.Flag = "Y";

          var field = GetField(export.Ap, "formattedName");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          break;
        }
      }

      // *** Probem report H00104086
      // *** 09/25/00 SWSRCHF
      // *** Removed check on READ EACH for End date = '2099-12-31' and
      // *** added SORT descending on End date
      foreach(var item in ReadCaseRole2())
      {
        if (ReadCsePerson())
        {
          local.WorkCsePersonsWorkSet.Number =
            entities.ExistingCsePerson.Number;
          local.ErrOnAdabasUnavailable.Flag = "Y";
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
              {
                var field1 =
                  GetField(export.Group.Item.Display, "narrativeText");

                field1.Color = "white";
                field1.Protected = true;

                var field2 =
                  GetField(export.Group.Item.DateWorkArea, "timestamp");

                field2.Color = "white";
                field2.Protected = true;

                var field3 = GetField(export.Group.Item.Common, "selectChar");

                field3.Protected = true;
              }
            }

            export.Group.CheckIndex();

            return;
          }

          export.Ar.FormattedName = local.WorkCsePersonsWorkSet.FormattedName;
          export.Ar.Number = local.WorkCsePersonsWorkSet.Number;

          break;
        }
        else
        {
          local.NotFound.Flag = "Y";

          var field = GetField(export.Ar, "formattedName");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          break;
        }
      }

      if (AsChar(local.NotFound.Flag) == 'Y')
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();

        return;
      }
    }

Test1:

    // ************************************************
    // Validate commands
    // ************************************************
    switch(TrimEnd(global.Command))
    {
      case "NATE":
        if (IsEmpty(export.Selected.CaseNumber))
        {
          export.Selected.CaseNumber = export.HeaderCase.Number;
        }

        ExitState = "ECO_LNK_TO_NATE";

        return;
      case "DISPLAY":
        local.ErrorDetected.Flag = "N";

        if (IsEmpty(export.HeaderCase.Number))
        {
          local.ErrorDetected.Flag = "Y";

          var field = GetField(export.Selected, "caseNumber");

          field.Protected = false;
          field.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          break;
        }

        if (!IsEmpty(export.Prompt.SelectChar))
        {
          local.ErrorDetected.Flag = "Y";

          var field = GetField(export.Prompt, "selectChar");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
        }

        if (IsEmpty(export.ShowAll.Flag))
        {
          export.ShowAll.Flag = "Y";
        }

        if (AsChar(export.ShowAll.Flag) != 'Y' && AsChar
          (export.ShowAll.Flag) != 'N')
        {
          local.ErrorDetected.Flag = "Y";

          var field = GetField(export.ShowAll, "flag");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "SP000_VALUES_CAN_BE_Y_N";

          break;
        }

        if (Equal(export.HeaderStart.Timestamp, null))
        {
          export.HeaderStart.Timestamp = Now().AddMonths(-1);
        }

        if (Equal(export.HeaderLast.Timestamp, null))
        {
          export.HeaderLast.Timestamp = Now();
        }

        export.HeaderStart.Date = Date(export.HeaderStart.Timestamp);

        if (Lt(Now().Date, export.HeaderStart.Date))
        {
          local.ErrorDetected.Flag = "Y";

          var field = GetField(export.HeaderStart, "date");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "SP000_CANNOT_GREATER_CURRENT";

          break;
        }

        export.HeaderLast.Date = Date(export.HeaderLast.Timestamp);

        if (Lt(export.HeaderLast.Date, export.HeaderStart.Date))
        {
          local.ErrorDetected.Flag = "Y";

          var field = GetField(export.HeaderStart, "date");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "SP000_THRUDATE_LESS_THAN_START";

          break;
        }

        // CQ50345  Remove filtering by external event types only.
        export.HeaderCsePersonsWorkSet.MiddleInitial = "";
        export.Scroll.PageNumber = 1;

        break;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ************************************************
          // Flowing from here to Next Tran.
          // ************************************************
          export.Hidden.LastTran = "CSLN";
          export.Hidden.CaseNumber = export.HeaderCase.Number;
          export.Hidden.CsePersonNumber = export.Ap.Number;
          export.Hidden.CsePersonNumberObligee = export.Ar.Number;
          export.Hidden.LegalActionIdentifier =
            (int?)export.Selected.DenormNumeric12.GetValueOrDefault();
          export.Hidden.CourtCaseNumber =
            export.HistFilterLegalAction.CourtCaseNumber ?? "";
          export.Hidden.InfrastructureId =
            export.Selected.SystemGeneratedIdentifier;
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;

            break;
          }

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "HIST":
        if (!Equal(local.Selected.CreatedTimestamp, local.Null1.CreatedTimestamp))
          
        {
          export.HeaderStart.Timestamp = local.Selected.CreatedTimestamp;
        }

        if (!IsEmpty(export.HeaderCase.Number))
        {
          export.Selected.CaseNumber = export.HeaderCase.Number;
        }

        ExitState = "ECO_XFER_FROM_CSLN_TO_HIST";

        return;
      case "LIST":
        if (AsChar(export.Prompt.SelectChar) != 'S')
        {
          local.ErrorDetected.Flag = "Y";

          var field = GetField(export.Prompt, "selectChar");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
            {
              var field1 = GetField(export.Group.Item.Display, "narrativeText");

              field1.Color = "white";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DateWorkArea, "timestamp");

              field2.Color = "white";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.Common, "selectChar");

              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
        }

        ExitState = "ECO_LNK_TO_EVLS";

        return;
      case "NEXT":
        if (IsEmpty(export.HeaderCase.Number))
        {
          var field = GetField(export.HeaderCase, "number");

          field.Protected = false;
          field.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        if (Equal(export.Scroll.ScrollingMessage, "More -"))
        {
          if (AsChar(export.HeaderCsePersonsWorkSet.MiddleInitial) == 'Y')
          {
            ExitState = "SP0000_LIST_IS_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          }

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
            {
              var field1 = GetField(export.Group.Item.Display, "narrativeText");

              field1.Color = "white";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DateWorkArea, "timestamp");

              field2.Color = "white";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.Common, "selectChar");

              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();

          return;
        }
        else if (Equal(export.Scroll.ScrollingMessage, "More"))
        {
          ExitState = "SP0000_NO_MORE_NARRATIVE_DETAILS";

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
            {
              var field1 = GetField(export.Group.Item.Display, "narrativeText");

              field1.Color = "white";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DateWorkArea, "timestamp");

              field2.Color = "white";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.Common, "selectChar");

              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();

          return;
        }

        ++export.Scroll.PageNumber;
        UseSpCabCslnGetNarrDetails();

        if (IsExitState("SP0000_LIST_IS_FULL"))
        {
          export.HeaderCsePersonsWorkSet.MiddleInitial = "Y";
        }

        if (export.Scroll.PageNumber == 1)
        {
          if (export.Group.IsEmpty)
          {
            export.Scroll.PageNumber = 0;
            export.Scroll.ScrollingMessage = "More";
            ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";

            return;
          }
          else if (!export.Group.IsFull)
          {
            export.Scroll.ScrollingMessage = "More";
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (export.Group.IsFull)
          {
            if (export.HiddenKeys.Count > 1)
            {
              export.Scroll.ScrollingMessage = "More +";
            }
            else
            {
              export.Scroll.ScrollingMessage = "More";
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
        }
        else
        {
          if (!export.Group.IsFull)
          {
            export.Scroll.ScrollingMessage = "More -";
          }
          else if (export.Group.IsFull)
          {
            if (export.HiddenKeys.Count > export.Scroll.PageNumber)
            {
              export.Scroll.ScrollingMessage = "More-+";
            }
            else
            {
              export.Scroll.ScrollingMessage = "More -";
            }
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();

        break;
      case "PREV":
        if (IsEmpty(export.HeaderCase.Number))
        {
          var field = GetField(export.HeaderCase, "number");

          field.Protected = false;
          field.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        if (Equal(export.Scroll.ScrollingMessage, "More +") || Equal
          (export.Scroll.ScrollingMessage, "More"))
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
            {
              var field1 = GetField(export.Group.Item.Display, "narrativeText");

              field1.Color = "white";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DateWorkArea, "timestamp");

              field2.Color = "white";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.Common, "selectChar");

              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();

          if (Equal(export.Scroll.ScrollingMessage, "More +"))
          {
            ExitState = "ACO_NI0000_TOP_OF_LIST";
          }
          else
          {
            ExitState = "SP0000_NO_MORE_NARRATIVE_DETAILS";
          }

          return;
        }

        --export.Scroll.PageNumber;
        UseSpCabCslnGetNarrDetails();

        if (export.Scroll.PageNumber == 1)
        {
          if (export.Group.IsEmpty)
          {
            export.Scroll.PageNumber = 0;
            export.Scroll.ScrollingMessage = "More";
            ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";

            return;
          }
          else if (!export.Group.IsFull)
          {
            export.Scroll.ScrollingMessage = "More";
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (export.Group.IsFull)
          {
            if (export.HiddenKeys.Count > 1)
            {
              export.Scroll.ScrollingMessage = "More +";
            }
            else
            {
              export.Scroll.ScrollingMessage = "More";
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
        }
        else
        {
          if (!export.Group.IsFull)
          {
            export.Scroll.ScrollingMessage = "More -";
          }
          else if (export.Group.IsFull)
          {
            if (export.HiddenKeys.Count > export.Scroll.PageNumber)
            {
              export.Scroll.ScrollingMessage = "More-+";
            }
            else
            {
              export.Scroll.ScrollingMessage = "More -";
            }
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();

        break;
      case "RETCSLN":
        if (!Equal(export.HeaderInfrastructure.CreatedTimestamp,
          local.Null1.CreatedTimestamp))
        {
          local.WorkBatchTimestampWorkArea.TextDateYyyy =
            NumberToString(Year(export.HeaderInfrastructure.CreatedTimestamp), 4);
            
          local.WorkBatchTimestampWorkArea.TextDateMm =
            NumberToString(Month(export.HeaderInfrastructure.CreatedTimestamp),
            2);
          local.WorkBatchTimestampWorkArea.TestDateDd =
            NumberToString(Day(export.HeaderInfrastructure.CreatedTimestamp), 2);
            
          local.WorkBatchTimestampWorkArea.TextTimestamp =
            local.WorkBatchTimestampWorkArea.TextDateYyyy + "-" + local
            .WorkBatchTimestampWorkArea.TextDateMm + "-" + local
            .WorkBatchTimestampWorkArea.TestDateDd + "-00.00.00.000000";
          export.HeaderStart.Timestamp =
            Timestamp(local.WorkBatchTimestampWorkArea.TextTimestamp);
        }

        global.Command = "DISPLAY";

        break;
      case "RETNATE":
        export.ShowAll.Flag = "Y";
        export.HeaderStart.Timestamp = Now().AddMonths(-1);
        export.HeaderLast.Timestamp = Now();
        export.HeaderCase.Number = export.HeaderInfrastructure.CaseNumber ?? Spaces
          (10);

        if (!IsEmpty(export.HeaderCase.Number))
        {
          local.WorkTextWorkArea.Text10 = export.HeaderCase.Number;
          UseEabPadLeftWithZeros();
          export.HeaderCase.Number = local.WorkTextWorkArea.Text10;

          // ****************************************************************
          // Get the AR anme and the AP name to be displayed on the screen.
          // ****************************************************************
          if (ReadCase())
          {
            export.Ap.FormattedName = "";
            export.Ap.Number = "";
            export.Ar.FormattedName = "";
            export.Ar.Number = "";
          }
          else
          {
            var field1 = GetField(export.HeaderCase, "number");

            field1.Error = true;

            var field2 = GetField(export.HeaderCase, "number");

            field2.Color = "red";
            field2.Protected = false;
            field2.Focused = true;

            ExitState = "CASE_NF";

            return;
          }

          local.NotFound.Flag = "N";

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole1())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
                  {
                    var field1 =
                      GetField(export.Group.Item.Display, "narrativeText");

                    field1.Color = "white";
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Group.Item.DateWorkArea, "timestamp");

                    field2.Color = "white";
                    field2.Protected = true;

                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Protected = true;
                  }
                }

                export.Group.CheckIndex();

                return;
              }

              export.Ap.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ap.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.NotFound.Flag = "Y";

              var field = GetField(export.Ap, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole2())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
                  {
                    var field1 =
                      GetField(export.Group.Item.Display, "narrativeText");

                    field1.Color = "white";
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Group.Item.DateWorkArea, "timestamp");

                    field2.Color = "white";
                    field2.Protected = true;

                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Protected = true;
                  }
                }

                export.Group.CheckIndex();

                return;
              }

              export.Ar.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ar.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.NotFound.Flag = "Y";

              var field = GetField(export.Ar, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          if (AsChar(local.NotFound.Flag) == 'Y')
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
              {
                var field1 =
                  GetField(export.Group.Item.Display, "narrativeText");

                field1.Color = "white";
                field1.Protected = true;

                var field2 =
                  GetField(export.Group.Item.DateWorkArea, "timestamp");

                field2.Color = "white";
                field2.Protected = true;

                var field3 = GetField(export.Group.Item.Common, "selectChar");

                field3.Protected = true;
              }
            }

            export.Group.CheckIndex();

            return;
          }
        }

        export.Scroll.PageNumber = 1;
        global.Command = "DISPLAY";

        break;
      case "RETHIST":
        export.ShowAll.Flag = "Y";

        if (Equal(export.HeaderInfrastructure.CreatedTimestamp,
          local.Null1.CreatedTimestamp))
        {
          export.HeaderStart.Timestamp = Now().AddMonths(-1);
        }
        else
        {
          export.HeaderStart.Timestamp =
            export.HeaderInfrastructure.CreatedTimestamp;
        }

        export.HeaderLast.Timestamp = Now();
        export.HeaderCase.Number = export.HeaderInfrastructure.CaseNumber ?? Spaces
          (10);

        if (!IsEmpty(export.HeaderCase.Number))
        {
          local.WorkTextWorkArea.Text10 = export.HeaderCase.Number;
          UseEabPadLeftWithZeros();
          export.HeaderCase.Number = local.WorkTextWorkArea.Text10;

          // ****************************************************************
          // Get the AR anme and the AP name to be displayed on the screen.
          // ****************************************************************
          if (ReadCase())
          {
            export.Ap.FormattedName = "";
            export.Ap.Number = "";
            export.Ar.FormattedName = "";
            export.Ar.Number = "";
          }
          else
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
              {
                var field3 =
                  GetField(export.Group.Item.Display, "narrativeText");

                field3.Color = "white";
                field3.Protected = true;

                var field4 =
                  GetField(export.Group.Item.DateWorkArea, "timestamp");

                field4.Color = "white";
                field4.Protected = true;

                var field5 = GetField(export.Group.Item.Common, "selectChar");

                field5.Protected = true;
              }
            }

            export.Group.CheckIndex();

            var field1 = GetField(export.HeaderCase, "number");

            field1.Error = true;

            var field2 = GetField(export.HeaderCase, "number");

            field2.Color = "red";
            field2.Protected = false;
            field2.Focused = true;

            ExitState = "CASE_NF";

            return;
          }

          local.NotFound.Flag = "N";

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole1())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
                  {
                    var field1 =
                      GetField(export.Group.Item.Display, "narrativeText");

                    field1.Color = "white";
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Group.Item.DateWorkArea, "timestamp");

                    field2.Color = "white";
                    field2.Protected = true;

                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Protected = true;
                  }
                }

                export.Group.CheckIndex();

                return;
              }

              export.Ap.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ap.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.NotFound.Flag = "Y";

              var field = GetField(export.Ap, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole2())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
                  {
                    var field1 =
                      GetField(export.Group.Item.Display, "narrativeText");

                    field1.Color = "white";
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Group.Item.DateWorkArea, "timestamp");

                    field2.Color = "white";
                    field2.Protected = true;

                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Protected = true;
                  }
                }

                export.Group.CheckIndex();

                return;
              }

              export.Ar.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ar.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.NotFound.Flag = "Y";

              var field = GetField(export.Ar, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          if (AsChar(local.NotFound.Flag) == 'Y')
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
              {
                var field1 =
                  GetField(export.Group.Item.Display, "narrativeText");

                field1.Color = "white";
                field1.Protected = true;

                var field2 =
                  GetField(export.Group.Item.DateWorkArea, "timestamp");

                field2.Color = "white";
                field2.Protected = true;

                var field3 = GetField(export.Group.Item.Common, "selectChar");

                field3.Protected = true;
              }
            }

            export.Group.CheckIndex();

            return;
          }
        }

        export.Scroll.PageNumber = 1;
        global.Command = "DISPLAY";

        break;
      case "RHST":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();

        break;
      case "RETEVLS":
        export.Prompt.SelectChar = "";
        export.ExternalEvent.EventDetailName = export.FromEvls.Name;

        // CQ50345 Remove filtering by external event types only.
        export.ExternalEvent.EventId = export.FromEvls.ControlNumber;

        // CQ50345 Remove filtering by external event types only.
        export.Scroll.PageNumber = 1;

        if (!IsEmpty(export.HeaderCase.Number))
        {
          global.Command = "DISPLAY";
        }
        else
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
            {
              var field1 = GetField(export.Group.Item.Display, "narrativeText");

              field1.Color = "white";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DateWorkArea, "timestamp");

              field2.Color = "white";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.Common, "selectChar");

              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();
          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";
        }

        break;
      case "RETURN":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "XXFMMENU":
        export.HeaderLast.Timestamp = Now();
        export.HeaderStart.Timestamp = Now().AddMonths(-1);
        export.ShowAll.Flag = "Y";
        ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

        return;
      case "XXNEXTXX":
        UseScCabNextTranGet();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
            {
              var field1 = GetField(export.Group.Item.Display, "narrativeText");

              field1.Color = "white";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DateWorkArea, "timestamp");

              field2.Color = "white";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.Common, "selectChar");

              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();

          var field = GetField(export.HeaderCase, "number");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          return;
        }

        export.Selected.SystemGeneratedIdentifier =
          export.Hidden.InfrastructureId.GetValueOrDefault();
        export.Selected.CsePersonNumber = export.Hidden.CsePersonNumber ?? "";
        export.HeaderLegalAction.CourtCaseNumber =
          export.Hidden.CourtOrderNumber ?? "";
        export.Selected.CaseNumber = export.Hidden.CaseNumber ?? "";
        export.HeaderCase.Number = export.Hidden.CaseNumber ?? Spaces(10);

        if (!IsEmpty(export.HeaderCase.Number))
        {
          local.WorkTextWorkArea.Text10 = export.HeaderCase.Number;
          UseEabPadLeftWithZeros();
          export.HeaderCase.Number = local.WorkTextWorkArea.Text10;

          // ****************************************************************
          // Get the AR anme and the AP name to be displayed on the screen.
          // ****************************************************************
          if (ReadCase())
          {
            export.Ap.FormattedName = "";
            export.Ap.Number = "";
            export.Ar.FormattedName = "";
            export.Ar.Number = "";
          }
          else
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
              {
                var field3 =
                  GetField(export.Group.Item.Display, "narrativeText");

                field3.Color = "white";
                field3.Protected = true;

                var field4 =
                  GetField(export.Group.Item.DateWorkArea, "timestamp");

                field4.Color = "white";
                field4.Protected = true;

                var field5 = GetField(export.Group.Item.Common, "selectChar");

                field5.Protected = true;
              }
            }

            export.Group.CheckIndex();

            var field1 = GetField(export.HeaderCase, "number");

            field1.Error = true;

            var field2 = GetField(export.HeaderCase, "number");

            field2.Color = "red";
            field2.Protected = false;
            field2.Focused = true;

            ExitState = "CASE_NF";

            return;
          }

          local.NotFound.Flag = "N";

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole1())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
                  {
                    var field1 =
                      GetField(export.Group.Item.Display, "narrativeText");

                    field1.Color = "white";
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Group.Item.DateWorkArea, "timestamp");

                    field2.Color = "white";
                    field2.Protected = true;

                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Protected = true;
                  }
                }

                export.Group.CheckIndex();

                return;
              }

              export.Ap.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ap.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.NotFound.Flag = "Y";

              var field = GetField(export.Ap, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole2())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
                  {
                    var field1 =
                      GetField(export.Group.Item.Display, "narrativeText");

                    field1.Color = "white";
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Group.Item.DateWorkArea, "timestamp");

                    field2.Color = "white";
                    field2.Protected = true;

                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Protected = true;
                  }
                }

                export.Group.CheckIndex();

                return;
              }

              export.Ar.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ar.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.NotFound.Flag = "Y";

              var field = GetField(export.Ar, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          if (AsChar(local.NotFound.Flag) == 'Y')
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
              {
                var field1 =
                  GetField(export.Group.Item.Display, "narrativeText");

                field1.Color = "white";
                field1.Protected = true;

                var field2 =
                  GetField(export.Group.Item.DateWorkArea, "timestamp");

                field2.Color = "white";
                field2.Protected = true;

                var field3 = GetField(export.Group.Item.Common, "selectChar");

                field3.Protected = true;
              }
            }

            export.Group.CheckIndex();

            return;
          }
        }
        else
        {
          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        export.Scroll.PageNumber = 1;
        global.Command = "DISPLAY";

        break;
      default:
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test2;
      }

      // ************************************************
      //  Security Validation
      // ************************************************
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();

        return;
      }
    }

Test2:

    if (Equal(global.Command, "DISPLAY") && IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.ErrorDetected.Flag) == 'Y')
      {
        local.ErrorDetected.Flag = "N";

        goto Test4;
      }

      export.Scroll.PageNumber = 1;
      local.Command.Command = global.Command;
      UseSpCabCslnGetNarrDetails();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SP0000_LIST_IS_FULL"))
        {
          goto Test3;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
          {
            var field1 = GetField(export.Group.Item.Display, "narrativeText");

            field1.Color = "white";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

            field2.Color = "white";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();

        return;
      }

Test3:

      if (export.Scroll.PageNumber == 1)
      {
        if (export.Group.IsEmpty)
        {
          export.Scroll.PageNumber = 0;
          export.Scroll.ScrollingMessage = "More";
          ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";

          return;
        }
        else if (!export.Group.IsFull)
        {
          export.Scroll.ScrollingMessage = "More";
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (export.Group.IsFull)
        {
          if (export.HiddenKeys.Count > 1)
          {
            export.Scroll.ScrollingMessage = "More +";
          }
          else
          {
            export.Scroll.ScrollingMessage = "More";
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
      }
      else
      {
        if (!export.Group.IsFull)
        {
          export.Scroll.ScrollingMessage = "More -";
        }
        else if (export.Group.IsFull)
        {
          if (export.HiddenKeys.Count > export.Scroll.PageNumber)
          {
            export.Scroll.ScrollingMessage = "More-+";
          }
          else
          {
            export.Scroll.ScrollingMessage = "More -";
          }
        }
      }
    }

Test4:

    // *******************************************************************
    // All the Narrative details should be displayed in white color
    // *******************************************************************
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (AsChar(export.Group.Item.NarInd.Flag) == 'Y')
      {
        var field1 = GetField(export.Group.Item.Display, "narrativeText");

        field1.Color = "white";
        field1.Protected = true;

        var field2 = GetField(export.Group.Item.DateWorkArea, "timestamp");

        field2.Color = "white";
        field2.Protected = true;

        var field3 = GetField(export.Group.Item.Common, "selectChar");

        field3.Color = "";
        field3.Protected = true;
      }
    }

    export.Group.CheckIndex();
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveGroup(SpCabCslnGetNarrDetails.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.Infrastructure.Assign(source.Infrastructure);
    target.NarrativeDetail.Assign(source.NarrativeDetail);
    target.DateWorkArea.Timestamp = source.DateWorkArea.Timestamp;
    target.Display.NarrativeText = source.Display.NarrativeText;
    target.NarInd.Flag = source.NarInd.Flag;
  }

  private static void MoveHiddenKeys1(Export.HiddenKeysGroup source,
    SpCabCslnGetNarrDetails.Import.HiddenKeysGroup target)
  {
    MoveInfrastructure2(source.GkeyInfrastructure, target.GkeyInfrastructure);
    target.GkeyNarrativeDetail.Assign(source.GkeyNarrativeDetail);
  }

  private static void MoveHiddenKeys2(SpCabCslnGetNarrDetails.Export.
    HiddenKeysGroup source, Export.HiddenKeysGroup target)
  {
    MoveInfrastructure2(source.HiddenKeyInfrastructure,
      target.GkeyInfrastructure);
    target.GkeyNarrativeDetail.Assign(source.HiddenKeyNarrativeDetail);
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessStatus = source.ProcessStatus;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.EventDetailName = source.EventDetailName;
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

    useImport.TextWorkArea.Text10 = local.WorkTextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.WorkTextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.WorkTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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

    useImport.NextTranInfo.Assign(export.Hidden);
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

    useImport.Case1.Number = export.HistFilterCase.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.WorkCsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.WorkCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseSpCabCslnGetNarrDetails()
  {
    var useImport = new SpCabCslnGetNarrDetails.Import();
    var useExport = new SpCabCslnGetNarrDetails.Export();

    useImport.Hidden.PageNumber = export.Scroll.PageNumber;
    useImport.Case1.Number = export.HeaderCase.Number;
    MoveDateWorkArea(export.HeaderLast, useImport.LastDate);
    MoveDateWorkArea(export.HeaderStart, useImport.StartingDate);
    useImport.ShowAll.Flag = export.ShowAll.Flag;
    MoveInfrastructure3(export.ExternalEvent, useImport.ExternalEvent);
    export.HiddenKeys.CopyTo(useImport.HiddenKeys, MoveHiddenKeys1);

    Call(SpCabCslnGetNarrDetails.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
    useExport.HiddenKeys.CopyTo(export.HiddenKeys, MoveHiddenKeys2);
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.HeaderCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole1()
  {
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRole.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingCaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Infrastructure.
      /// </summary>
      [JsonPropertyName("infrastructure")]
      public Infrastructure Infrastructure
      {
        get => infrastructure ??= new();
        set => infrastructure = value;
      }

      /// <summary>
      /// A value of NarrativeDetail.
      /// </summary>
      [JsonPropertyName("narrativeDetail")]
      public NarrativeDetail NarrativeDetail
      {
        get => narrativeDetail ??= new();
        set => narrativeDetail = value;
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
      /// A value of Display.
      /// </summary>
      [JsonPropertyName("display")]
      public NarrativeDetail Display
      {
        get => display ??= new();
        set => display = value;
      }

      /// <summary>
      /// A value of NarInd.
      /// </summary>
      [JsonPropertyName("narInd")]
      public Common NarInd
      {
        get => narInd ??= new();
        set => narInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common common;
      private Infrastructure infrastructure;
      private NarrativeDetail narrativeDetail;
      private DateWorkArea dateWorkArea;
      private NarrativeDetail display;
      private Common narInd;
    }

    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of Gkey.
      /// </summary>
      [JsonPropertyName("gkey")]
      public Infrastructure Gkey
      {
        get => gkey ??= new();
        set => gkey = value;
      }

      /// <summary>
      /// A value of ImpoertGKey.
      /// </summary>
      [JsonPropertyName("impoertGKey")]
      public NarrativeDetail ImpoertGKey
      {
        get => impoertGKey ??= new();
        set => impoertGKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure gkey;
      private NarrativeDetail impoertGKey;
    }

    /// <summary>
    /// A value of HeaderCase.
    /// </summary>
    [JsonPropertyName("headerCase")]
    public Case1 HeaderCase
    {
      get => headerCase ??= new();
      set => headerCase = value;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of ExtraPage.
    /// </summary>
    [JsonPropertyName("extraPage")]
    public Common ExtraPage
    {
      get => extraPage ??= new();
      set => extraPage = value;
    }

    /// <summary>
    /// A value of HeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("headerInfrastructure")]
    public Infrastructure HeaderInfrastructure
    {
      get => headerInfrastructure ??= new();
      set => headerInfrastructure = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public NarrativeDetail Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of HeaderStart.
    /// </summary>
    [JsonPropertyName("headerStart")]
    public DateWorkArea HeaderStart
    {
      get => headerStart ??= new();
      set => headerStart = value;
    }

    /// <summary>
    /// A value of HeaderLast.
    /// </summary>
    [JsonPropertyName("headerLast")]
    public DateWorkArea HeaderLast
    {
      get => headerLast ??= new();
      set => headerLast = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Infrastructure Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of HeaderCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("headerCsePersonsWorkSet")]
    public CsePersonsWorkSet HeaderCsePersonsWorkSet
    {
      get => headerCsePersonsWorkSet ??= new();
      set => headerCsePersonsWorkSet = value;
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
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
    }

    /// <summary>
    /// A value of CallingProcedureNameAs.
    /// </summary>
    [JsonPropertyName("callingProcedureNameAs")]
    public Standard CallingProcedureNameAs
    {
      get => callingProcedureNameAs ??= new();
      set => callingProcedureNameAs = value;
    }

    /// <summary>
    /// A value of HeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("headerServiceProvider")]
    public ServiceProvider HeaderServiceProvider
    {
      get => headerServiceProvider ??= new();
      set => headerServiceProvider = value;
    }

    /// <summary>
    /// A value of HistFilterLegalAction.
    /// </summary>
    [JsonPropertyName("histFilterLegalAction")]
    public LegalAction HistFilterLegalAction
    {
      get => histFilterLegalAction ??= new();
      set => histFilterLegalAction = value;
    }

    /// <summary>
    /// A value of HistFilterCase.
    /// </summary>
    [JsonPropertyName("histFilterCase")]
    public Case1 HistFilterCase
    {
      get => histFilterCase ??= new();
      set => histFilterCase = value;
    }

    /// <summary>
    /// A value of HistFilterCsePerson.
    /// </summary>
    [JsonPropertyName("histFilterCsePerson")]
    public CsePerson HistFilterCsePerson
    {
      get => histFilterCsePerson ??= new();
      set => histFilterCsePerson = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ExternalEvent.
    /// </summary>
    [JsonPropertyName("externalEvent")]
    public Infrastructure ExternalEvent
    {
      get => externalEvent ??= new();
      set => externalEvent = value;
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
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public Infrastructure Processed
    {
      get => processed ??= new();
      set => processed = value;
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
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of FromEvls.
    /// </summary>
    [JsonPropertyName("fromEvls")]
    public Event1 FromEvls
    {
      get => fromEvls ??= new();
      set => fromEvls = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
    }

    private Case1 headerCase;
    private Common showAll;
    private Common extraPage;
    private Infrastructure headerInfrastructure;
    private NarrativeDetail prev;
    private DateWorkArea headerStart;
    private DateWorkArea headerLast;
    private Infrastructure selected;
    private CsePersonsWorkSet headerCsePersonsWorkSet;
    private NextTranInfo hidden;
    private LegalAction headerLegalAction;
    private Standard callingProcedureNameAs;
    private ServiceProvider headerServiceProvider;
    private LegalAction histFilterLegalAction;
    private Case1 histFilterCase;
    private CsePerson histFilterCsePerson;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Infrastructure externalEvent;
    private Common prompt;
    private Infrastructure processed;
    private Common currentPage;
    private Standard scroll;
    private Standard standard;
    private AbendData abendData;
    private Common errOnAdabasUnavailable;
    private Event1 fromEvls;
    private Array<GroupGroup> group;
    private Array<HiddenKeysGroup> hiddenKeys;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Infrastructure.
      /// </summary>
      [JsonPropertyName("infrastructure")]
      public Infrastructure Infrastructure
      {
        get => infrastructure ??= new();
        set => infrastructure = value;
      }

      /// <summary>
      /// A value of NarrativeDetail.
      /// </summary>
      [JsonPropertyName("narrativeDetail")]
      public NarrativeDetail NarrativeDetail
      {
        get => narrativeDetail ??= new();
        set => narrativeDetail = value;
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
      /// A value of Display.
      /// </summary>
      [JsonPropertyName("display")]
      public NarrativeDetail Display
      {
        get => display ??= new();
        set => display = value;
      }

      /// <summary>
      /// A value of NarInd.
      /// </summary>
      [JsonPropertyName("narInd")]
      public Common NarInd
      {
        get => narInd ??= new();
        set => narInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common common;
      private Infrastructure infrastructure;
      private NarrativeDetail narrativeDetail;
      private DateWorkArea dateWorkArea;
      private NarrativeDetail display;
      private Common narInd;
    }

    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of GkeyInfrastructure.
      /// </summary>
      [JsonPropertyName("gkeyInfrastructure")]
      public Infrastructure GkeyInfrastructure
      {
        get => gkeyInfrastructure ??= new();
        set => gkeyInfrastructure = value;
      }

      /// <summary>
      /// A value of GkeyNarrativeDetail.
      /// </summary>
      [JsonPropertyName("gkeyNarrativeDetail")]
      public NarrativeDetail GkeyNarrativeDetail
      {
        get => gkeyNarrativeDetail ??= new();
        set => gkeyNarrativeDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure gkeyInfrastructure;
      private NarrativeDetail gkeyNarrativeDetail;
    }

    /// <summary>
    /// A value of HeaderCase.
    /// </summary>
    [JsonPropertyName("headerCase")]
    public Case1 HeaderCase
    {
      get => headerCase ??= new();
      set => headerCase = value;
    }

    /// <summary>
    /// A value of HeaderLast.
    /// </summary>
    [JsonPropertyName("headerLast")]
    public DateWorkArea HeaderLast
    {
      get => headerLast ??= new();
      set => headerLast = value;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of ExtraPage.
    /// </summary>
    [JsonPropertyName("extraPage")]
    public Common ExtraPage
    {
      get => extraPage ??= new();
      set => extraPage = value;
    }

    /// <summary>
    /// A value of HeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("headerInfrastructure")]
    public Infrastructure HeaderInfrastructure
    {
      get => headerInfrastructure ??= new();
      set => headerInfrastructure = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public NarrativeDetail Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of HeaderStart.
    /// </summary>
    [JsonPropertyName("headerStart")]
    public DateWorkArea HeaderStart
    {
      get => headerStart ??= new();
      set => headerStart = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Infrastructure Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of HeaderCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("headerCsePersonsWorkSet")]
    public CsePersonsWorkSet HeaderCsePersonsWorkSet
    {
      get => headerCsePersonsWorkSet ??= new();
      set => headerCsePersonsWorkSet = value;
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
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
    }

    /// <summary>
    /// A value of CallingProcedureNameAs.
    /// </summary>
    [JsonPropertyName("callingProcedureNameAs")]
    public Standard CallingProcedureNameAs
    {
      get => callingProcedureNameAs ??= new();
      set => callingProcedureNameAs = value;
    }

    /// <summary>
    /// A value of HeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("headerServiceProvider")]
    public ServiceProvider HeaderServiceProvider
    {
      get => headerServiceProvider ??= new();
      set => headerServiceProvider = value;
    }

    /// <summary>
    /// A value of HistFilterLegalAction.
    /// </summary>
    [JsonPropertyName("histFilterLegalAction")]
    public LegalAction HistFilterLegalAction
    {
      get => histFilterLegalAction ??= new();
      set => histFilterLegalAction = value;
    }

    /// <summary>
    /// A value of HistFilterCase.
    /// </summary>
    [JsonPropertyName("histFilterCase")]
    public Case1 HistFilterCase
    {
      get => histFilterCase ??= new();
      set => histFilterCase = value;
    }

    /// <summary>
    /// A value of ZdelExportHistFilter.
    /// </summary>
    [JsonPropertyName("zdelExportHistFilter")]
    public CsePerson ZdelExportHistFilter
    {
      get => zdelExportHistFilter ??= new();
      set => zdelExportHistFilter = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ExternalEvent.
    /// </summary>
    [JsonPropertyName("externalEvent")]
    public Infrastructure ExternalEvent
    {
      get => externalEvent ??= new();
      set => externalEvent = value;
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
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public Infrastructure Processed
    {
      get => processed ??= new();
      set => processed = value;
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
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of ToEvls.
    /// </summary>
    [JsonPropertyName("toEvls")]
    public Event1 ToEvls
    {
      get => toEvls ??= new();
      set => toEvls = value;
    }

    /// <summary>
    /// A value of FromEvls.
    /// </summary>
    [JsonPropertyName("fromEvls")]
    public Event1 FromEvls
    {
      get => fromEvls ??= new();
      set => fromEvls = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
    }

    private Case1 headerCase;
    private DateWorkArea headerLast;
    private Common showAll;
    private Common extraPage;
    private Infrastructure headerInfrastructure;
    private NarrativeDetail prev;
    private DateWorkArea headerStart;
    private Infrastructure selected;
    private CsePersonsWorkSet headerCsePersonsWorkSet;
    private NextTranInfo hidden;
    private LegalAction headerLegalAction;
    private Standard callingProcedureNameAs;
    private ServiceProvider headerServiceProvider;
    private LegalAction histFilterLegalAction;
    private Case1 histFilterCase;
    private CsePerson zdelExportHistFilter;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Infrastructure externalEvent;
    private Common prompt;
    private Infrastructure processed;
    private Common currentPage;
    private Standard scroll;
    private Standard standard;
    private AbendData abendData;
    private Common errOnAdabasUnavailable;
    private Event1 toEvls;
    private Event1 fromEvls;
    private Array<GroupGroup> group;
    private Array<HiddenKeysGroup> hiddenKeys;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ExtrEvt.
    /// </summary>
    [JsonPropertyName("extrEvt")]
    public TextWorkArea ExtrEvt
    {
      get => extrEvt ??= new();
      set => extrEvt = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Infrastructure Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Infrastructure Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of WorkBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("workBatchTimestampWorkArea")]
    public BatchTimestampWorkArea WorkBatchTimestampWorkArea
    {
      get => workBatchTimestampWorkArea ??= new();
      set => workBatchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of WorkCommon.
    /// </summary>
    [JsonPropertyName("workCommon")]
    public Common WorkCommon
    {
      get => workCommon ??= new();
      set => workCommon = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
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
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Common NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of BlankGrpCommon.
    /// </summary>
    [JsonPropertyName("blankGrpCommon")]
    public Common BlankGrpCommon
    {
      get => blankGrpCommon ??= new();
      set => blankGrpCommon = value;
    }

    /// <summary>
    /// A value of BlankGrpNarrativeDetail.
    /// </summary>
    [JsonPropertyName("blankGrpNarrativeDetail")]
    public NarrativeDetail BlankGrpNarrativeDetail
    {
      get => blankGrpNarrativeDetail ??= new();
      set => blankGrpNarrativeDetail = value;
    }

    /// <summary>
    /// A value of ErrorDetected.
    /// </summary>
    [JsonPropertyName("errorDetected")]
    public Common ErrorDetected
    {
      get => errorDetected ??= new();
      set => errorDetected = value;
    }

    /// <summary>
    /// A value of BlankStandard.
    /// </summary>
    [JsonPropertyName("blankStandard")]
    public Standard BlankStandard
    {
      get => blankStandard ??= new();
      set => blankStandard = value;
    }

    /// <summary>
    /// A value of BlankDateWorkArea.
    /// </summary>
    [JsonPropertyName("blankDateWorkArea")]
    public DateWorkArea BlankDateWorkArea
    {
      get => blankDateWorkArea ??= new();
      set => blankDateWorkArea = value;
    }

    /// <summary>
    /// A value of BlankInfrastructure.
    /// </summary>
    [JsonPropertyName("blankInfrastructure")]
    public Infrastructure BlankInfrastructure
    {
      get => blankInfrastructure ??= new();
      set => blankInfrastructure = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of BlankLegalAction.
    /// </summary>
    [JsonPropertyName("blankLegalAction")]
    public LegalAction BlankLegalAction
    {
      get => blankLegalAction ??= new();
      set => blankLegalAction = value;
    }

    /// <summary>
    /// A value of FirstBlankLine.
    /// </summary>
    [JsonPropertyName("firstBlankLine")]
    public Common FirstBlankLine
    {
      get => firstBlankLine ??= new();
      set => firstBlankLine = value;
    }

    /// <summary>
    /// A value of NotFound.
    /// </summary>
    [JsonPropertyName("notFound")]
    public Common NotFound
    {
      get => notFound ??= new();
      set => notFound = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NarrativeDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of FirstSelect.
    /// </summary>
    [JsonPropertyName("firstSelect")]
    public Common FirstSelect
    {
      get => firstSelect ??= new();
      set => firstSelect = value;
    }

    /// <summary>
    /// A value of FirstError.
    /// </summary>
    [JsonPropertyName("firstError")]
    public Common FirstError
    {
      get => firstError ??= new();
      set => firstError = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of WorkCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkCsePersonsWorkSet
    {
      get => workCsePersonsWorkSet ??= new();
      set => workCsePersonsWorkSet = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of WorkTextWorkArea.
    /// </summary>
    [JsonPropertyName("workTextWorkArea")]
    public TextWorkArea WorkTextWorkArea
    {
      get => workTextWorkArea ??= new();
      set => workTextWorkArea = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private TextWorkArea extrEvt;
    private Infrastructure null1;
    private Infrastructure selected;
    private BatchTimestampWorkArea workBatchTimestampWorkArea;
    private Common workCommon;
    private Common command;
    private Common common;
    private Common nextTran;
    private Common found;
    private Common blankGrpCommon;
    private NarrativeDetail blankGrpNarrativeDetail;
    private Common errorDetected;
    private Standard blankStandard;
    private DateWorkArea blankDateWorkArea;
    private Infrastructure blankInfrastructure;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private LegalAction blankLegalAction;
    private Common firstBlankLine;
    private Common notFound;
    private NarrativeDetail new1;
    private Common firstSelect;
    private Common firstError;
    private Infrastructure infrastructure;
    private CsePersonsWorkSet workCsePersonsWorkSet;
    private Common cse;
    private Common errOnAdabasUnavailable;
    private TextWorkArea workTextWorkArea;
    private DateWorkArea max;
    private Common position;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NarrativeDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingNarrativeDetail.
    /// </summary>
    [JsonPropertyName("existingNarrativeDetail")]
    public NarrativeDetail ExistingNarrativeDetail
    {
      get => existingNarrativeDetail ??= new();
      set => existingNarrativeDetail = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    private Event1 existingEvent;
    private CsePerson existingCsePerson;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private NarrativeDetail new1;
    private NarrativeDetail existingNarrativeDetail;
    private Infrastructure existingInfrastructure;
  }
#endregion
}

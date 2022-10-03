// Program: LE_MCSP_MAINTAIN_529_PARTICIPANT, ID: 1902442272, model: 746.
// Short name: SWEMCSPP
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
/// A program: LE_MCSP_MAINTAIN_529_PARTICIPANT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeMcspMaintain529Participant: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_MCSP_MAINTAIN_529_PARTICIPANT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeMcspMaintain529Participant(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeMcspMaintain529Participant.
  /// </summary>
  public LeMcspMaintain529Participant(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------------
    // ---
    // 
    // ---
    // ---
    // 
    // Maintain Full Time Equivalent
    // --
    // -
    // ---
    // 
    // ---
    // ---------------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 06/06/14  GVandy	CQ42192		Initial Development.  Created from a copy of 
    // MFTE.
    // 				
    // ---------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Clear scrolling group if command=clear.
      // ---------------------------------------------
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    export.DisplayHistory.Flag = import.DisplayHistory.Flag;
    export.Search.Number = import.Search.Number;

    if (!IsEmpty(export.Search.Number))
    {
      local.TextWorkArea.Text10 = export.Search.Number;
      UseEabPadLeftWithZeros();
      export.Search.Number = local.TextWorkArea.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------
    // Move group views if command <> display.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      local.CsePersonPrompt.Count = 0;
      local.CourtOrderPrompt.Count = 0;
      local.Select.Count = 0;

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.Data529AccountParticipant.Assign(
          import.Group.Item.Data529AccountParticipant);
        export.Group.Update.Hiddendata529AccountParticipant.Assign(
          import.Group.Item.Hiddendata529AccountParticipant);
        MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
          export.Group.Update.CsePersonsWorkSet);
        MoveCsePersonsWorkSet(import.Group.Item.HiddenCsePersonsWorkSet,
          export.Group.Update.HiddenCsePersonsWorkSet);

        if (!IsEmpty(export.Group.Item.CsePersonsWorkSet.Number))
        {
          local.TextWorkArea.Text10 =
            export.Group.Item.CsePersonsWorkSet.Number;
          UseEabPadLeftWithZeros();
          export.Group.Update.CsePersonsWorkSet.Number =
            local.TextWorkArea.Text10;
        }

        export.Group.Update.CsePerson.SelectChar =
          import.Group.Item.CsePerson.SelectChar;

        switch(AsChar(export.Group.Item.CsePerson.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.CsePersonPrompt.Count;

            break;
          default:
            var field = GetField(export.Group.Item.CsePerson, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        export.Group.Update.CourtOrder.SelectChar =
          import.Group.Item.CourtOrder.SelectChar;

        switch(AsChar(export.Group.Item.CourtOrder.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.CourtOrderPrompt.Count;

            break;
          default:
            var field = GetField(export.Group.Item.CourtOrder, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Select.Count;

            break;
          default:
            var field = GetField(export.Group.Item.CourtOrder, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        export.Group.Next();
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** All valid commands for this AB is validated in the following CASE OF
    // ****
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.Hidden);

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          UseScCabNextTranPut();

          return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "ADD":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "LIST":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "UPDATE":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ---------------------------------------------
        // Populate export views from local next_tran_info view read from the 
        // data base
        // Set command to initial command required or ESCAPE
        // ---------------------------------------------
        export.Hidden.Assign(local.NextTranInfo);
        global.Command = "DISPLAY";

        break;
      case "RETNAME":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.CsePerson.SelectChar) == 'S')
          {
            export.Group.Update.CsePerson.SelectChar = "";

            if (!IsEmpty(import.FromName.Number))
            {
              MoveCsePersonsWorkSet(import.FromName,
                export.Group.Update.CsePersonsWorkSet);
            }

            var field = GetField(export.Group.Item.CsePersonsWorkSet, "number");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
        }

        return;
      case "RETLAPS":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.CourtOrder.SelectChar) == 'S')
          {
            export.Group.Update.CourtOrder.SelectChar = "";

            if (!IsEmpty(import.FromLaps.StandardNumber))
            {
              export.Group.Update.Data529AccountParticipant.StandardNumber =
                import.FromLaps.StandardNumber ?? "";
            }

            var field =
              GetField(export.Group.Item.Data529AccountParticipant,
              "standardNumber");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
        }

        return;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
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

    // ---------------------------------------------
    // Must display before maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      switch(local.Select.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        case 1:
          break;
        default:
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

    // ---------------------------------------------
    // Prompt is only valid on PF4 List.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST"))
    {
      local.TotalPrompts.Count = local.CourtOrderPrompt.Count + local
        .CsePersonPrompt.Count;

      switch(local.TotalPrompts.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        case 1:
          break;
        default:
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.CsePerson.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.CsePerson, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.Group.Item.CourtOrder.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.CsePerson, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";

          return;
      }
    }
    else
    {
      local.TotalPrompts.Count = local.CourtOrderPrompt.Count + local
        .CsePersonPrompt.Count;

      if (local.TotalPrompts.Count == 0)
      {
      }
      else
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.CsePerson.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.CsePerson, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.Group.Item.CourtOrder.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.CourtOrder, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "ADD":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            // ---------------------------------------------
            // An add must be on a previously blank row.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.HiddenCsePersonsWorkSet.Number))
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

              return;
            }

            // ---------------------------------------------
            // Check for entry of mandatory fields.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.Data529AccountParticipant.
              StandardNumber))
            {
              var field =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field.Error = true;

              ExitState = "OE0014_MANDATORY_FIELD_MISSING";
            }

            if (IsEmpty(export.Group.Item.CsePersonsWorkSet.Number))
            {
              var field =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field.Error = true;

              ExitState = "OE0014_MANDATORY_FIELD_MISSING";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- Date and standard number edits.
            if (Equal(export.Group.Item.Data529AccountParticipant.StartDate,
              local.Null1.Date))
            {
              // -- Default start date to current date.
              export.Group.Update.Data529AccountParticipant.StartDate =
                Now().Date;
            }

            if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
              local.Null1.Date))
            {
              // -- Default end date to 2099-12-31.
              export.Group.Update.Data529AccountParticipant.EndDate =
                new DateTime(2099, 12, 31);
            }
            else
            {
              // -- End date must be greater or equal to start date.
              if (Lt(export.Group.Item.Data529AccountParticipant.EndDate,
                export.Group.Item.Data529AccountParticipant.StartDate))
              {
                var field =
                  GetField(export.Group.Item.Data529AccountParticipant,
                  "endDate");

                field.Error = true;

                ExitState = "INVALID_EFF_END_DATE_COMBINATION";
              }
            }

            if (ReadLegalAction())
            {
              // -- Person has some legal role on the court order.  Continue.
            }
            else
            {
              // -- Person does not have a legal role on the court order.
              var field1 =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.Data529AccountParticipant,
                "standardNumber");

              field2.Error = true;

              ExitState = "LE0000_PERSON_NOT_ON_COURT_ORDER";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (Read529AccountParticipant2())
            {
              if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
                new DateTime(2099, 12, 31)))
              {
                export.Group.Update.Data529AccountParticipant.EndDate =
                  local.Null1.Date;
              }

              var field1 =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.Data529AccountParticipant,
                "standardNumber");

              field2.Error = true;

              // -- There is already an overlapping 529 participation timeframe 
              // for the CP and standard number.
              ExitState = "LE0000_529_PARTICIPATION_OVERLAP";
            }
            else
            {
              // -- Continue
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- Retrieve person name
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- Create the 529 participation record.
            UseLeCreate529Participation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.CsePerson.SelectChar = "S";

              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field2.Error = true;

              return;
            }

            if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
              new DateTime(2099, 12, 31)))
            {
              export.Group.Update.Data529AccountParticipant.EndDate =
                local.Null1.Date;
            }

            export.Group.Update.Common.SelectChar = "";
            MoveCsePersonsWorkSet(export.Group.Item.CsePersonsWorkSet,
              export.Group.Update.HiddenCsePersonsWorkSet);
            export.Group.Update.Hiddendata529AccountParticipant.Assign(
              export.Group.Item.Data529AccountParticipant);
            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
          }
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            // ---------------------------------------------
            // An update must be performed on a populated
            // row.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.HiddenCsePersonsWorkSet.Number))
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

              return;
            }

            // ---------------------------------------------
            // Perform data validation for update request.
            // ---------------------------------------------
            if (!Equal(export.Group.Item.Data529AccountParticipant.
              StandardNumber,
              export.Group.Item.Hiddendata529AccountParticipant.
                StandardNumber))
            {
              var field =
                GetField(export.Group.Item.Data529AccountParticipant,
                "standardNumber");

              field.Error = true;

              export.Group.Update.Data529AccountParticipant.StandardNumber =
                export.Group.Item.Hiddendata529AccountParticipant.
                  StandardNumber ?? "";
              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (!Equal(export.Group.Item.CsePersonsWorkSet.Number,
              export.Group.Item.HiddenCsePersonsWorkSet.Number))
            {
              var field =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field.Error = true;

              export.Group.Update.CsePersonsWorkSet.Number =
                export.Group.Item.HiddenCsePersonsWorkSet.Number;
              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // ---------------------------------------------
            // Start or End date must have changed.
            // ---------------------------------------------
            if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
              export.Group.Item.Hiddendata529AccountParticipant.EndDate) && Equal
              (export.Group.Item.Data529AccountParticipant.StartDate,
              export.Group.Item.Hiddendata529AccountParticipant.StartDate))
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "SP0000_DATA_NOT_CHANGED";

              return;
            }

            if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
              local.Null1.Date))
            {
              // -- Default end date to 2099-12-31.
              export.Group.Update.Data529AccountParticipant.EndDate =
                new DateTime(2099, 12, 31);
            }
            else
            {
              // -- End date must be greater or equal to start date.
              if (Lt(export.Group.Item.Data529AccountParticipant.EndDate,
                export.Group.Item.Data529AccountParticipant.StartDate))
              {
                var field =
                  GetField(export.Group.Item.Data529AccountParticipant,
                  "endDate");

                field.Error = true;

                ExitState = "INVALID_EFF_END_DATE_COMBINATION";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (Read529AccountParticipant1())
            {
              if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
                new DateTime(2099, 12, 31)))
              {
                export.Group.Update.Data529AccountParticipant.EndDate =
                  local.Null1.Date;
              }

              var field1 =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.Data529AccountParticipant,
                "standardNumber");

              field2.Error = true;

              // -- There is already an overlapping 529 participation timeframe 
              // for the CP and standard number.
              ExitState = "LE0000_529_PARTICIPATION_OVERLAP";
            }
            else
            {
              // -- Continue
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // ---------------------------------------------
            // Data has passed validation. Update.
            // ---------------------------------------------
            UseLeUpdate529Participation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.CsePerson.SelectChar = "S";

              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.CsePersonsWorkSet, "number");

              field2.Error = true;

              return;
            }

            if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
              new DateTime(2099, 12, 31)))
            {
              export.Group.Update.Data529AccountParticipant.EndDate =
                local.Null1.Date;
            }

            export.Group.Update.Common.SelectChar = "";
            MoveCsePersonsWorkSet(export.Group.Item.CsePersonsWorkSet,
              export.Group.Update.HiddenCsePersonsWorkSet);
            export.Group.Update.Hiddendata529AccountParticipant.Assign(
              export.Group.Item.Data529AccountParticipant);
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
          }
        }

        break;
      case "LIST":
        // -- At this point we know that one and only one prompt was selected.
        // -- Set the data and exit state to flow to the appropriate prompt 
        // screen.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.CsePerson.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_NAME";
          }
          else if (AsChar(export.Group.Item.CourtOrder.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LAPS";
            export.ToLaps.Number = export.Group.Item.CsePersonsWorkSet.Number;
            export.ToLapsListByLrol.OneChar = "L";
          }
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      switch(AsChar(export.DisplayHistory.Flag))
      {
        case 'Y':
          break;
        case 'N':
          break;
        case ' ':
          export.DisplayHistory.Flag = "N";

          break;
        default:
          var field = GetField(export.DisplayHistory, "flag");

          field.Error = true;

          ExitState = "INVALID_INDICATOR_Y_N_SPACE";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in Read529AccountParticipantCsePerson())
      {
        // -- Retrieve person name
        export.Group.Update.CsePersonsWorkSet.Number =
          entities.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Group.Next();

          return;
        }

        MoveCsePersonsWorkSet(export.Group.Item.CsePersonsWorkSet,
          export.Group.Update.HiddenCsePersonsWorkSet);
        export.Group.Update.Data529AccountParticipant.Assign(
          entities.Data529AccountParticipant);
        export.Group.Update.Hiddendata529AccountParticipant.Assign(
          entities.Data529AccountParticipant);

        if (Equal(export.Group.Item.Data529AccountParticipant.EndDate,
          new DateTime(2099, 12, 31)))
        {
          export.Group.Update.Data529AccountParticipant.EndDate =
            local.Null1.Date;
        }

        export.Group.Next();
      }

      if (export.Group.IsEmpty)
      {
        ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveData529AccountParticipant(
    Data529AccountParticipant source, Data529AccountParticipant target)
  {
    target.Identifier = source.Identifier;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
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

  private void UseLeCreate529Participation()
  {
    var useImport = new LeCreate529Participation.Import();
    var useExport = new LeCreate529Participation.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Group.Item.CsePersonsWorkSet.Number;
    useImport.Data529AccountParticipant.Assign(
      export.Group.Item.Data529AccountParticipant);

    Call(LeCreate529Participation.Execute, useImport, useExport);

    export.Group.Update.Data529AccountParticipant.Assign(
      useExport.Data529AccountParticipant);
  }

  private void UseLeUpdate529Participation()
  {
    var useImport = new LeUpdate529Participation.Import();
    var useExport = new LeUpdate529Participation.Export();

    useImport.Data529AccountParticipant.Assign(
      export.Group.Item.Data529AccountParticipant);
    useImport.CsePersonsWorkSet.Number =
      export.Group.Item.CsePersonsWorkSet.Number;

    Call(LeUpdate529Participation.Execute, useImport, useExport);

    MoveData529AccountParticipant(useExport.Data529AccountParticipant,
      export.Group.Update.Data529AccountParticipant);
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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Group.Item.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.Group.Update.CsePersonsWorkSet);
  }

  private bool Read529AccountParticipant1()
  {
    entities.Data529AccountParticipant.Populated = false;

    return Read("Read529AccountParticipant1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.Group.Item.CsePersonsWorkSet.Number);
        db.SetNullableString(
          command, "standardNo",
          export.Group.Item.Data529AccountParticipant.StandardNumber ?? "");
        db.SetNullableDate(
          command, "startDate",
          export.Group.Item.Data529AccountParticipant.EndDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          export.Group.Item.Data529AccountParticipant.StartDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "identifier",
          export.Group.Item.Data529AccountParticipant.Identifier);
      },
      (db, reader) =>
      {
        entities.Data529AccountParticipant.Identifier = db.GetInt32(reader, 0);
        entities.Data529AccountParticipant.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.Data529AccountParticipant.StartDate =
          db.GetNullableDate(reader, 2);
        entities.Data529AccountParticipant.EndDate =
          db.GetNullableDate(reader, 3);
        entities.Data529AccountParticipant.CreatedBy = db.GetString(reader, 4);
        entities.Data529AccountParticipant.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Data529AccountParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Data529AccountParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.Data529AccountParticipant.CspNumber = db.GetString(reader, 8);
        entities.Data529AccountParticipant.Populated = true;
      });
  }

  private bool Read529AccountParticipant2()
  {
    entities.Data529AccountParticipant.Populated = false;

    return Read("Read529AccountParticipant2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.Group.Item.CsePersonsWorkSet.Number);
        db.SetNullableString(
          command, "standardNo",
          export.Group.Item.Data529AccountParticipant.StandardNumber ?? "");
        db.SetNullableDate(
          command, "startDate",
          export.Group.Item.Data529AccountParticipant.EndDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          export.Group.Item.Data529AccountParticipant.StartDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Data529AccountParticipant.Identifier = db.GetInt32(reader, 0);
        entities.Data529AccountParticipant.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.Data529AccountParticipant.StartDate =
          db.GetNullableDate(reader, 2);
        entities.Data529AccountParticipant.EndDate =
          db.GetNullableDate(reader, 3);
        entities.Data529AccountParticipant.CreatedBy = db.GetString(reader, 4);
        entities.Data529AccountParticipant.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Data529AccountParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Data529AccountParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.Data529AccountParticipant.CspNumber = db.GetString(reader, 8);
        entities.Data529AccountParticipant.Populated = true;
      });
  }

  private IEnumerable<bool> Read529AccountParticipantCsePerson()
  {
    return ReadEach("Read529AccountParticipantCsePerson",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", export.Search.Number);
        db.SetNullableDate(command, "endDate", date);
        db.SetString(command, "flag", export.DisplayHistory.Flag);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Data529AccountParticipant.Identifier = db.GetInt32(reader, 0);
        entities.Data529AccountParticipant.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.Data529AccountParticipant.StartDate =
          db.GetNullableDate(reader, 2);
        entities.Data529AccountParticipant.EndDate =
          db.GetNullableDate(reader, 3);
        entities.Data529AccountParticipant.CreatedBy = db.GetString(reader, 4);
        entities.Data529AccountParticipant.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Data529AccountParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Data529AccountParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.Data529AccountParticipant.CspNumber = db.GetString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.Data529AccountParticipant.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          export.Group.Item.Data529AccountParticipant.StandardNumber ?? "");
        db.SetNullableString(
          command, "cspNumber", export.Group.Item.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
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
      /// A value of Data529AccountParticipant.
      /// </summary>
      [JsonPropertyName("data529AccountParticipant")]
      public Data529AccountParticipant Data529AccountParticipant
      {
        get => data529AccountParticipant ??= new();
        set => data529AccountParticipant = value;
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
      /// A value of Hiddendata529AccountParticipant.
      /// </summary>
      [JsonPropertyName("hiddendata529AccountParticipant")]
      public Data529AccountParticipant Hiddendata529AccountParticipant
      {
        get => hiddendata529AccountParticipant ??= new();
        set => hiddendata529AccountParticipant = value;
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
      /// A value of CsePerson.
      /// </summary>
      [JsonPropertyName("csePerson")]
      public Common CsePerson
      {
        get => csePerson ??= new();
        set => csePerson = value;
      }

      /// <summary>
      /// A value of CourtOrder.
      /// </summary>
      [JsonPropertyName("courtOrder")]
      public Common CourtOrder
      {
        get => courtOrder ??= new();
        set => courtOrder = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Data529AccountParticipant data529AccountParticipant;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Data529AccountParticipant hiddendata529AccountParticipant;
      private CsePersonsWorkSet hiddenCsePersonsWorkSet;
      private Common csePerson;
      private Common courtOrder;
      private Common common;
    }

    /// <summary>
    /// A value of FromLaps.
    /// </summary>
    [JsonPropertyName("fromLaps")]
    public LegalAction FromLaps
    {
      get => fromLaps ??= new();
      set => fromLaps = value;
    }

    /// <summary>
    /// A value of FromName.
    /// </summary>
    [JsonPropertyName("fromName")]
    public CsePersonsWorkSet FromName
    {
      get => fromName ??= new();
      set => fromName = value;
    }

    /// <summary>
    /// A value of DisplayHistory.
    /// </summary>
    [JsonPropertyName("displayHistory")]
    public Common DisplayHistory
    {
      get => displayHistory ??= new();
      set => displayHistory = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePerson Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private LegalAction fromLaps;
    private CsePersonsWorkSet fromName;
    private Common displayHistory;
    private CsePerson search;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
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
      /// A value of Data529AccountParticipant.
      /// </summary>
      [JsonPropertyName("data529AccountParticipant")]
      public Data529AccountParticipant Data529AccountParticipant
      {
        get => data529AccountParticipant ??= new();
        set => data529AccountParticipant = value;
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
      /// A value of Hiddendata529AccountParticipant.
      /// </summary>
      [JsonPropertyName("hiddendata529AccountParticipant")]
      public Data529AccountParticipant Hiddendata529AccountParticipant
      {
        get => hiddendata529AccountParticipant ??= new();
        set => hiddendata529AccountParticipant = value;
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
      /// A value of CsePerson.
      /// </summary>
      [JsonPropertyName("csePerson")]
      public Common CsePerson
      {
        get => csePerson ??= new();
        set => csePerson = value;
      }

      /// <summary>
      /// A value of CourtOrder.
      /// </summary>
      [JsonPropertyName("courtOrder")]
      public Common CourtOrder
      {
        get => courtOrder ??= new();
        set => courtOrder = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Data529AccountParticipant data529AccountParticipant;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Data529AccountParticipant hiddendata529AccountParticipant;
      private CsePersonsWorkSet hiddenCsePersonsWorkSet;
      private Common csePerson;
      private Common courtOrder;
      private Common common;
    }

    /// <summary>
    /// A value of ToLapsListByLrol.
    /// </summary>
    [JsonPropertyName("toLapsListByLrol")]
    public Standard ToLapsListByLrol
    {
      get => toLapsListByLrol ??= new();
      set => toLapsListByLrol = value;
    }

    /// <summary>
    /// A value of ToLaps.
    /// </summary>
    [JsonPropertyName("toLaps")]
    public CsePersonsWorkSet ToLaps
    {
      get => toLaps ??= new();
      set => toLaps = value;
    }

    /// <summary>
    /// A value of DisplayHistory.
    /// </summary>
    [JsonPropertyName("displayHistory")]
    public Common DisplayHistory
    {
      get => displayHistory ??= new();
      set => displayHistory = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePerson Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Standard toLapsListByLrol;
    private CsePersonsWorkSet toLaps;
    private Common displayHistory;
    private CsePerson search;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of TotalPrompts.
    /// </summary>
    [JsonPropertyName("totalPrompts")]
    public Common TotalPrompts
    {
      get => totalPrompts ??= new();
      set => totalPrompts = value;
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
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private DateWorkArea null1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common totalPrompts;
    private Common select;
    private Common csePersonPrompt;
    private Common courtOrderPrompt;
    private Common common;
    private NextTranInfo nextTranInfo;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of Data529AccountParticipant.
    /// </summary>
    [JsonPropertyName("data529AccountParticipant")]
    public Data529AccountParticipant Data529AccountParticipant
    {
      get => data529AccountParticipant ??= new();
      set => data529AccountParticipant = value;
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

    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private Data529AccountParticipant data529AccountParticipant;
    private CsePerson csePerson;
  }
#endregion
}

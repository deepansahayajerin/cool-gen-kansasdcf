// Program: LE_LPCC_LST_CSE_PERS_BY_CRT_CASE, ID: 371978770, model: 746.
// Short name: SWELPCCP
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
/// A program: LE_LPCC_LST_CSE_PERS_BY_CRT_CASE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLpccLstCsePersByCrtCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LPCC_LST_CSE_PERS_BY_CRT_CASE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLpccLstCsePersByCrtCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLpccLstCsePersByCrtCase.
  /// </summary>
  public LeLpccLstCsePersByCrtCase(IContext context, Import import,
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
    // ---------------------------------------------------------------------
    // Date		Developer	Request #
    // Description
    // 08/16/95	S. Benton
    // Initial Code
    // 11/19/98	P McElderry	None listed
    // Enhanced READ structure of program and DISPLAY logic.
    // Changed logic w/in population of group views.
    // Coded to meet KESSEP test plan rules.
    // 12/30/98	P McElderry	None listed
    // Per Jan Brigham, highlighted members who have been
    // taken of the case (legal and obligation).
    // 08-12-02  K Doshi         PR149011     Fix screen Help Id.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ------------------------
    // Move Imports to Exports
    // ------------------------
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.ListOptionLrolLops.OneChar = import.ListOptionLrolLops.OneChar;
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    export.Tribunal.Assign(import.Tribunal);
    export.Foreign.Country = import.Foreign.Country;
    export.Fips.Assign(import.Fips);
    export.PreviousLegalAction.CourtCaseNumber =
      import.PreviousLegalAction.CourtCaseNumber;
    export.DataExists.Flag = import.DataExists.Flag;
    MoveFips(import.PreviousFips, export.PreviousFips);

    if (Equal(global.Command, "RLTRIB"))
    {
    }
    else
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

        export.Export1.Update.DetailCsePersonsWorkSet.Assign(
          import.Import1.Item.DetailCsePersonsWorkSet);
        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailPersonPrivateAttorney.Assign(
          import.Import1.Item.DetailPersonPrivateAttorney);
        export.Export1.Next();
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------------------
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // -------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // --------------------------------------------------------------
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          local.NextTranInfo.CsePersonNumber =
            export.Export1.Item.DetailCsePersonsWorkSet.Number;

          break;
        }
      }

      UseScCabNextTranPut();

      return;
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }

    // -------------------------------
    // Validate action level security
    // -------------------------------
    if (Equal(global.Command, "RLTRIB") || Equal(global.Command, "REDISP"))
    {
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

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // --------------------
      // Edit required fields
      // --------------------
      if (IsEmpty(import.LegalAction.CourtCaseNumber))
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }
      else
      {
        // --------------------
        // Continue processing
        // --------------------
      }

      if (export.Tribunal.Identifier == 0)
      {
        if (IsEmpty(export.Fips.CountyAbbreviation) || IsEmpty
          (export.Fips.StateAbbreviation))
        {
          var field1 = GetField(export.Fips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "stateAbbreviation");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
        else
        {
          // --------------------
          // Continue processing
          // --------------------
        }
      }
      else
      {
        // --------------------
        // Continue processing
        // --------------------
      }

      if (!IsEmpty(export.Fips.CountyAbbreviation) && !
        IsEmpty(export.Fips.StateAbbreviation) && !
        IsEmpty(export.Foreign.Country))
      {
        var field1 = GetField(export.Fips, "countyAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        var field3 = GetField(export.Foreign, "country");

        field3.Error = true;

        ExitState = "ACO_NI0000_CLEAR_SCREEN_TO_DISP";
      }
      else
      {
        // --------------------
        // Continue processing
        // --------------------
      }

      if (!Equal(export.LegalAction.CourtCaseNumber,
        export.PreviousLegalAction.CourtCaseNumber))
      {
        export.Foreign.Country = "";
        export.Tribunal.Identifier = 0;
      }
      else
      {
        // --------------------
        // Continue processing
        // --------------------
      }

      if (!Equal(export.Fips.CountyAbbreviation,
        export.PreviousFips.CountyAbbreviation) && !
        Equal(export.Fips.StateAbbreviation,
        export.PreviousFips.StateAbbreviation) && IsEmpty
        (export.Foreign.Country))
      {
        export.Fips.CountyDescription = "";
        export.Tribunal.Identifier = 0;

        if (ReadFips2())
        {
          export.Fips.Assign(entities.Fips);
          MoveFips(entities.Fips, export.PreviousFips);
        }
        else
        {
          var field1 = GetField(export.Fips, "stateAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "countyAbbreviation");

          field2.Error = true;

          ExitState = "INVALID_FIPS_STATE_COUNTY_LOCN";
        }
      }
      else
      {
        // --------------------
        // Continue processing
        // --------------------
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (export.Tribunal.Identifier == 0)
        {
          // --------------------------------------------------------------
          // The extra qualifiers for LEGAL_ACTION are needed because
          // if absent, the 1st TRIBUNAL w/the necessary FIPS info
          // will be brought forth.
          // --------------------------------------------------------------
          if (ReadTribunal())
          {
            export.Tribunal.Assign(entities.Tribunal);

            // --------------------------------------------------------------
            // Have to read on LEGAL_ACTION in order to populate
            // petitioner/respondent information
            // --------------------------------------------------------------
            if (ReadLegalAction2())
            {
              MoveLegalAction(entities.LegalAction, export.LegalAction);
              export.PreviousLegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;
            }
            else
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";
            }
          }
          else if (ReadLegalAction3())
          {
            ExitState = "LE0000_TRIBUNAL_NF";
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LEGAL_ACTION_NF";
          }
        }
        else if (ReadLegalAction1())
        {
          MoveLegalAction(entities.LegalAction, export.LegalAction);

          // -------------------------------------------------------------
          // Ensure that all FIPS fields are populated and applicable for
          // U.S. cases
          // -------------------------------------------------------------
          if (!IsEmpty(export.Fips.StateAbbreviation) && !
            IsEmpty(export.Fips.CountyAbbreviation))
          {
            if (ReadFips1())
            {
              // -------------------
              // Continue processing
              // -------------------
            }
            else if (ReadFips3())
            {
              ExitState = "LE0000_TRIBUNAL_NF";
            }
            else
            {
              var field1 = GetField(export.Fips, "stateAbbreviation");

              field1.Error = true;

              var field2 = GetField(export.Fips, "countyAbbreviation");

              field2.Error = true;

              ExitState = "INVALID_FIPS_STATE_COUNTY_LOCN";
            }
          }
          else if (ReadFipsTribAddress())
          {
            // -------------------
            // Continue processing
            // -------------------
          }
          else
          {
            var field = GetField(export.Foreign, "country");

            field.Error = true;

            ExitState = "FN0000_NO_FIPS_ADDRESS_FOUND";
          }
        }
        else
        {
          ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;
        }
      }
      else
      {
        // -------------------
        // Continue processing
        // -------------------
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(export.DataExists.Flag) == 'Y')
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

            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailCsePersonsWorkSet.Number = "";
            export.Export1.Update.DetailCsePersonsWorkSet.FormattedName = "";
            export.Export1.Update.DetailPersonPrivateAttorney.LastName = "";
            export.Export1.Update.DetailPersonPrivateAttorney.FirstName = "";
            export.DataExists.Flag = "";
            export.Export1.Next();
          }
        }
        else
        {
          // --------------------
          // No data to blank out
          // --------------------
        }

        return;
      }
      else
      {
        // --------------------
        // Continue processing
        // --------------------
      }

      UseLeGetPetitionerRespondent();

      // --------------------
      // end 11/19/98 changes
      // --------------------
    }

    switch(AsChar(export.PromptTribunal.SelectChar))
    {
      case 'S':
        if (Equal(global.Command, "LIST") || Equal(global.Command, "RLTRIB"))
        {
          break;
        }

        var field1 = GetField(export.PromptTribunal, "selectChar");

        field1.Error = true;

        ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

        return;
      case ' ':
        if (Equal(global.Command, "LIST") || Equal(global.Command, "RLTRIB"))
        {
          var field = GetField(export.PromptTribunal, "selectChar");

          field.Error = true;

          ExitState = "ZD_ACO_NE00_MUST_SELECT_4_PROMPT";
        }

        break;
      default:
        var field2 = GetField(export.PromptTribunal, "selectChar");

        field2.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
    }

    // ----------------------
    // Begin 11/19/98 changes
    // ----------------------
    local.TotalSelected.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
      {
        case ' ':
          if (Equal(global.Command, "LIST") && IsEmpty
            (export.PromptTribunal.SelectChar))
          {
            var field1 =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field1.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          break;
        case 'S':
          ++local.TotalSelected.Count;
          export.SelectedPersonPrivateAttorney.Assign(
            export.Export1.Item.DetailPersonPrivateAttorney);
          export.SelectedCsePersonsWorkSet.Assign(
            export.Export1.Item.DetailCsePersonsWorkSet);

          if (local.TotalSelected.Count > 1)
          {
            var field1 =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field1.Error = true;

            ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN1";
          }
          else
          {
            // ---------------------
            // Continue processing
            // ---------------------
          }

          if (Equal(global.Command, "LIST"))
          {
            var field1 =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field1.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";
          }

          break;
        default:
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }
    }

    // --------------------
    // end 11/19/98 changes
    // --------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }
    else
    {
      // ---------------------
      // Continue processing
      // ---------------------
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          export.Fips.CountyDescription = "";
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
        }

        break;
      case "RLTRIB":
        // -------------------
        // Perform edit checks
        // -------------------
        if (!Equal(export.Fips.CountyAbbreviation,
          import.DlgflwSelected.CountyAbbreviation))
        {
          export.Fips.CountyAbbreviation = "";

          if (!Equal(export.Fips.StateAbbreviation,
            import.DlgflwSelected.StateAbbreviation))
          {
            export.Fips.StateAbbreviation = "";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        // ----------------------------------------------------------
        // Returned from List Tribunals screen. Move values to export.
        // ----------------------------------------------------------
        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          export.PromptTribunal.SelectChar = "";
          export.PreviousLegalAction.CourtCaseNumber =
            export.LegalAction.CourtCaseNumber ?? "";

          if (import.DlgflwSelected.State > 0)
          {
            var field = GetField(export.ListOptionLrolLops, "oneChar");

            field.Protected = false;
            field.Focused = true;

            export.Fips.Assign(import.DlgflwSelected);
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        break;
      case "ATTY":
        // *************************************************************
        // If PF15 ATTY is pressed, link to Person Private Attorney
        // screen if an occurrence has been selected. Otherwise
        // display the screen.
        // *************************************************************
        // ----------------------
        // Begin 11/19/98 changes
        // ----------------------
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              if (export.Export1.Item.DetailPersonPrivateAttorney.Identifier ==
                0)
              {
                ExitState = "NO_PRIVATE_ATTORNEY_FOR_PERSON";
              }

              export.CsePerson.Number = export.SelectedCsePersonsWorkSet.Number;
              ExitState = "ECO_LNK_TO_PERS_PRIVATE_ATTORNEY";

              return;
            case ' ':
              ExitState = "OE0000_NO_RECORD_SELECTED";

              break;
            default:
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";

              break;
          }

          // --------------------
          // end 11/19/98 changes
          // --------------------
        }

        break;
      case "DISPLAY":
        if (IsEmpty(export.ListOptionLrolLops.OneChar))
        {
          export.ListOptionLrolLops.OneChar = "O";
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        if (AsChar(export.ListOptionLrolLops.OneChar) != 'L' && AsChar
          (export.ListOptionLrolLops.OneChar) != 'O')
        {
          ExitState = "LE0000_INVALID_LIST_BY_LROL_LOPS";

          var field = GetField(export.ListOptionLrolLops, "oneChar");

          field.Error = true;

          return;
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        UseLeListCsePersonByCcNoTrib();

        if (export.Export1.IsFull)
        {
          ExitState = "OE0000_LIST_IS_FULL";
          export.DataExists.Flag = "Y";
        }
        else if (export.Export1.IsEmpty)
        {
          export.DataExists.Flag = "N";
          ExitState = "LE0000_NO_LPCC_PERSON_TO_DISPLAY";
        }
        else
        {
          export.DataExists.Flag = "Y";
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        if (AsChar(export.DataExists.Flag) == 'Y')
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                

              field1.Color = "red";
              field1.Protected = false;

              var field2 =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                "formattedName");

              field2.Color = "red";
              field2.Protected = false;

              var field3 =
                GetField(export.Export1.Item.DetailPersonPrivateAttorney,
                "lastName");

              field3.Color = "red";
              field3.Protected = false;

              var field4 =
                GetField(export.Export1.Item.DetailPersonPrivateAttorney,
                "firstName");

              field4.Color = "red";
              field4.Protected = false;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }
          }
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        break;
      case "NEXT":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");

            field1.Color = "red";
            field1.Protected = false;

            var field2 =
              GetField(export.Export1.Item.DetailCsePersonsWorkSet,
              "formattedName");

            field2.Color = "red";
            field2.Protected = false;

            var field3 =
              GetField(export.Export1.Item.DetailPersonPrivateAttorney,
              "lastName");

            field3.Color = "red";
            field3.Protected = false;

            var field4 =
              GetField(export.Export1.Item.DetailPersonPrivateAttorney,
              "firstName");

            field4.Color = "red";
            field4.Protected = false;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }

        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");

            field1.Color = "red";
            field1.Protected = false;

            var field2 =
              GetField(export.Export1.Item.DetailCsePersonsWorkSet,
              "formattedName");

            field2.Color = "red";
            field2.Protected = false;

            var field3 =
              GetField(export.Export1.Item.DetailPersonPrivateAttorney,
              "lastName");

            field3.Color = "red";
            field3.Protected = false;

            var field4 =
              GetField(export.Export1.Item.DetailPersonPrivateAttorney,
              "firstName");

            field4.Color = "red";
            field4.Protected = false;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }

        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        // ------------------------------------------------------
        // Move selected view to a single view that will be mapped
        // back to calling screen view.
        // -------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "REDISP":
        if (IsEmpty(export.ListOptionLrolLops.OneChar))
        {
          export.ListOptionLrolLops.OneChar = "O";
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        if (AsChar(export.ListOptionLrolLops.OneChar) != 'L' && AsChar
          (export.ListOptionLrolLops.OneChar) != 'O')
        {
          ExitState = "LE0000_INVALID_LIST_BY_LROL_LOPS";

          var field = GetField(export.ListOptionLrolLops, "oneChar");

          field.Error = true;

          return;
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        UseLeListCsePersonByCcNoTrib();

        if (AsChar(export.DataExists.Flag) == 'Y')
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                

              field1.Color = "red";
              field1.Protected = false;

              var field2 =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                "formattedName");

              field2.Color = "red";
              field2.Protected = false;

              var field3 =
                GetField(export.Export1.Item.DetailPersonPrivateAttorney,
                "lastName");

              field3.Color = "red";
              field3.Protected = false;

              var field4 =
                GetField(export.Export1.Item.DetailPersonPrivateAttorney,
                "firstName");

              field4.Color = "red";
              field4.Protected = false;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }
          }
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveExport1(LeListCsePersonByCcNoTrib.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.Common.SelectChar;
    target.DetailCsePersonsWorkSet.Assign(source.CsePersonsWorkSet);
    MovePersonPrivateAttorney(source.PersonPrivateAttorney,
      target.DetailPersonPrivateAttorney);
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MovePersonPrivateAttorney(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.Identifier = source.Identifier;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeListCsePersonByCcNoTrib()
  {
    var useImport = new LeListCsePersonByCcNoTrib.Import();
    var useExport = new LeListCsePersonByCcNoTrib.Export();

    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    useImport.ListByLrolOrLops.OneChar = export.ListOptionLrolLops.OneChar;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(LeListCsePersonByCcNoTrib.Execute, useImport, useExport);

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

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.StateAbbreviation = db.GetString(reader, 5);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 6);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.StateAbbreviation = db.GetString(reader, 5);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 6);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.StateAbbreviation = db.GetString(reader, 5);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 6);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
        db.SetNullableString(command, "country", import.Foreign.Country ?? "");
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailPersonPrivateAttorney.
      /// </summary>
      [JsonPropertyName("detailPersonPrivateAttorney")]
      public PersonPrivateAttorney DetailPersonPrivateAttorney
      {
        get => detailPersonPrivateAttorney ??= new();
        set => detailPersonPrivateAttorney = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailCommon;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private PersonPrivateAttorney detailPersonPrivateAttorney;
    }

    /// <summary>
    /// A value of PreviousFips.
    /// </summary>
    [JsonPropertyName("previousFips")]
    public Fips PreviousFips
    {
      get => previousFips ??= new();
      set => previousFips = value;
    }

    /// <summary>
    /// A value of DataExists.
    /// </summary>
    [JsonPropertyName("dataExists")]
    public Common DataExists
    {
      get => dataExists ??= new();
      set => dataExists = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
    }

    /// <summary>
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of ListOptionLrolLops.
    /// </summary>
    [JsonPropertyName("listOptionLrolLops")]
    public Standard ListOptionLrolLops
    {
      get => listOptionLrolLops ??= new();
      set => listOptionLrolLops = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
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
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public Fips DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    private Fips previousFips;
    private Common dataExists;
    private LegalAction previousLegalAction;
    private FipsTribAddress foreign;
    private Common promptTribunal;
    private Fips fips;
    private Tribunal tribunal;
    private Standard listOptionLrolLops;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private LegalAction legalAction;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Fips dlgflwSelected;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailPersonPrivateAttorney.
      /// </summary>
      [JsonPropertyName("detailPersonPrivateAttorney")]
      public PersonPrivateAttorney DetailPersonPrivateAttorney
      {
        get => detailPersonPrivateAttorney ??= new();
        set => detailPersonPrivateAttorney = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailCommon;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private PersonPrivateAttorney detailPersonPrivateAttorney;
    }

    /// <summary>
    /// A value of PreviousFips.
    /// </summary>
    [JsonPropertyName("previousFips")]
    public Fips PreviousFips
    {
      get => previousFips ??= new();
      set => previousFips = value;
    }

    /// <summary>
    /// A value of DataExists.
    /// </summary>
    [JsonPropertyName("dataExists")]
    public Common DataExists
    {
      get => dataExists ??= new();
      set => dataExists = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
    }

    /// <summary>
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of ListOptionLrolLops.
    /// </summary>
    [JsonPropertyName("listOptionLrolLops")]
    public Standard ListOptionLrolLops
    {
      get => listOptionLrolLops ??= new();
      set => listOptionLrolLops = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("selectedPersonPrivateAttorney")]
    public PersonPrivateAttorney SelectedPersonPrivateAttorney
    {
      get => selectedPersonPrivateAttorney ??= new();
      set => selectedPersonPrivateAttorney = value;
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
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
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
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
    }

    private Fips previousFips;
    private Common dataExists;
    private LegalAction previousLegalAction;
    private FipsTribAddress foreign;
    private Common promptTribunal;
    private Fips fips;
    private Tribunal tribunal;
    private Standard listOptionLrolLops;
    private CsePerson csePerson;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private PersonPrivateAttorney selectedPersonPrivateAttorney;
    private LegalAction legalAction;
    private Array<ExportGroup> export1;
    private Standard standard;
    private Common displayActiveCasesOnly;
    private Code code;
    private PetitionerRespondentDetails petitionerRespondentDetails;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotalSelected.
    /// </summary>
    [JsonPropertyName("totalSelected")]
    public Common TotalSelected
    {
      get => totalSelected ??= new();
      set => totalSelected = value;
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

    private Common totalSelected;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private FipsTribAddress fipsTribAddress;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
  }
#endregion
}

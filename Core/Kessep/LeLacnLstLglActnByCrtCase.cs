// Program: LE_LACN_LST_LGL_ACTN_BY_CRT_CASE, ID: 372002453, model: 746.
// Short name: SWELACNP
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
/// A program: LE_LACN_LST_LGL_ACTN_BY_CRT_CASE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLacnLstLglActnByCrtCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LACN_LST_LGL_ACTN_BY_CRT_CASE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLacnLstLglActnByCrtCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLacnLstLglActnByCrtCase.
  /// </summary>
  public LeLacnLstLglActnByCrtCase(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 05/09/95  Dave Allen			Initial Code
    // 01/10/99  P. Sharp  			Changes based on the Phase II assessmant sheets.
    // 11/20/01  GVandy	PR132619	Pass selected legal action id on nextran.
    // 12/23/02  GVandy	WR10492		Display new attribute legal_action 
    // system_gen_ind.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.SearchLegalAction.Assign(import.SearchLegalAction);
    export.SearchFips.Assign(import.SearchFips);
    export.SearchTribunal.Assign(import.SearchTribunal);
    export.PromptClassificaton.SelectChar =
      import.PromptClassification.SelectChar;
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.SearchFips.Assign(import.SearchFips);
    export.SearchTribunal.Assign(import.SearchTribunal);
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;

    export.LegalAction.Index = 0;
    export.LegalAction.Clear();

    for(import.LegalAction.Index = 0; import.LegalAction.Index < import
      .LegalAction.Count; ++import.LegalAction.Index)
    {
      if (export.LegalAction.IsFull)
      {
        break;
      }

      export.LegalAction.Update.LappInd.Flag =
        import.LegalAction.Item.LappInd.Flag;
      export.LegalAction.Update.Common.SelectChar =
        import.LegalAction.Item.Common.SelectChar;
      export.LegalAction.Update.LegalAction1.Assign(
        import.LegalAction.Item.LegalAction1);
      export.LegalAction.Update.ServiceProcess.ServiceDate =
        import.LegalAction.Item.ServiceProcess.ServiceDate;
      export.LegalAction.Update.Classification.Text11 =
        import.LegalAction.Item.Classification.Text11;
      export.LegalAction.Update.LaActionTaken.Description =
        import.LegalAction.Item.LaActionTaken.Description;
      export.LegalAction.Next();
    }

    if (!Equal(global.Command, "RLCVAL") && !
      Equal(global.Command, "RLTRIB") && !Equal(global.Command, "LIST"))
    {
      export.PromptClassificaton.SelectChar = "";
      export.PromptTribunal.SelectChar = "";
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

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.SearchLegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";
      export.SearchLegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();

      if (export.SearchLegalAction.Identifier == 0)
      {
        return;
      }

      if (ReadLegalAction())
      {
        MoveLegalAction1(entities.LegalAction1, export.SearchLegalAction);
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";
        export.SearchLegalAction.Identifier = 0;

        return;
      }

      if (ReadTribunal())
      {
        export.SearchTribunal.Assign(entities.Tribunal);
      }
      else
      {
        ExitState = "TRIBUNAL_NF";
        export.SearchLegalAction.Identifier = 0;

        return;
      }

      if (ReadFips3())
      {
        MoveFips2(entities.Fips, export.SearchFips);
      }
      else if (ReadFipsTribAddress())
      {
        export.FipsTribAddress.Country = entities.FipsTribAddress.Country;
      }
      else
      {
        return;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.TotalSelected.Count = 0;

      for(export.LegalAction.Index = 0; export.LegalAction.Index < export
        .LegalAction.Count; ++export.LegalAction.Index)
      {
        if (IsEmpty(export.LegalAction.Item.Common.SelectChar))
        {
        }
        else
        {
          ++local.TotalSelected.Count;

          if (local.TotalSelected.Count > 1)
          {
            break;
          }

          if (AsChar(export.LegalAction.Item.Common.SelectChar) == 'S')
          {
            local.NextTranInfo.CourtCaseNumber =
              export.SearchLegalAction.CourtCaseNumber ?? "";
            local.NextTranInfo.LegalActionIdentifier =
              export.LegalAction.Item.LegalAction1.Identifier;
          }
          else
          {
            var field = GetField(export.LegalAction.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }
      }

      if (local.TotalSelected.Count > 1)
      {
        for(export.LegalAction.Index = 0; export.LegalAction.Index < export
          .LegalAction.Count; ++export.LegalAction.Index)
        {
          if (IsEmpty(export.LegalAction.Item.Common.SelectChar))
          {
          }
          else
          {
            var field = GetField(export.LegalAction.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseScCabNextTranPut();

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

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // ---------------------------------------------
    //             E D I T    L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // -- blank out the export group.
      export.LegalAction.Index = 0;
      export.LegalAction.Clear();

      for(local.Null1.Index = 0; local.Null1.Index < local.Null1.Count; ++
        local.Null1.Index)
      {
        if (export.LegalAction.IsFull)
        {
          break;
        }

        export.LegalAction.Next();
      }

      if (IsEmpty(export.SearchLegalAction.CourtCaseNumber) && IsEmpty
        (export.SearchLegalAction.StandardNumber))
      {
        var field1 = GetField(export.SearchLegalAction, "courtCaseNumber");

        field1.Error = true;

        var field2 = GetField(export.SearchLegalAction, "standardNumber");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (!IsEmpty(export.SearchLegalAction.CourtCaseNumber))
      {
        // ---------------------------------------------
        // Validate Tribunal.
        // ---------------------------------------------
        if ((IsEmpty(export.SearchFips.StateAbbreviation) || IsEmpty
          (export.SearchFips.CountyAbbreviation)) && IsEmpty
          (export.FipsTribAddress.Country) && export
          .SearchTribunal.Identifier > 0)
        {
          export.SearchFips.CountyDescription = "";

          var field1 = GetField(export.SearchFips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.SearchFips, "stateAbbreviation");

          field2.Error = true;

          export.SearchTribunal.Assign(local.InitTribunal);
          export.FipsTribAddress.Country = local.InitFipsTribAddress.Country;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_STATE_CNTRY_AND_CNTY_REQ";
          }
        }

        if (IsEmpty(export.SearchFips.StateAbbreviation))
        {
          if (export.SearchTribunal.Identifier == 0)
          {
            var field1 = GetField(export.SearchFips, "stateAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.FipsTribAddress, "country");

            field2.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "COUNTRY_OR_STATE_CODE_REQD";
            }
          }
        }
        else
        {
          if (IsEmpty(export.SearchFips.CountyAbbreviation))
          {
            var field = GetField(export.SearchFips, "countyAbbreviation");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_TRIB_COUNTY_REQUIRED";
            }
          }

          if (ReadFips1())
          {
            MoveFips2(entities.Fips, export.SearchFips);
          }
          else if (ReadFips2())
          {
            var field1 = GetField(export.SearchFips, "countyAbbreviation");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.SearchFips, "countyAbbreviation");

              field.Error = true;

              ExitState = "INVALID_COUNTY_CODE";
            }

            var field2 = GetField(export.SearchFips, "countyAbbreviation");

            field2.Error = true;
          }
          else
          {
            var field = GetField(export.SearchFips, "stateAbbreviation");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }
      }

      // ---------------------------------------------
      // Validate Classification.
      // ---------------------------------------------
      if (!IsEmpty(import.SearchLegalAction.Classification))
      {
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = import.SearchLegalAction.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.SearchLegalAction, "classification");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    local.TotalSelected.Count = 0;

    for(export.LegalAction.Index = 0; export.LegalAction.Index < export
      .LegalAction.Count; ++export.LegalAction.Index)
    {
      if (IsEmpty(export.LegalAction.Item.Common.SelectChar))
      {
      }
      else
      {
        ++local.TotalSelected.Count;

        if (local.TotalSelected.Count > 1)
        {
          break;
        }

        if (AsChar(export.LegalAction.Item.Common.SelectChar) == 'S')
        {
          local.DlgflwSelected.Assign(export.LegalAction.Item.LegalAction1);
        }
        else
        {
          var field = GetField(export.LegalAction.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }
      }
    }

    if (local.TotalSelected.Count > 1)
    {
      for(export.LegalAction.Index = 0; export.LegalAction.Index < export
        .LegalAction.Count; ++export.LegalAction.Index)
      {
        if (IsEmpty(export.LegalAction.Item.Common.SelectChar))
        {
        }
        else
        {
          var field = GetField(export.LegalAction.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "LACT":
        // ---------------------------------------------
        // If PF15 LACN is pressed, link to "Legal
        // Action" screen if an occurrence has been
        // selected. Otherwise display the screen.
        // ---------------------------------------------
        if (local.TotalSelected.Count == 0)
        {
          ExitState = "LE0000_LACT_MUST_BE_SELECTED";

          for(export.LegalAction.Index = 0; export.LegalAction.Index < export
            .LegalAction.Count; ++export.LegalAction.Index)
          {
            var field1 = GetField(export.LegalAction.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
        }
        else
        {
          export.DlgflwSelected.Assign(local.DlgflwSelected);
          ExitState = "ECO_LNK_TO_LEGAL_ACTION";

          return;
        }

        break;
      case "SERV":
        if (local.TotalSelected.Count == 0)
        {
          ExitState = "LE0000_LACT_MUST_BE_SELECTED";

          for(export.LegalAction.Index = 0; export.LegalAction.Index < export
            .LegalAction.Count; ++export.LegalAction.Index)
          {
            var field1 = GetField(export.LegalAction.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
        }
        else
        {
          export.DlgflwSelected.Assign(local.DlgflwSelected);
          ExitState = "ECO_LNK_TO_SERV";

          return;
        }

        break;
      case "LRES":
        if (local.TotalSelected.Count == 0)
        {
          ExitState = "LE0000_LACT_MUST_BE_SELECTED";

          for(export.LegalAction.Index = 0; export.LegalAction.Index < export
            .LegalAction.Count; ++export.LegalAction.Index)
          {
            var field1 = GetField(export.LegalAction.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
        }
        else
        {
          export.DlgflwSelected.Assign(local.DlgflwSelected);
          ExitState = "ECO_LNK_TO_LRES";

          return;
        }

        break;
      case "HEAR":
        if (local.TotalSelected.Count == 0)
        {
          ExitState = "LE0000_LACT_MUST_BE_SELECTED";

          for(export.LegalAction.Index = 0; export.LegalAction.Index < export
            .LegalAction.Count; ++export.LegalAction.Index)
          {
            var field1 = GetField(export.LegalAction.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
        }
        else
        {
          export.DlgflwSelected.Assign(local.DlgflwSelected);
          ExitState = "ECO_LNK_TO_HEAR";

          return;
        }

        break;
      case "IWGL":
        if (local.TotalSelected.Count == 0)
        {
          ExitState = "LE0000_LACT_MUST_BE_SELECTED";

          for(export.LegalAction.Index = 0; export.LegalAction.Index < export
            .LegalAction.Count; ++export.LegalAction.Index)
          {
            var field1 = GetField(export.LegalAction.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
        }
        else
        {
          export.DlgflwSelected.Assign(local.DlgflwSelected);

          if (AsChar(export.DlgflwSelected.Classification) == 'I' || AsChar
            (export.DlgflwSelected.Classification) == 'G')
          {
            if (AsChar(export.DlgflwSelected.Classification) == 'I')
            {
              export.DlgflwIwglType.Text1 = "I";
            }

            if (AsChar(export.DlgflwSelected.Classification) == 'G')
            {
              export.DlgflwIwglType.Text1 = "G";
            }

            ExitState = "ECO_LNK_TO_IWGL";

            return;
          }
          else
          {
            ExitState = "LE0000_I_OR_G_CLASS_FOR_IWGL";
          }
        }

        break;
      case "LIST":
        if (!IsEmpty(export.PromptClassificaton.SelectChar) && AsChar
          (export.PromptClassificaton.SelectChar) != 'S')
        {
          var field1 = GetField(export.PromptClassificaton, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsEmpty(export.PromptTribunal.SelectChar) && AsChar
          (export.PromptTribunal.SelectChar) != 'S')
        {
          var field1 = GetField(export.PromptTribunal, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (AsChar(export.PromptClassificaton.SelectChar) == 'S' && AsChar
          (export.PromptTribunal.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptClassificaton, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptTribunal, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        if (IsEmpty(export.PromptClassificaton.SelectChar) && IsEmpty
          (export.PromptTribunal.SelectChar))
        {
          var field1 = GetField(export.PromptClassificaton, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptTribunal, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.PromptClassificaton.SelectChar) == 'S')
        {
          export.DisplayActiveCasesOnly.Flag = "Y";
          export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        if (AsChar(export.PromptTribunal.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
        }

        return;
      case "RLCVAL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // ---------------------------------------------
        if (!IsEmpty(import.PromptClassification.SelectChar))
        {
          export.PromptClassificaton.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.SearchLegalAction.Classification =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field1 = GetField(export.SearchFips, "stateAbbreviation");

            field1.Protected = false;
            field1.Focused = true;

            export.PromptClassificaton.SelectChar = "";
          }
        }

        break;
      case "RLTRIB":
        // ---------------------------------------------
        // Returned from List Tribunals screen. Move
        // values to export.
        // ---------------------------------------------
        export.SearchFips.Assign(import.DlgflwSelectedFips);
        export.SearchTribunal.Assign(import.DlgflwSelectedTribunal);

        var field = GetField(export.SearchFips, "stateAbbreviation");

        field.Protected = false;
        field.Focused = true;

        export.PromptTribunal.SelectChar = "";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "RETURN":
        // ---------------------------------------------
        // Don't allow more than one occurrence to be
        // selected.
        // ---------------------------------------------
        local.TotalSelected.Count = 0;

        for(export.LegalAction.Index = 0; export.LegalAction.Index < export
          .LegalAction.Count; ++export.LegalAction.Index)
        {
          if (!IsEmpty(export.LegalAction.Item.Common.SelectChar))
          {
            ++local.TotalSelected.Count;
          }
        }

        if (local.TotalSelected.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          for(export.LegalAction.Index = 0; export.LegalAction.Index < export
            .LegalAction.Count; ++export.LegalAction.Index)
          {
            if (!IsEmpty(export.LegalAction.Item.Common.SelectChar))
            {
              var field1 =
                GetField(export.LegalAction.Item.Common, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        // ---------------------------------------------
        // Move selected view to a single view that will
        // be mapped back to calling screen view.
        // ---------------------------------------------
        for(export.LegalAction.Index = 0; export.LegalAction.Index < export
          .LegalAction.Count; ++export.LegalAction.Index)
        {
          switch(AsChar(export.LegalAction.Item.Common.SelectChar))
          {
            case 'S':
              export.DlgflwSelected.
                Assign(export.LegalAction.Item.LegalAction1);

              break;
            case ' ':
              break;
            default:
              var field1 =
                GetField(export.LegalAction.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "ENTER":
        // ---------------------------------------------
        // The ENTER key will not be used for functionality
        // here. If it is pressed, an exit state message
        // should be output.
        // ---------------------------------------------
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------
      // If a display is required, call the action
      // block that reads the next group of data based
      // on the starting key value.
      // If the Tribunal have been specified, use the
      // cab that retrieves the Legal Actions that are
      // within the specified Tribunal. 
      // ---------------------------------------------
      UseLeListLegalActsByCcNoTrib();

      for(export.LegalAction.Index = 0; export.LegalAction.Index < export
        .LegalAction.Count; ++export.LegalAction.Index)
      {
        export.SearchLegalAction.Identifier =
          export.LegalAction.Item.LegalAction1.Identifier;
        export.SearchLegalAction.StandardNumber =
          export.LegalAction.Item.LegalAction1.StandardNumber ?? "";
        export.SearchLegalAction.ForeignOrderNumber =
          export.LegalAction.Item.LegalAction1.ForeignOrderNumber ?? "";

        break;
      }

      if (export.SearchLegalAction.Identifier != 0)
      {
        UseLeGetPetitionerRespondent();
      }

      if (export.LegalAction.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else if (export.LegalAction.IsFull)
      {
        ExitState = "OE0000_LIST_IS_FULL";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }
    else
    {
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // If Filed Date is a max date (12312099), zero
    // it out so it doesn't display on the screen.
    // ---------------------------------------------
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    for(export.LegalAction.Index = 0; export.LegalAction.Index < export
      .LegalAction.Count; ++export.LegalAction.Index)
    {
      if (Equal(export.LegalAction.Item.LegalAction1.FiledDate,
        local.MaxDate.Date))
      {
        export.LegalAction.Update.LegalAction1.FiledDate = null;
      }

      if (Equal(export.LegalAction.Item.ServiceProcess.ServiceDate,
        local.MaxDate.Date))
      {
        export.LegalAction.Update.ServiceProcess.ServiceDate = null;
      }
    }
  }

  private static void MoveFips1(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveFips2(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
    target.CountyDescription = source.CountyDescription;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
  }

  private static void MoveLegalAction2(LeListLegalActsByCcNoTrib.Export.
    LegalActionGroup source, Export.LegalActionGroup target)
  {
    target.LaActionTaken.Description = source.LaActionTaken.Description;
    target.LappInd.Flag = source.LappInd.Flag;
    target.Common.SelectChar = source.Common.SelectChar;
    target.Classification.Text11 = source.Classification.Text11;
    target.LegalAction1.Assign(source.LegalAction1);
    target.ServiceProcess.ServiceDate = source.ServiceProcess.ServiceDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.SearchLegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeListLegalActsByCcNoTrib()
  {
    var useImport = new LeListLegalActsByCcNoTrib.Import();
    var useExport = new LeListLegalActsByCcNoTrib.Export();

    MoveFips1(export.SearchFips, useImport.SearchFips);
    useImport.SearchTribunal.Identifier = export.SearchTribunal.Identifier;
    useImport.SearchLegalAction.Assign(export.SearchLegalAction);

    Call(LeListLegalActsByCcNoTrib.Execute, useImport, useExport);

    export.SearchLegalAction.Assign(useExport.SearchLegalAction);
    useExport.LegalAction.CopyTo(export.LegalAction, MoveLegalAction2);
    export.SearchTribunal.Assign(useExport.SearchTribunal);
    MoveFips2(useExport.SearchFips, export.SearchFips);
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
        db.SetString(
          command, "stateAbbreviation", export.SearchFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", export.SearchFips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.SearchFips.StateAbbreviation);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction1.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", export.SearchLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction1.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction1.Classification = db.GetString(reader, 1);
        entities.LegalAction1.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction1.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction1.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction1.TrbId.GetValueOrDefault());
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
    /// <summary>A LegalActionGroup group.</summary>
    [Serializable]
    public class LegalActionGroup
    {
      /// <summary>
      /// A value of LaActionTaken.
      /// </summary>
      [JsonPropertyName("laActionTaken")]
      public CodeValue LaActionTaken
      {
        get => laActionTaken ??= new();
        set => laActionTaken = value;
      }

      /// <summary>
      /// A value of LappInd.
      /// </summary>
      [JsonPropertyName("lappInd")]
      public Common LappInd
      {
        get => lappInd ??= new();
        set => lappInd = value;
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
      /// A value of Classification.
      /// </summary>
      [JsonPropertyName("classification")]
      public WorkArea Classification
      {
        get => classification ??= new();
        set => classification = value;
      }

      /// <summary>
      /// A value of LegalAction1.
      /// </summary>
      [JsonPropertyName("legalAction1")]
      public LegalAction LegalAction1
      {
        get => legalAction1 ??= new();
        set => legalAction1 = value;
      }

      /// <summary>
      /// A value of ServiceProcess.
      /// </summary>
      [JsonPropertyName("serviceProcess")]
      public ServiceProcess ServiceProcess
      {
        get => serviceProcess ??= new();
        set => serviceProcess = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CodeValue laActionTaken;
      private Common lappInd;
      private Common common;
      private WorkArea classification;
      private LegalAction legalAction1;
      private ServiceProcess serviceProcess;
    }

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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
    }

    /// <summary>
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of PromptClassification.
    /// </summary>
    [JsonPropertyName("promptClassification")]
    public Common PromptClassification
    {
      get => promptClassification ??= new();
      set => promptClassification = value;
    }

    /// <summary>
    /// Gets a value of LegalAction.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionGroup> LegalAction => legalAction ??= new(
      LegalActionGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalAction for json serialization.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Computed]
    public IList<LegalActionGroup> LegalAction_Json
    {
      get => legalAction;
      set => LegalAction.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwSelectedCodeValue.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCodeValue")]
    public CodeValue DlgflwSelectedCodeValue
    {
      get => dlgflwSelectedCodeValue ??= new();
      set => dlgflwSelectedCodeValue = value;
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedFips.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedFips")]
    public Fips DlgflwSelectedFips
    {
      get => dlgflwSelectedFips ??= new();
      set => dlgflwSelectedFips = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedTribunal.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedTribunal")]
    public Tribunal DlgflwSelectedTribunal
    {
      get => dlgflwSelectedTribunal ??= new();
      set => dlgflwSelectedTribunal = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Common promptTribunal;
    private Tribunal searchTribunal;
    private Fips searchFips;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private LegalAction searchLegalAction;
    private Common promptClassification;
    private Array<LegalActionGroup> legalAction;
    private CodeValue dlgflwSelectedCodeValue;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
    private Fips dlgflwSelectedFips;
    private Tribunal dlgflwSelectedTribunal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LegalActionGroup group.</summary>
    [Serializable]
    public class LegalActionGroup
    {
      /// <summary>
      /// A value of LaActionTaken.
      /// </summary>
      [JsonPropertyName("laActionTaken")]
      public CodeValue LaActionTaken
      {
        get => laActionTaken ??= new();
        set => laActionTaken = value;
      }

      /// <summary>
      /// A value of LappInd.
      /// </summary>
      [JsonPropertyName("lappInd")]
      public Common LappInd
      {
        get => lappInd ??= new();
        set => lappInd = value;
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
      /// A value of Classification.
      /// </summary>
      [JsonPropertyName("classification")]
      public WorkArea Classification
      {
        get => classification ??= new();
        set => classification = value;
      }

      /// <summary>
      /// A value of LegalAction1.
      /// </summary>
      [JsonPropertyName("legalAction1")]
      public LegalAction LegalAction1
      {
        get => legalAction1 ??= new();
        set => legalAction1 = value;
      }

      /// <summary>
      /// A value of ServiceProcess.
      /// </summary>
      [JsonPropertyName("serviceProcess")]
      public ServiceProcess ServiceProcess
      {
        get => serviceProcess ??= new();
        set => serviceProcess = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CodeValue laActionTaken;
      private Common lappInd;
      private Common common;
      private WorkArea classification;
      private LegalAction legalAction1;
      private ServiceProcess serviceProcess;
    }

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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
    }

    /// <summary>
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
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
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    /// <summary>
    /// A value of PromptClassificaton.
    /// </summary>
    [JsonPropertyName("promptClassificaton")]
    public Common PromptClassificaton
    {
      get => promptClassificaton ??= new();
      set => promptClassificaton = value;
    }

    /// <summary>
    /// Gets a value of LegalAction.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionGroup> LegalAction => legalAction ??= new(
      LegalActionGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalAction for json serialization.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Computed]
    public IList<LegalActionGroup> LegalAction_Json
    {
      get => legalAction;
      set => LegalAction.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public LegalAction DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    /// <summary>
    /// A value of DlgflwIwglType.
    /// </summary>
    [JsonPropertyName("dlgflwIwglType")]
    public WorkArea DlgflwIwglType
    {
      get => dlgflwIwglType ??= new();
      set => dlgflwIwglType = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Common promptTribunal;
    private Tribunal searchTribunal;
    private Fips searchFips;
    private Standard standard;
    private LegalAction searchLegalAction;
    private Common promptClassificaton;
    private Array<LegalActionGroup> legalAction;
    private LegalAction dlgflwSelected;
    private Common displayActiveCasesOnly;
    private Code code;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
    private WorkArea dlgflwIwglType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A NullGroup group.</summary>
    [Serializable]
    public class NullGroup
    {
      /// <summary>
      /// A value of LaActionTaken.
      /// </summary>
      [JsonPropertyName("laActionTaken")]
      public CodeValue LaActionTaken
      {
        get => laActionTaken ??= new();
        set => laActionTaken = value;
      }

      /// <summary>
      /// A value of LappInd.
      /// </summary>
      [JsonPropertyName("lappInd")]
      public Common LappInd
      {
        get => lappInd ??= new();
        set => lappInd = value;
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
      /// A value of Classification.
      /// </summary>
      [JsonPropertyName("classification")]
      public WorkArea Classification
      {
        get => classification ??= new();
        set => classification = value;
      }

      /// <summary>
      /// A value of LegalAction1.
      /// </summary>
      [JsonPropertyName("legalAction1")]
      public LegalAction LegalAction1
      {
        get => legalAction1 ??= new();
        set => legalAction1 = value;
      }

      /// <summary>
      /// A value of ServiceProcess.
      /// </summary>
      [JsonPropertyName("serviceProcess")]
      public ServiceProcess ServiceProcess
      {
        get => serviceProcess ??= new();
        set => serviceProcess = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2;

      private CodeValue laActionTaken;
      private Common lappInd;
      private Common common;
      private WorkArea classification;
      private LegalAction legalAction1;
      private ServiceProcess serviceProcess;
    }

    /// <summary>
    /// Gets a value of Null1.
    /// </summary>
    [JsonIgnore]
    public Array<NullGroup> Null1 => null1 ??= new(NullGroup.Capacity);

    /// <summary>
    /// Gets a value of Null1 for json serialization.
    /// </summary>
    [JsonPropertyName("null1")]
    [Computed]
    public IList<NullGroup> Null1_Json
    {
      get => null1;
      set => Null1.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public LegalAction DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of InitTribunal.
    /// </summary>
    [JsonPropertyName("initTribunal")]
    public Tribunal InitTribunal
    {
      get => initTribunal ??= new();
      set => initTribunal = value;
    }

    /// <summary>
    /// A value of InitFipsTribAddress.
    /// </summary>
    [JsonPropertyName("initFipsTribAddress")]
    public FipsTribAddress InitFipsTribAddress
    {
      get => initFipsTribAddress ??= new();
      set => initFipsTribAddress = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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

    private Array<NullGroup> null1;
    private LegalAction dlgflwSelected;
    private Tribunal initTribunal;
    private FipsTribAddress initFipsTribAddress;
    private DateWorkArea maxDate;
    private Common totalSelected;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
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
    /// A value of LegalAction1.
    /// </summary>
    [JsonPropertyName("legalAction1")]
    public LegalAction LegalAction1
    {
      get => legalAction1 ??= new();
      set => legalAction1 = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private FipsTribAddress fipsTribAddress;
    private LegalAction legalAction1;
    private Tribunal tribunal;
    private Fips fips;
  }
#endregion
}

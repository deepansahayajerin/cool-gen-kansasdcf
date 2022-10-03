// Program: LE_LROL_LEGAL_ROLE, ID: 371998481, model: 746.
// Short name: SWELROLP
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
/// A program: LE_LROL_LEGAL_ROLE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLrolLegalRole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LROL_LEGAL_ROLE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLrolLegalRole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLrolLegalRole.
  /// </summary>
  public LeLrolLegalRole(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 05/22/95  Dave Allen			Initial Code
    // 12/18/98  P. Sharp    			Fixed problems per Phase II assessment.
    // 03/09/99  PMcElderry			Added logic for monitored activities, Event 95
    // 09/09/99  R. Jean	PR#H73001	Allow classifications M, O, and E to be 
    // entered without court
    // 					case numbers like P and F.
    // 10/28/99  R. Jean	PR#H78286	Allow classifications U to be entered
    // 					without court case numbers like P, F, M, O, E are.
    // 11/01/99  R. Jean	PR#H78577	Import cse-person, case, legal action to the 
    // security cab.
    // 03/16/00  J.Magat	WR# 000160	Paternity
    // 06/07/01  V.Madhira	PR# 120250	When flowing from COMN, using AP's person 
    // number, to
    // 					LROL, the names of the case participants do not show up.
    // 04/02/02  GVandy	PR# 138221	Read for end dated code values when 
    // retrieving
    // 					action taken description.
    // 07/01/02  GVandy	WR20338/	Require Petitioner, Respondent, and Child roles
    // 			WR20339		when adding new legal actions
    // 08/23/02  GVandy	PR155865	Don't display a blank line when returning from 
    // NAME without
    // 					a selection.
    // 09/24/02  GVandy	PR#132101	Allow classifications S to be entered
    // 					without court case numbers like P, F, M, O, E, U are.
    // 06/09/15  GVandy	CQ22212		Changes to support electronic Income 
    // Withholding (e-IWO).
    // 05/10/17  GVandy	CQ48108		IV-D PEP Changes.
    // -----------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.CurrentDate.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      if (AsChar(import.LegalActionFlow.Flag) == 'Y')
      {
        // -- Don't allow clear if in the automated flow which occurs when 
        // adding a new legal action.
        goto Test1;
      }

      // ------------------------------------------------------
      // Allow atleast one cse case to be entered when CLEAR is
      // pressed.
      // ------------------------------------------------------
      export.InputCseCases.Index = 0;
      export.InputCseCases.CheckSize();

      export.InputCseCases.Update.DetailInput.Number = "";

      return;
    }

Test1:

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.LegalAction.Assign(import.LegalAction);
    export.PromptClass.SelectChar = import.PromptClass.SelectChar;
    export.PromptTribunal.PromptField = import.PromptTribunal.PromptField;
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.Display.Flag = import.Display.Flag;
    export.Cpat.Number = import.Cpat.Number;
    export.LegalActionFlow.Flag = import.LegalActionFlow.Flag;
    export.EiwoSelection.Flag = import.EiwoSelection.Flag;

    // --05/10/17 GVandy CQ48108 (IV-D PEP Changes) New group to track which 
    // cases have been
    //   sent to CPAT during legal auto flow for paternity resolution.
    for(import.PrevCpatCases.Index = 0; import.PrevCpatCases.Index < import
      .PrevCpatCases.Count; ++import.PrevCpatCases.Index)
    {
      if (!import.PrevCpatCases.CheckSize())
      {
        break;
      }

      export.PrevCpatCases.Index = import.PrevCpatCases.Index;
      export.PrevCpatCases.CheckSize();

      export.PrevCpatCases.Update.GexpCpat.Number =
        import.PrevCpatCases.Item.GimpCpat.Number;
    }

    import.PrevCpatCases.CheckIndex();

    if (!Equal(global.Command, "CPAT"))
    {
      export.LegalActionFlowToCpat.Flag = import.LegalActionFlow.Flag;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        if (export.LegalAction.Identifier == 0)
        {
          // -------------------------------------------------------
          // legal action not known and court case number is blank.
          // --------------------------------------------------------
          ExitState = "ECO_LNK_TO_LAPS_LEG_ACT_BY_PERSN";

          return;
        }
      }
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL"))
    {
      export.PromptClass.SelectChar = "";
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETLTRB"))
    {
      export.PromptTribunal.PromptField = "";
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "ADD"))
    {
      // --------------------------------------------------------
      // Command ADD is treated the same as UPDATE. ADD
      // command is added to the screen only for consistency with
      // other screens
      // --------------------------------------------------------
      global.Command = "UPDATE";
      local.AddCommand.Flag = "Y";
    }
    else
    {
      local.AddCommand.Flag = "N";
    }

    // @@@
    local.CheckEmpty.Count = 0;
    import.CseCases2.Index = 0;

    for(var limit = import.CseCases2.Count; import.CseCases2.Index < limit; ++
      import.CseCases2.Index)
    {
      if (!import.CseCases2.CheckSize())
      {
        break;
      }

      export.InputCseCases.Index = import.CseCases2.Index;
      export.InputCseCases.CheckSize();

      export.InputCseCases.Update.DetailInput.Number =
        import.CseCases2.Item.DetailInput2.Number;
      local.TextWorkArea.Text10 = export.InputCseCases.Item.DetailInput.Number;

      if (!IsEmpty(export.InputCseCases.Item.DetailInput.Number))
      {
        local.CheckEmpty.Count = (int)((long)local.CheckEmpty.Count + 1);
        UseEabPadLeftWithZeros();
        export.InputCseCases.Update.DetailInput.Number =
          local.TextWorkArea.Text10;

        if (export.InputCseCases.Index == 0)
        {
          local.SecurityTest.Number = local.TextWorkArea.Text10;
        }
      }
    }

    import.CseCases2.CheckIndex();

    // @@@
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "REDISP") || Equal
      (global.Command, "RETLACN") || Equal(global.Command, "RETLAPS"))
    {
      // --------------------------------------------------------
      // Dont move the group import to group exports. They will be
      // populated afresh anyway.
      // --------------------------------------------------------
    }
    else
    {
      if (!import.LegalActionPerson.IsEmpty)
      {
        export.LegalActionPerson.Index = 0;
        export.LegalActionPerson.Clear();

        for(import.LegalActionPerson.Index = 0; import
          .LegalActionPerson.Index < import.LegalActionPerson.Count; ++
          import.LegalActionPerson.Index)
        {
          if (export.LegalActionPerson.IsFull)
          {
            break;
          }

          export.LegalActionPerson.Update.DetailDisplayed.Number =
            import.LegalActionPerson.Item.DetailDisplayed.Number;
          export.LegalActionPerson.Update.CaseRole.Assign(
            import.LegalActionPerson.Item.CaseRole);
          export.LegalActionPerson.Update.CsePersonsWorkSet.Assign(
            import.LegalActionPerson.Item.CsePersonsWorkSet);
          export.LegalActionPerson.Update.Common.SelectChar =
            import.LegalActionPerson.Item.Common.SelectChar;
          export.LegalActionPerson.Update.LegalActionPerson1.Assign(
            import.LegalActionPerson.Item.LegalActionPerson1);
          export.LegalActionPerson.Next();
        }
      }

      if (!import.ListEndReason.IsEmpty)
      {
        export.ListEndRsn.Index = -1;

        for(import.ListEndReason.Index = 0; import.ListEndReason.Index < import
          .ListEndReason.Count; ++import.ListEndReason.Index)
        {
          ++export.ListEndRsn.Index;
          export.ListEndRsn.CheckSize();

          export.ListEndRsn.Update.DetailListEndRsn.PromptField =
            import.ListEndReason.Item.DetailListEndRsn.PromptField;

          if (!Equal(global.Command, "LIST") && !
            Equal(global.Command, "RETCDVL"))
          {
            export.ListEndRsn.Update.DetailListEndRsn.PromptField = "";
          }
        }
      }
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    MoveFips(import.HiddenFips, export.HiddenFips);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // --------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // --------------------------------------------------------
      export.HiddenNextTranInfo.Assign(local.NextTranInfo);
      export.LegalAction.Identifier =
        export.HiddenNextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      export.LegalAction.CourtCaseNumber =
        export.HiddenNextTranInfo.CourtCaseNumber ?? "";
      export.HiddenLegalAction.Assign(export.LegalAction);

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }

      global.Command = "REDISP";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      if (AsChar(export.LegalActionFlow.Flag) == 'Y')
      {
        // -- Don't allow the user to nextran if in the automated flow which 
        // occurs
        // when adding a new legal action.
        export.Standard.NextTransaction = "";

        if (Equal(global.Command, "ENTER"))
        {
          // -- Reset the command so that we will get through the main CASE of 
          // command
          // below without flagging ENTER as an invalid function key.
          global.Command = "CLEAR";
        }

        ExitState = "LE0000_USE_PF9_TO_RETURN";

        goto Test2;
      }

      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      UseScCabNextTranPut();

      return;
    }

Test2:

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      // ****************************************************************
      // * 10/28/99		R. Jean
      // PR#H78577 - Import cse-person, case, legal action to the security cab.
      // ****************************************************************
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "FROMCAPT"))
    {
      export.HiddenLegalAction.Assign(export.LegalAction);
      global.Command = "REDISP";
    }

    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DELETE"))
    {
      for(export.InputCseCases.Index = 0; export.InputCseCases.Index < export
        .InputCseCases.Count; ++export.InputCseCases.Index)
      {
        if (!export.InputCseCases.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.InputCseCases.Item.DetailInput.Number))
        {
          if (!ReadCase())
          {
            var field =
              GetField(export.InputCseCases.Item.DetailInput, "number");

            field.Error = true;

            ExitState = "CASE_NF";

            return;
          }
        }
      }

      export.InputCseCases.CheckIndex();

      if (IsEmpty(export.LegalAction.Classification))
      {
        var field = GetField(export.LegalAction, "classification");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }
      }
      else
      {
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = export.LegalAction.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        // *********************************************
        // * Court Case Number can be spaces if        *
        // * Classification is 'P' or 'F'.         *
        // * 9/9/99 R. Jean
        // * Added classes M, O, E
        // * 10/28/99 R. Jean
        // * Added class U
        // * 09/24/02 GVandy
        // * Added class S
        // *********************************************
        if (AsChar(export.LegalAction.Classification) == 'P' || AsChar
          (export.LegalAction.Classification) == 'F' || AsChar
          (export.LegalAction.Classification) == 'M' || AsChar
          (export.LegalAction.Classification) == 'O' || AsChar
          (export.LegalAction.Classification) == 'E' || AsChar
          (export.LegalAction.Classification) == 'U' || AsChar
          (export.LegalAction.Classification) == 'S')
        {
        }
        else if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_CT_CASE_REQD_FOR_CLASS";

          return;
        }
      }

      if (IsEmpty(export.Fips.StateAbbreviation) && export
        .Tribunal.Identifier == 0)
      {
        var field1 = GetField(export.Fips, "countyAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        ExitState = "LE0000_STATE_CNTRY_AND_CNTY_REQ";

        return;
      }

      // ------------------------------------------------------------
      // A request was made by the user that after a display was
      // performed that if the state and county were blanked out that
      // the displayed tribunal information should be blanked out at
      // well.
      // ------------------------------------------------------------
      if ((IsEmpty(export.Fips.StateAbbreviation) || IsEmpty
        (export.Fips.CountyAbbreviation)) && IsEmpty
        (export.FipsTribAddress.Country) && export.Tribunal.Identifier > 0)
      {
        export.Fips.CountyDescription = "";
        export.Tribunal.Name = "";
        export.Tribunal.JudicialDistrict = "";
        export.Tribunal.JudicialDivision = "";
        export.Tribunal.Identifier = 0;

        if (!import.LegalActionPerson.IsEmpty)
        {
          export.LegalActionPerson.Index = 0;
          export.LegalActionPerson.Clear();

          for(import.LegalActionPerson.Index = 0; import
            .LegalActionPerson.Index < import.LegalActionPerson.Count; ++
            import.LegalActionPerson.Index)
          {
            if (export.LegalActionPerson.IsFull)
            {
              break;
            }

            export.LegalActionPerson.Update.DetailDisplayed.Number =
              import.LegalActionPerson.Item.DetailDisplayed.Number;
            export.LegalActionPerson.Update.CaseRole.Assign(
              import.LegalActionPerson.Item.CaseRole);
            export.LegalActionPerson.Update.CsePersonsWorkSet.Assign(
              import.LegalActionPerson.Item.CsePersonsWorkSet);
            export.LegalActionPerson.Update.Common.SelectChar =
              import.LegalActionPerson.Item.Common.SelectChar;
            export.LegalActionPerson.Update.LegalActionPerson1.Assign(
              import.LegalActionPerson.Item.LegalActionPerson1);
            export.LegalActionPerson.Next();
          }
        }

        if (!import.ListEndReason.IsEmpty)
        {
          export.ListEndRsn.Index = -1;

          for(import.ListEndReason.Index = 0; import.ListEndReason.Index < import
            .ListEndReason.Count; ++import.ListEndReason.Index)
          {
            ++export.ListEndRsn.Index;
            export.ListEndRsn.CheckSize();

            export.ListEndRsn.Update.DetailListEndRsn.PromptField =
              import.ListEndReason.Item.DetailListEndRsn.PromptField;

            if (!Equal(global.Command, "LIST") && !
              Equal(global.Command, "RETCDVL"))
            {
              export.ListEndRsn.Update.DetailListEndRsn.PromptField = "";
            }
          }
        }

        var field1 = GetField(export.Fips, "countyAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        ExitState = "LE0000_STATE_CNTRY_AND_CNTY_REQ";

        return;
      }

      if (!IsEmpty(export.Fips.StateAbbreviation))
      {
        if (IsEmpty(export.Fips.CountyAbbreviation))
        {
          ExitState = "LE0000_TRIB_COUNTY_REQUIRED";
          export.Fips.CountyDescription = "";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          return;
        }

        if (ReadFips1())
        {
          export.Fips.Assign(entities.Fips);
        }
        else if (ReadFips2())
        {
          export.Fips.CountyDescription = "";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          ExitState = "INVALID_COUNTY_CODE";

          return;
        }
        else
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_CODE";

          return;
        }
      }

      if (export.LegalAction.Identifier > 0)
      {
        if (!ReadLegalAction3())
        {
          MoveTribunal(local.InitialisedToSpaces, export.Tribunal);
          ExitState = "LEGAL_ACTION_NF";

          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          return;
        }
      }
      else if (!IsEmpty(export.Fips.StateAbbreviation))
      {
        // ------------------------------------------
        // Legal Action has not been selected yet
        // ------------------------------------------
        if (!IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          if (!ReadLegalAction1())
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.Fips, "stateAbbreviation");

            field2.Error = true;

            var field3 = GetField(export.Fips, "countyAbbreviation");

            field3.Error = true;

            MoveTribunal(local.InitialisedToSpaces, export.Tribunal);
            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

            return;
          }
        }
      }
      else if (!ReadLegalAction2())
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

        return;
      }
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (AsChar(export.Display.Flag) == 'Y')
      {
      }
      else
      {
        ExitState = "LE0000_DISP_BEFORE_ADD_UPD_DEL";

        return;
      }

      local.TotalSelected.Count = 0;

      for(export.LegalActionPerson.Index = 0; export.LegalActionPerson.Index < export
        .LegalActionPerson.Count; ++export.LegalActionPerson.Index)
      {
        switch(AsChar(export.LegalActionPerson.Item.Common.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.TotalSelected.Count;

            break;
          default:
            var field =
              GetField(export.LegalActionPerson.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ---------------------------------------------
      // At least one row must be selected on update.
      // ---------------------------------------------
      if (local.TotalSelected.Count < 1)
      {
        ExitState = "LE0000_MUST_SELECT_A_LEGAL_ROLE";

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          var field =
            GetField(export.LegalActionPerson.Item.Common, "selectChar");

          field.Error = true;

          return;
        }
      }
    }

    if (Equal(global.Command, "UPDATE"))
    {
      for(export.LegalActionPerson.Index = 0; export.LegalActionPerson.Index < export
        .LegalActionPerson.Count; ++export.LegalActionPerson.Index)
      {
        if (AsChar(export.LegalActionPerson.Item.Common.SelectChar) == 'S')
        {
          // ----------------------------------------------------------
          // Edit for group_export legal_action_person role for nonblank
          // has been removed from here.
          // ----------------------------------------------------------
          switch(AsChar(export.LegalActionPerson.Item.LegalActionPerson1.Role))
          {
            case 'P':
              break;
            case 'R':
              break;
            case 'C':
              break;
            default:
              var field =
                GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                "role");

              field.Error = true;

              ExitState = "LE0000_ROLE_MUST_BE_P_R_C";

              break;
          }

          if (Equal(export.LegalActionPerson.Item.LegalActionPerson1.
            EffectiveDate, local.Initialize.EffectiveDate))
          {
            export.LegalActionPerson.Update.LegalActionPerson1.EffectiveDate =
              local.CurrentDate.Date;
          }

          if (Lt(local.CurrentDate.Date,
            export.LegalActionPerson.Item.LegalActionPerson1.EffectiveDate))
          {
            var field =
              GetField(export.LegalActionPerson.Item.LegalActionPerson1,
              "effectiveDate");

            field.Error = true;

            ExitState = "LE0000_EFF_DT_GRTR_CURRENT";
          }

          if (Lt(local.Initialize.EffectiveDate,
            export.LegalActionPerson.Item.LegalActionPerson1.EndDate))
          {
            if (Lt(export.LegalActionPerson.Item.LegalActionPerson1.EndDate,
              export.LegalActionPerson.Item.LegalActionPerson1.EffectiveDate))
            {
              var field =
                GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                "endDate");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "LE0000_END_DT_PRIOR_EFF_DT";
              }
            }
          }

          if (!IsEmpty(export.LegalActionPerson.Item.LegalActionPerson1.
            EndReason) || export
            .LegalActionPerson.Item.LegalActionPerson1.EndDate != null)
          {
            if (Equal(export.LegalActionPerson.Item.LegalActionPerson1.EndDate,
              null))
            {
              var field =
                GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                "endDate");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (IsEmpty(export.LegalActionPerson.Item.LegalActionPerson1.
              EndReason))
            {
              var field =
                GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                "endReason");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }

              goto Test3;
            }

            local.Code.CodeName = "LEGAL ROLE END REASON";
            local.CodeValue.Cdvalue =
              export.LegalActionPerson.Item.LegalActionPerson1.EndReason ?? Spaces
              (10);
            UseCabValidateCodeValue();

            if (AsChar(local.ValidCode.Flag) != 'Y')
            {
              var field =
                GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                "endReason");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "LE0000_INVALID_LGL_ROLE_END_RSN";
              }
            }
          }
        }

Test3:
        ;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "NAME":
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        return;
      case "SIGNOFF":
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";

          break;
        }

        UseScCabSignoff();

        return;
      case "EXIT":
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";

          break;
        }

        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETNAME":
        local.Dummy.Flag = "N";

        if (!IsEmpty(import.SelectedNonCase.Number))
        {
          export.LegalActionPerson.Index = export.LegalActionPerson.Count;
          export.LegalActionPerson.CheckIndex();

          do
          {
            if (export.LegalActionPerson.IsFull)
            {
              break;
            }

            export.LegalActionPerson.Update.CsePersonsWorkSet.Assign(
              import.SelectedNonCase);
            export.LegalActionPerson.Update.Common.SelectChar = "";
            local.Dummy.Flag = "Y";
            export.LegalActionPerson.Next();

            break;

            export.LegalActionPerson.Next();
          }
          while(AsChar(local.Dummy.Flag) != 'Y');
        }

        export.ListEndRsn.Index = -1;

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          ++export.ListEndRsn.Index;
          export.ListEndRsn.CheckSize();
        }

        export.ListEndRsn.Update.DetailListEndRsn.PromptField = "";

        break;
      case "LISTPERS":
        for(export.InputCseCases.Index = 0; export.InputCseCases.Index < export
          .InputCseCases.Count; ++export.InputCseCases.Index)
        {
          if (!export.InputCseCases.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.InputCseCases.Item.DetailInput.Number))
          {
            if (!ReadCase())
            {
              var field =
                GetField(export.InputCseCases.Item.DetailInput, "number");

              field.Error = true;

              ExitState = "CASE_NF";

              return;
            }
          }
        }

        export.InputCseCases.CheckIndex();

        if (local.CheckEmpty.Count == 0)
        {
          if (export.LegalAction.Identifier > 0)
          {
            UseLeCabRelatedCseCasesFLact();
          }
        }

        UseLeLrolListLegActPersonsV2();

        if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
        {
          export.Display.Flag = "Y";
        }

        export.HiddenLegalAction.Assign(export.LegalAction);

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Action taken later
        // ---------------------------------------------
        break;
      case "REDISP":
        // ---------------------------------------------
        // Per PR# 120250, this CASE is added.
        //  Otherwise, in case of REDISP, the code in CASE OTHERWISE will be 
        // executed and a invalid exitstate is set. Later in CASE of REDISP
        // below,  while executing 'SI_READ_CSE_PERSON'  CAB, after reading
        // names from ADABAS, because of invalid exitstate( <> ALL_OK), instead
        // of moving the data from ADABAS to export view, the control is coming
        // out of the CAB and the names will not display on screen.
        // Now the screen must display the legal role names.
        //                                     
        // ---- Vithal (06/07/2001)
        // ---------------------------------------------
        // ---------------------------------------------
        // Action taken later
        // ---------------------------------------------
        break;
      case "LIST":
        // ---------------------------------------------
        // Class Prompt.
        // ---------------------------------------------
        switch(AsChar(export.PromptClass.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptClass, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.PromptTribunal.PromptField))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptTribunal, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        export.ListEndRsn.Index = -1;

        for(import.ListEndReason.Index = 0; import.ListEndReason.Index < import
          .ListEndReason.Count; ++import.ListEndReason.Index)
        {
          ++export.ListEndRsn.Index;
          export.ListEndRsn.CheckSize();

          switch(AsChar(export.ListEndRsn.Item.DetailListEndRsn.PromptField))
          {
            case 'S':
              ++local.PromptCount.Count;

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.ListEndRsn.Item.DetailListEndRsn, "promptField");
                

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.PromptCount.Count == 1)
        {
          if (AsChar(export.PromptClass.SelectChar) == 'S')
          {
            export.DisplayActiveCasesOnly.Flag = "Y";
            export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }

          if (AsChar(export.PromptTribunal.PromptField) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_TRIBUNALS1";
          }

          export.ListEndRsn.Index = -1;

          for(import.ListEndReason.Index = 0; import.ListEndReason.Index < import
            .ListEndReason.Count; ++import.ListEndReason.Index)
          {
            ++export.ListEndRsn.Index;
            export.ListEndRsn.CheckSize();

            if (AsChar(export.ListEndRsn.Item.DetailListEndRsn.PromptField) == 'S'
              )
            {
              export.DisplayActiveCasesOnly.Flag = "Y";
              export.Code.CodeName = "LEGAL ROLE END REASON";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
            }
          }
        }
        else if (local.PromptCount.Count > 1)
        {
          if (AsChar(export.PromptClass.SelectChar) == 'S')
          {
            var field = GetField(export.PromptClass, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.PromptTribunal.PromptField) == 'S')
          {
            var field = GetField(export.PromptTribunal, "promptField");

            field.Error = true;
          }

          export.ListEndRsn.Index = -1;

          for(import.ListEndReason.Index = 0; import.ListEndReason.Index < import
            .ListEndReason.Count; ++import.ListEndReason.Index)
          {
            ++export.ListEndRsn.Index;
            export.ListEndRsn.CheckSize();

            if (AsChar(export.ListEndRsn.Item.DetailListEndRsn.PromptField) == 'S'
              )
            {
              var field =
                GetField(export.ListEndRsn.Item.DetailListEndRsn, "promptField");
                

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        return;
      case "RETLTRB":
        export.PromptTribunal.PromptField = "";

        break;
      case "RETCDVL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // ---------------------------------------------
        if (AsChar(import.PromptClass.SelectChar) == 'S')
        {
          export.PromptClass.SelectChar = "";

          if (IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            var field = GetField(export.LegalAction, "classification");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.LegalAction.Classification =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.Fips, "stateAbbreviation");

            field.Protected = false;
            field.Focused = true;
          }
        }

        local.GrpEntry.Count = 0;

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          ++local.GrpEntry.Count;

          export.ListEndRsn.Index = local.GrpEntry.Count - 1;
          export.ListEndRsn.CheckSize();

          if (AsChar(export.ListEndRsn.Item.DetailListEndRsn.PromptField) == 'S'
            )
          {
            export.ListEndRsn.Update.DetailListEndRsn.PromptField = "";

            if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
            {
              export.LegalActionPerson.Update.LegalActionPerson1.EndReason =
                import.DlgflwSelectedCodeValue.Cdvalue;
            }
            else
            {
              var field =
                GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                "endReason");

              field.Protected = false;
              field.Focused = true;
            }

            return;
          }
        }

        return;
      case "RETLACN":
        // ---------------------------------------------
        // Returned from List Legal Actions by Court
        // Case Number.
        // ---------------------------------------------
        if (export.LegalAction.Identifier > 0)
        {
          export.HiddenLegalAction.Assign(export.LegalAction);

          if (local.CheckEmpty.Count == 0)
          {
            UseLeCabRelatedCseCasesFLact();
          }

          UseLeLrolListOnlyRelvntCroles();

          if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
            export.Display.Flag = "Y";
          }
        }

        break;
      case "RETLAPS":
        // ---------------------------------------------
        // Returned from List Legal Actions by CSE Person
        // ---------------------------------------------
        if (export.LegalAction.Identifier > 0)
        {
          export.HiddenLegalAction.Assign(export.LegalAction);

          if (local.CheckEmpty.Count == 0)
          {
            UseLeCabRelatedCseCasesFLact();
          }

          UseLeLrolListOnlyRelvntCroles();

          if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
            export.Display.Flag = "Y";
          }
        }

        break;
      case "RETCPAT":
        // ---------------------------------------------
        // Returned from Child Paternity
        // ---------------------------------------------
        // *** Continue where one left off before flowing to CPAT.
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          if (Equal(import.PrevH.Command, "CPAT"))
          {
            // -- The user went to CPAT by pressing F19, not as a result of the 
            // automated flow.  Therefore, skip the edits normally done as a
            // result of the automated flow.
            break;
          }

          // Determine if there are any other Children on the legal action which
          // have a
          // Born Out of Wedlock or CSE to Establish Paternity indicator of "U".
          UseLeLrolCheckForPaternity();

          if (IsExitState("SI0000_INV_PATERNITY_IND_COMB"))
          {
            ExitState = "ECO_LNK_TO_CPAT";
          }
          else
          {
            ExitState = "ACO_NE0000_RETURN";
          }

          return;
        }

        break;
      case "UPDATE":
        // ------------------------------------------------------------
        // Make sure that Court Case Number hasn't been changed
        // before an update.
        // ------------------------------------------------------------
        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.HiddenFips.StateAbbreviation,
          export.HiddenFips.StateAbbreviation))
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.HiddenFips.CountyAbbreviation,
          export.HiddenFips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ------------------------------------------------------------
        // Verify that a display has been performed before the update
        // can take place.
        // ------------------------------------------------------------
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier || import.LegalAction.Identifier == 0)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_DISP_BEFORE_ADD_UPD_DEL";

          return;
        }

        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          // -- During the automated flow for adding legal actions, don't check 
          // for paternity
          // since we will automatically flow the user to CPAT to correct any 
          // paternity issues.
          // -- Petitioner, Respondent, and Child roles are required during the 
          // automated flow.
          for(export.LegalActionPerson.Index = 0; export
            .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
            export.LegalActionPerson.Index)
          {
            if (export.LegalActionPerson.Item.LegalActionPerson1.Identifier != 0
              || AsChar(export.LegalActionPerson.Item.Common.SelectChar) == 'S'
              )
            {
              switch(AsChar(export.LegalActionPerson.Item.LegalActionPerson1.
                Role))
              {
                case 'P':
                  local.PetitionerFound.Flag = "Y";

                  break;
                case 'R':
                  local.RespondentFound.Flag = "Y";

                  break;
                case 'C':
                  local.ChildFound.Flag = "Y";

                  break;
                default:
                  break;
              }

              if (AsChar(local.PetitionerFound.Flag) == 'Y' && AsChar
                (local.RespondentFound.Flag) == 'Y' && AsChar
                (local.ChildFound.Flag) == 'Y')
              {
                break;
              }
            }
          }

          if (AsChar(local.PetitionerFound.Flag) == 'Y' && AsChar
            (local.RespondentFound.Flag) == 'Y' && AsChar
            (local.ChildFound.Flag) == 'Y')
          {
          }
          else
          {
            ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

            return;
          }
        }
        else
        {
          // --05/10/2017 GVandy CQ48108 (IV-D Changes) Paternity indicator can 
          // now be "U" when
          //    adding an AP.
        }

        // ---------------------------------------------
        // Update the current Legal Action Role.
        // ---------------------------------------------
        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          if (!IsEmpty(export.LegalActionPerson.Item.Common.SelectChar))
          {
            UseLeLrolUpdateLegalActionPers();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              if (IsExitState("CASE_ROLE_NF"))
              {
                var field1 =
                  GetField(export.LegalActionPerson.Item.CaseRole, "type1");

                field1.Error = true;

                var field2 =
                  GetField(export.LegalActionPerson.Item.CsePersonsWorkSet,
                  "number");

                field2.Error = true;

                return;
              }

              if (IsExitState("LEGAL_ACTION_NF"))
              {
                MoveTribunal(local.InitialisedToSpaces, export.Tribunal);

                var field1 = GetField(export.LegalAction, "courtCaseNumber");

                field1.Error = true;

                var field2 = GetField(export.LegalAction, "classification");

                field2.Error = true;
              }
              else
              {
                var field1 =
                  GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                  "role");

                field1.Error = true;

                var field2 =
                  GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                  "endDate");

                field3.Error = true;

                var field4 =
                  GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                  "endReason");

                field4.Error = true;
              }

              return;
            }
          }
        }

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          export.LegalActionPerson.Update.Common.SelectChar = "";
        }

        // ------------------------------------------------------------
        // Redisplay the roles again. This is required because, you
        // may modify data for one case_role record for a cse_person,
        // but it should reflect on all case_role lines for that person.
        // e.g. person 1 is an AP as well as FA; if you select AP
        // caserole record and change effective/end date/end reason,
        // after update, the screen be refreshed to show the same
        // updated details for both AP and FA caserole records for that
        // person
        // ------------------------------------------------------------
        UseLeLrolListOnlyRelvntCroles();

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          if (Equal(export.LegalActionPerson.Item.LegalActionPerson1.EndDate,
            local.Max.Date))
          {
            export.LegalActionPerson.Update.LegalActionPerson1.EndDate = null;
          }
        }

        if (AsChar(local.AddCommand.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          // If there are any Children on the legal action which have a Born Out
          // of
          // Wedlock or CSE to Establish Paternity indicator of "U" then flow to
          // CPAT
          // and force the user to change these values.
          UseLeLrolCheckForPaternity();

          if (IsExitState("SI0000_INV_PATERNITY_IND_COMB"))
          {
            ExitState = "ECO_LNK_TO_CPAT";
          }
          else
          {
            ExitState = "ACO_NE0000_RETURN";
          }

          return;
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        local.TotalSelected.Count = 0;

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          if (!IsEmpty(export.LegalActionPerson.Item.Common.SelectChar))
          {
            ++local.TotalSelected.Count;

            if (local.TotalSelected.Count > 1)
            {
              var field =
                GetField(export.LegalActionPerson.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              return;
            }

            // 06/09/15  GVandy  CQ22212  Changes to support electronic Income 
            // Withholding (e-IWO).
            if (AsChar(import.EiwoSelection.Flag) == 'Y' || AsChar
              (import.EiwoSelection.Flag) == 'N')
            {
              // -- Selected role type must be "AP"
              if (!Equal(export.LegalActionPerson.Item.CaseRole.Type1, "AP"))
              {
                var field =
                  GetField(export.LegalActionPerson.Item.CaseRole, "type1");

                field.Error = true;

                ExitState = "LE0000_EIWO_INVALID_ROLE_SELECT";

                return;
              }

              // -- Selected "AP" must have an active LROL record.
              if (!Lt(local.CurrentDate.Date,
                export.LegalActionPerson.Item.LegalActionPerson1.
                  EffectiveDate) && Lt
                (local.Null1.Date,
                export.LegalActionPerson.Item.LegalActionPerson1.
                  EffectiveDate) && (
                  !Lt(export.LegalActionPerson.Item.LegalActionPerson1.EndDate,
                local.CurrentDate.Date) || Equal
                (export.LegalActionPerson.Item.LegalActionPerson1.EndDate,
                local.Null1.Date)))
              {
              }
              else
              {
                var field1 =
                  GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                  "effectiveDate");

                field1.Error = true;

                var field2 =
                  GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                  "endDate");

                field2.Error = true;

                ExitState = "LE0000_EIWO_INVALID_AP_ROLE";

                return;
              }

              if (AsChar(import.EiwoSelection.Flag) == 'Y')
              {
                if (Equal(export.LegalAction.ActionTaken, "IWO") || Equal
                  (export.LegalAction.ActionTaken, "IWOMODO") || Equal
                  (export.LegalAction.ActionTaken, "ORDIWO2") || Equal
                  (export.LegalAction.ActionTaken, "ORDIOWLS"))
                {
                  // -- Selected "AP" must have an active Obligor LOPS record.
                  if (!ReadCaseCsePerson())
                  {
                    var field =
                      GetField(export.LegalActionPerson.Item.CsePersonsWorkSet,
                      "number");

                    field.Error = true;

                    ExitState = "LE0000_EIWO_MISSING_OBLIGOR_LOPS";

                    return;
                  }
                }
              }
              else if (AsChar(import.EiwoSelection.Flag) == 'N')
              {
                // -- Selected "AP" must have an active Obligor LOPS record.
                if (!ReadCaseCsePerson())
                {
                  var field =
                    GetField(export.LegalActionPerson.Item.CsePersonsWorkSet,
                    "number");

                  field.Error = true;

                  ExitState = "LE0000_EIWO_MISSING_OBLIGOR_LOPS";

                  return;
                }
              }

              export.DlgflwSelectedLegalActionPerson.Assign(
                export.LegalActionPerson.Item.LegalActionPerson1);
              export.DlgflwSelectedCase.Number =
                export.LegalActionPerson.Item.DetailDisplayed.Number;
              export.DlgflwSelectedCaseRole.Type1 =
                export.LegalActionPerson.Item.CaseRole.Type1;
              export.DlgflwSelectedCsePerson.Number =
                export.LegalActionPerson.Item.CsePersonsWorkSet.Number;
            }

            break;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          // -- During the automated flow for new legal actions, don't allow the
          // user to return without having entered a Petitioner, Respondent,
          // and Child.
          if (!ReadLegalActionPerson2())
          {
            ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

            goto Test4;
          }

          if (!ReadLegalActionPerson3())
          {
            ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

            goto Test4;
          }

          if (!ReadLegalActionPerson1())
          {
            ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";
          }
        }

Test4:

        break;
      case "DELETE":
        // ------------------------------------------------------------
        // Make sure that Court Case Number hasn't been changed
        // before a deletion.
        // ------------------------------------------------------------
        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // ------------------------------------------------------------
        // Verify that a display has been performed before the deletion
        // can take place.
        // ------------------------------------------------------------
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier || import.LegalAction.Identifier == 0)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        // ------------------------------------------------------------
        // Delete the selected occurrence of the Legal Action Person
        // and blank out the fields.
        // ------------------------------------------------------------
        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          if (!IsEmpty(export.LegalActionPerson.Item.Common.SelectChar))
          {
            UseLeDeleteLegalRoleAndPerson();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.LegalActionPerson.Update.LegalActionPerson1.Assign(
                local.Initialize);
            }
            else
            {
              UseEabRollbackCics();

              if (IsExitState("CASE_ROLE_NF"))
              {
                var field1 =
                  GetField(export.LegalActionPerson.Item.CaseRole, "type1");

                field1.Error = true;

                var field2 =
                  GetField(export.LegalActionPerson.Item.CsePersonsWorkSet,
                  "number");

                field2.Error = true;

                return;
              }

              var field =
                GetField(export.LegalActionPerson.Item.LegalActionPerson1,
                "effectiveDate");

              field.Error = true;

              return;
            }
          }
        }

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          export.LegalActionPerson.Update.Common.SelectChar = "";
        }

        UseLeLrolListOnlyRelvntCroles();

        for(export.LegalActionPerson.Index = 0; export
          .LegalActionPerson.Index < export.LegalActionPerson.Count; ++
          export.LegalActionPerson.Index)
        {
          if (Equal(export.LegalActionPerson.Item.LegalActionPerson1.EndDate,
            local.Max.Date))
          {
            export.LegalActionPerson.Update.LegalActionPerson1.EndDate = null;
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "LACT":
        // ---------------------------------------------
        // Transfer to "Legal Action" screen.
        // ---------------------------------------------
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";

          break;
        }

        ExitState = "ECO_XFR_TO_LEGAL_ACTION";

        return;
      case "LDET":
        // ---------------------------------------------
        // Transfer to "Legal Detail" screen.
        // ---------------------------------------------
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";

          break;
        }

        ExitState = "ECO_XFR_TO_LEGAL_DETAIL";

        return;
      case "CPAT":
        // ---------------------------------------------
        // Link to "CPAT:Child Paternity" screen.
        // ---------------------------------------------
        if (IsEmpty(export.Cpat.Number))
        {
          // *** If not set by SWE00836, use first available Case#.
          for(export.InputCseCases.Index = 0; export.InputCseCases.Index < export
            .InputCseCases.Count; ++export.InputCseCases.Index)
          {
            if (!export.InputCseCases.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.InputCseCases.Item.DetailInput.Number))
            {
              export.Cpat.Number = export.InputCseCases.Item.DetailInput.Number;

              break;
            }
          }

          export.InputCseCases.CheckIndex();
        }

        export.PrevH.Command = global.Command;
        export.LegalActionFlowToCpat.Flag = "";
        export.FromLrol.Flag = "Y";
        ExitState = "ECO_LNK_TO_CPAT";

        break;
      case "CLEAR":
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (IsExitState("LE0000_USE_PF9_TO_RETURN") && AsChar
      (export.LegalActionFlow.Flag) == 'Y')
    {
      // -- During the automated flow instead of giving the user a message sayng
      // to use F9 to return
      // when they press a function key that is disabled during this flow, give 
      // them message indicating
      // that Petitioner, Respondent, and Child are required if these roles have
      // not yet been entered.
      if (!ReadLegalActionPerson2())
      {
        ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

        goto Test5;
      }

      if (!ReadLegalActionPerson3())
      {
        ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

        goto Test5;
      }

      if (!ReadLegalActionPerson1())
      {
        ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";
      }
    }

Test5:

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ------------------------------------------------------------
        // If the screen has already been displayed, (identifier is
        // present and equal to hidden identifier) and the court case
        // number and classification haven't been changed, there is
        // no need to link to list screen to choose a legal action. It is
        // OK to display the screen.
        // ------------------------------------------------------------
        if (export.LegalAction.Identifier == export
          .HiddenLegalAction.Identifier && Equal
          (export.LegalAction.CourtCaseNumber,
          export.HiddenLegalAction.CourtCaseNumber) && AsChar
          (export.LegalAction.Classification) == AsChar
          (export.HiddenLegalAction.Classification) && export
          .LegalAction.Identifier > 0)
        {
          if (local.CheckEmpty.Count == 0)
          {
            UseLeCabRelatedCseCasesFLact();
          }

          UseLeLrolListOnlyRelvntCroles();

          if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
            export.Display.Flag = "Y";
          }
        }
        else
        {
          // ------------------------------------------------------------
          // If the Court Case Number that was entered, exists on more
          // than one Legal Action, display a list screen to select the
          // desired one.
          // ------------------------------------------------------------
          local.NoOfLegalActionsFound.Count = 0;

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            // -----------
            // US tribunal
            // -----------
            foreach(var item in ReadLegalActionTribunalFips())
            {
              if (IsEmpty(export.LegalAction.Classification))
              {
              }
              else if (AsChar(export.LegalAction.Classification) == AsChar
                (entities.LegalAction.Classification))
              {
              }
              else
              {
                continue;
              }

              ++local.NoOfLegalActionsFound.Count;

              if (local.NoOfLegalActionsFound.Count > 1)
              {
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              export.LegalAction.Assign(entities.LegalAction);
            }
          }
          else
          {
            // ---------------
            // Foreign tribunal
            // ---------------
            if (export.Tribunal.Identifier == 0)
            {
              ExitState = "LE0000_TRIB_REQD_4_FOREIGN_TRIB";

              var field = GetField(export.PromptTribunal, "promptField");

              field.Protected = false;
              field.Focused = true;

              return;
            }

            foreach(var item in ReadLegalActionTribunal())
            {
              if (IsEmpty(export.LegalAction.Classification))
              {
              }
              else if (AsChar(export.LegalAction.Classification) == AsChar
                (entities.LegalAction.Classification))
              {
              }
              else
              {
                continue;
              }

              ++local.NoOfLegalActionsFound.Count;

              if (local.NoOfLegalActionsFound.Count > 1)
              {
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              export.LegalAction.Assign(entities.LegalAction);
            }
          }

          if (local.NoOfLegalActionsFound.Count == 0)
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            MoveTribunal(local.InitialisedToSpaces, export.Tribunal);
            ExitState = "LEGAL_ACTION_NF";

            return;
          }

          if (local.CheckEmpty.Count == 0)
          {
            UseLeCabRelatedCseCasesFLact();
          }

          UseLeLrolListOnlyRelvntCroles();

          if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
            export.Display.Flag = "Y";
          }
        }

        if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
        {
          if (AsChar(export.EiwoSelection.Flag) == 'Y')
          {
            ExitState = "LE0000_EIWO_SELECT_AP";
          }
        }

        break;
      case "REDISP":
        if (local.CheckEmpty.Count == 0)
        {
          UseLeCabRelatedCseCasesFLact();
        }

        UseLeLrolListOnlyRelvntCroles();

        if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
        {
          export.Display.Flag = "Y";

          if (AsChar(export.LegalActionFlow.Flag) == 'Y')
          {
            if (!ReadLegalActionPerson2())
            {
              ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

              break;
            }

            if (!ReadLegalActionPerson3())
            {
              ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

              break;
            }

            if (!ReadLegalActionPerson1())
            {
              ExitState = "LE0000_PET_RESP_CHILD_REQUIRED";

              break;
            }

            // If there are any Children on the legal action which have a Born 
            // Out of
            // Wedlock or CSE to Establish Paternity indicator of "U" then flow 
            // to CPAT
            // and force the user to change these values.
            UseLeLrolCheckForPaternity();

            if (IsExitState("SI0000_INV_PATERNITY_IND_COMB"))
            {
              ExitState = "ECO_LNK_TO_CPAT";
            }
            else
            {
              ExitState = "ACO_NE0000_RETURN";
            }
          }
        }

        break;
      default:
        break;
    }

    // ------------------------------------------------------------
    // Make sure that one blank entry exists at the end so that the
    // user can enter the next cse case number. Note that the
    // 'unused entries are protected' on the screen and this option
    // is applied by IEF to both the repeating groups.
    // ------------------------------------------------------------
    if (export.InputCseCases.IsEmpty)
    {
      export.InputCseCases.Index = 0;
      export.InputCseCases.CheckSize();

      export.InputCseCases.Update.DetailInput.Number = "";
    }
    else
    {
      export.InputCseCases.Index = export.InputCseCases.Count - 1;
      export.InputCseCases.CheckSize();

      if (IsEmpty(export.InputCseCases.Item.DetailInput.Number))
      {
        // ---------------------------
        // blank entry already exists
        // ---------------------------
      }
      else if (export.InputCseCases.Index + 1 < Export
        .InputCseCasesGroup.Capacity)
      {
        ++export.InputCseCases.Index;
        export.InputCseCases.CheckSize();

        export.InputCseCases.Update.DetailInput.Number = "";
      }
    }

    // -----------------------------------
    // Move the action taken description
    // -----------------------------------
    if (export.LegalAction.Identifier > 0)
    {
      if (ReadLegalAction3())
      {
        export.LegalAction.Assign(entities.LegalAction);
        UseLeGetActionTakenDescription();
      }
    }

    // ------------------------------------------------------------
    // If these dates were stored as max dates, (12312099),
    // convert them to zeros and don't display them on the screen.
    // ------------------------------------------------------------
    for(export.LegalActionPerson.Index = 0; export.LegalActionPerson.Index < export
      .LegalActionPerson.Count; ++export.LegalActionPerson.Index)
    {
      if (Equal(export.LegalActionPerson.Item.LegalActionPerson1.EffectiveDate,
        local.Max.Date))
      {
        export.LegalActionPerson.Update.LegalActionPerson1.EffectiveDate = null;
      }

      if (Equal(export.LegalActionPerson.Item.LegalActionPerson1.EndDate,
        local.Max.Date))
      {
        export.LegalActionPerson.Update.LegalActionPerson1.EndDate = null;
      }

      if (Equal(export.LegalActionPerson.Item.CaseRole.EndDate, local.Max.Date))
      {
        export.LegalActionPerson.Update.CaseRole.EndDate = null;
      }
    }

    // ------------------------------------------------
    // If all processing completed successfully, move
    // all exports to previous exports.
    // ------------------------------------------------
    export.HiddenLegalAction.Assign(export.LegalAction);
    MoveFips(export.Fips, export.HiddenFips);
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveExpPrevCpatCases(LeLrolCheckForPaternity.Import.
    ExpPrevCpatCasesGroup source, Export.PrevCpatCasesGroup target)
  {
    target.GexpCpat.Number = source.GimpExpCpat.Number;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveInputCseCases(Export.InputCseCasesGroup source,
    LeLrolListLegActPersonsV2.Import.CseCasesGroup target)
  {
    target.Detail.Number = source.DetailInput.Number;
  }

  private static void MoveInputCseCasesToCseCasesList(Export.
    InputCseCasesGroup source,
    LeLrolListOnlyRelvntCroles.Import.CseCasesListGroup target)
  {
    target.DetailListOnly.Number = source.DetailInput.Number;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalActionPerson(Export.
    LegalActionPersonGroup source,
    LeLrolCheckForPaternity.Import.LegalActionPersonGroup target)
  {
    target.DetailDisplayed.Number = source.DetailDisplayed.Number;
    target.Common.SelectChar = source.Common.SelectChar;
    target.CaseRole.Assign(source.CaseRole);
    target.LegalActionPerson1.Assign(source.LegalActionPerson1);
    target.CsePersonsWorkSet.Assign(source.CsePersonsWorkSet);
  }

  private static void MoveListEndReasonToListEndRsn1(LeLrolListLegActPersonsV2.
    Export.ListEndReasonGroup source, Export.ListEndRsnGroup target)
  {
    target.DetailListEndRsn.PromptField = source.DetailListEndRsn.PromptField;
  }

  private static void MoveListEndReasonToListEndRsn2(LeLrolListOnlyRelvntCroles.
    Export.ListEndReasonGroup source, Export.ListEndRsnGroup target)
  {
    target.DetailListEndRsn.PromptField = source.DetailListEndRsn.PromptField;
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

  private static void MovePrevCpatCases(Export.PrevCpatCasesGroup source,
    LeLrolCheckForPaternity.Import.ExpPrevCpatCasesGroup target)
  {
    target.GimpExpCpat.Number = source.GexpCpat.Number;
  }

  private static void MoveRelatedCseCasesToInputCseCases(
    LeCabRelatedCseCasesFLact.Export.RelatedCseCasesGroup source,
    Export.InputCseCasesGroup target)
  {
    target.DetailInput.Number = source.DetailRelated.Number;
  }

  private static void MoveRoleToLegalActionPerson1(LeLrolListLegActPersonsV2.
    Export.RoleGroup source, Export.LegalActionPersonGroup target)
  {
    target.DetailDisplayed.Number = source.Detail.Number;
    target.Common.SelectChar = source.Common.SelectChar;
    target.CaseRole.Assign(source.CaseRole);
    target.LegalActionPerson1.Assign(source.LegalActionPerson);
    target.CsePersonsWorkSet.Assign(source.CsePersonsWorkSet);
  }

  private static void MoveRoleToLegalActionPerson2(LeLrolListOnlyRelvntCroles.
    Export.RoleGroup source, Export.LegalActionPersonGroup target)
  {
    target.DetailDisplayed.Number = source.Detail.Number;
    target.Common.SelectChar = source.Common.SelectChar;
    target.CaseRole.Assign(source.CaseRole);
    target.LegalActionPerson1.Assign(source.LegalActionPerson);
    target.CsePersonsWorkSet.Assign(source.CsePersonsWorkSet);
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Name = source.Name;
    target.JudicialDistrict = source.JudicialDistrict;
    target.JudicialDivision = source.JudicialDivision;
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

  private void UseLeCabRelatedCseCasesFLact()
  {
    var useImport = new LeCabRelatedCseCasesFLact.Import();
    var useExport = new LeCabRelatedCseCasesFLact.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeCabRelatedCseCasesFLact.Execute, useImport, useExport);

    useExport.RelatedCseCases.CopyTo(
      export.InputCseCases, MoveRelatedCseCasesToInputCseCases);
  }

  private void UseLeDeleteLegalRoleAndPerson()
  {
    var useImport = new LeDeleteLegalRoleAndPerson.Import();
    var useExport = new LeDeleteLegalRoleAndPerson.Export();

    useImport.Zdel.Identifier = export.LegalAction.Identifier;
    useImport.Case1.Number =
      export.LegalActionPerson.Item.DetailDisplayed.Number;
    MoveCaseRole(export.LegalActionPerson.Item.CaseRole, useImport.CaseRole);
    useImport.LegalActionPerson.Identifier =
      export.LegalActionPerson.Item.LegalActionPerson1.Identifier;
    useImport.CsePersonsWorkSet.Number =
      export.LegalActionPerson.Item.CsePersonsWorkSet.Number;

    Call(LeDeleteLegalRoleAndPerson.Execute, useImport, useExport);
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = export.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.LactActionTaken.Description = useExport.CodeValue.Description;
  }

  private void UseLeLrolCheckForPaternity()
  {
    var useImport = new LeLrolCheckForPaternity.Import();
    var useExport = new LeLrolCheckForPaternity.Export();

    export.LegalActionPerson.CopyTo(
      useImport.LegalActionPerson, MoveLegalActionPerson);
    export.PrevCpatCases.CopyTo(useImport.ExpPrevCpatCases, MovePrevCpatCases);

    Call(LeLrolCheckForPaternity.Execute, useImport, useExport);

    useImport.ExpPrevCpatCases.
      CopyTo(export.PrevCpatCases, MoveExpPrevCpatCases);
    export.Cpat.Number = useExport.Cpat.Number;
  }

  private void UseLeLrolListLegActPersonsV2()
  {
    var useImport = new LeLrolListLegActPersonsV2.Import();
    var useExport = new LeLrolListLegActPersonsV2.Export();

    useImport.LegalAction.Assign(export.LegalAction);
    export.InputCseCases.CopyTo(useImport.CseCases, MoveInputCseCases);

    Call(LeLrolListLegActPersonsV2.Execute, useImport, useExport);

    MoveLegalAction(useExport.LegalAction, export.LegalAction);
    useExport.Role.
      CopyTo(export.LegalActionPerson, MoveRoleToLegalActionPerson1);
    useExport.ListEndReason.CopyTo(
      export.ListEndRsn, MoveListEndReasonToListEndRsn1);
  }

  private void UseLeLrolListOnlyRelvntCroles()
  {
    var useImport = new LeLrolListOnlyRelvntCroles.Import();
    var useExport = new LeLrolListOnlyRelvntCroles.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    export.InputCseCases.CopyTo(
      useImport.CseCasesList, MoveInputCseCasesToCseCasesList);

    Call(LeLrolListOnlyRelvntCroles.Execute, useImport, useExport);

    export.Tribunal.Assign(useExport.Tribunal);
    export.Fips.Assign(useExport.Fips);
    MoveLegalAction(useExport.LegalAction, export.LegalAction);
    useExport.Role.
      CopyTo(export.LegalActionPerson, MoveRoleToLegalActionPerson2);
    useExport.ListEndReason.CopyTo(
      export.ListEndRsn, MoveListEndReasonToListEndRsn2);
  }

  private void UseLeLrolUpdateLegalActionPers()
  {
    var useImport = new LeLrolUpdateLegalActionPers.Import();
    var useExport = new LeLrolUpdateLegalActionPers.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Case1.Number =
      export.LegalActionPerson.Item.DetailDisplayed.Number;
    MoveCaseRole(export.LegalActionPerson.Item.CaseRole, useImport.CaseRole);
    useImport.LegalActionPerson.Assign(
      export.LegalActionPerson.Item.LegalActionPerson1);
    useImport.CsePersonsWorkSet.Number =
      export.LegalActionPerson.Item.CsePersonsWorkSet.Number;

    Call(LeLrolUpdateLegalActionPers.Execute, useImport, useExport);

    export.LegalActionPerson.Update.LegalActionPerson1.Assign(
      useExport.LegalActionPerson);
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

    useImport.CsePersonsWorkSet.Number = import.SelectedNonCase.Number;
    useImport.Case1.Number = local.SecurityTest.Number;
    useImport.LegalAction.Assign(export.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.InputCseCases.Item.DetailInput.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          export.LegalActionPerson.Item.CsePersonsWorkSet.Number);
        db.SetString(
          command, "casNum",
          export.LegalActionPerson.Item.DetailDisplayed.Number);
        db.SetDate(
          command, "effectiveDt", local.CurrentDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaRIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Type1 = db.GetString(reader, 2);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
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
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
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

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
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
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    entities.LegalActionPerson1.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson1.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson1.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson1.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson1.Role = db.GetString(reader, 3);
        entities.LegalActionPerson1.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson1.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson1.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson1.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson1.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.LegalActionPerson1.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson1.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson1.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson1.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson1.Role = db.GetString(reader, 3);
        entities.LegalActionPerson1.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson1.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson1.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson1.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson1.Populated = true;
      });
  }

  private bool ReadLegalActionPerson3()
  {
    entities.LegalActionPerson1.Populated = false;

    return Read("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson1.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson1.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson1.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson1.Role = db.GetString(reader, 3);
        entities.LegalActionPerson1.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson1.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson1.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson1.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.LegalAction.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 9);
        entities.Tribunal.Name = db.GetString(reader, 10);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.LegalAction.Populated = true;
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalFips()
  {
    entities.LegalAction.Populated = false;
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunalFips",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 9);
        entities.Tribunal.Name = db.GetString(reader, 10);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.Fips.Location = db.GetInt32(reader, 11);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.Fips.County = db.GetInt32(reader, 13);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.Fips.State = db.GetInt32(reader, 14);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 15);
        entities.Fips.StateAbbreviation = db.GetString(reader, 16);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 17);
        entities.LegalAction.Populated = true;
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;

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
    /// <summary>A CseCasesGroup group.</summary>
    [Serializable]
    public class CseCasesGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Case1 Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 detail;
    }

    /// <summary>A LegalActionPersonGroup group.</summary>
    [Serializable]
    public class LegalActionPersonGroup
    {
      /// <summary>
      /// A value of DetailDisplayed.
      /// </summary>
      [JsonPropertyName("detailDisplayed")]
      public Case1 DetailDisplayed
      {
        get => detailDisplayed ??= new();
        set => detailDisplayed = value;
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
      /// A value of CaseRole.
      /// </summary>
      [JsonPropertyName("caseRole")]
      public CaseRole CaseRole
      {
        get => caseRole ??= new();
        set => caseRole = value;
      }

      /// <summary>
      /// A value of LegalActionPerson1.
      /// </summary>
      [JsonPropertyName("legalActionPerson1")]
      public LegalActionPerson LegalActionPerson1
      {
        get => legalActionPerson1 ??= new();
        set => legalActionPerson1 = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private Case1 detailDisplayed;
      private Common common;
      private CaseRole caseRole;
      private LegalActionPerson legalActionPerson1;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>A ListEndReasonGroup group.</summary>
    [Serializable]
    public class ListEndReasonGroup
    {
      /// <summary>
      /// A value of DetailListEndRsn.
      /// </summary>
      [JsonPropertyName("detailListEndRsn")]
      public Standard DetailListEndRsn
      {
        get => detailListEndRsn ??= new();
        set => detailListEndRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private Standard detailListEndRsn;
    }

    /// <summary>A CseCases2Group group.</summary>
    [Serializable]
    public class CseCases2Group
    {
      /// <summary>
      /// A value of DetailInput2.
      /// </summary>
      [JsonPropertyName("detailInput2")]
      public Case1 DetailInput2
      {
        get => detailInput2 ??= new();
        set => detailInput2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 detailInput2;
    }

    /// <summary>A PrevCpatCasesGroup group.</summary>
    [Serializable]
    public class PrevCpatCasesGroup
    {
      /// <summary>
      /// A value of GimpCpat.
      /// </summary>
      [JsonPropertyName("gimpCpat")]
      public Case1 GimpCpat
      {
        get => gimpCpat ??= new();
        set => gimpCpat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 gimpCpat;
    }

    /// <summary>
    /// A value of SelectedNonCase.
    /// </summary>
    [JsonPropertyName("selectedNonCase")]
    public CsePersonsWorkSet SelectedNonCase
    {
      get => selectedNonCase ??= new();
      set => selectedNonCase = value;
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
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of PromptClass.
    /// </summary>
    [JsonPropertyName("promptClass")]
    public Common PromptClass
    {
      get => promptClass ??= new();
      set => promptClass = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// Gets a value of CseCases.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesGroup> CseCases => cseCases ??= new(
      CseCasesGroup.Capacity);

    /// <summary>
    /// Gets a value of CseCases for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases")]
    [Computed]
    public IList<CseCasesGroup> CseCases_Json
    {
      get => cseCases;
      set => CseCases.Assign(value);
    }

    /// <summary>
    /// Gets a value of LegalActionPerson.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionPersonGroup> LegalActionPerson =>
      legalActionPerson ??= new(LegalActionPersonGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActionPerson for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    [Computed]
    public IList<LegalActionPersonGroup> LegalActionPerson_Json
    {
      get => legalActionPerson;
      set => LegalActionPerson.Assign(value);
    }

    /// <summary>
    /// Gets a value of ListEndReason.
    /// </summary>
    [JsonIgnore]
    public Array<ListEndReasonGroup> ListEndReason => listEndReason ??= new(
      ListEndReasonGroup.Capacity);

    /// <summary>
    /// Gets a value of ListEndReason for json serialization.
    /// </summary>
    [JsonPropertyName("listEndReason")]
    [Computed]
    public IList<ListEndReasonGroup> ListEndReason_Json
    {
      get => listEndReason;
      set => ListEndReason.Assign(value);
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
    /// A value of DlgflwSelectedLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedLegalAction")]
    public LegalAction DlgflwSelectedLegalAction
    {
      get => dlgflwSelectedLegalAction ??= new();
      set => dlgflwSelectedLegalAction = value;
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
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of LactActionTaken.
    /// </summary>
    [JsonPropertyName("lactActionTaken")]
    public CodeValue LactActionTaken
    {
      get => lactActionTaken ??= new();
      set => lactActionTaken = value;
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of Cpat.
    /// </summary>
    [JsonPropertyName("cpat")]
    public Case1 Cpat
    {
      get => cpat ??= new();
      set => cpat = value;
    }

    /// <summary>
    /// A value of LegalActionFlow.
    /// </summary>
    [JsonPropertyName("legalActionFlow")]
    public Common LegalActionFlow
    {
      get => legalActionFlow ??= new();
      set => legalActionFlow = value;
    }

    /// <summary>
    /// Gets a value of CseCases2.
    /// </summary>
    [JsonIgnore]
    public Array<CseCases2Group> CseCases2 => cseCases2 ??= new(
      CseCases2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of CseCases2 for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases2")]
    [Computed]
    public IList<CseCases2Group> CseCases2_Json
    {
      get => cseCases2;
      set => CseCases2.Assign(value);
    }

    /// <summary>
    /// A value of PrevH.
    /// </summary>
    [JsonPropertyName("prevH")]
    public Common PrevH
    {
      get => prevH ??= new();
      set => prevH = value;
    }

    /// <summary>
    /// A value of EiwoSelection.
    /// </summary>
    [JsonPropertyName("eiwoSelection")]
    public Common EiwoSelection
    {
      get => eiwoSelection ??= new();
      set => eiwoSelection = value;
    }

    /// <summary>
    /// Gets a value of PrevCpatCases.
    /// </summary>
    [JsonIgnore]
    public Array<PrevCpatCasesGroup> PrevCpatCases => prevCpatCases ??= new(
      PrevCpatCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrevCpatCases for json serialization.
    /// </summary>
    [JsonPropertyName("prevCpatCases")]
    [Computed]
    public IList<PrevCpatCasesGroup> PrevCpatCases_Json
    {
      get => prevCpatCases;
      set => PrevCpatCases.Assign(value);
    }

    private CsePersonsWorkSet selectedNonCase;
    private FipsTribAddress fipsTribAddress;
    private Standard promptTribunal;
    private Tribunal tribunal;
    private Fips fips;
    private Standard standard;
    private LegalAction legalAction;
    private Common promptClass;
    private LegalAction hiddenLegalAction;
    private Array<CseCasesGroup> cseCases;
    private Array<LegalActionPersonGroup> legalActionPerson;
    private Array<ListEndReasonGroup> listEndReason;
    private CodeValue dlgflwSelectedCodeValue;
    private LegalAction dlgflwSelectedLegalAction;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
    private Common display;
    private CodeValue lactActionTaken;
    private Fips hiddenFips;
    private Case1 cpat;
    private Common legalActionFlow;
    private Array<CseCases2Group> cseCases2;
    private Common prevH;
    private Common eiwoSelection;
    private Array<PrevCpatCasesGroup> prevCpatCases;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A InputCseCasesGroup group.</summary>
    [Serializable]
    public class InputCseCasesGroup
    {
      /// <summary>
      /// A value of DetailInput.
      /// </summary>
      [JsonPropertyName("detailInput")]
      public Case1 DetailInput
      {
        get => detailInput ??= new();
        set => detailInput = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 detailInput;
    }

    /// <summary>A LegalActionPersonGroup group.</summary>
    [Serializable]
    public class LegalActionPersonGroup
    {
      /// <summary>
      /// A value of DetailDisplayed.
      /// </summary>
      [JsonPropertyName("detailDisplayed")]
      public Case1 DetailDisplayed
      {
        get => detailDisplayed ??= new();
        set => detailDisplayed = value;
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
      /// A value of CaseRole.
      /// </summary>
      [JsonPropertyName("caseRole")]
      public CaseRole CaseRole
      {
        get => caseRole ??= new();
        set => caseRole = value;
      }

      /// <summary>
      /// A value of LegalActionPerson1.
      /// </summary>
      [JsonPropertyName("legalActionPerson1")]
      public LegalActionPerson LegalActionPerson1
      {
        get => legalActionPerson1 ??= new();
        set => legalActionPerson1 = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private Case1 detailDisplayed;
      private Common common;
      private CaseRole caseRole;
      private LegalActionPerson legalActionPerson1;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>A ListEndRsnGroup group.</summary>
    [Serializable]
    public class ListEndRsnGroup
    {
      /// <summary>
      /// A value of DetailListEndRsn.
      /// </summary>
      [JsonPropertyName("detailListEndRsn")]
      public Standard DetailListEndRsn
      {
        get => detailListEndRsn ??= new();
        set => detailListEndRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private Standard detailListEndRsn;
    }

    /// <summary>A PrevCpatCasesGroup group.</summary>
    [Serializable]
    public class PrevCpatCasesGroup
    {
      /// <summary>
      /// A value of GexpCpat.
      /// </summary>
      [JsonPropertyName("gexpCpat")]
      public Case1 GexpCpat
      {
        get => gexpCpat ??= new();
        set => gexpCpat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 gexpCpat;
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
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of PromptClass.
    /// </summary>
    [JsonPropertyName("promptClass")]
    public Common PromptClass
    {
      get => promptClass ??= new();
      set => promptClass = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// Gets a value of InputCseCases.
    /// </summary>
    [JsonIgnore]
    public Array<InputCseCasesGroup> InputCseCases => inputCseCases ??= new(
      InputCseCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of InputCseCases for json serialization.
    /// </summary>
    [JsonPropertyName("inputCseCases")]
    [Computed]
    public IList<InputCseCasesGroup> InputCseCases_Json
    {
      get => inputCseCases;
      set => InputCseCases.Assign(value);
    }

    /// <summary>
    /// Gets a value of LegalActionPerson.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionPersonGroup> LegalActionPerson =>
      legalActionPerson ??= new(LegalActionPersonGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActionPerson for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    [Computed]
    public IList<LegalActionPersonGroup> LegalActionPerson_Json
    {
      get => legalActionPerson;
      set => LegalActionPerson.Assign(value);
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
    /// A value of DlgflwSelectedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedLegalActionPerson")]
    public LegalActionPerson DlgflwSelectedLegalActionPerson
    {
      get => dlgflwSelectedLegalActionPerson ??= new();
      set => dlgflwSelectedLegalActionPerson = value;
    }

    /// <summary>
    /// Gets a value of ListEndRsn.
    /// </summary>
    [JsonIgnore]
    public Array<ListEndRsnGroup> ListEndRsn => listEndRsn ??= new(
      ListEndRsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ListEndRsn for json serialization.
    /// </summary>
    [JsonPropertyName("listEndRsn")]
    [Computed]
    public IList<ListEndRsnGroup> ListEndRsn_Json
    {
      get => listEndRsn;
      set => ListEndRsn.Assign(value);
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
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of LactActionTaken.
    /// </summary>
    [JsonPropertyName("lactActionTaken")]
    public CodeValue LactActionTaken
    {
      get => lactActionTaken ??= new();
      set => lactActionTaken = value;
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of FromLrol.
    /// </summary>
    [JsonPropertyName("fromLrol")]
    public Common FromLrol
    {
      get => fromLrol ??= new();
      set => fromLrol = value;
    }

    /// <summary>
    /// A value of Cpat.
    /// </summary>
    [JsonPropertyName("cpat")]
    public Case1 Cpat
    {
      get => cpat ??= new();
      set => cpat = value;
    }

    /// <summary>
    /// A value of LegalActionFlow.
    /// </summary>
    [JsonPropertyName("legalActionFlow")]
    public Common LegalActionFlow
    {
      get => legalActionFlow ??= new();
      set => legalActionFlow = value;
    }

    /// <summary>
    /// A value of LegalActionFlowToCpat.
    /// </summary>
    [JsonPropertyName("legalActionFlowToCpat")]
    public Common LegalActionFlowToCpat
    {
      get => legalActionFlowToCpat ??= new();
      set => legalActionFlowToCpat = value;
    }

    /// <summary>
    /// A value of PrevH.
    /// </summary>
    [JsonPropertyName("prevH")]
    public Common PrevH
    {
      get => prevH ??= new();
      set => prevH = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCaseRole.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCaseRole")]
    public CaseRole DlgflwSelectedCaseRole
    {
      get => dlgflwSelectedCaseRole ??= new();
      set => dlgflwSelectedCaseRole = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCsePerson.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCsePerson")]
    public CsePerson DlgflwSelectedCsePerson
    {
      get => dlgflwSelectedCsePerson ??= new();
      set => dlgflwSelectedCsePerson = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCase.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCase")]
    public Case1 DlgflwSelectedCase
    {
      get => dlgflwSelectedCase ??= new();
      set => dlgflwSelectedCase = value;
    }

    /// <summary>
    /// A value of EiwoSelection.
    /// </summary>
    [JsonPropertyName("eiwoSelection")]
    public Common EiwoSelection
    {
      get => eiwoSelection ??= new();
      set => eiwoSelection = value;
    }

    /// <summary>
    /// Gets a value of PrevCpatCases.
    /// </summary>
    [JsonIgnore]
    public Array<PrevCpatCasesGroup> PrevCpatCases => prevCpatCases ??= new(
      PrevCpatCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrevCpatCases for json serialization.
    /// </summary>
    [JsonPropertyName("prevCpatCases")]
    [Computed]
    public IList<PrevCpatCasesGroup> PrevCpatCases_Json
    {
      get => prevCpatCases;
      set => PrevCpatCases.Assign(value);
    }

    private FipsTribAddress fipsTribAddress;
    private Standard promptTribunal;
    private Tribunal tribunal;
    private Fips fips;
    private Standard standard;
    private LegalAction legalAction;
    private Common promptClass;
    private LegalAction hiddenLegalAction;
    private Array<InputCseCasesGroup> inputCseCases;
    private Array<LegalActionPersonGroup> legalActionPerson;
    private Common displayActiveCasesOnly;
    private Code code;
    private LegalActionPerson dlgflwSelectedLegalActionPerson;
    private Array<ListEndRsnGroup> listEndRsn;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
    private Common display;
    private CodeValue lactActionTaken;
    private Fips hiddenFips;
    private Common fromLrol;
    private Case1 cpat;
    private Common legalActionFlow;
    private Common legalActionFlowToCpat;
    private Common prevH;
    private CaseRole dlgflwSelectedCaseRole;
    private CsePerson dlgflwSelectedCsePerson;
    private Case1 dlgflwSelectedCase;
    private Common eiwoSelection;
    private Array<PrevCpatCasesGroup> prevCpatCases;
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
    /// A value of ChildFound.
    /// </summary>
    [JsonPropertyName("childFound")]
    public Common ChildFound
    {
      get => childFound ??= new();
      set => childFound = value;
    }

    /// <summary>
    /// A value of RespondentFound.
    /// </summary>
    [JsonPropertyName("respondentFound")]
    public Common RespondentFound
    {
      get => respondentFound ??= new();
      set => respondentFound = value;
    }

    /// <summary>
    /// A value of PetitionerFound.
    /// </summary>
    [JsonPropertyName("petitionerFound")]
    public Common PetitionerFound
    {
      get => petitionerFound ??= new();
      set => petitionerFound = value;
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
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of AddCommand.
    /// </summary>
    [JsonPropertyName("addCommand")]
    public Common AddCommand
    {
      get => addCommand ??= new();
      set => addCommand = value;
    }

    /// <summary>
    /// A value of CheckEmpty.
    /// </summary>
    [JsonPropertyName("checkEmpty")]
    public Common CheckEmpty
    {
      get => checkEmpty ??= new();
      set => checkEmpty = value;
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
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public Tribunal InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
    }

    /// <summary>
    /// A value of GrpEntry.
    /// </summary>
    [JsonPropertyName("grpEntry")]
    public Common GrpEntry
    {
      get => grpEntry ??= new();
      set => grpEntry = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public LegalActionPerson Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
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
    /// A value of TotalSelected.
    /// </summary>
    [JsonPropertyName("totalSelected")]
    public Common TotalSelected
    {
      get => totalSelected ??= new();
      set => totalSelected = value;
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
    /// A value of NoOfLegalActionsFound.
    /// </summary>
    [JsonPropertyName("noOfLegalActionsFound")]
    public Common NoOfLegalActionsFound
    {
      get => noOfLegalActionsFound ??= new();
      set => noOfLegalActionsFound = value;
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
    /// A value of Event95.
    /// </summary>
    [JsonPropertyName("event95")]
    public Common Event95
    {
      get => event95 ??= new();
      set => event95 = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of SecurityTest.
    /// </summary>
    [JsonPropertyName("securityTest")]
    public Case1 SecurityTest
    {
      get => securityTest ??= new();
      set => securityTest = value;
    }

    private DateWorkArea null1;
    private Common childFound;
    private Common respondentFound;
    private Common petitionerFound;
    private Common promptCount;
    private DateWorkArea currentDate;
    private Common dummy;
    private Common addCommand;
    private Common checkEmpty;
    private TextWorkArea textWorkArea;
    private Tribunal initialisedToSpaces;
    private Common grpEntry;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common common;
    private LegalActionPerson initialize;
    private LegalAction legalAction;
    private Common totalSelected;
    private DateWorkArea max;
    private Common noOfLegalActionsFound;
    private NextTranInfo nextTranInfo;
    private Common event95;
    private LegalAction previous;
    private Case1 securityTest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson1.
    /// </summary>
    [JsonPropertyName("legalActionPerson1")]
    public LegalActionPerson LegalActionPerson1
    {
      get => legalActionPerson1 ??= new();
      set => legalActionPerson1 = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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

    private LegalActionDetail legalActionDetail;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson1;
    private CsePerson csePerson;
    private FipsTribAddress fipsTribAddress;
    private Case1 case1;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
  }
#endregion
}

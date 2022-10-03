// Program: LE_LAPP_LEGAL_APPEAL, ID: 371973532, model: 746.
// Short name: SWELAPPP
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
/// A program: LE_LAPP_LEGAL_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step action block maintains legal appeal
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLappLegalAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAPP_LEGAL_APPEAL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLappLegalAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLappLegalAppeal.
  /// </summary>
  public LeLappLegalAppeal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------
    // Date		Developer	Request #
    // Description
    // 09-06-95        Govind
    // Initial development
    // 01/03/96	T.O.Redmond
    // Add Security and Next Tran
    // 10/24/97	govind
    // Fixed to handle Identifier as a random number
    // 12/11-17/98	P McElderry	None listed
    // Enhanced: READ statements, program structure, and error logic.
    // Added view on dialog flow returning from LACN for L.A. CCN.
    // Fixed scrolling and PF4 help.
    // Made initial changes commiserate w/KESSEP test
    // standards.
    // 12/27/98		P McElderry
    // Deleted unused views.
    // Made final changes commiserate w/KESSEP test standards.
    // 4/3/02 		K. Cole		PR138221		Using new cab to retrieve the action taken 
    // description for the legal action.
    // ----------------------------------------------------------
    // -------------------------------------------------------------
    // 01-02-1996  List Legal Actions Prompt does not appear on
    // this screen. At this time the Make statement further in this
    // code is a note. If this prompt is not required it should be
    // removed from import/export and procedure logic.
    // --------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    local.HighlightError.Flag = "N";

    // --------------------------
    // Move imports to exports
    // --------------------------
    export.Debug.Flag = import.Debug.Flag;
    export.ListTribunalsTo.PromptField = import.ListTribunalsTo.PromptField;
    MoveLegalAction1(import.LegalAction, export.LegalAction);
    export.Fips.Assign(import.Fips);
    export.ListTribunalFrom.PromptField = import.ListTribunalFrom.PromptField;
    MoveAppeal1(import.Appeal, export.Appeal);
    export.FromTribunal.Assign(import.FromTribunal);
    export.ToTribunal.Assign(import.ToTribunal);
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    MoveLegalAction3(import.HiddenPrevLegalAction, export.HiddenPrevLegalAction);
      
    export.HiddenPrevAppeal.Identifier = import.HiddenPrevAppeal.Identifier;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenFips.Assign(import.HiddenFips);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenTribunal.Identifier = import.HiddenTribunal.Identifier;
    export.FromFipsTribAddress.Country = import.FromFipsTribAddress.Country;
    export.ToFipsTribAddress.Country = import.ToFipsTribAddress.Country;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.ActionTaken.Description = import.ActionTaken.Description;
    export.DataExists.Flag = import.DataExists.Flag;

    if (Equal(global.Command, "LACN"))
    {
      export.HiddenPrevUserAction.Command = "DISPLAY";
      ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    if (Equal(global.Command, "RETLTRB"))
    {
      if (AsChar(export.ListTribunalFrom.PromptField) == 'S')
      {
        export.FromTribunal.Assign(import.DlgflwSelectedTribunal);
        export.Fips.Assign(import.DlgflwSelectedFips);
        export.FromFipsTribAddress.Country = import.DlgflowSelected.Country;
        export.ListTribunalFrom.PromptField = "+";

        var field = GetField(export.Appeal, "docketNumber");

        field.Protected = false;
        field.Focused = true;

        return;
      }
      else if (AsChar(export.ListTribunalsTo.PromptField) == 'S')
      {
        export.ToTribunal.Assign(import.DlgflwSelectedTribunal);
        export.ToFipsTribAddress.Country = import.DlgflowSelected.Country;
        export.ListTribunalsTo.PromptField = "+";

        var field = GetField(export.Appeal, "filedByLastName");

        field.Protected = false;
        field.Focused = true;

        return;
      }
      else
      {
        var field = GetField(export.Appeal, "filedByLastName");

        field.Protected = false;
        field.Focused = true;

        return;
      }
    }

    // ------------------------
    // Nextran / security logic
    // ------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ------------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // -------------------------------------------------------------
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }
      else
      {
        // -------------------
        // Continue processing
        // -------------------
      }

      if (ReadLegalAction1())
      {
        if (ReadTribunal())
        {
          export.FromTribunal.Assign(entities.Tribunal);

          if (ReadFips2())
          {
            export.Fips.Assign(entities.Fips);
          }
          else if (ReadFipsTribAddress())
          {
            export.FromFipsTribAddress.Country = entities.Foreign.Country;
          }
          else
          {
            ExitState = "LE0000_TRIBUNAL_ADDRESS_NF";
          }
        }
        else
        {
          ExitState = "TRIBUNAL_NF";
        }
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // ---------------------------------------------------------------
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      UseScCabNextTranPut();

      return;
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETLACN") || Equal
      (global.Command, "RETLTRB"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
      else
      {
        // -------------------
        // Continue processing
        // -------------------
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        if (IsEmpty(export.Fips.CountyAbbreviation) || IsEmpty
          (export.Fips.StateAbbreviation))
        {
          if (IsEmpty(export.FromFipsTribAddress.Country))
          {
            var field = GetField(export.FromFipsTribAddress, "country");

            field.Error = true;
          }
          else
          {
            goto Test1;
          }

          var field1 = GetField(export.Fips, "stateAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "countyAbbreviation");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (!IsEmpty(export.FromFipsTribAddress.Country))
        {
          var field1 = GetField(export.FromFipsTribAddress, "country");

          field1.Error = true;

          var field2 = GetField(export.Fips, "stateAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.Fips, "countyAbbreviation");

          field3.Error = true;

          ExitState = "ACO_NI0000_CLEAR_SCREEN_TO_DISP";
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

Test1:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "BYPASS";
        }
        else
        {
          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenPrevLegalAction.CourtCaseNumber) && export
            .FromTribunal.Identifier == 0)
          {
            export.FromTribunal.Identifier = 0;
          }
          else
          {
            // ------------------
            // Continue processing
            // ------------------
          }

          if ((!Equal(
            export.Fips.CountyAbbreviation,
            export.HiddenFips.CountyAbbreviation) || !
            Equal(export.Fips.StateAbbreviation,
            export.HiddenFips.StateAbbreviation)) && IsEmpty
            (export.FromFipsTribAddress.Country))
          {
            export.FromTribunal.Identifier = 0;

            if (ReadFips1())
            {
              export.Fips.Assign(entities.Fips);
            }
            else
            {
              var field1 = GetField(export.Fips, "stateAbbreviation");

              field1.Error = true;

              var field2 = GetField(export.Fips, "countyAbbreviation");

              field2.Error = true;

              ExitState = "INVALID_FIPS_STATE_COUNTY_LOCN";
              global.Command = "BYPASS";
            }
          }
          else
          {
            // ------------------
            // Continue processing
            // ------------------
          }
        }

        if (Equal(global.Command, "BYPASS"))
        {
          export.Appeal.Identifier = 0;
          export.HiddenPrevAppeal.Identifier = 0;
          export.ActionTaken.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.Appeal.DocketNumber = "";
          export.Appeal.DocketingStmtFiledDate = null;
          export.ToTribunal.Name = "";
          export.ToTribunal.JudicialDistrict = "";
          export.ToTribunal.JudicialDivision = "";
          export.ToFipsTribAddress.Country = "";
          export.Appeal.FiledByLastName = "";
          export.Appeal.FiledByFirstName = "";
          export.Appeal.FiledByMi = "";
          export.Appeal.AttorneyLastName = "";
          export.Appeal.AttorneyFirstName = "";
          export.Appeal.AttorneyMiddleInitial = "";
          export.Appeal.AppealDate = null;
          export.Appeal.ExtentionReqGrantedDate = null;
          export.Appeal.AppellantBriefDate = null;
          export.Appeal.OralArgumentDate = null;
          export.Appeal.DecisionDate = null;
          export.Appeal.DecisionResult =
            Spaces(Appeal.DecisionResult_MaxLength);
          export.Appeal.FurtherAppealIndicator = "";
          export.DataExists.Flag = "";

          return;
        }
        else
        {
          // ------------------
          // Continue processing
          // ------------------
        }

        if (Equal(export.HiddenPrevUserAction.Command, "DELETE"))
        {
          goto Test3;
        }
        else
        {
          // ------------------
          // Continue processing
          // ------------------
        }

        if (Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) && export
          .FromTribunal.Identifier == export.HiddenTribunal.Identifier && Equal
          (export.Fips.CountyAbbreviation, export.HiddenFips.CountyAbbreviation) &&
          Equal
          (export.Fips.StateAbbreviation, export.HiddenFips.StateAbbreviation) &&
          Equal
          (export.FromFipsTribAddress.Country,
          export.HiddenFipsTribAddress.Country) && export.Appeal.Identifier != 0
          )
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

          return;
        }
        else
        {
          // --------------------------------------------------------------
          // USER to display information if only one record is available;
          // otherwise it will have to be specifically chosen
          // ---------------------------------------------------------------
          local.NoOfLegalActions.Count = 0;

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            if (export.FromTribunal.Identifier == 0)
            {
              foreach(var item in ReadTribunalLegalAction())
              {
                ++local.NoOfLegalActions.Count;

                if (local.NoOfLegalActions.Count > 1)
                {
                  export.HiddenPrevUserAction.Command = global.Command;
                  export.LegalAction.Classification = "J";
                  ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                  return;
                }
                else
                {
                  // ------------------
                  // Continue processing
                  // ------------------
                }

                MoveLegalAction1(entities.ExistingLegalAction,
                  export.LegalAction);
                export.FromTribunal.Assign(entities.Tribunal);
              }
            }
            else
            {
              foreach(var item in ReadLegalAction2())
              {
                ++local.NoOfLegalActions.Count;

                if (local.NoOfLegalActions.Count > 1)
                {
                  export.HiddenPrevUserAction.Command = global.Command;
                  export.LegalAction.Classification = "J";
                  ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                  return;
                }
                else
                {
                  // ------------------
                  // Continue processing
                  // ------------------
                }

                MoveLegalAction1(entities.ExistingLegalAction,
                  export.LegalAction);
              }
            }
          }
          else
          {
            foreach(var item in ReadLegalAction3())
            {
              ++local.NoOfLegalActions.Count;

              if (local.NoOfLegalActions.Count > 1)
              {
                export.HiddenPrevUserAction.Command = global.Command;
                export.LegalAction.Classification = "J";
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }
              else
              {
                // ------------------
                // Continue procesing
                // ------------------
              }

              MoveLegalAction1(entities.ExistingLegalAction, export.LegalAction);
                
            }
          }

          if (local.NoOfLegalActions.Count == 0)
          {
            global.Command = "BYPASS";

            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Protected = false;
            field1.Focused = true;

            var field2 = GetField(export.LegalAction, "courtCaseNumber");

            field2.Error = true;

            ExitState = "LEGAL_ACTION_NF";

            return;
          }
          else
          {
            // ------------------
            // Continue procesing
            // ------------------
          }
        }
      }
      else
      {
        if (Equal(global.Command, "ADD"))
        {
          if (export.LegalAction.Identifier != export
            .HiddenPrevLegalAction.Identifier || export
            .LegalAction.Identifier == 0)
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LE0000_DISPLAY_BEFORE_ADD";
            global.Command = "BYPASS";

            goto Test3;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          // ----------------------------------
          // Ensure key fields have not changed
          // ----------------------------------
          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenPrevLegalAction.CourtCaseNumber) || IsEmpty
            (export.LegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            global.Command = "BYPASS";
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          // ----------------------------------
          // Edit mandatory field requirements
          // ----------------------------------
        }
        else if (Equal(global.Command, "UPDATE"))
        {
          if (export.Appeal.Identifier > 0)
          {
            if (export.LegalAction.Identifier != export
              .HiddenPrevLegalAction.Identifier)
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
              global.Command = "BYPASS";

              goto Test2;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            // ----------------------------------
            // Ensure key fields have not changed
            // ----------------------------------
            if (!Equal(export.LegalAction.CourtCaseNumber,
              export.HiddenPrevLegalAction.CourtCaseNumber))
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              global.Command = "BYPASS";
              ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            if (!Equal(export.Fips.StateAbbreviation,
              export.HiddenFips.StateAbbreviation))
            {
              var field = GetField(export.Fips, "stateAbbreviation");

              field.Error = true;

              global.Command = "BYPASS";
              ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            if (!Equal(export.Fips.CountyAbbreviation,
              export.HiddenFips.CountyAbbreviation))
            {
              var field = GetField(export.Fips, "countyAbbreviation");

              field.Error = true;

              global.Command = "BYPASS";
              ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
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
            global.Command = "BYPASS";
            ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";
          }
        }
        else if (export.Appeal.Identifier > 0)
        {
          if (export.LegalAction.Identifier != export
            .HiddenPrevLegalAction.Identifier || export
            .LegalAction.Identifier == 0)
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            global.Command = "BYPASS";
            ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

            goto Test2;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          // ----------------------------------
          // Ensure key fields have not changed
          // ----------------------------------
          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenPrevLegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            global.Command = "BYPASS";
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          if (!Equal(export.Fips.StateAbbreviation,
            export.HiddenFips.StateAbbreviation))
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;

            global.Command = "BYPASS";
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          if (!Equal(export.Fips.CountyAbbreviation,
            export.HiddenFips.CountyAbbreviation))
          {
            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;

            global.Command = "BYPASS";
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
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
          global.Command = "BYPASS";
          ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";
        }

Test2:

        if (Equal(global.Command, "BYPASS"))
        {
          export.ActionTaken.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.Appeal.DocketNumber = "";
          export.Appeal.DocketingStmtFiledDate = null;
          export.ToTribunal.Name = "";
          export.ToTribunal.JudicialDistrict = "";
          export.ToTribunal.JudicialDivision = "";
          export.ToFipsTribAddress.Country = "";
          export.Appeal.FiledByLastName = "";
          export.Appeal.FiledByFirstName = "";
          export.Appeal.FiledByMi = "";
          export.Appeal.AttorneyLastName = "";
          export.Appeal.AttorneyFirstName = "";
          export.Appeal.AttorneyMiddleInitial = "";
          export.Appeal.AppealDate = null;
          export.Appeal.ExtentionReqGrantedDate = null;
          export.Appeal.AppellantBriefDate = null;
          export.Appeal.OralArgumentDate = null;
          export.Appeal.DecisionDate = null;
          export.Appeal.DecisionResult =
            Spaces(Appeal.DecisionResult_MaxLength);
          export.Appeal.FurtherAppealIndicator = "";
          export.DataExists.Flag = "";

          return;
        }
        else
        {
          // ------------------
          // Continue processing
          // ------------------
        }
      }
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

Test3:

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETLACN") && !
      Equal(global.Command, "RETLTRB") && !Equal(global.Command, "BYPASS"))
    {
      export.ListLegalActions.PromptField = "";
      export.ListTribunalsTo.PromptField = "";
    }
    else
    {
      // ------------------
      // Continue procesing
      // ------------------
    }

    if (Equal(global.Command, "RETLACN"))
    {
      // ---------------------------------------------------------
      // Determines if USER went to LACN and returned w/out
      // selecting a record
      // ----------------------------------------------------------
      if (IsEmpty(import.LegalAction.CourtCaseNumber))
      {
        export.LegalAction.CourtCaseNumber =
          import.FromLacn.CourtCaseNumber ?? "";

        return;
      }
      else
      {
        // -------------------
        // Continue processing
        // -------------------
      }

      if (export.FromTribunal.Identifier == 0)
      {
        return;
      }
      else
      {
        local.Retlacn.Command = global.Command;
        export.ListLegalActions.PromptField = "";
        MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
        UseLeGetActionTakenDescription();

        // ------------------------------------------------------------
        // Clear the docket number. Let the system pick the appeal for
        // the selected legal action and display it. Otherwise the old
        // docket number remains and it gives a misleading message
        // that the appeal was not found.
        // -------------------------------------------------------------
        export.Appeal.DocketNumber = "";
        global.Command = "DISPLAY";
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "BYPASS":
        // -------------------
        // Continue processing
        // -------------------
        break;
      case "LIST":
        if (!IsEmpty(export.ListTribunalFrom.PromptField))
        {
          if (!IsEmpty(export.ListTribunalsTo.PromptField))
          {
            var field1 = GetField(export.ListTribunalsTo, "promptField");

            field1.Error = true;

            var field2 = GetField(export.ListTribunalFrom, "promptField");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }
          else if (AsChar(export.ListTribunalFrom.PromptField) == 'S')
          {
            export.RequiredTribunalsFor.StateAbbreviation =
              export.Fips.StateAbbreviation;
            export.RequiredTribunalsFor.StateAbbreviation =
              export.Fips.StateAbbreviation;
            ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
          }
          else
          {
            var field1 = GetField(export.ListTribunalFrom, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }
        else if (IsEmpty(export.ListTribunalsTo.PromptField))
        {
          var field1 = GetField(export.ListTribunalsTo, "promptField");

          field1.Error = true;

          var field2 = GetField(export.ListTribunalFrom, "promptField");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
        else
        {
          switch(AsChar(export.ListTribunalsTo.PromptField))
          {
            case 'S':
              export.RequiredTribunalsFor.StateAbbreviation =
                export.Fips.StateAbbreviation;
              export.RequiredTribunalsFor.StateAbbreviation =
                export.Fips.StateAbbreviation;
              ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

              break;
            case ' ':
              ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

              break;
            default:
              var field1 = GetField(export.ListTribunalsTo, "promptField");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        break;
      case "RETLTRB":
        if (AsChar(export.ListTribunalFrom.PromptField) == 'S')
        {
          export.Fips.Assign(import.DlgflwSelectedFips);
          export.HiddenFips.Assign(import.DlgflwSelectedFips);
          export.FromFipsTribAddress.Country = import.DlgflowSelected.Country;
          export.HiddenFipsTribAddress.Country = import.DlgflowSelected.Country;
          export.FromTribunal.Assign(import.DlgflwSelectedTribunal);

          var field1 = GetField(export.Appeal, "docketNumber");

          field1.Protected = false;
          field1.Focused = true;

          return;
        }
        else
        {
          // ------------------
          // Continue procesing
          // ------------------
        }

        if (AsChar(export.ListTribunalsTo.PromptField) == 'S')
        {
          export.Fips.Assign(import.DlgflwSelectedFips);
          export.ToFipsTribAddress.Country = import.DlgflowSelected.Country;
          export.ToTribunal.Assign(import.DlgflwSelectedTribunal);
        }
        else
        {
          // ------------------
          // Continue procesing
          // ------------------
        }

        var field = GetField(export.Appeal, "filedByLastName");

        field.Protected = false;
        field.Focused = true;

        return;
      case "DISPLAY":
        UseLeGetPetitionerRespondent();
        UseLeLappDisplayLegalAppealV3();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenTribunal.Identifier = export.FromTribunal.Identifier;

          if (IsExitState("APPEAL_NF"))
          {
            MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);

            if (!IsEmpty(export.ToTribunal.Name))
            {
              export.ToTribunal.Identifier = 0;
              export.ToTribunal.JudicialDistrict = "";
              export.ToTribunal.JudicialDivision = "";
              export.ToTribunal.Name = "";
            }
          }
        }
        else
        {
          MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
          export.DataExists.Flag = "Y";
          local.Errors.Index = -1;
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        UseLeGetActionTakenDescription();

        break;
      case "PREV":
        if (Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) && export
          .FromTribunal.Identifier == export.HiddenTribunal.Identifier && Equal
          (export.Fips.CountyAbbreviation, export.HiddenFips.CountyAbbreviation) &&
          Equal
          (export.Fips.StateAbbreviation, export.HiddenFips.StateAbbreviation) &&
          Equal
          (export.FromFipsTribAddress.Country,
          export.HiddenFipsTribAddress.Country))
        {
          if (IsEmpty(export.ScrollingAttributes.MinusFlag))
          {
            if (export.Appeal.Identifier == 0)
            {
              ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_BACKWARD";
            }
          }
          else
          {
            UseLeGetPetitionerRespondent();
            UseLeLappDisplayLegalAppealV1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HiddenDisplayPerformed.Flag = "N";
            }
            else
            {
              export.DataExists.Flag = "Y";
              local.Errors.Index = -1;
              export.HiddenDisplayPerformed.Flag = "Y";
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
        }
        else
        {
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        break;
      case "NEXT":
        if (Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) && export
          .FromTribunal.Identifier == export.HiddenTribunal.Identifier && Equal
          (export.Fips.CountyAbbreviation, export.HiddenFips.CountyAbbreviation) &&
          Equal
          (export.Fips.StateAbbreviation, export.HiddenFips.StateAbbreviation) &&
          Equal
          (export.FromFipsTribAddress.Country,
          export.HiddenFipsTribAddress.Country))
        {
          if (IsEmpty(export.ScrollingAttributes.PlusFlag))
          {
            if (export.Appeal.Identifier == 0)
            {
              ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
            }
            else
            {
              ExitState = "ACO_NI0000_TOP_OF_LIST";
            }
          }
          else
          {
            UseLeGetPetitionerRespondent();
            UseLeLappDisplayLegalAppealV1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HiddenDisplayPerformed.Flag = "N";
            }
            else
            {
              export.DataExists.Flag = "Y";
              local.Errors.Index = -1;
              export.HiddenDisplayPerformed.Flag = "Y";
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
        }
        else
        {
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        break;
      case "ADD":
        UseLeLappValidateLegalAppeal1();

        if (local.LastErrorEntry.Count != 0)
        {
          ExitState = "ACO_RE0000_UNKNOWN_ERROR_RB";
          local.HighlightError.Flag = "Y";

          break;
        }
        else
        {
          // ------------------
          // Continue procesing
          // ------------------
        }

        UseLeLappCreateLegalAppeal();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }
        else
        {
          if (export.Appeal.Identifier != export.HiddenPrevAppeal.Identifier)
          {
            export.ScrollingAttributes.MinusFlag = "-";
          }
          else
          {
            // -------------
            // First appeal
            // -------------
          }

          local.Errors.Index = -1;
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        UseLeLappValidateLegalAppeal1();

        if (local.LastErrorEntry.Count != 0)
        {
          ExitState = "ACO_RE0000_UNKNOWN_ERROR_RB";
          local.HighlightError.Flag = "Y";

          break;
        }
        else
        {
          // ------------------
          // Continue procesing
          // ------------------
        }

        UseLeLappUpdateLegalAppeal();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }
        else
        {
          local.Errors.Index = -1;
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        local.UserAction.Command = global.Command;
        UseLeLappValidateLegalAppeal2();

        if (local.LastErrorEntry.Count != 0)
        {
          ExitState = "ACO_RE0000_UNKNOWN_ERROR_RB";
          local.HighlightError.Flag = "Y";

          break;
        }
        else
        {
          // ------------------
          // Continue procesing
          // ------------------
        }

        UseLeLappDeleteLegalAppeal();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }
        else
        {
          local.Errors.Index = -1;
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_DELETE") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
      ("ACO_NI0000_DISPLAY_SUCCESSFUL") && local.Errors.Index == -1)
    {
      MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
      export.HiddenPrevAppeal.Identifier = export.Appeal.Identifier;
      export.HiddenFips.Assign(export.Fips);
      export.HiddenTribunal.Identifier = export.FromTribunal.Identifier;
      export.HiddenPrevUserAction.Command = global.Command;
    }
    else
    {
      if (Equal(global.Command, "PREV") || Equal(global.Command, "NEXT"))
      {
      }
      else if (AsChar(export.DataExists.Flag) == 'Y' && Equal
        (global.Command, "BYPASS"))
      {
        export.Appeal.Identifier = 0;
        export.HiddenPrevAppeal.Identifier = 0;
        export.Appeal.DocketNumber = "";
        export.Appeal.DocketingStmtFiledDate = null;
        export.ToTribunal.Name = "";
        export.ToTribunal.JudicialDistrict = "";
        export.ToTribunal.JudicialDivision = "";
        export.ToFipsTribAddress.Country = "";
        export.Appeal.FiledByLastName = "";
        export.Appeal.FiledByFirstName = "";
        export.Appeal.FiledByMi = "";
        export.Appeal.AttorneyLastName = "";
        export.Appeal.AttorneyFirstName = "";
        export.Appeal.AttorneyMiddleInitial = "";
        export.Appeal.AppealDate = null;
        export.Appeal.ExtentionReqGrantedDate = null;
        export.Appeal.AppellantBriefDate = null;
        export.Appeal.OralArgumentDate = null;
        export.Appeal.DecisionDate = null;
        export.Appeal.DecisionResult = Spaces(Appeal.DecisionResult_MaxLength);
        export.Appeal.FurtherAppealIndicator = "";
        export.DataExists.Flag = "";
      }
      else
      {
        // ---------------------------
        // No data exists to blank out
        // ---------------------------
      }

      // -----------------------------------------------------------
      // If any validation error was encountered, highlight the error
      // and display the error message.
      // ------------------------------------------------------------
      if (AsChar(local.HighlightError.Flag) == 'Y')
      {
        local.Errors.Index = local.LastErrorEntry.Count - 1;
        local.Errors.CheckSize();

        while(local.Errors.Index >= 0)
        {
          switch(local.Errors.Item.DetailErrorCode.Count)
          {
            case 1:
              var field1 = GetField(export.LegalAction, "courtCaseNumber");

              field1.Error = true;

              ExitState = "LE0000_LEGAL_ACTION_NOT_SELECTED";

              break;
            case 2:
              var field2 = GetField(export.Appeal, "docketNumber");

              field2.Error = true;

              ExitState = "LE0000_APPEAL_DOCKET_NO_REQD";

              break;
            case 3:
              var field3 = GetField(export.Appeal, "docketingStmtFiledDate");

              field3.Error = true;

              ExitState = "LE0000_INVALID_DCKT_STMT_FLD_DT";

              break;
            case 4:
              var field4 = GetField(export.ToTribunal, "name");

              field4.Error = true;

              var field5 = GetField(export.ToTribunal, "judicialDistrict");

              field5.Error = true;

              var field6 = GetField(export.ToTribunal, "judicialDivision");

              field6.Error = true;

              ExitState = "INVALID_TRIBUNAL";

              break;
            case 5:
              var field7 = GetField(export.Appeal, "filedByLastName");

              field7.Error = true;

              ExitState = "LE0000_APPEAL_FILED_BY_REQD";

              break;
            case 6:
              var field8 = GetField(export.Appeal, "attorneyLastName");

              field8.Error = true;

              ExitState = "LE0000_ATTORNEY_LAST_NAME_REQD";

              break;
            case 7:
              var field9 = GetField(export.Appeal, "attorneyFirstName");

              field9.Error = true;

              ExitState = "LE0000_ATTORNEY_FIRST_NAME_REQD";

              break;
            case 8:
              var field10 = GetField(export.Appeal, "appealDate");

              field10.Error = true;

              ExitState = "LE0000_APPEAL_DATE_REQD";

              break;
            case 9:
              var field11 = GetField(export.Appeal, "extentionReqGrantedDate");

              field11.Error = true;

              ExitState = "LE0000_INVALID_APPL_EXT_RQST_DT";

              break;
            case 10:
              var field12 = GetField(export.Appeal, "dateExtensionGranted");

              field12.Error = true;

              ExitState = "LE0000_INVALID_EXT_GRANTED_DATE";

              break;
            case 11:
              var field13 = GetField(export.Appeal, "appellantBriefDate");

              field13.Error = true;

              ExitState = "LE0000_INVALID_APPLLNT_BRIEF_DT";

              break;
            case 12:
              var field14 = GetField(export.Appeal, "replyBriefDate");

              field14.Error = true;

              ExitState = "LE0000_INVALID_REPLY_BRIEF_DATE";

              break;
            case 13:
              var field15 = GetField(export.Appeal, "oralArgumentDate");

              field15.Error = true;

              ExitState = "LE0000_INVALID_ORAL_ARG_DATE";

              break;
            case 14:
              var field16 = GetField(export.Appeal, "decisionDate");

              field16.Error = true;

              ExitState = "LE0000_INVALID_APPEAL_DECSN_DT";

              break;
            case 15:
              var field17 = GetField(export.Appeal, "furtherAppealIndicator");

              field17.Error = true;

              ExitState = "LE0000_INVALID_FURTHER_APPL_IND";

              break;
            case 16:
              var field18 = GetField(export.LegalAction, "courtCaseNumber");

              field18.Error = true;

              ExitState = "APPEAL_NF";

              break;
            case 17:
              var field19 = GetField(export.ToTribunal, "judicialDivision");

              field19.Error = true;

              ExitState = "LE0000_TRIBUNAL_DIVISION_REQD";

              break;
            case 18:
              var field20 = GetField(export.LegalAction, "courtCaseNumber");

              field20.Error = true;

              ExitState = "LEGAL_ACTION_NF";

              break;
            case 19:
              var field21 = GetField(export.LegalAction, "courtCaseNumber");

              field21.Error = true;

              ExitState = "LE0000_APPEAL_ALREADY_EXISTS";

              break;
            case 20:
              var field22 = GetField(export.Appeal, "filedByFirstName");

              field22.Error = true;

              ExitState = "LE0000_APPEAL_FILED_BY_REQD";

              break;
            case 21:
              var field23 = GetField(export.Appeal, "docketNumber");

              field23.Error = true;

              var field24 = GetField(export.Appeal, "docketingStmtFiledDate");

              field24.Error = true;

              ExitState = "LE0000_APPEAL_DOCKET_NO_REQD";

              break;
            case 22:
              var field25 = GetField(export.ToTribunal, "name");

              field25.Error = true;

              var field26 = GetField(export.ToTribunal, "judicialDistrict");

              field26.Error = true;

              var field27 = GetField(export.ToTribunal, "judicialDivision");

              field27.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";

              break;
            default:
              ExitState = "ACO_NE0000_UNKNOWN_ERROR_CODE";

              break;
          }

          --local.Errors.Index;
          local.Errors.CheckSize();
        }
      }
      else
      {
        // -------------
        // End procesing
        // -------------
      }
    }
  }

  private static void MoveAppeal1(Appeal source, Appeal target)
  {
    target.Identifier = source.Identifier;
    target.DocketNumber = source.DocketNumber;
    target.FiledByLastName = source.FiledByLastName;
    target.FiledByFirstName = source.FiledByFirstName;
    target.FiledByMi = source.FiledByMi;
    target.AppealDate = source.AppealDate;
    target.DocketingStmtFiledDate = source.DocketingStmtFiledDate;
    target.AttorneyLastName = source.AttorneyLastName;
    target.AttorneyFirstName = source.AttorneyFirstName;
    target.AttorneyMiddleInitial = source.AttorneyMiddleInitial;
    target.AppellantBriefDate = source.AppellantBriefDate;
    target.ReplyBriefDate = source.ReplyBriefDate;
    target.OralArgumentDate = source.OralArgumentDate;
    target.DecisionDate = source.DecisionDate;
    target.DecisionResult = source.DecisionResult;
    target.FurtherAppealIndicator = source.FurtherAppealIndicator;
    target.ExtentionReqGrantedDate = source.ExtentionReqGrantedDate;
    target.DateExtensionGranted = source.DateExtensionGranted;
    target.CreatedTstamp = source.CreatedTstamp;
  }

  private static void MoveAppeal2(Appeal source, Appeal target)
  {
    target.Identifier = source.Identifier;
    target.DocketNumber = source.DocketNumber;
    target.FiledByLastName = source.FiledByLastName;
    target.FiledByFirstName = source.FiledByFirstName;
    target.FiledByMi = source.FiledByMi;
    target.AppealDate = source.AppealDate;
    target.DocketingStmtFiledDate = source.DocketingStmtFiledDate;
    target.AttorneyLastName = source.AttorneyLastName;
    target.AttorneyFirstName = source.AttorneyFirstName;
    target.AttorneyMiddleInitial = source.AttorneyMiddleInitial;
    target.AppellantBriefDate = source.AppellantBriefDate;
    target.ReplyBriefDate = source.ReplyBriefDate;
    target.OralArgumentDate = source.OralArgumentDate;
    target.DecisionDate = source.DecisionDate;
    target.DecisionResult = source.DecisionResult;
    target.FurtherAppealIndicator = source.FurtherAppealIndicator;
    target.DateExtensionGranted = source.DateExtensionGranted;
  }

  private static void MoveAppeal3(Appeal source, Appeal target)
  {
    target.Identifier = source.Identifier;
    target.DocketNumber = source.DocketNumber;
    target.CreatedTstamp = source.CreatedTstamp;
  }

  private static void MoveErrors(LeLappValidateLegalAppeal.Export.
    ErrorsGroup source, Local.ErrorsGroup target)
  {
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.FiledDate = source.FiledDate;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = export.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.CodeValue.Description;
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

  private void UseLeLappCreateLegalAppeal()
  {
    var useImport = new LeLappCreateLegalAppeal.Import();
    var useExport = new LeLappCreateLegalAppeal.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.To.Identifier = export.ToTribunal.Identifier;
    useImport.Tribunal.Assign(export.FromTribunal);
    useImport.Appeal.Assign(export.Appeal);

    Call(LeLappCreateLegalAppeal.Execute, useImport, useExport);

    export.ToTribunal.Assign(useExport.Tribunal);
    export.Appeal.Identifier = useExport.Appeal.Identifier;
  }

  private void UseLeLappDeleteLegalAppeal()
  {
    var useImport = new LeLappDeleteLegalAppeal.Import();
    var useExport = new LeLappDeleteLegalAppeal.Export();

    useImport.AppealedAgainst.Identifier = export.LegalAction.Identifier;
    useImport.Appeal.Identifier = export.Appeal.Identifier;

    Call(LeLappDeleteLegalAppeal.Execute, useImport, useExport);
  }

  private void UseLeLappDisplayLegalAppealV1()
  {
    var useImport = new LeLappDisplayLegalAppealV2.Import();
    var useExport = new LeLappDisplayLegalAppealV2.Export();

    useImport.Retlacn.Command = local.Retlacn.Command;
    MoveLegalAction1(export.LegalAction, useImport.AppealedTo);
    useImport.To.Assign(export.ToTribunal);
    useImport.From.Assign(export.FromTribunal);
    MoveAppeal3(export.Appeal, useImport.Appeal);

    Call(LeLappDisplayLegalAppealV2.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.ActionTaken.Description;
    MoveLegalAction1(useExport.AppealedAgainst, export.LegalAction);
    export.ToTribunal.Assign(useExport.AppealedTo);
    export.FromTribunal.Assign(useExport.AppealedFrom);
    export.Appeal.Assign(useExport.Appeal);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
  }

  private void UseLeLappDisplayLegalAppealV3()
  {
    var useImport = new LeLappDisplayLegalAppealV2.Import();
    var useExport = new LeLappDisplayLegalAppealV2.Export();

    useImport.Retlacn.Command = local.Retlacn.Command;
    MoveLegalAction1(export.LegalAction, useImport.AppealedTo);
    useImport.To.Assign(export.ToTribunal);
    useImport.From.Assign(export.FromTribunal);
    MoveAppeal3(export.Appeal, useImport.Appeal);

    Call(LeLappDisplayLegalAppealV2.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.ActionTaken.Description;
    export.ToTribunal.Assign(useExport.AppealedTo);
    export.FromTribunal.Assign(useExport.AppealedFrom);
    export.Appeal.Assign(useExport.Appeal);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
  }

  private void UseLeLappUpdateLegalAppeal()
  {
    var useImport = new LeLappUpdateLegalAppeal.Import();
    var useExport = new LeLappUpdateLegalAppeal.Export();

    useImport.NewLegalAction.Identifier = export.LegalAction.Identifier;
    useImport.NewTribunal.Assign(export.ToTribunal);
    useImport.Appeal.Assign(export.Appeal);

    Call(LeLappUpdateLegalAppeal.Execute, useImport, useExport);

    MoveAppeal2(useExport.Appeal, export.Appeal);
  }

  private void UseLeLappValidateLegalAppeal1()
  {
    var useImport = new LeLappValidateLegalAppeal.Import();
    var useExport = new LeLappValidateLegalAppeal.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    MoveLegalAction2(export.LegalAction, useImport.LegalAction);
    useImport.To.Assign(export.ToTribunal);
    useImport.Tribunal.Assign(export.FromTribunal);
    useImport.Appeal.Assign(export.Appeal);

    Call(LeLappValidateLegalAppeal.Execute, useImport, useExport);

    local.LastErrorEntry.Count = useExport.LastErrorEntry.Count;
    useExport.Errors.CopyTo(local.Errors, MoveErrors);
  }

  private void UseLeLappValidateLegalAppeal2()
  {
    var useImport = new LeLappValidateLegalAppeal.Import();
    var useExport = new LeLappValidateLegalAppeal.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    MoveLegalAction2(export.LegalAction, useImport.LegalAction);
    useImport.Tribunal.Assign(export.FromTribunal);
    useImport.Appeal.Assign(export.Appeal);

    Call(LeLappValidateLegalAppeal.Execute, useImport, useExport);

    local.LastErrorEntry.Count = useExport.LastErrorEntry.Count;
    useExport.Errors.CopyTo(local.Errors, MoveErrors);
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
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips2",
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
    entities.Foreign.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Foreign.Identifier = db.GetInt32(reader, 0);
        entities.Foreign.Country = db.GetNullableString(reader, 1);
        entities.Foreign.TrbId = db.GetNullableInt32(reader, 2);
        entities.Foreign.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetInt32(command, "identifier", export.FromTribunal.Identifier);
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", export.FromTribunal.Identifier);
        db.SetNullableString(
          command, "country", export.FromFipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
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

  private IEnumerable<bool> ReadTribunalLegalAction()
  {
    entities.Tribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadTribunalLegalAction",
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
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 7);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 8);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 9);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 10);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 11);
        entities.Tribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;

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
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// A value of FromLacn.
    /// </summary>
    [JsonPropertyName("fromLacn")]
    public LegalAction FromLacn
    {
      get => fromLacn ??= new();
      set => fromLacn = value;
    }

    /// <summary>
    /// A value of HiddenFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenFipsTribAddress")]
    public FipsTribAddress HiddenFipsTribAddress
    {
      get => hiddenFipsTribAddress ??= new();
      set => hiddenFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
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
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// A value of DlgflowSelected.
    /// </summary>
    [JsonPropertyName("dlgflowSelected")]
    public FipsTribAddress DlgflowSelected
    {
      get => dlgflowSelected ??= new();
      set => dlgflowSelected = value;
    }

    /// <summary>
    /// A value of ToFipsTribAddress.
    /// </summary>
    [JsonPropertyName("toFipsTribAddress")]
    public FipsTribAddress ToFipsTribAddress
    {
      get => toFipsTribAddress ??= new();
      set => toFipsTribAddress = value;
    }

    /// <summary>
    /// A value of FromFipsTribAddress.
    /// </summary>
    [JsonPropertyName("fromFipsTribAddress")]
    public FipsTribAddress FromFipsTribAddress
    {
      get => fromFipsTribAddress ??= new();
      set => fromFipsTribAddress = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of ListTribunalFrom.
    /// </summary>
    [JsonPropertyName("listTribunalFrom")]
    public Standard ListTribunalFrom
    {
      get => listTribunalFrom ??= new();
      set => listTribunalFrom = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
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
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevAppeal.
    /// </summary>
    [JsonPropertyName("hiddenPrevAppeal")]
    public Appeal HiddenPrevAppeal
    {
      get => hiddenPrevAppeal ??= new();
      set => hiddenPrevAppeal = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of ListLegalActions.
    /// </summary>
    [JsonPropertyName("listLegalActions")]
    public Standard ListLegalActions
    {
      get => listLegalActions ??= new();
      set => listLegalActions = value;
    }

    /// <summary>
    /// A value of ListTribunalsTo.
    /// </summary>
    [JsonPropertyName("listTribunalsTo")]
    public Standard ListTribunalsTo
    {
      get => listTribunalsTo ??= new();
      set => listTribunalsTo = value;
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
    /// A value of ToTribunal.
    /// </summary>
    [JsonPropertyName("toTribunal")]
    public Tribunal ToTribunal
    {
      get => toTribunal ??= new();
      set => toTribunal = value;
    }

    /// <summary>
    /// A value of FromTribunal.
    /// </summary>
    [JsonPropertyName("fromTribunal")]
    public Tribunal FromTribunal
    {
      get => fromTribunal ??= new();
      set => fromTribunal = value;
    }

    /// <summary>
    /// A value of Appeal.
    /// </summary>
    [JsonPropertyName("appeal")]
    public Appeal Appeal
    {
      get => appeal ??= new();
      set => appeal = value;
    }

    /// <summary>
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
    }

    /// <summary>
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
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
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    private LegalAction fromLacn;
    private FipsTribAddress hiddenFipsTribAddress;
    private Tribunal hiddenTribunal;
    private Common dataExists;
    private Common debug;
    private FipsTribAddress dlgflowSelected;
    private FipsTribAddress toFipsTribAddress;
    private FipsTribAddress fromFipsTribAddress;
    private Tribunal dlgflwSelectedTribunal;
    private Fips dlgflwSelectedFips;
    private NextTranInfo hiddenNextTranInfo;
    private Fips hiddenFips;
    private Fips fips;
    private Standard listTribunalFrom;
    private CodeValue actionTaken;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Common hiddenDisplayPerformed;
    private Appeal hiddenPrevAppeal;
    private LegalAction hiddenPrevLegalAction;
    private Common hiddenPrevUserAction;
    private Standard listLegalActions;
    private Standard listTribunalsTo;
    private LegalAction legalAction;
    private Tribunal toTribunal;
    private Tribunal fromTribunal;
    private Appeal appeal;
    private Standard nexttran;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Standard standard;
    private ScrollingAttributes scrollingAttributes;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// A value of HiddenFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenFipsTribAddress")]
    public FipsTribAddress HiddenFipsTribAddress
    {
      get => hiddenFipsTribAddress ??= new();
      set => hiddenFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
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
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// A value of ToFipsTribAddress.
    /// </summary>
    [JsonPropertyName("toFipsTribAddress")]
    public FipsTribAddress ToFipsTribAddress
    {
      get => toFipsTribAddress ??= new();
      set => toFipsTribAddress = value;
    }

    /// <summary>
    /// A value of FromFipsTribAddress.
    /// </summary>
    [JsonPropertyName("fromFipsTribAddress")]
    public FipsTribAddress FromFipsTribAddress
    {
      get => fromFipsTribAddress ??= new();
      set => fromFipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of ListTribunalFrom.
    /// </summary>
    [JsonPropertyName("listTribunalFrom")]
    public Standard ListTribunalFrom
    {
      get => listTribunalFrom ??= new();
      set => listTribunalFrom = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
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
    /// A value of RequiredTribunalsFor.
    /// </summary>
    [JsonPropertyName("requiredTribunalsFor")]
    public Fips RequiredTribunalsFor
    {
      get => requiredTribunalsFor ??= new();
      set => requiredTribunalsFor = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevAppeal.
    /// </summary>
    [JsonPropertyName("hiddenPrevAppeal")]
    public Appeal HiddenPrevAppeal
    {
      get => hiddenPrevAppeal ??= new();
      set => hiddenPrevAppeal = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of ListLegalActions.
    /// </summary>
    [JsonPropertyName("listLegalActions")]
    public Standard ListLegalActions
    {
      get => listLegalActions ??= new();
      set => listLegalActions = value;
    }

    /// <summary>
    /// A value of ListTribunalsTo.
    /// </summary>
    [JsonPropertyName("listTribunalsTo")]
    public Standard ListTribunalsTo
    {
      get => listTribunalsTo ??= new();
      set => listTribunalsTo = value;
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
    /// A value of ToTribunal.
    /// </summary>
    [JsonPropertyName("toTribunal")]
    public Tribunal ToTribunal
    {
      get => toTribunal ??= new();
      set => toTribunal = value;
    }

    /// <summary>
    /// A value of FromTribunal.
    /// </summary>
    [JsonPropertyName("fromTribunal")]
    public Tribunal FromTribunal
    {
      get => fromTribunal ??= new();
      set => fromTribunal = value;
    }

    /// <summary>
    /// A value of Appeal.
    /// </summary>
    [JsonPropertyName("appeal")]
    public Appeal Appeal
    {
      get => appeal ??= new();
      set => appeal = value;
    }

    /// <summary>
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
    }

    /// <summary>
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private FipsTribAddress hiddenFipsTribAddress;
    private Tribunal hiddenTribunal;
    private Common dataExists;
    private Common debug;
    private FipsTribAddress toFipsTribAddress;
    private FipsTribAddress fromFipsTribAddress;
    private Fips hiddenFips;
    private Fips fips;
    private Standard listTribunalFrom;
    private CodeValue actionTaken;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Fips requiredTribunalsFor;
    private Common hiddenDisplayPerformed;
    private Appeal hiddenPrevAppeal;
    private LegalAction hiddenPrevLegalAction;
    private Common hiddenPrevUserAction;
    private Standard listLegalActions;
    private Standard listTribunalsTo;
    private LegalAction legalAction;
    private Tribunal toTribunal;
    private Tribunal fromTribunal;
    private Appeal appeal;
    private Standard nexttran;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private ScrollingAttributes scrollingAttributes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
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
    /// A value of Bypass.
    /// </summary>
    [JsonPropertyName("bypass")]
    public Common Bypass
    {
      get => bypass ??= new();
      set => bypass = value;
    }

    /// <summary>
    /// A value of Retlacn.
    /// </summary>
    [JsonPropertyName("retlacn")]
    public Common Retlacn
    {
      get => retlacn ??= new();
      set => retlacn = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of NoOfLegalActions.
    /// </summary>
    [JsonPropertyName("noOfLegalActions")]
    public Common NoOfLegalActions
    {
      get => noOfLegalActions ??= new();
      set => noOfLegalActions = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of LastErrorEntry.
    /// </summary>
    [JsonPropertyName("lastErrorEntry")]
    public Common LastErrorEntry
    {
      get => lastErrorEntry ??= new();
      set => lastErrorEntry = value;
    }

    /// <summary>
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
    }

    /// <summary>
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
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

    private Common returnCode;
    private Common bypass;
    private Common retlacn;
    private CodeValue codeValue;
    private Code code;
    private Common noOfLegalActions;
    private Common userAction;
    private Common lastErrorEntry;
    private Common highlightError;
    private Array<ErrorsGroup> errors;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingRelatedLegalAction.
    /// </summary>
    [JsonPropertyName("existingRelatedLegalAction")]
    public RelatedLegalAction ExistingRelatedLegalAction
    {
      get => existingRelatedLegalAction ??= new();
      set => existingRelatedLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    private FipsTribAddress foreign;
    private Tribunal tribunal;
    private Fips fips;
    private RelatedLegalAction existingRelatedLegalAction;
    private LegalAction existingLegalAction;
  }
#endregion
}

// Program: LE_LRES_LEGAL_RESPONSE, ID: 371980937, model: 746.
// Short name: SWELRESP
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
/// A program: LE_LRES_LEGAL_RESPONSE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLresLegalResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LRES_LEGAL_RESPONSE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLresLegalResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLresLegalResponse.
  /// </summary>
  public LeLresLegalResponse(IContext context, Import import, Export export):
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
    // Date		Developer	Request #	Description
    // 06/20/95	Dave Allen
    // Initial Code
    // 01/03/96	T.O.Redmond
    // Added Security and Next Tran
    // 05/01/97	govind		Prob Report
    // Fixed to display Legal Action info namely Pet/Resp, Action
    // Taken on 	Nexttran into LRES.
    // 11/18/98	P McElderry	None listed
    // Enhanced READ statements; deleted unnecessary READ
    // statements.
    // 12/07-10/98	P McElderry	None listed
    // Enhanced structure of program to meet KESSEP standards.
    // Fixed DISPLAY, ADD, UPDATE, logic.
    // 04/03/02		K Cole
    // PR138221  Changed to call new cab to get action taken description.
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -----------------------
    // Move Imports to Exports
    // -----------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.LegalAction.Assign(import.LegalAction);
    export.ActionTaken.Description = import.ActionTaken.Description;
    MoveFips2(import.Fips, export.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.CutOffDate.Date = import.CutOffDate.Date;
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    export.PromptClassification.SelectChar =
      import.PromptClassification.SelectChar;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.DataExists.Flag = import.DataExists.Flag;

    export.LegalActResponse.Index = 0;
    export.LegalActResponse.Clear();

    for(import.LegalActResponse.Index = 0; import.LegalActResponse.Index < import
      .LegalActResponse.Count; ++import.LegalActResponse.Index)
    {
      if (export.LegalActResponse.IsFull)
      {
        break;
      }

      export.LegalActResponse.Update.Common.SelectChar =
        import.LegalActResponse.Item.Common.SelectChar;
      export.LegalActResponse.Update.LegalActionResponse.Assign(
        import.LegalActResponse.Item.LegalActionResponse);
      export.LegalActResponse.Update.PromptType.SelectChar =
        import.LegalActResponse.Item.PromptType.SelectChar;
      export.LegalActResponse.Next();
    }

    // -----------------
    // Move hidden views
    // -----------------
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    export.HiddenTribunal.Identifier = import.HiddenTribunal.Identifier;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveFips1(import.HiddenFips, export.HiddenFips);

    // ---------------------------
    // security / nextran logic
    // ---------------------------
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();
      export.HiddenNextTranInfo.Assign(local.NextTranInfo);

      // -----------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // -----------------------------------------------------------
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      export.LegalAction.CourtCaseNumber =
        export.HiddenNextTranInfo.CourtCaseNumber ?? "";

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }
      else
      {
        if (ReadLegalAction2())
        {
          export.LegalAction.Assign(entities.LegalAction);

          if (ReadTribunal())
          {
            export.Tribunal.Assign(entities.Tribunal);

            if (ReadFips3())
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
            }
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";
          }
        }
        else
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LEGAL_ACTION_NF";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          global.Command = "REDISP";
        }
      }
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // -------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // -------------------------------------------------------------
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      UseScCabNextTranPut1();

      return;
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";
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

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (Equal(global.Command, "REDISP") || Equal
      (global.Command, "RLLGLACT") || Equal(global.Command, "RLCVAL") || Equal
      (global.Command, "RLTRIB"))
    {
    }
    else
    {
      // -----------------------------
      // validate action level security
      // ------------------------------
      UseScCabTestSecurity();
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

    // --------------------------
    // Edit required fields
    // --------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
    {
      if (!Equal(global.Command, "DISPLAY"))
      {
        // ----------------------------------------------------------------
        // Verify display has been performed before an update, add, or
        // delete can take place.
        // -----------------------------------------------------------------
        if (Equal(global.Command, "UPDATE"))
        {
          if (export.LegalAction.Identifier != export
            .HiddenLegalAction.Identifier || export.LegalAction.Identifier == 0
            )
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

            goto Test1;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          // --------------------------------------------
          // Ensure key fields haven't been changed
          // --------------------------------------------
          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenLegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          if (AsChar(export.LegalAction.Classification) != AsChar
            (export.HiddenLegalAction.Classification))
          {
            var field = GetField(export.LegalAction, "classification");

            field.Error = true;

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

            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }
        else if (Equal(global.Command, "ADD"))
        {
          if (export.LegalAction.Identifier != export
            .HiddenLegalAction.Identifier || export.LegalAction.Identifier == 0
            )
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LE0000_DISPLAY_BEFORE_ADD";

            goto Test1;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          // --------------------------------------------
          // Ensure key fields haven't been changed
          // --------------------------------------------
          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenLegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

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
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }
        else if (Equal(global.Command, "DELETE"))
        {
          // ----------------------------------------------------
          // Make sure that Court Case Number hasn't been changed
          // before a delete.
          // -----------------------------------------------------
          // ---------------------------------------------
          // Verify that a display has been performed
          // before the delete can take place.
          // ---------------------------------------------
          if (export.LegalAction.Identifier != export
            .HiddenLegalAction.Identifier || export.LegalAction.Identifier == 0
            )
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

            goto Test1;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenLegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

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
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;
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

Test1:

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
      else if (IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        // ------------------------------------------------------
        // CCN is optional for Display when court case number not
        // entered - flow to LAPS
        // ------------------------------------------------------
        ExitState = "ECO_LNK_TO_LST_LGL_ACT_BY_CP";
      }
      else if (!IsEmpty(export.FipsTribAddress.Country) && (
        !IsEmpty(export.Fips.CountyAbbreviation) || !
        IsEmpty(export.Fips.StateAbbreviation)))
      {
        var field1 = GetField(export.FipsTribAddress, "country");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        var field3 = GetField(export.Fips, "countyAbbreviation");

        field3.Error = true;

        ExitState = "EITHER_STATE_OR_CNTRY_CODE";
      }
      else if (!IsEmpty(export.Fips.CountyAbbreviation) && !
        IsEmpty(export.Fips.StateAbbreviation) && IsEmpty
        (export.FipsTribAddress.Country))
      {
        if (!Equal(export.Fips.CountyAbbreviation,
          export.HiddenFips.CountyAbbreviation) || !
          Equal(export.Fips.StateAbbreviation,
          export.HiddenFips.StateAbbreviation))
        {
          if (export.Tribunal.Identifier == 0)
          {
            if (ReadFips2())
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
            }
          }
          else
          {
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

              ExitState = "INVALID_TRIBUNAL";
            }

            if (!IsEmpty(export.DataExists.Flag))
            {
              for(export.LegalActResponse.Index = 0; export
                .LegalActResponse.Index < export.LegalActResponse.Count; ++
                export.LegalActResponse.Index)
              {
                export.LegalActResponse.Update.LegalActionResponse.Type1 = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  ReceivedDate = null;
                export.LegalActResponse.Update.LegalActionResponse.
                  RespForLastName = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  RespForFirstName = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  RespForMiddleInitial = "";
                export.LegalActResponse.Update.LegalActionResponse.LastName =
                  "";
                export.LegalActResponse.Update.LegalActionResponse.FirstName =
                  "";
                export.LegalActResponse.Update.LegalActionResponse.
                  MiddleInitial = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  Relationship = "";
                export.LegalActResponse.Update.LegalActionResponse.Narrative =
                  Spaces(LegalActionResponse.Narrative_MaxLength);
              }
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
      }
      else if (!IsEmpty(export.FipsTribAddress.Country) && IsEmpty
        (export.Fips.CountyAbbreviation) && IsEmpty
        (export.Fips.StateAbbreviation))
      {
        // ---------------------------------------------------
        // Continue processing - validation logic in DISPLAY
        // ---------------------------------------------------
      }
      else
      {
        var field1 = GetField(export.FipsTribAddress, "country");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        var field3 = GetField(export.Fips, "countyAbbreviation");

        field3.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (!IsEmpty(export.DataExists.Flag))
        {
          for(export.LegalActResponse.Index = 0; export
            .LegalActResponse.Index < export.LegalActResponse.Count; ++
            export.LegalActResponse.Index)
          {
            export.LegalActResponse.Update.LegalActionResponse.Type1 = "";
            export.LegalActResponse.Update.LegalActionResponse.ReceivedDate =
              null;
            export.LegalActResponse.Update.LegalActionResponse.RespForLastName =
              "";
            export.LegalActResponse.Update.LegalActionResponse.
              RespForFirstName = "";
            export.LegalActResponse.Update.LegalActionResponse.
              RespForMiddleInitial = "";
            export.LegalActResponse.Update.LegalActionResponse.LastName = "";
            export.LegalActResponse.Update.LegalActionResponse.FirstName = "";
            export.LegalActResponse.Update.LegalActionResponse.MiddleInitial =
              "";
            export.LegalActResponse.Update.LegalActionResponse.Relationship =
              "";
            export.LegalActResponse.Update.LegalActionResponse.Narrative =
              Spaces(LegalActionResponse.Narrative_MaxLength);
          }
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        return;
      }
      else
      {
        // -------------------
        // Continue processing
        // -------------------
      }

      if (IsEmpty(export.LegalAction.Classification))
      {
        if (Equal(global.Command, "DISPLAY"))
        {
          // --------------------------------------
          // Classification is optional for Display
          // --------------------------------------
        }
        else
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "LE0000_LEG_ACT_CLASSIFN_REQD";
        }
      }
      else
      {
        // ------------------------------------------------------------
        // Nonblank classification entered. Validate the Classification
        // entered.
        // ------------------------------------------------------------
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = export.LegalAction.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "LE0000_INVALID_LEG_ACT_CLASSIFN";
        }
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

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      for(export.LegalActResponse.Index = 0; export.LegalActResponse.Index < export
        .LegalActResponse.Count; ++export.LegalActResponse.Index)
      {
        if (AsChar(export.LegalActResponse.Item.Common.SelectChar) == 'S')
        {
          if (IsEmpty(export.LegalActResponse.Item.LegalActionResponse.Type1))
          {
            var field =
              GetField(export.LegalActResponse.Item.LegalActionResponse, "type1");
              

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          if (Equal(export.LegalActResponse.Item.LegalActionResponse.
            ReceivedDate, null))
          {
            var field =
              GetField(export.LegalActResponse.Item.LegalActionResponse,
              "receivedDate");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          if (IsEmpty(export.LegalActResponse.Item.LegalActionResponse.
            RespForLastName))
          {
            var field =
              GetField(export.LegalActResponse.Item.LegalActionResponse,
              "respForLastName");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          if (IsEmpty(export.LegalActResponse.Item.LegalActionResponse.
            RespForFirstName))
          {
            var field =
              GetField(export.LegalActResponse.Item.LegalActionResponse,
              "respForFirstName");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }
        else if (IsEmpty(export.LegalActResponse.Item.PromptType.SelectChar))
        {
          // -------------------
          // Continue processing
          // -------------------
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
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
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "LIST"))
    {
      if (!Equal(global.Command, "LIST"))
      {
        // -----------------------------------------------------
        // At least one occurrence must be "S"elected on an Add,
        // Update, or Delete.
        // -----------------------------------------------------
        local.TotalSelected.Count = 0;

        for(export.LegalActResponse.Index = 0; export.LegalActResponse.Index < export
          .LegalActResponse.Count; ++export.LegalActResponse.Index)
        {
          if (!IsEmpty(export.LegalActResponse.Item.Common.SelectChar))
          {
            ++local.TotalSelected.Count;
          }
        }

        if (local.TotalSelected.Count == 0)
        {
          for(export.LegalActResponse.Index = 0; export
            .LegalActResponse.Index < export.LegalActResponse.Count; ++
            export.LegalActResponse.Index)
          {
            var field =
              GetField(export.LegalActResponse.Item.Common, "selectChar");

            field.Error = true;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";

            return;
          }
        }
      }

      // ------------------------
      // Validate Prompt Tribunal
      // ------------------------
      switch(AsChar(export.PromptTribunal.SelectChar))
      {
        case ' ':
          // -------------------
          // Continue processing
          // -------------------
          break;
        case 'S':
          if (!Equal(global.Command, "LIST"))
          {
            var field1 = GetField(export.PromptTribunal, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }

          break;
        default:
          var field = GetField(export.PromptTribunal, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
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

      // ------------------------------
      // Validate Prompt Classification
      // ------------------------------
      switch(AsChar(export.PromptClassification.SelectChar))
      {
        case ' ':
          // -------------------
          // Continue processing
          // -------------------
          break;
        case 'S':
          if (!Equal(global.Command, "LIST"))
          {
            var field1 = GetField(export.PromptClassification, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }

          break;
        default:
          var field = GetField(export.PromptClassification, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
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

      for(export.LegalActResponse.Index = 0; export.LegalActResponse.Index < export
        .LegalActResponse.Count; ++export.LegalActResponse.Index)
      {
        // ------------------
        // Validate Selection
        // ------------------
        switch(AsChar(export.LegalActResponse.Item.Common.SelectChar))
        {
          case ' ':
            // -------------------
            // Continue processing
            // -------------------
            break;
          case 'S':
            // -------------------
            // Continue processing
            // -------------------
            break;
          default:
            var field =
              GetField(export.LegalActResponse.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";

            break;
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

        // -------------
        // Validate Type
        // -------------
        if (!IsEmpty(export.LegalActResponse.Item.Common.SelectChar))
        {
          if (!Equal(global.Command, "LIST"))
          {
            local.Code.CodeName = "LEGAL ACTION RESPONSE TYPE";
            local.CodeValue.Cdvalue =
              export.LegalActResponse.Item.LegalActionResponse.Type1;
            UseCabValidateCodeValue();

            if (AsChar(local.ValidCode.Flag) != 'Y')
            {
              var field =
                GetField(export.LegalActResponse.Item.LegalActionResponse,
                "type1");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

              goto Test2;
            }

            if (Lt(local.Current.Date,
              export.LegalActResponse.Item.LegalActionResponse.ReceivedDate))
            {
              var field =
                GetField(export.LegalActResponse.Item.LegalActionResponse,
                "receivedDate");

              field.Error = true;

              ExitState = "LE0000_LRES_RESP_DATE_GT_CURR_DT";

              goto Test2;
            }
          }

          // --------------------
          // Validate Prompt Type
          // --------------------
          switch(AsChar(export.LegalActResponse.Item.PromptType.SelectChar))
          {
            case ' ':
              // -------------------
              // Continue processing
              // -------------------
              break;
            case 'S':
              if (!Equal(global.Command, "LIST"))
              {
                var field1 =
                  GetField(export.LegalActResponse.Item.PromptType, "selectChar");
                  

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
              }

              break;
            default:
              var field =
                GetField(export.LegalActResponse.Item.PromptType, "selectChar");
                

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
          }
        }

Test2:
        ;
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

      if (Equal(global.Command, "LIST"))
      {
        // ---------------------------------------------
        // Count how many prompts were selected.
        // ---------------------------------------------
        local.TotalSelected.Count = 0;

        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          ++local.TotalSelected.Count;
        }

        if (!IsEmpty(export.PromptClassification.SelectChar))
        {
          ++local.TotalSelected.Count;
        }

        for(export.LegalActResponse.Index = 0; export.LegalActResponse.Index < export
          .LegalActResponse.Count; ++export.LegalActResponse.Index)
        {
          if (!IsEmpty(export.LegalActResponse.Item.PromptType.SelectChar))
          {
            ++local.TotalSelected.Count;
          }
        }

        switch(local.TotalSelected.Count)
        {
          case 0:
            // -----------------------------------------
            // Prompt must be selected when PF4 selected
            // -----------------------------------------
            var field1 = GetField(export.PromptTribunal, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.PromptClassification, "selectChar");

            field2.Error = true;

            for(export.LegalActResponse.Index = 0; export
              .LegalActResponse.Index < export.LegalActResponse.Count; ++
              export.LegalActResponse.Index)
            {
              var field =
                GetField(export.LegalActResponse.Item.PromptType, "selectChar");
                

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
          case 1:
            // -------------------
            // Continue processing
            // -------------------
            break;
          default:
            if (!IsEmpty(export.PromptTribunal.SelectChar))
            {
              var field = GetField(export.PromptTribunal, "selectChar");

              field.Error = true;
            }

            if (!IsEmpty(export.PromptClassification.SelectChar))
            {
              var field = GetField(export.PromptClassification, "selectChar");

              field.Error = true;
            }

            for(export.LegalActResponse.Index = 0; export
              .LegalActResponse.Index < export.LegalActResponse.Count; ++
              export.LegalActResponse.Index)
            {
              var field =
                GetField(export.LegalActResponse.Item.PromptType, "selectChar");
                

              field.Error = true;
            }

            ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";

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
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // -----------------------------------------------------
        // Continue processing - logic performed in next CASE OF
        // -----------------------------------------------------
        break;
      case "LIST":
        // ------------
        // Flow to LTRB
        // ------------
        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          export.HiddenSecurity.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

          return;
        }

        export.DisplayActiveCasesOnly.Flag = "Y";

        if (!IsEmpty(export.PromptClassification.SelectChar))
        {
          export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        }
        else
        {
          for(export.LegalActResponse.Index = 0; export
            .LegalActResponse.Index < export.LegalActResponse.Count; ++
            export.LegalActResponse.Index)
          {
            if (!IsEmpty(export.LegalActResponse.Item.PromptType.SelectChar))
            {
              if (IsEmpty(export.LegalActResponse.Item.Common.SelectChar))
              {
                var field =
                  GetField(export.LegalActResponse.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                goto Test3;
              }

              export.Code.CodeName = "LEGAL ACTION RESPONSE TYPE";

              break;
            }
          }
        }

        // ------------
        // Flow to CDVL
        // ------------
        export.HiddenSecurity.LinkIndicator = "L";
        ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

        return;
      case "RLTRIB":
        // ------------------------------------------------------------
        // Returned from List Tribunals screen. Move values to export.
        // ------------------------------------------------------------
        export.Fips.Assign(import.DlgflwSelectedFips);
        export.Tribunal.Assign(import.DlgflwSelectedTribunal);
        export.PromptTribunal.SelectChar = "";

        var field1 = GetField(export.CutOffDate, "date");

        field1.Protected = false;
        field1.Focused = true;

        return;
      case "RLCVAL":
        // -----------------------------------------------------------------
        // Returned from List Code Values screen.  Move values to
        // export.
        // ------------------------------------------------------------------
        if (!IsEmpty(export.PromptClassification.SelectChar))
        {
          export.PromptClassification.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Protected = false;
            field.Focused = true;

            export.LegalAction.Classification =
              import.DlgflwSelectedCodeValue.Cdvalue;
          }
          else
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else
        {
          for(export.LegalActResponse.Index = 0; export
            .LegalActResponse.Index < export.LegalActResponse.Count; ++
            export.LegalActResponse.Index)
          {
            if (!IsEmpty(export.LegalActResponse.Item.PromptType.SelectChar))
            {
              export.LegalActResponse.Update.PromptType.SelectChar = "";

              if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
              {
                var field3 =
                  GetField(export.LegalActResponse.Item.LegalActionResponse,
                  "receivedDate");

                field3.Protected = false;
                field3.Focused = true;

                export.LegalActResponse.Update.LegalActionResponse.Type1 =
                  import.DlgflwSelectedCodeValue.Cdvalue;

                var field4 =
                  GetField(export.LegalActResponse.Item.LegalActionResponse,
                  "receivedDate");

                field4.Protected = false;
                field4.Focused = true;

                return;
              }
              else
              {
                var field =
                  GetField(export.LegalActResponse.Item.LegalActionResponse,
                  "receivedDate");

                field.Protected = false;
                field.Focused = true;

                return;
              }
            }
            else
            {
              var field =
                GetField(export.LegalActResponse.Item.LegalActionResponse,
                "receivedDate");

              field.Protected = false;
              field.Focused = true;

              return;
            }
          }
        }

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Protected = false;
        field2.Focused = true;

        return;
      case "RLLGLACT":
        // ----------------------------------------------------
        // Returned from List Legal Actions by Court Case Number
        // ----------------------------------------------------
        if (import.DlgflwSelectedLegalAction.Identifier > 0)
        {
          export.LegalAction.Assign(import.DlgflwSelectedLegalAction);
          MoveLegalAction(export.LegalAction, export.HiddenLegalAction);
          global.Command = "REDISP";
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        break;
      case "ADD":
        // ----------------------------------------------------------------
        // Verify a display has been performed before update can take
        //  place.
        // -----------------------------------------------------------------
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_DISPLAY_BEFORE_ADD";

          break;
        }

        // --------------------------------------------------------------
        // Ensure Court Case Number hasn't been changed before an
        // update.
        // ---------------------------------------------------------------
        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber) || AsChar
          (import.LegalAction.Classification) != AsChar
          (import.HiddenLegalAction.Classification))
        {
          var field3 = GetField(export.LegalAction, "courtCaseNumber");

          field3.Error = true;

          var field4 = GetField(export.LegalAction, "classification");

          field4.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        // ----------------------------------------------------
        // Create selected occurrences of Legal Action Response
        // ----------------------------------------------------
        for(export.LegalActResponse.Index = 0; export.LegalActResponse.Index < export
          .LegalActResponse.Count; ++export.LegalActResponse.Index)
        {
          if (!IsEmpty(export.LegalActResponse.Item.Common.SelectChar))
          {
            UseLeCreateLegalActionResponse();

            if (IsExitState("LEGAL_ACTION_NF"))
            {
              var field3 = GetField(export.LegalAction, "courtCaseNumber");

              field3.Error = true;

              var field4 = GetField(export.LegalAction, "classification");

              field4.Error = true;
            }
            else if (IsExitState("LEGAL_ACTION_RESPONSE_AE"))
            {
              var field =
                GetField(export.LegalActResponse.Item.Common, "selectChar");

              field.Error = true;
            }
            else
            {
              // ------------------------------------------------------------
              // Continue processing - no other exit states possible
              // ------------------------------------------------------------
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              goto Test3;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            // -------------------------------------------------------
            // Legal Response has been successfully created. Raise the
            // events here.
            // --------------------------------------------------------
            UseLeLresRaiseInfrastrucEvents();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              goto Test3;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            export.LegalActResponse.Update.Common.SelectChar = "";
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // -----------------------------------------------------
        // If possible, this error logic needs 2 be combined
        // -----------------------------------------------------
        if (export.LegalAction.Identifier == 0 || import
          .LegalAction.Identifier != import.HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          break;
        }

        // -----------------------------------------------------
        // Make sure that Court Case Number hasn't been changed
        // before an update.
        // -----------------------------------------------------
        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber) || AsChar
          (import.LegalAction.Classification) != AsChar
          (import.HiddenLegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        // ----------------------------------------------------
        // Update selected occurrences of Legal Action Response
        // ----------------------------------------------------
        for(export.LegalActResponse.Index = 0; export.LegalActResponse.Index < export
          .LegalActResponse.Count; ++export.LegalActResponse.Index)
        {
          if (!IsEmpty(export.LegalActResponse.Item.Common.SelectChar))
          {
            UseLeUpdateLegalActionResponse();

            if (IsExitState("LEGAL_ACTION_NF"))
            {
              var field3 = GetField(export.LegalAction, "courtCaseNumber");

              field3.Error = true;

              var field4 = GetField(export.LegalAction, "classification");

              field4.Error = true;
            }
            else if (IsExitState("CO0000_LEGAL_ACTION_RESPONSE_NF"))
            {
              var field =
                GetField(export.LegalActResponse.Item.Common, "selectChar");

              field.Error = true;
            }
            else if (IsExitState("CO0000_LEGAL_ACTION_RESPONSE_NU"))
            {
              var field =
                GetField(export.LegalActResponse.Item.Common, "selectChar");

              field.Error = true;
            }
            else
            {
              // ------------------------------------------------------------
              // Continue processing - if error exit state has been set, the
              // program will terminate
              // ------------------------------------------------------------
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              goto Test3;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            export.LegalActResponse.Update.Common.SelectChar = "";
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          // --------------------------------------
          // Came to this screen from HIST or MONA
          // --------------------------------------
          switch(TrimEnd(export.HiddenNextTranInfo.LastTran ?? ""))
          {
            case "SRPT":
              export.Standard.NextTransaction = "HIST";

              break;
            case "SRPU":
              export.Standard.NextTransaction = "MONA";

              break;
            default:
              break;
          }

          export.HiddenNextTranInfo.CourtCaseNumber =
            export.LegalAction.CourtCaseNumber ?? "";
          export.HiddenNextTranInfo.LegalActionIdentifier =
            export.LegalAction.Identifier;
          UseScCabNextTranPut2();

          return;
        }

        // --------------------------------------------------------
        // Otherwise it is a normal return to the linking procedure
        // --------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "DELETE":
        // ----------------------------------------------------
        // Make sure that Court Case Number hasn't been changed
        // before a delete.
        // -----------------------------------------------------
        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        // ---------------------------------------------
        // Verify that a display has been performed
        // before the delete can take place.
        // ---------------------------------------------
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          break;
        }

        // ---------------------------------------------
        // Delete selected occurrences of Legal Action
        // Response.
        // ---------------------------------------------
        for(export.LegalActResponse.Index = 0; export.LegalActResponse.Index < export
          .LegalActResponse.Count; ++export.LegalActResponse.Index)
        {
          if (!IsEmpty(export.LegalActResponse.Item.Common.SelectChar))
          {
            UseLeDeleteLegalActionResponse();

            if (IsExitState("LEGAL_ACTION_NF"))
            {
              var field3 = GetField(export.LegalAction, "courtCaseNumber");

              field3.Error = true;

              var field4 = GetField(export.LegalAction, "classification");

              field4.Error = true;
            }
            else if (IsExitState("CO0000_LEGAL_ACTION_RESPONSE_NF"))
            {
              var field =
                GetField(export.LegalActResponse.Item.Common, "selectChar");

              field.Error = true;
            }
            else
            {
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            export.LegalActResponse.Update.Common.SelectChar = "";
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "SIGNOFF":
        // -----------------------------------
        // Sign the user off the KESSEP system
        // -----------------------------------
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "REDISP":
        // -------------------
        // Continue processing
        // -------------------
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test3:

    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_DELETE"))
    {
      // -------------------
      // Continue processing
      // -------------------
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // -----------------------------------------------------------------
        // If the screen has already been displayed, (identifier is
        // present and equal to hidden identifier) and the court case
        // number and classification haven't been changed, there is
        // no need to link to list screen to choose a legal action. It is
        // OK to display the screen.
        // -------------------------------------------------------------------
        if (export.LegalAction.Identifier == export
          .HiddenLegalAction.Identifier && Equal
          (export.LegalAction.CourtCaseNumber,
          export.HiddenLegalAction.CourtCaseNumber) && AsChar
          (export.LegalAction.Classification) == AsChar
          (export.HiddenLegalAction.Classification) && export
          .Tribunal.Identifier == export.HiddenTribunal.Identifier && export
          .LegalAction.Identifier > 0)
        {
          // ------------------
          // Action taken below
          // ------------------
        }
        else
        {
          // -----------------------------------------------------------
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
            // ----------------
            // 11/18/98 changes
            // ----------------
            if (export.Tribunal.Identifier != 0)
            {
              // -----------------
              // USER went to LTRB
              // -----------------
              foreach(var item in ReadLegalAction3())
              {
                ++local.NoOfLegalActionsFound.Count;

                if (local.NoOfLegalActionsFound.Count > 1)
                {
                  // ---------------------------------------------------------
                  // Reset the classification to what was entered. Otherwise it
                  // uses the classification of the last legal action read.
                  // ---------------------------------------------------------
                  export.LegalAction.Classification =
                    import.LegalAction.Classification;
                  ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                  return;
                }

                export.LegalAction.Assign(entities.LegalAction);
              }
            }
            else
            {
              foreach(var item in ReadLegalActionTribunal())
              {
                ++local.NoOfLegalActionsFound.Count;

                if (local.NoOfLegalActionsFound.Count > 1)
                {
                  // ---------------------------------------------------------
                  // Reset the classification to what was entered. Otherwise it
                  // uses the classification of the last legal action read.
                  // ---------------------------------------------------------
                  export.LegalAction.Classification =
                    import.LegalAction.Classification;
                  ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                  return;
                }

                export.Tribunal.Assign(entities.Tribunal);
                export.LegalAction.Assign(entities.LegalAction);
              }
            }
          }
          else
          {
            // ----------------
            // Foreign tribunal
            // ----------------
            foreach(var item in ReadLegalAction4())
            {
              ++local.NoOfLegalActionsFound.Count;

              if (local.NoOfLegalActionsFound.Count > 1)
              {
                // ------------------------------------------------------------
                // Reset the classification to what was entered. Otherwise it
                // uses the classification of the last legal action read.
                // -------------------------------------------------------------
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                goto Test4;
              }

              export.LegalAction.Assign(entities.LegalAction);
            }
          }

          if (local.NoOfLegalActionsFound.Count == 0)
          {
            if (!IsEmpty(export.DataExists.Flag))
            {
              for(export.LegalActResponse.Index = 0; export
                .LegalActResponse.Index < export.LegalActResponse.Count; ++
                export.LegalActResponse.Index)
              {
                export.LegalActResponse.Update.LegalActionResponse.Type1 = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  ReceivedDate = null;
                export.LegalActResponse.Update.LegalActionResponse.
                  RespForLastName = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  RespForFirstName = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  RespForMiddleInitial = "";
                export.LegalActResponse.Update.LegalActionResponse.LastName =
                  "";
                export.LegalActResponse.Update.LegalActionResponse.FirstName =
                  "";
                export.LegalActResponse.Update.LegalActionResponse.
                  MiddleInitial = "";
                export.LegalActResponse.Update.LegalActionResponse.
                  Relationship = "";
                export.LegalActResponse.Update.LegalActionResponse.Narrative =
                  Spaces(LegalActionResponse.Narrative_MaxLength);
              }
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            if (ReadLegalAction1())
            {
              ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";
            }
            else
            {
              ExitState = "LEGAL_ACTION_NF";
            }

            if (export.Tribunal.Identifier != export.HiddenTribunal.Identifier)
            {
              export.Tribunal.Identifier = 0;
              export.HiddenTribunal.Identifier = 0;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            export.DataExists.Flag = "";

            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            return;
          }
        }

        UseLeGetPetitionerRespondent();

        // -----------------------------------------------------------
        // Use the code_value table to obtain the description for the
        // legal_action action_taken
        // ------------------------------------------------------------
        if (!IsEmpty(export.LegalAction.ActionTaken))
        {
          UseLeGetActionTakenDescription();
        }

        UseLeListLegalActionResponses();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.LegalActResponse.IsFull)
          {
            export.DataExists.Flag = "Y";
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else if (!export.LegalActResponse.IsEmpty)
          {
            export.DataExists.Flag = "Y";
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else
          {
            export.DataExists.Flag = "";
            ExitState = "LEGAL_RESPONSE_NOT_AVAIL";
          }
        }

        break;
      case "REDISP":
        UseLeGetPetitionerRespondent();

        // ------------------------------------------------------------
        // Use the code_value table to obtain the description for the
        // legal_action action_taken
        // ------------------------------------------------------------
        if (!IsEmpty(export.LegalAction.ActionTaken))
        {
          UseLeGetActionTakenDescription();
        }

        UseLeListLegalActionResponses();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.LegalActResponse.IsFull)
          {
            export.DataExists.Flag = "Y";
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else if (!export.LegalActResponse.IsEmpty)
          {
            export.DataExists.Flag = "Y";
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else
          {
            export.DataExists.Flag = "";
            ExitState = "LEGAL_RESPONSE_NOT_AVAIL";
          }
        }

        break;
      default:
        // -------------------
        // Continue processing
        // -------------------
        break;
    }

Test4:

    // -----------------------------------------------------------
    // If all processing completed successfully, move all exports
    // to previous exports.
    // ------------------------------------------------------------
    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_DISPLAY_SUCCESSFUL") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_DELETE") || IsExitState
      ("LEGAL_RESPONSE_NOT_AVAIL"))
    {
      MoveLegalAction(export.LegalAction, export.HiddenLegalAction);
      export.HiddenTribunal.Identifier = export.Tribunal.Identifier;
      MoveFips1(export.Fips, export.HiddenFips);
    }
    else
    {
      // -------------------
      // End of processing
      // -------------------
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

  private static void MoveLegalActResponse(LeListLegalActionResponses.Export.
    LegalActResponseGroup source, Export.LegalActResponseGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.PromptType.SelectChar = source.PromptType.SelectChar;
    target.LegalActionResponse.Assign(source.LegalActionResponse);
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.CourtCaseNumber = source.CourtCaseNumber;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeCreateLegalActionResponse()
  {
    var useImport = new LeCreateLegalActionResponse.Import();
    var useExport = new LeCreateLegalActionResponse.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionResponse.Assign(
      export.LegalActResponse.Item.LegalActionResponse);

    Call(LeCreateLegalActionResponse.Execute, useImport, useExport);

    export.LegalActResponse.Update.LegalActionResponse.Assign(
      useExport.LegalActionResponse);
  }

  private void UseLeDeleteLegalActionResponse()
  {
    var useImport = new LeDeleteLegalActionResponse.Import();
    var useExport = new LeDeleteLegalActionResponse.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionResponse.CreatedTstamp =
      export.LegalActResponse.Item.LegalActionResponse.CreatedTstamp;

    Call(LeDeleteLegalActionResponse.Execute, useImport, useExport);
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

  private void UseLeListLegalActionResponses()
  {
    var useImport = new LeListLegalActionResponses.Import();
    var useExport = new LeListLegalActionResponses.Export();

    useImport.CutOffDate.Date = export.CutOffDate.Date;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeListLegalActionResponses.Execute, useImport, useExport);

    useExport.LegalActResponse.CopyTo(
      export.LegalActResponse, MoveLegalActResponse);
  }

  private void UseLeLresRaiseInfrastrucEvents()
  {
    var useImport = new LeLresRaiseInfrastrucEvents.Import();
    var useExport = new LeLresRaiseInfrastrucEvents.Export();

    useImport.LegalActionResponse.ReceivedDate =
      export.LegalActResponse.Item.LegalActionResponse.ReceivedDate;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeLresRaiseInfrastrucEvents.Execute, useImport, useExport);
  }

  private void UseLeUpdateLegalActionResponse()
  {
    var useImport = new LeUpdateLegalActionResponse.Import();
    var useExport = new LeUpdateLegalActionResponse.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionResponse.Assign(
      export.LegalActResponse.Item.LegalActionResponse);

    Call(LeUpdateLegalActionResponse.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
          
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
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

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetString(
          command, "classification", export.LegalAction.Classification);
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction4()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetString(
          command, "classification", export.LegalAction.Classification);
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
        db.SetNullableString(
          command, "country", export.FipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.Tribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetString(
          command, "classification", export.LegalAction.Classification);
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
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 5);
        entities.Tribunal.Name = db.GetString(reader, 6);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.Tribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
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
    /// <summary>A LegalActResponseGroup group.</summary>
    [Serializable]
    public class LegalActResponseGroup
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
      /// A value of PromptType.
      /// </summary>
      [JsonPropertyName("promptType")]
      public Common PromptType
      {
        get => promptType ??= new();
        set => promptType = value;
      }

      /// <summary>
      /// A value of LegalActionResponse.
      /// </summary>
      [JsonPropertyName("legalActionResponse")]
      public LegalActionResponse LegalActionResponse
      {
        get => legalActionResponse ??= new();
        set => legalActionResponse = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private Common promptType;
      private LegalActionResponse legalActionResponse;
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
    /// A value of DataExists.
    /// </summary>
    [JsonPropertyName("dataExists")]
    public Common DataExists
    {
      get => dataExists ??= new();
      set => dataExists = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of CutOffDate.
    /// </summary>
    [JsonPropertyName("cutOffDate")]
    public DateWorkArea CutOffDate
    {
      get => cutOffDate ??= new();
      set => cutOffDate = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
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
    /// A value of PromptClassification.
    /// </summary>
    [JsonPropertyName("promptClassification")]
    public Common PromptClassification
    {
      get => promptClassification ??= new();
      set => promptClassification = value;
    }

    /// <summary>
    /// Gets a value of LegalActResponse.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActResponseGroup> LegalActResponse =>
      legalActResponse ??= new(LegalActResponseGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActResponse for json serialization.
    /// </summary>
    [JsonPropertyName("legalActResponse")]
    [Computed]
    public IList<LegalActResponseGroup> LegalActResponse_Json
    {
      get => legalActResponse;
      set => LegalActResponse.Assign(value);
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
    /// A value of DlgflwSelectedLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedLegalAction")]
    public LegalAction DlgflwSelectedLegalAction
    {
      get => dlgflwSelectedLegalAction ??= new();
      set => dlgflwSelectedLegalAction = value;
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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

    private Fips hiddenFips;
    private Common dataExists;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private DateWorkArea cutOffDate;
    private Tribunal hiddenTribunal;
    private Common promptTribunal;
    private Tribunal tribunal;
    private Fips fips;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private LegalAction legalAction;
    private Common promptClassification;
    private Array<LegalActResponseGroup> legalActResponse;
    private LegalAction hiddenLegalAction;
    private LegalAction dlgflwSelectedLegalAction;
    private CodeValue dlgflwSelectedCodeValue;
    private Security2 hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Fips dlgflwSelectedFips;
    private Tribunal dlgflwSelectedTribunal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LegalActResponseGroup group.</summary>
    [Serializable]
    public class LegalActResponseGroup
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
      /// A value of PromptType.
      /// </summary>
      [JsonPropertyName("promptType")]
      public Common PromptType
      {
        get => promptType ??= new();
        set => promptType = value;
      }

      /// <summary>
      /// A value of LegalActionResponse.
      /// </summary>
      [JsonPropertyName("legalActionResponse")]
      public LegalActionResponse LegalActionResponse
      {
        get => legalActionResponse ??= new();
        set => legalActionResponse = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private Common promptType;
      private LegalActionResponse legalActionResponse;
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
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
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
    /// A value of CutOffDate.
    /// </summary>
    [JsonPropertyName("cutOffDate")]
    public DateWorkArea CutOffDate
    {
      get => cutOffDate ??= new();
      set => cutOffDate = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
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
    /// A value of PromptClassification.
    /// </summary>
    [JsonPropertyName("promptClassification")]
    public Common PromptClassification
    {
      get => promptClassification ??= new();
      set => promptClassification = value;
    }

    /// <summary>
    /// Gets a value of LegalActResponse.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActResponseGroup> LegalActResponse =>
      legalActResponse ??= new(LegalActResponseGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActResponse for json serialization.
    /// </summary>
    [JsonPropertyName("legalActResponse")]
    [Computed]
    public IList<LegalActResponseGroup> LegalActResponse_Json
    {
      get => legalActResponse;
      set => LegalActResponse.Assign(value);
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    private Common dataExists;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private DateWorkArea cutOffDate;
    private Tribunal hiddenTribunal;
    private Common promptTribunal;
    private Tribunal tribunal;
    private Fips fips;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private LegalAction legalAction;
    private Common promptClassification;
    private Array<LegalActResponseGroup> legalActResponse;
    private LegalAction hiddenLegalAction;
    private Common displayActiveCasesOnly;
    private Code code;
    private Security2 hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private FipsTribAddress foreign;
    private Fips hiddenFips;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private DateWorkArea current;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private LegalAction legalAction;
    private Common totalSelected;
    private Common noOfLegalActionsFound;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private FipsTribAddress fipsTribAddress;
  }
#endregion
}

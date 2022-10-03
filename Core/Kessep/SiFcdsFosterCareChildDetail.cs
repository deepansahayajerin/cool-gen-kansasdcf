// Program: SI_FCDS_FOSTER_CARE_CHILD_DETAIL, ID: 371758425, model: 746.
// Short name: SWEFCDSP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_FCDS_FOSTER_CARE_CHILD_DETAIL.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiFcdsFosterCareChildDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_FCDS_FOSTER_CARE_CHILD_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiFcdsFosterCareChildDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiFcdsFosterCareChildDetail.
  /// </summary>
  public SiFcdsFosterCareChildDetail(IContext context, Import import,
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
    // ------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 04-24-95  HELEN SHARLAND - MTW	Initial Dev
    // 02-25-96  Paul Elie  -MTW	Retrofit Security and
    // 				nexttran
    // 11/01/96  G. Lofton		Add new security and removed
    // 				old.
    // 10-05-98  C Deghand             Added a local view for no AP's and
    //                                 
    // modified the structure of the
    // exit
    //                                 
    // states on a display to receive
    // the
    //                                 
    // correct message.
    //                                 
    // Added SET statements to set
    // prompts
    //                                 
    // to spaces on a display.
    // 11/3/98  C Deghand              Changed child person number and
    //                                 
    // name to protected fields per SME
    //                                 
    // (Pam V) request.
    // ------------------------------------------------------------
    // 01/13/99 W.Campbell             Removed set statement
    //                                 
    // for ZDEL_START_DATE of
    //                                 
    // entity type CSE_PERSON_ADDRESS.
    //                                 
    // Work done on IDCR454.
    // ------------------------------------------------------------------
    // 02/05/99 W.Campbell             In used CAB
    //                                 
    // SI_READ_CHILD_DETAILS
    //                                 
    // removed qualifier
    //                                 
    // from a READ which used
    //                                 
    // ZDEL_START_DATE from the
    //                                 
    // CSE_PERSON_ADDRESS entity type.
    //                                 
    // This was done on IDCR454.
    //                                 
    // The READ may need to be changed
    //                                 
    // to get the 'best' address.
    // ---------------------------------------------
    // 02/09/99 W.Campbell             Added code to USE
    //                                 
    // EAB_ROLLBACK_CICS for
    //                                 
    // correct DB/2 update and
    //                                 
    // rollback operation.
    // ---------------------------------------------
    // 02/09/99 W.Campbell             Disabled the
    //                                 
    // following set statement
    //                                 
    // as it doesn't appear to
    //                                 
    // be needed for any reason:
    //                                 
    // SET export_child_prompt
    //                                 
    // ief_supplied select_char
    //                                 
    // TO spaces.
    // ---------------------------------------------
    // 02/09/99 W.Campbell             Disabled an IF and MAKE
    //                                 
    // statements as they don't
    //                                 
    // appear to be needed for
    //                                 
    // any reason.  The view and
    //                                 
    // attribute export_child_prompt
    //                                 
    // ief_supplied select_char
    //                                 
    // don't appear to be needed.
    // ---------------------------------------------
    // 05/03/99 W.Campbell             Added code to send
    //                                 
    // selection needed msg to COMP.
    //                                 
    // BE SURE TO MATCH
    //                                 
    // next_tran_info ON THE
    //                                 
    // DIALOG FLOW.
    // -----------------------------------------------
    // 05/26/99 W.Campbell             Replaced zd exit states.
    // -----------------------------------------------
    // 12/21/99 M.Lachowicz            Fix problem for cases which
    //                                 
    // do not have assigned AP.
    //                                 
    // PR #78265.
    // -----------------------------------------------
    // 11/27/00 M.Lachowicz            Changed header line.
    //                                 
    // WR #298.
    // ------------------------------------------------------------------
    // 03/12/01  Madhu Kumar           Edit check for 4 digit and 5
    //                                 
    // digit zip codes .
    // ------------------------------------------------------------------
    // ----------------------------------------------------------------------------------
    // 08/29/2001    Vithal Madhira        PR# 121249, 124583, 124584
    // Fixed the code for family violence indicator. The screen is not 
    // displaying data even  if the family violence indicator is not on the CH.
    // It must display data if the family violence indicator is not on CH.
    // Changed code in SWE01082(SC_CAB_TEST_SECURITY)  and SWE00301(
    // SC_SECURITY_VALID_AUTH_FOR_FV) CABs and FCDS PSTEP.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    MoveNextTranInfo(import.Hidden, export.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.Ap.Assign(import.Ap);
    export.FcChildCsePersonsWorkSet.Assign(import.FcChildCsePersonsWorkSet);
    export.FcChildCaseRole.Assign(import.FcChildCaseRole);
    export.Placement.Assign(import.Placement);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.CountyPrompt.SelectChar = import.CountyPrompt.SelectChar;
    export.FreqPrompt.SelectChar = import.FreqPrompt.SelectChar;
    export.OrderEstPrompt.SelectChar = import.OrderEstPrompt.SelectChar;
    export.PlaceStatePrompt.SelectChar = import.PlaceStatePrompt.SelectChar;
    export.PlacementPrompt.SelectChar = import.PlacementPrompt.SelectChar;
    export.SourcePrompt.SelectChar = import.SourcePrompt.SelectChar;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);

    // 11/27/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/27/00 M.L End
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.ActiveChild.Flag = import.ActiveChild.Flag;

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenPreviousCase.Number = import.HiddenPreviousCase.Number;
    export.HiddenPreviousCsePersonsWorkSet.Number =
      import.HiddenPreviousCsePersonsWorkSet.Number;
    export.HiddenAe.Flag = import.HiddenAe.Flag;
    export.HiddenApSelected.Assign(import.HiddenApSelected);
    export.HiddenChSelected.Number = import.HiddenChSelected.Number;

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumberAp = export.Ap.Number;
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
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Ap.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "UPDATE"))
    {
      if (Length(TrimEnd(export.Placement.ZipCode)) > 0 && Length
        (TrimEnd(export.Placement.ZipCode)) < 5)
      {
        var field = GetField(export.Placement, "zipCode");

        field.Error = true;

        ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

        return;
      }

      if (Length(TrimEnd(export.Placement.ZipCode)) > 0 && Verify
        (export.Placement.ZipCode, "0123456789") != 0)
      {
        var field = GetField(export.Placement, "zipCode");

        field.Error = true;

        ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

        return;
      }

      if (Length(TrimEnd(export.Placement.ZipCode)) == 0 && Length
        (TrimEnd(export.Placement.Zip4)) > 0)
      {
        var field = GetField(export.Placement, "zipCode");

        field.Error = true;

        ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

        return;
      }

      if (Length(TrimEnd(export.Placement.ZipCode)) > 0 && Length
        (TrimEnd(export.Placement.Zip4)) > 0)
      {
        if (Length(TrimEnd(export.Placement.Zip4)) < 4)
        {
          var field = GetField(export.Placement, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

          return;
        }
        else if (Verify(export.Placement.Zip4, "0123456789") != 0)
        {
          var field = GetField(export.Placement, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }
      }

      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }

      if (AsChar(export.ActiveChild.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_INACTIVE_CHILD";

        return;
      }
    }

    if (Equal(global.Command, "RTLIST"))
    {
      // ---------------------------------------------
      // When the control is returned from a LIST screen
      // Populate the appropriate prompt fields.
      // ---------------------------------------------
      if (AsChar(export.PlacementPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.FcChildCaseRole.FcPlacementReason = import.Selected.Cdvalue;
        }

        export.PlacementPrompt.SelectChar = "";

        var field = GetField(export.PlacementPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.SourcePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.FcChildCaseRole.FcSourceOfFunding = import.Selected.Cdvalue;
        }

        export.SourcePrompt.SelectChar = "";

        var field = GetField(export.SourcePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.OrderEstPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.FcChildCaseRole.FcOrderEstBy = import.Selected.Cdvalue;
        }

        export.OrderEstPrompt.SelectChar = "";

        var field = GetField(export.OrderEstPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.PlaceStatePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.Placement.State = import.Selected.Cdvalue;
        }

        export.PlaceStatePrompt.SelectChar = "";

        var field = GetField(export.PlaceStatePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.FreqPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.FcChildCaseRole.FcCostOfCareFreq = import.Selected.Cdvalue;
        }

        export.FreqPrompt.SelectChar = "";

        var field = GetField(export.FreqPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.CountyPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.FcChildCaseRole.FcCountyChildRemovedFrom =
            import.Selected.Cdvalue;
        }

        export.CountyPrompt.SelectChar = "";

        var field = GetField(export.CountyPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      return;
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(export.ApPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.HiddenApSelected.Number))
        {
          export.Ap.Number = import.HiddenApSelected.Number;
        }

        export.ApPrompt.SelectChar = "";

        var field = GetField(export.ApPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(import.HiddenApSelected.Number))
      {
        if (!Equal(import.HiddenApSelected.Number,
          import.HiddenChSelected.Number))
        {
          export.Ap.Number = import.HiddenApSelected.Number;
        }
      }

      if (!IsEmpty(import.HiddenChSelected.Number))
      {
        export.FcChildCsePersonsWorkSet.Number = import.HiddenChSelected.Number;
      }

      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // ---------------------------------------------
        // This command allows the user to link to a
        // selection list and retrieve the appropriate
        // value, not losing any of the data already
        // entered.
        // ---------------------------------------------
        switch(AsChar(import.ApPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field = GetField(export.ApPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.PlacementPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "PLACEMENT";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PlacementPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.SourcePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "SOURCE OF FUNDING";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.SourcePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.OrderEstPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "ORDER ESTABLISHED BY";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.OrderEstPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.PlaceStatePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "STATE CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PlaceStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.FreqPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "SI FREQUENCY";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.FreqPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.CountyPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "COUNTY CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.CountyPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ApPrompt, "selectChar");

              field.Error = true;
            }

            // ---------------------------------------------
            // 02/09/99 W.Campbell - Disabled the
            // following if and make statements as they don't
            // appear to be needed for any reason.  The
            // view and attribute export_child_prompt
            // ief_supplied select_char don't appear to
            // be needed.
            // ---------------------------------------------
            if (AsChar(export.PlacementPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PlacementPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.SourcePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.SourcePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.OrderEstPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.OrderEstPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PlaceStatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PlaceStatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.FreqPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.FreqPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.CountyPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.CountyPrompt, "selectChar");

              field.Error = true;
            }

            break;
        }

        return;
      case "UPDATE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        if (!Equal(import.HiddenPreviousCase.Number, import.Case1.Number) || !
          Equal(import.HiddenPreviousCsePersonsWorkSet.Number,
          import.FcChildCsePersonsWorkSet.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ---------------------------------------------
        // All Non-Database validation should be done
        // here.  Do all validation as above.
        // Common action blocks (CABs) will be provided
        // for numeric validations on fields such as
        // date, amounts etc.
        // ---------------------------------------------
        // ---------------------------------------------
        // Validate the Previous PA
        // --------------------------------------------
        if (AsChar(import.FcChildCaseRole.FcPreviousPa) != 'Y' && AsChar
          (import.FcChildCaseRole.FcPreviousPa) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcPreviousPa))
        {
          var field = GetField(export.FcChildCaseRole, "fcPreviousPa");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        // ---------------------------------------------
        // Validate the Placement Reason
        // ---------------------------------------------
        if (!IsEmpty(import.FcChildCaseRole.FcPlacementReason))
        {
          local.Code.CodeName = "PLACEMENT";
          local.CodeValue.Cdvalue =
            import.FcChildCaseRole.FcPlacementReason ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field = GetField(export.FcChildCaseRole, "fcPlacementReason");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_PLACEMENT_REASON";
            }
          }
        }

        // ---------------------------------------------
        // Validate the Source of Funding
        // ---------------------------------------------
        if (!Equal(import.FcChildCaseRole.FcSourceOfFunding, "AF") && !
          Equal(import.FcChildCaseRole.FcSourceOfFunding, "GA") && !
          IsEmpty(import.FcChildCaseRole.FcSourceOfFunding))
        {
          var field = GetField(export.FcChildCaseRole, "fcSourceOfFunding");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SOURCE_OF_FUNDING";
          }
        }

        // ---------------------------------------------
        // Validate the CINC code
        // ---------------------------------------------
        switch(AsChar(import.FcChildCaseRole.FcCincInd))
        {
          case ' ':
            break;
          case 'Y':
            break;
          case 'N':
            break;
          default:
            var field = GetField(export.FcChildCaseRole, "fcCincInd");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
            }

            break;
        }

        // ---------------------------------------------
        // Validate the JO code
        // ---------------------------------------------
        switch(AsChar(import.FcChildCaseRole.FcJuvenileOffenderInd))
        {
          case ' ':
            break;
          case 'Y':
            break;
          case 'N':
            break;
          default:
            var field =
              GetField(export.FcChildCaseRole, "fcJuvenileOffenderInd");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
            }

            break;
        }

        if (AsChar(import.FcChildCaseRole.FcCincInd) == 'Y' && AsChar
          (import.FcChildCaseRole.FcJuvenileOffenderInd) == 'Y')
        {
          var field1 = GetField(export.FcChildCaseRole, "fcCincInd");

          field1.Error = true;

          var field2 =
            GetField(export.FcChildCaseRole, "fcJuvenileOffenderInd");

          field2.Error = true;

          ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";
        }

        // ---------------------------------------------
        // Validate the Order Established By code
        // ---------------------------------------------
        if (!IsEmpty(import.FcChildCaseRole.FcOrderEstBy))
        {
          local.Code.CodeName = "ORDER ESTABLISHED BY";
          local.CodeValue.Cdvalue = import.FcChildCaseRole.FcOrderEstBy ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field = GetField(export.FcChildCaseRole, "fcOrderEstBy");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_ORDER_ESTABLISHED_BY_CD";
            }
          }
        }

        // ---------------------------------------------
        // Validate the Next Juvenile Court Date
        // ---------------------------------------------
        if (Lt(import.FcChildCaseRole.FcNextJuvenileCtDt, Now().Date) && !
          Equal(import.FcChildCaseRole.FcNextJuvenileCtDt, null))
        {
          var field = GetField(export.FcChildCaseRole, "fcNextJuvenileCtDt");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------
            // 05/26/99 W.Campbell -  Replaced zd exit states.
            // -----------------------------------------------
            ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";
          }
        }

        // ---------------------------------------------
        // Validate the Placement State
        // ---------------------------------------------
        if (!IsEmpty(import.Placement.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.Placement.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field = GetField(export.Placement, "state");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }

        // ---------------------------------------------
        // Validate the Frequency
        // ---------------------------------------------
        if (!IsEmpty(import.FcChildCaseRole.FcCostOfCareFreq))
        {
          local.Code.CodeName = "SI FREQUENCY";
          local.CodeValue.Cdvalue = import.FcChildCaseRole.FcCostOfCareFreq ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_INVALID_FREQ_CODE";
            }

            var field = GetField(export.FcChildCaseRole, "fcCostOfCareFreq");

            field.Error = true;
          }
        }

        if (IsEmpty(export.FcChildCaseRole.FcCostOfCareFreq) && export
          .FcChildCaseRole.FcCostOfCare.GetValueOrDefault() != 0)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FREQ_REQUIRED_WHEN_AMT_ENTERED";
          }

          var field = GetField(export.FcChildCaseRole, "fcCostOfCareFreq");

          field.Error = true;
        }

        // ---------------------------------------------
        // Validate the Type of Benefits codes
        // ---------------------------------------------
        if (AsChar(import.FcChildCaseRole.FcSsa) != 'Y' && AsChar
          (import.FcChildCaseRole.FcSsa) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcSsa))
        {
          var field = GetField(export.FcChildCaseRole, "fcSsa");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        if (AsChar(import.FcChildCaseRole.FcSsi) != 'Y' && AsChar
          (import.FcChildCaseRole.FcSsi) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcSsi))
        {
          var field = GetField(export.FcChildCaseRole, "fcSsi");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        if (AsChar(import.FcChildCaseRole.FcVaInd) != 'Y' && AsChar
          (import.FcChildCaseRole.FcVaInd) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcVaInd))
        {
          var field = GetField(export.FcChildCaseRole, "fcVaInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        if (AsChar(import.FcChildCaseRole.FcZebInd) != 'Y' && AsChar
          (import.FcChildCaseRole.FcZebInd) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcZebInd))
        {
          var field = GetField(export.FcChildCaseRole, "fcZebInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        if (AsChar(import.FcChildCaseRole.FcOtherBenefitInd) != 'Y' && AsChar
          (import.FcChildCaseRole.FcOtherBenefitInd) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcOtherBenefitInd))
        {
          var field = GetField(export.FcChildCaseRole, "fcOtherBenefitInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        if (AsChar(import.FcChildCaseRole.FcWardsAccount) != 'Y' && AsChar
          (import.FcChildCaseRole.FcWardsAccount) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcWardsAccount))
        {
          var field = GetField(export.FcChildCaseRole, "fcWardsAccount");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        // ---------------------------------------------
        // Validate the SRS Payee code
        // ---------------------------------------------
        if (AsChar(import.FcChildCaseRole.FcSrsPayee) != 'Y' && AsChar
          (import.FcChildCaseRole.FcSrsPayee) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcSrsPayee))
        {
          var field = GetField(export.FcChildCaseRole, "fcSrsPayee");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        // ---------------------------------------------
        // Validate the AP Notified of Responsibility
        // to pay code
        // ---------------------------------------------
        if (AsChar(import.FcChildCaseRole.FcApNotified) != 'Y' && AsChar
          (import.FcChildCaseRole.FcApNotified) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcApNotified))
        {
          var field = GetField(export.FcChildCaseRole, "fcApNotified");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        // ---------------------------------------------
        // Validate the County Child Removed from
        // ---------------------------------------------
        if (!IsEmpty(import.FcChildCaseRole.FcCountyChildRemovedFrom))
        {
          local.Code.CodeName = "COUNTY CODE";
          local.CodeValue.Cdvalue =
            import.FcChildCaseRole.FcCountyChildRemovedFrom ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field =
              GetField(export.FcChildCaseRole, "fcCountyChildRemovedFrom");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_COUNTY";
            }
          }
        }

        // ---------------------------------------------
        // Validate the Adoption Disruption code
        // ---------------------------------------------
        if (AsChar(import.FcChildCaseRole.FcAdoptionDisruptionInd) != 'Y' && AsChar
          (import.FcChildCaseRole.FcAdoptionDisruptionInd) != 'N' && !
          IsEmpty(import.FcChildCaseRole.FcAdoptionDisruptionInd))
        {
          var field =
            GetField(export.FcChildCaseRole, "fcAdoptionDisruptionInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.CsePerson.Number = import.FcChildCsePersonsWorkSet.Number;
        UseSiUpdateCaseRole();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // 02/09/99 W.Campbell - Added code to USE
          // EAB_ROLLBACK_CICS for correct DB/2
          // update and rollback operation.
          // ---------------------------------------------
          UseEabRollbackCics();

          return;
        }

        if (Equal(export.Placement.Identifier, local.Null1.Identifier))
        {
          export.Placement.Type1 = "R";
          export.Placement.LocationType = "D";

          // -------------------------------------------
          // 01/13/99 W.Campbell - Removed set statement
          // for ZDEL_START_DATE of entity type
          // CSE_PERSON_ADDRESS.  Setting it to
          // CURRENT_DATE. Work done on IDCR454.
          // -----------------------------------------
          export.Placement.EndDate = UseCabSetMaximumDiscontinueDate();
          UseSiCreateCsePersonAddress();
        }
        else
        {
          UseSiUpdateCsePersonAddress();
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // 02/09/99 W.Campbell - Added code to USE
          // EAB_ROLLBACK_CICS for correct DB/2
          // update and rollback operation.
          // ---------------------------------------------
          UseEabRollbackCics();

          return;
        }

        UseSiUpdateCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // 02/09/99 W.Campbell - Added code to USE
          // EAB_ROLLBACK_CICS for correct DB/2
          // update and rollback operation.
          // ---------------------------------------------
          UseEabRollbackCics();

          return;
        }

        if (AsChar(import.HiddenAe.Flag) != 'O')
        {
          UseCabUpdateAdabasPerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            // ---------------------------------------------
            // 02/09/99 W.Campbell - Added code to USE
            // EAB_ROLLBACK_CICS for correct DB/2
            // update and rollback operation.
            // ---------------------------------------------
            UseEabRollbackCics();

            return;
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DISPLAY":
        export.ApPrompt.SelectChar = "";

        // ---------------------------------------------
        // 02/09/99 W.Campbell - Disabled the
        // following set statement as it doesn't
        // appear to be needed for any reason.
        // set export_child_prompt ief_supplied
        //              select_char to spaces.
        // ---------------------------------------------
        export.PlacementPrompt.SelectChar = "";
        export.SourcePrompt.SelectChar = "";
        export.OrderEstPrompt.SelectChar = "";
        export.PlaceStatePrompt.SelectChar = "";
        export.FreqPrompt.SelectChar = "";
        export.CountyPrompt.SelectChar = "";

        if (!IsEmpty(export.Next.Number))
        {
          UseCabZeroFillNumber();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
            {
              var field = GetField(export.Next, "number");

              field.Error = true;

              return;
            }
          }
        }
        else
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "CASE_NUMBER_REQUIRED";

          return;
        }

        if (IsEmpty(export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
        }

        if (!Equal(export.Next.Number, export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
          export.Ap.Number = "";
          export.FcChildCsePersonsWorkSet.Number = "";
        }

        if (!IsEmpty(export.Ap.Number))
        {
          local.TextWorkArea.Text10 = export.Ap.Number;
          UseEabPadLeftWithZeros();
          export.Ap.Number = local.TextWorkArea.Text10;
        }

        UseSiReadCaseHeaderInformation();

        if (!IsEmpty(local.AbendData.Type1))
        {
          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          return;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.Hidden.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        if (IsExitState("NO_APS_ON_A_CASE"))
        {
          // 12/21/99 M.L Start
          ExitState = "ACO_NN0000_ALL_OK";

          // 12/21/99 M.L End
          local.NoAp.Flag = "Y";
        }
        else
        {
          local.NoAp.Flag = "N";
        }

        UseSiReadOfficeOspHeader();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          return;
        }

        if (IsEmpty(export.FcChildCsePersonsWorkSet.Number))
        {
          UseSiRetrieveChildForCase();

          if (AsChar(local.MultipleAps.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }

          if (IsEmpty(export.FcChildCsePersonsWorkSet.Number))
          {
            var field = GetField(export.FcChildCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "CASE_ROLE_CHILD_NF";
          }
        }

        UseSiReadChildDetails();

        if (IsExitState("CSE_PERSON_NF"))
        {
          ExitState = "FOSTER_CARE_CHILD_NF";
        }

        if (IsEmpty(local.AbendData.Type1))
        {
        }
        else
        {
          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ----------------------------------------------------------------------
          // Now call the Family Violence CAB and pass the data to the CAB to 
          // check if the CHILD has  family violence Flag set.
          //                                                
          // Vithal(08/27/2001)
          // ----------------------------------------------------------------------
          UseScSecurityCheckForFv();

          if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
          {
            export.FcChildCaseRole.FcIvECaseNumber = "";
            export.FcChildCaseRole.FcPreviousPa = "";
            export.FcChildCaseRole.FcPlacementReason = "";
            export.FcChildCaseRole.FcSourceOfFunding = "";
            export.FcChildCaseRole.FcCincInd = "";
            export.FcChildCaseRole.FcJuvenileOffenderInd = "";
            export.FcChildCaseRole.FcJuvenileCourtOrder = "";
            export.FcChildCaseRole.FcOrderEstBy = "";
            export.FcChildCaseRole.FcNextJuvenileCtDt = local.Blank.Date;
            export.FcChildCaseRole.FcPlacementName = "";
            export.FcChildCaseRole.FcPlacementDate = local.Blank.Date;
            export.FcChildCaseRole.FcDateOfInitialCustody = local.Blank.Date;
            export.FcChildCaseRole.FcCostOfCare = 0;
            export.FcChildCaseRole.FcCostOfCareFreq = "";
            export.FcChildCaseRole.FcSsa = "";
            export.FcChildCaseRole.FcSsi = "";
            export.FcChildCaseRole.FcVaInd = "";
            export.FcChildCaseRole.FcZebInd = "";
            export.FcChildCaseRole.FcOtherBenefitInd = "";
            export.FcChildCaseRole.FcWardsAccount = "";
            export.FcChildCaseRole.FcApNotified = "";
            export.FcChildCaseRole.FcSrsPayee = "";
            export.FcChildCaseRole.FcCountyChildRemovedFrom = "";
            export.FcChildCaseRole.FcAdoptionDisruptionInd = "";
            export.FcChildCaseRole.Note = Spaces(CaseRole.Note_MaxLength);
            export.Placement.Assign(local.Placement);

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          if (AsChar(local.NoAp.Flag) == 'Y')
          {
            ExitState = "NO_APS_ON_A_CASE";
          }

          if (AsChar(export.ActiveChild.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_INACTIVE_CHILD";
          }

          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // ---------------------------------------------
    // If all processing completed successfully,
    // move all imports to previous exports .
    // --------------------------------------------
    export.HiddenPreviousCase.Number = export.Case1.Number;
    export.HiddenPreviousCsePersonsWorkSet.Number =
      export.FcChildCsePersonsWorkSet.Number;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.AbsenceReasonCode = source.AbsenceReasonCode;
    target.PriorMedicalSupport = source.PriorMedicalSupport;
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.DateOfEmancipation = source.DateOfEmancipation;
    target.FcAdoptionDisruptionInd = source.FcAdoptionDisruptionInd;
    target.FcApNotified = source.FcApNotified;
    target.FcCincInd = source.FcCincInd;
    target.FcCostOfCare = source.FcCostOfCare;
    target.FcCostOfCareFreq = source.FcCostOfCareFreq;
    target.FcCountyChildRemovedFrom = source.FcCountyChildRemovedFrom;
    target.FcDateOfInitialCustody = source.FcDateOfInitialCustody;
    target.FcInHomeServiceInd = source.FcInHomeServiceInd;
    target.FcIvECaseNumber = source.FcIvECaseNumber;
    target.FcJuvenileCourtOrder = source.FcJuvenileCourtOrder;
    target.FcJuvenileOffenderInd = source.FcJuvenileOffenderInd;
    target.FcLevelOfCare = source.FcLevelOfCare;
    target.FcNextJuvenileCtDt = source.FcNextJuvenileCtDt;
    target.FcOrderEstBy = source.FcOrderEstBy;
    target.FcOtherBenefitInd = source.FcOtherBenefitInd;
    target.FcParentalRights = source.FcParentalRights;
    target.FcPrevPayeeFirstName = source.FcPrevPayeeFirstName;
    target.FcPrevPayeeMiddleInitial = source.FcPrevPayeeMiddleInitial;
    target.FcPlacementDate = source.FcPlacementDate;
    target.FcPlacementName = source.FcPlacementName;
    target.FcPlacementReason = source.FcPlacementReason;
    target.FcPreviousPa = source.FcPreviousPa;
    target.FcPreviousPayeeLastName = source.FcPreviousPayeeLastName;
    target.FcSourceOfFunding = source.FcSourceOfFunding;
    target.FcSrsPayee = source.FcSrsPayee;
    target.FcSsa = source.FcSsa;
    target.FcSsi = source.FcSsi;
    target.FcVaInd = source.FcVaInd;
    target.FcWardsAccount = source.FcWardsAccount;
    target.FcZebInd = source.FcZebInd;
    target.Over18AndInSchool = source.Over18AndInSchool;
    target.ResidesWithArIndicator = source.ResidesWithArIndicator;
    target.SpecialtyArea = source.SpecialtyArea;
    target.RelToAr = source.RelToAr;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.SendDate = source.SendDate;
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
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
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
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabUpdateAdabasPerson()
  {
    var useImport = new CabUpdateAdabasPerson.Import();
    var useExport = new CabUpdateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.FcChildCsePersonsWorkSet);

    Call(CabUpdateAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Invalid.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
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

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.Case1.Number = export.Next.Number;
    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.Case1.Number = export.Next.Number;
    useImport.CsePersonsWorkSet.Number = export.FcChildCsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonAddress()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress2(export.Placement, useImport.CsePersonAddress);

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, export.Placement);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.Ap.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.Ap.Assign(useExport.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseSiReadChildDetails()
  {
    var useImport = new SiReadChildDetails.Import();
    var useExport = new SiReadChildDetails.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Child.Number = export.FcChildCsePersonsWorkSet.Number;

    Call(SiReadChildDetails.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.HiddenAe.Flag = useExport.Ae.Flag;
    export.FcChildCsePersonsWorkSet.Assign(useExport.ChildCsePersonsWorkSet);
    export.FcChildCaseRole.Assign(useExport.ChildCaseRole);
    export.Placement.Assign(useExport.ChildFc);
    export.ActiveChild.Flag = useExport.ActiveChild.Flag;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
  }

  private void UseSiRetrieveChildForCase()
  {
    var useImport = new SiRetrieveChildForCase.Import();
    var useExport = new SiRetrieveChildForCase.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CaseOpen.Flag = export.CaseOpen.Flag;

    Call(SiRetrieveChildForCase.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleChildren.Flag;
    export.FcChildCsePersonsWorkSet.Number = useExport.Child.Number;
  }

  private void UseSiUpdateCaseRole()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.Case1.Number = import.Case1.Number;
    MoveCaseRole(import.FcChildCaseRole, useImport.CaseRole);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    MoveCsePerson(local.CsePerson, useImport.CsePerson);
    useImport.CsePersonsWorkSet.Assign(export.FcChildCsePersonsWorkSet);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePersonAddress()
  {
    var useImport = new SiUpdateCsePersonAddress.Import();
    var useExport = new SiUpdateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress2(export.Placement, useImport.CsePersonAddress);

    Call(SiUpdateCsePersonAddress.Execute, useImport, useExport);
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
    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of HiddenChSelected.
    /// </summary>
    [JsonPropertyName("hiddenChSelected")]
    public CsePersonsWorkSet HiddenChSelected
    {
      get => hiddenChSelected ??= new();
      set => hiddenChSelected = value;
    }

    /// <summary>
    /// A value of HiddenApSelected.
    /// </summary>
    [JsonPropertyName("hiddenApSelected")]
    public CsePersonsWorkSet HiddenApSelected
    {
      get => hiddenApSelected ??= new();
      set => hiddenApSelected = value;
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
    /// A value of OrderEstPrompt.
    /// </summary>
    [JsonPropertyName("orderEstPrompt")]
    public Common OrderEstPrompt
    {
      get => orderEstPrompt ??= new();
      set => orderEstPrompt = value;
    }

    /// <summary>
    /// A value of PlaceStatePrompt.
    /// </summary>
    [JsonPropertyName("placeStatePrompt")]
    public Common PlaceStatePrompt
    {
      get => placeStatePrompt ??= new();
      set => placeStatePrompt = value;
    }

    /// <summary>
    /// A value of FreqPrompt.
    /// </summary>
    [JsonPropertyName("freqPrompt")]
    public Common FreqPrompt
    {
      get => freqPrompt ??= new();
      set => freqPrompt = value;
    }

    /// <summary>
    /// A value of CountyPrompt.
    /// </summary>
    [JsonPropertyName("countyPrompt")]
    public Common CountyPrompt
    {
      get => countyPrompt ??= new();
      set => countyPrompt = value;
    }

    /// <summary>
    /// A value of PlacementPrompt.
    /// </summary>
    [JsonPropertyName("placementPrompt")]
    public Common PlacementPrompt
    {
      get => placementPrompt ??= new();
      set => placementPrompt = value;
    }

    /// <summary>
    /// A value of SourcePrompt.
    /// </summary>
    [JsonPropertyName("sourcePrompt")]
    public Common SourcePrompt
    {
      get => sourcePrompt ??= new();
      set => sourcePrompt = value;
    }

    /// <summary>
    /// A value of ChildPrompt.
    /// </summary>
    [JsonPropertyName("childPrompt")]
    public Common ChildPrompt
    {
      get => childPrompt ??= new();
      set => childPrompt = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPreviousCsePersonsWorkSet
    {
      get => hiddenPreviousCsePersonsWorkSet ??= new();
      set => hiddenPreviousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCase.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCase")]
    public Case1 HiddenPreviousCase
    {
      get => hiddenPreviousCase ??= new();
      set => hiddenPreviousCase = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of FcChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fcChildCsePersonsWorkSet")]
    public CsePersonsWorkSet FcChildCsePersonsWorkSet
    {
      get => fcChildCsePersonsWorkSet ??= new();
      set => fcChildCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FcChildCaseRole.
    /// </summary>
    [JsonPropertyName("fcChildCaseRole")]
    public CaseRole FcChildCaseRole
    {
      get => fcChildCaseRole ??= new();
      set => fcChildCaseRole = value;
    }

    /// <summary>
    /// A value of Placement.
    /// </summary>
    [JsonPropertyName("placement")]
    public CsePersonAddress Placement
    {
      get => placement ??= new();
      set => placement = value;
    }

    /// <summary>
    /// A value of HiddenAe.
    /// </summary>
    [JsonPropertyName("hiddenAe")]
    public Common HiddenAe
    {
      get => hiddenAe ??= new();
      set => hiddenAe = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    private WorkArea headerLine;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePersonsWorkSet hiddenChSelected;
    private CsePersonsWorkSet hiddenApSelected;
    private CodeValue selected;
    private Common orderEstPrompt;
    private Common placeStatePrompt;
    private Common freqPrompt;
    private Common countyPrompt;
    private Common placementPrompt;
    private Common sourcePrompt;
    private Common childPrompt;
    private Common apPrompt;
    private CsePersonsWorkSet hiddenPreviousCsePersonsWorkSet;
    private Case1 hiddenPreviousCase;
    private Standard standard;
    private Case1 next;
    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet fcChildCsePersonsWorkSet;
    private CaseRole fcChildCaseRole;
    private CsePersonAddress placement;
    private Common hiddenAe;
    private NextTranInfo hidden;
    private Common caseOpen;
    private Common activeChild;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of HiddenChSelected.
    /// </summary>
    [JsonPropertyName("hiddenChSelected")]
    public CsePersonsWorkSet HiddenChSelected
    {
      get => hiddenChSelected ??= new();
      set => hiddenChSelected = value;
    }

    /// <summary>
    /// A value of HiddenApSelected.
    /// </summary>
    [JsonPropertyName("hiddenApSelected")]
    public CsePersonsWorkSet HiddenApSelected
    {
      get => hiddenApSelected ??= new();
      set => hiddenApSelected = value;
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
    /// A value of OrderEstPrompt.
    /// </summary>
    [JsonPropertyName("orderEstPrompt")]
    public Common OrderEstPrompt
    {
      get => orderEstPrompt ??= new();
      set => orderEstPrompt = value;
    }

    /// <summary>
    /// A value of PlaceStatePrompt.
    /// </summary>
    [JsonPropertyName("placeStatePrompt")]
    public Common PlaceStatePrompt
    {
      get => placeStatePrompt ??= new();
      set => placeStatePrompt = value;
    }

    /// <summary>
    /// A value of FreqPrompt.
    /// </summary>
    [JsonPropertyName("freqPrompt")]
    public Common FreqPrompt
    {
      get => freqPrompt ??= new();
      set => freqPrompt = value;
    }

    /// <summary>
    /// A value of CountyPrompt.
    /// </summary>
    [JsonPropertyName("countyPrompt")]
    public Common CountyPrompt
    {
      get => countyPrompt ??= new();
      set => countyPrompt = value;
    }

    /// <summary>
    /// A value of PlacementPrompt.
    /// </summary>
    [JsonPropertyName("placementPrompt")]
    public Common PlacementPrompt
    {
      get => placementPrompt ??= new();
      set => placementPrompt = value;
    }

    /// <summary>
    /// A value of SourcePrompt.
    /// </summary>
    [JsonPropertyName("sourcePrompt")]
    public Common SourcePrompt
    {
      get => sourcePrompt ??= new();
      set => sourcePrompt = value;
    }

    /// <summary>
    /// A value of ChildPrompt.
    /// </summary>
    [JsonPropertyName("childPrompt")]
    public Common ChildPrompt
    {
      get => childPrompt ??= new();
      set => childPrompt = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of HiddenAe.
    /// </summary>
    [JsonPropertyName("hiddenAe")]
    public Common HiddenAe
    {
      get => hiddenAe ??= new();
      set => hiddenAe = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPreviousCsePersonsWorkSet
    {
      get => hiddenPreviousCsePersonsWorkSet ??= new();
      set => hiddenPreviousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCase.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCase")]
    public Case1 HiddenPreviousCase
    {
      get => hiddenPreviousCase ??= new();
      set => hiddenPreviousCase = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of FcChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fcChildCsePersonsWorkSet")]
    public CsePersonsWorkSet FcChildCsePersonsWorkSet
    {
      get => fcChildCsePersonsWorkSet ??= new();
      set => fcChildCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FcChildCaseRole.
    /// </summary>
    [JsonPropertyName("fcChildCaseRole")]
    public CaseRole FcChildCaseRole
    {
      get => fcChildCaseRole ??= new();
      set => fcChildCaseRole = value;
    }

    /// <summary>
    /// A value of Placement.
    /// </summary>
    [JsonPropertyName("placement")]
    public CsePersonAddress Placement
    {
      get => placement ??= new();
      set => placement = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    private WorkArea headerLine;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePersonsWorkSet hiddenChSelected;
    private CsePersonsWorkSet hiddenApSelected;
    private Code prompt;
    private Common orderEstPrompt;
    private Common placeStatePrompt;
    private Common freqPrompt;
    private Common countyPrompt;
    private Common placementPrompt;
    private Common sourcePrompt;
    private Common childPrompt;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet hiddenPreviousCsePersonsWorkSet;
    private Case1 hiddenPreviousCase;
    private Standard standard;
    private Case1 next;
    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet fcChildCsePersonsWorkSet;
    private CaseRole fcChildCaseRole;
    private CsePersonAddress placement;
    private NextTranInfo hidden;
    private Common caseOpen;
    private Common activeChild;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Placement.
    /// </summary>
    [JsonPropertyName("placement")]
    public CsePersonAddress Placement
    {
      get => placement ??= new();
      set => placement = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of NoAp.
    /// </summary>
    [JsonPropertyName("noAp")]
    public Common NoAp
    {
      get => noAp ??= new();
      set => noAp = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CsePersonAddress Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of ReturnEab.
    /// </summary>
    [JsonPropertyName("returnEab")]
    public Common ReturnEab
    {
      get => returnEab ??= new();
      set => returnEab = value;
    }

    /// <summary>
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private CsePersonAddress placement;
    private DateWorkArea blank;
    private Common noAp;
    private Common multipleAps;
    private CsePersonAddress null1;
    private AbendData abendData;
    private Common returnEab;
    private Common invalid;
    private CodeValue codeValue;
    private Code code;
    private CsePerson csePerson;
    private Common common;
    private TextWorkArea textWorkArea;
  }
#endregion
}

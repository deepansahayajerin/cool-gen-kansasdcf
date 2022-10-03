// Program: SI_IREQ_INTERSTATE_REQ_HISTORY, ID: 372388762, model: 746.
// Short name: SWEIREQP
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
/// A program: SI_IREQ_INTERSTATE_REQ_HISTORY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIreqInterstateReqHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IREQ_INTERSTATE_REQ_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIreqInterstateReqHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIreqInterstateReqHistory.
  /// </summary>
  public SiIreqInterstateReqHistory(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 	  Sid Chowdhary		Initial Development
    // 11/05/96  G. Lofton		Add new security.
    // 11/06/97  Sid			Add description for converted cases.
    // 01/25/99  C. Ott                Refresh screen between displays
    //                                 
    // Fix problem with multiple prompt
    // selects
    // 03/22/99  C. Ott                Added Transaction Date to Export view to
    //                                 
    // ICAS
    // 03/24/99  C. Ott                Display all information on closed cases.
    // 11/17/00 M.Lachowicz   WR 298.  Create header information for screens.
    // 02/27/01 C Fairley  I00114726   Stopped flow to ICAS, when the Referral #
    // is ZERO
    //                                 
    // (blank on the screen)
    // 03/08/01 C Fairley  I00114997   Reset the EXIT STATE to 
    // aco_nn0000_all_ok, after
    //                                 
    // returning from
    // SI_READ_CASE_HEADER_INFORMATION,
    //                                 
    // when the EXIT STATE is equal to
    // Invalid_Case_Role.
    // 05/14/01 C Fairley  I00119237   After arriving via automatic flow from 
    // IIMC
    //                                 
    // (multiple Interstate Requests
    // exist), a selection
    //                                 
    // must be made before returning (
    // PF9) to IIMC.
    //                                 
    // Removed OLD commented out code.
    // 05/30/01 C Fairley  I00120592   Added a new flow from IATT.
    //                                 
    // After arriving via automatic
    // flow from IATT
    //                                 
    // (multiple Interstate Requests
    // exist), a selection
    //                                 
    // must be made before returning (
    // PF9) to IATT.
    // 03/14/02 M Ashworth WR10502     Added resend PFKEY
    // 02/25/04 L Brown    PR00200536  Use Created_Timestamp date instead of 
    // transaction date on IREQ screen.
    // 05/09/06 GVandy	    WR230751	1. Change F16 IIIN to F16 IIMC
    // 				2. Add support for Tribal IV-D Agencies.
    // 07/07/11	T Pierce	CQ 314, CQ 400, CQ 463	Changes to remove scrolling 
    // limitation,
    // 				make screen more beneficial to users.
    // ----------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    UseOeCabSetMnemonics();

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Allow the user to clear the screen
      // ---------------------------------------------
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.More.Flag = import.More.Flag;
    MoveCommon(import.PrevCommon, export.PrevCommon);
    export.HiddenNext.Number = import.HiddenNext.Number;
    export.HiddenSearchInterstateRequestHistory.Assign(
      import.HiddenSearchInterstateRequestHistory);
    export.HiddenSearchFips.StateAbbreviation =
      import.HiddenSearchFips.StateAbbreviation;
    export.NextInterstateRequestHistory.CreatedTimestamp =
      import.NextInterstateRequestHistory.CreatedTimestamp;
    export.PrevInterstateRequestHistory.CreatedTimestamp =
      import.PrevInterstateRequestHistory.CreatedTimestamp;
    export.Minus.Text1 = import.Minus.Text1;
    export.Plus.Text1 = import.Plus.Text1;
    export.NextCase.Number = import.NextCase.Number;
    export.OspServiceProvider.LastName = import.OspServiceProvider.LastName;
    MoveOffice(import.OspOffice, export.OspOffice);
    export.DisplayOnly.Number = import.DisplayOnly.Number;
    MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet, export.ApCsePersonsWorkSet);
      
    export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet, export.ArCsePersonsWorkSet);
      
    export.ArCsePerson.Number = export.ArCsePersonsWorkSet.Number;
    export.SearchInterstateRequestHistory.Assign(
      import.SearchInterstateRequestHistory);
    export.SearchFips.StateAbbreviation = import.SearchFips.StateAbbreviation;
    MoveInterstateRequest(import.InterstateRequest, export.InterstateRequest);
    export.PromptActionCd.SelectChar = import.PromptActionCd.SelectChar;
    export.PromptFunctionCd.SelectChar = import.PromptFunctionCd.SelectChar;
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;
    export.PromptReason.SelectChar = import.PromptReason.SelectChar;
    export.PromptState.SelectChar = import.PromptState.SelectChar;

    // 11/17/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/17/00 M.L End
    // *** Problem report I00119237
    // *** 05/14/01 swsrchf
    // *** start
    export.AutoFlow.Flag = import.AutoFlow.Flag;

    // *** end
    // *** 05/14/01 swsrchf
    // *** Problem report I00119237
    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Select.SelectChar =
        import.Import1.Item.Select.SelectChar;
      export.Export1.Update.DetailInterstateRequest.Assign(
        import.Import1.Item.DetailInterstateRequest);
      MoveFips(import.Import1.Item.State, export.Export1.Update.State);
      export.Export1.Update.DetailInterstateRequestHistory.Assign(
        import.Import1.Item.DetailInterstateRequestHistory);
      export.Export1.Update.StatusCd.Text4 = import.Import1.Item.StatusCd.Text4;
      MoveCodeValue(import.Import1.Item.ActionReason,
        export.Export1.Update.ActionReason);
      MoveTextWorkArea(import.Import1.Item.IvdAgency,
        export.Export1.Update.IvdAgency);

      if (Equal(import.Import1.Item.StatusCd.Text4, "ERR"))
      {
        var field = GetField(export.Export1.Item.StatusCd, "text4");

        field.Color = "red";
        field.Protected = true;
      }

      if (!IsEmpty(export.Export1.Item.Select.SelectChar))
      {
        ++local.NoOfSelects.Count;

        if (AsChar(export.Export1.Item.Select.SelectChar) != 'S')
        {
          var field = GetField(export.Export1.Item.Select, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
      }
    }

    import.Import1.CheckIndex();

    if (local.NoOfSelects.Count > 1 && !Equal(global.Command, "RESEND"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Export1.Item.Select.SelectChar))
        {
          var field = GetField(export.Export1.Item.Select, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
      }

      export.Export1.CheckIndex();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet,
        export.ApCsePersonsWorkSet);
      MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet,
        export.ArCsePersonsWorkSet);

      return;
    }

    if (!IsEmpty(export.NextCase.Number))
    {
      local.ZeroFill.Text10 = export.NextCase.Number;
      UseEabPadLeftWithZeros();
      export.NextCase.Number = local.ZeroFill.Text10;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet,
        export.ApCsePersonsWorkSet);
      MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet,
        export.ArCsePersonsWorkSet);

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.DisplayOnly.Number;
      export.Hidden.CsePersonNumberAp = export.ApCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet,
        export.ApCsePersonsWorkSet);
      MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet,
        export.ArCsePersonsWorkSet);

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ApCsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
        export.NextCase.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.DisplayOnly.Number = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // When control is returned from the Code List.
    // ---------------------------------------------
    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(export.PromptFunctionCd.SelectChar))
      {
        export.SearchInterstateRequestHistory.FunctionalTypeCode =
          import.SelectedCodeValue.Cdvalue;
      }
      else if (!IsEmpty(export.PromptActionCd.SelectChar))
      {
        export.SearchInterstateRequestHistory.ActionCode =
          import.SelectedCodeValue.Cdvalue;
      }
      else if (!IsEmpty(export.PromptReason.SelectChar))
      {
        export.SearchInterstateRequestHistory.ActionReasonCode =
          import.SelectedCodeValue.Cdvalue;
      }
      else if (!IsEmpty(export.PromptState.SelectChar))
      {
        export.SearchFips.StateAbbreviation = import.SelectedCodeValue.Cdvalue;
      }

      export.PromptActionCd.SelectChar = "";
      export.PromptFunctionCd.SelectChar = "";
      export.PromptReason.SelectChar = "";
      export.PromptState.SelectChar = "";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // When control is returned from the COMP screen.
    // ---------------------------------------------
    if (Equal(global.Command, "RETCOMP"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        export.ApCsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
        MoveCsePersonsWorkSet(import.SelectedCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
      }

      export.PromptPerson.SelectChar = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "LNK_IIOI"))
    {
      export.PrevCommon.Command = "LNK_IIOI";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "ICAS") || Equal(global.Command, "IIIN") || Equal
      (global.Command, "OINR") || Equal(global.Command, "IIOI") || Equal
      (global.Command, "LNK_OINR") || Equal(global.Command, "IIMC"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "LNK_OINR"))
    {
      export.PrevCommon.Command = "LNK_OINR";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        // *** Problem report I00119237
        // *** 05/14/01 swsrchf
        // *** start
        local.SelectionMade.Flag = "N";

        // *** end
        // *** 05/14/01 swsrchf
        // *** Problem report I00119237
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            if (Equal(export.PrevCommon.Command, "LNK_IIOI"))
            {
              if (!IsEmpty(export.Export1.Item.DetailInterstateRequest.Country) ||
                !
                IsEmpty(export.Export1.Item.DetailInterstateRequest.TribalAgency))
                
              {
                ExitState = "ACO_NE0000_NOT_DOMESTIC_CASE";

                var field = GetField(export.Export1.Item.Select, "selectChar");

                field.Error = true;

                return;
              }
            }

            export.SelectedInterstateRequest.Assign(
              export.Export1.Item.DetailInterstateRequest);
            export.SelectedFips.StateAbbreviation =
              export.Export1.Item.State.StateAbbreviation;
            export.SelectedInterstateRequestHistory.Assign(
              export.Export1.Item.DetailInterstateRequestHistory);

            // *** Problem report I00119237
            // *** 05/14/01 swsrchf
            // *** start
            local.SelectionMade.Flag = "Y";

            // *** end
            // *** 05/14/01 swsrchf
            // *** Problem report I00119237
          }
        }

        export.Export1.CheckIndex();

        if (Equal(export.PrevCommon.Command, "LNK_OINR"))
        {
          export.AutoFlow.Flag = "Y";
        }

        // *** Problem report I00119237
        // *** 05/14/01 swsrchf
        // *** start
        if (AsChar(export.AutoFlow.Flag) == 'Y' && AsChar
          (local.SelectionMade.Flag) == 'N')
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        // *** end
        // *** 05/14/01 swsrchf
        // *** Problem report I00119237
        if (Equal(export.PrevCommon.Command, "LNK_OINR"))
        {
          ExitState = "ECO_XFR_TO_OINR";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "":
        break;
      case "IIOI":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.NextCase.Number))
        {
          var field = GetField(export.NextCase, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            export.SelectedInterstateRequestHistory.Assign(
              export.Export1.Item.DetailInterstateRequestHistory);
            ExitState = "ECO_XFR_TO_IIOI";

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "OINR":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.NextCase.Number))
        {
          var field = GetField(export.NextCase, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          break;
        }

        if (export.Export1.Count == 0)
        {
          ExitState = "ECO_XFR_TO_OINR";
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
            {
              export.SelectedInterstateRequest.Assign(
                export.Export1.Item.DetailInterstateRequest);
              export.SelectedFips.StateAbbreviation =
                export.Export1.Item.State.StateAbbreviation;
              ExitState = "ECO_XFR_TO_OINR";

              return;
            }
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "IIMC":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.NextCase.Number))
        {
          var field = GetField(export.NextCase, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          break;
        }

        if (export.Export1.Count == 0)
        {
          ExitState = "ECO_LNK_TO_IIMC";
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
            {
              export.SelectedInterstateRequest.Assign(
                export.Export1.Item.DetailInterstateRequest);
              export.SelectedFips.StateAbbreviation =
                export.Export1.Item.State.StateAbbreviation;
              export.SelectedInterstateRequestHistory.Assign(
                export.Export1.Item.DetailInterstateRequestHistory);
              ExitState = "ECO_LNK_TO_IIMC";

              return;
            }
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "DISPLAY":
        export.DisplayOnly.Assign(local.RefreshCase);
        MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
          export.ArCsePersonsWorkSet);
        export.InterstateRequest.KsCaseInd =
          local.RefreshInterstateRequest.KsCaseInd;

        if (IsEmpty(export.NextCase.Number))
        {
          var field = GetField(export.NextCase, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          break;
        }
        else
        {
          UseSiReadCaseHeaderInformation();

          // *** Problem report I00114997
          // *** 03/08/01 swsrchf
          // *** start
          if (IsExitState("INVALID_CASE_ROLE"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          // *** end
          // *** 03/08/01 swsrchf
          // *** Problem report I00114997
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (!Equal(export.NextCase.Number, export.DisplayOnly.Number) && !
          IsEmpty(export.DisplayOnly.Number))
        {
          export.ApCsePerson.Number = "";
          export.ApCsePersonsWorkSet.Number = "";
          export.ApCsePersonsWorkSet.FormattedName = "";
        }

        if (!IsEmpty(export.SearchInterstateRequestHistory.FunctionalTypeCode))
        {
          local.CodeValue.Cdvalue =
            export.SearchInterstateRequestHistory.FunctionalTypeCode;
          UseCabValidateCodeValue1();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field =
              GetField(export.SearchInterstateRequestHistory,
              "functionalTypeCode");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }

        if (!IsEmpty(export.SearchInterstateRequestHistory.ActionCode))
        {
          local.CodeValue.Cdvalue =
            export.SearchInterstateRequestHistory.ActionCode;
          UseCabValidateCodeValue2();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field =
              GetField(export.SearchInterstateRequestHistory, "actionCode");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }

        if (!IsEmpty(export.SearchInterstateRequestHistory.ActionReasonCode))
        {
          if (Equal(export.SearchInterstateRequestHistory.ActionReasonCode,
            "IICNV") || Equal
            (export.SearchInterstateRequestHistory.ActionReasonCode, "OICNV") ||
            Equal
            (export.SearchInterstateRequestHistory.ActionReasonCode, "IICLS") ||
            Equal
            (export.SearchInterstateRequestHistory.ActionReasonCode, "OICLS") ||
            Equal
            (export.SearchInterstateRequestHistory.ActionReasonCode, "IICRO") ||
            Equal
            (export.SearchInterstateRequestHistory.ActionReasonCode, "OICRO"))
          {
            // These codes are not on the INTERSTATE REASON code table.  Don't 
            // validate.
            goto Test1;
          }

          local.CodeValue.Cdvalue =
            export.SearchInterstateRequestHistory.ActionReasonCode ?? Spaces
            (10);
          UseCabValidateCodeValue3();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field =
              GetField(export.SearchInterstateRequestHistory, "actionReasonCode");
              

            field.Error = true;

            ExitState = "ACO_NE0000_INVL_REASON_CODE";

            return;
          }
        }

Test1:

        if (!IsEmpty(export.SearchFips.StateAbbreviation))
        {
          local.CodeValue.Cdvalue = export.SearchFips.StateAbbreviation;
          UseCabValidateCodeValue4();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field = GetField(export.SearchFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.InterstateRequest.KsCaseInd =
            local.RefreshInterstateRequest.KsCaseInd;
          export.DisplayOnly.Number = export.NextCase.Number;

          break;
        }

        UseSiIreqListIsRequestHistory3();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          var field = GetField(export.NextCase, "number");

          field.Error = true;
        }
        else
        {
          export.HiddenNext.Number = export.NextCase.Number;
          export.HiddenSearchInterstateRequestHistory.Assign(
            export.SearchInterstateRequestHistory);
          export.HiddenSearchFips.StateAbbreviation =
            export.SearchFips.StateAbbreviation;

          if (AsChar(local.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }

          if (AsChar(export.More.Flag) == 'Y')
          {
            export.Plus.Text1 = "+";
          }
          else
          {
            export.Plus.Text1 = "";
          }

          if (AsChar(export.PrevCommon.Flag) == 'Y')
          {
            export.Minus.Text1 = "-";
          }
          else
          {
            export.Minus.Text1 = "";
          }
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        local.NoOfPrompts.Count = 0;
        MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
        MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet,
          export.ArCsePersonsWorkSet);

        switch(AsChar(export.PromptActionCd.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.NoOfPrompts.Count;

            break;
          default:
            var field = GetField(export.PromptActionCd, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.PromptFunctionCd.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.NoOfPrompts.Count;

            break;
          default:
            var field = GetField(export.PromptFunctionCd, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.PromptPerson.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.NoOfPrompts.Count;

            break;
          default:
            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.PromptReason.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.NoOfPrompts.Count;

            break;
          default:
            var field = GetField(export.PromptActionCd, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.PromptState.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.NoOfPrompts.Count;

            break;
          default:
            var field = GetField(export.PromptActionCd, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet,
            export.ArCsePersonsWorkSet);

          break;
        }

        if (local.NoOfPrompts.Count > 1)
        {
          if (AsChar(export.PromptActionCd.SelectChar) == 'S')
          {
            var field = GetField(export.PromptActionCd, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.PromptFunctionCd.SelectChar) == 'S')
          {
            var field = GetField(export.PromptFunctionCd, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.PromptPerson.SelectChar) == 'S')
          {
            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.PromptReason.SelectChar) == 'S')
          {
            var field = GetField(export.PromptReason, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.PromptState.SelectChar) == 'S')
          {
            var field = GetField(export.PromptState, "selectChar");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
        }
        else if (local.NoOfPrompts.Count == 0)
        {
          var field1 = GetField(export.PromptActionCd, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptFunctionCd, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.PromptPerson, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.PromptReason, "selectChar");

          field4.Error = true;

          var field5 = GetField(export.PromptState, "selectChar");

          field5.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          break;
        }

        if (AsChar(export.PromptActionCd.SelectChar) == 'S')
        {
          export.HiddenReturnMultRecs.Flag = "N";
          export.Prompt.CodeName = local.ActionType.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.PromptFunctionCd.SelectChar) == 'S')
        {
          export.HiddenReturnMultRecs.Flag = "N";
          export.Prompt.CodeName = local.FunctionalType.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.PromptPerson.SelectChar) == 'S')
        {
          if (IsEmpty(export.NextCase.Number))
          {
            var field = GetField(export.NextCase, "number");

            field.Error = true;

            ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";

            break;
          }
          else
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          }
        }

        if (AsChar(export.PromptReason.SelectChar) == 'S')
        {
          export.HiddenReturnMultRecs.Flag = "N";
          export.Prompt.CodeName = local.ActionReason.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.HiddenReturnMultRecs.Flag = "N";
          export.Prompt.CodeName = local.States.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        break;
      case "PREV":
        if (!Equal(export.NextCase.Number, export.HiddenNext.Number) || AsChar
          (export.SearchInterstateRequestHistory.ActionCode) != AsChar
          (export.HiddenSearchInterstateRequestHistory.ActionCode) || !
          Equal(export.SearchInterstateRequestHistory.FunctionalTypeCode,
          export.HiddenSearchInterstateRequestHistory.FunctionalTypeCode) || !
          Equal(export.SearchInterstateRequestHistory.ActionReasonCode,
          export.HiddenSearchInterstateRequestHistory.ActionReasonCode) || !
          Equal(export.SearchInterstateRequestHistory.CreatedTimestamp,
          export.HiddenSearchInterstateRequestHistory.CreatedTimestamp) || !
          Equal(export.SearchFips.StateAbbreviation,
          export.HiddenSearchFips.StateAbbreviation))
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        if (AsChar(export.PrevCommon.Flag) == 'Y')
        {
          UseSiIreqListIsRequestHistory1();

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
          {
            var field = GetField(export.NextCase, "number");

            field.Error = true;
          }
          else
          {
            if (AsChar(local.CaseOpen.Flag) == 'N')
            {
              ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
            }

            if (AsChar(export.More.Flag) == 'Y')
            {
              export.Plus.Text1 = "+";
            }
            else
            {
              export.Plus.Text1 = "";
            }

            if (AsChar(export.PrevCommon.Flag) == 'Y')
            {
              export.Minus.Text1 = "-";
            }
            else
            {
              export.Minus.Text1 = "";
            }
          }
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }

        break;
      case "NEXT":
        if (!Equal(export.NextCase.Number, export.HiddenNext.Number) || AsChar
          (export.SearchInterstateRequestHistory.ActionCode) != AsChar
          (export.HiddenSearchInterstateRequestHistory.ActionCode) || !
          Equal(export.SearchInterstateRequestHistory.FunctionalTypeCode,
          export.HiddenSearchInterstateRequestHistory.FunctionalTypeCode) || !
          Equal(export.SearchInterstateRequestHistory.ActionReasonCode,
          export.HiddenSearchInterstateRequestHistory.ActionReasonCode) || !
          Equal(export.SearchInterstateRequestHistory.CreatedTimestamp,
          export.HiddenSearchInterstateRequestHistory.CreatedTimestamp) || !
          Equal(export.SearchFips.StateAbbreviation,
          export.HiddenSearchFips.StateAbbreviation))
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        if (AsChar(export.More.Flag) == 'Y')
        {
          UseSiIreqListIsRequestHistory2();

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
          {
            var field = GetField(export.NextCase, "number");

            field.Error = true;
          }
          else
          {
            if (AsChar(local.CaseOpen.Flag) == 'N')
            {
              ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
            }

            if (AsChar(export.More.Flag) == 'Y')
            {
              export.Plus.Text1 = "+";
            }
            else
            {
              export.Plus.Text1 = "";
            }

            if (AsChar(export.PrevCommon.Flag) == 'Y')
            {
              export.Minus.Text1 = "-";
            }
            else
            {
              export.Minus.Text1 = "";
            }
          }
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "ICAS":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            // *** Problem report I00114726
            // *** 02/27/01 swsrchf
            // *** start
            if (export.Export1.Item.DetailInterstateRequestHistory.
              TransactionSerialNum == 0)
            {
              ExitState = "SI0000_NO_ITERSTATE_REFERRAL";

              goto Test2;
            }

            // *** end
            // *** 02/27/01 swsrchf
            // *** Problem report I00114726
            export.Icas.ActionCode =
              export.Export1.Item.DetailInterstateRequestHistory.ActionCode;
            export.Icas.ActionReasonCode =
              export.Export1.Item.DetailInterstateRequestHistory.
                ActionReasonCode ?? "";
            export.Icas.FunctionalTypeCode =
              export.Export1.Item.DetailInterstateRequestHistory.
                FunctionalTypeCode;
            export.Icas.InterstateCaseId =
              export.Export1.Item.DetailInterstateRequest.OtherStateCaseId ?? ""
              ;
            export.Icas.KsCaseId = export.DisplayOnly.Number;
            export.Icas.TransSerialNumber =
              export.Export1.Item.DetailInterstateRequestHistory.
                TransactionSerialNum;
            export.Icas.TransactionDate =
              export.Export1.Item.DetailInterstateRequestHistory.
                TransactionDate;
            ExitState = "ECO_XFR_TO_CSENET_REFERRAL_CASE";

            return;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "RESEND":
        ExitState = "ACO_NN0000_ALL_OK";
        local.NoOfSelects.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Export1.Item.Select.SelectChar))
          {
            case 'S':
              ++local.NoOfSelects.Count;

              if (!Equal(export.Export1.Item.StatusCd.Text4, "ERR"))
              {
                ExitState = "SI0000_ONLY_RESEND_E";

                var field1 = GetField(export.Export1.Item.StatusCd, "text4");

                field1.Color = "red";
                field1.Protected = true;

                var field2 = GetField(export.Export1.Item.Select, "selectChar");

                field2.Protected = false;
                field2.Focused = true;

                goto Test2;
              }

              if (Lt(export.Export1.Item.DetailInterstateRequestHistory.
                TransactionDate, Now().Date.AddDays(-180)))
              {
                ExitState = "SI0000_CANNOT_RESEND_TRANS";

                goto Test2;
              }

              break;
            case ' ':
              continue;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Export1.Item.Select, "selectChar");

              field.Error = true;

              goto Test2;
          }

          UseSiResendCsenetTransaction();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Export1.Update.StatusCd.Text4 = "SENT";
          }
          else
          {
            goto Test2;
          }
        }

        export.Export1.CheckIndex();

        if (local.NoOfSelects.Count == 0)
        {
          ExitState = "FN0000_MUST_SEL_REC_FOR_PROC";

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Export1.Item.Select.SelectChar))
          {
            export.Export1.Update.Select.SelectChar = "";
          }
        }

        export.Export1.CheckIndex();
        ExitState = "SI0000_RESEND_SUCCESSFUL";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test2:

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (Equal(export.Export1.Item.StatusCd.Text4, "ERR"))
      {
        var field = GetField(export.Export1.Item.StatusCd, "text4");

        field.Color = "red";
        field.Protected = true;
      }
    }

    export.Export1.CheckIndex();
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.StatusDate = source.StatusDate;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(SiIreqListIsRequestHistory.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveTextWorkArea(source.IvdAgency, target.IvdAgency);
    MoveFips(source.State, target.State);
    MoveCodeValue(source.ActionReason, target.ActionReason);
    target.Select.SelectChar = source.Select.SelectChar;
    target.DetailInterstateRequest.Assign(source.DetailInterstateRequest);
    target.DetailInterstateRequestHistory.Assign(
      source.DetailInterstateRequestHistory);
    target.StatusCd.Text4 = source.StatusCd.Text4;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.LocationDescription = source.LocationDescription;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.KsCaseInd = source.KsCaseInd;
    target.Country = source.Country;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.TransactionSerialNum = source.TransactionSerialNum;
    target.TransactionDate = source.TransactionDate;
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

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text4 = source.Text4;
    target.Text12 = source.Text12;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.FunctionalType.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.ActionType.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.ActionReason.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue4()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.States.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.FunctionalType.CodeName = useExport.CsenetFunctionalType.CodeName;
    local.ActionType.CodeName = useExport.CsenetActionType.CodeName;
    local.ActionReason.CodeName = useExport.CsenetActionReason.CodeName;
    local.States.CodeName = useExport.State.CodeName;
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
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.Case1.Number = export.NextCase.Number;
    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiIreqListIsRequestHistory1()
  {
    var useImport = new SiIreqListIsRequestHistory.Import();
    var useExport = new SiIreqListIsRequestHistory.Export();

    useImport.CaseOpen.Flag = local.CaseOpen.Flag;
    useImport.SearchFips.StateAbbreviation =
      export.SearchFips.StateAbbreviation;
    useImport.Starting.CreatedTimestamp =
      export.PrevInterstateRequestHistory.CreatedTimestamp;
    useImport.Case1.Number = export.NextCase.Number;
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);
    useImport.SearchInterstateRequestHistory.Assign(
      export.SearchInterstateRequestHistory);

    Call(SiIreqListIsRequestHistory.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.PrevInterstateRequestHistory.CreatedTimestamp =
      useExport.PrevInterstateRequestHistory.CreatedTimestamp;
    export.NextInterstateRequestHistory.CreatedTimestamp =
      useExport.Next.CreatedTimestamp;
    export.More.Flag = useExport.More.Flag;
    MoveCase1(useExport.Case1, export.DisplayOnly);
    MoveInterstateRequest(useExport.InterstateRequest, export.InterstateRequest);
      
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    export.PrevCommon.Flag = useExport.PrevCommon.Flag;
  }

  private void UseSiIreqListIsRequestHistory2()
  {
    var useImport = new SiIreqListIsRequestHistory.Import();
    var useExport = new SiIreqListIsRequestHistory.Export();

    useImport.CaseOpen.Flag = local.CaseOpen.Flag;
    useImport.SearchFips.StateAbbreviation =
      export.SearchFips.StateAbbreviation;
    useImport.Starting.CreatedTimestamp =
      export.NextInterstateRequestHistory.CreatedTimestamp;
    useImport.Case1.Number = export.NextCase.Number;
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);
    useImport.SearchInterstateRequestHistory.Assign(
      export.SearchInterstateRequestHistory);

    Call(SiIreqListIsRequestHistory.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.PrevInterstateRequestHistory.CreatedTimestamp =
      useExport.PrevInterstateRequestHistory.CreatedTimestamp;
    export.NextInterstateRequestHistory.CreatedTimestamp =
      useExport.Next.CreatedTimestamp;
    export.More.Flag = useExport.More.Flag;
    MoveCase1(useExport.Case1, export.DisplayOnly);
    MoveInterstateRequest(useExport.InterstateRequest, export.InterstateRequest);
      
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    export.PrevCommon.Flag = useExport.PrevCommon.Flag;
  }

  private void UseSiIreqListIsRequestHistory3()
  {
    var useImport = new SiIreqListIsRequestHistory.Import();
    var useExport = new SiIreqListIsRequestHistory.Export();

    useImport.CaseOpen.Flag = local.CaseOpen.Flag;
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);
    useImport.Case1.Number = export.NextCase.Number;
    useImport.SearchInterstateRequestHistory.Assign(
      export.SearchInterstateRequestHistory);
    useImport.SearchFips.StateAbbreviation =
      export.SearchFips.StateAbbreviation;

    Call(SiIreqListIsRequestHistory.Execute, useImport, useExport);

    MoveCase1(useExport.Case1, export.DisplayOnly);
    MoveInterstateRequest(useExport.InterstateRequest, export.InterstateRequest);
      
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.PrevInterstateRequestHistory.CreatedTimestamp =
      useExport.PrevInterstateRequestHistory.CreatedTimestamp;
    export.NextInterstateRequestHistory.CreatedTimestamp =
      useExport.Next.CreatedTimestamp;
    export.More.Flag = useExport.More.Flag;
    export.PrevCommon.Flag = useExport.PrevCommon.Flag;
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.NextCase.Number;
    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.CaseOpen.Flag = useExport.CaseOpen.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.NextCase.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.OspServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.OspOffice);
  }

  private void UseSiResendCsenetTransaction()
  {
    var useImport = new SiResendCsenetTransaction.Import();
    var useExport = new SiResendCsenetTransaction.Export();

    MoveInterstateRequestHistory(export.Export1.Item.
      DetailInterstateRequestHistory, useImport.InterstateRequestHistory);

    Call(SiResendCsenetTransaction.Execute, useImport, useExport);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of IvdAgency.
      /// </summary>
      [JsonPropertyName("ivdAgency")]
      public TextWorkArea IvdAgency
      {
        get => ivdAgency ??= new();
        set => ivdAgency = value;
      }

      /// <summary>
      /// A value of State.
      /// </summary>
      [JsonPropertyName("state")]
      public Fips State
      {
        get => state ??= new();
        set => state = value;
      }

      /// <summary>
      /// A value of ActionReason.
      /// </summary>
      [JsonPropertyName("actionReason")]
      public CodeValue ActionReason
      {
        get => actionReason ??= new();
        set => actionReason = value;
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
      /// A value of DetailInterstateRequest.
      /// </summary>
      [JsonPropertyName("detailInterstateRequest")]
      public InterstateRequest DetailInterstateRequest
      {
        get => detailInterstateRequest ??= new();
        set => detailInterstateRequest = value;
      }

      /// <summary>
      /// A value of DetailInterstateRequestHistory.
      /// </summary>
      [JsonPropertyName("detailInterstateRequestHistory")]
      public InterstateRequestHistory DetailInterstateRequestHistory
      {
        get => detailInterstateRequestHistory ??= new();
        set => detailInterstateRequestHistory = value;
      }

      /// <summary>
      /// A value of StatusCd.
      /// </summary>
      [JsonPropertyName("statusCd")]
      public TextWorkArea StatusCd
      {
        get => statusCd ??= new();
        set => statusCd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private TextWorkArea ivdAgency;
      private Fips state;
      private CodeValue actionReason;
      private Common select;
      private InterstateRequest detailInterstateRequest;
      private InterstateRequestHistory detailInterstateRequestHistory;
      private TextWorkArea statusCd;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public TextWorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public TextWorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
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
    /// A value of NextInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("nextInterstateRequestHistory")]
    public InterstateRequestHistory NextInterstateRequestHistory
    {
      get => nextInterstateRequestHistory ??= new();
      set => nextInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of PrevInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("prevInterstateRequestHistory")]
    public InterstateRequestHistory PrevInterstateRequestHistory
    {
      get => prevInterstateRequestHistory ??= new();
      set => prevInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
    }

    /// <summary>
    /// A value of HiddenSearchInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("hiddenSearchInterstateRequestHistory")]
    public InterstateRequestHistory HiddenSearchInterstateRequestHistory
    {
      get => hiddenSearchInterstateRequestHistory ??= new();
      set => hiddenSearchInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of HiddenSearchFips.
    /// </summary>
    [JsonPropertyName("hiddenSearchFips")]
    public Fips HiddenSearchFips
    {
      get => hiddenSearchFips ??= new();
      set => hiddenSearchFips = value;
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
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// A value of PromptActionCd.
    /// </summary>
    [JsonPropertyName("promptActionCd")]
    public Common PromptActionCd
    {
      get => promptActionCd ??= new();
      set => promptActionCd = value;
    }

    /// <summary>
    /// A value of PromptFunctionCd.
    /// </summary>
    [JsonPropertyName("promptFunctionCd")]
    public Common PromptFunctionCd
    {
      get => promptFunctionCd ??= new();
      set => promptFunctionCd = value;
    }

    /// <summary>
    /// A value of PromptReason.
    /// </summary>
    [JsonPropertyName("promptReason")]
    public Common PromptReason
    {
      get => promptReason ??= new();
      set => promptReason = value;
    }

    /// <summary>
    /// A value of PromptState.
    /// </summary>
    [JsonPropertyName("promptState")]
    public Common PromptState
    {
      get => promptState ??= new();
      set => promptState = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("searchInterstateRequestHistory")]
    public InterstateRequestHistory SearchInterstateRequestHistory
    {
      get => searchInterstateRequestHistory ??= new();
      set => searchInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of NextCase.
    /// </summary>
    [JsonPropertyName("nextCase")]
    public Case1 NextCase
    {
      get => nextCase ??= new();
      set => nextCase = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

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
    /// A value of AutoFlow.
    /// </summary>
    [JsonPropertyName("autoFlow")]
    public Common AutoFlow
    {
      get => autoFlow ??= new();
      set => autoFlow = value;
    }

    /// <summary>
    /// A value of PrevCommon.
    /// </summary>
    [JsonPropertyName("prevCommon")]
    public Common PrevCommon
    {
      get => prevCommon ??= new();
      set => prevCommon = value;
    }

    private TextWorkArea minus;
    private TextWorkArea plus;
    private Array<ImportGroup> import1;
    private InterstateRequestHistory nextInterstateRequestHistory;
    private InterstateRequestHistory prevInterstateRequestHistory;
    private Case1 hiddenNext;
    private InterstateRequestHistory hiddenSearchInterstateRequestHistory;
    private Fips hiddenSearchFips;
    private Fips searchFips;
    private Common more;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Common promptPerson;
    private CodeValue selectedCodeValue;
    private Common promptActionCd;
    private Common promptFunctionCd;
    private Common promptReason;
    private Common promptState;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private InterstateRequestHistory searchInterstateRequestHistory;
    private InterstateRequest interstateRequest;
    private CsePerson arCsePerson;
    private CsePerson apCsePerson;
    private Case1 nextCase;
    private Case1 displayOnly;
    private Standard standard;
    private NextTranInfo hidden;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private WorkArea headerLine;
    private Common autoFlow;
    private Common prevCommon;
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
      /// A value of IvdAgency.
      /// </summary>
      [JsonPropertyName("ivdAgency")]
      public TextWorkArea IvdAgency
      {
        get => ivdAgency ??= new();
        set => ivdAgency = value;
      }

      /// <summary>
      /// A value of State.
      /// </summary>
      [JsonPropertyName("state")]
      public Fips State
      {
        get => state ??= new();
        set => state = value;
      }

      /// <summary>
      /// A value of ActionReason.
      /// </summary>
      [JsonPropertyName("actionReason")]
      public CodeValue ActionReason
      {
        get => actionReason ??= new();
        set => actionReason = value;
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
      /// A value of DetailInterstateRequest.
      /// </summary>
      [JsonPropertyName("detailInterstateRequest")]
      public InterstateRequest DetailInterstateRequest
      {
        get => detailInterstateRequest ??= new();
        set => detailInterstateRequest = value;
      }

      /// <summary>
      /// A value of DetailInterstateRequestHistory.
      /// </summary>
      [JsonPropertyName("detailInterstateRequestHistory")]
      public InterstateRequestHistory DetailInterstateRequestHistory
      {
        get => detailInterstateRequestHistory ??= new();
        set => detailInterstateRequestHistory = value;
      }

      /// <summary>
      /// A value of StatusCd.
      /// </summary>
      [JsonPropertyName("statusCd")]
      public TextWorkArea StatusCd
      {
        get => statusCd ??= new();
        set => statusCd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private TextWorkArea ivdAgency;
      private Fips state;
      private CodeValue actionReason;
      private Common select;
      private InterstateRequest detailInterstateRequest;
      private InterstateRequestHistory detailInterstateRequestHistory;
      private TextWorkArea statusCd;
    }

    /// <summary>
    /// A value of PromptReason.
    /// </summary>
    [JsonPropertyName("promptReason")]
    public Common PromptReason
    {
      get => promptReason ??= new();
      set => promptReason = value;
    }

    /// <summary>
    /// A value of PromptState.
    /// </summary>
    [JsonPropertyName("promptState")]
    public Common PromptState
    {
      get => promptState ??= new();
      set => promptState = value;
    }

    /// <summary>
    /// A value of HiddenSearchFips.
    /// </summary>
    [JsonPropertyName("hiddenSearchFips")]
    public Fips HiddenSearchFips
    {
      get => hiddenSearchFips ??= new();
      set => hiddenSearchFips = value;
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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public TextWorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public TextWorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
    }

    /// <summary>
    /// A value of HiddenSearchInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("hiddenSearchInterstateRequestHistory")]
    public InterstateRequestHistory HiddenSearchInterstateRequestHistory
    {
      get => hiddenSearchInterstateRequestHistory ??= new();
      set => hiddenSearchInterstateRequestHistory = value;
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
    /// A value of PrevInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("prevInterstateRequestHistory")]
    public InterstateRequestHistory PrevInterstateRequestHistory
    {
      get => prevInterstateRequestHistory ??= new();
      set => prevInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of NextInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("nextInterstateRequestHistory")]
    public InterstateRequestHistory NextInterstateRequestHistory
    {
      get => nextInterstateRequestHistory ??= new();
      set => nextInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
    }

    /// <summary>
    /// A value of SelectedInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("selectedInterstateRequestHistory")]
    public InterstateRequestHistory SelectedInterstateRequestHistory
    {
      get => selectedInterstateRequestHistory ??= new();
      set => selectedInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of SelectedInterstateRequest.
    /// </summary>
    [JsonPropertyName("selectedInterstateRequest")]
    public InterstateRequest SelectedInterstateRequest
    {
      get => selectedInterstateRequest ??= new();
      set => selectedInterstateRequest = value;
    }

    /// <summary>
    /// A value of HiddenReturnMultRecs.
    /// </summary>
    [JsonPropertyName("hiddenReturnMultRecs")]
    public Common HiddenReturnMultRecs
    {
      get => hiddenReturnMultRecs ??= new();
      set => hiddenReturnMultRecs = value;
    }

    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
    }

    /// <summary>
    /// A value of NextCase.
    /// </summary>
    [JsonPropertyName("nextCase")]
    public Case1 NextCase
    {
      get => nextCase ??= new();
      set => nextCase = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Icas.
    /// </summary>
    [JsonPropertyName("icas")]
    public InterstateCase Icas
    {
      get => icas ??= new();
      set => icas = value;
    }

    /// <summary>
    /// A value of PromptActionCd.
    /// </summary>
    [JsonPropertyName("promptActionCd")]
    public Common PromptActionCd
    {
      get => promptActionCd ??= new();
      set => promptActionCd = value;
    }

    /// <summary>
    /// A value of PromptFunctionCd.
    /// </summary>
    [JsonPropertyName("promptFunctionCd")]
    public Common PromptFunctionCd
    {
      get => promptFunctionCd ??= new();
      set => promptFunctionCd = value;
    }

    /// <summary>
    /// A value of SearchInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("searchInterstateRequestHistory")]
    public InterstateRequestHistory SearchInterstateRequestHistory
    {
      get => searchInterstateRequestHistory ??= new();
      set => searchInterstateRequestHistory = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

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
    /// A value of AutoFlow.
    /// </summary>
    [JsonPropertyName("autoFlow")]
    public Common AutoFlow
    {
      get => autoFlow ??= new();
      set => autoFlow = value;
    }

    /// <summary>
    /// A value of PrevCommon.
    /// </summary>
    [JsonPropertyName("prevCommon")]
    public Common PrevCommon
    {
      get => prevCommon ??= new();
      set => prevCommon = value;
    }

    private Common promptReason;
    private Common promptState;
    private Fips hiddenSearchFips;
    private Fips searchFips;
    private TextWorkArea minus;
    private TextWorkArea plus;
    private Case1 hiddenNext;
    private InterstateRequestHistory hiddenSearchInterstateRequestHistory;
    private Array<ExportGroup> export1;
    private InterstateRequestHistory prevInterstateRequestHistory;
    private InterstateRequestHistory nextInterstateRequestHistory;
    private Common more;
    private InterstateRequestHistory selectedInterstateRequestHistory;
    private InterstateRequest selectedInterstateRequest;
    private Common hiddenReturnMultRecs;
    private Fips selectedFips;
    private Common promptPerson;
    private Case1 displayOnly;
    private Case1 nextCase;
    private Code prompt;
    private CsePerson arCsePerson;
    private CsePerson apCsePerson;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private InterstateCase icas;
    private Common promptActionCd;
    private Common promptFunctionCd;
    private InterstateRequestHistory searchInterstateRequestHistory;
    private Standard standard;
    private NextTranInfo hidden;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private WorkArea headerLine;
    private Common autoFlow;
    private Common prevCommon;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NoOfSelects.
    /// </summary>
    [JsonPropertyName("noOfSelects")]
    public Common NoOfSelects
    {
      get => noOfSelects ??= new();
      set => noOfSelects = value;
    }

    /// <summary>
    /// A value of NoOfPrompts.
    /// </summary>
    [JsonPropertyName("noOfPrompts")]
    public Common NoOfPrompts
    {
      get => noOfPrompts ??= new();
      set => noOfPrompts = value;
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
    /// A value of RefreshInterstateRequest.
    /// </summary>
    [JsonPropertyName("refreshInterstateRequest")]
    public InterstateRequest RefreshInterstateRequest
    {
      get => refreshInterstateRequest ??= new();
      set => refreshInterstateRequest = value;
    }

    /// <summary>
    /// A value of RefreshCase.
    /// </summary>
    [JsonPropertyName("refreshCase")]
    public Case1 RefreshCase
    {
      get => refreshCase ??= new();
      set => refreshCase = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of RefreshInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("refreshInterstateRequestHistory")]
    public InterstateRequestHistory RefreshInterstateRequestHistory
    {
      get => refreshInterstateRequestHistory ??= new();
      set => refreshInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
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
    /// A value of FunctionalType.
    /// </summary>
    [JsonPropertyName("functionalType")]
    public Code FunctionalType
    {
      get => functionalType ??= new();
      set => functionalType = value;
    }

    /// <summary>
    /// A value of States.
    /// </summary>
    [JsonPropertyName("states")]
    public Code States
    {
      get => states ??= new();
      set => states = value;
    }

    /// <summary>
    /// A value of ActionType.
    /// </summary>
    [JsonPropertyName("actionType")]
    public Code ActionType
    {
      get => actionType ??= new();
      set => actionType = value;
    }

    /// <summary>
    /// A value of ActionReason.
    /// </summary>
    [JsonPropertyName("actionReason")]
    public Code ActionReason
    {
      get => actionReason ??= new();
      set => actionReason = value;
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
    /// A value of SelectionMade.
    /// </summary>
    [JsonPropertyName("selectionMade")]
    public Common SelectionMade
    {
      get => selectionMade ??= new();
      set => selectionMade = value;
    }

    private Common noOfSelects;
    private Common noOfPrompts;
    private Common caseOpen;
    private InterstateRequest refreshInterstateRequest;
    private Case1 refreshCase;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private InterstateRequestHistory refreshInterstateRequestHistory;
    private TextWorkArea zeroFill;
    private CodeValue codeValue;
    private Code functionalType;
    private Code states;
    private Code actionType;
    private Code actionReason;
    private Common error;
    private Common selectionMade;
  }
#endregion
}

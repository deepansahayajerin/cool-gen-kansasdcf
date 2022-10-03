// Program: SI_IIOI_INCOMING_IS_OG_INFO, ID: 372556841, model: 746.
// Short name: SWEIIOIP
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
/// A program: SI_IIOI_INCOMING_IS_OG_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIioiIncomingIsOgInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIOI_INCOMING_IS_OG_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIioiIncomingIsOgInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIioiIncomingIsOgInfo.
  /// </summary>
  public SiIioiIncomingIsOgInfo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ----------------------------------------------------------------------
    // 		Various butchers		Various butcherings
    // 04/29/2002	M.Ashworth	PR143798
    // Don't send csenet transaction to states that are not csenet ready
    // 09/18/2002	GVandy		PR155782
    // Don't allow court case number to be selected when sending an 
    // acknowledgement.
    // 09/23/2002	GVandy		PR158443
    // Correct previous data being blanked out when returning from LACS.
    // 10/16/2002	M Ramirez	114395
    // Rewrote SEND for normal SEND command and for REJECT
    // command (executed from ICAS but implemented here)
    // 10/24/2002	M Ramirez
    // Rewrote entire screen to assist maintenance
    // 03/05/2003	GVandy		PR172099
    // Use transaction_date passed from ICAS when rejecting a referral.
    // 05/09/2006	GVandy		WR230751
    // Change F16 IIIN to F16 IIMC
    // 02-23-2007       AHockman              PR245429
    //                          fix to allow rejected transactions to go through
    //                          the feds, changed  codes we are returning
    //                          to ones that do not require ot st case number.
    // 03-01-2011	TPierce		CQ24439
    // Change to allow "send" for new GSC14 CSENet transaction.
    // - - - - -  - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
    // - - - - - - - - - - - - - - -
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    local.Current.Date = Now().Date;
    UseOeCabSetMnemonics();

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Case1.Number = import.Case1.Number;
    MoveCase1(import.Previous, export.Previous);
    MoveCsePersonsWorkSet2(import.Ap, export.Ap);
    MoveCsePersonsWorkSet2(import.Ar, export.Ar);
    export.HeaderLine.Text35 = import.HeaderLine.Text35;
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.ReferralInterstateRequestHistory.Assign(
      import.ReferralInterstateRequestHistory);
    export.InterstateMiscellaneous.Assign(import.InterstateMiscellaneous);
    MoveFips(import.ReferralFips, export.ReferralFips);
    export.PreviousReferral.StateAbbreviation =
      import.PreviousReferral.StateAbbreviation;
    export.LinkFromIcas.Flag = import.LinkFromIcas.Flag;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.PromptAp.SelectChar = import.PromptAp.SelectChar;
    export.PromptState.SelectChar = import.PromptState.SelectChar;
    export.PromptFunction.SelectChar = import.PromptFunction.SelectChar;
    export.PromptReason.SelectChar = import.PromptReason.SelectChar;
    export.PromptAttachment.SelectChar = import.PromptAttachment.SelectChar;

    export.Children.Index = 0;
    export.Children.Clear();

    for(import.Children.Index = 0; import.Children.Index < import
      .Children.Count; ++import.Children.Index)
    {
      if (export.Children.IsFull)
      {
        break;
      }

      export.Children.Update.GexportSelectChild.SelectChar =
        import.Children.Item.GimportSelectChild.SelectChar;
      export.Children.Update.GexportChild.Assign(
        import.Children.Item.GimportChild);

      switch(AsChar(export.Children.Item.GexportSelectChild.SelectChar))
      {
        case 'S':
          break;
        case '*':
          export.Children.Update.GexportSelectChild.SelectChar = "";

          break;
        case ' ':
          break;
        default:
          var field =
            GetField(export.Children.Item.GexportSelectChild, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      export.Children.Next();
    }

    export.CourtOrders.Index = 0;
    export.CourtOrders.Clear();

    for(import.CourtOrders.Index = 0; import.CourtOrders.Index < import
      .CourtOrders.Count; ++import.CourtOrders.Index)
    {
      if (export.CourtOrders.IsFull)
      {
        break;
      }

      export.CourtOrders.Update.GexportPromptCourtOrder.SelectChar =
        import.CourtOrders.Item.GimportPromptCourtOrder.SelectChar;
      export.CourtOrders.Update.GexportCourtOrder.Assign(
        import.CourtOrders.Item.GimportCourtOrder);

      switch(AsChar(export.CourtOrders.Item.GexportPromptCourtOrder.SelectChar))
      {
        case 'S':
          ++local.Count.Count;

          break;
        case '*':
          export.CourtOrders.Update.GexportPromptCourtOrder.SelectChar = "";

          break;
        case ' ':
          break;
        default:
          var field =
            GetField(export.CourtOrders.Item.GexportPromptCourtOrder,
            "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      export.CourtOrders.Next();
    }

    // ---------------------------------------------
    // Validate Prompt Characters
    // ---------------------------------------------
    switch(AsChar(export.PromptAp.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptAp.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptAp, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
    }

    switch(AsChar(export.PromptState.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptState.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptState, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
    }

    switch(AsChar(export.PromptFunction.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptFunction.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptFunction, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
    }

    switch(AsChar(export.PromptReason.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptReason.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptReason, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
    }

    switch(AsChar(export.PromptAttachment.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptAttachment.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptAttachment, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (local.Count.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

      if (AsChar(export.PromptAp.SelectChar) == 'S')
      {
        var field = GetField(export.PromptAp, "selectChar");

        field.Error = true;
      }

      if (AsChar(export.PromptState.SelectChar) == 'S')
      {
        var field = GetField(export.PromptState, "selectChar");

        field.Error = true;
      }

      if (AsChar(export.PromptFunction.SelectChar) == 'S')
      {
        var field = GetField(export.PromptFunction, "selectChar");

        field.Error = true;
      }

      if (AsChar(export.PromptReason.SelectChar) == 'S')
      {
        var field = GetField(export.PromptReason, "selectChar");

        field.Error = true;
      }

      if (AsChar(export.PromptAttachment.SelectChar) == 'S')
      {
        var field = GetField(export.PromptAttachment, "selectChar");

        field.Error = true;
      }

      export.CourtOrders.Index = 0;
      export.CourtOrders.Clear();

      for(import.CourtOrders.Index = 0; import.CourtOrders.Index < import
        .CourtOrders.Count; ++import.CourtOrders.Index)
      {
        if (export.CourtOrders.IsFull)
        {
          break;
        }

        if (AsChar(export.CourtOrders.Item.GexportPromptCourtOrder.SelectChar) ==
          'S')
        {
          var field = GetField(export.PromptReason, "selectChar");

          field.Error = true;
        }

        export.CourtOrders.Next();
      }
    }
    else if (local.Count.Count == 1)
    {
      if (!Equal(global.Command, "LIST") && !
        Equal(global.Command, "RETCOMP") && !
        Equal(global.Command, "RETLACS") && !Equal(global.Command, "RETCDVL"))
      {
        ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
      }
    }
    else if (Equal(global.Command, "LIST"))
    {
      ExitState = "ACO_NE0000_NO_SELECTION_MADE";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!IsEmpty(export.Case1.Number))
    {
      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        return;
      }
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
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

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.Ap.Number = export.Hidden.CsePersonNumberAp ?? Spaces(10);
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "SEND"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------------------------------
    // Special processing to keep Interstate Request History Note
    // in sync with Interstate Miscellaneous Text Lines
    // This is done because Interstate Miscellaneous has five 80-character 
    // lines, whereas Interstate Request History has one 400-character line.
    // The screen limits display fields to 255 characters, which will not
    // accomodate the Interstate Request History, however Interstate Request
    // History is the field that is used in the action blocks that send
    // transactions and reject transactions.
    // ---------------------------------------------------------------------
    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine5))
    {
      if (IsEmpty(export.InterstateMiscellaneous.InformationTextLine4))
      {
        export.InterstateMiscellaneous.InformationTextLine4 =
          export.InterstateMiscellaneous.InformationTextLine5 ?? "";
        export.InterstateMiscellaneous.InformationTextLine5 = "";
      }
    }

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine4))
    {
      if (IsEmpty(export.InterstateMiscellaneous.InformationTextLine3))
      {
        export.InterstateMiscellaneous.InformationTextLine3 =
          export.InterstateMiscellaneous.InformationTextLine4 ?? "";

        if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine5))
        {
          export.InterstateMiscellaneous.InformationTextLine4 =
            export.InterstateMiscellaneous.InformationTextLine5 ?? "";
          export.InterstateMiscellaneous.InformationTextLine5 = "";
        }
        else
        {
          export.InterstateMiscellaneous.InformationTextLine4 = "";
        }
      }
    }

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine3))
    {
      if (IsEmpty(export.InterstateMiscellaneous.InformationTextLine2))
      {
        export.InterstateMiscellaneous.InformationTextLine2 =
          export.InterstateMiscellaneous.InformationTextLine3 ?? "";

        if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine4))
        {
          export.InterstateMiscellaneous.InformationTextLine3 =
            export.InterstateMiscellaneous.InformationTextLine4 ?? "";

          if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine5))
          {
            export.InterstateMiscellaneous.InformationTextLine4 =
              export.InterstateMiscellaneous.InformationTextLine5 ?? "";
            export.InterstateMiscellaneous.InformationTextLine5 = "";
          }
          else
          {
            export.InterstateMiscellaneous.InformationTextLine4 = "";
          }
        }
        else
        {
          export.InterstateMiscellaneous.InformationTextLine3 = "";
        }
      }
    }

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine2))
    {
      if (IsEmpty(export.InterstateMiscellaneous.InformationTextLine1))
      {
        export.InterstateMiscellaneous.InformationTextLine1 =
          export.InterstateMiscellaneous.InformationTextLine2 ?? "";

        if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine3))
        {
          export.InterstateMiscellaneous.InformationTextLine2 =
            export.InterstateMiscellaneous.InformationTextLine3 ?? "";

          if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine4))
          {
            export.InterstateMiscellaneous.InformationTextLine3 =
              export.InterstateMiscellaneous.InformationTextLine4 ?? "";

            if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine5))
            {
              export.InterstateMiscellaneous.InformationTextLine4 =
                export.InterstateMiscellaneous.InformationTextLine5 ?? "";
              export.InterstateMiscellaneous.InformationTextLine5 = "";
            }
            else
            {
              export.InterstateMiscellaneous.InformationTextLine4 = "";
            }
          }
          else
          {
            export.InterstateMiscellaneous.InformationTextLine3 = "";
          }
        }
        else
        {
          export.InterstateMiscellaneous.InformationTextLine2 = "";
        }
      }
    }

    export.ReferralInterstateRequestHistory.Note =
      Spaces(InterstateRequestHistory.Note_MaxLength);

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine1))
    {
      export.ReferralInterstateRequestHistory.Note =
        export.InterstateMiscellaneous.InformationTextLine1 ?? "";
    }

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine2))
    {
      export.ReferralInterstateRequestHistory.Note =
        TrimEnd(export.ReferralInterstateRequestHistory.Note) + (
          export.InterstateMiscellaneous.InformationTextLine2 ?? "");
    }

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine3))
    {
      export.ReferralInterstateRequestHistory.Note =
        TrimEnd(export.ReferralInterstateRequestHistory.Note) + (
          export.InterstateMiscellaneous.InformationTextLine3 ?? "");
    }

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine4))
    {
      export.ReferralInterstateRequestHistory.Note =
        TrimEnd(export.ReferralInterstateRequestHistory.Note) + (
          export.InterstateMiscellaneous.InformationTextLine4 ?? "");
    }

    if (!IsEmpty(export.InterstateMiscellaneous.InformationTextLine5))
    {
      export.ReferralInterstateRequestHistory.Note =
        TrimEnd(export.ReferralInterstateRequestHistory.Note) + (
          export.InterstateMiscellaneous.InformationTextLine5 ?? "");
    }

    // ---------------------------------------------------------------------
    // END of special processing to keep Interstate Request History
    // Note in sync with Interstate Miscellaneous Text Lines
    // ---------------------------------------------------------------------
    // ----------------------------------------------------------
    // When coming from the ICAS screen to reject a referral
    // ----------------------------------------------------------
    if (AsChar(export.LinkFromIcas.Flag) == 'Y')
    {
      // Protect all unprotected attributes on the screen except the note line
      var field1 = GetField(export.PromptAp, "selectChar");

      field1.Color = "";
      field1.Intensity = Intensity.Normal;
      field1.Protected = true;

      var field2 = GetField(export.ReferralFips, "stateAbbreviation");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.PromptState, "selectChar");

      field3.Intensity = Intensity.Normal;
      field3.Protected = true;

      var field4 = GetField(export.InterstateRequest, "otherStateCaseId");

      field4.Color = "cyan";
      field4.Highlighting = Highlighting.Underscore;
      field4.Protected = true;

      var field5 =
        GetField(export.ReferralInterstateRequestHistory, "functionalTypeCode");
        

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.PromptFunction, "selectChar");

      field6.Intensity = Intensity.Normal;
      field6.Protected = true;

      var field7 =
        GetField(export.ReferralInterstateRequestHistory, "actionCode");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 =
        GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.PromptReason, "selectChar");

      field9.Intensity = Intensity.Normal;
      field9.Protected = true;

      var field10 =
        GetField(export.ReferralInterstateRequestHistory, "attachmentIndicator");
        

      field10.Intensity = Intensity.Normal;
      field10.Protected = true;

      var field11 = GetField(export.PromptAttachment, "selectChar");

      field11.Intensity = Intensity.Normal;
      field11.Protected = true;

      for(export.Children.Index = 0; export.Children.Index < export
        .Children.Count; ++export.Children.Index)
      {
        var field =
          GetField(export.Children.Item.GexportSelectChild, "selectChar");

        field.Color = "cyan";
        field.Intensity = Intensity.Normal;
        field.Protected = true;
      }

      for(export.CourtOrders.Index = 0; export.CourtOrders.Index < export
        .CourtOrders.Count; ++export.CourtOrders.Index)
      {
        var field =
          GetField(export.CourtOrders.Item.GexportPromptCourtOrder, "selectChar");
          

        field.Color = "cyan";
        field.Intensity = Intensity.Normal;
        field.Protected = true;
      }

      switch(TrimEnd(global.Command))
      {
        case "FRMICAS":
          // Set all required attributes from ICAS on the screen.
          export.ReferralInterstateRequestHistory.TransactionSerialNum =
            import.FromIcas.TransSerialNumber;
          export.ReferralInterstateRequestHistory.TransactionDate =
            import.FromIcas.TransactionDate;
          export.InterstateRequest.OtherStateFips =
            import.FromIcas.OtherFipsState;
          export.InterstateRequest.CaseType = import.FromIcas.CaseType;
          export.ReferralFips.State = import.FromIcas.OtherFipsState;
          UseSiValidateStateFips();

          // *** - - - - - - - - - - - - - - - - - - - - - - - - - - - -
          //    pr245429   AHockman fix to allow rejected transactions to go 
          // through
          //                          the feds, changed  codes we are returning
          //                         to ones that do not require ot st case 
          // number.
          // *****- - - - - - - - - - - - - -  -- - - - - -  - - - - - - - -
          switch(TrimEnd(import.FromIcas.FunctionalTypeCode))
          {
            case "CSI":
              export.ReferralInterstateRequestHistory.FunctionalTypeCode =
                "CSI";
              export.ReferralInterstateRequestHistory.ActionCode = "P";
              export.ReferralInterstateRequestHistory.ActionReasonCode =
                "FUINF";

              break;
            case "ENF":
              export.ReferralInterstateRequestHistory.FunctionalTypeCode =
                "MSC";
              export.ReferralInterstateRequestHistory.ActionCode = "P";
              export.ReferralInterstateRequestHistory.ActionReasonCode =
                "REJCT";

              break;
            case "EST":
              export.ReferralInterstateRequestHistory.FunctionalTypeCode =
                "MSC";
              export.ReferralInterstateRequestHistory.ActionCode = "P";
              export.ReferralInterstateRequestHistory.ActionReasonCode =
                "REJCT";

              break;
            case "LO1":
              export.ReferralInterstateRequestHistory.FunctionalTypeCode =
                "LO1";
              export.ReferralInterstateRequestHistory.ActionCode = "P";
              export.ReferralInterstateRequestHistory.ActionReasonCode =
                "LUALL";

              break;
            case "PAT":
              export.ReferralInterstateRequestHistory.FunctionalTypeCode =
                "MSC";
              export.ReferralInterstateRequestHistory.ActionCode = "P";
              export.ReferralInterstateRequestHistory.ActionReasonCode =
                "REJCT";

              break;
            default:
              export.ReferralInterstateRequestHistory.FunctionalTypeCode =
                "MSC";
              export.ReferralInterstateRequestHistory.ActionCode = "P";
              export.ReferralInterstateRequestHistory.ActionReasonCode =
                "REJCT";

              break;
          }

          var field =
            GetField(export.InterstateMiscellaneous, "informationTextLine1");

          field.Protected = false;
          field.Focused = true;

          ExitState = "SI0000_NOTE_REQD";

          break;
        case "SEND":
          if (IsEmpty(export.InterstateMiscellaneous.InformationTextLine1))
          {
            var field12 =
              GetField(export.InterstateMiscellaneous, "informationTextLine1");

            field12.Protected = false;
            field12.Focused = true;

            ExitState = "SI0000_NOTE_REQD";

            return;
          }

          // :  UPDATE INCOMING TRANSACTION AND CREATE OUTGOING TRANSACTION FOR 
          // THIS REFERRAL.
          local.InterstateCase.TransSerialNumber =
            export.ReferralInterstateRequestHistory.TransactionSerialNum;
          local.InterstateCase.TransactionDate =
            export.ReferralInterstateRequestHistory.TransactionDate;
          local.InterstateCase.AssnDeactInd = "R";
          UseSiCloseIncomingCsenetTrans();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          ExitState = "ACO_NE0000_RETURN";

          break;
        case "RETURN":
          ExitState = "ACO_NE0000_RETURN";

          break;
        default:
          ExitState = "SI0000_MUST_FLOW_BACK_TO_ICAS";

          break;
      }

      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "IREQ":
        ExitState = "ECO_XFR_TO_IREQ";

        break;
      case "IATT":
        ExitState = "ECO_XFR_TO_IATT";

        break;
      case "IIMC":
        ExitState = "ECO_LNK_TO_IIMC";

        break;
      case "RETCDVL":
        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.PromptState.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue) && !
            Equal(import.SelectedCodeValue.Cdvalue,
            export.ReferralFips.StateAbbreviation))
          {
            export.ReferralFips.StateAbbreviation =
              import.SelectedCodeValue.Cdvalue;

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Protected = false;
            field.Focused = true;

            global.Command = "DISPLAY";
          }

          break;
        }

        if (AsChar(export.PromptFunction.SelectChar) == 'S')
        {
          export.PromptFunction.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.ReferralInterstateRequestHistory.FunctionalTypeCode =
              Substring(import.SelectedCodeValue.Cdvalue, 1, 3);
            export.ReferralInterstateRequestHistory.ActionCode =
              Substring(import.SelectedCodeValue.Cdvalue, 5, 1);

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "actionReasonCode");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.PromptReason.SelectChar) == 'S')
        {
          export.PromptReason.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.ReferralInterstateRequestHistory.ActionReasonCode =
              import.SelectedCodeValue.Cdvalue;

            var field = GetField(export.PromptAttachment, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.PromptAttachment.SelectChar) == 'S')
        {
          export.PromptAttachment.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Description))
          {
            export.ReferralInterstateRequestHistory.AttachmentIndicator = "Y";
            local.Position.Count =
              Find(export.ReferralInterstateRequestHistory.Note,
              TrimEnd(import.SelectedCodeValue.Description));

            if (local.Position.Count > 0)
            {
              goto Test;
            }

            if (AsChar(export.ReferralInterstateRequestHistory.ActionCode) == 'P'
              )
            {
              local.WorkArea.Text80 = "KANSAS SENT THE FOLLOWING DOCUMENTS:";
            }
            else
            {
              local.WorkArea.Text80 = "PLEASE SEND THE FOLLOWING DOCUMENTS:";
            }

            local.Length.Count =
              Length(TrimEnd(export.ReferralInterstateRequestHistory.Note));

            if (local.Length.Count == 0)
            {
              export.ReferralInterstateRequestHistory.Note =
                TrimEnd(local.WorkArea.Text80) + " " + TrimEnd
                (import.SelectedCodeValue.Description) + ".";
            }
            else
            {
              local.Position.Count =
                Find(export.ReferralInterstateRequestHistory.Note,
                "THE FOLLOWING DOCUMENTS:");

              if (local.Position.Count == 0)
              {
                export.ReferralInterstateRequestHistory.Note =
                  TrimEnd(export.ReferralInterstateRequestHistory.Note) + ".  " +
                  TrimEnd(local.WorkArea.Text80) + " " + TrimEnd
                  (import.SelectedCodeValue.Description) + ".";
              }
              else
              {
                export.ReferralInterstateRequestHistory.Note =
                  Substring(export.ReferralInterstateRequestHistory.Note, 1,
                  local.Position.Count + 24) + TrimEnd
                  (import.SelectedCodeValue.Description) + ", " + Substring
                  (export.ReferralInterstateRequestHistory.Note,
                  local.Position.Count + 25, local.Length.Count - local
                  .Position.Count - 24);
              }
            }

            local.Length.Count =
              Length(TrimEnd(export.ReferralInterstateRequestHistory.Note));
            export.InterstateMiscellaneous.InformationTextLine1 =
              Substring(export.ReferralInterstateRequestHistory.Note, 1, 79);

            if (local.Length.Count > 79)
            {
              export.InterstateMiscellaneous.InformationTextLine2 =
                Substring(export.ReferralInterstateRequestHistory.Note, 80, 79);
                

              if (local.Length.Count > 158)
              {
                export.InterstateMiscellaneous.InformationTextLine3 =
                  Substring(export.ReferralInterstateRequestHistory.Note, 159,
                  79);

                if (local.Length.Count > 237)
                {
                  export.InterstateMiscellaneous.InformationTextLine4 =
                    Substring(export.ReferralInterstateRequestHistory.Note, 238,
                    79);

                  if (local.Length.Count > 316)
                  {
                    export.InterstateMiscellaneous.InformationTextLine5 =
                      Substring(export.ReferralInterstateRequestHistory.Note,
                      317, 79);
                  }
                }
              }
            }

            for(export.Children.Index = 0; export.Children.Index < export
              .Children.Count; ++export.Children.Index)
            {
              var field =
                GetField(export.Children.Item.GexportSelectChild, "selectChar");
                

              field.Protected = false;
              field.Focused = true;

              break;
            }
          }

Test:

          if (IsEmpty(export.InterstateMiscellaneous.InformationTextLine1))
          {
            export.ReferralInterstateRequestHistory.AttachmentIndicator = "N";
          }
        }

        return;
      case "RETCOMP":
        export.PromptAp.SelectChar = "*";

        if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number) && !
          Equal(import.SelectedCsePersonsWorkSet.Number, export.Ap.Number))
        {
          MoveCsePersonsWorkSet2(import.SelectedCsePersonsWorkSet, export.Ap);
          MoveFips(local.NullFips, export.ReferralFips);
          global.Command = "DISPLAY";
        }

        break;
      case "RETLACS":
        for(export.CourtOrders.Index = 0; export.CourtOrders.Index < export
          .CourtOrders.Count; ++export.CourtOrders.Index)
        {
          if (IsEmpty(export.CourtOrders.Item.GexportPromptCourtOrder.SelectChar))
            
          {
            continue;
          }

          if (import.SelectedFromLacs.Identifier == 0)
          {
            export.CourtOrders.Update.GexportPromptCourtOrder.SelectChar = "*";
            export.CourtOrders.Update.GexportCourtOrder.Assign(
              local.NullLegalAction);

            continue;
          }

          if (ReadLegalAction())
          {
            if (Lt(entities.LegalAction.EndDate, local.Current.Date))
            {
              var field =
                GetField(export.CourtOrders.Item.GexportPromptCourtOrder,
                "selectChar");

              field.Error = true;

              ExitState = "CO0000_INVALID_EFF_OR_EXP_DATE";

              return;
            }
          }
          else
          {
            var field =
              GetField(export.CourtOrders.Item.GexportPromptCourtOrder,
              "selectChar");

            field.Error = true;

            ExitState = "LEGAL_ACTION_NF";

            return;
          }

          for(export.Children.Index = 0; export.Children.Index < export
            .Children.Count; ++export.Children.Index)
          {
            if (AsChar(export.Children.Item.GexportSelectChild.SelectChar) == 'S'
              )
            {
              UseSiCheckCourtCaseForReferral();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Children.Update.GexportSelectChild.SelectChar = "*";

                var field =
                  GetField(export.Children.Item.GexportSelectChild, "selectChar");
                  

                field.Error = true;

                ExitState = "ACO_NN0000_ALL_OK";
              }
            }
          }

          for(export.Children.Index = 0; export.Children.Index < export
            .Children.Count; ++export.Children.Index)
          {
            if (AsChar(export.Children.Item.GexportSelectChild.SelectChar) == '*'
              )
            {
              var field =
                GetField(export.Children.Item.GexportSelectChild, "selectChar");
                

              field.Error = true;

              ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

              break;
            }
          }

          export.CourtOrders.Update.GexportPromptCourtOrder.SelectChar = "*";
          export.CourtOrders.Update.GexportCourtOrder.Assign(
            import.SelectedFromLacs);

          return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "LIST":
        if (AsChar(export.PromptAp.SelectChar) == 'S')
        {
          if (IsEmpty(export.Case1.Number))
          {
            var field = GetField(export.Case1, "number");

            field.Error = true;

            ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }

          return;
        }

        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.Prompt.CodeName = local.State.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptFunction.SelectChar) == 'S')
        {
          export.Prompt.CodeName = "INTERSTATE TRANS TYPE";

          if (!IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
            
          {
            export.CombinationCode.CodeName = local.CsenetOgTranReason.CodeName;
            export.CombinationCodeValue.Cdvalue =
              export.ReferralInterstateRequestHistory.ActionReasonCode ?? Spaces
              (10);
          }

          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptReason.SelectChar) == 'S')
        {
          export.Prompt.CodeName = local.CsenetOgTranReason.CodeName;

          if (!IsEmpty(export.ReferralInterstateRequestHistory.
            FunctionalTypeCode))
          {
            export.CombinationCode.CodeName = "INTERSTATE TRANS TYPE";
            export.CombinationCodeValue.Cdvalue =
              export.ReferralInterstateRequestHistory.FunctionalTypeCode + " " +
              export.ReferralInterstateRequestHistory.ActionCode;
          }

          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptAttachment.SelectChar) == 'S')
        {
          export.Prompt.CodeName = "INTERSTATE ATTACHMENTS";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        // --------------------------------------------------
        // Only Prompt left is LACS
        // --------------------------------------------------
        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "OE0000_CASE_NO_REQUIRED_FOR_LACS";
        }
        else
        {
          ExitState = "CO0000_LIST_LEGL_ACT_BY_CSE_CASE";
        }

        return;
      case "DISPLAY":
        break;
      case "SEND":
        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (!Equal(export.Case1.Number, export.Previous.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "SI0000_DISPLAY_BEFORE_SEND";

          return;
        }

        if (!Equal(export.ReferralFips.StateAbbreviation,
          export.PreviousReferral.StateAbbreviation))
        {
          var field = GetField(export.ReferralFips, "stateAbbreviation");

          field.Error = true;

          ExitState = "SI0000_DISPLAY_BEFORE_SEND";

          return;
        }

        if (AsChar(export.CaseOpen.Flag) != 'Y')
        {
          ExitState = "SI0000_NO_CSENET_OUT_CLOSED_CASE";

          return;
        }

        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) != 'O')
        {
          ExitState = "SI0000_UPDATE_NOT_ALLOWED_CASE_C";

          return;
        }

        if (IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode))
        {
          var field =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsEmpty(export.ReferralInterstateRequestHistory.ActionCode))
        {
          var field =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
        {
          var field =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(AsChar(export.ReferralInterstateRequestHistory.
          AttachmentIndicator))
        {
          case 'Y':
            if (IsEmpty(export.InterstateMiscellaneous.InformationTextLine1))
            {
              var field1 =
                GetField(export.InterstateMiscellaneous, "informationTextLine1");
                

              field1.Error = true;

              ExitState = "SI0000_NOTE_REQD_FOR_ATTACHMENT";
            }

            break;
          case 'N':
            break;
          case ' ':
            export.ReferralInterstateRequestHistory.AttachmentIndicator = "N";

            break;
          default:
            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "attachmentIndicator");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // *****************************************************************
        // 2/12/02 T.Bobb PR136159 Added edit to prevent LO1
        // function to be used by this screen.
        // *****************************************************************
        if (Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
          "LO1"))
        {
          switch(AsChar(export.ReferralInterstateRequestHistory.ActionCode))
          {
            case 'P':
              var field1 =
                GetField(export.ReferralInterstateRequestHistory,
                "functionalTypeCode");

              field1.Error = true;

              var field2 =
                GetField(export.ReferralInterstateRequestHistory, "actionCode");
                

              field2.Error = true;

              ExitState = "SI0000_LO1_PROVISION_SEND";

              break;
            case 'R':
              var field3 =
                GetField(export.ReferralInterstateRequestHistory,
                "functionalTypeCode");

              field3.Error = true;

              var field4 =
                GetField(export.ReferralInterstateRequestHistory, "actionCode");
                

              field4.Error = true;

              ExitState = "SI0000_LO1_REQUEST_SEND";

              break;
            default:
              var field5 =
                GetField(export.ReferralInterstateRequestHistory,
                "functionalTypeCode");

              field5.Error = true;

              var field6 =
                GetField(export.ReferralInterstateRequestHistory, "actionCode");
                

              field6.Error = true;

              ExitState = "SI0000_LO1_INVALID_ACTION_CODE";

              break;
          }

          return;
        }

        // ----------------------------------------------------------
        // Closure transactions can only be sent from IIMC
        // for incoming interstate involvement
        // ----------------------------------------------------------
        if (Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
          "MSC") && AsChar
          (export.ReferralInterstateRequestHistory.ActionCode) == 'P' && Equal
          (export.ReferralInterstateRequestHistory.ActionReasonCode, 1, 3, "GSC"))
          
        {
          if (Equal(export.ReferralInterstateRequestHistory.ActionReasonCode,
            "GSC14"))
          {
            // T. Pierce CQ24439 -- GSC14, a new CSENet transaction code, is not
            // considered a closure code.
          }
          else
          {
            ExitState = "SI0000_UNABLE_TO_CREATE_SEND";

            return;
          }
        }

        local.Ap.Number = export.Ap.Number;
        local.Automatic.Flag = "N";
        local.Children.Index = -1;

        for(export.Children.Index = 0; export.Children.Index < export
          .Children.Count; ++export.Children.Index)
        {
          if (AsChar(export.Children.Item.GexportSelectChild.SelectChar) == 'S')
          {
            ++local.Children.Index;
            local.Children.CheckSize();

            local.Children.Update.GlocalChild.Number =
              export.Children.Item.GexportChild.Number;
          }
        }

        local.LegalActions.Index = -1;

        for(export.CourtOrders.Index = 0; export.CourtOrders.Index < export
          .CourtOrders.Count; ++export.CourtOrders.Index)
        {
          if (export.CourtOrders.Item.GexportCourtOrder.Identifier > 0)
          {
            ++local.LegalActions.Index;
            local.LegalActions.CheckSize();

            local.LegalActions.Update.G.Identifier =
              export.CourtOrders.Item.GexportCourtOrder.Identifier;
          }
        }

        UseSiCreateCsenetTrans();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.ReferralInterstateRequestHistory.TransactionSerialNum =
            local.InterstateCase.TransSerialNumber;
          export.ReferralInterstateRequestHistory.TransactionDate =
            local.InterstateCase.TransactionDate;
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.Case1.Number))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      // ----------------------------------------------------------
      // Find the Case details
      // ----------------------------------------------------------
      if (!Equal(export.Case1.Number, export.Previous.Number))
      {
        // ----------------------------------------------------
        // Blank out all display fields
        // ----------------------------------------------------
        MoveCase1(local.NullCase, export.Previous);
        MoveCsePersonsWorkSet2(local.NullCsePersonsWorkSet, export.Ap);
        MoveCsePersonsWorkSet2(local.NullCsePersonsWorkSet, export.Ar);
        export.HeaderLine.Text35 = "";
        export.CaseOpen.Flag = "";
        MoveInterstateRequest(local.NullInterstateRequest,
          export.InterstateRequest);
        MoveInterstateRequestHistory(local.NullInterstateRequestHistory,
          export.ReferralInterstateRequestHistory);
        export.InterstateMiscellaneous.
          Assign(local.NullInterstateMiscellaneous);
        MoveFips(local.NullFips, export.ReferralFips);
        export.PreviousReferral.StateAbbreviation =
          local.NullFips.StateAbbreviation;

        for(export.Children.Index = 0; export.Children.Index < export
          .Children.Count; ++export.Children.Index)
        {
          export.Children.Update.GexportSelectChild.SelectChar = "";
          MoveCsePersonsWorkSet1(local.NullCsePersonsWorkSet,
            export.Children.Update.GexportChild);
        }

        for(export.CourtOrders.Index = 0; export.CourtOrders.Index < export
          .CourtOrders.Count; ++export.CourtOrders.Index)
        {
          export.CourtOrders.Update.GexportPromptCourtOrder.SelectChar = "";
          export.CourtOrders.Update.GexportCourtOrder.Assign(
            local.NullLegalAction);
        }

        // ----------------------------------------------------------
        // If the user has just returned from IREQ this will be populated
        // ----------------------------------------------------------
        if (!IsEmpty(import.ApFromIreq.Number) && !
          Equal(import.ApFromIreq.Number, export.Ap.Number))
        {
          export.Ap.Number = import.ApFromIreq.Number;
          export.Ap.FormattedName = "";
        }

        UseSiReadCaseHeaderInformation();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.Children.Index = 0;
        export.Children.Clear();

        foreach(var item in ReadCsePerson())
        {
          export.Children.Update.GexportChild.Number =
            entities.CsePerson.Number;
          UseSiReadCsePerson2();

          if (!IsEmpty(local.AbendData.Type1))
          {
            var field =
              GetField(export.Children.Item.GexportChild, "firstName");

            field.Error = true;

            switch(AsChar(local.AbendData.Type1))
            {
              case 'A':
                switch(TrimEnd(local.AbendData.AdabasResponseCd))
                {
                  case "0113":
                    ExitState = "ACO_ADABAS_PERSON_NF_113";

                    break;
                  case "0148":
                    ExitState = "ACO_ADABAS_UNAVAILABLE";

                    break;
                  default:
                    ExitState = "ADABAS_READ_UNSUCCESSFUL";

                    break;
                }

                break;
              case 'C':
                ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

                break;
              default:
                ExitState = "ADABAS_INVALID_RETURN_CODE";

                break;
            }

            export.Children.Next();

            return;
          }

          export.Children.Update.GexportSelectChild.SelectChar = "S";
          export.Children.Next();
        }

        export.Previous.Number = export.Case1.Number;
      }

      // ----------------------------------------------------------
      // If the user has just returned from IREQ this will be populated
      // ----------------------------------------------------------
      if (!IsEmpty(import.SelectedFips.StateAbbreviation) && !
        Equal(import.SelectedFips.StateAbbreviation,
        export.ReferralFips.StateAbbreviation))
      {
        MoveFips(import.SelectedFips, export.ReferralFips);
      }

      // ----------------------------------------------------------
      // Find the State details
      // ----------------------------------------------------------
      if (!IsEmpty(export.ReferralFips.StateAbbreviation))
      {
        if (!Equal(export.ReferralFips.StateAbbreviation,
          export.PreviousReferral.StateAbbreviation))
        {
          if (ReadFips2())
          {
            MoveFips(entities.Fips, export.ReferralFips);
            export.PreviousReferral.StateAbbreviation =
              export.ReferralFips.StateAbbreviation;
          }
          else
          {
            var field = GetField(export.ReferralFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            return;
          }
        }
      }
      else
      {
        MoveFips(local.NullFips, export.ReferralFips);
        export.PreviousReferral.StateAbbreviation =
          export.ReferralFips.StateAbbreviation;
      }

      // ----------------------------------------------------------
      // Find an appropriate interstate request
      // The Case number is the only thing we are guaranteed to
      // know.  The AP and State may not be entered, in which case
      // we attempt to determine for which AP and State the Case
      // has an interstate request.
      // In the event of multiple APs with incoming interstate
      // involvement flow to COMP.
      // In the event of multiple States with incoming interstate
      // involvement for a single AP flow to IREQ.
      // ----------------------------------------------------------
      MoveInterstateRequest(local.NullInterstateRequest,
        export.InterstateRequest);
      ExitState = "CASE_NOT_INTERSTATE";

      foreach(var item in ReadInterstateRequestCsePerson())
      {
        if (export.ReferralFips.State > 0 && entities
          .InterstateRequest.OtherStateFips != export.ReferralFips.State)
        {
          var field = GetField(export.ReferralFips, "stateAbbreviation");

          field.Protected = false;
          field.Focused = true;

          ExitState = "AP_NOT_INVOLVED_IN_INTERSTATE";

          continue;
        }

        if (AsChar(entities.InterstateRequest.KsCaseInd) != 'N')
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          continue;
        }

        if (!IsEmpty(export.Ap.Number))
        {
          if (!Equal(entities.CsePerson.Number, export.Ap.Number))
          {
            if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
            {
              continue;
            }
            else
            {
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

              return;
            }
          }
        }
        else
        {
          export.Ap.Number = entities.CsePerson.Number;
          export.Ap.FormattedName = "";
        }

        if (export.InterstateRequest.IntHGeneratedId > 0)
        {
          if (IsEmpty(export.ReferralFips.StateAbbreviation) && !
            IsEmpty(import.SelectedFips.StateAbbreviation))
          {
            continue;
          }

          if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) != 'O')
          {
            break;
          }

          MoveInterstateRequest(local.NullInterstateRequest,
            export.InterstateRequest);
          ExitState = "ECO_LNK_TO_IREQ";

          return;
        }

        export.InterstateRequest.Assign(entities.InterstateRequest);
      }

      // ----------------------------------------------------------
      // We didn't find an appropriate interstate request
      // ----------------------------------------------------------
      if (export.InterstateRequest.IntHGeneratedId == 0)
      {
        return;
      }

      // ----------------------------------------------------------
      // We found an interstate request without the user specifying
      // the AP
      // Find the AP details now
      // ----------------------------------------------------------
      if (IsEmpty(export.Ap.FormattedName))
      {
        if (IsEmpty(export.Ap.Number))
        {
          ExitState = "AP_NF_RB";

          return;
        }

        UseSiReadCsePerson1();

        if (!IsEmpty(local.AbendData.Type1))
        {
          var field = GetField(export.Ap, "number");

          field.Error = true;

          switch(AsChar(local.AbendData.Type1))
          {
            case 'A':
              switch(TrimEnd(local.AbendData.AdabasResponseCd))
              {
                case "0113":
                  ExitState = "ACO_ADABAS_PERSON_NF_113";

                  break;
                case "0148":
                  ExitState = "ACO_ADABAS_UNAVAILABLE";

                  break;
                default:
                  ExitState = "ADABAS_READ_UNSUCCESSFUL";

                  break;
              }

              break;
            case 'C':
              ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

              break;
            default:
              ExitState = "ADABAS_INVALID_RETURN_CODE";

              break;
          }

          return;
        }

        UseSiFormatCsePersonName();
      }

      // ----------------------------------------------------------
      // We found an interstate request without the user specifying
      // the state
      // Find the State abbreviation now
      // ----------------------------------------------------------
      if (IsEmpty(export.ReferralFips.StateAbbreviation))
      {
        if (ReadFips1())
        {
          MoveFips(entities.Fips, export.ReferralFips);
          export.PreviousReferral.StateAbbreviation =
            export.ReferralFips.StateAbbreviation;
        }
        else
        {
          var field = GetField(export.ReferralFips, "stateAbbreviation");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_CODE";

          return;
        }
      }

      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "SI0000_DISP_OK_CLOSED_INTERSTATE";
      }
      else if (AsChar(export.InterstateRequest.OtherStateCaseStatus) != 'O')
      {
        ExitState = "FN0000_INTERSTATE_CASE_NOT_OPEN";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveChildren(Local.ChildrenGroup source,
    SiCreateCsenetTrans.Import.ChildrenGroup target)
  {
    target.GimportChild.Number = source.GlocalChild.Number;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.KsCaseInd = source.KsCaseInd;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedBy = source.CreatedBy;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.ActionReasonCode = source.ActionReasonCode;
    target.AttachmentIndicator = source.AttachmentIndicator;
    target.Note = source.Note;
  }

  private static void MoveLegalActions(Local.LegalActionsGroup source,
    SiCreateCsenetTrans.Import.LegalActionsGroup target)
  {
    target.G.Identifier = source.G.Identifier;
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

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Case1.Number = useImport.Case1.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.State.CodeName = useExport.State.CodeName;
    local.CsenetActionType.CodeName = useExport.CsenetActionType.CodeName;
    local.CsenetFunctionalType.CodeName =
      useExport.CsenetFunctionalType.CodeName;
    local.CsenetActionReason.CodeName = useExport.CsenetActionReason.CodeName;
    local.CsenetOgTranReason.CodeName = useExport.CsenetOgTranReason.CodeName;
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

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCheckCourtCaseForReferral()
  {
    var useImport = new SiCheckCourtCaseForReferral.Import();
    var useExport = new SiCheckCourtCaseForReferral.Export();

    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;
    useImport.Child.Number = export.Children.Item.GexportChild.Number;

    Call(SiCheckCourtCaseForReferral.Execute, useImport, useExport);
  }

  private void UseSiCloseIncomingCsenetTrans()
  {
    var useImport = new SiCloseIncomingCsenetTrans.Import();
    var useExport = new SiCloseIncomingCsenetTrans.Export();

    useImport.Send.Assign(export.ReferralInterstateRequestHistory);
    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiCloseIncomingCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateCsenetTrans()
  {
    var useImport = new SiCreateCsenetTrans.Import();
    var useExport = new SiCreateCsenetTrans.Export();

    useImport.InterstateRequest.Assign(export.InterstateRequest);
    useImport.InterstateRequestHistory.Assign(
      export.ReferralInterstateRequestHistory);
    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = local.Ap.Number;
    local.Children.CopyTo(useImport.Children, MoveChildren);
    local.LegalActions.CopyTo(useImport.LegalActions, MoveLegalActions);
    useImport.AutomaticTrans.Flag = local.Automatic.Flag;

    Call(SiCreateCsenetTrans.Execute, useImport, useExport);

    MoveInterstateCase(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Ap.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.Ap.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    MoveCsePersonsWorkSet2(useExport.Ap, export.Ap);
    MoveCsePersonsWorkSet2(useExport.Ar, export.Ar);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Children.Item.GexportChild.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.Children.Update.GexportChild.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
  }

  private void UseSiValidateStateFips()
  {
    var useImport = new SiValidateStateFips.Import();
    var useExport = new SiValidateStateFips.Export();

    MoveFips(export.ReferralFips, useImport.State);

    Call(SiValidateStateFips.Execute, useImport, useExport);

    MoveFips(useExport.Fips, export.ReferralFips);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Children.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.InterstateRequest.OtherStateFips);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
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
          command, "stateAbbreviation", export.ReferralFips.StateAbbreviation);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCsePerson()
  {
    entities.InterstateRequest.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadInterstateRequestCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 10);
        entities.InterstateRequest.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.
          SetInt32(command, "legalActionId", import.SelectedFromLacs.Identifier);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 2);
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
    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GimportSelectChild.
      /// </summary>
      [JsonPropertyName("gimportSelectChild")]
      public Common GimportSelectChild
      {
        get => gimportSelectChild ??= new();
        set => gimportSelectChild = value;
      }

      /// <summary>
      /// A value of GimportChild.
      /// </summary>
      [JsonPropertyName("gimportChild")]
      public CsePersonsWorkSet GimportChild
      {
        get => gimportChild ??= new();
        set => gimportChild = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Common gimportSelectChild;
      private CsePersonsWorkSet gimportChild;
    }

    /// <summary>A CourtOrdersGroup group.</summary>
    [Serializable]
    public class CourtOrdersGroup
    {
      /// <summary>
      /// A value of GimportPromptCourtOrder.
      /// </summary>
      [JsonPropertyName("gimportPromptCourtOrder")]
      public Common GimportPromptCourtOrder
      {
        get => gimportPromptCourtOrder ??= new();
        set => gimportPromptCourtOrder = value;
      }

      /// <summary>
      /// A value of GimportCourtOrder.
      /// </summary>
      [JsonPropertyName("gimportCourtOrder")]
      public LegalAction GimportCourtOrder
      {
        get => gimportCourtOrder ??= new();
        set => gimportCourtOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common gimportPromptCourtOrder;
      private LegalAction gimportCourtOrder;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of ReferralInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("referralInterstateRequestHistory")]
    public InterstateRequestHistory ReferralInterstateRequestHistory
    {
      get => referralInterstateRequestHistory ??= new();
      set => referralInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of ReferralFips.
    /// </summary>
    [JsonPropertyName("referralFips")]
    public Fips ReferralFips
    {
      get => referralFips ??= new();
      set => referralFips = value;
    }

    /// <summary>
    /// A value of PreviousReferral.
    /// </summary>
    [JsonPropertyName("previousReferral")]
    public Fips PreviousReferral
    {
      get => previousReferral ??= new();
      set => previousReferral = value;
    }

    /// <summary>
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
    }

    /// <summary>
    /// Gets a value of CourtOrders.
    /// </summary>
    [JsonIgnore]
    public Array<CourtOrdersGroup> CourtOrders => courtOrders ??= new(
      CourtOrdersGroup.Capacity);

    /// <summary>
    /// Gets a value of CourtOrders for json serialization.
    /// </summary>
    [JsonPropertyName("courtOrders")]
    [Computed]
    public IList<CourtOrdersGroup> CourtOrders_Json
    {
      get => courtOrders;
      set => CourtOrders.Assign(value);
    }

    /// <summary>
    /// A value of PromptAp.
    /// </summary>
    [JsonPropertyName("promptAp")]
    public Common PromptAp
    {
      get => promptAp ??= new();
      set => promptAp = value;
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
    /// A value of PromptFunction.
    /// </summary>
    [JsonPropertyName("promptFunction")]
    public Common PromptFunction
    {
      get => promptFunction ??= new();
      set => promptFunction = value;
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
    /// A value of PromptAttachment.
    /// </summary>
    [JsonPropertyName("promptAttachment")]
    public Common PromptAttachment
    {
      get => promptAttachment ??= new();
      set => promptAttachment = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of StateFromIatt.
    /// </summary>
    [JsonPropertyName("stateFromIatt")]
    public Common StateFromIatt
    {
      get => stateFromIatt ??= new();
      set => stateFromIatt = value;
    }

    /// <summary>
    /// A value of LinkFromIcas.
    /// </summary>
    [JsonPropertyName("linkFromIcas")]
    public Common LinkFromIcas
    {
      get => linkFromIcas ??= new();
      set => linkFromIcas = value;
    }

    /// <summary>
    /// A value of FromIcas.
    /// </summary>
    [JsonPropertyName("fromIcas")]
    public InterstateCase FromIcas
    {
      get => fromIcas ??= new();
      set => fromIcas = value;
    }

    /// <summary>
    /// A value of ApFromIreq.
    /// </summary>
    [JsonPropertyName("apFromIreq")]
    public CsePerson ApFromIreq
    {
      get => apFromIreq ??= new();
      set => apFromIreq = value;
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
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of SelectedFromLacs.
    /// </summary>
    [JsonPropertyName("selectedFromLacs")]
    public LegalAction SelectedFromLacs
    {
      get => selectedFromLacs ??= new();
      set => selectedFromLacs = value;
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

    private Case1 case1;
    private Case1 previous;
    private WorkArea headerLine;
    private Common caseOpen;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory referralInterstateRequestHistory;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Fips referralFips;
    private Fips previousReferral;
    private Array<ChildrenGroup> children;
    private Array<CourtOrdersGroup> courtOrders;
    private Common promptAp;
    private Common promptState;
    private Common promptFunction;
    private Common promptReason;
    private Common promptAttachment;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Common stateFromIatt;
    private Common linkFromIcas;
    private InterstateCase fromIcas;
    private CsePerson apFromIreq;
    private InterstateRequestHistory selectedInterstateRequestHistory;
    private Fips selectedFips;
    private LegalAction selectedFromLacs;
    private CodeValue selectedCodeValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GexportSelectChild.
      /// </summary>
      [JsonPropertyName("gexportSelectChild")]
      public Common GexportSelectChild
      {
        get => gexportSelectChild ??= new();
        set => gexportSelectChild = value;
      }

      /// <summary>
      /// A value of GexportChild.
      /// </summary>
      [JsonPropertyName("gexportChild")]
      public CsePersonsWorkSet GexportChild
      {
        get => gexportChild ??= new();
        set => gexportChild = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Common gexportSelectChild;
      private CsePersonsWorkSet gexportChild;
    }

    /// <summary>A CourtOrdersGroup group.</summary>
    [Serializable]
    public class CourtOrdersGroup
    {
      /// <summary>
      /// A value of GexportPromptCourtOrder.
      /// </summary>
      [JsonPropertyName("gexportPromptCourtOrder")]
      public Common GexportPromptCourtOrder
      {
        get => gexportPromptCourtOrder ??= new();
        set => gexportPromptCourtOrder = value;
      }

      /// <summary>
      /// A value of GexportCourtOrder.
      /// </summary>
      [JsonPropertyName("gexportCourtOrder")]
      public LegalAction GexportCourtOrder
      {
        get => gexportCourtOrder ??= new();
        set => gexportCourtOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common gexportPromptCourtOrder;
      private LegalAction gexportCourtOrder;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of ReferralInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("referralInterstateRequestHistory")]
    public InterstateRequestHistory ReferralInterstateRequestHistory
    {
      get => referralInterstateRequestHistory ??= new();
      set => referralInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of ReferralFips.
    /// </summary>
    [JsonPropertyName("referralFips")]
    public Fips ReferralFips
    {
      get => referralFips ??= new();
      set => referralFips = value;
    }

    /// <summary>
    /// A value of PreviousReferral.
    /// </summary>
    [JsonPropertyName("previousReferral")]
    public Fips PreviousReferral
    {
      get => previousReferral ??= new();
      set => previousReferral = value;
    }

    /// <summary>
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
    }

    /// <summary>
    /// Gets a value of CourtOrders.
    /// </summary>
    [JsonIgnore]
    public Array<CourtOrdersGroup> CourtOrders => courtOrders ??= new(
      CourtOrdersGroup.Capacity);

    /// <summary>
    /// Gets a value of CourtOrders for json serialization.
    /// </summary>
    [JsonPropertyName("courtOrders")]
    [Computed]
    public IList<CourtOrdersGroup> CourtOrders_Json
    {
      get => courtOrders;
      set => CourtOrders.Assign(value);
    }

    /// <summary>
    /// A value of PromptAp.
    /// </summary>
    [JsonPropertyName("promptAp")]
    public Common PromptAp
    {
      get => promptAp ??= new();
      set => promptAp = value;
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
    /// A value of PromptFunction.
    /// </summary>
    [JsonPropertyName("promptFunction")]
    public Common PromptFunction
    {
      get => promptFunction ??= new();
      set => promptFunction = value;
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
    /// A value of PromptAttachment.
    /// </summary>
    [JsonPropertyName("promptAttachment")]
    public Common PromptAttachment
    {
      get => promptAttachment ??= new();
      set => promptAttachment = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of CombinationCode.
    /// </summary>
    [JsonPropertyName("combinationCode")]
    public Code CombinationCode
    {
      get => combinationCode ??= new();
      set => combinationCode = value;
    }

    /// <summary>
    /// A value of CombinationCodeValue.
    /// </summary>
    [JsonPropertyName("combinationCodeValue")]
    public CodeValue CombinationCodeValue
    {
      get => combinationCodeValue ??= new();
      set => combinationCodeValue = value;
    }

    /// <summary>
    /// A value of LinkFromIcas.
    /// </summary>
    [JsonPropertyName("linkFromIcas")]
    public Common LinkFromIcas
    {
      get => linkFromIcas ??= new();
      set => linkFromIcas = value;
    }

    private Case1 case1;
    private Case1 previous;
    private WorkArea headerLine;
    private Common caseOpen;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory referralInterstateRequestHistory;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Fips referralFips;
    private Fips previousReferral;
    private Array<ChildrenGroup> children;
    private Array<CourtOrdersGroup> courtOrders;
    private Common promptAp;
    private Common promptState;
    private Common promptFunction;
    private Common promptReason;
    private Common promptAttachment;
    private NextTranInfo hidden;
    private Standard standard;
    private Code prompt;
    private Code combinationCode;
    private CodeValue combinationCodeValue;
    private Common linkFromIcas;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LegalActionsGroup group.</summary>
    [Serializable]
    public class LegalActionsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public LegalAction G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private LegalAction g;
    }

    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GlocalChild.
      /// </summary>
      [JsonPropertyName("glocalChild")]
      public CsePerson GlocalChild
      {
        get => glocalChild ??= new();
        set => glocalChild = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private CsePerson glocalChild;
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
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of NullInterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("nullInterstateMiscellaneous")]
    public InterstateMiscellaneous NullInterstateMiscellaneous
    {
      get => nullInterstateMiscellaneous ??= new();
      set => nullInterstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of NullInterstateRequest.
    /// </summary>
    [JsonPropertyName("nullInterstateRequest")]
    public InterstateRequest NullInterstateRequest
    {
      get => nullInterstateRequest ??= new();
      set => nullInterstateRequest = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
    }

    /// <summary>
    /// A value of NullFips.
    /// </summary>
    [JsonPropertyName("nullFips")]
    public Fips NullFips
    {
      get => nullFips ??= new();
      set => nullFips = value;
    }

    /// <summary>
    /// Gets a value of LegalActions.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionsGroup> LegalActions => legalActions ??= new(
      LegalActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActions for json serialization.
    /// </summary>
    [JsonPropertyName("legalActions")]
    [Computed]
    public IList<LegalActionsGroup> LegalActions_Json
    {
      get => legalActions;
      set => LegalActions.Assign(value);
    }

    /// <summary>
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public Common Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
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

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of CsenetActionType.
    /// </summary>
    [JsonPropertyName("csenetActionType")]
    public Code CsenetActionType
    {
      get => csenetActionType ??= new();
      set => csenetActionType = value;
    }

    /// <summary>
    /// A value of CsenetFunctionalType.
    /// </summary>
    [JsonPropertyName("csenetFunctionalType")]
    public Code CsenetFunctionalType
    {
      get => csenetFunctionalType ??= new();
      set => csenetFunctionalType = value;
    }

    /// <summary>
    /// A value of CsenetActionReason.
    /// </summary>
    [JsonPropertyName("csenetActionReason")]
    public Code CsenetActionReason
    {
      get => csenetActionReason ??= new();
      set => csenetActionReason = value;
    }

    /// <summary>
    /// A value of CsenetOgTranReason.
    /// </summary>
    [JsonPropertyName("csenetOgTranReason")]
    public Code CsenetOgTranReason
    {
      get => csenetOgTranReason ??= new();
      set => csenetOgTranReason = value;
    }

    /// <summary>
    /// A value of CrossValidation.
    /// </summary>
    [JsonPropertyName("crossValidation")]
    public CodeValue CrossValidation
    {
      get => crossValidation ??= new();
      set => crossValidation = value;
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
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullCase.
    /// </summary>
    [JsonPropertyName("nullCase")]
    public Case1 NullCase
    {
      get => nullCase ??= new();
      set => nullCase = value;
    }

    /// <summary>
    /// A value of NullInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("nullInterstateRequestHistory")]
    public InterstateRequestHistory NullInterstateRequestHistory
    {
      get => nullInterstateRequestHistory ??= new();
      set => nullInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of ZdelLocCountryClosureInfo.
    /// </summary>
    [JsonPropertyName("zdelLocCountryClosureInfo")]
    public Common ZdelLocCountryClosureInfo
    {
      get => zdelLocCountryClosureInfo ??= new();
      set => zdelLocCountryClosureInfo = value;
    }

    /// <summary>
    /// A value of ZdelLocalCaseOpen.
    /// </summary>
    [JsonPropertyName("zdelLocalCaseOpen")]
    public Common ZdelLocalCaseOpen
    {
      get => zdelLocalCaseOpen ??= new();
      set => zdelLocalCaseOpen = value;
    }

    /// <summary>
    /// A value of ZdelLocalErrorCommon.
    /// </summary>
    [JsonPropertyName("zdelLocalErrorCommon")]
    public Common ZdelLocalErrorCommon
    {
      get => zdelLocalErrorCommon ??= new();
      set => zdelLocalErrorCommon = value;
    }

    /// <summary>
    /// A value of ZdelLocalZeroFill.
    /// </summary>
    [JsonPropertyName("zdelLocalZeroFill")]
    public TextWorkArea ZdelLocalZeroFill
    {
      get => zdelLocalZeroFill ??= new();
      set => zdelLocalZeroFill = value;
    }

    /// <summary>
    /// A value of ZdelLocalRefreshInterstateRequest.
    /// </summary>
    [JsonPropertyName("zdelLocalRefreshInterstateRequest")]
    public InterstateRequest ZdelLocalRefreshInterstateRequest
    {
      get => zdelLocalRefreshInterstateRequest ??= new();
      set => zdelLocalRefreshInterstateRequest = value;
    }

    /// <summary>
    /// A value of ZdelLocalCodeValidation.
    /// </summary>
    [JsonPropertyName("zdelLocalCodeValidation")]
    public InterstateCase ZdelLocalCodeValidation
    {
      get => zdelLocalCodeValidation ??= new();
      set => zdelLocalCodeValidation = value;
    }

    /// <summary>
    /// A value of ZdelLocalErrorWorkArea.
    /// </summary>
    [JsonPropertyName("zdelLocalErrorWorkArea")]
    public WorkArea ZdelLocalErrorWorkArea
    {
      get => zdelLocalErrorWorkArea ??= new();
      set => zdelLocalErrorWorkArea = value;
    }

    /// <summary>
    /// A value of ZdelLocalIncomingCase.
    /// </summary>
    [JsonPropertyName("zdelLocalIncomingCase")]
    public Common ZdelLocalIncomingCase
    {
      get => zdelLocalIncomingCase ??= new();
      set => zdelLocalIncomingCase = value;
    }

    /// <summary>
    /// A value of ZdelInfrastructure.
    /// </summary>
    [JsonPropertyName("zdelInfrastructure")]
    public Infrastructure ZdelInfrastructure
    {
      get => zdelInfrastructure ??= new();
      set => zdelInfrastructure = value;
    }

    /// <summary>
    /// A value of ZdelLocalRefreshServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelLocalRefreshServiceProvider")]
    public ServiceProvider ZdelLocalRefreshServiceProvider
    {
      get => zdelLocalRefreshServiceProvider ??= new();
      set => zdelLocalRefreshServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelLocalHistoryInfo.
    /// </summary>
    [JsonPropertyName("zdelLocalHistoryInfo")]
    public Common ZdelLocalHistoryInfo
    {
      get => zdelLocalHistoryInfo ??= new();
      set => zdelLocalHistoryInfo = value;
    }

    /// <summary>
    /// A value of ZdelLocalStateClosureInfo.
    /// </summary>
    [JsonPropertyName("zdelLocalStateClosureInfo")]
    public Common ZdelLocalStateClosureInfo
    {
      get => zdelLocalStateClosureInfo ??= new();
      set => zdelLocalStateClosureInfo = value;
    }

    /// <summary>
    /// A value of ZdelCsenetStateTable.
    /// </summary>
    [JsonPropertyName("zdelCsenetStateTable")]
    public CsenetStateTable ZdelCsenetStateTable
    {
      get => zdelCsenetStateTable ??= new();
      set => zdelCsenetStateTable = value;
    }

    /// <summary>
    /// A value of ZdelLocalMultipleAps.
    /// </summary>
    [JsonPropertyName("zdelLocalMultipleAps")]
    public Common ZdelLocalMultipleAps
    {
      get => zdelLocalMultipleAps ??= new();
      set => zdelLocalMultipleAps = value;
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
    /// A value of ZdelLocalRefreshProgram.
    /// </summary>
    [JsonPropertyName("zdelLocalRefreshProgram")]
    public Program ZdelLocalRefreshProgram
    {
      get => zdelLocalRefreshProgram ??= new();
      set => zdelLocalRefreshProgram = value;
    }

    /// <summary>
    /// A value of ZdelLocalValidation.
    /// </summary>
    [JsonPropertyName("zdelLocalValidation")]
    public CodeValue ZdelLocalValidation
    {
      get => zdelLocalValidation ??= new();
      set => zdelLocalValidation = value;
    }

    /// <summary>
    /// A value of ZdelLocalAr.
    /// </summary>
    [JsonPropertyName("zdelLocalAr")]
    public CsePersonsWorkSet ZdelLocalAr
    {
      get => zdelLocalAr ??= new();
      set => zdelLocalAr = value;
    }

    private Common position;
    private Common length;
    private WorkArea workArea;
    private InterstateMiscellaneous nullInterstateMiscellaneous;
    private DateWorkArea nullDateWorkArea;
    private InterstateRequest nullInterstateRequest;
    private LegalAction nullLegalAction;
    private Fips nullFips;
    private Array<LegalActionsGroup> legalActions;
    private Array<ChildrenGroup> children;
    private CsePerson ap;
    private InterstateCase interstateCase;
    private DateWorkArea current;
    private Common automatic;
    private Common count;
    private Code state;
    private Code csenetActionType;
    private Code csenetFunctionalType;
    private Code csenetActionReason;
    private Code csenetOgTranReason;
    private CodeValue crossValidation;
    private AbendData abendData;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private Case1 nullCase;
    private InterstateRequestHistory nullInterstateRequestHistory;
    private Common zdelLocCountryClosureInfo;
    private Common zdelLocalCaseOpen;
    private Common zdelLocalErrorCommon;
    private TextWorkArea zdelLocalZeroFill;
    private InterstateRequest zdelLocalRefreshInterstateRequest;
    private InterstateCase zdelLocalCodeValidation;
    private WorkArea zdelLocalErrorWorkArea;
    private Common zdelLocalIncomingCase;
    private Infrastructure zdelInfrastructure;
    private ServiceProvider zdelLocalRefreshServiceProvider;
    private Common zdelLocalHistoryInfo;
    private Common zdelLocalStateClosureInfo;
    private CsenetStateTable zdelCsenetStateTable;
    private Common zdelLocalMultipleAps;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Program zdelLocalRefreshProgram;
    private CodeValue zdelLocalValidation;
    private CsePersonsWorkSet zdelLocalAr;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public InterstateCase Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private InterstateRequest interstateRequest;
    private CsePerson csePerson;
    private Case1 case1;
    private LegalAction legalAction;
    private CaseRole ap;
    private Fips fips;
    private CaseRole child;
    private InterstateCase zdel;
  }
#endregion
}

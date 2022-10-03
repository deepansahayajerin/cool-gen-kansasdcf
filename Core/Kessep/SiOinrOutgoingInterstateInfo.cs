// Program: SI_OINR_OUTGOING_INTERSTATE_INFO, ID: 373468749, model: 746.
// Short name: SWEOINRP
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
/// A program: SI_OINR_OUTGOING_INTERSTATE_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiOinrOutgoingInterstateInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_OINR_OUTGOING_INTERSTATE_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiOinrOutgoingInterstateInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiOinrOutgoingInterstateInfo.
  /// </summary>
  public SiOinrOutgoingInterstateInfo(IContext context, Import import,
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
    // -----------------------------------------------------------------
    //                  M A I N T E N A N C E    L O G
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------
    // 		Various Developers		Various Developments
    // 04/23/2002	T Bobb		PR00128861
    // Do not set the KS_CASE_IND to spaces when closing.
    // 09/16/2002	T Bobb		PR00155338
    // Update other state case number when doing a manual conversion.
    // 11/05/2002	G Vandy		PR00161719
    // Allow U class legal actions for PAT functional type codes.
    // 11/08/2002	M Ramirez	138863
    // Fix transactions sent from OINR
    // 11/08/2002	M Ramirez
    // Rewrote entire screen to assist maintenance
    // 12/08/2003        L Brown        PR00191842
    // When a case closes, record close date in case closure date.
    // 12/17/2003        A Hockman  CSENet 
    // Eflash 03-05 Case Closure Transaction
    // Code changes.
    // 
    // Eliminated GSC01 and replace with GSC02.
    // 12/18/2003        L Brown        PR00158330
    // When exceptions 'ENF', 'EST', or 'PAT' are entered, must not have other 
    // state case identifier.
    // Change 'Responding State' to 'Other State'.
    // 01/07/2004        L Brown        PR00166872
    // This allows selection of state without causing problems of not being able
    // to finish adding the
    // interstate request.
    // 01/13/2004        L Brown        PR00138968
    // Blank state abbrev for new OINR screen, or when case number is changed.
    // 07/30/2004        L Brown        PR00181670
    // Change processing so that if an 'ADD' is aborted because of an error, the
    // correction can be made,
    // and the 'ADD' function continued.
    // 09/16/2004        L Brown       PR00216588
    // Add code so that when an ACTION_CODE of an 'R' is used with 
    // FUNCTIONAL_TYPE_CODE of 'ENF', 'EST',
    // or 'PAT' during 'ADD' processing, the correct LEGAL_ACTION CLASSIFICATION
    // is submitted. For a
    // FUNCTIONAL_TYPE_CODE of 'ENF', a LEGAL_ACTION CLASSIFICATION 'J' is to be
    // used. The remaining two,
    // 'EST' and 'PAT', are to use LEGAL_ACTION CLASSIFICATION of 'U'. Used an 
    // added field 'LOCAL_FUNCTION_CHECK IEF_SUPPLIED FLAG' to indicate the
    // presense or absense of the required field values.
    // 05/10/06 GVandy 	 WR230751	Add support for Foreign and Tribal IV-D 
    // agencies.
    // 3/28/2006        AHockman         pr 247093 	fix to force users to create
    // a transaction on a new outgoing 							interstate case.
    // 11/28/07  	AHockman	cq1415        resend problem.  The issue per Raj's 
    // writeup  is that we update the rqst table but not the interstate case
    // table when a new other state case number is added.   Staff get an error
    // so they add the case number and do a resend but when we resend we read
    // interstate case so the transaction is not being resent.    Also changing
    // pf5 add/ manual convert to just pf5 add per Jolene Bickel.   AFter
    // further review the cq1415 resend problem will be fixed another way, since
    // the case table is actually a history of what we have sent and received.
    // cq526 highlight issues on a send will be fixed though.  01/31/08.
    // Anita
    // 4-22-08   cq 4022  fixed code in the OICNV add area to set the direction 
    // ind to O so that these cases will be picked up by the batch 244 when
    // creating citax transactions.   Anita Hockman
    // 07-11-08   cq5898   users now want to be able to send blank transactions.
    // Added the word blank in the table to force them to choose something and
    // we convert it to spaces later.     Anita Hockman
    // 07-17-08 Arun Mathias - CQ#318 - Display a specific message when the user
    // wants to re-open the insterstate case on a closed kansas case.
    // 12-12-08 Arun Mathias - CQ#527 - If the interstate case is open then the 
    // case status field should not be protected.
    // 07-27-09 Anita Hockman  cq11646  changes to be able to display when there
    // are multiple  ap's so you can get to the add if you want a new outgoing
    // case.    Added a view  on multiple ap flag and then check that and send
    // to comp  to select an ap before display if the flag is set.
    // 02-07-2011 Tony Pierce CQ24439 -- Incorporate new OCSE transaction types 
    // and equate with new Kansas closure codes.
    // 07-25-2011  A Hockman cq7200 - allow for o/s case# to be entered on ADD.
    // 03/14/2018  JHarden   CQ60510  Add fax # field to OINR
    // 03/30/2020  GVandy   CQ67805  Can't add a tribal interstate case for a 
    // case that was previously outgoing to another state.
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
    MoveCase1(import.PreviousCase, export.PreviousCase);
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.HeaderLine.Text35 = import.HeaderLine.Text35;
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.PreviousInterstateRequest.Assign(import.PreviousInterstateRequest);
    MoveInterstateRequestHistory2(import.ReferralInterstateRequestHistory,
      export.ReferralInterstateRequestHistory);
    MoveFips(import.ReferralFips, export.ReferralFips);
    export.PreviousReferral.StateAbbreviation =
      import.PreviousReferral.StateAbbreviation;
    export.InterstateContact.Assign(import.InterstateContact);
    export.PreviousInterstateContact.Assign(import.PreviousInterstateContact);
    export.InterstateContactAddress.Assign(import.InterstateContactAddress);
    export.PreviousInterstateContactAddress.Assign(
      import.PreviousInterstateContactAddress);
    export.PromptAp.SelectChar = import.PromptAp.SelectChar;
    export.PromptFunction.SelectChar = import.PromptFunctionCd.SelectChar;
    export.PromptReason.SelectChar = import.PromptReason.SelectChar;
    export.PromptCloseReason.SelectChar = import.PromptCloseReason.SelectChar;
    export.PromptState.SelectChar = import.PromptState.SelectChar;
    export.PromptAttachment.SelectChar = import.PromptAttachment.SelectChar;
    export.PromptCountry.SelectChar = import.PromptCountry.SelectChar;
    export.PromptTribalAgency.SelectChar = import.PromptTribalAgency.SelectChar;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.PreviousCommon.Command = import.PreviousCommon.Command;
    export.DoubleConfirmation.Flag = import.DoubleConfiirmation.Flag;

    if (!IsEmpty(import.PreviousCommon.Command) && !
      Equal(global.Command, import.PreviousCommon.Command))
    {
      global.Command = "DISPLAY";
    }

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
          if (!IsEmpty(import.PreviousCommon.Command) && Equal
            (global.Command, "DISPLAY"))
          {
            export.Children.Next();

            continue;
          }

          var field =
            GetField(export.Children.Item.GexportSelectChild, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      export.Children.Next();
    }

    export.CourtOrder.Index = 0;
    export.CourtOrder.Clear();

    for(import.CourtOrder.Index = 0; import.CourtOrder.Index < import
      .CourtOrder.Count; ++import.CourtOrder.Index)
    {
      if (export.CourtOrder.IsFull)
      {
        break;
      }

      export.CourtOrder.Update.GexportPromptCourtOrder.SelectChar =
        import.CourtOrder.Item.GimportPromptCourtOrder.SelectChar;
      export.CourtOrder.Update.GexportCourtOrder.Assign(
        import.CourtOrder.Item.GimportCourtOrder);

      switch(AsChar(export.CourtOrder.Item.GexportPromptCourtOrder.SelectChar))
      {
        case 'S':
          ++local.Count.Count;

          break;
        case '*':
          export.CourtOrder.Update.GexportPromptCourtOrder.SelectChar = "";

          break;
        case ' ':
          break;
        default:
          if (!IsEmpty(import.PreviousCommon.Command) && Equal
            (global.Command, "DISPLAY"))
          {
            export.CourtOrder.Next();

            continue;
          }

          var field =
            GetField(export.CourtOrder.Item.GexportPromptCourtOrder,
            "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      export.CourtOrder.Next();
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

    switch(AsChar(export.PromptCountry.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptCountry.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptCountry, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
    }

    switch(AsChar(export.PromptTribalAgency.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptTribalAgency.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptTribalAgency, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
    }

    switch(AsChar(export.PromptCloseReason.SelectChar))
    {
      case 'S':
        ++local.Count.Count;

        break;
      case '*':
        export.PromptCloseReason.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptCloseReason, "selectChar");

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
      if (!IsEmpty(import.PreviousCommon.Command) && Equal
        (global.Command, "DISPLAY"))
      {
        goto Test1;
      }

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

      if (AsChar(export.PromptCountry.SelectChar) == 'S')
      {
        var field = GetField(export.PromptCountry, "selectChar");

        field.Error = true;
      }

      if (AsChar(export.PromptTribalAgency.SelectChar) == 'S')
      {
        var field = GetField(export.PromptTribalAgency, "selectChar");

        field.Error = true;
      }

      if (AsChar(export.PromptCloseReason.SelectChar) == 'S')
      {
        var field = GetField(export.PromptCloseReason, "selectChar");

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

      for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
        .CourtOrder.Count; ++export.CourtOrder.Index)
      {
        if (AsChar(export.CourtOrder.Item.GexportPromptCourtOrder.SelectChar) ==
          'S')
        {
          var field =
            GetField(export.CourtOrder.Item.GexportPromptCourtOrder,
            "selectChar");

          field.Error = true;
        }
      }
    }
    else if (local.Count.Count == 1)
    {
      if (!Equal(global.Command, "LIST") && !
        Equal(global.Command, "RETCOMP") && !
        Equal(global.Command, "RETLACS") && !Equal(global.Command, "RETCDVL"))
      {
        if (!IsEmpty(import.PreviousCommon.Command) && Equal
          (global.Command, "DISPLAY"))
        {
        }
        else
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
        }
      }
    }
    else if (Equal(global.Command, "LIST"))
    {
      ExitState = "ACO_NE0000_NO_SELECTION_MADE";
    }

Test1:

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
      export.Hidden.CaseNumber = export.PreviousCase.Number;
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
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "REOPEN") || Equal
      (global.Command, "SEND"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "SEND"))
    {
      // ---------------------------------------------------------------
      // Don't allow a CLOSURE transaction on an SEND.  Use UPDATE
      // ---------------------------------------------------------------
      if (Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
        "MSC") && AsChar
        (export.ReferralInterstateRequestHistory.ActionCode) == 'P' && Equal
        (export.ReferralInterstateRequestHistory.ActionReasonCode, 1, 3, "GSC"))
      {
        if (Equal(export.ReferralInterstateRequestHistory.ActionReasonCode,
          "GSC17"))
        {
          // T. Pierce  CQ24439 -- GSC17 is not equated with a closure code.
        }
        else
        {
          global.Command = "UPDATE";
        }
      }
    }

    // *** CQ#527 Changes Begin Here ***
    // ** Also, Changed the Screen attribute to unprotected **
    if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O' || AsChar
      (export.PreviousInterstateRequest.OtherStateCaseStatus) == 'O')
    {
      var field = GetField(export.InterstateRequest, "otherStateCaseStatus");

      field.Color = "green";
      field.Protected = false;
    }

    // *** CQ#527 Changes End   Here ***
    // ----------------------------------------------------
    // Common edits
    // ----------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "REOPEN") || Equal(global.Command, "SEND"))
    {
      if (IsEmpty(export.Case1.Number))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (!Equal(export.Case1.Number, export.PreviousCase.Number))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      if (AsChar(export.CaseOpen.Flag) != 'Y')
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        if (Equal(global.Command, "SEND"))
        {
          ExitState = "SI0000_NO_CSENET_OUT_CLOSED_CASE";

          // *** CQ#318 Display a specific message **
          // *** Changes Begin Here ***
        }
        else if (Equal(global.Command, "REOPEN"))
        {
          ExitState = "SI0000_MUST_REOPEN_FROM_COMN";

          // *** Changes End Here   ***
        }
        else
        {
          ExitState = "CANNOT_MODIFY_CLOSED_CASE";
        }

        return;
      }

      // @@@
      // --  Validate State, Country, and Tribal Agency
      // Determine how many of State, Country, and Tribal agency were entered.
      local.Common.Count = 0;

      if (!IsEmpty(export.ReferralFips.StateAbbreviation))
      {
        ++local.Common.Count;
      }

      if (!IsEmpty(export.InterstateRequest.Country))
      {
        ++local.Common.Count;
      }

      if (!IsEmpty(export.InterstateRequest.TribalAgency))
      {
        ++local.Common.Count;
      }

      if (local.Common.Count == 1)
      {
        // -- One agency was selected.  Validate the entry and retrieve the 
        // agency description.
        if (!IsEmpty(export.ReferralFips.StateAbbreviation))
        {
          local.Validation.Cdvalue = export.ReferralFips.StateAbbreviation;
          UseCabValidateCodeValue4();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field = GetField(export.ReferralFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }

          // KS is not allowed as a State agency in this situation.
          if (Equal(export.ReferralFips.StateAbbreviation, "KS"))
          {
            var field = GetField(export.ReferralFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "SI0000_OG_TRAN_CANT_BE_SEND_4_KS";
          }

          // -- Find FIPS code for the state abbreviation.
          if (ReadFips2())
          {
            MoveFips(entities.Fips, export.ReferralFips);
          }
          else
          {
            var field = GetField(export.ReferralFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        if (!IsEmpty(export.InterstateRequest.Country))
        {
          local.Validation.Cdvalue = export.InterstateRequest.Country ?? Spaces
            (10);
          UseCabValidateCodeValue2();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field = GetField(export.InterstateRequest, "country");

            field.Error = true;

            ExitState = "LE0000_INVALID_COUNTRY_CODE";
          }

          // US is not allowed as a Foreign agency in this situation.
          if (Equal(export.InterstateRequest.Country, "US"))
          {
            var field = GetField(export.InterstateRequest, "country");

            field.Error = true;

            ExitState = "SI0000_CANT_USE_US_FOR_COUNTRY";
          }
        }

        if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          local.Validation.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
            (10);
          UseCabValidateCodeValue3();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field = GetField(export.InterstateRequest, "tribalAgency");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_TRIBAL_AGENCY";
          }
        }
      }
      else
      {
        // --  Either none or more than one of State, Foreign, and Tribal agency
        // were entered.
        var field1 = GetField(export.ReferralFips, "stateAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Error = true;

        var field3 = GetField(export.InterstateRequest, "tribalAgency");

        field3.Error = true;

        export.Agency.Description = Spaces(CodeValue.Description_MaxLength);
        ExitState = "SI0000_SELECT_STATE_COUNTRY_TRIB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (!Equal(export.ReferralFips.StateAbbreviation,
        export.PreviousReferral.StateAbbreviation) || !
        Equal(export.InterstateRequest.Country,
        export.PreviousInterstateRequest.Country) || !
        Equal(export.InterstateRequest.TribalAgency,
        export.PreviousInterstateRequest.TribalAgency))
      {
        if (Equal(global.Command, "ADD"))
        {
          if (IsEmpty(export.PreviousReferral.StateAbbreviation) && IsEmpty
            (export.PreviousInterstateRequest.Country) && IsEmpty
            (export.PreviousInterstateRequest.TribalAgency))
          {
            goto Test2;
          }
        }

        var field1 = GetField(export.ReferralFips, "stateAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Error = true;

        var field3 = GetField(export.InterstateRequest, "tribalAgency");

        field3.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      // @@@
    }

Test2:

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "IREQ":
        // --------------------------------------------
        // Command is passed in dialog flow
        // --------------------------------------------
        global.Command = "DISPLAY";
        ExitState = "ECO_XFR_TO_IREQ";

        break;
      case "RETCOMP":
        // ** 8-06-2009  cq 11646 added this code to fix  how display works 
        // depending on if you have prompted out to choose
        // another ap.   Anita Hockman
        if (AsChar(export.PromptAp.SelectChar) == 'S')
        {
          export.PromptAp.SelectChar = "*";
        }

        global.Command = "DISPLAY";

        break;
      case "RETCDVL":
        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.PromptState.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue) && !
            Equal(import.SelectedCodeValue.Cdvalue,
            export.ReferralFips.StateAbbreviation))
          {
            // 03/30/2020 GVandy CQ67805 Clear out old values for State, 
            // Country, and Tribal Code.
            MoveInterstateRequest2(local.NullInterstateRequest,
              export.InterstateRequest);
            export.PreviousInterstateRequest.
              Assign(local.NullInterstateRequest);
            MoveFips(local.NullFips, export.ReferralFips);
            export.PreviousReferral.StateAbbreviation =
              local.NullFips.StateAbbreviation;
            export.ReferralFips.StateAbbreviation =
              import.SelectedCodeValue.Cdvalue;
            export.Agency.Description = import.SelectedCodeValue.Description;

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Protected = false;
            field.Focused = true;

            global.Command = "DISPLAY";
          }

          break;
        }

        if (AsChar(export.PromptCountry.SelectChar) == 'S')
        {
          export.PromptCountry.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue) && !
            Equal(import.SelectedCodeValue.Cdvalue,
            export.InterstateRequest.Country))
          {
            // 03/30/2020 GVandy CQ67805 Clear out old values for State, 
            // Country, and Tribal Code.
            MoveInterstateRequest2(local.NullInterstateRequest,
              export.InterstateRequest);
            export.PreviousInterstateRequest.
              Assign(local.NullInterstateRequest);
            MoveFips(local.NullFips, export.ReferralFips);
            export.PreviousReferral.StateAbbreviation =
              local.NullFips.StateAbbreviation;
            export.InterstateRequest.Country = import.SelectedCodeValue.Cdvalue;
            export.Agency.Description = import.SelectedCodeValue.Description;

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Protected = false;
            field.Focused = true;

            global.Command = "DISPLAY";
          }

          break;
        }

        if (AsChar(export.PromptTribalAgency.SelectChar) == 'S')
        {
          export.PromptTribalAgency.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue) && !
            Equal(import.SelectedCodeValue.Cdvalue,
            export.InterstateRequest.TribalAgency))
          {
            // 03/30/2020 GVandy CQ67805 Clear out old values for State, 
            // Country, and Tribal Code.
            MoveInterstateRequest2(local.NullInterstateRequest,
              export.InterstateRequest);
            export.PreviousInterstateRequest.
              Assign(local.NullInterstateRequest);
            MoveFips(local.NullFips, export.ReferralFips);
            export.PreviousReferral.StateAbbreviation =
              local.NullFips.StateAbbreviation;
            export.InterstateRequest.TribalAgency =
              import.SelectedCodeValue.Cdvalue;
            export.Agency.Description = import.SelectedCodeValue.Description;

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Protected = false;
            field.Focused = true;

            global.Command = "DISPLAY";
          }

          break;
        }

        if (AsChar(export.PromptCloseReason.SelectChar) == 'S')
        {
          export.PromptCloseReason.SelectChar = "*";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.InterstateRequest.OtherStateCaseClosureReason =
              import.SelectedCodeValue.Cdvalue;

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.PromptFunction.SelectChar) == 'S')
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
              goto Test3;
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
          }

Test3:

          if (IsEmpty(export.ReferralInterstateRequestHistory.Note))
          {
            export.ReferralInterstateRequestHistory.AttachmentIndicator = "N";
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

        return;
      case "RETLACS":
        for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
          .CourtOrder.Count; ++export.CourtOrder.Index)
        {
          if (IsEmpty(export.CourtOrder.Item.GexportPromptCourtOrder.SelectChar))
            
          {
            continue;
          }

          if (import.SelectedFromLacs.Identifier == 0)
          {
            export.CourtOrder.Update.GexportPromptCourtOrder.SelectChar = "*";
            export.CourtOrder.Update.GexportCourtOrder.Assign(
              local.NullLegalAction);

            continue;
          }

          if (ReadLegalAction())
          {
            if (Lt(entities.LegalAction.EndDate, local.Current.Date))
            {
              var field =
                GetField(export.CourtOrder.Item.GexportPromptCourtOrder,
                "selectChar");

              field.Error = true;

              ExitState = "CO0000_INVALID_EFF_OR_EXP_DATE";

              return;
            }
          }
          else
          {
            var field =
              GetField(export.CourtOrder.Item.GexportPromptCourtOrder,
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

          export.CourtOrder.Update.GexportPromptCourtOrder.SelectChar = "*";
          export.CourtOrder.Update.GexportCourtOrder.Assign(
            import.SelectedFromLacs);

          return;
        }

        break;
      case "ADD":
        if (!Equal(export.ReferralFips.StateAbbreviation,
          export.PreviousReferral.StateAbbreviation) && IsEmpty
          (export.InterstateRequest.Country))
        {
          if (Equal(export.ReferralFips.StateAbbreviation, "KS"))
          {
            var field = GetField(export.ReferralFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "SI0000_OG_TRAN_CANT_BE_SEND_4_KS";

            return;
          }

          if (ReadFips2())
          {
            MoveFips(entities.Fips, export.ReferralFips);
            export.PreviousReferral.StateAbbreviation =
              entities.Fips.StateAbbreviation;
          }
          else
          {
            var field = GetField(export.ReferralFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            return;
          }
        }

        // ---------------------------------------------------------------
        // If we reach this point we know the user has:
        // 1)  Provided a valid case number of an open case
        // 2)  Provided a valid State abbreviation
        // 3)  Performed a DISPLAY
        // If the interstate_request id is populated then we know that
        // Case an AP has outgoing interstate involvement, either
        // active or inactive.
        // If the interstate_request id is not populated we know that Case and 
        // AP has no outgoing
        // interstate involvement for the given State.  We are now only 
        // concerned that
        //  the Case and AP has incoming interstate involvement.
        // ---------------------------------------------------------------
        // mlb - PR00181670 - 07/30/2004 - Changed the read to look for any 
        // interstate request for this case,
        // this person, and this state, that is either incoming or outgoing. 
        // Then if the interstate request
        // is open, another cannot be added. The last statement applies for 
        // either incoming or outgoing. The
        // statements following this new read, were then disabled.
        foreach(var item in ReadInterstateRequest())
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O')
            {
              ExitState = "ACO_NE0000_OPEN_OUT_INTER_REQ_AE";
            }
            else
            {
              ExitState = "SI0000_UPDATE_NOT_ALLOWED_CASE_C";
            }
          }
          else if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O'
            )
          {
            ExitState = "ACO_NE0000_OPEN_IN_INTER_REQ_AE";
          }
        }

        // mlb - PR00181670 - end
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode) ||
          !IsEmpty(export.ReferralInterstateRequestHistory.ActionCode) || !
          IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
        {
          if (IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode))
            
          {
            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else if (!Equal(export.ReferralInterstateRequestHistory.
            FunctionalTypeCode, "ENF") && !
            Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
            "EST") && !
            Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
            "PAT"))
          {
            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Error = true;

            ExitState = "SI0000_ADD_REQS_ENF_EST_OR_PAT";
          }
          else if (!IsEmpty(export.InterstateRequest.OtherStateCaseId))
          {
            // *** cq7200 changed this to allow for other state case number to 
            // be entered on a new ADD for another state.          AH   7-25-
            // 2011
          }

          if (IsEmpty(export.ReferralInterstateRequestHistory.ActionCode))
          {
            var field =
              GetField(export.ReferralInterstateRequestHistory, "actionCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else if (AsChar(export.ReferralInterstateRequestHistory.ActionCode) !=
            'R')
          {
            var field =
              GetField(export.ReferralInterstateRequestHistory, "actionCode");

            field.Error = true;

            ExitState = "SI0000_ADD_REQS_ENF_EST_OR_PAT";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // PR00158330 - MLB ------- Require other state case id if 'ENF', '
          // EST', 'PAT' codes have
          // not been entered. 12/17/2003
          // Rework for  PR00181670 - 08/03/2004 - The exception to the above is
          // that for a manual
          // conversion, all of the fields, 'Function', 'Action', and 'Reason', 
          // must be blank and the
          // 'Other State Case #' cannot be present.
        }
        else
        {
          // PR00158330 - mlb - end
        }

        // PR00216588 - mlb ------ If the action code is an 'R', require a legal
        // action classification of
        // 'J' or 'U' for functional type codes of 'ENF', 'EST', or 'PAT'.
        if (AsChar(export.ReferralInterstateRequestHistory.ActionCode) == 'R')
        {
          if (Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
            "ENF") || Equal
            (export.ReferralInterstateRequestHistory.FunctionalTypeCode, "EST") ||
            Equal
            (export.ReferralInterstateRequestHistory.FunctionalTypeCode, "PAT"))
          {
            local.FunctionCheck.Flag = "N";

            for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
              .CourtOrder.Count; ++export.CourtOrder.Index)
            {
              if (Equal(export.ReferralInterstateRequestHistory.
                FunctionalTypeCode, "ENF") && AsChar
                (export.CourtOrder.Item.GexportCourtOrder.Classification) == 'J'
                )
              {
                local.FunctionCheck.Flag = "Y";
              }
              else if ((Equal(
                export.ReferralInterstateRequestHistory.FunctionalTypeCode,
                "EST") || Equal
                (export.ReferralInterstateRequestHistory.FunctionalTypeCode,
                "PAT")) && AsChar
                (export.CourtOrder.Item.GexportCourtOrder.Classification) == 'U'
                )
              {
                local.FunctionCheck.Flag = "Y";
              }
            }

            if (AsChar(local.FunctionCheck.Flag) == 'N')
            {
              if (Equal(export.ReferralInterstateRequestHistory.
                FunctionalTypeCode, "ENF"))
              {
                ExitState = "SI0000_J_CLASS_USED_FOR_ENF";

                return;
              }
              else if (Equal(export.ReferralInterstateRequestHistory.
                FunctionalTypeCode, "EST") || Equal
                (export.ReferralInterstateRequestHistory.FunctionalTypeCode,
                "PAT"))
              {
                ExitState = "SI0000_U_CLASS_FOR_EST_PAT";

                return;
              }
            }
          }

          // PR00216588 - mlb - end
        }

        // *****  - - - - - - - - - - - - - -  -- - -  - - -  - - - - - -  - - -
        // -  - - - - - -
        // pr 247093      AHockman         fix to force users to create a 
        // transaction
        //                                  
        // on a new outgoing interstate
        // case.
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // - - - - - - - - - -
        if (IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode) ||
          IsEmpty(export.ReferralInterstateRequestHistory.ActionCode) || IsEmpty
          (export.ReferralInterstateRequestHistory.ActionReasonCode))
        {
          var field1 =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field1.Error = true;

          var field2 =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field2.Error = true;

          var field3 =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field3.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // ***   - - - - - - - - - - - - - - - -
        //      end of change for pr 247093
        // - - - - - - - - - - - - - - - - - - - -
        local.Ap.Number = export.Ap.Number;
        export.InterstateRequest.KsCaseInd = "Y";
        export.InterstateRequest.OtherStateFips = export.ReferralFips.State;
        UseSiCabCreateIsRequest();

        if (!IsEmpty(export.PreviousCommon.Command))
        {
          var field1 = GetField(export.PromptAp, "selectChar");

          field1.Protected = true;

          var field2 = GetField(export.Case1, "number");

          field2.Protected = true;

          var field3 = GetField(export.ReferralFips, "stateAbbreviation");

          field3.Protected = true;

          var field4 = GetField(export.PromptState, "selectChar");

          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "country");

          field5.Protected = true;

          var field6 = GetField(export.PromptCountry, "selectChar");

          field6.Protected = true;

          var field7 = GetField(export.InterstateRequest, "tribalAgency");

          field7.Protected = true;

          var field8 = GetField(export.PromptTribalAgency, "selectChar");

          field8.Protected = true;

          var field9 = GetField(export.InterstateRequest, "otherStateCaseId");

          field9.Protected = true;

          var field10 =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field10.Protected = true;

          var field11 =
            GetField(export.InterstateRequest, "otherStateCaseStatus");

          field11.Protected = true;

          var field12 = GetField(export.PromptCloseReason, "selectChar");

          field12.Protected = true;

          var field13 =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field13.Protected = true;

          var field14 = GetField(export.PromptFunction, "selectChar");

          field14.Protected = true;

          var field15 =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field15.Protected = true;

          var field16 =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field16.Protected = true;

          var field17 = GetField(export.PromptReason, "selectChar");

          field17.Protected = true;

          var field18 =
            GetField(export.ReferralInterstateRequestHistory,
            "attachmentIndicator");

          field18.Protected = true;

          var field19 = GetField(export.PromptAttachment, "selectChar");

          field19.Protected = true;

          for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
            .CourtOrder.Count; ++export.CourtOrder.Index)
          {
            var field =
              GetField(export.CourtOrder.Item.GexportPromptCourtOrder,
              "selectChar");

            field.Protected = true;
          }

          for(export.Children.Index = 0; export.Children.Index < export
            .Children.Count; ++export.Children.Index)
          {
            var field =
              GetField(export.Children.Item.GexportSelectChild, "selectChar");

            field.Protected = true;
          }

          var field20 =
            GetField(export.ReferralInterstateRequestHistory, "note");

          field20.Protected = true;

          var field21 = GetField(export.InterstateContact, "nameLast");

          field21.Protected = true;

          var field22 = GetField(export.InterstateContact, "nameFirst");

          field22.Protected = true;

          var field23 = GetField(export.InterstateContact, "nameMiddle");

          field23.Protected = true;

          var field24 =
            GetField(export.InterstateContact, "contactInternetAddress");

          field24.Protected = true;

          var field25 = GetField(export.InterstateContact, "areaCode");

          field25.Protected = true;

          var field26 = GetField(export.InterstateContact, "contactPhoneNum");

          field26.Protected = true;

          var field27 =
            GetField(export.InterstateContact, "contactPhoneExtension");

          field27.Protected = true;

          var field28 = GetField(export.InterstateContactAddress, "street1");

          field28.Protected = true;

          var field29 = GetField(export.InterstateContactAddress, "street2");

          field29.Protected = true;

          var field30 = GetField(export.InterstateContactAddress, "city");

          field30.Protected = true;

          var field31 = GetField(export.InterstateContactAddress, "state");

          field31.Protected = true;

          var field32 = GetField(export.InterstateContactAddress, "zipCode");

          field32.Protected = true;

          var field33 = GetField(export.InterstateContactAddress, "zip4");

          field33.Protected = true;

          var field34 = GetField(export.InterstateContactAddress, "province");

          field34.Protected = true;

          var field35 = GetField(export.InterstateContactAddress, "postalCode");

          field35.Protected = true;

          var field36 = GetField(export.InterstateContactAddress, "country");

          field36.Protected = true;

          // CQ60510
          var field37 =
            GetField(export.InterstateContact, "contactFaxAreaCode");

          field37.Protected = true;

          var field38 = GetField(export.InterstateContact, "contactFaxNumber");

          field38.Protected = true;

          // set exit state to first double confirmation exit state
          ExitState = "ACO_NI0000_CONFIRM_ADD_CHK_EXMP";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.InterstateRequest.OtherStateCaseStatus = "O";
        export.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
        export.PreviousInterstateRequest.Assign(export.InterstateRequest);

        // ***  AHockman 4-22-08   added set below to set transaction direction 
        // ind to 'O' so that the table will show it and the batch job will pick
        // these up for citax transactions.
        local.InterstateRequestHistory.TransactionDirectionInd = "O";
        local.InterstateRequestHistory.ActionReasonCode = "OICNV";
        local.InterstateRequestHistory.CreatedBy = "SWEIOINR";
        local.InterstateRequestHistory.TransactionDate = local.Current.Date;
        UseSiCabCreateIsRequestHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        break;
      case "UPDATE":
        // ---------------------------------------------------------------
        // If we reach this point we know the user has:
        // 1)  Provided a valid case number of an open case
        // 2)  Provided a valid State abbreviation
        // 3)  Performed a DISPLAY
        // If the interstate_request id is populated then we know that
        // Case and AP has outgoing interstate involvement, either
        // active or inactive.
        // If the interstate_request id is not populated we know that Case and 
        // AP has no outgoing interstate involvement for the given State.  No
        // update can be performed.
        // ---------------------------------------------------------------
        if (export.InterstateRequest.IntHGeneratedId == 0)
        {
          ExitState = "CASE_NOT_OG_INTERSTATE";

          return;
        }

        if (!IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode) ||
          !IsEmpty(export.ReferralInterstateRequestHistory.ActionCode) || !
          IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
        {
          if (Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
            "MSC") && AsChar
            (export.ReferralInterstateRequestHistory.ActionCode) == 'P' && Equal
            (export.ReferralInterstateRequestHistory.ActionReasonCode, 1, 3,
            "GSC"))
          {
            if (Equal(export.ReferralInterstateRequestHistory.ActionReasonCode,
              "GSC17"))
            {
              // T. Pierce  CQ24439 -- GSC17 is not equated with a closure code.
            }
            else
            {
              export.ReferralInterstateRequestHistory.FunctionalTypeCode = "";
              export.ReferralInterstateRequestHistory.ActionCode = "";
              export.ReferralInterstateRequestHistory.ActionReasonCode = "";

              var field1 =
                GetField(export.InterstateRequest, "otherStateCaseClosureReason");
                

              field1.Error = true;

              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseStatus");

              field2.Error = true;

              ExitState = "SI0000_MUST_USE_UPDATE_TO_CLOSE";
            }
          }
          else
          {
            if (!IsEmpty(export.ReferralInterstateRequestHistory.
              ActionReasonCode))
            {
              export.ReferralInterstateRequestHistory.ActionReasonCode = "";

              var field =
                GetField(export.ReferralInterstateRequestHistory,
                "actionReasonCode");

              field.Error = true;

              ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
            }

            if (!IsEmpty(export.ReferralInterstateRequestHistory.ActionCode))
            {
              export.ReferralInterstateRequestHistory.ActionCode = "";

              var field =
                GetField(export.ReferralInterstateRequestHistory, "actionCode");
                

              field.Error = true;

              ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
            }

            if (!IsEmpty(export.ReferralInterstateRequestHistory.
              FunctionalTypeCode))
            {
              export.ReferralInterstateRequestHistory.FunctionalTypeCode = "";

              var field =
                GetField(export.ReferralInterstateRequestHistory,
                "functionalTypeCode");

              field.Error = true;

              ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        if (Equal(export.InterstateRequest.OtherStateCaseId,
          export.PreviousInterstateRequest.OtherStateCaseId) && AsChar
          (export.InterstateRequest.OtherStateCaseStatus) == AsChar
          (export.PreviousInterstateRequest.OtherStateCaseStatus) && Equal
          (export.InterstateRequest.OtherStateCaseClosureReason,
          export.PreviousInterstateRequest.OtherStateCaseClosureReason) && Equal
          (export.InterstateContact.NameLast,
          export.PreviousInterstateContact.NameLast) && Equal
          (export.InterstateContact.NameFirst,
          export.PreviousInterstateContact.NameFirst) && AsChar
          (export.InterstateContact.NameMiddle) == AsChar
          (export.PreviousInterstateContact.NameMiddle) && Equal
          (export.InterstateContact.ContactInternetAddress,
          export.PreviousInterstateContact.ContactInternetAddress) && export
          .InterstateContact.AreaCode.GetValueOrDefault() == export
          .PreviousInterstateContact.AreaCode.GetValueOrDefault() && export
          .InterstateContact.ContactPhoneNum.GetValueOrDefault() == export
          .PreviousInterstateContact.ContactPhoneNum.GetValueOrDefault() && Equal
          (export.InterstateContact.ContactPhoneExtension,
          export.PreviousInterstateContact.ContactPhoneExtension) && export
          .InterstateContact.ContactFaxAreaCode.GetValueOrDefault() == export
          .PreviousInterstateContact.ContactFaxAreaCode.GetValueOrDefault() && export
          .InterstateContact.ContactFaxNumber.GetValueOrDefault() == export
          .PreviousInterstateContact.ContactFaxNumber.GetValueOrDefault() && Equal
          (export.InterstateContactAddress.Street1,
          export.PreviousInterstateContactAddress.Street1) && Equal
          (export.InterstateContactAddress.Street2,
          export.PreviousInterstateContactAddress.Street2) && Equal
          (export.InterstateContactAddress.City,
          export.PreviousInterstateContactAddress.City) && Equal
          (export.InterstateContactAddress.ZipCode,
          export.PreviousInterstateContactAddress.ZipCode) && Equal
          (export.InterstateContactAddress.Zip4,
          export.PreviousInterstateContactAddress.Zip4) && Equal
          (export.InterstateContactAddress.State,
          export.PreviousInterstateContactAddress.State) && Equal
          (export.InterstateContactAddress.Province,
          export.PreviousInterstateContactAddress.Province) && Equal
          (export.InterstateContactAddress.PostalCode,
          export.PreviousInterstateContactAddress.PostalCode) && Equal
          (export.InterstateContactAddress.Country,
          export.PreviousInterstateContactAddress.Country))
        {
          ExitState = "FN0000_NO_UPDATES_MADE";

          return;
        }

        if (!Equal(export.InterstateRequest.OtherStateCaseId,
          export.PreviousInterstateRequest.OtherStateCaseId))
        {
          export.FlagOthrStCaseIdChg.Flag = "Y";
        }

        if (IsEmpty(import.PreviousCommon.Command))
        {
          export.PreviousCommon.Command = global.Command;
        }
        else
        {
          export.PreviousCommon.Command = "";
        }

        if (!IsEmpty(export.PreviousCommon.Command))
        {
          var field1 = GetField(export.PromptAp, "selectChar");

          field1.Protected = true;

          var field2 = GetField(export.Case1, "number");

          field2.Protected = true;

          var field3 = GetField(export.ReferralFips, "stateAbbreviation");

          field3.Protected = true;

          var field4 = GetField(export.PromptState, "selectChar");

          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "country");

          field5.Protected = true;

          var field6 = GetField(export.PromptCountry, "selectChar");

          field6.Protected = true;

          var field7 = GetField(export.InterstateRequest, "tribalAgency");

          field7.Protected = true;

          var field8 = GetField(export.PromptTribalAgency, "selectChar");

          field8.Protected = true;

          var field9 = GetField(export.InterstateRequest, "otherStateCaseId");

          field9.Protected = true;

          var field10 =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field10.Protected = true;

          var field11 =
            GetField(export.InterstateRequest, "otherStateCaseStatus");

          field11.Protected = true;

          var field12 = GetField(export.PromptCloseReason, "selectChar");

          field12.Protected = true;

          var field13 =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field13.Protected = true;

          var field14 = GetField(export.PromptFunction, "selectChar");

          field14.Protected = true;

          var field15 =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field15.Protected = true;

          var field16 =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field16.Protected = true;

          var field17 = GetField(export.PromptReason, "selectChar");

          field17.Protected = true;

          var field18 =
            GetField(export.ReferralInterstateRequestHistory,
            "attachmentIndicator");

          field18.Protected = true;

          var field19 = GetField(export.PromptAttachment, "selectChar");

          field19.Protected = true;

          for(export.Children.Index = 0; export.Children.Index < export
            .Children.Count; ++export.Children.Index)
          {
            var field =
              GetField(export.Children.Item.GexportSelectChild, "selectChar");

            field.Protected = true;
          }

          for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
            .CourtOrder.Count; ++export.CourtOrder.Index)
          {
            var field =
              GetField(export.CourtOrder.Item.GexportPromptCourtOrder,
              "selectChar");

            field.Protected = true;
          }

          var field20 =
            GetField(export.ReferralInterstateRequestHistory, "note");

          field20.Protected = true;

          var field21 = GetField(export.InterstateContact, "nameLast");

          field21.Protected = true;

          var field22 = GetField(export.InterstateContact, "nameFirst");

          field22.Protected = true;

          var field23 = GetField(export.InterstateContact, "nameMiddle");

          field23.Protected = true;

          var field24 =
            GetField(export.InterstateContact, "contactInternetAddress");

          field24.Protected = true;

          var field25 = GetField(export.InterstateContact, "areaCode");

          field25.Protected = true;

          var field26 = GetField(export.InterstateContact, "contactPhoneNum");

          field26.Protected = true;

          var field27 =
            GetField(export.InterstateContact, "contactPhoneExtension");

          field27.Protected = true;

          var field28 = GetField(export.InterstateContactAddress, "street1");

          field28.Protected = true;

          var field29 = GetField(export.InterstateContactAddress, "street2");

          field29.Protected = true;

          var field30 = GetField(export.InterstateContactAddress, "city");

          field30.Protected = true;

          var field31 = GetField(export.InterstateContactAddress, "state");

          field31.Protected = true;

          var field32 = GetField(export.InterstateContactAddress, "zipCode");

          field32.Protected = true;

          var field33 = GetField(export.InterstateContactAddress, "zip4");

          field33.Protected = true;

          var field34 = GetField(export.InterstateContactAddress, "province");

          field34.Protected = true;

          var field35 = GetField(export.InterstateContactAddress, "postalCode");

          field35.Protected = true;

          var field36 = GetField(export.InterstateContactAddress, "country");

          field36.Protected = true;

          // CQ60510
          var field37 =
            GetField(export.InterstateContact, "contactFaxAreaCode");

          field37.Protected = true;

          var field38 = GetField(export.InterstateContact, "contactFaxNumber");

          field38.Protected = true;

          // set exit state to first double confirmation exit state
          ExitState = "ACO_NI0000_CONFIRM_UPDT_CHK_EXMP";

          return;
        }

        if (!Equal(export.InterstateRequest.OtherStateCaseId,
          export.PreviousInterstateRequest.OtherStateCaseId) || AsChar
          (export.InterstateRequest.OtherStateCaseStatus) != AsChar
          (export.PreviousInterstateRequest.OtherStateCaseStatus) || !
          Equal(export.InterstateRequest.OtherStateCaseClosureReason,
          export.PreviousInterstateRequest.OtherStateCaseClosureReason))
        {
          if (AsChar(export.InterstateRequest.OtherStateCaseStatus) != AsChar
            (export.PreviousInterstateRequest.OtherStateCaseStatus))
          {
            switch(AsChar(export.InterstateRequest.OtherStateCaseStatus))
            {
              case 'O':
                ExitState = "SI0000_UPDATE_NOT_ALLOWED_CASE_C";

                return;
              case 'C':
                if (IsEmpty(export.InterstateRequest.OtherStateCaseClosureReason))
                  
                {
                  var field1 =
                    GetField(export.InterstateRequest,
                    "otherStateCaseClosureReason");

                  field1.Error = true;

                  ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                  return;
                }

                break;
              default:
                var field =
                  GetField(export.InterstateRequest, "otherStateCaseStatus");

                field.Error = true;

                ExitState = "SI0000_INVALID_CASE_STATUS";

                return;
            }

            local.CodeValue.Cdvalue =
              export.InterstateRequest.OtherStateCaseClosureReason ?? Spaces
              (10);
            UseCabValidateCodeValue6();

            if (AsChar(local.Valid.Flag) != 'Y')
            {
              var field =
                GetField(export.InterstateRequest, "otherStateCaseClosureReason");
                

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

              return;
            }

            if (IsEmpty(import.PreviousCommon.Command))
            {
              goto Test4;
            }

            export.ReferralInterstateRequestHistory.FunctionalTypeCode = "MSC";
            export.ReferralInterstateRequestHistory.ActionCode = "P";

            // -----------------------------------------------------------
            // CQ 24439 2/2011  T. Pierce
            // Added conditional statements for closure codes "IC", "IN",
            // "IS", and "IW".
            // -----------------------------------------------------------
            switch(TrimEnd(export.InterstateRequest.
              OtherStateCaseClosureReason ?? ""))
            {
              case "MJ":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC02";

                break;
              case "CC":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC02";

                break;
              case "DC":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC03";

                break;
              case "EM":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC4A";

                break;
              case "NP":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC4B";

                break;
              case "4D":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC4C";

                break;
              case "NL":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC05";

                break;
              case "AB":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC06";

                break;
              case "FO":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC07";

                break;
              case "LO":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC08";

                break;
              case "AR":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC09";

                break;
              case "GC":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC10";

                break;
              case "LC":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC11";

                break;
              case "FC":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC12";

                break;
              case "IC":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC16";

                break;
              case "IN":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC15";

                break;
              case "IS":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC13";

                break;
              case "IW":
                export.ReferralInterstateRequestHistory.ActionReasonCode =
                  "GSC18";

                break;
              default:
                var field =
                  GetField(export.ReferralInterstateRequestHistory,
                  "actionReasonCode");

                field.Error = true;

                ExitState = "INVALID_VALUE";

                return;
            }

            local.InterstateRequestHistory.ActionReasonCode = "OICLS";
            local.InterstateRequestHistory.CreatedBy = "SWEIOINR";
            local.InterstateRequestHistory.TransactionDate = local.Current.Date;
            export.InterstateRequest.OtherStateCaseClosureDate =
              local.Current.Date;
            UseSiCabCreateIsRequestHistory();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            if (AsChar(export.PreviousInterstateRequest.OtherStateCaseStatus) ==
              'C')
            {
              ExitState = "SI0000_UPDATE_NOT_ALLOWED_CASE_C";

              return;
            }

            if (!IsEmpty(export.InterstateRequest.OtherStateCaseClosureReason))
            {
              var field =
                GetField(export.InterstateRequest, "otherStateCaseClosureReason");
                

              field.Error = true;

              ExitState = "FN0000_FIELD_MUST_BE_SPACES";

              return;
            }
          }

Test4:

          UseSiCabUpdateIsRequest1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        break;
      case "REOPEN":
        // ---------------------------------------------------------------
        // If we reach this point we know the user has:
        // 1)  Provided a valid case number of an open case
        // 2)  Provided a valid State abbreviation
        // 3)  Performed a DISPLAY
        // If the interstate_request id is populated then we know that
        // Case and AP has outgoing interstate involvement, either
        // active or inactive.
        // If the interstate_request id is not populated we know that Case
        // and AP has no outgoing interstate involvement for the given State.
        // No update can be performed.
        // ---------------------------------------------------------------
        if (export.InterstateRequest.IntHGeneratedId == 0)
        {
          ExitState = "CASE_NOT_OG_INTERSTATE";

          return;
        }

        if (AsChar(export.PreviousInterstateRequest.OtherStateCaseStatus) == 'O'
          )
        {
          ExitState = "ACO_NE0000_OPEN_OUT_INTER_REQ_AE";

          return;
        }

        if (!IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode) ||
          !IsEmpty(export.ReferralInterstateRequestHistory.ActionCode) || !
          IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
        {
          if (!IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
            
          {
            export.ReferralInterstateRequestHistory.ActionReasonCode = "";

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "actionReasonCode");

            field.Error = true;

            ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
          }

          if (!IsEmpty(export.ReferralInterstateRequestHistory.ActionCode))
          {
            export.ReferralInterstateRequestHistory.ActionCode = "";

            var field =
              GetField(export.ReferralInterstateRequestHistory, "actionCode");

            field.Error = true;

            ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
          }

          if (!IsEmpty(export.ReferralInterstateRequestHistory.
            FunctionalTypeCode))
          {
            export.ReferralInterstateRequestHistory.FunctionalTypeCode = "";

            var field =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field.Error = true;

            ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        if (IsEmpty(import.PreviousCommon.Command))
        {
          export.PreviousCommon.Command = global.Command;
        }
        else
        {
          export.PreviousCommon.Command = "";
        }

        if (!IsEmpty(export.PreviousCommon.Command))
        {
          var field1 = GetField(export.PromptAp, "selectChar");

          field1.Protected = true;

          var field2 = GetField(export.Case1, "number");

          field2.Protected = true;

          var field3 = GetField(export.ReferralFips, "stateAbbreviation");

          field3.Protected = true;

          var field4 = GetField(export.PromptState, "selectChar");

          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "country");

          field5.Protected = true;

          var field6 = GetField(export.PromptCountry, "selectChar");

          field6.Protected = true;

          var field7 = GetField(export.InterstateRequest, "tribalAgency");

          field7.Protected = true;

          var field8 = GetField(export.PromptTribalAgency, "selectChar");

          field8.Protected = true;

          var field9 = GetField(export.InterstateRequest, "otherStateCaseId");

          field9.Protected = true;

          var field10 =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field10.Protected = true;

          var field11 =
            GetField(export.InterstateRequest, "otherStateCaseStatus");

          field11.Protected = true;

          var field12 = GetField(export.PromptCloseReason, "selectChar");

          field12.Protected = true;

          var field13 =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field13.Protected = true;

          var field14 = GetField(export.PromptFunction, "selectChar");

          field14.Protected = true;

          var field15 =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field15.Protected = true;

          var field16 =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field16.Protected = true;

          var field17 = GetField(export.PromptReason, "selectChar");

          field17.Protected = true;

          var field18 =
            GetField(export.ReferralInterstateRequestHistory,
            "attachmentIndicator");

          field18.Protected = true;

          var field19 = GetField(export.PromptAttachment, "selectChar");

          field19.Protected = true;

          for(export.Children.Index = 0; export.Children.Index < export
            .Children.Count; ++export.Children.Index)
          {
            var field =
              GetField(export.Children.Item.GexportSelectChild, "selectChar");

            field.Protected = true;
          }

          for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
            .CourtOrder.Count; ++export.CourtOrder.Index)
          {
            var field =
              GetField(export.CourtOrder.Item.GexportPromptCourtOrder,
              "selectChar");

            field.Protected = true;
          }

          var field20 =
            GetField(export.ReferralInterstateRequestHistory, "note");

          field20.Protected = true;

          var field21 = GetField(export.InterstateContact, "nameLast");

          field21.Protected = true;

          var field22 = GetField(export.InterstateContact, "nameFirst");

          field22.Protected = true;

          var field23 = GetField(export.InterstateContact, "nameMiddle");

          field23.Protected = true;

          var field24 =
            GetField(export.InterstateContact, "contactInternetAddress");

          field24.Protected = true;

          var field25 = GetField(export.InterstateContact, "areaCode");

          field25.Protected = true;

          var field26 = GetField(export.InterstateContact, "contactPhoneNum");

          field26.Protected = true;

          var field27 =
            GetField(export.InterstateContact, "contactPhoneExtension");

          field27.Protected = true;

          var field28 = GetField(export.InterstateContactAddress, "street1");

          field28.Protected = true;

          var field29 = GetField(export.InterstateContactAddress, "street2");

          field29.Protected = true;

          var field30 = GetField(export.InterstateContactAddress, "city");

          field30.Protected = true;

          var field31 = GetField(export.InterstateContactAddress, "state");

          field31.Protected = true;

          var field32 = GetField(export.InterstateContactAddress, "zipCode");

          field32.Protected = true;

          var field33 = GetField(export.InterstateContactAddress, "zip4");

          field33.Protected = true;

          var field34 = GetField(export.InterstateContactAddress, "province");

          field34.Protected = true;

          var field35 = GetField(export.InterstateContactAddress, "postalCode");

          field35.Protected = true;

          var field36 = GetField(export.InterstateContactAddress, "country");

          field36.Protected = true;

          // CQ60510
          var field37 =
            GetField(export.InterstateContact, "contactFaxAreaCode");

          field37.Protected = true;

          var field38 = GetField(export.InterstateContact, "contactFaxNumber");

          field38.Protected = true;

          // set exit state to first double confirmation exit state
          ExitState = "ACO_NI0000_CONFIRM_REOP_CHK_EMMP";

          return;
        }

        export.InterstateRequest.OtherStateCaseStatus = "O";
        export.InterstateRequest.OtherStateCaseClosureReason = "";
        export.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
        UseSiCabUpdateIsRequest2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.InterstateRequestHistory.ActionReasonCode = "OICRO";
        local.InterstateRequestHistory.CreatedBy = "SWEIOINR";
        local.InterstateRequestHistory.TransactionDate = local.Current.Date;
        UseSiCabCreateIsRequestHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        export.ReferralInterstateRequestHistory.FunctionalTypeCode = "MSC";
        export.ReferralInterstateRequestHistory.ActionCode = "P";
        export.ReferralInterstateRequestHistory.ActionReasonCode = "GSSTA";
        export.ReferralInterstateRequestHistory.Note =
          "Interstate Case reopened between KS and " + export
          .ReferralFips.StateAbbreviation;

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
          export.Code.CodeName = local.LiteralState.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptCountry.SelectChar) == 'S')
        {
          export.Code.CodeName = local.LiteralCountry.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptTribalAgency.SelectChar) == 'S')
        {
          export.Code.CodeName = local.LiteralTribalAgency.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptCloseReason.SelectChar) == 'S')
        {
          export.Code.CodeName = local.LiteralCsenetCaseClosure.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptFunction.SelectChar) == 'S')
        {
          export.Code.CodeName = "INTERSTATE TRANS TYPE";

          if (!IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
            
          {
            export.CombinationCode.CodeName =
              local.LiteralCsenetOgCaseReason.CodeName;
            export.CombinationCodeValue.Cdvalue =
              export.ReferralInterstateRequestHistory.ActionReasonCode ?? Spaces
              (10);
          }

          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptReason.SelectChar) == 'S')
        {
          export.Code.CodeName = local.LiteralCsenetOgCaseReason.CodeName;

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
          export.Code.CodeName = "INTERSTATE ATTACHMENTS";
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
        // ---------------------------------------------------------------
        // If we reach this point we know the user has:
        // 1)  Provided a valid case number of an open case
        // 2)  Provided a valid State abbreviation
        // 3)  Performed a DISPLAY
        // If the interstate_request id is populated then we know that
        // Case and AP has outgoing interstate involvement, either
        // active or inactive.
        // If the interstate_request id is not populated we know that Case and 
        // AP has no outgoing interstate involvement for the given State.  No
        // update can be performed.
        // ---------------------------------------------------------------
        if (export.InterstateRequest.IntHGeneratedId == 0)
        {
          ExitState = "CASE_NOT_OG_INTERSTATE";

          return;
        }

        if (AsChar(export.PreviousInterstateRequest.OtherStateCaseStatus) != 'O'
          )
        {
          ExitState = "SI0000_NO_CSENET_OUT_CLOSED_CASE";

          return;
        }

        // ***  01/13/2008   cq526  copied code here from the add so this will 
        // error the fields if any of the
        // three are not filled in on a send.     AHockman
        if (IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode) ||
          IsEmpty(export.ReferralInterstateRequestHistory.ActionCode) || IsEmpty
          (export.ReferralInterstateRequestHistory.ActionReasonCode))
        {
          var field1 =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field1.Error = true;

          var field2 =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field2.Error = true;

          var field3 =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field3.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // ***  11/28/07   Anita Hockman    cq 526  deactivated the code below 
        // related to pr 158330 now that we have made changes
        // on OINR to force the worker to send a transaction when opening a new 
        // case if the other state is  csenet ready
        //  we want them to be able to send one using enf, est or pat and also 
        // add the case number for the other state.
        //    We don't want to require them to enter it, we just want to allow 
        // them to  if they have it.   The required
        //  field missing error was confusing when in fact they had entered the 
        // case number and just were not being a
        // llowed to do so on an ADD.   Once they had added their record they  
        // could go back and add the other state case number with no problem,
        // but there is no reason to make them go through all that.    Fixed to
        // highlight the field they did not enter in case they didn't enter all
        // three.
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

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------------------------------------------
    // Common logic for maintaining contact info
    // ----------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "REOPEN") || Equal(global.Command, "SEND"))
    {
      // ------------------------------------------
      // Contact information validations
      // ------------------------------------------
      if (export.InterstateContact.AreaCode.GetValueOrDefault() == 0 && export
        .InterstateContact.ContactPhoneNum.GetValueOrDefault() == 0)
      {
        if (!IsEmpty(export.InterstateContact.ContactPhoneExtension))
        {
          var field1 = GetField(export.InterstateContact, "areaCode");

          field1.Error = true;

          var field2 = GetField(export.InterstateContact, "contactPhoneNum");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          UseEabRollbackCics();

          return;
        }
      }
      else
      {
        if (export.InterstateContact.ContactPhoneNum.GetValueOrDefault() == 0)
        {
          var field = GetField(export.InterstateContact, "contactPhoneNum");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (export.InterstateContact.AreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.InterstateContact, "areaCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
      }

      // CQ60510
      if (export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault() > 0
        || export.InterstateContact.ContactFaxNumber.GetValueOrDefault() > 0)
      {
        if (export.InterstateContact.ContactFaxNumber.GetValueOrDefault() == 0)
        {
          var field = GetField(export.InterstateContact, "contactFaxNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault() == 0
          )
        {
          var field = GetField(export.InterstateContact, "contactFaxAreaCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
      }

      // ------------------------------------------
      // Contact address validations
      // ------------------------------------------
      // @@@
      if (!IsEmpty(export.InterstateContactAddress.Street1) || !
        IsEmpty(export.InterstateContactAddress.Street2) || !
        IsEmpty(export.InterstateContactAddress.City) || !
        IsEmpty(export.InterstateContactAddress.State) || !
        IsEmpty(export.InterstateContactAddress.ZipCode) || !
        IsEmpty(export.InterstateContactAddress.Zip4) || !
        IsEmpty(export.InterstateContactAddress.Province) || !
        IsEmpty(export.InterstateContactAddress.Country) || !
        IsEmpty(export.InterstateContactAddress.PostalCode))
      {
        if (IsEmpty(export.InterstateContactAddress.Street1))
        {
          var field = GetField(export.InterstateContactAddress, "street1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.InterstateContactAddress.City))
        {
          var field = GetField(export.InterstateContactAddress, "city");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if ((!IsEmpty(export.InterstateContactAddress.State) || !
          IsEmpty(export.InterstateContactAddress.ZipCode) || !
          IsEmpty(export.InterstateContactAddress.Zip4)) && (
            !IsEmpty(export.InterstateContactAddress.PostalCode) || !
          IsEmpty(export.InterstateContactAddress.Province) || !
          IsEmpty(export.InterstateContactAddress.Country)))
        {
          var field1 = GetField(export.InterstateContactAddress, "state");

          field1.Error = true;

          var field2 = GetField(export.InterstateContactAddress, "zipCode");

          field2.Error = true;

          var field3 = GetField(export.InterstateContactAddress, "zip4");

          field3.Error = true;

          var field4 = GetField(export.InterstateContactAddress, "postalCode");

          field4.Error = true;

          var field5 = GetField(export.InterstateContactAddress, "province");

          field5.Error = true;

          var field6 = GetField(export.InterstateContactAddress, "country");

          field6.Error = true;

          ExitState = "SI0000_FOREIGN_OR_DOMESTIC_ADDR";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        if (IsEmpty(export.InterstateContactAddress.City) && IsEmpty
          (export.InterstateContactAddress.Province))
        {
          var field1 = GetField(export.InterstateContactAddress, "city");

          field1.Error = true;

          var field2 = GetField(export.InterstateContactAddress, "province");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.InterstateContactAddress.State) && IsEmpty
          (export.InterstateContactAddress.Country))
        {
          var field1 = GetField(export.InterstateContactAddress, "state");

          field1.Error = true;

          var field2 = GetField(export.InterstateContactAddress, "country");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.InterstateContactAddress.ZipCode) && IsEmpty
          (export.InterstateContactAddress.PostalCode))
        {
          var field1 = GetField(export.InterstateContactAddress, "zipCode");

          field1.Error = true;

          var field2 = GetField(export.InterstateContactAddress, "postalCode");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        if (!IsEmpty(export.InterstateContactAddress.Street1) || !
          IsEmpty(export.InterstateContactAddress.City) || !
          IsEmpty(export.InterstateContactAddress.State) || !
          IsEmpty(export.InterstateContactAddress.Country))
        {
          if (!IsEmpty(export.InterstateContactAddress.State))
          {
            local.Validation.Cdvalue =
              export.InterstateContactAddress.State ?? Spaces(10);
            UseCabValidateCodeValue5();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.InterstateContactAddress, "state");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }

            if (IsEmpty(export.InterstateContactAddress.ZipCode))
            {
              var field = GetField(export.InterstateContactAddress, "zipCode");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }

          if (!IsEmpty(export.InterstateContactAddress.Country))
          {
            local.Validation.Cdvalue =
              export.InterstateContactAddress.Country ?? Spaces(10);
            UseCabValidateCodeValue1();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Error = true;

              ExitState = "LE0000_INVALID_COUNTRY_CODE";
            }

            if (IsEmpty(export.InterstateContactAddress.PostalCode))
            {
              var field =
                GetField(export.InterstateContactAddress, "postalCode");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        if (!IsEmpty(export.InterstateContactAddress.Zip4))
        {
          if (Length(TrimEnd(export.InterstateContactAddress.Zip4)) < 4)
          {
            var field = GetField(export.InterstateContactAddress, "zip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";
          }

          if (Verify(export.InterstateContactAddress.Zip4, "0123456789") > 0)
          {
            var field = GetField(export.InterstateContactAddress, "zip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";
          }
        }

        if (Length(TrimEnd(export.InterstateContactAddress.ZipCode)) < 5 && !
          IsEmpty(export.InterstateContactAddress.ZipCode))
        {
          var field = GetField(export.InterstateContactAddress, "zipCode");

          field.Error = true;

          ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";
        }

        if (Verify(export.InterstateContactAddress.ZipCode, "0123456789") > 0
          && !IsEmpty(export.InterstateContactAddress.ZipCode))
        {
          var field = GetField(export.InterstateContactAddress, "zipCode");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        // -- Give a confirmation warning message if the address state/country 
        // does not match the IV-D agency location.
        if (IsEmpty(import.AddressMismatch.Flag))
        {
          if (!IsEmpty(export.ReferralFips.StateAbbreviation))
          {
            if (!Equal(export.ReferralFips.StateAbbreviation,
              export.InterstateContactAddress.State) && !
              IsEmpty(export.InterstateContactAddress.State))
            {
              var field1 = GetField(export.ReferralFips, "stateAbbreviation");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.Country))
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.InterstateRequest.Country))
          {
            if (!Equal(export.InterstateRequest.Country,
              export.InterstateContactAddress.Country) && !
              IsEmpty(export.InterstateContactAddress.Country))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "country");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.State))
            {
              var field = GetField(export.InterstateContactAddress, "state");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.TribalAgency.State =
              Substring(export.Agency.Description, 1, 2);

            if (!Equal(local.TribalAgency.State,
              export.InterstateContactAddress.State) && !
              IsEmpty(export.InterstateContactAddress.State))
            {
              var field1 = GetField(export.InterstateRequest, "tribalAgency");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.Country))
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }
        }

        if (AsChar(export.AddressMismatch.Flag) == 'Y')
        {
          if (Equal(global.Command, "ADD"))
          {
            ExitState = "SI0000_IVD_ADDRESS_MISMATCH_ADD";
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            ExitState = "SI0000_IVD_ADDRESS_MISMATCH_UPDT";
          }

          UseEabRollbackCics();

          return;
        }
      }

      if (Equal(global.Command, "ADD"))
      {
        if (!IsEmpty(export.InterstateContactAddress.Street1) || !
          IsEmpty(export.InterstateContactAddress.Street2) || !
          IsEmpty(export.InterstateContactAddress.City) || !
          IsEmpty(export.InterstateContactAddress.ZipCode) || !
          IsEmpty(export.InterstateContactAddress.Zip4))
        {
          UseSiCabCreateIsContact();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          UseSiCabCreateIsContactAddress();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }
        else if (!IsEmpty(export.InterstateContact.NameFirst) || !
          IsEmpty(export.InterstateContact.NameLast) || !
          IsEmpty(export.InterstateContact.NameMiddle) || !
          IsEmpty(export.InterstateContact.ContactInternetAddress) || export
          .InterstateContact.AreaCode.GetValueOrDefault() > 0 || export
          .InterstateContact.ContactPhoneNum.GetValueOrDefault() > 0 || !
          IsEmpty(export.InterstateContact.ContactPhoneExtension) || export
          .InterstateContact.ContactFaxAreaCode.GetValueOrDefault() > 0 || export
          .InterstateContact.ContactFaxNumber.GetValueOrDefault() > 0)
        {
          UseSiCabCreateIsContact();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          UseSiCabDeleteIsContactAddress2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          export.InterstateContactAddress.Assign(
            local.NullInterstateContactAddress);
          export.PreviousInterstateContactAddress.Assign(
            local.NullInterstateContactAddress);
        }
        else
        {
          UseSiCabDeleteIsContact();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          MoveInterstateContact2(local.NullInterstateContact,
            export.InterstateContact);
          MoveInterstateContact4(local.NullInterstateContact,
            export.PreviousInterstateContact);
          export.InterstateContactAddress.Assign(
            local.NullInterstateContactAddress);
          export.PreviousInterstateContactAddress.Assign(
            local.NullInterstateContactAddress);
        }
      }
      else
      {
        if (IsEmpty(export.InterstateContactAddress.Street1) && IsEmpty
          (export.InterstateContactAddress.Street2) && IsEmpty
          (export.InterstateContactAddress.City) && IsEmpty
          (export.InterstateContactAddress.ZipCode) && IsEmpty
          (export.InterstateContactAddress.Zip4) && IsEmpty
          (export.InterstateContactAddress.Province) && IsEmpty
          (export.InterstateContactAddress.PostalCode) && IsEmpty
          (export.InterstateContactAddress.Country))
        {
          if (IsEmpty(export.InterstateContact.NameLast) && IsEmpty
            (export.InterstateContact.NameFirst) && IsEmpty
            (export.InterstateContact.NameMiddle) && IsEmpty
            (export.InterstateContact.ContactInternetAddress) && export
            .InterstateContact.AreaCode.GetValueOrDefault() == 0 && export
            .InterstateContact.ContactPhoneNum.GetValueOrDefault() == 0 && IsEmpty
            (export.InterstateContact.ContactPhoneExtension) && export
            .InterstateContact.ContactFaxAreaCode.GetValueOrDefault() == 0 && export
            .InterstateContact.ContactFaxNumber.GetValueOrDefault() == 0)
          {
            UseSiCabDeleteIsContact();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }

            MoveInterstateContact2(local.NullInterstateContact,
              export.InterstateContact);
            MoveInterstateContact4(local.NullInterstateContact,
              export.PreviousInterstateContact);
            export.InterstateContactAddress.Assign(
              local.NullInterstateContactAddress);
            export.PreviousInterstateContactAddress.Assign(
              local.NullInterstateContactAddress);

            goto Test5;
          }
          else
          {
            UseSiCabDeleteIsContactAddress1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }

            export.InterstateContactAddress.Assign(
              local.NullInterstateContactAddress);
            export.PreviousInterstateContactAddress.Assign(
              local.NullInterstateContactAddress);
          }
        }

        if (!Equal(export.InterstateContact.NameLast,
          export.PreviousInterstateContact.NameLast) || !
          Equal(export.InterstateContact.NameFirst,
          export.PreviousInterstateContact.NameFirst) || AsChar
          (export.InterstateContact.NameMiddle) != AsChar
          (export.PreviousInterstateContact.NameMiddle) || !
          Equal(export.InterstateContact.ContactInternetAddress,
          export.PreviousInterstateContact.ContactInternetAddress) || export
          .InterstateContact.AreaCode.GetValueOrDefault() != export
          .PreviousInterstateContact.AreaCode.GetValueOrDefault() || export
          .InterstateContact.ContactPhoneNum.GetValueOrDefault() != export
          .PreviousInterstateContact.ContactPhoneNum.GetValueOrDefault() || !
          Equal(export.InterstateContact.ContactPhoneExtension,
          export.PreviousInterstateContact.ContactPhoneExtension) || export
          .InterstateContact.ContactFaxAreaCode.GetValueOrDefault() != export
          .PreviousInterstateContact.ContactFaxAreaCode.GetValueOrDefault() || export
          .InterstateContact.ContactFaxNumber.GetValueOrDefault() != export
          .PreviousInterstateContact.ContactFaxNumber.GetValueOrDefault())
        {
          UseSiCabCreateIsContact();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          export.PreviousInterstateContact.Assign(export.InterstateContact);
        }

        if (!Equal(export.InterstateContactAddress.Street1,
          export.PreviousInterstateContactAddress.Street1) || !
          Equal(export.InterstateContactAddress.Street2,
          export.PreviousInterstateContactAddress.Street2) || !
          Equal(export.InterstateContactAddress.City,
          export.PreviousInterstateContactAddress.City) || !
          Equal(export.InterstateContactAddress.ZipCode,
          export.PreviousInterstateContactAddress.ZipCode) || !
          Equal(export.InterstateContactAddress.Zip4,
          export.PreviousInterstateContactAddress.Zip4) || !
          Equal(export.InterstateContactAddress.Province,
          export.PreviousInterstateContactAddress.Province) || !
          Equal(export.InterstateContactAddress.PostalCode,
          export.PreviousInterstateContactAddress.PostalCode) || !
          Equal(export.InterstateContactAddress.Country,
          export.PreviousInterstateContactAddress.Country))
        {
          if (Equal(export.InterstateContact.StartDate,
            local.NullDateWorkArea.Date))
          {
            UseSiCabCreateIsContact();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }

            export.PreviousInterstateContact.Assign(export.InterstateContact);
          }

          UseSiCabCreateIsContactAddress();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          export.PreviousInterstateContactAddress.Assign(
            export.InterstateContactAddress);
        }
      }
    }

Test5:

    // ----------------------------------------------------
    // Common logic for sending transactions
    // ----------------------------------------------------
    // @@@
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "REOPEN") || Equal(global.Command, "SEND"))
    {
      if (Equal(global.Command, "ADD"))
      {
        if (IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode) &&
          IsEmpty(export.ReferralInterstateRequestHistory.ActionCode))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD_NEXT_E";

          goto Test6;
        }

        if (IsEmpty(export.ReferralFips.StateAbbreviation))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD_NEXT_E";

          goto Test6;
        }
      }

      if (Equal(global.Command, "UPDATE"))
      {
        if (IsEmpty(export.ReferralInterstateRequestHistory.FunctionalTypeCode) &&
          IsEmpty(export.ReferralInterstateRequestHistory.ActionCode))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE_NEX";

          goto Test6;
        }

        if (IsEmpty(export.ReferralFips.StateAbbreviation))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE_NEX";

          goto Test6;
        }
      }

      if (Equal(global.Command, "SEND"))
      {
        if (IsEmpty(export.ReferralFips.StateAbbreviation))
        {
          UseEabRollbackCics();
          ExitState = "SI0000_NOT_CSENET_ENABLED";

          goto Test6;
        }
      }

      if (Equal(global.Command, "REOPEN"))
      {
        if (IsEmpty(export.ReferralFips.StateAbbreviation))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_REOPEN_NEX";

          goto Test6;
        }
      }

      if (Equal(global.Command, "SEND"))
      {
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

        // *** 01/03/08   A Hockman    cq526  Added this IF to check for action 
        // reason code blank on a send, the other two types were already here.
        if (IsEmpty(export.ReferralInterstateRequestHistory.ActionReasonCode))
        {
          var field =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (Equal(global.Command, "SEND"))
      {
        if (IsEmpty(export.ReferralFips.StateAbbreviation))
        {
          UseEabRollbackCics();
          ExitState = "SI0000_NOT_CSENET_ENABLED";

          return;
        }
      }

      if (Equal(global.Command, "REOPEN"))
      {
        if (IsEmpty(export.ReferralFips.StateAbbreviation))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_REOPEN_NEX";

          return;
        }
      }

      if (IsEmpty(export.ReferralInterstateRequestHistory.ActionCode))
      {
        var field =
          GetField(export.ReferralInterstateRequestHistory, "actionCode");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabRollbackCics();

        return;
      }

      switch(AsChar(export.ReferralInterstateRequestHistory.AttachmentIndicator))
        
      {
        case 'Y':
          if (IsEmpty(export.ReferralInterstateRequestHistory.Note))
          {
            var field1 =
              GetField(export.ReferralInterstateRequestHistory, "note");

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
        UseEabRollbackCics();

        return;
      }

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

      for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
        .CourtOrder.Count; ++export.CourtOrder.Index)
      {
        if (export.CourtOrder.Item.GexportCourtOrder.Identifier > 0)
        {
          ++local.LegalActions.Index;
          local.LegalActions.CheckSize();

          local.LegalActions.Update.G.Identifier =
            export.CourtOrder.Item.GexportCourtOrder.Identifier;
        }
      }

      // *** IF STATEMENT ADDED BELOW TO ASSURE THAT WORKER HAS MADE A CONSCIOUS
      // CHOICE TO PICK BLANK, HOWEVER
      // THE OUTGOING CODE FOR CSENET NEEDS TO BE SPACES SO BEFORE THE 
      // TRANSACTION IS SENT WE CONVERT THE WORD
      // BLANK TO SPACES.    ANITA H.  7/11/08
      if (Equal(export.ReferralInterstateRequestHistory.ActionReasonCode,
        "BLANK"))
      {
        export.ReferralInterstateRequestHistory.ActionReasonCode = "";
      }

      local.Ap.Number = export.Ap.Number;
      local.Automatic.Flag = "N";
      export.ReferralInterstateRequestHistory.CreatedBy = "SWEIOINR";
      UseSiCreateCsenetTrans();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CO0000_STATE_NOT_CSENET_READY") || IsExitState
          ("CO0000_CSENET_NOT_SENT_TO_STATE"))
        {
          // *** CQ#527 Changes Begin Here ***
          if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O')
          {
            var field =
              GetField(export.InterstateRequest, "otherStateCaseStatus");

            field.Color = "green";
            field.Protected = false;
          }
          else
          {
            var field =
              GetField(export.InterstateRequest, "otherStateCaseStatus");

            field.Color = "cyan";
            field.Protected = true;
          }

          switch(TrimEnd(global.Command))
          {
            case "ADD":
              ExitState = "ACO_NI0000_ADD_SUCCESS_NO_CSENET";

              break;
            case "UPDATE":
              ExitState = "ACO_NI0000_CLOS_SUCCESS_NO_CSNET";

              break;
            case "REOPEN":
              ExitState = "ACO_NI0000_REOPN_SUCCES_NO_CSNET";

              break;
            default:
              // SEND
              // no need to change message for a send keep as it is
              break;
          }

          // *** CQ#527 Changes End   Here ***
          return;
        }

        if (IsExitState("CASE_NF"))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;
        }
        else if (IsExitState("SP0000_INVALID_FUNCTION"))
        {
          var field =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field.Error = true;
        }
        else if (IsExitState("ACO_NE0000_INVALID_ACTION"))
        {
          var field =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field.Error = true;
        }
        else if (IsExitState("SI0000_INVALID_OG_CASE_REASON"))
        {
          var field =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field.Error = true;
        }
        else if (IsExitState("SI0000_REASON_INVALID_WITH_FUNCT"))
        {
          var field1 =
            GetField(export.ReferralInterstateRequestHistory, "actionReasonCode");
            

          field1.Error = true;

          var field2 =
            GetField(export.ReferralInterstateRequestHistory, "actionCode");

          field2.Error = true;

          var field3 =
            GetField(export.ReferralInterstateRequestHistory,
            "functionalTypeCode");

          field3.Error = true;
        }
        else
        {
          var field = GetField(export.ReferralFips, "stateAbbreviation");

          field.Error = true;
        }

        UseEabRollbackCics();

        return;

        // *** CQ#527 Changes Begin Here ***
        if (Equal(global.Command, "UPDATE") || Equal(global.Command, "REOPEN"))
        {
          if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O')
          {
            var field =
              GetField(export.InterstateRequest, "otherStateCaseStatus");

            field.Color = "green";
            field.Protected = false;
          }
          else
          {
            var field =
              GetField(export.InterstateRequest, "otherStateCaseStatus");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        // *** CQ#527 Changes End   Here ***
        switch(TrimEnd(global.Command))
        {
          case "ADD":
            var field1 =
              GetField(export.ReferralInterstateRequestHistory,
              "actionReasonCode");

            field1.Error = true;

            var field2 =
              GetField(export.ReferralInterstateRequestHistory, "actionCode");

            field2.Error = true;

            var field3 =
              GetField(export.ReferralInterstateRequestHistory,
              "functionalTypeCode");

            field3.Error = true;

            UseEabRollbackCics();

            return;
          case "UPDATE":
            export.ReferralInterstateRequestHistory.FunctionalTypeCode = "";
            export.ReferralInterstateRequestHistory.ActionCode = "";
            export.ReferralInterstateRequestHistory.ActionReasonCode = "";
            ExitState = "SI0000_CSENET_CLOSED_SEND_MANUAL";

            return;
          case "REOPEN":
            export.ReferralInterstateRequestHistory.FunctionalTypeCode = "";
            export.ReferralInterstateRequestHistory.ActionCode = "";
            export.ReferralInterstateRequestHistory.ActionReasonCode = "";
            export.ReferralInterstateRequestHistory.Note =
              Spaces(InterstateRequestHistory.Note_MaxLength);
            ExitState = "SI0000_CSENET_REOPEN_SEND_MANUAL";

            return;
          default:
            // SEND
            var field4 = GetField(export.ReferralFips, "stateAbbreviation");

            field4.Error = true;

            return;
        }
      }

      local.Ap.Number = export.Ap.Number;
      local.Infrastructure.EventId = 13;
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.CsenetInOutCode = "O";
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.Infrastructure.Function =
        export.ReferralInterstateRequestHistory.FunctionalTypeCode;
      local.Infrastructure.ReasonCode =
        export.ReferralInterstateRequestHistory.ActionReasonCode ?? Spaces(15);
      local.Infrastructure.DenormNumeric12 =
        local.InterstateCase.TransSerialNumber;
      local.Infrastructure.DenormDate = local.InterstateCase.TransactionDate;

      if (Equal(export.ReferralInterstateRequestHistory.FunctionalTypeCode,
        "MSC") && AsChar
        (export.ReferralInterstateRequestHistory.ActionCode) == 'P' && Equal
        (export.ReferralInterstateRequestHistory.ActionReasonCode, 1, 3, "GSC"))
      {
        if (Equal(export.ReferralInterstateRequestHistory.ActionReasonCode,
          "GSC17"))
        {
          // T. Pierce CQ24439 -- GSC17 is not equated with a closure code.
          local.Infrastructure.Detail =
            "Outgoing Interstate Request to the State of " + TrimEnd
            (export.ReferralFips.StateAbbreviation) + " informing close IWO.";
        }
        else
        {
          local.Infrastructure.Detail =
            "Outgoing Interstate Request to the State of " + TrimEnd
            (export.ReferralFips.StateAbbreviation) + "was closed.";
        }
      }

      UseOeCabRaiseEvent();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      MoveInterstateRequestHistory1(local.NullInterstateRequestHistory,
        export.ReferralInterstateRequestHistory);

      switch(TrimEnd(global.Command))
      {
        case "ADD":
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD_NEXT_E";

          break;
        case "UPDATE":
          ExitState = "ACO_NI0000_SUCCESSFUL_CLOSED_NEX";

          break;
        case "REOPEN":
          ExitState = "ACO_NI0000_SUCCESSFUL_REOPEN_NEX";

          break;
        default:
          // SEND
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          break;
      }
    }

Test6:

    // *** CQ#527 Changes Begin Here ***
    if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O')
    {
      var field = GetField(export.InterstateRequest, "otherStateCaseStatus");

      field.Color = "green";
      field.Protected = false;
    }
    else
    {
      var field = GetField(export.InterstateRequest, "otherStateCaseStatus");

      field.Color = "cyan";
      field.Protected = true;
    }

    // *** CQ#527 Changes End   Here ***
    if (Equal(global.Command, "DISPLAY"))
    {
      // -- Check for a case number passed in.
      if (!IsEmpty(import.SelectedCase.Number) && !
        Equal(import.SelectedCase.Number, export.Case1.Number))
      {
        export.Case1.Number = import.SelectedCase.Number;
      }

      // *** CQ#527 Changes Begin Here ***
      // *** If F2 is pressed with some changes on the screen it should refresh 
      // the data ***
      if (!Equal(export.Case1.Number, import.SelectedCase.Number))
      {
        if (!IsEmpty(export.Case1.Number) && !
          IsEmpty(export.PreviousCase.Number))
        {
          export.PreviousCase.Number = "";
        }
      }

      // *** CQ#527 Changes End   Here ***
      if (!Equal(export.Case1.Number, export.PreviousCase.Number))
      {
        // -- New case number has been entered/selected.  Blank out any previous
        // info displayed on the screen.
        MoveCsePersonsWorkSet(local.NullCsePersonsWorkSet, export.Ap);
        MoveCsePersonsWorkSet(local.NullCsePersonsWorkSet, export.Ar);

        if (!Equal(export.ReferralFips.StateAbbreviation,
          export.PreviousReferral.StateAbbreviation) || !
          Equal(export.InterstateRequest.Country,
          export.PreviousInterstateRequest.Country) || !
          Equal(export.InterstateRequest.TribalAgency,
          export.PreviousInterstateRequest.TribalAgency))
        {
          // --  Blank out all fields in the interstate_request except Country 
          // and Tribal_Agency.
          MoveInterstateRequest5(export.InterstateRequest, local.TempSave);
          MoveInterstateRequest2(local.NullInterstateRequest,
            export.InterstateRequest);
          MoveInterstateRequest5(local.TempSave, export.InterstateRequest);
        }
        else
        {
          // --  Blank out all fields in the interstate_request and 
          // export_referral fips views.
          MoveInterstateRequest2(local.NullInterstateRequest,
            export.InterstateRequest);
          export.PreviousInterstateRequest.Assign(local.NullInterstateRequest);
          export.PreviousReferral.StateAbbreviation =
            local.NullFips.StateAbbreviation;
          MoveFips(local.NullFips, export.ReferralFips);
        }

        MoveInterstateContact2(local.NullInterstateContact,
          export.InterstateContact);
        MoveInterstateContact4(local.NullInterstateContact,
          export.PreviousInterstateContact);
        export.InterstateContactAddress.Assign(
          local.NullInterstateContactAddress);
        export.PreviousInterstateContactAddress.Assign(
          local.NullInterstateContactAddress);
        export.Agency.Description = Spaces(CodeValue.Description_MaxLength);
        export.HeaderLine.Text35 = "";
        export.CaseOpen.Flag = "";

        for(export.Children.Index = 0; export.Children.Index < export
          .Children.Count; ++export.Children.Index)
        {
          export.Children.Update.GexportSelectChild.SelectChar = "";
          export.Children.Update.GexportChild.
            Assign(local.NullCsePersonsWorkSet);
        }

        MoveCase1(local.NullCase, export.PreviousCase);
      }

      MoveInterstateRequestHistory1(local.NullInterstateRequestHistory,
        export.ReferralInterstateRequestHistory);

      for(export.CourtOrder.Index = 0; export.CourtOrder.Index < export
        .CourtOrder.Count; ++export.CourtOrder.Index)
      {
        export.CourtOrder.Update.GexportPromptCourtOrder.SelectChar = "";
        export.CourtOrder.Update.GexportCourtOrder.
          Assign(local.NullLegalAction);
      }

      // -- Check for selected IV-D agency from IREQ.
      if (!IsEmpty(import.SelectedFips.StateAbbreviation) || !
        IsEmpty(import.SelectedInterstateRequest.Country) || !
        IsEmpty(import.SelectedInterstateRequest.TribalAgency))
      {
        MoveFips(import.SelectedFips, export.ReferralFips);
        MoveInterstateRequest5(import.SelectedInterstateRequest,
          export.InterstateRequest);
      }

      if (!Equal(export.ReferralFips.StateAbbreviation,
        export.PreviousReferral.StateAbbreviation) || !
        Equal(export.InterstateRequest.Country,
        export.PreviousInterstateRequest.Country) || !
        Equal(export.InterstateRequest.TribalAgency,
        export.PreviousInterstateRequest.TribalAgency))
      {
        // -- New IV-D agency has been entered/selected.  Blank out any previous
        // info displayed on the screen.
        MoveInterstateRequest5(export.InterstateRequest, local.TempSave);
        MoveInterstateRequest2(local.NullInterstateRequest,
          export.InterstateRequest);
        MoveInterstateRequest5(local.TempSave, export.InterstateRequest);
        export.PreviousInterstateRequest.Assign(local.NullInterstateRequest);
        MoveInterstateRequestHistory1(local.NullInterstateRequestHistory,
          export.ReferralInterstateRequestHistory);
        MoveInterstateContact2(local.NullInterstateContact,
          export.InterstateContact);
        MoveInterstateContact4(local.NullInterstateContact,
          export.PreviousInterstateContact);
        export.InterstateContactAddress.Assign(
          local.NullInterstateContactAddress);
        export.PreviousInterstateContactAddress.Assign(
          local.NullInterstateContactAddress);
        export.Agency.Description = Spaces(CodeValue.Description_MaxLength);
      }

      // -- Case number is required.
      if (IsEmpty(export.Case1.Number))
      {
        export.PreviousReferral.StateAbbreviation =
          local.NullFips.StateAbbreviation;
        MoveFips(local.NullFips, export.ReferralFips);
        MoveInterstateRequest2(local.NullInterstateRequest,
          export.InterstateRequest);

        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      // -- Validate IV-D agency if entered.
      local.Common.Count = 0;

      if (IsEmpty(export.ReferralFips.StateAbbreviation))
      {
        export.ReferralFips.State = 0;
      }
      else
      {
        ++local.Common.Count;
      }

      if (!IsEmpty(export.InterstateRequest.Country))
      {
        ++local.Common.Count;
      }

      if (!IsEmpty(export.InterstateRequest.TribalAgency))
      {
        ++local.Common.Count;
      }

      switch(local.Common.Count)
      {
        case 0:
          // -- No IV-D agency was selected.  Continue.
          break;
        case 1:
          // -- One agency was selected.  Validate the entry and retrieve the 
          // agency description.
          if (!IsEmpty(export.ReferralFips.StateAbbreviation))
          {
            local.Validation.Cdvalue = export.ReferralFips.StateAbbreviation;
            UseCabValidateCodeValue4();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.ReferralFips, "stateAbbreviation");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }

            // KS is not allowed as a State agency in this situation.
            if (Equal(export.ReferralFips.StateAbbreviation, "KS"))
            {
              var field = GetField(export.ReferralFips, "stateAbbreviation");

              field.Error = true;

              ExitState = "SI0000_OG_TRAN_CANT_BE_SEND_4_KS";
            }

            // -- Find FIPS code for the state abbreviation.
            if (ReadFips2())
            {
              MoveFips(entities.Fips, export.ReferralFips);
              export.PreviousReferral.StateAbbreviation =
                entities.Fips.StateAbbreviation;
            }
            else
            {
              var field = GetField(export.ReferralFips, "stateAbbreviation");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }

          if (!IsEmpty(export.InterstateRequest.Country))
          {
            local.Validation.Cdvalue = export.InterstateRequest.Country ?? Spaces
              (10);
            UseCabValidateCodeValue2();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.InterstateRequest, "country");

              field.Error = true;

              ExitState = "LE0000_INVALID_COUNTRY_CODE";
            }

            // US is not allowed as a Foreign agency in this situation.
            if (Equal(export.InterstateRequest.Country, "US"))
            {
              var field = GetField(export.InterstateRequest, "country");

              field.Error = true;

              ExitState = "SI0000_CANT_USE_US_FOR_COUNTRY";
            }
          }

          if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.Validation.Cdvalue =
              export.InterstateRequest.TribalAgency ?? Spaces(10);
            UseCabValidateCodeValue3();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.InterstateRequest, "tribalAgency");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_TRIBAL_AGENCY";
            }
          }

          break;
        default:
          // -- More than one agency was selected.
          if (!IsEmpty(export.ReferralFips.StateAbbreviation))
          {
            var field = GetField(export.ReferralFips, "stateAbbreviation");

            field.Error = true;
          }

          if (!IsEmpty(export.InterstateRequest.Country))
          {
            var field = GetField(export.InterstateRequest, "country");

            field.Error = true;
          }

          if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            var field = GetField(export.InterstateRequest, "tribalAgency");

            field.Error = true;
          }

          ExitState = "SI0000_SELECT_STATE_COUNTRY_TRIB";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ----------------------------------------------------------
      // Find the Case details
      // ----------------------------------------------------------
      // -- If the user has just returned from IREQ this will be populated
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

      // ***   cq 11646 changes to allow a flow to comp to select an ap on 
      // multiple ap cases  AH
      if (AsChar(local.MultipleAps.Flag) == 'Y' && IsEmpty
        (import.SelectedCsePersonsWorkSet.Number))
      {
        if (!IsEmpty(import.PreviousCommon.Command) && !
          Equal(global.Command, import.PreviousCommon.Command))
        {
          export.PreviousCommon.Command = "";
          ExitState = "SAME_PF_KEY_WAS_NOT_PRESSED";

          // exit state message - "same pf key was not pressed, changes not 
          // saved"
          return;
        }

        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        return;
      }

      // *** copied this IF statement section up here from the display below.  
      // This program was originally written to only display the ap if there is
      // an outgoing case, but in cq11646 they want to display any ap they
      // choose on comp and be able to add an outgoing  case.  This has only
      // been a problem when there are multiple active ap's.   AH
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number) && !
        Equal(import.SelectedCsePersonsWorkSet.Number, export.Ap.Number))
      {
        export.Ap.Number = import.SelectedCsePersonsWorkSet.Number;
        export.Ap.FormattedName =
          import.SelectedCsePersonsWorkSet.FormattedName;
        MoveFips(local.NullFips, export.ReferralFips);
      }

      // *** end of 11646 changes   AH
      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (!IsEmpty(import.PreviousCommon.Command) && !
          Equal(global.Command, import.PreviousCommon.Command))
        {
          export.PreviousCommon.Command = "";
          ExitState = "SAME_PF_KEY_WAS_NOT_PRESSED";

          // exit state message - "same pf key was not pressed, changes not 
          // saved"
          return;
        }

        return;
      }

      export.Children.Index = 0;
      export.Children.Clear();

      foreach(var item in ReadCsePerson())
      {
        export.Children.Update.GexportChild.Number = entities.CsePerson.Number;
        UseSiReadCsePerson2();

        if (!IsEmpty(local.AbendData.Type1))
        {
          var field = GetField(export.Children.Item.GexportChild, "firstName");

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

      export.PreviousCase.Number = export.Case1.Number;

      // ----------------------------------------------------------
      // Find an appropriate interstate request
      // The Case number is the only thing we are guaranteed to
      // know.  The AP and IV-D agency may not be entered, in which case
      // we attempt to determine for which AP and IV-D agency the Case
      // has an interstate request.
      // In the event of multiple APs with outgoing interstate
      // involvement flow to COMP.
      // In the event of multiple IV-D agencies with outgoing interstate
      // involvement for a single AP flow to IREQ.
      // ----------------------------------------------------------
      foreach(var item in ReadInterstateRequestCsePerson3())
      {
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
              if (!IsEmpty(import.PreviousCommon.Command) && !
                Equal(global.Command, import.PreviousCommon.Command))
              {
                export.PreviousCommon.Command = "";
                ExitState = "SAME_PF_KEY_WAS_NOT_PRESSED";

                // exit state message - "same pf key was not pressed, changes 
                // not saved"
                return;
              }

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
          if (Equal(entities.InterstateRequest.Country,
            export.InterstateRequest.Country) && entities
            .InterstateRequest.OtherStateFips == export
            .InterstateRequest.OtherStateFips && Equal
            (entities.InterstateRequest.TribalAgency,
            export.InterstateRequest.TribalAgency))
          {
            continue;
          }

          if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) != AsChar
            (export.InterstateRequest.OtherStateCaseStatus))
          {
            break;
          }

          MoveInterstateRequest2(local.NullInterstateRequest,
            export.InterstateRequest);
          export.PreviousInterstateRequest.Assign(local.NullInterstateRequest);

          if (!IsEmpty(import.PreviousCommon.Command) && !
            Equal(global.Command, import.PreviousCommon.Command))
          {
            export.PreviousCommon.Command = "";
            ExitState = "SAME_PF_KEY_WAS_NOT_PRESSED";

            // exit state message - "same pf key was not pressed, changes not 
            // saved"
            return;
          }

          global.Command = "LNK_OINR";
          ExitState = "ECO_XFR_TO_IREQ";

          return;
        }

        export.InterstateRequest.Assign(entities.InterstateRequest);
        MoveInterstateRequest4(entities.InterstateRequest,
          export.PreviousInterstateRequest);
      }

      if (export.InterstateRequest.IntHGeneratedId == 0)
      {
        if (ReadInterstateRequestCsePerson2())
        {
          // -- The case is an outgoing case but not to the AP/agency combo 
          // specified.
          ExitState = "AP_NOT_INVOLVED_IN_INTERSTATE";
        }
        else if (ReadInterstateRequestCsePerson1())
        {
          // -- The case is not an outgoing interstate case (i.e. it is an 
          // incoming interstate case).
          ExitState = "CASE_NOT_OG_INTERSTATE";
        }
        else
        {
          // -- The case has no interstate involvement.
          ExitState = "CASE_NOT_INTERSTATE";
        }
      }
      else if (ReadInterstateContact())
      {
        MoveInterstateContact3(entities.InterstateContact,
          export.InterstateContact);
        export.PreviousInterstateContact.Assign(entities.InterstateContact);

        if (ReadInterstateContactAddress())
        {
          export.InterstateContactAddress.Assign(
            entities.InterstateContactAddress);
          export.PreviousInterstateContactAddress.Assign(
            entities.InterstateContactAddress);
        }
      }

      // ----------------------------------------------------------
      // We didn't find an appropriate interstate request
      // ----------------------------------------------------------
      if (export.InterstateRequest.IntHGeneratedId == 0)
      {
        if (IsExitState("CASE_NOT_INTERSTATE"))
        {
          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }

          // mlb - PR00166872 - 01/07/2004 - This will allow the selection
          // of a state without causing the problem of not being able to
          // finish adding the interstate request.
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

            UseSiFormatCsePersonName();
          }
        }

        // End
        if (!IsEmpty(import.PreviousCommon.Command) && !
          Equal(global.Command, import.PreviousCommon.Command))
        {
          export.PreviousCommon.Command = "";
          ExitState = "SAME_PF_KEY_WAS_NOT_PRESSED";

          // exit state message - "same pf key was not pressed, changes not 
          // saved"
          return;
        }

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
      if (IsEmpty(export.ReferralFips.StateAbbreviation) && export
        .InterstateRequest.OtherStateFips > 0)
      {
        if (ReadFips1())
        {
          MoveFips(entities.Fips, export.ReferralFips);
          export.PreviousReferral.StateAbbreviation =
            entities.Fips.StateAbbreviation;
        }
        else
        {
          var field = GetField(export.ReferralFips, "stateAbbreviation");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_CODE";

          if (!IsEmpty(import.PreviousCommon.Command) && !
            Equal(global.Command, import.PreviousCommon.Command))
          {
            export.PreviousCommon.Command = "";
            ExitState = "SAME_PF_KEY_WAS_NOT_PRESSED";

            // exit state message - "same pf key was not pressed, changes not 
            // saved"
          }

          return;
        }
      }
      else
      {
        export.PreviousReferral.StateAbbreviation =
          export.ReferralFips.StateAbbreviation;
      }

      if (!IsEmpty(export.ReferralFips.StateAbbreviation))
      {
        local.IvdAgency.CodeName = local.LiteralState.CodeName;
        local.CodeValue.Cdvalue = export.ReferralFips.StateAbbreviation;
      }
      else if (!IsEmpty(export.InterstateRequest.Country))
      {
        local.IvdAgency.CodeName = local.LiteralCountry.CodeName;
        local.CodeValue.Cdvalue = export.InterstateRequest.Country ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.InterstateRequest.TribalAgency))
      {
        local.IvdAgency.CodeName = local.LiteralTribalAgency.CodeName;
        local.CodeValue.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
          (10);
      }

      UseCabGetCodeValueDescription();

      if (ReadInterstateContact())
      {
        MoveInterstateContact3(entities.InterstateContact,
          export.InterstateContact);
        export.PreviousInterstateContact.Assign(entities.InterstateContact);

        if (ReadInterstateContactAddress())
        {
          export.InterstateContactAddress.Assign(
            entities.InterstateContactAddress);
          export.PreviousInterstateContactAddress.Assign(
            entities.InterstateContactAddress);
        }
      }

      // mlb - PR00138968 - 01/07/2004 - Allow an 'O' status to be changed to '
      // C'; protect all others.
      if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O')
      {
        var field = GetField(export.InterstateRequest, "otherStateCaseStatus");

        field.Color = "green";
        field.Protected = false;

        // mlb - PR00138968 - end
      }
      else
      {
        // *** CQ#527 Changes Begin Here ***
        // *** Added the Else part       ***
        var field = GetField(export.InterstateRequest, "otherStateCaseStatus");

        field.Color = "cyan";
        field.Protected = true;

        // *** CQ#527 Changes End Here   ***
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

      if (!IsEmpty(import.PreviousCommon.Command) && !
        Equal(global.Command, import.PreviousCommon.Command))
      {
        export.PreviousCommon.Command = "";
        ExitState = "SAME_PF_KEY_WAS_NOT_PRESSED";

        // exit state message - "same pf key was not pressed, changes not saved"
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

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
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

  private static void MoveInterstateContact1(InterstateContact source,
    InterstateContact target)
  {
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.StartDate = source.StartDate;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.AreaCode = source.AreaCode;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
  }

  private static void MoveInterstateContact2(InterstateContact source,
    InterstateContact target)
  {
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.StartDate = source.StartDate;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.AreaCode = source.AreaCode;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactInternetAddress = source.ContactInternetAddress;
  }

  private static void MoveInterstateContact3(InterstateContact source,
    InterstateContact target)
  {
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.StartDate = source.StartDate;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.AreaCode = source.AreaCode;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
  }

  private static void MoveInterstateContact4(InterstateContact source,
    InterstateContact target)
  {
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.StartDate = source.StartDate;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.AreaCode = source.AreaCode;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactInternetAddress = source.ContactInternetAddress;
  }

  private static void MoveInterstateContactAddress(
    InterstateContactAddress source, InterstateContactAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.StartDate = source.StartDate;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.KsCaseInd = source.KsCaseInd;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.KsCaseInd = source.KsCaseInd;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest3(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.KsCaseInd = source.KsCaseInd;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest4(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest5(InterstateRequest source,
    InterstateRequest target)
  {
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequestHistory1(
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

  private static void MoveInterstateRequestHistory2(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
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

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.IvdAgency.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.LiteralCountry.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.LiteralCountry.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.LiteralTribalAgency.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue4()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.LiteralState.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue5()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.LiteralState.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue6()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.LiteralCsenetCaseClosure.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Valid.Flag = useExport.ValidCode.Flag;
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

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    useImport.CsePerson.Number = local.Ap.Number;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(OeCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.LiteralCountry.CodeName = useExport.Country.CodeName;
    local.LiteralTribalAgency.CodeName = useExport.TribalAgency.CodeName;
    local.LiteralCsenetCaseClosure.CodeName =
      useExport.CsenetCaseClosure.CodeName;
    local.LiteralState.CodeName = useExport.State.CodeName;
    local.LiteralCsenetActionType.CodeName =
      useExport.CsenetActionType.CodeName;
    local.LiteralCsenetOgCaseReason.CodeName =
      useExport.CsenetOgCaseReason.CodeName;
    local.LiteralCsenetFunctionalType.CodeName =
      useExport.CsenetFunctionalType.CodeName;
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

    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);
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

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCabCreateIsContact()
  {
    var useImport = new SiCabCreateIsContact.Import();
    var useExport = new SiCabCreateIsContact.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;
    MoveInterstateContact1(export.InterstateContact, useImport.InterstateContact);
      

    Call(SiCabCreateIsContact.Execute, useImport, useExport);

    export.InterstateContact.StartDate = useExport.InterstateContact.StartDate;
  }

  private void UseSiCabCreateIsContactAddress()
  {
    var useImport = new SiCabCreateIsContactAddress.Import();
    var useExport = new SiCabCreateIsContactAddress.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;
    useImport.InterstateContact.StartDate = export.InterstateContact.StartDate;
    MoveInterstateContactAddress(export.InterstateContactAddress,
      useImport.InterstateContactAddress);

    Call(SiCabCreateIsContactAddress.Execute, useImport, useExport);

    export.InterstateContactAddress.StartDate =
      useExport.InterstateContactAddress.StartDate;
  }

  private void UseSiCabCreateIsRequest()
  {
    var useImport = new SiCabCreateIsRequest.Import();
    var useExport = new SiCabCreateIsRequest.Export();

    useImport.Ap.Number = local.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;
    MoveInterstateRequest3(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.Previous.Command = import.PreviousCommon.Command;

    Call(SiCabCreateIsRequest.Execute, useImport, useExport);

    export.InterstateRequest.IntHGeneratedId =
      useExport.InterstateRequest.IntHGeneratedId;
    export.PreviousCommon.Command = useExport.Previous.Command;
  }

  private void UseSiCabCreateIsRequestHistory()
  {
    var useImport = new SiCabCreateIsRequestHistory.Import();
    var useExport = new SiCabCreateIsRequestHistory.Export();

    useImport.InterstateRequestHistory.Assign(local.InterstateRequestHistory);
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(SiCabCreateIsRequestHistory.Execute, useImport, useExport);
  }

  private void UseSiCabDeleteIsContact()
  {
    var useImport = new SiCabDeleteIsContact.Import();
    var useExport = new SiCabDeleteIsContact.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(SiCabDeleteIsContact.Execute, useImport, useExport);
  }

  private void UseSiCabDeleteIsContactAddress1()
  {
    var useImport = new SiCabDeleteIsContactAddress.Import();
    var useExport = new SiCabDeleteIsContactAddress.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(SiCabDeleteIsContactAddress.Execute, useImport, useExport);
  }

  private void UseSiCabDeleteIsContactAddress2()
  {
    var useImport = new SiCabDeleteIsContactAddress.Import();
    var useExport = new SiCabDeleteIsContactAddress.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;
    useImport.InterstateContact.StartDate = export.InterstateContact.StartDate;

    Call(SiCabDeleteIsContactAddress.Execute, useImport, useExport);
  }

  private void UseSiCabUpdateIsRequest1()
  {
    var useImport = new SiCabUpdateIsRequest.Import();
    var useExport = new SiCabUpdateIsRequest.Export();

    useImport.OtherStateCaseIdChg.Flag = export.FlagOthrStCaseIdChg.Flag;
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      

    Call(SiCabUpdateIsRequest.Execute, useImport, useExport);
  }

  private void UseSiCabUpdateIsRequest2()
  {
    var useImport = new SiCabUpdateIsRequest.Import();
    var useExport = new SiCabUpdateIsRequest.Export();

    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      

    Call(SiCabUpdateIsRequest.Execute, useImport, useExport);
  }

  private void UseSiCheckCourtCaseForReferral()
  {
    var useImport = new SiCheckCourtCaseForReferral.Import();
    var useExport = new SiCheckCourtCaseForReferral.Export();

    useImport.LegalAction.Assign(import.SelectedFromLacs);
    useImport.Child.Number = export.Children.Item.GexportChild.Number;

    Call(SiCheckCourtCaseForReferral.Execute, useImport, useExport);
  }

  private void UseSiCreateCsenetTrans()
  {
    var useImport = new SiCreateCsenetTrans.Import();
    var useExport = new SiCreateCsenetTrans.Export();

    useImport.Ap.Number = local.Ap.Number;
    useImport.AutomaticTrans.Flag = local.Automatic.Flag;
    local.Children.CopyTo(useImport.Children, MoveChildren);
    local.LegalActions.CopyTo(useImport.LegalActions, MoveLegalActions);
    useImport.Case1.Number = export.Case1.Number;
    useImport.InterstateRequest.Assign(export.InterstateRequest);
    useImport.InterstateRequestHistory.Assign(
      export.ReferralInterstateRequestHistory);

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

    MoveCsePersonsWorkSet(useExport.Ap, export.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
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

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", export.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.ContactPhoneNum =
          db.GetNullableInt32(reader, 2);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 3);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 4);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 5);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 6);
        entities.InterstateContact.AreaCode = db.GetNullableInt32(reader, 7);
        entities.InterstateContact.ContactPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.InterstateContact.ContactFaxNumber =
          db.GetNullableInt32(reader, 9);
        entities.InterstateContact.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.InterstateContact.ContactInternetAddress =
          db.GetNullableString(reader, 11);
        entities.InterstateContact.Populated = true;
      });
  }

  private bool ReadInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);
    entities.InterstateContactAddress.Populated = false;

    return Read("ReadInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 5);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 7);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 9);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 11);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 13);
        entities.InterstateContactAddress.Populated = true;
        CheckValid<InterstateContactAddress>("LocationType",
          entities.InterstateContactAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.ReferralFips.State);
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
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
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestCsePerson1",
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
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.CsePerson.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequestCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestCsePerson2",
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
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.CsePerson.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCsePerson3()
  {
    entities.CsePerson.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestCsePerson3",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.ReferralFips.State);
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
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
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.CsePerson.Populated = true;
        entities.InterstateRequest.Populated = true;
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
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
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

    /// <summary>A CourtOrderGroup group.</summary>
    [Serializable]
    public class CourtOrderGroup
    {
      /// <summary>
      /// A value of GimportCourtOrder.
      /// </summary>
      [JsonPropertyName("gimportCourtOrder")]
      public LegalAction GimportCourtOrder
      {
        get => gimportCourtOrder ??= new();
        set => gimportCourtOrder = value;
      }

      /// <summary>
      /// A value of GimportPromptCourtOrder.
      /// </summary>
      [JsonPropertyName("gimportPromptCourtOrder")]
      public Common GimportPromptCourtOrder
      {
        get => gimportPromptCourtOrder ??= new();
        set => gimportPromptCourtOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private LegalAction gimportCourtOrder;
      private Common gimportPromptCourtOrder;
    }

    /// <summary>
    /// A value of DoubleConfiirmation.
    /// </summary>
    [JsonPropertyName("doubleConfiirmation")]
    public Common DoubleConfiirmation
    {
      get => doubleConfiirmation ??= new();
      set => doubleConfiirmation = value;
    }

    /// <summary>
    /// A value of PreviousCommon.
    /// </summary>
    [JsonPropertyName("previousCommon")]
    public Common PreviousCommon
    {
      get => previousCommon ??= new();
      set => previousCommon = value;
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
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of PromptTribalAgency.
    /// </summary>
    [JsonPropertyName("promptTribalAgency")]
    public Common PromptTribalAgency
    {
      get => promptTribalAgency ??= new();
      set => promptTribalAgency = value;
    }

    /// <summary>
    /// A value of PromptCountry.
    /// </summary>
    [JsonPropertyName("promptCountry")]
    public Common PromptCountry
    {
      get => promptCountry ??= new();
      set => promptCountry = value;
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
    /// A value of PreviousCase.
    /// </summary>
    [JsonPropertyName("previousCase")]
    public Case1 PreviousCase
    {
      get => previousCase ??= new();
      set => previousCase = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of PreviousInterstateRequest.
    /// </summary>
    [JsonPropertyName("previousInterstateRequest")]
    public InterstateRequest PreviousInterstateRequest
    {
      get => previousInterstateRequest ??= new();
      set => previousInterstateRequest = value;
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
    /// Gets a value of CourtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<CourtOrderGroup> CourtOrder => courtOrder ??= new(
      CourtOrderGroup.Capacity);

    /// <summary>
    /// Gets a value of CourtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    [Computed]
    public IList<CourtOrderGroup> CourtOrder_Json
    {
      get => courtOrder;
      set => CourtOrder.Assign(value);
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of PreviousInterstateContact.
    /// </summary>
    [JsonPropertyName("previousInterstateContact")]
    public InterstateContact PreviousInterstateContact
    {
      get => previousInterstateContact ??= new();
      set => previousInterstateContact = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of PreviousInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("previousInterstateContactAddress")]
    public InterstateContactAddress PreviousInterstateContactAddress
    {
      get => previousInterstateContactAddress ??= new();
      set => previousInterstateContactAddress = value;
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
    /// A value of PromptCloseReason.
    /// </summary>
    [JsonPropertyName("promptCloseReason")]
    public Common PromptCloseReason
    {
      get => promptCloseReason ??= new();
      set => promptCloseReason = value;
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
    /// A value of PromptAttachment.
    /// </summary>
    [JsonPropertyName("promptAttachment")]
    public Common PromptAttachment
    {
      get => promptAttachment ??= new();
      set => promptAttachment = value;
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
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
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
    /// A value of ApFromIreq.
    /// </summary>
    [JsonPropertyName("apFromIreq")]
    public CsePersonsWorkSet ApFromIreq
    {
      get => apFromIreq ??= new();
      set => apFromIreq = value;
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
    /// A value of AddressMismatch.
    /// </summary>
    [JsonPropertyName("addressMismatch")]
    public Common AddressMismatch
    {
      get => addressMismatch ??= new();
      set => addressMismatch = value;
    }

    private Common doubleConfiirmation;
    private Common previousCommon;
    private InterstateRequest selectedInterstateRequest;
    private CodeValue agency;
    private Common promptTribalAgency;
    private Common promptCountry;
    private Case1 case1;
    private Case1 previousCase;
    private WorkArea headerLine;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Common caseOpen;
    private Fips referralFips;
    private Fips previousReferral;
    private InterstateRequest interstateRequest;
    private InterstateRequest previousInterstateRequest;
    private InterstateRequestHistory referralInterstateRequestHistory;
    private Array<ChildrenGroup> children;
    private Array<CourtOrderGroup> courtOrder;
    private InterstateContact interstateContact;
    private InterstateContact previousInterstateContact;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContactAddress previousInterstateContactAddress;
    private Common promptAp;
    private Common promptState;
    private Common promptCloseReason;
    private Common promptFunctionCd;
    private Common promptReason;
    private Common promptAttachment;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private CodeValue selectedCodeValue;
    private Case1 selectedCase;
    private Fips selectedFips;
    private LegalAction selectedFromLacs;
    private CsePersonsWorkSet apFromIreq;
    private NextTranInfo hidden;
    private Standard standard;
    private Common addressMismatch;
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

    /// <summary>A CourtOrderGroup group.</summary>
    [Serializable]
    public class CourtOrderGroup
    {
      /// <summary>
      /// A value of GexportCourtOrder.
      /// </summary>
      [JsonPropertyName("gexportCourtOrder")]
      public LegalAction GexportCourtOrder
      {
        get => gexportCourtOrder ??= new();
        set => gexportCourtOrder = value;
      }

      /// <summary>
      /// A value of GexportPromptCourtOrder.
      /// </summary>
      [JsonPropertyName("gexportPromptCourtOrder")]
      public Common GexportPromptCourtOrder
      {
        get => gexportPromptCourtOrder ??= new();
        set => gexportPromptCourtOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private LegalAction gexportCourtOrder;
      private Common gexportPromptCourtOrder;
    }

    /// <summary>
    /// A value of DoubleConfirmation.
    /// </summary>
    [JsonPropertyName("doubleConfirmation")]
    public Common DoubleConfirmation
    {
      get => doubleConfirmation ??= new();
      set => doubleConfirmation = value;
    }

    /// <summary>
    /// A value of PreviousCommon.
    /// </summary>
    [JsonPropertyName("previousCommon")]
    public Common PreviousCommon
    {
      get => previousCommon ??= new();
      set => previousCommon = value;
    }

    /// <summary>
    /// A value of FlagOthrStCaseIdChg.
    /// </summary>
    [JsonPropertyName("flagOthrStCaseIdChg")]
    public Common FlagOthrStCaseIdChg
    {
      get => flagOthrStCaseIdChg ??= new();
      set => flagOthrStCaseIdChg = value;
    }

    /// <summary>
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of PromptTribalAgency.
    /// </summary>
    [JsonPropertyName("promptTribalAgency")]
    public Common PromptTribalAgency
    {
      get => promptTribalAgency ??= new();
      set => promptTribalAgency = value;
    }

    /// <summary>
    /// A value of PromptCountry.
    /// </summary>
    [JsonPropertyName("promptCountry")]
    public Common PromptCountry
    {
      get => promptCountry ??= new();
      set => promptCountry = value;
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
    /// A value of PreviousCase.
    /// </summary>
    [JsonPropertyName("previousCase")]
    public Case1 PreviousCase
    {
      get => previousCase ??= new();
      set => previousCase = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of PreviousInterstateRequest.
    /// </summary>
    [JsonPropertyName("previousInterstateRequest")]
    public InterstateRequest PreviousInterstateRequest
    {
      get => previousInterstateRequest ??= new();
      set => previousInterstateRequest = value;
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
    /// Gets a value of CourtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<CourtOrderGroup> CourtOrder => courtOrder ??= new(
      CourtOrderGroup.Capacity);

    /// <summary>
    /// Gets a value of CourtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    [Computed]
    public IList<CourtOrderGroup> CourtOrder_Json
    {
      get => courtOrder;
      set => CourtOrder.Assign(value);
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of PreviousInterstateContact.
    /// </summary>
    [JsonPropertyName("previousInterstateContact")]
    public InterstateContact PreviousInterstateContact
    {
      get => previousInterstateContact ??= new();
      set => previousInterstateContact = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of PreviousInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("previousInterstateContactAddress")]
    public InterstateContactAddress PreviousInterstateContactAddress
    {
      get => previousInterstateContactAddress ??= new();
      set => previousInterstateContactAddress = value;
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
    /// A value of PromptCloseReason.
    /// </summary>
    [JsonPropertyName("promptCloseReason")]
    public Common PromptCloseReason
    {
      get => promptCloseReason ??= new();
      set => promptCloseReason = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of CombinationCode.
    /// </summary>
    [JsonPropertyName("combinationCode")]
    public Code CombinationCode
    {
      get => combinationCode ??= new();
      set => combinationCode = value;
    }

    /// <summary>
    /// A value of AddressMismatch.
    /// </summary>
    [JsonPropertyName("addressMismatch")]
    public Common AddressMismatch
    {
      get => addressMismatch ??= new();
      set => addressMismatch = value;
    }

    private Common doubleConfirmation;
    private Common previousCommon;
    private Common flagOthrStCaseIdChg;
    private CodeValue agency;
    private Common promptTribalAgency;
    private Common promptCountry;
    private Case1 case1;
    private Case1 previousCase;
    private WorkArea headerLine;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Common caseOpen;
    private Fips referralFips;
    private Fips previousReferral;
    private InterstateRequest interstateRequest;
    private InterstateRequest previousInterstateRequest;
    private InterstateRequestHistory referralInterstateRequestHistory;
    private Array<ChildrenGroup> children;
    private Array<CourtOrderGroup> courtOrder;
    private InterstateContact interstateContact;
    private InterstateContact previousInterstateContact;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContactAddress previousInterstateContactAddress;
    private Common promptAp;
    private Common promptState;
    private Common promptCloseReason;
    private Common promptFunction;
    private Common promptReason;
    private Common promptAttachment;
    private NextTranInfo hidden;
    private Standard standard;
    private Code code;
    private CodeValue combinationCodeValue;
    private Code combinationCode;
    private Common addressMismatch;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of IvdAgency.
    /// </summary>
    [JsonPropertyName("ivdAgency")]
    public Code IvdAgency
    {
      get => ivdAgency ??= new();
      set => ivdAgency = value;
    }

    /// <summary>
    /// A value of TribalAgency.
    /// </summary>
    [JsonPropertyName("tribalAgency")]
    public Common TribalAgency
    {
      get => tribalAgency ??= new();
      set => tribalAgency = value;
    }

    /// <summary>
    /// A value of TempSave.
    /// </summary>
    [JsonPropertyName("tempSave")]
    public InterstateRequest TempSave
    {
      get => tempSave ??= new();
      set => tempSave = value;
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
    /// A value of LiteralTribalAgency.
    /// </summary>
    [JsonPropertyName("literalTribalAgency")]
    public Code LiteralTribalAgency
    {
      get => literalTribalAgency ??= new();
      set => literalTribalAgency = value;
    }

    /// <summary>
    /// A value of FunctionCheck.
    /// </summary>
    [JsonPropertyName("functionCheck")]
    public Common FunctionCheck
    {
      get => functionCheck ??= new();
      set => functionCheck = value;
    }

    /// <summary>
    /// A value of LiteralCountry.
    /// </summary>
    [JsonPropertyName("literalCountry")]
    public Code LiteralCountry
    {
      get => literalCountry ??= new();
      set => literalCountry = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public Common Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of Valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public Common Valid
    {
      get => valid ??= new();
      set => valid = value;
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
    /// A value of NullCase.
    /// </summary>
    [JsonPropertyName("nullCase")]
    public Case1 NullCase
    {
      get => nullCase ??= new();
      set => nullCase = value;
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
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
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
    /// A value of NullInterstateContact.
    /// </summary>
    [JsonPropertyName("nullInterstateContact")]
    public InterstateContact NullInterstateContact
    {
      get => nullInterstateContact ??= new();
      set => nullInterstateContact = value;
    }

    /// <summary>
    /// A value of NullInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("nullInterstateContactAddress")]
    public InterstateContactAddress NullInterstateContactAddress
    {
      get => nullInterstateContactAddress ??= new();
      set => nullInterstateContactAddress = value;
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
    /// A value of LiteralCsenetCaseClosure.
    /// </summary>
    [JsonPropertyName("literalCsenetCaseClosure")]
    public Code LiteralCsenetCaseClosure
    {
      get => literalCsenetCaseClosure ??= new();
      set => literalCsenetCaseClosure = value;
    }

    /// <summary>
    /// A value of LiteralState.
    /// </summary>
    [JsonPropertyName("literalState")]
    public Code LiteralState
    {
      get => literalState ??= new();
      set => literalState = value;
    }

    /// <summary>
    /// A value of LiteralCsenetActionType.
    /// </summary>
    [JsonPropertyName("literalCsenetActionType")]
    public Code LiteralCsenetActionType
    {
      get => literalCsenetActionType ??= new();
      set => literalCsenetActionType = value;
    }

    /// <summary>
    /// A value of LiteralCsenetOgCaseReason.
    /// </summary>
    [JsonPropertyName("literalCsenetOgCaseReason")]
    public Code LiteralCsenetOgCaseReason
    {
      get => literalCsenetOgCaseReason ??= new();
      set => literalCsenetOgCaseReason = value;
    }

    /// <summary>
    /// A value of LiteralCsenetFunctionalType.
    /// </summary>
    [JsonPropertyName("literalCsenetFunctionalType")]
    public Code LiteralCsenetFunctionalType
    {
      get => literalCsenetFunctionalType ??= new();
      set => literalCsenetFunctionalType = value;
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
    /// A value of Validation.
    /// </summary>
    [JsonPropertyName("validation")]
    public CodeValue Validation
    {
      get => validation ??= new();
      set => validation = value;
    }

    private Common multipleAps;
    private Code ivdAgency;
    private Common tribalAgency;
    private InterstateRequest tempSave;
    private Common common;
    private Code literalTribalAgency;
    private Common functionCheck;
    private Code literalCountry;
    private InterstateRequest nullInterstateRequest;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private CsePerson ap;
    private InterstateCase interstateCase;
    private Common automatic;
    private Array<ChildrenGroup> children;
    private Array<LegalActionsGroup> legalActions;
    private Infrastructure infrastructure;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea current;
    private Common position;
    private WorkArea workArea;
    private Common length;
    private Common count;
    private Common valid;
    private CodeValue codeValue;
    private Case1 nullCase;
    private Fips nullFips;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private InterstateRequestHistory nullInterstateRequestHistory;
    private InterstateContact nullInterstateContact;
    private InterstateContactAddress nullInterstateContactAddress;
    private LegalAction nullLegalAction;
    private Code literalCsenetCaseClosure;
    private Code literalState;
    private Code literalCsenetActionType;
    private Code literalCsenetOgCaseReason;
    private Code literalCsenetFunctionalType;
    private Common error;
    private CodeValue validation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private Case1 case1;
    private Fips fips;
    private CaseRole ap;
    private CsePerson csePerson;
    private InterstateRequest interstateRequest;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private CaseRole child;
    private LegalAction legalAction;
  }
#endregion
}

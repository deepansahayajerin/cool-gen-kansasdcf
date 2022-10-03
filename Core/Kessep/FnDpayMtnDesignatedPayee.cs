// Program: FN_DPAY_MTN_DESIGNATED_PAYEE, ID: 371752427, model: 746.
// Short name: SWEDPAYP
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
/// A program: FN_DPAY_MTN_DESIGNATED_PAYEE.
/// </para>
/// <para>
/// RESP: FINANCE
/// DESC: This procedure will support the business requirement to allow for a 
/// designated payee to receive disbursements for a payee.  The designated payee
/// must be a CSE Person and maybe designated for a specific period of time or
/// starting at one point in time and going on indefinitely.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDpayMtnDesignatedPayee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DPAY_MTN_DESIGNATED_PAYEE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDpayMtnDesignatedPayee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDpayMtnDesignatedPayee.
  /// </summary>
  public FnDpayMtnDesignatedPayee(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************************
    // Procedure : Maintain Designated Payee
    // Modified By : R.B.mohapatra - MTW
    // Change Log :
    // 04-22-1996  -  *Change in the Procedure Logic
    // *development of 3 CABs for CREATE, UPDATE and DELETE of designated payee 
    // info
    // 12/18/96        R. Marchman       Add new security/next tran
    // 05/14/97        S.Mahapatra - MTW  Modified to display data when coming 
    // from obligation trans.
    // 06/12/97        H. Kennedy-MTW    Added logic to Update CAB
    // 
    // to validate for date overlap. Added logic to display the most
    // current Designated Payee if an active one does not exist.
    // Added logic to display by identifier when coming from DPYL.
    // 08/01/97      Paul R. Egger     When flowing to DPAY from an
    // obligation, the screen was incorrectly changing  the
    // obligation that was passed to DPAY if using the prompt facility to look
    // up a person's CSE number.  In this case, instead of going through all of
    // the display logic, processing stops after the name is retrieved.
    // 08/25/97    JF.Caillouet 	Can now add a discontinue date on the Current 
    // Date.
    // ***************************************************************
    // ****************************************************************
    // 09/21/98   RK. Made the following changes
    //                   1.  Put in flow to and from Menu(DMEN)
    //                   2.  Removed from screen: Supported person,
    //                        Interstate Obligation, Payor, Court Order,
    //                        Obligation Type.
    //                   3.  Added to the screen: Street 3, Street 4,
    //                        Country, Province, Postal Code for
    //                        foreign addresses. Zip+4 for domestic.
    //                   4.  Non-cse related Payee cannot have a DP.
    //                   5.  Took out 80 % of code(not views) of the
    //                        obsolete code relating to tying a DP to
    //                        a specific obligation.
    //                   6.  After the Payee and DP are brought back
    //                        to the screen the prompt to NAME for a
    //                        new DP is disabled.
    //                   7.  Do not allow the same CSE person to be
    //                        added as an Payee and DP
    //                   8.  Overlapping timeframes for DP's isn't
    //                        allowed.
    //                  9.  The effective date cannot be blanked out
    //                        after a display and updated.
    //                 10.  Should be able to Delete a DP record that
    //                        was created today.
    // ****************************************************************
    // ***************************************************************
    // PR#78935 SWSRKXD 11/01/1999
    // - Remove case_role check for 'CH'. (An AR may have been a
    // CH earlier) Hence look at AR or AP roles only.
    // - Remove check to ensure DP is not the AP on a case where
    // Payee is AR.
    // ***************************************************************
    // ***************************************************************
    // PR#82489 SWSRKXD 1/4/2000
    // - Delete views of ob_trn_desig_payee.
    // ***************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    local.NoOfDesigPayeesFound.Count = 0;
    export.Hidden.Assign(import.Hidden);

    // If command is CLEAR, escape before moving Imports to Exports
    // so that the screen is blanked out.
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NN0000_ALL_OK";

      return;
    }

    // Get sys gen number for payee
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.HiddenInitialCommand.Command = import.HiddenInitialCommand.Command;

    // Move all IMPORTs to EXPORTs (IF COMMAND <> DISPLAY).
    export.Payee.Assign(import.Payee);
    export.DesignatedPayee.Assign(import.DesignatedPayee);
    export.DesignatedPayeePrompt.Flag = import.DesignatedPayeePrompt.Flag;
    export.ArpayeePrompt.Flag = import.ArpayeePrompt.Flag;
    export.HiddenDesignatedPayee.Number = import.HiddenDesignatedPayee.Number;
    export.HiddenPayee.Number = import.HiddenPayee.Number;
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.CsePersonAddress.Assign(import.CsePersonAddress);
    MoveCsePersonsWorkSet(import.Payor, export.Payor);
    MoveCsePersonsWorkSet(import.Supported, export.Supported);
    export.BothTypeOfDpIn1View.Assign(import.BothTypeOfDpIn1View);
    export.HidBothTypeOfDpIn1.Assign(import.HidBothTypeOfDpIn1);

    if (Equal(global.Command, "RETDPYL"))
    {
      if (IsEmpty(export.DesignatedPayee.Number))
      {
        export.DesignatedPayee.Number = export.HiddenDesignatedPayee.Number;
        export.BothTypeOfDpIn1View.Assign(export.HidBothTypeOfDpIn1);
      }

      global.Command = "DISPLAY";
    }

    // *** Checks if the control returns from the cse_name_list proc. 
    // coresponding to the PROMPT for Payee or Designated Payee. This is
    // necessary since the cse_name_list procedure is LINKED from this proc by a
    // single LINK for both payee and designated payee prompt and the link
    // returns only one cse_person number.  While Returning from the LINK, the
    // command is set to "BYPASS".
    // The following control structure will determine the receiving screen field
    // for the cse_person number in case of a LINK. Otherwise it will assume
    // the user has input the cse_person numbers.
    // ***
    if (Equal(global.Command, "RETCSENO"))
    {
      if (AsChar(import.ArpayeePrompt.Flag) == 'S')
      {
        export.ArpayeePrompt.Flag = "";
        MoveCsePersonsWorkSet(import.ThruFlowCsePersonsWorkSet, export.Payee);
        export.HiddenPayee.Number = export.Payee.Number;
      }
      else if (AsChar(import.DesignatedPayeePrompt.Flag) == 'S')
      {
        export.DesignatedPayeePrompt.Flag = "";
        MoveCsePersonsWorkSet(import.ThruFlowCsePersonsWorkSet,
          export.DesignatedPayee);
        export.HiddenDesignatedPayee.Number = export.DesignatedPayee.Number;
      }

      if (!Equal(export.HiddenInitialCommand.Command, "OBLIG"))
      {
        global.Command = "DISPLAY";
      }
    }

    // *** Left Padding Payee numbers ***
    if (!IsEmpty(export.Payee.Number))
    {
      local.LeftPadding.Text10 = export.Payee.Number;
      UseEabPadLeftWithZeros();
      export.Payee.Number = local.LeftPadding.Text10;
      UseSiReadCsePerson3();
    }

    // *** Left Padding Designated-Payee numbers ***
    if (!IsEmpty(export.DesignatedPayee.Number))
    {
      local.LeftPadding.Text10 = export.DesignatedPayee.Number;
      UseEabPadLeftWithZeros();
      export.DesignatedPayee.Number = local.LeftPadding.Text10;
    }

    local.Payee.Number = export.Payee.Number;
    local.DesignatedPayee.Number = export.DesignatedPayee.Number;

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumberObligee = export.Payee.Number;
      export.Hidden.CsePersonNumber = export.Payee.Number;
      export.Hidden.StandardCrtOrdNumber =
        import.LegalAction.StandardNumber ?? "";
      export.Hidden.CsePersonNumberObligor = import.ThruFlowObligor.Number;
      export.Hidden.ObligationId =
        import.ThruFlowObligation.SystemGeneratedIdentifier;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    // ****************************************************************
    // If the DP field has been populated correctly then protect this field and 
    // disable the flow to NAME(PF4). RK 9/21/98
    // ****************************************************************
    if (!IsEmpty(export.DesignatedPayee.Number))
    {
      var field1 = GetField(export.DesignatedPayee, "number");

      field1.Protected = true;

      var field2 = GetField(export.DesignatedPayeePrompt, "flag");

      field2.Protected = true;
    }

    if (Equal(global.Command, "RETCSENO"))
    {
      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CsePersonNumberObligee))
      {
        export.Payee.Number = export.Hidden.CsePersonNumberObligee ?? Spaces
          (10);
      }
      else
      {
        export.Payee.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      export.ThruFlowObligation.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      export.LegalAction.StandardNumber =
        export.Hidden.StandardCrtOrdNumber ?? "";

      // *** Left Padding Payee numbers ***
      if (!IsEmpty(export.Payee.Number))
      {
        local.LeftPadding.Text10 = export.Payee.Number;
        UseEabPadLeftWithZeros();
        export.Payee.Number = local.LeftPadding.Text10;
      }

      // *** Left Padding Designated-Payee numbers ***
      if (!IsEmpty(export.DesignatedPayee.Number))
      {
        local.LeftPadding.Text10 = export.DesignatedPayee.Number;
        UseEabPadLeftWithZeros();
        export.DesignatedPayee.Number = local.LeftPadding.Text10;
      }

      local.Payee.Number = export.Payee.Number;
      local.DesignatedPayee.Number = export.DesignatedPayee.Number;
      export.ThruFlowCsePerson.Number = export.Payee.Number;
      export.ThruFlowWorkArea.Text33 = export.Payee.FormattedName;
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "DPYL"))
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

    // **** end   group C ****
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      (
        !Equal(export.DesignatedPayee.Number,
      import.HiddenDesignatedPayee.Number) || !
      Equal(export.Payee.Number, import.HiddenPayee.Number)))
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field1 = GetField(export.DesignatedPayee, "number");

      field1.Error = true;

      var field2 = GetField(export.Payee, "number");

      field2.Error = true;

      return;
    }

    // ********************
    // Common validation for display and Add
    // - Payee is mandatory and must be valid cse_person
    // - Payee must be an 'AP' or 'AR' case_role on some case
    // *******************
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD"))
    {
      // *** Validate payee number ***
      if (IsEmpty(export.Payee.Number))
      {
        var field = GetField(export.Payee, "number");

        field.Error = true;

        ExitState = "KEY_FIELD_IS_BLANK";

        return;
      }

      local.Payee.Number = export.Payee.Number;

      if (!ReadCsePerson3())
      {
        ExitState = "CSE_PERSON_NF";

        var field = GetField(export.Payee, "number");

        field.Error = true;

        return;
      }

      local.Payee.Assign(entities.PayeeAr);

      if (AsChar(entities.PayeeAr.Type1) == 'O')
      {
        export.Payee.FormattedName = entities.PayeeAr.OrganizationName ?? Spaces
          (33);
      }
      else
      {
        UseSiReadCsePerson1();
      }

      // ***************************************************************
      // The payee must be a AP, or AR(case role) be active in that case role's-
      // role, and be on an open case. RK 10/1/98
      // Changed read, doesn't have to be on an active Case or an active case 
      // role, just use to be. rk 7/18/99
      // ***************************************************************
      // ***************************************************************
      // KD - PR#78935 11/01/99
      // Remove 'CH' from read each and disable check below. Also
      // remove 'case' from read each since it is not being
      // referenced later.
      // ***************************************************************
      if (ReadCaseRole())
      {
        local.CaseRelatedPerson.Flag = "Y";
      }

      // ***************************************************************
      // KD - PR#78935 11/01/99
      // Add check to ensure Payee is case related. This check was
      // being performed for add action but not for display. The
      // chunk of code in 'Add' has now been disabled.
      // ***************************************************************
      if (AsChar(local.CaseRelatedPerson.Flag) != 'Y')
      {
        ExitState = "FN0000_PAYEE_NOT_CASE_REL_NO_DP";

        var field = GetField(export.Payee, "number");

        field.Error = true;

        return;
      }

      // *** Validate Designated payee number ***
      if (!IsEmpty(export.DesignatedPayee.Number))
      {
        if (!ReadCsePerson1())
        {
          ExitState = "FN0000_DESIG_PAYEE_CSE_PERSON_NF";

          var field = GetField(export.DesignatedPayee, "number");

          field.Error = true;

          return;
        }

        local.LeftPadding.Text10 = entities.DesignatedPayee.Number;
        UseEabPadLeftWithZeros();
        export.DesignatedPayee.Number = local.LeftPadding.Text10;

        if (AsChar(entities.DesignatedPayee.Type1) == 'O')
        {
          export.DesignatedPayee.FormattedName =
            entities.DesignatedPayee.OrganizationName ?? Spaces(33);
        }
        else
        {
          UseSiReadCsePerson2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.DesignatedPayeePrompt, "flag");

            field.Protected = true;
          }
          else
          {
            return;
          }
        }
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (IsEmpty(export.Payee.Number) || IsEmpty
        (export.DesignatedPayee.Number))
      {
        var field = GetField(export.Payee, "number");

        field.Error = true;

        ExitState = "KEY_FIELD_IS_BLANK";

        return;
      }

      if (IsEmpty(export.DesignatedPayee.Number))
      {
        var field = GetField(export.DesignatedPayee, "number");

        field.Error = true;

        ExitState = "KEY_FIELD_IS_BLANK";

        return;
      }
    }

    // Validation common to CREATE and UPDATE.  If an error is found, EXIT STATE
    // should be set.
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (Equal(import.BothTypeOfDpIn1View.EffectiveDate,
        local.InitialisedToZeros.Date))
      {
        ExitState = "EFFECTIVE_DATE_REQUIRED";

        var field = GetField(export.BothTypeOfDpIn1View, "effectiveDate");

        field.Error = true;

        return;
      }

      // If discontinue date is blank, then default it to maximum date
      if (Equal(import.BothTypeOfDpIn1View.DiscontinueDate,
        local.InitialisedToZeros.Date))
      {
        export.BothTypeOfDpIn1View.DiscontinueDate = local.Max.Date;
      }

      // Discontinue date cannot be prior to current date
      // *** Can now add discontinue date on Current Date ***
      if (Lt(export.BothTypeOfDpIn1View.DiscontinueDate, Now().Date))
      {
        ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

        var field = GetField(export.BothTypeOfDpIn1View, "discontinueDate");

        field.Error = true;

        goto Test1;
      }

      // *** A record cannot have discontinue date <= effective date ***
      // ****************************************************************
      // Changed exit state to bring back a more complete message. Discontinue 
      // date must be greater than effective date. rk 9/28
      // ****************************************************************
      if (!Lt(export.BothTypeOfDpIn1View.EffectiveDate,
        export.BothTypeOfDpIn1View.DiscontinueDate))
      {
        var field = GetField(export.BothTypeOfDpIn1View, "discontinueDate");

        field.Error = true;

        ExitState = "FN0000_DISC_DT_GREATER_THAN_EFF";

        goto Test1;
      }

      // *** Narrative (description) is required ***
      if (IsEmpty(export.BothTypeOfDpIn1View.Notes))
      {
        var field = GetField(export.BothTypeOfDpIn1View, "notes");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      // check on userid
    }

Test1:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate, local.Max.Date))
      {
        export.BothTypeOfDpIn1View.DiscontinueDate =
          local.InitialisedToZeros.Date;
      }

      return;
    }

    // The following CASE OF COMMAND validates whether the desired command has 
    // all the correct inputs to continue its execution.
    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        MoveCsePersonsWorkSet(export.DesignatedPayee,
          export.PassedDesignatedPayee);

        if (!Lt(Now().Date, export.BothTypeOfDpIn1View.EffectiveDate) && (
          !Lt(export.BothTypeOfDpIn1View.DiscontinueDate, Now().Date) || Equal
          (export.BothTypeOfDpIn1View.DiscontinueDate,
          local.InitialisedToZeros.Date)))
        {
          // <<<RBM  11/19/98   The Designated Payee is currently active ; So we
          // may pass it to the Calling program. >>>
        }
        else
        {
          export.PassedDesignatedPayee.Number = "";
          export.PassedDesignatedPayee.FormattedName = "";
        }

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "ADD":
        // ****************************************************************
        // The Payee and DP aren't allowed to be the same cse person. RK 9/21/98
        // ****************************************************************
        if (Equal(export.DesignatedPayee.Number, export.Payee.Number))
        {
          var field1 = GetField(export.DesignatedPayee, "number");

          field1.Error = true;

          var field2 = GetField(export.Payee, "number");

          field2.Error = true;

          ExitState = "FN0000_PAYEE_AND_DP_CANT_BE_SAME";

          return;
        }

        // Cannot create or update a record that has effective date prior to 
        // current date
        //  *** - Sumanta - MTW - 05/12/97
        //      - Changed 'less than equal to'  to 'less than'
        if (Lt(export.BothTypeOfDpIn1View.EffectiveDate, Now().Date))
        {
          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          var field = GetField(export.BothTypeOfDpIn1View, "effectiveDate");

          field.Error = true;

          return;
        }

        // <<< The Designated Payee can not be the AP in any of the cases in 
        // which the Payee features as the AR >>>
        local.CaseRelatedPerson.Flag = "N";

        // ***************************************************************
        // KD - PR#78935 11/01/1999
        // Check already performed earlier (common validation for
        // display and add)
        // This read each was however doing extra work to ensure DP
        // is not an AP in a case where Payee is AR. But this check is
        // no longer required!
        // ***************************************************************
        // ****************************************************************
        // Payees that are tied to Incoming Interstate Cases are not allowed to 
        // have DPs. No code was placed here due to the fact that the tables are
        // not stable, aren't being populated now, and are uncertain to be
        // populated on the day of implementation. So I've only included and
        // note here and raised an issue so that this check will not be
        // forgotten. RK 10/1/1998
        // ****************************************************************
        if (Equal(export.Payor.Number, export.DesignatedPayee.Number))
        {
          ExitState = "FN0000_PAYEE_CANT_BE_DESG_PAY_2";

          var field = GetField(export.DesignatedPayee, "number");

          field.Error = true;

          return;
        }

        break;
      case "UPDATE":
        // ****************************************************************
        // The effective date field is not allowed to be blank on an update. RK 
        // 9/21/98
        // ****************************************************************
        if (Equal(export.BothTypeOfDpIn1View.EffectiveDate,
          local.InitialisedToZeros.Date))
        {
          ExitState = "KEY_FIELD_IS_BLANK";

          return;
        }

        // **********************************
        // The effective date has changed
        // **********************************
        if (!Equal(export.BothTypeOfDpIn1View.EffectiveDate,
          export.HidBothTypeOfDpIn1.EffectiveDate))
        {
          if (!Lt(export.HidBothTypeOfDpIn1.EffectiveDate, local.Current.Date) &&
            Lt(export.BothTypeOfDpIn1View.EffectiveDate, local.Current.Date))
          {
            ExitState = "FN0000_EFF_DATE_CHG_NOT_ALLOWED";

            var field = GetField(export.BothTypeOfDpIn1View, "effectiveDate");

            field.Error = true;
          }

          if (Lt(export.HidBothTypeOfDpIn1.EffectiveDate, local.Current.Date))
          {
            // ****************************************************************
            // Changed the exit state to be of the fn0000 format and have a more
            // decriptive explanation.   rk 9/21/98
            // 
            // ****************************************************************
            if (Lt(export.HidBothTypeOfDpIn1.DiscontinueDate, local.Current.Date))
              
            {
              ExitState = "FN0000_CANT_CHG_INACTIVE_DATES";

              var field = GetField(export.BothTypeOfDpIn1View, "effectiveDate");

              field.Error = true;
            }
            else
            {
              ExitState = "FN0000_CANT_CHANGE_ACTIVE_EFF_DT";

              var field = GetField(export.BothTypeOfDpIn1View, "effectiveDate");

              field.Error = true;
            }
          }

          if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate, local.Max.Date))
          {
            export.BothTypeOfDpIn1View.DiscontinueDate =
              local.InitialisedToZeros.Date;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.BothTypeOfDpIn1View.Assign(export.HidBothTypeOfDpIn1);

            return;
          }
        }

        if (!Equal(export.HidBothTypeOfDpIn1.DiscontinueDate,
          local.InitialisedToZeros.Date) && !
          Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
          export.HidBothTypeOfDpIn1.DiscontinueDate))
        {
          if (Lt(export.HidBothTypeOfDpIn1.DiscontinueDate, local.Current.Date))
          {
            ExitState = "FN0000_CANT_CHG_INACTIVE_DATES";
            export.BothTypeOfDpIn1View.Assign(export.HidBothTypeOfDpIn1);

            var field = GetField(export.BothTypeOfDpIn1View, "discontinueDate");

            field.Error = true;

            return;
          }
          else if (Lt(export.BothTypeOfDpIn1View.DiscontinueDate, Now().Date))
          {
            ExitState = "FN0000_DISCONTINUE_DT_LT_CURRENT";

            var field = GetField(export.BothTypeOfDpIn1View, "discontinueDate");

            field.Error = true;

            return;
          }
          else if (Lt(export.BothTypeOfDpIn1View.DiscontinueDate,
            export.BothTypeOfDpIn1View.EffectiveDate))
          {
            ExitState = "FN0000_DISC_DATE_BEFORE_EFF";

            var field = GetField(export.BothTypeOfDpIn1View, "discontinueDate");

            field.Error = true;

            return;
          }
          else
          {
            // Continue Processing
          }
        }

        break;
      default:
        break;
    }

    // Get sys gen number for payee
    // ------------------------------------------------------------
    //                     Main CASE OF COMMAND.
    // ------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // Validate Prompt
        switch(AsChar(export.ArpayeePrompt.Flag))
        {
          case ' ':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.ArpayeePrompt, "flag");

            field.Error = true;

            return;
        }

        switch(AsChar(export.DesignatedPayeePrompt.Flag))
        {
          case ' ':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.DesignatedPayeePrompt, "flag");

            field.Error = true;

            return;
        }

        var field1 = GetField(export.ArpayeePrompt, "flag");

        field1.Error = true;

        var field2 = GetField(export.DesignatedPayeePrompt, "flag");

        field2.Error = true;

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        return;
      case "DISPLAY":
        local.NoOfDesigPayeesFound.Count = 0;
        local.DpForEffDateFound.Flag = "N";
        local.ActiveAddressFound.Flag = "N";
        local.ActiveDesigPayeeFound.Flag = "N";
        local.OtherDesigPFound.Flag = "N";
        local.OtherDesigPayee.Count = 0;
        local.NoOfDesigPayeesFound.Count = 0;

        // << Payee and Designated Payee Cse_Person has already been read in a 
        // previous statement and currency has already been established. >>
        if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
          local.InitialisedToZeros.Date))
        {
          export.BothTypeOfDpIn1View.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
        }

        if (IsEmpty(export.Payee.Number) && IsEmpty
          (export.DesignatedPayee.Number))
        {
          var field = GetField(export.Payee, "number");

          field.Error = true;

          ExitState = "FN0000_INVALID_INPUT";

          break;
        }
        else if (!IsEmpty(export.Payee.Number) && !
          IsEmpty(export.DesignatedPayee.Number))
        {
          // *** Both Payee and Designated Payee exist ***
          // <<< If the effective date is present >>>
          if (!Equal(export.BothTypeOfDpIn1View.EffectiveDate,
            local.InitialisedToZeros.Date))
          {
            if (ReadCsePersonDesigPayee2())
            {
              export.BothTypeOfDpIn1View.Assign(entities.CsePersonDesigPayee);

              if (!Lt(local.Current.Date,
                export.BothTypeOfDpIn1View.EffectiveDate) && !
                Lt(export.BothTypeOfDpIn1View.DiscontinueDate,
                local.Current.Date))
              {
                local.ActiveDesigPayeeFound.Flag = "Y";
              }
              else
              {
                local.DpForEffDateFound.Flag = "Y";
              }

              if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
                local.Max.Date))
              {
                export.BothTypeOfDpIn1View.DiscontinueDate =
                  local.InitialisedToZeros.Date;
              }

              // <<< GET THE ACTIVE ADDRESS >>>
              export.CsePersonAddress.City = "";
              export.CsePersonAddress.State = "";
              export.CsePersonAddress.Street1 = "";
              export.CsePersonAddress.Street2 = "";
              export.CsePersonAddress.ZipCode = "";

              // ****************************************
              // New screen fields added. RK 9/21/98
              // ****************************************
              export.CsePersonAddress.Street3 = "";
              export.CsePersonAddress.Street4 = "";
              export.CsePersonAddress.Country = "";
              export.CsePersonAddress.Province = "";
              export.CsePersonAddress.PostalCode = "";
              export.CsePersonAddress.Zip4 = "";
              UseFnGetActiveCsePersonAddress();

              if (AsChar(local.ActiveAddressFound.Flag) == 'N')
              {
                export.CsePersonAddress.Street1 = "*No Active Address Found*";
              }

              local.NoOfDesigPayeesFound.Count =
                (int)((long)local.NoOfDesigPayeesFound.Count + 1);
            }
            else
            {
              // <<< No Designated Payee found for this effective date >>>
              local.DpForEffDateFound.Flag = "N";
            }
          }

          // <<< Get the Currently Active Designated Payee, if any exists >>>
          if (AsChar(local.DpForEffDateFound.Flag) == 'N' && AsChar
            (local.ActiveDesigPayeeFound.Flag) == 'N')
          {
            local.ActiveDesigPayeeFound.Flag = "N";

            if (ReadCsePersonDesigPayee1())
            {
              export.BothTypeOfDpIn1View.Assign(entities.CsePersonDesigPayee);

              if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
                local.Max.Date))
              {
                export.BothTypeOfDpIn1View.DiscontinueDate =
                  local.InitialisedToZeros.Date;
              }

              // <<< GET THE ACTIVE DESIGNATED PAYEE ADDRESS>>>
              export.CsePersonAddress.City = "";
              export.CsePersonAddress.State = "";
              export.CsePersonAddress.Street1 = "";
              export.CsePersonAddress.Street2 = "";
              export.CsePersonAddress.ZipCode = "";

              // ****************************************
              // New screen fields added. RK 9/21/98
              // ****************************************
              export.CsePersonAddress.Street3 = "";
              export.CsePersonAddress.Street4 = "";
              export.CsePersonAddress.Country = "";
              export.CsePersonAddress.Province = "";
              export.CsePersonAddress.PostalCode = "";
              export.CsePersonAddress.Zip4 = "";
              UseFnGetActiveCsePersonAddress();

              if (AsChar(local.ActiveAddressFound.Flag) == 'N')
              {
                export.CsePersonAddress.Street1 = "*No Active Address Found*";
              }

              local.NoOfDesigPayeesFound.Count =
                (int)((long)local.NoOfDesigPayeesFound.Count + 1);
              local.ActiveDesigPayeeFound.Flag = "Y";
            }
            else
            {
              local.ActiveDesigPayeeFound.Flag = "N";
            }
          }

          // <<< CHECK IF MULTIPLE DESIG PAYEES ARE SET UP   >>>
          local.OtherDesigPFound.Flag = "N";

          // ****************************************************************
          // Changed the read to look for any DP associated to this Payee, not 
          // just the one currently entered. RK 9/21/98
          // ****************************************************************
          foreach(var item in ReadCsePersonDesigPayee5())
          {
            ++local.OtherDesigPayee.Count;

            if (AsChar(local.ActiveDesigPayeeFound.Flag) == 'N' && AsChar
              (local.OtherDesigPFound.Flag) == 'N' && AsChar
              (local.DpForEffDateFound.Flag) == 'N')
            {
              local.OtherDesigPFound.Flag = "Y";
              export.BothTypeOfDpIn1View.Assign(entities.CsePersonDesigPayee);

              if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
                local.Max.Date))
              {
                export.BothTypeOfDpIn1View.DiscontinueDate =
                  local.InitialisedToZeros.Date;
              }
            }
            else if (local.OtherDesigPayee.Count > 1)
            {
              break;
            }
          }

          local.NoOfDesigPayeesFound.Count += local.OtherDesigPayee.Count;

          if (local.NoOfDesigPayeesFound.Count > 1)
          {
            if (AsChar(local.DpForEffDateFound.Flag) == 'Y')
            {
              ExitState = "FN0000_DP_FND_FOR_EFF_DT_MULT_DP";
            }
            else if (AsChar(local.ActiveDesigPayeeFound.Flag) == 'Y')
            {
              ExitState = "FN0000_1ACTIVE_AND_MULTI_DP_FND";
            }
            else if (AsChar(local.OtherDesigPFound.Flag) == 'Y')
            {
              ExitState = "FN0000_NO_ACTVE_BUT_MULT_DP_EXST";
            }
          }
          else if (local.NoOfDesigPayeesFound.Count == 0)
          {
            ExitState = "FN0000_NO_DESIGNATED_PAYEE_FOUND";
          }
          else if (AsChar(local.ActiveDesigPayeeFound.Flag) == 'Y' || AsChar
            (local.DpForEffDateFound.Flag) == 'Y')
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (AsChar(local.OtherDesigPFound.Flag) == 'Y')
          {
            ExitState = "FN0000_NO_ACTIVE_DP";
          }
        }
        else if (!IsEmpty(export.Payee.Number) && IsEmpty
          (export.DesignatedPayee.Number))
        {
          // <<< Only the Payee Exists.
          //     Get the Currently Active Designated Payee, if any exists, if 
          // the effective date is blank >>>
          // <<< If the effective date is present >>>
          if (!Equal(export.BothTypeOfDpIn1View.EffectiveDate,
            local.InitialisedToZeros.Date))
          {
            if (ReadCsePersonDesigPayee4())
            {
              export.BothTypeOfDpIn1View.Assign(entities.CsePersonDesigPayee);

              if (ReadCsePerson2())
              {
                export.DesignatedPayee.Number = entities.DesignatedPayee.Number;

                if (AsChar(entities.DesignatedPayee.Type1) == 'O')
                {
                  export.DesignatedPayee.FormattedName =
                    entities.DesignatedPayee.OrganizationName ?? Spaces(33);
                }
                else
                {
                  UseSiReadCsePerson2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.DesignatedPayee.FormattedName =
                      "*NAME NOT RETURNED FROM CAB*";
                  }
                }

                if (!Lt(local.Current.Date,
                  export.BothTypeOfDpIn1View.EffectiveDate) && !
                  Lt(export.BothTypeOfDpIn1View.DiscontinueDate,
                  local.Current.Date))
                {
                  local.ActiveDesigPayeeFound.Flag = "Y";
                }
                else
                {
                  local.DpForEffDateFound.Flag = "Y";
                }

                if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
                  local.Max.Date))
                {
                  export.BothTypeOfDpIn1View.DiscontinueDate =
                    local.InitialisedToZeros.Date;
                }

                // <<< GET THE ACTIVE ADDRESS>>>
                export.CsePersonAddress.City = "";
                export.CsePersonAddress.State = "";
                export.CsePersonAddress.Street1 = "";
                export.CsePersonAddress.Street2 = "";
                export.CsePersonAddress.ZipCode = "";

                // ****************************************
                // New screen fields added. RK 9/21/98
                // ****************************************
                export.CsePersonAddress.Street3 = "";
                export.CsePersonAddress.Street4 = "";
                export.CsePersonAddress.Country = "";
                export.CsePersonAddress.Province = "";
                export.CsePersonAddress.PostalCode = "";
                export.CsePersonAddress.Zip4 = "";
                UseFnGetActiveCsePersonAddress();

                if (AsChar(local.ActiveAddressFound.Flag) == 'N')
                {
                  export.CsePersonAddress.Street1 = "*No Active Address Found*";
                }

                local.NoOfDesigPayeesFound.Count =
                  (int)((long)local.NoOfDesigPayeesFound.Count + 1);
              }
              else
              {
                ExitState = "FN0000_PERS_DESIG_PAYEE_NF_RB";

                break;
              }
            }
            else
            {
              // <<< No Designated Payee found for this effective date >>>
              local.DpForEffDateFound.Flag = "N";
            }
          }

          // <<< GET THE ACTIVE DP,IF ANY >>>
          if (AsChar(local.ActiveDesigPayeeFound.Flag) == 'N' && AsChar
            (local.DpForEffDateFound.Flag) == 'N')
          {
            if (ReadCsePersonDesigPayee3())
            {
              if (ReadCsePerson2())
              {
                export.BothTypeOfDpIn1View.Assign(entities.CsePersonDesigPayee);

                if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
                  local.Max.Date))
                {
                  export.BothTypeOfDpIn1View.DiscontinueDate =
                    local.InitialisedToZeros.Date;
                }

                local.LeftPadding.Text10 = entities.DesignatedPayee.Number;
                UseEabPadLeftWithZeros();
                export.DesignatedPayee.Number = local.LeftPadding.Text10;
                export.HiddenDesignatedPayee.Number = local.LeftPadding.Text10;

                if (AsChar(entities.DesignatedPayee.Type1) == 'O')
                {
                  export.DesignatedPayee.FormattedName =
                    entities.DesignatedPayee.OrganizationName ?? Spaces(33);
                }
                else
                {
                  UseSiReadCsePerson2();

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                  }
                  else
                  {
                    return;
                  }
                }

                // <<< GET THE ADDRESS >>>
                export.CsePersonAddress.City = "";
                export.CsePersonAddress.State = "";
                export.CsePersonAddress.Street1 = "";
                export.CsePersonAddress.Street2 = "";
                export.CsePersonAddress.ZipCode = "";

                // ****************************************
                // New screen fields added. RK 9/21/98
                // ****************************************
                export.CsePersonAddress.Street3 = "";
                export.CsePersonAddress.Street4 = "";
                export.CsePersonAddress.Country = "";
                export.CsePersonAddress.Province = "";
                export.CsePersonAddress.PostalCode = "";
                export.CsePersonAddress.Zip4 = "";
                UseFnGetActiveCsePersonAddress();

                if (AsChar(local.ActiveAddressFound.Flag) == 'N')
                {
                  export.CsePersonAddress.Street1 = "*No Active Address Found*";
                }
              }

              local.NoOfDesigPayeesFound.Count =
                (int)((long)local.NoOfDesigPayeesFound.Count + 1);
              local.ActiveDesigPayeeFound.Flag = "Y";
            }
            else
            {
              local.ActiveDesigPayeeFound.Flag = "N";
            }
          }

          local.OtherDesigPFound.Flag = "N";

          foreach(var item in ReadCsePersonDesigPayee6())
          {
            ++local.OtherDesigPayee.Count;

            if (AsChar(local.ActiveDesigPayeeFound.Flag) == 'N' && AsChar
              (local.DpForEffDateFound.Flag) == 'N' && AsChar
              (local.OtherDesigPFound.Flag) == 'N')
            {
              if (ReadCsePerson2())
              {
                local.OtherDesigPFound.Flag = "Y";
                export.BothTypeOfDpIn1View.Assign(entities.CsePersonDesigPayee);

                if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
                  local.Max.Date))
                {
                  export.BothTypeOfDpIn1View.DiscontinueDate =
                    local.InitialisedToZeros.Date;
                }

                local.LeftPadding.Text10 = entities.DesignatedPayee.Number;
                UseEabPadLeftWithZeros();
                export.DesignatedPayee.Number = local.LeftPadding.Text10;
                export.HiddenDesignatedPayee.Number = local.LeftPadding.Text10;

                if (AsChar(entities.DesignatedPayee.Type1) == 'O')
                {
                  export.DesignatedPayee.FormattedName =
                    entities.DesignatedPayee.OrganizationName ?? Spaces(33);
                }
                else
                {
                  UseSiReadCsePerson2();

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                  }
                  else
                  {
                    goto Test2;
                  }
                }

                export.CsePersonAddress.City = "";
                export.CsePersonAddress.State = "";
                export.CsePersonAddress.Street1 = "";
                export.CsePersonAddress.Street2 = "";
                export.CsePersonAddress.ZipCode = "";

                // ****************************************
                // New screen fields added. RK 9/21/98
                // ****************************************
                export.CsePersonAddress.Street3 = "";
                export.CsePersonAddress.Street4 = "";
                export.CsePersonAddress.Country = "";
                export.CsePersonAddress.Province = "";
                export.CsePersonAddress.PostalCode = "";
                export.CsePersonAddress.Zip4 = "";
                UseFnGetActiveCsePersonAddress();

                if (AsChar(local.ActiveAddressFound.Flag) == 'N')
                {
                  export.CsePersonAddress.Street1 = "*No Active Address Found*";
                }
              }
              else
              {
                ExitState = "FN0000_DP_FOR_CSE_P_DP_NF";
                export.BothTypeOfDpIn1View.DiscontinueDate =
                  local.InitialisedToZeros.Date;

                goto Test2;
              }
            }
            else if (local.OtherDesigPayee.Count > 1)
            {
              break;
            }
          }

          local.NoOfDesigPayeesFound.Count += local.OtherDesigPayee.Count;

          if (local.NoOfDesigPayeesFound.Count > 1)
          {
            if (AsChar(local.DpForEffDateFound.Flag) == 'Y')
            {
              ExitState = "FN0000_DP_FND_FOR_EFF_DT_MULT_DP";
            }
            else if (AsChar(local.ActiveDesigPayeeFound.Flag) == 'Y')
            {
              ExitState = "FN0000_1ACTIVE_AND_MULTI_DP_FND";
            }
            else if (AsChar(local.OtherDesigPFound.Flag) == 'Y')
            {
              ExitState = "FN0000_NO_ACTVE_BUT_MULT_DP_EXST";
            }
          }
          else if (local.NoOfDesigPayeesFound.Count == 0)
          {
            ExitState = "FN0000_NO_DESIGNATED_PAYEE_FOUND";
          }
          else if (AsChar(local.ActiveDesigPayeeFound.Flag) == 'Y' || AsChar
            (local.DpForEffDateFound.Flag) == 'Y')
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (AsChar(local.OtherDesigPFound.Flag) == 'Y')
          {
            ExitState = "FN0000_NO_ACTIVE_DP";
          }
        }
        else
        {
          ExitState = "FN0000_NEED_PAYEE_OR_DESIG_PAYEE";

          var field3 = GetField(export.DesignatedPayee, "number");

          field3.Error = true;

          var field4 = GetField(export.Payee, "number");

          field4.Error = true;

          return;
        }

        // <<<  Set the Hidden Fields   >>>
        export.HiddenPayee.Number = export.Payee.Number;
        export.HiddenDesignatedPayee.Number = export.DesignatedPayee.Number;
        export.HidBothTypeOfDpIn1.Assign(export.BothTypeOfDpIn1View);

        break;
      case "ADD":
        // This case will create the relationship between two existing  
        // Cse_Persons OR betwwen a Cse_person and a Supported Person
        // All necessary validations are already performed and currency has been
        // obtained on required participating entity types
        local.Payee.Number = export.Payee.Number;
        local.DesignatedPayee.Number = export.DesignatedPayee.Number;
        UseCabCreateDesignatedPayee();

        if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate, local.Max.Date))
        {
          export.BothTypeOfDpIn1View.DiscontinueDate =
            local.InitialisedToZeros.Date;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // <<<  Set the Hidden Fields   >>>
          export.HiddenPayee.Number = export.Payee.Number;
          export.HiddenDesignatedPayee.Number = export.DesignatedPayee.Number;
          export.HidBothTypeOfDpIn1.Assign(export.BothTypeOfDpIn1View);
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_ADD_2";
        }
        else
        {
          var field3 = GetField(export.Payee, "number");

          field3.Error = true;

          var field4 = GetField(export.DesignatedPayee, "number");

          field4.Error = true;
        }

        break;
      case "UPDATE":
        // This case will update the relationship between two existence CSE 
        // Persons.
        if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate,
          local.InitialisedToZeros.Date))
        {
          export.BothTypeOfDpIn1View.DiscontinueDate = local.Max.Date;
        }

        if (Lt(export.BothTypeOfDpIn1View.DiscontinueDate,
          export.BothTypeOfDpIn1View.EffectiveDate))
        {
          ExitState = "DISC_DATE_CANNOT_LT_EFF_DATE";

          var field3 = GetField(export.BothTypeOfDpIn1View, "discontinueDate");

          field3.Error = true;

          var field4 = GetField(export.BothTypeOfDpIn1View, "effectiveDate");

          field4.Error = true;

          return;
        }
        else if (Lt(export.BothTypeOfDpIn1View.DiscontinueDate, Now().Date))
        {
          ExitState = "FN0000_DISCONTINUE_DT_LT_CURRENT";

          var field = GetField(export.BothTypeOfDpIn1View, "discontinueDate");

          field.Error = true;

          return;
        }

        UseCabUpdateDesignatedPayee();

        if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate, local.Max.Date))
        {
          export.BothTypeOfDpIn1View.DiscontinueDate =
            local.InitialisedToZeros.Date;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // <<<  Set the Hidden Fields   >>>
          export.HiddenPayee.Number = export.Payee.Number;
          export.HiddenDesignatedPayee.Number = export.DesignatedPayee.Number;
          export.HidBothTypeOfDpIn1.Assign(export.BothTypeOfDpIn1View);
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_2";
        }
        else
        {
        }

        break;
      case "DELETE":
        // This case will delete the relationship between two CSE Persons.
        // ****************************************************************
        // You can delete a DP if they're effective date is in the future, or 
        // the effective date is today and the create date is today as well.  RK
        // 9/21/98
        // ****************************************************************
        UseCabDeleteDesignatedPayee();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.DesignatedPayee, "number");

          field.Error = true;

          break;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenDesignatedPayee.Number = "";
          export.HiddenPayee.Number = "";
          export.DesignatedPayee.Number = "";
          export.DesignatedPayee.FormattedName = "";
          export.BothTypeOfDpIn1View.EffectiveDate =
            local.InitialisedToZeros.Date;
          export.BothTypeOfDpIn1View.DiscontinueDate =
            local.InitialisedToZeros.Date;
          export.BothTypeOfDpIn1View.CreatedBy = "";
          export.BothTypeOfDpIn1View.LastUpdatedBy = "";
          export.BothTypeOfDpIn1View.Notes = "";
          export.BothTypeOfDpIn1View.SequentialIdentifier = 0;
          export.CsePersonAddress.Street1 = "";
          export.CsePersonAddress.Street2 = "";
          export.CsePersonAddress.City = "";
          export.CsePersonAddress.State = "";
          export.CsePersonAddress.LocationType = "";
          export.CsePersonAddress.ZipCode = "";

          // ****************************************
          // New screen fields added. RK 9/21/98
          // ****************************************
          export.CsePersonAddress.Street3 = "";
          export.CsePersonAddress.Street4 = "";
          export.CsePersonAddress.Country = "";
          export.CsePersonAddress.Province = "";
          export.CsePersonAddress.PostalCode = "";
          export.CsePersonAddress.Zip4 = "";
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else
        {
        }

        return;
      case "DPYL":
        ExitState = "ECO_LNK_TO_LST_DESIG_PAYEE";

        return;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

Test2:

    if (!IsEmpty(export.DesignatedPayee.Number))
    {
      var field1 = GetField(export.DesignatedPayee, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.DesignatedPayeePrompt, "flag");

      field2.Protected = true;
    }

    if (Equal(export.BothTypeOfDpIn1View.DiscontinueDate, local.Max.Date))
    {
      export.BothTypeOfDpIn1View.DiscontinueDate =
        local.InitialisedToZeros.Date;
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
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

  private void UseCabCreateDesignatedPayee()
  {
    var useImport = new CabCreateDesignatedPayee.Import();
    var useExport = new CabCreateDesignatedPayee.Export();

    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      entities.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.Obligation.Assign(entities.Obligation);
    useImport.DataPassedThruFlow.Flag = local.DataReceivedThruFlow.Flag;
    useImport.DesignatedPayee.Number = local.DesignatedPayee.Number;
    useImport.Payee.Number = local.Payee.Number;
    useImport.DpInfoInOneView.Assign(export.BothTypeOfDpIn1View);

    Call(CabCreateDesignatedPayee.Execute, useImport, useExport);

    export.BothTypeOfDpIn1View.Assign(useImport.DpInfoInOneView);
  }

  private void UseCabDeleteDesignatedPayee()
  {
    var useImport = new CabDeleteDesignatedPayee.Import();
    var useExport = new CabDeleteDesignatedPayee.Export();

    useImport.CurrentDate.Date = local.Current.Date;
    useImport.DesignatedPayee.Number = local.DesignatedPayee.Number;
    useImport.Payee.Number = local.Payee.Number;
    useImport.BothTypeOfDpIn1View.Assign(export.BothTypeOfDpIn1View);

    Call(CabDeleteDesignatedPayee.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabUpdateDesignatedPayee()
  {
    var useImport = new CabUpdateDesignatedPayee.Import();
    var useExport = new CabUpdateDesignatedPayee.Export();

    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      entities.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.Obligation.Assign(entities.Obligation);
    useImport.DataPassedThruFlow.Flag = local.DataReceivedThruFlow.Flag;
    useImport.DesignatedPayee.Number = local.DesignatedPayee.Number;
    useImport.Payee.Number = local.Payee.Number;
    useImport.BothTypeOfDpIn1View.Assign(export.BothTypeOfDpIn1View);

    Call(CabUpdateDesignatedPayee.Execute, useImport, useExport);

    export.BothTypeOfDpIn1View.Assign(useImport.BothTypeOfDpIn1View);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnGetActiveCsePersonAddress()
  {
    var useImport = new FnGetActiveCsePersonAddress.Import();
    var useExport = new FnGetActiveCsePersonAddress.Export();

    useImport.AsOfDate.Date = local.Current.Date;
    useImport.CsePersonsWorkSet.Number = export.DesignatedPayee.Number;

    Call(FnGetActiveCsePersonAddress.Execute, useImport, useExport);

    local.ActiveAddressFound.Flag = useExport.ActiveAddressFound.Flag;
    MoveCsePersonAddress(useExport.CsePersonAddress, export.CsePersonAddress);
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

    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    useImport.CsePerson.Number = import.ObligorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ValidCseReturn.Flag = useExport.Ae.Flag;
    export.Payee.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.DesignatedPayee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.DesignatedPayee.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Payee.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.PayeeAr.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.DesignatedPayee.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.DesignatedPayee.Number);
      },
      (db, reader) =>
      {
        entities.DesignatedPayee.Number = db.GetString(reader, 0);
        entities.DesignatedPayee.Type1 = db.GetString(reader, 1);
        entities.DesignatedPayee.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.DesignatedPayee.Populated = true;
        CheckValid<CsePerson>("Type1", entities.DesignatedPayee.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonDesigPayee.Populated);
    entities.DesignatedPayee.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.CsePersonDesigPayee.CsePersNum ?? "");
      },
      (db, reader) =>
      {
        entities.DesignatedPayee.Number = db.GetString(reader, 0);
        entities.DesignatedPayee.Type1 = db.GetString(reader, 1);
        entities.DesignatedPayee.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.DesignatedPayee.Populated = true;
        CheckValid<CsePerson>("Type1", entities.DesignatedPayee.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.PayeeAr.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Payee.Number);
      },
      (db, reader) =>
      {
        entities.PayeeAr.Number = db.GetString(reader, 0);
        entities.PayeeAr.Type1 = db.GetString(reader, 1);
        entities.PayeeAr.OrganizationName = db.GetNullableString(reader, 2);
        entities.PayeeAr.Populated = true;
        CheckValid<CsePerson>("Type1", entities.PayeeAr.Type1);
      });
  }

  private bool ReadCsePersonDesigPayee1()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "csePersoNum", entities.PayeeAr.Number);
        db.SetNullableString(
          command, "csePersNum", entities.DesignatedPayee.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayee2()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.BothTypeOfDpIn1View.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "csePersoNum", entities.PayeeAr.Number);
        db.SetNullableString(
          command, "csePersNum", entities.DesignatedPayee.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayee3()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "csePersoNum", entities.PayeeAr.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayee4()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.BothTypeOfDpIn1View.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "csePersoNum", entities.PayeeAr.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonDesigPayee5()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return ReadEach("ReadCsePersonDesigPayee5",
      (db, command) =>
      {
        db.SetString(command, "csePersoNum", entities.PayeeAr.Number);
        db.SetInt32(
          command, "sequentialId",
          export.BothTypeOfDpIn1View.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonDesigPayee6()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return ReadEach("ReadCsePersonDesigPayee6",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequentialId",
          export.BothTypeOfDpIn1View.SequentialIdentifier);
        db.SetString(command, "csePersoNum", entities.PayeeAr.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;

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
    /// <summary>
    /// A value of TestOnlyView.
    /// </summary>
    [JsonPropertyName("testOnlyView")]
    public CsePersonDesigPayee TestOnlyView
    {
      get => testOnlyView ??= new();
      set => testOnlyView = value;
    }

    /// <summary>
    /// A value of ThruFlowStartingFrom.
    /// </summary>
    [JsonPropertyName("thruFlowStartingFrom")]
    public DateWorkArea ThruFlowStartingFrom
    {
      get => thruFlowStartingFrom ??= new();
      set => thruFlowStartingFrom = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of HidBothTypeOfDpIn1.
    /// </summary>
    [JsonPropertyName("hidBothTypeOfDpIn1")]
    public CsePersonDesigPayee HidBothTypeOfDpIn1
    {
      get => hidBothTypeOfDpIn1 ??= new();
      set => hidBothTypeOfDpIn1 = value;
    }

    /// <summary>
    /// A value of BothTypeOfDpIn1View.
    /// </summary>
    [JsonPropertyName("bothTypeOfDpIn1View")]
    public CsePersonDesigPayee BothTypeOfDpIn1View
    {
      get => bothTypeOfDpIn1View ??= new();
      set => bothTypeOfDpIn1View = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public CsePersonsWorkSet Payor
    {
      get => payor ??= new();
      set => payor = value;
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
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ThruFlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("thruFlowCsePersonsWorkSet")]
    public CsePersonsWorkSet ThruFlowCsePersonsWorkSet
    {
      get => thruFlowCsePersonsWorkSet ??= new();
      set => thruFlowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ThruFlowObligor.
    /// </summary>
    [JsonPropertyName("thruFlowObligor")]
    public CsePerson ThruFlowObligor
    {
      get => thruFlowObligor ??= new();
      set => thruFlowObligor = value;
    }

    /// <summary>
    /// A value of ThruFlowObligationType.
    /// </summary>
    [JsonPropertyName("thruFlowObligationType")]
    public ObligationType ThruFlowObligationType
    {
      get => thruFlowObligationType ??= new();
      set => thruFlowObligationType = value;
    }

    /// <summary>
    /// A value of ThruFlowObligationTransaction.
    /// </summary>
    [JsonPropertyName("thruFlowObligationTransaction")]
    public ObligationTransaction ThruFlowObligationTransaction
    {
      get => thruFlowObligationTransaction ??= new();
      set => thruFlowObligationTransaction = value;
    }

    /// <summary>
    /// A value of ThruFlowObligation.
    /// </summary>
    [JsonPropertyName("thruFlowObligation")]
    public Obligation ThruFlowObligation
    {
      get => thruFlowObligation ??= new();
      set => thruFlowObligation = value;
    }

    /// <summary>
    /// A value of DesignatedPayeePrompt.
    /// </summary>
    [JsonPropertyName("designatedPayeePrompt")]
    public Common DesignatedPayeePrompt
    {
      get => designatedPayeePrompt ??= new();
      set => designatedPayeePrompt = value;
    }

    /// <summary>
    /// A value of ArpayeePrompt.
    /// </summary>
    [JsonPropertyName("arpayeePrompt")]
    public Common ArpayeePrompt
    {
      get => arpayeePrompt ??= new();
      set => arpayeePrompt = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonsWorkSet DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of HiddenDesignatedPayee.
    /// </summary>
    [JsonPropertyName("hiddenDesignatedPayee")]
    public CsePerson HiddenDesignatedPayee
    {
      get => hiddenDesignatedPayee ??= new();
      set => hiddenDesignatedPayee = value;
    }

    /// <summary>
    /// A value of HiddenPayee.
    /// </summary>
    [JsonPropertyName("hiddenPayee")]
    public CsePerson HiddenPayee
    {
      get => hiddenPayee ??= new();
      set => hiddenPayee = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenInitialCommand.
    /// </summary>
    [JsonPropertyName("hiddenInitialCommand")]
    public Common HiddenInitialCommand
    {
      get => hiddenInitialCommand ??= new();
      set => hiddenInitialCommand = value;
    }

    private CsePersonDesigPayee testOnlyView;
    private DateWorkArea thruFlowStartingFrom;
    private CsePersonsWorkSet supported;
    private CsePersonDesigPayee hidBothTypeOfDpIn1;
    private CsePersonDesigPayee bothTypeOfDpIn1View;
    private Obligation obligation;
    private CsePersonAddress csePersonAddress;
    private CsePersonsWorkSet payor;
    private LegalAction legalAction;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePersonsWorkSet thruFlowCsePersonsWorkSet;
    private CsePerson thruFlowObligor;
    private ObligationType thruFlowObligationType;
    private ObligationTransaction thruFlowObligationTransaction;
    private Obligation thruFlowObligation;
    private Common designatedPayeePrompt;
    private Common arpayeePrompt;
    private CsePersonsWorkSet designatedPayee;
    private CsePersonsWorkSet payee;
    private CsePerson hiddenDesignatedPayee;
    private CsePerson hiddenPayee;
    private NextTranInfo hidden;
    private Standard standard;
    private ObligationType obligationType;
    private CsePerson obligorCsePerson;
    private Common hiddenInitialCommand;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PassedDesignatedPayee.
    /// </summary>
    [JsonPropertyName("passedDesignatedPayee")]
    public CsePersonsWorkSet PassedDesignatedPayee
    {
      get => passedDesignatedPayee ??= new();
      set => passedDesignatedPayee = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of ThruFlowStartingFrom.
    /// </summary>
    [JsonPropertyName("thruFlowStartingFrom")]
    public DateWorkArea ThruFlowStartingFrom
    {
      get => thruFlowStartingFrom ??= new();
      set => thruFlowStartingFrom = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of HidBothTypeOfDpIn1.
    /// </summary>
    [JsonPropertyName("hidBothTypeOfDpIn1")]
    public CsePersonDesigPayee HidBothTypeOfDpIn1
    {
      get => hidBothTypeOfDpIn1 ??= new();
      set => hidBothTypeOfDpIn1 = value;
    }

    /// <summary>
    /// A value of BothTypeOfDpIn1View.
    /// </summary>
    [JsonPropertyName("bothTypeOfDpIn1View")]
    public CsePersonDesigPayee BothTypeOfDpIn1View
    {
      get => bothTypeOfDpIn1View ??= new();
      set => bothTypeOfDpIn1View = value;
    }

    /// <summary>
    /// A value of ThruFlowObligationTransaction.
    /// </summary>
    [JsonPropertyName("thruFlowObligationTransaction")]
    public ObligationTransaction ThruFlowObligationTransaction
    {
      get => thruFlowObligationTransaction ??= new();
      set => thruFlowObligationTransaction = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public CsePersonsWorkSet Payor
    {
      get => payor ??= new();
      set => payor = value;
    }

    /// <summary>
    /// A value of ThruFlowCsePerson.
    /// </summary>
    [JsonPropertyName("thruFlowCsePerson")]
    public CsePerson ThruFlowCsePerson
    {
      get => thruFlowCsePerson ??= new();
      set => thruFlowCsePerson = value;
    }

    /// <summary>
    /// A value of ThruFlowWorkArea.
    /// </summary>
    [JsonPropertyName("thruFlowWorkArea")]
    public WorkArea ThruFlowWorkArea
    {
      get => thruFlowWorkArea ??= new();
      set => thruFlowWorkArea = value;
    }

    /// <summary>
    /// A value of ThruFlowObligor.
    /// </summary>
    [JsonPropertyName("thruFlowObligor")]
    public CsePerson ThruFlowObligor
    {
      get => thruFlowObligor ??= new();
      set => thruFlowObligor = value;
    }

    /// <summary>
    /// A value of ThruFlowObligationType.
    /// </summary>
    [JsonPropertyName("thruFlowObligationType")]
    public ObligationType ThruFlowObligationType
    {
      get => thruFlowObligationType ??= new();
      set => thruFlowObligationType = value;
    }

    /// <summary>
    /// A value of ThruFlowObligation.
    /// </summary>
    [JsonPropertyName("thruFlowObligation")]
    public Obligation ThruFlowObligation
    {
      get => thruFlowObligation ??= new();
      set => thruFlowObligation = value;
    }

    /// <summary>
    /// A value of ArDesignSearch.
    /// </summary>
    [JsonPropertyName("arDesignSearch")]
    public Common ArDesignSearch
    {
      get => arDesignSearch ??= new();
      set => arDesignSearch = value;
    }

    /// <summary>
    /// A value of DesignatedPayeePrompt.
    /// </summary>
    [JsonPropertyName("designatedPayeePrompt")]
    public Common DesignatedPayeePrompt
    {
      get => designatedPayeePrompt ??= new();
      set => designatedPayeePrompt = value;
    }

    /// <summary>
    /// A value of ArpayeePrompt.
    /// </summary>
    [JsonPropertyName("arpayeePrompt")]
    public Common ArpayeePrompt
    {
      get => arpayeePrompt ??= new();
      set => arpayeePrompt = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonsWorkSet DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of HiddenDesignatedPayee.
    /// </summary>
    [JsonPropertyName("hiddenDesignatedPayee")]
    public CsePerson HiddenDesignatedPayee
    {
      get => hiddenDesignatedPayee ??= new();
      set => hiddenDesignatedPayee = value;
    }

    /// <summary>
    /// A value of HiddenPayee.
    /// </summary>
    [JsonPropertyName("hiddenPayee")]
    public CsePerson HiddenPayee
    {
      get => hiddenPayee ??= new();
      set => hiddenPayee = value;
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
    /// A value of HiddenInitialCommand.
    /// </summary>
    [JsonPropertyName("hiddenInitialCommand")]
    public Common HiddenInitialCommand
    {
      get => hiddenInitialCommand ??= new();
      set => hiddenInitialCommand = value;
    }

    private CsePersonsWorkSet passedDesignatedPayee;
    private CsePersonsWorkSet selected;
    private DateWorkArea thruFlowStartingFrom;
    private CsePersonsWorkSet supported;
    private CsePersonDesigPayee hidBothTypeOfDpIn1;
    private CsePersonDesigPayee bothTypeOfDpIn1View;
    private ObligationTransaction thruFlowObligationTransaction;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAddress csePersonAddress;
    private LegalAction legalAction;
    private CsePersonsWorkSet payor;
    private CsePerson thruFlowCsePerson;
    private WorkArea thruFlowWorkArea;
    private CsePerson thruFlowObligor;
    private ObligationType thruFlowObligationType;
    private Obligation thruFlowObligation;
    private Common arDesignSearch;
    private Common designatedPayeePrompt;
    private Common arpayeePrompt;
    private CsePersonsWorkSet designatedPayee;
    private CsePersonsWorkSet payee;
    private CsePerson hiddenDesignatedPayee;
    private CsePerson hiddenPayee;
    private NextTranInfo hidden;
    private Standard standard;
    private Common hiddenInitialCommand;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ConvertTimestampToDate.
    /// </summary>
    [JsonPropertyName("convertTimestampToDate")]
    public DateWorkArea ConvertTimestampToDate
    {
      get => convertTimestampToDate ??= new();
      set => convertTimestampToDate = value;
    }

    /// <summary>
    /// A value of CaseRelatedPerson.
    /// </summary>
    [JsonPropertyName("caseRelatedPerson")]
    public Common CaseRelatedPerson
    {
      get => caseRelatedPerson ??= new();
      set => caseRelatedPerson = value;
    }

    /// <summary>
    /// A value of ActiveAddressFound.
    /// </summary>
    [JsonPropertyName("activeAddressFound")]
    public Common ActiveAddressFound
    {
      get => activeAddressFound ??= new();
      set => activeAddressFound = value;
    }

    /// <summary>
    /// A value of OtherDesigPayee.
    /// </summary>
    [JsonPropertyName("otherDesigPayee")]
    public Common OtherDesigPayee
    {
      get => otherDesigPayee ??= new();
      set => otherDesigPayee = value;
    }

    /// <summary>
    /// A value of DpForEffDateFound.
    /// </summary>
    [JsonPropertyName("dpForEffDateFound")]
    public Common DpForEffDateFound
    {
      get => dpForEffDateFound ??= new();
      set => dpForEffDateFound = value;
    }

    /// <summary>
    /// A value of OtherDesigPFound.
    /// </summary>
    [JsonPropertyName("otherDesigPFound")]
    public Common OtherDesigPFound
    {
      get => otherDesigPFound ??= new();
      set => otherDesigPFound = value;
    }

    /// <summary>
    /// A value of DpIsAnApForArCase.
    /// </summary>
    [JsonPropertyName("dpIsAnApForArCase")]
    public Common DpIsAnApForArCase
    {
      get => dpIsAnApForArCase ??= new();
      set => dpIsAnApForArCase = value;
    }

    /// <summary>
    /// A value of NoOfDesigPayeesFound.
    /// </summary>
    [JsonPropertyName("noOfDesigPayeesFound")]
    public Common NoOfDesigPayeesFound
    {
      get => noOfDesigPayeesFound ??= new();
      set => noOfDesigPayeesFound = value;
    }

    /// <summary>
    /// A value of InfoPassedFromOblig.
    /// </summary>
    [JsonPropertyName("infoPassedFromOblig")]
    public Common InfoPassedFromOblig
    {
      get => infoPassedFromOblig ??= new();
      set => infoPassedFromOblig = value;
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
    /// A value of ActiveDesigPayeeFound.
    /// </summary>
    [JsonPropertyName("activeDesigPayeeFound")]
    public Common ActiveDesigPayeeFound
    {
      get => activeDesigPayeeFound ??= new();
      set => activeDesigPayeeFound = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    /// <summary>
    /// A value of DataReceivedThruFlow.
    /// </summary>
    [JsonPropertyName("dataReceivedThruFlow")]
    public Common DataReceivedThruFlow
    {
      get => dataReceivedThruFlow ??= new();
      set => dataReceivedThruFlow = value;
    }

    /// <summary>
    /// A value of ToFlow.
    /// </summary>
    [JsonPropertyName("toFlow")]
    public WorkArea ToFlow
    {
      get => toFlow ??= new();
      set => toFlow = value;
    }

    /// <summary>
    /// A value of ValidCseReturn.
    /// </summary>
    [JsonPropertyName("validCseReturn")]
    public Common ValidCseReturn
    {
      get => validCseReturn ??= new();
      set => validCseReturn = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private DateWorkArea convertTimestampToDate;
    private Common caseRelatedPerson;
    private Common activeAddressFound;
    private Common otherDesigPayee;
    private Common dpForEffDateFound;
    private Common otherDesigPFound;
    private Common dpIsAnApForArCase;
    private Common noOfDesigPayeesFound;
    private Common infoPassedFromOblig;
    private DateWorkArea current;
    private Common activeDesigPayeeFound;
    private TextWorkArea leftPadding;
    private Common dataReceivedThruFlow;
    private WorkArea toFlow;
    private Common validCseReturn;
    private CsePerson designatedPayee;
    private CsePerson payee;
    private DateWorkArea max;
    private DateWorkArea initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncomingInterstateRequest.
    /// </summary>
    [JsonPropertyName("incomingInterstateRequest")]
    public InterstateRequest IncomingInterstateRequest
    {
      get => incomingInterstateRequest ??= new();
      set => incomingInterstateRequest = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of PayeeAr.
    /// </summary>
    [JsonPropertyName("payeeAr")]
    public CsePerson PayeeAr
    {
      get => payeeAr ??= new();
      set => payeeAr = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    private InterstateRequest incomingInterstateRequest;
    private CaseRole ap;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePersonDesigPayee csePersonDesigPayee;
    private ObligationTransaction obligationTransaction;
    private CsePerson designatedPayee;
    private CsePerson payeeAr;
    private CsePersonAddress csePersonAddress;
    private CsePersonAccount obligor1;
    private ObligationType obligationType;
    private Obligation obligation;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private CsePerson csePerson;
    private CsePerson obligor2;
  }
#endregion
}

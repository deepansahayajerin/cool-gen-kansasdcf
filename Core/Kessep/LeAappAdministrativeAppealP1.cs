// Program: LE_AAPP_ADMINISTRATIVE_APPEAL_P1, ID: 372605064, model: 746.
// Short name: SWEAAPPP
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
/// A program: LE_AAPP_ADMINISTRATIVE_APPEAL_P1.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This procstep maintains admin appeals
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAappAdministrativeAppealP1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AAPP_ADMINISTRATIVE_APPEAL_P1 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAappAdministrativeAppealP1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAappAdministrativeAppealP1.
  /// </summary>
  public LeAappAdministrativeAppealP1(IContext context, Import import,
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
    // --------------------------------------------------------------------
    // Every initial development and change to that development
    // needs to be documented.
    // --------------------------------------------------------------------
    //   Date	   Developer	Request #	Description
    // --------------------------------------------------------------------
    // 05-24-95   S. Benton			Initial development
    // 09-09-96   govind			Received Date was made optional
    // 					per Jan's request 09-03-1996
    // 06-09-97   M. D. Wheaton                Removed datenum function
    // 08-25-97  JF.Caillouet  H00025466	PF20 ASIN-display 1st error.
    // 09-09-97  JF.Caillouet  Same		Display after an ADD.
    // 10-31-97   govind	H00030782	Fixed to allow flowing to ASIN
    // 					on immediately after successful
    // 					retaaps.
    // 11/30/98   M. Ramirez			Modified print function.
    // 01/14/99  J. Katz			Modified logic as described in
    // 					signed Screen Assessment and
    // 					Correction form dated 01/06/99.
    // 01/28/99  J. Katz			Added Review State field to screen.
    // 					Modified logic to maintain the new
    // 					Review State attribute.
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ----------------------------------------------------------
    // Move Imports to Exports
    // ----------------------------------------------------------*
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.SsnWorkArea.Assign(import.SsnWorkArea);
    MoveAdministrativeAction(import.AdministrativeAction,
      export.AdministrativeAction);
    export.AdminActionTypePrompt.SelectChar =
      import.AdminActionTypePrompt.SelectChar;
    export.ActionTaken.Date = import.ActionTaken.Date;
    export.AdministrativeAppeal.Assign(import.AdministrativeAppeal);
    export.AppealTypePrompt.SelectChar = import.AppealTypePrompt.SelectChar;
    export.AdminRvwStatePrompt.SelectChar =
      import.AdminReviewStatePrompt.SelectChar;
    export.Code.CodeName = import.Code.CodeName;
    MoveDocument(import.Document, export.Document);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ----------------------------------------------------------
    // Move Hidden Imports to Hidden Exports.
    // ----------------------------------------------------------
    export.HiddenCsePersonsWorkSet.Assign(import.HiddenCsePersonsWorkSet);
    export.HiddenSsnWorkArea.Assign(import.HiddenSsnWorkArea);
    MoveAdministrativeAction(import.HiddenAdministrativeAction,
      export.HiddenAdministrativeAction);
    export.HiddenActionTaken.Date = import.HiddenActionTaken.Date;
    export.HiddenAdministrativeAppeal.Assign(import.HiddenAdministrativeAppeal);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // ---------------------------------------------
    // Next tran logic.
    // ---------------------------------------------
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // -------------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base.  Set command to initial command
      // required or ESCAPE.
      // -------------------------------------------------------------
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        // --- nexttran from HIST or MONA
        if (ReadAdministrativeAppeal())
        {
          export.AdministrativeAppeal.Assign(entities.AdministrativeAppeal);

          if (ReadCsePerson())
          {
            export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          }
        }
      }
      else
      {
        export.CsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ----------------------------------------------------------
      // User is going out of this screen to another.
      // Set up local next_tran_info for saving the current values
      // for the next screen.
      // ----------------------------------------------------------
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut1();

      return;
    }

    // mjr
    // --------------------------------------
    // 12/01/1998
    // Security logic.
    // Changed security to test CRUD actions.
    // ---------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // --------------------------------------------------------------
    // If the command is Add or Display, validate the CSE Person
    // Number and SSN if either has been changed.
    // --------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number) && export
        .SsnWorkArea.SsnNumPart1 == 0 && export.SsnWorkArea.SsnNumPart2 == 0
        && export.SsnWorkArea.SsnNumPart3 == 0)
      {
        export.CsePersonsWorkSet.FormattedName = "";
        export.HiddenCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
        export.HiddenSsnWorkArea.Assign(local.ClearSsnWorkArea);
        MoveAdministrativeAction(local.ClearAdministrativeAction,
          export.HiddenAdministrativeAction);
        export.HiddenActionTaken.Date = local.ClearActionTaken.Date;
        export.HiddenAdministrativeAppeal.
          Assign(local.ClearAdministrativeAppeal);

        var field1 = GetField(export.CsePersonsWorkSet, "number");

        field1.Error = true;

        var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

        field2.Error = true;

        var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

        field3.Error = true;

        var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

        field4.Error = true;

        if (Equal(global.Command, "DISPLAY"))
        {
          MoveAdministrativeAction(local.ClearAdministrativeAction,
            export.AdministrativeAction);
          export.ActionTaken.Date = local.ClearActionTaken.Date;
          export.AdministrativeAppeal.Assign(local.ClearAdministrativeAppeal);
        }

        ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

        return;
      }

      if (!Equal(export.CsePersonsWorkSet.Number,
        export.HiddenCsePersonsWorkSet.Number))
      {
        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson();

          if (IsEmpty(export.CsePersonsWorkSet.Number))
          {
            export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
            export.CsePersonsWorkSet.FormattedName = "";
            export.HiddenCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
            MoveSsnWorkArea3(local.ClearSsnWorkArea, export.SsnWorkArea);
            export.HiddenSsnWorkArea.Assign(local.ClearSsnWorkArea);

            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }

          if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
          {
            export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
            UseCabSsnConvertTextToNum();
          }

          export.HiddenCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
          export.HiddenSsnWorkArea.Assign(export.SsnWorkArea);
        }
      }
      else if (export.SsnWorkArea.SsnNumPart1 != export
        .HiddenSsnWorkArea.SsnNumPart1 || export.SsnWorkArea.SsnNumPart2 != export
        .HiddenSsnWorkArea.SsnNumPart2 || export.SsnWorkArea.SsnNumPart3 != export
        .HiddenSsnWorkArea.SsnNumPart3)
      {
        export.SsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText();
        export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          local.SearchOption.Flag = "1";
          UseCabMatchCsePerson();

          local.Local1.Index = 0;
          local.Local1.CheckSize();

          if (!IsEmpty(local.Local1.Item.Detail.Number))
          {
            MoveCsePersonsWorkSet(local.Local1.Item.Detail,
              export.CsePersonsWorkSet);
            UseSiFormatCsePersonName();
            export.CsePersonsWorkSet.FormattedName =
              local.Returned.FormattedName;
            export.HiddenCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
            export.HiddenSsnWorkArea.Assign(export.SsnWorkArea);
          }
          else
          {
            export.CsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
            export.HiddenCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
            export.HiddenSsnWorkArea.Assign(local.ClearSsnWorkArea);

            var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

            field1.Error = true;

            var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

            field2.Error = true;

            var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

            field3.Error = true;

            if (Equal(global.Command, "DISPLAY"))
            {
              MoveAdministrativeAction(local.ClearAdministrativeAction,
                export.AdministrativeAction);
              export.ActionTaken.Date = local.ClearActionTaken.Date;
              export.AdministrativeAppeal.
                Assign(local.ClearAdministrativeAppeal);
            }

            ExitState = "CSE_PERSON_NF";

            return;
          }
        }
        else
        {
          export.HiddenCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
          export.HiddenSsnWorkArea.Assign(local.ClearSsnWorkArea);
          MoveAdministrativeAction(local.ClearAdministrativeAction,
            export.HiddenAdministrativeAction);
          export.HiddenActionTaken.Date = local.ClearActionTaken.Date;
          export.HiddenAdministrativeAppeal.Assign(
            local.ClearAdministrativeAppeal);

          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field2.Error = true;

          var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field3.Error = true;

          var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field4.Error = true;

          ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

          return;
        }
      }
      else
      {
        // ----------------------------------------------------
        // CSE Person Number and SSN have not changed.
        // No further processing of CSE Person required.
        // ----------------------------------------------------
      }

      if (!Equal(export.AdministrativeAppeal.Number,
        export.HiddenAdministrativeAppeal.Number))
      {
        export.AdministrativeAppeal.Identifier = 0;
      }
    }

    // ----------------------------------------------------------
    // Verify that a display has been performed before the Upd,
    // Del, Page2 (AAP2), ASIN, or Print can take place.
    // ----------------------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "AAP2") || Equal(global.Command, "AAPS") || Equal
      (global.Command, "AAAD") || Equal(global.Command, "AHEA") || Equal
      (global.Command, "POST") || Equal(global.Command, "ASIN") || Equal
      (global.Command, "PRINT"))
    {
      if (!Equal(export.CsePersonsWorkSet.Number,
        export.HiddenCsePersonsWorkSet.Number) || IsEmpty
        (export.HiddenCsePersonsWorkSet.Number))
      {
        export.HiddenCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
        export.HiddenSsnWorkArea.Assign(local.ClearSsnWorkArea);

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
      }

      if (export.SsnWorkArea.SsnNumPart1 != export
        .HiddenSsnWorkArea.SsnNumPart1 || export.SsnWorkArea.SsnNumPart2 != export
        .HiddenSsnWorkArea.SsnNumPart2 || export.SsnWorkArea.SsnNumPart3 != export
        .HiddenSsnWorkArea.SsnNumPart3)
      {
        export.HiddenCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
        export.HiddenSsnWorkArea.Assign(local.ClearSsnWorkArea);

        var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

        field1.Error = true;

        var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

        field2.Error = true;

        var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

        field3.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ----------------------------------------------------------
    // Perform validations common to both CREATEs and UPDATEs.
    // ----------------------------------------------------------
    if (Equal(global.Command, "ADD"))
    {
      if (!IsEmpty(export.AdministrativeAction.Type1) || Lt
        (local.NullDateWorkArea.Date, export.ActionTaken.Date))
      {
        if (IsEmpty(export.AdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (Equal(export.ActionTaken.Date, local.NullDateWorkArea.Date))
        {
          var field = GetField(export.ActionTaken, "date");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "PRINT") || Equal(global.Command, "ASIN"))
    {
      // *********************************************
      // Required fields  EDIT LOGIC
      // *********************************************
      if (IsEmpty(export.AdministrativeAppeal.Type1))
      {
        var field = GetField(export.AdministrativeAppeal, "type1");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (Equal(export.AdministrativeAppeal.RequestDate,
        local.NullDateWorkArea.Date))
      {
        var field = GetField(export.AdministrativeAppeal, "requestDate");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      // -------------------------------------------------------------
      // Admin Appeal Received Date was changed to optional from
      // mandatory per Jan's request 09-03-1996. So the
      // corresponding IF statement was removed from here.
      // -------------------------------------------------------------
      if (IsEmpty(export.AdministrativeAppeal.Reason))
      {
        var field = GetField(export.AdministrativeAppeal, "reason");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(export.AdministrativeAppeal.Respondent))
      {
        var field = GetField(export.AdministrativeAppeal, "respondent");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // *********************************************
      // Validate Administrative Appeal Type
      // *********************************************
      local.ToBeValidatedCode.CodeName = "ADMINISTRATIVE APPEAL TYPE";
      local.ToBeValidatedCodeValue.Cdvalue = export.AdministrativeAppeal.Type1;
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        var field = GetField(export.AdministrativeAppeal, "type1");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

        return;
      }

      // *********************************************
      // Validate Administrative Review State
      // *********************************************
      if (!IsEmpty(export.AdministrativeAppeal.AdminReviewState))
      {
        if (!Equal(export.AdministrativeAppeal.AdminReviewState,
          export.HiddenAdministrativeAppeal.AdminReviewState))
        {
          local.ToBeValidatedCode.CodeName = "STATE CODE";
          local.ToBeValidatedCodeValue.Cdvalue =
            export.AdministrativeAppeal.AdminReviewState;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            export.HiddenAdministrativeAppeal.AdminReviewState = "";

            var field =
              GetField(export.AdministrativeAppeal, "adminReviewState");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            return;
          }
          else
          {
            export.HiddenAdministrativeAppeal.AdminReviewState =
              export.AdministrativeAppeal.AdminReviewState;
          }
        }
      }
      else
      {
        export.AdministrativeAppeal.AdminReviewState = "KS";
      }

      // *********************************************
      // Date to Current Date  EDIT LOGIC
      // *********************************************
      if (Lt(Now().Date, export.AdministrativeAppeal.RequestDate))
      {
        var field = GetField(export.AdministrativeAppeal, "requestDate");

        field.Error = true;

        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }

      if (Lt(export.AdministrativeAppeal.RequestDate, export.ActionTaken.Date))
      {
        var field = GetField(export.AdministrativeAppeal, "requestDate");

        field.Error = true;

        ExitState = "LE0000_DATE_EARLIER_THAN_TAKN_DT";

        return;
      }

      if (Lt(Now().Date, export.AdministrativeAppeal.ReceivedDate))
      {
        var field = GetField(export.AdministrativeAppeal, "receivedDate");

        field.Error = true;

        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }

      if (Lt(local.NullDateWorkArea.Date,
        export.AdministrativeAppeal.ReceivedDate))
      {
        if (Lt(export.AdministrativeAppeal.ReceivedDate, export.ActionTaken.Date))
          
        {
          var field = GetField(export.AdministrativeAppeal, "receivedDate");

          field.Error = true;

          ExitState = "LE0000_DATE_EARLIER_THAN_TAKN_DT";

          return;
        }

        if (Lt(export.AdministrativeAppeal.ReceivedDate,
          export.AdministrativeAppeal.RequestDate))
        {
          var field = GetField(export.AdministrativeAppeal, "receivedDate");

          field.Error = true;

          ExitState = "LE0000_AAPP_RECVD_DATE_LT_REQ_DT";

          return;
        }
      }

      if (Lt(local.NullDateWorkArea.Date, export.AdministrativeAppeal.Date))
      {
        if (Lt(Now().Date, export.AdministrativeAppeal.Date))
        {
          var field = GetField(export.AdministrativeAppeal, "date");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (Lt(export.AdministrativeAppeal.Date, export.ActionTaken.Date))
        {
          ExitState = "LE0000_DATE_EARLIER_THAN_TAKN_DT";

          var field = GetField(export.AdministrativeAppeal, "date");

          field.Error = true;

          return;
        }
      }

      if (Lt(local.NullDateWorkArea.Date,
        export.AdministrativeAppeal.AdminOrderDate))
      {
        if (Lt(Now().Date, export.AdministrativeAppeal.AdminOrderDate))
        {
          var field = GetField(export.AdministrativeAppeal, "adminOrderDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (Lt(export.AdministrativeAppeal.AdminOrderDate,
          export.ActionTaken.Date))
        {
          var field = GetField(export.AdministrativeAppeal, "adminOrderDate");

          field.Error = true;

          ExitState = "LE0000_DATE_EARLIER_THAN_TAKN_DT";

          return;
        }

        if (Lt(export.AdministrativeAppeal.AdminOrderDate,
          export.AdministrativeAppeal.RequestDate) || Lt
          (export.AdministrativeAppeal.AdminOrderDate,
          export.AdministrativeAppeal.ReceivedDate) || Lt
          (export.AdministrativeAppeal.AdminOrderDate,
          export.AdministrativeAppeal.Date))
        {
          var field = GetField(export.AdministrativeAppeal, "adminOrderDate");

          field.Error = true;

          ExitState = "LE0000_AAPP_ORD_DT_LT_OTH_DTS";

          return;
        }
      }

      // -----------------------------------------------------------
      // Cross Field  EDIT LOGIC
      // Administrative Action Type FDSO requires Appeal Type F.
      // Administrative Action Type SDSO requires Appeal Type S.
      // -----------------------------------------------------------
      if (Equal(export.AdministrativeAction.Type1, "FDSO") && AsChar
        (export.AdministrativeAppeal.Type1) != 'F' || AsChar
        (export.AdministrativeAppeal.Type1) == 'F' && !
        Equal(export.AdministrativeAction.Type1, "FDSO"))
      {
        var field = GetField(export.AdministrativeAppeal, "type1");

        field.Error = true;

        ExitState = "LE0000_INV_APP_TYP_FOR_ADM_ACT";
      }

      if (Equal(export.AdministrativeAction.Type1, "SDSO") && AsChar
        (export.AdministrativeAppeal.Type1) != 'S' || AsChar
        (export.AdministrativeAppeal.Type1) == 'S' && !
        Equal(export.AdministrativeAction.Type1, "SDSO"))
      {
        var field = GetField(export.AdministrativeAppeal, "type1");

        field.Error = true;

        ExitState = "LE0000_INV_APP_TYP_FOR_ADM_ACT";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // -----------------------------------------------------------
      // If either the Appellant First Name, Last Name, or
      // Relationship is entered, all three fields must be entered.
      // -----------------------------------------------------------
      if (!IsEmpty(export.AdministrativeAppeal.AppellantLastName) || !
        IsEmpty(export.AdministrativeAppeal.AppellantFirstName) || !
        IsEmpty(export.AdministrativeAppeal.AppellantRelationship))
      {
        if (IsEmpty(export.AdministrativeAppeal.AppellantLastName))
        {
          var field =
            GetField(export.AdministrativeAppeal, "appellantLastName");

          field.Error = true;

          ExitState = "LE0000_APPELLANT_NAME_X_EDIT_REQ";
        }

        if (IsEmpty(export.AdministrativeAppeal.AppellantFirstName))
        {
          var field =
            GetField(export.AdministrativeAppeal, "appellantFirstName");

          field.Error = true;

          ExitState = "LE0000_APPELLANT_NAME_X_EDIT_REQ";
        }

        if (IsEmpty(export.AdministrativeAppeal.AppellantRelationship))
        {
          var field =
            GetField(export.AdministrativeAppeal, "appellantRelationship");

          field.Error = true;

          ExitState = "LE0000_APPELLANT_NAME_X_EDIT_REQ";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -----------------------------------------------------------
      // If either the Withdrawal Date or Withdrawal Reason fields
      // are entered, both must be entered.
      // -----------------------------------------------------------
      if (Lt(local.NullDateWorkArea.Date,
        export.AdministrativeAppeal.WithdrawDate) || !
        IsEmpty(export.AdministrativeAppeal.WithdrawReason))
      {
        if (Equal(export.AdministrativeAppeal.WithdrawDate,
          local.NullDateWorkArea.Date))
        {
          var field = GetField(export.AdministrativeAppeal, "withdrawDate");

          field.Error = true;

          ExitState = "WITHDRAW_CROSS_EDIT_REQ";
        }

        if (IsEmpty(export.AdministrativeAppeal.WithdrawReason))
        {
          var field = GetField(export.AdministrativeAppeal, "withdrawReason");

          field.Error = true;

          ExitState = "WITHDRAW_CROSS_EDIT_REQ";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -----------------------------------------------------------
      // The Withdrawal Date cannot be a future date and it cannot
      // be prior to the action taken date or the request date.
      // -----------------------------------------------------------
      if (Lt(local.NullDateWorkArea.Date,
        export.AdministrativeAppeal.WithdrawDate))
      {
        if (Lt(Now().Date, export.AdministrativeAppeal.WithdrawDate))
        {
          var field = GetField(export.AdministrativeAppeal, "withdrawDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (Lt(export.AdministrativeAppeal.WithdrawDate, export.ActionTaken.Date)
          || Lt
          (export.AdministrativeAppeal.WithdrawDate,
          export.AdministrativeAppeal.RequestDate))
        {
          var field = GetField(export.AdministrativeAppeal, "withdrawDate");

          field.Error = true;

          ExitState = "LE0000_WTHDRW_DT_LT_ACT_OR_REQ";

          return;
        }
      }
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    local.DisplayAfterAdd.Flag = "";

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ---------------------------------------------------------
        // PF2  Display logic handled after main case of command.
        // ---------------------------------------------------------
        break;
      case "LIST":
        // ---------------------------------------------------------
        // PF4  List
        // This command allows the user to link to a selection list
        // and retrieve the appropriate value, not losing any of the
        // data already entered.
        // ---------------------------------------------------------
        local.NumPromptsSelected.Count = 0;

        switch(AsChar(export.AdminActionTypePrompt.SelectChar))
        {
          case 'S':
            // ------------------------------------------------------------
            // Link to ADAA - Administrative Actions Available.
            // ------------------------------------------------------------
            ++local.NumPromptsSelected.Count;

            var field4 = GetField(export.AdminActionTypePrompt, "selectChar");

            field4.Error = true;

            ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";

            break;
          case ' ':
            break;
          default:
            var field5 = GetField(export.AdminActionTypePrompt, "selectChar");

            field5.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.AppealTypePrompt.SelectChar))
        {
          case 'S':
            // ------------------------------------------------------------
            // Link to CDVL - Code Value List for the Administrative
            // Appeal Type code name.
            // ------------------------------------------------------------
            ++local.NumPromptsSelected.Count;

            var field4 = GetField(export.AppealTypePrompt, "selectChar");

            field4.Error = true;

            export.DisplayActiveCasesOnly.Flag = "Y";
            export.Code.CodeName = "ADMINISTRATIVE APPEAL TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          case ' ':
            break;
          default:
            var field5 = GetField(export.AppealTypePrompt, "selectChar");

            field5.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.AdminRvwStatePrompt.SelectChar))
        {
          case 'S':
            // ------------------------------------------------------------
            // Link to CDVL - Code Value List for the State Code code name.
            // ------------------------------------------------------------
            ++local.NumPromptsSelected.Count;

            var field4 = GetField(export.AdminRvwStatePrompt, "selectChar");

            field4.Error = true;

            export.Code.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          case ' ':
            break;
          default:
            var field5 = GetField(export.AdminRvwStatePrompt, "selectChar");

            field5.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        if (local.NumPromptsSelected.Count == 0)
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else if (local.NumPromptsSelected.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        return;
      case "RTLIST":
        // -------------------------------------------------------------
        // Returned from ADAA list of Administrative Actions Available.
        // -------------------------------------------------------------
        export.AdminActionTypePrompt.SelectChar = "";

        if (!IsEmpty(import.SelectedAdministrativeAction.Type1))
        {
          MoveAdministrativeAction(import.SelectedAdministrativeAction,
            export.AdministrativeAction);
        }

        var field1 = GetField(export.ActionTaken, "date");

        field1.Protected = false;
        field1.Focused = true;

        break;
      case "RLCVAL":
        // -------------------------------------------------------------
        // Returned from CDVL list of Administrative Appeal Types.
        // Move the value to the export view.
        // -------------------------------------------------------------
        if (AsChar(export.AppealTypePrompt.SelectChar) == 'S')
        {
          export.AppealTypePrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.AdministrativeAppeal.Type1 =
              import.SelectedCodeValue.Cdvalue;
          }

          var field = GetField(export.AdministrativeAppeal, "requestDate");

          field.Protected = false;
          field.Focused = true;
        }
        else if (AsChar(export.AdminRvwStatePrompt.SelectChar) == 'S')
        {
          export.AdminRvwStatePrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.AdministrativeAppeal.AdminReviewState =
              import.SelectedCodeValue.Cdvalue;
          }

          var field =
            GetField(export.AdministrativeAppeal, "appellantLastName");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "ADD":
        // -----------------------------------------------
        // PF5  Add
        // -----------------------------------------------
        if (Equal(export.AdministrativeAction.Type1, "COAG"))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";

          return;
        }

        // ------------------------------------------------------------
        // Validate Administrative Action Type
        // Add logic to skip validation if the Type is spaces or has
        // already been validated.  JLK  01/13/99
        // ------------------------------------------------------------
        if (!Equal(export.AdministrativeAction.Type1,
          export.HiddenAdministrativeAction.Type1))
        {
          MoveAdministrativeAction(local.ClearAdministrativeAction,
            export.HiddenAdministrativeAction);

          if (!IsEmpty(export.AdministrativeAction.Type1))
          {
            UseCabReadAdminAction();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              MoveAdministrativeAction(export.AdministrativeAction,
                export.HiddenAdministrativeAction);
            }
            else
            {
              var field = GetField(export.AdministrativeAction, "type1");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

              return;
            }
          }
        }

        UseReceiveAdministrativeAppeal();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_AE"))
        {
          return;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }
        else
        {
          return;
        }

        break;
      case "UPDATE":
        // -----------------------------------------------
        // PF6  Update
        // Edit key fields for invalid change.
        // -----------------------------------------------
        if (!Equal(import.AdministrativeAction.Type1,
          import.HiddenAdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.ActionTaken.Date, import.HiddenActionTaken.Date))
        {
          var field = GetField(export.ActionTaken, "date");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -----------------------------------------------
        // Update appeal.
        // -----------------------------------------------
        UseResolveAdministrativeAppeal();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          UseEabRollbackCics();

          return;
        }

        break;
      case "DELETE":
        // -----------------------------------------------
        // PF10  Delete
        // -----------------------------------------------
        ExitState = "ECO_LNK_TO_ADMIN_APPEAL_TWO";

        break;
      case "AAP2":
        // -----------------------------------------------
        // PF15  Page 2
        // -----------------------------------------------
        ExitState = "ECO_LNK_TO_AAP2";

        break;
      case "AAPS":
        // -----------------------------------------------
        // PF16  AAPS
        // -----------------------------------------------
        ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";

        return;
      case "AAAD":
        // -----------------------------------------------
        // PF17  AAAD
        // -----------------------------------------------
        ExitState = "ECO_XFR_TO_ADMIN_APPEAL_ADDRESS";

        break;
      case "AHEA":
        // -----------------------------------------------
        // PF18  AHEA
        // -----------------------------------------------
        ExitState = "ECO_XFR_TO_ADMIN_APPEAL_HEARING";

        break;
      case "POST":
        // -----------------------------------------------
        // PF19  Post
        // -----------------------------------------------
        ExitState = "ECO_XFR_TO_POSITION_STATEMENT";

        break;
      case "ASIN":
        // -----------------------------------------------
        // PF20  ASIN
        // -----------------------------------------------
        if (!Equal(import.AdministrativeAction.Type1,
          import.HiddenAdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.ActionTaken.Date, import.HiddenActionTaken.Date))
        {
          var field = GetField(export.ActionTaken, "date");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.Asin.Number = export.CsePersonsWorkSet.Number;
        export.AsinObject.Text20 = "ADMIN APPEAL";
        ExitState = "ECO_LNK_TO_ASIN";

        return;
      case "PRINT":
        // -----------------------------------------------
        // PF24  Print
        // Edit key fields for invalid change.
        // -----------------------------------------------
        if (!Equal(import.AdministrativeAction.Type1,
          import.HiddenAdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.ActionTaken.Date, import.HiddenActionTaken.Date))
        {
          var field = GetField(export.ActionTaken, "date");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -----------------------------------------------
        // Display document maintenance screen.
        // -----------------------------------------------
        export.Document.Name = "";
        export.Document.Type1 = "AAPP";
        export.DocmProtectFilter.Flag = "Y";
        export.HiddenSecurity.LinkIndicator = "L";
        ExitState = "ECO_LNK_TO_DOCUMENT_MAINT";

        return;
      case "RETDOCM":
        // -----------------------------------------------
        // Return from SP_DOCM_DOCUMENT_MAINT procedure
        // executed by Print command.
        // -----------------------------------------------
        if (IsEmpty(import.Document.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          return;
        }

        if (Equal(import.Document.Name, "AAPPAFAU"))
        {
        }
        else if (Equal(import.Document.Name, "AAPPSDSO") && AsChar
          (import.AdministrativeAppeal.Type1) == 'S')
        {
        }
        else if (Equal(import.Document.Name, "AAPPFDSO") && AsChar
          (import.AdministrativeAppeal.Type1) == 'F')
        {
        }
        else if ((Equal(import.Document.Name, "AAPPRQFH") || Equal
          (import.Document.Name, "AAPPSTMT")) && (
            AsChar(import.AdministrativeAppeal.Type1) == 'F' || AsChar
          (import.AdministrativeAppeal.Type1) == 'H'))
        {
        }
        else
        {
          ExitState = "LE0000_DOC_NOT_ALLWD_FOR_TYPE";

          return;
        }

        // mjr---------------------------11/30/1998
        // Call to sp_print_aapp_docs was removed
        // Added new method to execute Print
        // ----------------------------------------
        // mjr
        // -------------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before invoking print process
        // --------------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.NullNextTranInfo);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 = "PRINT:" + import.Document.Name;
        export.HiddenNextTranInfo.CsePersonNumber =
          export.CsePersonsWorkSet.Number;
        export.HiddenNextTranInfo.MiscNum1 =
          export.AdministrativeAppeal.Identifier;
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 11/30/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.    [Command is passed via nextran views.]
        // ------------------------------------------------------------
        UseScCabNextTranGet();
        export.CsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
        export.AdministrativeAppeal.Identifier =
          (int)export.HiddenNextTranInfo.MiscNum1.GetValueOrDefault();
        global.Command = "REDISP";

        break;
      case "RETAAPS":
        // ----------------------------------------------------
        // Transfer from LE_AAPS_LST_ADM_APPLS_BY_CSE_PER
        // ----------------------------------------------------
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        if (export.AdministrativeAppeal.Identifier > 0)
        {
          // --------------------------------------------
          // A record has been selected from AAPS list.
          // Display selected appeal.
          // --------------------------------------------
          global.Command = "DISPLAY";
        }

        break;
      case "RTNERR":
        var field2 = GetField(export.AdministrativeAppeal, "number");

        field2.Error = true;

        ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

        return;
      case "RTNERR2":
        var field3 = GetField(export.AdministrativeAppeal, "number");

        field3.Error = true;

        ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NU";

        return;
      case "EXIT":
        // ----------------------------------------------------
        // PF3  Exit
        // Allows the user to transfer to the menu.
        // ----------------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETURN":
        // -----------------------------------------------
        // PF9  Return
        // -----------------------------------------------
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
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

          UseScCabNextTranPut2();

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // ----------------------------------------------------
        // PF12  Signoff
        // Sign the user off the Kessep system.
        // ----------------------------------------------------
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      case "":
        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseLeCabReadAdminAppeal();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else if (IsExitState("LE0000_NO_ADMIN_APPLS_4_PRSN"))
      {
        export.HiddenCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
        export.HiddenSsnWorkArea.Assign(export.SsnWorkArea);

        if (!IsEmpty(export.AdministrativeAppeal.Number))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;
        }

        return;
      }
      else if (IsExitState("ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER"))
      {
        export.HiddenCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
        export.HiddenSsnWorkArea.Assign(export.SsnWorkArea);

        return;
      }
      else if (IsExitState("CSE_PERSON_NF"))
      {
        export.HiddenCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
        MoveSsnWorkArea3(local.ClearSsnWorkArea, export.SsnWorkArea);
        export.HiddenSsnWorkArea.Assign(local.ClearSsnWorkArea);

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }
      else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
      {
        export.HiddenCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
        export.HiddenSsnWorkArea.Assign(export.SsnWorkArea);

        var field = GetField(export.AdministrativeAppeal, "number");

        field.Error = true;

        return;
      }
      else if (IsExitState("ADMINISTRATIVE_ACTION_NF"))
      {
        export.HiddenCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
        export.HiddenSsnWorkArea.Assign(export.SsnWorkArea);

        var field = GetField(export.AdministrativeAction, "type1");

        field.Error = true;

        return;
      }
      else
      {
        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(local.DisplayAfterAdd.Flag) == 'A')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
      }
    }
    else if (Equal(global.Command, "REDISP"))
    {
      UseLeCabReadAdminAppeal();

      // mjr
      // ----------------------------------------
      // 11/30/1998
      // Added check for an exitstate returned from Print
      // -----------------------------------------------------
      local.Position.Count =
        Find(export.HiddenNextTranInfo.MiscText2, "PRINT:");

      if (local.Position.Count <= 0)
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
        // mjr---> Determines the appropriate exitstate for the Print process
        local.PrintRetCode.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces
          (50);
        UseSpPrintDecodeReturnCode();
        export.HiddenNextTranInfo.MiscText2 = local.PrintRetCode.Text50;
      }
    }

    // ----------------------------------------------------------
    // If all processing completed successfully, move all
    // exports to hidden exports .
    // ----------------------------------------------------------
    export.HiddenCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    export.HiddenSsnWorkArea.Assign(export.SsnWorkArea);
    MoveAdministrativeAction(export.AdministrativeAction,
      export.HiddenAdministrativeAction);
    export.HiddenActionTaken.Date = export.ActionTaken.Date;
    export.HiddenAdministrativeAppeal.Assign(export.AdministrativeAppeal);
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveAdministrativeAppeal1(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private static void MoveAdministrativeAppeal2(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.RequestDate = source.RequestDate;
    target.ReceivedDate = source.ReceivedDate;
    target.Reason = source.Reason;
    target.Respondent = source.Respondent;
    target.AppellantLastName = source.AppellantLastName;
    target.AppellantFirstName = source.AppellantFirstName;
    target.AppellantMiddleInitial = source.AppellantMiddleInitial;
    target.AppellantSuffix = source.AppellantSuffix;
    target.AppellantRelationship = source.AppellantRelationship;
    target.Outcome = source.Outcome;
    target.ReviewOutcome = source.ReviewOutcome;
    target.Date = source.Date;
    target.AdminOrderDate = source.AdminOrderDate;
    target.WithdrawDate = source.WithdrawDate;
    target.WithdrawReason = source.WithdrawReason;
    target.RequestFurtherReview = source.RequestFurtherReview;
    target.RequestFurtherReviewDate = source.RequestFurtherReviewDate;
    target.JudicialReviewInd = source.JudicialReviewInd;
    target.AdminReviewState = source.AdminReviewState;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Percentage = source.Percentage;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Type1 = source.Type1;
  }

  private static void MoveExport1ToLocal1(CabMatchCsePerson.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.DetailAlt.Flag = source.Ae.Flag;
    target.Ae.Flag = source.Cse.Flag;
    target.Cse.Flag = source.Kanpay.Flag;
    target.Kanpay.Flag = source.Kscares.Flag;
    target.Kscares.Flag = source.Alt.Flag;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea3(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    MoveCommon(local.SearchOption, useImport.Search);
    useImport.CsePersonsWorkSet.Ssn = export.CsePersonsWorkSet.Ssn;

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
  }

  private void UseCabReadAdminAction()
  {
    var useImport = new CabReadAdminAction.Import();
    var useExport = new CabReadAdminAction.Export();

    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;

    Call(CabReadAdminAction.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.ToBeValidatedCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ToBeValidatedCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeCabReadAdminAppeal()
  {
    var useImport = new LeCabReadAdminAppeal.Import();
    var useExport = new LeCabReadAdminAppeal.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveAdministrativeAppeal1(export.AdministrativeAppeal,
      useImport.AdministrativeAppeal);

    Call(LeCabReadAdminAppeal.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    export.ActionTaken.Date = useExport.ActionTaken.Date;
    MoveAdministrativeAppeal2(useExport.AdministrativeAppeal,
      export.AdministrativeAppeal);
  }

  private void UseReceiveAdministrativeAppeal()
  {
    var useImport = new ReceiveAdministrativeAppeal.Import();
    var useExport = new ReceiveAdministrativeAppeal.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.ActionTaken.Date = export.ActionTaken.Date;
    useImport.AdministrativeAppeal.Assign(export.AdministrativeAppeal);

    Call(ReceiveAdministrativeAppeal.Execute, useImport, useExport);

    MoveAdministrativeAppeal2(useExport.AdministrativeAppeal,
      export.AdministrativeAppeal);
  }

  private void UseResolveAdministrativeAppeal()
  {
    var useImport = new ResolveAdministrativeAppeal.Import();
    var useExport = new ResolveAdministrativeAppeal.Export();

    useImport.AdministrativeAppeal.Assign(export.AdministrativeAppeal);

    Call(ResolveAdministrativeAppeal.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.Local1.Item.Detail);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.Returned.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.PrintRetCode.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.PrintRetCode.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "miscNum1",
          export.HiddenNextTranInfo.MiscNum1.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.RequestDate = db.GetDate(reader, 3);
        entities.AdministrativeAppeal.ReceivedDate = db.GetDate(reader, 4);
        entities.AdministrativeAppeal.Respondent = db.GetString(reader, 5);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 7);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.AppellantSuffix =
          db.GetNullableString(reader, 9);
        entities.AdministrativeAppeal.AppellantRelationship =
          db.GetNullableString(reader, 10);
        entities.AdministrativeAppeal.Date = db.GetNullableDate(reader, 11);
        entities.AdministrativeAppeal.AdminOrderDate =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeAppeal.WithdrawDate =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeAppeal.RequestFurtherReviewDate =
          db.GetNullableDate(reader, 14);
        entities.AdministrativeAppeal.CreatedBy = db.GetString(reader, 15);
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.AdministrativeAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 18);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 21);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 22);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 24);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 25);
        entities.AdministrativeAppeal.AdminReviewState =
          db.GetString(reader, 26);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.AdministrativeAppeal.CspQNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdminActionTypePrompt.
    /// </summary>
    [JsonPropertyName("adminActionTypePrompt")]
    public Common AdminActionTypePrompt
    {
      get => adminActionTypePrompt ??= new();
      set => adminActionTypePrompt = value;
    }

    /// <summary>
    /// A value of AdminReviewStatePrompt.
    /// </summary>
    [JsonPropertyName("adminReviewStatePrompt")]
    public Common AdminReviewStatePrompt
    {
      get => adminReviewStatePrompt ??= new();
      set => adminReviewStatePrompt = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public DateWorkArea ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AppealTypePrompt.
    /// </summary>
    [JsonPropertyName("appealTypePrompt")]
    public Common AppealTypePrompt
    {
      get => appealTypePrompt ??= new();
      set => appealTypePrompt = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenSsnWorkArea.
    /// </summary>
    [JsonPropertyName("hiddenSsnWorkArea")]
    public SsnWorkArea HiddenSsnWorkArea
    {
      get => hiddenSsnWorkArea ??= new();
      set => hiddenSsnWorkArea = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenActionTaken.
    /// </summary>
    [JsonPropertyName("hiddenActionTaken")]
    public DateWorkArea HiddenActionTaken
    {
      get => hiddenActionTaken ??= new();
      set => hiddenActionTaken = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    /// <summary>
    /// A value of SelectedAdministrativeAction.
    /// </summary>
    [JsonPropertyName("selectedAdministrativeAction")]
    public AdministrativeAction SelectedAdministrativeAction
    {
      get => selectedAdministrativeAction ??= new();
      set => selectedAdministrativeAction = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private SsnWorkArea ssnWorkArea;
    private AdministrativeAction administrativeAction;
    private Common adminActionTypePrompt;
    private Common adminReviewStatePrompt;
    private DateWorkArea actionTaken;
    private AdministrativeAppeal administrativeAppeal;
    private Common appealTypePrompt;
    private Code code;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private SsnWorkArea hiddenSsnWorkArea;
    private AdministrativeAction hiddenAdministrativeAction;
    private DateWorkArea hiddenActionTaken;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private Document document;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
    private AdministrativeAction selectedAdministrativeAction;
    private CodeValue selectedCodeValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdminActionTypePrompt.
    /// </summary>
    [JsonPropertyName("adminActionTypePrompt")]
    public Common AdminActionTypePrompt
    {
      get => adminActionTypePrompt ??= new();
      set => adminActionTypePrompt = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public DateWorkArea ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AppealTypePrompt.
    /// </summary>
    [JsonPropertyName("appealTypePrompt")]
    public Common AppealTypePrompt
    {
      get => appealTypePrompt ??= new();
      set => appealTypePrompt = value;
    }

    /// <summary>
    /// A value of AdminRvwStatePrompt.
    /// </summary>
    [JsonPropertyName("adminRvwStatePrompt")]
    public Common AdminRvwStatePrompt
    {
      get => adminRvwStatePrompt ??= new();
      set => adminRvwStatePrompt = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenSsnWorkArea.
    /// </summary>
    [JsonPropertyName("hiddenSsnWorkArea")]
    public SsnWorkArea HiddenSsnWorkArea
    {
      get => hiddenSsnWorkArea ??= new();
      set => hiddenSsnWorkArea = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenActionTaken.
    /// </summary>
    [JsonPropertyName("hiddenActionTaken")]
    public DateWorkArea HiddenActionTaken
    {
      get => hiddenActionTaken ??= new();
      set => hiddenActionTaken = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of DocmProtectFilter.
    /// </summary>
    [JsonPropertyName("docmProtectFilter")]
    public Common DocmProtectFilter
    {
      get => docmProtectFilter ??= new();
      set => docmProtectFilter = value;
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
    }

    /// <summary>
    /// A value of AsinObject.
    /// </summary>
    [JsonPropertyName("asinObject")]
    public SpTextWorkArea AsinObject
    {
      get => asinObject ??= new();
      set => asinObject = value;
    }

    /// <summary>
    /// A value of Asin.
    /// </summary>
    [JsonPropertyName("asin")]
    public CsePerson Asin
    {
      get => asin ??= new();
      set => asin = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private SsnWorkArea ssnWorkArea;
    private AdministrativeAction administrativeAction;
    private Common adminActionTypePrompt;
    private DateWorkArea actionTaken;
    private AdministrativeAppeal administrativeAppeal;
    private Common appealTypePrompt;
    private Common adminRvwStatePrompt;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private SsnWorkArea hiddenSsnWorkArea;
    private AdministrativeAction hiddenAdministrativeAction;
    private DateWorkArea hiddenActionTaken;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private Document document;
    private Common docmProtectFilter;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
    private Code code;
    private Common displayActiveCasesOnly;
    private SpTextWorkArea asinObject;
    private CsePerson asin;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of NumPromptsSelected.
    /// </summary>
    [JsonPropertyName("numPromptsSelected")]
    public Common NumPromptsSelected
    {
      get => numPromptsSelected ??= new();
      set => numPromptsSelected = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CsePersonsWorkSet Returned
    {
      get => returned ??= new();
      set => returned = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of SearchOption.
    /// </summary>
    [JsonPropertyName("searchOption")]
    public Common SearchOption
    {
      get => searchOption ??= new();
      set => searchOption = value;
    }

    /// <summary>
    /// A value of ToBeValidatedCode.
    /// </summary>
    [JsonPropertyName("toBeValidatedCode")]
    public Code ToBeValidatedCode
    {
      get => toBeValidatedCode ??= new();
      set => toBeValidatedCode = value;
    }

    /// <summary>
    /// A value of ToBeValidatedCodeValue.
    /// </summary>
    [JsonPropertyName("toBeValidatedCodeValue")]
    public CodeValue ToBeValidatedCodeValue
    {
      get => toBeValidatedCodeValue ??= new();
      set => toBeValidatedCodeValue = value;
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
    /// A value of DisplayAfterAdd.
    /// </summary>
    [JsonPropertyName("displayAfterAdd")]
    public Common DisplayAfterAdd
    {
      get => displayAfterAdd ??= new();
      set => displayAfterAdd = value;
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
    /// A value of PrintRetCode.
    /// </summary>
    [JsonPropertyName("printRetCode")]
    public WorkArea PrintRetCode
    {
      get => printRetCode ??= new();
      set => printRetCode = value;
    }

    /// <summary>
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ClearSsnWorkArea.
    /// </summary>
    [JsonPropertyName("clearSsnWorkArea")]
    public SsnWorkArea ClearSsnWorkArea
    {
      get => clearSsnWorkArea ??= new();
      set => clearSsnWorkArea = value;
    }

    /// <summary>
    /// A value of ClearAdministrativeAction.
    /// </summary>
    [JsonPropertyName("clearAdministrativeAction")]
    public AdministrativeAction ClearAdministrativeAction
    {
      get => clearAdministrativeAction ??= new();
      set => clearAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ClearActionTaken.
    /// </summary>
    [JsonPropertyName("clearActionTaken")]
    public DateWorkArea ClearActionTaken
    {
      get => clearActionTaken ??= new();
      set => clearActionTaken = value;
    }

    /// <summary>
    /// A value of ClearAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("clearAdministrativeAppeal")]
    public AdministrativeAppeal ClearAdministrativeAppeal
    {
      get => clearAdministrativeAppeal ??= new();
      set => clearAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of NullNextTranInfo.
    /// </summary>
    [JsonPropertyName("nullNextTranInfo")]
    public NextTranInfo NullNextTranInfo
    {
      get => nullNextTranInfo ??= new();
      set => nullNextTranInfo = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    private Common numPromptsSelected;
    private CsePersonsWorkSet returned;
    private DateWorkArea nullDateWorkArea;
    private TextWorkArea textWorkArea;
    private Common searchOption;
    private Code toBeValidatedCode;
    private CodeValue toBeValidatedCodeValue;
    private Common validCode;
    private Common displayAfterAdd;
    private Common position;
    private WorkArea printRetCode;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
    private SsnWorkArea clearSsnWorkArea;
    private AdministrativeAction clearAdministrativeAction;
    private DateWorkArea clearActionTaken;
    private AdministrativeAppeal clearAdministrativeAppeal;
    private NextTranInfo nullNextTranInfo;
    private Array<LocalGroup> local1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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

    private AdministrativeAppeal administrativeAppeal;
    private CsePerson csePerson;
  }
#endregion
}

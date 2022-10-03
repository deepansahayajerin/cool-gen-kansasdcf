// Program: FN_CSUP_MTN_COLL_DISB_SUPPRSSION, ID: 371751793, model: 746.
// Short name: SWECSUPP
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
/// A program: FN_CSUP_MTN_COLL_DISB_SUPPRSSION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCsupMtnCollDisbSupprssion: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CSUP_MTN_COLL_DISB_SUPPRSSION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCsupMtnCollDisbSupprssion(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCsupMtnCollDisbSupprssion.
  /// </summary>
  public FnCsupMtnCollDisbSupprssion(IContext context, Import import,
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
    // ******************************************************************
    //                  Developed for KESSEP by MTW
    //                   D. M. Nilsen  09/06/95
    // Modified By : R.B.Mohapatra - MTW  05/28/1996
    // *******************************************************************
    // ****************************************************************
    // Changed the following for Phase II. RK 10/26/98
    // 1.  Created a new field to say if more than one Collection Suppression 
    // exists for this payee.
    // 2.  Should be able to update a future effective or discontinue date. Can'
    // t update an active effective date. Can update the discontinue date is
    // equal to today or greater. Can't update either date if in the past.
    // 3.  Non Case related persons and Children can't have Suppresions.
    // 4.  The discontinue date must be greater than the effecitve date.
    // **************************************************************
    // ****************************************************************
    // Changed the following for phase II integrations testing RK 04/14/99.
    // 1.  If an invalid CSE Person number is entered and a flow to another 
    // screen is requested(pf key or NEXT) then an error message will show
    // saying that the screen must be cleared on the invalid number prior to
    // flowing.
    // ****************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // Move all IMPORTs to EXPORTs.
    export.CsePerson.Number = import.CsePerson.Number;

    // *****  changes for WR 040796
    export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    export.CourtOrderPrompt.Text1 = import.CourtOrderPrompt.Text1;

    // *****  changes for WR 040796
    export.CollectionType.Assign(import.CollectionType);
    export.DisbSuppressionStatusHistory.EffectiveDate =
      import.DisbSuppressionStatusHistory.EffectiveDate;

    if (Equal(global.Command, "RETLINK"))
    {
      if (!IsEmpty(import.FromFlowCsePerson.Number))
      {
        export.CsePerson.Number = import.FromFlowCsePerson.Number;
        export.DisbSuppressionStatusHistory.EffectiveDate =
          import.FromFlowDisbSuppressionStatusHistory.EffectiveDate;
      }

      if (!IsEmpty(import.FromFlowCollectionType.Code))
      {
        export.CollectionType.Code = import.FromFlowCollectionType.Code;
      }

      global.Command = "DISPLAY";
    }

    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate1();

    if (Equal(global.Command, "RETCSENO"))
    {
      if (!IsEmpty(import.FromFlowCsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Assign(import.FromFlowCsePersonsWorkSet);
        export.CsePerson.Number = import.FromFlowCsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.PersonPropmt.Text1 = import.PersonPrompt.Text1;
      export.CollectionPrompt.Text1 = import.CollectionPrompt.Text1;
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
      export.DisbSuppressionStatusHistory.Assign(
        import.DisbSuppressionStatusHistory);
      export.SuppressAll.Flag = import.SuppressAll.Flag;
      export.OtherCollSupressExist.Flag = import.OtherCollSupressExist.Flag;
      export.HiddenIdCsePerson.Number = import.HiddenIdCsePerson.Number;
      export.HiddenIdDisbSuppressionStatusHistory.Assign(
        import.HiddenIdDisbSuppressionStatusHistory);
      MoveCollectionType(import.HiddenCollectionType,
        export.HiddenCollectionType);
      export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

      // *****  changes for WR 040796
      export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
      export.CourtOrderPrompt.Text1 = import.CourtOrderPrompt.Text1;
      export.HiddenLegalAction.StandardNumber =
        import.HiddenLegalAction.StandardNumber;

      // *****  changes for WR 040796
    }

    local.LeftPadding.Text10 = export.CsePerson.Number;
    UseEabPadLeftWithZeros();
    export.CsePerson.Number = local.LeftPadding.Text10;
    export.CsePersonsWorkSet.Number = export.CsePerson.Number;

    if (!IsEmpty(export.CsePerson.Number))
    {
      UseSiReadCsePerson();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************************************************
      // If a flow is requested while an invalid CSE Person Number is on screen 
      // then error off. rk 4/14/99.
      // ****************************************************************
      if (Equal(global.Command, "PSUP") || Equal(global.Command, "PACC") || Equal
        (global.Command, "LDSP") || Equal(global.Command, "LPSP") || !
        IsEmpty(import.Standard.NextTransaction))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "FN0000_NO_FLOW_ALLWD_INV_PERSON";
      }

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.HiddenNextTranInfo.CsePersonNumberObligee =
        export.CsePersonsWorkSet.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligee))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberObligee ?? Spaces(10);
      }
      else
      {
        export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
      }

      local.LeftPadding.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.LeftPadding.Text10;
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;

      if (!IsEmpty(export.CsePerson.Number))
      {
        UseSiReadCsePerson();
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "PSUP") || Equal(global.Command, "PACC") || Equal
      (global.Command, "LDSP") || Equal(global.Command, "LPSP"))
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

    // *********** Hardcode Area ***********
    local.CsePersonAccount.Type1 = "E";
    export.DisbSuppressionStatusHistory.Type1 = "C";

    // The logic assumes that a record cannot be
    // UPDATEd or DELETEd without first being displayed.
    // Therefore, a key change with either command is invalid.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      (!Equal(export.CsePerson.Number, import.HiddenIdCsePerson.Number) || !
      Equal(import.CollectionType.Code, import.HiddenCollectionType.Code) || import
      .HiddenIdDisbSuppressionStatusHistory.SystemGeneratedIdentifier != import
      .DisbSuppressionStatusHistory.SystemGeneratedIdentifier))
    {
      ExitState = "FN0000_MUST_DISPLAY_BEFORE_CHNG";

      var field1 = GetField(export.CsePerson, "number");

      field1.Error = true;

      var field2 = GetField(export.CollectionType, "code");

      field2.Error = true;

      return;
    }

    // If the key field is blank, certain
    // commands are not allowed.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE")) && (
        IsEmpty(export.CsePerson.Number) || IsEmpty
      (export.CollectionType.Code)))
    {
      ExitState = "KEY_FIELD_IS_BLANK";

      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;
      }

      if (IsEmpty(export.CollectionType.Code))
      {
        var field = GetField(export.CollectionType, "code");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      // check to see if person has been displayed
      if (!Equal(import.HiddenIdCsePerson.Number, export.CsePerson.Number))
      {
        export.HiddenIdDisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          0;

        // must display person
        UseFnReadPersonForDisbSupp();

        // *******
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenIdCsePerson.Number = export.CsePerson.Number;
          export.DisbSuppressionStatusHistory.SystemGeneratedIdentifier = 0;
        }
        else
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }
      }

      if (!IsEmpty(export.CsePerson.Number))
      {
        var field1 = GetField(export.CsePerson, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.PersonPropmt, "text1");

        field2.Color = "cyan";
        field2.Protected = true;
      }

      if (!IsEmpty(export.CollectionType.Code))
      {
        var field1 = GetField(export.CollectionType, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.CollectionPrompt, "text1");

        field2.Color = "cyan";
        field2.Protected = true;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // ******
      // Validate the input dates
      // Reason Text is mandatory
      // ******
      local.DisbSuppressionStatusHistory.Assign(
        import.DisbSuppressionStatusHistory);

      // *****  changes for WR 040796
      if (!IsEmpty(import.LegalAction.StandardNumber))
      {
        local.DisbSuppressionStatusHistory.Type1 = "O";
      }
      else
      {
        local.DisbSuppressionStatusHistory.Type1 = "C";
      }

      // *****  changes for WR 040796
      if (Equal(local.DisbSuppressionStatusHistory.EffectiveDate,
        local.Zero.Date))
      {
        ExitState = "EFFECTIVE_DATE_REQUIRED";

        var field =
          GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

        field.Error = true;

        return;
      }

      if (Equal(local.DisbSuppressionStatusHistory.DiscontinueDate,
        local.Zero.Date))
      {
        local.DisbSuppressionStatusHistory.DiscontinueDate =
          UseCabSetMaximumDiscontinueDate2();
      }

      if (Lt(local.DisbSuppressionStatusHistory.EffectiveDate, Now().Date))
      {
        if (Equal(global.Command, "ADD"))
        {
          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;

          return;
        }
        else if (!Equal(import.HiddenIdDisbSuppressionStatusHistory.
          EffectiveDate, import.DisbSuppressionStatusHistory.EffectiveDate))
        {
          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;

          return;
        }
      }

      // **************************************************************
      // Discontinue date must be greater than the Effecitve date
      // **************************************************************
      if (!Lt(local.DisbSuppressionStatusHistory.EffectiveDate,
        local.DisbSuppressionStatusHistory.DiscontinueDate))
      {
        ExitState = "FN0000_DISC_DT_GREATER_THAN_EFF";

        var field1 =
          GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

        field1.Error = true;

        var field2 =
          GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

        field2.Error = true;

        return;
      }

      if (IsEmpty(import.DisbSuppressionStatusHistory.ReasonText))
      {
        var field = GetField(export.DisbSuppressionStatusHistory, "reasonText");

        field.Error = true;

        ExitState = "SP0000_REQUIRED_FIELD_MISSING";

        return;
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "PSUP":
        ExitState = "ECO_XFR_TO_MTN_PERSON_DISB_SUPP";

        break;
      case "LDSP":
        ExitState = "ECO_XFR_TO_LST_MTN_DISB_SUPP";

        break;
      case "LPSP":
        ExitState = "ECO_LNK_LST_PAYEE_W_DISB_SUP";

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        // *****  changes for WR 040796
        if (!IsEmpty(import.CollectionPrompt.Text1) && AsChar
          (import.CollectionPrompt.Text1) != 'S')
        {
          var field = GetField(export.CollectionPrompt, "text1");

          field.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";
        }

        if (!IsEmpty(import.CourtOrderPrompt.Text1) && AsChar
          (import.CourtOrderPrompt.Text1) != 'S')
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";
        }

        if (!IsEmpty(import.PersonPrompt.Text1) && AsChar
          (import.PersonPrompt.Text1) != 'S')
        {
          var field = GetField(export.PersonPropmt, "text1");

          field.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(import.PersonPrompt.Text1) == 'S')
        {
          if (AsChar(import.CollectionPrompt.Text1) == 'S')
          {
            var field1 = GetField(export.CollectionPrompt, "text1");

            field1.Error = true;

            var field2 = GetField(export.PersonPropmt, "text1");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }

          if (AsChar(import.CourtOrderPrompt.Text1) == 'S')
          {
            var field1 = GetField(export.CourtOrderPrompt, "text1");

            field1.Error = true;

            var field2 = GetField(export.PersonPropmt, "text1");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.PersonPropmt.Text1 = "+";
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }
        }
        else if (AsChar(import.CourtOrderPrompt.Text1) == 'S')
        {
          if (AsChar(import.CollectionPrompt.Text1) == 'S')
          {
            var field1 = GetField(export.CollectionPrompt, "text1");

            field1.Error = true;

            var field2 = GetField(export.CourtOrderPrompt, "text1");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }
          else
          {
            ExitState = "ECO_LNK_TO_LAPS";
          }
        }
        else if (AsChar(import.CollectionPrompt.Text1) == 'S')
        {
          ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";
        }

        // *****  changes for WR 040796
        break;
      case "DISPLAY":
        // **************************************************************
        // View matched on a flag to tell if other Collection Suppression exist 
        // for this Payee. RK 10/26/98
        // **************************************************************
        UseFnReadCollectionDisbSuppress();

        if (IsExitState("FN0000_NO_ACTIVE_OR_FUTURE_SUPPR"))
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenIdCsePerson.Number = "";
          export.HiddenIdDisbSuppressionStatusHistory.
            SystemGeneratedIdentifier = 0;
          export.HiddenCollectionType.SequentialIdentifier = 0;
        }
        else if (IsExitState("FN0000_ACTIVE_AND_FUTURE_SUPPRS"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenIdCsePerson.Number = export.CsePerson.Number;
          export.HiddenCollectionType.Assign(export.CollectionType);
          export.HiddenIdDisbSuppressionStatusHistory.Assign(
            export.DisbSuppressionStatusHistory);
        }
        else if (IsExitState("FN0000_COLLECTION_TYPE_NF"))
        {
          var field = GetField(export.CollectionType, "code");

          field.Error = true;

          // Set the hidden key field to spaces or zero.
          export.HiddenIdCsePerson.Number = "";
          export.HiddenIdDisbSuppressionStatusHistory.
            SystemGeneratedIdentifier = 0;
          export.HiddenCollectionType.SequentialIdentifier = 0;
        }
        else if (IsExitState("FN0000_DISB_SUPP_NF_FOR_DATE"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;

          // Set the hidden key field to spaces or zero.
          export.HiddenIdCsePerson.Number = "";
          export.HiddenIdDisbSuppressionStatusHistory.
            SystemGeneratedIdentifier = 0;
          export.HiddenCollectionType.SequentialIdentifier = 0;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenIdCsePerson.Number = export.CsePerson.Number;
          export.HiddenCollectionType.Assign(export.CollectionType);
          export.HiddenIdDisbSuppressionStatusHistory.Assign(
            export.DisbSuppressionStatusHistory);
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenIdCsePerson.Number = "";
          export.HiddenIdDisbSuppressionStatusHistory.
            SystemGeneratedIdentifier = 0;
          export.HiddenCollectionType.SequentialIdentifier = 0;
        }

        break;
      case "ADD":
        // **************************
        // Payee must be an AR.
        // **************************
        if (ReadCaseRole())
        {
          // Continue
        }
        else
        {
          ExitState = "FN0000_AR_CSE_PERSON_NF";

          return;
        }

        UseFnCreateCollDisbSuppress();

        if (IsExitState("FN0000_COLLECTION_TYPE_NF"))
        {
          var field = GetField(export.CollectionType, "code");

          field.Error = true;
        }
        else if (IsExitState("FN0000_DATE_OVERLAP_ERROR"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field1.Error = true;

          var field2 =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          // No exit states are returned from cab below
          export.HiddenIdCsePerson.Number = export.CsePerson.Number;
          export.HiddenIdDisbSuppressionStatusHistory.Assign(
            export.DisbSuppressionStatusHistory);
          export.HiddenCollectionType.Assign(export.CollectionType);
          local.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
            export.DisbSuppressionStatusHistory.SystemGeneratedIdentifier;

          // No exit states are returned from cab below
          UseFnCheckForOtherDispSupp1();
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_ADD_2";
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_AE"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_PV"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;
        }
        else if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "UPDATE":
        UseFnUpdCollectionDisbSuppress();

        if (Equal(export.DisbSuppressionStatusHistory.DiscontinueDate,
          local.MaxDate.Date))
        {
          export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // No exit states are returned from cab below
          UseFnCheckForOtherDispSupp2();
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_2";
        }
        else if (IsExitState("EXPIRE_DATE_PRIOR_TO_EFFECTIVE"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field1.Error = true;

          var field2 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field2.Error = true;
        }
        else if (IsExitState("FN0000_SUPPR_MUST_RELE_IN_FUTURE"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field.Error = true;
        }
        else if (IsExitState("FN0000_CANNOT_CHG_EFF_DT"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;
        }
        else if (IsExitState("EFFECTIVE_DATE_PRIOR_TO_CURRENT"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field1.Error = true;

          var field2 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field2.Error = true;
        }
        else if (IsExitState("CANNOT_CHANGE_A_DISCONTINUED_REC"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.CollectionType, "code");

          field2.Error = true;
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_NU"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.CollectionType, "code");

          field2.Error = true;
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_PV"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.CollectionType, "code");

          field2.Error = true;
        }
        else if (IsExitState("FN0000_DATE_OVERLAP_ERROR"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.CsePerson, "number");

          field2.Error = true;
        }
        else
        {
        }

        break;
      case "DELETE":
        UseFnDelPersonDisbSuppression();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_DELETE_SUCCESSFUL";

          // Set the hidden key field to spaces or zero.
          export.HiddenIdCsePerson.Number = "";
          export.HiddenIdDisbSuppressionStatusHistory.
            SystemGeneratedIdentifier = 0;
          export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;
          export.DisbSuppressionStatusHistory.EffectiveDate = local.Zero.Date;
          export.DisbSuppressionStatusHistory.CreatedBy = "";
          export.DisbSuppressionStatusHistory.LastUpdatedBy = "";
          export.DisbSuppressionStatusHistory.ReasonText =
            Spaces(DisbSuppressionStatusHistory.ReasonText_MaxLength);
        }
        else if (IsExitState("CANNOT_DELETE_EFFECTIVE_DATE"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_NF"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.CsePerson, "number");

          field2.Error = true;
        }
        else
        {
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveDisbSuppressionStatusHistory(
    DisbSuppressionStatusHistory source, DisbSuppressionStatusHistory target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
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

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private void UseFnCheckForOtherDispSupp1()
  {
    var useImport = new FnCheckForOtherDispSupp.Import();
    var useExport = new FnCheckForOtherDispSupp.Export();

    useImport.DisbSuppressionStatusHistory.Assign(
      local.DisbSuppressionStatusHistory);
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnCheckForOtherDispSupp.Execute, useImport, useExport);

    export.OtherCollSupressExist.Flag = useExport.OtherCollSuppressExist.Flag;
    export.SuppressAll.Flag = useExport.SuppressedAll.Flag;
  }

  private void UseFnCheckForOtherDispSupp2()
  {
    var useImport = new FnCheckForOtherDispSupp.Import();
    var useExport = new FnCheckForOtherDispSupp.Export();

    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.DisbSuppressionStatusHistory.Assign(
      export.DisbSuppressionStatusHistory);

    Call(FnCheckForOtherDispSupp.Execute, useImport, useExport);

    export.OtherCollSupressExist.Flag = useExport.OtherCollSuppressExist.Flag;
    export.SuppressAll.Flag = useExport.SuppressedAll.Flag;
  }

  private void UseFnCreateCollDisbSuppress()
  {
    var useImport = new FnCreateCollDisbSuppress.Import();
    var useExport = new FnCreateCollDisbSuppress.Export();

    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    MoveCollectionType(import.CollectionType, useImport.CollectionType);
    useImport.DisbSuppressionStatusHistory.Assign(
      local.DisbSuppressionStatusHistory);
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnCreateCollDisbSuppress.Execute, useImport, useExport);

    MoveCollectionType(useExport.CollectionType, export.CollectionType);
    export.DisbSuppressionStatusHistory.Assign(
      useExport.DisbSuppressionStatusHistory);
  }

  private void UseFnDelPersonDisbSuppression()
  {
    var useImport = new FnDelPersonDisbSuppression.Import();
    var useExport = new FnDelPersonDisbSuppression.Export();

    useImport.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
      import.DisbSuppressionStatusHistory.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnDelPersonDisbSuppression.Execute, useImport, useExport);
  }

  private void UseFnReadCollectionDisbSuppress()
  {
    var useImport = new FnReadCollectionDisbSuppress.Import();
    var useExport = new FnReadCollectionDisbSuppress.Export();

    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CollectionType.Assign(export.CollectionType);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.DisbSuppressionStatusHistory.Assign(
      export.DisbSuppressionStatusHistory);

    Call(FnReadCollectionDisbSuppress.Execute, useImport, useExport);

    export.OtherCollSupressExist.Flag = useExport.OtherCollSupressExist.Flag;
    export.SuppressAll.Flag = useExport.SuppressAll.Flag;
    export.CollectionType.Assign(useExport.CollectionType);
    export.DisbSuppressionStatusHistory.Assign(
      useExport.DisbSuppressionStatusHistory);
  }

  private void UseFnReadPersonForDisbSupp()
  {
    var useImport = new FnReadPersonForDisbSupp.Import();
    var useExport = new FnReadPersonForDisbSupp.Export();

    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnReadPersonForDisbSupp.Execute, useImport, useExport);

    local.CsePersonAccount.Type1 = useExport.CsePersonAccount.Type1;
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseFnUpdCollectionDisbSuppress()
  {
    var useImport = new FnUpdCollectionDisbSuppress.Import();
    var useExport = new FnUpdCollectionDisbSuppress.Export();

    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.DisbSuppressionStatusHistory.Assign(
      local.DisbSuppressionStatusHistory);
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CollectionType.Code = export.CollectionType.Code;
    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveDisbSuppressionStatusHistory(export.
      HiddenIdDisbSuppressionStatusHistory, useImport.Hidden);

    Call(FnUpdCollectionDisbSuppress.Execute, useImport, useExport);

    export.DisbSuppressionStatusHistory.Assign(
      useExport.DisbSuppressionStatusHistory);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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
    /// A value of FromFlowDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("fromFlowDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory FromFlowDisbSuppressionStatusHistory
    {
      get => fromFlowDisbSuppressionStatusHistory ??= new();
      set => fromFlowDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of OtherCollSupressExist.
    /// </summary>
    [JsonPropertyName("otherCollSupressExist")]
    public Common OtherCollSupressExist
    {
      get => otherCollSupressExist ??= new();
      set => otherCollSupressExist = value;
    }

    /// <summary>
    /// A value of CollectionPrompt.
    /// </summary>
    [JsonPropertyName("collectionPrompt")]
    public TextWorkArea CollectionPrompt
    {
      get => collectionPrompt ??= new();
      set => collectionPrompt = value;
    }

    /// <summary>
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public TextWorkArea PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
    }

    /// <summary>
    /// A value of FromFlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fromFlowCsePersonsWorkSet")]
    public CsePersonsWorkSet FromFlowCsePersonsWorkSet
    {
      get => fromFlowCsePersonsWorkSet ??= new();
      set => fromFlowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FromFlowCsePerson.
    /// </summary>
    [JsonPropertyName("fromFlowCsePerson")]
    public CsePerson FromFlowCsePerson
    {
      get => fromFlowCsePerson ??= new();
      set => fromFlowCsePerson = value;
    }

    /// <summary>
    /// A value of FromFlowCollectionType.
    /// </summary>
    [JsonPropertyName("fromFlowCollectionType")]
    public CollectionType FromFlowCollectionType
    {
      get => fromFlowCollectionType ??= new();
      set => fromFlowCollectionType = value;
    }

    /// <summary>
    /// A value of SuppressAll.
    /// </summary>
    [JsonPropertyName("suppressAll")]
    public Common SuppressAll
    {
      get => suppressAll ??= new();
      set => suppressAll = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenIdCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenIdCsePerson")]
    public CsePerson HiddenIdCsePerson
    {
      get => hiddenIdCsePerson ??= new();
      set => hiddenIdCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenIdDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("hiddenIdDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory HiddenIdDisbSuppressionStatusHistory
    {
      get => hiddenIdDisbSuppressionStatusHistory ??= new();
      set => hiddenIdDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenCollectionType")]
    public CollectionType HiddenCollectionType
    {
      get => hiddenCollectionType ??= new();
      set => hiddenCollectionType = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public TextWorkArea CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
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

    private DisbSuppressionStatusHistory fromFlowDisbSuppressionStatusHistory;
    private Common otherCollSupressExist;
    private TextWorkArea collectionPrompt;
    private TextWorkArea personPrompt;
    private CsePersonsWorkSet fromFlowCsePersonsWorkSet;
    private CsePerson fromFlowCsePerson;
    private CollectionType fromFlowCollectionType;
    private Common suppressAll;
    private CollectionType collectionType;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson hiddenIdCsePerson;
    private DisbSuppressionStatusHistory hiddenIdDisbSuppressionStatusHistory;
    private CollectionType hiddenCollectionType;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Standard standard;
    private LegalAction legalAction;
    private TextWorkArea courtOrderPrompt;
    private LegalAction hiddenLegalAction;
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
    /// A value of OtherCollSupressExist.
    /// </summary>
    [JsonPropertyName("otherCollSupressExist")]
    public Common OtherCollSupressExist
    {
      get => otherCollSupressExist ??= new();
      set => otherCollSupressExist = value;
    }

    /// <summary>
    /// A value of CollectionPrompt.
    /// </summary>
    [JsonPropertyName("collectionPrompt")]
    public TextWorkArea CollectionPrompt
    {
      get => collectionPrompt ??= new();
      set => collectionPrompt = value;
    }

    /// <summary>
    /// A value of PersonPropmt.
    /// </summary>
    [JsonPropertyName("personPropmt")]
    public TextWorkArea PersonPropmt
    {
      get => personPropmt ??= new();
      set => personPropmt = value;
    }

    /// <summary>
    /// A value of SuppressAll.
    /// </summary>
    [JsonPropertyName("suppressAll")]
    public Common SuppressAll
    {
      get => suppressAll ??= new();
      set => suppressAll = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenIdCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenIdCsePerson")]
    public CsePerson HiddenIdCsePerson
    {
      get => hiddenIdCsePerson ??= new();
      set => hiddenIdCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenIdDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("hiddenIdDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory HiddenIdDisbSuppressionStatusHistory
    {
      get => hiddenIdDisbSuppressionStatusHistory ??= new();
      set => hiddenIdDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenCollectionType")]
    public CollectionType HiddenCollectionType
    {
      get => hiddenCollectionType ??= new();
      set => hiddenCollectionType = value;
    }

    /// <summary>
    /// A value of ObligactionCount.
    /// </summary>
    [JsonPropertyName("obligactionCount")]
    public Common ObligactionCount
    {
      get => obligactionCount ??= new();
      set => obligactionCount = value;
    }

    /// <summary>
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public TextWorkArea CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
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

    private Common otherCollSupressExist;
    private TextWorkArea collectionPrompt;
    private TextWorkArea personPropmt;
    private Common suppressAll;
    private CollectionType collectionType;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson hiddenIdCsePerson;
    private DisbSuppressionStatusHistory hiddenIdDisbSuppressionStatusHistory;
    private CollectionType hiddenCollectionType;
    private Common obligactionCount;
    private Common caseCount;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Standard standard;
    private LegalAction legalAction;
    private TextWorkArea courtOrderPrompt;
    private LegalAction hiddenLegalAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonCaseRelated.
    /// </summary>
    [JsonPropertyName("csePersonCaseRelated")]
    public Common CsePersonCaseRelated
    {
      get => csePersonCaseRelated ??= new();
      set => csePersonCaseRelated = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of Fresh.
    /// </summary>
    [JsonPropertyName("fresh")]
    public DisbSuppressionStatusHistory Fresh
    {
      get => fresh ??= new();
      set => fresh = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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

    private Common csePersonCaseRelated;
    private Common count;
    private DateWorkArea maxDate;
    private DisbSuppressionStatusHistory fresh;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CollectionType collectionType;
    private CsePerson csePerson;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DateWorkArea dateWorkArea;
    private DateWorkArea zero;
    private CsePersonAccount csePersonAccount;
    private WorkArea workArea;
    private NextTranInfo nextTranInfo;
    private TextWorkArea leftPadding;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}

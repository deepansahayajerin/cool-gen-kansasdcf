// Program: FN_PSUP_MTN_PRSN_DISB_SUPPRSSN, ID: 371755011, model: 746.
// Short name: SWEPSUPP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PSUP_MTN_PRSN_DISB_SUPPRSSN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPsupMtnPrsnDisbSupprssn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PSUP_MTN_PRSN_DISB_SUPPRSSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPsupMtnPrsnDisbSupprssn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPsupMtnPrsnDisbSupprssn.
  /// </summary>
  public FnPsupMtnPrsnDisbSupprssn(IContext context, Import import,
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
    // 12/13/96	R. Marchman	Add new security/next tran
    // 11/13/97        Skip Hardy MTW  Removed external action block to do
    //                                 
    // a commit to the database.
    // 10/17/01        Mark Ashworth   Removed end date qualification to read of
    //                                 
    // case in order to add a person
    // that has been
    //                                 
    // end dated.  PR130132
    // ****************************************************************
    // 10/15/1998      Changed the following for Phase II work. rk
    // 1.   Future effective and discontinue dates can be altered
    // 2.   No person's suppression time frame should overlap with another.
    // 3.   A Child or Non-Case related person can't be set up with       a 
    // Suppression.
    // 4.   The display function will now bring 
    // back the Current             record if no
    // date is entered. Was just returning the
    // 
    // highest effective date record.
    // 5.   The Discontinue date must be greater than the Effective date.
    // 6.   If the record is already active then the effective date cannot be 
    // changed.  And an inactive record's dates can't be altered at all.
    // 7.   A suppression can only be deleted if the record isn't active yet or 
    // it was created today.
    // Changed the following for phase II integration testing RK 03/14/99.
    // 1.  If an invalid CSE Person number is entered and a flow to another 
    // screen is requested(pf key or NEXT) then an error message will be shown
    // saying that the screen must be cleared of the invalid number prior to
    // flowing.
    // Changed for PR #142898. If person selected to add is not an AR, error and
    // escape.
    // K Cole
    // : 11/25/02, pr# 148011, K Doshi - Fix Screen Help
    // ****************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

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
    export.PersonPrompt.Text1 = import.PersonPrompt.Text1;
    export.CsePerson.Number = import.CsePerson.Number;

    // *****  changes for WR 040796
    export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    export.CourtOrderPrompt.Text1 = import.CourtOrderPrompt.Text1;

    // *****  changes for WR 040796
    export.DisbSuppressionStatusHistory.EffectiveDate =
      import.DisbSuppressionStatusHistory.EffectiveDate;

    if (Equal(global.Command, "RETLINK"))
    {
      if (!IsEmpty(import.FromFlowCsePerson.Number))
      {
        export.CsePerson.Number = import.FromFlowCsePerson.Number;
      }

      global.Command = "DISPLAY";
    }

    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate1();

    if (Equal(global.Command, "RETCSENO"))
    {
      if (!IsEmpty(import.FromFlowCsePersonsWorkSet.Number))
      {
        MoveCsePersonsWorkSet(import.FromFlowCsePersonsWorkSet,
          export.CsePersonsWorkSet);
        export.CsePerson.Number = import.FromFlowCsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";

      // *****  changes for WR 040796
    }
    else if (Equal(global.Command, "RETLAPS"))
    {
      global.Command = "DISPLAY";
    }

    // *****  changes for WR 040796
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
      export.DisbSuppressionStatusHistory.Assign(
        import.DisbSuppressionStatusHistory);
      export.HiddenIdCsePerson.Number = import.HiddenIdCsePerson.Number;
      MoveDisbSuppressionStatusHistory(import.
        HiddenIdDisbSuppressionStatusHistory,
        export.HiddenIdDisbSuppressionStatusHistory);
      export.HiddenCsePersonsWorkSet.FormattedName =
        import.HiddenCsePersonsWorkSet.FormattedName;
      export.SuppressAll.Flag = import.SuppressAll.Flag;
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
      // then error off. rk 3/14/99.
      // ****************************************************************
      if (Equal(global.Command, "CSUP") || Equal(global.Command, "PACC") || Equal
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
        import.CsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumber = import.CsePerson.Number;
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
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
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
      }

      // *****  changes for WR 040796
      if (!IsEmpty(export.HiddenNextTranInfo.StandardCrtOrdNumber))
      {
        export.LegalAction.StandardNumber =
          export.HiddenNextTranInfo.StandardCrtOrdNumber ?? "";
      }

      // *****  changes for WR 040796
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "LDSP") || Equal
      (global.Command, "CSUP") || Equal(global.Command, "LPSP"))
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

    // Hardcode Area
    local.CsePersonAccount.Type1 = "E";

    // The logic assumes that a record cannot be
    // UPDATEd or DELETEd without first being displayed.
    // Therefore, a key change with either command is invalid.
    // *****  changes for WR 040796
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      (!Equal(export.CsePerson.Number, import.HiddenIdCsePerson.Number) || import
      .HiddenIdDisbSuppressionStatusHistory.SystemGeneratedIdentifier != import
      .DisbSuppressionStatusHistory.SystemGeneratedIdentifier))
    {
      // *****  changes for WR 040796
      ExitState = "FN0000_MUST_DISPLAY_BEFORE_CHNG";

      var field = GetField(export.CsePerson, "number");

      field.Error = true;

      return;
    }

    // If the key field is blank, certain
    // commands are not allowed.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE")) && IsEmpty
      (export.CsePerson.Number))
    {
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.CsePerson, "number");

      field.Error = true;

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

      // ***************************************************************
      // Protect the payee's number and prompt if a valid number has been 
      // entered and read from the database. rk 10/21
      // ***************************************************************
      if (!IsEmpty(export.CsePerson.Number))
      {
        var field1 = GetField(export.CsePerson, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.PersonPrompt, "text1");

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
      local.DisbSuppressionStatusHistory.Type1 = "P";

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
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        break;
      case "SIGNOFF":
        // use standard sign off cab here
        UseScCabSignoff();

        break;
      case "LIST":
        // *****  changes for WR 040796
        if (!IsEmpty(import.PersonPrompt.Text1) && !
          IsEmpty(import.CourtOrderPrompt.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        // *****  changes for WR 040796
        if (AsChar(import.PersonPrompt.Text1) == 'S')
        {
          export.PersonPrompt.Text1 = "+";
          ExitState = "ECO_LNK_TO_SELECT_PERSON";

          return;

          // *****  changes for WR 040796
        }
        else if (AsChar(import.CourtOrderPrompt.Text1) == 'S')
        {
          export.CourtOrderPrompt.Text1 = "+";
          ExitState = "ECO_LNK_TO_LAPS";
        }
        else if (!IsEmpty(import.PersonPrompt.Text1))
        {
          var field = GetField(export.PersonPrompt, "text1");

          field.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE2";

          return;
        }
        else if (!IsEmpty(import.CourtOrderPrompt.Text1))
        {
          var field = GetField(export.CourtOrderPrompt, "text1");

          field.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE2";
        }
        else
        {
          var field1 = GetField(export.CourtOrderPrompt, "text1");

          field1.Error = true;

          var field2 = GetField(export.PersonPrompt, "text1");

          field2.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

          return;
        }

        // *****  changes for WR 040796
        break;
      case "DISPLAY":
        export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;
        export.DisbSuppressionStatusHistory.ReasonText =
          Spaces(DisbSuppressionStatusHistory.ReasonText_MaxLength);
        export.DisbSuppressionStatusHistory.LastUpdatedBy = "";

        // *****  changes for WR 040796
        UseFnReadPersonDisbSuppression();

        // *****  changes for WR 040796
        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("FN0000_ACTIVE_AND_FUTURE_SUPPRS"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenIdCsePerson.Number = export.CsePerson.Number;
          MoveDisbSuppressionStatusHistory(export.DisbSuppressionStatusHistory,
            export.HiddenIdDisbSuppressionStatusHistory);

          // *****  changes for WR 040796
          export.HiddenIdLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;

          // *****  changes for WR 040796
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else
        {
          if (IsExitState("FN0000_DISB_SUPP_NF_FOR_DATE"))
          {
            var field =
              GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

            field.Error = true;
          }
          else if (IsExitState("LEGAL_ACTION_NF_RB"))
          {
            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;
          }

          // Set the hidden key field to spaces or zero.
          export.HiddenIdCsePerson.Number = "";
          export.HiddenIdDisbSuppressionStatusHistory.
            SystemGeneratedIdentifier = 0;

          // *****  changes for WR 040796
          export.HiddenIdLegalAction.StandardNumber = "";

          // *****  changes for WR 040796
        }

        break;
      case "ADD":
        if (!ReadCaseRole())
        {
          ExitState = "FN0000_AR_CSE_PERSON_NF";

          return;
        }

        UseFnCrePersonDisbSuppression();

        if (IsExitState("LEGAL_ACTION_NF_RB"))
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0000_DATE_OVERLAP_ERROR"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field1.Error = true;

          var field2 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *****  changes for WR 040796
          local.Hold.Assign(export.DisbSuppressionStatusHistory);
          local.Hold.Type1 = "P";

          // *****  changes for WR 040796
          // No exit states are returned from cab below
          // *****  changes for WR 040796
          UseFnCheckForOtherDispSupp();

          // *****  changes for WR 040796
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_ADD_2";

          // Set the hidden key field to that of the new record.
          export.HiddenIdCsePerson.Number = export.CsePerson.Number;
          MoveDisbSuppressionStatusHistory(export.DisbSuppressionStatusHistory,
            export.HiddenIdDisbSuppressionStatusHistory);

          // *****  changes for WR 040796
          export.HiddenIdLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;

          // *****  changes for WR 040796
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
        UseFnUpdPersonDisbSuppression();

        if (Equal(export.DisbSuppressionStatusHistory.DiscontinueDate,
          local.MaxDate.Date))
        {
          export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;
        }

        if (IsExitState("FN0000_DATE_OVERLAP_ERROR"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field1.Error = true;

          var field2 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field2.Error = true;
        }
        else if (IsExitState("FN0000_DATE_OVERLAP_FOR_TYPE"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field1.Error = true;

          var field2 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // No exit states are returned from cab below
          MoveDisbSuppressionStatusHistory(export.DisbSuppressionStatusHistory,
            export.HiddenIdDisbSuppressionStatusHistory);

          // *****  changes for WR 040796
          export.HiddenIdLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;
          local.Hold.Assign(export.DisbSuppressionStatusHistory);
          local.Hold.Type1 = "P";

          // *****  changes for WR 040796
          UseFnCheckForOtherDispSupp();
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_2";
        }
        else if (IsExitState("FN0000_CANNOT_CHG_EFF_DT"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field.Error = true;
        }
        else if (IsExitState("CANNOT_CHANGE_A_DISCONTINUED_REC"))
        {
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_NU"))
        {
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_PV"))
        {
        }
        else if (IsExitState("FN0000_DISB_SUPP_STAT_NF"))
        {
          var field1 =
            GetField(export.DisbSuppressionStatusHistory, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.CsePerson, "number");

          field2.Error = true;
        }
        else if (IsExitState("FN0000_SUPPR_MUST_RELE_IN_FUTURE"))
        {
          var field =
            GetField(export.DisbSuppressionStatusHistory, "discontinueDate");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "DELETE":
        UseFnDelPersonDisbSuppression();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to spaces or zero.
          export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;
          export.DisbSuppressionStatusHistory.ReasonText =
            Spaces(DisbSuppressionStatusHistory.ReasonText_MaxLength);
          export.DisbSuppressionStatusHistory.LastUpdatedBy = "";
          export.HiddenIdDisbSuppressionStatusHistory.
            SystemGeneratedIdentifier = 0;
          ExitState = "FN0000_DELETE_SUCCESSFUL";
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
      case "CSUP":
        ExitState = "ECO_XFR_TO_MTN_COLL_DISB_SUPPR";

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
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDisbSuppressionStatusHistory(
    DisbSuppressionStatusHistory source, DisbSuppressionStatusHistory target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
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

  private void UseFnCheckForOtherDispSupp()
  {
    var useImport = new FnCheckForOtherDispSupp.Import();
    var useExport = new FnCheckForOtherDispSupp.Export();

    useImport.DisbSuppressionStatusHistory.Assign(local.Hold);
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnCheckForOtherDispSupp.Execute, useImport, useExport);

    export.SuppressAll.Flag = useExport.SuppressedAll.Flag;
  }

  private void UseFnCrePersonDisbSuppression()
  {
    var useImport = new FnCrePersonDisbSuppression.Import();
    var useExport = new FnCrePersonDisbSuppression.Export();

    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.DisbSuppressionStatusHistory.Assign(
      local.DisbSuppressionStatusHistory);
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnCrePersonDisbSuppression.Execute, useImport, useExport);

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

  private void UseFnReadPersonDisbSuppression()
  {
    var useImport = new FnReadPersonDisbSuppression.Import();
    var useExport = new FnReadPersonDisbSuppression.Export();

    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.DisbSuppressionStatusHistory.Assign(
      export.DisbSuppressionStatusHistory);

    Call(FnReadPersonDisbSuppression.Execute, useImport, useExport);

    export.SuppressAll.Flag = useExport.SuppressAll.Flag;
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

  private void UseFnUpdPersonDisbSuppression()
  {
    var useImport = new FnUpdPersonDisbSuppression.Import();
    var useExport = new FnUpdPersonDisbSuppression.Export();

    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.DisbSuppressionStatusHistory.Assign(
      local.DisbSuppressionStatusHistory);
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnUpdPersonDisbSuppression.Execute, useImport, useExport);

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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
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
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
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
    /// <summary>
    /// A value of FromFlowEffectiveDate.
    /// </summary>
    [JsonPropertyName("fromFlowEffectiveDate")]
    public DisbSuppressionStatusHistory FromFlowEffectiveDate
    {
      get => fromFlowEffectiveDate ??= new();
      set => fromFlowEffectiveDate = value;
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
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
    }

    /// <summary>
    /// A value of ObligationCount.
    /// </summary>
    [JsonPropertyName("obligationCount")]
    public Common ObligationCount
    {
      get => obligationCount ??= new();
      set => obligationCount = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    /// A value of HiddenIdLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenIdLegalAction")]
    public LegalAction HiddenIdLegalAction
    {
      get => hiddenIdLegalAction ??= new();
      set => hiddenIdLegalAction = value;
    }

    private DisbSuppressionStatusHistory fromFlowEffectiveDate;
    private TextWorkArea personPrompt;
    private CsePersonsWorkSet fromFlowCsePersonsWorkSet;
    private CsePerson fromFlowCsePerson;
    private Common caseCount;
    private Common obligationCount;
    private Common suppressAll;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson hiddenIdCsePerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private DisbSuppressionStatusHistory hiddenIdDisbSuppressionStatusHistory;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private LegalAction legalAction;
    private TextWorkArea courtOrderPrompt;
    private LegalAction hiddenIdLegalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
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
    /// A value of SuppressAll.
    /// </summary>
    [JsonPropertyName("suppressAll")]
    public Common SuppressAll
    {
      get => suppressAll ??= new();
      set => suppressAll = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    /// A value of HiddenIdLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenIdLegalAction")]
    public LegalAction HiddenIdLegalAction
    {
      get => hiddenIdLegalAction ??= new();
      set => hiddenIdLegalAction = value;
    }

    private TextWorkArea personPrompt;
    private Common caseCount;
    private Common obligactionCount;
    private Common suppressAll;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson hiddenIdCsePerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private DisbSuppressionStatusHistory hiddenIdDisbSuppressionStatusHistory;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private LegalAction legalAction;
    private TextWorkArea courtOrderPrompt;
    private LegalAction hiddenIdLegalAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public DisbSuppressionStatusHistory Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    private DisbSuppressionStatusHistory hold;
    private External external;
    private DateWorkArea maxDate;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePersonAccount csePersonAccount;
    private DateWorkArea zero;
    private DateWorkArea dateWorkArea;
    private TextWorkArea leftPadding;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private CsePersonAccount obligee;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}

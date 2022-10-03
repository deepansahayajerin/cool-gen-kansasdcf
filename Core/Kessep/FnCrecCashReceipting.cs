// Program: FN_CREC_CASH_RECEIPTING, ID: 371721869, model: 746.
// Short name: SWECRECP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREC_CASH_RECEIPTING.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrecCashReceipting: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREC_CASH_RECEIPTING program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrecCashReceipting(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrecCashReceipting.
  /// </summary>
  public FnCrecCashReceipting(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------
    // Every initial development and change to that development
    // needs to be documented below:
    // Date 	  	Developer Name		Description
    // 02/01/96	Holly Kennedy-MTW	Retrofits
    // 04/02/96	Holly Kennedy-MTW	Added worker id on screen.
    // 04/02/96	Holly Kennedy-MTW	Changed logic reading against
    // 					initialized timestamp view.
    // 					set the date view to 							0001-01-01-00.00.00.000000
    // 10/02/96	SHERAZ MALIK-MTW	IDCRS
    // 11/26/96	R. Marchman		Add new security and next tran
    // 06/11/1997	A Samuels		Removed INTF status from display;
    // 					Added new display field 'Net
    // 					Interface Amount'
    // 07/01/1997	A Samuels		Clear data fields after
    //  					a successful add
    // 10/09/1998	J. Katz			Add data validation logic.
    // 					Provide ability to create
    // 					manual interface receipt.
    // 					Implement modifications to
    // 					accommodate EFT requirements.
    // 					Add program documentation.
    // 12/12/1998	J. Katz			Add logic for new Cash Receipt
    // 					 - Payor SSN attribute.
    // 02/01/1999      J. Katz			Added PF Key and logic to support
    // 					flow to MTRN.
    // ----------------------------------------------------------------------------
    // INTEGRATION TEST MODIFICATIONS
    // ----------------------------------------------------------------------------
    // Date 	  	Developer Name		Description
    // ----------------------------------------------------------------------------
    // 03/01/1999      J. Katz			Modified logic to accommodate
    // 					the use of Bogus receipts to
    // 					reflect a refund on an interface
    // 					account balance and to support
    // 					the Unmatch functionality
    // 					on CRMI.
    // 05/05/99	J. Katz-SRG		Move protection logic to the bottom
    // 					of the procedure and remove
    // 					protection logic duplicated
    // 					throughout the code.
    // 					Modify protection logic to allow
    // 					the NOTE field to be modified
    // 					when the status is FWD, DEP, or DEL.
    // 					Modify protection logic to allow
    // 					the NOTE field to be modified when
    // 					the receipt type is EFT.
    // 					Modify protection logic to allow
    // 					the NOTE field to be modified when
    // 					the receipt is created by CONVERSN.
    // 05/14/99	J. Katz-SRG		Edit logic to allow CSENet type
    // 					receipts to be created and
    // 					updated on CREC.
    // 06/03/99	J. Katz-SRG		Analyzed READ statements and
    // 					changed read property to
    // 					Select Only where appropriate.
    // 					Added edit to prevent flow to
    // 					CRMI when status is FWD.
    // 07/26/99	J. Katz - SRG	
    // 	Added logic to verify that the
    // 		PR # 109		Check Number is all numeric prior
    // 					to using the numtext function.  If
    // 					the Check Number is not all
    // 					numeric, display error message.
    // ----------------------------------------------------------------------------
    // SYSTEM TEST MODIFICATIONS
    // ----------------------------------------------------------------------------
    // Date 	  	Developer Name		Description
    // ----------------------------------------------------------------------------
    // 07/26/99	J. Katz - SRG	
    // 	Added logic to verify that the
    // 		PR # 109		Check Number is all numeric prior
    // 					to using the numtext function.  If
    // 					the Check Number is not all
    // 					numeric, display error message.
    // 09/06/99	J. Katz - SRG		Allow user to update court
    // 					interface receipts in INTF
    // 					status with a new Cash Due
    // 					[Net Interface Amount].
    // ----------------------------------------------------------------------------
    // 11/06/99	P.Phinney - SRG   H00070503   Added TEMPORARY logic 
    // FIXes to REMOVE 45 DAY check.
    // ---------------------------------------------------------------
    // 11/25/14	GVandy	CQ42192		"Check" cash receipt type must be used with CSSI
    // 					source type.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    var field = GetField(export.CashReceipt, "cashDue");

    field.Color = "cyan";
    field.Protected = true;

    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Low.Date = new DateTime(1, 1, 1);

    // ------------------------------------------------------------------
    // If the cash receipt information from unmatching an interface
    // receipt has not been processed, only allow the List and Add
    // actions.  The RETCDVL, FROMCRSL, and FROMCRTL commands may
    // also be executed since they support the List action.
    // JLK  03/01/99
    // ------------------------------------------------------------------
    if (Equal(import.LastAction.Command, "UNMATCH"))
    {
      if (Equal(global.Command, "LIST") || Equal(global.Command, "ADD") || Equal
        (global.Command, "RETCDVL") || Equal(global.Command, "FROMCRSL") || Equal
        (global.Command, "FROMCRTL"))
      {
        export.LastAction.Command = import.LastAction.Command;
      }
      else
      {
        if (Equal(global.Command, "DISPLAY") || IsEmpty(global.Command))
        {
          // ---------------------------------------------------------
          // Need to read the Cash Receipt Type imported from CRMI to
          // obtain the Type Code.    JLK  03/01/99
          // ---------------------------------------------------------
          if (ReadCashReceiptType2())
          {
            export.CashReceiptType.Assign(entities.ExistingCashReceiptType);

            if (IsEmpty(global.Command))
            {
              ExitState = "FN0000_MUST_PROCESS_CASH_RCPT";
            }
          }
          else
          {
            export.CashReceiptType.Assign(import.CashReceiptType);

            var field1 = GetField(export.CashReceiptType, "code");

            field1.Error = true;

            ExitState = "FN0000_CASH_RECEIPT_TYPE_NF";
          }
        }
        else
        {
          export.CashReceiptType.Assign(import.CashReceiptType);
        }

        export.CashReceiptSourceType.Assign(import.CashReceiptSourceType);
        export.CashReceiptEvent.Assign(import.CashReceiptEvent);
        export.CashReceipt.Assign(import.CashReceipt);
        export.LastAction.Command = import.LastAction.Command;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_MUST_PROCESS_UNMTCH_RCPT";
        }

        return;
      }
    }

    if (Equal(global.Command, "CLEAR"))
    {
      export.CashReceiptType.Code = "CHECK";

      var field1 = GetField(export.CashReceiptSourceType, "code");

      field1.Protected = false;
      field1.Focused = true;

      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // --------------------------------------------------------------------
    // Move import views to export views.
    // --------------------------------------------------------------------
    export.CashReceiptSourceType.Assign(import.CashReceiptSourceType);
    export.CashReceiptEvent.Assign(import.CashReceiptEvent);
    export.CashReceiptType.Assign(import.CashReceiptType);
    export.CashReceipt.Assign(import.CashReceipt);
    MoveCashReceiptStatus(import.CashReceiptStatus, export.CashReceiptStatus);
    export.HiddenCashReceiptSourceType.
      Assign(import.HiddenCashReceiptSourceType);
    export.HiddenCashReceiptEvent.Assign(import.HiddenCashReceiptEvent);
    MoveCashReceiptType1(import.HiddenCashReceiptType,
      export.HiddenCashReceiptType);
    export.HiddenCashReceipt.Assign(import.HiddenCashReceipt);
    export.HiddenNoOfCrDetails.Count = import.HiddenNoOfCrDetails.Count;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // --------------------------------------------------------------------
    // If the next tran info is not equal to spaces, this implies the
    // user requested a next tran action.  Now validate.
    // --------------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    // -----------------------------------------------------
    // Perform security check for all commands resulting in
    // database actions.
    // -----------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
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

    // -----------------------------------------------------
    // Populate local views with hardcoded Cash Receipting
    // information.
    // -----------------------------------------------------
    UseFnHardcodedCashReceipting();

    // -------------------------------------------------------
    // Data Validation Edits for Required Data
    // -------------------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "COLL") || Equal(global.Command, "MTRN"))
    {
      if (export.CashReceipt.SequentialNumber != export
        .HiddenCashReceipt.SequentialNumber || export
        .CashReceipt.SequentialNumber == 0)
      {
        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        global.Command = "";
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.CashReceiptSourceType.Code))
      {
        ExitState = "FN0000_MANDATORY_FIELDS";

        var field1 = GetField(export.CashReceiptSourceType, "code");

        field1.Error = true;
      }

      if (IsEmpty(export.CashReceiptType.Code))
      {
        ExitState = "FN0000_MANDATORY_FIELDS";

        var field1 = GetField(export.CashReceiptType, "code");

        field1.Error = true;
      }

      if (IsEmpty(export.CashReceipt.CheckType))
      {
        ExitState = "FN0000_MANDATORY_FIELDS";

        var field1 = GetField(export.CashReceipt, "checkType");

        field1.Error = true;
      }

      if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
      {
        if (Equal(global.Command, "ADD"))
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          goto Test1;
        }

        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtManint.SystemGeneratedIdentifier)
        {
          if (export.CashReceipt.CashDue.GetValueOrDefault() <= 0)
          {
            ExitState = "FN0000_CASH_DUE_MUST_BE_GT_ZERO";
          }
        }
      }
      else if (export.CashReceipt.ReceiptAmount == 0)
      {
        // -----------------------------------------------------------
        // If the receipt is in DEL status or is a BOGUS type receipt,
        // the Receipt Amount can be zero.        JLK  02/24/99
        // -----------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier != local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier && !
          Equal(export.CashReceipt.CheckType, "BOGUS"))
        {
          // ---------------------------------------------------------
          // If the receipt is a court interface receipt and the Cash
          // Due is less than or equal to zero, the Receipt Amount
          // will remain zero.        JLK  09/06/99
          // ---------------------------------------------------------
          if (AsChar(export.CashReceiptSourceType.CourtInd) == 'C' && Lt
            (local.Low.Date, export.CashReceiptEvent.SourceCreationDate) && export
            .CashReceipt.CashDue.GetValueOrDefault() <= 0)
          {
            goto Test1;
          }

          ExitState = "FN0000_MANDATORY_FIELDS";

          var field1 = GetField(export.CashReceipt, "receiptAmount");

          field1.Error = true;
        }
      }
      else if (export.CashReceipt.ReceiptAmount < 0 && IsExitState
        ("ACO_NN0000_ALL_OK"))
      {
        ExitState = "FN0128_CASH_RCPT_AMOUNT_LT_ZERO";

        var field1 = GetField(export.CashReceipt, "receiptAmount");

        field1.Error = true;
      }
      else if (Equal(export.CashReceipt.CheckType, "BOGUS") && IsExitState
        ("ACO_NN0000_ALL_OK"))
      {
        ExitState = "FN0000_BOGUS_CR_REQ_ZER_RCPT_AMT";

        var field1 = GetField(export.CashReceipt, "receiptAmount");

        field1.Error = true;
      }

Test1:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "";

        goto Test2;
      }

      // -----------------------------------------------------------------
      // SET DEFAULT date for received date if it is not supplied.
      // Edit to make sure the receipt and received dates are
      // chronologically in the correct order.
      // Make received date mandatory for MANUAL INTERFACES.
      // As per meeting with Finance Staff on 01/09/98 - Venkatesh Kamara
      // -----------------------------------------------------------------
      if (Equal(export.CashReceiptType.Code, "MANINT"))
      {
        if (Equal(export.CashReceiptEvent.ReceivedDate,
          local.ClearCashReceiptEvent.ReceivedDate))
        {
          var field1 = GetField(export.CashReceiptEvent, "receivedDate");

          field1.Error = true;

          ExitState = "FN0000_MANDATORY_FIELDS";
        }
      }
      else if (Equal(export.CashReceiptEvent.ReceivedDate,
        local.ClearCashReceiptEvent.ReceivedDate))
      {
        export.CashReceiptEvent.ReceivedDate = Now().Date;
      }

      // -------------------------------------------------------
      // Validation Edits for Quality of Data
      // Received Date cannot be a future date.
      // -------------------------------------------------------
      if (Lt(local.Current.Date, export.CashReceiptEvent.ReceivedDate))
      {
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        var field1 = GetField(export.CashReceiptEvent, "receivedDate");

        field1.Error = true;
      }
      else
      {
        local.PassArea.ReceivedDate = export.CashReceiptEvent.ReceivedDate;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "";

        goto Test2;
      }

      // -------------------------------------------------------
      // SSN can only be entered for non-CSE Program Fund Types.
      // SSN can only contain numbers or spaces.
      // -------------------------------------------------------
      if (Equal(export.CashReceipt.CheckType, "CSE") && !
        IsEmpty(export.CashReceipt.PayorSocialSecurityNumber))
      {
        var field1 = GetField(export.CashReceipt, "payorSocialSecurityNumber");

        field1.Error = true;

        ExitState = "FN0000_SSN_ONLY_VALID_FOR_NONCSE";
        global.Command = "";

        goto Test2;
      }

      if (Verify(export.CashReceipt.PayorSocialSecurityNumber, " 0123456789") !=
        0)
      {
        var field1 = GetField(export.CashReceipt, "payorSocialSecurityNumber");

        field1.Error = true;

        ExitState = "FN0000_SSN_CONTAINS_INVALID_DATA";
        global.Command = "";

        goto Test2;
      }

      // -------------------------------------------------------
      // If the cash receipt type code is BOGUS, the program
      // fund type code must also be BOGUS, and vice versa.
      // JLK  02/24/99
      // -------------------------------------------------------
      if (Equal(export.CashReceiptType.Code, "BOGUS") && !
        Equal(export.CashReceipt.CheckType, "BOGUS"))
      {
        var field1 = GetField(export.CashReceipt, "checkType");

        field1.Error = true;

        ExitState = "FN0000_CR_TYP_CHK_TYP_MISMTC";
      }

      if (!Equal(export.CashReceiptType.Code, "BOGUS") && Equal
        (export.CashReceipt.CheckType, "BOGUS"))
      {
        var field1 = GetField(export.CashReceiptType, "code");

        field1.Error = true;

        ExitState = "FN0000_CR_TYP_CHK_TYP_MISMTC";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "";
      }
    }

Test2:

    // -------------------------------------------
    // Main CASE OF COMMAND
    // -------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RETCDVL":
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.CashReceipt.CheckType = import.Selected.Cdvalue;
        }

        export.CheckTypePrompt.PromptField = "";

        var field1 = GetField(export.CashReceipt, "checkType");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;
        field1.Focused = true;

        break;
      case "FROMCRSL":
        if (IsEmpty(import.CashReceiptSourceType.Code))
        {
          MoveCashReceiptSourceType(export.HiddenCashReceiptSourceType,
            export.CashReceiptSourceType);
        }

        export.Prompts.SourceTypePrompt.ActionEntry = "";

        var field2 = GetField(export.CashReceiptType, "code");

        field2.Protected = false;
        field2.Focused = true;

        break;
      case "FROMCRTL":
        if (IsEmpty(import.CashReceiptType.Code))
        {
          MoveCashReceiptType1(export.HiddenCashReceiptType,
            export.CashReceiptType);
        }

        export.Prompts.CashRcptTypePrompt.ActionEntry = "";

        var field3 = GetField(export.CashReceiptEvent, "receivedDate");

        field3.Protected = false;
        field3.Focused = true;

        break;
      case "LIST":
        // -----------------------------------------------------------------
        // PF4  List
        // -----------------------------------------------------------------
        local.NumPromptsSelected.Count = 0;

        if (Equal(import.Prompts.SourceTypePrompt.ActionEntry, "S"))
        {
          ++local.NumPromptsSelected.Count;
          export.HiddenCashReceiptSourceType.
            Assign(export.CashReceiptSourceType);
          export.Prompts.SourceTypePrompt.ActionEntry =
            import.Prompts.SourceTypePrompt.ActionEntry;

          var field4 = GetField(export.Prompts.SourceTypePrompt, "actionEntry");

          field4.Error = true;

          ExitState = "ECO_LNK_LST_CASH_SOURCES";
        }
        else if (IsEmpty(import.Prompts.SourceTypePrompt.ActionEntry))
        {
        }
        else
        {
          export.Prompts.SourceTypePrompt.ActionEntry =
            import.Prompts.SourceTypePrompt.ActionEntry;

          var field4 = GetField(export.Prompts.SourceTypePrompt, "actionEntry");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (Equal(import.Prompts.CashRcptTypePrompt.ActionEntry, "S"))
        {
          ++local.NumPromptsSelected.Count;
          MoveCashReceiptType1(export.CashReceiptType,
            export.HiddenCashReceiptType);
          export.Prompts.CashRcptTypePrompt.ActionEntry =
            import.Prompts.CashRcptTypePrompt.ActionEntry;

          var field4 =
            GetField(export.Prompts.CashRcptTypePrompt, "actionEntry");

          field4.Error = true;

          ExitState = "ECO_LNK_LST_CASH_TYPES";
        }
        else if (IsEmpty(import.Prompts.CashRcptTypePrompt.ActionEntry))
        {
        }
        else
        {
          export.Prompts.CashRcptTypePrompt.ActionEntry =
            import.Prompts.CashRcptTypePrompt.ActionEntry;

          var field4 =
            GetField(export.Prompts.CashRcptTypePrompt, "actionEntry");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (AsChar(import.CheckTypePrompt.PromptField) == 'S')
        {
          ++local.NumPromptsSelected.Count;
          export.PassCode.CodeName = "CHECK TYPE";
          export.CheckTypePrompt.PromptField =
            import.CheckTypePrompt.PromptField;

          var field4 = GetField(export.CheckTypePrompt, "promptField");

          field4.Error = true;

          ExitState = "ECO_LNK_TO_CODE_VALUES";
        }
        else if (IsEmpty(import.CheckTypePrompt.PromptField))
        {
        }
        else
        {
          export.CheckTypePrompt.PromptField =
            import.CheckTypePrompt.PromptField;

          var field4 = GetField(export.CheckTypePrompt, "promptField");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (local.NumPromptsSelected.Count == 0)
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          if (IsEmpty(export.CashReceiptStatus.Code))
          {
            var field4 =
              GetField(export.Prompts.SourceTypePrompt, "actionEntry");

            field4.Color = "red";
            field4.Intensity = Intensity.High;
            field4.Highlighting = Highlighting.ReverseVideo;
            field4.Protected = false;
            field4.Focused = true;
          }
          else if (import.HiddenNoOfCrDetails.Count == 0)
          {
            var field4 = GetField(export.CheckTypePrompt, "promptField");

            field4.Color = "red";
            field4.Intensity = Intensity.High;
            field4.Highlighting = Highlighting.ReverseVideo;
            field4.Protected = false;
            field4.Focused = true;
          }
          else
          {
            ExitState = "FN0000_LIST_INVALID_FOR_SEL_RCPT";
          }
        }
        else if (local.NumPromptsSelected.Count == 1)
        {
          // ----------------------------------------------
          // Flow to appropriate list screen.
          // ----------------------------------------------
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        break;
      case "DISPLAY":
        // -----------------------------------------------------------------
        // PF2  Display
        // -----------------------------------------------------------------
        if ((export.CashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .CashReceiptType.SystemGeneratedIdentifier == 0) && export
          .CashReceipt.SequentialNumber == 0)
        {
          var field4 = GetField(export.CashReceipt, "sequentialNumber");

          field4.Error = true;

          MoveCashReceiptSourceType(local.ClearCashReceiptSourceType,
            export.CashReceiptSourceType);
          export.CashReceiptType.Assign(local.ClearCashReceiptType);
          MoveCashReceipt3(local.ClearCashReceipt, export.CashReceipt);
          export.CashReceiptEvent.Assign(local.ClearCashReceiptEvent);
          MoveCashReceiptStatus(local.ClearCashReceiptStatus,
            export.CashReceiptStatus);
          ExitState = "FN0000_CANT_DISPLAY_CASH_RECEIPT";

          return;
        }

        UseDisplayCashReceipt();
        export.HiddenCashReceiptSourceType.Code = "";
        export.HiddenCashReceiptType.Code = "";
        export.HiddenNoOfCrDetails.Count = local.NoOfCrDetails.Count;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          MoveCashReceipt2(local.PassArea, export.CashReceipt);
          export.HiddenCashReceiptSourceType.
            Assign(export.CashReceiptSourceType);
          export.HiddenCashReceiptEvent.Assign(export.CashReceiptEvent);
          MoveCashReceiptType1(export.CashReceiptType,
            export.HiddenCashReceiptType);
          export.HiddenCashReceipt.Assign(export.CashReceipt);
        }
        else
        {
          var field4 = GetField(export.CashReceipt, "sequentialNumber");

          field4.Error = true;
        }

        break;
      case "ADD":
        // -----------------------------------------------------------------
        // PF5  Add
        // -----------------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier > 0)
        {
          ExitState = "FN0000_CLEAR_BEFORE_ADD";

          break;
        }

        // -----------------------------------------------------------------
        // Edit to make sure the receipt and received dates are
        // chronologically in the correct order.
        // -----------------------------------------------------------------
        export.CashReceipt.ReceiptDate = Now().Date;

        if (Lt(export.CashReceipt.ReceiptDate,
          export.CashReceiptEvent.ReceivedDate))
        {
          var field4 = GetField(export.CashReceiptEvent, "receivedDate");

          field4.Error = true;

          ExitState = "CANNOT_RECEIPT_BEFORE_RECEIVING";

          return;
        }

        // ----------------------------------------------------------------
        // Cannot enter Cash Receipt Number
        // JLK    09/29/98
        // ----------------------------------------------------------------
        if (export.CashReceipt.SequentialNumber > 0)
        {
          var field4 = GetField(export.CashReceipt, "sequentialNumber");

          field4.Error = true;

          ExitState = "FN0000_DISB_AMT_GRTR";

          return;
        }

        // ----------------------------------------------------------------
        // VALIDATE the source type to make sure it is active.
        // Also retrieves the identifier for use by the pad.
        // ----------------------------------------------------------------
        if (ReadCashReceiptSourceType())
        {
          export.CashReceiptSourceType.Assign(
            entities.ExistingCashReceiptSourceType);

          if (!Lt(export.CashReceiptEvent.ReceivedDate,
            entities.ExistingCashReceiptSourceType.EffectiveDate) && Lt
            (export.CashReceiptEvent.ReceivedDate,
            entities.ExistingCashReceiptSourceType.DiscontinueDate))
          {
          }
          else
          {
            var field4 = GetField(export.CashReceiptSourceType, "code");

            field4.Error = true;

            ExitState = "CASH_RCPT_SRC_TYP_EFF_DT_NOT_ACT";

            return;
          }
        }
        else
        {
          var field4 = GetField(export.CashReceiptSourceType, "code");

          field4.Error = true;

          ExitState = "FN0000_CR_SOURCE_TYPE_NF_RB";

          return;
        }

        // ----------------------------------------------------------
        // VALIDATE the cash receipt type to make sure it is active.
        // Also retrieves the identifier for use by the pad.
        // ----------------------------------------------------------
        if (ReadCashReceiptType1())
        {
          export.CashReceiptType.Assign(entities.ExistingCashReceiptType);

          if (!Lt(export.CashReceipt.ReceiptDate,
            entities.ExistingCashReceiptType.EffectiveDate) && Lt
            (export.CashReceipt.ReceiptDate,
            entities.ExistingCashReceiptType.DiscontinueDate))
          {
          }
          else
          {
            var field4 = GetField(export.CashReceiptType, "code");

            field4.Error = true;

            ExitState = "FN0115_CASH_RCPT_TYPE_NOT_ACTIVE";

            return;
          }

          if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtEft.SystemGeneratedIdentifier)
          {
            var field4 = GetField(export.CashReceiptType, "code");

            field4.Error = true;

            ExitState = "FN0305_INVALID_CR_TYP_FOR_AUD";

            return;
          }

          // 11/25/14  GVandy  CQ42192  "Check" cash receipt type must be used 
          // with CSSI source type.
          if (Equal(import.CashReceiptSourceType.Code, "CSSI") && import
            .CashReceiptType.SystemGeneratedIdentifier != local
            .HardcodedCrtCheck.SystemGeneratedIdentifier)
          {
            var field4 = GetField(export.CashReceiptType, "code");

            field4.Error = true;

            ExitState = "FN0000_CSSI_SOURCE_TYPE_W_CHECK";

            return;
          }

          if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtCheck.SystemGeneratedIdentifier || entities
            .ExistingCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtMoneyOrder.SystemGeneratedIdentifier || entities
            .ExistingCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtInterfund.SystemGeneratedIdentifier)
          {
            // -----------------------------------------------------------
            // The Check Number and Check Date must be entered if
            // the Cash Receipt Type is CHECK or MONEY ORDER.
            // Check Number and Check Date are also mandatory for
            // Cash Receipt Type INTERFUND.   JLK  05/28/99
            // -----------------------------------------------------------
            if (IsEmpty(export.CashReceipt.CheckNumber))
            {
              var field4 = GetField(export.CashReceipt, "checkNumber");

              field4.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            if (Equal(export.CashReceipt.CheckDate,
              local.ClearCashReceipt.CheckDate))
            {
              var field4 = GetField(export.CashReceipt, "checkDate");

              field4.Error = true;

              ExitState = "FN0000_CHECK_DATE_MISSING";

              return;
            }
          }
          else if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier ==
            local.HardcodedCrtCurrency.SystemGeneratedIdentifier)
          {
            // -----------------------------------------------------------
            // The Payor Name or Payor Organization must be entered if
            // the Cash Receipt Type is CURRENCY.    JLK  09/29/98
            // -----------------------------------------------------------
            if ((IsEmpty(export.CashReceipt.PayorFirstName) || IsEmpty
              (export.CashReceipt.PayorLastName)) && IsEmpty
              (export.CashReceipt.PayorOrganization))
            {
              var field4 = GetField(export.CashReceipt, "payorFirstName");

              field4.Error = true;

              var field5 = GetField(export.CashReceipt, "payorLastName");

              field5.Error = true;

              var field6 = GetField(export.CashReceipt, "payorOrganization");

              field6.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }
          }

          // -----------------------------------------------------------
          // A BOGUS receipt type is only valid for interface
          // source codes.        JLK  02/24/99
          // -----------------------------------------------------------
          if (Equal(export.CashReceiptType.Code, "BOGUS") && Equal
            (export.CashReceipt.CheckType, "BOGUS"))
          {
            if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) != 'Y')
            {
              var field4 = GetField(export.CashReceiptSourceType, "code");

              field4.Error = true;

              ExitState = "FN0000_BOGUS_CR_ONLY_4_INTF_SRC";
            }
          }

          // -----------------------------------------------------------
          // A Cash Receipt Type of MANINT must have a Source Code
          // with an Interface Indicator of Y.       JLK  09/29/98
          // -----------------------------------------------------------
          if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtManint.SystemGeneratedIdentifier && AsChar
            (entities.ExistingCashReceiptSourceType.InterfaceIndicator) != 'Y')
          {
            export.CashReceipt.ReceiptDate = local.ClearCashReceipt.ReceiptDate;

            var field4 = GetField(export.CashReceiptSourceType, "code");

            field4.Error = true;

            var field5 = GetField(export.CashReceiptType, "code");

            field5.Error = true;

            ExitState = "FN0000_SRC_TYPE_INVALID_4_CR_TY";

            return;
          }

          // -----------------------------------------------------------
          // Only non-cash receipts with a Cash Receipt Type Code of
          // RDIR PMT and BOGUS may be added on CREC.
          // CSENet type receipts can also be added on CREC.  JLK 05/14/99
          // -----------------------------------------------------------
          if (AsChar(entities.ExistingCashReceiptType.CategoryIndicator) == AsChar
            (local.HardcodedCrtCatNonCash.CategoryIndicator))
          {
            if (export.CashReceiptType.SystemGeneratedIdentifier != local
              .HardcodedCrtRdirPmt.SystemGeneratedIdentifier && export
              .CashReceiptType.SystemGeneratedIdentifier != local
              .HardcodedCrtCsenet.SystemGeneratedIdentifier && !
              Equal(export.CashReceiptType.Code, "BOGUS"))
            {
              var field4 = GetField(export.CashReceiptType, "code");

              field4.Error = true;

              ExitState = "FN0305_INVALID_CR_TYP_FOR_AUD";

              return;
            }
          }
        }
        else
        {
          ExitState = "FN0113_CASH_RCPT_TYPE_NF";

          var field4 = GetField(export.CashReceiptType, "code");

          field4.Error = true;

          return;
        }

        // -----------------------------------------------------------------
        // If the Cash Receipt Type is MANINT and Cash Receipt Source Type
        // Court Ind is C, the Receipt Amount may be less or equal to zero.
        // Otherwise, the Receipt Amount should be greater than zero.
        // JLK  09/06/99
        // -----------------------------------------------------------------
        if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtManint.SystemGeneratedIdentifier && IsEmpty
          (entities.ExistingCashReceiptSourceType.CourtInd))
        {
          if (export.CashReceipt.ReceiptAmount == 0)
          {
            ExitState = "FN0000_MANDATORY_FIELDS";

            var field4 = GetField(export.CashReceipt, "receiptAmount");

            field4.Error = true;

            return;
          }
          else if (export.CashReceipt.ReceiptAmount < 0)
          {
            ExitState = "FN0128_CASH_RCPT_AMOUNT_LT_ZERO";

            var field4 = GetField(export.CashReceipt, "receiptAmount");

            field4.Error = true;

            return;
          }
        }

        if (export.CashReceipt.CashDue.GetValueOrDefault() != 0)
        {
          ExitState = "FN0000_INVALID_NET_INTF_AMT";

          return;
        }

        local.Pass.CodeName = "CHECK TYPE";
        local.CodeValue.Cdvalue = export.CashReceipt.CheckType ?? Spaces(10);
        UseCabValidateCodeValue();

        if (local.CdValueReturn.Count == 1 || local.CdValueReturn.Count == 2)
        {
          var field4 = GetField(export.CashReceipt, "checkType");

          field4.Error = true;

          ExitState = "CODE_VALUE_NF";

          return;
        }

        export.CashReceipt.ReceivedDate = export.CashReceiptEvent.ReceivedDate;
        MoveCashReceipt2(export.CashReceipt, local.PassArea);
        export.CashReceiptEvent.SystemGeneratedIdentifier = 0;

        // ----------------------------------------------------------------
        // If a receipt is being created for an interface source and the
        // program fund type (check type) is CSE, display the CRMI
        // screen to allow the user to Match, Interface, or Receipt the
        // data entered.    JLK  03-01-99
        // If the receipt type is RDIR PMT or CSENET, do not need to
        // go to CRMI to create the receipt.  JLK  05-13-99
        // ----------------------------------------------------------------
        if (AsChar(entities.ExistingCashReceiptSourceType.InterfaceIndicator) ==
          'Y' && Equal(export.CashReceipt.CheckType, "CSE") && export
          .CashReceiptType.SystemGeneratedIdentifier != local
          .HardcodedCrtRdirPmt.SystemGeneratedIdentifier && export
          .CashReceiptType.SystemGeneratedIdentifier != local
          .HardcodedCrtCsenet.SystemGeneratedIdentifier)
        {
          ExitState = "ECO_LNK_MATCH_INTERFACE";

          return;
        }

        export.CashReceiptStatus.SystemGeneratedIdentifier =
          local.HardcodedCrsRecorded.SystemGeneratedIdentifier;
        UseFnCreateCashReceipt();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCashReceiptSourceType(local.ClearCashReceiptSourceType,
            export.CashReceiptSourceType);
          export.CashReceiptEvent.Assign(local.ClearCashReceiptEvent);
          export.CashReceiptType.Assign(local.ClearCashReceiptType);
          MoveCashReceipt3(local.ClearCashReceipt, export.CashReceipt);
          MoveCashReceiptStatus(local.ClearCashReceiptStatus,
            export.CashReceiptStatus);
          export.HiddenCashReceiptSourceType.Assign(
            local.ClearCashReceiptSourceType);
          export.HiddenCashReceiptEvent.Assign(local.ClearCashReceiptEvent);
          MoveCashReceiptType1(local.ClearCashReceiptType,
            export.HiddenCashReceiptType);
          export.HiddenCashReceipt.Assign(local.ClearCashReceipt);
          export.CashReceiptType.Code = "CHECK";
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

          if (Equal(export.LastAction.Command, "UNMATCH"))
          {
            export.LastAction.Command = "";
          }
        }
        else
        {
          export.CashReceiptStatus.SystemGeneratedIdentifier = 0;

          return;
        }

        break;
      case "UPDATE":
        // -----------------------------------------------------------------
        // PF6  Update
        // Validate Identifying Information
        // -----------------------------------------------------------------
        if (!Equal(export.HiddenCashReceiptSourceType.Code,
          export.CashReceiptSourceType.Code))
        {
          ExitState = "FN0000_KEYS_CHNGD_DURING_UPD_DEL";

          var field4 = GetField(export.CashReceiptSourceType, "code");

          field4.Error = true;
        }

        if (!Equal(export.HiddenCashReceiptType.Code,
          export.CashReceiptType.Code))
        {
          ExitState = "FN0000_KEYS_CHNGD_DURING_UPD_DEL";

          var field4 = GetField(export.CashReceiptType, "code");

          field4.Error = true;
        }

        if (export.HiddenCashReceiptEvent.SystemGeneratedIdentifier != export
          .CashReceiptEvent.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_KEYS_CHNGD_DURING_UPD_DEL";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // --------------------------------------------------------------
        // Cannot update a cash receipt with a type of FCRT REC or
        // FDIR PMT.    JLK  12/18/98
        // --------------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          break;
        }

        // 11/06/99	P.Phinney - SRG   H00079503 Added TEMPORARY logic 
        // FIXes to REMOVE 45 DAY check.
        // ---------------------------------------------------------------
        // DISABLED following IF statement
        // * * * * * * * * * * * * * * * * * * *
        // --------------------------------------------------------------
        // Cannot update a cash receipt more than 45 days old.
        // Must receive a cash receipt before it can be receipted.
        // --------------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
        {
          // --------------------------------------------------------------
          // Receipt Amount cannot be updated.
          // --------------------------------------------------------------
          if (export.CashReceipt.ReceiptAmount != export
            .HiddenCashReceipt.ReceiptAmount)
          {
            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

            break;
          }

          // --------------------------------------------------------------
          // Interface receipts entered manually can be updated.
          // If the Cash Due amount is changed, the new amount must
          // be greater or equal to the total of the detail Collection
          // Amounts.   JLK  09/08/99
          // --------------------------------------------------------------
          if (export.CashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtManint.SystemGeneratedIdentifier)
          {
            if (export.CashReceipt.CashDue.GetValueOrDefault() != export
              .HiddenCashReceipt.CashDue.GetValueOrDefault())
            {
              if (export.CashReceipt.CashDue.GetValueOrDefault() <= 0)
              {
                ExitState = "FN0000_CASH_DUE_MUST_BE_GT_ZERO";

                break;
              }

              ReadCashReceiptDetail();

              if (export.CashReceipt.CashDue.GetValueOrDefault() < local
                .Total.CollectionAmount)
              {
                ExitState = "FN0000_CASH_DUE_LT_TOT_DET_AMT";

                break;
              }
              else
              {
                // --->  ok to update
              }
            }
          }
          else if (AsChar(export.CashReceiptSourceType.CourtInd) == 'C' && export
            .HiddenCashReceipt.CashDue.GetValueOrDefault() <= 0)
          {
            // --------------------------------------------------------------
            // The Cash Due amount of a court interface receipt processed
            // by batch can be updated if the existing Cash Due amount is
            // less than or equal to zero.    JLK  09/08/99
            // --------------------------------------------------------------
            // --->  ok to update
          }
          else
          {
            ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

            break;
          }
        }

        // ---------------------------------------------------------------
        // Data Validation for interface receipts in REC status.
        // ---------------------------------------------------------------
        if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) == 'Y' && export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsRecorded.SystemGeneratedIdentifier)
        {
          if (Lt(local.Low.Date, export.CashReceiptEvent.SourceCreationDate))
          {
            // -----------------------------------------------------------
            // The check date cannot be updated on a matched interface
            // receipt in REC status.  Must unmatch the interface receipt
            // prior to changing check date.    JLK  02/24/99
            // The receipt amount cannot be updated on a matched
            // interface receipt in REC status.  Must unmatch the
            // interface receipt prior to changing check date.
            // JLK  05/28/99
            // -----------------------------------------------------------
            if (!Equal(export.CashReceipt.CheckDate,
              export.HiddenCashReceipt.CheckDate) && export
              .CashReceipt.SequentialNumber == export
              .HiddenCashReceipt.SequentialNumber)
            {
              ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";
            }

            if (export.CashReceipt.ReceiptAmount != export
              .HiddenCashReceipt.ReceiptAmount)
            {
              ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            // -----------------------------------------------------------
            // If the Receipt Amount is changed on an interface receipt
            // in REC status, the Balance Due and Balance Reason
            // must be determined.    JLK  02/24/99
            // Removed the ability to update the receipt amount for a
            // matched interface receipt in REC status.  JLK  05/28/99
            // -----------------------------------------------------------
          }
          else
          {
            // -----------------------------------------------------------
            // The receipt amount for a receipt with an interface source
            // code cannot be changed if receipt amount adjustments
            // exist.    JLK  02/24/99
            // -----------------------------------------------------------
            if (export.CashReceipt.ReceiptAmount != export
              .HiddenCashReceipt.ReceiptAmount)
            {
              UseFnCabCalcNetRcptAmtAndBal();

              if (local.QtyReceiptAmtAdj.Count > 0)
              {
                ExitState = "FN0000_CR_ADJ_EXIST_CANT_CHG_AMT";

                break;
              }
            }
          }
        }

        // -------------------------------------------------------
        // Cannot change Receipt Amount if status is BAL.
        // Cannot change Received Date or Check Date if the Source
        // Type has an interface indicator of Y and the status is
        // BAL.         JLK  09/29/98
        // -------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsBalanced.SystemGeneratedIdentifier)
        {
          if (export.CashReceipt.ReceiptAmount != export
            .HiddenCashReceipt.ReceiptAmount)
          {
            ExitState = "FN0024_CASH_RCPT_ALREADY_BALANCD";

            break;
          }

          if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) == 'Y')
          {
            if (!Equal(export.CashReceiptEvent.ReceivedDate,
              export.HiddenCashReceiptEvent.ReceivedDate))
            {
              ExitState = "FN0000_CRE_RCVD_DT_CANT_BE_CHGD";

              break;
            }

            if (!Equal(export.CashReceipt.CheckDate,
              export.HiddenCashReceipt.CheckDate))
            {
              ExitState = "FN0000_CANNOT_CHANGE_CHECK_DATE";

              break;
            }
          }
        }

        // --------------------------------------------------------------
        // Only the Note field can be updated if the receipt status is
        // forwarded, deposited, or deleted.
        // Only the Note field can be updated on EFT type receipts per
        // T. Hood's request.    JLK    09/30/98
        // Only the Note field can be updated if the receipt was
        // created by CONVERSN.  JLK    02/99
        // --------------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsForwarded.SystemGeneratedIdentifier || export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeposited.SystemGeneratedIdentifier || export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtEft.SystemGeneratedIdentifier || Equal
          (export.CashReceipt.CreatedBy, "CONVERSN"))
        {
          if (!Equal(export.CashReceiptEvent.ReceivedDate,
            export.HiddenCashReceiptEvent.ReceivedDate))
          {
            ExitState = "FN0000_ONLY_CR_NOTE_CAN_BE_UPD";
          }

          if (export.CashReceipt.ReceiptAmount != export
            .HiddenCashReceipt.ReceiptAmount || !
            Equal(export.CashReceipt.CheckType,
            export.HiddenCashReceipt.CheckType) || !
            Equal(export.CashReceipt.CheckNumber,
            export.HiddenCashReceipt.CheckNumber) || !
            Equal(export.CashReceipt.CheckDate,
            export.HiddenCashReceipt.CheckDate) || !
            Equal(export.CashReceipt.PayorSocialSecurityNumber,
            export.HiddenCashReceipt.PayorSocialSecurityNumber) || !
            Equal(export.CashReceipt.PayorOrganization,
            export.HiddenCashReceipt.PayorOrganization) || !
            Equal(export.CashReceipt.PayorFirstName,
            export.HiddenCashReceipt.PayorFirstName) || !
            Equal(export.CashReceipt.PayorMiddleName,
            export.HiddenCashReceipt.PayorMiddleName) || !
            Equal(export.CashReceipt.PayorLastName,
            export.HiddenCashReceipt.PayorLastName))
          {
            ExitState = "FN0000_ONLY_CR_NOTE_CAN_BE_UPD";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (Equal(export.CashReceipt.Note, export.HiddenCashReceipt.Note))
            {
              ExitState = "FN0000_NO_CHANGE_TO_UPDATE";

              break;
            }
          }
          else
          {
            break;
          }
        }

        // -------------------------------------------------------------
        // Validate Program Fund Type code (check type).
        // -------------------------------------------------------------
        local.Pass.CodeName = "CHECK TYPE";
        local.CodeValue.Cdvalue = export.CashReceipt.CheckType ?? Spaces(10);
        UseCabValidateCodeValue();

        if (local.CdValueReturn.Count == 1 || local.CdValueReturn.Count == 2)
        {
          var field4 = GetField(export.CashReceipt, "checkType");

          field4.Error = true;

          ExitState = "CODE_VALUE_NF";

          break;
        }

        // --------------------------------------------------------------
        // Validation on Changed Fields     JLK  09/29/98
        // --------------------------------------------------------------
        if (Lt(export.CashReceipt.ReceiptDate,
          export.CashReceiptEvent.ReceivedDate))
        {
          ExitState = "CANNOT_RECEIPT_BEFORE_RECEIVING";

          break;
        }

        // -------------------------------------------------------
        // Cannot change Check Type (Program Fund Type) if details
        // exist.     JLK  09/29/98
        // -------------------------------------------------------
        if (export.HiddenNoOfCrDetails.Count > 0)
        {
          if (!Equal(export.CashReceipt.CheckType,
            export.HiddenCashReceipt.CheckType))
          {
            ExitState = "FN0000_CHECK_TYPE_CANNOT_BE_CHGD";

            break;
          }
        }

        // -----------------------------------------------------------
        // The Check Number and Check Date must be entered if
        // the Cash Receipt Type is CHECK or MONEY ORDER.
        // Check Number and Check Date are also mandatory for
        // Cash Receipt Type INTERFUND.   JLK  05/28/99
        // -----------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtCheck.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtMoneyOrder.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtInterfund.SystemGeneratedIdentifier)
        {
          if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
            .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
          {
            // ---> ok to continue
          }
          else
          {
            if (IsEmpty(export.CashReceipt.CheckNumber))
            {
              var field4 = GetField(export.CashReceipt, "checkNumber");

              field4.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              break;
            }

            if (Equal(export.CashReceipt.CheckDate,
              local.ClearCashReceipt.CheckDate))
            {
              var field4 = GetField(export.CashReceipt, "checkDate");

              field4.Error = true;

              ExitState = "FN0000_CHECK_DATE_MISSING";

              break;
            }
          }
        }
        else if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtCurrency.SystemGeneratedIdentifier)
        {
          // -----------------------------------------------------------
          // The Payor Name or Payor Organization must be entered if
          // the Cash Receipt Type is CURRENCY.    JLK  09/29/98
          // -----------------------------------------------------------
          if ((IsEmpty(export.CashReceipt.PayorFirstName) || IsEmpty
            (export.CashReceipt.PayorLastName)) && IsEmpty
            (export.CashReceipt.PayorOrganization))
          {
            var field4 = GetField(export.CashReceipt, "payorFirstName");

            field4.Error = true;

            var field5 = GetField(export.CashReceipt, "payorLastName");

            field5.Error = true;

            var field6 = GetField(export.CashReceipt, "payorOrganization");

            field6.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            break;
          }
        }

        // -----------------------------------------------------------------
        // If the cash receipt has an active status of INTF and the type
        // is MANINT, the following updates should occur:
        // -----------------------------------------------------------------
        // * The Cash Receipt Event Anticipated Check Amount should be
        //   set to the Cash Receipt Cash Due Amount.
        // * Total Cash Trans (Original Interface Amount) should be set
        //   to the export Cash Receipt Cash Due amount for the
        //   Cash Receipt and the Cash Receipt Event.
        // * The Cash Receipt Cash Balance Amount should be set to
        //   the Cash Due amount.
        // * Set the Cash Balance Reason to UNDER.
        // * The Cash Receipt Event and Cash Receipt 'Received Date'
        //   fields should be set to the imported Received Date.
        // If the receipt was entered by BATCH, the following updates
        // should occur:
        // -----------------------------------------------------------------
        // * The Cash Receipt Event Anticipated Check Amount should be
        //   set to the Cash Receipt Cash Due Amount.
        // * The Cash Receipt Cash Balance Amount should be set to the
        //   Cash Due amount.
        // * Set the Cash Balance Reason based on the new Cash Balance Amount.
        // JLK   09/08/99
        // -----------------------------------------------------------------
        MoveCashReceipt2(export.CashReceipt, local.PassArea);

        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
        {
          if (export.CashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtManint.SystemGeneratedIdentifier)
          {
            local.New1.TotalCashAmt =
              local.PassArea.CashDue.GetValueOrDefault();
            local.New1.AnticipatedCheckAmt =
              local.PassArea.CashDue.GetValueOrDefault();
            local.PassArea.TotalCashTransactionAmount =
              local.PassArea.CashDue.GetValueOrDefault();
            local.PassArea.CashBalanceAmt =
              local.PassArea.CashDue.GetValueOrDefault();
            local.PassArea.ReceivedDate = export.CashReceiptEvent.ReceivedDate;
          }
          else
          {
            local.New1.AnticipatedCheckAmt =
              local.PassArea.CashDue.GetValueOrDefault();
            local.PassArea.CashBalanceAmt =
              local.PassArea.CashDue.GetValueOrDefault();
          }

          // ------------------------------------------------------------
          // Set the Cash Balance Reason values based on the new Cash
          // Balance Amount.
          // ------------------------------------------------------------
          if (local.PassArea.CashBalanceAmt.GetValueOrDefault() > 0)
          {
            local.PassArea.CashBalanceReason = "UNDER";
          }
          else if (local.PassArea.CashBalanceAmt.GetValueOrDefault() < 0)
          {
            local.PassArea.CashBalanceReason = "OVER";
          }
          else
          {
            local.PassArea.CashBalanceReason = "";
          }
        }
        else
        {
          local.PassArea.ReceivedDate = export.CashReceiptEvent.ReceivedDate;
        }

        // ----------------------------------------------------------------
        // Perform database actions for UPDATE.
        // Update Cash Receipt Event with Total Cash Amount and
        // Anticipated Check Amount if the Cash Due amount was
        // changed.   JLK  09/08/99
        // ----------------------------------------------------------------
        if (export.CashReceipt.CashDue.GetValueOrDefault() != export
          .HiddenCashReceipt.CashDue.GetValueOrDefault())
        {
          if (ReadCashReceiptEvent())
          {
            if (export.CashReceiptType.SystemGeneratedIdentifier != local
              .HardcodedCrtManint.SystemGeneratedIdentifier)
            {
              local.New1.TotalCashAmt =
                entities.ExistingCashReceiptEvent.TotalCashAmt;
            }

            try
            {
              UpdateCashReceiptEvent();

              // ---> ok to continue
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_CASH_RCPT_EVENT_NF";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0080_CASH_RCPT_EVENT_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            ExitState = "FN0000_CASH_RCPT_EVENT_NF";

            break;
          }
        }

        UseUpdateCashReceipt();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCashReceipt2(local.PassArea, export.CashReceipt);
          MoveCashReceipt4(local.PassArea, export.HiddenCashReceipt);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else if (IsExitState("FN0000_INTF_AE_4_SOURCE_RCVD_DT"))
        {
          var field4 = GetField(export.CashReceiptEvent, "receivedDate");

          field4.Error = true;
        }
        else
        {
        }

        break;
      case "DELETE":
        // --------------------------------------------------------------
        // PF10  Delete
        // Added logic to prevent flow to CRDE if:
        //   * Cash Receipt Type is EFT.
        //   * Active status is BAL, DEP, or FWD.
        //   * Cash Receipt has details.
        //   * Cash Receipt has an interface Source and Type is not MANINT.
        //   * Active status is INTF, Type is MANINT, but receipt date
        //     is not current date.
        //   * Active status is REC, Type category is C, but receipt date
        //     is not current date.
        // JLK    10/09/98
        // ---------------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtEft.SystemGeneratedIdentifier)
        {
          ExitState = "FN0305_INVALID_CR_TYP_FOR_AUD";

          break;
        }

        if (export.CashReceiptStatus.SystemGeneratedIdentifier != local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier && (
            export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFdirPmt.SystemGeneratedIdentifier))
        {
          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          break;
        }

        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsBalanced.SystemGeneratedIdentifier || export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeposited.SystemGeneratedIdentifier || export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsForwarded.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          break;
        }

        if (export.HiddenNoOfCrDetails.Count > 0)
        {
          ExitState = "FN0304_CR_HAS_DETAILS_CANNOT_DEL";

          break;
        }

        if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) == 'Y' && Lt
          (local.Low.Date, export.CashReceiptEvent.SourceCreationDate) && export
          .CashReceiptType.SystemGeneratedIdentifier != local
          .HardcodedCrtManint.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_ONLY_DEL_IF_TYPE_MANINT";

          break;
        }

        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsInterfaced.SystemGeneratedIdentifier && !
          Equal(export.CashReceipt.ReceiptDate, local.Current.Date))
        {
          ExitState = "FN0000_MANINT_CR_ONLY_DEL_CUR_DT";

          break;
        }

        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsRecorded.SystemGeneratedIdentifier && AsChar
          (export.CashReceiptType.CategoryIndicator) == AsChar
          (local.HardcodedCrtCatCash.CategoryIndicator) && !
          Equal(export.CashReceipt.ReceiptDate, local.Current.Date))
        {
          ExitState = "FN0303_CR_CANNOT_BE_DELETED";

          break;
        }

        ExitState = "ECO_LNK_TO_DEL_CASH_RECEIPT";

        break;
      case "DETAIL":
        // -----------------------------------------------------------------
        // PF15  CREL
        // -----------------------------------------------------------------
        ExitState = "ECO_LNK_TO_VIEW_DETAILS";

        return;
      case "BALANCE":
        // -----------------------------------------------------------------
        // PF16  CRTB
        // -----------------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";
        }
        else
        {
          ExitState = "ECO_LNK_TO_BALANCE_RECEIPT";
        }

        break;
      case "COLL":
        // --------------------------------------------------------------
        // PF17  CRRC
        // Added logic to prevent flow to CRRC unless:
        //   *  Check Type (Program Fund Type) is CSE.
        //   *  Either details already exist or source interface indicator
        //      is Y when cash receipt status is REC or BAL.
        //   *  Cash receipt status is DEP or INTF.
        //   *  Cash receipt type is FCRT REC or FDIR PMT.
        // JLK    10/01/98
        // ---------------------------------------------------------------
        if (!Equal(export.CashReceipt.CheckType, "CSE"))
        {
          ExitState = "FN0000_INVALID_CHECK_TYPE_4_COL";

          break;
        }

        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_CASH_RECEIPT_DELETED";

          break;
        }

        if (export.CashReceipt.ReceiptAmount <= 0 && export
          .CashReceiptStatus.SystemGeneratedIdentifier != local
          .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          break;
        }

        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsRecorded.SystemGeneratedIdentifier || export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsBalanced.SystemGeneratedIdentifier)
        {
          if (export.HiddenNoOfCrDetails.Count > 0 || AsChar
            (export.CashReceiptSourceType.InterfaceIndicator) == 'Y')
          {
            // ---------------------------------------------------
            // OK to flow to CRRC.
            // ---------------------------------------------------
          }
          else if (export.CashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
            .CashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
          {
            // ---------------------------------------------------
            // OK to flow to CRRC.
            // ---------------------------------------------------
          }
          else
          {
            ExitState = "FN0000_COLL_CANNOT_BE_ADDED";

            break;
          }
        }
        else if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeposited.SystemGeneratedIdentifier || export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
        {
          // ---------------------------------------------------
          // OK to flow to CRRC.
          // ---------------------------------------------------
        }
        else
        {
          ExitState = "FN0000_COLL_CANNOT_BE_ADDED";

          break;
        }

        ExitState = "ECO_LNK_TO_COLLECTION_DETAILS";

        break;
      case "CRMI":
        // -----------------------------------------------------------------
        // PF18  CRMI
        // Added validation logic to ensure that only receipts with an
        // interface source code can flow to CRMI for matching.
        // JLK    10/01/98
        // -----------------------------------------------------------------
        if (IsEmpty(export.CashReceiptStatus.Code))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        // -----------------------------------------------------------------
        // Cannot match deleted receipts to an interface.
        // JLK   02/15/99
        // Cannot match forwarded receipts to an interface.
        // JLK  06/03/99
        // -----------------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier || export
          .CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsForwarded.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          break;
        }

        // -----------------------------------------------------------------
        // Payment history records cannot be matched to an interface.
        // JLK   02/15/99
        // -----------------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          break;
        }

        // -----------------------------------------------------------------
        // Cannot match receipts with BOGUS cash receipt type to an
        // interface.       JLK  03-01-99
        // -----------------------------------------------------------------
        if (Equal(export.CashReceiptType.Code, "BOGUS"))
        {
          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          break;
        }

        // -----------------------------------------------------------------
        // Cannot match receipts with non-interface source code to an
        // interface and program fund type must be CSE.    JLK  03-01-99
        // -----------------------------------------------------------------
        if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) != 'Y')
        {
          ExitState = "FN0000_ONLY_MATCH_INTF_SOURCE";
        }
        else if (!Equal(export.CashReceipt.CheckType, "CSE"))
        {
          ExitState = "FN0000_INVALID_CHK_TYPE_4_PFKEY";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // -----------------------------------------------------------------
        // Cannot match a cash receipt that is converted from KAECSES.
        // JLK   02/15/99
        // -----------------------------------------------------------------
        if (Equal(export.CashReceipt.CreatedBy, "CONVERSN"))
        {
          ExitState = "FN0000_CANT_UPD_OR_MTCH_CONV_CR";

          break;
        }

        ExitState = "ECO_LNK_MATCH_INTERFACE";

        break;
      case "MTRN":
        // -----------------------------------------------------------------
        // PF19  MTRN    Must display prior to linking to MTRN.
        //               JLK    02/01/99
        // -----------------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtEft.SystemGeneratedIdentifier)
        {
          // --------------------------------------------------------------
          // Added logic to verify that the Check Number is all numeric
          // prior to using the numtext function.  If the Check Number is
          // not all numeric, display error message.  JLK  07/26/99
          // --------------------------------------------------------------
          if (Verify(export.CashReceipt.CheckNumber, "0123456789") > 0)
          {
            ExitState = "FN0000_EFT_NUMBER_INVALID";

            break;
          }

          export.PassElectronicFundTransmission.TransmissionType = "I";
          export.PassElectronicFundTransmission.TransmissionIdentifier =
            (int)StringToNumber(export.CashReceipt.CheckNumber);
          ExitState = "ECO_LNK_TO_MTRN";
        }
        else
        {
          ExitState = "FN0000_CR_STATUS_OR_TYPE_INVALID";
        }

        break;
      case "RETURN":
        // -----------------------------------------------------------------
        // PF9 Return
        // -----------------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // -----------------------------------------------------------------
        // PF12 Signoff
        // -----------------------------------------------------------------
        UseScCabSignoff();

        return;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // -------------------------------------------------------
    // Update Protection Logic
    // -------------------------------------------------------
    if (!IsEmpty(export.CashReceiptStatus.Code))
    {
      // --------------------------------------------------------------
      // Cash Receipt is in update mode.  Protect the fields on the
      // screen that cannot be changed.    JLK    09/30/98
      // --------------------------------------------------------------
      var field1 = GetField(export.CashReceiptSourceType, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Prompts.SourceTypePrompt, "actionEntry");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.CashReceiptType, "code");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.Prompts.CashRcptTypePrompt, "actionEntry");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.CashReceipt, "sequentialNumber");

      field5.Color = "cyan";
      field5.Protected = true;

      // --------------------------------------------------------------
      // The Cash Receipt Check Type (Program Fund Type) can
      // only be changed if the receipt does not have any associated
      // details.       JLK    09/30/98
      // --------------------------------------------------------------
      if (export.HiddenNoOfCrDetails.Count > 0)
      {
        var field6 = GetField(export.CashReceipt, "checkType");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.CheckTypePrompt, "promptField");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.CashReceipt, "payorSocialSecurityNumber");

        field8.Color = "cyan";
        field8.Protected = true;
      }

      // --------------------------------------------------------------
      // If the Cash Receipt source code has an interface indicator of
      // Y and the type is not MANINT, do not allow interface data to
      // be updated.    JLK    10/05/98
      // --------------------------------------------------------------
      if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) == 'Y' && Lt
        (local.Low.Date, export.CashReceiptEvent.SourceCreationDate))
      {
        if (export.CashReceiptType.SystemGeneratedIdentifier != local
          .HardcodedCrtManint.SystemGeneratedIdentifier)
        {
          var field6 = GetField(export.CashReceiptEvent, "receivedDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.CashReceipt, "checkType");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.CheckTypePrompt, "promptField");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 =
            GetField(export.CashReceipt, "payorSocialSecurityNumber");

          field9.Color = "cyan";
          field9.Protected = true;
        }
      }

      // --------------------------------------------------------------
      // Protect fields on screen based on active Cash Receipt
      // status.    JLK    09/30/98
      // --------------------------------------------------------------
      if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
      {
        // --------------------------------------------------------------
        // Receipt Amount must remain zero -- the value cannot be changed.
        // --------------------------------------------------------------
        var field6 = GetField(export.CashReceipt, "receiptAmount");

        field6.Color = "cyan";
        field6.Protected = true;

        // --------------------------------------------------------------
        // MANINT type Interface receipts can be updated if status is INTF.
        // --------------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtManint.SystemGeneratedIdentifier)
        {
          if (IsExitState("FN0000_CASH_DUE_LT_TOT_DET_AMT") || IsExitState
            ("FN0000_CASH_DUE_MUST_BE_GT_ZERO") || IsExitState
            ("FN0000_CR_CASH_DUE_LT_ZERO") || IsExitState
            ("FN0000_INVALID_NET_INTF_AMT"))
          {
            var field7 = GetField(export.CashReceipt, "cashDue");

            field7.Error = true;
          }
          else
          {
            var field7 = GetField(export.CashReceipt, "cashDue");

            field7.Protected = false;
          }
        }
        else
        {
          // --------------------------------------------------------------
          // Batch processed Interface receipts cannot be updated.
          // --------------------------------------------------------------
          var field7 = GetField(export.CashReceiptEvent, "receivedDate");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.CashReceipt, "receiptAmount");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.CashReceipt, "checkType");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.CashReceipt, "checkNumber");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.CashReceipt, "checkDate");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.CashReceipt, "note");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 =
            GetField(export.CashReceipt, "payorSocialSecurityNumber");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.CashReceipt, "payorFirstName");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.CashReceipt, "payorLastName");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.CashReceipt, "payorMiddleName");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.CashReceipt, "payorOrganization");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 = GetField(export.CheckTypePrompt, "promptField");

          field18.Color = "cyan";
          field18.Protected = true;

          // --------------------------------------------------------------
          // Batch processed Interface receipts with a court indicator of
          // 'C' and a Net Interface Amount [Cash Due] less than or
          // equal to zero can be updated to accommodate negative net
          // amounts that occur when total adjustments exceed the total
          // collections for a business date.  JLK  09/07/99
          // --------------------------------------------------------------
          if (AsChar(export.CashReceiptSourceType.CourtInd) == 'C' && export
            .CashReceipt.CashDue.GetValueOrDefault() <= 0)
          {
            if (IsExitState("FN0000_CASH_DUE_LT_TOT_DET_AMT") || IsExitState
              ("FN0000_CASH_DUE_MUST_BE_GT_ZERO") || IsExitState
              ("FN0000_CR_CASH_DUE_LT_ZERO") || IsExitState
              ("FN0000_INVALID_NET_INTF_AMT"))
            {
              var field19 = GetField(export.CashReceipt, "cashDue");

              field19.Error = true;
            }
            else
            {
              var field19 = GetField(export.CashReceipt, "cashDue");

              field19.Protected = false;
            }
          }
        }
      }
      else if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsRecorded.SystemGeneratedIdentifier)
      {
        // --------------------------------------------------------------
        // If the active status of an interface receipt is REC, do not
        // allow the check date to be changed.    JLK  05/28/99
        // --------------------------------------------------------------
        if (Lt(local.Low.Date, export.CashReceiptEvent.SourceCreationDate))
        {
          var field6 = GetField(export.CashReceipt, "checkDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.CashReceipt, "receiptAmount");

          field7.Color = "cyan";
          field7.Protected = true;
        }
      }
      else if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsBalanced.SystemGeneratedIdentifier)
      {
        if (IsExitState("FN0024_CASH_RCPT_ALREADY_BALANCD"))
        {
          var field6 = GetField(export.CashReceipt, "receiptAmount");

          field6.Color = "red";
          field6.Intensity = Intensity.High;
          field6.Highlighting = Highlighting.ReverseVideo;
          field6.Protected = true;
        }
        else
        {
          var field6 = GetField(export.CashReceipt, "receiptAmount");

          field6.Color = "cyan";
          field6.Protected = true;
        }

        if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) == 'Y')
        {
          if (IsExitState("FN0000_CRE_RCVD_DT_CANT_BE_CHGD"))
          {
            var field6 = GetField(export.CashReceiptEvent, "receivedDate");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceiptEvent, "receivedDate");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (IsExitState("FN0000_CANNOT_CHANGE_CHECK_DATE"))
          {
            var field6 = GetField(export.CashReceipt, "checkDate");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "checkDate");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }
      }
      else if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsForwarded.SystemGeneratedIdentifier || export
        .CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsDeleted.SystemGeneratedIdentifier || export
        .CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsDeposited.SystemGeneratedIdentifier || export
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtEft.SystemGeneratedIdentifier && AsChar
        (export.CashReceiptSourceType.InterfaceIndicator) != 'Y' || Equal
        (export.CashReceipt.CreatedBy, "CONVERSN"))
      {
        // --------------------------------------------------------------
        // Only the Note field can be updated if the receipt status is
        // forwarded, deposited, or deleted.
        // Only the Note field can be updated on EFT type receipts per
        // T. Hood's request.
        // Only the Note field can be updated if the receipt was
        // created by CONVERSN.
        // JLK    05/05/99
        // --------------------------------------------------------------
        if (IsExitState("FN0000_ONLY_CR_NOTE_CAN_BE_UPD"))
        {
          if (!Equal(export.CashReceiptEvent.ReceivedDate,
            export.HiddenCashReceiptEvent.ReceivedDate))
          {
            var field6 = GetField(export.CashReceiptEvent, "receivedDate");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceiptEvent, "receivedDate");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (export.CashReceipt.ReceiptAmount != export
            .HiddenCashReceipt.ReceiptAmount)
          {
            var field6 = GetField(export.CashReceipt, "receiptAmount");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "receiptAmount");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.CheckType,
            export.HiddenCashReceipt.CheckType))
          {
            var field6 = GetField(export.CashReceipt, "checkType");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;

            var field7 = GetField(export.CheckTypePrompt, "promptField");

            field7.Color = "cyan";
            field7.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "checkType");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.CheckNumber,
            export.HiddenCashReceipt.CheckNumber))
          {
            var field6 = GetField(export.CashReceipt, "checkNumber");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "checkNumber");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.CheckDate,
            export.HiddenCashReceipt.CheckDate))
          {
            var field6 = GetField(export.CashReceipt, "checkDate");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "checkDate");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.PayorSocialSecurityNumber,
            export.HiddenCashReceipt.PayorSocialSecurityNumber))
          {
            var field6 =
              GetField(export.CashReceipt, "payorSocialSecurityNumber");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 =
              GetField(export.CashReceipt, "payorSocialSecurityNumber");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.PayorOrganization,
            export.HiddenCashReceipt.PayorOrganization))
          {
            var field6 = GetField(export.CashReceipt, "payorOrganization");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "payorOrganization");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.PayorFirstName,
            export.HiddenCashReceipt.PayorFirstName))
          {
            var field6 = GetField(export.CashReceipt, "payorFirstName");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "payorFirstName");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.PayorMiddleName,
            export.HiddenCashReceipt.PayorMiddleName))
          {
            var field6 = GetField(export.CashReceipt, "payorMiddleName");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "payorMiddleName");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (!Equal(export.CashReceipt.PayorLastName,
            export.HiddenCashReceipt.PayorLastName))
          {
            var field6 = GetField(export.CashReceipt, "payorLastName");

            field6.Color = "red";
            field6.Intensity = Intensity.High;
            field6.Highlighting = Highlighting.ReverseVideo;
            field6.Protected = true;
          }
          else
          {
            var field6 = GetField(export.CashReceipt, "payorLastName");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }
        else
        {
          var field6 = GetField(export.CashReceiptEvent, "receivedDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.CashReceipt, "receiptAmount");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.CashReceipt, "checkType");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.CashReceipt, "checkNumber");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.CashReceipt, "checkDate");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.CashReceipt, "payorSocialSecurityNumber");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.CashReceipt, "payorOrganization");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.CashReceipt, "payorFirstName");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.CashReceipt, "payorMiddleName");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.CashReceipt, "payorLastName");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.CheckTypePrompt, "promptField");

          field16.Color = "cyan";
          field16.Protected = true;
        }
      }

      // 11/06/99	P.Phinney - SRG   H00079503 Added TEMPORARY logic 
      // FIXes to REMOVE 45 DAY check.
      // ---------------------------------------------------------------
      // Removed THIS line from following IF statement
      // OR (export cash_receipt receipt_date < CURRENT_DATE - 45 DAYS)
      // * * * * * * * * * * * * * * * * * * *
      // ------------------------------------------------------------------
      // The receipt cannot be updated if the receipt is a payment
      // history record [FDIR PMT or FCRT REC].
      // Also cannot update receipt if receipt is more than 45 days old.
      // ------------------------------------------------------------------
      if (export.CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
      {
        // * * * * * * * * * * * * * * * * * * *
        // H00079503  - - - Removed line goes here
        // * * * * * * * * * * * * * * * * * * *
        var field6 = GetField(export.CashReceiptEvent, "receivedDate");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.CashReceipt, "receiptAmount");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.CashReceipt, "cashDue");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.CashReceipt, "checkType");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.CashReceipt, "checkNumber");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.CashReceipt, "checkDate");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.CashReceipt, "note");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.CashReceipt, "payorSocialSecurityNumber");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.CashReceipt, "payorFirstName");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.CashReceipt, "payorLastName");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.CashReceipt, "payorMiddleName");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.CashReceipt, "payorOrganization");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.CheckTypePrompt, "promptField");

        field18.Color = "cyan";
        field18.Protected = true;
      }
    }
  }

  private static void MoveCashReceipt1(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
  }

  private static void MoveCashReceipt2(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CheckDate = source.CheckDate;
    target.ReceivedDate = source.ReceivedDate;
    target.Note = source.Note;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorFirstName = source.PayorFirstName;
    target.PayorMiddleName = source.PayorMiddleName;
    target.PayorLastName = source.PayorLastName;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CashDue = source.CashDue;
    target.CashBalanceAmt = source.CashBalanceAmt;
    target.CashBalanceReason = source.CashBalanceReason;
    target.PayorSocialSecurityNumber = source.PayorSocialSecurityNumber;
  }

  private static void MoveCashReceipt3(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CheckDate = source.CheckDate;
    target.ReceivedDate = source.ReceivedDate;
    target.Note = source.Note;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorFirstName = source.PayorFirstName;
    target.PayorMiddleName = source.PayorMiddleName;
    target.PayorLastName = source.PayorLastName;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.CreatedBy = source.CreatedBy;
    target.CashDue = source.CashDue;
    target.CashBalanceAmt = source.CashBalanceAmt;
    target.CashBalanceReason = source.CashBalanceReason;
    target.PayorSocialSecurityNumber = source.PayorSocialSecurityNumber;
  }

  private static void MoveCashReceipt4(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CheckDate = source.CheckDate;
    target.Note = source.Note;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorFirstName = source.PayorFirstName;
    target.PayorMiddleName = source.PayorMiddleName;
    target.PayorLastName = source.PayorLastName;
    target.CashDue = source.CashDue;
    target.PayorSocialSecurityNumber = source.PayorSocialSecurityNumber;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
    target.SourceCreationDate = source.SourceCreationDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.CourtInd = source.CourtInd;
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType1(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType2(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CategoryIndicator = source.CategoryIndicator;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Pass.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.CdValueReturn.Count = useExport.ReturnCode.Count;
  }

  private void UseDisplayCashReceipt()
  {
    var useImport = new DisplayCashReceipt.Import();
    var useExport = new DisplayCashReceipt.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;

    Call(DisplayCashReceipt.Execute, useImport, useExport);

    local.NoOfCrDetails.Count = useExport.NumberOfCrDetails.Count;
    MoveCashReceipt2(useExport.CashReceipt, local.PassArea);
    export.CashReceiptSourceType.Assign(useExport.CashReceiptSourceType);
    export.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
    export.CashReceiptType.Assign(useExport.CashReceiptType);
    MoveCashReceiptStatus(useExport.CashReceiptStatus, export.CashReceiptStatus);
      
  }

  private void UseFnCabCalcNetRcptAmtAndBal()
  {
    var useImport = new FnCabCalcNetRcptAmtAndBal.Import();
    var useExport = new FnCabCalcNetRcptAmtAndBal.Export();

    MoveCashReceipt1(export.CashReceipt, useImport.CashReceipt);

    Call(FnCabCalcNetRcptAmtAndBal.Execute, useImport, useExport);

    local.QtyReceiptAmtAdj.Count = useExport.QtyReceiptAmtAdj.Count;
  }

  private void UseFnCreateCashReceipt()
  {
    var useImport = new FnCreateCashReceipt.Import();
    var useExport = new FnCreateCashReceipt.Export();

    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      export.CashReceiptStatus.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(export.CashReceiptEvent, useImport.CashReceiptEvent);
    useImport.CashReceipt.Assign(local.PassArea);

    Call(FnCreateCashReceipt.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrtCatCash.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
    local.HardcodedCrsRecorded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrsInterfaced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
    local.HardcodedCrsBalanced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdBalanced.SystemGeneratedIdentifier;
    local.HardcodedCrsForwarded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdForwarded.SystemGeneratedIdentifier;
    local.HardcodedCrsDeposited.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrtCatNonCash.CategoryIndicator =
      useExport.CrtCategory.CrtCatNonCash.CategoryIndicator;
    local.HardcodedCrtCheck.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdCheck.SystemGeneratedIdentifier;
    MoveCashReceiptType2(useExport.CrtSystemId.CrtIdFcrtRec,
      local.HardcodedCrtFcrtRec);
    MoveCashReceiptType2(useExport.CrtSystemId.CrtIdFdirPmt,
      local.HardcodedCrtFdirPmt);
    local.HardcodedCrtMoneyOrder.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdMnyOrd.SystemGeneratedIdentifier;
    local.HardcodedCrtCurrency.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdCurrency.SystemGeneratedIdentifier;
    local.HardcodedCrtEft.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdEft.SystemGeneratedIdentifier;
    local.HardcodedCrtCsenet.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdCsenet.SystemGeneratedIdentifier;
    local.HardcodedCrtRdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdRdirPmt.SystemGeneratedIdentifier;
    local.HardcodedCrtManint.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdManint.SystemGeneratedIdentifier;
    local.HardcodedCrtInterfund.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdInterfund.SystemGeneratedIdentifier;
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

  private void UseUpdateCashReceipt()
  {
    var useImport = new UpdateCashReceipt.Import();
    var useExport = new UpdateCashReceipt.Export();

    useImport.CashReceipt.Assign(local.PassArea);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.Assign(export.CashReceiptEvent);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;

    Call(UpdateCashReceipt.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptDetail()
  {
    local.Total.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", export.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        local.Total.CollectionAmount = db.GetDecimal(reader, 0);
        local.Total.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    entities.ExistingCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", export.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingCashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.ExistingCashReceiptEvent.AnticipatedCheckAmt =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingCashReceiptEvent.TotalCashAmt =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.ExistingCashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptType1()
  {
    entities.ExistingCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptType.CategoryIndicator =
          db.GetString(reader, 2);
        entities.ExistingCashReceiptType.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingCashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.ExistingCashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptType2()
  {
    entities.ExistingCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptType.CategoryIndicator =
          db.GetString(reader, 2);
        entities.ExistingCashReceiptType.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingCashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.ExistingCashReceiptType.CategoryIndicator);
      });
  }

  private void UpdateCashReceiptEvent()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptEvent.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;
    var anticipatedCheckAmt =
      local.New1.AnticipatedCheckAmt.GetValueOrDefault();
    var totalCashAmt = local.New1.TotalCashAmt.GetValueOrDefault();

    entities.ExistingCashReceiptEvent.Populated = false;
    Update("UpdateCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "anticCheckAmt", anticipatedCheckAmt);
        db.SetNullableDecimal(command, "totalCashAmt", totalCashAmt);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptEvent.CstIdentifier);
        db.SetInt32(
          command, "creventId",
          entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier);
      });

    entities.ExistingCashReceiptEvent.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCashReceiptEvent.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingCashReceiptEvent.AnticipatedCheckAmt = anticipatedCheckAmt;
    entities.ExistingCashReceiptEvent.TotalCashAmt = totalCashAmt;
    entities.ExistingCashReceiptEvent.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A PromptsGroup group.</summary>
    [Serializable]
    public class PromptsGroup
    {
      /// <summary>
      /// A value of SourceTypePrompt.
      /// </summary>
      [JsonPropertyName("sourceTypePrompt")]
      public Common SourceTypePrompt
      {
        get => sourceTypePrompt ??= new();
        set => sourceTypePrompt = value;
      }

      /// <summary>
      /// A value of CashRcptTypePrompt.
      /// </summary>
      [JsonPropertyName("cashRcptTypePrompt")]
      public Common CashRcptTypePrompt
      {
        get => cashRcptTypePrompt ??= new();
        set => cashRcptTypePrompt = value;
      }

      private Common sourceTypePrompt;
      private Common cashRcptTypePrompt;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceipt.
    /// </summary>
    [JsonPropertyName("hiddenCashReceipt")]
    public CashReceipt HiddenCashReceipt
    {
      get => hiddenCashReceipt ??= new();
      set => hiddenCashReceipt = value;
    }

    /// <summary>
    /// Gets a value of Prompts.
    /// </summary>
    [JsonPropertyName("prompts")]
    public PromptsGroup Prompts
    {
      get => prompts ?? (prompts = new());
      set => prompts = value;
    }

    /// <summary>
    /// A value of CheckTypePrompt.
    /// </summary>
    [JsonPropertyName("checkTypePrompt")]
    public Standard CheckTypePrompt
    {
      get => checkTypePrompt ??= new();
      set => checkTypePrompt = value;
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
    /// A value of HiddenNoOfCrDetails.
    /// </summary>
    [JsonPropertyName("hiddenNoOfCrDetails")]
    public Common HiddenNoOfCrDetails
    {
      get => hiddenNoOfCrDetails ??= new();
      set => hiddenNoOfCrDetails = value;
    }

    /// <summary>
    /// A value of LastAction.
    /// </summary>
    [JsonPropertyName("lastAction")]
    public Common LastAction
    {
      get => lastAction ??= new();
      set => lastAction = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceipt hiddenCashReceipt;
    private PromptsGroup prompts;
    private Standard checkTypePrompt;
    private CodeValue selected;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common hiddenNoOfCrDetails;
    private Common lastAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PromptsGroup group.</summary>
    [Serializable]
    public class PromptsGroup
    {
      /// <summary>
      /// A value of SourceTypePrompt.
      /// </summary>
      [JsonPropertyName("sourceTypePrompt")]
      public Common SourceTypePrompt
      {
        get => sourceTypePrompt ??= new();
        set => sourceTypePrompt = value;
      }

      /// <summary>
      /// A value of CashRcptTypePrompt.
      /// </summary>
      [JsonPropertyName("cashRcptTypePrompt")]
      public Common CashRcptTypePrompt
      {
        get => cashRcptTypePrompt ??= new();
        set => cashRcptTypePrompt = value;
      }

      private Common sourceTypePrompt;
      private Common cashRcptTypePrompt;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceipt.
    /// </summary>
    [JsonPropertyName("hiddenCashReceipt")]
    public CashReceipt HiddenCashReceipt
    {
      get => hiddenCashReceipt ??= new();
      set => hiddenCashReceipt = value;
    }

    /// <summary>
    /// Gets a value of Prompts.
    /// </summary>
    [JsonPropertyName("prompts")]
    public PromptsGroup Prompts
    {
      get => prompts ?? (prompts = new());
      set => prompts = value;
    }

    /// <summary>
    /// A value of CheckTypePrompt.
    /// </summary>
    [JsonPropertyName("checkTypePrompt")]
    public Standard CheckTypePrompt
    {
      get => checkTypePrompt ??= new();
      set => checkTypePrompt = value;
    }

    /// <summary>
    /// A value of PassCode.
    /// </summary>
    [JsonPropertyName("passCode")]
    public Code PassCode
    {
      get => passCode ??= new();
      set => passCode = value;
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
    /// A value of HiddenNoOfCrDetails.
    /// </summary>
    [JsonPropertyName("hiddenNoOfCrDetails")]
    public Common HiddenNoOfCrDetails
    {
      get => hiddenNoOfCrDetails ??= new();
      set => hiddenNoOfCrDetails = value;
    }

    /// <summary>
    /// A value of PassElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("passElectronicFundTransmission")]
    public ElectronicFundTransmission PassElectronicFundTransmission
    {
      get => passElectronicFundTransmission ??= new();
      set => passElectronicFundTransmission = value;
    }

    /// <summary>
    /// A value of LastAction.
    /// </summary>
    [JsonPropertyName("lastAction")]
    public Common LastAction
    {
      get => lastAction ??= new();
      set => lastAction = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceipt hiddenCashReceipt;
    private PromptsGroup prompts;
    private Standard checkTypePrompt;
    private Code passCode;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common hiddenNoOfCrDetails;
    private ElectronicFundTransmission passElectronicFundTransmission;
    private Common lastAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public CashReceiptDetail Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of QtyReceiptAmtAdj.
    /// </summary>
    [JsonPropertyName("qtyReceiptAmtAdj")]
    public Common QtyReceiptAmtAdj
    {
      get => qtyReceiptAmtAdj ??= new();
      set => qtyReceiptAmtAdj = value;
    }

    /// <summary>
    /// A value of NetReceiptAmt.
    /// </summary>
    [JsonPropertyName("netReceiptAmt")]
    public Common NetReceiptAmt
    {
      get => netReceiptAmt ??= new();
      set => netReceiptAmt = value;
    }

    /// <summary>
    /// A value of NewBal.
    /// </summary>
    [JsonPropertyName("newBal")]
    public CashReceipt NewBal
    {
      get => newBal ??= new();
      set => newBal = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptEvent New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCatCash.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCatCash")]
    public CashReceiptType HardcodedCrtCatCash
    {
      get => hardcodedCrtCatCash ??= new();
      set => hardcodedCrtCatCash = value;
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
    /// A value of Low.
    /// </summary>
    [JsonPropertyName("low")]
    public DateWorkArea Low
    {
      get => low ??= new();
      set => low = value;
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
    /// A value of CdValueReturn.
    /// </summary>
    [JsonPropertyName("cdValueReturn")]
    public Common CdValueReturn
    {
      get => cdValueReturn ??= new();
      set => cdValueReturn = value;
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
    /// A value of NoOfCrDetails.
    /// </summary>
    [JsonPropertyName("noOfCrDetails")]
    public Common NoOfCrDetails
    {
      get => noOfCrDetails ??= new();
      set => noOfCrDetails = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public CashReceipt PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Code Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of ChangeStatus.
    /// </summary>
    [JsonPropertyName("changeStatus")]
    public Common ChangeStatus
    {
      get => changeStatus ??= new();
      set => changeStatus = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("clearCashReceiptSourceType")]
    public CashReceiptSourceType ClearCashReceiptSourceType
    {
      get => clearCashReceiptSourceType ??= new();
      set => clearCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("clearCashReceiptEvent")]
    public CashReceiptEvent ClearCashReceiptEvent
    {
      get => clearCashReceiptEvent ??= new();
      set => clearCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptType.
    /// </summary>
    [JsonPropertyName("clearCashReceiptType")]
    public CashReceiptType ClearCashReceiptType
    {
      get => clearCashReceiptType ??= new();
      set => clearCashReceiptType = value;
    }

    /// <summary>
    /// A value of ClearCashReceipt.
    /// </summary>
    [JsonPropertyName("clearCashReceipt")]
    public CashReceipt ClearCashReceipt
    {
      get => clearCashReceipt ??= new();
      set => clearCashReceipt = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("clearCashReceiptStatus")]
    public CashReceiptStatus ClearCashReceiptStatus
    {
      get => clearCashReceiptStatus ??= new();
      set => clearCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HardcodedCrsRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedCrsRecorded")]
    public CashReceiptStatus HardcodedCrsRecorded
    {
      get => hardcodedCrsRecorded ??= new();
      set => hardcodedCrsRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedCrsInterfaced.
    /// </summary>
    [JsonPropertyName("hardcodedCrsInterfaced")]
    public CashReceiptStatus HardcodedCrsInterfaced
    {
      get => hardcodedCrsInterfaced ??= new();
      set => hardcodedCrsInterfaced = value;
    }

    /// <summary>
    /// A value of HardcodedCrsBalanced.
    /// </summary>
    [JsonPropertyName("hardcodedCrsBalanced")]
    public CashReceiptStatus HardcodedCrsBalanced
    {
      get => hardcodedCrsBalanced ??= new();
      set => hardcodedCrsBalanced = value;
    }

    /// <summary>
    /// A value of HardcodedCrsForwarded.
    /// </summary>
    [JsonPropertyName("hardcodedCrsForwarded")]
    public CashReceiptStatus HardcodedCrsForwarded
    {
      get => hardcodedCrsForwarded ??= new();
      set => hardcodedCrsForwarded = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeposited")]
    public CashReceiptStatus HardcodedCrsDeposited
    {
      get => hardcodedCrsDeposited ??= new();
      set => hardcodedCrsDeposited = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeleted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeleted")]
    public CashReceiptStatus HardcodedCrsDeleted
    {
      get => hardcodedCrsDeleted ??= new();
      set => hardcodedCrsDeleted = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCatNonCash.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCatNonCash")]
    public CashReceiptType HardcodedCrtCatNonCash
    {
      get => hardcodedCrtCatNonCash ??= new();
      set => hardcodedCrtCatNonCash = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCheck.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCheck")]
    public CashReceiptType HardcodedCrtCheck
    {
      get => hardcodedCrtCheck ??= new();
      set => hardcodedCrtCheck = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFcrtRec.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFcrtRec")]
    public CashReceiptType HardcodedCrtFcrtRec
    {
      get => hardcodedCrtFcrtRec ??= new();
      set => hardcodedCrtFcrtRec = value;
    }

    /// <summary>
    /// A value of HardcodedCrtMoneyOrder.
    /// </summary>
    [JsonPropertyName("hardcodedCrtMoneyOrder")]
    public CashReceiptType HardcodedCrtMoneyOrder
    {
      get => hardcodedCrtMoneyOrder ??= new();
      set => hardcodedCrtMoneyOrder = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCurrency.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCurrency")]
    public CashReceiptType HardcodedCrtCurrency
    {
      get => hardcodedCrtCurrency ??= new();
      set => hardcodedCrtCurrency = value;
    }

    /// <summary>
    /// A value of HardcodedCrtEft.
    /// </summary>
    [JsonPropertyName("hardcodedCrtEft")]
    public CashReceiptType HardcodedCrtEft
    {
      get => hardcodedCrtEft ??= new();
      set => hardcodedCrtEft = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFdirPmt")]
    public CashReceiptType HardcodedCrtFdirPmt
    {
      get => hardcodedCrtFdirPmt ??= new();
      set => hardcodedCrtFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCsenet.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCsenet")]
    public CashReceiptType HardcodedCrtCsenet
    {
      get => hardcodedCrtCsenet ??= new();
      set => hardcodedCrtCsenet = value;
    }

    /// <summary>
    /// A value of HardcodedCrtRdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtRdirPmt")]
    public CashReceiptType HardcodedCrtRdirPmt
    {
      get => hardcodedCrtRdirPmt ??= new();
      set => hardcodedCrtRdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrtManint.
    /// </summary>
    [JsonPropertyName("hardcodedCrtManint")]
    public CashReceiptType HardcodedCrtManint
    {
      get => hardcodedCrtManint ??= new();
      set => hardcodedCrtManint = value;
    }

    /// <summary>
    /// A value of HardcodedCrtInterfund.
    /// </summary>
    [JsonPropertyName("hardcodedCrtInterfund")]
    public CashReceiptType HardcodedCrtInterfund
    {
      get => hardcodedCrtInterfund ??= new();
      set => hardcodedCrtInterfund = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      total = null;
      qtyReceiptAmtAdj = null;
      netReceiptAmt = null;
      newBal = null;
      new1 = null;
      hardcodedCrtCatCash = null;
      current = null;
      low = null;
      numPromptsSelected = null;
      cdValueReturn = null;
      codeValue = null;
      passArea = null;
      pass = null;
      changeStatus = null;
      clearCashReceiptSourceType = null;
      clearCashReceiptEvent = null;
      clearCashReceiptType = null;
      clearCashReceipt = null;
      clearCashReceiptStatus = null;
      hardcodedCrsRecorded = null;
      hardcodedCrsInterfaced = null;
      hardcodedCrsBalanced = null;
      hardcodedCrsForwarded = null;
      hardcodedCrsDeposited = null;
      hardcodedCrsDeleted = null;
      hardcodedCrtCatNonCash = null;
      hardcodedCrtCheck = null;
      hardcodedCrtFcrtRec = null;
      hardcodedCrtMoneyOrder = null;
      hardcodedCrtCurrency = null;
      hardcodedCrtEft = null;
      hardcodedCrtFdirPmt = null;
      hardcodedCrtCsenet = null;
      hardcodedCrtRdirPmt = null;
      hardcodedCrtManint = null;
      hardcodedCrtInterfund = null;
    }

    private CashReceiptDetail total;
    private Common qtyReceiptAmtAdj;
    private Common netReceiptAmt;
    private CashReceipt newBal;
    private CashReceiptEvent new1;
    private CashReceiptType hardcodedCrtCatCash;
    private DateWorkArea current;
    private DateWorkArea low;
    private Common numPromptsSelected;
    private Common cdValueReturn;
    private CodeValue codeValue;
    private Common noOfCrDetails;
    private CashReceipt passArea;
    private Code pass;
    private Common changeStatus;
    private CashReceiptSourceType clearCashReceiptSourceType;
    private CashReceiptEvent clearCashReceiptEvent;
    private CashReceiptType clearCashReceiptType;
    private CashReceipt clearCashReceipt;
    private CashReceiptStatus clearCashReceiptStatus;
    private CashReceiptStatus hardcodedCrsRecorded;
    private CashReceiptStatus hardcodedCrsInterfaced;
    private CashReceiptStatus hardcodedCrsBalanced;
    private CashReceiptStatus hardcodedCrsForwarded;
    private CashReceiptStatus hardcodedCrsDeposited;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptType hardcodedCrtCatNonCash;
    private CashReceiptType hardcodedCrtCheck;
    private CashReceiptType hardcodedCrtFcrtRec;
    private CashReceiptType hardcodedCrtMoneyOrder;
    private CashReceiptType hardcodedCrtCurrency;
    private CashReceiptType hardcodedCrtEft;
    private CashReceiptType hardcodedCrtFdirPmt;
    private CashReceiptType hardcodedCrtCsenet;
    private CashReceiptType hardcodedCrtRdirPmt;
    private CashReceiptType hardcodedCrtManint;
    private CashReceiptType hardcodedCrtInterfund;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptType existingCashReceiptType;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
  }
#endregion
}

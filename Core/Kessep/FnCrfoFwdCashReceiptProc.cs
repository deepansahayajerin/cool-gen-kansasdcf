// Program: FN_CRFO_FWD_CASH_RECEIPT_PROC, ID: 371726908, model: 746.
// Short name: SWECRFOP
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
/// A program: FN_CRFO_FWD_CASH_RECEIPT_PROC.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrfoFwdCashReceiptProc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRFO_FWD_CASH_RECEIPT_PROC program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrfoFwdCashReceiptProc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrfoFwdCashReceiptProc.
  /// </summary>
  public FnCrfoFwdCashReceiptProc(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------------------
    // Date      Developer Name 	Description
    // -------   --------------------  
    // ----------------------------------------
    // 02/06/96  Burrell - SRS     	New Development
    // 02/12/96  Kennedy - MTW		Retrofits
    // 04/08/96  Kennedy - MTW     	Complete code test and fix.
    // 12/13/96  R. Marchman		Add new security/next tran
    // 04/08/97  C. Dasgupta		Added "Program Fund Type" field
    // 				in the screen
    // 04/28/97  JF. Caillouet		Change Current Date
    // 10/01/98  J Katz		Correct the display of status history
    // 				information when receipt is in FWD status.
    // 				Add validation to prevent the display
    // 				of a receipt in DEL status.
    // 				Remove duplicate code and unused
    // 				views.
    // 				Add edits for Forward, Unforward,
    // 				and Update actions.
    // 06/08/99  J Katz		Analyzed READ statements and changed
    // 				read property to Select Only
    // 				where appropriate.
    // 06/24/99  J Katz		Modified protection logic to consistently
    // 				allow the forwarding address and reason
    // 				text to be updated for forwarded receipts.
    // 				Modified 'read only' fields on the screen
    // 				to ensure that the fields stay protected
    // 				in error conditions.
    // ------------------------------------------------------------------------
    // -------------------------------------------------------
    // Parameters passed when called by program CREL or CRTB:
    // 	Cash Receipt Sequential Number
    // -------------------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------
    // If COMMAND is CLEAR, escape before moving imports to exports
    // so that the screen in blanked out.
    // -------------------------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NN0000_ALL_OK";

      return;
    }

    if (Equal(global.Command, "ENTER"))
    {
      global.Command = "UPDATE";
    }

    // ---------------------------------------------
    //           ***SET UP LOCAL VIEWS ***
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // -----------------------------------------------------------
    // The following statements are required for additional cash
    // receipting hardcoded values which are not currently in the
    // FN Hardcoded Cash Receipting CAB.    JLK  09/23/98
    // -----------------------------------------------------------
    local.HardcodedCrtMoneyOrder.SystemGeneratedIdentifier = 3;
    local.HardcodedCrtInterfund.SystemGeneratedIdentifier = 10;

    // ---------------------------------------------
    // Move all IMPORTS to EXPORTS
    // ---------------------------------------------
    MoveCashReceipt1(import.CashReceipt, export.CashReceipt);
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptSourceType(import.CashReceiptSourceType,
      export.CashReceiptSourceType);
    MoveCashReceiptType(import.CashReceiptType, export.CashReceiptType);
    export.CashReceiptStatusHistory.Assign(import.CashReceiptStatusHistory);
    MoveCashReceiptStatus(import.CashReceiptStatus, export.CashReceiptStatus);
    export.Payor.Name = import.Payor.Name;
    export.ActiveStatusCreated.Date = import.ActiveStatusCreated.Date;
    export.HiddenCashReceipt.SequentialNumber =
      import.HiddenCashReceipt.SequentialNumber;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------
    // Next Tran Logic
    // ---------------------------------------------
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
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
      global.Command = "DISPLAY";

      return;
    }

    // ---------------------------------------------
    // Validate action level security
    // ---------------------------------------------
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------
    // Protect forwarding information if the active status for the
    // Cash Receipt is not BAL with a type of Check, Money
    // Order, or Interfund OR if the active status is FWD and the
    // effective date of the active status is not current date.
    // JLK        10/05/98
    // ------------------------------------------------------------
    if (!IsEmpty(export.CashReceiptStatus.Code))
    {
      if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsBalanced.SystemGeneratedIdentifier && (
          export.CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtCheck.SystemGeneratedIdentifier || export
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtMoneyOrder.SystemGeneratedIdentifier || export
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtInterfund.SystemGeneratedIdentifier))
      {
        // ***  Forwarding information may be entered. ***
      }
      else if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsForwarded.SystemGeneratedIdentifier)
      {
        // ***  Forwarding information may be entered. ***
      }
      else
      {
        var field1 = GetField(export.CashReceipt, "forwardedToName");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.CashReceipt, "forwardedStreet1");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.CashReceipt, "forwardedStreet2");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.CashReceipt, "forwardedCity");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.CashReceipt, "forwardedState");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.CashReceipt, "forwardedZip5");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.CashReceipt, "forwardedZip4");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.CashReceipt, "forwardedZip3");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.CashReceiptStatusHistory, "reasonText");

        field9.Color = "cyan";
        field9.Protected = true;
      }
    }

    // ---------------------------------------------
    //               *** Edit Screen ***
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "FORWARD") || Equal(global.Command, "UNFRWRD") || Equal
      (global.Command, "UPDATE"))
    {
      if (import.CashReceipt.SequentialNumber == 0)
      {
        var field = GetField(export.CashReceipt, "sequentialNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }

    if (Equal(global.Command, "FORWARD") || Equal
      (global.Command, "UNFRWRD") || Equal(global.Command, "UPDATE"))
    {
      if (export.HiddenCashReceipt.SequentialNumber != export
        .CashReceipt.SequentialNumber)
      {
        ExitState = "FN0000_MUST_DISPLAY_BEFORE_CHNG";

        return;
      }
    }

    if (Equal(global.Command, "FORWARD") || Equal(global.Command, "UPDATE"))
    {
      // ---------------------------------------------
      // Check for required fields.
      // --------------------------------------------
      if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsBalanced.SystemGeneratedIdentifier && (
          export.CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtCheck.SystemGeneratedIdentifier || export
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtMoneyOrder.SystemGeneratedIdentifier || export
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtInterfund.SystemGeneratedIdentifier) || export
        .CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsForwarded.SystemGeneratedIdentifier)
      {
        if (IsEmpty(import.CashReceipt.ForwardedToName))
        {
          var field = GetField(export.CashReceipt, "forwardedToName");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(import.CashReceipt.ForwardedStreet1) && IsEmpty
          (import.CashReceipt.ForwardedStreet2))
        {
          var field = GetField(export.CashReceipt, "forwardedStreet1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(import.CashReceipt.ForwardedCity))
        {
          var field = GetField(export.CashReceipt, "forwardedCity");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(import.CashReceipt.ForwardedState))
        {
          var field = GetField(export.CashReceipt, "forwardedState");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(import.CashReceipt.ForwardedZip5))
        {
          var field = GetField(export.CashReceipt, "forwardedZip5");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(import.CashReceiptStatusHistory.ReasonText))
        {
          var field = GetField(export.CashReceiptStatusHistory, "reasonText");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // Validate that Zip codes are numeric
        // ---------------------------------------------
        if (Verify(export.CashReceipt.ForwardedZip3, " 0123456789") != 0)
        {
          var field = GetField(export.CashReceipt, "forwardedZip3");

          field.Error = true;

          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
        }

        if (Verify(export.CashReceipt.ForwardedZip4, " 0123456789") != 0)
        {
          var field = GetField(export.CashReceipt, "forwardedZip4");

          field.Error = true;

          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
        }

        if (Verify(export.CashReceipt.ForwardedZip5, "0123456789") != 0)
        {
          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

          var field = GetField(export.CashReceipt, "forwardedZip5");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // Check for valid state code
        // ---------------------------------------------
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = import.CashReceipt.ForwardedState ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidStateCode.Flag) == 'N')
        {
          var field = GetField(export.CashReceipt, "forwardedState");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_CODE";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_CR_STATUS_OR_TYPE_INVALID";

        return;
      }
    }

    // ---------------------------------------------
    // Main CASE OF COMMAND
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!ReadCashReceipt2())
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }

        if (!ReadCashReceiptStatusHistory1())
        {
          ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";

          return;
        }

        if (!ReadCashReceiptStatus1())
        {
          ExitState = "FN0108_CASH_RCPT_STAT_NF";

          return;
        }

        // -------------------------------------------------------------
        // If active status is DEL, cash receipt does not logically exist.
        // Set exit state to not found condition.
        // -------------------------------------------------------------
        if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier)
        {
          MoveCashReceipt2(local.ClearCashReceipt, export.CashReceipt);
          MoveCashReceiptSourceType(local.ClearCashReceiptSourceType,
            export.CashReceiptSourceType);
          MoveCashReceiptType(local.ClearCashReceiptType, export.CashReceiptType);
            
          MoveCashReceiptStatus(local.ClearCashReceiptStatus,
            export.CashReceiptStatus);
          export.CashReceiptStatusHistory.Assign(
            local.ClearCashReceiptStatusHistory);
          export.ActiveStatusCreated.Date = local.ClearDateWorkArea.Date;
          export.Payor.Name = "";
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }

        // ---------------------------------------------
        // Get Source Code
        // ---------------------------------------------
        if (!ReadCashReceiptEvent())
        {
          ExitState = "FN0000_CASH_RCPT_EVENT_NF";

          return;
        }

        if (!ReadCashReceiptSourceType())
        {
          ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

          return;
        }

        // ---------------------------------------------
        // Get Receipt Type
        // ---------------------------------------------
        if (!ReadCashReceiptType())
        {
          ExitState = "FN0113_CASH_RCPT_TYPE_NF";

          return;
        }

        // --------------------------------------------
        // Move all Entity Action Views to Export Views
        // --------------------------------------------
        export.CashReceipt.Assign(entities.CashReceipt);
        export.HiddenCashReceipt.SequentialNumber =
          entities.CashReceipt.SequentialNumber;
        MoveCashReceiptSourceType(entities.CashReceiptSourceType,
          export.CashReceiptSourceType);
        export.CashReceiptEvent.SystemGeneratedIdentifier =
          entities.CashReceiptEvent.SystemGeneratedIdentifier;
        MoveCashReceiptType(entities.CashReceiptType, export.CashReceiptType);
        MoveCashReceiptStatus(entities.CashReceiptStatus,
          export.CashReceiptStatus);
        export.ActiveStatusCreated.Date =
          Date(entities.CashReceiptStatusHistory.CreatedTimestamp);

        if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsForwarded.SystemGeneratedIdentifier)
        {
          export.CashReceiptStatusHistory.Assign(
            entities.CashReceiptStatusHistory);

          // -----------------------------------------------------------
          // If Cash Receipt forwarding information was changed
          // subsequent to the receipt being assigned an active status
          // of FWD, display the Last Updated Timestamp.
          // JLK     11/03/98
          // -----------------------------------------------------------
          if (Lt(entities.CashReceiptStatusHistory.CreatedTimestamp,
            entities.CashReceipt.LastUpdatedTimestamp))
          {
            export.CashReceipt.LastUpdatedTimestamp =
              entities.CashReceipt.LastUpdatedTimestamp;
          }
          else
          {
            export.CashReceipt.LastUpdatedTimestamp =
              local.ClearCashReceipt.LastUpdatedTimestamp;
          }
        }
        else
        {
          export.CashReceiptStatusHistory.Assign(
            local.ClearCashReceiptStatusHistory);
          export.CashReceipt.LastUpdatedTimestamp =
            local.ClearCashReceipt.LastUpdatedTimestamp;
        }

        // --------------------------------------------
        // Format Payor Name if used
        // --------------------------------------------
        if (!IsEmpty(entities.CashReceipt.PayorLastName))
        {
          export.Payor.Name = TrimEnd(entities.CashReceipt.PayorLastName) + ", " +
            TrimEnd(entities.CashReceipt.PayorFirstName) + " " + TrimEnd
            (entities.CashReceipt.PayorMiddleName);
        }
        else
        {
          export.Payor.Name = entities.CashReceipt.PayorOrganization ?? Spaces
            (50);
        }

        // -------------------------------------------------------------
        // Cash Receipt must have an active status of BAL or a type of
        // Check, Money Order, or Interfund to forward.
        // If the active status is FWD and the active status history
        // created date is equal to current date, the forwarding
        // information can be updated.      JLK  10/05/98
        // Removed the edit mandating that the Cash Receipt forwarding
        // information can only be updated on the same that the the
        // receipt was placed in FWD status.    JLK   11/03/98
        // -------------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsBalanced.SystemGeneratedIdentifier && (
            export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtCheck.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtMoneyOrder.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtInterfund.SystemGeneratedIdentifier))
        {
          var field1 = GetField(export.CashReceipt, "forwardedToName");

          field1.Color = "green";
          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.CashReceipt, "forwardedStreet1");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.CashReceipt, "forwardedStreet2");

          field3.Color = "green";
          field3.Protected = false;

          var field4 = GetField(export.CashReceipt, "forwardedCity");

          field4.Color = "green";
          field4.Protected = false;

          var field5 = GetField(export.CashReceipt, "forwardedState");

          field5.Color = "green";
          field5.Protected = false;

          var field6 = GetField(export.CashReceipt, "forwardedZip5");

          field6.Color = "green";
          field6.Protected = false;

          var field7 = GetField(export.CashReceipt, "forwardedZip4");

          field7.Color = "green";
          field7.Protected = false;

          var field8 = GetField(export.CashReceipt, "forwardedZip3");

          field8.Color = "green";
          field8.Protected = false;

          var field9 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field9.Color = "green";
          field9.Protected = false;
        }
        else if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsForwarded.SystemGeneratedIdentifier)
        {
          var field1 = GetField(export.CashReceipt, "forwardedToName");

          field1.Color = "green";
          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.CashReceipt, "forwardedStreet1");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.CashReceipt, "forwardedStreet2");

          field3.Color = "green";
          field3.Protected = false;

          var field4 = GetField(export.CashReceipt, "forwardedCity");

          field4.Color = "green";
          field4.Protected = false;

          var field5 = GetField(export.CashReceipt, "forwardedState");

          field5.Color = "green";
          field5.Protected = false;

          var field6 = GetField(export.CashReceipt, "forwardedZip5");

          field6.Color = "green";
          field6.Protected = false;

          var field7 = GetField(export.CashReceipt, "forwardedZip4");

          field7.Color = "green";
          field7.Protected = false;

          var field8 = GetField(export.CashReceipt, "forwardedZip3");

          field8.Color = "green";
          field8.Protected = false;

          var field9 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field9.Color = "green";
          field9.Protected = false;
        }
        else
        {
          var field1 = GetField(export.CashReceipt, "forwardedToName");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.CashReceipt, "forwardedStreet1");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceipt, "forwardedStreet2");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.CashReceipt, "forwardedCity");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.CashReceipt, "forwardedState");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.CashReceipt, "forwardedZip5");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.CashReceipt, "forwardedZip4");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.CashReceipt, "forwardedZip3");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field9.Color = "cyan";
          field9.Protected = true;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "FORWARD":
        // -----------------------------------------------------
        // Only receipts with a status of BAL can be forwarded.
        // -----------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsBalanced.SystemGeneratedIdentifier)
        {
        }
        else
        {
          ExitState = "INVALID_STATUS_FOR_FORWARDING";

          return;
        }

        // -----------------------------------------------------
        // Only receipts with a type of Check, Money Order, or
        // Interfund can be forwarded.     JLK  09/24/98
        // -----------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtCheck.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtMoneyOrder.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtInterfund.SystemGeneratedIdentifier)
        {
        }
        else
        {
          ExitState = "FN0000_INVALID_CR_TYPE_4_FWDING";

          return;
        }

        // -----------------------------------------------------
        //              Forward the Cash Receipt
        // The fn_forward_cash_rcpt AB has validation logic to
        // ensure that the receipt to be forwarded does not have
        // details.
        // -----------------------------------------------------
        UseFnForwardCashRcpt2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.ActiveStatusCreated.Date =
            Date(export.CashReceiptStatusHistory.CreatedTimestamp);
          export.CashReceipt.LastUpdatedTimestamp =
            local.ClearCashReceipt.LastUpdatedTimestamp;
          ExitState = "FN0000_FORWARD_SUCCESSFUL";
        }

        break;
      case "UPDATE":
        if (export.CashReceiptStatus.SystemGeneratedIdentifier != local
          .HardcodedCrsForwarded.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_FORWARD_UPDATE_NOT_ALLWD";

          return;
        }

        // -------------------------------------------------------------
        // If the active status is FWD and the active status history
        // created date is equal to current date, the forwarding
        // information can be updated.      JLK  10/05/98
        // Removed the edit mandating that the Cash Receipt forwarding
        // information can only be updated on the same that the the
        // receipt was placed in FWD status.    JLK   11/03/98
        // -------------------------------------------------------------
        if (!ReadCashReceipt1())
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }

        if (!ReadCashReceiptStatusHistory1())
        {
          ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";

          return;
        }

        if (!ReadCashReceiptStatus2())
        {
          ExitState = "FN0108_CASH_RCPT_STAT_NF";

          return;
        }

        UseFnForwardCashRcpt1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -------------------------------------------------------------
          // Set the Forwarded Address Last Updated timestamp on the
          // screen to the local Cash Receipt Last Updated timestamp
          // assigned to the requested update action.    JLK   11/03/98
          // -------------------------------------------------------------
          export.CashReceipt.LastUpdatedTimestamp =
            local.ForwardedAddress.LastUpdatedTimestamp;
          export.ActiveStatusCreated.Date =
            Date(export.CashReceiptStatusHistory.CreatedTimestamp);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "UNFRWRD":
        // ---------------------------------------------------------------
        // Receipt must be in Forwarded status to Unforward.
        // Receipt must have been Forwarded on current date to
        // Unforward.
        // ---------------------------------------------------------------
        if (export.CashReceiptStatus.SystemGeneratedIdentifier != local
          .HardcodedCrsForwarded.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_FORWARD_UPDATE_NOT_ALLWD";

          return;
        }

        if (!Equal(Date(export.CashReceiptStatusHistory.CreatedTimestamp),
          local.Current.Date))
        {
          ExitState = "FN0000_RECEIPT_ALREADY_FORWARDED";

          return;
        }

        if (!ReadCashReceipt1())
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }

        if (!ReadCashReceiptStatusHistory1())
        {
          ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";

          return;
        }

        if (!ReadCashReceiptStatus2())
        {
          ExitState = "FN0108_CASH_RCPT_STAT_NF";

          return;
        }

        // ------------------------------------------------------
        // Remove forwarding info from cash receipt.
        // [Changed update logic to only include the attributes
        // appropriate for the Unforwarding action. JLK 09/25]
        // ------------------------------------------------------
        // 	
        export.CashReceipt.Assign(entities.CashReceipt);
        export.CashReceipt.ForwardedToName = "";
        export.CashReceipt.ForwardedStreet1 = "";
        export.CashReceipt.ForwardedStreet2 = "";
        export.CashReceipt.ForwardedCity = "";
        export.CashReceipt.ForwardedState = "";
        export.CashReceipt.ForwardedZip5 = "";
        export.CashReceipt.ForwardedZip4 = "";
        export.CashReceipt.ForwardedZip3 = "";

        try
        {
          UpdateCashReceipt();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0088_CASH_RCPT_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0090_CASH_RCPT_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // ------------------------------------------------------------
        // Find cash receipt status history prior to current status
        // history of forward.  Since update function creates a new
        // forward status history, read until prior status is not equal
        // to forward.  Save prior status so new status history can be
        // associated to it -- in effect putting receipt in status
        // prior to forward status.
        // -----------------------------------------------------------
        foreach(var item in ReadCashReceiptStatusHistory2())
        {
          if (!Lt(local.Current.Date,
            entities.CashReceiptStatusHistory.DiscontinueDate))
          {
            // ---------------------------------------------
            // Get prior status id
            // ---------------------------------------------
            if (ReadCashReceiptStatus2())
            {
              if (entities.Persistent.SystemGeneratedIdentifier != local
                .HardcodedCrsForwarded.SystemGeneratedIdentifier)
              {
                break;
              }
            }
            else
            {
              ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

              return;
            }
          }
        }

        export.CashReceiptStatusHistory.Assign(
          local.ClearCashReceiptStatusHistory);
        UseFnChangeCashRcptStatusHist();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCashReceiptStatus(entities.Persistent, export.CashReceiptStatus);
          export.ActiveStatusCreated.Date =
            Date(export.CashReceiptStatusHistory.CreatedTimestamp);
          export.CashReceipt.LastUpdatedTimestamp =
            local.ClearCashReceipt.LastUpdatedTimestamp;
          export.CashReceiptStatusHistory.Assign(
            local.ClearCashReceiptStatusHistory);
          ExitState = "FN0000_UNFORWARD_SUCCESSFUL";
        }
        else
        {
          ExitState = "FN0000_ROLLBACK_RB";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCashReceipt1(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.ForwardedToName = source.ForwardedToName;
    target.ForwardedStreet1 = source.ForwardedStreet1;
    target.ForwardedStreet2 = source.ForwardedStreet2;
    target.ForwardedCity = source.ForwardedCity;
    target.ForwardedState = source.ForwardedState;
    target.ForwardedZip5 = source.ForwardedZip5;
    target.ForwardedZip4 = source.ForwardedZip4;
    target.ForwardedZip3 = source.ForwardedZip3;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveCashReceipt2(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorFirstName = source.PayorFirstName;
    target.PayorMiddleName = source.PayorMiddleName;
    target.PayorLastName = source.PayorLastName;
    target.ForwardedToName = source.ForwardedToName;
    target.ForwardedStreet1 = source.ForwardedStreet1;
    target.ForwardedStreet2 = source.ForwardedStreet2;
    target.ForwardedCity = source.ForwardedCity;
    target.ForwardedState = source.ForwardedState;
    target.ForwardedZip5 = source.ForwardedZip5;
    target.ForwardedZip4 = source.ForwardedZip4;
    target.ForwardedZip3 = source.ForwardedZip3;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveCashReceipt3(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.ForwardedToName = source.ForwardedToName;
    target.ForwardedStreet1 = source.ForwardedStreet1;
    target.ForwardedStreet2 = source.ForwardedStreet2;
    target.ForwardedCity = source.ForwardedCity;
    target.ForwardedState = source.ForwardedState;
    target.ForwardedZip5 = source.ForwardedZip5;
    target.ForwardedZip4 = source.ForwardedZip4;
    target.ForwardedZip3 = source.ForwardedZip3;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidStateCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseFnChangeCashRcptStatusHist()
  {
    var useImport = new FnChangeCashRcptStatusHist.Import();
    var useExport = new FnChangeCashRcptStatusHist.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.NewPersistent.Assign(entities.Persistent);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptStatusHistory.ReasonText =
      export.CashReceiptStatusHistory.ReasonText;

    Call(FnChangeCashRcptStatusHist.Execute, useImport, useExport);

    MoveCashReceiptStatus(useImport.NewPersistent, entities.Persistent);
    export.CashReceiptStatusHistory.Assign(useExport.CashReceiptStatusHistory);
  }

  private void UseFnForwardCashRcpt1()
  {
    var useImport = new FnForwardCashRcpt.Import();
    var useExport = new FnForwardCashRcpt.Export();

    MoveCashReceipt3(export.CashReceipt, useImport.CashReceipt);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptStatusHistory.ReasonText =
      export.CashReceiptStatusHistory.ReasonText;

    Call(FnForwardCashRcpt.Execute, useImport, useExport);

    local.ForwardedAddress.LastUpdatedTimestamp =
      useExport.ForwardedAddress.LastUpdatedTimestamp;
    export.CashReceiptStatusHistory.Assign(useExport.CashReceiptStatusHistory);
    MoveCashReceiptStatus(useExport.CashReceiptStatus, export.CashReceiptStatus);
      
  }

  private void UseFnForwardCashRcpt2()
  {
    var useImport = new FnForwardCashRcpt.Import();
    var useExport = new FnForwardCashRcpt.Export();

    MoveCashReceipt3(export.CashReceipt, useImport.CashReceipt);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptStatusHistory.ReasonText =
      export.CashReceiptStatusHistory.ReasonText;

    Call(FnForwardCashRcpt.Execute, useImport, useExport);

    export.CashReceiptStatusHistory.Assign(useExport.CashReceiptStatusHistory);
    MoveCashReceiptStatus(useExport.CashReceiptStatus, export.CashReceiptStatus);
      
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsBalanced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdBalanced.SystemGeneratedIdentifier;
    local.HardcodedCrsForwarded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdForwarded.SystemGeneratedIdentifier;
    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrtCheck.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdCheck.SystemGeneratedIdentifier;
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

  private bool ReadCashReceipt1()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 6);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 7);
        entities.CashReceipt.PayorOrganization =
          db.GetNullableString(reader, 8);
        entities.CashReceipt.PayorFirstName = db.GetNullableString(reader, 9);
        entities.CashReceipt.PayorMiddleName = db.GetNullableString(reader, 10);
        entities.CashReceipt.PayorLastName = db.GetNullableString(reader, 11);
        entities.CashReceipt.ForwardedToName = db.GetNullableString(reader, 12);
        entities.CashReceipt.ForwardedStreet1 =
          db.GetNullableString(reader, 13);
        entities.CashReceipt.ForwardedStreet2 =
          db.GetNullableString(reader, 14);
        entities.CashReceipt.ForwardedCity = db.GetNullableString(reader, 15);
        entities.CashReceipt.ForwardedState = db.GetNullableString(reader, 16);
        entities.CashReceipt.ForwardedZip5 = db.GetNullableString(reader, 17);
        entities.CashReceipt.ForwardedZip4 = db.GetNullableString(reader, 18);
        entities.CashReceipt.ForwardedZip3 = db.GetNullableString(reader, 19);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceipt2()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 6);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 7);
        entities.CashReceipt.PayorOrganization =
          db.GetNullableString(reader, 8);
        entities.CashReceipt.PayorFirstName = db.GetNullableString(reader, 9);
        entities.CashReceipt.PayorMiddleName = db.GetNullableString(reader, 10);
        entities.CashReceipt.PayorLastName = db.GetNullableString(reader, 11);
        entities.CashReceipt.ForwardedToName = db.GetNullableString(reader, 12);
        entities.CashReceipt.ForwardedStreet1 =
          db.GetNullableString(reader, 13);
        entities.CashReceipt.ForwardedStreet2 =
          db.GetNullableString(reader, 14);
        entities.CashReceipt.ForwardedCity = db.GetNullableString(reader, 15);
        entities.CashReceipt.ForwardedState = db.GetNullableString(reader, 16);
        entities.CashReceipt.ForwardedZip5 = db.GetNullableString(reader, 17);
        entities.CashReceipt.ForwardedZip4 = db.GetNullableString(reader, 18);
        entities.CashReceipt.ForwardedZip3 = db.GetNullableString(reader, 19);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus1()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.CashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus2()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);
    entities.Persistent.Populated = false;

    return Read("ReadCashReceiptStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.CashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        entities.Persistent.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Persistent.Code = db.GetString(reader, 1);
        entities.Persistent.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory1",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.CashReceiptStatusHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;

    return ReadEach("ReadCashReceiptStatusHistory2",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.CashReceiptStatusHistory.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var forwardedToName = export.CashReceipt.ForwardedToName ?? "";
    var forwardedStreet1 = export.CashReceipt.ForwardedStreet1 ?? "";
    var forwardedStreet2 = export.CashReceipt.ForwardedStreet2 ?? "";
    var forwardedCity = export.CashReceipt.ForwardedCity ?? "";
    var forwardedState = export.CashReceipt.ForwardedState ?? "";
    var forwardedZip5 = export.CashReceipt.ForwardedZip5 ?? "";
    var forwardedZip4 = export.CashReceipt.ForwardedZip4 ?? "";
    var forwardedZip3 = export.CashReceipt.ForwardedZip3 ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableString(command, "frwrdToName", forwardedToName);
        db.SetNullableString(command, "frwrdStreet1", forwardedStreet1);
        db.SetNullableString(command, "frwrdStreet2", forwardedStreet2);
        db.SetNullableString(command, "frwrdCity", forwardedCity);
        db.SetNullableString(command, "frwrdState", forwardedState);
        db.SetNullableString(command, "frwrdZip5", forwardedZip5);
        db.SetNullableString(command, "frwrdZip4", forwardedZip4);
        db.SetNullableString(command, "frwrdZip3", forwardedZip3);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.ForwardedToName = forwardedToName;
    entities.CashReceipt.ForwardedStreet1 = forwardedStreet1;
    entities.CashReceipt.ForwardedStreet2 = forwardedStreet2;
    entities.CashReceipt.ForwardedCity = forwardedCity;
    entities.CashReceipt.ForwardedState = forwardedState;
    entities.CashReceipt.ForwardedZip5 = forwardedZip5;
    entities.CashReceipt.ForwardedZip4 = forwardedZip4;
    entities.CashReceipt.ForwardedZip3 = forwardedZip3;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
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
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public FnConcatName Payor
    {
      get => payor ??= new();
      set => payor = value;
    }

    /// <summary>
    /// A value of ActiveStatusCreated.
    /// </summary>
    [JsonPropertyName("activeStatusCreated")]
    public DateWorkArea ActiveStatusCreated
    {
      get => activeStatusCreated ??= new();
      set => activeStatusCreated = value;
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

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private FnConcatName payor;
    private DateWorkArea activeStatusCreated;
    private CashReceipt hiddenCashReceipt;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
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
    /// A value of ActiveStatusCreated.
    /// </summary>
    [JsonPropertyName("activeStatusCreated")]
    public DateWorkArea ActiveStatusCreated
    {
      get => activeStatusCreated ??= new();
      set => activeStatusCreated = value;
    }

    /// <summary>
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public FnConcatName Payor
    {
      get => payor ??= new();
      set => payor = value;
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

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private DateWorkArea activeStatusCreated;
    private FnConcatName payor;
    private CashReceipt hiddenCashReceipt;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForwardedAddress.
    /// </summary>
    [JsonPropertyName("forwardedAddress")]
    public CashReceipt ForwardedAddress
    {
      get => forwardedAddress ??= new();
      set => forwardedAddress = value;
    }

    /// <summary>
    /// A value of CrDetails.
    /// </summary>
    [JsonPropertyName("crDetails")]
    public Common CrDetails
    {
      get => crDetails ??= new();
      set => crDetails = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of ValidStateCode.
    /// </summary>
    [JsonPropertyName("validStateCode")]
    public Common ValidStateCode
    {
      get => validStateCode ??= new();
      set => validStateCode = value;
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
    /// A value of HardcodedCrsDeleted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeleted")]
    public CashReceiptStatus HardcodedCrsDeleted
    {
      get => hardcodedCrsDeleted ??= new();
      set => hardcodedCrsDeleted = value;
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
    /// A value of HardcodedCrtMoneyOrder.
    /// </summary>
    [JsonPropertyName("hardcodedCrtMoneyOrder")]
    public CashReceiptType HardcodedCrtMoneyOrder
    {
      get => hardcodedCrtMoneyOrder ??= new();
      set => hardcodedCrtMoneyOrder = value;
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
    /// A value of ClearCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("clearCashReceiptSourceType")]
    public CashReceiptSourceType ClearCashReceiptSourceType
    {
      get => clearCashReceiptSourceType ??= new();
      set => clearCashReceiptSourceType = value;
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
    /// A value of ClearCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("clearCashReceiptStatus")]
    public CashReceiptStatus ClearCashReceiptStatus
    {
      get => clearCashReceiptStatus ??= new();
      set => clearCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("clearCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ClearCashReceiptStatusHistory
    {
      get => clearCashReceiptStatusHistory ??= new();
      set => clearCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ClearDateWorkArea.
    /// </summary>
    [JsonPropertyName("clearDateWorkArea")]
    public DateWorkArea ClearDateWorkArea
    {
      get => clearDateWorkArea ??= new();
      set => clearDateWorkArea = value;
    }

    private CashReceipt forwardedAddress;
    private Common crDetails;
    private DateWorkArea current;
    private DateWorkArea maximum;
    private Code code;
    private CodeValue codeValue;
    private Common validStateCode;
    private CashReceiptStatus hardcodedCrsBalanced;
    private CashReceiptStatus hardcodedCrsForwarded;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptType hardcodedCrtCheck;
    private CashReceiptType hardcodedCrtMoneyOrder;
    private CashReceiptType hardcodedCrtInterfund;
    private CashReceipt clearCashReceipt;
    private CashReceiptSourceType clearCashReceiptSourceType;
    private CashReceiptType clearCashReceiptType;
    private CashReceiptStatus clearCashReceiptStatus;
    private CashReceiptStatusHistory clearCashReceiptStatusHistory;
    private DateWorkArea clearDateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptStatus Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus persistent;
  }
#endregion
}

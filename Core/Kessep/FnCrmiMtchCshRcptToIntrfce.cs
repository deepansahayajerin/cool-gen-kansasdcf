// Program: FN_CRMI_MTCH_CSH_RCPT_TO_INTRFCE, ID: 372346397, model: 746.
// Short name: SWECRMIP
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
/// A program: FN_CRMI_MTCH_CSH_RCPT_TO_INTRFCE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrmiMtchCshRcptToIntrfce: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRMI_MTCH_CSH_RCPT_TO_INTRFCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrmiMtchCshRcptToIntrfce(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrmiMtchCshRcptToIntrfce.
  /// </summary>
  public FnCrmiMtchCshRcptToIntrfce(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Every initial development and change to that development needs to
    // be documented below.
    // ----------------------------------------------------------------------------
    // Date 	  Developer Name     Request #  Description
    // ----------------------------------------------------------------------------
    // 02/05/96  Holly Kennedy-MTW		Retrofits
    // 04/03/96  Holly Kennedy-MTW		Added a Starting Cash Receipt
    // 					value to the screen in order
    // 					to view more data.
    // 12/16/96  R. Marchman			Add new security/next tran
    // 04/28/97  JF. Caillouet			Change Current Date
    // 09/24/97  Siraj Konkader		Discussed w/ Gina and
    // 					Schiffelbein. Agreed to remove
    // 					edits on commands of NOMATCH
    // 					and HOLD that were checking
    // 					for certain types of Cash
    // 					Receipt Type.
    // 10/17/97  Siraj Konkader		Modified pfkey NoMatch to
    // 					Interface and added pfkey
    // 					Receipt, command = RECEIPT
    // 					per PR# xxxxx Tim Hood.
    // 01/12/98  Venkatesh Kamaraj   		Deleted the HOLD logic. The
    // 					request that the HOLD key
    // 					should make the Cash Receipt
    // 					in Recorded Status made
    // 					it similar to Receipt Key.
    // 12/08/98  Judy Katz - SRG		Remove logic for XXFMMENU
    // 					command which is not valid for
    // 					this procedure.
    // 					Add in-code documentation.
    // 05/26/99  Judy Katz - SRG		Modify display logic to skip
    // 					receipts with INTF status that
    // 					have a net interface amount
    // 					(cash due) that is less than
    // 					or equal to zero.
    // 06/08/99  Judy Katz - SRG		Analyze READ statements and
    // 					change read property to
    // 					Select Only where appropriate.
    // 09/10/99	J Katz - SRG		Remove edit requiring the Check
    // 					Date to match the Interface
    // 					Date per Heat problem report
    // 					# H 00073153.
    // ---------------------------------------------------------------------------
    // Production Fix Modifications
    // ----------------------------------------------------------------------------
    // Date 	  Developer Name     Request #  Description
    // ----------------------------------------------------------------------------
    // 09/11/99  J Katz - SRG	H00072967	Modify CRMI to check for receipt
    // 					status of DEL when determining if
    // 					the interface receipt already
    // 					exists.
    // 10/05/99  J Katz - SRG	H00075873	Modify the interface adjustment
    // 					amount calculation.
    // ----------------------------------------------------------------------------
    // 06/22/12 GVandy	 CQ33628  		Make KSDLUI source type an INTERFUND
    // 					cash receipt type for UNMATCH.	
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // -------------------------------------------------------------------
    // Next Tran Logic
    // If the next tran info is not equal to spaces, the user requested a
    // next tran action.  Now validate the requested next tran.
    // -------------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Standard.NextTransaction = import.Standard.NextTransaction;
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
      global.Command = "DISPLAY";

      return;
    }

    // -------------------------------------------------------------------
    // Validate action level security
    // -------------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "MATCH") || Equal
      (global.Command, "INTRFACE") || Equal(global.Command, "RECEIPT") || Equal
      (global.Command, "UNMATCH") || Equal(global.Command, "CREC"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------
    // Move Imports to Exports
    // -------------------------------------------------------------------
    MoveCashReceiptSourceType(import.ReceiptedCashReceiptSourceType,
      export.ReceiptedCashReceiptSourceType);
    export.ReceiptedCashReceipt.Assign(import.ReceiptedCashReceipt);
    export.Starting.SequentialNumber = import.Starting.SequentialNumber;
    export.CheckOrIntf.Date = import.CheckOrIntf.Date;
    export.HiddenCashReceiptEvent.Assign(import.HiddenCashReceiptEvent);
    export.HiddenCashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    export.HiddenCashReceipt.SequentialNumber =
      import.HiddenCashReceipt.SequentialNumber;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.CashReceiptEvent.Assign(
        import.Import1.Item.CashReceiptEvent);
      export.Export1.Update.CashReceipt.Assign(import.Import1.Item.CashReceipt);
      export.Export1.Update.IntfAdjAmt.TotalCurrency =
        import.Import1.Item.IntfAdjAmt.TotalCurrency;
      export.Export1.Update.HiddenCashReceiptSourceType.
        SystemGeneratedIdentifier =
          import.Import1.Item.HiddenCashReceiptSourceType.
          SystemGeneratedIdentifier;
      export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
        import.Import1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;

      switch(AsChar(export.Export1.Item.Common.SelectChar))
      {
        case 'S':
          export.HiddenCashReceipt.SequentialNumber =
            export.Export1.Item.CashReceipt.SequentialNumber;
          export.FlowCashReceiptEvent.Assign(
            export.Export1.Item.CashReceiptEvent);
          export.FlowCashReceiptSourceType.SystemGeneratedIdentifier =
            export.Export1.Item.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier;
          export.FlowCashReceiptType.SystemGeneratedIdentifier =
            export.Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
            

          var field1 = GetField(export.Export1.Item.Common, "selectChar");

          field1.Error = true;

          ++local.SelectionCount.Count;

          break;
        case ' ':
          break;
        default:
          var field2 = GetField(export.Export1.Item.Common, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      export.Export1.Next();
    }

    if (local.SelectionCount.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------
    // Set up local views
    // -------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();
    local.HardcodedCrtEft.SystemGeneratedIdentifier = 6;
    local.HardcodedCrtInterfund.SystemGeneratedIdentifier = 10;
    local.HardcodedCrtManint.SystemGeneratedIdentifier = 12;

    // -------------------------------------------------------------------
    // Only open the Original Interface Amount for data entry if the
    // receipt has not already been created and the Receipt Type
    // is MANINT.
    // -------------------------------------------------------------------
    if (export.ReceiptedCashReceipt.SequentialNumber > 0 && export
      .HiddenCashReceiptEvent.SystemGeneratedIdentifier > 0)
    {
      // ----------------------------------------------------------------
      // The receipt displayed in the top portion of the screen has
      // already been created.
      // ----------------------------------------------------------------
      if (Equal(global.Command, "CREC"))
      {
        if (Lt(local.Null1.Date,
          export.HiddenCashReceiptEvent.SourceCreationDate))
        {
          export.CheckOrIntf.Date = export.HiddenCashReceiptEvent.ReceivedDate;
        }
        else
        {
          export.CheckOrIntf.Date = export.ReceiptedCashReceipt.CheckDate;
        }

        global.Command = "DISPLAY";
      }

      var field =
        GetField(export.ReceiptedCashReceipt, "totalCashTransactionAmount");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      // ----------------------------------------------------------------
      // The receipt displayed in the top portion of the screen has
      // not yet been created.
      // ----------------------------------------------------------------
      if (export.HiddenCashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtManint.SystemGeneratedIdentifier)
      {
        // ------------------------------------------------------------
        // The receipt type is MANINT indicating that an interface is
        // to be created.  The Match and Receipt actions are not valid.
        // Allow the Original Interface Amount to be entered.
        // ------------------------------------------------------------
        if (Equal(global.Command, "CREC"))
        {
          export.HiddenCashReceiptEvent.SourceCreationDate =
            export.HiddenCashReceiptEvent.ReceivedDate;
          export.HiddenCashReceiptEvent.AnticipatedCheckAmt =
            export.ReceiptedCashReceipt.ReceiptAmount;
          export.ReceiptedCashReceipt.ReceivedDate =
            export.HiddenCashReceiptEvent.ReceivedDate;
          export.ReceiptedCashReceipt.CashDue =
            export.ReceiptedCashReceipt.ReceiptAmount;
          export.ReceiptedCashReceipt.ReceiptAmount = 0;
          export.ReceiptedCashReceipt.CashBalanceAmt =
            export.ReceiptedCashReceipt.CashDue.GetValueOrDefault();

          if (export.ReceiptedCashReceipt.CashDue.GetValueOrDefault() > 0)
          {
            export.ReceiptedCashReceipt.CashBalanceReason = "UNDER";
          }

          export.CheckOrIntf.Date = export.HiddenCashReceiptEvent.ReceivedDate;
          global.Command = "DISPLAY";
        }
      }
      else
      {
        // -------------------------------------------------------------
        // The receipt type is not MANINT indicating that a cash
        // instrument is being receipted into the system.  Valid
        // actions are PF16 Match or PF19 Receipt.
        // Initial processing when accessing screen from CREC.
        // -------------------------------------------------------------
        if (Equal(global.Command, "CREC"))
        {
          export.CheckOrIntf.Date = export.ReceiptedCashReceipt.CheckDate;
          global.Command = "DISPLAY";
        }

        // ------------------------------------------------------------
        // The Original Interface Amount field is not valid for data
        // entry and should be protected.
        // ------------------------------------------------------------
        var field =
          GetField(export.ReceiptedCashReceipt, "totalCashTransactionAmount");

        field.Color = "cyan";
        field.Protected = true;
      }
    }

    // -------------------------------------------------------------------
    // Main Case of Command
    // -------------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ----------------------------------------------------
        // PF2  Display
        // Display logic is located at the bottom of the PrAD.
        // ----------------------------------------------------
        break;
      case "MATCH":
        // ---------------------------------------------------------------
        // PF16  Match
        // Data validation section:
        //   *  Cash Receipt Type cannot be MANINT [Id = 12].
        //   *  Imported cash receipt to be matched cannot be an
        //      interface receipt.
        //   *  One interface receipt must be selected from the list.
        //   *  The cash receipt Check Date must be the same as the
        //      interface receipt Cash Receipt Event Received Date.
        //   *  Status of cash receipt cannot be INTF or DEL.
        // ---------------------------------------------------------------
        if (export.HiddenCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtManint.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_MANINT_FUNCTION";

          return;
        }

        if (Lt(local.Null1.Date,
          export.HiddenCashReceiptEvent.SourceCreationDate))
        {
          ExitState = "FN0000_INVALID_CR_FOR_MATCH";

          return;
        }

        if (local.SelectionCount.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        // ---------------------------------------------------------------
        // Edit requiring the cash receipt Check Date to be the same
        // as the Interface Date [Cash Receipt Event Received Date]
        // was found to be too restrictive.  Several courts do not date
        // the check to match the interface business date.
        // JLK  09/10/99
        // ---------------------------------------------------------------
        if (import.ReceiptedCashReceipt.SequentialNumber > 0)
        {
          // -- Get the current status of the Monetary Instrument
          if (ReadCashReceiptStatus2())
          {
            if (entities.EavCashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedCrsInterfaced.SystemGeneratedIdentifier || entities
              .EavCashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedCrsDeleted.SystemGeneratedIdentifier)
            {
              ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

              return;
            }

            local.CashReceiptStatus.SystemGeneratedIdentifier =
              entities.EavCashReceiptStatus.SystemGeneratedIdentifier;
          }
          else
          {
            ExitState = "FN0108_CASH_RCPT_STAT_NF";

            return;
          }
        }
        else
        {
          local.CashReceiptStatus.SystemGeneratedIdentifier =
            local.HardcodedCrsReceipted.SystemGeneratedIdentifier;
        }

        // ---------------------------------------------------------------
        // Set the Cash Receipt Created By value of the interface
        // receipt to the person who is matching the receipt to the
        // interface.  This will facilitate the balancing and depositing
        // actions on CRTB.  JLK  02/15/99
        // ---------------------------------------------------------------
        export.ReceiptedCashReceipt.CreatedBy = global.UserId;
        UseFnMatchInterfaceCashReceipt();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "DISPLAY";
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          UseEabRollbackCics();
        }

        return;
      case "INTRFACE":
        // --------------------------------------------------------------
        // PF17  Interface
        // The User has indicated that no match is possible.
        // Need to add the cash receipt and the set status to 'INTF'.
        // If the imported receipt has a cash receipt number and a
        // cash receipt event system generated number, the receipt
        // has already been added to the database.  JLK  01/16/99
        // --------------------------------------------------------------
        if (export.ReceiptedCashReceipt.SequentialNumber > 0 || export
          .HiddenCashReceiptEvent.SystemGeneratedIdentifier > 0)
        {
          ExitState = "FN0000_IMP_RECEIPT_ALREADY_ADDED";

          return;
        }

        // --------------------------------------------------------------
        // Only receipts with a Cash Receipt Type of MANINT can be
        // added as an interface receipt.  The PF 19 Receipt action
        // must be used to add the receipt as a cash receipt record.
        // --------------------------------------------------------------
        if (export.HiddenCashReceiptType.SystemGeneratedIdentifier != local
          .HardcodedCrtManint.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_CANT_INTERFACE_CHECKS";

          return;
        }

        foreach(var item in ReadCashReceiptEvent2())
        {
          if (ReadCashReceiptStatus1())
          {
            if (entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier ==
              local.HardcodedCrsDeleted.SystemGeneratedIdentifier)
            {
              // -------------------------------------------------------
              // This interface receipt has been deleted.
              // -------------------------------------------------------
              continue;
            }
            else
            {
              ExitState = "FN0000_INTERFACE_AE";

              return;
            }
          }
          else
          {
            ExitState = "FN0000_CASH_RECEIPT_STAT_HIST_NF";

            return;
          }
        }

        if (export.ReceiptedCashReceipt.TotalCashTransactionAmount.
          GetValueOrDefault() == 0)
        {
          // --------------------------------------------------------------
          // Default the Original Interface Amount to the Net
          // Interface Amount.
          // --------------------------------------------------------------
          export.HiddenCashReceiptEvent.TotalCashAmt =
            export.ReceiptedCashReceipt.CashDue.GetValueOrDefault();
          export.ReceiptedCashReceipt.TotalCashTransactionAmount =
            export.ReceiptedCashReceipt.CashDue.GetValueOrDefault();
        }
        else
        {
          // --------------------------------------------------------------
          // The Original Interface Amount must be greater than or equal
          // to the Net Interface Amount.
          // --------------------------------------------------------------
          if (export.ReceiptedCashReceipt.TotalCashTransactionAmount.
            GetValueOrDefault() < export
            .ReceiptedCashReceipt.CashDue.GetValueOrDefault())
          {
            ExitState = "FN0000_INVALID_ORIG_INTF_AMT";

            return;
          }

          export.HiddenCashReceiptEvent.TotalCashAmt =
            export.ReceiptedCashReceipt.TotalCashTransactionAmount.
              GetValueOrDefault();
        }

        UseFnCreateCashReceipt2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCashReceipt3(local.ClearCashReceipt, export.ReceiptedCashReceipt);
          export.Starting.SequentialNumber =
            local.ClearCashReceipt.SequentialNumber;
          export.CheckOrIntf.Date = local.Null1.Date;
          export.HiddenCashReceiptType.SystemGeneratedIdentifier = 0;
          export.HiddenCashReceipt.SequentialNumber =
            local.New1.SequentialNumber;
          local.Previous.Command = "INTRFACE";
          global.Command = "DISPLAY";
        }
        else
        {
          return;
        }

        break;
      case "RECEIPT":
        // --------------------------------------------------------------
        // PF19  Receipt
        // The User has indicated that no match is possible.
        // Need to add the cash receipt and set the status to 'REC'.
        // If the imported receipt has a cash receipt number and a
        // cash receipt event system generated number, the receipt
        // has already been added to the database.  JLK  01/16/99
        // --------------------------------------------------------------
        if (export.ReceiptedCashReceipt.SequentialNumber > 0 || export
          .HiddenCashReceiptEvent.SystemGeneratedIdentifier > 0)
        {
          ExitState = "FN0000_IMP_RECEIPT_ALREADY_ADDED";

          return;
        }

        // --------------------------------------------------------------
        // A receipt with a Cash Receipt Type of MANINT cannot be
        // added as a cash receipt.  The PF 17 Interface action must
        // be used to add the receipt as a manual interface record.
        // --------------------------------------------------------------
        if (export.HiddenCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtManint.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_CANT_RECEIPT_MANINT";

          return;
        }

        UseFnCreateCashReceipt1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.FlowCashReceiptSourceType.SystemGeneratedIdentifier =
            export.ReceiptedCashReceiptSourceType.SystemGeneratedIdentifier;
          export.FlowCashReceiptType.SystemGeneratedIdentifier =
            export.HiddenCashReceiptType.SystemGeneratedIdentifier;
          export.ReceiptedCashReceipt.SequentialNumber =
            local.New1.SequentialNumber;
          global.Command = "DISPLAY";
          ExitState = "ACO_NE0000_RETURN";
        }

        return;
      case "UNMATCH":
        // -------------------------------------------------------------
        // PF20  Unmatch
        // Provides a mechanism to remove the cash receipt information
        // from a matched interface receipt so that the cash receipt
        // can then be rematched to a different interface or receipted
        // into the system.
        // -------------------------------------------------------------
        if (Equal(export.HiddenCashReceiptEvent.SourceCreationDate,
          local.Null1.Date) || export.ReceiptedCashReceipt.ReceiptAmount == 0
          || export.ReceiptedCashReceipt.SequentialNumber == 0 || export
          .HiddenCashReceiptEvent.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_INVALID_RECEIPT_FOR_UNMTC";

          return;
        }

        // -------------------------------------------------------------
        // Set up local view with new values for cash receipt
        // information displayed in the header.
        // -------------------------------------------------------------
        local.UnmatchCashCashReceiptEvent.Assign(local.ClearCashReceiptEvent);
        local.UnmatchCashCashReceiptEvent.ReceivedDate =
          export.ReceiptedCashReceipt.ReceivedDate;
        MoveCashReceipt1(export.ReceiptedCashReceipt,
          local.UnmatchCashCashReceipt);
        local.UnmatchCashCashReceipt.SequentialNumber = 0;
        local.UnmatchCashCashReceipt.ReceiptDate = local.Null1.Date;
        local.UnmatchCashCashReceipt.BalancedTimestamp = local.Null1.Timestamp;
        local.UnmatchCashCashReceipt.DepositReleaseDate = local.Null1.Date;
        local.UnmatchCashCashReceipt.CreatedBy = "";
        local.UnmatchCashCashReceipt.CreatedTimestamp = local.Null1.Timestamp;
        local.UnmatchCashCashReceipt.LastUpdatedBy = "";
        local.UnmatchCashCashReceipt.LastUpdatedTimestamp =
          local.Null1.Timestamp;
        local.UnmatchCashCashReceipt.TotalCashTransactionAmount = 0;
        local.UnmatchCashCashReceipt.TotalCashTransactionCount = 0;
        local.UnmatchCashCashReceipt.TotalCashFeeAmount = 0;
        local.UnmatchCashCashReceipt.CashDue = 0;
        local.UnmatchCashCashReceipt.CashBalanceAmt = 0;
        local.UnmatchCashCashReceipt.CashBalanceReason = "";
        local.UnmatchCashCashReceipt.Note = Spaces(CashReceipt.Note_MaxLength);

        // 06/22/12 GVandy	 CQ33628  Make KSDLUI source type an INTERFUND cash 
        // receipt type for UNMATCH.
        if (Equal(export.ReceiptedCashReceiptSourceType.Code, "SDSO") || Equal
          (export.ReceiptedCashReceiptSourceType.Code, "KSDLUI"))
        {
          local.UnmatchCashCashReceiptType.SystemGeneratedIdentifier =
            local.HardcodedCrtInterfund.SystemGeneratedIdentifier;
        }
        else if (Equal(export.ReceiptedCashReceiptSourceType.Code, "FDSO"))
        {
          local.UnmatchCashCashReceiptType.SystemGeneratedIdentifier =
            local.HardcodedCrtEft.SystemGeneratedIdentifier;
        }
        else
        {
          local.UnmatchCashCashReceiptType.SystemGeneratedIdentifier =
            local.HardcodedCrtCheck.SystemGeneratedIdentifier;
        }

        // -------------------------------------------------------------
        // Set up local view with new values for interface receipt.
        // -------------------------------------------------------------
        MoveCashReceipt2(export.ReceiptedCashReceipt, local.UnmatchInterface);
        local.UnmatchInterface.ReceiptDate =
          Date(export.ReceiptedCashReceipt.CreatedTimestamp);
        local.UnmatchInterface.ReceivedDate =
          export.HiddenCashReceiptEvent.ReceivedDate;
        local.UnmatchInterface.ReceiptAmount = 0;
        local.UnmatchInterface.CheckDate = local.Null1.Date;
        local.UnmatchInterface.CheckNumber = "";
        local.UnmatchInterface.BalancedTimestamp = local.Null1.Timestamp;
        local.UnmatchInterface.DepositReleaseDate = local.Null1.Date;
        local.UnmatchInterface.PayorLastName = "";
        local.UnmatchInterface.PayorFirstName = "";
        local.UnmatchInterface.PayorMiddleName = "";
        local.UnmatchInterface.PayorOrganization = "";
        local.UnmatchInterface.Note = Spaces(CashReceipt.Note_MaxLength);
        local.UnmatchInterface.CashBalanceAmt =
          local.UnmatchInterface.CashDue.GetValueOrDefault() - local
          .UnmatchInterface.ReceiptAmount;

        if (local.UnmatchInterface.CashBalanceAmt.GetValueOrDefault() > 0)
        {
          local.UnmatchInterface.CashBalanceReason = "UNDER";
        }
        else if (local.UnmatchInterface.CashBalanceAmt.GetValueOrDefault() < 0)
        {
          local.UnmatchInterface.CashBalanceReason = "OVER";
        }
        else
        {
          local.UnmatchInterface.CashBalanceReason = "";
        }

        UseFnUnmatchInterfaceReceipt();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.FlowCashReceiptSourceType.SystemGeneratedIdentifier =
            export.ReceiptedCashReceiptSourceType.SystemGeneratedIdentifier;
          export.FlowCashReceiptEvent.Assign(local.UnmatchCashCashReceiptEvent);
          export.FlowCashReceiptType.SystemGeneratedIdentifier =
            local.UnmatchCashCashReceiptType.SystemGeneratedIdentifier;
          export.ReceiptedCashReceipt.Assign(local.UnmatchCashCashReceipt);
        }
        else
        {
          return;
        }

        export.FlowLastAction.Command = "UNMATCH";
        global.Command = "";
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "PREV":
        // ----------------------------------------------------
        // PF7  Prev
        // ----------------------------------------------------
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        return;
      case "NEXT":
        // ----------------------------------------------------
        // PF8  Next
        // ----------------------------------------------------
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        return;
      case "RETURN":
        // ----------------------------------------------------
        // PF9  Return
        // ----------------------------------------------------
        global.Command = "DISPLAY";
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        // ----------------------------------------------------
        // PF12  Signoff
        // ----------------------------------------------------
        UseScCabSignoff();

        return;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadCashReceiptStatus3())
      {
        // OK
      }
      else
      {
        ExitState = "FN0108_CASH_RCPT_STAT_NF";

        return;
      }

      if (ReadCashReceiptSourceType())
      {
        MoveCashReceiptSourceType(entities.EavCashReceiptSourceType,
          export.ReceiptedCashReceiptSourceType);
      }
      else
      {
        ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";

        return;
      }

      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceipt())
      {
        // -------------------------------------------------------------------------
        // If the cash due is less or equal to zero, the court will not be
        // providing a check to SRS.  Skip to the next receipt.
        // JLK  05/26/99
        // -------------------------------------------------------------------------
        if (!Lt(0, entities.EavCashReceipt.CashDue))
        {
          export.Export1.Next();

          continue;
        }

        if (ReadCashReceiptType())
        {
          if (AsChar(entities.EavCashReceiptType.CategoryIndicator) == 'C')
          {
            if (ReadCashReceiptEvent1())
            {
              // --->  Continue
            }
            else
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.CashReceiptEvent.Assign(
              entities.EavCashReceiptEvent);
            export.Export1.Update.CashReceipt.Assign(entities.EavCashReceipt);
            export.Export1.Update.HiddenCashReceiptType.
              SystemGeneratedIdentifier =
                entities.EavCashReceiptType.SystemGeneratedIdentifier;
            export.Export1.Update.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier =
                entities.EavCashReceiptSourceType.SystemGeneratedIdentifier;
            export.Export1.Update.IntfAdjAmt.TotalCurrency =
              export.Export1.Item.CashReceipt.CashDue.GetValueOrDefault() - export
              .Export1.Item.CashReceipt.TotalCashTransactionAmount.
                GetValueOrDefault();
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        export.Export1.Next();
      }

      switch(TrimEnd(local.Previous.Command))
      {
        case "INTRFACE":
          ExitState = "FN0000_INTF_RCPT_CREATED_OK";

          break;
        case "UNMATCH":
          ExitState = "FN0000_RECEIPT_UNMATCHED_OK";

          break;
        default:
          if (export.Export1.IsEmpty)
          {
            var field = GetField(export.ReceiptedCashReceiptSourceType, "code");

            field.Error = true;

            ExitState = "NO_INTERFACES_TO_MATCH";
          }
          else if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          break;
      }
    }
  }

  private static void MoveCashReceipt1(CashReceipt source, CashReceipt target)
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
    target.CashDue = source.CashDue;
    target.CashBalanceAmt = source.CashBalanceAmt;
    target.CashBalanceReason = source.CashBalanceReason;
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
    target.CashDue = source.CashDue;
    target.CashBalanceAmt = source.CashBalanceAmt;
    target.CashBalanceReason = source.CashBalanceReason;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCreateCashReceipt1()
  {
    var useImport = new FnCreateCashReceipt.Import();
    var useExport = new FnCreateCashReceipt.Export();

    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      local.HardcodedCrsReceipted.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.ReceiptedCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.Assign(import.HiddenCashReceiptEvent);
    MoveCashReceipt1(export.ReceiptedCashReceipt, useImport.CashReceipt);

    Call(FnCreateCashReceipt.Execute, useImport, useExport);

    local.New1.SequentialNumber = useExport.CashReceipt.SequentialNumber;
    export.FlowCashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
  }

  private void UseFnCreateCashReceipt2()
  {
    var useImport = new FnCreateCashReceipt.Import();
    var useExport = new FnCreateCashReceipt.Export();

    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      local.HardcodedCrsInterfaced.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.ReceiptedCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.Assign(export.HiddenCashReceiptEvent);
    MoveCashReceipt1(export.ReceiptedCashReceipt, useImport.CashReceipt);

    Call(FnCreateCashReceipt.Execute, useImport, useExport);

    local.New1.SequentialNumber = useExport.CashReceipt.SequentialNumber;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsInterfaced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
    local.HardcodedCrsReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrtCheck.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdCheck.SystemGeneratedIdentifier;
  }

  private void UseFnMatchInterfaceCashReceipt()
  {
    var useImport = new FnMatchInterfaceCashReceipt.Import();
    var useExport = new FnMatchInterfaceCashReceipt.Export();

    useImport.InterfaceCashReceiptSourceType.SystemGeneratedIdentifier =
      export.FlowCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.InterfaceCashReceiptEvent.SystemGeneratedIdentifier =
      export.FlowCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.InterfaceCashReceiptType.SystemGeneratedIdentifier =
      export.FlowCashReceiptType.SystemGeneratedIdentifier;
    useImport.ReceiptedCashReceipt.Assign(export.ReceiptedCashReceipt);
    useImport.ReceiptedCashReceiptStatus.SystemGeneratedIdentifier =
      local.CashReceiptStatus.SystemGeneratedIdentifier;

    Call(FnMatchInterfaceCashReceipt.Execute, useImport, useExport);

    MoveCashReceipt3(useExport.Matched, export.ReceiptedCashReceipt);
  }

  private void UseFnUnmatchInterfaceReceipt()
  {
    var useImport = new FnUnmatchInterfaceReceipt.Import();
    var useExport = new FnUnmatchInterfaceReceipt.Export();

    useImport.CashReceipt.Assign(local.UnmatchInterface);

    Call(FnUnmatchInterfaceReceipt.Execute, useImport, useExport);
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

  private IEnumerable<bool> ReadCashReceipt()
  {
    return ReadEach("ReadCashReceipt",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crsIdentifier",
          entities.EavCashReceiptStatus.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.EavCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(command, "cashReceiptId", import.Starting.SequentialNumber);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.EavCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.EavCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.EavCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.EavCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.EavCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.EavCashReceipt.CashDue = db.GetNullableDecimal(reader, 5);
        entities.EavCashReceipt.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptEvent1()
  {
    System.Diagnostics.Debug.Assert(entities.EavCashReceipt.Populated);
    entities.EavCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent1",
      (db, command) =>
      {
        db.
          SetInt32(command, "creventId", entities.EavCashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.EavCashReceipt.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.EavCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.EavCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.EavCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.EavCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.EavCashReceiptEvent.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.EavCashReceiptEvent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptEvent2()
  {
    entities.EavCashReceiptEvent.Populated = false;

    return ReadEach("ReadCashReceiptEvent2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          export.ReceiptedCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "receivedDate",
          export.HiddenCashReceiptEvent.ReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EavCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.EavCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.EavCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.EavCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.EavCashReceiptEvent.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.EavCashReceiptEvent.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.EavCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetString(command, "code", import.ReceiptedCashReceiptSourceType.Code);
          
      },
      (db, reader) =>
      {
        entities.EavCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.EavCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.EavCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus1()
  {
    System.Diagnostics.Debug.Assert(entities.EavCashReceiptEvent.Populated);
    entities.ExistingCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crvIdentifier",
          entities.EavCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.EavCashReceiptEvent.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus2()
  {
    entities.EavCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "cashReceiptId",
          import.ReceiptedCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.EavCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.EavCashReceiptStatus.Code = db.GetString(reader, 1);
        entities.EavCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus3()
  {
    entities.EavCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsInterfaced.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.EavCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.EavCashReceiptStatus.Code = db.GetString(reader, 1);
        entities.EavCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.EavCashReceipt.Populated);
    entities.EavCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.EavCashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.EavCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.EavCashReceiptType.CategoryIndicator = db.GetString(reader, 1);
        entities.EavCashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.EavCashReceiptType.CategoryIndicator);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
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
      /// A value of CashReceiptEvent.
      /// </summary>
      [JsonPropertyName("cashReceiptEvent")]
      public CashReceiptEvent CashReceiptEvent
      {
        get => cashReceiptEvent ??= new();
        set => cashReceiptEvent = value;
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
      /// A value of IntfAdjAmt.
      /// </summary>
      [JsonPropertyName("intfAdjAmt")]
      public Common IntfAdjAmt
      {
        get => intfAdjAmt ??= new();
        set => intfAdjAmt = value;
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
      /// A value of HiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptType")]
      public CashReceiptType HiddenCashReceiptType
      {
        get => hiddenCashReceiptType ??= new();
        set => hiddenCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private CashReceiptEvent cashReceiptEvent;
      private CashReceipt cashReceipt;
      private Common intfAdjAmt;
      private CashReceiptSourceType hiddenCashReceiptSourceType;
      private CashReceiptType hiddenCashReceiptType;
    }

    /// <summary>
    /// A value of ReceiptedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("receiptedCashReceiptSourceType")]
    public CashReceiptSourceType ReceiptedCashReceiptSourceType
    {
      get => receiptedCashReceiptSourceType ??= new();
      set => receiptedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ReceiptedCashReceipt.
    /// </summary>
    [JsonPropertyName("receiptedCashReceipt")]
    public CashReceipt ReceiptedCashReceipt
    {
      get => receiptedCashReceipt ??= new();
      set => receiptedCashReceipt = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceipt Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of CheckOrIntf.
    /// </summary>
    [JsonPropertyName("checkOrIntf")]
    public DateWorkArea CheckOrIntf
    {
      get => checkOrIntf ??= new();
      set => checkOrIntf = value;
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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

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

    private CashReceiptSourceType receiptedCashReceiptSourceType;
    private CashReceipt receiptedCashReceipt;
    private CashReceipt starting;
    private DateWorkArea checkOrIntf;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceipt hiddenCashReceipt;
    private Array<ImportGroup> import1;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
      }

      /// <summary>
      /// A value of IntfAdjAmt.
      /// </summary>
      [JsonPropertyName("intfAdjAmt")]
      public Common IntfAdjAmt
      {
        get => intfAdjAmt ??= new();
        set => intfAdjAmt = value;
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
      /// A value of HiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptType")]
      public CashReceiptType HiddenCashReceiptType
      {
        get => hiddenCashReceiptType ??= new();
        set => hiddenCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private CashReceiptEvent cashReceiptEvent;
      private CashReceipt cashReceipt;
      private Common intfAdjAmt;
      private CashReceiptSourceType hiddenCashReceiptSourceType;
      private CashReceiptType hiddenCashReceiptType;
    }

    /// <summary>
    /// A value of ReceiptedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("receiptedCashReceiptSourceType")]
    public CashReceiptSourceType ReceiptedCashReceiptSourceType
    {
      get => receiptedCashReceiptSourceType ??= new();
      set => receiptedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ReceiptedCashReceipt.
    /// </summary>
    [JsonPropertyName("receiptedCashReceipt")]
    public CashReceipt ReceiptedCashReceipt
    {
      get => receiptedCashReceipt ??= new();
      set => receiptedCashReceipt = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceipt Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of CheckOrIntf.
    /// </summary>
    [JsonPropertyName("checkOrIntf")]
    public DateWorkArea CheckOrIntf
    {
      get => checkOrIntf ??= new();
      set => checkOrIntf = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of FlowCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("flowCashReceiptSourceType")]
    public CashReceiptSourceType FlowCashReceiptSourceType
    {
      get => flowCashReceiptSourceType ??= new();
      set => flowCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of FlowCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("flowCashReceiptEvent")]
    public CashReceiptEvent FlowCashReceiptEvent
    {
      get => flowCashReceiptEvent ??= new();
      set => flowCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of FlowCashReceiptType.
    /// </summary>
    [JsonPropertyName("flowCashReceiptType")]
    public CashReceiptType FlowCashReceiptType
    {
      get => flowCashReceiptType ??= new();
      set => flowCashReceiptType = value;
    }

    /// <summary>
    /// A value of FlowLastAction.
    /// </summary>
    [JsonPropertyName("flowLastAction")]
    public Common FlowLastAction
    {
      get => flowLastAction ??= new();
      set => flowLastAction = value;
    }

    private CashReceiptSourceType receiptedCashReceiptSourceType;
    private CashReceipt receiptedCashReceipt;
    private CashReceipt starting;
    private DateWorkArea checkOrIntf;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceipt hiddenCashReceipt;
    private Array<ExportGroup> export1;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CashReceiptSourceType flowCashReceiptSourceType;
    private CashReceiptEvent flowCashReceiptEvent;
    private CashReceiptType flowCashReceiptType;
    private Common flowLastAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of SelectionCount.
    /// </summary>
    [JsonPropertyName("selectionCount")]
    public Common SelectionCount
    {
      get => selectionCount ??= new();
      set => selectionCount = value;
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
    /// A value of ClearCashReceipt.
    /// </summary>
    [JsonPropertyName("clearCashReceipt")]
    public CashReceipt ClearCashReceipt
    {
      get => clearCashReceipt ??= new();
      set => clearCashReceipt = value;
    }

    /// <summary>
    /// A value of UnmatchCashCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("unmatchCashCashReceiptEvent")]
    public CashReceiptEvent UnmatchCashCashReceiptEvent
    {
      get => unmatchCashCashReceiptEvent ??= new();
      set => unmatchCashCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of UnmatchCashCashReceiptType.
    /// </summary>
    [JsonPropertyName("unmatchCashCashReceiptType")]
    public CashReceiptType UnmatchCashCashReceiptType
    {
      get => unmatchCashCashReceiptType ??= new();
      set => unmatchCashCashReceiptType = value;
    }

    /// <summary>
    /// A value of UnmatchCashCashReceipt.
    /// </summary>
    [JsonPropertyName("unmatchCashCashReceipt")]
    public CashReceipt UnmatchCashCashReceipt
    {
      get => unmatchCashCashReceipt ??= new();
      set => unmatchCashCashReceipt = value;
    }

    /// <summary>
    /// A value of UnmatchInterface.
    /// </summary>
    [JsonPropertyName("unmatchInterface")]
    public CashReceipt UnmatchInterface
    {
      get => unmatchInterface ??= new();
      set => unmatchInterface = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceipt New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of HardcodedCrsInterfaced.
    /// </summary>
    [JsonPropertyName("hardcodedCrsInterfaced")]
    public CashReceiptStatus HardcodedCrsInterfaced
    {
      get => hardcodedCrsInterfaced ??= new();
      set => hardcodedCrsInterfaced = value;
    }

    /// <summary>
    /// A value of HardcodedCrsReceipted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsReceipted")]
    public CashReceiptStatus HardcodedCrsReceipted
    {
      get => hardcodedCrsReceipted ??= new();
      set => hardcodedCrsReceipted = value;
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
    /// A value of HardcodedCrtManint.
    /// </summary>
    [JsonPropertyName("hardcodedCrtManint")]
    public CashReceiptType HardcodedCrtManint
    {
      get => hardcodedCrtManint ??= new();
      set => hardcodedCrtManint = value;
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
    /// A value of HardcodedCrtInterfund.
    /// </summary>
    [JsonPropertyName("hardcodedCrtInterfund")]
    public CashReceiptType HardcodedCrtInterfund
    {
      get => hardcodedCrtInterfund ??= new();
      set => hardcodedCrtInterfund = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Common selectionCount;
    private CashReceiptEvent clearCashReceiptEvent;
    private CashReceipt clearCashReceipt;
    private CashReceiptEvent unmatchCashCashReceiptEvent;
    private CashReceiptType unmatchCashCashReceiptType;
    private CashReceipt unmatchCashCashReceipt;
    private CashReceipt unmatchInterface;
    private CashReceipt new1;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatus hardcodedCrsInterfaced;
    private CashReceiptStatus hardcodedCrsReceipted;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptType hardcodedCrtManint;
    private CashReceiptType hardcodedCrtCheck;
    private CashReceiptType hardcodedCrtInterfund;
    private CashReceiptType hardcodedCrtEft;
    private Common previous;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EavCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("eavCashReceiptSourceType")]
    public CashReceiptSourceType EavCashReceiptSourceType
    {
      get => eavCashReceiptSourceType ??= new();
      set => eavCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of EavCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("eavCashReceiptEvent")]
    public CashReceiptEvent EavCashReceiptEvent
    {
      get => eavCashReceiptEvent ??= new();
      set => eavCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of EavCashReceiptType.
    /// </summary>
    [JsonPropertyName("eavCashReceiptType")]
    public CashReceiptType EavCashReceiptType
    {
      get => eavCashReceiptType ??= new();
      set => eavCashReceiptType = value;
    }

    /// <summary>
    /// A value of EavCashReceipt.
    /// </summary>
    [JsonPropertyName("eavCashReceipt")]
    public CashReceipt EavCashReceipt
    {
      get => eavCashReceipt ??= new();
      set => eavCashReceipt = value;
    }

    /// <summary>
    /// A value of EavCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("eavCashReceiptStatusHistory")]
    public CashReceiptStatusHistory EavCashReceiptStatusHistory
    {
      get => eavCashReceiptStatusHistory ??= new();
      set => eavCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of EavCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("eavCashReceiptStatus")]
    public CashReceiptStatus EavCashReceiptStatus
    {
      get => eavCashReceiptStatus ??= new();
      set => eavCashReceiptStatus = value;
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
    /// A value of ExistingCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ExistingCashReceiptStatusHistory
    {
      get => existingCashReceiptStatusHistory ??= new();
      set => existingCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatus")]
    public CashReceiptStatus ExistingCashReceiptStatus
    {
      get => existingCashReceiptStatus ??= new();
      set => existingCashReceiptStatus = value;
    }

    private CashReceiptSourceType eavCashReceiptSourceType;
    private CashReceiptEvent eavCashReceiptEvent;
    private CashReceiptType eavCashReceiptType;
    private CashReceipt eavCashReceipt;
    private CashReceiptStatusHistory eavCashReceiptStatusHistory;
    private CashReceiptStatus eavCashReceiptStatus;
    private CashReceipt existingCashReceipt;
    private CashReceiptStatusHistory existingCashReceiptStatusHistory;
    private CashReceiptStatus existingCashReceiptStatus;
  }
#endregion
}

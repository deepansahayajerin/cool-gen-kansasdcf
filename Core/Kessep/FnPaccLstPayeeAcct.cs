// Program: FN_PACC_LST_PAYEE_ACCT, ID: 372519786, model: 746.
// Short name: SWEPACCP
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
/// A program: FN_PACC_LST_PAYEE_ACCT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPaccLstPayeeAcct: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PACC_LST_PAYEE_ACCT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPaccLstPayeeAcct(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPaccLstPayeeAcct.
  /// </summary>
  public FnPaccLstPayeeAcct(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************
    // Procedure    : List Payee Account
    // Screen Id    : PACC
    // Developed by : R.B.Mohapatra
    // Start Date   : 08-31-95
    // Change Log :
    // -----------
    // 1.  02/20/1996  Change in views to establish compatibility with linked 
    // procedures
    // 2.  02/22/1996  Change to processing logic due to change in the DATA-SENT
    // and DISPLAY property of the Dialog Flow
    // 3.  03/11/1996  Set up Dialog Flow accross Warrant Screens
    // 4.  03/13/1996  Retrofitting Security/NEXTTRAN info
    // R. Marchman
    // 5.  12/11/96	Add new security and next tran
    // G.Lofton - MTW
    // 6.  02/09/97	Required fixes
    // 7.  03/03/97    Fixed flow to NAME.
    // Siraj Konkader
    // 8.  03/10/1997  Changed flow to PAYR (from OSUM).
    // Sumanta - MTW  04/21/1997
    //  - allowed the flow to CRRC and PAYR if a collection is selected.
    // Alan Samuels (MTW) 5/6/97
    // -- Removed Print Date and changed content of Reference Number to be 
    // Receipt date, Receipt Number, and Collection Number.
    // Siraj Konkader
    // 7/31/97     Made Debit/Credit fields signed.
    // Newman/Parker-DIR
    // 09/09/97    Added logic to test for overflow and provide exitstate.  
    // Changed max from 30 to 100.
    // Siraj Konkader
    // 09/16/97    When Reference # was changed on the screen, code was not 
    // changed accordingly - wherever it was used for processing.
    // 09/19/97    When logic to test for overflow was added, no consideration 
    // was given to displaying all that was read so far. This is because the
    // move to export was done when summarizing the local views used to collect
    // the data.
    // Adwait Phadnis
    // 12/12/97     Fixed scrolling problems and flow to CRRC for type DISB not 
    // allowed.
    // Adwait Phadnis
    // 02/06/98     Added the adjustment switch to negative credit on 
    // collection.
    // Naveen Engoor
    // 11/01/1998  Fixed the flow problems to DHST.
    // *******************************************************
    // ********************************************************
    // Kalpesh Doshi
    // 01/28/99  Rename views
    // 01/29/99  Make PF15 and PF16 work.
    // - Various phase2 changes! See design doc for complete list.
    // 07/28/99  Add obligee qualification in READ to use index.
    // *******************************************************
    // ********************************************************
    // Kalpesh Doshi
    // 11/16/99 PR#80019
    // Change sort on READ EACH from collection_date Asc to Desc.
    // PF15 - will now display data for previous month ONLY.
    // PF16 - will now display data for next month ONLY.
    // 11/18/99 PR#80496
    // Use CRE.Received_date instead of CR.Receipt_date.
    // 12/20/99 PR#82498
    // Expand group view size from 100 to 120. Increasing it any
    // further will hit the 31K compile limit.
    // 1/4/2000 PR#82370
    // Add new flow to OREL.
    // 2/3/2000 PR#84875
    // Display processing date instead of created_ts
    // 3/8/2000 PR#90072
    // Use process_dte instead of create_dte to net COLL and URA records.
    // 5/13/2000 PRWORA Work Order # 164
    // Flag URA disbursements with an 'X' in the Disb Type field.
    // 4/11/2001 SWSRKXD PR# 117667
    // Inflate local GVs. Display GV overflow message.
    // *******************************************************
    // 12/13/00 swsrchf  000238   Return super "NEXT TRAN" to ALRT
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *** MOVE Imports to Exports ***
    export.CsePerson.Number = import.CsePerson.Number;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.FlowPaymentRequest.CsePersonNumber = import.CsePerson.Number;
    export.CsePersonsWorkSet.FormattedName =
      import.CsePersonsWorkSet.FormattedName;
    export.HiddenShowAdj.Flag = import.ShowAdj.Flag;
    export.Prompt.SelectChar = import.Prompt.SelectChar;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.ShowAdj.Flag = import.ShowAdj.Flag;
    export.HiddenShowAdj.Flag = import.HiddenShowAdj.Flag;
    export.HiddenStartingDateIn.Date = import.HiddenStartingDate.Date;
    export.HiddenEndingDate.Date = import.HiddenEndingDate.Date;

    if (IsEmpty(export.ShowAdj.Flag))
    {
      export.ShowAdj.Flag = "Y";
      export.HiddenShowAdj.Flag = "Y";
    }

    if (import.StartingDate.Date == null)
    {
      local.DateWorkArea.Date = Now().Date.AddMonths(-1);
      UseCabFirstAndLastDateOfMonth2();
      export.HiddenStartingDateIn.Date = export.StartingDate.Date;
    }
    else
    {
      export.StartingDate.Date = import.StartingDate.Date;
      export.HiddenStartingDateIn.Date = import.HiddenStartingDate.Date;
    }

    local.CurrentDate.Date = Now().Date;

    if (import.EndingDate.Date == null)
    {
      export.EndingDate.Date = Now().Date;
      export.HiddenEndingDate.Date = export.EndingDate.Date;
    }
    else
    {
      export.EndingDate.Date = import.EndingDate.Date;
      export.HiddenEndingDate.Date = import.HiddenEndingDate.Date;
    }

    if (Equal(global.Command, "PREV_MTH"))
    {
      // *** Compute the FIRST DAY OF THE PREVIOUS MONTH ***
      // *** and assign this value to STARTING DATE      ***
      local.DateWorkArea.Date = AddMonths(export.StartingDate.Date, -1);
      UseCabFirstAndLastDateOfMonth1();
      global.Command = "DISPLAY";
    }
    else if (Equal(global.Command, "NEXT_MTH"))
    {
      // *** Compute the FIRST DAY OF THE NEXT MONTH ***
      // *** and assign this value to STARTING DATE  ***
      local.DateWorkArea.Date = AddMonths(export.StartingDate.Date, 1);
      UseCabFirstAndLastDateOfMonth1();

      if (Lt(Now().Date, export.StartingDate.Date))
      {
        ExitState = "FN0000_NEXT_MTH_NOT_ALLOWED";

        return;
      }

      if (Lt(Now().Date, export.EndingDate.Date))
      {
        export.EndingDate.Date = Now().Date;
      }

      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "DISPLAY") || Equal
      (export.CsePerson.Number, export.HiddenCsePerson.Number) && Equal
      (export.StartingDate.Date, export.HiddenStartingDateIn.Date) && Equal
      (export.EndingDate.Date, export.HiddenEndingDate.Date) && AsChar
      (export.ShowAdj.Flag) == AsChar(export.HiddenShowAdj.Flag))
    {
      local.Select.Count = 0;

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        if (Equal(global.Command, "RTFRMLNK"))
        {
        }
        else
        {
          export.Group.Update.Common.Flag = import.Group.Item.Common.Flag;
        }

        export.Group.Update.LocalFinanceWorkArea.DisbDate =
          import.Group.Item.LocalFinanceWorkArea.DisbDate;
        export.Group.Update.DisbursementTransactionType.Code =
          import.Group.Item.DisbursementTransactionType.Code;
        export.Group.Update.DisbType.Text10 = import.Group.Item.DisbType.Text10;
        export.Group.Update.PaymentRequest.Assign(
          import.Group.Item.PaymentRequest);
        export.Group.Update.Db.Amount = import.Group.Item.Db.Amount;
        export.Group.Update.Cr.Amount = import.Group.Item.Cr.Amount;
        MoveDisbursementTransaction1(import.Group.Item.DisbursementTransaction,
          export.Group.Update.DisbursementTransaction);
        export.Group.Update.CashReceipt.ReceivedDate =
          import.Group.Item.CashReceipt.ReceivedDate;
        export.Group.Update.RefNo.CrdCrCombo =
          import.Group.Item.RefNo.CrdCrCombo;

        // ********************************************************
        // 1/4/2000 PR#82370
        // Add new flow to OREL.
        // *******************************************************
        if (Equal(global.Command, "CRRC") || Equal(global.Command, "PAYR") || Equal
          (global.Command, "WDTL") || Equal(global.Command, "DHST") || Equal
          (global.Command, "EDTL") || Equal(global.Command, "OREL"))
        {
          if (!IsEmpty(import.Group.Item.Common.Flag))
          {
            local.Select.Flag = import.Group.Item.Common.Flag;
            local.Select.Count = (int)((long)local.Select.Count + 1);
            MovePaymentRequest(import.Group.Item.PaymentRequest,
              export.FlowPaymentRequest);

            // -----------------------------
            // Naveen - 11/02/1998
            // Add two set stmnts for passing the Disbursement date and the 
            // amount to DHST.
            // ----------------------------
            local.SendDhst.DisbDate =
              import.Group.Item.LocalFinanceWorkArea.DisbDate;
            local.SendDhst.DisbAmount = import.Group.Item.Db.Amount;
            MoveDisbursementTransaction1(import.Group.Item.
              DisbursementTransaction, local.PrevTempDisbursementTransaction);
            local.PrevTempDisbursementTransactionType.Code =
              import.Group.Item.DisbursementTransactionType.Code;
          }
        }

        export.Group.Next();
      }
    }

    if (Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.Selected.Number))
      {
        export.CsePerson.Number = import.Selected.Number;
        export.CsePersonsWorkSet.Number = import.Selected.Number;
        global.Command = "DISPLAY";

        // ----------------------
        // KD - 2/5/99
        // Place cursor on next enterable field
        // ---------------------
        var field = GetField(export.StartingDate, "date");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;
      }
      else
      {
        return;
      }
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumber = import.CsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumberObligee =
        import.CsePerson.Number;
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

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligee))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberObligee ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // -------------------------------------------------
    // KD - 5/20/99
    // When an invalid CSE person number is entered, display
    // error message.
    // 1/4/2000 PR#82370
    // Add new flow to OREL.
    // ------------------------------------------------
    if (Equal(global.Command, "CRRC") || Equal(global.Command, "DHST") || Equal
      (global.Command, "WDTL") || Equal(global.Command, "PAYR") || Equal
      (global.Command, "EDTL") || Equal(global.Command, "PSUM") || Equal
      (global.Command, "OREL"))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();

      if (!ReadCsePerson1())
      {
        ExitState = "FN0000_ENTER_VALID_CSE_NUMBER";

        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        return;
      }
    }
    else if (Equal(global.Command, "PREV_MTH") || Equal
      (global.Command, "NEXT_MTH") || Equal(global.Command, "RTFRMLNK"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseCabSetMaximumDiscontinueDate1();

    // ****** MAIN CASE_OF_COMMAND STRUCTURE ******
    switch(TrimEnd(global.Command))
    {
      case "RTFRMLNK":
        // Returning from PAYR
        break;
      case "DISPLAY":
        if (IsEmpty(export.CsePerson.Number))
        {
          // -------------------------------------------------
          // KD - 1/29/99
          // Replace 'invalid entry' exit state with 'mandatory fields reqd'
          // ------------------------------------------------
          ExitState = "SP0000_MANDATORY_FIELD_NOT_ENTRD";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        local.TextWorkArea.Text10 = export.CsePerson.Number;
        UseEabPadLeftWithZeros();
        export.CsePerson.Number = local.TextWorkArea.Text10;

        // -----------------------------------------------------
        // KD - 6/11/99
        // Disallow 'State of Kansas' payee to be displayed. Call
        // hardcoded CAB to retrieve Payee # for State.
        // ------------------------------------------------
        if (Equal(export.CsePerson.Number, "000000017O"))
        {
          ExitState = "FN0000_STATE_ACCT_NOT_ACCESSIBLE";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson2();
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -------------------------------------------------
          // KD - 1/29/99
          // Highlight cse_person field when in error.
          // ------------------------------------------------
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        if (Equal(export.CsePerson.Number, import.HiddenCsePerson.Number) && Equal
          (export.StartingDate.Date, import.HiddenStartingDate.Date) && Equal
          (export.EndingDate.Date, export.HiddenEndingDate.Date) && AsChar
          (export.ShowAdj.Flag) == AsChar(export.HiddenShowAdj.Flag))
        {
          // *** Since the Input parameters have not changed, redisplay the 
          // Previous output ***
          ExitState = "FN0000_DATA_NOT_CHANGED_REDSPLY";

          return;
        }

        // -------------------------------------------------
        // KD - 2/9/99
        // Ensure End Date is not before Start Date
        // ------------------------------------------------
        if (Lt(export.EndingDate.Date, export.StartingDate.Date))
        {
          var field1 = GetField(export.EndingDate, "date");

          field1.Error = true;

          var field2 = GetField(export.StartingDate, "date");

          field2.Error = true;

          ExitState = "START_DATE_GREATER_THAN_END_DATE";

          return;
        }

        // -------------------------------------------------
        // KD - 2/9/99
        // Ensure End Date is not a Future Date
        // ------------------------------------------------
        if (Lt(Now().Date, export.EndingDate.Date))
        {
          var field = GetField(export.EndingDate, "date");

          field.Error = true;

          ExitState = "END_DT_GREATER_THAN_CURRENT_DT";

          return;
        }

        export.HiddenCsePerson.Number = export.CsePerson.Number;
        export.HiddenShowAdj.Flag = export.ShowAdj.Flag;
        export.HiddenStartingDateIn.Date = export.StartingDate.Date;
        export.HiddenEndingDate.Date = export.EndingDate.Date;

        // *** USE SI_READ_CSE_PERSON to get the CSE_PERSON FORMATTED NAME and 
        // Move it to the Screen ***
        local.CsePersonsWorkSet.Number = export.CsePerson.Number;

        if (IsEmpty(import.Received.FormattedName))
        {
          UseSiReadCsePerson1();
        }
        else
        {
          export.CsePersonsWorkSet.FormattedName =
            import.Received.FormattedName;
        }

        if (!ReadCsePerson2())
        {
          ExitState = "CSE_PERSON_NF";

          return;
        }

        if (!ReadObligee())
        {
          ExitState = "FN0000_OBLIGEE_PACC_NF";

          return;
        }

        local.Coll.Index = -1;

        // -----------------------------------------------------------
        // KD - 3/22/99
        // Add created_timestamp as level 3 sort
        // -----------------------------------------------------------
        // -----------------------------------------------------------
        // KD - 11/16/99 , PR#80019
        // Change sort on collection_date from asc to desc.
        // KD - 3/8/2000 PR#90072
        // Replace sort on created_ts with process_date. COLL and
        // URA records are rolled up by process_date instead of
        // create_date.
        // -----------------------------------------------------------
        foreach(var item in ReadDisbursementTransaction())
        {
          local.DisbTranFoundFlagIn.Flag = "Y";

          // *** CREDIT SIDE SPECIFICATION ***
          // -----------------------------------------------------------
          // KD - 6/1/99
          // Do not display Credit info for URA and Passthru
          // -----------------------------------------------------------
          if (AsChar(entities.Cr.Type1) == 'C')
          {
            // -----------------------------------------------------------
            // KD - 3/22/99
            // Ensure array is not full
            // -----------------------------------------------------------
            if (local.Coll.Index + 1 >= Local.CollGroup.Capacity)
            {
              local.GvOverflow.Flag = "Y";

              break;
            }

            ++local.Coll.Index;
            local.Coll.CheckSize();

            local.Coll.Update.CollLocalFinanceWorkArea.DisbDate =
              entities.Cr.CollectionDate;
            local.Coll.Update.CollGrpCr.Amount = entities.Cr.Amount;
            MoveDisbursementTransaction2(entities.Cr,
              local.Coll.Update.CollDisbursementTransaction);
            local.Coll.Update.CollDisbursementTransactionType.Code = "COLL";

            // ********************************************************
            // Kalpesh Doshi
            // 11/18/99 PR#80496
            // Use CRE.Received_date instead of CR.Receipt_date.
            // NB - SMEs requested CR.Received_date first and then they
            // changed it to CRE.Received_date. TO avoid any further view
            // changes to this group view (not to mention the screen
            // changes), CR.Received_date attr view is being used.
            // *******************************************************
            if (!ReadCashReceiptEvent())
            {
              ExitState = "CASH_RECEIPT_DETAIL_NF";

              return;
            }

            local.Coll.Update.CollCashReceipt.ReceivedDate =
              entities.CashReceiptEvent.ReceivedDate;
            local.Coll.Update.CollGrpRefNbr.CrdCrCombo =
              entities.Cr.ReferenceNumber ?? Spaces(14);
          }

          // *** DEBIT side Specification ***
          // --------------------------------------------------
          // KD - 3/17/99
          // Fuse the reads for disb_trn_rln and disbursement.
          // KD - 5/27/2000
          // Add sort on Excess_URA_ind
          // --------------------------------------------------
          foreach(var item1 in ReadDisbursementTransactionRlnDisbursement())
          {
            if (local.Coll.Index + 1 >= Local.CollGroup.Capacity)
            {
              local.GvOverflow.Flag = "Y";

              goto ReadEach;
            }

            ++local.Coll.Index;
            local.Coll.CheckSize();

            local.Coll.Update.CollGrpDb.Amount = entities.Disbursement.Amount;

            switch(AsChar(entities.Cr.Type1))
            {
              case 'C':
                local.Coll.Update.CollDisbursementTransactionType.Code = "DISB";

                // ---------------------------------------------------
                // KD - 2/3/2000 PR#84875
                // Display processing date instead of created_ts
                // ----------------------------------------------------
                local.Coll.Update.CollLocalFinanceWorkArea.DisbDate =
                  entities.Disbursement.ProcessDate;

                break;
              case 'P':
                local.Coll.Update.CollDisbursementTransactionType.Code = "PASS";

                // ---------------------------------------------------
                // KD - 6/18/99
                // After discussing with SMEs(Jennifer E, Linda D and Ellen F)
                // it was decided that Passthrus will display Collection date
                // and not creation date.
                // ----------------------------------------------------
                local.Coll.Update.CollLocalFinanceWorkArea.DisbDate =
                  entities.Cr.CollectionDate;

                // ---------------------------------------------------
                // KD 5/27/2000
                // This code is still required since we are not converting
                // Excess URA disbursements.
                // ----------------------------------------------------
                break;
              case 'X':
                local.Coll.Update.CollDisbursementTransactionType.Code = "URA";

                // ---------------------------------------------------
                // KD - 2/3/2000 PR#84875
                // Display processing date instead of created_ts
                // ----------------------------------------------------
                local.Coll.Update.CollLocalFinanceWorkArea.DisbDate =
                  entities.Disbursement.ProcessDate;

                // ---------------------------------------------------
                // KD - 3/7/2000 PR#90072
                // Use process date of the 'cr' record for netting. URA process
                // sets this to 'CURRENT DATE'. So it won't matter anyway.
                // This change is to keep the URA netting process consistent
                // with COLL records.
                // --------------------------------------------------
                local.Coll.Update.CollDisbursementTransaction.ProcessDate =
                  entities.Cr.ProcessDate;

                break;
              default:
                break;
            }

            // ---------------------------------------------------
            // KD - 3/17/99
            // For Debit records do not display Reference # and Cash
            // receipt date
            // --------------------------------------------------
            local.Coll.Update.CollDisbursementTransaction.ReferenceNumber =
              entities.Disbursement.ReferenceNumber;
            local.Coll.Update.CollDisbursementTransaction.
              SystemGeneratedIdentifier =
                entities.Disbursement.SystemGeneratedIdentifier;

            if (!ReadDisbursementType())
            {
              ExitState = "FN0000_DISB_TYP_NF";

              return;
            }

            // ---------------------------------------------------
            // KD 5/27/2000
            // Flag Excess URA disbursements. Replace Entity Attr view
            // disb_type.code with Work Attr text_work_area.text_10.
            // ----------------------------------------------------
            if (AsChar(entities.Disbursement.ExcessUraInd) == 'Y')
            {
              local.Coll.Update.CollGrpDisbType.Text10 =
                TrimEnd(entities.DisbursementType.Code) + " X";
            }
            else
            {
              local.Coll.Update.CollGrpDisbType.Text10 =
                entities.DisbursementType.Code;
            }

            // ----------------------------------------
            // KD - 1/28/99
            // Add USE statement below.
            // ----------------------------------------
            UseCabSetMaximumDiscontinueDate2();

            // ----------------------------------------
            // KD - 6/1/99
            // Read each optimized for 1
            // Retrieve most recent status(since hist end dates are set to
            // current when suppressions are released, there could
            // possibly be more than one record which meet our selection
            // criteria)
            // ----------------------------------------
            // ----------------------------------------
            // KD - 7/16/99
            // Use local view of Current Date.
            // ----------------------------------------
            ReadDisbursementStatusDisbursementStatusHistory();

            if (!entities.DisbursementStatusHistory.Populated)
            {
              ExitState = "FN0000_DISB_STAT_HIST_NF";

              return;
            }

            // ----------------------------------------
            // KD - 1/28/99
            // Remove call to CAB hardcode_disb_values.
            // Suppress status has an id = 3
            // ----------------------------------------
            if (entities.DisbursementStatus.SystemGeneratedIdentifier == 3)
            {
              local.Coll.Update.CollPaymentRequest.Number = "SUPPRESS";
            }
            else
            {
              if (!ReadPaymentRequest2())
              {
                local.Coll.Update.CollPaymentRequest.Number = "";

                goto Test1;
              }

              local.Coll.Update.CollPaymentRequest.Assign(
                entities.PaymentRequest);

              if (Equal(entities.PaymentRequest.Type1, "EFT"))
              {
                if (!ReadElectronicFundTransmission())
                {
                  ExitState = "FN0000_EFT_NF";

                  return;
                }

                local.Coll.Update.CollPaymentRequest.Number =
                  NumberToString(entities.ElectronicFundTransmission.
                    TransmissionIdentifier, 7, 9);
              }
            }

Test1:
            ;
          }
        }

ReadEach:

        if (AsChar(local.DisbTranFoundFlagIn.Flag) != 'Y')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (export.Group.IsFull)
            {
              break;
            }

            export.Group.Next();

            break;

            export.Group.Next();
          }

          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        // *** DO THE SUMMATION AND REPORT DETAIL LINES ***
        //     
        // ----------------------------------------
        // --------------------------------------------------------------
        // Updated netting rules :-
        // 1. PASS records will NOT be rolled up.
        // 2. COLL records are rolled up with other COLL records if
        // they are both positive (or both negative) and if they have
        // same process_dte, CRE received_dte and Reference #.
        // 3. DISB records are rolled up with other DISB records if they
        // are both positive (or both negative) and have same
        // disb_type, reference #, warr # and disb_date(process_date
        // of 'D' record).
        // 4. URA records will be rolled up with other URA records if
        // they have same process_date (of 'X' record) and disb_type.
        // --------------------------------------------------------------
        local.FirsttimeFlag.Flag = "Y";
        local.InsertDisbLine.Flag = "N";

        local.Coll.Index = 0;
        local.Coll.CheckSize();

        local.Disb.Index = -1;
        local.Group.Index = -1;
        local.LastSuscriptDisbdet.Count = 0;
        local.LastSubscriptDisbcoll.Count = local.Coll.Count;

        while(local.Coll.Index < local.LastSubscriptDisbcoll.Count)
        {
          // -------------------------------------------------------------
          // KD - 6/18/99
          // Do not net PASS records! This business rule results
          // from a discussion with SMEs Jennifer E, Ellen F and Linda
          // D. The SMEs want to see Collection date on Passthrus and
          // not create date.
          // --------------------------------------------------------------
          // ------------------------------------------------------------
          // This IF statement is 'True' for COLL, PASS and URA records.
          // ------------------------------------------------------------
          if (!Equal(local.Coll.Item.CollDisbursementTransactionType.Code,
            "DISB"))
          {
            // *******************************************************
            // Kalpesh Doshi
            // 11/18/99 PR#80496
            // Use CRE.Received_date instead of CR.Receipt_date.
            // 3/8/2000 PR#90072
            // Use process_date instead of create_date to net COLL and URA 
            // records.
            // 5/27/2000 Work Order #164
            // Use work attribute view disb_type instead of Entity view 
            // Disbursement_type.
            // *******************************************************
            if (Equal(local.Coll.Item.CollCashReceipt.ReceivedDate,
              local.PrevTempCashReceipt.ReceivedDate) && Equal
              (local.Coll.Item.CollGrpRefNbr.CrdCrCombo,
              local.PrevTempCrdCrComboNo.CrdCrCombo) && Equal
              (local.Coll.Item.CollDisbursementTransactionType.Code,
              local.PrevTempDisbursementTransactionType.Code) && Equal
              (local.Coll.Item.CollDisbursementTransaction.ProcessDate,
              local.PrevTempDisbursementTransaction.ProcessDate) && Equal
              (local.Coll.Item.CollGrpDisbType.Text10,
              local.PrevTempDisbType.Text10) && !
              Equal(local.Coll.Item.CollDisbursementTransactionType.Code, "PASS")
              && !
              Equal(local.PrevTempDisbursementTransactionType.Code, "PASS"))
            {
              if (Equal(local.Coll.Item.CollDisbursementTransactionType.Code,
                "COLL"))
              {
                // Maintain +ve and -ve totals seperately.
                if (local.Coll.Item.CollGrpCr.Amount < 0)
                {
                  local.NegativeCredit.DisbAmount += local.Coll.Item.CollGrpCr.
                    Amount;
                }
                else
                {
                  local.PrevCredit.DisbAmount += local.Coll.Item.CollGrpCr.
                    Amount;
                }
              }
              else
              {
                // ------------------------------------------------------
                // KD - 6/14/99
                // Must be PASS or URA. Maintain +ve and -ve running debits.
                // -----------------------------------------------------
                if (local.Coll.Item.CollGrpDb.Amount < 0)
                {
                  local.NegativeDebit.DisbAmount += local.Coll.Item.CollGrpDb.
                    Amount;
                }
                else
                {
                  local.PrevDebit.DisbAmount += local.Coll.Item.CollGrpDb.
                    Amount;
                }
              }

              if (local.Coll.Index + 1 >= Local.CollGroup.Capacity)
              {
                local.GvOverflow.Flag = "Y";

                break;
              }

              ++local.Coll.Index;
              local.Coll.CheckSize();

              continue;
            }
            else
            {
              if (AsChar(local.FirsttimeFlag.Flag) != 'Y')
              {
                // **** MOVE THE PREV REC. TO EXPORT GR. VIEW ***
                if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                {
                  local.GvOverflow.Flag = "Y";

                  break;
                }

                // If negative total then check if need to show adjustment add
                // a row with negative collection amount, else do not display
                // the collection.
                if (local.NegativeCredit.DisbAmount < 0)
                {
                  if (AsChar(export.ShowAdj.Flag) == 'N')
                  {
                    for(local.Disb.Index = 0; local.Disb.Index < local
                      .Disb.Count; ++local.Disb.Index)
                    {
                      if (!local.Disb.CheckSize())
                      {
                        break;
                      }

                      local.Disb.Update.DisbDisbursementTransactionType.Code =
                        "";
                      local.Disb.Update.DisbGrpDisbType.Text10 = "";
                      local.Disb.Update.DisbLocalFinanceWorkArea.DisbDate =
                        null;
                      local.Disb.Update.DisbPaymentRequest.Type1 = "";
                      local.Disb.Update.DisbPaymentRequest.Number = "";
                      local.Disb.Update.DisbPaymentRequest.PrintDate = null;
                      local.Disb.Update.DisbGrpCr.Amount = 0;
                      local.Disb.Update.DisbGrpDb.Amount = 0;
                      local.Disb.Update.DisbDisbursementTransaction.
                        SystemGeneratedIdentifier = 0;
                      local.Disb.Update.DisbDisbursementTransaction.
                        ReferenceNumber =
                          Spaces(DisbursementTransaction.
                          ReferenceNumber_MaxLength);

                      // ********************************************************
                      // Kalpesh Doshi
                      // 11/18/99 PR#80496
                      // Use CRE.Received_date instead of CR.Receipt_date.
                      // *******************************************************
                      local.Disb.Update.DisbCashReceipt.ReceivedDate =
                        local.Disb.Item.DisbLocalFinanceWorkArea.DisbDate;
                      local.Disb.Update.DisbGrpRefNbr.CrdCrCombo = "";
                    }

                    local.Disb.CheckIndex();
                    local.NegativeCredit.DisbAmount = 0;

                    goto Test2;
                  }

                  // *** Move prev record to local_grp ***
                  if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                  {
                    local.GvOverflow.Flag = "Y";

                    break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  // ********************************************
                  //    Fix for the view overflow problem .
                  //  PR # 121989 .
                  //       -----Madhu Kumar
                  // ********************************************
                  local.Group.Update.LocalFinanceWorkArea.DisbDate =
                    local.PrevTempLocalFinanceWorkArea.DisbDate;
                  local.Group.Update.DisbursementTransactionType.Code =
                    local.PrevTempDisbursementTransactionType.Code;
                  local.Group.Update.DisbType.Text10 =
                    local.PrevTempDisbType.Text10;
                  local.Group.Update.PaymentRequest.Assign(
                    local.PrevTempPaymentRequest);
                  MoveDisbursementTransaction1(local.
                    PrevTempDisbursementTransaction,
                    local.Group.Update.DisbursementTransaction);
                  local.Group.Update.CashReceipt.ReceivedDate =
                    local.PrevTempCashReceipt.ReceivedDate;
                  local.Group.Update.RefNbr.CrdCrCombo =
                    local.PrevTempCrdCrComboNo.CrdCrCombo;
                  local.Group.Update.Db.Amount = 0;
                  local.Group.Update.Cr.Amount =
                    local.NegativeCredit.DisbAmount;

                  // -------------------------------------
                  // KD - 3/23/99
                  // RESET the previous record
                  // -------------------------------------
                  local.NegativeCredit.DisbAmount = 0;
                  local.SortsumFlag.Flag = "Y";
                }

                if (local.PrevCredit.DisbAmount > 0)
                {
                  if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                  {
                    local.GvOverflow.Flag = "Y";

                    break;
                  }

                  // ********************************************
                  //    Fix for the view overflow problem .
                  //  PR # 121989 .
                  // -----Madhu Kumar
                  // ********************************************
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.LocalFinanceWorkArea.DisbDate =
                    local.PrevTempLocalFinanceWorkArea.DisbDate;
                  local.Group.Update.DisbursementTransactionType.Code =
                    local.PrevTempDisbursementTransactionType.Code;
                  local.Group.Update.DisbType.Text10 =
                    local.PrevTempDisbType.Text10;
                  local.Group.Update.PaymentRequest.Assign(
                    local.PrevTempPaymentRequest);
                  MoveDisbursementTransaction1(local.
                    PrevTempDisbursementTransaction,
                    local.Group.Update.DisbursementTransaction);
                  local.Group.Update.CashReceipt.ReceivedDate =
                    local.PrevTempCashReceipt.ReceivedDate;
                  local.Group.Update.RefNbr.CrdCrCombo =
                    local.PrevTempCrdCrComboNo.CrdCrCombo;
                  local.Group.Update.Db.Amount = 0;
                  local.Group.Update.Cr.Amount = local.PrevCredit.DisbAmount;

                  // -----------------------------------------
                  // KD - 3/23/99
                  // RESET the previous record
                  // -----------------------------------------
                  local.PrevCredit.DisbAmount = 0;
                  local.SortsumFlag.Flag = "Y";
                }

                if (local.NegativeDebit.DisbAmount < 0)
                {
                  if (AsChar(export.ShowAdj.Flag) == 'N')
                  {
                    for(local.Disb.Index = 0; local.Disb.Index < local
                      .Disb.Count; ++local.Disb.Index)
                    {
                      if (!local.Disb.CheckSize())
                      {
                        break;
                      }

                      local.Disb.Update.DisbDisbursementTransactionType.Code =
                        "";
                      local.Disb.Update.DisbGrpDisbType.Text10 = "";
                      local.Disb.Update.DisbLocalFinanceWorkArea.DisbDate =
                        null;
                      local.Disb.Update.DisbPaymentRequest.Type1 = "";
                      local.Disb.Update.DisbPaymentRequest.Number = "";
                      local.Disb.Update.DisbPaymentRequest.PrintDate = null;
                      local.Disb.Update.DisbGrpCr.Amount = 0;
                      local.Disb.Update.DisbGrpDb.Amount = 0;
                      local.Disb.Update.DisbDisbursementTransaction.
                        SystemGeneratedIdentifier = 0;
                      local.Disb.Update.DisbDisbursementTransaction.
                        ReferenceNumber =
                          Spaces(DisbursementTransaction.
                          ReferenceNumber_MaxLength);

                      // ********************************************************
                      // Kalpesh Doshi
                      // 11/18/99 PR#80496
                      // Use CRE.Received_date instead of CR.Receipt_date.
                      // *******************************************************
                      local.Disb.Update.DisbCashReceipt.ReceivedDate =
                        local.Disb.Item.DisbLocalFinanceWorkArea.DisbDate;
                      local.Disb.Update.DisbGrpRefNbr.CrdCrCombo = "";
                    }

                    local.Disb.CheckIndex();
                    local.NegativeDebit.DisbAmount = 0;

                    goto Test2;
                  }

                  // *** Move prev record to local_grp ***
                  if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                  {
                    local.GvOverflow.Flag = "Y";

                    break;
                  }

                  // ********************************************
                  //     Fix for the view overflow problem .
                  //  PR # 121989 .
                  // -----Madhu Kumar
                  // ********************************************
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.LocalFinanceWorkArea.DisbDate =
                    local.PrevTempLocalFinanceWorkArea.DisbDate;
                  local.Group.Update.DisbursementTransactionType.Code =
                    local.PrevTempDisbursementTransactionType.Code;
                  local.Group.Update.DisbType.Text10 =
                    local.PrevTempDisbType.Text10;
                  local.Group.Update.PaymentRequest.Assign(
                    local.PrevTempPaymentRequest);
                  MoveDisbursementTransaction1(local.
                    PrevTempDisbursementTransaction,
                    local.Group.Update.DisbursementTransaction);
                  local.Group.Update.CashReceipt.ReceivedDate =
                    local.PrevTempCashReceipt.ReceivedDate;
                  local.Group.Update.RefNbr.CrdCrCombo =
                    local.PrevTempCrdCrComboNo.CrdCrCombo;
                  local.Group.Update.Db.Amount = local.NegativeDebit.DisbAmount;
                  local.Group.Update.Cr.Amount = 0;

                  // ------------------------------------------
                  // KD - 3/23/99
                  // RESET the previous record
                  // ------------------------------------------
                  local.NegativeDebit.DisbAmount = 0;
                }

                if (local.PrevDebit.DisbAmount > 0)
                {
                  if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                  {
                    local.GvOverflow.Flag = "Y";

                    break;
                  }

                  // ********************************************
                  //     Fix for the view overflow problem .
                  //  PR # 121989 .
                  // -----Madhu Kumar
                  // ********************************************
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.LocalFinanceWorkArea.DisbDate =
                    local.PrevTempLocalFinanceWorkArea.DisbDate;
                  local.Group.Update.DisbursementTransactionType.Code =
                    local.PrevTempDisbursementTransactionType.Code;
                  local.Group.Update.DisbType.Text10 =
                    local.PrevTempDisbType.Text10;
                  local.Group.Update.PaymentRequest.Assign(
                    local.PrevTempPaymentRequest);
                  MoveDisbursementTransaction1(local.
                    PrevTempDisbursementTransaction,
                    local.Group.Update.DisbursementTransaction);
                  local.Group.Update.CashReceipt.ReceivedDate =
                    local.PrevTempCashReceipt.ReceivedDate;
                  local.Group.Update.RefNbr.CrdCrCombo =
                    local.PrevTempCrdCrComboNo.CrdCrCombo;
                  local.Group.Update.Db.Amount = local.PrevDebit.DisbAmount;
                  local.Group.Update.Cr.Amount = 0;

                  // -----------------------------------------
                  // KD - 3/23/99
                  // RESET the previous record
                  // -----------------------------------------
                  local.PrevDebit.DisbAmount = 0;
                }
              }

Test2:

              // *** INITIALIZE THE PREVIOUS RECORD WITH THE CURRENT RECORD ***
              local.PrevTempLocalFinanceWorkArea.DisbDate =
                local.Coll.Item.CollLocalFinanceWorkArea.DisbDate;
              local.PrevTempDisbursementTransactionType.Code =
                local.Coll.Item.CollDisbursementTransactionType.Code;
              local.PrevTempDisbType.Text10 =
                local.Coll.Item.CollGrpDisbType.Text10;
              local.PrevTempPaymentRequest.Assign(
                local.Coll.Item.CollPaymentRequest);

              if (local.Coll.Item.CollGrpCr.Amount < 0)
              {
                local.NegativeCredit.DisbAmount =
                  local.Coll.Item.CollGrpCr.Amount;
              }
              else if (local.Coll.Item.CollGrpCr.Amount > 0)
              {
                local.PrevCredit.DisbAmount = local.Coll.Item.CollGrpCr.Amount;
              }

              if (local.Coll.Item.CollGrpDb.Amount < 0)
              {
                local.NegativeDebit.DisbAmount =
                  local.Coll.Item.CollGrpDb.Amount;
              }
              else if (local.Coll.Item.CollGrpDb.Amount > 0)
              {
                local.PrevDebit.DisbAmount = local.Coll.Item.CollGrpDb.Amount;
              }

              local.PrevTempDisbursementTransaction.Assign(
                local.Coll.Item.CollDisbursementTransaction);
              local.PrevTempCashReceipt.ReceivedDate =
                local.Coll.Item.CollCashReceipt.ReceivedDate;
              local.PrevTempCrdCrComboNo.CrdCrCombo =
                local.Coll.Item.CollGrpRefNbr.CrdCrCombo;

              if (local.Coll.Index + 1 >= Local.CollGroup.Capacity)
              {
                local.GvOverflow.Flag = "Y";

                break;
              }

              ++local.Coll.Index;
              local.Coll.CheckSize();

              local.FirsttimeFlag.Flag = "N";
            }
          }
          else
          {
            // *** MOVE THE DISB RECORD TO THE LOCAL_DISB_GROUP  ***
            //     
            // ---------------------------------------------------
            if (local.Disb.Index + 1 >= Local.DisbGroup.Capacity)
            {
              local.GvOverflow.Flag = "Y";

              break;
            }

            ++local.Disb.Index;
            local.Disb.CheckSize();

            local.Disb.Update.DisbLocalFinanceWorkArea.DisbDate =
              local.Coll.Item.CollLocalFinanceWorkArea.DisbDate;
            local.Disb.Update.DisbDisbursementTransactionType.Code =
              local.Coll.Item.CollDisbursementTransactionType.Code;
            local.Disb.Update.DisbGrpDisbType.Text10 =
              local.Coll.Item.CollGrpDisbType.Text10;
            local.Disb.Update.DisbPaymentRequest.Assign(
              local.Coll.Item.CollPaymentRequest);
            local.Disb.Update.DisbGrpDb.Amount =
              local.Coll.Item.CollGrpDb.Amount;
            local.Disb.Update.DisbGrpCr.Amount =
              local.Coll.Item.CollGrpCr.Amount;
            MoveDisbursementTransaction1(local.Coll.Item.
              CollDisbursementTransaction,
              local.Disb.Update.DisbDisbursementTransaction);
            local.Disb.Update.DisbCashReceipt.ReceivedDate =
              local.Coll.Item.CollCashReceipt.ReceivedDate;
            local.Disb.Update.DisbGrpRefNbr.CrdCrCombo =
              local.Coll.Item.CollGrpRefNbr.CrdCrCombo;

            if (local.Coll.Index + 1 >= Local.CollGroup.Capacity)
            {
              local.GvOverflow.Flag = "Y";

              break;
            }

            ++local.Coll.Index;
            local.Coll.CheckSize();
          }

          if (AsChar(local.SortsumFlag.Flag) == 'Y')
          {
            if (local.Disb.Count > 1)
            {
              // --------------------------------------------------------------
              // CAB sorts input grp view in following order:
              // Disb Dte Asc
              // Ref Nbr Asc
              // Disb type Asc
              // Pay Req # Asc(a.k.a. Warr # Asc)
              // Summation will be performed if  Disb dte, Ref Nbr, Disb
              // Type and Pay Req # are the same.
              // --------------------------------------------------------------
              UseFnDisbSortSumGrpViewVer2();

              if (IsExitState("FN0000_GROUP_VIEW_LIMIT_EXCEEDED"))
              {
                return;
              }
            }

            local.SortsumFlag.Flag = "N";
            local.InsertDisbLine.Flag = "Y";
            local.LastSuscriptDisbdet.Count = local.Disb.Count;

            for(local.Disb.Index = 0; local.Disb.Index < local.Disb.Count; ++
              local.Disb.Index)
            {
              if (!local.Disb.CheckSize())
              {
                break;
              }

              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                local.GvOverflow.Flag = "Y";

                goto AfterCycle;
              }

              ++local.Group.Index;
              local.Group.CheckSize();

              local.Group.Update.LocalFinanceWorkArea.DisbDate =
                local.Disb.Item.DisbLocalFinanceWorkArea.DisbDate;
              local.Group.Update.DisbursementTransactionType.Code =
                local.Disb.Item.DisbDisbursementTransactionType.Code;
              local.Group.Update.DisbType.Text10 =
                local.Disb.Item.DisbGrpDisbType.Text10;
              local.Group.Update.PaymentRequest.Assign(
                local.Disb.Item.DisbPaymentRequest);
              local.Group.Update.Cr.Amount = local.Disb.Item.DisbGrpCr.Amount;
              local.Group.Update.Db.Amount = local.Disb.Item.DisbGrpDb.Amount;
              MoveDisbursementTransaction1(local.Disb.Item.
                DisbDisbursementTransaction,
                local.Group.Update.DisbursementTransaction);
              local.Group.Update.CashReceipt.ReceivedDate =
                local.Disb.Item.DisbCashReceipt.ReceivedDate;
              local.Group.Update.RefNbr.CrdCrCombo =
                local.Disb.Item.DisbGrpRefNbr.CrdCrCombo;
            }

            local.Disb.CheckIndex();

            // *** INITIALIZE local_disb_group ***
            //     ----------------------------
            for(local.Disb.Index = 0; local.Disb.Index < local.Disb.Count; ++
              local.Disb.Index)
            {
              if (!local.Disb.CheckSize())
              {
                break;
              }

              local.Disb.Update.DisbDisbursementTransactionType.Code = "";
              local.Disb.Update.DisbGrpDisbType.Text10 = "";
              local.Disb.Update.DisbLocalFinanceWorkArea.DisbDate = null;
              local.Disb.Update.DisbPaymentRequest.Type1 = "";
              local.Disb.Update.DisbPaymentRequest.Number = "";
              local.Disb.Update.DisbPaymentRequest.PrintDate = null;
              local.Disb.Update.DisbGrpCr.Amount = 0;
              local.Disb.Update.DisbGrpDb.Amount = 0;
              local.Disb.Update.DisbDisbursementTransaction.
                SystemGeneratedIdentifier = 0;
              local.Disb.Update.DisbDisbursementTransaction.ReferenceNumber =
                Spaces(DisbursementTransaction.ReferenceNumber_MaxLength);

              // ********************************************************
              // Kalpesh Doshi
              // 11/18/99 PR#80496
              // Use CRE.Received_date instead of CR.Receipt_date.
              // *******************************************************
              local.Disb.Update.DisbCashReceipt.ReceivedDate =
                local.Disb.Item.DisbLocalFinanceWorkArea.DisbDate;
              local.Disb.Update.DisbGrpRefNbr.CrdCrCombo = "";
            }

            local.Disb.CheckIndex();
            local.Disb.Index = -1;
            local.Disb.Count = 0;
          }
        }

AfterCycle:

        // ** INCLUDE THE LAST-COLLECTION & THE CORRESPONDING DISB. HERE **
        // --------------------------------------------------------------
        // CAB sorts input grp view in following order:
        // Disb Dte Asc
        // Ref Nbr Asc
        // Disb type Asc
        // Pay Req # Asc(a.k.a. Warr # Asc)
        // Summation will be performed if  Disb dte, Ref Nbr, Disb
        // Type and Pay Req # are the same.
        // --------------------------------------------------------------
        // --------------------------------------------------------------
        // SWSRKXD - 7/19/99
        // Replace with new version.
        // --------------------------------------------------------------
        UseFnDisbSortSumGrpViewVer2();

        if (IsExitState("FN0000_GROUP_VIEW_LIMIT_EXCEEDED"))
        {
          return;
        }

        local.Disb.Index = -1;
        local.LastSuscriptDisbdet.Count = local.Disb.Count;
        local.InsertBlank.Flag = "Y";

        do
        {
          if (local.Disb.Index == -1)
          {
            // --------------------------------------------------------------
            // SWSRKXD - 9/14/99. PR # 73183
            // There was a bug whereby it would only display either the +
            // or - CR not both. It should display both. This was a problem
            // only when it is the last collection in the array.
            // --------------------------------------------------------------
            if (local.PrevCredit.DisbAmount != 0)
            {
              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                local.GvOverflow.Flag = "Y";

                break;
              }

              ++local.Group.Index;
              local.Group.CheckSize();

              local.Group.Update.LocalFinanceWorkArea.DisbDate =
                local.PrevTempLocalFinanceWorkArea.DisbDate;
              local.Group.Update.DisbursementTransactionType.Code =
                local.PrevTempDisbursementTransactionType.Code;
              local.Group.Update.DisbType.Text10 =
                local.PrevTempDisbType.Text10;
              local.Group.Update.PaymentRequest.Assign(
                local.PrevTempPaymentRequest);
              MoveDisbursementTransaction1(local.
                PrevTempDisbursementTransaction,
                local.Group.Update.DisbursementTransaction);
              local.Group.Update.CashReceipt.ReceivedDate =
                local.PrevTempCashReceipt.ReceivedDate;
              local.Group.Update.RefNbr.CrdCrCombo =
                local.PrevTempCrdCrComboNo.CrdCrCombo;
              local.Group.Update.Db.Amount = 0;
              local.Group.Update.Cr.Amount = local.PrevCredit.DisbAmount;

              // ------------------------------------------
              // KD - 3/23/99
              // RESET the previous record
              // ------------------------------------------
              local.PrevCredit.DisbAmount = 0;
            }

            // If negative total then check if need to show adjustment add a row
            // with negative collection amount, else do not display the
            // collection.
            if (local.NegativeCredit.DisbAmount < 0)
            {
              if (AsChar(export.ShowAdj.Flag) == 'N')
              {
                break;
              }

              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                local.GvOverflow.Flag = "Y";

                break;
              }

              ++local.Group.Index;
              local.Group.CheckSize();

              local.Group.Update.LocalFinanceWorkArea.DisbDate =
                local.PrevTempLocalFinanceWorkArea.DisbDate;
              local.Group.Update.DisbursementTransactionType.Code =
                local.PrevTempDisbursementTransactionType.Code;
              local.Group.Update.DisbType.Text10 =
                local.PrevTempDisbType.Text10;
              local.Group.Update.PaymentRequest.Assign(
                local.PrevTempPaymentRequest);
              MoveDisbursementTransaction1(local.
                PrevTempDisbursementTransaction,
                local.Group.Update.DisbursementTransaction);
              local.Group.Update.CashReceipt.ReceivedDate =
                local.PrevTempCashReceipt.ReceivedDate;
              local.Group.Update.RefNbr.CrdCrCombo =
                local.PrevTempCrdCrComboNo.CrdCrCombo;
              local.Group.Update.Db.Amount = 0;
              local.Group.Update.Cr.Amount = local.NegativeCredit.DisbAmount;

              // ------------------------------------------
              // KD - 3/23/99
              // RESET the previous record
              // ------------------------------------------
              local.NegativeCredit.DisbAmount = 0;
            }

            if (local.NegativeDebit.DisbAmount < 0)
            {
              if (AsChar(export.ShowAdj.Flag) == 'N')
              {
                break;
              }

              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                local.GvOverflow.Flag = "Y";

                break;
              }

              ++local.Group.Index;
              local.Group.CheckSize();

              local.Group.Update.LocalFinanceWorkArea.DisbDate =
                local.PrevTempLocalFinanceWorkArea.DisbDate;
              local.Group.Update.DisbursementTransactionType.Code =
                local.PrevTempDisbursementTransactionType.Code;
              local.Group.Update.DisbType.Text10 =
                local.PrevTempDisbType.Text10;
              local.Group.Update.PaymentRequest.Assign(
                local.PrevTempPaymentRequest);
              MoveDisbursementTransaction1(local.
                PrevTempDisbursementTransaction,
                local.Group.Update.DisbursementTransaction);
              local.Group.Update.CashReceipt.ReceivedDate =
                local.PrevTempCashReceipt.ReceivedDate;
              local.Group.Update.RefNbr.CrdCrCombo =
                local.PrevTempCrdCrComboNo.CrdCrCombo;
              local.Group.Update.Db.Amount = local.NegativeDebit.DisbAmount;
              local.Group.Update.Cr.Amount = 0;

              // ------------------------------------------
              // KD - 3/23/99
              // RESET the previous record
              // ------------------------------------------
              local.NegativeDebit.DisbAmount = 0;
            }

            if (local.PrevDebit.DisbAmount != 0)
            {
              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                local.GvOverflow.Flag = "Y";

                break;
              }

              ++local.Group.Index;
              local.Group.CheckSize();

              local.Group.Update.LocalFinanceWorkArea.DisbDate =
                local.PrevTempLocalFinanceWorkArea.DisbDate;
              local.Group.Update.DisbursementTransactionType.Code =
                local.PrevTempDisbursementTransactionType.Code;
              local.Group.Update.DisbType.Text10 =
                local.PrevTempDisbType.Text10;
              local.Group.Update.PaymentRequest.Assign(
                local.PrevTempPaymentRequest);
              MoveDisbursementTransaction1(local.
                PrevTempDisbursementTransaction,
                local.Group.Update.DisbursementTransaction);
              local.Group.Update.CashReceipt.ReceivedDate =
                local.PrevTempCashReceipt.ReceivedDate;
              local.Group.Update.RefNbr.CrdCrCombo =
                local.PrevTempCrdCrComboNo.CrdCrCombo;
              local.Group.Update.Db.Amount = local.PrevDebit.DisbAmount;
              local.Group.Update.Cr.Amount = 0;

              // -----------------------------------------
              // KD - 3/23/99
              // RESET the previous record
              // -----------------------------------------
              local.PrevDebit.DisbAmount = 0;
            }

            local.SortsumFlag.Flag = "Y";

            if (local.Disb.Index + 1 >= Local.DisbGroup.Capacity)
            {
              local.GvOverflow.Flag = "Y";

              break;
            }

            ++local.Disb.Index;
            local.Disb.CheckSize();
          }
          else
          {
            if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
            {
              local.GvOverflow.Flag = "Y";

              break;
            }

            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.DisbursementTransactionType.Code =
              local.Disb.Item.DisbDisbursementTransactionType.Code;
            local.Group.Update.DisbType.Text10 =
              local.Disb.Item.DisbGrpDisbType.Text10;
            local.Group.Update.LocalFinanceWorkArea.DisbDate =
              local.Disb.Item.DisbLocalFinanceWorkArea.DisbDate;
            local.Group.Update.PaymentRequest.Assign(
              local.Disb.Item.DisbPaymentRequest);
            local.Group.Update.Cr.Amount = local.Disb.Item.DisbGrpCr.Amount;
            local.Group.Update.Db.Amount = local.Disb.Item.DisbGrpDb.Amount;
            MoveDisbursementTransaction1(local.Disb.Item.
              DisbDisbursementTransaction,
              local.Group.Update.DisbursementTransaction);
            local.Group.Update.CashReceipt.ReceivedDate =
              local.Disb.Item.DisbCashReceipt.ReceivedDate;
            local.Group.Update.RefNbr.CrdCrCombo =
              local.Disb.Item.DisbGrpRefNbr.CrdCrCombo;

            if (local.Disb.Index + 1 >= Local.DisbGroup.Capacity)
            {
              local.GvOverflow.Flag = "Y";

              break;
            }

            ++local.Disb.Index;
            local.Disb.CheckSize();
          }
        }
        while(local.Disb.Index < local.LastSuscriptDisbdet.Count);

        // **********************************************************************
        // Include the code to insert blank in the EXPORT gr view. The input may
        // be an Explicitly Gr. view.
        // **********************************************************************
        local.Group.Index = 0;
        local.Group.CheckSize();

        local.LastSubscriptDisbcoll.Count = local.Group.Count;
        local.InsertBlank.Flag = "Y";

        export.Group.Index = 0;
        export.Group.Clear();

        while(local.Group.Index < local.LastSubscriptDisbcoll.Count)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          if (local.Group.Index >= 1 && (
            Equal(local.Group.Item.DisbursementTransactionType.Code, "COLL") &&
            (Equal(local.PrevTempDisbursementTransactionType.Code, "DISB") || Equal
            (local.PrevTempDisbursementTransactionType.Code, "URA") || Equal
            (local.PrevTempDisbursementTransactionType.Code, "PASS")) || Equal
            (local.Group.Item.DisbursementTransactionType.Code, "URA") || Equal
            (local.Group.Item.DisbursementTransactionType.Code, "PASS")))
          {
            if (AsChar(local.InsertBlank.Flag) == 'Y')
            {
              export.Group.Update.Common.Flag = "";

              var field = GetField(export.Group.Item.Common, "flag");

              field.Color = "";
              field.Protected = true;

              local.InsertBlank.Flag = "N";
              export.Group.Next();

              continue;
            }

            local.InsertBlank.Flag = "Y";
          }

          // If the Collections for the Passthru are not needed, then bring the 
          // following block of MOVEs in to this IF.... statement.
          // If group_local_disbcoll_detail disbursement_transaction_type code 
          // IS NE "PASS"
          // ************************** START OF MOVE BLOCK 
          // ***********************
          export.Group.Update.LocalFinanceWorkArea.DisbDate =
            local.Group.Item.LocalFinanceWorkArea.DisbDate;
          export.Group.Update.DisbursementTransactionType.Code =
            local.Group.Item.DisbursementTransactionType.Code;
          export.Group.Update.DisbType.Text10 =
            local.Group.Item.DisbType.Text10;
          export.Group.Update.PaymentRequest.Assign(
            local.Group.Item.PaymentRequest);
          export.Group.Update.Cr.Amount = local.Group.Item.Cr.Amount;
          export.Group.Update.Db.Amount = local.Group.Item.Db.Amount;
          MoveDisbursementTransaction1(local.Group.Item.DisbursementTransaction,
            export.Group.Update.DisbursementTransaction);
          export.Group.Update.CashReceipt.ReceivedDate =
            local.Group.Item.CashReceipt.ReceivedDate;
          export.Group.Update.RefNo.CrdCrCombo =
            local.Group.Item.RefNbr.CrdCrCombo;

          // ************************** END OF MOVE BLOCK 
          // ***********************
          local.PrevTempDisbursementTransactionType.Code =
            local.Group.Item.DisbursementTransactionType.Code;

          ++local.Group.Index;
          local.Group.CheckSize();

          if (local.Group.Index >= 100)
          {
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";
            export.Group.Next();

            break;
          }

          export.Group.Next();
        }

        if (local.Group.Index + 1 < local.LastSubscriptDisbcoll.Count && export
          .Group.IsFull || AsChar(local.GvOverflow.Flag) == 'Y')
        {
          ExitState = "FN0000_GROUP_VIEW_OVERFLOW";
        }

        if (export.Group.IsEmpty)
        {
          export.Group.Index = 0;
          export.Group.Clear();

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (export.Group.IsFull)
            {
              break;
            }

            export.Group.Next();

            break;

            export.Group.Next();
          }

          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (Equal(export.Group.Item.LocalFinanceWorkArea.DisbDate, null))
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
            }

            break;
          }
        }

        // ------------------------------------------------
        // KD - 1/29/99
        // Flash message when display successful
        // ------------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "EXIT":
        break;
      case "LIST":
        // *** Processing for PROMPT ***
        if (AsChar(import.Prompt.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          // -------------------------------------------------
          // KD - 1/29/99
          // Highlight field when in error.
          // ------------------------------------------------
          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;
        }
        else
        {
          // --------------------------------------------
          // KD - 2/3/99
          // Reset prompt selection flag
          // --------------------------------------------
          export.Prompt.SelectChar = "";

          // -------------------------------------------------
          // KD - 2/3/99
          // Pass phonetic flag and percentage via dialog flow
          // ------------------------------------------------
          export.Phonetic.Flag = "Y";
          export.Phonetic.Percentage = 35;
          ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RETURN":
        // *** Work request 000238
        // *** 12/13/00 swsrchf
        // *** start
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPQ"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " XXNEXTXX";
            

          return;
        }

        // *** end
        // *** 12/13/00 swsrchf
        // *** Work request 000238
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();
        ExitState = "ECO_XFR_TO_SIGNOFF_PROCEDURE";

        break;
      case "WDTL":
        if (local.Select.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else if (local.Select.Count == 0)
        {
          ExitState = "ECO_XFR_TO_LST_WARRANT_DETAIL";

          return;
        }

        // --------------------------------------------
        // KD - 6/15/99
        // Allow flow to WDTL and DHST for URA and PASS.
        // --------------------------------------------
        if (!Equal(local.PrevTempDisbursementTransactionType.Code, "COLL"))
        {
          if (Equal(export.FlowPaymentRequest.Type1, "WAR"))
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!IsEmpty(export.Group.Item.Common.Flag))
              {
                export.Group.Update.Common.Flag = "";

                break;
              }
            }

            ExitState = "ECO_XFR_TO_LST_WARRANT_DETAIL";
          }
          else
          {
            ExitState = "FN0000_SELECT_WAR_RECORD";
          }
        }
        else
        {
          ExitState = "SELECT_DISBURSEMENT_RECORD";
        }

        break;
      case "DHST":
        if (local.Select.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else if (local.Select.Count == 0)
        {
          ExitState = "ECO_XFR_TO_LST_DISB_STAT_HST";

          return;
        }

        if (AsChar(local.Select.Flag) == 'S')
        {
          // *** Valid Selection...continue processing
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }

        // Prev_temp disbursement_transaction system_generated_id stores the 
        // DISBURSEMENT_TRANSACTION_RLN system_gen_id for the selected row, disb
        // or coll. Using this, the corresponding Collection or Disbursement
        // can be found out.
        // --------------------------------------------
        // KD - 6/15/99
        // Allow flow to WDTL and DHST for URA and PASS.
        // --------------------------------------------
        if (!Equal(local.PrevTempDisbursementTransactionType.Code, "COLL"))
        {
          // ----------------------------------------------------------
          // If the below READ is successful the exports are being set to flow 
          // to DHST. The Disbursement identifier is being compared with the
          // local_prev temp Disbursement Transaction sysgen_id. This local view
          // stores the Disbursement identifier.
          // -----------------------------------------------------------
          if (ReadDisbursement())
          {
            export.FlowDisbursementTransaction.SystemGeneratedIdentifier =
              entities.Disbursement.SystemGeneratedIdentifier;
            export.FlowDisbursementTransaction.Amount =
              local.SendDhst.DisbAmount;
            export.SendToDhst.Date = local.SendDhst.DisbDate;
            export.FlowCsePersonAccount.Type1 = "E";

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!IsEmpty(export.Group.Item.Common.Flag))
              {
                export.Group.Update.Common.Flag = "";

                break;
              }
            }

            ExitState = "ECO_XFR_TO_LST_DISB_STAT_HST";
          }
          else
          {
            ExitState = "FN0000_DISB_TRANSACTION_NF";
          }

          // ----------------------------------------------------------
          // Naveen - 11/01/1998
          // Commented out this this READ because the comparison was being done 
          // between the Disbursement transaction identifier and the
          // Disbursement identifier.
          // -----------------------------------------------------------
        }
        else
        {
          ExitState = "SELECT_DISBURSEMENT_RECORD";
        }

        break;
      case "CRRC":
        if (local.Select.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else if (local.Select.Count == 0)
        {
          ExitState = "ECO_XFR_TO_REC_CRRC";

          return;
        }

        if (AsChar(local.Select.Flag) == 'S')
        {
          // *** Valid Selection....continue processing
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }

        // ==> Pass the Cash_receipt_detail, cash_receipt_event, 
        // cash_receipt_source_type and cash_receipt_type system_generated_ids
        // to the "Record Cash Receipt Collection Details" screen
        // Prev_temp disbursement_transaction system_generated_id stores the 
        // disbursement_transaction_rln system_gen_id for the selected row, disb
        // or coll. Using this, the corresponding Collection or Disbursement
        // can be found out.
        // *** Get the corresponding COLLECTION Disbursement_Transaction ***
        if (Equal(local.PrevTempDisbursementTransactionType.Code, "COLL"))
        {
          local.DisbursementTransaction.SystemGeneratedIdentifier =
            local.PrevTempDisbursementTransaction.SystemGeneratedIdentifier;
        }
        else
        {
          // Do not allow to flow to CRRC screen if the type is other than COLL.
          // The rest of the logic can be used, if the user decides to have a
          // flow for type DISB to any other screen.
          ExitState = "FN0000_DISB_TYPE_SEL_NOT_ALLOWED";

          return;
        }

        // ----------------------------------------------
        // SWSRKXD - 7/28/99
        // Add obligee qualification in READ to use index.
        // ----------------------------------------------
        if (ReadCashReceiptDetail())
        {
          // *** Cash_receipt_detail found..pass it to the CRRC with other 
          // Identifiers...Continue Processing
          export.FlowCashReceiptDetail.SequentialIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
        }
        else
        {
          ExitState = "CASH_RECEIPT_DETAIL_NF";

          return;
        }

        if (ReadCashReceiptEventCashReceiptSourceType())
        {
          // *** Cash_receipt event and source_type found.. pass their IDs with 
          // other Ids to CRRC... Continue processing
          export.FlowCashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          export.FlowCashReceiptSourceType.SystemGeneratedIdentifier =
            entities.CashReceiptSourceType.SystemGeneratedIdentifier;
        }
        else
        {
          ExitState = "CASH_RCPT_EV_SRCTYPE_NF";

          return;
        }

        if (ReadCashReceiptType())
        {
          // ***Cash_receipt_type found...pass its ID with other Ids to CRRC...
          // continue processing
          export.FlowCashReceiptType.SystemGeneratedIdentifier =
            entities.CashReceiptType.SystemGeneratedIdentifier;
        }
        else
        {
          ExitState = "FN0113_CASH_RCPT_TYPE_NF";

          return;
        }

        if (ReadCollectionType())
        {
          // ***Collection_type is found...pass its ID with others to CRRC
          export.FlowCollectionType.SequentialIdentifier =
            entities.CollectionType.SequentialIdentifier;
        }
        else
        {
          ExitState = "FN0000_COLLECTION_TYPE_NF";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.Flag))
          {
            export.Group.Update.Common.Flag = "";

            break;
          }
        }

        // *** All the Identifiers were obtained ; set exitstate to transfer
        // to CRRC passing these IDs
        ExitState = "ECO_XFR_TO_REC_CRRC";

        break;
      case "PAYR":
        if (local.Select.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else if (local.Select.Count == 0)
        {
          ExitState = "FN0000_SEL_A_COLL_RECORD";

          return;
        }

        if (AsChar(local.Select.Flag) == 'S')
        {
          // *** Valid Selection....continue processing
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }

        // ********** Now read the obligor associated to this disbursement.
        if (!Equal(local.PrevTempDisbursementTransactionType.Code, "COLL"))
        {
          ExitState = "FN0000_SEL_A_COLL_RECORD";

          return;
        }

        local.DisbursementTransaction.SystemGeneratedIdentifier =
          local.PrevTempDisbursementTransaction.SystemGeneratedIdentifier;

        // ----------------------------------------------
        // SWSRKXD - 7/28/99
        // Add obligee qualification in READ to use index.
        // ----------------------------------------------
        if (!ReadCollection())
        {
          ExitState = "FN0000_COLLECTION_NF";

          return;
        }

        if (ReadCsePersonObligation())
        {
          export.Payor.Number = entities.CsePerson.Number;
        }
        else
        {
          ExitState = "CSE_PERS_TO_PASS_TO_DISB_SUM_NF";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.Flag))
          {
            export.Group.Update.Common.Flag = "";

            break;
          }
        }

        // -**** Set exitstate to flow to PAYR
        ExitState = "ECO_LNK_TO_COLLECTION_DETAILS";

        break;
      case "PSUM":
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        ExitState = "ECO_XFR_TO_LST_MNTHLY_PAYEE_SUM";

        break;
      case "EDTL":
        if (local.Select.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        if (local.Select.Count == 0)
        {
          ExitState = "ECO_LNK_LST_EFT_DETAIL";

          return;
        }

        if (Equal(local.PrevTempDisbursementTransactionType.Code, "DISB"))
        {
          if (Equal(export.FlowPaymentRequest.Type1, "EFT"))
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!IsEmpty(export.Group.Item.Common.Flag))
              {
                export.Group.Update.Common.Flag = "";

                // ----------------------------------------------
                // SWSRKXD - 6/24/99
                // Export EFT transmission_id.
                // ----------------------------------------------
                export.ElectronicFundTransmission.TransmissionIdentifier =
                  (int)StringToNumber(export.Group.Item.PaymentRequest.Number);
                export.ElectronicFundTransmission.TransmissionType = "O";

                break;
              }
            }

            ExitState = "ECO_LNK_LST_EFT_DETAIL";

            return;
          }

          ExitState = "FN0000_SELECT_EFT_RECORD";
        }
        else
        {
          ExitState = "SELECT_DISBURSEMENT_RECORD";
        }

        break;
      case "OREL":
        if (local.Select.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        if (local.Select.Count == 0)
        {
          ExitState = "ECO_LNK_LST_POTNTL_RCVRY_OBLG";

          return;
        }

        if (!Equal(export.FlowPaymentRequest.Type1, "RCV"))
        {
          ExitState = "FN0000_SELECT_RCV_RECORD";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.Flag))
          {
            export.Group.Update.Common.Flag = "";

            if (!ReadPaymentRequest1())
            {
              ExitState = "FN0000_PAYMENT_REQUEST_NF";

              return;
            }

            export.OrelFromDate.ProcessDate =
              Date(entities.PaymentRequest.CreatedTimestamp);
            export.OrelToDate.ProcessDate =
              Date(entities.PaymentRequest.CreatedTimestamp);

            if (!ReadPaymentStatus())
            {
              ExitState = "FN0000_PYMNT_STAT_HIST_NF";

              return;
            }

            switch(TrimEnd(entities.PaymentStatus.Code))
            {
              case "RCVPOT":
                export.OrelSearchStatus.SelectChar = "P";

                break;
              case "RCVCRE":
                export.OrelSearchStatus.SelectChar = "C";

                break;
              case "RCVDEN":
                export.OrelSearchStatus.SelectChar = "D";

                break;
              default:
                break;
            }

            break;
          }
        }

        ExitState = "ECO_LNK_LST_POTNTL_RCVRY_OBLG";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveDisbToGroup(Local.DisbGroup source,
    FnDisbSortSumGrpViewVer2.Import.GroupGroup target)
  {
    target.DisbType.Text10 = source.DisbGrpDisbType.Text10;
    target.DetCashReceipt.ReceivedDate = source.DisbCashReceipt.ReceivedDate;
    target.DetCrdCrComboNo.CrdCrCombo = source.DisbGrpRefNbr.CrdCrCombo;
    target.DisbDate.DisbDate = source.DisbLocalFinanceWorkArea.DisbDate;
    target.DetDisbursementTransactionType.Code =
      source.DisbDisbursementTransactionType.Code;
    target.DetDisbursementType.Code = source.DisbDisbursementType.Code;
    target.DetPaymentRequest.Assign(source.DisbPaymentRequest);
    target.CreditDet.Amount = source.DisbGrpCr.Amount;
    target.DebitDet.Amount = source.DisbGrpDb.Amount;
    MoveDisbursementTransaction1(source.DisbDisbursementTransaction,
      target.DetDisbursementTransaction);
  }

  private static void MoveDisbursementTransaction1(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
  }

  private static void MoveDisbursementTransaction2(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessDate = source.ProcessDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveGroupToDisb(FnDisbSortSumGrpViewVer2.Export.
    GroupGroup source, Local.DisbGroup target)
  {
    target.DisbGrpDisbType.Text10 = source.DisbType.Text10;
    target.DisbCashReceipt.ReceivedDate = source.DetCashReceipt.ReceivedDate;
    target.DisbGrpRefNbr.CrdCrCombo = source.DetCrdCrComboNo.CrdCrCombo;
    target.DisbLocalFinanceWorkArea.DisbDate = source.DisbDate.DisbDate;
    target.DisbDisbursementTransactionType.Code =
      source.DetDisbursementTransactionType.Code;
    target.DisbDisbursementType.Code = source.DetDisbursementType.Code;
    target.DisbPaymentRequest.Assign(source.DetPaymentRequest);
    target.DisbGrpCr.Amount = source.CreditDet.Amount;
    target.DisbGrpDb.Amount = source.DebitDet.Amount;
    MoveDisbursementTransaction1(source.DetDisbursementTransaction,
      target.DisbDisbursementTransaction);
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.StartingDate.Date = useExport.First.Date;
    export.EndingDate.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.StartingDate.Date = useExport.First.Date;
  }

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnDisbSortSumGrpViewVer2()
  {
    var useImport = new FnDisbSortSumGrpViewVer2.Import();
    var useExport = new FnDisbSortSumGrpViewVer2.Export();

    local.Disb.CopyTo(useImport.Group, MoveDisbToGroup);

    Call(FnDisbSortSumGrpViewVer2.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Disb, MoveGroupToDisb);
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

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          local.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.Cr.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.
          SetInt32(command, "creventId", entities.Cr.CrvId.GetValueOrDefault());
          
        db.SetInt32(
          command, "cstIdentifier", entities.Cr.CstId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          local.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.Populated = true;
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.TextWorkArea.Text10);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CsePerson.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadCsePersonObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Obligation.CspNumber = db.GetString(reader, 3);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 5);
        entities.CsePerson.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadDisbursement()
  {
    entities.Disbursement.Populated = false;

    return Read("ReadDisbursement",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          local.PrevTempDisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Disbursement.Type1 = db.GetString(reader, 3);
        entities.Disbursement.Amount = db.GetDecimal(reader, 4);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Disbursement.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Disbursement.DisbursementDate = db.GetNullableDate(reader, 7);
        entities.Disbursement.CashNonCashInd = db.GetString(reader, 8);
        entities.Disbursement.RecapturedInd = db.GetString(reader, 9);
        entities.Disbursement.CollectionDate = db.GetNullableDate(reader, 10);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 11);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 12);
        entities.Disbursement.ReferenceNumber =
          db.GetNullableString(reader, 13);
        entities.Disbursement.ExcessUraInd = db.GetNullableString(reader, 14);
        entities.Disbursement.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Disbursement.Type1);
          
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Disbursement.CashNonCashInd);
      });
  }

  private bool ReadDisbursementStatusDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.DisbursementStatus.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.Disbursement.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Disbursement.CspNumber);
        db.SetString(command, "cpaType", entities.Disbursement.CpaType);
        db.SetNullableDate(
          command, "discontinueDate",
          local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 4);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 6);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.DisbursementStatus.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.Cr.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
        db.SetDate(
          command, "date1", export.StartingDate.Date.GetValueOrDefault());
        db.
          SetDate(command, "date2", export.EndingDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Cr.CpaType = db.GetString(reader, 0);
        entities.Cr.CspNumber = db.GetString(reader, 1);
        entities.Cr.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Cr.Type1 = db.GetString(reader, 3);
        entities.Cr.Amount = db.GetDecimal(reader, 4);
        entities.Cr.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Cr.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Cr.CollectionDate = db.GetNullableDate(reader, 7);
        entities.Cr.OtyId = db.GetNullableInt32(reader, 8);
        entities.Cr.OtrTypeDisb = db.GetNullableString(reader, 9);
        entities.Cr.OtrId = db.GetNullableInt32(reader, 10);
        entities.Cr.CpaTypeDisb = db.GetNullableString(reader, 11);
        entities.Cr.CspNumberDisb = db.GetNullableString(reader, 12);
        entities.Cr.ObgId = db.GetNullableInt32(reader, 13);
        entities.Cr.CrdId = db.GetNullableInt32(reader, 14);
        entities.Cr.CrvId = db.GetNullableInt32(reader, 15);
        entities.Cr.CstId = db.GetNullableInt32(reader, 16);
        entities.Cr.CrtId = db.GetNullableInt32(reader, 17);
        entities.Cr.ColId = db.GetNullableInt32(reader, 18);
        entities.Cr.ReferenceNumber = db.GetNullableString(reader, 19);
        entities.Cr.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Cr.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Cr.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Cr.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Cr.CpaTypeDisb);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionRlnDisbursement()
  {
    System.Diagnostics.Debug.Assert(entities.Cr.Populated);
    entities.DisbursementTransactionRln.Populated = false;
    entities.Disbursement.Populated = false;

    return ReadEach("ReadDisbursementTransactionRlnDisbursement",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId", entities.Cr.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Cr.CpaType);
        db.SetString(command, "cspPNumber", entities.Cr.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionRln.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DisbursementTransactionRln.CspNumber = db.GetString(reader, 3);
        entities.Disbursement.CspNumber = db.GetString(reader, 3);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 4);
        entities.Disbursement.CpaType = db.GetString(reader, 4);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 5);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 6);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 7);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 8);
        entities.Disbursement.Type1 = db.GetString(reader, 9);
        entities.Disbursement.Amount = db.GetDecimal(reader, 10);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 11);
        entities.Disbursement.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.Disbursement.DisbursementDate = db.GetNullableDate(reader, 13);
        entities.Disbursement.CashNonCashInd = db.GetString(reader, 14);
        entities.Disbursement.RecapturedInd = db.GetString(reader, 15);
        entities.Disbursement.CollectionDate = db.GetNullableDate(reader, 16);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 17);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 18);
        entities.Disbursement.ReferenceNumber =
          db.GetNullableString(reader, 19);
        entities.Disbursement.ExcessUraInd = db.GetNullableString(reader, 20);
        entities.DisbursementTransactionRln.Populated = true;
        entities.Disbursement.Populated = true;
        CheckValid<DisbursementTransactionRln>("CpaType",
          entities.DisbursementTransactionRln.CpaType);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransactionRln>("CpaPType",
          entities.DisbursementTransactionRln.CpaPType);
        CheckValid<DisbursementTransaction>("Type1", entities.Disbursement.Type1);
          
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Disbursement.CashNonCashInd);

        return true;
      });
  }

  private bool ReadDisbursementType()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          entities.Disbursement.DbtGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 0);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 1);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligee.CspNumber = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 1);
        entities.Obligee.CreatedTmst = db.GetDateTime(reader, 2);
        entities.Obligee.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee.Type1);
      });
  }

  private bool ReadPaymentRequest1()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          export.Group.Item.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 2);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 3);
        entities.PaymentRequest.Type1 = db.GetString(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.Disbursement.PrqGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 2);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 3);
        entities.PaymentRequest.Type1 = db.GetString(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.CurrentDate.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
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
      /// A value of RefNo.
      /// </summary>
      [JsonPropertyName("refNo")]
      public CrdCrComboNo RefNo
      {
        get => refNo ??= new();
        set => refNo = value;
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
      /// A value of LocalFinanceWorkArea.
      /// </summary>
      [JsonPropertyName("localFinanceWorkArea")]
      public LocalFinanceWorkArea LocalFinanceWorkArea
      {
        get => localFinanceWorkArea ??= new();
        set => localFinanceWorkArea = value;
      }

      /// <summary>
      /// A value of DisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("disbursementTransactionType")]
      public DisbursementTransactionType DisbursementTransactionType
      {
        get => disbursementTransactionType ??= new();
        set => disbursementTransactionType = value;
      }

      /// <summary>
      /// A value of DisbursementType.
      /// </summary>
      [JsonPropertyName("disbursementType")]
      public DisbursementType DisbursementType
      {
        get => disbursementType ??= new();
        set => disbursementType = value;
      }

      /// <summary>
      /// A value of PaymentRequest.
      /// </summary>
      [JsonPropertyName("paymentRequest")]
      public PaymentRequest PaymentRequest
      {
        get => paymentRequest ??= new();
        set => paymentRequest = value;
      }

      /// <summary>
      /// A value of Cr.
      /// </summary>
      [JsonPropertyName("cr")]
      public DisbursementTransaction Cr
      {
        get => cr ??= new();
        set => cr = value;
      }

      /// <summary>
      /// A value of Db.
      /// </summary>
      [JsonPropertyName("db")]
      public DisbursementTransaction Db
      {
        get => db ??= new();
        set => db = value;
      }

      /// <summary>
      /// A value of DisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbursementTransaction")]
      public DisbursementTransaction DisbursementTransaction
      {
        get => disbursementTransaction ??= new();
        set => disbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private TextWorkArea disbType;
      private CashReceipt cashReceipt;
      private CrdCrComboNo refNo;
      private Common common;
      private LocalFinanceWorkArea localFinanceWorkArea;
      private DisbursementTransactionType disbursementTransactionType;
      private DisbursementType disbursementType;
      private PaymentRequest paymentRequest;
      private DisbursementTransaction cr;
      private DisbursementTransaction db;
      private DisbursementTransaction disbursementTransaction;
    }

    /// <summary>
    /// A value of HiddenEndingDate.
    /// </summary>
    [JsonPropertyName("hiddenEndingDate")]
    public DateWorkArea HiddenEndingDate
    {
      get => hiddenEndingDate ??= new();
      set => hiddenEndingDate = value;
    }

    /// <summary>
    /// A value of EndingDate.
    /// </summary>
    [JsonPropertyName("endingDate")]
    public DateWorkArea EndingDate
    {
      get => endingDate ??= new();
      set => endingDate = value;
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
    /// A value of Received.
    /// </summary>
    [JsonPropertyName("received")]
    public CsePersonsWorkSet Received
    {
      get => received ??= new();
      set => received = value;
    }

    /// <summary>
    /// A value of StartingDate.
    /// </summary>
    [JsonPropertyName("startingDate")]
    public DateWorkArea StartingDate
    {
      get => startingDate ??= new();
      set => startingDate = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of ShowAdj.
    /// </summary>
    [JsonPropertyName("showAdj")]
    public Common ShowAdj
    {
      get => showAdj ??= new();
      set => showAdj = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenStartingDate.
    /// </summary>
    [JsonPropertyName("hiddenStartingDate")]
    public DateWorkArea HiddenStartingDate
    {
      get => hiddenStartingDate ??= new();
      set => hiddenStartingDate = value;
    }

    /// <summary>
    /// A value of HiddenShowAdj.
    /// </summary>
    [JsonPropertyName("hiddenShowAdj")]
    public Common HiddenShowAdj
    {
      get => hiddenShowAdj ??= new();
      set => hiddenShowAdj = value;
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
    /// A value of ZdelImportHidden.
    /// </summary>
    [JsonPropertyName("zdelImportHidden")]
    public Security2 ZdelImportHidden
    {
      get => zdelImportHidden ??= new();
      set => zdelImportHidden = value;
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

    private DateWorkArea hiddenEndingDate;
    private DateWorkArea endingDate;
    private CsePersonsWorkSet selected;
    private CsePersonsWorkSet received;
    private DateWorkArea startingDate;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common prompt;
    private Common showAdj;
    private Array<GroupGroup> group;
    private CsePerson hiddenCsePerson;
    private DateWorkArea hiddenStartingDate;
    private Common hiddenShowAdj;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 zdelImportHidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
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
      /// A value of RefNo.
      /// </summary>
      [JsonPropertyName("refNo")]
      public CrdCrComboNo RefNo
      {
        get => refNo ??= new();
        set => refNo = value;
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
      /// A value of LocalFinanceWorkArea.
      /// </summary>
      [JsonPropertyName("localFinanceWorkArea")]
      public LocalFinanceWorkArea LocalFinanceWorkArea
      {
        get => localFinanceWorkArea ??= new();
        set => localFinanceWorkArea = value;
      }

      /// <summary>
      /// A value of DisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("disbursementTransactionType")]
      public DisbursementTransactionType DisbursementTransactionType
      {
        get => disbursementTransactionType ??= new();
        set => disbursementTransactionType = value;
      }

      /// <summary>
      /// A value of DisbursementType.
      /// </summary>
      [JsonPropertyName("disbursementType")]
      public DisbursementType DisbursementType
      {
        get => disbursementType ??= new();
        set => disbursementType = value;
      }

      /// <summary>
      /// A value of PaymentRequest.
      /// </summary>
      [JsonPropertyName("paymentRequest")]
      public PaymentRequest PaymentRequest
      {
        get => paymentRequest ??= new();
        set => paymentRequest = value;
      }

      /// <summary>
      /// A value of Cr.
      /// </summary>
      [JsonPropertyName("cr")]
      public DisbursementTransaction Cr
      {
        get => cr ??= new();
        set => cr = value;
      }

      /// <summary>
      /// A value of Db.
      /// </summary>
      [JsonPropertyName("db")]
      public DisbursementTransaction Db
      {
        get => db ??= new();
        set => db = value;
      }

      /// <summary>
      /// A value of DisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbursementTransaction")]
      public DisbursementTransaction DisbursementTransaction
      {
        get => disbursementTransaction ??= new();
        set => disbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private TextWorkArea disbType;
      private CashReceipt cashReceipt;
      private CrdCrComboNo refNo;
      private Common common;
      private LocalFinanceWorkArea localFinanceWorkArea;
      private DisbursementTransactionType disbursementTransactionType;
      private DisbursementType disbursementType;
      private PaymentRequest paymentRequest;
      private DisbursementTransaction cr;
      private DisbursementTransaction db;
      private DisbursementTransaction disbursementTransaction;
    }

    /// <summary>
    /// A value of OrelToDate.
    /// </summary>
    [JsonPropertyName("orelToDate")]
    public PaymentRequest OrelToDate
    {
      get => orelToDate ??= new();
      set => orelToDate = value;
    }

    /// <summary>
    /// A value of OrelSearchStatus.
    /// </summary>
    [JsonPropertyName("orelSearchStatus")]
    public Common OrelSearchStatus
    {
      get => orelSearchStatus ??= new();
      set => orelSearchStatus = value;
    }

    /// <summary>
    /// A value of OrelFromDate.
    /// </summary>
    [JsonPropertyName("orelFromDate")]
    public PaymentRequest OrelFromDate
    {
      get => orelFromDate ??= new();
      set => orelFromDate = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of HiddenEndingDate.
    /// </summary>
    [JsonPropertyName("hiddenEndingDate")]
    public DateWorkArea HiddenEndingDate
    {
      get => hiddenEndingDate ??= new();
      set => hiddenEndingDate = value;
    }

    /// <summary>
    /// A value of EndingDate.
    /// </summary>
    [JsonPropertyName("endingDate")]
    public DateWorkArea EndingDate
    {
      get => endingDate ??= new();
      set => endingDate = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of SendToDhst.
    /// </summary>
    [JsonPropertyName("sendToDhst")]
    public DateWorkArea SendToDhst
    {
      get => sendToDhst ??= new();
      set => sendToDhst = value;
    }

    /// <summary>
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public CsePerson Payor
    {
      get => payor ??= new();
      set => payor = value;
    }

    /// <summary>
    /// A value of FlowObligationType.
    /// </summary>
    [JsonPropertyName("flowObligationType")]
    public ObligationType FlowObligationType
    {
      get => flowObligationType ??= new();
      set => flowObligationType = value;
    }

    /// <summary>
    /// A value of FlowDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("flowDisbursementTransaction")]
    public DisbursementTransaction FlowDisbursementTransaction
    {
      get => flowDisbursementTransaction ??= new();
      set => flowDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of FlowCsePersonAccount.
    /// </summary>
    [JsonPropertyName("flowCsePersonAccount")]
    public CsePersonAccount FlowCsePersonAccount
    {
      get => flowCsePersonAccount ??= new();
      set => flowCsePersonAccount = value;
    }

    /// <summary>
    /// A value of FlowCollectionType.
    /// </summary>
    [JsonPropertyName("flowCollectionType")]
    public CollectionType FlowCollectionType
    {
      get => flowCollectionType ??= new();
      set => flowCollectionType = value;
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
    /// A value of FlowCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("flowCashReceiptDetail")]
    public CashReceiptDetail FlowCashReceiptDetail
    {
      get => flowCashReceiptDetail ??= new();
      set => flowCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of FlowObligation.
    /// </summary>
    [JsonPropertyName("flowObligation")]
    public Obligation FlowObligation
    {
      get => flowObligation ??= new();
      set => flowObligation = value;
    }

    /// <summary>
    /// A value of FlowPaymentRequest.
    /// </summary>
    [JsonPropertyName("flowPaymentRequest")]
    public PaymentRequest FlowPaymentRequest
    {
      get => flowPaymentRequest ??= new();
      set => flowPaymentRequest = value;
    }

    /// <summary>
    /// A value of StartingDate.
    /// </summary>
    [JsonPropertyName("startingDate")]
    public DateWorkArea StartingDate
    {
      get => startingDate ??= new();
      set => startingDate = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of HiddenShowAdj.
    /// </summary>
    [JsonPropertyName("hiddenShowAdj")]
    public Common HiddenShowAdj
    {
      get => hiddenShowAdj ??= new();
      set => hiddenShowAdj = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenStartingDateIn.
    /// </summary>
    [JsonPropertyName("hiddenStartingDateIn")]
    public DateWorkArea HiddenStartingDateIn
    {
      get => hiddenStartingDateIn ??= new();
      set => hiddenStartingDateIn = value;
    }

    /// <summary>
    /// A value of ShowAdj.
    /// </summary>
    [JsonPropertyName("showAdj")]
    public Common ShowAdj
    {
      get => showAdj ??= new();
      set => showAdj = value;
    }

    /// <summary>
    /// A value of ZdelExportPayee.
    /// </summary>
    [JsonPropertyName("zdelExportPayee")]
    public CsePersonsWorkSet ZdelExportPayee
    {
      get => zdelExportPayee ??= new();
      set => zdelExportPayee = value;
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
    /// A value of ZdelExportHidden.
    /// </summary>
    [JsonPropertyName("zdelExportHidden")]
    public Security2 ZdelExportHidden
    {
      get => zdelExportHidden ??= new();
      set => zdelExportHidden = value;
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

    private PaymentRequest orelToDate;
    private Common orelSearchStatus;
    private PaymentRequest orelFromDate;
    private ElectronicFundTransmission electronicFundTransmission;
    private DateWorkArea hiddenEndingDate;
    private DateWorkArea endingDate;
    private Common phonetic;
    private DateWorkArea sendToDhst;
    private CsePerson payor;
    private ObligationType flowObligationType;
    private DisbursementTransaction flowDisbursementTransaction;
    private CsePersonAccount flowCsePersonAccount;
    private CollectionType flowCollectionType;
    private CashReceiptType flowCashReceiptType;
    private CashReceiptSourceType flowCashReceiptSourceType;
    private CashReceiptEvent flowCashReceiptEvent;
    private CashReceiptDetail flowCashReceiptDetail;
    private Obligation flowObligation;
    private PaymentRequest flowPaymentRequest;
    private DateWorkArea startingDate;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common prompt;
    private Common hiddenShowAdj;
    private Array<GroupGroup> group;
    private CsePerson hiddenCsePerson;
    private DateWorkArea hiddenStartingDateIn;
    private Common showAdj;
    private CsePersonsWorkSet zdelExportPayee;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 zdelExportHidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CollGroup group.</summary>
    [Serializable]
    public class CollGroup
    {
      /// <summary>
      /// A value of CollGrpDisbType.
      /// </summary>
      [JsonPropertyName("collGrpDisbType")]
      public TextWorkArea CollGrpDisbType
      {
        get => collGrpDisbType ??= new();
        set => collGrpDisbType = value;
      }

      /// <summary>
      /// A value of CollCashReceipt.
      /// </summary>
      [JsonPropertyName("collCashReceipt")]
      public CashReceipt CollCashReceipt
      {
        get => collCashReceipt ??= new();
        set => collCashReceipt = value;
      }

      /// <summary>
      /// A value of CollGrpRefNbr.
      /// </summary>
      [JsonPropertyName("collGrpRefNbr")]
      public CrdCrComboNo CollGrpRefNbr
      {
        get => collGrpRefNbr ??= new();
        set => collGrpRefNbr = value;
      }

      /// <summary>
      /// A value of CollLocalFinanceWorkArea.
      /// </summary>
      [JsonPropertyName("collLocalFinanceWorkArea")]
      public LocalFinanceWorkArea CollLocalFinanceWorkArea
      {
        get => collLocalFinanceWorkArea ??= new();
        set => collLocalFinanceWorkArea = value;
      }

      /// <summary>
      /// A value of CollDisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("collDisbursementTransactionType")]
      public DisbursementTransactionType CollDisbursementTransactionType
      {
        get => collDisbursementTransactionType ??= new();
        set => collDisbursementTransactionType = value;
      }

      /// <summary>
      /// A value of CollDisbursementType.
      /// </summary>
      [JsonPropertyName("collDisbursementType")]
      public DisbursementType CollDisbursementType
      {
        get => collDisbursementType ??= new();
        set => collDisbursementType = value;
      }

      /// <summary>
      /// A value of CollPaymentRequest.
      /// </summary>
      [JsonPropertyName("collPaymentRequest")]
      public PaymentRequest CollPaymentRequest
      {
        get => collPaymentRequest ??= new();
        set => collPaymentRequest = value;
      }

      /// <summary>
      /// A value of CollGrpCr.
      /// </summary>
      [JsonPropertyName("collGrpCr")]
      public DisbursementTransaction CollGrpCr
      {
        get => collGrpCr ??= new();
        set => collGrpCr = value;
      }

      /// <summary>
      /// A value of CollGrpDb.
      /// </summary>
      [JsonPropertyName("collGrpDb")]
      public DisbursementTransaction CollGrpDb
      {
        get => collGrpDb ??= new();
        set => collGrpDb = value;
      }

      /// <summary>
      /// A value of CollDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("collDisbursementTransaction")]
      public DisbursementTransaction CollDisbursementTransaction
      {
        get => collDisbursementTransaction ??= new();
        set => collDisbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private TextWorkArea collGrpDisbType;
      private CashReceipt collCashReceipt;
      private CrdCrComboNo collGrpRefNbr;
      private LocalFinanceWorkArea collLocalFinanceWorkArea;
      private DisbursementTransactionType collDisbursementTransactionType;
      private DisbursementType collDisbursementType;
      private PaymentRequest collPaymentRequest;
      private DisbursementTransaction collGrpCr;
      private DisbursementTransaction collGrpDb;
      private DisbursementTransaction collDisbursementTransaction;
    }

    /// <summary>A DisbGroup group.</summary>
    [Serializable]
    public class DisbGroup
    {
      /// <summary>
      /// A value of DisbGrpDisbType.
      /// </summary>
      [JsonPropertyName("disbGrpDisbType")]
      public TextWorkArea DisbGrpDisbType
      {
        get => disbGrpDisbType ??= new();
        set => disbGrpDisbType = value;
      }

      /// <summary>
      /// A value of DisbCashReceipt.
      /// </summary>
      [JsonPropertyName("disbCashReceipt")]
      public CashReceipt DisbCashReceipt
      {
        get => disbCashReceipt ??= new();
        set => disbCashReceipt = value;
      }

      /// <summary>
      /// A value of DisbGrpRefNbr.
      /// </summary>
      [JsonPropertyName("disbGrpRefNbr")]
      public CrdCrComboNo DisbGrpRefNbr
      {
        get => disbGrpRefNbr ??= new();
        set => disbGrpRefNbr = value;
      }

      /// <summary>
      /// A value of DisbLocalFinanceWorkArea.
      /// </summary>
      [JsonPropertyName("disbLocalFinanceWorkArea")]
      public LocalFinanceWorkArea DisbLocalFinanceWorkArea
      {
        get => disbLocalFinanceWorkArea ??= new();
        set => disbLocalFinanceWorkArea = value;
      }

      /// <summary>
      /// A value of DisbDisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("disbDisbursementTransactionType")]
      public DisbursementTransactionType DisbDisbursementTransactionType
      {
        get => disbDisbursementTransactionType ??= new();
        set => disbDisbursementTransactionType = value;
      }

      /// <summary>
      /// A value of DisbDisbursementType.
      /// </summary>
      [JsonPropertyName("disbDisbursementType")]
      public DisbursementType DisbDisbursementType
      {
        get => disbDisbursementType ??= new();
        set => disbDisbursementType = value;
      }

      /// <summary>
      /// A value of DisbPaymentRequest.
      /// </summary>
      [JsonPropertyName("disbPaymentRequest")]
      public PaymentRequest DisbPaymentRequest
      {
        get => disbPaymentRequest ??= new();
        set => disbPaymentRequest = value;
      }

      /// <summary>
      /// A value of DisbGrpCr.
      /// </summary>
      [JsonPropertyName("disbGrpCr")]
      public DisbursementTransaction DisbGrpCr
      {
        get => disbGrpCr ??= new();
        set => disbGrpCr = value;
      }

      /// <summary>
      /// A value of DisbGrpDb.
      /// </summary>
      [JsonPropertyName("disbGrpDb")]
      public DisbursementTransaction DisbGrpDb
      {
        get => disbGrpDb ??= new();
        set => disbGrpDb = value;
      }

      /// <summary>
      /// A value of DisbDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbDisbursementTransaction")]
      public DisbursementTransaction DisbDisbursementTransaction
      {
        get => disbDisbursementTransaction ??= new();
        set => disbDisbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private TextWorkArea disbGrpDisbType;
      private CashReceipt disbCashReceipt;
      private CrdCrComboNo disbGrpRefNbr;
      private LocalFinanceWorkArea disbLocalFinanceWorkArea;
      private DisbursementTransactionType disbDisbursementTransactionType;
      private DisbursementType disbDisbursementType;
      private PaymentRequest disbPaymentRequest;
      private DisbursementTransaction disbGrpCr;
      private DisbursementTransaction disbGrpDb;
      private DisbursementTransaction disbDisbursementTransaction;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
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
      /// A value of RefNbr.
      /// </summary>
      [JsonPropertyName("refNbr")]
      public CrdCrComboNo RefNbr
      {
        get => refNbr ??= new();
        set => refNbr = value;
      }

      /// <summary>
      /// A value of LocalFinanceWorkArea.
      /// </summary>
      [JsonPropertyName("localFinanceWorkArea")]
      public LocalFinanceWorkArea LocalFinanceWorkArea
      {
        get => localFinanceWorkArea ??= new();
        set => localFinanceWorkArea = value;
      }

      /// <summary>
      /// A value of DisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("disbursementTransactionType")]
      public DisbursementTransactionType DisbursementTransactionType
      {
        get => disbursementTransactionType ??= new();
        set => disbursementTransactionType = value;
      }

      /// <summary>
      /// A value of DisbursementType.
      /// </summary>
      [JsonPropertyName("disbursementType")]
      public DisbursementType DisbursementType
      {
        get => disbursementType ??= new();
        set => disbursementType = value;
      }

      /// <summary>
      /// A value of PaymentRequest.
      /// </summary>
      [JsonPropertyName("paymentRequest")]
      public PaymentRequest PaymentRequest
      {
        get => paymentRequest ??= new();
        set => paymentRequest = value;
      }

      /// <summary>
      /// A value of Cr.
      /// </summary>
      [JsonPropertyName("cr")]
      public DisbursementTransaction Cr
      {
        get => cr ??= new();
        set => cr = value;
      }

      /// <summary>
      /// A value of Db.
      /// </summary>
      [JsonPropertyName("db")]
      public DisbursementTransaction Db
      {
        get => db ??= new();
        set => db = value;
      }

      /// <summary>
      /// A value of DisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbursementTransaction")]
      public DisbursementTransaction DisbursementTransaction
      {
        get => disbursementTransaction ??= new();
        set => disbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 600;

      private TextWorkArea disbType;
      private CashReceipt cashReceipt;
      private CrdCrComboNo refNbr;
      private LocalFinanceWorkArea localFinanceWorkArea;
      private DisbursementTransactionType disbursementTransactionType;
      private DisbursementType disbursementType;
      private PaymentRequest paymentRequest;
      private DisbursementTransaction cr;
      private DisbursementTransaction db;
      private DisbursementTransaction disbursementTransaction;
    }

    /// <summary>
    /// A value of GvOverflow.
    /// </summary>
    [JsonPropertyName("gvOverflow")]
    public Common GvOverflow
    {
      get => gvOverflow ??= new();
      set => gvOverflow = value;
    }

    /// <summary>
    /// A value of PrevTempDisbType.
    /// </summary>
    [JsonPropertyName("prevTempDisbType")]
    public TextWorkArea PrevTempDisbType
    {
      get => prevTempDisbType ??= new();
      set => prevTempDisbType = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of NegativeDebit.
    /// </summary>
    [JsonPropertyName("negativeDebit")]
    public LocalFinanceWorkArea NegativeDebit
    {
      get => negativeDebit ??= new();
      set => negativeDebit = value;
    }

    /// <summary>
    /// A value of StateOfKs.
    /// </summary>
    [JsonPropertyName("stateOfKs")]
    public CsePerson StateOfKs
    {
      get => stateOfKs ??= new();
      set => stateOfKs = value;
    }

    /// <summary>
    /// A value of EndTs.
    /// </summary>
    [JsonPropertyName("endTs")]
    public DateWorkArea EndTs
    {
      get => endTs ??= new();
      set => endTs = value;
    }

    /// <summary>
    /// A value of StartTs.
    /// </summary>
    [JsonPropertyName("startTs")]
    public DateWorkArea StartTs
    {
      get => startTs ??= new();
      set => startTs = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of SendDhst.
    /// </summary>
    [JsonPropertyName("sendDhst")]
    public LocalFinanceWorkArea SendDhst
    {
      get => sendDhst ??= new();
      set => sendDhst = value;
    }

    /// <summary>
    /// A value of PositiveCredit.
    /// </summary>
    [JsonPropertyName("positiveCredit")]
    public LocalFinanceWorkArea PositiveCredit
    {
      get => positiveCredit ??= new();
      set => positiveCredit = value;
    }

    /// <summary>
    /// A value of NegativeCredit.
    /// </summary>
    [JsonPropertyName("negativeCredit")]
    public LocalFinanceWorkArea NegativeCredit
    {
      get => negativeCredit ??= new();
      set => negativeCredit = value;
    }

    /// <summary>
    /// A value of RefNumber.
    /// </summary>
    [JsonPropertyName("refNumber")]
    public CrdCrComboNo RefNumber
    {
      get => refNumber ??= new();
      set => refNumber = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// Gets a value of Coll.
    /// </summary>
    [JsonIgnore]
    public Array<CollGroup> Coll => coll ??= new(CollGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Coll for json serialization.
    /// </summary>
    [JsonPropertyName("coll")]
    [Computed]
    public IList<CollGroup> Coll_Json
    {
      get => coll;
      set => Coll.Assign(value);
    }

    /// <summary>
    /// Gets a value of Disb.
    /// </summary>
    [JsonIgnore]
    public Array<DisbGroup> Disb => disb ??= new(DisbGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Disb for json serialization.
    /// </summary>
    [JsonPropertyName("disb")]
    [Computed]
    public IList<DisbGroup> Disb_Json
    {
      get => disb;
      set => Disb.Assign(value);
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of LastSubscriptDisbcoll.
    /// </summary>
    [JsonPropertyName("lastSubscriptDisbcoll")]
    public Common LastSubscriptDisbcoll
    {
      get => lastSubscriptDisbcoll ??= new();
      set => lastSubscriptDisbcoll = value;
    }

    /// <summary>
    /// A value of LastSuscriptDisbdet.
    /// </summary>
    [JsonPropertyName("lastSuscriptDisbdet")]
    public Common LastSuscriptDisbdet
    {
      get => lastSuscriptDisbdet ??= new();
      set => lastSuscriptDisbdet = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HardcodedSuppress.
    /// </summary>
    [JsonPropertyName("hardcodedSuppress")]
    public DisbursementStatus HardcodedSuppress
    {
      get => hardcodedSuppress ??= new();
      set => hardcodedSuppress = value;
    }

    /// <summary>
    /// A value of PrevTempCashReceipt.
    /// </summary>
    [JsonPropertyName("prevTempCashReceipt")]
    public CashReceipt PrevTempCashReceipt
    {
      get => prevTempCashReceipt ??= new();
      set => prevTempCashReceipt = value;
    }

    /// <summary>
    /// A value of PrevTempCrdCrComboNo.
    /// </summary>
    [JsonPropertyName("prevTempCrdCrComboNo")]
    public CrdCrComboNo PrevTempCrdCrComboNo
    {
      get => prevTempCrdCrComboNo ??= new();
      set => prevTempCrdCrComboNo = value;
    }

    /// <summary>
    /// A value of PrevTempLocalFinanceWorkArea.
    /// </summary>
    [JsonPropertyName("prevTempLocalFinanceWorkArea")]
    public LocalFinanceWorkArea PrevTempLocalFinanceWorkArea
    {
      get => prevTempLocalFinanceWorkArea ??= new();
      set => prevTempLocalFinanceWorkArea = value;
    }

    /// <summary>
    /// A value of PrevTempDisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("prevTempDisbursementTransactionType")]
    public DisbursementTransactionType PrevTempDisbursementTransactionType
    {
      get => prevTempDisbursementTransactionType ??= new();
      set => prevTempDisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of PrevTempDisbursementType.
    /// </summary>
    [JsonPropertyName("prevTempDisbursementType")]
    public DisbursementType PrevTempDisbursementType
    {
      get => prevTempDisbursementType ??= new();
      set => prevTempDisbursementType = value;
    }

    /// <summary>
    /// A value of PrevTempPaymentRequest.
    /// </summary>
    [JsonPropertyName("prevTempPaymentRequest")]
    public PaymentRequest PrevTempPaymentRequest
    {
      get => prevTempPaymentRequest ??= new();
      set => prevTempPaymentRequest = value;
    }

    /// <summary>
    /// A value of PrevDebit.
    /// </summary>
    [JsonPropertyName("prevDebit")]
    public LocalFinanceWorkArea PrevDebit
    {
      get => prevDebit ??= new();
      set => prevDebit = value;
    }

    /// <summary>
    /// A value of PrevCredit.
    /// </summary>
    [JsonPropertyName("prevCredit")]
    public LocalFinanceWorkArea PrevCredit
    {
      get => prevCredit ??= new();
      set => prevCredit = value;
    }

    /// <summary>
    /// A value of PrevTempDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("prevTempDisbursementTransaction")]
    public DisbursementTransaction PrevTempDisbursementTransaction
    {
      get => prevTempDisbursementTransaction ??= new();
      set => prevTempDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of LdisbStatusHistFoundFlagIn.
    /// </summary>
    [JsonPropertyName("ldisbStatusHistFoundFlagIn")]
    public Common LdisbStatusHistFoundFlagIn
    {
      get => ldisbStatusHistFoundFlagIn ??= new();
      set => ldisbStatusHistFoundFlagIn = value;
    }

    /// <summary>
    /// A value of PaymentReqFoundFlagIn.
    /// </summary>
    [JsonPropertyName("paymentReqFoundFlagIn")]
    public Common PaymentReqFoundFlagIn
    {
      get => paymentReqFoundFlagIn ??= new();
      set => paymentReqFoundFlagIn = value;
    }

    /// <summary>
    /// A value of DisbFoundFlagIn.
    /// </summary>
    [JsonPropertyName("disbFoundFlagIn")]
    public Common DisbFoundFlagIn
    {
      get => disbFoundFlagIn ??= new();
      set => disbFoundFlagIn = value;
    }

    /// <summary>
    /// A value of DisbTranRlnFoundFlagIn.
    /// </summary>
    [JsonPropertyName("disbTranRlnFoundFlagIn")]
    public Common DisbTranRlnFoundFlagIn
    {
      get => disbTranRlnFoundFlagIn ??= new();
      set => disbTranRlnFoundFlagIn = value;
    }

    /// <summary>
    /// A value of DisbTranFoundFlagIn.
    /// </summary>
    [JsonPropertyName("disbTranFoundFlagIn")]
    public Common DisbTranFoundFlagIn
    {
      get => disbTranFoundFlagIn ??= new();
      set => disbTranFoundFlagIn = value;
    }

    /// <summary>
    /// A value of InsertDisbLine.
    /// </summary>
    [JsonPropertyName("insertDisbLine")]
    public Common InsertDisbLine
    {
      get => insertDisbLine ??= new();
      set => insertDisbLine = value;
    }

    /// <summary>
    /// A value of InsertBlank.
    /// </summary>
    [JsonPropertyName("insertBlank")]
    public Common InsertBlank
    {
      get => insertBlank ??= new();
      set => insertBlank = value;
    }

    /// <summary>
    /// A value of FirsttimeFlag.
    /// </summary>
    [JsonPropertyName("firsttimeFlag")]
    public Common FirsttimeFlag
    {
      get => firsttimeFlag ??= new();
      set => firsttimeFlag = value;
    }

    /// <summary>
    /// A value of SortsumFlag.
    /// </summary>
    [JsonPropertyName("sortsumFlag")]
    public Common SortsumFlag
    {
      get => sortsumFlag ??= new();
      set => sortsumFlag = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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

    private Common gvOverflow;
    private TextWorkArea prevTempDisbType;
    private DateWorkArea currentDate;
    private LocalFinanceWorkArea negativeDebit;
    private CsePerson stateOfKs;
    private DateWorkArea endTs;
    private DateWorkArea startTs;
    private DateWorkArea end;
    private DateWorkArea start;
    private LocalFinanceWorkArea sendDhst;
    private LocalFinanceWorkArea positiveCredit;
    private LocalFinanceWorkArea negativeCredit;
    private CrdCrComboNo refNumber;
    private WorkArea workArea;
    private Common select;
    private Array<CollGroup> coll;
    private Array<DisbGroup> disb;
    private Array<GroupGroup> group;
    private Common lastSubscriptDisbcoll;
    private Common lastSuscriptDisbdet;
    private DateWorkArea dateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DisbursementStatus hardcodedSuppress;
    private CashReceipt prevTempCashReceipt;
    private CrdCrComboNo prevTempCrdCrComboNo;
    private LocalFinanceWorkArea prevTempLocalFinanceWorkArea;
    private DisbursementTransactionType prevTempDisbursementTransactionType;
    private DisbursementType prevTempDisbursementType;
    private PaymentRequest prevTempPaymentRequest;
    private LocalFinanceWorkArea prevDebit;
    private LocalFinanceWorkArea prevCredit;
    private DisbursementTransaction prevTempDisbursementTransaction;
    private Common ldisbStatusHistFoundFlagIn;
    private Common paymentReqFoundFlagIn;
    private Common disbFoundFlagIn;
    private Common disbTranRlnFoundFlagIn;
    private Common disbTranFoundFlagIn;
    private Common insertDisbLine;
    private Common insertBlank;
    private Common firsttimeFlag;
    private Common sortsumFlag;
    private DisbursementTransaction disbursementTransaction;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of Cr.
    /// </summary>
    [JsonPropertyName("cr")]
    public DisbursementTransaction Cr
    {
      get => cr ??= new();
      set => cr = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private ElectronicFundTransmission electronicFundTransmission;
    private DisbursementTransaction credit;
    private ObligationType obligationType;
    private CashReceipt cashReceipt;
    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private DisbursementStatus disbursementStatus;
    private DisbursementStatusHistory disbursementStatusHistory;
    private CsePersonAccount obligee;
    private CsePerson csePerson;
    private DisbursementType disbursementType;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbursement;
    private DisbursementTransaction cr;
    private PaymentRequest paymentRequest;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private ObligationTransaction obligationTransaction;
  }
#endregion
}

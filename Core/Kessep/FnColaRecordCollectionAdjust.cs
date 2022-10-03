// Program: FN_COLA_RECORD_COLLECTION_ADJUST, ID: 372378820, model: 746.
// Short name: SWECOLAP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_COLA_RECORD_COLLECTION_ADJUST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnColaRecordCollectionAdjust: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COLA_RECORD_COLLECTION_ADJUST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnColaRecordCollectionAdjust(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnColaRecordCollectionAdjust.
  /// </summary>
  public FnColaRecordCollectionAdjust(IContext context, Import import,
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
    // ---------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // 01/03/97	R. Marchman	Add new security/next tran.
    // 03/28/97        C. Dasgupta     following changes/corrections have been 
    // made
    // 1.Changed Cash Receipt Number to enterable field
    // 2.Added PF2 for display information against Cash Receipt
    // 3.Changed adjustment Code to Adjustment Reason Code
    // 4.Changes in view / dialog flow from CAJR and screen have been made to 
    // display full name for the reason code
    // 5.Length of the cash receipt # has been increased to 12 bytes
    // 6.Dialog flow from PAYR has been corrected to display amount owed 
    // information
    // 7.Dialog flow has been corrected and code and code is added to correct 
    // the problem of wrong error message display , when coming back from CAJR
    // 8.Code has been changed to fix a bug related to LIST when when something 
    // other than S is entered in the list prompt field
    // 9.Action block FN_READ_CSE_PERSON is created
    // 10.Action block FN_READ_CR_DETAIL_EVENT_TYPE is created
    // 11.Status field is added in the screen for display (Code to populate 
    // status has been added in the FN_READ_COLLECTION action block )
    // 04/10/97       C.Dasgupta
    // Correction made in  FN_MARK_ALL_COLLECTIONS_ADJ to update all collections
    // related to a cash receipt detail instead of one collection
    // 06/11/1997   A Samuels		The following:
    //   1) Added additional req'd search field 'Collection ID #'.
    //   2) Added 3 new fields for add processing: Court Order #, CSE Person #, 
    // & SSN.
    //   3) Implemented appropriate edits and logic for above changes.
    // ---------------------------------------------
    // ------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // 021398	govind		Fixed to get the SSN and pass it to subsequent acblks
    // 10/23/1998 N.Engoor fixed the flow problem while coming back from CAJR 
    // without a selection being made.
    // 11/02/1998   N.Engoor
    // Removed the fields SSN, Court order number and the Obligor person number 
    // from the screen
    // 11/10/1998 N.Engoor
    // If COLA is flowed to from any screen other than COLL and DEBT the view 
    // Import debt coll flag is spaces. In this case all the details that would
    // be shown on the screen would be the most current details of the cash
    // receipt detail.
    // If COLA is flowed to from COLL or DEBT the view Import debt coll flag is 
    // passsed with a value of 'Y'. In this case the amount that would be shown
    // in the distributed amount field would be the collection amount as of
    // then. Rest all information on the screen would be the most current
    // details of the cash receipt detail.
    // 04/02/2001   M. Brown  PR# 111813
    // Next tran modifications.
    // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
    // ------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    // If command is CLEAR or Cancel, escape before moving
    // Imports to Exports so that the screen is
    // blanked out. All fields are cleared.
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }
    else if (IsEmpty(import.CollectionAdjustmentReason.Code))
    {
      MoveCollectionAdjustmentReason2(import.Prev,
        export.CollectionAdjustmentReason);
    }
    else
    {
      export.CollectionAdjustmentReason.
        Assign(import.CollectionAdjustmentReason);
    }

    // Move all other IMPORTs to EXPORTs.
    // ----------------------------------------
    // Naveen - 11/16/1998
    // Added a new field on the screen - Dist Collection Amt
    // This field will display the collection amount when flowing from either 
    // COLL or DEBT.
    // ---------------------------------------
    export.ManualPostedCollExists.SelectChar =
      import.ManualPostedCollExists.SelectChar;
    MoveCollection1(import.DebtCollCollection, export.DebCollCollection);
    export.DebCollCommon.Flag = import.DebtCollCommon.Flag;
    export.Debcoll.Amount = import.Debcoll.Amount;
    export.AmtPrompt.Text1 = import.AmtPrompt.Text1;
    export.Entered.SequentialNumber = import.Entered.SequentialNumber;
    export.CashReceiptDetail.Assign(import.CashReceiptDetail);
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptSourceType(import.CashReceiptSourceType,
      export.CashReceiptSourceType);
    MoveCashReceiptType(import.CashReceiptType, export.CashReceiptType);
    MoveCollectionType(import.CollectionType, export.CollectionType);
    export.Collection.Assign(import.Collection);
    export.CsePerson.Number = import.CsePerson.Number;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.FirstTimeFlag.Flag = import.FirstTimeFlag.Flag;
    export.ListPrompt.Flag = import.ListPrompt.Flag;
    export.UndistributedAmount.TotalCurrency =
      import.UndistributedAmount.TotalCurrency;
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.Hidden);
    export.CashReceiptDetailStatus.Code = import.CashReceiptDetailStatus.Code;
    export.CollProtExists.Flag = import.CollProtExists.Flag;
    export.UserConfirmedAdj.Flag = import.UserConfirmedAdj.Flag;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "RETCAJR"))
    {
      if (IsEmpty(import.CollectionAdjustmentReason.Code))
      {
        var field = GetField(export.CollectionAdjustmentReason, "code");

        field.Protected = false;
        field.Focused = true;
      }
      else
      {
        var field =
          GetField(export.Collection, "collectionAdjustmentReasonTxt");

        field.Protected = false;
        field.Focused = true;
      }

      return;
    }

    export.CollectionAdjustmentReason.Assign(import.CollectionAdjustmentReason);

    if (IsEmpty(export.CsePerson.Number))
    {
      export.CsePerson.Number =
        export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, the user requested a next 
    // tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
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
      // --------------
      // This is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // --------------
      UseScCabNextTranGet();

      // 04/02/2001   M. Brown  PR# 111813: Next tran modifications.
      export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      ExitState = "FN_NO_NEXT_TO_COLA";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(export.FirstTimeFlag.Flag) != 'N')
    {
      export.FirstTimeFlag.Flag = "N";
      global.Command = "DISPLAY";
    }

    // :Jan., 2002 M. Brown Work Order # 010504 Retro Processing.
    // Part of confirm logic for when the user is asked to confirm add of 
    // collection
    // adjustment if protected collections exist on the Cash Receipt Detail.
    if (AsChar(export.UserConfirmedAdj.Flag) == 'Y' && !
      Equal(global.Command, "ADD"))
    {
      export.UserConfirmedAdj.Flag = "";
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD"))
    {
      if (export.Entered.SequentialNumber == 0 || export
        .CashReceiptDetail.SequentialIdentifier == 0)
      {
        export.CsePersonsWorkSet.FormattedName = "";
        export.CollectionAdjustmentReason.Code = "";
        export.CollectionAdjustmentReason.Name = "";
        export.Collection.CollectionAdjustmentReasonTxt =
          Spaces(Collection.CollectionAdjustmentReasonTxt_MaxLength);
        export.Collection.LastUpdatedBy = "";
        export.Collection.CollectionAdjustmentDt = null;
        export.CashReceiptType.Code = "";
        export.CsePersonsWorkSet.FormattedName = "";
        export.CsePerson.Number = "";
        export.CollectionType.Code = "";
        export.CashReceiptSourceType.Code = "";
        export.CashReceiptDetail.CollectionDate = null;
        export.CashReceiptDetailStatus.Code = "";
        export.ScreenOwedAmounts.ErrorInformationLine = "";
        export.CashReceiptDetail.CollectionAmount = 0;
        export.CashReceiptDetail.RefundedAmount = 0;
        export.CashReceiptDetail.DistributedAmount = 0;
        export.CashReceiptDetail.CourtOrderNumber = "";
        export.UndistributedAmount.TotalCurrency = 0;
      }

      if (IsEmpty(export.CollectionAdjustmentReason.Code))
      {
        export.CollectionAdjustmentReason.Name = "";
      }

      if (AsChar(import.DebtCollCommon.Flag) != 'Y')
      {
        if (export.Entered.SequentialNumber == 0 && import
          .Pass.SequentialNumber == 0)
        {
          ExitState = "SP0000_REQUIRED_FIELD_MISSING";

          var field = GetField(export.Entered, "sequentialNumber");

          field.Error = true;
        }

        if (import.CashReceiptDetail.SequentialIdentifier == 0)
        {
          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;

          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "CRRC":
        if (export.CashReceiptDetail.SequentialIdentifier == 0 || export
          .Entered.SequentialNumber == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          return;
        }

        if ((Equal(export.CollectionAdjustmentReason.Code, "WR ACCT") || Equal
          (export.CollectionAdjustmentReason.Code, "WR AMT")) && AsChar
          (export.Collection.AdjustedInd) == 'Y')
        {
          ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";
        }
        else
        {
          if (AsChar(export.Collection.AdjustedInd) == 'Y')
          {
            var field1 = GetField(export.CollectionAdjustmentReason, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Collection, "collectionAdjustmentReasonTxt");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.AmtPrompt, "text1");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          ExitState = "FN0000_CANNOT_FLOW_TO_CRRC";
        }

        break;
      case "RETURN":
        if (AsChar(export.Collection.AdjustedInd) == 'Y' && (
          Equal(export.CashReceiptDetailStatus.Code, "ADJ") || Equal
          (export.CashReceiptDetailStatus.Code, "REC") || Equal
          (export.CashReceiptDetailStatus.Code, "REL")))
        {
          var field1 = GetField(export.CollectionAdjustmentReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.AmtPrompt, "text1");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Collection, "collectionAdjustmentReasonTxt");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        // Calls the display module.
        if (import.Pass.SequentialNumber > 0)
        {
          // -------------------------------------------
          // A flow has been taken from other transactions with these fields
          // -------------------------------------------
          export.Entered.SequentialNumber = import.Pass.SequentialNumber;
        }

        UseFnReadAdjustedCashRecDetail1();

        if (IsExitState("CASH_RECEIPT_DETAIL_NF"))
        {
          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;
        }

        if (IsExitState("FN0000_CASH_RECEIPT_NF"))
        {
          var field = GetField(export.Entered, "sequentialNumber");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CollectionAdjustmentReason.Code = "";
          export.CollectionAdjustmentReason.Name = "";
          export.Collection.CollectionAdjustmentReasonTxt =
            Spaces(Collection.CollectionAdjustmentReasonTxt_MaxLength);
          export.CashReceiptType.Code = "";
          export.CashReceiptSourceType.Code = "";
          export.CsePerson.Number = "";
          export.CollectionType.Code = "";
          export.CashReceiptDetailStatus.Code = "";
          export.ScreenOwedAmounts.ErrorInformationLine = "";
          export.CashReceiptDetail.CollectionAmount = 0;
          export.CashReceiptDetail.CollectionDate = null;
          export.CashReceiptDetail.RefundedAmount = 0;
          export.UndistributedAmount.TotalCurrency = 0;
          export.CashReceiptDetail.DistributedAmount = 0;
          export.CashReceiptDetail.CourtOrderNumber = "";
          export.Collection.LastUpdatedBy = "";
          export.Collection.CollectionAdjustmentDt = null;

          return;
        }
        else
        {
          export.CashReceiptDetail.Assign(local.CashReceiptDetail);
          export.Entered.SequentialNumber = local.Entered.SequentialNumber;
        }

        UseFnCheckForUnprocessedTrans();

        if (IsExitState("FN0000_UNPROCESSED_TRANS_EXIST"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -----------------------
          // Naveen - 11/02/1998
          // Adjustment code and reason must be protected to prevent entry if 
          // all the collections have been adjusted.
          // -----------------------
          if (AsChar(export.Collection.AdjustedInd) == 'Y' && !
            IsEmpty(export.CollectionAdjustmentReason.Code))
          {
            var field1 = GetField(export.CollectionAdjustmentReason, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.AmtPrompt, "text1");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.CollectionAdjustmentReason, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Collection, "collectionAdjustmentReasonTxt");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (export.CashReceiptDetail.RefundedAmount.GetValueOrDefault() != 0
            && export
            .CashReceiptDetail.DistributedAmount.GetValueOrDefault() != 0)
          {
            ExitState = "FN0000_DISP_SUCC_REF_EXISTS";
          }
          else
          {
            if (AsChar(export.ManualPostedCollExists.SelectChar) == 'Y')
            {
              ExitState = "ACO_NW0000_DISP_SUCC_MAN_EXIST";
            }
            else
            {
              ExitState = "ACO_NW0000_DISP_SUCC_NO_MAN_COLL";
            }

            if (AsChar(export.CollProtExists.Flag) == 'Y')
            {
              ExitState = "FN0000_DISP_SUCC_PROT_EXIST";
            }
          }
        }
        else
        {
          export.CsePersonsWorkSet.FormattedName = "";
          export.CollectionAdjustmentReason.Code = "";
          export.CollectionAdjustmentReason.Name = "";
          export.Collection.CollectionAdjustmentReasonTxt =
            Spaces(Collection.CollectionAdjustmentReasonTxt_MaxLength);
          export.CashReceiptType.Code = "";
          export.CashReceiptSourceType.Code = "";
          export.CsePerson.Number = "";
          export.CollectionType.Code = "";
          export.CashReceiptDetailStatus.Code = "";
          export.ScreenOwedAmounts.ErrorInformationLine = "";
          export.CashReceiptDetail.CollectionAmount = 0;
          export.CashReceiptDetail.RefundedAmount = 0;
          export.UndistributedAmount.TotalCurrency = 0;
          export.CashReceiptDetail.DistributedAmount = 0;
          export.CashReceiptDetail.CourtOrderNumber = "";
        }

        break;
      case "ADD":
        if (export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() == 0)
        {
          if (AsChar(export.Collection.AdjustedInd) == 'Y')
          {
            var field1 = GetField(export.CollectionAdjustmentReason, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Collection, "collectionAdjustmentReasonTxt");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.AmtPrompt, "text1");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          ExitState = "FN0000_NO_MONEY_HAS_BEEN_DIST";

          return;
        }

        if (Equal(export.CashReceiptDetailStatus.Code, "ADJ") || Equal
          (export.CashReceiptDetailStatus.Code, "REC") || Equal
          (export.CashReceiptDetailStatus.Code, "REL") && AsChar
          (export.Collection.AdjustedInd) == 'Y')
        {
          var field1 = GetField(export.CollectionAdjustmentReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Collection, "collectionAdjustmentReasonTxt");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.AmtPrompt, "text1");

          field3.Color = "cyan";
          field3.Protected = true;

          ExitState = "FN0000_COLLECTION_ALREADY_ADJUST";

          return;
        }

        // -------------------------------------------------
        // Naveen - 10/30/1998
        // While doing a collection adjustment none of the system generated 
        // collection adjustment reason codes should be selected
        // -------------------------------------------------
        if (ReadCollectionAdjustmentReason())
        {
          MoveCollectionAdjustmentReason1(entities.CollectionAdjustmentReason,
            export.CollectionAdjustmentReason);
        }
        else
        {
          export.CollectionAdjustmentReason.Name = "";

          var field = GetField(export.CollectionAdjustmentReason, "code");

          field.Error = true;

          ExitState = "FN0000_COLL_ADJUST_REASON_NF";

          return;
        }

        if (IsEmpty(export.CollectionAdjustmentReason.Code))
        {
          export.CollectionAdjustmentReason.Name = "";

          var field = GetField(export.CollectionAdjustmentReason, "code");

          field.Error = true;

          ExitState = "SP0000_MUST_HAVE_A_CODE";
        }
        else if (!Equal(export.CollectionAdjustmentReason.Code, "BAD CK") && !
          Equal(export.CollectionAdjustmentReason.Code, "COURTAD") && !
          Equal(export.CollectionAdjustmentReason.Code, "REIPADJ") && !
          Equal(export.CollectionAdjustmentReason.Code, "REFUND") && !
          Equal(export.CollectionAdjustmentReason.Code, "ST PMT") && !
          Equal(export.CollectionAdjustmentReason.Code, "WR ACCT") && !
          Equal(export.CollectionAdjustmentReason.Code, "WR DDTL") && !
          Equal(export.CollectionAdjustmentReason.Code, "WR AMT") && !
          Equal(export.CollectionAdjustmentReason.Code, "KPC ADJ"))
        {
          var field1 = GetField(export.CollectionAdjustmentReason, "code");

          field1.Error = true;

          var field2 = GetField(export.CollectionAdjustmentReason, "name");

          field2.Error = true;

          ExitState = "FN0000_SYSGEN_CODE_NOT_FOR_ADJST";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        if (IsEmpty(export.Collection.CollectionAdjustmentReasonTxt))
        {
          var field =
            GetField(export.Collection, "collectionAdjustmentReasonTxt");

          field.Error = true;

          ExitState = "SP0000_TXT_REQD_FOR_ADJ";

          return;
        }

        // :Jan., 2002 M. Brown Work Order # 010504 Retro Processing.
        // Part of confirm logic for when the user is asked to confirm add of 
        // collection
        // adjustment if protected collections exist on the Cash Receipt Detail.
        if (AsChar(export.CollProtExists.Flag) == 'Y')
        {
          if (AsChar(export.UserConfirmedAdj.Flag) == 'Y')
          {
            // : User has been through first pass of this logic, where a confirm
            // exitstate is set.
            export.UserConfirmedAdj.Flag = "";
          }
          else
          {
            var field1 =
              GetField(export.CashReceiptDetail, "sequentialIdentifier");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Entered, "sequentialNumber");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.CollectionAdjustmentReason, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Collection, "collectionAdjustmentReasonTxt");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.AmtPrompt, "text1");

            field5.Color = "cyan";
            field5.Protected = true;

            export.UserConfirmedAdj.Flag = "Y";
            ExitState = "FN0000_CONFIRM_ADJ_OF_PROT_COLL";

            return;
          }
        }

        // -----------------------------------------
        // N.Engoor - 03/09/1999
        // 1. If reason code is 'BAD CK', 'ST PMT', 'COURTAD' 'KPC ADJ'  or '
        // REIPADJ'  set the status to 'ADJ'.
        // 2. If the reason code is 'WR ACCT' or 'WR AMT'  set the status to '
        // REC'.
        // 3. If the reason code is 'WR DDTL' or 'REFUND' set the status to '
        // REL'.
        // -----------------------------------------
        if (Equal(export.CollectionAdjustmentReason.Code, "BAD CK") || Equal
          (export.CollectionAdjustmentReason.Code, "ST PMT") || Equal
          (export.CollectionAdjustmentReason.Code, "COURTAD") || Equal
          (export.CollectionAdjustmentReason.Code, "KPC ADJ") || Equal
          (export.CollectionAdjustmentReason.Code, "REIPADJ"))
        {
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier = 2;
        }
        else if (Equal(export.CollectionAdjustmentReason.Code, "WR ACCT") || Equal
          (export.CollectionAdjustmentReason.Code, "WR AMT"))
        {
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier = 1;
        }
        else if (Equal(export.CollectionAdjustmentReason.Code, "WR DDTL") || Equal
          (export.CollectionAdjustmentReason.Code, "REFUND"))
        {
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier = 6;
        }

        local.ProgramProcessingInfo.Name = global.UserId;
        local.ProgramProcessingInfo.ProcessDate = Now().Date;
        local.Collection.CollectionAdjustmentDt =
          local.ProgramProcessingInfo.ProcessDate;
        local.Obligor.PgmChgEffectiveDate =
          export.CashReceiptDetail.CollectionDate;
        local.Collection.CollectionAdjustmentReasonTxt =
          export.Collection.CollectionAdjustmentReasonTxt ?? "";
        UseFnCabReverseOneCshRcptDtl();

        if (IsExitState("FN0000_COLL_ADJUST_REASON_NF"))
        {
          var field1 = GetField(export.CollectionAdjustmentReason, "code");

          field1.Error = true;

          var field2 = GetField(export.CollectionAdjustmentReason, "name");

          field2.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -----------------------
        // Naveen - 03/18/1999
        // If the reason code entered is WR DDTL and an adjustment is made, 
        // force the worker to flow to MCOL.
        // Naveen - 04/25/1999
        // If the reason code entered is REFUND and an adjustment is made, force
        // the worker to flow to CRRU.
        // -----------------------
        if (Equal(export.CollectionAdjustmentReason.Code, "WR DDTL"))
        {
          ExitState = "ECO_LNK_TO_MANUAL_DIST_OF_COLL";

          return;
        }

        if (Equal(export.CollectionAdjustmentReason.Code, "REFUND"))
        {
          ExitState = "ECO_LNK_TO_REFUND_COLLECTION";

          return;
        }

        UseFnReadAdjustedCashRecDetail2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -----------------------
        // Naveen - 11/02/1998
        // Adjustment code and reason must be protected to prevent entry if the 
        // collection has already been adjusted.
        // -----------------------
        if (!IsEmpty(export.CollectionAdjustmentReason.Code))
        {
          var field1 = GetField(export.CollectionAdjustmentReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.AmtPrompt, "text1");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Collection, "collectionAdjustmentReasonTxt");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.ManualPostedCollExists.SelectChar = "N";

          if (Equal(export.CashReceiptDetailStatus.Code, "REC"))
          {
            ExitState = "FN0000_COLL_ADJ_FLOW_TO_CRRC";
          }
          else
          {
            ExitState = "FN0000_COLL_ADJUSTMENT_ADDED";
          }
        }

        break;
      case "LIST":
        switch(AsChar(import.AmtPrompt.Text1))
        {
          case '+':
            break;
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.AmtPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        if (local.PromptCount.Count == 0)
        {
          var field = GetField(export.AmtPrompt, "text1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }
        else
        {
        }

        if (AsChar(export.AmtPrompt.Text1) == 'S')
        {
          export.AmtPrompt.Text1 = "+";
          MoveCollectionAdjustmentReason2(export.CollectionAdjustmentReason,
            export.Prev);
          ExitState = "ECO_LNK_LST_ADJUSTMENTS";

          // ---------------------------------------------
          // MTW - Chayan 3/28/1997 Change Start
          // For providing proper error msg
          // ---------------------------------------------
        }
        else
        {
        }

        break;
      default:
        if (IsEmpty(export.CollectionAdjustmentReason.Code))
        {
          export.CollectionAdjustmentReason.Name = "";
        }

        if (export.Entered.SequentialNumber == 0 || export
          .CashReceiptDetail.SequentialIdentifier == 0)
        {
          export.CollectionAdjustmentReason.Code = "";
          export.CsePersonsWorkSet.FormattedName = "";
          export.CollectionAdjustmentReason.Name = "";
          export.Collection.CollectionAdjustmentReasonTxt =
            Spaces(Collection.CollectionAdjustmentReasonTxt_MaxLength);
          export.CashReceiptType.Code = "";
          export.CashReceiptSourceType.Code = "";
          export.CsePerson.Number = "";
          export.CsePersonsWorkSet.FormattedName = "";
          export.CollectionType.Code = "";
          export.CashReceiptDetail.CollectionDate = null;
          export.CashReceiptDetailStatus.Code = "";
          export.ScreenOwedAmounts.ErrorInformationLine = "";
          export.CashReceiptDetail.CollectionAmount = 0;
          export.CashReceiptDetail.RefundedAmount = 0;
          export.CashReceiptDetail.DistributedAmount = 0;
          export.UndistributedAmount.TotalCurrency = 0;
        }

        if (export.Entered.SequentialNumber == 0)
        {
          var field = GetField(export.Entered, "sequentialNumber");

          field.Error = true;
        }

        if (export.CashReceiptDetail.SequentialIdentifier == 0)
        {
          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;
        }

        if (AsChar(export.Collection.AdjustedInd) == 'Y' && (
          Equal(export.CashReceiptDetailStatus.Code, "ADJ") || Equal
          (export.CashReceiptDetailStatus.Code, "REC") || Equal
          (export.CashReceiptDetailStatus.Code, "REL")))
        {
          var field1 = GetField(export.CollectionAdjustmentReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.AmtPrompt, "text1");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Collection, "collectionAdjustmentReasonTxt");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // Add any common logic that must occur at
    // the end of every pass.
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
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

  private static void MoveCollection1(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveCollectionAdjustmentReason1(
    CollectionAdjustmentReason source, CollectionAdjustmentReason target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionAdjustmentReason2(
    CollectionAdjustmentReason source, CollectionAdjustmentReason target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseFnCabReverseOneCshRcptDtl()
  {
    var useImport = new FnCabReverseOneCshRcptDtl.Import();
    var useExport = new FnCabReverseOneCshRcptDtl.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      export.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Obligor.PgmChgEffectiveDate = local.Obligor.PgmChgEffectiveDate;
    MoveCashReceiptDetail(export.CashReceiptDetail, useImport.CashReceiptDetail);
      
    useImport.Max.Date = local.Max.Date;
    MoveCollection2(local.Collection, useImport.Collection);
    useImport.RecAdjStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber = export.Entered.SequentialNumber;

    Call(FnCabReverseOneCshRcptDtl.Execute, useImport, useExport);
  }

  private void UseFnCheckForUnprocessedTrans()
  {
    var useImport = new FnCheckForUnprocessedTrans.Import();
    var useExport = new FnCheckForUnprocessedTrans.Export();

    useImport.Obligor.Number = export.CsePerson.Number;

    Call(FnCheckForUnprocessedTrans.Execute, useImport, useExport);

    export.ScreenOwedAmounts.ErrorInformationLine =
      useExport.ScreenOwedAmounts.ErrorInformationLine;
  }

  private void UseFnReadAdjustedCashRecDetail1()
  {
    var useImport = new FnReadAdjustedCashRecDetail.Import();
    var useExport = new FnReadAdjustedCashRecDetail.Export();

    MoveCollection1(import.DebtCollCollection, useImport.DebtCollCollection);
    useImport.DebtCollCommon.Flag = import.DebtCollCommon.Flag;
    useImport.CashReceipt.SequentialNumber = export.Entered.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(FnReadAdjustedCashRecDetail.Execute, useImport, useExport);

    export.Debcoll.Amount = useExport.Debcoll.Amount;
    export.CashReceiptDetailStatus.Code =
      useExport.CashReceiptDetailStatus.Code;
    export.CollectionAdjustmentReason.Assign(
      useExport.CollectionAdjustmentReason);
    MoveCollectionType(useExport.CollectionType, export.CollectionType);
    export.UndistributedAmount.TotalCurrency =
      useExport.UndistAmt.TotalCurrency;
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      export.CashReceiptSourceType);
    MoveCashReceiptType(useExport.CashReceiptType, export.CashReceiptType);
    export.Collection.Assign(useExport.Collection);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CsePerson.Number = useExport.CsePerson.Number;
    local.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
    local.Entered.SequentialNumber = useExport.CashReceipt.SequentialNumber;
    export.CollProtExists.Flag = useExport.CollProtExists.Flag;
    export.ManualPostedCollExists.SelectChar =
      useExport.ManuallyPostedColl.SelectChar;
  }

  private void UseFnReadAdjustedCashRecDetail2()
  {
    var useImport = new FnReadAdjustedCashRecDetail.Import();
    var useExport = new FnReadAdjustedCashRecDetail.Export();

    MoveCollection1(import.DebtCollCollection, useImport.DebtCollCollection);
    useImport.DebtCollCommon.Flag = import.DebtCollCommon.Flag;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber = export.Entered.SequentialNumber;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(FnReadAdjustedCashRecDetail.Execute, useImport, useExport);

    export.CashReceiptDetailStatus.Code =
      useExport.CashReceiptDetailStatus.Code;
    export.CollectionAdjustmentReason.Assign(
      useExport.CollectionAdjustmentReason);
    MoveCollectionType(useExport.CollectionType, export.CollectionType);
    export.UndistributedAmount.TotalCurrency =
      useExport.UndistAmt.TotalCurrency;
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      export.CashReceiptSourceType);
    MoveCashReceiptType(useExport.CashReceiptType, export.CashReceiptType);
    export.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
    export.Collection.Assign(useExport.Collection);
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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCollectionAdjustmentReason()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetString(
          command, "obTrnRlnRsnCd", import.CollectionAdjustmentReason.Code);
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Populated = true;
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
    /// A value of CollProtExists.
    /// </summary>
    [JsonPropertyName("collProtExists")]
    public Common CollProtExists
    {
      get => collProtExists ??= new();
      set => collProtExists = value;
    }

    /// <summary>
    /// A value of ManualPostedCollExists.
    /// </summary>
    [JsonPropertyName("manualPostedCollExists")]
    public Common ManualPostedCollExists
    {
      get => manualPostedCollExists ??= new();
      set => manualPostedCollExists = value;
    }

    /// <summary>
    /// A value of Debcoll.
    /// </summary>
    [JsonPropertyName("debcoll")]
    public Collection Debcoll
    {
      get => debcoll ??= new();
      set => debcoll = value;
    }

    /// <summary>
    /// A value of DebtCollCommon.
    /// </summary>
    [JsonPropertyName("debtCollCommon")]
    public Common DebtCollCommon
    {
      get => debtCollCommon ??= new();
      set => debtCollCommon = value;
    }

    /// <summary>
    /// A value of DebtCollCollection.
    /// </summary>
    [JsonPropertyName("debtCollCollection")]
    public Collection DebtCollCollection
    {
      get => debtCollCollection ??= new();
      set => debtCollCollection = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CollectionAdjustmentReason Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of AmtPrompt.
    /// </summary>
    [JsonPropertyName("amtPrompt")]
    public TextWorkArea AmtPrompt
    {
      get => amtPrompt ??= new();
      set => amtPrompt = value;
    }

    /// <summary>
    /// A value of Entered.
    /// </summary>
    [JsonPropertyName("entered")]
    public CashReceipt Entered
    {
      get => entered ??= new();
      set => entered = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of UndistributedAmount.
    /// </summary>
    [JsonPropertyName("undistributedAmount")]
    public Common UndistributedAmount
    {
      get => undistributedAmount ??= new();
      set => undistributedAmount = value;
    }

    /// <summary>
    /// A value of ListPrompt.
    /// </summary>
    [JsonPropertyName("listPrompt")]
    public Common ListPrompt
    {
      get => listPrompt ??= new();
      set => listPrompt = value;
    }

    /// <summary>
    /// A value of FirstTimeFlag.
    /// </summary>
    [JsonPropertyName("firstTimeFlag")]
    public Common FirstTimeFlag
    {
      get => firstTimeFlag ??= new();
      set => firstTimeFlag = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CashReceipt Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Adjusting.
    /// </summary>
    [JsonPropertyName("adjusting")]
    public CashReceiptDetail Adjusting
    {
      get => adjusting ??= new();
      set => adjusting = value;
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
    /// A value of UserConfirmedAdj.
    /// </summary>
    [JsonPropertyName("userConfirmedAdj")]
    public Common UserConfirmedAdj
    {
      get => userConfirmedAdj ??= new();
      set => userConfirmedAdj = value;
    }

    private Common collProtExists;
    private Common manualPostedCollExists;
    private Collection debcoll;
    private Common debtCollCommon;
    private Collection debtCollCollection;
    private CollectionAdjustmentReason prev;
    private TextWorkArea amtPrompt;
    private CashReceipt entered;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistributedAmount;
    private Common listPrompt;
    private Common firstTimeFlag;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CollectionType collectionType;
    private CashReceipt pass;
    private CsePerson csePerson;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private Collection collection;
    private CashReceiptDetail adjusting;
    private NextTranInfo hidden;
    private Standard standard;
    private Common userConfirmedAdj;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CollProtExists.
    /// </summary>
    [JsonPropertyName("collProtExists")]
    public Common CollProtExists
    {
      get => collProtExists ??= new();
      set => collProtExists = value;
    }

    /// <summary>
    /// A value of ManualPostedCollExists.
    /// </summary>
    [JsonPropertyName("manualPostedCollExists")]
    public Common ManualPostedCollExists
    {
      get => manualPostedCollExists ??= new();
      set => manualPostedCollExists = value;
    }

    /// <summary>
    /// A value of DebCollCollection.
    /// </summary>
    [JsonPropertyName("debCollCollection")]
    public Collection DebCollCollection
    {
      get => debCollCollection ??= new();
      set => debCollCollection = value;
    }

    /// <summary>
    /// A value of DebCollCommon.
    /// </summary>
    [JsonPropertyName("debCollCommon")]
    public Common DebCollCommon
    {
      get => debCollCommon ??= new();
      set => debCollCommon = value;
    }

    /// <summary>
    /// A value of Debcoll.
    /// </summary>
    [JsonPropertyName("debcoll")]
    public Collection Debcoll
    {
      get => debcoll ??= new();
      set => debcoll = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CollectionAdjustmentReason Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of AmtPrompt.
    /// </summary>
    [JsonPropertyName("amtPrompt")]
    public TextWorkArea AmtPrompt
    {
      get => amtPrompt ??= new();
      set => amtPrompt = value;
    }

    /// <summary>
    /// A value of Entered.
    /// </summary>
    [JsonPropertyName("entered")]
    public CashReceipt Entered
    {
      get => entered ??= new();
      set => entered = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ListPrompt.
    /// </summary>
    [JsonPropertyName("listPrompt")]
    public Common ListPrompt
    {
      get => listPrompt ??= new();
      set => listPrompt = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of UndistributedAmount.
    /// </summary>
    [JsonPropertyName("undistributedAmount")]
    public Common UndistributedAmount
    {
      get => undistributedAmount ??= new();
      set => undistributedAmount = value;
    }

    /// <summary>
    /// A value of FirstTimeFlag.
    /// </summary>
    [JsonPropertyName("firstTimeFlag")]
    public Common FirstTimeFlag
    {
      get => firstTimeFlag ??= new();
      set => firstTimeFlag = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of UserConfirmedAdj.
    /// </summary>
    [JsonPropertyName("userConfirmedAdj")]
    public Common UserConfirmedAdj
    {
      get => userConfirmedAdj ??= new();
      set => userConfirmedAdj = value;
    }

    private Common collProtExists;
    private Common manualPostedCollExists;
    private Collection debCollCollection;
    private Common debCollCommon;
    private Collection debcoll;
    private CollectionAdjustmentReason prev;
    private TextWorkArea amtPrompt;
    private CashReceipt entered;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Common listPrompt;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistributedAmount;
    private Common firstTimeFlag;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private Collection collection;
    private NextTranInfo hidden;
    private Standard standard;
    private Common userConfirmedAdj;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Entered.
    /// </summary>
    [JsonPropertyName("entered")]
    public CashReceipt Entered
    {
      get => entered ??= new();
      set => entered = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

    /// <summary>
    /// A value of PadCsePersonNumber.
    /// </summary>
    [JsonPropertyName("padCsePersonNumber")]
    public TextWorkArea PadCsePersonNumber
    {
      get => padCsePersonNumber ??= new();
      set => padCsePersonNumber = value;
    }

    private CashReceipt entered;
    private Collection collection;
    private CsePersonAccount obligor;
    private ProgramProcessingInfo programProcessingInfo;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private DateWorkArea max;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private Common promptCount;
    private TextWorkArea padCsePersonNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    private CollectionAdjustmentReason collectionAdjustmentReason;
  }
#endregion
}

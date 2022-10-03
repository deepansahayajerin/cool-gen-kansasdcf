// Program: FN_CRRC_REC_COLLCTN_DETAILS_PRI, ID: 371769956, model: 746.
// Short name: SWECRRCP
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
/// A program: FN_CRRC_REC_COLLCTN_DETAILS_PRI.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrrcRecCollctnDetailsPri: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRRC_REC_COLLCTN_DETAILS_PRI program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrrcRecCollctnDetailsPri(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrrcRecCollctnDetailsPri.
  /// </summary>
  public FnCrrcRecCollctnDetailsPri(IContext context, Import import,
    Export export):
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
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ----------------------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 02/05/96	Holly Kennedy-MTW			Retrofits
    // 02/11/96	Holly Kennedy-MTW			Added logic to validate entered state.
    // 04/04/96	Holly Kennedy-MTW			Logic in the Record Collection CAB was 
    // passing a potentially Unpopulated view to the Create Status CAB.
    // Added logic to the update processing to allow the user to add an address 
    // if data is entered in the address fields and a current one does not
    // exist.
    // 04/09/96	Holly Kennedy-MTW			Added logic to Check for current status when
    // pending or Releasing 06/15/96	Holly Kennedy-MTW			added flow to MCOL
    // screen.
    // 11/04/96	Holly Kennedy-MTW			Added field to the screen to display the 
    // total amount of the Collections already applied to the Cash Receipt.
    // Added logic to disallow a Collection to be added if the total of it and 
    // all other Collections exceeds the amount of the Cash Receipt amount.
    // 01/02/97	R. Marchman	Add new security/next tran.
    // 10/02/97	Siraj Konkader
    // Added edits for SSN and Person #. Xref SSN and Person # if both entered. 
    // Edit for SSN <> 0. Add exit state check after call to CABS.
    // 11/03/97        Venkatesh Kamaraj	PR# 31414 Changed the CAB that gets the
    // total collection applied to also consider the refunded amount. Changed
    // the >= to > after the call of this CAB
    // 11/04/97	Venkatesh Kamaraj	PR #31525 Added two fields for Multi-Payor to 
    // the screen and logic to populate it in display/add/update logic
    // 11/25/97	Venkatesh Kamaraj	PR #32060, #32279, #32049, #32725, #31414
    // Automatic release of the collections reltated to a single cash receipt.
    // 10/16/98	Sunya Sharp	Make changes per screen assessment signed 9/25/98.
    // 02/06/99	Sunya Sharp	Change the net interface amount (cash due) to the 
    // original interface amount (total cash transaction amount).  This will
    // include change in the logic and the screen.  Also add logic to only
    // require the interface transaction id when the receipt is manual
    // interface.
    // 05/25/99	Sunya Sharp	Making changes for integration problems.
    // 06/07/99	Sunya Sharp	Changed PF20 to flow to CRDA.  Add logic to not 
    // allow user to pend a detail that was created on REIP.  Changed the
    // tranfsers coming and going to FEES to be a link.
    // 07/10/99	Sunya Sharp	System Test PR #101.  Would not allow user to add 
    // CRDs to a cash receipt with a source code of an area office and a cash
    // receipt type of RDIRPMT.  Need to change logic that checks which amount
    // fields to compare when adding, updating and releasing a cash receipt
    // detail.
    // 08/10/99	Sunya Sharp	TDR #94.  Fix abend problem when flowing to MTRN.  
    // Need to check to ensure that the check/EFT number is all numeric before
    // flowing to MTRN.
    // 09/17/99	Sunya Sharp	H00074363 - Do not allow the release of a CRD unless
    // the distribution information is stored on the database not just
    // displayed on the screen
    // 11/2/99	Sunya Sharp	PR# 00078845 - Do not allow flow to MCOL unless the 
    // detail is in released status.
    // 12/13/99	Sunya Sharp	PR# 78962 - Allow for multi-payor to be Father/
    // Father or Mother/Mother.
    // 02/01/00	Sunya Sharp	PR# 85804 - Add 'Flow to' field to CRRC and new 
    // flows to MCOL, OPAY, COLL, and PACC.
    // 02/21/00	Sunya Sharp	PR# 85889 - Add new PF key to adjust a CRD.
    // 03/02/00        pphinney        H00089803 - Changed order of PF Keys
    // 03/08/00        pphinney        H00090300 - Added PAYR to PF22 Flow
    // 05/01/00        pphinney        H00093903 - Changed so that "ADJ" key 
    // causes flow thru security.
    // 06/02/00        pphinney        H00096632    Changed logic to BLOCK 
    // unauthorized
    // USERs from Viewing Address.
    // 08/02/00        pphinney        H00098632    Changed logic to NOT allow 
    // change of
    // collection date if CRD is REL
    // w/distributed or refunded amount
    // 08/21/00        pphinney        H00101532    Changed logic to NOT allow 
    // change of
    // collection TYPE if CRD is REL
    // w/distributed or refunded amount
    // 10/27/00        pphinney        I00106505    Changed logic to NOT allow
    // CASH collection TYPE
    // with NON-CASH Cash_Receipt
    // NON-CASH collection TYPE
    // with CASH Cash_Receipt
    // 05/31/2001    Vithal Madhira    PR# 105882
    // The cash_receipt_detail collection amount should be totally undistributed
    // with the status of PEND for adjust (PF21). (New business rule added).
    // 09/05/2002    KDoshi         PR149011    Fix Screen Help.
    // 09/23/02        pphinney        I00155999    Changed logic to allow
    // PEND of NON-Cash CRDs
    // 08/20/07	CLocke		PR182853	Changed logic so can unpend Non-cash
    // 10/19/07	CLocke				Changed verify statements for CoolGen V7.6 to put
    // 						space in front instead of last character in
    // 						the string.
    // 11/25/14	GVandy 		CQ42192		"C" collection type must be used with CSSI
    // 						source type.
    // -----------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // *** Added exitstate for successful clear.  Sunya Sharp 10/22/98 ***
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.HidMcolCurrent.SequentialIdentifier =
      import.HidMcolCurrent.SequentialIdentifier;
    export.AmtPrompt.Text1 = import.AmtPrompt.Text1;
    export.CashReceipt.Assign(import.CashReceipt);
    export.CashReceiptDetail.Assign(import.CashReceiptDetail);
    export.CashReceiptDetailAddress.Assign(import.CashReceiptDetailAddress);
    export.CashReceiptDetailStatHistory.ReasonCodeId =
      import.CashReceiptDetailStatHistory.ReasonCodeId;
    MoveCashReceiptDetailStatus(import.CashReceiptDetailStatus,
      export.CashReceiptDetailStatus);
    export.CashReceiptLiterals.Assign(import.CashReceiptLiterals);
    export.CashReceiptSourceType.Assign(import.CashReceiptSourceType);
    MoveCollectionType(import.CollectionType2, export.CollectionType2);
    export.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    export.DeletedCashReceiptFlag.Flag = import.DeletedCashReceiptFlag.Flag;
    export.HiddenCashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    export.Suspended.TotalCurrency = import.Suspended.TotalCurrency;
    export.CollAmtApplied.TotalCurrency = import.CollAmtApplied.TotalCurrency;
    MoveCashReceiptDetail4(import.Previous, export.Previous);
    export.Collection.PromptField = import.Collection.PromptField;
    export.CollectionType1.PromptField = import.CollectionType1.PromptField;
    export.PendRsnPrompt.PromptField = import.PendRsnPrompt.PromptField;
    export.TotalNoOfColl.Count = import.TotalNoOfColl.Count;
    export.Adjustment.AverageCurrency = import.Adjustment.AverageCurrency;
    export.OriginalCollection.AverageCurrency =
      import.OriginalCollection.AverageCurrency;
    export.WorkIsMultiPayor.Flag = import.WorkIsMultiPayor.Flag;
    MoveStandard(import.Pf17FlowTo, export.Pf17FlowTo);
    export.Code.CodeName = import.Code.CodeName;

    // 03/08/00        pphinney        H00090300 - Added PAYR to PF22 Flow
    export.HiddenCashReceipt.SequentialNumber =
      import.HiddenCashReceipt.SequentialNumber;

    // 06/02/00        pphinney        H00096632    Changed logic to BLOCK 
    // unauthorized
    export.HiddenCashReceiptDetail.ObligorPhoneNumber =
      import.HiddenCashReceiptDetail.ObligorPhoneNumber;

    // 08/02/00        pphinney        H00098632    Changed logic to NOT allow 
    // change of
    export.Save.CollectionDate = import.Save.CollectionDate;

    // 08/21/00        pphinney        H00101532    Changed logic to NOT allow 
    // change of
    export.HiddenCollectionType.Code = import.HiddenCollectionType.Code;

    // ---- Left pad Case Number & Cse-Person number with zeroes
    if (!IsEmpty(export.CashReceiptDetail.CaseNumber))
    {
      local.TextWorkArea.Text10 = export.CashReceiptDetail.CaseNumber ?? Spaces
        (10);
      UseEabPadLeftWithZeros();
      export.CashReceiptDetail.CaseNumber = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
    {
      local.TextWorkArea.Text10 =
        export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.CashReceiptDetail.ObligorPersonNumber = local.TextWorkArea.Text10;
    }

    // *****
    // Next Tran/Security logic
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CashReceiptDetail.ObligorPersonNumber ?? "";
      export.HiddenNextTranInfo.CaseNumber =
        export.CashReceiptDetail.CaseNumber ?? "";
      export.HiddenNextTranInfo.CourtOrderNumber =
        export.CashReceiptDetail.CourtOrderNumber ?? "";
      export.HiddenNextTranInfo.CsePersonNumberObligor =
        export.CashReceiptDetail.ObligorPersonNumber ?? "";
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      // *** Moved cab before set statements.  And deleted statement command is 
      // display.  There is not enough information to display cash receipt or
      // cash receipt detail.  Sunya Sharp 10/27/98 ***
      UseScCabNextTranGet();
      export.CashReceiptDetail.CaseNumber =
        export.HiddenNextTranInfo.CaseNumber ?? "";
      export.CashReceiptDetail.CourtOrderNumber =
        export.HiddenNextTranInfo.CourtOrderNumber ?? "";

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligor))
      {
        export.CashReceiptDetail.ObligorPersonNumber =
          export.HiddenNextTranInfo.CsePersonNumberObligor ?? "";
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.CashReceiptDetail.ObligorPersonNumber =
          export.HiddenNextTranInfo.CsePersonNumber ?? "";
      }

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "CRRC"))
    {
      ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

      return;
    }

    // to validate action level security
    // 05/01/00        pphinney        H00093903 - Changed so that "ADJ" key 
    // causes flow thru security.  Removed from list to bypass.
    // 06/02/00        pphinney        H00096632    Changed logic to BLOCK 
    // unauthorized
    // MADE "RESEARCH" flow thru security
    if (Equal(global.Command, "REFUND") || Equal(global.Command, "FEES") || Equal
      (global.Command, "NOTES") || Equal(global.Command, "COLA") || Equal
      (global.Command, "RETMCOL") || Equal(global.Command, "RETRCOL") || Equal
      (global.Command, "RETCLCT") || Equal(global.Command, "RETCRDL") || Equal
      (global.Command, "RETCRUC") || Equal(global.Command, "MTRN") || Equal
      (global.Command, "RETLINK") || Equal(global.Command, "FLOWSTO"))
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

    UseFnHardcodedCashReceipting1();

    // -------------------------------------------------
    // The following IF logic is for the command of PEND, trying to determine 
    // whether the user is actually trying to PEND/UNPEND
    // -------------------------------------------------
    if (Equal(global.Command, "PEND"))
    {
      // *** Changed logic to make error the collection amout prompt when there 
      // is no detail displayed.  Added logic to make error when no receipt is
      // displayed.  Sunya Sharp 11/9/98 ***
      if (export.CashReceipt.SequentialNumber == 0)
      {
        var field = GetField(export.CashReceipt, "sequentialNumber");

        field.Error = true;

        ExitState = "FN0000_DISPLAY_CRD_FIRST_B4_PEND";

        return;
      }

      if (export.CashReceiptDetail.SequentialIdentifier == 0)
      {
        var field = GetField(export.Collection, "promptField");

        field.Error = true;

        ExitState = "FN0000_DISPLAY_CRD_FIRST_B4_PEND";

        return;
      }

      if (export.Suspended.TotalCurrency == 0)
      {
        ExitState = "FN0000_CANT_PEND_UNPEND_DIST_COL";

        return;
      }

      if (ReadCashReceipt3())
      {
        if (!Equal(entities.MatchedForPersistent.CheckType, "CSE"))
        {
          ExitState = "FN0000_TRNSMITTL_ACTION_INVLD_RB";

          return;
        }

        // *** Added logic to check to see if the cash receipt is in the correct
        // status to pend.  User should not be able to pend a detail that was
        // created on REIP.  Sunya Sharp 06/08/1999 ***
        if (ReadCashReceiptStatusHistoryCashReceiptStatus2())
        {
          // 09/23/02        pphinney        I00155999   Allow PEND of NON-Cash
        }
        else
        {
          UseEabRollbackCics();
          ExitState = "FN0000_CANT_PEND_CASH_RECEIPT";

          return;
        }
      }
      else
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }

      // Reading to see if collection type or cash receipt type is non-cash. 
      // Will not allow pend if either is non-cash. K Cole 7/31/01
      if (!IsEmpty(import.CollectionType2.Code))
      {
        if (ReadCollectionType())
        {
          MoveCollectionType(entities.CollectionType, export.CollectionType2);

          // 09/23/02        pphinney        I00155999   Allow PEND of NON-Cash
          ReadCashReceiptType2();
        }
        else
        {
          var field = GetField(export.CollectionType2, "code");

          field.Error = true;

          ExitState = "FN0000_COLLECTION_TYPE_NF";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_CASH_RCPT_DTL_COL_TYP_RQD";

        var field = GetField(export.CollectionType2, "code");

        field.Error = true;

        return;
      }

      if (export.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedPended.SystemGeneratedIdentifier)
      {
        // --------------------------------------------------
        // The user is trying to unpend the collection. So flow thru the update 
        // logic to verify all the necessary fields are valid
        // --------------------------------------------------
        local.ToBeUnpended.Flag = "Y";
        global.Command = "UPDATE";
      }
    }

    // *** This logic is repeated in the display logic to display the correct 
    // information for the detail requested for display.  Sunya Sharp 10/16/98 *
    // **
    // *** This logic was only needed for add and update.  All other processes 
    // did not require this action.  Sunya Sharp 10/27/98 ***
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
      {
        local.CsePersonsWorkSet.Number =
          export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
        UseSiReadCsePerson2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CashReceiptDetail, "obligorPersonNumber");

          field.Error = true;

          return;
        }

        export.CashReceiptDetail.ObligorLastName =
          local.CsePersonsWorkSet.LastName;
        export.CashReceiptDetail.ObligorFirstName =
          local.CsePersonsWorkSet.FirstName;
        export.CashReceiptDetail.ObligorMiddleName =
          local.CsePersonsWorkSet.MiddleInitial;

        if (!IsEmpty(export.CashReceiptDetail.ObligorSocialSecurityNumber) && Verify
          (export.CashReceiptDetail.ObligorSocialSecurityNumber, "0") != 0)
        {
          if (!Equal(export.CashReceiptDetail.ObligorSocialSecurityNumber,
            local.CsePersonsWorkSet.Ssn))
          {
            var field =
              GetField(export.CashReceiptDetail, "obligorSocialSecurityNumber");
              

            field.Error = true;

            ExitState = "FN0000_SSN_AND_CSE_NO_DONT_MATCH";

            return;
          }
        }

        if (Verify(local.CsePersonsWorkSet.Ssn, "0") == 0)
        {
          export.CashReceiptDetail.ObligorSocialSecurityNumber = "";
        }
        else
        {
          export.CashReceiptDetail.ObligorSocialSecurityNumber =
            local.CsePersonsWorkSet.Ssn;
        }

        // *** Add logic to get the phone number for the obligor. Sunya Sharp 10
        // /27/98 ***
        export.CashReceiptDetail.ObligorPhoneNumber =
          NumberToString(local.Phone.HomePhoneAreaCode.GetValueOrDefault(), 13,
          3) + NumberToString(local.Phone.HomePhone.GetValueOrDefault(), 9, 7);
      }

      if (!IsEmpty(export.CashReceiptDetail.ObligorSocialSecurityNumber) && IsEmpty
        (export.CashReceiptDetail.ObligorPersonNumber))
      {
        local.CsePersonsWorkSet.Ssn =
          export.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces(9);
        UseFnReadCsePersonUsingSsnO();

        // *** Logic was checking to see if not equal to all ok.  There are not 
        // exitstates returned from the eab.  Sunya Sharp 10/26/98 ***
        if (IsEmpty(local.CsePersonsWorkSet.Number))
        {
          ExitState = "FN0000_CRD_OBLIGOR_SSN_INVALID";

          var field =
            GetField(export.CashReceiptDetail, "obligorSocialSecurityNumber");

          field.Error = true;

          return;
        }

        export.CashReceiptDetail.ObligorPersonNumber =
          local.CsePersonsWorkSet.Number;
        export.CashReceiptDetail.ObligorLastName =
          local.CsePersonsWorkSet.LastName;
        export.CashReceiptDetail.ObligorFirstName =
          local.CsePersonsWorkSet.FirstName;
        export.CashReceiptDetail.ObligorMiddleName =
          local.CsePersonsWorkSet.MiddleInitial;

        // *** Add logic to get the phone number for the obligor. Sunya Sharp 10
        // /27/98 ***
        UseSiReadCsePerson1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CashReceiptDetail, "obligorPersonNumber");

          field.Error = true;

          return;
        }
        else
        {
          export.CashReceiptDetail.ObligorPhoneNumber =
            NumberToString(local.Phone.HomePhoneAreaCode.GetValueOrDefault(),
            13, 3) + NumberToString
            (local.Phone.HomePhone.GetValueOrDefault(), 9, 7);
        }
      }

      if (!IsEmpty(export.CashReceiptDetail.CourtOrderNumber))
      {
        // --------------------------------------------------
        // Validate the existance of Court Order and check whether the Ct. Order
        // is Multi-Payor
        // --------------------------------------------------
        UseFnAbObligorListForCtOrder();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (local.WorkNoOfObligors.Count > 1)
          {
            export.WorkIsMultiPayor.Flag = "Y";
            local.Female.Count = 0;
            local.Male.Count = 0;
            local.MultipayorOk.Flag = "";

            if (AsChar(export.CashReceiptDetail.MultiPayor) == 'M')
            {
              local.WorkSex.Sex = "F";
            }
            else if (AsChar(export.CashReceiptDetail.MultiPayor) == 'F')
            {
              local.WorkSex.Sex = "M";
            }
            else
            {
              var field = GetField(export.CashReceiptDetail, "multiPayor");

              field.Error = true;

              ExitState = "FN0000_MULT_PAYOR_IDENTIFY_PAYOR";

              return;
            }

            // *** PR# 78962 - Adding logic to support a multi-payor case with 2
            // fathers or 2 mothers.  Sunya Sharp 12/13/1999 ***
            for(local.ObligorList.Index = 0; local.ObligorList.Index < local
              .ObligorList.Count; ++local.ObligorList.Index)
            {
              if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == 'M')
              {
                ++local.Male.Count;
              }

              if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == 'F')
              {
                ++local.Female.Count;
              }
            }

            if (local.Male.Count > 1 && IsEmpty
              (export.CashReceiptDetail.ObligorPersonNumber))
            {
              var field =
                GetField(export.CashReceiptDetail, "obligorPersonNumber");

              field.Error = true;

              ExitState = "FN0000_MULTIPAYOR_NEEDS_CSE_NBR";

              return;
            }

            if (local.Female.Count > 1 && IsEmpty
              (export.CashReceiptDetail.ObligorPersonNumber))
            {
              var field =
                GetField(export.CashReceiptDetail, "obligorPersonNumber");

              field.Error = true;

              ExitState = "FN0000_MULTIPAYOR_NEEDS_CSE_NBR";

              return;
            }

            if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
            {
              for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                .ObligorList.Count; ++local.ObligorList.Index)
              {
                if (Equal(local.ObligorList.Item.GrpsWork.Number,
                  export.CashReceiptDetail.ObligorPersonNumber))
                {
                  if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == AsChar
                    (local.WorkSex.Sex))
                  {
                    export.CashReceiptDetail.ObligorPersonNumber =
                      local.ObligorList.Item.GrpsWork.Number;
                    export.CashReceiptDetail.ObligorSocialSecurityNumber =
                      local.ObligorList.Item.GrpsWork.Ssn;
                    export.CashReceiptDetail.ObligorLastName =
                      local.ObligorList.Item.GrpsWork.LastName;
                    export.CashReceiptDetail.ObligorFirstName =
                      local.ObligorList.Item.GrpsWork.FirstName;
                    export.CashReceiptDetail.ObligorMiddleName =
                      local.ObligorList.Item.GrpsWork.MiddleInitial;

                    // *** Add logic to get the phone number for the obligor. 
                    // Sunya Sharp 10/27/98 ***
                    export.CashReceiptDetail.ObligorPhoneNumber =
                      NumberToString(local.ObligorList.Item.Grps.
                        HomePhoneAreaCode.GetValueOrDefault(), 13, 3) + NumberToString
                      (local.ObligorList.Item.Grps.HomePhone.
                        GetValueOrDefault(), 9, 7);
                    local.MultipayorOk.Flag = "Y";

                    break;
                  }
                  else
                  {
                    // *** Loops to next person or Falls to error logic ***
                  }
                }
                else
                {
                  // *** Loops to next person or Falls to error logic ***
                }
              }

              if (AsChar(local.MultipayorOk.Flag) != 'Y')
              {
                var field =
                  GetField(export.CashReceiptDetail, "obligorPersonNumber");

                field.Error = true;

                ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                return;
              }
            }
            else
            {
              for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                .ObligorList.Count; ++local.ObligorList.Index)
              {
                if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == AsChar
                  (local.WorkSex.Sex))
                {
                  if (!Equal(local.ObligorList.Item.GrpsWork.Number,
                    export.CashReceiptDetail.ObligorPersonNumber) && !
                    IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "obligorPersonNumber");
                      

                    field.Error = true;

                    ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                    return;
                  }

                  export.CashReceiptDetail.ObligorPersonNumber =
                    local.ObligorList.Item.GrpsWork.Number;
                  export.CashReceiptDetail.ObligorSocialSecurityNumber =
                    local.ObligorList.Item.GrpsWork.Ssn;
                  export.CashReceiptDetail.ObligorLastName =
                    local.ObligorList.Item.GrpsWork.LastName;
                  export.CashReceiptDetail.ObligorFirstName =
                    local.ObligorList.Item.GrpsWork.FirstName;
                  export.CashReceiptDetail.ObligorMiddleName =
                    local.ObligorList.Item.GrpsWork.MiddleInitial;

                  // *** Add logic to get the phone number for the obligor. 
                  // Sunya Sharp 10/27/98 ***
                  export.CashReceiptDetail.ObligorPhoneNumber =
                    NumberToString(local.ObligorList.Item.Grps.
                      HomePhoneAreaCode.GetValueOrDefault(), 13, 3) + NumberToString
                    (local.ObligorList.Item.Grps.HomePhone.GetValueOrDefault(),
                    9, 7);

                  break;
                }
              }
            }
          }
          else if (local.WorkNoOfObligors.Count == 1)
          {
            for(local.ObligorList.Index = 0; local.ObligorList.Index < local
              .ObligorList.Count; ++local.ObligorList.Index)
            {
              if (!Equal(local.ObligorList.Item.GrpsWork.Number,
                export.CashReceiptDetail.ObligorPersonNumber))
              {
                if (IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                {
                  export.WorkIsMultiPayor.Flag = "N";
                  export.CashReceiptDetail.ObligorPersonNumber =
                    local.ObligorList.Item.GrpsWork.Number;
                  export.CashReceiptDetail.ObligorSocialSecurityNumber =
                    local.ObligorList.Item.GrpsWork.Ssn;
                  export.CashReceiptDetail.ObligorLastName =
                    local.ObligorList.Item.GrpsWork.LastName;
                  export.CashReceiptDetail.ObligorFirstName =
                    local.ObligorList.Item.GrpsWork.FirstName;
                  export.CashReceiptDetail.ObligorMiddleName =
                    local.ObligorList.Item.GrpsWork.MiddleInitial;

                  // *** Add logic to get the phone number for the obligor. 
                  // Sunya Sharp 10/27/98 ***
                  export.CashReceiptDetail.ObligorPhoneNumber =
                    NumberToString(local.ObligorList.Item.Grps.
                      HomePhoneAreaCode.GetValueOrDefault(), 13, 3) + NumberToString
                    (local.ObligorList.Item.Grps.HomePhone.GetValueOrDefault(),
                    9, 7);
                }
                else
                {
                  var field =
                    GetField(export.CashReceiptDetail, "obligorPersonNumber");

                  field.Error = true;

                  ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                  return;
                }
              }
            }
          }
          else
          {
            var field = GetField(export.CashReceiptDetail, "courtOrderNumber");

            field.Error = true;

            ExitState = "FN0000_NO_OBLIGORS_FOR_CT_ORDER";

            return;
          }
        }
        else
        {
          return;
        }
      }
      else if (!IsEmpty(export.CashReceiptDetail.MultiPayor))
      {
        var field = GetField(export.CashReceiptDetail, "multiPayor");

        field.Error = true;

        ExitState = "MULTI_PAYOR_INVALID_WO_CT_ORDER";

        return;
      }
      else
      {
        export.WorkIsMultiPayor.Flag = "N";

        var field = GetField(export.CashReceiptDetail, "multiPayor");

        field.Color = "cyan";
        field.Protected = true;
      }

      if (Verify(export.CashReceiptDetail.ObligorPhoneNumber, " 0") == 0)
      {
        export.CashReceiptDetail.ObligorPhoneNumber = "";
      }
    }

    // *** PR# 85804 - Moved logic for the return from CDVL so that it can fall 
    // into the flow to logic if needed.  Sunya Sharp 2/2/2000 ***
    if (Equal(global.Command, "RETCDVL"))
    {
      // *** Added logic to check if what is passed to CRRC is not blank before 
      // overriding what was there before.  Sunya Sharp 10/29/98 ***
      // *** PR# 85804 - Add logic to support new prompt with the flow to 
      // function.  Sunya Sharp 2/2/2000 ***
      if (!IsEmpty(import.CodeValue.Cdvalue))
      {
        if (Equal(export.Code.CodeName, "PEND/SUSP REASON"))
        {
          export.CashReceiptDetailStatHistory.ReasonCodeId =
            import.CodeValue.Cdvalue;

          return;
        }

        if (Equal(export.Code.CodeName, "FLOW FROM CRRC"))
        {
          export.Pf17FlowTo.NextTransaction = import.CodeValue.Cdvalue;
          global.Command = "FLOWSTO";
        }
      }
      else
      {
        return;
      }
    }

    // *** PR# 85804 - Add logic for 'Flow to'.  Sunya Sharp 2/1/2000 ***
    if (Equal(global.Command, "FLOWSTO"))
    {
      switch(TrimEnd(export.Pf17FlowTo.NextTransaction))
      {
        case "COLA":
          // *** Removed logic to flow to MCOL from a PF key and added logic to 
          // flow to COLA from a PF key.  Sunya Sharp 10/24/98 ***
          export.Pf17FlowTo.NextTransaction = "";
          ExitState = "ECO_LNK_TO_REC_COLL_ADJMNT";

          return;
        case "COLL":
          if ((export.HiddenCashReceiptEvent.SystemGeneratedIdentifier == 0 || export
            .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
            .HiddenCashReceiptType.SystemGeneratedIdentifier == 0) && export
            .CashReceipt.SequentialNumber == 0)
          {
            var field = GetField(export.CashReceipt, "sequentialNumber");

            field.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            return;
          }

          if (ReadCollection())
          {
            export.CsePerson.Number =
              export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
            export.Pf17FlowTo.NextTransaction = "";
            ExitState = "ECO_LNK_TO_LST_COLL_BY_AP_PYR";

            return;
          }
          else
          {
            var field = GetField(export.Pf17FlowTo, "nextTransaction");

            field.Error = true;

            ExitState = "FN0000_NO_COLL_TO_FLOW_TO_COLL";

            return;
          }

          break;
        case "CRDA":
          // *** Changed logic and dialog flow to go to CRDA instead of CRAJ.  
          // Sunya Sharp 6/7/1999 ***
          if (Equal(export.CashReceiptDetailStatus.Code, "ADJ"))
          {
            if (ReadCashReceipt2())
            {
              export.ToCrda.SequentialNumber =
                entities.AdjustCashReceipt.SequentialNumber;
              export.Pf17FlowTo.NextTransaction = "";
              ExitState = "ECO_LNK_TO_CRDA";

              return;
            }
            else
            {
              ExitState = "FN0000_NOT_INTERFACE_ADJ";

              return;
            }
          }
          else
          {
            ExitState = "FN0000_MUST_BE_IN_ADJ_STATUS";

            return;
          }

          break;
        case "MCOL":
          if (Equal(export.CashReceiptDetailStatus.Code, "REL") || Equal
            (export.CashReceiptDetailStatus.Code, "DIST") || Equal
            (export.CashReceiptDetailStatus.Code, "REF"))
          {
            if (export.Suspended.TotalCurrency > 0)
            {
              export.Pf17FlowTo.NextTransaction = "";
              ExitState = "ECO_LNK_TO_MANUAL_DIST_OF_COLL";

              return;
            }
            else
            {
              ExitState = "FN0000_NO_UNDIST_FLOW_TO_MCOL";

              return;
            }
          }
          else if (Equal(export.CashReceiptDetailStatus.Code, "SUSP") && (
            Equal(export.CashReceiptDetailStatHistory.ReasonCodeId, "MANUALDIST")
            || Equal
            (export.CashReceiptDetailStatHistory.ReasonCodeId, "MANREAPPLY")))
          {
            local.SkipManDistRead.Flag = "Y";
            export.Pf17FlowTo.NextTransaction = "";
            global.Command = "RELEASE";
          }
          else
          {
            ExitState = "FN0000_CANT_FLOW_TO_MCOL";

            return;
          }

          break;
        case "MTRN":
          // *** Add new flow to MTRN.  Sunya Sharp 5/26/1999 ***
          // *** TDR #94.  Add logic to ensure that the check/EFT number is all 
          // numeric before flowing to MTRN.  If not, return error message to
          // the screen.  Sunya Sharp 08/10/1999. ***
          if (Verify(export.CashReceipt.CheckNumber, " 0123456789") == 0)
          {
            export.ToMtrn.TransmissionType = "I";
            export.ToMtrn.TransmissionIdentifier =
              (int)StringToNumber(export.CashReceipt.CheckNumber);
            ExitState = "ECO_LNK_TO_MTRN";
            export.Pf17FlowTo.NextTransaction = "";

            return;
          }
          else
          {
            var field = GetField(export.CashReceipt, "checkNumber");

            field.Error = true;

            ExitState = "FN0000_EFT_NUMBER_INVALID";

            return;
          }

          break;
        case "OPAY":
          if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
          {
            export.FlowTo.Number =
              export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
            export.Pf17FlowTo.NextTransaction = "";
            ExitState = "ECO_LNK_LST_OPAY_OBLG_BY_AP";

            return;
          }
          else
          {
            var field =
              GetField(export.CashReceiptDetail, "obligorPersonNumber");

            field.Error = true;

            ExitState = "CSE_PERSON_NO_REQUIRED";

            return;
          }

          break;
        case "PACC":
          if (ReadCashReceiptDetail1())
          {
            foreach(var item in ReadCsePerson())
            {
              if (IsEmpty(local.CsePerson.Number))
              {
                local.CsePerson.Number = entities.CsePerson.Number;
              }
              else if (!Equal(local.CsePerson.Number, entities.CsePerson.Number) &&
                !IsEmpty(entities.CsePerson.Number))
              {
                var field = GetField(export.Pf17FlowTo, "nextTransaction");

                field.Error = true;

                ExitState = "FN0000_FLOW_TO_COLL_FIRST";

                return;
              }
            }

            if (!IsEmpty(local.CsePerson.Number))
            {
              export.CsePerson.Number = local.CsePerson.Number;
              export.FlowPaccEndDate.Date =
                export.CashReceiptDetail.CollectionDate;
              export.FlowPaccStartDate.Date =
                AddMonths(export.CashReceiptDetail.CollectionDate, -1);
              UseCabFirstAndLastDateOfMonth();
              export.Pf17FlowTo.NextTransaction = "";
              ExitState = "ECO_LNK_TO_LST_APACC";

              return;
            }
            else
            {
              var field = GetField(export.Pf17FlowTo, "nextTransaction");

              field.Error = true;

              ExitState = "FN0000_CANNOT_FLOW_TO_PACC";

              return;
            }
          }
          else
          {
            var field = GetField(export.CashReceipt, "sequentialNumber");

            field.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            return;
          }

          break;
        case "PAYR":
          // 03/08/00        pphinney        H00090300 - Added PAYR to PF22 Flow
          if ((export.HiddenCashReceiptEvent.SystemGeneratedIdentifier == 0 || export
            .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
            .HiddenCashReceiptType.SystemGeneratedIdentifier == 0) && export
            .CashReceipt.SequentialNumber == 0 || export
            .CashReceipt.SequentialNumber != export
            .HiddenCashReceipt.SequentialNumber)
          {
            var field = GetField(export.CashReceipt, "sequentialNumber");

            field.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            return;
          }

          if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
          {
            if (Lt(new DateTime(1900, 1, 1),
              export.CashReceiptDetail.CollectionDate))
            {
              export.FlowPaccEndDate.Date =
                export.CashReceiptDetail.CollectionDate;
              export.FlowPaccStartDate.Date =
                AddMonths(export.CashReceiptDetail.CollectionDate, -1);
              UseCabFirstAndLastDateOfMonth();
            }
            else
            {
              export.FlowPaccEndDate.Date = local.Initial.Date;
              export.FlowPaccStartDate.Date = local.Initial.Date;
            }

            export.CsePerson.Number =
              export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
            export.Pf17FlowTo.NextTransaction = "";
            ExitState = "ECO_XFR_TO_PAYR";

            return;
          }
          else
          {
            var field =
              GetField(export.CashReceiptDetail, "obligorPersonNumber");

            field.Error = true;

            ExitState = "CSE_PERSON_NO_REQUIRED";

            return;
          }

          break;
        default:
          break;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "ADJUST":
        // ----------------------------------------------------------------------------
        // Per PR# 105882 : The "PEND" code is included in the below IF 
        // statement.
        //                                                   
        // ----- Vithal (05/31/2001)
        // ----------------------------------------------------------------------------
        if (Equal(export.CashReceiptDetailStatus.Code, "SUSP") || Equal
          (export.CashReceiptDetailStatus.Code, "REF") || Equal
          (export.CashReceiptDetailStatus.Code, "PEND"))
        {
          if (export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() > 0
            )
          {
            ExitState = "FN0000_CANNOT_ADJUST_WITH_DIST";

            return;
          }

          local.Current.Day = Day(local.Current.Date);
          local.Current.Month = Month(local.Current.Date);
          local.Current.Year = Year(local.Current.Date);
          local.CurrentDateText.Text10 =
            NumberToString(local.Current.Month, 14, 2) + "/" + NumberToString
            (local.Current.Day, 14, 2) + "/" + NumberToString
            (local.Current.Year, 12, 4);
          export.CashReceiptDetail.Notes =
            TrimEnd(export.CashReceiptDetail.Notes) + " CASH RECEIPT DETAIL WAS ADJUSTED ON " +
            local.CurrentDateText.Text10 + " BY CRRC.";
          local.AdjustInUpdateCab.Flag = "Y";

          if (export.CashReceiptDetail.RefundedAmount.GetValueOrDefault() > 0)
          {
            local.AdjustWithRefundOk.Flag = "Y";
          }
          else
          {
            local.AdjustOk.Flag = "Y";
          }

          UseUpdateCollection2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          UseFnChangeCashRcptDtlStatHis2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
          else
          {
            global.Command = "DISPLAY";
          }
        }
        else
        {
          var field = GetField(export.CashReceiptDetailStatus, "code");

          field.Error = true;

          ExitState = "FN0000_CRD_NOT_STATUS_TO_ADJUST";

          return;
        }

        break;
      case "RETLINK":
        // *** Added return for MTRN.  The return exitstate has a message and 
        // this is to prevent this from appearing.  Sunya Sharp 5/26/1999 ***
        // *** Using for return from CRDA  Sunya Sharp 6/7/1999 ***
        return;
      case "RETCRUC":
        // *** Added return for CRUC.  The return exitstate has a message and 
        // this is to prevent this from appearing.  Sunya Sharp 10/30/98 ***
        return;
      case "RETCLCT":
        // *** Added new view and logic to check if what is passed to CRRC is 
        // not blank before overriding what was there before.  Sunya Sharp 10/29
        // /98 ***
        if (!IsEmpty(import.PassFromClct.Code))
        {
          MoveCollectionType(import.PassFromClct, export.CollectionType2);
        }

        return;
      case "RETCRDL":
        // *** Added new view and logic to check if what is passed to CRRC is 
        // not blank before overriding what was there before.  Changed dialog
        // flow to now be returning command of RETCRDL instead of DISPLAY.
        // Sunya Sharp 10/29/98 ***
        if (import.PassFromCrdlCashReceiptDetail.SequentialIdentifier > 0)
        {
          export.CashReceipt.Assign(import.PassFromCrdlCashReceipt);
          export.CashReceiptDetail.SequentialIdentifier =
            import.PassFromCrdlCashReceiptDetail.SequentialIdentifier;
          export.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
            import.PassFromCrdlCashReceiptEvent.SystemGeneratedIdentifier;
          export.CashReceiptSourceType.Assign(
            import.PassFromCrdlCashReceiptSourceType);
          export.HiddenCashReceiptType.SystemGeneratedIdentifier =
            import.PassFromCrdlCashReceiptType.SystemGeneratedIdentifier;
        }

        global.Command = "DISPLAY";

        break;
      case "RETMCOL":
        // --------------------------------------------------
        // This read is necessary to achieve concurrency of Cash_receipt down 
        // below for command of CRRC
        // --------------------------------------------------
        if (ReadCashReceipt1())
        {
          local.Retmcol.Flag = "Y";
          global.Command = "CRRC";
        }
        else
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }

        break;
      case "RETRCOL":
        return;
      case "":
        break;
      case "DISPLAY":
        // Display logic at bottom of PrAD
        break;
      case "ADD":
        if (export.HiddenCashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .HiddenCashReceiptType.SystemGeneratedIdentifier == 0)
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "FN0136_CASH_RCPT_REQD_ADD_UPD";

          return;
        }

        if (AsChar(export.DeletedCashReceiptFlag.Flag) == 'Y')
        {
          ExitState = "FN0000_CASH_RECEIPT_DELETED";

          return;
        }

        if (ReadCashReceipt3())
        {
          if (!Equal(entities.MatchedForPersistent.CheckType, "CSE"))
          {
            ExitState = "FN0000_TRNSMITTL_ACTION_INVLD_RB";

            return;
          }

          // *** Removed logic to check to see if the cash receipt source type 
          // is court, FDSO, or SDSO.  Now the cash receipt must be in a status
          // of interface or deposit to be added.  Sunya Sharp 11/3/98 ***
          if (ReadCashReceiptStatusHistoryCashReceiptStatus2())
          {
            if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedDeposited.SystemGeneratedIdentifier || entities
              .CashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedInterface.SystemGeneratedIdentifier)
            {
            }
            else
            {
              UseEabRollbackCics();
              ExitState = "FN0000_CANT_ADD_UPD_REL_CR_IN";

              return;
            }
          }
          else
          {
            UseEabRollbackCics();
            ExitState = "FN0000_CANT_ADD_UPD_REL_CR_IN";

            return;
          }

          // --- Validate interface tran-id, if the source type is interfaced, 
          // this is a mandatory field ---
          // : Interface Id not necessary for manual Interfaces. PR # 37790 02/
          // 09/98 Venkatesh
          // *** This is no longer valid... Removing per screen assessment. 
          // Sunya Sharp 11/3/98 ***
          // *** Added logic to also check to ensure that the cash receipt is 
          // from a court.  Sunya Sharp 11/3/98 ***
          if (ReadCashReceiptSourceType())
          {
            if (AsChar(entities.InterfaceCashReceiptSourceType.
              InterfaceIndicator) == 'Y' && AsChar
              (entities.InterfaceCashReceiptSourceType.CourtInd) == 'C')
            {
              if (ReadCashReceiptType1())
              {
                if (Equal(entities.InterfaceCashReceiptType.Code, "MANINT"))
                {
                  if (IsEmpty(export.CashReceiptDetail.InterfaceTransId))
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "interfaceTransId");

                    field.Error = true;

                    ExitState = "FN0000_MANDATORY_FIELDS";

                    return;
                  }
                  else if (ReadCashReceiptDetail2())
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "interfaceTransId");

                    field.Error = true;

                    ExitState = "FN0000_INTERFACE_TRAN_ID_AE";

                    return;
                  }
                }
              }
              else
              {
                ExitState = "FN0000_CASH_RECEIPT_TYPE_NF";

                return;
              }
            }
          }
          else
          {
            ExitState = "FN0096_CASH_RCPT_SOURCE_NF";

            return;
          }

          export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedRecorded.SystemGeneratedIdentifier;
          local.CollAmtApplied.TotalCurrency = 0;
          UseFnAbDetermineCollAmtApplied3();

          // *****
          // Before recording collection determine if the amount will be greater
          // than the cash receipt amount.  This logic has to be performed
          // before entering the Record Collection CAB as the Record Collection
          // CAB is used with batch programs whose Cash Receipt is not updated
          // with the appropriate receipt amount until all the details are
          // processed.
          // *****
          local.CollAmtApplied.TotalCurrency += export.CashReceiptDetail.
            ReceivedAmount;

          // *** Added logic to ensure the correct fields are compared.  The 
          // court can have a receipt that was an interface or a regular cash
          // receipt.  Sunya Sharp 2/6/1999 ***
          // *** Add logic to allow area offices to work the same as courts.  
          // But do not change the way that FDSO and SDSO are working.  System
          // Test PR# 101.  Sunya Sharp 07/10/1999 ***
          if (AsChar(entities.InterfaceCashReceiptSourceType.InterfaceIndicator) ==
            'Y')
          {
            if (Lt(0, entities.MatchedForPersistent.TotalCashTransactionAmount))
            {
              if (Lt(entities.MatchedForPersistent.TotalCashTransactionAmount,
                local.CollAmtApplied.TotalCurrency))
              {
                var field1 =
                  GetField(export.CashReceiptDetail, "receivedAmount");

                field1.Error = true;

                var field2 =
                  GetField(export.CashReceipt, "totalCashTransactionAmount");

                field2.Error = true;

                var field3 = GetField(export.CollAmtApplied, "totalCurrency");

                field3.Error = true;

                ExitState = "FN0000_CRDTL_AMT_EXCEEDS_INTRFAC";

                return;
              }
            }
            else if (entities.MatchedForPersistent.ReceiptAmount < local
              .CollAmtApplied.TotalCurrency)
            {
              var field1 = GetField(export.CashReceiptDetail, "receivedAmount");

              field1.Error = true;

              var field2 = GetField(export.CollAmtApplied, "totalCurrency");

              field2.Error = true;

              var field3 = GetField(export.CashReceipt, "receiptAmount");

              field3.Error = true;

              ExitState = "FN0000_COLL_AMT_GT_REC_AMT";

              return;
            }
          }
          else if (entities.MatchedForPersistent.ReceiptAmount < local
            .CollAmtApplied.TotalCurrency)
          {
            var field1 = GetField(export.CashReceiptDetail, "receivedAmount");

            field1.Error = true;

            var field2 = GetField(export.CollAmtApplied, "totalCurrency");

            field2.Error = true;

            var field3 = GetField(export.CashReceipt, "receiptAmount");

            field3.Error = true;

            ExitState = "FN0000_COLL_AMT_GT_REC_AMT";

            return;
          }
        }
        else
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }

        if (import.CashReceiptDetail.CollectionAmount <= 0)
        {
          ExitState = "FN0000_AMT_CANT_BE_ZERO_OR_NEG";

          var field = GetField(export.CashReceiptDetail, "collectionAmount");

          field.Error = true;

          return;
        }

        if (import.CashReceiptDetail.ReceivedAmount <= 0)
        {
          ExitState = "FN0000_AMT_CANT_BE_ZERO_OR_NEG";

          var field = GetField(export.CashReceiptDetail, "receivedAmount");

          field.Error = true;

          return;
        }

        if (export.CashReceiptDetail.CollectionAmount >= export
          .CashReceiptDetail.ReceivedAmount)
        {
          if (export.CashReceiptDetail.CollectionAmount > export
            .CashReceiptDetail.ReceivedAmount)
          {
            local.FeesRequired.Flag = "Y";
          }
        }
        else
        {
          ExitState = "FN0000_COLL_AMT_LT_RECEIVED_AMT";

          var field1 = GetField(export.CashReceiptDetail, "collectionAmount");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetail, "receivedAmount");

          field2.Error = true;

          return;
        }

        MoveCashReceiptDetail3(export.CashReceiptDetail,
          local.PassAreaCashReceiptDetail);

        if (Equal(import.CashReceiptDetail.CollectionDate, null))
        {
          export.CashReceiptDetail.CollectionDate = Now().Date;
          local.PassAreaCashReceiptDetail.CollectionDate = Now().Date;
          local.PassAreaCashReceiptDetail.DefaultedCollectionDateInd = "Y";
        }
        else if (Lt(Now().Date, import.CashReceiptDetail.CollectionDate))
        {
          var field = GetField(export.CashReceiptDetail, "collectionDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (!IsEmpty(import.CollectionType2.Code))
        {
          if (ReadCollectionType())
          {
            MoveCollectionType(entities.CollectionType, export.CollectionType2);

            // 10/27/00        pphinney        I00106505    Verify Collection 
            // Type
            // Done this way so we ALWAYS attempt to Read Cash_Receipt_Type BUT 
            // above original logic is NOT affected
            ReadCashReceiptType2();

            if (AsChar(entities.Verify.CategoryIndicator) == 'C' && AsChar
              (entities.CollectionType.CashNonCashInd) != 'C')
            {
              var field = GetField(export.CollectionType2, "code");

              field.Error = true;

              ExitState = "FN0000_CASH_CR_NON_CASH_CRD";

              return;
            }

            if (AsChar(entities.Verify.CategoryIndicator) == 'N' && AsChar
              (entities.CollectionType.CashNonCashInd) != 'N')
            {
              var field = GetField(export.CollectionType2, "code");

              field.Error = true;

              ExitState = "FN0000_NON_CASH_CR_CASH_CRD";

              return;
            }

            // 11/25/2014 GVandy CQ42192  "C" collection type must be used with 
            // CSSI source type.
            if (!Equal(import.CollectionType2.Code, "C") && Equal
              (import.CashReceiptSourceType.Code, "CSSI"))
            {
              var field = GetField(export.CollectionType2, "code");

              field.Error = true;

              ExitState = "FN0000_CSSI_SOURCE_W_C_COLL_TYP";

              return;
            }
          }
          else
          {
            var field = GetField(export.CollectionType2, "code");

            field.Error = true;

            ExitState = "FN0000_COLLECTION_TYPE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_CASH_RCPT_DTL_COL_TYP_RQD";

          var field = GetField(export.CollectionType2, "code");

          field.Error = true;

          return;
        }

        // *** Moved manditory check after validation of amounts, type and date.
        // Sunya Sharp 11/06/98 ***
        if (IsEmpty(export.CashReceiptDetail.CourtOrderNumber) && IsEmpty
          (export.CashReceiptDetail.ObligorPersonNumber) && IsEmpty
          (export.CashReceiptDetail.ObligorSocialSecurityNumber))
        {
          if (IsEmpty(export.CashReceiptDetailStatHistory.ReasonCodeId))
          {
            ExitState = "FN0000_PEND_REASON_CODE_NEEDED";

            var field =
              GetField(export.CashReceiptDetailStatHistory, "reasonCodeId");

            field.Error = true;

            return;
          }
        }

        // *****
        // Validate Pending reason code
        // *****
        if (!IsEmpty(export.CashReceiptDetailStatHistory.ReasonCodeId))
        {
          local.Pass.Cdvalue =
            export.CashReceiptDetailStatHistory.ReasonCodeId ?? Spaces(10);
          local.Code.CodeName = "PEND/SUSP REASON";
          UseCabValidateCodeValue();

          if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
          {
            var field =
              GetField(export.CashReceiptDetailStatHistory, "reasonCodeId");

            field.Error = true;

            ExitState = "PENDING_REASON_CODE_INVALID";

            return;
          }
          else if (!Equal(export.CashReceiptDetailStatHistory.ReasonCodeId,
            "RESEARCH"))
          {
            var field =
              GetField(export.CashReceiptDetailStatHistory, "reasonCodeId");

            field.Error = true;

            ExitState = "PENDING_REASON_CODE_INVALID";

            return;
          }
        }

        UseCabGenerateCrDetailSeqId();
        local.PassAreaCashReceiptDetail.InterfaceTransId =
          export.CashReceiptDetail.InterfaceTransId ?? "";

        // *** Move check for mandatory fields to the top of the add process.  
        // Also check to see if obligor person number is present to find
        // address.  Sunya Sharp 10/26/98 ***
        // *** Added logic to set the address to spaces if no person number was 
        // found.  This way if pending when adding the address from a previous
        // collection detail will be removed.  Sunya Sharp 10/29/98 ***
        if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
        {
          local.CsePerson.Number =
            export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
          UseSiGetCsePersonMailingAddr();

          if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
            (local.CsePersonAddress.City))
          {
            export.CashReceiptDetailAddress.Street1 = "No Active address found";
            export.CashReceiptDetailAddress.State = "KS";
            export.CashReceiptDetailAddress.Street2 = "";
            export.CashReceiptDetailAddress.City = "";
            export.CashReceiptDetailAddress.ZipCode5 = "";
            export.CashReceiptDetailAddress.ZipCode4 = "";
            export.CashReceiptDetailAddress.ZipCode3 = "";
          }
          else
          {
            export.CashReceiptDetailAddress.Street1 =
              local.CsePersonAddress.Street1 ?? Spaces(25);
            export.CashReceiptDetailAddress.Street2 =
              local.CsePersonAddress.Street2 ?? "";
            export.CashReceiptDetailAddress.City =
              local.CsePersonAddress.City ?? Spaces(30);
            export.CashReceiptDetailAddress.State =
              local.CsePersonAddress.State ?? Spaces(2);
            export.CashReceiptDetailAddress.ZipCode5 =
              local.CsePersonAddress.ZipCode ?? Spaces(5);
            export.CashReceiptDetailAddress.ZipCode4 =
              local.CsePersonAddress.Zip4 ?? "";
            export.CashReceiptDetailAddress.ZipCode3 =
              local.CsePersonAddress.Zip3 ?? "";
          }
        }
        else
        {
          local.PassAreaCashReceiptDetail.ObligorLastName = "";
          local.PassAreaCashReceiptDetail.ObligorFirstName = "";
          local.PassAreaCashReceiptDetail.ObligorMiddleName = "";
          local.PassAreaCashReceiptDetail.ObligorPhoneNumber = "";
          export.CashReceiptDetail.ObligorLastName = "";
          export.CashReceiptDetail.ObligorFirstName = "";
          export.CashReceiptDetail.ObligorMiddleName = "";
          export.CashReceiptDetail.ObligorPhoneNumber = "";
          export.CashReceiptDetailAddress.Street1 = "";
          export.CashReceiptDetailAddress.Street2 = "";
          export.CashReceiptDetailAddress.City = "";
          export.CashReceiptDetailAddress.State = "";
          export.CashReceiptDetailAddress.ZipCode5 = "";
          export.CashReceiptDetailAddress.ZipCode4 = "";
          export.CashReceiptDetailAddress.ZipCode3 = "";
        }

        UseRecordCollection();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CashReceiptDetail.LastUpdatedBy = global.UserId;
          ++export.TotalNoOfColl.Count;
          export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            entities.MatchForPersistent.SystemGeneratedIdentifier;
          export.Suspended.TotalCurrency =
            export.CashReceiptDetail.CollectionAmount - export
            .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - export
            .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
          export.OriginalCollection.AverageCurrency =
            export.CashReceiptDetail.CollectionAmount;
          export.CollAmtApplied.TotalCurrency =
            local.CollAmtApplied.TotalCurrency;
          export.CashReceiptLiterals.
            Assign(local.InitializeCashReceiptLiterals);

          // *** Changed logic to use export views.  These are what is populated
          // in getting the address from above.  Was not creating the address.
          // Sunya Sharp 10/27/98 ***
          if (!IsEmpty(export.CashReceiptDetailAddress.Street1) && !
            Equal(export.CashReceiptDetailAddress.Street1,
            "No Active address found") || !
            IsEmpty(export.CashReceiptDetailAddress.Street2) || !
            IsEmpty(export.CashReceiptDetailAddress.City) || !
            IsEmpty(export.CashReceiptDetailAddress.State) || !
            IsEmpty(export.CashReceiptDetailAddress.ZipCode3) || !
            IsEmpty(export.CashReceiptDetailAddress.ZipCode4) || !
            IsEmpty(export.CashReceiptDetailAddress.ZipCode5))
          {
            UseCreateCrDetailAddress2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field =
                GetField(export.CashReceiptDetail, "sequentialIdentifier");

              field.Error = true;

              return;
            }
          }

          // *** Add logic to allow area offices to work the same as courts.  
          // But do not change the way that FDSO and SDSO are working.  System
          // Test PR# 101.  Sunya Sharp 07/10/1999 ***
          if (IsEmpty(export.CashReceiptDetailStatHistory.ReasonCodeId))
          {
            export.CashReceiptDetailStatus.Code = "REC";

            if (AsChar(entities.InterfaceCashReceiptSourceType.
              InterfaceIndicator) == 'Y')
            {
              if (Lt(0, entities.MatchedForPersistent.TotalCashTransactionAmount))
                
              {
                if (Lt(local.CollAmtApplied.TotalCurrency,
                  entities.MatchedForPersistent.TotalCashTransactionAmount))
                {
                  ExitState = "FN0000_ADD_SUCCESSFUL_NEED_MORE";
                }
                else
                {
                  ExitState = "FN0000_ADD_SUCC_PRESS_PF16";
                }
              }
              else if (entities.MatchedForPersistent.ReceiptAmount > local
                .CollAmtApplied.TotalCurrency)
              {
                ExitState = "FN0000_ADD_SUCCESSFUL_NEED_MORE";
              }
              else
              {
                ExitState = "FN0000_ADD_SUCC_PRESS_PF16";
              }
            }
            else if (entities.MatchedForPersistent.ReceiptAmount > local
              .CollAmtApplied.TotalCurrency)
            {
              ExitState = "FN0000_ADD_SUCCESSFUL_NEED_MORE";
            }
            else
            {
              ExitState = "FN0000_ADD_SUCC_PRESS_PF16";
            }
          }
          else
          {
            UseFnChangeCashRcptDtlStatHis3();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.CashReceiptDetailStatus.Code = "PEND";
              ExitState = "FN0000_CR_DETAIL_ADDED_N_PENDED";
            }
            else
            {
              UseEabRollbackCics();

              return;
            }
          }

          MoveCashReceiptDetail4(export.CashReceiptDetail, export.Previous);

          if (AsChar(local.FeesRequired.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_REC_COLLECTION_FEES";

            return;
          }
        }
        else
        {
          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;

          return;
        }

        // 08/02/00        pphinney        H00098632    Changed logic to NOT 
        // allow change of
        // collection date if CRD is REL
        export.Save.CollectionDate = export.CashReceiptDetail.CollectionDate;

        // 08/21/00        pphinney        H00101532    Changed logic to NOT 
        // allow change of
        // collection TYPE if CRD is REL
        export.HiddenCollectionType.Code = export.CollectionType2.Code;

        break;
      case "UPDATE":
        if (export.HiddenCashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .HiddenCashReceiptType.SystemGeneratedIdentifier == 0)
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "FN0136_CASH_RCPT_REQD_ADD_UPD";

          return;
        }

        if (AsChar(export.DeletedCashReceiptFlag.Flag) == 'Y')
        {
          ExitState = "FN0000_CASH_RECEIPT_DELETED";

          return;
        }

        if (export.CashReceiptDetail.SequentialIdentifier == 0)
        {
          ExitState = "FN0000_CASH_RCPT_DTL_ID_MISSING";

          var field = GetField(export.Collection, "promptField");

          field.Error = true;

          return;
        }

        // *** Moved manditory check to the top of the update process.  Sunya 
        // Sharp 10/26/98 ***
        if (IsEmpty(export.CashReceiptDetail.CourtOrderNumber) && IsEmpty
          (export.CashReceiptDetail.ObligorPersonNumber) && IsEmpty
          (export.CashReceiptDetail.ObligorSocialSecurityNumber))
        {
          var field1 =
            GetField(export.CashReceiptDetail, "obligorPersonNumber");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetail, "courtOrderNumber");

          field2.Error = true;

          var field3 =
            GetField(export.CashReceiptDetail, "obligorSocialSecurityNumber");

          field3.Error = true;

          ExitState = "FN0000_OBLIGOR_NOT_IDNTFD_NO_ADD";

          return;
        }

        if (Equal(export.CashReceiptDetailStatus.Code, "DIST") || Equal
          (export.CashReceiptDetailStatus.Code, "REF") || Equal
          (export.CashReceiptDetailStatus.Code, "ADJ"))
        {
          ExitState = "FN0000_INVALID_STATUS_FOR_UPDATE";

          return;
        }

        // 08/02/00        pphinney        H00098632    Changed logic to NOT 
        // allow change of
        // collection date if CRD is REL
        if (!Equal(export.Save.CollectionDate,
          export.CashReceiptDetail.CollectionDate) && export
          .CashReceiptDetail.DistributedAmount.GetValueOrDefault() != 0)
        {
          var field = GetField(export.CashReceiptDetail, "collectionDate");

          field.Error = true;

          ExitState = "FN0000_COL_DATE_WITH_REL_CRD";

          return;
        }

        if (!Equal(export.Save.CollectionDate,
          export.CashReceiptDetail.CollectionDate) && export
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault() != 0)
        {
          var field = GetField(export.CashReceiptDetail, "collectionDate");

          field.Error = true;

          ExitState = "FN0000_COL_DATE_WITH_REL_CRD";

          return;
        }

        // 08/21/00        pphinney        H00101532    Changed logic to NOT 
        // allow change of
        // collection TYPE if CRD is REL
        if (!Equal(export.HiddenCollectionType.Code, export.CollectionType2.Code)
          && export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() != 0
          )
        {
          var field = GetField(export.CollectionType2, "code");

          field.Error = true;

          ExitState = "FN0000_COL_TYPE_WITH_REL_CRD";

          return;
        }

        if (!Equal(export.HiddenCollectionType.Code, export.CollectionType2.Code)
          && export.CashReceiptDetail.RefundedAmount.GetValueOrDefault() != 0)
        {
          var field = GetField(export.CollectionType2, "code");

          field.Error = true;

          ExitState = "FN0000_COL_TYPE_WITH_REL_CRD";

          return;
        }

        export.HiddenCollectionType.Code = export.CollectionType2.Code;

        // *** Do not allow the interface tran id to be updated.  Sunya Sharp 5/
        // 25/1999 ***
        if (!Equal(export.Previous.InterfaceTransId,
          export.CashReceiptDetail.InterfaceTransId))
        {
          var field = GetField(export.CashReceiptDetail, "interfaceTransId");

          field.Error = true;

          ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";

          return;
        }

        if (ReadCashReceipt4())
        {
          if (ReadCashReceiptStatusHistoryCashReceiptStatus2())
          {
            // *** Removed logic to check to see if the cash receipt source type
            // is court, FDSO, or SDSO.  Now the cash receipt must be in a
            // status of interface or deposit to be added.  Sunya Sharp 11/3/98
            // ***
            // *** Detail can be updated if the cash receipt is in REC status if
            // the created by is CONVERSN.  Sunya Sharp 5/25/1999 ***
            if (AsChar(local.ToBeUnpended.Flag) == 'Y')
            {
              // *****************************Do Not need to check when 
              // unpending a Non Cash cash receipt   PR 182853
              // ****************************				
            }
            else if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedDeposited.SystemGeneratedIdentifier || entities
              .CashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedInterface.SystemGeneratedIdentifier)
            {
            }
            else if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedReceipted.SystemGeneratedIdentifier && Equal
              (export.CashReceiptDetail.CreatedBy, "CONVERSN"))
            {
            }
            else
            {
              UseEabRollbackCics();
              ExitState = "FN0000_CANT_ADD_UPD_REL_CR_IN";

              return;
            }
          }
          else
          {
            UseEabRollbackCics();
            ExitState = "FN0000_CANT_ADD_UPD_REL_CR_IN";

            return;
          }

          // *** Added logic to ensure that the interface trans id is there if 
          // the cash receipt id from a court.  Sunya Sharp 11/3/98 ***
          if (ReadCashReceiptSourceType())
          {
            if (AsChar(entities.InterfaceCashReceiptSourceType.
              InterfaceIndicator) == 'Y' && AsChar
              (entities.InterfaceCashReceiptSourceType.CourtInd) == 'C')
            {
              if (ReadCashReceiptType1())
              {
                if (Equal(entities.InterfaceCashReceiptType.Code, "MANINT"))
                {
                  if (IsEmpty(export.CashReceiptDetail.InterfaceTransId))
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "interfaceTransId");

                    field.Error = true;

                    ExitState = "FN0000_MANDATORY_FIELDS";

                    return;
                  }
                }
              }
              else
              {
                ExitState = "FN0000_CASH_RECEIPT_TYPE_NF";

                return;
              }
            }
          }
          else
          {
            ExitState = "FN0096_CASH_RCPT_SOURCE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }

        UseFnAbDetermineCollAmtApplied2();
        export.CollAmtApplied.TotalCurrency =
          local.CollAmtApplied.TotalCurrency;

        // *** Add logic to allow area offices to work the same as courts.  But 
        // do not change the way that FDSO and SDSO are working.  System Test PR
        // # 101.  Sunya Sharp 07/10/1999 ***
        if (AsChar(entities.InterfaceCashReceiptSourceType.InterfaceIndicator) ==
          'Y')
        {
          if (export.CashReceipt.TotalCashTransactionAmount.
            GetValueOrDefault() > 0)
          {
            if (local.CollAmtApplied.TotalCurrency - local
              .BeforeUpdate.ReceivedAmount + export
              .CashReceiptDetail.ReceivedAmount > export
              .CashReceipt.TotalCashTransactionAmount.GetValueOrDefault())
            {
              var field1 = GetField(export.CashReceiptDetail, "receivedAmount");

              field1.Error = true;

              var field2 =
                GetField(export.CashReceipt, "totalCashTransactionAmount");

              field2.Error = true;

              var field3 = GetField(export.CollAmtApplied, "totalCurrency");

              field3.Error = true;

              ExitState = "FN0000_CRDTL_AMT_EXCEEDS_INTRFAC";

              return;
            }
          }
          else if (local.CollAmtApplied.TotalCurrency - local
            .BeforeUpdate.ReceivedAmount + export
            .CashReceiptDetail.ReceivedAmount > export
            .CashReceipt.ReceiptAmount)
          {
            var field1 = GetField(export.CashReceiptDetail, "receivedAmount");

            field1.Error = true;

            var field2 = GetField(export.CollAmtApplied, "totalCurrency");

            field2.Error = true;

            var field3 = GetField(export.CashReceipt, "receiptAmount");

            field3.Error = true;

            ExitState = "FN0000_COLL_AMT_GT_REC_AMT";

            return;
          }
        }
        else if (local.CollAmtApplied.TotalCurrency - local
          .BeforeUpdate.ReceivedAmount + export
          .CashReceiptDetail.ReceivedAmount > export.CashReceipt.ReceiptAmount)
        {
          var field1 = GetField(export.CashReceiptDetail, "receivedAmount");

          field1.Error = true;

          var field2 = GetField(export.CollAmtApplied, "totalCurrency");

          field2.Error = true;

          var field3 = GetField(export.CashReceipt, "receiptAmount");

          field3.Error = true;

          ExitState = "FN0000_COLL_AMT_GT_REC_AMT";

          return;
        }

        // ---- Validate collection & received amounts
        if (IsEmpty(export.CashReceiptLiterals.AdjustmentsExist))
        {
          if (export.CashReceiptDetail.CollectionAmount <= 0)
          {
            ExitState = "FN0000_AMT_CANT_BE_ZERO_OR_NEG";

            var field = GetField(export.CashReceiptDetail, "collectionAmount");

            field.Error = true;

            return;
          }

          if (import.CashReceiptDetail.ReceivedAmount <= 0)
          {
            ExitState = "FN0000_AMT_CANT_BE_ZERO_OR_NEG";

            var field = GetField(export.CashReceiptDetail, "receivedAmount");

            field.Error = true;

            return;
          }
        }

        // *** Determine if the fees for the cash receipt are equal to the  
        // difference of the collection amount - the received amount.  Sunya
        // Sharp 11/3/98 ***
        local.TotalFees.TotalCurrency = 0;

        foreach(var item in ReadCashReceiptDetailFee())
        {
          local.TotalFees.TotalCurrency += entities.CashReceiptDetailFee.Amount;
        }

        if (export.CashReceiptDetail.CollectionAmount - export
          .CashReceiptDetail.ReceivedAmount != local.TotalFees.TotalCurrency)
        {
          local.FeesRequired.Flag = "Y";
        }

        if (export.CashReceiptDetail.CollectionAmount >= export
          .CashReceiptDetail.ReceivedAmount)
        {
        }
        else
        {
          ExitState = "FN0000_COLL_AMT_LT_RECEIVED_AMT";

          var field1 = GetField(export.CashReceiptDetail, "collectionAmount");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetail, "receivedAmount");

          field2.Error = true;

          return;
        }

        if (Equal(import.CashReceiptDetail.CollectionDate, null))
        {
          export.CashReceiptDetail.CollectionDate = Now().Date;
        }
        else if (Lt(Now().Date, import.CashReceiptDetail.CollectionDate))
        {
          var field = GetField(export.CashReceiptDetail, "collectionDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        // ---- Validate collection type
        if (!IsEmpty(import.CollectionType2.Code))
        {
          if (ReadCollectionType())
          {
            MoveCollectionType(entities.CollectionType, export.CollectionType2);

            // 10/27/00        pphinney        I00106505    Verify Collection 
            // Type
            ReadCashReceiptType2();

            if (AsChar(entities.Verify.CategoryIndicator) == 'C' && AsChar
              (entities.CollectionType.CashNonCashInd) != 'C')
            {
              var field = GetField(export.CollectionType2, "code");

              field.Error = true;

              ExitState = "FN0000_CASH_CR_NON_CASH_CRD";

              return;
            }

            if (AsChar(entities.Verify.CategoryIndicator) == 'N' && AsChar
              (entities.CollectionType.CashNonCashInd) != 'N')
            {
              var field = GetField(export.CollectionType2, "code");

              field.Error = true;

              ExitState = "FN0000_NON_CASH_CR_CASH_CRD";

              return;
            }
          }
          else
          {
            var field = GetField(export.CollectionType2, "code");

            field.Error = true;

            ExitState = "FN0000_COLLECTION_TYPE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_CASH_RCPT_DTL_COL_TYP_RQD";

          var field = GetField(export.CollectionType2, "code");

          field.Error = true;

          return;
        }

        // *****
        // Validate Pending reason code
        // *****
        // *** Added logic to allow for suspend codes as well.  Sunya Sharp 10/
        // 27/98 ***
        if (!IsEmpty(export.CashReceiptDetailStatHistory.ReasonCodeId))
        {
          if (Equal(export.CashReceiptDetailStatus.Code, "PEND") || Equal
            (export.CashReceiptDetailStatus.Code, "SUSP"))
          {
          }
          else
          {
            var field =
              GetField(export.CashReceiptDetailStatHistory, "reasonCodeId");

            field.Error = true;

            ExitState = "FN0000_PENDED_CODE_NOT_NEEDED";

            return;
          }
        }

        export.CashReceiptDetail.LastUpdatedTmst = Now();

        // *** Move check for mandatory fields to the top of the update process.
        // Also check to see if obligor person number is present to find
        // address.  Sunya Sharp 10/26/98
        // 06/02/00        pphinney        H00096632    Changed logic to BLOCK 
        // unauthorized
        if (Equal(export.CashReceiptDetailAddress.Street1,
          "Security Block on Address") && IsEmpty
          (export.CashReceiptDetail.ObligorPhoneNumber))
        {
          export.CashReceiptDetail.ObligorPhoneNumber =
            import.HiddenCashReceiptDetail.ObligorPhoneNumber;
        }

        if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
        {
          local.CsePerson.Number =
            export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
          UseSiGetCsePersonMailingAddr();

          // *** This logic allows for the address to be updated to nothing if 
          // the person updated to does not have an address.  Sunya Sharp 10/30/
          // 98 ***
          if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
            (local.CsePersonAddress.City))
          {
            export.CashReceiptDetailAddress.Street1 = "No Active address found";
            export.CashReceiptDetailAddress.Street2 = "";
            export.CashReceiptDetailAddress.City = "";
            export.CashReceiptDetailAddress.State = "KS";
            export.CashReceiptDetailAddress.ZipCode5 = "";
            export.CashReceiptDetailAddress.ZipCode4 = "";
            export.CashReceiptDetailAddress.ZipCode3 = "";
          }
          else
          {
            export.CashReceiptDetailAddress.Street1 =
              local.CsePersonAddress.Street1 ?? Spaces(25);
            export.CashReceiptDetailAddress.Street2 =
              local.CsePersonAddress.Street2 ?? "";
            export.CashReceiptDetailAddress.City =
              local.CsePersonAddress.City ?? Spaces(30);
            export.CashReceiptDetailAddress.State =
              local.CsePersonAddress.State ?? Spaces(2);
            export.CashReceiptDetailAddress.ZipCode5 =
              local.CsePersonAddress.ZipCode ?? Spaces(5);
            export.CashReceiptDetailAddress.ZipCode4 =
              local.CsePersonAddress.Zip4 ?? "";
            export.CashReceiptDetailAddress.ZipCode3 =
              local.CsePersonAddress.Zip3 ?? "";
          }
        }

        UseUpdateCollection1();

        if (IsExitState("UNPEND_SUCCESSFUL"))
        {
          export.CashReceiptDetailStatHistory.ReasonCodeId = "";
          export.CashReceiptDetailStatus.Code = "REC";
        }
        else if (IsExitState("FN0000_UPDATE_SUCCESSFUL_WA"))
        {
          if (Equal(export.CashReceiptDetailStatus.Code, "REL"))
          {
            export.CashReceiptDetailStatus.Code = "REC";
            ExitState = "FN0000_UPDATE_SUCCESSFUL_CRD";
          }
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          UseEabRollbackCics();

          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;

          return;
        }

        export.CashReceiptDetail.LastUpdatedBy = global.UserId;

        if (!IsEmpty(export.CashReceiptDetailAddress.Street1) && (
          !Equal(export.CashReceiptDetailAddress.Street1,
          "Security Block on Address") || !
          Equal(export.CashReceiptDetailAddress.Street1,
          "No Active address found")) || !
          IsEmpty(export.CashReceiptDetailAddress.Street2) || !
          IsEmpty(export.CashReceiptDetailAddress.City) || !
          IsEmpty(export.CashReceiptDetailAddress.State) || !
          IsEmpty(export.CashReceiptDetailAddress.ZipCode3) || !
          IsEmpty(export.CashReceiptDetailAddress.ZipCode4) || !
          IsEmpty(export.CashReceiptDetailAddress.ZipCode5))
        {
          if (Equal(export.CashReceiptDetailAddress.SystemGeneratedIdentifier,
            local.InitializeCashReceiptDetailAddress.
              SystemGeneratedIdentifier))
          {
            UseCreateCrDetailAddress1();
          }
          else
          {
            UseUpdateCrDetailAddress();
          }
        }

        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
        }
        else if (IsExitState("UNPEND_SUCCESSFUL"))
        {
        }
        else if (IsExitState("FN0000_UPDATE_SUCCESSFUL_CRD"))
        {
        }
        else if (IsExitState("FN0000_UPDATE_SUCCESSFUL_WA"))
        {
          ExitState = "FN0000_UPDATE_SUCCESSFUL_CRD";
        }
        else if (IsExitState("NO_UPDATE_REQUIRED"))
        {
          ExitState = "FN0000_UPDATE_SUCCESSFUL_CRD";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          UseEabRollbackCics();

          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;

          return;
        }

        MoveCashReceiptDetail4(export.CashReceiptDetail, export.Previous);

        if (AsChar(local.FeesRequired.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_TO_REC_COLLECTION_FEES";

          return;
        }
        else
        {
          // *** Need to redisplay after successful update.  None enterable 
          // information is not being populated correctly.  Put here because did
          // not need to go through the other display logic.  Sunya Sharp 11/3/
          // 98 ***
          UseDisplayCrDetails2();

          if (IsExitState("FN0052_CASH_RCPT_DTL_NF"))
          {
            export.CashReceipt.Assign(import.CashReceipt);

            var field =
              GetField(export.CashReceiptDetail, "sequentialIdentifier");

            field.Error = true;

            return;
          }
          else if (IsExitState("FN0064_CASH_RCPT_DTL_STAT_HST_NF"))
          {
            export.CashReceipt.Assign(import.CashReceipt);

            var field =
              GetField(export.CashReceiptDetail, "sequentialIdentifier");

            field.Error = true;

            return;
          }
          else if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // H00096632    06/02/00   pphinney
            global.Command = "RESEARCH";
            UseScCabTestSecurity();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HiddenCashReceiptDetail.ObligorPhoneNumber = "";
            }
            else
            {
              export.CashReceiptDetailAddress.Assign(local.Blank);
              export.HiddenCashReceiptDetail.ObligorPhoneNumber =
                export.CashReceiptDetail.ObligorPhoneNumber ?? "";
              export.CashReceiptDetail.ObligorPhoneNumber = "";
              export.CashReceiptDetailAddress.Street1 =
                "Security Block on Address";
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              export.CashReceiptDetailAddress.Assign(local.Blank);
              export.CashReceiptDetail.ObligorPhoneNumber = "";
              export.CashReceiptDetailAddress.Street1 =
                "Security Block on Address";
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
          else
          {
            // H00096632    06/02/00   pphinney
            export.CashReceiptDetailAddress.Assign(local.Blank);
          }

          // *** SME requested.  If this is a normal update and the total 
          // received amount is equal to the total received the user would like
          // the message to tell the user to release.  Sunya Sharp 11/9/98 ***
          // *** Add logic to allow area offices to work the same as courts.  
          // But do not change the way that FDSO and SDSO are working.  System
          // Test PR# 101.  Sunya Sharp 07/10/1999 ***
          if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
            if (AsChar(entities.InterfaceCashReceiptSourceType.
              InterfaceIndicator) == 'Y')
            {
              if (export.CashReceipt.TotalCashTransactionAmount.
                GetValueOrDefault() > 0)
              {
                if (export.CashReceipt.TotalCashTransactionAmount.
                  GetValueOrDefault() == export.CollAmtApplied.TotalCurrency)
                {
                  if (!Equal(export.CashReceiptDetailStatus.Code, "REL"))
                  {
                    ExitState = "FN0000_UPDATE_SUCCESS_PRESS_PF16";
                  }
                }
              }
              else if (export.CashReceipt.ReceiptAmount == export
                .CollAmtApplied.TotalCurrency)
              {
                if (!Equal(export.CashReceiptDetailStatus.Code, "REL"))
                {
                  ExitState = "FN0000_UPDATE_SUCCESS_PRESS_PF16";
                }
              }
            }
            else if (export.CashReceipt.ReceiptAmount == export
              .CollAmtApplied.TotalCurrency)
            {
              if (!Equal(export.CashReceiptDetailStatus.Code, "REL"))
              {
                ExitState = "FN0000_UPDATE_SUCCESS_PRESS_PF16";
              }
            }
          }
        }

        // 08/02/00        pphinney        H00098632    Changed logic to NOT 
        // allow change of
        // collection date if CRD is REL
        export.Save.CollectionDate = export.CashReceiptDetail.CollectionDate;

        // 08/21/00        pphinney        H00101532    Changed logic to NOT 
        // allow change of
        // collection TYPE if CRD is REL
        export.HiddenCollectionType.Code = export.CollectionType2.Code;

        break;
      case "PEND":
        if (AsChar(export.DeletedCashReceiptFlag.Flag) == 'Y')
        {
          ExitState = "FN0000_CASH_RECEIPT_DELETED";

          return;
        }

        UsePendCollection();

        if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
        {
          var field =
            GetField(export.CashReceiptDetailStatHistory, "reasonCodeId");

          field.Error = true;
        }
        else if (IsExitState("FN0084_CASH_RCPT_NF"))
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("PENDING_REASON_CODE_INVALID"))
        {
          var field =
            GetField(export.CashReceiptDetailStatHistory, "reasonCodeId");

          field.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CashReceiptDetail.LastUpdatedBy = global.UserId;
          ExitState = "PEND_SUCCESSFUL";
        }
        else
        {
          UseEabRollbackCics();

          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;
        }

        return;
      case "RELEASE":
        if (export.HiddenCashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .HiddenCashReceiptType.SystemGeneratedIdentifier == 0)
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "FN0000_DISPLAY_BEFORE_RELEASE";

          return;
        }

        if (AsChar(export.DeletedCashReceiptFlag.Flag) == 'Y')
        {
          ExitState = "FN0000_CASH_RECEIPT_DELETED";

          return;
        }

        if (export.CashReceiptDetail.SequentialIdentifier == 0)
        {
          var field = GetField(export.Collection, "promptField");

          field.Error = true;

          ExitState = "FN0000_DISPLAY_BEFORE_RELEASE";

          return;
        }

        if (!ReadCashReceipt1())
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }

        if (ReadCashReceiptStatusHistoryCashReceiptStatus1())
        {
          // *** Removed logic to check to see if the cash receipt source type 
          // is court, FDSO, or SDSO.  Now the cash receipt must be in a status
          // of interface or deposit to be added.  Sunya Sharp 11/3/98 ***
          // *** Detail can be updated if the cash receipt is in REC status if 
          // the created by is CONVERSN.  Sunya Sharp 5/25/1999 ***
          if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
            .HardcodedDeposited.SystemGeneratedIdentifier || entities
            .CashReceiptStatus.SystemGeneratedIdentifier == local
            .HardcodedInterface.SystemGeneratedIdentifier)
          {
          }
          else if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
            .HardcodedReceipted.SystemGeneratedIdentifier && Equal
            (export.CashReceiptDetail.CreatedBy, "CONVERSN"))
          {
          }
          else
          {
            UseEabRollbackCics();
            ExitState = "FN0000_CANT_ADD_UPD_REL_CR_IN";

            return;
          }
        }
        else
        {
          UseEabRollbackCics();
          ExitState = "FN0000_CANT_ADD_UPD_REL_CR_IN";

          return;
        }

        UseFnAbDetermineCollAmtApplied1();

        // ---- Check if source code is interfaced AND cash receipt type is not
        // 'FCRT REC' ---
        if (AsChar(export.CashReceiptSourceType.InterfaceIndicator) == 'Y' && export
          .HiddenCashReceiptType.SystemGeneratedIdentifier != 2)
        {
          if (Lt(0, entities.CashReceipt.TotalCashTransactionAmount))
          {
            if (!Equal(local.CollAmtApplied.TotalCurrency,
              entities.CashReceipt.TotalCashTransactionAmount))
            {
              ExitState = "FN0000_UNACCNTD_MONY_REMAIN_INTF";

              return;
            }
          }
          else if (local.CollAmtApplied.TotalCurrency != entities
            .CashReceipt.ReceiptAmount)
          {
            ExitState = "FN0000_UNACCOUNTD_MONEY_REMAINNG";

            return;
          }
        }
        else if (local.CollAmtApplied.TotalCurrency != entities
          .CashReceipt.ReceiptAmount)
        {
          ExitState = "FN0000_UNACCOUNTD_MONEY_REMAINNG";

          return;
        }

        // *** H00074363 - Added logic to validate the collection type before 
        // release. ***
        // ---- Validate collection type
        if (!IsEmpty(import.CollectionType2.Code))
        {
          if (ReadCollectionType())
          {
            MoveCollectionType(entities.CollectionType, export.CollectionType2);
          }
          else
          {
            var field = GetField(export.CollectionType2, "code");

            field.Error = true;

            ExitState = "FN0000_COLLECTION_TYPE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_CASH_RCPT_DTL_COL_TYP_RQD";

          var field = GetField(export.CollectionType2, "code");

          field.Error = true;

          return;
        }

        local.CanBeReleased.Flag = "N";
        local.IsCrDetailPopulated.Flag = "N";
        local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          local.HardcodedReleased.SystemGeneratedIdentifier;

        // *** When releasing a cash receipt in suspended status can ONLY 
        // release if it is the current detail being displayed.   Sunya Sharp 11
        // /4/98 ***
        // *** H00074363 - Moved validation logic for the court interface here.
        // The user was able to release even though they had not updated the
        // information on the screen to the database.  Sunya Sharp 09/22/1999 **
        // *
        if (Equal(import.CashReceiptDetailStatus.Code, "SUSP"))
        {
          if (ReadCashReceiptDetail3())
          {
            if (!Equal(export.CashReceiptDetail.ObligorPersonNumber,
              entities.ForPersistentView.ObligorPersonNumber))
            {
              var field =
                GetField(export.CashReceiptDetail, "obligorPersonNumber");

              field.Error = true;

              ExitState = "FN0000_UPDATE_CRD_BEFORE_RELEASE";

              return;
            }

            if (!Equal(export.CashReceiptDetail.ObligorSocialSecurityNumber,
              entities.ForPersistentView.ObligorSocialSecurityNumber))
            {
              if (Verify(entities.ForPersistentView.ObligorSocialSecurityNumber,
                "0") == 0)
              {
                goto Test;
              }

              var field =
                GetField(export.CashReceiptDetail, "obligorSocialSecurityNumber");
                

              field.Error = true;

              ExitState = "FN0000_UPDATE_CRD_BEFORE_RELEASE";

              return;
            }

Test:

            if (!Equal(export.CashReceiptDetail.CourtOrderNumber,
              entities.ForPersistentView.CourtOrderNumber))
            {
              var field =
                GetField(export.CashReceiptDetail, "courtOrderNumber");

              field.Error = true;

              ExitState = "FN0000_UPDATE_CRD_BEFORE_RELEASE";

              return;
            }

            if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
            {
              local.CsePersonsWorkSet.Number =
                export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
              UseSiReadCsePerson2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field =
                  GetField(export.CashReceiptDetail, "obligorPersonNumber");

                field.Error = true;

                return;
              }
            }

            if (!IsEmpty(export.CashReceiptDetail.ObligorSocialSecurityNumber) &&
              IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
            {
              local.CsePersonsWorkSet.Ssn =
                export.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces
                (9);
              UseFnReadCsePersonUsingSsnO();

              // *** Logic was checking to see if not equal to all ok.  There 
              // are not exitstates returned from the eab.  Sunya Sharp 10/26/98
              // ***
              if (IsEmpty(local.CsePersonsWorkSet.Number))
              {
                ExitState = "FN0000_CRD_OBLIGOR_SSN_INVALID";

                var field =
                  GetField(export.CashReceiptDetail,
                  "obligorSocialSecurityNumber");

                field.Error = true;

                return;
              }
            }

            if (!IsEmpty(export.CashReceiptDetail.CourtOrderNumber))
            {
              // --------------------------------------------------
              // Validate the existance of Court Order and check whether the Ct.
              // Order is Multi-Payor
              // --------------------------------------------------
              UseFnAbObligorListForCtOrder();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (local.WorkNoOfObligors.Count > 1)
                {
                  export.WorkIsMultiPayor.Flag = "Y";
                  local.Female.Count = 0;
                  local.Male.Count = 0;
                  local.MultipayorOk.Flag = "";

                  if (AsChar(export.CashReceiptDetail.MultiPayor) == 'M')
                  {
                    local.WorkSex.Sex = "F";
                  }
                  else if (AsChar(export.CashReceiptDetail.MultiPayor) == 'F')
                  {
                    local.WorkSex.Sex = "M";
                  }
                  else
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "multiPayor");

                    field.Error = true;

                    ExitState = "FN0000_MULT_PAYOR_IDENTIFY_PAYOR";

                    return;
                  }

                  // *** PR# 78962 - Adding logic to support a multi-payor case 
                  // with 2 fathers or 2 mothers.  Sunya Sharp 12/13/1999 ***
                  for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                    .ObligorList.Count; ++local.ObligorList.Index)
                  {
                    if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == 'M')
                    {
                      ++local.Male.Count;
                    }

                    if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == 'F')
                    {
                      ++local.Female.Count;
                    }
                  }

                  if (local.Male.Count > 1 && IsEmpty
                    (export.CashReceiptDetail.ObligorPersonNumber))
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "obligorPersonNumber");
                      

                    field.Error = true;

                    ExitState = "FN0000_MULTIPAYOR_NEEDS_CSE_NBR";

                    return;
                  }

                  if (local.Female.Count > 1 && IsEmpty
                    (export.CashReceiptDetail.ObligorPersonNumber))
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "obligorPersonNumber");
                      

                    field.Error = true;

                    ExitState = "FN0000_MULTIPAYOR_NEEDS_CSE_NBR";

                    return;
                  }

                  if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                  {
                    for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                      .ObligorList.Count; ++local.ObligorList.Index)
                    {
                      if (Equal(local.ObligorList.Item.GrpsWork.Number,
                        export.CashReceiptDetail.ObligorPersonNumber))
                      {
                        if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == AsChar
                          (local.WorkSex.Sex))
                        {
                          export.CashReceiptDetail.ObligorPersonNumber =
                            local.ObligorList.Item.GrpsWork.Number;
                          export.CashReceiptDetail.ObligorSocialSecurityNumber =
                            local.ObligorList.Item.GrpsWork.Ssn;
                          export.CashReceiptDetail.ObligorLastName =
                            local.ObligorList.Item.GrpsWork.LastName;
                          export.CashReceiptDetail.ObligorFirstName =
                            local.ObligorList.Item.GrpsWork.FirstName;
                          export.CashReceiptDetail.ObligorMiddleName =
                            local.ObligorList.Item.GrpsWork.MiddleInitial;

                          // *** Add logic to get the phone number for the 
                          // obligor. Sunya Sharp 10/27/98 ***
                          export.CashReceiptDetail.ObligorPhoneNumber =
                            NumberToString(local.ObligorList.Item.Grps.
                              HomePhoneAreaCode.GetValueOrDefault(), 13, 3) + NumberToString
                            (local.ObligorList.Item.Grps.HomePhone.
                              GetValueOrDefault(), 9, 7);
                          local.MultipayorOk.Flag = "Y";

                          break;
                        }
                        else
                        {
                          // *** Loops to next person or Falls to error logic **
                          // *
                        }
                      }
                      else
                      {
                        // *** Loops to next person or Falls to error logic ***
                      }
                    }

                    if (AsChar(local.MultipayorOk.Flag) != 'Y')
                    {
                      var field =
                        GetField(export.CashReceiptDetail, "obligorPersonNumber");
                        

                      field.Error = true;

                      ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                      return;
                    }
                  }
                  else
                  {
                    for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                      .ObligorList.Count; ++local.ObligorList.Index)
                    {
                      if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == AsChar
                        (local.WorkSex.Sex))
                      {
                        if (!Equal(local.ObligorList.Item.GrpsWork.Number,
                          export.CashReceiptDetail.ObligorPersonNumber) && !
                          IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                        {
                          var field =
                            GetField(export.CashReceiptDetail,
                            "obligorPersonNumber");

                          field.Error = true;

                          ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                          return;
                        }

                        export.CashReceiptDetail.ObligorPersonNumber =
                          local.ObligorList.Item.GrpsWork.Number;
                        export.CashReceiptDetail.ObligorSocialSecurityNumber =
                          local.ObligorList.Item.GrpsWork.Ssn;
                        export.CashReceiptDetail.ObligorLastName =
                          local.ObligorList.Item.GrpsWork.LastName;
                        export.CashReceiptDetail.ObligorFirstName =
                          local.ObligorList.Item.GrpsWork.FirstName;
                        export.CashReceiptDetail.ObligorMiddleName =
                          local.ObligorList.Item.GrpsWork.MiddleInitial;

                        // *** Add logic to get the phone number for the 
                        // obligor. Sunya Sharp 10/27/98 ***
                        export.CashReceiptDetail.ObligorPhoneNumber =
                          NumberToString(local.ObligorList.Item.Grps.
                            HomePhoneAreaCode.GetValueOrDefault(), 13, 3) + NumberToString
                          (local.ObligorList.Item.Grps.HomePhone.
                            GetValueOrDefault(), 9, 7);

                        break;
                      }
                    }
                  }
                }
                else if (local.WorkNoOfObligors.Count == 1)
                {
                  for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                    .ObligorList.Count; ++local.ObligorList.Index)
                  {
                    if (!Equal(local.ObligorList.Item.GrpsWork.Number,
                      export.CashReceiptDetail.ObligorPersonNumber))
                    {
                      if (IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                      {
                        var field =
                          GetField(export.CashReceiptDetail,
                          "obligorPersonNumber");

                        field.Error = true;

                        ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

                        return;
                      }
                      else
                      {
                        var field =
                          GetField(export.CashReceiptDetail,
                          "obligorPersonNumber");

                        field.Error = true;

                        ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                        return;
                      }
                    }
                  }
                }
                else
                {
                  var field =
                    GetField(export.CashReceiptDetail, "courtOrderNumber");

                  field.Error = true;

                  ExitState = "FN0000_NO_OBLIGORS_FOR_CT_ORDER";

                  return;
                }
              }
              else
              {
                return;
              }
            }

            if (!ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus2())
            {
              ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

              return;
            }

            local.CashReceiptDetail.SequentialIdentifier =
              entities.ForPersistentView.SequentialIdentifier;
            UseFnChangeCashRcptDtlStatHis1();

            // *** Added logic to display the correct user id after successful 
            // release.  Also when a suspended detail is released removed the
            // reason code id from the status history.  Sunya Sharp 11/2/98 ***
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.CashReceiptDetailStatHistory.ReasonCodeId = "";
              export.CashReceiptDetailStatus.Code = "REL";
              export.CashReceiptDetail.LastUpdatedBy = global.UserId;
            }
            else
            {
              UseEabRollbackCics();

              return;
            }
          }
          else
          {
            ExitState = "CASH_RECEIPT_DETAIL_NF";

            return;
          }

          // *** PR# 85804 - Trying to be able to reuse logic.  When a flow to 
          // to MCOL is done and the CRD is in SUSP for MANUALDIST or
          // MANREAPPLY, the user would like the CRD to be released and then
          // automatically flow to MCOL without check to see if there is a
          // manual debt set up for the court order/person.  The skip flag will
          // be set in the flow to logic.  When this logic is used with just the
          // RELEASE (PF16) there must be the check to verify if a manual debt
          // exists.  Sunya Sharp 2/21/2000 ***
          if (AsChar(local.SkipManDistRead.Flag) == 'Y')
          {
            export.HidMcolCurrent.SequentialIdentifier = 9999;
            MoveCashReceiptDetail2(entities.ForPersistentView,
              export.CashReceiptDetail);
            ExitState = "ECO_LNK_TO_MANUAL_DIST_OF_COLL";

            return;
          }
          else
          {
            UseFnCabCheckManualDistForCrd();

            // *** Set export hid mcol current to high value.  This way it does 
            // not need to loop through the reset of the details.  This is a
            // quick fix to eliminate having to rework the return logic from
            // MCOL.  Sunya Sharp 11/9/98 ***
            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("RELEASE_FOR_DIST_SUCCESSFUL"))
            {
              if (AsChar(local.ManualDistribInd.Flag) == 'Y')
              {
                export.HidMcolCurrent.SequentialIdentifier = 9999;
                MoveCashReceiptDetail2(entities.ForPersistentView,
                  export.CashReceiptDetail);
                ExitState = "ECO_LNK_TO_MANUAL_DIST_OF_COLL";

                return;
              }
              else
              {
                ExitState = "RELEASE_FOR_DIST_SUCCESSFUL";

                return;
              }
            }
            else
            {
              return;
            }
          }
        }
        else
        {
          if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
          {
            local.CsePersonsWorkSet.Number =
              export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
            UseSiReadCsePerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field =
                GetField(export.CashReceiptDetail, "obligorPersonNumber");

              field.Error = true;

              return;
            }
          }

          if (!IsEmpty(export.CashReceiptDetail.ObligorSocialSecurityNumber) &&
            IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
          {
            local.CsePersonsWorkSet.Ssn =
              export.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces
              (9);
            UseFnReadCsePersonUsingSsnO();

            // *** Logic was checking to see if not equal to all ok.  There are 
            // not exitstates returned from the eab.  Sunya Sharp 10/26/98 ***
            if (IsEmpty(local.CsePersonsWorkSet.Number))
            {
              ExitState = "FN0000_CRD_OBLIGOR_SSN_INVALID";

              var field =
                GetField(export.CashReceiptDetail, "obligorSocialSecurityNumber");
                

              field.Error = true;

              return;
            }
          }

          if (!IsEmpty(export.CashReceiptDetail.CourtOrderNumber))
          {
            // --------------------------------------------------
            // Validate the existance of Court Order and check whether the Ct. 
            // Order is Multi-Payor
            // --------------------------------------------------
            UseFnAbObligorListForCtOrder();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (local.WorkNoOfObligors.Count > 1)
              {
                export.WorkIsMultiPayor.Flag = "Y";
                local.Female.Count = 0;
                local.Male.Count = 0;
                local.MultipayorOk.Flag = "";

                if (AsChar(export.CashReceiptDetail.MultiPayor) == 'M')
                {
                  local.WorkSex.Sex = "F";
                }
                else if (AsChar(export.CashReceiptDetail.MultiPayor) == 'F')
                {
                  local.WorkSex.Sex = "M";
                }
                else
                {
                  var field = GetField(export.CashReceiptDetail, "multiPayor");

                  field.Error = true;

                  ExitState = "FN0000_MULT_PAYOR_IDENTIFY_PAYOR";

                  return;
                }

                // *** PR# 78962 - Adding logic to support a multi-payor case 
                // with 2 fathers or 2 mothers.  Sunya Sharp 12/13/1999 ***
                for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                  .ObligorList.Count; ++local.ObligorList.Index)
                {
                  if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == 'M')
                  {
                    ++local.Male.Count;
                  }

                  if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == 'F')
                  {
                    ++local.Female.Count;
                  }
                }

                if (local.Male.Count > 1 && IsEmpty
                  (export.CashReceiptDetail.ObligorPersonNumber))
                {
                  var field =
                    GetField(export.CashReceiptDetail, "obligorPersonNumber");

                  field.Error = true;

                  ExitState = "FN0000_MULTIPAYOR_NEEDS_CSE_NBR";

                  return;
                }

                if (local.Female.Count > 1 && IsEmpty
                  (export.CashReceiptDetail.ObligorPersonNumber))
                {
                  var field =
                    GetField(export.CashReceiptDetail, "obligorPersonNumber");

                  field.Error = true;

                  ExitState = "FN0000_MULTIPAYOR_NEEDS_CSE_NBR";

                  return;
                }

                if (!IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                {
                  for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                    .ObligorList.Count; ++local.ObligorList.Index)
                  {
                    if (Equal(local.ObligorList.Item.GrpsWork.Number,
                      export.CashReceiptDetail.ObligorPersonNumber))
                    {
                      if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == AsChar
                        (local.WorkSex.Sex))
                      {
                        export.CashReceiptDetail.ObligorPersonNumber =
                          local.ObligorList.Item.GrpsWork.Number;
                        export.CashReceiptDetail.ObligorSocialSecurityNumber =
                          local.ObligorList.Item.GrpsWork.Ssn;
                        export.CashReceiptDetail.ObligorLastName =
                          local.ObligorList.Item.GrpsWork.LastName;
                        export.CashReceiptDetail.ObligorFirstName =
                          local.ObligorList.Item.GrpsWork.FirstName;
                        export.CashReceiptDetail.ObligorMiddleName =
                          local.ObligorList.Item.GrpsWork.MiddleInitial;

                        // *** Add logic to get the phone number for the 
                        // obligor. Sunya Sharp 10/27/98 ***
                        export.CashReceiptDetail.ObligorPhoneNumber =
                          NumberToString(local.ObligorList.Item.Grps.
                            HomePhoneAreaCode.GetValueOrDefault(), 13, 3) + NumberToString
                          (local.ObligorList.Item.Grps.HomePhone.
                            GetValueOrDefault(), 9, 7);
                        local.MultipayorOk.Flag = "Y";

                        break;
                      }
                      else
                      {
                        // *** Loops to next person or Falls to error logic ***
                      }
                    }
                    else
                    {
                      // *** Loops to next person or Falls to error logic ***
                    }
                  }

                  if (AsChar(local.MultipayorOk.Flag) != 'Y')
                  {
                    var field =
                      GetField(export.CashReceiptDetail, "obligorPersonNumber");
                      

                    field.Error = true;

                    ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                    return;
                  }
                }
                else
                {
                  for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                    .ObligorList.Count; ++local.ObligorList.Index)
                  {
                    if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == AsChar
                      (local.WorkSex.Sex))
                    {
                      if (!Equal(local.ObligorList.Item.GrpsWork.Number,
                        export.CashReceiptDetail.ObligorPersonNumber) && !
                        IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                      {
                        var field =
                          GetField(export.CashReceiptDetail,
                          "obligorPersonNumber");

                        field.Error = true;

                        ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                        return;
                      }

                      export.CashReceiptDetail.ObligorPersonNumber =
                        local.ObligorList.Item.GrpsWork.Number;
                      export.CashReceiptDetail.ObligorSocialSecurityNumber =
                        local.ObligorList.Item.GrpsWork.Ssn;
                      export.CashReceiptDetail.ObligorLastName =
                        local.ObligorList.Item.GrpsWork.LastName;
                      export.CashReceiptDetail.ObligorFirstName =
                        local.ObligorList.Item.GrpsWork.FirstName;
                      export.CashReceiptDetail.ObligorMiddleName =
                        local.ObligorList.Item.GrpsWork.MiddleInitial;

                      // *** Add logic to get the phone number for the obligor. 
                      // Sunya Sharp 10/27/98 ***
                      export.CashReceiptDetail.ObligorPhoneNumber =
                        NumberToString(local.ObligorList.Item.Grps.
                          HomePhoneAreaCode.GetValueOrDefault(), 13, 3) + NumberToString
                        (local.ObligorList.Item.Grps.HomePhone.
                          GetValueOrDefault(), 9, 7);

                      break;
                    }
                  }
                }
              }
              else if (local.WorkNoOfObligors.Count == 1)
              {
                for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                  .ObligorList.Count; ++local.ObligorList.Index)
                {
                  if (!Equal(local.ObligorList.Item.GrpsWork.Number,
                    export.CashReceiptDetail.ObligorPersonNumber))
                  {
                    if (IsEmpty(export.CashReceiptDetail.ObligorPersonNumber))
                    {
                      var field =
                        GetField(export.CashReceiptDetail, "obligorPersonNumber");
                        

                      field.Error = true;

                      ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

                      return;
                    }
                    else
                    {
                      var field =
                        GetField(export.CashReceiptDetail, "obligorPersonNumber");
                        

                      field.Error = true;

                      ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

                      return;
                    }
                  }
                }
              }
              else
              {
                var field =
                  GetField(export.CashReceiptDetail, "courtOrderNumber");

                field.Error = true;

                ExitState = "FN0000_NO_OBLIGORS_FOR_CT_ORDER";

                return;
              }
            }
            else
            {
              return;
            }
          }

          foreach(var item in ReadCashReceiptDetail4())
          {
            if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus1())
            {
              if (IsEmpty(entities.CashReceiptDetail.CourtOrderNumber) && IsEmpty
                (entities.CashReceiptDetail.ObligorPersonNumber) && IsEmpty
                (entities.CashReceiptDetail.ObligorSocialSecurityNumber))
              {
                if (entities.CashReceiptDetailStatus.
                  SystemGeneratedIdentifier == local
                  .HardcodedPended.SystemGeneratedIdentifier || entities
                  .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
                  .HardcodedSuspended.SystemGeneratedIdentifier)
                {
                }
                else
                {
                  ExitState = "FN0000_DIST_REQ_CT_OR_SSN_CSE_NO";
                  UseEabRollbackCics();

                  return;
                }
              }
            }
            else
            {
              ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

              return;
            }

            local.IsCrDetailPopulated.Flag = "Y";

            // -------------------------------------------------
            // Can be released only if the collection is in:
            // 1. RECorded (1)
            // 2. SUSPended (3)
            // -------------------------------------------------
            if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
              .HardcodedRecorded.SystemGeneratedIdentifier || entities
              .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
              .HardcodedSuspended.SystemGeneratedIdentifier)
            {
              // *** When releasing a cash receipt in suspended status can ONLY 
              // release if it is the current detail being displayed.  If other
              // details are in recorded status still release.  Sunya Sharp 11/4
              // /98 ***
              if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier ==
                local.HardcodedSuspended.SystemGeneratedIdentifier)
              {
                if (entities.CashReceiptDetail.SequentialIdentifier != export
                  .CashReceiptDetail.SequentialIdentifier)
                {
                  continue;
                }
              }

              local.CanBeReleased.Flag = "Y";
              local.CashReceiptDetail.SequentialIdentifier =
                entities.CashReceiptDetail.SequentialIdentifier;
              UseFnChangeCashRcptDtlStatHis1();

              // *** Added logic to display the correct user id after successful
              // release.  Also when a suspended detail is released removed the
              // reason code id from the status history.  Sunya Sharp 11/2/98 *
              // **
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (export.CashReceiptDetail.SequentialIdentifier == local
                  .CashReceiptDetail.SequentialIdentifier)
                {
                  if (entities.CashReceiptDetailStatus.
                    SystemGeneratedIdentifier == local
                    .HardcodedSuspended.SystemGeneratedIdentifier)
                  {
                    export.CashReceiptDetailStatHistory.ReasonCodeId = "";
                  }

                  export.CashReceiptDetailStatus.Code = "REL";
                  export.CashReceiptDetail.LastUpdatedBy = global.UserId;
                }
              }
              else
              {
                UseEabRollbackCics();

                return;
              }
            }
          }

          if (AsChar(local.IsCrDetailPopulated.Flag) == 'Y')
          {
          }
          else
          {
            ExitState = "CASH_RECEIPT_DETAIL_NF";

            return;
          }

          if (AsChar(local.CanBeReleased.Flag) == 'N')
          {
            ExitState = "FN0000_NO_CR_DTL_IN_STAT_TO_REL";

            return;
          }
          else
          {
            export.HidMcolCurrent.SequentialIdentifier = 0;
            ExitState = "RELEASE_FOR_DIST_SUCCESSFUL";
            global.Command = "CRRC";
          }
        }

        break;
      case "NEXT":
        if (export.CashReceiptDetail.SequentialIdentifier == 0)
        {
          ExitState = "DISPLAY_OR_CREATE_B4_PAGE_2";
        }
        else
        {
          ExitState = "ECO_LNK_TO_REC_COLLECTION_SEC";
        }

        return;
      case "LIST":
        switch(AsChar(export.Collection.PromptField))
        {
          case '+':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          case 'S':
            export.Collection.PromptField = "+";
            ExitState = "ECO_LNK_TO_LST_CASH_RECEIPT_DTL";

            return;
          case ' ':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.Collection, "promptField");

            field.Error = true;

            return;
        }

        switch(AsChar(export.CollectionType1.PromptField))
        {
          case '+':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          case 'S':
            export.CollectionType1.PromptField = "+";
            ExitState = "ECO_LNK_LST_COLLECTIONS";

            return;
          case ' ':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          default:
            var field = GetField(export.CollectionType1, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.PendRsnPrompt.PromptField))
        {
          case '+':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          case 'S':
            export.Code.CodeName = "PEND/SUSP REASON";
            export.PendRsnPrompt.PromptField = "+";
            ExitState = "ECO_LNK_TO_CDVL1";

            return;
          case ' ':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          default:
            var field = GetField(export.PendRsnPrompt, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.AmtPrompt.Text1))
        {
          case '+':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          case 'S':
            export.CsePerson.Number =
              export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
            export.AmtPrompt.Text1 = "+";
            ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";

            return;
          case ' ':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          default:
            var field = GetField(export.AmtPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.Pf17FlowTo.PromptField))
        {
          case '+':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          case 'S':
            export.Code.CodeName = "FLOW FROM CRRC";
            export.Pf17FlowTo.PromptField = "+";
            ExitState = "ECO_LNK_TO_CDVL1";

            return;
          case ' ':
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            break;
          default:
            var field = GetField(export.Pf17FlowTo, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        return;
      case "RESEARCH":
        ExitState = "ECO_LNK_TO_RESEARCH_COLLECTION";

        return;
      case "REFUND":
        if (export.CashReceipt.SequentialNumber == 0 || export
          .CashReceiptDetail.SequentialIdentifier == 0 || export
          .CashReceiptDetail.CollectionAmount == 0 || Equal
          (export.CashReceiptDetail.CollectionDate, null))
        {
          ExitState = "FN0000_DISPLAY_BEFORE_FLOW_CRRU";

          return;
        }

        if (Equal(export.CashReceiptDetailStatus.Code, "PEND"))
        {
          var field = GetField(export.CashReceiptDetailStatus, "code");

          field.Error = true;

          ExitState = "FN0000_CANT_FLOW_TO_CRRU_PEND";

          return;
        }

        if (AsChar(export.DeletedCashReceiptFlag.Flag) == 'Y')
        {
          ExitState = "FN0000_CASH_RECEIPT_DELETED";

          return;
        }

        ExitState = "ECO_LNK_TO_REFUND_COLLECTION";

        return;
      case "NOTES":
        ExitState = "ECO_LNK_TO_REC_COLLECTION_NOTES";

        return;
      case "FEES":
        ExitState = "ECO_LNK_TO_REC_COLLECTION_FEES";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // *** Removed logic that was no longer needed when flowing to MCOL.  Sunya 
    // Sharp 10/26/98 ***
    if (Equal(global.Command, "CRRC"))
    {
      // *** PR# 00078854 - Added logic to check to make sure that only details 
      // in released status are checked for manual distribtion.  Sunya Sharp 11/
      // 2/1999 ***
      UseFnHardcodedCashReceipting2();

      foreach(var item in ReadCashReceiptDetail5())
      {
        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier != local
            .HardcodedReleased.SystemGeneratedIdentifier)
          {
            continue;
          }
        }
        else
        {
          ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

          return;
        }

        UseFnCabCheckManualDistForCrd();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("RELEASE_FOR_DIST_SUCCESSFUL"))
        {
          if (AsChar(local.ManualDistribInd.Flag) == 'Y')
          {
            export.HidMcolCurrent.SequentialIdentifier =
              entities.ForPersistentView.SequentialIdentifier;
            MoveCashReceiptDetail2(entities.ForPersistentView,
              export.CashReceiptDetail);
            ExitState = "ECO_LNK_TO_MANUAL_DIST_OF_COLL";

            return;
          }
          else
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }

      if (AsChar(local.Retmcol.Flag) == 'Y')
      {
        global.Command = "DISPLAY";
      }
      else
      {
        ExitState = "RELEASE_FOR_DIST_SUCCESSFUL";
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if ((export.HiddenCashReceiptEvent.SystemGeneratedIdentifier == 0 || export
        .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
        .HiddenCashReceiptType.SystemGeneratedIdentifier == 0) && export
        .CashReceipt.SequentialNumber == 0)
      {
        var field = GetField(export.CashReceipt, "sequentialNumber");

        field.Error = true;

        ExitState = "FN0137_CASH_RCPT_REQD_DISP";

        return;
      }

      if (export.CashReceiptDetail.SequentialIdentifier > 0)
      {
        // ---- Continue
        UseFnReadCrAndCountCrdtls2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.TotalNoOfColl.Count = local.NoOfCrDetails.Count;
        }
        else
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }
      }
      else
      {
        UseFnReadCrAndCountCrdtls1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.TotalNoOfColl.Count = local.NoOfCrDetails.Count;

          if (local.NoOfCrDetails.Count > 1)
          {
            ExitState = "ECO_LNK_TO_LST_CASH_RECEIPT_DTL";

            return;
          }

          if (local.NoOfCrDetails.Count == 0)
          {
            ExitState = "FN0000_NO_CRDTL_FOR_THIS_CR";

            var field = GetField(export.CashReceiptDetail, "collectionAmount");

            field.Protected = false;
            field.Focused = true;

            return;
          }
        }
        else
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }
      }

      UseDisplayCrDetails1();

      if (Equal(export.CashReceiptSourceType.Code, "FDSO"))
      {
        export.CashReceiptLiterals.MoreOnPage2 = "NEXT PAGE";
      }
      else
      {
        export.CashReceiptLiterals.MoreOnPage2 = "";
      }

      if (IsEmpty(export.CashReceiptDetail.LastUpdatedBy))
      {
        export.CashReceiptDetail.LastUpdatedBy =
          export.CashReceiptDetail.CreatedBy;
      }

      if (Verify(export.CashReceiptDetail.ObligorPhoneNumber, " 0") == 0)
      {
        export.CashReceiptDetail.ObligorPhoneNumber = "";
      }

      if (Verify(export.CashReceiptDetail.ObligorSocialSecurityNumber, "0") == 0
        )
      {
        export.CashReceiptDetail.ObligorSocialSecurityNumber = "";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // 03/08/00        pphinney        H00090300 - Added PAYR to PF22 Flow
        export.HiddenCashReceipt.SequentialNumber =
          export.CashReceipt.SequentialNumber;

        // H00096632    06/02/00   pphinney
        global.Command = "RESEARCH";
        UseScCabTestSecurity();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenCashReceiptDetail.ObligorPhoneNumber = "";
        }
        else
        {
          export.CashReceiptDetailAddress.Assign(local.Blank);
          export.HiddenCashReceiptDetail.ObligorPhoneNumber =
            export.CashReceiptDetail.ObligorPhoneNumber ?? "";
          export.CashReceiptDetail.ObligorPhoneNumber = "";
          export.CashReceiptDetailAddress.Street1 = "Security Block on Address";
        }

        MoveCashReceiptDetail4(export.CashReceiptDetail, export.Previous);

        var field = GetField(export.CashReceipt, "sequentialNumber");

        field.Color = "cyan";
        field.Protected = true;

        if (AsChar(local.AdjustOk.Flag) == 'Y')
        {
          ExitState = "FN0000_ADJUST_OK_GO_TO_CRCN";
        }
        else if (AsChar(local.AdjustWithRefundOk.Flag) == 'Y')
        {
          ExitState = "FN0000_ADJUST_WITH_REFUND_OK";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
      }
      else
      {
        export.CashReceipt.Assign(import.CashReceipt);

        var field = GetField(export.CashReceiptDetail, "sequentialIdentifier");

        field.Error = true;

        // H00096632    06/02/00   pphinney
        export.CashReceiptDetailAddress.Assign(local.Blank);
      }

      // 08/02/00        pphinney        H00098632    Changed logic to NOT allow
      // change of
      // collection date if CRD is REL
      export.Save.CollectionDate = export.CashReceiptDetail.CollectionDate;

      // 08/21/00        pphinney        H00101532    Changed logic to NOT allow
      // change of
      // collection TYPE if CRD is REL
      export.HiddenCollectionType.Code = export.CollectionType2.Code;
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.CheckNumber = source.CheckNumber;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.CashDue = source.CashDue;
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.InterfaceTransId = source.InterfaceTransId;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.DefaultedCollectionDateInd = source.DefaultedCollectionDateInd;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
    target.PayeeFirstName = source.PayeeFirstName;
    target.PayeeMiddleName = source.PayeeMiddleName;
    target.PayeeLastName = source.PayeeLastName;
    target.Attribute1SupportedPersonFirstName =
      source.Attribute1SupportedPersonFirstName;
    target.Attribute1SupportedPersonMiddleName =
      source.Attribute1SupportedPersonMiddleName;
    target.Attribute1SupportedPersonLastName =
      source.Attribute1SupportedPersonLastName;
    target.Attribute2SupportedPersonFirstName =
      source.Attribute2SupportedPersonFirstName;
    target.Attribute2SupportedPersonLastName =
      source.Attribute2SupportedPersonLastName;
    target.Attribute2SupportedPersonMiddleName =
      source.Attribute2SupportedPersonMiddleName;
    target.Reference = source.Reference;
    target.Notes = source.Notes;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.InjuredSpouseInd = source.InjuredSpouseInd;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.InterfaceTransId = source.InterfaceTransId;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.DefaultedCollectionDateInd = source.DefaultedCollectionDateInd;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
    target.PayeeFirstName = source.PayeeFirstName;
    target.PayeeMiddleName = source.PayeeMiddleName;
    target.PayeeLastName = source.PayeeLastName;
    target.Attribute1SupportedPersonFirstName =
      source.Attribute1SupportedPersonFirstName;
    target.Attribute1SupportedPersonMiddleName =
      source.Attribute1SupportedPersonMiddleName;
    target.Attribute1SupportedPersonLastName =
      source.Attribute1SupportedPersonLastName;
    target.Attribute2SupportedPersonFirstName =
      source.Attribute2SupportedPersonFirstName;
    target.Attribute2SupportedPersonLastName =
      source.Attribute2SupportedPersonLastName;
    target.Attribute2SupportedPersonMiddleName =
      source.Attribute2SupportedPersonMiddleName;
    target.Reference = source.Reference;
    target.Notes = source.Notes;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptDetail3(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.DefaultedCollectionDateInd = source.DefaultedCollectionDateInd;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
    target.PayeeFirstName = source.PayeeFirstName;
    target.PayeeMiddleName = source.PayeeMiddleName;
    target.PayeeLastName = source.PayeeLastName;
    target.Attribute1SupportedPersonFirstName =
      source.Attribute1SupportedPersonFirstName;
    target.Attribute1SupportedPersonMiddleName =
      source.Attribute1SupportedPersonMiddleName;
    target.Attribute1SupportedPersonLastName =
      source.Attribute1SupportedPersonLastName;
    target.Attribute2SupportedPersonFirstName =
      source.Attribute2SupportedPersonFirstName;
    target.Attribute2SupportedPersonLastName =
      source.Attribute2SupportedPersonLastName;
    target.Attribute2SupportedPersonMiddleName =
      source.Attribute2SupportedPersonMiddleName;
    target.Reference = source.Reference;
    target.Notes = source.Notes;
  }

  private static void MoveCashReceiptDetail4(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.CollectionAmount = source.CollectionAmount;
  }

  private static void MoveCashReceiptDetail5(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
  }

  private static void MoveCashReceiptDetail6(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
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
    target.Zip3 = source.Zip3;
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

  private static void MoveObligorList(FnAbObligorListForCtOrder.Export.
    ObligorListGroup source, Local.ObligorListGroup target)
  {
    MoveCsePerson1(source.Grps, target.Grps);
    target.GrpsWork.Assign(source.GrpsWork);
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = export.FlowPaccStartDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.FlowPaccStartDate.Date = useExport.First.Date;
  }

  private void UseCabGenerateCrDetailSeqId()
  {
    var useImport = new CabGenerateCrDetailSeqId.Import();
    var useExport = new CabGenerateCrDetailSeqId.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;

    Call(CabGenerateCrDetailSeqId.Execute, useImport, useExport);

    local.PassAreaCashReceiptDetail.SequentialIdentifier =
      useExport.CashReceiptDetail.SequentialIdentifier;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.Pass.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseCreateCrDetailAddress1()
  {
    var useImport = new CreateCrDetailAddress.Import();
    var useExport = new CreateCrDetailAddress.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailAddress.Assign(export.CashReceiptDetailAddress);

    Call(CreateCrDetailAddress.Execute, useImport, useExport);

    export.CashReceiptDetailAddress.SystemGeneratedIdentifier =
      useExport.CashReceiptDetailAddress.SystemGeneratedIdentifier;
  }

  private void UseCreateCrDetailAddress2()
  {
    var useImport = new CreateCrDetailAddress.Import();
    var useExport = new CreateCrDetailAddress.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptDetailAddress.Assign(export.CashReceiptDetailAddress);

    Call(CreateCrDetailAddress.Execute, useImport, useExport);

    export.CashReceiptDetailAddress.SystemGeneratedIdentifier =
      useExport.CashReceiptDetailAddress.SystemGeneratedIdentifier;
  }

  private void UseDisplayCrDetails1()
  {
    var useImport = new DisplayCrDetails.Import();
    var useExport = new DisplayCrDetails.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(DisplayCrDetails.Execute, useImport, useExport);

    export.Adjustment.AverageCurrency = useExport.AdjustmentAmt.AverageCurrency;
    export.OriginalCollection.AverageCurrency =
      useExport.OriginalCollAmt.AverageCurrency;
    export.CollAmtApplied.TotalCurrency =
      useExport.CollAmtApplied.TotalCurrency;
    export.Suspended.TotalCurrency = useExport.Suspended.TotalCurrency;
    export.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
    MoveCashReceipt(useExport.CashReceipt, export.CashReceipt);
    export.CashReceiptSourceType.Assign(useExport.CashReceiptSourceType);
    export.CashReceiptDetailAddress.Assign(useExport.CashReceiptDetailAddress);
    MoveCollectionType(useExport.CollectionType, export.CollectionType2);
    MoveCashReceiptDetailStatus(useExport.CashReceiptDetailStatus,
      export.CashReceiptDetailStatus);
    export.CashReceiptDetailStatHistory.ReasonCodeId =
      useExport.CashReceiptDetailStatHistory.ReasonCodeId;
    export.CashReceiptLiterals.Assign(useExport.CashReceiptLiterals);
    export.WorkIsMultiPayor.Flag = useExport.WorkIsMultiPayor.Flag;
  }

  private void UseDisplayCrDetails2()
  {
    var useImport = new DisplayCrDetails.Import();
    var useExport = new DisplayCrDetails.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(DisplayCrDetails.Execute, useImport, useExport);

    export.Adjustment.AverageCurrency = useExport.AdjustmentAmt.AverageCurrency;
    export.OriginalCollection.AverageCurrency =
      useExport.OriginalCollAmt.AverageCurrency;
    export.CollAmtApplied.TotalCurrency =
      useExport.CollAmtApplied.TotalCurrency;
    export.Suspended.TotalCurrency = useExport.Suspended.TotalCurrency;
    export.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
    MoveCashReceipt(useExport.CashReceipt, export.CashReceipt);
    export.CashReceiptSourceType.Assign(useExport.CashReceiptSourceType);
    export.CashReceiptDetailAddress.Assign(useExport.CashReceiptDetailAddress);
    MoveCollectionType(useExport.CollectionType, export.CollectionType2);
    MoveCashReceiptDetailStatus(useExport.CashReceiptDetailStatus,
      export.CashReceiptDetailStatus);
    export.CashReceiptDetailStatHistory.ReasonCodeId =
      useExport.CashReceiptDetailStatHistory.ReasonCodeId;
    export.CashReceiptLiterals.Assign(useExport.CashReceiptLiterals);
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnAbDetermineCollAmtApplied1()
  {
    var useImport = new FnAbDetermineCollAmtApplied.Import();
    var useExport = new FnAbDetermineCollAmtApplied.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnAbDetermineCollAmtApplied.Execute, useImport, useExport);

    local.TotalAdjusted.TotalCurrency = useExport.TotalAdjusted.TotalCurrency;
    local.TotalRefunded.TotalCurrency = useExport.TotalRefunded.TotalCurrency;
    local.CollAmtApplied.TotalCurrency = useExport.CollAmtApplied.TotalCurrency;
  }

  private void UseFnAbDetermineCollAmtApplied2()
  {
    var useImport = new FnAbDetermineCollAmtApplied.Import();
    var useExport = new FnAbDetermineCollAmtApplied.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    MoveCashReceiptDetail5(export.CashReceiptDetail, useImport.CashReceiptDetail);
      
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnAbDetermineCollAmtApplied.Execute, useImport, useExport);

    MoveCashReceiptDetail6(useExport.Current, local.BeforeUpdate);
    local.CollAmtApplied.TotalCurrency = useExport.CollAmtApplied.TotalCurrency;
  }

  private void UseFnAbDetermineCollAmtApplied3()
  {
    var useImport = new FnAbDetermineCollAmtApplied.Import();
    var useExport = new FnAbDetermineCollAmtApplied.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnAbDetermineCollAmtApplied.Execute, useImport, useExport);

    local.CollAmtApplied.TotalCurrency = useExport.CollAmtApplied.TotalCurrency;
  }

  private void UseFnAbObligorListForCtOrder()
  {
    var useImport = new FnAbObligorListForCtOrder.Import();
    var useExport = new FnAbObligorListForCtOrder.Export();

    useImport.CashReceiptDetail.CourtOrderNumber =
      export.CashReceiptDetail.CourtOrderNumber;

    Call(FnAbObligorListForCtOrder.Execute, useImport, useExport);

    local.WorkNoOfObligors.Count = useExport.WorkNoOfObligors.Count;
    useExport.ObligorList.CopyTo(local.ObligorList, MoveObligorList);
  }

  private void UseFnCabCheckManualDistForCrd()
  {
    var useImport = new FnCabCheckManualDistForCrd.Import();
    var useExport = new FnCabCheckManualDistForCrd.Export();

    useImport.CashReceiptDetail.Assign(entities.ForPersistentView);

    Call(FnCabCheckManualDistForCrd.Execute, useImport, useExport);

    local.ManualDistribInd.Flag = useExport.ManualDistribInd.Flag;
  }

  private void UseFnChangeCashRcptDtlStatHis1()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnChangeCashRcptDtlStatHis2()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedAdjusted.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnChangeCashRcptDtlStatHis3()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedPended.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.New1.ReasonCodeId =
      export.CashReceiptDetailStatHistory.ReasonCodeId;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting1()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedDeposited.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
    local.HardcodedInterface.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
    local.HardcodedAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
    local.HardcodedDistributed.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    local.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedPended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
    local.HardcodedReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedCashReceipting2()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
  }

  private void UseFnReadCrAndCountCrdtls1()
  {
    var useImport = new FnReadCrAndCountCrdtls.Import();
    var useExport = new FnReadCrAndCountCrdtls.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnReadCrAndCountCrdtls.Execute, useImport, useExport);

    local.NoOfCrDetails.Count = useExport.NoOfCrDetails.Count;
    export.DeletedCashReceiptFlag.Flag = useExport.DeletedCashReceiptFlag.Flag;
    export.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
    export.HiddenCashReceiptType.SystemGeneratedIdentifier =
      useExport.CashReceiptType.SystemGeneratedIdentifier;
    export.CashReceiptDetail.SequentialIdentifier =
      useExport.CashReceiptDetail.SequentialIdentifier;
    MoveCashReceipt(useExport.CashReceipt, export.CashReceipt);
    export.CashReceiptSourceType.Assign(useExport.CashReceiptSourceType);
  }

  private void UseFnReadCrAndCountCrdtls2()
  {
    var useImport = new FnReadCrAndCountCrdtls.Import();
    var useExport = new FnReadCrAndCountCrdtls.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnReadCrAndCountCrdtls.Execute, useImport, useExport);

    local.NoOfCrDetails.Count = useExport.NoOfCrDetails.Count;
    export.DeletedCashReceiptFlag.Flag = useExport.DeletedCashReceiptFlag.Flag;
  }

  private void UseFnReadCsePersonUsingSsnO()
  {
    var useImport = new FnReadCsePersonUsingSsnO.Import();
    var useExport = new FnReadCsePersonUsingSsnO.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    useExport.AbendData.Assign(local.AbendData);
    useExport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(FnReadCsePersonUsingSsnO.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UsePendCollection()
  {
    var useImport = new PendCollection.Import();
    var useExport = new PendCollection.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptDetailStatus(export.CashReceiptDetailStatus,
      useImport.CashReceiptDetailStatus);
    useImport.CashReceiptDetailStatHistory.ReasonCodeId =
      export.CashReceiptDetailStatHistory.ReasonCodeId;

    Call(PendCollection.Execute, useImport, useExport);

    MoveCashReceiptDetailStatus(useExport.CashReceiptDetailStatus,
      export.CashReceiptDetailStatus);
    export.CashReceiptDetailStatHistory.ReasonCodeId =
      useExport.CashReceiptDetailStatHistory.ReasonCodeId;
  }

  private void UseRecordCollection()
  {
    var useImport = new RecordCollection.Import();
    var useExport = new RecordCollection.Export();

    useImport.PersistentCashReceiptDetailStatus.Assign(
      entities.MatchForPersistent);
    useImport.CashReceipt.SequentialNumber =
      entities.MatchedForPersistent.SequentialNumber;
    MoveCashReceiptDetail3(local.PassAreaCashReceiptDetail,
      useImport.CashReceiptDetail);
    MoveCollectionType(export.CollectionType2, useImport.CollectionType);

    Call(RecordCollection.Execute, useImport, useExport);

    entities.MatchForPersistent.SystemGeneratedIdentifier =
      useImport.PersistentCashReceiptDetailStatus.SystemGeneratedIdentifier;
    MoveCashReceiptDetail1(useExport.CashReceiptDetail, export.CashReceiptDetail);
      
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

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson2(useExport.CsePerson, local.Phone);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson2(useExport.CsePerson, local.Phone);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseUpdateCollection1()
  {
    var useImport = new UpdateCollection.Import();
    var useExport = new UpdateCollection.Export();

    useImport.ToBeUnpended.Flag = local.ToBeUnpended.Flag;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.Assign(export.CashReceiptDetail);
    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CollectionType.SequentialIdentifier =
      export.CollectionType2.SequentialIdentifier;

    Call(UpdateCollection.Execute, useImport, useExport);
  }

  private void UseUpdateCollection2()
  {
    var useImport = new UpdateCollection.Import();
    var useExport = new UpdateCollection.Export();

    useImport.Adjust.Flag = local.AdjustInUpdateCab.Flag;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.Assign(export.CashReceiptDetail);
    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CollectionType.SequentialIdentifier =
      export.CollectionType2.SequentialIdentifier;

    Call(UpdateCollection.Execute, useImport, useExport);
  }

  private void UseUpdateCrDetailAddress()
  {
    var useImport = new UpdateCrDetailAddress.Import();
    var useExport = new UpdateCrDetailAddress.Export();

    useImport.CashReceiptDetailAddress.Assign(export.CashReceiptDetailAddress);

    Call(UpdateCrDetailAddress.Execute, useImport, useExport);
  }

  private bool ReadCashReceipt1()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          export.HiddenCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.HiddenCashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceipt2()
  {
    entities.AdjustCashReceipt.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          export.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          export.HiddenCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          export.HiddenCashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.AdjustCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.AdjustCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.AdjustCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.AdjustCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.AdjustCashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceipt3()
  {
    entities.MatchedForPersistent.Populated = false;

    return Read("ReadCashReceipt3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          export.HiddenCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          export.HiddenCashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MatchedForPersistent.CrvIdentifier = db.GetInt32(reader, 0);
        entities.MatchedForPersistent.CstIdentifier = db.GetInt32(reader, 1);
        entities.MatchedForPersistent.CrtIdentifier = db.GetInt32(reader, 2);
        entities.MatchedForPersistent.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.MatchedForPersistent.SequentialNumber = db.GetInt32(reader, 4);
        entities.MatchedForPersistent.CheckType =
          db.GetNullableString(reader, 5);
        entities.MatchedForPersistent.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.MatchedForPersistent.CashDue =
          db.GetNullableDecimal(reader, 7);
        entities.MatchedForPersistent.Populated = true;
      });
  }

  private bool ReadCashReceipt4()
  {
    entities.MatchedForPersistent.Populated = false;

    return Read("ReadCashReceipt4",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.HiddenCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.HiddenCashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MatchedForPersistent.CrvIdentifier = db.GetInt32(reader, 0);
        entities.MatchedForPersistent.CstIdentifier = db.GetInt32(reader, 1);
        entities.MatchedForPersistent.CrtIdentifier = db.GetInt32(reader, 2);
        entities.MatchedForPersistent.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.MatchedForPersistent.SequentialNumber = db.GetInt32(reader, 4);
        entities.MatchedForPersistent.CheckType =
          db.GetNullableString(reader, 5);
        entities.MatchedForPersistent.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.MatchedForPersistent.CashDue =
          db.GetNullableDecimal(reader, 7);
        entities.MatchedForPersistent.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", export.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", export.CashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          export.HiddenCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          export.HiddenCashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "interfaceTranId",
          export.CashReceiptDetail.InterfaceTransId ?? "");
        db.SetInt32(
          command, "cstIdentifier",
          entities.InterfaceCashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ForPersistentView.Populated = false;

    return Read("ReadCashReceiptDetail3",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "crdId", export.CashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.ForPersistentView.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ForPersistentView.CstIdentifier = db.GetInt32(reader, 1);
        entities.ForPersistentView.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ForPersistentView.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ForPersistentView.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.ForPersistentView.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ForPersistentView.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ForPersistentView.CaseNumber = db.GetNullableString(reader, 7);
        entities.ForPersistentView.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.ForPersistentView.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.ForPersistentView.CollectionAmount = db.GetDecimal(reader, 10);
        entities.ForPersistentView.CollectionDate = db.GetDate(reader, 11);
        entities.ForPersistentView.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.ForPersistentView.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.ForPersistentView.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.ForPersistentView.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.ForPersistentView.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.ForPersistentView.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.ForPersistentView.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.ForPersistentView.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.ForPersistentView.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.ForPersistentView.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.ForPersistentView.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.ForPersistentView.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.ForPersistentView.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.ForPersistentView.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.ForPersistentView.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.ForPersistentView.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.ForPersistentView.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.ForPersistentView.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.ForPersistentView.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.ForPersistentView.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.ForPersistentView.CreatedBy = db.GetString(reader, 32);
        entities.ForPersistentView.CreatedTmst = db.GetDateTime(reader, 33);
        entities.ForPersistentView.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.ForPersistentView.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.ForPersistentView.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.ForPersistentView.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.ForPersistentView.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.ForPersistentView.Reference = db.GetNullableString(reader, 39);
        entities.ForPersistentView.Notes = db.GetNullableString(reader, 40);
        entities.ForPersistentView.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.ForPersistentView.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.ForPersistentView.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.ForPersistentView.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ForPersistentView.CollectionAmtFullyAppliedInd);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail4()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail4",
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
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail5()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ForPersistentView.Populated = false;

    return ReadEach("ReadCashReceiptDetail5",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "crdId", export.HidMcolCurrent.SequentialIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ForPersistentView.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ForPersistentView.CstIdentifier = db.GetInt32(reader, 1);
        entities.ForPersistentView.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ForPersistentView.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ForPersistentView.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.ForPersistentView.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ForPersistentView.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ForPersistentView.CaseNumber = db.GetNullableString(reader, 7);
        entities.ForPersistentView.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.ForPersistentView.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.ForPersistentView.CollectionAmount = db.GetDecimal(reader, 10);
        entities.ForPersistentView.CollectionDate = db.GetDate(reader, 11);
        entities.ForPersistentView.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.ForPersistentView.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.ForPersistentView.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.ForPersistentView.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.ForPersistentView.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.ForPersistentView.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.ForPersistentView.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.ForPersistentView.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.ForPersistentView.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.ForPersistentView.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.ForPersistentView.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.ForPersistentView.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.ForPersistentView.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.ForPersistentView.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.ForPersistentView.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.ForPersistentView.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.ForPersistentView.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.ForPersistentView.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.ForPersistentView.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.ForPersistentView.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.ForPersistentView.CreatedBy = db.GetString(reader, 32);
        entities.ForPersistentView.CreatedTmst = db.GetDateTime(reader, 33);
        entities.ForPersistentView.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.ForPersistentView.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.ForPersistentView.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.ForPersistentView.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.ForPersistentView.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.ForPersistentView.Reference = db.GetNullableString(reader, 39);
        entities.ForPersistentView.Notes = db.GetNullableString(reader, 40);
        entities.ForPersistentView.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.ForPersistentView.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.ForPersistentView.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.ForPersistentView.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ForPersistentView.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(entities.MatchedForPersistent.Populated);
    entities.CashReceiptDetailFee.Populated = false;

    return ReadEach("ReadCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          export.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.MatchedForPersistent.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.MatchedForPersistent.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.MatchedForPersistent.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFee.CrdIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailFee.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailFee.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailFee.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 4);
        entities.CashReceiptDetailFee.Amount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetailFee.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus2()
  {
    System.Diagnostics.Debug.Assert(entities.ForPersistentView.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ForPersistentView.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.ForPersistentView.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.ForPersistentView.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.ForPersistentView.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatusCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ForPersistentView.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatusCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ForPersistentView.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.ForPersistentView.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.ForPersistentView.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.ForPersistentView.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.MatchedForPersistent.Populated);
    entities.InterfaceCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.MatchedForPersistent.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.InterfaceCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.InterfaceCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.InterfaceCashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.InterfaceCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 3);
        entities.InterfaceCashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.InterfaceCashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus1",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptStatusHistory.Populated = true;
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus2()
  {
    System.Diagnostics.Debug.Assert(entities.MatchedForPersistent.Populated);
    entities.CashReceiptStatusHistory.Populated = false;
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier",
          entities.MatchedForPersistent.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.MatchedForPersistent.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.MatchedForPersistent.CrvIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptStatusHistory.Populated = true;
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptType1()
  {
    System.Diagnostics.Debug.Assert(entities.MatchedForPersistent.Populated);
    entities.InterfaceCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.MatchedForPersistent.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.InterfaceCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.InterfaceCashReceiptType.Code = db.GetString(reader, 1);
        entities.InterfaceCashReceiptType.CategoryIndicator =
          db.GetString(reader, 2);
        entities.InterfaceCashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.InterfaceCashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptType2()
  {
    System.Diagnostics.Debug.Assert(entities.MatchedForPersistent.Populated);
    entities.Verify.Populated = false;

    return Read("ReadCashReceiptType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.MatchedForPersistent.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.Verify.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Verify.Code = db.GetString(reader, 1);
        entities.Verify.CategoryIndicator = db.GetString(reader, 2);
        entities.Verify.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.Verify.CategoryIndicator);
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", export.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", export.CashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          export.HiddenCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          export.HiddenCashReceiptType.SystemGeneratedIdentifier);
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
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType2.Code);
        db.SetDate(
          command, "effectiveDate",
          export.CashReceiptDetail.CollectionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.CashNonCashInd = db.GetString(reader, 2);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 3);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.CollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CollectionType.CashNonCashInd);
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
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
    /// A value of Pf17FlowTo.
    /// </summary>
    [JsonPropertyName("pf17FlowTo")]
    public Standard Pf17FlowTo
    {
      get => pf17FlowTo ??= new();
      set => pf17FlowTo = value;
    }

    /// <summary>
    /// A value of PassFromCrdlCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("passFromCrdlCashReceiptDetail")]
    public CashReceiptDetail PassFromCrdlCashReceiptDetail
    {
      get => passFromCrdlCashReceiptDetail ??= new();
      set => passFromCrdlCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PassFromCrdlCashReceipt.
    /// </summary>
    [JsonPropertyName("passFromCrdlCashReceipt")]
    public CashReceipt PassFromCrdlCashReceipt
    {
      get => passFromCrdlCashReceipt ??= new();
      set => passFromCrdlCashReceipt = value;
    }

    /// <summary>
    /// A value of PassFromCrdlCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("passFromCrdlCashReceiptEvent")]
    public CashReceiptEvent PassFromCrdlCashReceiptEvent
    {
      get => passFromCrdlCashReceiptEvent ??= new();
      set => passFromCrdlCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of PassFromCrdlCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("passFromCrdlCashReceiptSourceType")]
    public CashReceiptSourceType PassFromCrdlCashReceiptSourceType
    {
      get => passFromCrdlCashReceiptSourceType ??= new();
      set => passFromCrdlCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PassFromCrdlCashReceiptType.
    /// </summary>
    [JsonPropertyName("passFromCrdlCashReceiptType")]
    public CashReceiptType PassFromCrdlCashReceiptType
    {
      get => passFromCrdlCashReceiptType ??= new();
      set => passFromCrdlCashReceiptType = value;
    }

    /// <summary>
    /// A value of PassFromClct.
    /// </summary>
    [JsonPropertyName("passFromClct")]
    public CollectionType PassFromClct
    {
      get => passFromClct ??= new();
      set => passFromClct = value;
    }

    /// <summary>
    /// A value of HidMcolCurrent.
    /// </summary>
    [JsonPropertyName("hidMcolCurrent")]
    public CashReceiptDetail HidMcolCurrent
    {
      get => hidMcolCurrent ??= new();
      set => hidMcolCurrent = value;
    }

    /// <summary>
    /// A value of WorkIsMultiPayor.
    /// </summary>
    [JsonPropertyName("workIsMultiPayor")]
    public Common WorkIsMultiPayor
    {
      get => workIsMultiPayor ??= new();
      set => workIsMultiPayor = value;
    }

    /// <summary>
    /// A value of DeletedCashReceiptFlag.
    /// </summary>
    [JsonPropertyName("deletedCashReceiptFlag")]
    public Common DeletedCashReceiptFlag
    {
      get => deletedCashReceiptFlag ??= new();
      set => deletedCashReceiptFlag = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public Common Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of OriginalCollection.
    /// </summary>
    [JsonPropertyName("originalCollection")]
    public Common OriginalCollection
    {
      get => originalCollection ??= new();
      set => originalCollection = value;
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
    /// A value of TotalNoOfColl.
    /// </summary>
    [JsonPropertyName("totalNoOfColl")]
    public Common TotalNoOfColl
    {
      get => totalNoOfColl ??= new();
      set => totalNoOfColl = value;
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
    /// A value of PendRsnPrompt.
    /// </summary>
    [JsonPropertyName("pendRsnPrompt")]
    public Standard PendRsnPrompt
    {
      get => pendRsnPrompt ??= new();
      set => pendRsnPrompt = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CashReceiptDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of CollAmtApplied.
    /// </summary>
    [JsonPropertyName("collAmtApplied")]
    public Common CollAmtApplied
    {
      get => collAmtApplied ??= new();
      set => collAmtApplied = value;
    }

    /// <summary>
    /// A value of CollectionType1.
    /// </summary>
    [JsonPropertyName("collectionType1")]
    public Standard CollectionType1
    {
      get => collectionType1 ??= new();
      set => collectionType1 = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Standard Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public Common Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of CollectionType2.
    /// </summary>
    [JsonPropertyName("collectionType2")]
    public CollectionType CollectionType2
    {
      get => collectionType2 ??= new();
      set => collectionType2 = value;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptLiterals.
    /// </summary>
    [JsonPropertyName("cashReceiptLiterals")]
    public CashReceiptLiterals CashReceiptLiterals
    {
      get => cashReceiptLiterals ??= new();
      set => cashReceiptLiterals = value;
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
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
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
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
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
    /// A value of HiddenCashReceipt.
    /// </summary>
    [JsonPropertyName("hiddenCashReceipt")]
    public CashReceipt HiddenCashReceipt
    {
      get => hiddenCashReceipt ??= new();
      set => hiddenCashReceipt = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDetail")]
    public CashReceiptDetail HiddenCashReceiptDetail
    {
      get => hiddenCashReceiptDetail ??= new();
      set => hiddenCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CashReceiptDetail Save
    {
      get => save ??= new();
      set => save = value;
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

    private Code code;
    private Standard pf17FlowTo;
    private CashReceiptDetail passFromCrdlCashReceiptDetail;
    private CashReceipt passFromCrdlCashReceipt;
    private CashReceiptEvent passFromCrdlCashReceiptEvent;
    private CashReceiptSourceType passFromCrdlCashReceiptSourceType;
    private CashReceiptType passFromCrdlCashReceiptType;
    private CollectionType passFromClct;
    private CashReceiptDetail hidMcolCurrent;
    private Common workIsMultiPayor;
    private Common deletedCashReceiptFlag;
    private Common adjustment;
    private Common originalCollection;
    private TextWorkArea amtPrompt;
    private Common totalNoOfColl;
    private CodeValue codeValue;
    private Standard pendRsnPrompt;
    private CashReceiptDetail previous;
    private Common collAmtApplied;
    private Standard collectionType1;
    private Standard collection;
    private Common suspended;
    private CashReceipt cashReceipt;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CollectionType collectionType2;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptLiterals cashReceiptLiterals;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType hiddenCashReceiptType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CashReceipt hiddenCashReceipt;
    private CashReceiptDetail hiddenCashReceiptDetail;
    private CashReceiptDetail save;
    private CollectionType hiddenCollectionType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FlowPaccStartDate.
    /// </summary>
    [JsonPropertyName("flowPaccStartDate")]
    public DateWorkArea FlowPaccStartDate
    {
      get => flowPaccStartDate ??= new();
      set => flowPaccStartDate = value;
    }

    /// <summary>
    /// A value of FlowPaccEndDate.
    /// </summary>
    [JsonPropertyName("flowPaccEndDate")]
    public DateWorkArea FlowPaccEndDate
    {
      get => flowPaccEndDate ??= new();
      set => flowPaccEndDate = value;
    }

    /// <summary>
    /// A value of FlowTo.
    /// </summary>
    [JsonPropertyName("flowTo")]
    public CsePersonsWorkSet FlowTo
    {
      get => flowTo ??= new();
      set => flowTo = value;
    }

    /// <summary>
    /// A value of Pf17FlowTo.
    /// </summary>
    [JsonPropertyName("pf17FlowTo")]
    public Standard Pf17FlowTo
    {
      get => pf17FlowTo ??= new();
      set => pf17FlowTo = value;
    }

    /// <summary>
    /// A value of ToCrda.
    /// </summary>
    [JsonPropertyName("toCrda")]
    public CashReceipt ToCrda
    {
      get => toCrda ??= new();
      set => toCrda = value;
    }

    /// <summary>
    /// A value of ToMtrn.
    /// </summary>
    [JsonPropertyName("toMtrn")]
    public ElectronicFundTransmission ToMtrn
    {
      get => toMtrn ??= new();
      set => toMtrn = value;
    }

    /// <summary>
    /// A value of HidMcolCurrent.
    /// </summary>
    [JsonPropertyName("hidMcolCurrent")]
    public CashReceiptDetail HidMcolCurrent
    {
      get => hidMcolCurrent ??= new();
      set => hidMcolCurrent = value;
    }

    /// <summary>
    /// A value of WorkIsMultiPayor.
    /// </summary>
    [JsonPropertyName("workIsMultiPayor")]
    public Common WorkIsMultiPayor
    {
      get => workIsMultiPayor ??= new();
      set => workIsMultiPayor = value;
    }

    /// <summary>
    /// A value of DeletedCashReceiptFlag.
    /// </summary>
    [JsonPropertyName("deletedCashReceiptFlag")]
    public Common DeletedCashReceiptFlag
    {
      get => deletedCashReceiptFlag ??= new();
      set => deletedCashReceiptFlag = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public Common Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of OriginalCollection.
    /// </summary>
    [JsonPropertyName("originalCollection")]
    public Common OriginalCollection
    {
      get => originalCollection ??= new();
      set => originalCollection = value;
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
    /// A value of AmtPrompt.
    /// </summary>
    [JsonPropertyName("amtPrompt")]
    public TextWorkArea AmtPrompt
    {
      get => amtPrompt ??= new();
      set => amtPrompt = value;
    }

    /// <summary>
    /// A value of TotalNoOfColl.
    /// </summary>
    [JsonPropertyName("totalNoOfColl")]
    public Common TotalNoOfColl
    {
      get => totalNoOfColl ??= new();
      set => totalNoOfColl = value;
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
    /// A value of PendRsnPrompt.
    /// </summary>
    [JsonPropertyName("pendRsnPrompt")]
    public Standard PendRsnPrompt
    {
      get => pendRsnPrompt ??= new();
      set => pendRsnPrompt = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CashReceiptDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of CollAmtApplied.
    /// </summary>
    [JsonPropertyName("collAmtApplied")]
    public Common CollAmtApplied
    {
      get => collAmtApplied ??= new();
      set => collAmtApplied = value;
    }

    /// <summary>
    /// A value of CollectionType1.
    /// </summary>
    [JsonPropertyName("collectionType1")]
    public Standard CollectionType1
    {
      get => collectionType1 ??= new();
      set => collectionType1 = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Standard Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public Common Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of CollectionType2.
    /// </summary>
    [JsonPropertyName("collectionType2")]
    public CollectionType CollectionType2
    {
      get => collectionType2 ??= new();
      set => collectionType2 = value;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptLiterals.
    /// </summary>
    [JsonPropertyName("cashReceiptLiterals")]
    public CashReceiptLiterals CashReceiptLiterals
    {
      get => cashReceiptLiterals ??= new();
      set => cashReceiptLiterals = value;
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
    /// A value of SendTo.
    /// </summary>
    [JsonPropertyName("sendTo")]
    public CashReceiptDetailAddress SendTo
    {
      get => sendTo ??= new();
      set => sendTo = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePerson Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDetail")]
    public CashReceiptDetail HiddenCashReceiptDetail
    {
      get => hiddenCashReceiptDetail ??= new();
      set => hiddenCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CashReceiptDetail Save
    {
      get => save ??= new();
      set => save = value;
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

    private DateWorkArea flowPaccStartDate;
    private DateWorkArea flowPaccEndDate;
    private CsePersonsWorkSet flowTo;
    private Standard pf17FlowTo;
    private CashReceipt toCrda;
    private ElectronicFundTransmission toMtrn;
    private CashReceiptDetail hidMcolCurrent;
    private Common workIsMultiPayor;
    private Common deletedCashReceiptFlag;
    private Common adjustment;
    private Common originalCollection;
    private CsePerson csePerson;
    private TextWorkArea amtPrompt;
    private Common totalNoOfColl;
    private Code code;
    private Standard pendRsnPrompt;
    private CashReceiptDetail previous;
    private Common collAmtApplied;
    private Standard collectionType1;
    private Standard collection;
    private Common suspended;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CollectionType collectionType2;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptLiterals cashReceiptLiterals;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CashReceiptDetailAddress sendTo;
    private CashReceipt hiddenCashReceipt;
    private CsePerson pass;
    private CashReceiptDetail hiddenCashReceiptDetail;
    private CashReceiptDetail save;
    private CollectionType hiddenCollectionType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>A ObligorListGroup group.</summary>
    [Serializable]
    public class ObligorListGroup
    {
      /// <summary>
      /// A value of Grps.
      /// </summary>
      [JsonPropertyName("grps")]
      public CsePerson Grps
      {
        get => grps ??= new();
        set => grps = value;
      }

      /// <summary>
      /// A value of GrpsWork.
      /// </summary>
      [JsonPropertyName("grpsWork")]
      public CsePersonsWorkSet GrpsWork
      {
        get => grpsWork ??= new();
        set => grpsWork = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePerson grps;
      private CsePersonsWorkSet grpsWork;
    }

    /// <summary>
    /// A value of AdjustInUpdateCab.
    /// </summary>
    [JsonPropertyName("adjustInUpdateCab")]
    public Common AdjustInUpdateCab
    {
      get => adjustInUpdateCab ??= new();
      set => adjustInUpdateCab = value;
    }

    /// <summary>
    /// A value of AdjustWithRefundOk.
    /// </summary>
    [JsonPropertyName("adjustWithRefundOk")]
    public Common AdjustWithRefundOk
    {
      get => adjustWithRefundOk ??= new();
      set => adjustWithRefundOk = value;
    }

    /// <summary>
    /// A value of AdjustOk.
    /// </summary>
    [JsonPropertyName("adjustOk")]
    public Common AdjustOk
    {
      get => adjustOk ??= new();
      set => adjustOk = value;
    }

    /// <summary>
    /// A value of CurrentDateText.
    /// </summary>
    [JsonPropertyName("currentDateText")]
    public TextWorkArea CurrentDateText
    {
      get => currentDateText ??= new();
      set => currentDateText = value;
    }

    /// <summary>
    /// A value of SkipManDistRead.
    /// </summary>
    [JsonPropertyName("skipManDistRead")]
    public Common SkipManDistRead
    {
      get => skipManDistRead ??= new();
      set => skipManDistRead = value;
    }

    /// <summary>
    /// A value of MultipayorOk.
    /// </summary>
    [JsonPropertyName("multipayorOk")]
    public Common MultipayorOk
    {
      get => multipayorOk ??= new();
      set => multipayorOk = value;
    }

    /// <summary>
    /// A value of Female.
    /// </summary>
    [JsonPropertyName("female")]
    public Common Female
    {
      get => female ??= new();
      set => female = value;
    }

    /// <summary>
    /// A value of Male.
    /// </summary>
    [JsonPropertyName("male")]
    public Common Male
    {
      get => male ??= new();
      set => male = value;
    }

    /// <summary>
    /// A value of HardcodedReceipted.
    /// </summary>
    [JsonPropertyName("hardcodedReceipted")]
    public CashReceiptStatus HardcodedReceipted
    {
      get => hardcodedReceipted ??= new();
      set => hardcodedReceipted = value;
    }

    /// <summary>
    /// A value of Retmcol.
    /// </summary>
    [JsonPropertyName("retmcol")]
    public Common Retmcol
    {
      get => retmcol ??= new();
      set => retmcol = value;
    }

    /// <summary>
    /// A value of InitializeCashReceiptLiterals.
    /// </summary>
    [JsonPropertyName("initializeCashReceiptLiterals")]
    public CashReceiptLiterals InitializeCashReceiptLiterals
    {
      get => initializeCashReceiptLiterals ??= new();
      set => initializeCashReceiptLiterals = value;
    }

    /// <summary>
    /// A value of TotalFees.
    /// </summary>
    [JsonPropertyName("totalFees")]
    public Common TotalFees
    {
      get => totalFees ??= new();
      set => totalFees = value;
    }

    /// <summary>
    /// A value of HardcodedDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedDeposited")]
    public CashReceiptStatus HardcodedDeposited
    {
      get => hardcodedDeposited ??= new();
      set => hardcodedDeposited = value;
    }

    /// <summary>
    /// A value of HardcodedInterface.
    /// </summary>
    [JsonPropertyName("hardcodedInterface")]
    public CashReceiptStatus HardcodedInterface
    {
      get => hardcodedInterface ??= new();
      set => hardcodedInterface = value;
    }

    /// <summary>
    /// A value of HardcodedAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedAdjusted")]
    public CashReceiptDetailStatus HardcodedAdjusted
    {
      get => hardcodedAdjusted ??= new();
      set => hardcodedAdjusted = value;
    }

    /// <summary>
    /// A value of HardcodedDistributed.
    /// </summary>
    [JsonPropertyName("hardcodedDistributed")]
    public CashReceiptDetailStatus HardcodedDistributed
    {
      get => hardcodedDistributed ??= new();
      set => hardcodedDistributed = value;
    }

    /// <summary>
    /// A value of HardcodedRefunded.
    /// </summary>
    [JsonPropertyName("hardcodedRefunded")]
    public CashReceiptDetailStatus HardcodedRefunded
    {
      get => hardcodedRefunded ??= new();
      set => hardcodedRefunded = value;
    }

    /// <summary>
    /// A value of HardcodedReleased.
    /// </summary>
    [JsonPropertyName("hardcodedReleased")]
    public CashReceiptDetailStatus HardcodedReleased
    {
      get => hardcodedReleased ??= new();
      set => hardcodedReleased = value;
    }

    /// <summary>
    /// A value of HardcodedRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedRecorded")]
    public CashReceiptDetailStatus HardcodedRecorded
    {
      get => hardcodedRecorded ??= new();
      set => hardcodedRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedSuspended")]
    public CashReceiptDetailStatus HardcodedSuspended
    {
      get => hardcodedSuspended ??= new();
      set => hardcodedSuspended = value;
    }

    /// <summary>
    /// A value of HardcodedPended.
    /// </summary>
    [JsonPropertyName("hardcodedPended")]
    public CashReceiptDetailStatus HardcodedPended
    {
      get => hardcodedPended ??= new();
      set => hardcodedPended = value;
    }

    /// <summary>
    /// A value of Phone.
    /// </summary>
    [JsonPropertyName("phone")]
    public CsePerson Phone
    {
      get => phone ??= new();
      set => phone = value;
    }

    /// <summary>
    /// A value of ToBeUnpended.
    /// </summary>
    [JsonPropertyName("toBeUnpended")]
    public Common ToBeUnpended
    {
      get => toBeUnpended ??= new();
      set => toBeUnpended = value;
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
    /// A value of TotalAdjusted.
    /// </summary>
    [JsonPropertyName("totalAdjusted")]
    public Common TotalAdjusted
    {
      get => totalAdjusted ??= new();
      set => totalAdjusted = value;
    }

    /// <summary>
    /// A value of TotalRefunded.
    /// </summary>
    [JsonPropertyName("totalRefunded")]
    public Common TotalRefunded
    {
      get => totalRefunded ??= new();
      set => totalRefunded = value;
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
    /// A value of IsAddressFound.
    /// </summary>
    [JsonPropertyName("isAddressFound")]
    public Common IsAddressFound
    {
      get => isAddressFound ??= new();
      set => isAddressFound = value;
    }

    /// <summary>
    /// A value of IsCrDetailPopulated.
    /// </summary>
    [JsonPropertyName("isCrDetailPopulated")]
    public Common IsCrDetailPopulated
    {
      get => isCrDetailPopulated ??= new();
      set => isCrDetailPopulated = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CanBeReleased.
    /// </summary>
    [JsonPropertyName("canBeReleased")]
    public Common CanBeReleased
    {
      get => canBeReleased ??= new();
      set => canBeReleased = value;
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
    /// A value of WorkSex.
    /// </summary>
    [JsonPropertyName("workSex")]
    public CsePersonsWorkSet WorkSex
    {
      get => workSex ??= new();
      set => workSex = value;
    }

    /// <summary>
    /// A value of WorkNoOfObligors.
    /// </summary>
    [JsonPropertyName("workNoOfObligors")]
    public Common WorkNoOfObligors
    {
      get => workNoOfObligors ??= new();
      set => workNoOfObligors = value;
    }

    /// <summary>
    /// Gets a value of ObligorList.
    /// </summary>
    [JsonIgnore]
    public Array<ObligorListGroup> ObligorList => obligorList ??= new(
      ObligorListGroup.Capacity);

    /// <summary>
    /// Gets a value of ObligorList for json serialization.
    /// </summary>
    [JsonPropertyName("obligorList")]
    [Computed]
    public IList<ObligorListGroup> ObligorList_Json
    {
      get => obligorList;
      set => ObligorList.Assign(value);
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
    /// A value of ManualDistribInd.
    /// </summary>
    [JsonPropertyName("manualDistribInd")]
    public Common ManualDistribInd
    {
      get => manualDistribInd ??= new();
      set => manualDistribInd = value;
    }

    /// <summary>
    /// A value of BeforeUpdate.
    /// </summary>
    [JsonPropertyName("beforeUpdate")]
    public CashReceiptDetail BeforeUpdate
    {
      get => beforeUpdate ??= new();
      set => beforeUpdate = value;
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
    /// A value of CollectionDifference.
    /// </summary>
    [JsonPropertyName("collectionDifference")]
    public Common CollectionDifference
    {
      get => collectionDifference ??= new();
      set => collectionDifference = value;
    }

    /// <summary>
    /// A value of CollAmtApplied.
    /// </summary>
    [JsonPropertyName("collAmtApplied")]
    public Common CollAmtApplied
    {
      get => collAmtApplied ??= new();
      set => collAmtApplied = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InitializeCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("initializeCashReceiptDetailAddress")]
    public CashReceiptDetailAddress InitializeCashReceiptDetailAddress
    {
      get => initializeCashReceiptDetailAddress ??= new();
      set => initializeCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public CashReceiptDetailStatus Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of FeesRequired.
    /// </summary>
    [JsonPropertyName("feesRequired")]
    public Common FeesRequired
    {
      get => feesRequired ??= new();
      set => feesRequired = value;
    }

    /// <summary>
    /// A value of PassAreaCollectionType.
    /// </summary>
    [JsonPropertyName("passAreaCollectionType")]
    public CollectionType PassAreaCollectionType
    {
      get => passAreaCollectionType ??= new();
      set => passAreaCollectionType = value;
    }

    /// <summary>
    /// A value of PassAreaCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("passAreaCashReceiptDetail")]
    public CashReceiptDetail PassAreaCashReceiptDetail
    {
      get => passAreaCashReceiptDetail ??= new();
      set => passAreaCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CodeValue Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of Initial.
    /// </summary>
    [JsonPropertyName("initial")]
    public DateWorkArea Initial
    {
      get => initial ??= new();
      set => initial = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CashReceiptDetailAddress Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      adjustInUpdateCab = null;
      adjustWithRefundOk = null;
      adjustOk = null;
      currentDateText = null;
      skipManDistRead = null;
      multipayorOk = null;
      female = null;
      male = null;
      hardcodedReceipted = null;
      initializeCashReceiptLiterals = null;
      totalFees = null;
      hardcodedDeposited = null;
      hardcodedInterface = null;
      hardcodedAdjusted = null;
      hardcodedDistributed = null;
      hardcodedRefunded = null;
      hardcodedReleased = null;
      hardcodedRecorded = null;
      hardcodedSuspended = null;
      hardcodedPended = null;
      phone = null;
      toBeUnpended = null;
      max = null;
      totalAdjusted = null;
      totalRefunded = null;
      csePersonAddress = null;
      isAddressFound = null;
      isCrDetailPopulated = null;
      cashReceiptDetailStatus = null;
      cashReceiptDetail = null;
      canBeReleased = null;
      current = null;
      workSex = null;
      workNoOfObligors = null;
      obligorList = null;
      abendData = null;
      manualDistribInd = null;
      beforeUpdate = null;
      noOfCrDetails = null;
      collectionDifference = null;
      collAmtApplied = null;
      textWorkArea = null;
      csePersonsWorkSet = null;
      initializeCashReceiptDetailAddress = null;
      temp = null;
      legalAction = null;
      case1 = null;
      csePerson = null;
      feesRequired = null;
      passAreaCollectionType = null;
      passAreaCashReceiptDetail = null;
      pass = null;
      code = null;
      returnCode = null;
      initial = null;
      blank = null;
    }

    private Common adjustInUpdateCab;
    private Common adjustWithRefundOk;
    private Common adjustOk;
    private TextWorkArea currentDateText;
    private Common skipManDistRead;
    private Common multipayorOk;
    private Common female;
    private Common male;
    private CashReceiptStatus hardcodedReceipted;
    private Common retmcol;
    private CashReceiptLiterals initializeCashReceiptLiterals;
    private Common totalFees;
    private CashReceiptStatus hardcodedDeposited;
    private CashReceiptStatus hardcodedInterface;
    private CashReceiptDetailStatus hardcodedAdjusted;
    private CashReceiptDetailStatus hardcodedDistributed;
    private CashReceiptDetailStatus hardcodedRefunded;
    private CashReceiptDetailStatus hardcodedReleased;
    private CashReceiptDetailStatus hardcodedRecorded;
    private CashReceiptDetailStatus hardcodedSuspended;
    private CashReceiptDetailStatus hardcodedPended;
    private CsePerson phone;
    private Common toBeUnpended;
    private DateWorkArea max;
    private Common totalAdjusted;
    private Common totalRefunded;
    private CsePersonAddress csePersonAddress;
    private Common isAddressFound;
    private Common isCrDetailPopulated;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail cashReceiptDetail;
    private Common canBeReleased;
    private DateWorkArea current;
    private CsePersonsWorkSet workSex;
    private Common workNoOfObligors;
    private Array<ObligorListGroup> obligorList;
    private AbendData abendData;
    private Common manualDistribInd;
    private CashReceiptDetail beforeUpdate;
    private Common noOfCrDetails;
    private Common collectionDifference;
    private Common collAmtApplied;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CashReceiptDetailAddress initializeCashReceiptDetailAddress;
    private CashReceiptDetailStatus temp;
    private LegalAction legalAction;
    private Case1 case1;
    private CsePerson csePerson;
    private Common feesRequired;
    private CollectionType passAreaCollectionType;
    private CashReceiptDetail passAreaCashReceiptDetail;
    private CodeValue pass;
    private Code code;
    private Common returnCode;
    private DateWorkArea initial;
    private CashReceiptDetailAddress blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of AdjustCashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("adjustCashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj AdjustCashReceiptDetailBalanceAdj
    {
      get => adjustCashReceiptDetailBalanceAdj ??= new();
      set => adjustCashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of AdjustCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustCashReceipt")]
    public CashReceipt AdjustCashReceipt
    {
      get => adjustCashReceipt ??= new();
      set => adjustCashReceipt = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public CashReceiptDetail Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    /// <summary>
    /// A value of AdjustCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustCashReceiptDetail")]
    public CashReceiptDetail AdjustCashReceiptDetail
    {
      get => adjustCashReceiptDetail ??= new();
      set => adjustCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptType.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptType")]
    public CashReceiptType InterfaceCashReceiptType
    {
      get => interfaceCashReceiptType ??= new();
      set => interfaceCashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptSourceType")]
    public CashReceiptSourceType InterfaceCashReceiptSourceType
    {
      get => interfaceCashReceiptSourceType ??= new();
      set => interfaceCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ForPersistentView.
    /// </summary>
    [JsonPropertyName("forPersistentView")]
    public CashReceiptDetail ForPersistentView
    {
      get => forPersistentView ??= new();
      set => forPersistentView = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of MatchForPersistent.
    /// </summary>
    [JsonPropertyName("matchForPersistent")]
    public CashReceiptDetailStatus MatchForPersistent
    {
      get => matchForPersistent ??= new();
      set => matchForPersistent = value;
    }

    /// <summary>
    /// A value of MatchedForPersistent.
    /// </summary>
    [JsonPropertyName("matchedForPersistent")]
    public CashReceipt MatchedForPersistent
    {
      get => matchedForPersistent ??= new();
      set => matchedForPersistent = value;
    }

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public CashReceiptType Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    private ProfileAuthorization profileAuthorization;
    private Profile profile;
    private ServiceProviderProfile serviceProviderProfile;
    private ServiceProvider serviceProvider;
    private DisbursementTransaction disbursementTransaction;
    private Collection collection;
    private CashReceiptDetailBalanceAdj adjustCashReceiptDetailBalanceAdj;
    private CashReceipt adjustCashReceipt;
    private CashReceiptDetail adjusted;
    private CashReceiptDetail adjustCashReceiptDetail;
    private CashReceiptType interfaceCashReceiptType;
    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CsePersonAccount csePersonAccount;
    private CashReceiptSourceType interfaceCashReceiptSourceType;
    private CashReceiptDetail forPersistentView;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private CashReceiptDetailStatus matchForPersistent;
    private CashReceipt matchedForPersistent;
    private CashReceiptType verify;
  }
#endregion
}

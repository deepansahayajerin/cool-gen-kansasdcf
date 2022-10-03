// Program: FN_WAST_MTN_WARRANT_REQUEST, ID: 371866270, model: 746.
// Short name: SWEWASTP
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
/// A program: FN_WAST_MTN_WARRANT_REQUEST.
/// </para>
/// <para>
/// This screen will be used to perform a number of functions relating to 
/// warrants. It will allow the user to update the &quot;Mailed to Address&quot;
/// ( in the case of incorrectly entering a remail address). It will allow the
/// user to change the status of a warrant, enter a remail address for the
/// warrant.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnWastMtnWarrantRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_WAST_MTN_WARRANT_REQUEST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnWastMtnWarrantRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnWastMtnWarrantRequest.
  /// </summary>
  public FnWastMtnWarrantRequest(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // Procedure : Maintain Warrant Request
    // Screen Id.: WAST
    // Developed by : R.B.Mohapatra, MTW
    // Date started : 10-12-1995
    // Change Log:
    //  1. 02-26-96  Addition of export views export_pass_thru_flow for 
    // establishing Dialog flows
    //  2. 03-14-96  Retrofitting Security & NEXTTRAN
    //  3. 04-16-96  New Requirement - Adjustment of Cash_Receipt_Detail 
    // Refund_Amount for warrants with status OUTLAW,
    //               CANDUM or CAN
    // 12/18/96      R. Marchman     Add new security/next tran
    // 11/12/97      R. Marchman     Applied Current Date fix.  Fixed Remail 
    // Date to only allow current date or greater as input and will default to
    // current date.
    // 11/20/97      R. Marchman     Fixed address creation problem.
    // 11/21/97      RBM  Fixed the following problem reports :
    //       #31637, 31638, 32100, 31760, 32289
    // 12/29/97	Venkatesh Kamaraj Changed logic  to set situation number to 0 
    // because of changes to infrastructure
    // 12/5/98     Kalpesh Doshi  Various phase 2 changes as requested by SMEs
    // 04/08/1999	M Ramirez	Added creation of document trigger for POSTMAST.
    // 070799  SWSRKXD Infrastructure changes.
    // 080399  SWSRKXD PR 326 _ remove redundant call to CAB set_maximum_date 
    // and add original warrant# in reason_text.
    // 090899 SWSRKXD PR # H73180 - Remove call to 
    // fn_read_program_for_supp_person.
    // ===============================================
    // -------------------------------------------------------------------------------
    // SWSRKXD - 11/4/99, PR # H73180.
    // When a warrant is being 'REMaiLed' and no active mailing 
    // cse_person_address exists for Payee, create one.  When found, old mailing
    // cse_person_address record is ended.
    // SWSRKXD - 11/4/99 PR#79114
    // Misc refunds do not have a cash_receipt_detail. Don't set error when '
    // CANcelling' a warrant and CRD is nf!
    // M Ramirez - 11/12/1999 xxxxx
    // Changed POSTMAST from printing batch to printing online
    // 01/19/2000      N.Engoor       Added a flag on the screen to decide if to
    // create an alert on going to Return status. The flag will default to N.
    // 01/24/2000      N.Engoor       As per meeting on 01/24/2000 decided to 
    // add a new payment status code - CANRESET. This code is to be used to
    // reset the refunded amount.
    // ----------------------------------------------------------------------------------
    // SWSRKXD - 2/21/2000 PR#86019, #86329
    // Set worker_id when ending cse_person_address.
    // 3/4/2000
    // Only send alerts for cases with 'AR' and 'AP' case roles.
    // 04/06/00   C. Scroggins     Added security call for family violence.
    // 09/01/00 K. Doshi - Update Interstate_payment_address
    // for interstate warrants.
    // 11/09/2000 K. Doshi - PR# 106752
    // Street1 is now mandatory.  Street2 is optional.
    // Fangman  12/19/00 - WR 000234
    // Added interstate indicator to the screen.
    // K. Doshi 02/12/2001 - WR# 263
    // Replace DOA payment status with KPC.
    // K. Doshi 04/11/2001 - PR# 113671
    // Change verbiage for alerts.
    // K. Doshi 05/15/2001 - PR# 114679
    // Add validation for zip code.
    // K. Doshi 06/26/2001 -  PR# 111410.
    // Add family violence check for DP.
    // K. Doshi 06/26/2001 - WR# 285
    // Cater for Duplicate warrant #s.
    // 11/28/2001  K. Doshi - WR# 020147
    // KPC Recoupment changes.
    // L. Bachura 1-16-02. PR119723 - commented out check for CANDUM and OUTLAW 
    // status  at about line 1013 of the code. Note follows at that point.
    // 10/22/2002  K. Doshi - PR#156757
    // Allow status changes even if FVI is set. Protect Address though.
    // 12/30/03 GVandy WR040134  Allow KPC recoupment indicator to be entered if
    // spaces and protected otherwise.
    // 02/09/10 GVandy CQ15268  Default Return Alert indicator to spaces instead
    // of "N".
    // 09/09/2014  GVandy CQ45743  For CANRESET status check if the warrant is a
    // re-issued
    // refund warrant.  If so, adjust the cash receipt detail refunded amount 
    // and fully
    // applied indicator.
    // 03/08/2019  GVandy CQ65422  Correct logic so that warrants without 
    // warrant numbers
    // (e.g. in REQ or KPC status) will display.
    // -----------------------------------------------------------------------
    var field = GetField(export.PaymentRequest, "recoupmentIndKpc");

    field.Highlighting = Highlighting.Underscore;
    field.Protected = false;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // mjr--->  Added		11/12/1999
    UseSpDocSetLiterals();

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenPaymentRequest.Assign(import.HiddenPaymentRequest);

    // **** end   group A ****
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "UNDO"))
    {
      if (AsChar(import.DisplayIndicator.Flag) == 'Y')
      {
        // **Information already Displayed...Continue Processing
      }
      else
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFOR_UPD_DEL";

        return;
      }
    }

    // ==>MOVE THE IMPORTS TO EXPORTS
    if (Equal(global.Command, "DISPLAY"))
    {
      export.DisplayIndicator.Flag = "N";
    }
    else
    {
      export.CreateReturnAlertFlag.SelectChar =
        import.CreateReturnAlertFlag.SelectChar;
      export.NameInMailedAddr.Text32 = import.NameInMailedAddr.Text32;
      export.DisplayIndicator.Flag = import.DisplayIndicator.Flag;
      export.PaymentRequest.Assign(import.PaymentRequest);
      export.PaymentStatus.Assign(import.PaymentStatus);
      MoveCsePersonsWorkSet(import.PayeeName, export.Payee);
      MoveCsePersonsWorkSet(import.DesigPayee, export.DesigPayee);
      export.Mailed.Assign(import.Mailed);
      MovePaymentStatus(import.New1, export.New1);
      export.OldAndNew.ReasonText = import.OldAndNew.ReasonText;
      export.WarrantRemailAddress.Assign(import.WarrantRemailAddress);
      MovePaymentRequest4(import.NewReis, export.NewReis);
      export.HiddenReadFromTable.SystemGeneratedIdentifier =
        import.HiddenReadFromTable.SystemGeneratedIdentifier;

      // -----------------------------------------------
      // PR156757 - Allow status change even if FVI is set.
      // -----------------------------------------------
      export.HiddenProtectAddrField.Flag = import.HiddenProtectAddrField.Flag;
      export.HiddenProtectKpcRecoup.Flag = import.HiddenProtectKpcRecoup.Flag;
      local.CurrentDate.Date = Now().Date;

      // ---------------------
      // Added a Set stmnt for cash receipt detail suspended status.
      // ---------------------
      local.HardcodeSuspend.SystemGeneratedIdentifier = 3;
      export.PassThruFlowCsePerson.Number =
        import.PaymentRequest.CsePersonNumber ?? Spaces(10);
      MovePaymentRequest2(import.PaymentRequest,
        export.PassThruFlowPaymentRequest);
      export.ReisTo.Assign(import.ReisTo);
    }

    // -- 12/30/03 GVandy WR040134  Protect KPC recoupment indicator once set.
    if (AsChar(export.HiddenProtectKpcRecoup.Flag) == 'Y')
    {
      var field1 = GetField(export.PaymentRequest, "recoupmentIndKpc");

      field1.Color = "cyan";
      field1.Protected = true;
    }

    // *** If RETURNed from a LIST screen, Escape to simulate DISPLAY FIRST ***
    if (Equal(global.Command, "RETCDVL"))
    {
      var field1 = GetField(export.New1, "code");

      field1.Color = "green";
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;
      field1.Focused = true;

      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.HiddenNextTranInfo.CsePersonNumberObligee =
        import.PaymentRequest.CsePersonNumber ?? "";
      UseScCabNextTranPut1();

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
      // -----------
      // This is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // -----------
      // -----------
      // Since the nexttran_info does not carry the Warrant_number, no export 
      // view can be set from the nexttran_info.
      // -----------
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "WARA") || Equal
      (global.Command, "WARR") || Equal(global.Command, "WDTL") || Equal
      (global.Command, "WHST") || Equal(global.Command, "PRINTRET"))
    {
    }
    else if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------------
      // K. Doshi 06/26/2001 -  PR# 111410.
      // Don't pass Payee # during display action.
      // ----------------------------------------------------
      UseScCabTestSecurity1();
    }
    else
    {
      UseScCabTestSecurity2();
    }

    // -----------------------------------------------
    // PR156757 - Allow status change even if FVI is set.
    // -----------------------------------------------
    if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      export.HiddenProtectAddrField.Flag = "Y";
    }
    else if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    if (!IsEmpty(import.PaymentRequest.Number))
    {
      local.TextWorkArea.Text10 = import.PaymentRequest.Number ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.PaymentRequest.Number = Substring(local.TextWorkArea.Text10, 2, 9);
    }

    if (!IsEmpty(export.NewReis.Number))
    {
      local.TextWorkArea.Text10 = export.NewReis.Number ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.NewReis.Number = Substring(local.TextWorkArea.Text10, 2, 9);
    }

    // *****
    // Hardcode Payee type.
    // *****
    local.HardcodePayee.Type1 = "E";
    local.HardcodeObligor.Type1 = "R";
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate2();

    if (Equal(global.Command, "PRINTRET"))
    {
      // mjr
      // -----------------------------------------------
      // 11/12/1999
      // After the document is Printed (the user may still be looking
      // at WordPerfect), control is returned here.  Any cleanup
      // processing which is necessary after a print, should be done
      // now.
      // ------------------------------------------------------------
      UseScCabNextTranGet();

      // mjr
      // ----------------------------------------------------
      // Extract identifiers from next tran
      // -------------------------------------------------------
      local.Position.Count =
        Find(export.HiddenNextTranInfo.MiscText2, "PMTREQ:");

      if (local.Position.Count > 0)
      {
        export.PaymentRequest.Number =
          Substring(export.HiddenNextTranInfo.MiscText2, local.Position.Count +
          7, 9);
      }

      global.Command = "DISPLAY";
    }

    // ***** Main CASE OF COMMAND structure *****
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.CreateReturnAlertFlag.SelectChar = "";

        if (!IsEmpty(import.WarantNoPrompt.SelectChar))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }

        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@ New code below
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // 03/08/2019  GVandy CQ65422  Correct logic so that warrants without 
        // warrant numbers
        // (e.g. in REQ or KPC status) will display.
        if (!Equal(import.PaymentRequest.Number,
          import.HiddenPaymentRequest.Number) && import
          .PaymentRequest.SystemGeneratedIdentifier == import
          .HiddenPaymentRequest.SystemGeneratedIdentifier)
        {
          export.PaymentRequest.SystemGeneratedIdentifier = 0;
        }
        else
        {
          export.PaymentRequest.SystemGeneratedIdentifier =
            import.PaymentRequest.SystemGeneratedIdentifier;
        }

        if (IsEmpty(export.PaymentRequest.Number) && export
          .PaymentRequest.SystemGeneratedIdentifier == 0)
        {
          var field3 = GetField(export.PaymentRequest, "number");

          field3.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        if (export.PaymentRequest.SystemGeneratedIdentifier != 0)
        {
          if (!ReadPaymentRequest2())
          {
            ExitState = "FN0000_PASSED_PAYMENT_REQ_NF";

            return;
          }
        }
        else
        {
          // ---------------------------------------------
          // 06/26/01 - K. Doshi - WR# 285
          // Cater for Duplicate warrant #s.
          // ---------------------------------------------
          ReadPaymentRequest4();

          if (local.NbrOfWarrants.Count > 1)
          {
            // ------------------------------------------------------------
            // 06/26/01 - K. Doshi - WR# 285.
            // Duplicate exists. Only send warrant # and duplicate_warrant
            // flag via dialog flow.
            // ------------------------------------------------------------
            export.Payee.Number = "";
            export.Payee.FormattedName = "";
            MovePaymentRequest2(export.PaymentRequest,
              export.PassThruFlowPaymentRequest);
            export.HiddenPaymentRequest.Number = "";
            export.DuplicateWarrants.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_WARRANTS";

            return;
          }

          if (!ReadPaymentRequest1())
          {
            var field3 = GetField(export.PaymentRequest, "number");

            field3.Error = true;

            ExitState = "FN0000_WARRANT_NF";

            return;
          }
        }

        // 03/08/2019  GVandy CQ65422  End of changes for this ticket.
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@ New code above
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        export.PaymentRequest.Assign(entities.PaymentRequest);
        MovePaymentRequest3(entities.PaymentRequest, export.HiddenPaymentRequest);
          

        if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
        {
          export.PaymentRequest.InterstateInd = "N";
        }

        // -- 12/30/03 GVandy WR040134  Protect KPC recoupment indicator once 
        // set.
        if (!IsEmpty(entities.PaymentRequest.RecoupmentIndKpc))
        {
          export.HiddenProtectKpcRecoup.Flag = "Y";

          var field3 = GetField(export.PaymentRequest, "recoupmentIndKpc");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (ReadPaymentRequest9())
        {
          if (AsChar(entities.ReissuedFrom.InterstateInd) == 'Y')
          {
            // Set the interstate indicator to the original payment request 
            // interstate indicator
            // ONLY if the original payment request interstate indicator was Y.
            // Per 11/14/2000 business rule changes by J.E.
            export.PaymentRequest.InterstateInd = "Y";
          }
        }
        else
        {
          // Continue
        }

        if (AsChar(export.PaymentRequest.InterstateInd) != 'Y')
        {
          export.PaymentRequest.InterstateInd = "N";
        }

        export.DisplayIndicator.Flag = "Y";

        if (Equal(export.PaymentRequest.DesignatedPayeeCsePersonNo,
          export.PaymentRequest.CsePersonNumber) || IsEmpty
          (export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          export.PaymentRequest.DesignatedPayeeCsePersonNo = "";
          export.DesigPayee.FormattedName = "";
        }
        else
        {
          export.DesigPayee.Number =
            entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
          UseSiReadCsePerson1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FN0000_DESIG_PAYEE_NF_RB";

            return;
          }
        }

        export.Payee.Number = entities.PaymentRequest.CsePersonNumber ?? Spaces
          (10);
        UseSiReadCsePerson2();

        // ------------------------------------------------------
        // KD - 12/2/98
        // Set Remail Date to latest 'REML' status date
        // ------------------------------------------------------
        if (ReadPaymentStatusHistory3())
        {
          export.Mailed.RemailDate =
            entities.PaymentStatusHistory.EffectiveDate;
        }

        // ***** Now get the CURRENT Payment-Status code *****
        if (ReadPaymentStatusPaymentStatusHistory())
        {
          export.HiddenReadFromTable.SystemGeneratedIdentifier =
            entities.PaymentStatusHistory.SystemGeneratedIdentifier;
          export.OldAndNew.ReasonText =
            entities.PaymentStatusHistory.ReasonText;
          export.PaymentStatus.Assign(entities.PaymentStatus);
          export.PaymentStatus.LastUpdateBy =
            entities.PaymentStatusHistory.CreatedBy;
        }
        else
        {
          ExitState = "FN0000_PYMNT_STAT_HIST_NF_RB";

          return;
        }

        if (ReadPaymentRequest5())
        {
          if (IsEmpty(entities.ForReissue.Number))
          {
            export.ReisTo.Number = "KPC";
          }
          else
          {
            export.ReisTo.Number = entities.ForReissue.Number;
          }

          export.ReisTo.SystemGeneratedIdentifier =
            entities.ForReissue.SystemGeneratedIdentifier;
        }
        else
        {
          export.ReisTo.Number = "";
          export.ReisTo.SystemGeneratedIdentifier = 0;
        }

        // ------------------------------------------------------------------
        // 06/26/01 - K. Doshi - PR# 111410.
        // Add family violence check for DP.
        // ------------------------------------------------------------------
        if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          UseScSecurityCheckForFv1();

          // -----------------------------------------------
          // PR156757 - Allow status change even if FVI is set.
          // -----------------------------------------------
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.HiddenProtectAddrField.Flag = "N";
          }
          else
          {
            export.HiddenProtectAddrField.Flag = "Y";

            break;
          }
        }

        // ---------------------------------------------------------------------------------------
        // Add call to security to check for family violence. CLS. 04/06/00
        // ---------------------------------------------------------------------------------------
        UseScSecurityCheckForFv2();

        // -----------------------------------------------
        // PR156757 - Allow status change even if FVI is set.
        // -----------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenProtectAddrField.Flag = "N";
        }
        else
        {
          export.HiddenProtectAddrField.Flag = "Y";

          break;
        }

        if (ReadWarrantRemailAddress2())
        {
          export.NameInMailedAddr.Text32 =
            entities.WarrantRemailAddress.Name ?? Spaces(32);
          export.Mailed.AddressLine1 = entities.WarrantRemailAddress.Street1;
          export.Mailed.AddressLine2 =
            entities.WarrantRemailAddress.Street2 ?? Spaces(30);
          export.Mailed.City = entities.WarrantRemailAddress.City;
          export.Mailed.State = entities.WarrantRemailAddress.State;
          export.Mailed.ZipCode =
            TrimEnd(entities.WarrantRemailAddress.ZipCode5) + " " + TrimEnd
            (entities.WarrantRemailAddress.ZipCode4) + " " + TrimEnd
            (entities.WarrantRemailAddress.ZipCode3);
        }

        // mjr
        // -----------------------------------------------
        // 11/12/1999
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText2,
          NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count > 0)
        {
          // mjr---> Determines the appropriate exitstate for the Print process
          local.Print.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces
            (50);
          UseSpPrintDecodeReturnCode();
          export.HiddenNextTranInfo.MiscText2 = local.Print.Text50;
        }

        if (AsChar(export.PaymentRequest.RecoupmentIndKpc) == 'Y')
        {
          ExitState = "FN0000_DISPLAY_SUCCES_KPC_RCPMNT";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "UPDATE":
        // *********************************************
        // Description is required.
        // *********************************************
        if (IsEmpty(import.OldAndNew.ReasonText))
        {
          var field3 = GetField(export.OldAndNew, "reasonText");

          field3.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        if (IsEmpty(import.New1.Code) && IsEmpty(import.NewReis.Number) && Equal
          (import.WarrantRemailAddress.RemailDate, local.Initialize.RemailDate))
        {
          local.UpdateOnlyNarrative.Flag = "Y";
        }
        else
        {
          local.UpdateOnlyNarrative.Flag = "N";
        }

        if (AsChar(local.UpdateOnlyNarrative.Flag) == 'N')
        {
          if (!IsEmpty(import.New1.Code) && !
            Equal(import.New1.Code, import.PaymentStatus.Code))
          {
          }
          else
          {
            // *** The New Payment_Status code should not be Blank or same as 
            // the Old Payment_Status code
            var field3 = GetField(export.New1, "code");

            field3.Error = true;

            ExitState = "FN0000_NEW_PMNT_STAT_CODE_INVALD";

            return;
          }
        }

        if (Equal(import.New1.Code, "REIS"))
        {
          if (IsEmpty(import.NewReis.Number))
          {
            var field3 = GetField(export.NewReis, "number");

            field3.Error = true;

            ExitState = "REISSUE_WAR_NO_MISSING";

            return;
          }
        }

        if (Equal(import.New1.Code, "CANRESET"))
        {
          if (!Equal(import.PaymentRequest.Classification, "REF"))
          {
            var field3 = GetField(export.New1, "code");

            field3.Error = true;

            ExitState = "FN0000_CANRESET_IS_FOR_REF_ONLY";

            return;
          }
        }

        // -------------------------------------------------------------------
        // KD-1/12/98
        // Remail Address may only be entered when updating status to REIS or 
        // REML.
        // -------------------------------------------------------------------
        // -----------------------------------------------
        // PR156757 - Allow status change even if FVI is set.
        // -----------------------------------------------
        if (!Equal(import.New1.Code, "REIS") && !
          Equal(import.New1.Code, "REML") || AsChar
          (export.HiddenProtectAddrField.Flag) == 'Y')
        {
          if (!IsEmpty(import.WarrantRemailAddress.Name))
          {
            var field3 = GetField(export.WarrantRemailAddress, "name");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsEmpty(import.WarrantRemailAddress.Street1))
          {
            var field3 = GetField(export.WarrantRemailAddress, "street1");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsEmpty(import.WarrantRemailAddress.Street2))
          {
            var field3 = GetField(export.WarrantRemailAddress, "street2");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsEmpty(import.WarrantRemailAddress.City))
          {
            var field3 = GetField(export.WarrantRemailAddress, "city");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsEmpty(import.WarrantRemailAddress.State))
          {
            var field3 = GetField(export.WarrantRemailAddress, "state");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsEmpty(import.WarrantRemailAddress.ZipCode3))
          {
            var field3 = GetField(export.WarrantRemailAddress, "zipCode3");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsEmpty(import.WarrantRemailAddress.ZipCode4))
          {
            var field3 = GetField(export.WarrantRemailAddress, "zipCode4");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsEmpty(import.WarrantRemailAddress.ZipCode5))
          {
            var field3 = GetField(export.WarrantRemailAddress, "zipCode5");

            field3.Error = true;

            ExitState = "FN0000_ADDR_FIELDS_NOT_ENTERABLE";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // --------------------------------------------
        // K. Doshi 06/26/2001 - WR# 285
        // Cater for Duplicate warrant #s.
        // --------------------------------------------
        if (!ReadPaymentRequest3())
        {
          var field3 = GetField(export.PaymentRequest, "number");

          field3.Error = true;

          ExitState = "FN0000_PYMT_REQUEST_NF";

          return;
        }

        // -- 12/30/03 GVandy WR040134  Allow KPC recoupment indicator to be 
        // entered once.
        if (AsChar(export.HiddenProtectKpcRecoup.Flag) != 'Y' && AsChar
          (entities.PaymentRequest.RecoupmentIndKpc) != AsChar
          (export.PaymentRequest.RecoupmentIndKpc))
        {
          // -- Validate the KPC recoupment indicator value.
          switch(AsChar(export.PaymentRequest.RecoupmentIndKpc))
          {
            case 'Y':
              break;
            case ' ':
              break;
            default:
              var field3 = GetField(export.PaymentRequest, "recoupmentIndKpc");

              field3.Error = true;

              ExitState = "INVALID_INDICATOR_Y_OR_SPACE";

              return;
          }

          // -- Store the KPC recoupment indicator value.
          try
          {
            UpdatePaymentRequest1();
            local.KpcRecoupIndUpdated.Flag = "Y";
            export.HiddenProtectKpcRecoup.Flag = "Y";

            var field3 = GetField(export.PaymentRequest, "recoupmentIndKpc");

            field3.Color = "cyan";
            field3.Protected = true;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PAYMENT_REQUEST_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PAYMENT_REQUEST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        // -- Validate the Create Return Alert indicator value.
        switch(AsChar(export.CreateReturnAlertFlag.SelectChar))
        {
          case 'Y':
            break;
          case 'N':
            break;
          case ' ':
            break;
          default:
            var field3 = GetField(export.CreateReturnAlertFlag, "selectChar");

            field3.Error = true;

            ExitState = "INVALID_INDICATOR_Y_N_SPACE";

            return;
        }

        if (ReadPaymentStatus3())
        {
          // *** Continue Processing
        }
        else
        {
          ExitState = "FN0000_PYMNT_STAT_NF";

          return;
        }

        if (AsChar(local.UpdateOnlyNarrative.Flag) == 'Y')
        {
          // ----------
          // RBM  11/21/1997  Read the Displayed Payment_Status thru the hidden 
          // Payment_Status_History Id
          // ----------
          if (ReadPaymentStatusHistory1())
          {
            // -------------------------------------------------------------------
            // KD-1/12/98
            // Ensure narrative has been updated
            // -------------------------------------------------------------------
            if (Equal(import.OldAndNew.ReasonText,
              entities.PaymentStatusHistory.ReasonText))
            {
              if (AsChar(local.KpcRecoupIndUpdated.Flag) == 'Y')
              {
                // -- User updated the KPC recoupment indicator.  It is OK that 
                // they didn't change the narrative.
                goto Read1;
              }

              UseEabRollbackCics();
              ExitState = "FN0000_UPDATE_UNSUCCESSFUL";

              return;
            }

            try
            {
              UpdatePaymentStatusHistory2();

              // *** Continue Processing
              export.OldAndNew.ReasonText = import.OldAndNew.ReasonText;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_PYMNT_STAT_HISTORY_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";

            return;
          }

Read1:

          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          export.CreateReturnAlertFlag.SelectChar = "";

          return;
        }

        if (ReadPaymentStatus5())
        {
          // *******************************************************
          // Check if Transition from Old Status to New Status is allowed.
          // *******************************************************
          UseCabCheckPmntStatStateChange1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(local.StatusChangeFlag.Flag) == 'Y')
            {
              // *** Continue Processing
            }
            else
            {
              var field3 = GetField(export.New1, "code");

              field3.Error = true;

              ExitState = "INVALID_PAYMENT_STATUS_CHANGE";

              return;
            }
          }
          else
          {
            var field3 = GetField(export.New1, "code");

            field3.Error = true;

            ExitState = "INVALID_STATUS_CODE";

            return;
          }

          // ----------
          // Get the old Pmnt_stat_Hist associated with the Old Status.
          // The system_generated_id for the payment_status_history whose status
          // is already displayed, is passed as a HIDDEN attribute thru IM/EX
          // read_from_table payment_status_history. Hence the old
          // Payment_status_hist record can be obtained by READING thru the
          // System_generated_Id instead of READING THRU Realationship.
          // -----------
          if (!ReadPaymentStatusHistory1())
          {
            ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";

            return;
          }

          try
          {
            UpdatePaymentStatusHistory1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // ***Check to see if any Narrative Text is Input.
          if (IsEmpty(import.OldAndNew.ReasonText))
          {
            // *** NO REASON_TEXT IS INPUT
            export.OldAndNew.ReasonText =
              Spaces(PaymentStatusHistory.ReasonText_MaxLength);
          }
          else
          {
            export.OldAndNew.ReasonText = import.OldAndNew.ReasonText ?? "";
          }

          try
          {
            CreatePaymentStatusHistory4();
            export.PaymentStatus.LastUpdateBy =
              entities.PaymentStatusHistory.CreatedBy;
            local.ReadFromTable.SystemGeneratedIdentifier =
              entities.PaymentStatusHistory.SystemGeneratedIdentifier;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PYMNT_STAT_HIST_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HIST_PV";

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
          var field3 = GetField(export.New1, "code");

          field3.Error = true;

          ExitState = "FN0000_PYMNT_STAT_NF";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        // -----------------------------------------------
        // NPE - 01/20/00
        // Added code to create an Alert if the new Payment Status Code is being
        // set to RET. If the Alert indicator flag is being set to 'Y' an Alert
        // is to be created else skip it.
        // -----------------------------------------------
        if (Equal(import.New1.Code, "RET"))
        {
          if (AsChar(export.CreateReturnAlertFlag.SelectChar) == 'Y')
          {
            local.Infrastructure.SituationNumber = 0;
            local.Infrastructure.EventId = 10;
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.UserId = "WAST";
            local.Infrastructure.ReasonCode = "RETWAR";
            local.Infrastructure.InitiatingStateCode = "KS";
            local.Infrastructure.BusinessObjectCd = "CPA";
            local.Infrastructure.ReferenceDate = local.CurrentDate.Date;

            // -----------------------------------------------
            // NPE - 01/20/00
            // If DP exists, use DP cse person # for infrastructure.
            // -----------------------------------------------
            if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo) && !
              Equal(export.PaymentRequest.DesignatedPayeeCsePersonNo,
              export.PaymentRequest.CsePersonNumber))
            {
              local.Infrastructure.CsePersonNumber =
                export.PaymentRequest.DesignatedPayeeCsePersonNo ?? "";
            }
            else
            {
              local.Infrastructure.CsePersonNumber =
                export.PaymentRequest.CsePersonNumber ?? "";
            }

            local.Infrastructure.Detail = "Returned WARR # ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + TrimEnd
              (export.PaymentRequest.Number);

            // ---------------------------------------------------
            // PR# 113671 SWSRKXD 4/11/2001
            // Change verbiage for alerts.
            // ---------------------------------------------------
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + " Please enter new verified address.";
              

            // ----------------------------------------------------
            // SWSRKXD 3/4/2000
            // Only send alerts for cases with 'AR' and 'AP' case roles.
            // ---------------------------------------------------
            foreach(var item in ReadCase4())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              UseSpCabCreateInfrastructure();
            }
          }
        }

        // *** Update of Status/Remail Address if new status is REIS
        if (Equal(import.New1.Code, "REIS") || Equal(import.New1.Code, "REML"))
        {
          if (Equal(import.New1.Code, "REIS"))
          {
            if (ReadPaymentRequest7())
            {
              var field3 = GetField(export.NewReis, "number");

              field3.Error = true;

              ExitState = "FN0000_REISSUED_WARRANT_AE";
              UseEabRollbackCics();

              return;
            }
            else
            {
              // - OK, because this is the first time this warrant is created.
            }

            // ---------------------------------------------------------------
            // Obtain currency on Payment_Status for KPC, REQ and PAID
            // --------------------------------------------------------------
            if (!ReadPaymentStatus4())
            {
              ExitState = "FN0000_PYMNT_STAT_NF";

              return;
            }

            if (!ReadPaymentStatus6())
            {
              ExitState = "FN0000_PYMNT_STAT_NF";

              return;
            }

            if (!ReadPaymentStatus7())
            {
              ExitState = "FN0000_PYMNT_STAT_NF";

              return;
            }

            // -------------------------------------------------------
            // KD - 12/13/98
            // Set the print date to CURRENT instead of old warrant print date.
            // -------------------------------------------------------
            try
            {
              CreatePaymentRequest();

              // *** Creates a Payment_Status_Hist for REQ
              try
              {
                CreatePaymentStatusHistory2();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              // *** Creates a Payment_Status_Hist for KPC
              try
              {
                CreatePaymentStatusHistory1();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              // *** Creates a Payment_Status_Hist for PAID
              try
              {
                CreatePaymentStatusHistory3();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_PAYMENT_REQUEST_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // -----------------------------------------------
          // PR156757 - Allow status change even if FVI is set.
          // -----------------------------------------------
          if (AsChar(export.HiddenProtectAddrField.Flag) == 'Y')
          {
            goto Test2;
          }

          // ---------------------------------------------------------------------------------------------
          // ADDRESS VALIDATION. Last updated 05/16/01
          // -Remail address may only be entered when updating warrant status to
          // REIS or REML.
          // -Remail address is optional when updating status to REIS or REML.
          // -When left blank, use existing mailing address. If none exists, 
          // then error.
          // -When entered, all fields must be entered - Name, St1, City, State,
          // ZIP5.(i.e.All or nothing)
          // -St2 is optional
          // -ZIP5 must be numeric and 5 bytes in length.
          // -ZIP4 is optional.
          // -When entered, ZIP4 must be numeric and 4 bytes in length
          // ------------------------------------------------------------------------------------------
          local.AddrError.Flag = "N";

          if (!IsEmpty(import.WarrantRemailAddress.Street1) && !
            IsEmpty(import.WarrantRemailAddress.City) && !
            IsEmpty(import.WarrantRemailAddress.State) && !
            IsEmpty(import.WarrantRemailAddress.ZipCode5) && !
            IsEmpty(import.WarrantRemailAddress.Name))
          {
            // <<< RBM   All address fields have been entered >>>
            if (Length(TrimEnd(import.WarrantRemailAddress.ZipCode5)) < 5)
            {
              var field3 = GetField(export.WarrantRemailAddress, "zipCode5");

              field3.Error = true;

              UseEabRollbackCics();
              ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";

              return;
            }

            if (Verify(import.WarrantRemailAddress.ZipCode5, "0123456789") != 0)
            {
              var field3 = GetField(export.WarrantRemailAddress, "zipCode5");

              field3.Error = true;

              UseEabRollbackCics();
              ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

              return;
            }

            if (Length(TrimEnd(import.WarrantRemailAddress.ZipCode4)) > 0)
            {
              if (Length(TrimEnd(import.WarrantRemailAddress.ZipCode4)) < 4)
              {
                var field3 = GetField(export.WarrantRemailAddress, "zipCode4");

                field3.Error = true;

                UseEabRollbackCics();
                ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";

                return;
              }

              if (Verify(import.WarrantRemailAddress.ZipCode4, "0123456789") !=
                0)
              {
                var field3 = GetField(export.WarrantRemailAddress, "zipCode4");

                field3.Error = true;

                UseEabRollbackCics();
                ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";

                return;
              }
            }

            MoveWarrantRemailAddress(import.WarrantRemailAddress,
              local.WarrantRemailAddress);
          }
          else if (IsEmpty(import.WarrantRemailAddress.Street1) && IsEmpty
            (import.WarrantRemailAddress.Street2) && IsEmpty
            (import.WarrantRemailAddress.City) && IsEmpty
            (import.WarrantRemailAddress.State) && IsEmpty
            (import.WarrantRemailAddress.ZipCode5) && IsEmpty
            (import.WarrantRemailAddress.Name))
          {
            // <<< RBM ----  ALL  Remail Address fields are BLANK >>>
            // <<< check if the mailed to name and address is non-blank >>>
            if (IsEmpty(export.Mailed.AddressLine1) && IsEmpty
              (export.Mailed.AddressLine2) && IsEmpty(export.Mailed.City) && IsEmpty
              (export.Mailed.State) && IsEmpty(export.Mailed.ZipCode) && IsEmpty
              (export.NameInMailedAddr.Text32))
            {
              UseEabRollbackCics();
              ExitState = "FN0000_MUST_ENTER_THE_ADDRESS";

              return;
            }

            // *** Set local_warrant_reml_addr to Mailed_to_addr
            local.Found.Flag = "N";
            local.WarrantRemailAddress.Name = import.NameInMailedAddr.Text32;
            local.WarrantRemailAddress.Street1 = import.Mailed.AddressLine1;
            local.WarrantRemailAddress.Street2 = import.Mailed.AddressLine2;
            local.WarrantRemailAddress.City = import.Mailed.City;
            local.WarrantRemailAddress.State = import.Mailed.State;
            local.WarrantRemailAddress.ZipCode5 =
              Substring(import.Mailed.ZipCode, 1, 5);
            local.WarrantRemailAddress.ZipCode4 =
              Substring(import.Mailed.ZipCode, 6, 4);
            local.WarrantRemailAddress.ZipCode4 =
              Substring(import.Mailed.ZipCode, 10, 3);
            local.WarrantRemailAddress.CreatedBy = global.UserId;
            local.WarrantRemailAddress.CreatedTimestamp = Now();
          }
          else
          {
            // <<< RBM    Some of the Remail Address fields are Blank. Error 
            // them out >>>
            if (IsEmpty(import.WarrantRemailAddress.Name))
            {
              var field3 = GetField(export.WarrantRemailAddress, "name");

              field3.Error = true;

              local.AddrError.Flag = "Y";
            }

            // ------------------------------------------------------------
            // PR# 106752 - SWSRKXD 11/09/2000
            // Street1 is now mandatory.  Street2 is optional.
            // ------------------------------------------------------------
            if (IsEmpty(import.WarrantRemailAddress.Street1))
            {
              var field3 = GetField(export.WarrantRemailAddress, "street1");

              field3.Error = true;

              local.AddrError.Flag = "Y";
            }

            if (IsEmpty(import.WarrantRemailAddress.City))
            {
              var field3 = GetField(export.WarrantRemailAddress, "city");

              field3.Error = true;

              local.AddrError.Flag = "Y";
            }

            if (IsEmpty(import.WarrantRemailAddress.State))
            {
              var field3 = GetField(export.WarrantRemailAddress, "state");

              field3.Error = true;

              local.AddrError.Flag = "Y";
            }

            if (IsEmpty(import.WarrantRemailAddress.ZipCode5))
            {
              var field3 = GetField(export.WarrantRemailAddress, "zipCode5");

              field3.Error = true;

              local.AddrError.Flag = "Y";
            }
          }

          if (AsChar(local.AddrError.Flag) == 'Y')
          {
            ExitState = "FN0000_REML_ADDR_INFO_INCMPLT_RB";

            return;
          }

          export.WarrantRemailAddress.Assign(local.WarrantRemailAddress);

          // *******************************************
          // RCG - Review for insertion of financial Event
          // Update Warrant Address
          // *******************************************
          if (Equal(import.New1.Code, "REIS"))
          {
            // *** Pass Reissued_to_Payment_Request and local_warrant_reml_addr
            UseCabCreateWarRemailAddress2();
          }
          else
          {
            // *** Pass Payment_request and local_warrant_reml_addr
            UseCabCreateWarRemailAddress1();
          }

          // *** Move the Input Remail_Addr to Mailed_to_Address
          // -------------------------------------------------------
          // KD - 1/11/99
          // Only refresh address if not REIS.
          // ------------------------------------------------------
          // -----------------------------------------------
          // PR156757 - Allow status change even if FVI is set.
          // -----------------------------------------------
          if (IsExitState("ACO_NN0000_ALL_OK") && !
            Equal(import.New1.Code, "REIS") && AsChar
            (export.HiddenProtectAddrField.Flag) != 'Y')
          {
            export.NameInMailedAddr.Text32 =
              local.WarrantRemailAddress.Name ?? Spaces(32);
            export.Mailed.AddressLine1 = local.WarrantRemailAddress.Street1;
            export.Mailed.AddressLine2 = local.WarrantRemailAddress.Street2 ?? Spaces
              (30);
            export.Mailed.City = local.WarrantRemailAddress.City;
            export.Mailed.State = local.WarrantRemailAddress.State;
            export.Mailed.ZipCode =
              TrimEnd(local.WarrantRemailAddress.ZipCode5) + " " + TrimEnd
              (local.WarrantRemailAddress.ZipCode4) + " " + TrimEnd
              (local.WarrantRemailAddress.ZipCode3);
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FN0000_WAR_REML_ADDR_NC_RB";

            return;
          }

          // ----------------------------------------------------------
          // SWSRKXD - 7/6/99
          // Send an alert to the case coordinator indicating that CRU
          // has been provided with a new address when entering a Remail 
          // address.
          // -----------------------------------------------------------
          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.EventId = 10;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = "WAST";
          local.Infrastructure.ReasonCode = "RMLWAR";
          local.Infrastructure.InitiatingStateCode = "KS";
          local.Infrastructure.BusinessObjectCd = "CPA";
          local.Infrastructure.ReferenceDate = local.CurrentDate.Date;

          // -----------------------------------------------
          // KD - 10/20/99
          // If DP exists, use DP cse person # for infrastructure.
          // -----------------------------------------------
          if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo) && !
            Equal(export.PaymentRequest.DesignatedPayeeCsePersonNo,
            export.PaymentRequest.CsePersonNumber))
          {
            local.Infrastructure.CsePersonNumber =
              export.PaymentRequest.DesignatedPayeeCsePersonNo ?? "";
          }
          else
          {
            local.Infrastructure.CsePersonNumber =
              export.PaymentRequest.CsePersonNumber ?? "";
          }

          local.Infrastructure.Detail = "Warrant #";
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (export.PaymentRequest.Number);
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (" payable to");

          // -----------------------------------------------
          // KD - 10/20/99
          // If a DP exists, use DP cse person Name
          // -----------------------------------------------
          if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo) && !
            Equal(export.PaymentRequest.DesignatedPayeeCsePersonNo,
            export.PaymentRequest.CsePersonNumber))
          {
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + export
              .DesigPayee.FormattedName;
          }
          else
          {
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + export
              .Payee.FormattedName;
          }

          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ","
            ;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (local.Infrastructure.CsePersonNumber);
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " has been remailed.";
            
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (global.UserId);

          // -----------------------------------------------
          // KD - 7/7/99
          // Raise RMLWAR event for all CASEs.
          // KD - 10/20/99
          // When DP is setup, read case role for DP else for Payee.
          // -----------------------------------------------
          // ----------------------------------------------------
          // SWSRKXD 3/4/2000
          // Only send alerts for cases with 'AR' and 'AP' case roles.
          // ---------------------------------------------------
          foreach(var item in ReadCase3())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;
            UseSpCabCreateInfrastructure();
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          // *******************************************************
          // Add logic for IDCR - When adding a Warrant Remail Address, a CSE 
          // Person Address with the Address Source of REC, Start Date of
          // current date and End Date of current date, must also be added
          // *******************************************************
          // -------------------------------------------------------------------
          // SWSRKXD - 7/1/99
          // Add a CSE Person Address with source=CRU
          // --------------------------------------------------------------------
          // ---------------
          // If this Remail Address is already there in the Cse_Person Address, 
          // then we need not add it again. So Get the most recent 'Mailing'
          // address of the Payee. If it is not the same as the Current
          // Remail_Address, then record Current Remail Address. Otherwise,
          // escape this processing loop.
          // ---------------
          // *****************************************
          // Determine case_role - used later for add
          // cse_person_address and also to raise
          // infrastructure/postmaster letter
          // *****************************************
          export.PassThruFlowCsePerson.Number =
            local.Infrastructure.CsePersonNumber ?? Spaces(10);
          UseFnDetermineCaseRoleForOrec();

          // ********************
          // CAB sets no Exit State.
          // ********************
          if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
          {
            if (ReadCsePersonAddress())
            {
              MoveCsePersonAddress1(entities.CsePersonAddress,
                local.MostRecentPayee);
              local.ActiveMailingAddrFound.Flag = "Y";
            }

            // -----------------------------------------------------------------
            // SWSRKXD - 10/15/99; PR# 77617
            // Code was structured to NOT create cse_person_address
            // when active address was nf.  Re-arrange code to
            // create new cse_person_addr irrespective.  As before, old
            // address will be ended when found.
            // -----------------------------------------------------------------
            local.AddNewAddress.Flag = "Y";

            if (AsChar(local.ActiveMailingAddrFound.Flag) == 'Y')
            {
              if (AsChar(local.MostRecentPayee.Type1) == 'M' && Equal
                (local.MostRecentPayee.City, local.WarrantRemailAddress.City) &&
                Equal
                (local.MostRecentPayee.State, local.WarrantRemailAddress.State) &&
                Equal
                (local.MostRecentPayee.Street1,
                local.WarrantRemailAddress.Street1) && Equal
                (local.MostRecentPayee.Street2,
                local.WarrantRemailAddress.Street2) && Equal
                (local.MostRecentPayee.ZipCode,
                local.WarrantRemailAddress.ZipCode5))
              {
                // <<< Same Address is the currently active address.. No need to
                // set up again. >>>
                goto Test2;
              }

              // *****
              // SWSRKXD - 7/1/99
              // If only the zip code has changed, it is done to correct a
              // mistake. Update existing record. Don't create new one!
              // *****
              if (AsChar(local.MostRecentPayee.Type1) == 'M' && Equal
                (local.MostRecentPayee.City, local.WarrantRemailAddress.City) &&
                Equal
                (local.MostRecentPayee.State, local.WarrantRemailAddress.State) &&
                Equal
                (local.MostRecentPayee.Street1,
                local.WarrantRemailAddress.Street1) && Equal
                (local.MostRecentPayee.Street2,
                local.WarrantRemailAddress.Street2) && !
                Equal(local.MostRecentPayee.ZipCode,
                local.WarrantRemailAddress.ZipCode5))
              {
                // -------------------------------------------------------------------
                // SWSRKXD - 10/15/99; PR# 77617
                // Don't create new address!
                // --------------------------------------------------------------------
                local.AddNewAddress.Flag = "N";

                // -------------------------------------------------------------------
                // SWSRKXD - 10/20/99;
                // Add set statement for zip code.
                // --------------------------------------------------------------------
                local.MostRecentPayee.ZipCode =
                  local.WarrantRemailAddress.ZipCode5;

                // -------------------------------------------------------------------
                // SWSRKXD - 11/5/99; PR# 77617
                // Don't set end_code when updating zip_code. As per SMEs
                // this attribute should only bet set when ending the address
                // record.
                // --------------------------------------------------------------------
                local.MostRecentPayee.SendDate = local.CurrentDate.Date;
                local.MostRecentPayee.VerifiedDate = local.CurrentDate.Date;
                local.MostRecentPayee.LastUpdatedBy = global.UserId;
                local.MostRecentPayee.EndDate = local.Maximum.Date;
                UseSiUpdateCsePersonAddress();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "FN0000_CSE_PRSN_ADDRS_NOT_ADD_RB";

                  return;
                }

                // -------------------------------------------------------------------
                // SWSRKXD - 10/15/99; PR# 77617
                // Code was ending old address after updating Zip code and
                // creating new one! Add else statement below and move the
                // 'end / add' logic in Else loop.
                // -----------------------------------------------------------------
              }
              else
              {
                // *****
                // SWSRKXD - 7/1/99
                // End existing cse_person_address record.
                // *****
                local.MostRecentPayee.EndDate = local.CurrentDate.Date;
                local.MostRecentPayee.EndCode = "CU";

                // ----------------------------------------------------
                // SWSRKXD - 2/21/2000 PR#86019, #86329
                // Set worker_id when ending cse_person_address.
                // ---------------------------------------------------
                local.MostRecentPayee.WorkerId = global.UserId;
                UseSiUpdateCsePersonAddress();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "FN0000_CSE_PRSN_ADDRS_NOT_ADD_RB";

                  return;
                }
              }
            }

            if (AsChar(local.AddNewAddress.Flag) == 'Y')
            {
              // ***********************************************
              // SWSRKXD - 7/1/99
              // If Payee is AP, set Verified Date else set Send Date.
              // Look for active AP role first.
              // ************************************************
              if (Equal(local.CaseRole.Type1, "AP"))
              {
                local.CsePersonAddress.SendDate = local.CurrentDate.Date;
              }
              else
              {
                // ********************
                // Assume AR role.
                // ********************
                local.CsePersonAddress.VerifiedDate = local.CurrentDate.Date;
              }

              local.CsePersonAddress.EndDate =
                UseCabSetMaximumDiscontinueDate1();
              local.CsePersonAddress.Source = "CRU";
              local.CsePersonAddress.City = local.WarrantRemailAddress.City;
              local.CsePersonAddress.State = local.WarrantRemailAddress.State;
              local.CsePersonAddress.Street1 =
                local.WarrantRemailAddress.Street1;
              local.CsePersonAddress.Street2 =
                local.WarrantRemailAddress.Street2 ?? "";
              local.CsePersonAddress.ZipCode =
                local.WarrantRemailAddress.ZipCode5;
              local.CsePersonAddress.Zip4 =
                local.WarrantRemailAddress.ZipCode4 ?? "";
              local.CsePersonAddress.Zip3 =
                local.WarrantRemailAddress.ZipCode3 ?? "";
              local.CsePersonAddress.Type1 = "M";
              local.CsePersonAddress.LastUpdatedBy = global.UserId;
              local.CsePersonAddress.CreatedBy = global.UserId;
              local.CsePersonAddress.LocationType = "D";
              UseSiCreateCsePersonAddress();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "FN0000_WAR_REML_ADDR_NC_RB";

                return;
              }
            }
          }
          else
          {
            // ------------------------
            // Interstate warrant.
            // ------------------------
            if (!ReadInterstateRequest())
            {
              ExitState = "FN0000_INTERSTATE_REQUEST_NF_RB";

              return;
            }

            foreach(var item in ReadInterstatePaymentAddress())
            {
              if (Equal(entities.InterstatePaymentAddress.City,
                local.WarrantRemailAddress.City) && Equal
                (entities.InterstatePaymentAddress.State,
                local.WarrantRemailAddress.State) && Equal
                (entities.InterstatePaymentAddress.Street1,
                local.WarrantRemailAddress.Street1) && Equal
                (entities.InterstatePaymentAddress.Street2,
                local.WarrantRemailAddress.Street2) && Equal
                (entities.InterstatePaymentAddress.ZipCode,
                local.WarrantRemailAddress.ZipCode5))
              {
                // <<< Same Address is the currently active address.... >>>
                goto Test2;
              }

              try
              {
                UpdateInterstatePaymentAddress();

                goto Test1;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    UseEabRollbackCics();
                    ExitState = "INTERSTATE_PAYMENT_ADDRESS_NU";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    UseEabRollbackCics();
                    ExitState = "INTERSTATE_PAYMENT_ADDRESS_PV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            try
            {
              CreateInterstatePaymentAddress();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  UseEabRollbackCics();
                  ExitState = "INTERSTATE_PAYMENT_ADDRESS_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  UseEabRollbackCics();
                  ExitState = "INTERSTATE_PAYMENT_ADDRESS_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

Test1:

          export.HiddenReadFromTable.SystemGeneratedIdentifier =
            local.ReadFromTable.SystemGeneratedIdentifier;

          if (Equal(local.CaseRole.Type1, "AR"))
          {
            // *****
            // AR - raise infrastrucure event.
            // *****
            local.Infrastructure.SituationNumber = 0;
            local.Infrastructure.EventId = 10;
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.UserId = "WAST";
            local.Infrastructure.ReasonCode = "ARNEWADDRFN";
            local.Infrastructure.InitiatingStateCode = "KS";
            local.Infrastructure.BusinessObjectCd = "CPA";
            local.Infrastructure.ReferenceDate = local.CurrentDate.Date;

            // ------------------------------------------------------------------
            // SWSRKXD - 10/15/99; PR# 77617
            // local infrastrucure cse_person_number already set.
            // ------------------------------------------------------------------
            if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
            {
              local.Infrastructure.DenormTimestamp =
                local.CsePersonAddress.Identifier;
            }
            else
            {
              local.Infrastructure.DenormTimestamp =
                entities.InterstatePaymentAddress.LastUpdatedTimestamp;
            }

            local.Infrastructure.Detail =
              TrimEnd(local.WarrantRemailAddress.Street1) + ",";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + TrimEnd
              (local.WarrantRemailAddress.Street2);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + ",";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + TrimEnd
              (local.WarrantRemailAddress.City);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + ",";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + TrimEnd
              (local.WarrantRemailAddress.State);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + ",";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + TrimEnd
              (local.WarrantRemailAddress.ZipCode5);

            // -----------------------------------------------
            // KD - 10/20/99
            // When DP is setup, read case role for DP else for Payee.
            // -----------------------------------------------
            // ----------------------------------------------------
            // SWSRKXD 3/4/2000
            // Only send alerts for cases with 'AR' and 'AP' case roles.
            // ---------------------------------------------------
            foreach(var item in ReadCase2())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              UseSpCabCreateInfrastructure();
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }

            // -----------------------------------------------
            // KD - 10/20/99
            // When DP is setup, read case role for DP else for Payee.
            // -----------------------------------------------
            foreach(var item in ReadProgram())
            {
              if (Equal(local.Program.Code, "NA") || Equal
                (local.Program.Code, "NC") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FCI") || Equal
                (local.Program.Code, "MAI") || Equal
                (local.Program.Code, "NAI"))
              {
                continue;
              }

              // ***---  External alert to the worker involved with the 
              // assistance
              // ***--- Codes of AF, NF, FS, CC, MA, MS, CI, SI, MP, MK, or FC
              // ***---  Alert code 45 is for either domestic or foreign address
              // changes
              local.InterfaceAlert.AlertCode = "45";
              UseSpAddrExternalAlert();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "FN0000_ERROR_ON_EXTRNL_EVENT_CRE";

                return;
              }

              goto Test2;
            }
          }
          else
          {
            // *****
            // AP. Send postmaster letter
            // *****
            // -----------------------------------------------
            // KD - 10/20/99
            // When DP is setup, send postmaster letter for DP, else Payee.
            // -----------------------------------------------
            // mjr
            // ----------------------------------------------
            // 11/12/1999
            // Changed POSTMAST to print online (from batch)
            // -----------------------------------------------------------
            // mjr
            // -----------------------------------------------------
            // 11/12/1999
            // Verify that the AR is a client
            // 	In this situation this doesn't seem necessary
            // 	If it is necessary, uncomment the following
            // ------------------------------------------------------------------
            // mjr
            // -----------------------------------------------------
            // 11/12/1999
            // Only send a POSTMAST letter on one case (it must be open).
            // ------------------------------------------------------------------
            // ----------------------------------------------------
            // SWSRKXD 3/15/2000
            // Only send alerts for cases with 'AR' and 'AP' case roles.
            // ---------------------------------------------------
            if (ReadCase1())
            {
              // mjr
              // ----------------------------------------------------
              // Place identifiers into next tran
              // -------------------------------------------------------
              export.HiddenNextTranInfo.MiscText2 =
                TrimEnd(local.SpDocLiteral.IdDocument) + "POSTMAST";
              export.HiddenNextTranInfo.CaseNumber = entities.Case1.Number;
              export.HiddenNextTranInfo.MiscText1 =
                TrimEnd(local.SpDocLiteral.IdPrNumber) + (
                  local.Infrastructure.CsePersonNumber ?? "");
              local.BatchTimestampWorkArea.IefTimestamp =
                local.CsePersonAddress.Identifier;
              UseLeCabConvertTimestamp();
              local.Print.Text50 =
                TrimEnd(local.SpDocLiteral.IdPersonAddress) + local
                .BatchTimestampWorkArea.TextTimestamp;
              export.HiddenNextTranInfo.MiscText1 =
                TrimEnd(export.HiddenNextTranInfo.MiscText1) + ";" + local
                .Print.Text50;

              // mjr
              // --------------------------------------------------------------
              // Place Payment Request Number in next tran for Re-Display upon
              // return from Print process
              // -----------------------------------------------------------------
              local.Print.Text50 = "PMTREQ:" + (
                export.PaymentRequest.Number ?? "") + ";";
              export.HiddenNextTranInfo.MiscText2 =
                TrimEnd(export.HiddenNextTranInfo.MiscText2) + ";" + local
                .Print.Text50;
              export.Standard.NextTransaction = "DKEY";
              local.PrintProcess.Flag = "Y";
              local.PrintProcess.Command = "PRINT";
              UseScCabNextTranPut2();

              return;
            }
          }
        }

Test2:

        // ---------------------------------------------------------------
        // If the new status is CANRESET, CANDUM or OUTLAW and the warrant is a 
        // Refund Warrant, set the Cash_receipt_detail refund_amount to CRD
        // refund_amount MINUS the warrant amount.
        // N.Engoor  -  01/24/2000
        // A new payment status code has been added - CANRESET. Only if the 
        // payment status code is CANRESET, CANDUM or OUTLAW is the refunded
        // amount and the fully applied ind to be reset.
        // -----------------------------------------------------------------
        // PR119723 on 1-16-02 by L. Bachura. Delete the check for CANDUM and 
        // OUTLAW per revised business rules from SME's
        // Deleted the check for import_new payment_status code is = candum  or 
        // import_new payment_status code is = outlaw.
        if (Equal(import.New1.Code, "CANRESET"))
        {
          if (ReadReceiptRefund1())
          {
            // *** Continue Processing ......
          }
          else
          {
            // -- 09/09/2014  GVandy CQ45743  If the warrant was re-issued check
            // each previous
            // -- warrant to determine if it was for a refund.
            local.Temp.SystemGeneratedIdentifier =
              entities.PaymentRequest.SystemGeneratedIdentifier;

            do
            {
              if (ReadPaymentRequest8())
              {
                if (ReadReceiptRefund2())
                {
                  // *** Continue Processing ......
                  goto Read2;
                }
                else
                {
                  local.Temp.SystemGeneratedIdentifier =
                    entities.Reissued.SystemGeneratedIdentifier;
                }
              }
              else
              {
                // Continue
                local.Temp.SystemGeneratedIdentifier = 0;
              }
            }
            while(local.Temp.SystemGeneratedIdentifier != 0);

            // <<< This is not a Refund...So no adjustments necessary >>>
            goto Test3;
          }

Read2:

          if (ReadCashReceiptDetail())
          {
            // ----------------------------------
            // Read CRE, CRT and CRST for later
            // ----------------------------------
            if (!ReadCashReceiptEventCashReceiptTypeCashReceiptSourceType())
            {
              ExitState = "FN0000_CASH_RECEIPT_EVENT_NF";
              UseEabRollbackCics();

              return;
            }

            // ------------------------------------------------------
            // N.Engoor - 01/25/2000
            // Everytime a cash receipt detail is updated a cash receipt detail 
            // history needs to be created for audit trail. The identifier for
            // the cash receipt detail history is the Last_updated_timestamp
            // which is set to the cash receipt detail last_updated_timestsamp.
            // NB - Create CRDH before updating CRD so CRDH.last_updated_ts is 
            // set to pre-update CRD.ts
            // ------------------------------------------------------
            try
            {
              CreateCashReceiptDetailHistory();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_CRD_HISTORY_AE";
                  UseEabRollbackCics();

                  return;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            try
            {
              UpdateCashReceiptDetail();
              local.CrdAmtAdjusted.Flag = "Y";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0121_CASH_RCPT_DTL_NU";
                  UseEabRollbackCics();

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0056_CASH_RCPT_DTL_PV";
                  UseEabRollbackCics();

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // -------------
            // N.Engoor - 01/26/2000
            // End most recent cash receipt detail history and create a new one 
            // with a status of SUSP. A new SUSP status code 'PROCCANREF' has
            // been added in the code table.
            // ---------------
            if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
            {
              UpdateCashReceiptDetailStatHistory();
            }

            if (ReadCashReceiptDetailStatus())
            {
              try
              {
                CreateCashReceiptDetailStatHistory();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_CASH_RCPT_DTL_STAT_HST_AE";
                    UseEabRollbackCics();

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0050_CASH_RCPT_DTL_HIST_PV";
                    UseEabRollbackCics();

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
              ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            // -----------------------------------------------------------
            // KD - 11/4/99 PR#79114
            // Misc refunds will not have a cash_receipt_detail. Do NOT set 
            // error!
            // ----------------------------------------------------------
          }
        }

Test3:

        export.HiddenReadFromTable.SystemGeneratedIdentifier =
          local.ReadFromTable.SystemGeneratedIdentifier;
        export.WarrantRemailAddress.Assign(local.Initialize);
        MovePaymentStatus(export.New1, export.PaymentStatus);

        // -----------------------------------------------------------
        // KD - 1/8/99
        // Refresh screen fields after successful update
        // ----------------------------------------------------------
        if (Equal(import.New1.Code, "REIS") && IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.ReisTo.Number = export.NewReis.Number ?? "";
        }

        if (Equal(import.New1.Code, "REML") && IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------------------------------------------
          // KD - 12/2/98
          // Remail Date is set to the last 'REML' status date
          // ------------------------------------------------------
          if (ReadPaymentStatusHistory2())
          {
            export.Mailed.RemailDate =
              entities.PaymentStatusHistory.EffectiveDate;
          }
        }

        export.New1.Code = "";
        export.NewReis.Number = "";
        export.WarrantRemailAddress.RemailDate = null;
        export.CreateReturnAlertFlag.SelectChar = "";

        if (IsExitState("ACO_NN0000_ALL_OK") && AsChar
          (local.CrdAmtAdjusted.Flag) == 'Y' && (
            Equal(import.New1.Code, "CANRESET") || Equal
          (import.New1.Code, "CANDUM") || Equal(import.New1.Code, "OUTLAW")))
        {
          // -----------------------------------------------------------
          // KD - 2/16/2000
          // Display 'yellow' warning message instead of 'white' info message.
          // ----------------------------------------------------------
          ExitState = "FN0000_CRD_AMT_ADJUSTED_BY_REF";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "UNDO":
        // ---------------------------------------------------------
        // Obtain currency on payment_request & existing payment_status.
        // --------------------------------------------------------
        // --------------------------------------------
        // K. Doshi 06/26/2001 - WR# 285
        // Cater for Duplicate warrant #s.
        // --------------------------------------------
        if (!ReadPaymentRequest3())
        {
          ExitState = "FN0000_PYMT_REQUEST_NF";

          return;
        }

        if (!ReadPaymentStatus3())
        {
          ExitState = "ZD_FN0000_PYMNT_STAT_NF_2";

          return;
        }

        // -----------------------------------------------
        // Cannot Undo if current status is PAID or REML.
        // ------------------------------------------------
        if (Equal(entities.ExistingPaymentStatus.Code, "PAID"))
        {
          ExitState = "FN0000_CANNOT_DELETE_PAID_STATUS";

          return;
        }

        if (Equal(entities.ExistingPaymentStatus.Code, "REML"))
        {
          ExitState = "FN0000_CANNOT_DELETE_REML_STATUS";

          return;
        }

        // -----------------------------------------------
        // KD - 1/10/99
        // Cannot Undo if current status is CANRESET.
        // PF10 on CANRESET needs to rollback the CRD, CRDH and
        // CRDSH updates. This is a Future Enhancement. For the
        // timebeing, disable PF10 for CANRESET.
        // ------------------------------------------------
        if (Equal(entities.ExistingPaymentStatus.Code, "CANRESET"))
        {
          ExitState = "FN0000_CANNOT_DELETE_CANRESET";

          return;
        }

        // *** Continue Processing ....
        // Get the list of possible previous Statuses which may be changed to 
        // the current status
        UseCabCheckPmntStatStateChange2();

        // ** Get the Payment_Status_History for the current Payment_Request and
        // Payment_Status; the desired payment_status_hist record can be
        // obtained by reading thru the import_read_from_table payment_stat_hist
        // system_generated_id
        if (ReadPaymentStatusHistory1())
        {
          // ----------------------------------------------------------
          // KD - 12/14/98
          // Cannot undo if the current status eff date is in the past.
          // -----------------------------------------------------------
          if (Lt(entities.PaymentStatusHistory.EffectiveDate, Now().Date))
          {
            ExitState = "FN0000_CANNOT_DELETE_PAST_DATE";

            return;
          }

          // ** Get the NEXT MOST RECENT history- record
          local.PaymentStatFound.Flag = "N";

          foreach(var item in ReadPaymentStatusHistory4())
          {
            // *** Get the Payment_Status associated with the already
            //     read Next recent Payment_Status_History
            if (ReadPaymentStatus1())
            {
              local.PaymentStatFound.Flag = "N";

              for(local.PossiblePrevStat.Index = 0; local
                .PossiblePrevStat.Index < local.PossiblePrevStat.Count; ++
                local.PossiblePrevStat.Index)
              {
                if (Equal(entities.PaymentStatus.Code,
                  local.PossiblePrevStat.Item.GrDetailPossPrev.Code))
                {
                  export.PaymentStatus.Code = entities.PaymentStatus.Code;
                  local.PaymentStatFound.Flag = "Y";

                  goto ReadEach;
                }
              }
            }
            else
            {
              ExitState = "ZD_FN0000_PYMNT_STAT_NF_RB_1";

              return;
            }
          }

ReadEach:

          if (AsChar(local.PaymentStatFound.Flag) != 'Y')
          {
            ExitState = "PREV_STATUS_NF";

            return;
          }

          // ----------------------------------------------------
          // Discontinue current status, Create new status.
          // ----------------------------------------------------
          try
          {
            UpdatePaymentStatusHistory1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (IsEmpty(import.OldAndNew.ReasonText))
          {
            export.OldAndNew.ReasonText =
              "Manually Undone to the previous status ";
          }
          else
          {
            export.OldAndNew.ReasonText = import.OldAndNew.ReasonText ?? "";
          }

          try
          {
            CreatePaymentStatusHistory5();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          export.HiddenReadFromTable.SystemGeneratedIdentifier =
            entities.PaymentStatusHistory.SystemGeneratedIdentifier;
        }
        else
        {
          ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";

          return;
        }

        // -----------------------------------------------
        // PR156757 - Allow status change even if FVI is set.
        // -----------------------------------------------
        if (AsChar(export.HiddenProtectAddrField.Flag) != 'Y')
        {
          local.RemailAddressFound.Flag = "";

          if (ReadWarrantRemailAddress1())
          {
            local.RemailAddressFound.Flag = "Y";
            export.NameInMailedAddr.Text32 =
              entities.WarrantRemailAddress.Name ?? Spaces(32);
            export.Mailed.AddressLine1 = entities.WarrantRemailAddress.Street1;
            export.Mailed.AddressLine2 =
              entities.WarrantRemailAddress.Street2 ?? Spaces(30);
            export.Mailed.City = entities.WarrantRemailAddress.City;
            export.Mailed.State = entities.WarrantRemailAddress.State;
            export.Mailed.ZipCode =
              TrimEnd(entities.WarrantRemailAddress.ZipCode5) + " " + TrimEnd
              (entities.WarrantRemailAddress.ZipCode4) + " " + TrimEnd
              (entities.WarrantRemailAddress.ZipCode3);
          }

          if (AsChar(local.RemailAddressFound.Flag) != 'Y')
          {
            ExitState = "ADDRESS_NF";

            return;
          }
        }

        // ------------------------------------------------------------------------
        // KD - 1/8/99
        // Dis-associate old and reissued warrants and update status of reissued
        // warrant to CAN.
        // ----------------------------------------------------------------------
        if (Equal(entities.ExistingPaymentStatus.Code, "REIS"))
        {
          if (!ReadPaymentRequest6())
          {
            ExitState = "FN0000_PYMT_REQUEST_NF";

            return;
          }

          try
          {
            UpdatePaymentRequest2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PAYMENT_REQUEST_NU";
                UseEabRollbackCics();

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PAYMENT_REQUEST_AE_RB";
                UseEabRollbackCics();

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          foreach(var item in ReadPaymentStatusHistory5())
          {
            // -------------------------------------
            // KD - 1/8/99
            // Enddate current status for the reissued warrant
            // ------------------------------------
            try
            {
              UpdatePaymentStatusHistory1();

              // *** Continue Processing
              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  UseEabRollbackCics();
                  ExitState = "FN0000_PYMNT_STAT_HIST_NU_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  UseEabRollbackCics();
                  ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // ------------------------------------
          // KD - 1/8/99
          // Now create CAN status
          // ------------------------------------
          if (!ReadPaymentStatus2())
          {
            UseEabRollbackCics();
            ExitState = "PAYMENT_STATUS_NF";

            return;
          }

          // ----------------------------------------------
          // KD - 8/3/99
          // Add original warrant # in reason_text.
          // ----------------------------------------------
          try
          {
            CreatePaymentStatusHistory6();

            // *** Continue Processing
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                UseEabRollbackCics();
                ExitState = "FN0000_PYMNT_STAT_HISTORY_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                UseEabRollbackCics();
                ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          export.ReisTo.Number = "";
        }

        ExitState = "FN0000_STAT_SUCCESSFULLY_UNDONE";

        break;
      case "LIST":
        if (!IsEmpty(import.WarantNoPrompt.SelectChar) && !
          IsEmpty(import.StatusPrompt.SelectChar))
        {
          export.WarrantNoPrompt.SelectChar = import.WarantNoPrompt.SelectChar;
          export.StatusPrompt.SelectChar = import.StatusPrompt.SelectChar;

          var field3 = GetField(export.StatusPrompt, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.WarrantNoPrompt, "selectChar");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        switch(AsChar(import.WarantNoPrompt.SelectChar))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_LST_WARRANTS";

            return;
          case ' ':
            break;
          default:
            export.WarrantNoPrompt.SelectChar =
              import.WarantNoPrompt.SelectChar;

            var field3 = GetField(export.WarrantNoPrompt, "selectChar");

            field3.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(import.StatusPrompt.SelectChar))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_LST_PAYMENT_STATUSES";

            return;
          case ' ':
            break;
          default:
            export.StatusPrompt.SelectChar = import.StatusPrompt.SelectChar;

            var field3 = GetField(export.StatusPrompt, "selectChar");

            field3.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        var field1 = GetField(export.StatusPrompt, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.WarrantNoPrompt, "selectChar");

        field2.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "WARR":
        export.FromDate.Date = export.PaymentRequest.ProcessDate;
        ExitState = "ECO_LNK_TO_LST_WARRANTS";

        break;
      case "WDTL":
        ExitState = "ECO_LNK_TO_WDTL";

        break;
      case "WHST":
        ExitState = "ECO_LNK_TO_WHST";

        break;
      case "WARA":
        ExitState = "ECO_LNK_TO_WARA";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // -----------------------------------------------
    // PR156757 - Allow status change even if FVI is set.
    // -----------------------------------------------
    if (AsChar(export.HiddenProtectAddrField.Flag) == 'Y')
    {
      var field1 = GetField(export.WarrantRemailAddress, "name");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.WarrantRemailAddress, "street1");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.WarrantRemailAddress, "street2");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.WarrantRemailAddress, "city");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.WarrantRemailAddress, "state");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.WarrantRemailAddress, "zipCode5");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.WarrantRemailAddress, "zipCode4");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.WarrantRemailAddress, "zipCode3");

      field8.Color = "cyan";
      field8.Protected = true;
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedBy = source.CreatedBy;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveCsePersonAddress4(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.VerifiedDate = source.VerifiedDate;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
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

  private static void MovePaymentRequest1(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Classification = source.Classification;
    target.ImprestFundCode = source.ImprestFundCode;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CsePersonNumber = source.CsePersonNumber;
    target.DesignatedPayeeCsePersonNo = source.DesignatedPayeeCsePersonNo;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.CsePersonNumber = source.CsePersonNumber;
    target.Number = source.Number;
  }

  private static void MovePaymentRequest3(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MovePaymentRequest4(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MovePaymentStatus(PaymentStatus source,
    PaymentStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MovePossiblePrevStat(CabCheckPmntStatStateChange.Export.
    PossiblePrevStatGroup source, Local.PossiblePrevStatGroup target)
  {
    MovePaymentStatus(source.DetailPrevStat, target.GrDetailPossPrev);
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdPersonAddress = source.IdPersonAddress;
    target.IdPrNumber = source.IdPrNumber;
  }

  private static void MoveWarrantRemailAddress(WarrantRemailAddress source,
    WarrantRemailAddress target)
  {
    target.RemailDate = source.RemailDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Name = source.Name;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.ZipCode3 = source.ZipCode3;
  }

  private void UseCabCheckPmntStatStateChange1()
  {
    var useImport = new CabCheckPmntStatStateChange.Import();
    var useExport = new CabCheckPmntStatStateChange.Export();

    useImport.New1.Code = import.New1.Code;
    useImport.Old.Code = import.PaymentStatus.Code;

    Call(CabCheckPmntStatStateChange.Execute, useImport, useExport);

    local.StatusChangeFlag.Flag = useExport.StatusChangeFlag.Flag;
  }

  private void UseCabCheckPmntStatStateChange2()
  {
    var useImport = new CabCheckPmntStatStateChange.Import();
    var useExport = new CabCheckPmntStatStateChange.Export();

    useImport.Old.Code = import.PaymentStatus.Code;

    Call(CabCheckPmntStatStateChange.Execute, useImport, useExport);

    useExport.PossiblePrevStat.CopyTo(
      local.PossiblePrevStat, MovePossiblePrevStat);
  }

  private void UseCabCreateWarRemailAddress1()
  {
    var useImport = new CabCreateWarRemailAddress.Import();
    var useExport = new CabCreateWarRemailAddress.Export();

    useImport.PaymentStatus.Code = import.New1.Code;
    useImport.PaymentRequest.Assign(entities.PaymentRequest);
    useImport.WarrantRemailAddress.Assign(local.WarrantRemailAddress);

    Call(CabCreateWarRemailAddress.Execute, useImport, useExport);

    MovePaymentRequest1(useImport.PaymentRequest, entities.PaymentRequest);
  }

  private void UseCabCreateWarRemailAddress2()
  {
    var useImport = new CabCreateWarRemailAddress.Import();
    var useExport = new CabCreateWarRemailAddress.Export();

    useImport.PaymentStatus.Code = import.New1.Code;
    useImport.Reissued.Assign(entities.ReissuedTo);
    useImport.WarrantRemailAddress.Assign(local.WarrantRemailAddress);

    Call(CabCreateWarRemailAddress.Execute, useImport, useExport);

    MovePaymentRequest1(useImport.Reissued, entities.ReissuedTo);
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
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

    useImport.DateWorkArea.Date = local.Maximum.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private void UseFnDetermineCaseRoleForOrec()
  {
    var useImport = new FnDetermineCaseRoleForOrec.Import();
    var useExport = new FnDetermineCaseRoleForOrec.Export();

    useImport.Current.Date = local.CurrentDate.Date;
    useImport.Obligor.Number = export.PassThruFlowCsePerson.Number;

    Call(FnDetermineCaseRoleForOrec.Execute, useImport, useExport);

    local.CaseRole.Type1 = useExport.CaseRole.Type1;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
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
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveCommon(local.PrintProcess, useImport.PrintProcess);
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity1()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity2()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePersonsWorkSet.Number = import.PayeeName.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv1()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.DesigPayee.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv2()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonAddress()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    MoveCsePersonAddress3(local.CsePersonAddress, useImport.CsePersonAddress);
    useImport.CsePerson.Number = export.PassThruFlowCsePerson.Number;

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress2(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.DesigPayee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.DesigPayee);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Payee);
  }

  private void UseSiUpdateCsePersonAddress()
  {
    var useImport = new SiUpdateCsePersonAddress.Import();
    var useExport = new SiUpdateCsePersonAddress.Export();

    useImport.CsePersonAddress.Assign(local.MostRecentPayee);
    useImport.CsePerson.Number = export.PassThruFlowCsePerson.Number;

    Call(SiUpdateCsePersonAddress.Execute, useImport, useExport);
  }

  private void UseSpAddrExternalAlert()
  {
    var useImport = new SpAddrExternalAlert.Import();
    var useExport = new SpAddrExternalAlert.Export();

    MoveCsePersonAddress4(local.CsePersonAddress, useImport.CsePersonAddress);
    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    useImport.CsePerson.Number = export.PassThruFlowCsePerson.Number;

    Call(SpAddrExternalAlert.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.Print.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.Print.Text50 = useExport.WorkArea.Text50;
  }

  private void CreateCashReceiptDetailHistory()
  {
    var lastUpdatedTmst = entities.CashReceiptDetail.LastUpdatedTmst;
    var interfaceTransId = entities.CashReceiptDetail.InterfaceTransId;
    var offsetTaxid = entities.CashReceiptDetail.OffsetTaxid;
    var jointReturnInd = entities.CashReceiptDetail.JointReturnInd;
    var jointReturnName = entities.CashReceiptDetail.JointReturnName;
    var refundedAmount = entities.CashReceiptDetail.RefundedAmount;
    var distributedAmount = entities.CashReceiptDetail.DistributedAmount;
    var adjustmentInd = entities.CashReceiptDetail.AdjustmentInd;
    var sequentialIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var attribute2SupportedPersonFirstName =
      entities.CashReceiptDetail.Attribute2SupportedPersonFirstName;
    var attribute2SupportedPersonLastName =
      entities.CashReceiptDetail.Attribute2SupportedPersonLastName;
    var attribute2SupportedPersonMiddleName =
      entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName;
    var cashReceiptEventNumber =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    var cashReceiptNumber = import.CashReceiptDetailHistory.CashReceiptNumber;
    var collectionDate = import.CashReceiptDetailHistory.CollectionDate;
    var obligorPersonNumber = entities.CsePerson.Number;
    var courtOrderNumber = entities.CashReceiptDetail.CourtOrderNumber ?? Substring
      (entities.CashReceiptDetail.CourtOrderNumber, 1, 17);
    var caseNumber = import.CashReceiptDetailHistory.CaseNumber ?? "";
    var obligorFirstName = entities.CashReceiptDetail.ObligorFirstName;
    var obligorLastName = entities.CashReceiptDetail.ObligorLastName;
    var obligorMiddleName = entities.CashReceiptDetail.ObligorMiddleName;
    var obligorPhoneNumber = entities.CashReceiptDetail.ObligorPhoneNumber;
    var obligorSocialSecurityNumber =
      entities.CashReceiptDetail.ObligorSocialSecurityNumber;
    var offsetTaxYear = entities.CashReceiptDetail.OffsetTaxYear;
    var defaultedCollectionDateInd =
      entities.CashReceiptDetail.DefaultedCollectionDateInd;
    var multiPayor = entities.CashReceiptDetail.MultiPayor;
    var receivedAmount = entities.CashReceiptDetail.ReceivedAmount;
    var collectionAmount = entities.CashReceiptDetail.CollectionAmount;
    var payeeFirstName = entities.CashReceiptDetail.PayeeFirstName;
    var payeeMiddleName = entities.CashReceiptDetail.PayeeMiddleName;
    var payeeLastName = entities.CashReceiptDetail.PayeeLastName;
    var attribute1SupportedPersonFirstName =
      entities.CashReceiptDetail.Attribute1SupportedPersonFirstName;
    var attribute1SupportedPersonMiddleName =
      entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName;
    var attribute1SupportedPersonLastName =
      entities.CashReceiptDetail.Attribute1SupportedPersonLastName;
    var createdBy = entities.CashReceiptDetail.CreatedBy;
    var createdTmst = entities.CashReceiptDetail.CreatedTmst;
    var lastUpdatedBy = entities.CashReceiptDetail.LastUpdatedBy ?? Spaces(8);
    var cashReceiptType1 = entities.CashReceiptType.SystemGeneratedIdentifier;
    var cashReceiptSourceType1 =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    var reference = entities.CashReceiptDetail.Reference;
    var notes = entities.CashReceiptDetail.Notes;

    CheckValid<CashReceiptDetailHistory>("JointReturnInd", jointReturnInd);
    CheckValid<CashReceiptDetailHistory>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    CheckValid<CashReceiptDetailHistory>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetailHistory>("CollectionAmtFullyAppliedInd", "");
    entities.CashReceiptDetailHistory.Populated = false;
    Update("CreateCashReceiptDetailHistory",
      (db, command) =>
      {
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "interfaceTransId", interfaceTransId);
        db.SetNullableInt32(command, "offsetTaxid", offsetTaxid);
        db.SetNullableString(command, "jointReturnInd", jointReturnInd);
        db.SetNullableString(command, "jointReturnName", jointReturnName);
        db.SetNullableDecimal(command, "refundedAmount", refundedAmount);
        db.SetNullableDecimal(command, "distributedAmount", distributedAmount);
        db.SetNullableString(command, "adjustmentInd", adjustmentInd);
        db.SetNullableInt32(command, "crdetailHistId", sequentialIdentifier);
        db.SetNullableString(
          command, "suppPrsnFn2", attribute2SupportedPersonFirstName);
        db.SetNullableString(
          command, "suppPrsnLn2", attribute2SupportedPersonLastName);
        db.SetNullableString(
          command, "suppPrsnMn2", attribute2SupportedPersonMiddleName);
        db.SetInt32(command, "collctTypeId", 0);
        db.SetInt32(command, "creventNbrId", cashReceiptEventNumber);
        db.SetInt32(command, "crNbrId", cashReceiptNumber);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "oblgorPersNbrId", obligorPersonNumber);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMiddleNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", obligorPhoneNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableInt32(command, "offsetTaxYear", offsetTaxYear);
        db.SetNullableString(
          command, "dfltCllctnDtInd", defaultedCollectionDateInd);
        db.SetNullableString(command, "multiPayor", multiPayor);
        db.SetNullableDecimal(command, "receivedAmount", receivedAmount);
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableString(command, "payeeFirstName", payeeFirstName);
        db.SetNullableString(command, "payeeMiddleName", payeeMiddleName);
        db.SetNullableString(command, "payeeLastName", payeeLastName);
        db.SetNullableString(
          command, "suppPrsnFn1", attribute1SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnMidNm1", attribute1SupportedPersonMiddleName);
        db.SetNullableString(
          command, "supPrsnLstNm1", attribute1SupportedPersonLastName);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "collectionAmtFul", "");
        db.SetInt32(command, "cashRecType", cashReceiptType1);
        db.SetInt32(command, "cashRecSrcType", cashReceiptSourceType1);
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
      });

    entities.CashReceiptDetailHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetailHistory.InterfaceTransId = interfaceTransId;
    entities.CashReceiptDetailHistory.OffsetTaxid = offsetTaxid;
    entities.CashReceiptDetailHistory.JointReturnInd = jointReturnInd;
    entities.CashReceiptDetailHistory.JointReturnName = jointReturnName;
    entities.CashReceiptDetailHistory.RefundedAmount = refundedAmount;
    entities.CashReceiptDetailHistory.DistributedAmount = distributedAmount;
    entities.CashReceiptDetailHistory.AdjustmentInd = adjustmentInd;
    entities.CashReceiptDetailHistory.SequentialIdentifier =
      sequentialIdentifier;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName =
      attribute2SupportedPersonFirstName;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonLastName =
      attribute2SupportedPersonLastName;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName =
      attribute2SupportedPersonMiddleName;
    entities.CashReceiptDetailHistory.CollectionTypeIdentifier = 0;
    entities.CashReceiptDetailHistory.CashReceiptEventNumber =
      cashReceiptEventNumber;
    entities.CashReceiptDetailHistory.CashReceiptNumber = cashReceiptNumber;
    entities.CashReceiptDetailHistory.CollectionDate = collectionDate;
    entities.CashReceiptDetailHistory.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetailHistory.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetailHistory.CaseNumber = caseNumber;
    entities.CashReceiptDetailHistory.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetailHistory.ObligorLastName = obligorLastName;
    entities.CashReceiptDetailHistory.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetailHistory.ObligorPhoneNumber = obligorPhoneNumber;
    entities.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetailHistory.OffsetTaxYear = offsetTaxYear;
    entities.CashReceiptDetailHistory.DefaultedCollectionDateInd =
      defaultedCollectionDateInd;
    entities.CashReceiptDetailHistory.MultiPayor = multiPayor;
    entities.CashReceiptDetailHistory.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetailHistory.CollectionAmount = collectionAmount;
    entities.CashReceiptDetailHistory.PayeeFirstName = payeeFirstName;
    entities.CashReceiptDetailHistory.PayeeMiddleName = payeeMiddleName;
    entities.CashReceiptDetailHistory.PayeeLastName = payeeLastName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName =
      attribute1SupportedPersonFirstName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName =
      attribute1SupportedPersonMiddleName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonLastName =
      attribute1SupportedPersonLastName;
    entities.CashReceiptDetailHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailHistory.CreatedTmst = createdTmst;
    entities.CashReceiptDetailHistory.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd = "";
    entities.CashReceiptDetailHistory.CashReceiptType = cashReceiptType1;
    entities.CashReceiptDetailHistory.CashReceiptSourceType =
      cashReceiptSourceType1;
    entities.CashReceiptDetailHistory.Reference = reference;
    entities.CashReceiptDetailHistory.Notes = notes;
    entities.CashReceiptDetailHistory.Populated = true;
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier =
      entities.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var reasonCodeId = "PROCCANREF";
    var createdBy = global.UserId;
    var discontinueDate = local.Maximum.Date;
    var reasonText = Spaces(240);

    entities.NewCashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", reasonCodeId);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.NewCashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.NewCashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.NewCashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.NewCashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.NewCashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.NewCashReceiptDetailStatHistory.CreatedTimestamp =
      createdTimestamp;
    entities.NewCashReceiptDetailStatHistory.ReasonCodeId = reasonCodeId;
    entities.NewCashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.NewCashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.NewCashReceiptDetailStatHistory.ReasonText = reasonText;
    entities.NewCashReceiptDetailStatHistory.Populated = true;
  }

  private void CreateInterstatePaymentAddress()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var addressStartDate = local.CurrentDate.Date;
    var type1 = "PY";
    var street1 = local.WarrantRemailAddress.Street1;
    var street2 = local.WarrantRemailAddress.Street2 ?? "";
    var city = Substring(local.WarrantRemailAddress.City, 1, 18);
    var zip5 = local.WarrantRemailAddress.ZipCode5;
    var addressEndDate = local.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var payableToName = local.WarrantRemailAddress.Name ?? "";
    var state = local.WarrantRemailAddress.State;
    var zip4 = local.WarrantRemailAddress.ZipCode4 ?? "";
    var zip3 = local.WarrantRemailAddress.ZipCode3 ?? "";
    var locationType = "D";

    CheckValid<InterstatePaymentAddress>("LocationType", locationType);
    entities.InterstatePaymentAddress.Populated = false;
    Update("CreateInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "addressStartDate", addressStartDate);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetNullableString(command, "zip5", zip5);
        db.SetNullableDate(command, "addressEndDate", addressEndDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "payableToName", payableToName);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zip5);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", "");
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "country", "");
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "fipsCounty", "");
        db.SetNullableString(command, "fipsState", "");
        db.SetNullableString(command, "fipsLocation", "");
        db.SetNullableInt32(command, "routingNumberAba", 0);
        db.SetNullableString(command, "accountNumberDfi", "");
        db.SetNullableString(command, "accountType", "");
      });

    entities.InterstatePaymentAddress.IntGeneratedId = intGeneratedId;
    entities.InterstatePaymentAddress.AddressStartDate = addressStartDate;
    entities.InterstatePaymentAddress.Type1 = type1;
    entities.InterstatePaymentAddress.Street1 = street1;
    entities.InterstatePaymentAddress.Street2 = street2;
    entities.InterstatePaymentAddress.City = city;
    entities.InterstatePaymentAddress.Zip5 = zip5;
    entities.InterstatePaymentAddress.AddressEndDate = addressEndDate;
    entities.InterstatePaymentAddress.CreatedBy = createdBy;
    entities.InterstatePaymentAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.LastUpdatedBy = createdBy;
    entities.InterstatePaymentAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.PayableToName = payableToName;
    entities.InterstatePaymentAddress.State = state;
    entities.InterstatePaymentAddress.ZipCode = zip5;
    entities.InterstatePaymentAddress.Zip4 = zip4;
    entities.InterstatePaymentAddress.Zip3 = zip3;
    entities.InterstatePaymentAddress.County = "";
    entities.InterstatePaymentAddress.LocationType = locationType;
    entities.InterstatePaymentAddress.FipsCounty = "";
    entities.InterstatePaymentAddress.FipsState = "";
    entities.InterstatePaymentAddress.FipsLocation = "";
    entities.InterstatePaymentAddress.RoutingNumberAba = 0;
    entities.InterstatePaymentAddress.AccountNumberDfi = "";
    entities.InterstatePaymentAddress.AccountType = "";
    entities.InterstatePaymentAddress.Populated = true;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = (int)((long)1000 * Microsecond(Now()) + 10
      * Now().Second) + Now().Minute;
    var processDate = local.CurrentDate.Date;
    var amount = entities.PaymentRequest.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo =
      entities.PaymentRequest.DesignatedPayeeCsePersonNo;
    var csePersonNumber = entities.PaymentRequest.CsePersonNumber;
    var imprestFundCode = entities.PaymentRequest.ImprestFundCode;
    var classification = entities.PaymentRequest.Classification;
    var number = export.NewReis.Number ?? "";
    var printDate = Now().Date;
    var type1 = entities.PaymentRequest.Type1;
    var prqRGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;

    CheckValid<PaymentRequest>("Type1", type1);
    entities.ReissuedTo.Populated = false;
    Update("CreatePaymentRequest",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.
          SetNullableString(command, "dpCsePerNum", designatedPayeeCsePersonNo);
          
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", imprestFundCode);
        db.SetString(command, "classification", classification);
        db.SetString(command, "recoveryFiller", "");
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", number);
        db.SetNullableDate(command, "printDate", printDate);
        db.SetString(command, "type", type1);
        db.SetNullableInt32(command, "prqRGeneratedId", prqRGeneratedId);
      });

    entities.ReissuedTo.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.ReissuedTo.ProcessDate = processDate;
    entities.ReissuedTo.Amount = amount;
    entities.ReissuedTo.CreatedBy = createdBy;
    entities.ReissuedTo.CreatedTimestamp = createdTimestamp;
    entities.ReissuedTo.DesignatedPayeeCsePersonNo = designatedPayeeCsePersonNo;
    entities.ReissuedTo.CsePersonNumber = csePersonNumber;
    entities.ReissuedTo.ImprestFundCode = imprestFundCode;
    entities.ReissuedTo.Classification = classification;
    entities.ReissuedTo.Number = number;
    entities.ReissuedTo.PrintDate = printDate;
    entities.ReissuedTo.Type1 = type1;
    entities.ReissuedTo.PrqRGeneratedId = prqRGeneratedId;
    entities.ReissuedTo.Populated = true;
  }

  private void CreatePaymentStatusHistory1()
  {
    var pstGeneratedId = entities.Kpc.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.ReissuedTo.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = TimeToInt(Time(Now()));
    var effectiveDate = local.CurrentDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = export.OldAndNew.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", effectiveDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = effectiveDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory2()
  {
    var pstGeneratedId = entities.Req.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.ReissuedTo.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = TimeToInt(Time(Now()));
    var effectiveDate = local.CurrentDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = export.OldAndNew.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", effectiveDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = effectiveDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory3()
  {
    var pstGeneratedId = entities.Paid.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.ReissuedTo.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = TimeToInt(Time(Now()));
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate1();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = export.OldAndNew.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory3",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory4()
  {
    var pstGeneratedId = entities.NewPaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate1();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = export.OldAndNew.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory4",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory5()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate1();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = export.OldAndNew.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory5",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory6()
  {
    var pstGeneratedId = entities.Can.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.Reissued.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate1();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText =
      Substring("PF10 delete action performed on original warrant # " +
      (export.PaymentRequest.Number ?? ""), 1, 240);

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory6",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "type", local.HardcodeObligor.Type1);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "type", local.HardcodePayee.Type1);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase4()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase4",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.ReceiptRefund.CrdIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crvIdentifier",
          entities.ReceiptRefund.CrvIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.ReceiptRefund.CstIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier",
          entities.ReceiptRefund.CrtIdentifier.GetValueOrDefault());
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
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 11);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 32);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.CashReceiptDetail.SuppPersonNoForVol =
          db.GetNullableString(reader, 39);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 41);
        entities.CashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 42);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.CashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatus.Populated = false;
    entities.ExistingCashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
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
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 10);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 11);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          local.HardcodeSuspend.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptTypeCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptTypeCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtypeId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptType.Code = db.GetString(reader, 4);
        entities.CashReceiptType.Name = db.GetString(reader, 5);
        entities.CashReceiptType.EffectiveDate = db.GetDate(reader, 6);
        entities.CashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 8);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 9);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 10);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableDate(
          command, "verifiedDate", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 22);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 23);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 24);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 25);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadInterstatePaymentAddress()
  {
    entities.InterstatePaymentAddress.Populated = false;

    return ReadEach("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetNullableDate(command, "addressEndDate", date);
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 3);
        entities.InterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 5);
        entities.InterstatePaymentAddress.Zip5 =
          db.GetNullableString(reader, 6);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 7);
        entities.InterstatePaymentAddress.CreatedBy = db.GetString(reader, 8);
        entities.InterstatePaymentAddress.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.InterstatePaymentAddress.LastUpdatedBy =
          db.GetString(reader, 10);
        entities.InterstatePaymentAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 12);
        entities.InterstatePaymentAddress.State =
          db.GetNullableString(reader, 13);
        entities.InterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 14);
        entities.InterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 15);
        entities.InterstatePaymentAddress.Zip3 =
          db.GetNullableString(reader, 16);
        entities.InterstatePaymentAddress.County =
          db.GetNullableString(reader, 17);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 18);
        entities.InterstatePaymentAddress.FipsCounty =
          db.GetNullableString(reader, 19);
        entities.InterstatePaymentAddress.FipsState =
          db.GetNullableString(reader, 20);
        entities.InterstatePaymentAddress.FipsLocation =
          db.GetNullableString(reader, 21);
        entities.InterstatePaymentAddress.RoutingNumberAba =
          db.GetNullableInt64(reader, 22);
        entities.InterstatePaymentAddress.AccountNumberDfi =
          db.GetNullableString(reader, 23);
        entities.InterstatePaymentAddress.AccountType =
          db.GetNullableString(reader, 24);
        entities.InterstatePaymentAddress.Populated = true;
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.InterstatePaymentAddress.LocationType);

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.PaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 12);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 13);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 15);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest2()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          export.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 12);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 13);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 15);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest3()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest3",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 12);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 13);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 15);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest4()
  {
    return Read("ReadPaymentRequest4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.PaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        local.NbrOfWarrants.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadPaymentRequest5()
  {
    entities.ForReissue.Populated = false;

    return Read("ReadPaymentRequest5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqRGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ForReissue.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ForReissue.Number = db.GetNullableString(reader, 1);
        entities.ForReissue.Type1 = db.GetString(reader, 2);
        entities.ForReissue.PrqRGeneratedId = db.GetNullableInt32(reader, 3);
        entities.ForReissue.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.ForReissue.Type1);
      });
  }

  private bool ReadPaymentRequest6()
  {
    entities.Reissued.Populated = false;

    return Read("ReadPaymentRequest6",
      (db, command) =>
      {
        db.SetNullableString(command, "number", import.ReisTo.Number ?? "");
      },
      (db, reader) =>
      {
        entities.Reissued.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Reissued.Number = db.GetNullableString(reader, 1);
        entities.Reissued.Type1 = db.GetString(reader, 2);
        entities.Reissued.RctRTstamp = db.GetNullableDateTime(reader, 3);
        entities.Reissued.PrqRGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Reissued.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.Reissued.Type1);
      });
  }

  private bool ReadPaymentRequest7()
  {
    entities.ReissuedTo.Populated = false;

    return Read("ReadPaymentRequest7",
      (db, command) =>
      {
        db.SetNullableString(command, "number", export.NewReis.Number ?? "");
      },
      (db, reader) =>
      {
        entities.ReissuedTo.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ReissuedTo.ProcessDate = db.GetDate(reader, 1);
        entities.ReissuedTo.Amount = db.GetDecimal(reader, 2);
        entities.ReissuedTo.CreatedBy = db.GetString(reader, 3);
        entities.ReissuedTo.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.ReissuedTo.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.ReissuedTo.CsePersonNumber = db.GetNullableString(reader, 6);
        entities.ReissuedTo.ImprestFundCode = db.GetNullableString(reader, 7);
        entities.ReissuedTo.Classification = db.GetString(reader, 8);
        entities.ReissuedTo.Number = db.GetNullableString(reader, 9);
        entities.ReissuedTo.PrintDate = db.GetNullableDate(reader, 10);
        entities.ReissuedTo.Type1 = db.GetString(reader, 11);
        entities.ReissuedTo.PrqRGeneratedId = db.GetNullableInt32(reader, 12);
        entities.ReissuedTo.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.ReissuedTo.Type1);
      });
  }

  private bool ReadPaymentRequest8()
  {
    entities.Reissued.Populated = false;

    return Read("ReadPaymentRequest8",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId", local.Temp.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Reissued.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Reissued.Number = db.GetNullableString(reader, 1);
        entities.Reissued.Type1 = db.GetString(reader, 2);
        entities.Reissued.RctRTstamp = db.GetNullableDateTime(reader, 3);
        entities.Reissued.PrqRGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Reissued.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.Reissued.Type1);
      });
  }

  private bool ReadPaymentRequest9()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentRequest.Populated);
    entities.ReissuedFrom.Populated = false;

    return Read("ReadPaymentRequest9",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.PrqRGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReissuedFrom.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReissuedFrom.PrqRGeneratedId = db.GetNullableInt32(reader, 1);
        entities.ReissuedFrom.InterstateInd = db.GetNullableString(reader, 2);
        entities.ReissuedFrom.Populated = true;
      });
  }

  private bool ReadPaymentStatus1()
  {
    System.Diagnostics.Debug.Assert(entities.NewPaymentStatusHistory.Populated);
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          entities.NewPaymentStatusHistory.PstGeneratedId);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Name = db.GetString(reader, 2);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatus.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PaymentStatus.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatus.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentStatus.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.PaymentStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentStatus.Description = db.GetNullableString(reader, 9);
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatus2()
  {
    entities.Can.Populated = false;

    return Read("ReadPaymentStatus2",
      null,
      (db, reader) =>
      {
        entities.Can.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Can.Code = db.GetString(reader, 1);
        entities.Can.Populated = true;
      });
  }

  private bool ReadPaymentStatus3()
  {
    entities.ExistingPaymentStatus.Populated = false;

    return Read("ReadPaymentStatus3",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
      },
      (db, reader) =>
      {
        entities.ExistingPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentStatus.Code = db.GetString(reader, 1);
        entities.ExistingPaymentStatus.LastUpdateBy =
          db.GetNullableString(reader, 2);
        entities.ExistingPaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatus4()
  {
    entities.Kpc.Populated = false;

    return Read("ReadPaymentStatus4",
      null,
      (db, reader) =>
      {
        entities.Kpc.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Kpc.Code = db.GetString(reader, 1);
        entities.Kpc.Populated = true;
      });
  }

  private bool ReadPaymentStatus5()
  {
    entities.NewPaymentStatus.Populated = false;

    return Read("ReadPaymentStatus5",
      (db, command) =>
      {
        db.SetString(command, "code", import.New1.Code);
      },
      (db, reader) =>
      {
        entities.NewPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NewPaymentStatus.Code = db.GetString(reader, 1);
        entities.NewPaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatus6()
  {
    entities.Paid.Populated = false;

    return Read("ReadPaymentStatus6",
      null,
      (db, reader) =>
      {
        entities.Paid.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Paid.Code = db.GetString(reader, 1);
        entities.Paid.Populated = true;
      });
  }

  private bool ReadPaymentStatus7()
  {
    entities.Req.Populated = false;

    return Read("ReadPaymentStatus7",
      null,
      (db, reader) =>
      {
        entities.Req.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Req.Code = db.GetString(reader, 1);
        entities.Req.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory1()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "pymntStatHistId",
          import.HiddenReadFromTable.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.ExistingPaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory2()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.PaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory3()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory3",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory4()
  {
    entities.NewPaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusHistory4",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PaymentStatusHistory.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewPaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.NewPaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.NewPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.NewPaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.NewPaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.NewPaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.NewPaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory5()
  {
    entities.PaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusHistory5",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.Reissued.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private bool ReadPaymentStatusPaymentStatusHistory()
  {
    entities.PaymentStatus.Populated = false;
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Name = db.GetString(reader, 2);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatus.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PaymentStatus.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatus.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentStatus.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.PaymentStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentStatus.Description = db.GetNullableString(reader, 9);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 10);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 12);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 14);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 16);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    entities.Program.Populated = false;

    return ReadEach("ReadProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadReceiptRefund1()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentRequest.Populated);
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PaymentRequest.RctRTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 1);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 2);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private bool ReadReceiptRefund2()
  {
    System.Diagnostics.Debug.Assert(entities.Reissued.Populated);
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Reissued.RctRTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 1);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 2);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private bool ReadWarrantRemailAddress1()
  {
    entities.WarrantRemailAddress.Populated = false;

    return Read("ReadWarrantRemailAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqId", entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.WarrantRemailAddress.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.WarrantRemailAddress.Street1 = db.GetString(reader, 1);
        entities.WarrantRemailAddress.Street2 = db.GetNullableString(reader, 2);
        entities.WarrantRemailAddress.City = db.GetString(reader, 3);
        entities.WarrantRemailAddress.State = db.GetString(reader, 4);
        entities.WarrantRemailAddress.ZipCode4 =
          db.GetNullableString(reader, 5);
        entities.WarrantRemailAddress.ZipCode5 = db.GetString(reader, 6);
        entities.WarrantRemailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.WarrantRemailAddress.Name = db.GetNullableString(reader, 8);
        entities.WarrantRemailAddress.RemailDate = db.GetDate(reader, 9);
        entities.WarrantRemailAddress.CreatedBy = db.GetString(reader, 10);
        entities.WarrantRemailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.WarrantRemailAddress.PrqId = db.GetInt32(reader, 12);
        entities.WarrantRemailAddress.Populated = true;
      });
  }

  private bool ReadWarrantRemailAddress2()
  {
    entities.WarrantRemailAddress.Populated = false;

    return Read("ReadWarrantRemailAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqId", entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.WarrantRemailAddress.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.WarrantRemailAddress.Street1 = db.GetString(reader, 1);
        entities.WarrantRemailAddress.Street2 = db.GetNullableString(reader, 2);
        entities.WarrantRemailAddress.City = db.GetString(reader, 3);
        entities.WarrantRemailAddress.State = db.GetString(reader, 4);
        entities.WarrantRemailAddress.ZipCode4 =
          db.GetNullableString(reader, 5);
        entities.WarrantRemailAddress.ZipCode5 = db.GetString(reader, 6);
        entities.WarrantRemailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.WarrantRemailAddress.Name = db.GetNullableString(reader, 8);
        entities.WarrantRemailAddress.RemailDate = db.GetDate(reader, 9);
        entities.WarrantRemailAddress.CreatedBy = db.GetString(reader, 10);
        entities.WarrantRemailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.WarrantRemailAddress.PrqId = db.GetInt32(reader, 12);
        entities.WarrantRemailAddress.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount =
      entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() -
      entities.PaymentRequest.Amount;

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd", "");
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "refundedAmt", refundedAmount);
        db.SetNullableString(command, "collamtApplInd", "");
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.RefundedAmount = refundedAmount;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd = "";
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCashReceiptDetailStatHistory.Populated);

    var discontinueDate = local.CurrentDate.Date;

    entities.ExistingCashReceiptDetailStatHistory.Populated = false;
    Update("UpdateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier);
        db.SetInt32(
          command, "cdsIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
      discontinueDate;
    entities.ExistingCashReceiptDetailStatHistory.Populated = true;
  }

  private void UpdateInterstatePaymentAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstatePaymentAddress.Populated);

    var street1 = local.WarrantRemailAddress.Street1;
    var street2 = local.WarrantRemailAddress.Street2 ?? "";
    var city = Substring(local.WarrantRemailAddress.City, 1, 18);
    var zip5 = local.WarrantRemailAddress.ZipCode5;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var payableToName = local.WarrantRemailAddress.Name ?? "";
    var state = local.WarrantRemailAddress.State;
    var zip4 = local.WarrantRemailAddress.ZipCode4 ?? "";
    var zip3 = local.WarrantRemailAddress.ZipCode3 ?? "";

    entities.InterstatePaymentAddress.Populated = false;
    Update("UpdateInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetNullableString(command, "zip5", zip5);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "payableToName", payableToName);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zip5);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstatePaymentAddress.IntGeneratedId);
        db.SetDate(
          command, "addressStartDate",
          entities.InterstatePaymentAddress.AddressStartDate.
            GetValueOrDefault());
      });

    entities.InterstatePaymentAddress.Street1 = street1;
    entities.InterstatePaymentAddress.Street2 = street2;
    entities.InterstatePaymentAddress.City = city;
    entities.InterstatePaymentAddress.Zip5 = zip5;
    entities.InterstatePaymentAddress.LastUpdatedBy = lastUpdatedBy;
    entities.InterstatePaymentAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstatePaymentAddress.PayableToName = payableToName;
    entities.InterstatePaymentAddress.State = state;
    entities.InterstatePaymentAddress.ZipCode = zip5;
    entities.InterstatePaymentAddress.Zip4 = zip4;
    entities.InterstatePaymentAddress.Zip3 = zip3;
    entities.InterstatePaymentAddress.Populated = true;
  }

  private void UpdatePaymentRequest1()
  {
    var recoupmentIndKpc = export.PaymentRequest.RecoupmentIndKpc ?? "";

    entities.PaymentRequest.Populated = false;
    Update("UpdatePaymentRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "recoupmentIndKpc", recoupmentIndKpc);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    entities.PaymentRequest.RecoupmentIndKpc = recoupmentIndKpc;
    entities.PaymentRequest.Populated = true;
  }

  private void UpdatePaymentRequest2()
  {
    entities.Reissued.Populated = false;
    Update("UpdatePaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.Reissued.SystemGeneratedIdentifier);
      });

    entities.Reissued.PrqRGeneratedId = null;
    entities.Reissued.Populated = true;
  }

  private void UpdatePaymentStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = local.CurrentDate.Date;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory1",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void UpdatePaymentStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var reasonText = import.OldAndNew.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory2",
      (db, command) =>
      {
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
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
    /// A value of HiddenProtectKpcRecoup.
    /// </summary>
    [JsonPropertyName("hiddenProtectKpcRecoup")]
    public Common HiddenProtectKpcRecoup
    {
      get => hiddenProtectKpcRecoup ??= new();
      set => hiddenProtectKpcRecoup = value;
    }

    /// <summary>
    /// A value of HiddenProtectAddrField.
    /// </summary>
    [JsonPropertyName("hiddenProtectAddrField")]
    public Common HiddenProtectAddrField
    {
      get => hiddenProtectAddrField ??= new();
      set => hiddenProtectAddrField = value;
    }

    /// <summary>
    /// A value of HiddenPaymentRequest.
    /// </summary>
    [JsonPropertyName("hiddenPaymentRequest")]
    public PaymentRequest HiddenPaymentRequest
    {
      get => hiddenPaymentRequest ??= new();
      set => hiddenPaymentRequest = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
    }

    /// <summary>
    /// A value of CreateReturnAlertFlag.
    /// </summary>
    [JsonPropertyName("createReturnAlertFlag")]
    public Common CreateReturnAlertFlag
    {
      get => createReturnAlertFlag ??= new();
      set => createReturnAlertFlag = value;
    }

    /// <summary>
    /// A value of ReisTo.
    /// </summary>
    [JsonPropertyName("reisTo")]
    public PaymentRequest ReisTo
    {
      get => reisTo ??= new();
      set => reisTo = value;
    }

    /// <summary>
    /// A value of HiddenReadFromTable.
    /// </summary>
    [JsonPropertyName("hiddenReadFromTable")]
    public PaymentStatusHistory HiddenReadFromTable
    {
      get => hiddenReadFromTable ??= new();
      set => hiddenReadFromTable = value;
    }

    /// <summary>
    /// A value of DisplayIndicator.
    /// </summary>
    [JsonPropertyName("displayIndicator")]
    public Common DisplayIndicator
    {
      get => displayIndicator ??= new();
      set => displayIndicator = value;
    }

    /// <summary>
    /// A value of OldAndNew.
    /// </summary>
    [JsonPropertyName("oldAndNew")]
    public PaymentStatusHistory OldAndNew
    {
      get => oldAndNew ??= new();
      set => oldAndNew = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PaymentStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of NewReis.
    /// </summary>
    [JsonPropertyName("newReis")]
    public PaymentRequest NewReis
    {
      get => newReis ??= new();
      set => newReis = value;
    }

    /// <summary>
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
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
    /// A value of StatusPrompt.
    /// </summary>
    [JsonPropertyName("statusPrompt")]
    public Common StatusPrompt
    {
      get => statusPrompt ??= new();
      set => statusPrompt = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public CsePersonsWorkSet DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of PayeeName.
    /// </summary>
    [JsonPropertyName("payeeName")]
    public CsePersonsWorkSet PayeeName
    {
      get => payeeName ??= new();
      set => payeeName = value;
    }

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
    /// A value of WarantNoPrompt.
    /// </summary>
    [JsonPropertyName("warantNoPrompt")]
    public Common WarantNoPrompt
    {
      get => warantNoPrompt ??= new();
      set => warantNoPrompt = value;
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
    /// A value of NameInMailedAddr.
    /// </summary>
    [JsonPropertyName("nameInMailedAddr")]
    public WorkArea NameInMailedAddr
    {
      get => nameInMailedAddr ??= new();
      set => nameInMailedAddr = value;
    }

    /// <summary>
    /// A value of Mailed.
    /// </summary>
    [JsonPropertyName("mailed")]
    public LocalWorkAddr Mailed
    {
      get => mailed ??= new();
      set => mailed = value;
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

    private Common hiddenProtectKpcRecoup;
    private Common hiddenProtectAddrField;
    private PaymentRequest hiddenPaymentRequest;
    private CashReceiptDetailHistory cashReceiptDetailHistory;
    private Common createReturnAlertFlag;
    private PaymentRequest reisTo;
    private PaymentStatusHistory hiddenReadFromTable;
    private Common displayIndicator;
    private PaymentStatusHistory oldAndNew;
    private PaymentStatus new1;
    private PaymentRequest newReis;
    private WarrantRemailAddress warrantRemailAddress;
    private PaymentStatusHistory paymentStatusHistory;
    private Common statusPrompt;
    private CsePersonsWorkSet desigPayee;
    private CsePersonsWorkSet payeeName;
    private PaymentStatus paymentStatus;
    private Common warantNoPrompt;
    private PaymentRequest paymentRequest;
    private WorkArea nameInMailedAddr;
    private LocalWorkAddr mailed;
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
    /// A value of HiddenProtectKpcRecoup.
    /// </summary>
    [JsonPropertyName("hiddenProtectKpcRecoup")]
    public Common HiddenProtectKpcRecoup
    {
      get => hiddenProtectKpcRecoup ??= new();
      set => hiddenProtectKpcRecoup = value;
    }

    /// <summary>
    /// A value of HiddenProtectAddrField.
    /// </summary>
    [JsonPropertyName("hiddenProtectAddrField")]
    public Common HiddenProtectAddrField
    {
      get => hiddenProtectAddrField ??= new();
      set => hiddenProtectAddrField = value;
    }

    /// <summary>
    /// A value of DuplicateWarrants.
    /// </summary>
    [JsonPropertyName("duplicateWarrants")]
    public Common DuplicateWarrants
    {
      get => duplicateWarrants ??= new();
      set => duplicateWarrants = value;
    }

    /// <summary>
    /// A value of HiddenPaymentRequest.
    /// </summary>
    [JsonPropertyName("hiddenPaymentRequest")]
    public PaymentRequest HiddenPaymentRequest
    {
      get => hiddenPaymentRequest ??= new();
      set => hiddenPaymentRequest = value;
    }

    /// <summary>
    /// A value of CreateReturnAlertFlag.
    /// </summary>
    [JsonPropertyName("createReturnAlertFlag")]
    public Common CreateReturnAlertFlag
    {
      get => createReturnAlertFlag ??= new();
      set => createReturnAlertFlag = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DateWorkArea FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of PassThruFlowPaymentRequest.
    /// </summary>
    [JsonPropertyName("passThruFlowPaymentRequest")]
    public PaymentRequest PassThruFlowPaymentRequest
    {
      get => passThruFlowPaymentRequest ??= new();
      set => passThruFlowPaymentRequest = value;
    }

    /// <summary>
    /// A value of PassThruFlowCsePerson.
    /// </summary>
    [JsonPropertyName("passThruFlowCsePerson")]
    public CsePerson PassThruFlowCsePerson
    {
      get => passThruFlowCsePerson ??= new();
      set => passThruFlowCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenReadFromTable.
    /// </summary>
    [JsonPropertyName("hiddenReadFromTable")]
    public PaymentStatusHistory HiddenReadFromTable
    {
      get => hiddenReadFromTable ??= new();
      set => hiddenReadFromTable = value;
    }

    /// <summary>
    /// A value of DisplayIndicator.
    /// </summary>
    [JsonPropertyName("displayIndicator")]
    public Common DisplayIndicator
    {
      get => displayIndicator ??= new();
      set => displayIndicator = value;
    }

    /// <summary>
    /// A value of OldAndNew.
    /// </summary>
    [JsonPropertyName("oldAndNew")]
    public PaymentStatusHistory OldAndNew
    {
      get => oldAndNew ??= new();
      set => oldAndNew = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PaymentStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of NewReis.
    /// </summary>
    [JsonPropertyName("newReis")]
    public PaymentRequest NewReis
    {
      get => newReis ??= new();
      set => newReis = value;
    }

    /// <summary>
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
    }

    /// <summary>
    /// A value of StatusPrompt.
    /// </summary>
    [JsonPropertyName("statusPrompt")]
    public Common StatusPrompt
    {
      get => statusPrompt ??= new();
      set => statusPrompt = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public CsePersonsWorkSet DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of WarrantNoPrompt.
    /// </summary>
    [JsonPropertyName("warrantNoPrompt")]
    public Common WarrantNoPrompt
    {
      get => warrantNoPrompt ??= new();
      set => warrantNoPrompt = value;
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
    /// A value of NameInMailedAddr.
    /// </summary>
    [JsonPropertyName("nameInMailedAddr")]
    public WorkArea NameInMailedAddr
    {
      get => nameInMailedAddr ??= new();
      set => nameInMailedAddr = value;
    }

    /// <summary>
    /// A value of Mailed.
    /// </summary>
    [JsonPropertyName("mailed")]
    public LocalWorkAddr Mailed
    {
      get => mailed ??= new();
      set => mailed = value;
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
    /// A value of EmptyAddr.
    /// </summary>
    [JsonPropertyName("emptyAddr")]
    public CsePersonAddress EmptyAddr
    {
      get => emptyAddr ??= new();
      set => emptyAddr = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of ReisTo.
    /// </summary>
    [JsonPropertyName("reisTo")]
    public PaymentRequest ReisTo
    {
      get => reisTo ??= new();
      set => reisTo = value;
    }

    private Common hiddenProtectKpcRecoup;
    private Common hiddenProtectAddrField;
    private Common duplicateWarrants;
    private PaymentRequest hiddenPaymentRequest;
    private Common createReturnAlertFlag;
    private DateWorkArea fromDate;
    private PaymentRequest passThruFlowPaymentRequest;
    private CsePerson passThruFlowCsePerson;
    private PaymentStatusHistory hiddenReadFromTable;
    private Common displayIndicator;
    private PaymentStatusHistory oldAndNew;
    private PaymentStatus new1;
    private PaymentRequest newReis;
    private WarrantRemailAddress warrantRemailAddress;
    private Common statusPrompt;
    private CsePersonsWorkSet desigPayee;
    private CsePersonsWorkSet payee;
    private PaymentStatus paymentStatus;
    private Common warrantNoPrompt;
    private PaymentRequest paymentRequest;
    private WorkArea nameInMailedAddr;
    private LocalWorkAddr mailed;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePersonAddress emptyAddr;
    private Case1 next;
    private PaymentRequest reisTo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PossiblePrevStatGroup group.</summary>
    [Serializable]
    public class PossiblePrevStatGroup
    {
      /// <summary>
      /// A value of GrDetailPossPrev.
      /// </summary>
      [JsonPropertyName("grDetailPossPrev")]
      public PaymentStatus GrDetailPossPrev
      {
        get => grDetailPossPrev ??= new();
        set => grDetailPossPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PaymentStatus grDetailPossPrev;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public PaymentRequest Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of KpcRecoupIndUpdated.
    /// </summary>
    [JsonPropertyName("kpcRecoupIndUpdated")]
    public Common KpcRecoupIndUpdated
    {
      get => kpcRecoupIndUpdated ??= new();
      set => kpcRecoupIndUpdated = value;
    }

    /// <summary>
    /// A value of NbrOfWarrants.
    /// </summary>
    [JsonPropertyName("nbrOfWarrants")]
    public Common NbrOfWarrants
    {
      get => nbrOfWarrants ??= new();
      set => nbrOfWarrants = value;
    }

    /// <summary>
    /// A value of CrdAmtAdjusted.
    /// </summary>
    [JsonPropertyName("crdAmtAdjusted")]
    public Common CrdAmtAdjusted
    {
      get => crdAmtAdjusted ??= new();
      set => crdAmtAdjusted = value;
    }

    /// <summary>
    /// A value of HardcodeSuspend.
    /// </summary>
    [JsonPropertyName("hardcodeSuspend")]
    public CashReceiptDetailStatus HardcodeSuspend
    {
      get => hardcodeSuspend ??= new();
      set => hardcodeSuspend = value;
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
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public WorkArea Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of AddNewAddress.
    /// </summary>
    [JsonPropertyName("addNewAddress")]
    public Common AddNewAddress
    {
      get => addNewAddress ??= new();
      set => addNewAddress = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of MostRecentPayee.
    /// </summary>
    [JsonPropertyName("mostRecentPayee")]
    public CsePersonAddress MostRecentPayee
    {
      get => mostRecentPayee ??= new();
      set => mostRecentPayee = value;
    }

    /// <summary>
    /// A value of ActiveMailingAddrFound.
    /// </summary>
    [JsonPropertyName("activeMailingAddrFound")]
    public Common ActiveMailingAddrFound
    {
      get => activeMailingAddrFound ??= new();
      set => activeMailingAddrFound = value;
    }

    /// <summary>
    /// A value of UpdateOnlyNarrative.
    /// </summary>
    [JsonPropertyName("updateOnlyNarrative")]
    public Common UpdateOnlyNarrative
    {
      get => updateOnlyNarrative ??= new();
      set => updateOnlyNarrative = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    /// <summary>
    /// A value of HardcodePayee.
    /// </summary>
    [JsonPropertyName("hardcodePayee")]
    public CsePersonAccount HardcodePayee
    {
      get => hardcodePayee ??= new();
      set => hardcodePayee = value;
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
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public WarrantRemailAddress Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of AddrError.
    /// </summary>
    [JsonPropertyName("addrError")]
    public Common AddrError
    {
      get => addrError ??= new();
      set => addrError = value;
    }

    /// <summary>
    /// A value of ReadFromTable.
    /// </summary>
    [JsonPropertyName("readFromTable")]
    public PaymentStatusHistory ReadFromTable
    {
      get => readFromTable ??= new();
      set => readFromTable = value;
    }

    /// <summary>
    /// A value of TempPrev.
    /// </summary>
    [JsonPropertyName("tempPrev")]
    public PaymentStatus TempPrev
    {
      get => tempPrev ??= new();
      set => tempPrev = value;
    }

    /// <summary>
    /// Gets a value of PossiblePrevStat.
    /// </summary>
    [JsonIgnore]
    public Array<PossiblePrevStatGroup> PossiblePrevStat =>
      possiblePrevStat ??= new(PossiblePrevStatGroup.Capacity);

    /// <summary>
    /// Gets a value of PossiblePrevStat for json serialization.
    /// </summary>
    [JsonPropertyName("possiblePrevStat")]
    [Computed]
    public IList<PossiblePrevStatGroup> PossiblePrevStat_Json
    {
      get => possiblePrevStat;
      set => PossiblePrevStat.Assign(value);
    }

    /// <summary>
    /// A value of StatusChangeFlag.
    /// </summary>
    [JsonPropertyName("statusChangeFlag")]
    public Common StatusChangeFlag
    {
      get => statusChangeFlag ??= new();
      set => statusChangeFlag = value;
    }

    /// <summary>
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
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
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of RemailAddressFound.
    /// </summary>
    [JsonPropertyName("remailAddressFound")]
    public Common RemailAddressFound
    {
      get => remailAddressFound ??= new();
      set => remailAddressFound = value;
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
    /// A value of PaymentStatFound.
    /// </summary>
    [JsonPropertyName("paymentStatFound")]
    public Common PaymentStatFound
    {
      get => paymentStatFound ??= new();
      set => paymentStatFound = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private PaymentRequest temp;
    private Common kpcRecoupIndUpdated;
    private Common nbrOfWarrants;
    private Common crdAmtAdjusted;
    private CashReceiptDetailStatus hardcodeSuspend;
    private Common position;
    private Common printProcess;
    private WorkArea print;
    private SpDocLiteral spDocLiteral;
    private Common addNewAddress;
    private CaseRole caseRole;
    private Document document;
    private SpDocKey spDocKey;
    private CsePersonAddress mostRecentPayee;
    private Common activeMailingAddrFound;
    private Common updateOnlyNarrative;
    private DateWorkArea currentDate;
    private DateWorkArea null1;
    private CsePersonAccount hardcodeObligor;
    private CsePersonAccount hardcodePayee;
    private CsePersonAddress csePersonAddress;
    private WarrantRemailAddress initialize;
    private Common addrError;
    private PaymentStatusHistory readFromTable;
    private PaymentStatus tempPrev;
    private Array<PossiblePrevStatGroup> possiblePrevStat;
    private Common statusChangeFlag;
    private WarrantRemailAddress warrantRemailAddress;
    private Common found;
    private DateWorkArea dateWorkArea;
    private Common update;
    private Common remailAddressFound;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common paymentStatFound;
    private Infrastructure infrastructure;
    private TextWorkArea textWorkArea;
    private DateWorkArea maximum;
    private DateWorkArea blank;
    private Program program;
    private InterfaceAlert interfaceAlert;
    private BatchTimestampWorkArea batchTimestampWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public PaymentRequest Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Kpc.
    /// </summary>
    [JsonPropertyName("kpc")]
    public PaymentStatus Kpc
    {
      get => kpc ??= new();
      set => kpc = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
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
    /// A value of NewCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("newCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory NewCashReceiptDetailStatHistory
    {
      get => newCashReceiptDetailStatHistory ??= new();
      set => newCashReceiptDetailStatHistory = value;
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
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Can.
    /// </summary>
    [JsonPropertyName("can")]
    public PaymentStatus Can
    {
      get => can ??= new();
      set => can = value;
    }

    /// <summary>
    /// A value of Reissued.
    /// </summary>
    [JsonPropertyName("reissued")]
    public PaymentRequest Reissued
    {
      get => reissued ??= new();
      set => reissued = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

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
    /// A value of Paid.
    /// </summary>
    [JsonPropertyName("paid")]
    public PaymentStatus Paid
    {
      get => paid ??= new();
      set => paid = value;
    }

    /// <summary>
    /// A value of Doa.
    /// </summary>
    [JsonPropertyName("doa")]
    public PaymentStatus Doa
    {
      get => doa ??= new();
      set => doa = value;
    }

    /// <summary>
    /// A value of Req.
    /// </summary>
    [JsonPropertyName("req")]
    public PaymentStatus Req
    {
      get => req ??= new();
      set => req = value;
    }

    /// <summary>
    /// A value of ExistingPaymentStatus.
    /// </summary>
    [JsonPropertyName("existingPaymentStatus")]
    public PaymentStatus ExistingPaymentStatus
    {
      get => existingPaymentStatus ??= new();
      set => existingPaymentStatus = value;
    }

    /// <summary>
    /// A value of NewPaymentStatus.
    /// </summary>
    [JsonPropertyName("newPaymentStatus")]
    public PaymentStatus NewPaymentStatus
    {
      get => newPaymentStatus ??= new();
      set => newPaymentStatus = value;
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
    /// A value of NewPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("newPaymentStatusHistory")]
    public PaymentStatusHistory NewPaymentStatusHistory
    {
      get => newPaymentStatusHistory ??= new();
      set => newPaymentStatusHistory = value;
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
    /// A value of ReissuedTo.
    /// </summary>
    [JsonPropertyName("reissuedTo")]
    public PaymentRequest ReissuedTo
    {
      get => reissuedTo ??= new();
      set => reissuedTo = value;
    }

    /// <summary>
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of ForReissue.
    /// </summary>
    [JsonPropertyName("forReissue")]
    public PaymentRequest ForReissue
    {
      get => forReissue ??= new();
      set => forReissue = value;
    }

    /// <summary>
    /// A value of ReissuedFrom.
    /// </summary>
    [JsonPropertyName("reissuedFrom")]
    public PaymentRequest ReissuedFrom
    {
      get => reissuedFrom ??= new();
      set => reissuedFrom = value;
    }

    private PaymentRequest temp;
    private PaymentStatus kpc;
    private InterstateRequest interstateRequest;
    private InterstatePaymentAddress interstatePaymentAddress;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailStatHistory newCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailHistory cashReceiptDetailHistory;
    private Program program;
    private PersonProgram personProgram;
    private PaymentStatus can;
    private PaymentRequest reissued;
    private CaseAssignment caseAssignment;
    private ServiceProvider serviceProvider;
    private CaseUnit caseUnit;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePersonAccount csePersonAccount;
    private DisbursementTransactionRln disbursementTransactionRln;
    private ReceiptRefund receiptRefund;
    private DisbursementTransaction disbursement;
    private DisbursementTransaction disbCollection;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private PaymentStatus paymentStatus;
    private PaymentStatus paid;
    private PaymentStatus doa;
    private PaymentStatus req;
    private PaymentStatus existingPaymentStatus;
    private PaymentStatus newPaymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatusHistory newPaymentStatusHistory;
    private PaymentRequest paymentRequest;
    private PaymentRequest reissuedTo;
    private WarrantRemailAddress warrantRemailAddress;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private PaymentRequest forReissue;
    private PaymentRequest reissuedFrom;
  }
#endregion
}

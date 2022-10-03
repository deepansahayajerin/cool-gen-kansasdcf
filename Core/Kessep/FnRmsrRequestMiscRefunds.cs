// Program: FN_RMSR_REQUEST_MISC_REFUNDS, ID: 372311862, model: 746.
// Short name: SWERMSRP
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
/// A program: FN_RMSR_REQUEST_MISC_REFUNDS.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRmsrRequestMiscRefunds: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RMSR_REQUEST_MISC_REFUNDS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRmsrRequestMiscRefunds(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRmsrRequestMiscRefunds.
  /// </summary>
  public FnRmsrRequestMiscRefunds(IContext context, Import import, Export export)
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
    // testing update
    // ------------------------------------------------------------------------
    // Date	  Developer Name	Description
    // 01/29/96  Holly Kennedy-MTW	Source
    // 02/08/96  Holly Kennedy-MTW	Retrofits/
    // 04/15/96  Holly Kennedy-MTW	Added screen changes required for signoff.
    // 01/04/97  R. Marchman		Add new security next tran.
    // 02/07/97  R. Welborn         	Numerous fixes.
    // 05/20/97  Sumanta - MTW		Fixed the flow between ORGZ and RMSR
    //      				Fixed other testing problems
    // 06/18/97  T.O.Redmond		Add Tribunal Name.
    // 03/10/99  Sunya Sharp		Make changes per screen assessement form approved 
    // by the SME.
    // 05/13/99  Sunya Sharp		Do not allow refunds for certain source codes.  
    // When creating a refund do not allow creation if the source code does not
    // have a organization number.
    // 10/18/99	Sunya Sharp		H00077672 - Do not allow refund amount to be 
    // negative.
    // 12/20/99	Sunya Sharp		PR# 82930 - Change next tran to not use person 
    // number passed from other screens.
    // 01/20/00	Sunya Sharp		PR# 85053 - Missing address for source code and 
    // organization is preventing multiple adds.  Changing logic to be similiar
    // to the person logic.
    // 08/11/00         P. Phinney             H00101298  -  Prevent UN-
    // Authorized persons
    // from viewing Addresses
    // 11/26/01         Kalpesh Doshi          WR020147 - KPC Recoupment.
    // 06/22/12	 GVandy		 CQ33628 - Do not allow refunds to KSDLUI source type
    // 08/12/14	 GVandy		 CQ42192 - Do not allow refunds to CSSI source type.
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    // *** Add exit state for successful clear.  Sunya Sharp 3/10/1999 ***
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // ************************************************
    // *Move imports to exports                       *
    // ************************************************
    MoveStandard(import.Standard, export.Standard);
    export.Set.Assign(import.Set);
    export.PaymentStatus.Code = import.PaymentStatus.Code;
    export.ReceiptRefund.Assign(import.ReceiptRefund);
    MoveCashReceiptSourceType(import.CashReceiptSourceType,
      export.CashReceiptSourceType);
    export.Current.Assign(import.Current);
    export.SendTo.Assign(import.SendTo);
    export.DisplayComplete.Flag = import.DisplayComplete.Flag;
    MoveCashReceiptSourceType(import.PreviousCashReceiptSourceType,
      export.PreviousCashReceiptSourceType);
    export.PreviousReceiptRefund.Assign(import.PreviousReceiptRefund);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    MoveCsePersonsWorkSet(import.PreviousCsePersonsWorkSet,
      export.PreviousCsePersonsWorkSet);
    export.RefundReason.PromptField = import.RefundReason.PromptField;
    export.Selected.Assign(import.Selected);
    export.Source.PromptField = import.Source.PromptField;

    // H00101298  HERE to save and display
    export.CurrentHidden.Assign(import.CurrentHidden);
    export.SendToHidden.Assign(import.SendToHidden);
    local.AddFlag.Flag = "";
    local.UpdateFlag.Flag = "";

    if (!IsEmpty(import.FromFlowCsePersonsWorkSet.Number))
    {
      MoveCsePersonsWorkSet(import.FromFlowCsePersonsWorkSet,
        export.CsePersonsWorkSet);
    }

    local.LeftPadCsePerson.Text10 = export.CsePersonsWorkSet.Number;
    UseEabPadLeftWithZeros();
    export.CsePersonsWorkSet.Number = local.LeftPadCsePerson.Text10;

    if (Equal(global.Command, "PRMPTRET"))
    {
      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        global.Command = "DISPLAY";
      }
      else if (IsEmpty(import.CsePersonsWorkSet.Number) && !
        IsEmpty(import.PreviousCsePersonsWorkSet.Number))
      {
        MoveCsePersonsWorkSet(export.PreviousCsePersonsWorkSet,
          export.CsePersonsWorkSet);
        global.Command = "DISPLAY";
      }
      else
      {
        export.ReceiptRefund.PayeeName = "";
        export.ReceiptRefund.Taxid = "";
        export.ReceiptRefund.RequestDate = local.NullDateWorkArea.Date;
        export.SendTo.Street1 = "";
        export.SendTo.Street2 = "";
        export.SendTo.City = "";
        export.SendTo.State = "";
        export.SendTo.ZipCode5 = "";
        export.SendTo.ZipCode4 = "";
        export.SendTo.ZipCode3 = "";

        // H00101298  HERE to save and display
        // This is actually INITIALIZING the fields to spaces
        export.SendToHidden.SystemGeneratedIdentifier =
          export.SendTo.SystemGeneratedIdentifier;
        export.SendToHidden.City = export.SendTo.City;
        export.SendToHidden.State = export.SendTo.State;
        export.SendToHidden.Street1 = export.SendTo.Street1;
        export.SendToHidden.Street2 = export.SendTo.Street2 ?? "";
        export.SendToHidden.ZipCode3 = export.SendTo.ZipCode3 ?? "";
        export.SendToHidden.ZipCode4 = export.SendTo.ZipCode4 ?? "";
        export.SendToHidden.ZipCode5 = export.SendTo.ZipCode5;

        var field1 = GetField(export.CashReceiptSourceType, "code");

        field1.Error = true;

        var field2 = GetField(export.CsePersonsWorkSet, "number");

        field2.Error = true;

        ExitState = "FN0000_CSE_PRSN_OR_SOUR_CD_REQD";

        return;
      }
    }

    // *** Add logic to evaluate if information was passed from CRSL before 
    // returning to the screen.  Sunya Sharp 3/12/1999 ***
    if (Equal(global.Command, "RETCRSL"))
    {
      if (!IsEmpty(import.FromFlowCashReceiptSourceType.Code))
      {
        MoveCashReceiptSourceType(import.FromFlowCashReceiptSourceType,
          export.CashReceiptSourceType);
        global.Command = "DISPLAY";
      }
      else if (IsEmpty(export.CashReceiptSourceType.Code) && IsEmpty
        (export.CsePersonsWorkSet.Number))
      {
        var field1 = GetField(export.CashReceiptSourceType, "code");

        field1.Error = true;

        var field2 = GetField(export.CsePersonsWorkSet, "number");

        field2.Error = true;

        ExitState = "FN0000_CSE_PRSN_OR_SOUR_CD_REQD";

        return;
      }
      else
      {
        return;
      }
    }

    // *** Add logic to check value before setting export value.  Sunya Sharp 3/
    // 12/1999 ***
    if (Equal(global.Command, "RETDISP"))
    {
      if (!IsEmpty(import.Return1.Cdvalue))
      {
        export.ReceiptRefund.ReasonCode = import.Return1.Cdvalue;
      }
    }

    if (Equal(global.Command, "FROMTRIB"))
    {
      if (export.Selected.Identifier != 0)
      {
        // ************************************************
        // *Default Tribunal Address                      *
        // ************************************************
        if (ReadTribunal2())
        {
          export.ReceiptRefund.PayeeName = entities.ExistingTribunal.Name;
        }
        else
        {
          export.ReceiptRefund.PayeeName = "\"Invalid Tribunal\"";
          ExitState = "INVALID_TRIBUNAL";

          return;
        }

        UseCabFnGetTribunalAddress2();
        export.SendTo.Street1 = local.FipsTribAddress.Street1;
        export.SendTo.Street2 = local.FipsTribAddress.Street2 ?? "";
        export.SendTo.City = local.FipsTribAddress.City;
        export.SendTo.State = local.FipsTribAddress.State;
        export.SendTo.ZipCode5 = local.FipsTribAddress.ZipCode;
        export.SendTo.ZipCode4 = local.FipsTribAddress.Zip4 ?? "";
        export.SendTo.ZipCode3 = local.FipsTribAddress.Zip3 ?? "";

        // H00101298  HERE to save and display
        // * * SAVE current values
        export.SendToHidden.SystemGeneratedIdentifier =
          export.SendTo.SystemGeneratedIdentifier;
        export.SendToHidden.City = export.SendTo.City;
        export.SendToHidden.State = export.SendTo.State;
        export.SendToHidden.Street1 = export.SendTo.Street1;
        export.SendToHidden.Street2 = export.SendTo.Street2 ?? "";
        export.SendToHidden.ZipCode3 = export.SendTo.ZipCode3 ?? "";
        export.SendToHidden.ZipCode4 = export.SendTo.ZipCode4 ?? "";
        export.SendToHidden.ZipCode5 = export.SendTo.ZipCode5;
        global.Command = "RESEARCH";

        // * * Check SECURITY - Block accordingly
        UseScCabTestSecurity1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          export.SendTo.City = "";
          export.SendTo.State = "";
          export.SendTo.Street1 = "Security Block on Address";
          export.SendTo.Street2 = "";
          export.SendTo.ZipCode3 = "";
          export.SendTo.ZipCode4 = "";
          export.SendTo.ZipCode5 = "";
          ExitState = "ACO_NN0000_ALL_OK";
        }

        global.Command = "FROMTRIB";

        // ***--- Sumanta - 05/20/97
        //        Wipe out any existing data in the
        //        cse_person/source_cd field
        //        Do not blank out the source code fields.  Sunya Sharp 3/10/
        // 1999
        // ***---
        export.ReceiptRefund.Taxid = import.Selected.TaxId ?? "";
        export.ReceiptRefund.TaxIdSuffix = import.Selected.TaxIdSuffix ?? "";

        // ??????????????????????????????????????????????????????
        // Notice that the Receipt Refund does not have a tax id
        // Suffix whereas the Tribunal does.
        // Possible Model Change Required. 6/19/97 TOR
        // Field was added to the receipt refund.  Value is now set.  Sunya 
        // Sharp 3/10/1999
        // ??????????????????????????????????????????????????????
        export.CsePersonsWorkSet.Number = "";
        export.CsePersonsWorkSet.FormattedName = "";
      }

      return;
    }

    if (Equal(global.Command, "RETCRRL"))
    {
      if (Lt(local.NullReceiptRefund.CreatedTimestamp,
        import.FromFlowReceiptRefund.CreatedTimestamp))
      {
        export.ReceiptRefund.CreatedTimestamp =
          import.FromFlowReceiptRefund.CreatedTimestamp;
        global.Command = "DISPLAY";
      }
      else if (IsEmpty(export.CashReceiptSourceType.Code) && IsEmpty
        (export.CsePersonsWorkSet.Number))
      {
        var field1 = GetField(export.CashReceiptSourceType, "code");

        field1.Error = true;

        var field2 = GetField(export.CsePersonsWorkSet, "number");

        field2.Error = true;

        ExitState = "FN0000_CSE_PRSN_OR_SOUR_CD_REQD";

        return;
      }
      else
      {
        return;
      }
    }

    // *****
    // Next Tran/Security logic
    // *****
    MoveStandard(import.Standard, export.Standard);

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
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

      // *** PR# 82930 - Do not use the person number when next tranning to this
      // screen.  Change logic to just display message to enter refund
      // information.  Users were not paying attention and incorrect information
      // was being used in creating the refund.  Sunya Sharp 12/20/1999 ***
      ExitState = "FN0000_ENTER_REFUND_INFORMATION";

      return;
    }

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

      if (ReadCsePerson())
      {
        if (AsChar(entities.ExistingCsePerson.Type1) == 'C')
        {
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.CsePersonsWorkSet.FormattedName =
              "**** ADABAS UNAVAILABLE ****";
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            export.CsePersonsWorkSet.FormattedName =
              local.CsePersonsWorkSet.FormattedName;
            export.ReceiptRefund.PayeeName =
              local.CsePersonsWorkSet.FormattedName;
            export.ReceiptRefund.Taxid = local.CsePersonsWorkSet.Ssn;
          }
        }
        else
        {
          export.CsePersonsWorkSet.FormattedName =
            entities.ExistingCsePerson.OrganizationName ?? Spaces(33);
          export.ReceiptRefund.PayeeName =
            entities.ExistingCsePerson.OrganizationName;
          export.ReceiptRefund.Taxid = entities.ExistingCsePerson.TaxId;
        }
      }
      else
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    // H0101298 - added bypass for "RESEARCH" as it is only checked for DISPLAY 
    // purposes
    if (Equal(global.Command, "PRMPTRET") || Equal
      (global.Command, "RETDISP") || Equal(global.Command, "LTRB") || Equal
      (global.Command, "RESEARCH"))
    {
    }
    else
    {
      UseScCabTestSecurity2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // ************************************************
    // *Validate common required fields for commands  *
    // ************************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // When no data is displayed for an add - highlight all enterable fields 
      // and display error message.  When no data is displayed for an update -
      // highlight the person number and source code and display error message.
      // Sunya Sharp 3/15/1999 ***
      switch(TrimEnd(global.Command))
      {
        case "ADD":
          if (export.ReceiptRefund.Amount == 0 && IsEmpty
            (export.ReceiptRefund.ReasonCode) && IsEmpty
            (export.CsePersonsWorkSet.Number) && IsEmpty
            (export.CashReceiptSourceType.Code) && IsEmpty
            (export.ReceiptRefund.ReasonText))
          {
            var field1 = GetField(export.ReceiptRefund, "amount");

            field1.Error = true;

            var field2 = GetField(export.ReceiptRefund, "reasonCode");

            field2.Error = true;

            var field3 = GetField(export.CashReceiptSourceType, "code");

            field3.Error = true;

            var field4 = GetField(export.CsePersonsWorkSet, "number");

            field4.Error = true;

            var field5 = GetField(export.ReceiptRefund, "reasonText");

            field5.Error = true;

            ExitState = "FN0000_MANDATORY_FIELDS";

            return;
          }

          break;
        case "UPDATE":
          // ************************************************
          // *Force user to display first.                  *
          // ************************************************
          if (AsChar(export.DisplayComplete.Flag) != 'Y')
          {
            var field1 = GetField(export.CashReceiptSourceType, "code");

            field1.Error = true;

            var field2 = GetField(export.CsePersonsWorkSet, "number");

            field2.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

            return;
          }

          break;
        default:
          break;
      }

      // ************************************************
      // *Validate that both Source and CSE Person are  *
      // *entered.
      // 
      // *
      // ************************************************
      // *** Both person number and source code are not required.  User must 
      // enter one or the other, not both.  Sunya Sharp 3/15/1999 ***
      if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
        (export.CashReceiptSourceType.Code))
      {
        var field1 = GetField(export.CashReceiptSourceType, "code");

        field1.Error = true;

        var field2 = GetField(export.CsePersonsWorkSet, "number");

        field2.Error = true;

        export.CsePersonsWorkSet.FormattedName = "";
        ExitState = "FN0000_MANDATORY_FIELDS";

        return;
      }

      if (!IsEmpty(export.CsePersonsWorkSet.Number) && !
        IsEmpty(export.CashReceiptSourceType.Code))
      {
        var field1 = GetField(export.CashReceiptSourceType, "code");

        field1.Error = true;

        var field2 = GetField(export.CsePersonsWorkSet, "number");

        field2.Error = true;

        export.CsePersonsWorkSet.FormattedName = "";
        ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

        return;
      }

      // ************************************************
      // *Validate entered Source type                  *
      // ************************************************
      if (!IsEmpty(export.CashReceiptSourceType.Code))
      {
        if (ReadCashReceiptSourceType())
        {
          MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
            export.CashReceiptSourceType);
          local.CashReceiptSourceType2.Assign(
            entities.ExistingCashReceiptSourceType);

          // *** Add logic to not allow refunds to be created for the source 
          // codes of FDSO, SDSO, MISC, and EMP.  Sunya Sharp 5/13/1999 ***
          // 06/22/12  GVandy  CQ33628  Do not allow refunds to KSDLUI source 
          // type.
          // 08/12/12  GVandy  CQ42192  Do not allow refunds to CSSI source 
          // type.
          if (Equal(entities.ExistingCashReceiptSourceType.Code, "SDSO") || Equal
            (entities.ExistingCashReceiptSourceType.Code, "FDSO") || Equal
            (entities.ExistingCashReceiptSourceType.Code, "MISC") || Equal
            (entities.ExistingCashReceiptSourceType.Code, "EMP") || Equal
            (entities.ExistingCashReceiptSourceType.Code, "KSDLUI") || Equal
            (entities.ExistingCashReceiptSourceType.Code, "CSSI"))
          {
            var field = GetField(export.CashReceiptSourceType, "code");

            field.Error = true;

            ExitState = "FN0000_REFUND_NOT_ALLOW_FOR_SRC";

            return;
          }
        }
        else
        {
          var field = GetField(export.CashReceiptSourceType, "code");

          field.Error = true;

          ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

          return;
        }
      }

      // ************************************************
      // *Default field values                          *
      // ************************************************
      // *** PR#85053 - Add logic for retrieving a blank address for source and 
      // organization to match the logic that is there for person.  Sunya Sharp
      // 01/20/2000 ***
      if (Equal(global.Command, "ADD"))
      {
        export.ReceiptRefund.RequestDate = Now().Date;

        if ((IsEmpty(export.SendTo.Street1) || Equal
          (export.SendTo.Street1, "Security Block on Address")) && IsEmpty
          (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
          (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
        {
          if (!IsEmpty(export.CsePersonsWorkSet.Number))
          {
            local.CsePerson.Number = export.CsePersonsWorkSet.Number;

            // H00090532    03/14/00   pdp
            UseSiGetCsePersonMailingAddr();
            export.SendTo.Street1 = local.CsePersonAddress.Street1 ?? Spaces
              (25);
            export.SendTo.Street2 = local.CsePersonAddress.Street2 ?? "";
            export.SendTo.City = local.CsePersonAddress.City ?? Spaces(30);
            export.SendTo.State = local.CsePersonAddress.State ?? Spaces(2);
            export.SendTo.ZipCode5 = local.CsePersonAddress.ZipCode ?? Spaces
              (5);
            export.SendTo.ZipCode4 = local.CsePersonAddress.Zip4 ?? "";
            export.SendTo.ZipCode3 = local.CsePersonAddress.Zip3 ?? "";

            if (IsEmpty(export.SendTo.Street1) && IsEmpty
              (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
              (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
            {
              if (AsChar(entities.ExistingCsePerson.Type1) == 'O')
              {
                if (ReadFipsTribAddress())
                {
                  ExitState = "ACO_NN0000_ALL_OK";
                  export.SendTo.Street1 =
                    entities.ExistingFipsTribAddress.Street1;
                  export.SendTo.Street2 =
                    entities.ExistingFipsTribAddress.Street2;
                  export.SendTo.City = entities.ExistingFipsTribAddress.City;
                  export.SendTo.State = entities.ExistingFipsTribAddress.State;
                  export.SendTo.ZipCode5 =
                    entities.ExistingFipsTribAddress.ZipCode;
                  export.SendTo.ZipCode4 =
                    entities.ExistingFipsTribAddress.Zip4;
                  export.SendTo.ZipCode3 =
                    entities.ExistingFipsTribAddress.Zip3;
                }
              }
            }
          }
          else if (!IsEmpty(export.CashReceiptSourceType.Code))
          {
            if (ReadTribunal1())
            {
              export.ReceiptRefund.PayeeName = entities.ExistingTribunal.Name;
              export.ReceiptRefund.Taxid = entities.ExistingTribunal.TaxId;
              export.ReceiptRefund.TaxIdSuffix =
                entities.ExistingTribunal.TaxIdSuffix;
            }

            UseCabFnGetTribunalAddress1();
            export.SendTo.Street1 = local.FipsTribAddress.Street1;
            export.SendTo.Street2 = local.FipsTribAddress.Street2 ?? "";
            export.SendTo.City = local.FipsTribAddress.City;
            export.SendTo.State = local.FipsTribAddress.State;
            export.SendTo.ZipCode5 = local.FipsTribAddress.ZipCode;
            export.SendTo.ZipCode4 = local.FipsTribAddress.Zip4 ?? "";
            export.SendTo.ZipCode3 = local.FipsTribAddress.Zip3 ?? "";
          }
        }
      }

      // ************************************************
      // *Validate Required Fields                      *
      // ************************************************
      // *** Removed logic to require the Tax ID/SSN to be populated.  This was 
      // per user request.  Sunya Sharp 3/18/1999 ***
      // *** Add logic to prevent the refund amount from being negative.  HEAT 
      // H00077672.  Sunya Sharp 10/18/1999. ***
      if (IsEmpty(export.ReceiptRefund.PayeeName))
      {
        var field = GetField(export.ReceiptRefund, "payeeName");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (export.ReceiptRefund.Amount == 0)
      {
        var field = GetField(export.ReceiptRefund, "amount");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (export.ReceiptRefund.Amount < 0)
      {
        var field = GetField(export.ReceiptRefund, "amount");

        field.Error = true;

        ExitState = "FN0000_AMT_CANNOT_BE_NEGATIVE";
      }

      if (IsEmpty(export.ReceiptRefund.ReasonCode))
      {
        var field = GetField(export.ReceiptRefund, "reasonCode");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      // H00101298  HERE to Re-Load original values if ADDRESS was previously 
      // Blocked - this will probably ONLY occur on a UPDATE
      if (Equal(export.SendTo.Street1, "Security Block on Address"))
      {
        export.SendTo.SystemGeneratedIdentifier =
          export.SendToHidden.SystemGeneratedIdentifier;
        export.SendTo.City = export.SendToHidden.City;
        export.SendTo.State = export.SendToHidden.State;
        export.SendTo.Street1 = export.SendToHidden.Street1;
        export.SendTo.Street2 = export.SendToHidden.Street2 ?? "";
        export.SendTo.ZipCode3 = export.SendToHidden.ZipCode3 ?? "";
        export.SendTo.ZipCode4 = export.SendToHidden.ZipCode4 ?? "";
        export.SendTo.ZipCode5 = export.SendToHidden.ZipCode5;
      }

      if (Equal(global.Command, "ADD"))
      {
        if (IsEmpty(export.SendTo.Street1) && IsEmpty(export.SendTo.Street2))
        {
          var field1 = GetField(export.SendTo, "street1");

          field1.Error = true;

          var field2 = GetField(export.SendTo, "street2");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.SendTo.City))
        {
          var field = GetField(export.SendTo, "city");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.SendTo.State))
        {
          var field = GetField(export.SendTo, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.SendTo.ZipCode5))
        {
          var field = GetField(export.SendTo, "zipCode5");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.ReceiptRefund.ReasonText))
        {
          var field = GetField(export.ReceiptRefund, "reasonText");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }
      else if (!IsEmpty(export.SendTo.Street1) || !
        IsEmpty(export.SendTo.Street2) || !IsEmpty(export.SendTo.City) || !
        IsEmpty(export.SendTo.State) || !IsEmpty(export.SendTo.ZipCode5))
      {
        if (IsEmpty(export.SendTo.Street1) && IsEmpty(export.SendTo.Street2))
        {
          var field1 = GetField(export.SendTo, "street1");

          field1.Error = true;

          var field2 = GetField(export.SendTo, "street2");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.SendTo.City))
        {
          var field = GetField(export.SendTo, "city");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(import.SendTo.State))
        {
          var field = GetField(export.SendTo, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.SendTo.ZipCode5))
        {
          var field = GetField(export.SendTo, "zipCode5");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ************************************************
      // *Validate code values                          *
      // ************************************************
      if (!IsEmpty(export.SendTo.Street1) || !
        IsEmpty(export.SendTo.Street2) || !IsEmpty(export.SendTo.City) || !
        IsEmpty(export.SendTo.State) || !IsEmpty(export.SendTo.ZipCode5))
      {
        // ************************************************
        // *If Address is entered validate state          *
        // ************************************************
        local.PassCode.CodeName = "STATE CODE";
        local.PassCodeValue.Cdvalue = export.SendTo.State;
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field = GetField(export.SendTo, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";

          return;
        }

        // ************************************************
        // *Validate that entered Zip Codes are numeric.  *
        // ************************************************
        if (Verify(export.SendTo.ZipCode5, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode5");

          field.Error = true;

          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
        }

        if (Verify(export.SendTo.ZipCode4, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode4");

          field.Error = true;

          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
        }

        if (Verify(export.SendTo.ZipCode3, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode3");

          field.Error = true;

          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
        }

        if (IsExitState("ACO_NE0000_ZIP_CODE_NOT_NUMERIC"))
        {
          return;
        }

        // ************************************************
        // *Validate the Reason Code against the Codes    *
        // *table.
        // 
        // *
        // ************************************************
        local.PassCode.CodeName = "REFUND REASON";
        local.PassCodeValue.Cdvalue = export.ReceiptRefund.ReasonCode;
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field = GetField(export.ReceiptRefund, "reasonCode");

          field.Error = true;

          ExitState = "FN0000_ENTER_VALID_REFUND_REASON";

          return;
        }
      }
    }

    // *****
    // Main Case of Command
    // *****
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (Equal(export.ReceiptRefund.CreatedTimestamp, null))
        {
          if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
            (export.CashReceiptSourceType.Code))
          {
            var field4 = GetField(export.CashReceiptSourceType, "code");

            field4.Error = true;

            var field5 = GetField(export.CsePersonsWorkSet, "number");

            field5.Error = true;

            ExitState = "FN0000_CSE_PRSN_OR_SOUR_CD_REQD";

            return;
          }

          if (!IsEmpty(export.CsePersonsWorkSet.Number))
          {
            // ---- User provided cse person number
            if (ReadCsePerson())
            {
              if (AsChar(entities.ExistingCsePerson.Type1) == 'C')
              {
                export.ReceiptRefund.PayeeName =
                  local.CsePersonsWorkSet.FormattedName;
                export.ReceiptRefund.Taxid = local.CsePersonsWorkSet.Ssn;
              }
              else
              {
                export.ReceiptRefund.PayeeName =
                  entities.ExistingCsePerson.OrganizationName;
                export.ReceiptRefund.Taxid = entities.ExistingCsePerson.TaxId;
              }

              export.PreviousCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
            }
            else
            {
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";

              return;
            }

            // --- Pick the existing address for the cse_person
            local.CsePerson.Number = export.CsePersonsWorkSet.Number;
            UseSiGetCsePersonMailingAddr();
            ExitState = "ACO_NN0000_ALL_OK";

            // ***--- Sumanta - MTW - 05/20/97
            //        The following IF stmt has been added ..
            // ***---
            if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
              (local.CsePersonAddress.Street2) && IsEmpty
              (local.CsePersonAddress.City) && IsEmpty
              (local.CsePersonAddress.State))
            {
              // Trying to determine if the CSE Person is an Organization.
              if (AsChar(entities.ExistingCsePerson.Type1) == 'O')
              {
                if (ReadFipsTribAddress())
                {
                  export.SendTo.Street1 =
                    entities.ExistingFipsTribAddress.Street1;
                  export.SendTo.Street2 =
                    entities.ExistingFipsTribAddress.Street2;
                  export.SendTo.City = entities.ExistingFipsTribAddress.City;
                  export.SendTo.State = entities.ExistingFipsTribAddress.State;
                  export.SendTo.ZipCode5 =
                    entities.ExistingFipsTribAddress.ZipCode;
                  export.SendTo.ZipCode4 =
                    entities.ExistingFipsTribAddress.Zip4;
                  export.SendTo.ZipCode3 =
                    entities.ExistingFipsTribAddress.Zip3;
                }
              }
            }
            else
            {
              export.SendTo.Street1 = local.CsePersonAddress.Street1 ?? Spaces
                (25);
              export.SendTo.Street2 = local.CsePersonAddress.Street2 ?? "";
              export.SendTo.City = local.CsePersonAddress.City ?? Spaces(30);
              export.SendTo.State = local.CsePersonAddress.State ?? Spaces(2);
              export.SendTo.ZipCode5 = local.CsePersonAddress.ZipCode ?? Spaces
                (5);
              export.SendTo.ZipCode4 = local.CsePersonAddress.Zip4 ?? "";
              export.SendTo.ZipCode3 = local.CsePersonAddress.Zip3 ?? "";
            }

            local.NoOfRefundTrans.Count = 0;

            foreach(var item in ReadReceiptRefundPaymentRequest1())
            {
              if (ReadCashReceiptCashReceiptDetail())
              {
                continue;
              }
              else
              {
                ++local.NoOfRefundTrans.Count;
                MoveReceiptRefund2(entities.ExistingReceiptRefund, local.OnlyOne);
                  
              }
            }
          }
          else
          {
            // ---- User  provided source code
            if (ReadCashReceiptSourceType())
            {
              MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
                export.CashReceiptSourceType);
              local.CashReceiptSourceType2.Assign(
                entities.ExistingCashReceiptSourceType);
            }
            else
            {
              var field = GetField(export.CashReceiptSourceType, "code");

              field.Error = true;

              ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

              return;
            }

            // *** Added logic to default the address, tax id and payee name for
            // the source code.  Sunya Sharp 3/11/1999 ***
            if (ReadTribunal1())
            {
              export.ReceiptRefund.PayeeName = entities.ExistingTribunal.Name;
              export.ReceiptRefund.Taxid = entities.ExistingTribunal.TaxId;
              export.ReceiptRefund.TaxIdSuffix =
                entities.ExistingTribunal.TaxIdSuffix;
            }

            UseCabFnGetTribunalAddress1();
            export.SendTo.Street1 = local.FipsTribAddress.Street1;
            export.SendTo.Street2 = local.FipsTribAddress.Street2 ?? "";
            export.SendTo.City = local.FipsTribAddress.City;
            export.SendTo.State = local.FipsTribAddress.State;
            export.SendTo.ZipCode5 = local.FipsTribAddress.ZipCode;
            export.SendTo.ZipCode4 = local.FipsTribAddress.Zip4 ?? "";
            export.SendTo.ZipCode3 = local.FipsTribAddress.Zip3 ?? "";
            local.NoOfRefundTrans.Count = 0;

            foreach(var item in ReadReceiptRefundPaymentRequest2())
            {
              if (ReadCashReceiptCashReceiptDetail())
              {
                continue;
              }
              else
              {
                ++local.NoOfRefundTrans.Count;
                MoveReceiptRefund2(entities.ExistingReceiptRefund, local.OnlyOne);
                  
              }
            }
          }

          // H00101298  HERE to save and display
          export.SendToHidden.SystemGeneratedIdentifier =
            export.SendTo.SystemGeneratedIdentifier;
          export.SendToHidden.City = export.SendTo.City;
          export.SendToHidden.State = export.SendTo.State;
          export.SendToHidden.Street1 = export.SendTo.Street1;
          export.SendToHidden.Street2 = export.SendTo.Street2 ?? "";
          export.SendToHidden.ZipCode3 = export.SendTo.ZipCode3 ?? "";
          export.SendToHidden.ZipCode4 = export.SendTo.ZipCode4 ?? "";
          export.SendToHidden.ZipCode5 = export.SendTo.ZipCode5;

          // WITH Security Check
          global.Command = "RESEARCH";
          UseScCabTestSecurity1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            export.SendTo.City = "";
            export.SendTo.State = "";
            export.SendTo.Street1 = "Security Block on Address";
            export.SendTo.Street2 = "";
            export.SendTo.ZipCode3 = "";
            export.SendTo.ZipCode4 = "";
            export.SendTo.ZipCode5 = "";
            ExitState = "ACO_NN0000_ALL_OK";
          }

          global.Command = "DISPLAY";

          switch(local.NoOfRefundTrans.Count)
          {
            case 1:
              if (!Equal(export.PreviousReceiptRefund.CreatedTimestamp,
                export.ReceiptRefund.CreatedTimestamp))
              {
                export.PreviousReceiptRefund.Assign(export.ReceiptRefund);
              }

              export.ReceiptRefund.Assign(local.OnlyOne);

              break;
            case 0:
              if (!IsEmpty(export.CsePersonsWorkSet.Number))
              {
                ExitState = "FN0000_REFUND_4_THE_CSE_PRSN_NF";
              }
              else
              {
                ExitState = "FN0000_REFUND_4_SOURCE_CODE_NF";
              }

              return;
            default:
              if (!IsEmpty(export.CsePersonsWorkSet.Number))
              {
                ExitState = "FN0000_MULT_REFUNDS_EXIST";
              }
              else
              {
                ExitState = "FN0000_MULT_REFUND_EXIST_SOURCE";
              }

              return;
          }
        }

        break;
      case "LIST":
        // ************************************************
        // *Flow to List Refunds and Name screen.         *
        // *If a selection is made flow to Name screen if *
        // *not flow to the refund screen.                *
        // ************************************************
        // *** Added logic to highlight fields and display error message when no
        // prompt fields have an "S" and PF4 is pressed.  Added logic to handle
        // multiple prompts selected.  Also when invalid prompt was in the
        // reason prompt the wrong field was highlighted in error.  Changed the
        // make statement.  Sunya Sharp 3/15/1999 ***
        switch(AsChar(export.Standard.PromptField))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            if (AsChar(export.RefundReason.PromptField) == 'S')
            {
              var field4 = GetField(export.RefundReason, "promptField");

              field4.Error = true;

              var field5 = GetField(export.Standard, "promptField");

              field5.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.Source.PromptField) == 'S')
            {
              var field4 = GetField(export.Source, "promptField");

              field4.Error = true;

              var field5 = GetField(export.Standard, "promptField");

              field5.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.Standard.PromptField = "";
            export.PassToName.Percentage = 35;
            export.PassToName.Flag = "Y";
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          default:
            var field = GetField(export.Standard, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.RefundReason.PromptField))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            if (AsChar(export.Standard.PromptField) == 'S')
            {
              var field4 = GetField(export.Standard, "promptField");

              field4.Error = true;

              var field5 = GetField(export.RefundReason, "promptField");

              field5.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.Source.PromptField) == 'S')
            {
              var field4 = GetField(export.Source, "promptField");

              field4.Error = true;

              var field5 = GetField(export.RefundReason, "promptField");

              field5.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.RefundReason.PromptField = "";
            export.CodeName.CodeName = "REFUND REASON";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          default:
            var field = GetField(export.RefundReason, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.Source.PromptField))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            if (AsChar(export.Standard.PromptField) == 'S')
            {
              var field4 = GetField(export.Standard, "promptField");

              field4.Error = true;

              var field5 = GetField(export.Source, "promptField");

              field5.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.RefundReason.PromptField) == 'S')
            {
              var field4 = GetField(export.RefundReason, "promptField");

              field4.Error = true;

              var field5 = GetField(export.Source, "promptField");

              field5.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.Source.PromptField = "";
            ExitState = "ECO_LNK_LST_CASH_SOURCES";

            return;
          default:
            var field = GetField(export.Source, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        var field1 = GetField(export.RefundReason, "promptField");

        field1.Error = true;

        var field2 = GetField(export.Standard, "promptField");

        field2.Error = true;

        var field3 = GetField(export.Source, "promptField");

        field3.Error = true;

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        export.DisplayAll.Flag = "Y";

        return;
      case "PRMPTRET":
        break;
      case "RETDISP":
        break;
      case "ADD":
        // ************************************************
        // *Validate date to be current or greater        *
        // ************************************************
        if (Lt(export.ReceiptRefund.RequestDate, Now().Date))
        {
          var field = GetField(export.ReceiptRefund, "requestDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

          return;
        }

        if (export.ReceiptRefund.Amount == export
          .PreviousReceiptRefund.Amount && Equal
          (export.ReceiptRefund.PayeeName,
          export.PreviousReceiptRefund.PayeeName) && Equal
          (export.ReceiptRefund.ReasonCode,
          export.PreviousReceiptRefund.ReasonCode) && Equal
          (export.ReceiptRefund.RequestDate,
          export.PreviousReceiptRefund.RequestDate))
        {
          ExitState = "FN0000_CANNOT_ADD_SAME_RECEIPT";

          return;
        }

        // ************************************************
        // *Add the Refund
        // 
        // *
        // ************************************************
        export.Set.Amount = export.ReceiptRefund.Amount;
        export.Set.Classification = "REF";
        export.Set.RecoupmentIndKpc = "";
        export.Set.CsePersonNumber = export.CsePersonsWorkSet.Number;
        export.Set.Type1 = "WAR";
        UseFnAbCreateMiscRefund();

        if (IsExitState("FN0000_NO_ADD_REFUND_NO_ORG_NUMB"))
        {
          // WITH Security Check
          global.Command = "RESEARCH";
          UseScCabTestSecurity1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            export.SendTo.City = "";
            export.SendTo.State = "";
            export.SendTo.Street1 = "Security Block on Address";
            export.SendTo.Street2 = "";
            export.SendTo.ZipCode3 = "";
            export.SendTo.ZipCode4 = "";
            export.SendTo.ZipCode5 = "";
            ExitState = "FN0000_NO_ADD_REFUND_NO_ORG_NUMB";
          }

          global.Command = "ADD";

          var field = GetField(export.CashReceiptSourceType, "code");

          field.Error = true;

          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SendTo.City = "";
          export.SendTo.State = "";
          export.SendTo.Street1 = "Security Block on Address";
          export.SendTo.Street2 = "";
          export.SendTo.ZipCode3 = "";
          export.SendTo.ZipCode4 = "";
          export.SendTo.ZipCode5 = "";

          return;
        }
        else
        {
          export.PreviousReceiptRefund.Assign(export.ReceiptRefund);
        }

        // ************************************************
        // *All processing completed successfully         *
        // ************************************************
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        global.Command = "DISPLAY";

        break;
      case "UPDATE":
        // ***Do not allow a refund to be updated if created prior to current 
        // date.  Sunya Sharp 3/15/1999 ***
        if (Lt(export.ReceiptRefund.RequestDate, Now().Date))
        {
          var field = GetField(export.PaymentStatus, "code");

          field.Error = true;

          ExitState = "FN0000_CANT_UPD_OR_DEL_REFUND";

          return;
        }

        // ************************************************
        // *Determine if the CSE Person or Court info. has*
        // *changed.  Disallow these changes.             *
        // ************************************************
        if (!Equal(export.CsePersonsWorkSet.Number,
          export.PreviousCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
        }

        if (!Equal(export.CashReceiptSourceType.Code,
          export.PreviousCashReceiptSourceType.Code))
        {
          var field = GetField(export.CashReceiptSourceType, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ************************************************
        // *Validate the Reason Code against the Codes    *
        // *table.
        // 
        // *
        // ************************************************
        local.PassCode.CodeName = "REFUND REASON";
        local.PassCodeValue.Cdvalue = export.ReceiptRefund.ReasonCode;
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field = GetField(export.ReceiptRefund, "reasonCode");

          field.Error = true;

          ExitState = "FN0000_ENTER_VALID_REFUND_REASON";

          return;
        }

        // ************************************************
        // *Update the Refund                             *
        // ************************************************
        // H00101298  HERE to save and display
        if (Equal(export.Current.Street1, "Security Block on Address"))
        {
          export.Current.City = export.CurrentHidden.City;
          export.Current.State = export.CurrentHidden.State;
          export.Current.Street1 = export.CurrentHidden.Street1;
          export.Current.Street2 = export.CurrentHidden.Street2 ?? "";
          export.Current.ZipCode3 = export.CurrentHidden.ZipCode3 ?? "";
          export.Current.ZipCode4 = export.CurrentHidden.ZipCode4 ?? "";
          export.Current.ZipCode5 = export.CurrentHidden.ZipCode5;
          ExitState = "ACO_NN0000_ALL_OK";
        }

        UseFnAbUpdateMiscRefund();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("FN0000_CANT_UPD_OR_DEL_REFUND"))
          {
            var field = GetField(export.PaymentStatus, "code");

            field.Error = true;
          }

          return;
        }
        else
        {
          export.ReceiptRefund.CreatedBy = global.UserId;
        }

        // *****
        // Processing completed successfully
        // *****
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        global.Command = "DISPLAY";

        break;
      case "DELETE":
        // *****
        // Force user to display first.
        // *****
        if (AsChar(export.DisplayComplete.Flag) != 'Y')
        {
          var field4 = GetField(export.CashReceiptSourceType, "code");

          field4.Error = true;

          var field5 = GetField(export.CsePersonsWorkSet, "number");

          field5.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        // ***Do not allow a refund to be deleted if created prior to current 
        // date.  Sunya Sharp 3/15/1999 ***
        if (Lt(export.ReceiptRefund.RequestDate, Now().Date))
        {
          var field = GetField(export.PaymentStatus, "code");

          field.Error = true;

          ExitState = "FN0000_CANT_UPD_OR_DEL_REFUND";

          return;
        }

        // *****
        // Delete the Refund
        // *****
        UseFnAbDeleteMiscRefunds();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("FN0000_CANT_UPD_OR_DEL_REFUND"))
          {
            var field = GetField(export.PaymentStatus, "code");

            field.Error = true;
          }

          return;
        }

        // *****
        // All processing completed successfully
        // *****
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        export.Current.Assign(local.BlankCashReceiptDetailAddress);
        export.SendTo.Assign(local.BlankCashReceiptDetailAddress);
        export.ReceiptRefund.Assign(local.BlankReceiptRefund);
        MoveCashReceiptSourceType(local.BlankCashReceiptSourceType,
          export.CashReceiptSourceType);
        MoveCsePersonsWorkSet(local.BlankCsePersonsWorkSet,
          export.CsePersonsWorkSet);
        MoveCsePersonsWorkSet(local.BlankCsePersonsWorkSet,
          export.PreviousCsePersonsWorkSet);
        export.PreviousReceiptRefund.Assign(local.BlankReceiptRefund);
        export.PaymentStatus.Code = "";

        // H00101298  HERE to save and display
        export.CurrentHidden.Assign(local.BlankCashReceiptDetailAddress);
        export.SendToHidden.Assign(local.BlankCashReceiptDetailAddress);

        break;
      case "LTRB":
        // *** Do not allow flow to LTRB if person number is populated.  Sunya 
        // Sharp 3/10/1999 ***
        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          ExitState = "FN0000_CANNOT_FLOW_TO_LTRB";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        // *** Added logic to require the source code to be populated before 
        // flowing to LTRB.  Also, added logic to determine data to be passed to
        // LTRB.  If the source code is a state, send to state and nothing in
        // the county.  If the source is a county, send the state as KS and the
        // source code as the county.  This is per user request.  Sunya Sharp 3/
        // 10/1999 ***
        // *** Do not require the source code to be valid before flowing  to 
        // LTRB  Sunya Sharp 3/18/1999 ***
        if (IsEmpty(export.CashReceiptSourceType.Code))
        {
          var field = GetField(export.CashReceiptSourceType, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          // *** This is to populate the description for the source code.  Sunya
          // Sharp 3/18/1999 ***
          if (ReadCashReceiptSourceType())
          {
            MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
              export.CashReceiptSourceType);
          }

          // *** User would like to pass only the first four letter of the 
          // source code name to LTRB.  The search process on LTRB was not
          // working well with just the source code.  Sunya Sharp 5/15/1999 ***
          if (Equal(export.CashReceiptSourceType.Code, 4, 5, "STATE"))
          {
            export.PassToLtrb.StateAbbreviation =
              Substring(export.CashReceiptSourceType.Code, 1, 2);
            export.PassToLtrb.CountyDescription = "";
          }
          else
          {
            export.PassToLtrb.StateAbbreviation = "KS";
            export.PassToLtrb.CountyDescription =
              Substring(entities.ExistingCashReceiptSourceType.Name, 1, 4);
          }
        }

        ExitState = "ECO_LNK_TO_LST_TRIBUNAL";

        return;
      case "CRRL":
        MoveReceiptRefund4(export.ReceiptRefund, export.PassToCrrl);
        export.Pass.Text4 = "RMSR";
        ExitState = "ECO_LNK_LST_SEL_REFUNDS";

        return;
      case "ORGZ":
        // *** Pass person number is available.  Sunya Sharp 3/11/1999 ***
        export.PassToOrgz.Number = export.CsePersonsWorkSet.Number;
        ExitState = "ECO_LNK_TO_ORGZ";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // *****
      // Initialize send to address when displaying a new refund
      // *****
      export.SendTo.Assign(local.NullCashReceiptDetailAddress);

      // H00101298  HERE to save and display
      export.SendToHidden.Assign(local.NullCashReceiptDetailAddress);

      if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
      {
        local.UpdateFlag.Flag = "Y";
      }

      if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        local.AddFlag.Flag = "Y";
      }

      // *****
      // If Refund has been selected Display the Data on the screen.
      // *****
      UseFnAbReadReceiptRefund();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
          ("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          goto Test1;
        }

        export.CurrentHidden.SystemGeneratedIdentifier =
          export.Current.SystemGeneratedIdentifier;
        export.CurrentHidden.City = export.Current.City;
        export.CurrentHidden.State = export.Current.State;
        export.CurrentHidden.Street1 = export.Current.Street1;
        export.CurrentHidden.Street2 = export.Current.Street2 ?? "";
        export.CurrentHidden.ZipCode3 = export.Current.ZipCode3 ?? "";
        export.CurrentHidden.ZipCode4 = export.Current.ZipCode4 ?? "";
        export.CurrentHidden.ZipCode5 = export.Current.ZipCode5;
        export.Current.City = "";
        export.Current.State = "";
        export.Current.Street1 = "Security Block on Address";
        export.Current.Street2 = "";
        export.Current.ZipCode3 = "";
        export.Current.ZipCode4 = "";
        export.Current.ZipCode5 = "";

        return;
      }

Test1:

      // H00101298  HERE to save and display
      export.CurrentHidden.SystemGeneratedIdentifier =
        export.Current.SystemGeneratedIdentifier;
      export.CurrentHidden.City = export.Current.City;
      export.CurrentHidden.State = export.Current.State;
      export.CurrentHidden.Street1 = export.Current.Street1;
      export.CurrentHidden.Street2 = export.Current.Street2 ?? "";
      export.CurrentHidden.ZipCode3 = export.Current.ZipCode3 ?? "";
      export.CurrentHidden.ZipCode4 = export.Current.ZipCode4 ?? "";
      export.CurrentHidden.ZipCode5 = export.Current.ZipCode5;

      // WITH Security Check
      global.Command = "RESEARCH";
      UseScCabTestSecurity1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.Current.City = "";
        export.Current.State = "";
        export.Current.Street1 = "Security Block on Address";
        export.Current.Street2 = "";
        export.Current.ZipCode3 = "";
        export.Current.ZipCode4 = "";
        export.Current.ZipCode5 = "";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      global.Command = "DISPLAY";

      if (AsChar(local.AddFlag.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }

      if (AsChar(local.UpdateFlag.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }

      if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
        ("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        export.DisplayComplete.Flag = "Y";

        goto Test2;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

        if (ReadCsePerson())
        {
          if (AsChar(entities.ExistingCsePerson.Type1) == 'C')
          {
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.CsePersonsWorkSet.FormattedName =
                "**** ADABAS UNAVAILABLE ****";
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              export.CsePersonsWorkSet.FormattedName =
                local.CsePersonsWorkSet.FormattedName;
              export.ReceiptRefund.PayeeName =
                local.CsePersonsWorkSet.FormattedName;
              export.ReceiptRefund.Taxid = local.CsePersonsWorkSet.Ssn;
            }
          }
          else
          {
            export.CsePersonsWorkSet.FormattedName =
              entities.ExistingCsePerson.OrganizationName ?? Spaces(33);
            export.ReceiptRefund.PayeeName =
              entities.ExistingCsePerson.OrganizationName;
            export.ReceiptRefund.Taxid = entities.ExistingCsePerson.TaxId;
          }
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          return;
        }
      }

      if (AsChar(export.Set.RecoupmentIndKpc) == 'Y')
      {
        ExitState = "FN0000_DISPLAY_SUCC_KPC_RECOUP";
      }
      else
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }

      var field1 = GetField(export.CashReceiptSourceType, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.CsePersonsWorkSet, "number");

      field2.Color = "cyan";
      field2.Protected = true;

      export.DisplayComplete.Flag = "Y";
    }

Test2:

    if ((IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState
      ("FN0000_DISPLAY_SUCC_KPC_RECOUP")) && export
      .ReceiptRefund.CreatedTimestamp != null || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
        export.PreviousCsePersonsWorkSet);
      export.PreviousReceiptRefund.Assign(export.ReceiptRefund);
      MoveCashReceiptSourceType(export.CashReceiptSourceType,
        export.PreviousCashReceiptSourceType);
    }
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
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
    target.Type1 = source.Type1;
    target.Classification = source.Classification;
    target.Amount = source.Amount;
    target.CsePersonNumber = source.CsePersonNumber;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.ProcessDate = source.ProcessDate;
    target.RecoupmentIndKpc = source.RecoupmentIndKpc;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private static void MoveReceiptRefund1(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
    target.Amount = source.Amount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.RequestDate = source.RequestDate;
    target.ReasonText = source.ReasonText;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveReceiptRefund2(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
    target.Amount = source.Amount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.RequestDate = source.RequestDate;
    target.ReasonText = source.ReasonText;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.TaxIdSuffix = source.TaxIdSuffix;
  }

  private static void MoveReceiptRefund3(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
    target.Amount = source.Amount;
    target.RequestDate = source.RequestDate;
    target.ReasonText = source.ReasonText;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveReceiptRefund4(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
  }

  private static void MoveReceiptRefund5(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.Amount = source.Amount;
    target.RequestDate = source.RequestDate;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
  }

  private void UseCabFnGetTribunalAddress1()
  {
    var useImport = new CabFnGetTribunalAddress.Import();
    var useExport = new CabFnGetTribunalAddress.Export();

    useImport.Tribunal.Identifier = entities.ExistingTribunal.Identifier;

    Call(CabFnGetTribunalAddress.Execute, useImport, useExport);

    local.FipsTribAddress.Assign(useExport.FipsTribAddress);
  }

  private void UseCabFnGetTribunalAddress2()
  {
    var useImport = new CabFnGetTribunalAddress.Import();
    var useExport = new CabFnGetTribunalAddress.Export();

    useImport.Tribunal.Identifier = import.Selected.Identifier;

    Call(CabFnGetTribunalAddress.Execute, useImport, useExport);

    local.FipsTribAddress.Assign(useExport.FipsTribAddress);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.PassCodeValue.Cdvalue;
    useImport.Code.CodeName = local.PassCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadCsePerson.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadCsePerson.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadCsePerson.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnAbCreateMiscRefund()
  {
    var useImport = new FnAbCreateMiscRefund.Import();
    var useExport = new FnAbCreateMiscRefund.Export();

    MovePaymentRequest1(export.Set, useImport.PaymentRequest);
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.ReceiptRefund.Assign(export.ReceiptRefund);
    useImport.CashReceiptDetailAddress.Assign(export.SendTo);
    useImport.CashReceiptSourceType.Code = export.CashReceiptSourceType.Code;

    Call(FnAbCreateMiscRefund.Execute, useImport, useExport);

    MoveReceiptRefund3(useExport.ReceiptRefund, export.ReceiptRefund);
  }

  private void UseFnAbDeleteMiscRefunds()
  {
    var useImport = new FnAbDeleteMiscRefunds.Import();
    var useExport = new FnAbDeleteMiscRefunds.Export();

    useImport.CashReceiptSourceType.Code = import.CashReceiptSourceType.Code;
    useImport.ReceiptRefund.Assign(export.ReceiptRefund);

    Call(FnAbDeleteMiscRefunds.Execute, useImport, useExport);
  }

  private void UseFnAbReadReceiptRefund()
  {
    var useImport = new FnAbReadReceiptRefund.Import();
    var useExport = new FnAbReadReceiptRefund.Export();

    useImport.ReceiptRefund.CreatedTimestamp =
      export.ReceiptRefund.CreatedTimestamp;

    Call(FnAbReadReceiptRefund.Execute, useImport, useExport);

    export.PaymentStatus.Code = useExport.PaymentStatus.Code;
    MovePaymentRequest2(useExport.PaymentRequest, export.Set);
    export.CsePersonsWorkSet.Number = useExport.CsePersonsWorkSet.Number;
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      export.CashReceiptSourceType);
    export.Current.Assign(useExport.CashReceiptDetailAddress);
    MoveReceiptRefund1(useExport.ReceiptRefund, export.ReceiptRefund);
  }

  private void UseFnAbUpdateMiscRefund()
  {
    var useImport = new FnAbUpdateMiscRefund.Import();
    var useExport = new FnAbUpdateMiscRefund.Export();

    MoveReceiptRefund5(export.PreviousReceiptRefund, useImport.Current);
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.CashReceiptSourceType.Code = export.CashReceiptSourceType.Code;
    useImport.ReceiptRefund.Assign(export.ReceiptRefund);
    useImport.CashReceiptDetailAddress.Assign(export.SendTo);
    useImport.Existing.SystemGeneratedIdentifier =
      export.Current.SystemGeneratedIdentifier;

    Call(FnAbUpdateMiscRefund.Execute, useImport, useExport);
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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCashReceiptCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingReceiptRefund.Populated);
    entities.ExistingCashReceipt.Populated = false;
    entities.ExistingCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crvIdentifier",
          entities.ExistingReceiptRefund.CrvIdentifier.GetValueOrDefault());
        db.SetNullableInt32(
          command, "cstIdentifier",
          entities.ExistingReceiptRefund.CstIdentifier.GetValueOrDefault());
        db.SetNullableInt32(
          command, "crtIdentifier",
          entities.ExistingReceiptRefund.CrtIdentifier.GetValueOrDefault());
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.ExistingReceiptRefund.CrdIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", export.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Name = db.GetString(reader, 2);
        entities.ExistingCashReceiptSourceType.State =
          db.GetNullableInt32(reader, 3);
        entities.ExistingCashReceiptSourceType.County =
          db.GetNullableInt32(reader, 4);
        entities.ExistingCashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.ExistingCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.TaxId = db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.OrganizationName =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 1);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 3);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 4);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Zip4 = db.GetNullableString(reader, 6);
        entities.ExistingFipsTribAddress.Zip3 = db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 8);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 9);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 10);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 11);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefundPaymentRequest1()
  {
    entities.ExistingPaymentRequest.Populated = false;
    entities.ExistingReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefundPaymentRequest1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingReceiptRefund.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingPaymentRequest.RctRTstamp =
          db.GetNullableDateTime(reader, 0);
        entities.ExistingReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ExistingReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ExistingReceiptRefund.PayeeName =
          db.GetNullableString(reader, 3);
        entities.ExistingReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ExistingReceiptRefund.OffsetTaxYear =
          db.GetNullableInt32(reader, 5);
        entities.ExistingReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ExistingReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ExistingReceiptRefund.CspNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingReceiptRefund.CstAIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingReceiptRefund.CrvIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingReceiptRefund.CrdIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingReceiptRefund.OffsetClosed = db.GetString(reader, 12);
        entities.ExistingReceiptRefund.DateTransmitted =
          db.GetNullableDate(reader, 13);
        entities.ExistingReceiptRefund.TaxIdSuffix =
          db.GetNullableString(reader, 14);
        entities.ExistingReceiptRefund.ReasonText =
          db.GetNullableString(reader, 15);
        entities.ExistingReceiptRefund.CrtIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.ExistingReceiptRefund.CstIdentifier =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 19);
        entities.ExistingPaymentRequest.Populated = true;
        entities.ExistingReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ExistingReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefundPaymentRequest2()
  {
    entities.ExistingPaymentRequest.Populated = false;
    entities.ExistingReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefundPaymentRequest2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cstAIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingReceiptRefund.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingPaymentRequest.RctRTstamp =
          db.GetNullableDateTime(reader, 0);
        entities.ExistingReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ExistingReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ExistingReceiptRefund.PayeeName =
          db.GetNullableString(reader, 3);
        entities.ExistingReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ExistingReceiptRefund.OffsetTaxYear =
          db.GetNullableInt32(reader, 5);
        entities.ExistingReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ExistingReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ExistingReceiptRefund.CspNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingReceiptRefund.CstAIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingReceiptRefund.CrvIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingReceiptRefund.CrdIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingReceiptRefund.OffsetClosed = db.GetString(reader, 12);
        entities.ExistingReceiptRefund.DateTransmitted =
          db.GetNullableDate(reader, 13);
        entities.ExistingReceiptRefund.TaxIdSuffix =
          db.GetNullableString(reader, 14);
        entities.ExistingReceiptRefund.ReasonText =
          db.GetNullableString(reader, 15);
        entities.ExistingReceiptRefund.CrtIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.ExistingReceiptRefund.CstIdentifier =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 19);
        entities.ExistingPaymentRequest.Populated = true;
        entities.ExistingReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ExistingReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fipState",
          local.CashReceiptSourceType2.State.GetValueOrDefault());
        db.SetNullableInt32(
          command, "fipCounty",
          local.CashReceiptSourceType2.County.GetValueOrDefault());
        db.SetNullableInt32(
          command, "fipLocation",
          local.CashReceiptSourceType2.Location.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.Name = db.GetString(reader, 0);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 2);
        entities.ExistingTribunal.TaxIdSuffix = db.GetNullableString(reader, 3);
        entities.ExistingTribunal.TaxId = db.GetNullableString(reader, 4);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.Selected.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.Name = db.GetString(reader, 0);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 2);
        entities.ExistingTribunal.TaxIdSuffix = db.GetNullableString(reader, 3);
        entities.ExistingTribunal.TaxId = db.GetNullableString(reader, 4);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.Populated = true;
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
    /// A value of FromFlowCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("fromFlowCashReceiptSourceType")]
    public CashReceiptSourceType FromFlowCashReceiptSourceType
    {
      get => fromFlowCashReceiptSourceType ??= new();
      set => fromFlowCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of FromFlowReceiptRefund.
    /// </summary>
    [JsonPropertyName("fromFlowReceiptRefund")]
    public ReceiptRefund FromFlowReceiptRefund
    {
      get => fromFlowReceiptRefund ??= new();
      set => fromFlowReceiptRefund = value;
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
    /// A value of Set.
    /// </summary>
    [JsonPropertyName("set")]
    public PaymentRequest Set
    {
      get => set ??= new();
      set => set = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Tribunal Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CashReceiptDetailAddress Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of RefundReason.
    /// </summary>
    [JsonPropertyName("refundReason")]
    public Standard RefundReason
    {
      get => refundReason ??= new();
      set => refundReason = value;
    }

    /// <summary>
    /// A value of Source.
    /// </summary>
    [JsonPropertyName("source")]
    public Standard Source
    {
      get => source ??= new();
      set => source = value;
    }

    /// <summary>
    /// A value of PreviousReceiptRefund.
    /// </summary>
    [JsonPropertyName("previousReceiptRefund")]
    public ReceiptRefund PreviousReceiptRefund
    {
      get => previousReceiptRefund ??= new();
      set => previousReceiptRefund = value;
    }

    /// <summary>
    /// A value of PreviousCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("previousCashReceiptSourceType")]
    public CashReceiptSourceType PreviousCashReceiptSourceType
    {
      get => previousCashReceiptSourceType ??= new();
      set => previousCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("previousCsePersonsWorkSet")]
    public CsePersonsWorkSet PreviousCsePersonsWorkSet
    {
      get => previousCsePersonsWorkSet ??= new();
      set => previousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DisplayComplete.
    /// </summary>
    [JsonPropertyName("displayComplete")]
    public Common DisplayComplete
    {
      get => displayComplete ??= new();
      set => displayComplete = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public CodeValue Return1
    {
      get => return1 ??= new();
      set => return1 = value;
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
    /// A value of CurrentHidden.
    /// </summary>
    [JsonPropertyName("currentHidden")]
    public CashReceiptDetailAddress CurrentHidden
    {
      get => currentHidden ??= new();
      set => currentHidden = value;
    }

    /// <summary>
    /// A value of SendToHidden.
    /// </summary>
    [JsonPropertyName("sendToHidden")]
    public CashReceiptDetailAddress SendToHidden
    {
      get => sendToHidden ??= new();
      set => sendToHidden = value;
    }

    private CashReceiptSourceType fromFlowCashReceiptSourceType;
    private ReceiptRefund fromFlowReceiptRefund;
    private CsePersonsWorkSet fromFlowCsePersonsWorkSet;
    private PaymentRequest set;
    private PaymentStatus paymentStatus;
    private Tribunal selected;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress current;
    private CashReceiptDetailAddress sendTo;
    private CashReceiptSourceType cashReceiptSourceType;
    private Standard standard;
    private Standard refundReason;
    private Standard source;
    private ReceiptRefund previousReceiptRefund;
    private CashReceiptSourceType previousCashReceiptSourceType;
    private CsePersonsWorkSet previousCsePersonsWorkSet;
    private Common displayComplete;
    private CodeValue return1;
    private NextTranInfo hidden;
    private CashReceiptDetailAddress currentHidden;
    private CashReceiptDetailAddress sendToHidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PassToName.
    /// </summary>
    [JsonPropertyName("passToName")]
    public Common PassToName
    {
      get => passToName ??= new();
      set => passToName = value;
    }

    /// <summary>
    /// A value of PassToOrgz.
    /// </summary>
    [JsonPropertyName("passToOrgz")]
    public CsePerson PassToOrgz
    {
      get => passToOrgz ??= new();
      set => passToOrgz = value;
    }

    /// <summary>
    /// A value of PassToCrrl.
    /// </summary>
    [JsonPropertyName("passToCrrl")]
    public ReceiptRefund PassToCrrl
    {
      get => passToCrrl ??= new();
      set => passToCrrl = value;
    }

    /// <summary>
    /// A value of PassToLtrb.
    /// </summary>
    [JsonPropertyName("passToLtrb")]
    public Fips PassToLtrb
    {
      get => passToLtrb ??= new();
      set => passToLtrb = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public TextWorkArea Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Tribunal Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CashReceiptDetailAddress Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of RefundReason.
    /// </summary>
    [JsonPropertyName("refundReason")]
    public Standard RefundReason
    {
      get => refundReason ??= new();
      set => refundReason = value;
    }

    /// <summary>
    /// A value of Source.
    /// </summary>
    [JsonPropertyName("source")]
    public Standard Source
    {
      get => source ??= new();
      set => source = value;
    }

    /// <summary>
    /// A value of DisplayAll.
    /// </summary>
    [JsonPropertyName("displayAll")]
    public Common DisplayAll
    {
      get => displayAll ??= new();
      set => displayAll = value;
    }

    /// <summary>
    /// A value of PreviousTribunal.
    /// </summary>
    [JsonPropertyName("previousTribunal")]
    public Tribunal PreviousTribunal
    {
      get => previousTribunal ??= new();
      set => previousTribunal = value;
    }

    /// <summary>
    /// A value of PreviousReceiptRefund.
    /// </summary>
    [JsonPropertyName("previousReceiptRefund")]
    public ReceiptRefund PreviousReceiptRefund
    {
      get => previousReceiptRefund ??= new();
      set => previousReceiptRefund = value;
    }

    /// <summary>
    /// A value of PreviousCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("previousCashReceiptSourceType")]
    public CashReceiptSourceType PreviousCashReceiptSourceType
    {
      get => previousCashReceiptSourceType ??= new();
      set => previousCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("previousCsePersonsWorkSet")]
    public CsePersonsWorkSet PreviousCsePersonsWorkSet
    {
      get => previousCsePersonsWorkSet ??= new();
      set => previousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DisplayComplete.
    /// </summary>
    [JsonPropertyName("displayComplete")]
    public Common DisplayComplete
    {
      get => displayComplete ??= new();
      set => displayComplete = value;
    }

    /// <summary>
    /// A value of Set.
    /// </summary>
    [JsonPropertyName("set")]
    public PaymentRequest Set
    {
      get => set ??= new();
      set => set = value;
    }

    /// <summary>
    /// A value of CodeName.
    /// </summary>
    [JsonPropertyName("codeName")]
    public Code CodeName
    {
      get => codeName ??= new();
      set => codeName = value;
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
    /// A value of CurrentHidden.
    /// </summary>
    [JsonPropertyName("currentHidden")]
    public CashReceiptDetailAddress CurrentHidden
    {
      get => currentHidden ??= new();
      set => currentHidden = value;
    }

    /// <summary>
    /// A value of SendToHidden.
    /// </summary>
    [JsonPropertyName("sendToHidden")]
    public CashReceiptDetailAddress SendToHidden
    {
      get => sendToHidden ??= new();
      set => sendToHidden = value;
    }

    private Common passToName;
    private CsePerson passToOrgz;
    private ReceiptRefund passToCrrl;
    private Fips passToLtrb;
    private PaymentStatus paymentStatus;
    private TextWorkArea pass;
    private Tribunal selected;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress current;
    private CashReceiptDetailAddress sendTo;
    private CashReceiptSourceType cashReceiptSourceType;
    private Standard standard;
    private Standard refundReason;
    private Standard source;
    private Common displayAll;
    private Tribunal previousTribunal;
    private ReceiptRefund previousReceiptRefund;
    private CashReceiptSourceType previousCashReceiptSourceType;
    private CsePersonsWorkSet previousCsePersonsWorkSet;
    private Common displayComplete;
    private PaymentRequest set;
    private Code codeName;
    private NextTranInfo hidden;
    private CashReceiptDetailAddress currentHidden;
    private CashReceiptDetailAddress sendToHidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of OnlyOne.
    /// </summary>
    [JsonPropertyName("onlyOne")]
    public ReceiptRefund OnlyOne
    {
      get => onlyOne ??= new();
      set => onlyOne = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType1.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType1")]
    public CashReceiptSourceType CashReceiptSourceType1
    {
      get => cashReceiptSourceType1 ??= new();
      set => cashReceiptSourceType1 = value;
    }

    /// <summary>
    /// A value of CsePersonAddressExists.
    /// </summary>
    [JsonPropertyName("csePersonAddressExists")]
    public Common CsePersonAddressExists
    {
      get => csePersonAddressExists ??= new();
      set => csePersonAddressExists = value;
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
    /// A value of LeftPadCsePerson.
    /// </summary>
    [JsonPropertyName("leftPadCsePerson")]
    public TextWorkArea LeftPadCsePerson
    {
      get => leftPadCsePerson ??= new();
      set => leftPadCsePerson = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of BlankCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("blankCashReceiptDetailAddress")]
    public CashReceiptDetailAddress BlankCashReceiptDetailAddress
    {
      get => blankCashReceiptDetailAddress ??= new();
      set => blankCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of BlankCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("blankCashReceiptSourceType")]
    public CashReceiptSourceType BlankCashReceiptSourceType
    {
      get => blankCashReceiptSourceType ??= new();
      set => blankCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of BlankReceiptRefund.
    /// </summary>
    [JsonPropertyName("blankReceiptRefund")]
    public ReceiptRefund BlankReceiptRefund
    {
      get => blankReceiptRefund ??= new();
      set => blankReceiptRefund = value;
    }

    /// <summary>
    /// A value of NoOfRefundTrans.
    /// </summary>
    [JsonPropertyName("noOfRefundTrans")]
    public Common NoOfRefundTrans
    {
      get => noOfRefundTrans ??= new();
      set => noOfRefundTrans = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of PassCodeValue.
    /// </summary>
    [JsonPropertyName("passCodeValue")]
    public CodeValue PassCodeValue
    {
      get => passCodeValue ??= new();
      set => passCodeValue = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of CashReceiptSourceType2.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType2")]
    public CashReceiptSourceType CashReceiptSourceType2
    {
      get => cashReceiptSourceType2 ??= new();
      set => cashReceiptSourceType2 = value;
    }

    /// <summary>
    /// A value of NullReceiptRefund.
    /// </summary>
    [JsonPropertyName("nullReceiptRefund")]
    public ReceiptRefund NullReceiptRefund
    {
      get => nullReceiptRefund ??= new();
      set => nullReceiptRefund = value;
    }

    /// <summary>
    /// A value of NullCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("nullCashReceiptDetailAddress")]
    public CashReceiptDetailAddress NullCashReceiptDetailAddress
    {
      get => nullCashReceiptDetailAddress ??= new();
      set => nullCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of UpdateFlag.
    /// </summary>
    [JsonPropertyName("updateFlag")]
    public Common UpdateFlag
    {
      get => updateFlag ??= new();
      set => updateFlag = value;
    }

    /// <summary>
    /// A value of AddFlag.
    /// </summary>
    [JsonPropertyName("addFlag")]
    public Common AddFlag
    {
      get => addFlag ??= new();
      set => addFlag = value;
    }

    private CsePerson csePerson;
    private ReceiptRefund onlyOne;
    private CashReceiptSourceType cashReceiptSourceType1;
    private Common csePersonAddressExists;
    private DateWorkArea nullDateWorkArea;
    private TextWorkArea leftPadCsePerson;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private CashReceiptDetailAddress blankCashReceiptDetailAddress;
    private CashReceiptSourceType blankCashReceiptSourceType;
    private ReceiptRefund blankReceiptRefund;
    private Common noOfRefundTrans;
    private FipsTribAddress fipsTribAddress;
    private CsePersonAddress csePersonAddress;
    private CodeValue passCodeValue;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common returnCode;
    private Code passCode;
    private CashReceiptSourceType cashReceiptSourceType2;
    private ReceiptRefund nullReceiptRefund;
    private CashReceiptDetailAddress nullCashReceiptDetailAddress;
    private Common updateFlag;
    private Common addFlag;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePersonAddress.
    /// </summary>
    [JsonPropertyName("existingCsePersonAddress")]
    public CsePersonAddress ExistingCsePersonAddress
    {
      get => existingCsePersonAddress ??= new();
      set => existingCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
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

    /// <summary>
    /// A value of ExistingPaymentRequest.
    /// </summary>
    [JsonPropertyName("existingPaymentRequest")]
    public PaymentRequest ExistingPaymentRequest
    {
      get => existingPaymentRequest ??= new();
      set => existingPaymentRequest = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailAddress")]
    public CashReceiptDetailAddress ExistingCashReceiptDetailAddress
    {
      get => existingCashReceiptDetailAddress ??= new();
      set => existingCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of ExistingReceiptRefund.
    /// </summary>
    [JsonPropertyName("existingReceiptRefund")]
    public ReceiptRefund ExistingReceiptRefund
    {
      get => existingReceiptRefund ??= new();
      set => existingReceiptRefund = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    private CsePersonAddress existingCsePersonAddress;
    private Tribunal existingTribunal;
    private FipsTribAddress existingFipsTribAddress;
    private Fips existingFips;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private PaymentRequest existingPaymentRequest;
    private CashReceiptDetailAddress existingCashReceiptDetailAddress;
    private ReceiptRefund existingReceiptRefund;
    private CsePerson existingCsePerson;
    private CashReceiptSourceType existingCashReceiptSourceType;
  }
#endregion
}

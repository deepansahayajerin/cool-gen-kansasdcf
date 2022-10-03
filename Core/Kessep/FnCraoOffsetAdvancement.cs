// Program: FN_CRAO_OFFSET_ADVANCEMENT, ID: 372304769, model: 746.
// Short name: SWECRAOP
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
/// A program: FN_CRAO_OFFSET_ADVANCEMENT.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This procedure will display, create, change, and delete an offset 
/// advancement which is a type of receipt refund.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCraoOffsetAdvancement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRAO_OFFSET_ADVANCEMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCraoOffsetAdvancement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCraoOffsetAdvancement.
  /// </summary>
  public FnCraoOffsetAdvancement(IContext context, Import import, Export export):
    
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
    // Every initial development and change to that development needs to be 
    // documented.
    // ----------------------------------------------------------------------------
    // Date 	  Developer Name	Request #	Description
    // 02/06/96  Holly Kennedy-MTW			Retrofits
    // 02/11/96  Holly Kennedy-MTW			Added logic to validate
    // 						entered State
    // 02/18/96  Holly Kennedy-MTW			Removed funding logic
    // 01/02/97  R. Marchman				Add new security/next tran
    // 04/08/97  Sumanta Mahapatra - MTW Made the following
    //                                    
    // modifications :-
    //    - Offset tax year validation - cannot be current year
    //    - PF15 - changed to "Advancement Closed"
    //    - Added Worker Id field
    //    - made offset Tax Id field numeric
    // 06/18/97  M. D. Wheaton				Deleted datenum
    // 09/12/97  Siraj Konkader 	PR# 27380/27747
    // >> On the flow from CRAL I matched CSE PERSON to the hidden view in CRAO 
    // because the current logic checks to see if there were any changes to the
    // key fields (for re-display).
    // >> ADD cmd: set request date to current date
    // >> warrant number was being erased when it was highlighted in error on 
    // delete attempt because CAB CANCEL_ADVANCEMENT, although exporting data
    // had no code to move entity view to export after READ. Removed view match.
    // >> on delete, taxid was not being initialized
    // 02/25/98  A Samuels		PR39016		Having to press PF2
    // 						twice for display
    // 01/19/99  Sunya Sharp		Make changes per screen assessment form approved 
    // by the SMEs.  Right justified the advancement amount field.  Added logic
    // to support the new attributes (last updated by and last updated timestamp
    // )  for the receipt refund.
    // 03/25/99	Sunya Sharp		Change literals on the screen and use 
    // si_get_cse_person_mailing_addr instead of
    // fn_get_active_cse_person_address to get the address for a person.
    // 03/20/00    PDP   Prevent manually enterd names from being overlaid
    // 08/23/99      Sunya Sharp   Make changes per PR#120.  Warrant information
    // is not displaying correctly when an advancement has both a warrant and a
    // potential recovery.
    // 10/18/99      Sunya Sharp   H00077672 - Add logic to prevent the refund 
    // amount from being negative.
    // 04/26/01      Madhu Kumar   PR #114679 - For any screen allowing add or 
    // edit of ZIP Code, have edit that will not allow less than 5 digits. Edit
    // checks for 4 digits and 5 digit zip Code done.
    // 11/20/01     Kalpesh Doshi  WR020147 - KPC Recoupment.
    // 06/22/12  GVandy		 CQ33628	Do not allow advancements on U type 
    // collections.
    // ----------------------------------------------------------------------------
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 02/03/2010  Raj S              CQ15282     Modified to fix the verify 
    // function V76  *
    // *
    // 
    // upgrade issues.                          *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // *** Added exit state when successful clear.  Sunya Sharp 1/19/1999 ***
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // Move Imports to Exports.
    export.Taxid.AverageInteger = import.Taxid.AverageInteger;
    export.CashReceiptSourceType.Code = import.CashReceiptSourceType.Code;
    export.HiddenCashReceiptSourceType.Code =
      import.HiddenCashReceiptSourceType.Code;
    MoveCollectionType(import.CollectionType, export.CollectionType);
    MoveCollectionType(import.HiddenCollectionType, export.HiddenCollectionType);
      
    export.PromptCsePersonNumber.SelectChar =
      import.PromptCsePersonNumber.SelectChar;
    export.PromptCollectionType.PromptField =
      import.PromptCollectionType.PromptField;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.CsePerson.Number = import.CsePerson.Number;
    export.CashReceiptDetailAddress.Assign(import.CashReceiptDetailAddress);
    export.PromptStateCode.SelectChar = import.PromptStateCode.SelectChar;
    export.ReceiptRefund.Assign(import.ReceiptRefund);
    MoveReceiptRefund3(import.HiddenReceiptRefund, export.HiddenReceiptRefund);
    export.PaymentRequest.Assign(import.PaymentRequest);
    export.CashReceiptDetail.CollectionAmount =
      import.CashReceiptDetail.CollectionAmount;
    export.PaymentStatus.Code = import.PaymentStatus.Code;
    export.HdisplayPerformedInd.Flag = import.HdisplayPerformedInd.Flag;

    // *** This is for the return from CRRC.  Sunya Sharp 1/28/1999 ***
    if (Equal(global.Command, "RTFRMLNK"))
    {
      return;
    }

    // -------------------------------------------------------
    // Left pad with zeros -Syed Hasan,MTW 12/15/97
    // -------------------------------------------------------
    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    if (Equal(global.Command, "DETAIL"))
    {
      local.DisplayDetail.Flag = "Y";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "PRMPTRET"))
    {
      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
      }
      else if (!IsEmpty(export.HiddenCsePerson.Number))
      {
        export.CsePerson.Number = export.HiddenCsePerson.Number;
      }

      export.PromptCsePersonNumber.SelectChar = "";
      global.Command = "DISPLAY";
    }

    // *** A data model change was requested to include the collection type to 
    // further define the receipt refund.  This is the new logic when returning
    // from CLCT (List collection types).  Not all collection types are allowed
    // on this screen.  The SME wanted an error when returnng to inform the user
    // that an invalid selection was made prior to the display.  Sunya Sharp 1/
    // 19/1999 ***
    if (Equal(global.Command, "RETCLCT"))
    {
      if (!IsEmpty(import.FromFlow.Code))
      {
        switch(TrimEnd(import.FromFlow.Code))
        {
          case "F":
            break;
          case "S":
            break;
          case "U":
            break;
          case "K":
            break;
          case "R":
            break;
          case "T":
            break;
          case "Y":
            break;
          case "Z":
            break;
          default:
            var field = GetField(export.CollectionType, "code");

            field.Error = true;

            MoveCollectionType(import.FromFlow, export.CollectionType);
            export.PromptCollectionType.PromptField = "";
            ExitState = "FN0000_INVALID_COLL_TYPE";

            return;
        }

        MoveCollectionType(import.FromFlow, export.CollectionType);
        export.PromptCollectionType.PromptField = "";

        return;
      }
      else
      {
        export.PromptCollectionType.PromptField = "";

        return;
      }
    }

    if (Equal(global.Command, "RTLIST"))
    {
      export.PromptCsePersonNumber.SelectChar = "";
      export.HiddenCsePerson.Number = export.CsePerson.Number;
      MoveReceiptRefund3(export.ReceiptRefund, export.HiddenReceiptRefund);
      export.HiddenCashReceiptSourceType.Code =
        export.CashReceiptSourceType.Code;
      export.HiddenCollectionType.Code = export.CollectionType.Code;
      global.Command = "DISPLAY";
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
      export.HiddenNextTranInfo.CsePersonNumber = export.CsePerson.Number;
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
      UseScCabNextTranGet();
      export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (IsEmpty(export.CsePerson.Number))
      {
        ExitState = "FN0000_ENTER_ADVANCEMENT_INFO";

        return;
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    local.CsePersonsWorkSet.Number = export.CsePerson.Number;

    // Return formatted name for Valid entered CSE Person #.
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "SIGNOFF") && !
      Equal(global.Command, "CRAL") && !Equal(global.Command, "RETURN") && !
      Equal(global.Command, "RLCVAL"))
    {
      UseSiReadCsePerson2();

      if (IsExitState("CSE_PERSON_NF"))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        return;
      }
    }

    // Common Edit checking for Add & Update Advancements commands.
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (AsChar(export.HdisplayPerformedInd.Flag) != 'Y')
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      export.ReceiptRefund.Taxid =
        NumberToString(import.Taxid.AverageInteger, 7, 9);

      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
      else
      {
        // 03/20/00    PDP   Prevent manually enterd names from being overlaid
        // Added "IF" Statement
        if (IsEmpty(export.ReceiptRefund.PayeeName))
        {
          export.ReceiptRefund.PayeeName =
            export.CsePersonsWorkSet.FormattedName;
        }

        if (IsEmpty(export.CashReceiptDetailAddress.Street1) && IsEmpty
          (export.CashReceiptDetailAddress.Street2) && IsEmpty
          (export.CashReceiptDetailAddress.City) && IsEmpty
          (export.CashReceiptDetailAddress.State))
        {
          UseCabFnReadCsePersonAddress();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
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
      }

      if (IsEmpty(export.ReceiptRefund.Taxid))
      {
        export.ReceiptRefund.Taxid = export.CsePersonsWorkSet.Ssn;
        export.Taxid.AverageInteger =
          (int)StringToNumber(export.CsePersonsWorkSet.Ssn);
      }

      // *** Removed logic for validation of the source type.  Added logic for 
      // validation of the collection type.  Sunya Sharp 1/19/1999 ***
      // 06/22/12  GVandy CQ33628  Do not allow advancements on U type 
      // collections.
      //          (Removed CASE converting U collection type to SDSO source 
      // type.)
      switch(TrimEnd(export.CollectionType.Code))
      {
        case "F":
          local.Converted.Code = "FDSO";

          if (export.ReceiptRefund.OffsetTaxYear.GetValueOrDefault() == 0)
          {
            var field = GetField(export.ReceiptRefund, "offsetTaxYear");

            field.Error = true;

            ExitState = "FN0000_OFFSET_TAX_YEAR_REQUIRED";

            return;
          }

          break;
        case "S":
          local.Converted.Code = "SDSO";

          break;
        case "K":
          local.Converted.Code = "SDSO";

          break;
        case "R":
          local.Converted.Code = "SDSO";

          break;
        case "T":
          local.Converted.Code = "FDSO";

          break;
        case "Y":
          local.Converted.Code = "FDSO";

          break;
        case "Z":
          local.Converted.Code = "FDSO";

          break;
        case "":
          var field1 = GetField(export.CollectionType, "code");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        default:
          var field2 = GetField(export.CollectionType, "code");

          field2.Error = true;

          ExitState = "FN0000_INVALID_COLL_TYPE";

          return;
      }

      // *** Add logic to prevent the tax year from having a value if the 
      // collection type is not "F".  Also ensure that the tax year is not
      // greater than current year.  Sunya Sharp 1/27/1999 ***
      if (!Equal(export.CollectionType.Code, "F") && export
        .ReceiptRefund.OffsetTaxYear.GetValueOrDefault() > 0)
      {
        var field1 = GetField(export.ReceiptRefund, "offsetTaxYear");

        field1.Error = true;

        var field2 = GetField(export.CollectionType, "code");

        field2.Error = true;

        ExitState = "FN0000_OFFSET_TAX_YEAR_NOT_ALLOW";

        return;
      }

      if (export.ReceiptRefund.OffsetTaxYear.GetValueOrDefault() > Now
        ().Date.Year)
      {
        var field = GetField(export.ReceiptRefund, "offsetTaxYear");

        field.Error = true;

        ExitState = "FN0000_YEAR_INVALID";

        return;
      }

      if (export.ReceiptRefund.Amount == 0)
      {
        var field = GetField(export.ReceiptRefund, "amount");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      // *** Add logic to prevent the refund amount from being negative.  HEAT 
      // H00077672.  Sunya Sharp 10/18/1999. ***
      if (export.ReceiptRefund.Amount < 0)
      {
        var field = GetField(export.ReceiptRefund, "amount");

        field.Error = true;

        ExitState = "FN0000_AMT_CANNOT_BE_NEGATIVE";

        return;
      }

      if (IsEmpty(export.ReceiptRefund.PayeeName))
      {
        export.ReceiptRefund.PayeeName = export.CsePersonsWorkSet.FormattedName;
      }

      if (IsEmpty(export.CashReceiptDetailAddress.Street1))
      {
        var field = GetField(export.CashReceiptDetailAddress, "street1");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(export.CashReceiptDetailAddress.City))
      {
        var field = GetField(export.CashReceiptDetailAddress, "city");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(export.CashReceiptDetailAddress.State))
      {
        var field = GetField(export.CashReceiptDetailAddress, "state");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(export.CashReceiptDetailAddress.ZipCode5))
      {
        var field = GetField(export.CashReceiptDetailAddress, "zipCode5");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
      {
        return;
      }

      if (Length(TrimEnd(export.CashReceiptDetailAddress.ZipCode4)) < 4 && Length
        (TrimEnd(export.CashReceiptDetailAddress.ZipCode4)) > 0)
      {
        var field = GetField(export.CashReceiptDetailAddress, "zipCode4");

        field.Error = true;

        ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

        return;
      }

      // Checks for numeric entered Zipcode fields(5, 4 & 3 digit).
      if (Verify(export.CashReceiptDetailAddress.ZipCode5, "0123456789") > 0)
      {
        var field = GetField(export.CashReceiptDetailAddress, "zipCode5");

        field.Error = true;

        ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
      }

      // ***************************************************************************************
      // * Modified below mentioend verify statements comparision string to move
      // the spaces to *
      // * beginning of the string. CQ15282
      // 
      // *
      // ***************************************************************************************
      if (Verify(export.CashReceiptDetailAddress.ZipCode4, " 0123456789") > 0)
      {
        var field = GetField(export.CashReceiptDetailAddress, "zipCode4");

        field.Error = true;

        ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
      }

      if (Verify(export.CashReceiptDetailAddress.ZipCode3, " 0123456789") > 0)
      {
        var field = GetField(export.CashReceiptDetailAddress, "zipCode3");

        field.Error = true;

        ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
      }

      if (IsExitState("ACO_NE0000_ZIP_CODE_NOT_NUMERIC"))
      {
        return;
      }

      if (IsEmpty(export.ReceiptRefund.ReasonText))
      {
        var field = GetField(export.ReceiptRefund, "reasonText");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }

    // to validate action level security
    if (Equal(global.Command, "PRMPTRET") || Equal
      (global.Command, "RLCVAL") || Equal(global.Command, "CRRC"))
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

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (Equal(import.ReceiptRefund.CreatedTimestamp,
          local.InitializedReceiptRefund.Timestamp) || !
          Equal(export.CsePerson.Number, export.HiddenCsePerson.Number) && !
          IsEmpty(export.CsePerson.Number) || !
          Equal(export.CollectionType.Code, export.HiddenCollectionType.Code) &&
          !IsEmpty(export.CollectionType.Code) || export
          .ReceiptRefund.OffsetTaxYear.GetValueOrDefault() != export
          .HiddenReceiptRefund.OffsetTaxYear.GetValueOrDefault() && export
          .ReceiptRefund.OffsetTaxYear.GetValueOrDefault() != 0)
        {
          // *** Did not want to change the display logic but was having a 
          // problem displaying a single detail when coming from CRAL.  It falls
          // into this logic and finds multiples.  You would have to flow back
          // to CRAO select again and flow back.  This is a quick fix.  When
          // trying to mess with the IF statement above is messed up the
          // display.  Sunya Sharp 1/26/1999 ***
          if (AsChar(local.DisplayDetail.Flag) == 'Y')
          {
            local.ReceiptRefund.CreatedTimestamp =
              export.ReceiptRefund.CreatedTimestamp;

            goto Test;
          }

          if (!IsEmpty(export.CsePerson.Number))
          {
            if (ReadCsePerson())
            {
              if (AsChar(entities.CsePerson.Type1) == 'C')
              {
                local.CsePersonsWorkSet.Number = export.CsePerson.Number;
                UseSiReadCsePerson1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.CsePersonsWorkSet.FormattedName =
                    "**** ADABAS UNAVAILABLE ****";
                }
                else
                {
                  export.CsePersonsWorkSet.FormattedName =
                    local.CsePersonsWorkSet.FormattedName;
                  export.ReceiptRefund.PayeeName =
                    local.CsePersonsWorkSet.FormattedName;
                  export.Taxid.AverageInteger =
                    (int)StringToNumber(local.CsePersonsWorkSet.Ssn);
                }
              }
              else
              {
                export.CsePersonsWorkSet.FormattedName =
                  entities.CsePerson.OrganizationName ?? Spaces(33);
                export.ReceiptRefund.PayeeName =
                  entities.CsePerson.OrganizationName;
                export.Taxid.AverageInteger =
                  (int)StringToNumber(entities.CsePerson.TaxId);
              }
            }
            else
            {
              var field1 = GetField(export.CsePerson, "number");

              field1.Error = true;

              ExitState = "FN0000_CSE_PERSON_UNKNOWN";

              return;
            }

            UseSiGetCsePersonMailingAddr();

            if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
              (local.CsePersonAddress.Street2) && IsEmpty
              (local.CsePersonAddress.City) && IsEmpty
              (local.CsePersonAddress.State))
            {
              if (AsChar(entities.CsePerson.Type1) == 'O')
              {
                if (ReadFipsTribAddress())
                {
                  ExitState = "ACO_NN0000_ALL_OK";
                  export.SendTo.Street1 = entities.FipsTribAddress.Street1;
                  export.SendTo.Street2 = entities.FipsTribAddress.Street2;
                  export.SendTo.City = entities.FipsTribAddress.City;
                  export.SendTo.State = entities.FipsTribAddress.State;
                  export.SendTo.ZipCode5 = entities.FipsTribAddress.ZipCode;
                  export.SendTo.ZipCode4 = entities.FipsTribAddress.Zip4;
                  export.SendTo.ZipCode3 = entities.FipsTribAddress.Zip3;
                }
              }
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
            export.CsePersonsWorkSet.FormattedName = "";
            export.ReceiptRefund.PayeeName = "";
            export.Taxid.AverageInteger = 0;
            export.ReceiptRefund.RequestDate = local.Zero.Date;
            export.CashReceiptDetailAddress.Street1 = "";
            export.CashReceiptDetailAddress.Street2 = "";
            export.CashReceiptDetailAddress.City = "";
            export.CashReceiptDetailAddress.State = "";
            export.CashReceiptDetailAddress.ZipCode5 = "";
            export.CashReceiptDetailAddress.ZipCode4 = "";
            export.CashReceiptDetailAddress.ZipCode3 = "";
          }

          local.NumberOfEntitiesRead.Count = 0;

          foreach(var item in ReadReceiptRefundCashReceiptSourceType())
          {
            if (Equal(entities.CashReceiptSourceType.Code, "FDSO") || Equal
              (entities.CashReceiptSourceType.Code, "SDSO"))
            {
              ++local.NumberOfEntitiesRead.Count;

              if (local.NumberOfEntitiesRead.Count > 1)
              {
                break;
              }
            }
          }

          switch(local.NumberOfEntitiesRead.Count)
          {
            case 0:
              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

              break;
            case 1:
              local.ReceiptRefund.CreatedTimestamp =
                entities.ReceiptRefund.CreatedTimestamp;

              break;
            default:
              ExitState = "FN0000_MULTIPLE_ADVANCES_EXIST";

              break;
          }
        }
        else
        {
          local.ReceiptRefund.CreatedTimestamp =
            export.ReceiptRefund.CreatedTimestamp;
        }

Test:

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY") && !
          IsExitState("FN0000_MULTIPLE_ADVANCES_EXIST"))
        {
          if (ReadReceiptRefund())
          {
            MoveReceiptRefund2(entities.ReceiptRefund, export.ReceiptRefund);

            if (Equal(export.ReceiptRefund.RequestDate,
              local.InitializedDateWorkArea.Date))
            {
              export.ReceiptRefund.RequestDate = Now().Date;
            }

            if (IsEmpty(export.ReceiptRefund.OffsetClosed))
            {
              export.ReceiptRefund.OffsetClosed = "N";
            }

            if (!IsEmpty(entities.ReceiptRefund.Taxid))
            {
              export.Taxid.AverageInteger =
                (int)StringToNumber(entities.ReceiptRefund.Taxid);
            }

            if (IsEmpty(export.ReceiptRefund.LastUpdatedBy))
            {
              export.ReceiptRefund.LastUpdatedBy =
                export.ReceiptRefund.CreatedBy;
            }

            if (ReadCashReceiptSourceType())
            {
              export.CashReceiptSourceType.Code =
                entities.CashReceiptSourceType.Code;
            }
            else
            {
              ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

              return;
            }

            if (ReadCollectionType())
            {
              MoveCollectionType(entities.CollectionType, export.CollectionType);
                
            }
            else
            {
              ExitState = "FN0000_COLLECTION_TYPE_NF";

              return;
            }
          }
          else
          {
            ExitState = "SELECTED_ADVANCEMENT_NF";

            return;
          }

          if (ReadCashReceiptDetail())
          {
            export.CashReceiptDetail.CollectionAmount =
              entities.CashReceiptDetail.CollectionAmount;
          }

          if (ReadCashReceiptDetailAddress())
          {
            export.CashReceiptDetailAddress.Assign(
              entities.CashReceiptDetailAddress);
          }
          else
          {
            ExitState = "FN0039_CASH_RCPT_DTL_ADDR_NF";

            return;
          }

          // *** Add changes per PR#120.  Sunya Sharp 08/23/1999 ***
          if (ReadPaymentRequest())
          {
            MovePaymentRequest3(entities.PaymentRequest, export.PaymentRequest);

            if (ReadPaymentStatusHistoryPaymentStatus())
            {
              export.PaymentStatus.Code = entities.PaymentStatus.Code;
            }
          }
          else
          {
            // OKAY, Did not want to find one.
          }

          if (AsChar(export.PaymentRequest.RecoupmentIndKpc) == 'Y')
          {
            ExitState = "FN0000_DISPLAY_SUCC_KPC_RECOUP";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }
        else
        {
          export.CollectionType.Code = "";
          export.CashReceiptSourceType.Code = "";
          export.ReceiptRefund.Amount = 0;
          export.ReceiptRefund.RequestDate = local.Zero.Date;
          export.CashReceiptDetail.CollectionAmount = 0;
          export.ReceiptRefund.OffsetTaxYear = 0;
          export.ReceiptRefund.OffsetClosed = "";
          export.ReceiptRefund.LastUpdatedBy = "";
          export.ReceiptRefund.ReasonText =
            Spaces(ReceiptRefund.ReasonText_MaxLength);
          export.ReceiptRefund.CreatedTimestamp =
            local.InitializedReceiptRefund.Timestamp;
          export.PaymentStatus.Code = local.InitializePaymentStatus.Code;
          MovePaymentRequest4(local.InitializedPaymentRequest,
            export.PaymentRequest);
        }

        MoveCollectionType(export.CollectionType, export.HiddenCollectionType);
        export.HiddenCashReceiptSourceType.Code =
          export.CashReceiptSourceType.Code;
        export.HiddenCsePerson.Number = export.CsePerson.Number;
        MoveReceiptRefund3(export.ReceiptRefund, export.HiddenReceiptRefund);
        export.HdisplayPerformedInd.Flag = "Y";

        break;
      case "LIST":
        // This case allows the searching of CSE Person numbers and State codes 
        // from listing.
        switch(AsChar(export.PromptCsePersonNumber.SelectChar))
        {
          case 'S':
            // Check for multiples.
            if (AsChar(export.PromptCollectionType.PromptField) == 'S')
            {
              var field2 = GetField(export.PromptCollectionType, "promptField");

              field2.Error = true;

              var field3 = GetField(export.PromptCsePersonNumber, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.PromptStateCode.SelectChar) == 'S')
            {
              var field2 = GetField(export.PromptStateCode, "selectChar");

              field2.Error = true;

              var field3 = GetField(export.PromptCsePersonNumber, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              return;
            }

            // Otherwise flow to screen.
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          case ' ':
            break;
          default:
            // Invalid selection character.  Must use "S" in selection field to 
            // pick CSE Person number from listing.
            var field1 = GetField(export.PromptCsePersonNumber, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        switch(AsChar(export.PromptStateCode.SelectChar))
        {
          case 'S':
            // Check for multiples.
            if (AsChar(export.PromptCollectionType.PromptField) == 'S')
            {
              var field2 = GetField(export.PromptCollectionType, "promptField");

              field2.Error = true;

              var field3 = GetField(export.PromptStateCode, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.PromptCsePersonNumber.SelectChar) == 'S')
            {
              var field2 = GetField(export.PromptStateCode, "selectChar");

              field2.Error = true;

              var field3 = GetField(export.PromptCsePersonNumber, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              return;
            }

            // Otherwise flow to screen.
            export.StateCode.CodeName = "STATE CODE";
            export.StartingState.Cdvalue =
              export.CashReceiptDetailAddress.State;
            export.PromptStateCode.SelectChar = "";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          case ' ':
            break;
          default:
            // Invalid selection character.  Must use "S" in selection field to 
            // pick State code.
            var field1 = GetField(export.PromptStateCode, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        switch(AsChar(export.PromptCollectionType.PromptField))
        {
          case 'S':
            // Check for multiples.
            if (AsChar(export.PromptStateCode.SelectChar) == 'S')
            {
              var field2 = GetField(export.PromptCollectionType, "promptField");

              field2.Error = true;

              var field3 = GetField(export.PromptStateCode, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.PromptCsePersonNumber.SelectChar) == 'S')
            {
              var field2 = GetField(export.PromptCollectionType, "promptField");

              field2.Error = true;

              var field3 = GetField(export.PromptCsePersonNumber, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              return;
            }

            // Otherwise flow to screen.
            export.PromptCollectionType.PromptField = "";
            ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";

            return;
          case ' ':
            break;
          default:
            // Invalid selection character.  Must use "S" in selection field to 
            // pick State code.
            var field1 = GetField(export.PromptCollectionType, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        if (AsChar(export.PromptCsePersonNumber.SelectChar) != 'S' && AsChar
          (export.PromptStateCode.SelectChar) != 'S' && AsChar
          (export.PromptCollectionType.PromptField) != 'S')
        {
          var field1 = GetField(export.PromptCsePersonNumber, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptStateCode, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.PromptCollectionType, "promptField");

          field3.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case "RLCVAL":
        // -----------------------------------------------------------------
        // Return from List Code Value procedure.
        // -----------------------------------------------------------------
        if (!IsEmpty(import.StateCode.Cdvalue))
        {
          export.CashReceiptDetailAddress.State = import.StateCode.Cdvalue;
        }

        export.PromptStateCode.SelectChar = "";

        var field = GetField(export.CashReceiptDetailAddress, "state");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;

        break;
      case "ADD":
        // Default Area for Case ADD Fields
        // *****
        // Validate Entered state
        // *****
        local.Pass.Cdvalue = export.CashReceiptDetailAddress.State;
        local.Code.CodeName = "STATE CODE";
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field1 = GetField(export.CashReceiptDetailAddress, "state");

          field1.Error = true;

          ExitState = "CODE_NF";

          return;
        }

        export.ReceiptRefund.ReasonCode = "ADVANCE";
        export.CashReceiptDetail.CollectionAmount = 0;

        // End of Default Area for Case Add Fields
        export.PaymentRequest.Amount = export.ReceiptRefund.Amount;
        export.PaymentRequest.Classification = "ADV";
        export.PaymentRequest.CsePersonNumber = export.CsePerson.Number;
        export.PaymentRequest.Type1 = "WAR";
        export.PaymentRequest.Number = "";
        export.PaymentRequest.RecoupmentIndKpc = "";
        export.PaymentRequest.PrintDate = local.Zero.Date;
        export.ReceiptRefund.RequestDate = Now().Date;
        UseRequestAdvancement();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.PaymentRequest.Number = "";
          export.PaymentRequest.RecoupmentIndKpc = "";
          export.PaymentRequest.PrintDate =
            local.InitializedPaymentRequest.PrintDate;
          export.ReceiptRefund.LastUpdatedBy = export.ReceiptRefund.CreatedBy;
          export.HiddenCashReceiptSourceType.Code =
            export.CashReceiptSourceType.Code;
          MoveCollectionType(export.CollectionType, export.HiddenCollectionType);
            
          export.HiddenCsePerson.Number = export.CsePerson.Number;

          if (IsEmpty(export.ReceiptRefund.OffsetClosed))
          {
            export.ReceiptRefund.OffsetClosed = "N";
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

          return;
        }

        if (IsExitState("CSE_PERSON_NF"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          return;
        }

        if (IsExitState("FN0038_CASH_RCPT_DTL_ADDR_AE"))
        {
          var field1 = GetField(export.CashReceiptDetailAddress, "street1");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetailAddress, "city");

          field2.Error = true;

          var field3 = GetField(export.CashReceiptDetailAddress, "state");

          field3.Error = true;

          var field4 = GetField(export.CashReceiptDetailAddress, "zipCode5");

          field4.Error = true;

          return;
        }

        if (IsExitState("FN0041_CASH_RCPT_DTL_ADDR_PV"))
        {
          var field1 = GetField(export.CashReceiptDetailAddress, "street1");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetailAddress, "city");

          field2.Error = true;

          var field3 = GetField(export.CashReceiptDetailAddress, "state");

          field3.Error = true;

          var field4 = GetField(export.CashReceiptDetailAddress, "zipCode5");

          field4.Error = true;

          return;
        }

        if (IsExitState("FN0097_CASH_RCPT_SOURCE_TYPE_NF") || IsExitState
          ("FN0000_COLLECTION_TYPE_NF"))
        {
          var field1 = GetField(export.CollectionType, "code");

          field1.Error = true;
        }

        break;
      case "UPDATE":
        // Offset Type Update not allowed for existing receipt refund.  Compare 
        // export or screen Offset Type value against hidden or data base Offset
        // Type.  If they do not match, the offset type field is highlighted in
        // error.
        // *** Changed logic to check the collection type code instead of the 
        // cash receipt source type code.  Sunya Sharp 1/26/1999 ***
        if (!Equal(export.CollectionType.Code, export.HiddenCollectionType.Code))
          
        {
          var field1 = GetField(export.CollectionType, "code");

          field1.Error = true;

          ExitState = "FN0000_NO_UPDATE_OFFSET_TYPE";

          return;
        }

        // CSE Person # Update not allowed for existing receipt refund.  Compare
        // export or screen CSE Person # against hidden or data base CSE Person
        // #.  If they do not match, the CSE Person # field is highlighted in
        // error.
        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "FN0000_NO_UPDATE_CSE_PERSON_NMBR";

          return;
        }

        // --- Deleted logic that disallowed offset_tax_yr to be EQUAL TO the 
        // current year. (Problem # 26550) ---
        // *****
        // Validate Entered state
        // *****
        local.Pass.Cdvalue = export.CashReceiptDetailAddress.State;
        local.Code.CodeName = "STATE CODE";
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field1 = GetField(export.CashReceiptDetailAddress, "state");

          field1.Error = true;

          ExitState = "CODE_NF";

          return;
        }

        if (IsEmpty(export.ReceiptRefund.ReasonText))
        {
          var field1 = GetField(export.ReceiptRefund, "reasonText");

          field1.Error = true;

          ExitState = "FN0000_MANDATORY_FIELDS";

          return;
        }

        UseUpdateAdvancement();

        // Receipt refund update not allowed when warrant linked to it.
        // *** User would like the status highlighted instead of the print date 
        // and number.  Sunya Sharp 1/27/1999 ***
        if (IsExitState("FN0000_ADVANCEMENT_HAS_WARRANT"))
        {
          var field1 = GetField(export.PaymentStatus, "code");

          field1.Error = true;

          return;
        }

        // Receipt refund update not allowed when cash receipt detail or 
        // collection receipt exists.
        if (IsExitState("FN0000_ADVANCEMENT_HAS_RCPT_DTL"))
        {
          var field1 = GetField(export.CashReceiptDetail, "collectionAmount");

          field1.Color = "red";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = true;

          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        // *** Add logic to prevent the user from deleting an advancement before
        // displaying it first.  Sunya Sharp 1/27/1999 ***
        if (AsChar(export.HdisplayPerformedInd.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

          return;
        }

        UseCancelAdvancement();

        // Receipt refund cancellation not allowed when warrant linked to it.
        // *** User would like the status highlighted instead of the print date 
        // and number.  Sunya Sharp 1/27/1999 ***
        if (IsExitState("FN0000_ADVANCEMENT_HAS_WARRANT"))
        {
          var field1 = GetField(export.PaymentStatus, "code");

          field1.Error = true;

          return;
        }

        // Receipt refund cancellation not allowed when cash receipt detail or 
        // collection receipt exists.
        if (IsExitState("FN0000_ADVANCEMENT_HAS_RCPT_DTL"))
        {
          var field1 = GetField(export.CashReceiptDetail, "collectionAmount");

          field1.Color = "red";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = true;

          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CashReceiptDetailAddress.Assign(
            local.InitializeCashReceiptDetailAddress);
          export.ReceiptRefund.Assign(local.InitializeReceiptRefund);
          export.PaymentStatus.Code = local.InitializePaymentStatus.Code;
          MovePaymentRequest2(local.InitializePaymentRequest,
            export.PaymentRequest);
          export.CashReceiptSourceType.Code = "";
          export.CollectionType.Code = "";
          export.Taxid.AverageInteger = 0;
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      case "CLOSE":
        if (AsChar(export.ReceiptRefund.OffsetClosed) == 'Y')
        {
          var field1 = GetField(export.ReceiptRefund, "offsetClosed");

          field1.Error = true;

          ExitState = "FN0000_OFFSET_ALREADY_CLOSED";

          return;
        }

        UseFnAbCloseAdvancement();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          ExitState = "FN0000_OFFSET_CLOSED";
        }

        if (IsEmpty(export.ReceiptRefund.OffsetClosed))
        {
          export.ReceiptRefund.OffsetClosed = "N";
        }

        break;
      case "CRAL":
        // *** User would like the closed indicator on CRAL to be "Y" when 
        // coming from CRAO and a person number is passed.  Sunya Sharp 1/19/
        // 1999 ***
        // *** User requested that only valid collection types be allowed to 
        // flow to CRAL.  Sunya Sharp 5/15/1999 ***
        switch(TrimEnd(export.CollectionType.Code))
        {
          case "F":
            break;
          case "S":
            break;
          case "U":
            break;
          case "K":
            break;
          case "R":
            break;
          case "T":
            break;
          case "Y":
            break;
          case "Z":
            break;
          default:
            var field1 = GetField(export.CollectionType, "code");

            field1.Error = true;

            ExitState = "FN0000_INVALID_COLL_TYPE";

            return;
        }

        if (!IsEmpty(export.CsePerson.Number))
        {
          export.PassToCralClosed.Flag = "Y";
        }

        ExitState = "ECO_LNK_TO_LIST_ADVANCEMENTS";

        break;
      case "CRRC":
        // *** User would like to be able to flow to CRRC to view the cash 
        // receipt detail associated to the advancement.  Sunya Sharp 1/28/1999
        // ***
        if (export.CashReceiptDetail.CollectionAmount > 0)
        {
          if (ReadCashReceiptDetailCashReceiptCashReceiptEvent())
          {
            export.ToCrrcCashReceipt.SequentialNumber =
              entities.CrrcCashReceipt.SequentialNumber;
            export.ToCrrcCashReceiptDetail.SequentialIdentifier =
              entities.CrrcCashReceiptDetail.SequentialIdentifier;
            export.ToCrrcCashReceiptEvent.SystemGeneratedIdentifier =
              entities.CrrcCashReceiptEvent.SystemGeneratedIdentifier;
            export.ToCrrcCashReceiptSourceType.SystemGeneratedIdentifier =
              entities.CrrcCashReceiptSourceType.SystemGeneratedIdentifier;
            export.ToCrrcCashReceiptType.SystemGeneratedIdentifier =
              entities.CrrcCashReceiptType.SystemGeneratedIdentifier;
          }

          ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";
        }
        else
        {
          ExitState = "FN0000_NO_FLOW_NO_DETAIL_FOR_REF";
        }

        break;
      case "RETURN":
        // ------------------------------------------------------------
        // The RETURN command should be valid for the return flow from
        // CRAO to CRAL.
        // ------------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // Add any common logic that must occur at
    // the end of every pass.
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
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

  private static void MovePaymentRequest1(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Classification = source.Classification;
    target.Amount = source.Amount;
    target.CsePersonNumber = source.CsePersonNumber;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Classification = source.Classification;
    target.Amount = source.Amount;
    target.CsePersonNumber = source.CsePersonNumber;
    target.RecoupmentIndKpc = source.RecoupmentIndKpc;
  }

  private static void MovePaymentRequest3(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.RecoupmentIndKpc = source.RecoupmentIndKpc;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private static void MovePaymentRequest4(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.RecoupmentIndKpc = source.RecoupmentIndKpc;
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
    target.OffsetClosed = source.OffsetClosed;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private static void MoveReceiptRefund3(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.RequestDate = source.RequestDate;
  }

  private void UseCabFnReadCsePersonAddress()
  {
    var useImport = new CabFnReadCsePersonAddress.Import();
    var useExport = new CabFnReadCsePersonAddress.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabFnReadCsePersonAddress.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
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

  private void UseCancelAdvancement()
  {
    var useImport = new CancelAdvancement.Import();
    var useExport = new CancelAdvancement.Export();

    useImport.ReceiptRefund.CreatedTimestamp =
      export.ReceiptRefund.CreatedTimestamp;

    Call(CancelAdvancement.Execute, useImport, useExport);
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

  private void UseFnAbCloseAdvancement()
  {
    var useImport = new FnAbCloseAdvancement.Import();
    var useExport = new FnAbCloseAdvancement.Export();

    useImport.ReceiptRefund.CreatedTimestamp =
      export.ReceiptRefund.CreatedTimestamp;

    Call(FnAbCloseAdvancement.Execute, useImport, useExport);

    export.ReceiptRefund.Assign(useExport.ReceiptRefund);
  }

  private void UseRequestAdvancement()
  {
    var useImport = new RequestAdvancement.Import();
    var useExport = new RequestAdvancement.Export();

    useImport.CashReceiptSourceType.Code = local.Converted.Code;
    MovePaymentRequest1(export.PaymentRequest, useImport.PaymentRequest);
    useImport.CashReceiptDetailAddress.Assign(export.CashReceiptDetailAddress);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.ReceiptRefund.Assign(export.ReceiptRefund);
    MoveCollectionType(export.CollectionType, useImport.CollectionType);

    Call(RequestAdvancement.Execute, useImport, useExport);

    export.CashReceiptDetailAddress.Assign(useExport.CashReceiptDetailAddress);
    MoveReceiptRefund1(useExport.ReceiptRefund, export.ReceiptRefund);
    export.CashReceiptSourceType.Code = useExport.CashReceiptSourceType.Code;
    export.PaymentStatus.Code = useExport.PaymentStatus.Code;
    MoveCollectionType(useExport.CollectionType, export.CollectionType);
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
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseUpdateAdvancement()
  {
    var useImport = new UpdateAdvancement.Import();
    var useExport = new UpdateAdvancement.Export();

    MovePaymentRequest1(export.PaymentRequest, useImport.PaymentRequest);
    useImport.CashReceiptDetailAddress.Assign(export.CashReceiptDetailAddress);
    useImport.ReceiptRefund.Assign(export.ReceiptRefund);

    Call(UpdateAdvancement.Execute, useImport, useExport);

    MovePaymentRequest1(useExport.PaymentRequest, export.PaymentRequest);
    export.ReceiptRefund.Assign(useExport.ReceiptRefund);
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
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.ReceiptRefund.CdaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.Street1 = db.GetString(reader, 1);
        entities.CashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.CashReceiptDetailAddress.City = db.GetString(reader, 3);
        entities.CashReceiptDetailAddress.State = db.GetString(reader, 4);
        entities.CashReceiptDetailAddress.ZipCode5 = db.GetString(reader, 5);
        entities.CashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceiptCashReceiptEvent()
  {
    entities.CrrcCashReceiptType.Populated = false;
    entities.CrrcCashReceiptSourceType.Populated = false;
    entities.CrrcCashReceiptEvent.Populated = false;
    entities.CrrcCashReceiptDetail.Populated = false;
    entities.CrrcCashReceipt.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptCashReceiptEvent",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CrrcCashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CrrcCashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CrrcCashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CrrcCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CrrcCashReceipt.CrvIdentifier = db.GetInt32(reader, 4);
        entities.CrrcCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CrrcCashReceipt.CstIdentifier = db.GetInt32(reader, 5);
        entities.CrrcCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CrrcCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CrrcCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CrrcCashReceipt.CrtIdentifier = db.GetInt32(reader, 6);
        entities.CrrcCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.CrrcCashReceipt.SequentialNumber = db.GetInt32(reader, 7);
        entities.CrrcCashReceiptType.Populated = true;
        entities.CrrcCashReceiptSourceType.Populated = true;
        entities.CrrcCashReceiptEvent.Populated = true;
        entities.CrrcCashReceiptDetail.Populated = true;
        entities.CrrcCashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          entities.ReceiptRefund.CstAIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.ReceiptRefund.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.CreatedBy = db.GetString(reader, 4);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CsePerson.TaxIdSuffix = db.GetNullableString(reader, 8);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 9);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 10);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 11);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 1);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 2);
        entities.PaymentRequest.Type1 = db.GetString(reader, 3);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatusHistoryPaymentStatus()
  {
    entities.PaymentStatusHistory.Populated = false;
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatusHistoryPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.PaymentStatus.Code = db.GetString(reader, 4);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadReceiptRefund()
  {
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 12);
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 13);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 14);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 17);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 18);
        entities.ReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ReceiptRefund.OffsetClosed);
      });
  }

  private IEnumerable<bool> ReadReceiptRefundCashReceiptSourceType()
  {
    entities.ReceiptRefund.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

    return ReadEach("ReadReceiptRefundCashReceiptSourceType",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 12);
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 13);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 14);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 17);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 18);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 19);
        entities.ReceiptRefund.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ReceiptRefund.OffsetClosed);

        return true;
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
    /// A value of PromptCollectionType.
    /// </summary>
    [JsonPropertyName("promptCollectionType")]
    public Standard PromptCollectionType
    {
      get => promptCollectionType ??= new();
      set => promptCollectionType = value;
    }

    /// <summary>
    /// A value of FromFlow.
    /// </summary>
    [JsonPropertyName("fromFlow")]
    public CollectionType FromFlow
    {
      get => fromFlow ??= new();
      set => fromFlow = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of HdisplayPerformedInd.
    /// </summary>
    [JsonPropertyName("hdisplayPerformedInd")]
    public Common HdisplayPerformedInd
    {
      get => hdisplayPerformedInd ??= new();
      set => hdisplayPerformedInd = value;
    }

    /// <summary>
    /// A value of Taxid.
    /// </summary>
    [JsonPropertyName("taxid")]
    public Common Taxid
    {
      get => taxid ??= new();
      set => taxid = value;
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
    /// A value of StateCode.
    /// </summary>
    [JsonPropertyName("stateCode")]
    public CodeValue StateCode
    {
      get => stateCode ??= new();
      set => stateCode = value;
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
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of HiddenReceiptRefund.
    /// </summary>
    [JsonPropertyName("hiddenReceiptRefund")]
    public ReceiptRefund HiddenReceiptRefund
    {
      get => hiddenReceiptRefund ??= new();
      set => hiddenReceiptRefund = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
    }

    /// <summary>
    /// A value of PromptStateCode.
    /// </summary>
    [JsonPropertyName("promptStateCode")]
    public Common PromptStateCode
    {
      get => promptStateCode ??= new();
      set => promptStateCode = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private Standard promptCollectionType;
    private CollectionType fromFlow;
    private CollectionType hiddenCollectionType;
    private CollectionType collectionType;
    private Common hdisplayPerformedInd;
    private Common taxid;
    private CashReceiptDetail cashReceiptDetail;
    private CodeValue stateCode;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson hiddenCsePerson;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
    private ReceiptRefund hiddenReceiptRefund;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CsePerson csePerson;
    private Common promptCsePersonNumber;
    private Common promptStateCode;
    private CashReceiptSourceType cashReceiptSourceType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private PaymentStatus paymentStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ToCrrcCashReceiptType.
    /// </summary>
    [JsonPropertyName("toCrrcCashReceiptType")]
    public CashReceiptType ToCrrcCashReceiptType
    {
      get => toCrrcCashReceiptType ??= new();
      set => toCrrcCashReceiptType = value;
    }

    /// <summary>
    /// A value of ToCrrcCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("toCrrcCashReceiptSourceType")]
    public CashReceiptSourceType ToCrrcCashReceiptSourceType
    {
      get => toCrrcCashReceiptSourceType ??= new();
      set => toCrrcCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ToCrrcCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("toCrrcCashReceiptEvent")]
    public CashReceiptEvent ToCrrcCashReceiptEvent
    {
      get => toCrrcCashReceiptEvent ??= new();
      set => toCrrcCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ToCrrcCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("toCrrcCashReceiptDetail")]
    public CashReceiptDetail ToCrrcCashReceiptDetail
    {
      get => toCrrcCashReceiptDetail ??= new();
      set => toCrrcCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ToCrrcCashReceipt.
    /// </summary>
    [JsonPropertyName("toCrrcCashReceipt")]
    public CashReceipt ToCrrcCashReceipt
    {
      get => toCrrcCashReceipt ??= new();
      set => toCrrcCashReceipt = value;
    }

    /// <summary>
    /// A value of PromptCollectionType.
    /// </summary>
    [JsonPropertyName("promptCollectionType")]
    public Standard PromptCollectionType
    {
      get => promptCollectionType ??= new();
      set => promptCollectionType = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of PassToCralClosed.
    /// </summary>
    [JsonPropertyName("passToCralClosed")]
    public Common PassToCralClosed
    {
      get => passToCralClosed ??= new();
      set => passToCralClosed = value;
    }

    /// <summary>
    /// A value of HdisplayPerformedInd.
    /// </summary>
    [JsonPropertyName("hdisplayPerformedInd")]
    public Common HdisplayPerformedInd
    {
      get => hdisplayPerformedInd ??= new();
      set => hdisplayPerformedInd = value;
    }

    /// <summary>
    /// A value of Taxid.
    /// </summary>
    [JsonPropertyName("taxid")]
    public Common Taxid
    {
      get => taxid ??= new();
      set => taxid = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of StateCode.
    /// </summary>
    [JsonPropertyName("stateCode")]
    public Code StateCode
    {
      get => stateCode ??= new();
      set => stateCode = value;
    }

    /// <summary>
    /// A value of StartingState.
    /// </summary>
    [JsonPropertyName("startingState")]
    public CodeValue StartingState
    {
      get => startingState ??= new();
      set => startingState = value;
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
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
    }

    /// <summary>
    /// A value of PromptStateCode.
    /// </summary>
    [JsonPropertyName("promptStateCode")]
    public Common PromptStateCode
    {
      get => promptStateCode ??= new();
      set => promptStateCode = value;
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
    /// A value of HiddenReceiptRefund.
    /// </summary>
    [JsonPropertyName("hiddenReceiptRefund")]
    public ReceiptRefund HiddenReceiptRefund
    {
      get => hiddenReceiptRefund ??= new();
      set => hiddenReceiptRefund = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
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

    private CashReceiptType toCrrcCashReceiptType;
    private CashReceiptSourceType toCrrcCashReceiptSourceType;
    private CashReceiptEvent toCrrcCashReceiptEvent;
    private CashReceiptDetail toCrrcCashReceiptDetail;
    private CashReceipt toCrrcCashReceipt;
    private Standard promptCollectionType;
    private CollectionType hiddenCollectionType;
    private CollectionType collectionType;
    private Common passToCralClosed;
    private Common hdisplayPerformedInd;
    private Common taxid;
    private PaymentRequest set;
    private CashReceiptDetail cashReceiptDetail;
    private Code stateCode;
    private CodeValue startingState;
    private CsePerson hiddenCsePerson;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private PaymentRequest paymentRequest;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CsePerson csePerson;
    private Common promptCsePersonNumber;
    private Common promptStateCode;
    private ReceiptRefund receiptRefund;
    private ReceiptRefund hiddenReceiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private PaymentStatus paymentStatus;
    private CashReceiptDetailAddress sendTo;
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
    /// A value of DisplayDetail.
    /// </summary>
    [JsonPropertyName("displayDetail")]
    public Common DisplayDetail
    {
      get => displayDetail ??= new();
      set => displayDetail = value;
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
    /// A value of NumberOfEntitiesRead.
    /// </summary>
    [JsonPropertyName("numberOfEntitiesRead")]
    public Common NumberOfEntitiesRead
    {
      get => numberOfEntitiesRead ??= new();
      set => numberOfEntitiesRead = value;
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
    /// A value of InitializeCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("initializeCashReceiptDetailAddress")]
    public CashReceiptDetailAddress InitializeCashReceiptDetailAddress
    {
      get => initializeCashReceiptDetailAddress ??= new();
      set => initializeCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of InitializePaymentStatus.
    /// </summary>
    [JsonPropertyName("initializePaymentStatus")]
    public PaymentStatus InitializePaymentStatus
    {
      get => initializePaymentStatus ??= new();
      set => initializePaymentStatus = value;
    }

    /// <summary>
    /// A value of InitializeReceiptRefund.
    /// </summary>
    [JsonPropertyName("initializeReceiptRefund")]
    public ReceiptRefund InitializeReceiptRefund
    {
      get => initializeReceiptRefund ??= new();
      set => initializeReceiptRefund = value;
    }

    /// <summary>
    /// A value of InitializePaymentRequest.
    /// </summary>
    [JsonPropertyName("initializePaymentRequest")]
    public PaymentRequest InitializePaymentRequest
    {
      get => initializePaymentRequest ??= new();
      set => initializePaymentRequest = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of InitializedPaymentRequest.
    /// </summary>
    [JsonPropertyName("initializedPaymentRequest")]
    public PaymentRequest InitializedPaymentRequest
    {
      get => initializedPaymentRequest ??= new();
      set => initializedPaymentRequest = value;
    }

    /// <summary>
    /// A value of InitializedReceiptRefund.
    /// </summary>
    [JsonPropertyName("initializedReceiptRefund")]
    public DateWorkArea InitializedReceiptRefund
    {
      get => initializedReceiptRefund ??= new();
      set => initializedReceiptRefund = value;
    }

    /// <summary>
    /// A value of Converted.
    /// </summary>
    [JsonPropertyName("converted")]
    public CashReceiptSourceType Converted
    {
      get => converted ??= new();
      set => converted = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private DateWorkArea current;
    private Common displayDetail;
    private Common csePersonAddressExists;
    private Common numberOfEntitiesRead;
    private DateWorkArea zero;
    private CashReceiptDetailAddress initializeCashReceiptDetailAddress;
    private PaymentStatus initializePaymentStatus;
    private ReceiptRefund initializeReceiptRefund;
    private PaymentRequest initializePaymentRequest;
    private CsePersonAddress csePersonAddress;
    private ReceiptRefund receiptRefund;
    private PaymentRequest initializedPaymentRequest;
    private DateWorkArea initializedReceiptRefund;
    private CashReceiptSourceType converted;
    private DateWorkArea initializedDateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue pass;
    private Code code;
    private Common returnCode;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CrrcCashReceiptType.
    /// </summary>
    [JsonPropertyName("crrcCashReceiptType")]
    public CashReceiptType CrrcCashReceiptType
    {
      get => crrcCashReceiptType ??= new();
      set => crrcCashReceiptType = value;
    }

    /// <summary>
    /// A value of CrrcCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("crrcCashReceiptSourceType")]
    public CashReceiptSourceType CrrcCashReceiptSourceType
    {
      get => crrcCashReceiptSourceType ??= new();
      set => crrcCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CrrcCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("crrcCashReceiptEvent")]
    public CashReceiptEvent CrrcCashReceiptEvent
    {
      get => crrcCashReceiptEvent ??= new();
      set => crrcCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CrrcCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("crrcCashReceiptDetail")]
    public CashReceiptDetail CrrcCashReceiptDetail
    {
      get => crrcCashReceiptDetail ??= new();
      set => crrcCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CrrcCashReceipt.
    /// </summary>
    [JsonPropertyName("crrcCashReceipt")]
    public CashReceipt CrrcCashReceipt
    {
      get => crrcCashReceipt ??= new();
      set => crrcCashReceipt = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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

    private CashReceiptType crrcCashReceiptType;
    private CashReceiptSourceType crrcCashReceiptSourceType;
    private CashReceiptEvent crrcCashReceiptEvent;
    private CashReceiptDetail crrcCashReceiptDetail;
    private CashReceipt crrcCashReceipt;
    private CollectionType collectionType;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private CsePersonAddress csePersonAddress;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private CsePerson csePerson;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private ReceiptRefund receiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
    private PaymentRequest paymentRequest;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}

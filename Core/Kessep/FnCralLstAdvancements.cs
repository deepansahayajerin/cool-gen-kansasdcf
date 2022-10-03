// Program: FN_CRAL_LST_ADVANCEMENTS, ID: 372302987, model: 746.
// Short name: SWECRALP
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
/// A program: FN_CRAL_LST_ADVANCEMENTS.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This procedure will list advancements (which are receipt-refunds) for a CSE 
/// Person.   It allows 2 options:  list advancements &amp; view advancement
/// details.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCralLstAdvancements: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRAL_LST_ADVANCEMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCralLstAdvancements(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCralLstAdvancements.
  /// </summary>
  public FnCralLstAdvancements(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Date 	  Developer Name	Description
    // 02/06/96  Holly Kennedy-MTW	Retrofits
    // 12/02/96  R. Marchman		Add new security and next tran
    // 09/11/97  Siraj Konkader
    // During display, group view should not be populated when there is a 
    // potential NEXT.
    // Entity view CASH RECEIPT DETAIL did not contain COLLECTION AMOUNT.
    // 12/10/97  Syed Hasan            Modified code to display default
    //                                 
    // dates as requested on problem
    // report # 28535.
    // 12/28/98	Sunya Sharp	Make changes per screen assessment.
    // 08/23/99	Sunya Sharp	Make changes for PR#120.  Screen is not displaying 
    // advancements that have both warrants and potential recoveries.
    // 11/20/01	Kalpesh Doshi	WR020147 - KPC Recoupment. Add new flag to list.
    // ----------------------------------------------------------------------
    // Set Initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // Move Imports to Exports.
    export.CsePerson.Number = import.CsePerson.Number;
    export.SearchCsePersonNumber.SelectChar =
      import.SearchCsePersonNumber.SelectChar;
    export.OffsetType.PromptField = import.OffsetType.PromptField;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.Input.Code = import.CollectionType.Code;
    export.InputFrom.Date = import.From.Date;

    if (IsEmpty(import.Closed.Flag))
    {
      export.InputClosed.Flag = "N";
    }
    else
    {
      export.InputClosed.Flag = import.Closed.Flag;
    }

    export.InputTo.Date = import.To.Date;
    export.SelectedReceiptRefund.CreatedTimestamp =
      import.Selected.CreatedTimestamp;

    if (Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
        export.SearchCsePersonNumber.SelectChar = "";
        global.Command = "DISPLAY";
      }
      else
      {
        export.CsePersonsWorkSet.FormattedName = "";
        export.SearchCsePersonNumber.SelectChar = "";
      }
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      UseSiReadCsePerson2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        return;
      }
    }
    else
    {
      export.CsePersonsWorkSet.FormattedName = "";
    }

    // *** A data model change was requested to include the collection type to 
    // further define the receipt refund.  This is the new logic when returning
    // CLCT (List collection types).  Not all collection types are allowed on
    // this screen.  The SME wanted an error when returning to inform the user
    // that an invalid selection was made prior to the display.  Sunya Sharp 1/4
    // /1999 ***
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
            var field1 = GetField(export.Input, "code");

            field1.Error = true;

            export.Input.Code = import.FromFlow.Code;
            export.OffsetType.PromptField = "";
            ExitState = "FN0000_INVALID_COLL_TYPE";

            return;
        }

        global.Command = "DISPLAY";

        var field = GetField(export.Input, "code");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;

        export.Input.Code = import.FromFlow.Code;
        export.OffsetType.PromptField = "";
      }
      else
      {
        export.OffsetType.PromptField = "";

        var field = GetField(export.Input, "code");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;
      }
    }

    // *** User would like when returning from CRAO that the screen display as 
    // was before flowing from CRAL.  Changed logic to populate "selected" views
    // instead of the views that are the search filter for the screen.  Sunya
    // Sharp 12/28/1998 ***
    // Move Repeating Group Import to Group Export.
    export.Advancements.Index = 0;
    export.Advancements.Clear();

    for(import.Advancements.Index = 0; import.Advancements.Index < import
      .Advancements.Count; ++import.Advancements.Index)
    {
      if (export.Advancements.IsFull)
      {
        break;
      }

      export.Advancements.Update.DetailCommon.SelectChar =
        import.Advancements.Item.DetailCommon.SelectChar;
      export.Advancements.Update.DetailCashReceiptSourceType.Code =
        import.Advancements.Item.DetailCashReceiptSourceType.Code;
      MoveCollectionType(import.Advancements.Item.DetailCollectionType,
        export.Advancements.Update.DetailCollectionType);
      export.Advancements.Update.DetailReceiptRefund.Assign(
        import.Advancements.Item.DetailReceiptRefund);
      export.Advancements.Update.DetailPaymentRequest.Assign(
        import.Advancements.Item.DetailPaymentRequest);
      export.Advancements.Update.DetailCashReceiptDetail.CollectionAmount =
        import.Advancements.Item.DetailCashReceiptDetail.CollectionAmount;
      export.Advancements.Update.DetailCsePerson.Number =
        import.Advancements.Item.DetailCsePerson.Number;
      export.Advancements.Update.DetailCsePersonsWorkSet.FormattedName =
        import.Advancements.Item.DetailCsePersonsWorkSet.FormattedName;
      export.Advancements.Update.DetailPaymentStatus.Code =
        import.Advancements.Item.DetailPaymentStatus.Code;
      export.Advancements.Update.DetailName.Text4 =
        import.Advancements.Item.DetailName.Text4;
      MoveTextWorkArea(import.Advancements.Item.DetailOffset,
        export.Advancements.Update.DetailOffset);

      // Determine whether a selection has been made.  Only one selection will 
      // be processed at a time.  An error will occur when more than one
      // selection has been made.
      if (!IsEmpty(export.Advancements.Item.DetailCommon.SelectChar))
      {
        if (AsChar(export.Advancements.Item.DetailCommon.SelectChar) == 'S')
        {
          ++local.SelectionCounter.Count;
          export.SelectedReceiptRefund.CreatedTimestamp =
            export.Advancements.Item.DetailReceiptRefund.CreatedTimestamp;
          export.SelectedCsePersonsWorkSet.FormattedName =
            export.Advancements.Item.DetailCsePersonsWorkSet.FormattedName;
          export.SelectedCsePerson.Number =
            export.Advancements.Item.DetailCsePerson.Number;
          export.SelectedCashReceiptDetail.CollectionAmount =
            export.Advancements.Item.DetailCashReceiptDetail.CollectionAmount;
          export.Advancements.Update.DetailCommon.SelectChar = "";
        }
        else
        {
          // Invalid Selection Made.  Must use "S" in selection field to pick an
          // advancement or receipt refund.
          var field =
            GetField(export.Advancements.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }
      }

      export.Advancements.Next();
    }

    if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
    {
      return;
    }

    if (local.SelectionCounter.Count > 1)
    {
      for(export.Advancements.Index = 0; export.Advancements.Index < export
        .Advancements.Count; ++export.Advancements.Index)
      {
        if (!IsEmpty(export.Advancements.Item.DetailCommon.SelectChar))
        {
          var field =
            GetField(export.Advancements.Item.DetailCommon, "selectChar");

          field.Error = true;
        }
      }

      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    // *** When returning from CLCT or NAME the screen loses the group view if 
    // nothing is selected.  Added logic to escape after to group view have been
    // moved to export.  Sunya Sharp 1/7/1999 ***
    if (Equal(global.Command, "RETCLCT") || Equal(global.Command, "RETNAME"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
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
      export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    if (Equal(global.Command, "DETAIL") || Equal
      (global.Command, "RTFRMLNK") || Equal(global.Command, "CRRC") || Equal
      (global.Command, "PAYR"))
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

    // *** Prompt is not valid with any function other then list.  Added logic 
    // to handle this scenerio.  Sunya Sharp 1/11/1999 ***
    if (!IsEmpty(export.OffsetType.PromptField) || !
      IsEmpty(export.SearchCsePersonNumber.SelectChar))
    {
      if (!Equal(global.Command, "LIST"))
      {
        if (!IsEmpty(export.OffsetType.PromptField))
        {
          var field = GetField(export.OffsetType, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.SearchCsePersonNumber.SelectChar))
        {
          var field = GetField(export.SearchCsePersonNumber, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

        return;
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "RTFRMLNK":
        break;
      case "LIST":
        // This case allows the searching for Offset Types from a list screen.
        switch(AsChar(export.OffsetType.PromptField))
        {
          case '+':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            // Invalid selection character.  Must use "S" in selection field to 
            // search CSE Person number.
            var field = GetField(export.OffsetType, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        // This case allows the searching of CSE Person numbers from listing.
        switch(AsChar(export.SearchCsePersonNumber.SelectChar))
        {
          case '+':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            // Invalid selection character.  Must use "S" in selection field to 
            // search CSE Person number.
            var field = GetField(export.SearchCsePersonNumber, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        switch(local.Common.Count)
        {
          case 0:
            var field1 = GetField(export.OffsetType, "promptField");

            field1.Error = true;

            var field2 = GetField(export.SearchCsePersonNumber, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            // Continue
            break;
          default:
            var field3 = GetField(export.OffsetType, "promptField");

            field3.Error = true;

            var field4 = GetField(export.SearchCsePersonNumber, "selectChar");

            field4.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "DETAIL":
        // *** User would like to be able to return from CRAO even when no 
        // selection was made from the list.  Sunya Sharp 12/28/1998 ***
        // Do Transfer Flow to Offset Advancement Screen.
        ExitState = "ECO_XFR_TO_OFFSET_ADVANCEMENT";

        break;
      case "CRRC":
        // *** There is no way to know the exact cash receipt detail that has 
        // been applied to the refund.  The user has request that the
        // information be accessable.  A flow to CRRC has been added for the
        // purpose.  Sunya Sharp 1/6/1999 ***
        switch(local.SelectionCounter.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            if (export.SelectedCashReceiptDetail.CollectionAmount > 0)
            {
              if (ReadCashReceiptDetailCashReceiptCashReceiptEvent())
              {
                export.SelectedCashReceipt.SequentialNumber =
                  entities.CashReceipt.SequentialNumber;
                MoveCashReceiptDetail(entities.CashReceiptDetail,
                  export.SelectedCashReceiptDetail);
                export.SelectedCashReceiptEvent.SystemGeneratedIdentifier =
                  entities.CashReceiptEvent.SystemGeneratedIdentifier;
                export.SelectedCashReceiptSourceType.SystemGeneratedIdentifier =
                  entities.Detail.SystemGeneratedIdentifier;
                export.SelectedCashReceiptType.SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
              }

              ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";
            }
            else
            {
              ExitState = "FN0000_NO_FLOW_NO_DETAIL_FOR_REF";
            }

            break;
          default:
            // *** Multiple selection is handled above.  Sunya Sharp 1/6/1999 **
            // *
            break;
        }

        break;
      case "PAYR":
        // *** User requested the ability to flow to PAYR to be able to view if 
        // the account for a person.  Sunya Sharp 1/6/1999 ***
        switch(local.SelectionCounter.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_LST_COLL_BY_AP_PYR";

            break;
          default:
            // *** Multiple selection is handled above.  Sunya Sharp 1/6/1999 **
            // *
            break;
        }

        break;
      case "DISPLAY":
        // Check for advancement selection with display command.  Selection is 
        // not allowed with display command.
        if (local.SelectionCounter.Count > 0)
        {
          var field =
            GetField(export.Advancements.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "NO_SELECT_WITH_DISPLAY_OPTION";

          return;
        }

        // ***************************************************************
        // If request date is blank, set to default. List from date
        // defaults to the first date of last month. List to date
        // defaults to the last day of current month. - Syed Hasan, MTW
        // ***************************************************************
        // *** User would like the from date to be one month previous to current
        // date and the to date to be current date.  Also if the CSE person
        // number is entered do not default the date.  If it is there use it.
        // Sunya Sharp 12/29/1998 ***
        if (IsEmpty(export.CsePerson.Number))
        {
          if (Equal(export.InputFrom.Date, local.Null1.Date) || Equal
            (export.InputTo.Date, local.Null1.Date))
          {
            export.InputTo.Date = Now().Date;
            export.InputFrom.Date = Now().Date.AddMonths(-1);
          }
        }

        if (Equal(export.InputFrom.Date, local.Null1.Date) && Lt
          (local.Null1.Date, export.InputTo.Date) || Lt
          (local.Null1.Date, export.InputFrom.Date) && Equal
          (export.InputTo.Date, local.Null1.Date))
        {
          var field1 = GetField(export.InputTo, "date");

          field1.Error = true;

          var field2 = GetField(export.InputFrom, "date");

          field2.Error = true;

          ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

          return;
        }

        if (Lt(export.InputTo.Date, export.InputFrom.Date))
        {
          var field1 = GetField(export.InputTo, "date");

          field1.Error = true;

          var field2 = GetField(export.InputFrom, "date");

          field2.Error = true;

          ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

          return;
        }

        // *** User would like the group view sorted in descending request date 
        // instead of ascending.  Sunya Sharp 12/28/1998 ***
        // *** Add logic for the new relationship between receipt refund and 
        // collection type.  The user would now like for this value to be
        // displayed on the screen and remove the cash receipt source type.
        // Sunya Sharp 1/4/1999 ***
        // *** Changed the "Show Open Offsets" to "Show Closed Advancements".  
        // Reversed the logic to go with the new use of the field.  Sunya Sharp
        // 1/6/1998
        // Determine CSE Person # field entry for advancements display.
        if (IsEmpty(export.CsePerson.Number))
        {
          export.Advancements.Index = 0;
          export.Advancements.Clear();

          foreach(var item in ReadReceiptRefundCsePerson())
          {
            if (AsChar(export.InputClosed.Flag) == 'N' && AsChar
              (entities.ReceiptRefund.OffsetClosed) == 'Y')
            {
              export.Advancements.Next();

              continue;
            }

            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePerson1();

            if (IsExitState("CSE_PERSON_NF"))
            {
              var field = GetField(export.CsePerson, "number");

              field.Error = true;

              export.Advancements.Next();

              return;
            }

            if (ReadCashReceiptSourceType())
            {
              if (!Equal(entities.CashReceiptSourceType.Code, "FDSO") && !
                Equal(entities.CashReceiptSourceType.Code, "SDSO"))
              {
                export.Advancements.Next();

                continue;
              }
            }
            else
            {
              ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
              export.Advancements.Next();

              return;
            }

            if (ReadCollectionType())
            {
              if (!IsEmpty(export.Input.Code) && !
                Equal(entities.CollectionType.Code, export.Input.Code))
              {
                export.Advancements.Next();

                continue;
              }
            }
            else
            {
              ExitState = "FN0000_COLLECTION_TYPE_NF";
              export.Advancements.Next();

              return;
            }

            // *** Add changes for PR#120.  Sunya Sharp 08/23/1999 ***
            if (ReadPaymentRequest())
            {
              if (!Equal(entities.PaymentRequest.Classification, "ADV"))
              {
                export.Advancements.Next();

                continue;
              }
            }
            else
            {
              // OKAY, Did not want to find one.
            }

            ReadPaymentStatusHistoryPaymentStatus();

            if (ReadCashReceiptDetail())
            {
              export.Advancements.Update.DetailCashReceiptDetail.
                CollectionAmount = entities.CashReceiptDetail.CollectionAmount;
            }
            else
            {
              export.Advancements.Update.DetailCashReceiptDetail.
                CollectionAmount = 0;
            }

            export.Advancements.Update.DetailPaymentStatus.Code =
              entities.PaymentStatus.Code;
            export.Advancements.Update.DetailCashReceiptSourceType.Code =
              entities.CashReceiptSourceType.Code;
            MoveCollectionType(entities.CollectionType,
              export.Advancements.Update.DetailCollectionType);
            MoveReceiptRefund(entities.ReceiptRefund,
              export.Advancements.Update.DetailReceiptRefund);
            export.Advancements.Update.DetailPaymentRequest.Assign(
              entities.PaymentRequest);
            export.Advancements.Update.DetailCsePerson.Number =
              entities.CsePerson.Number;

            if (IsEmpty(entities.ReceiptRefund.OffsetClosed))
            {
              export.Advancements.Update.DetailReceiptRefund.OffsetClosed = "N";
            }

            if (!IsEmpty(export.Advancements.Item.DetailCsePersonsWorkSet.
              FormattedName))
            {
              export.Advancements.Update.DetailName.Text4 = "Name";
            }

            if (!IsEmpty(export.Advancements.Item.DetailReceiptRefund.Taxid))
            {
              export.Advancements.Update.DetailOffset.Text10 = "Offset Tax";
              export.Advancements.Update.DetailOffset.Text4 = "ID";
            }

            export.Advancements.Next();
          }
        }
        else
        {
          UseCabZeroFillNumber();

          if (!Equal(export.InputFrom.Date, local.Null1.Date) && !
            Equal(export.InputTo.Date, local.Null1.Date))
          {
            export.Advancements.Index = 0;
            export.Advancements.Clear();

            foreach(var item in ReadReceiptRefund1())
            {
              if (AsChar(export.InputClosed.Flag) == 'N' && AsChar
                (entities.ReceiptRefund.OffsetClosed) == 'Y')
              {
                export.Advancements.Next();

                continue;
              }

              if (ReadCashReceiptSourceType())
              {
                if (!Equal(entities.CashReceiptSourceType.Code, "FDSO") && !
                  Equal(entities.CashReceiptSourceType.Code, "SDSO"))
                {
                  export.Advancements.Next();

                  continue;
                }
              }
              else
              {
                ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
                export.Advancements.Next();

                return;
              }

              if (ReadCollectionType())
              {
                if (!IsEmpty(export.Input.Code) && !
                  Equal(entities.CollectionType.Code, export.Input.Code))
                {
                  export.Advancements.Next();

                  continue;
                }
              }
              else
              {
                ExitState = "FN0000_COLLECTION_TYPE_NF";
                export.Advancements.Next();

                return;
              }

              // *** Add changes for PR#120.  Sunya Sharp 08/23/1999 ***
              if (ReadPaymentRequest())
              {
                if (!Equal(entities.PaymentRequest.Classification, "ADV"))
                {
                  export.Advancements.Next();

                  continue;
                }
              }
              else
              {
                // OKAY, Did not want to find one.
              }

              ReadPaymentStatusHistoryPaymentStatus();

              if (ReadCashReceiptDetail())
              {
                export.Advancements.Update.DetailCashReceiptDetail.
                  CollectionAmount =
                    entities.CashReceiptDetail.CollectionAmount;
              }
              else
              {
                export.Advancements.Update.DetailCashReceiptDetail.
                  CollectionAmount = 0;
              }

              export.Advancements.Update.DetailPaymentStatus.Code =
                entities.PaymentStatus.Code;
              export.Advancements.Update.DetailCashReceiptSourceType.Code =
                entities.CashReceiptSourceType.Code;
              MoveCollectionType(entities.CollectionType,
                export.Advancements.Update.DetailCollectionType);
              MoveReceiptRefund(entities.ReceiptRefund,
                export.Advancements.Update.DetailReceiptRefund);

              if (IsEmpty(export.Advancements.Item.DetailReceiptRefund.
                OffsetClosed))
              {
                export.Advancements.Update.DetailReceiptRefund.OffsetClosed =
                  "N";
              }

              export.Advancements.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);
              export.Advancements.Update.DetailCsePerson.Number =
                export.CsePersonsWorkSet.Number;
              export.Advancements.Update.DetailCsePersonsWorkSet.FormattedName =
                export.CsePersonsWorkSet.FormattedName;

              if (!IsEmpty(export.Advancements.Item.DetailCsePersonsWorkSet.
                FormattedName))
              {
                export.Advancements.Update.DetailName.Text4 = "Name";
              }

              if (!IsEmpty(export.Advancements.Item.DetailReceiptRefund.Taxid))
              {
                export.Advancements.Update.DetailOffset.Text10 = "Offset Tax";
                export.Advancements.Update.DetailOffset.Text4 = "ID";
              }

              export.Advancements.Next();
            }
          }
          else
          {
            export.Advancements.Index = 0;
            export.Advancements.Clear();

            foreach(var item in ReadReceiptRefund2())
            {
              if (AsChar(export.InputClosed.Flag) == 'N' && AsChar
                (entities.ReceiptRefund.OffsetClosed) == 'Y')
              {
                export.Advancements.Next();

                continue;
              }

              if (ReadCashReceiptSourceType())
              {
                if (!Equal(entities.CashReceiptSourceType.Code, "FDSO") && !
                  Equal(entities.CashReceiptSourceType.Code, "SDSO"))
                {
                  export.Advancements.Next();

                  continue;
                }
              }
              else
              {
                ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
                export.Advancements.Next();

                return;
              }

              if (ReadCollectionType())
              {
                if (!IsEmpty(export.Input.Code) && !
                  Equal(entities.CollectionType.Code, export.Input.Code))
                {
                  export.Advancements.Next();

                  continue;
                }
              }
              else
              {
                ExitState = "FN0000_COLLECTION_TYPE_NF";
                export.Advancements.Next();

                return;
              }

              // *** Add changes for PR#120.  Sunya Sharp 08/23/1999 ***
              if (ReadPaymentRequest())
              {
                if (!Equal(entities.PaymentRequest.Classification, "ADV"))
                {
                  export.Advancements.Next();

                  continue;
                }
              }
              else
              {
                // OKAY, Did not want to find one.
              }

              ReadPaymentStatusHistoryPaymentStatus();

              if (ReadCashReceiptDetail())
              {
                export.Advancements.Update.DetailCashReceiptDetail.
                  CollectionAmount =
                    entities.CashReceiptDetail.CollectionAmount;
              }
              else
              {
                export.Advancements.Update.DetailCashReceiptDetail.
                  CollectionAmount = 0;
              }

              export.Advancements.Update.DetailPaymentStatus.Code =
                entities.PaymentStatus.Code;
              export.Advancements.Update.DetailCashReceiptSourceType.Code =
                entities.CashReceiptSourceType.Code;
              MoveCollectionType(entities.CollectionType,
                export.Advancements.Update.DetailCollectionType);
              MoveReceiptRefund(entities.ReceiptRefund,
                export.Advancements.Update.DetailReceiptRefund);

              if (IsEmpty(export.Advancements.Item.DetailReceiptRefund.
                OffsetClosed))
              {
                export.Advancements.Update.DetailReceiptRefund.OffsetClosed =
                  "N";
              }

              export.Advancements.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);
              export.Advancements.Update.DetailCsePerson.Number =
                export.CsePersonsWorkSet.Number;
              export.Advancements.Update.DetailCsePersonsWorkSet.FormattedName =
                export.CsePersonsWorkSet.FormattedName;

              if (!IsEmpty(export.Advancements.Item.DetailCsePersonsWorkSet.
                FormattedName))
              {
                export.Advancements.Update.DetailName.Text4 = "Name";
              }

              if (!IsEmpty(export.Advancements.Item.DetailReceiptRefund.Taxid))
              {
                export.Advancements.Update.DetailOffset.Text10 = "Offset Tax";
                export.Advancements.Update.DetailOffset.Text4 = "ID";
              }

              export.Advancements.Next();
            }
          }
        }

        if (export.Advancements.IsEmpty)
        {
          // This for each clears the screen when a receipt refund is not found 
          // for a valid CSE Person.  Message is displayed.
          export.Advancements.Index = 0;
          export.Advancements.Clear();

          for(import.Advancements.Index = 0; import.Advancements.Index < import
            .Advancements.Count; ++import.Advancements.Index)
          {
            if (export.Advancements.IsFull)
            {
              break;
            }

            export.Advancements.Next();
          }

          ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

          return;
        }

        // Check whether advancement listing exceed maximum group length.
        if (export.Advancements.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        break;
      case "RETURN":
        // Execute link flow back to Offset Advancement Screen.
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        // Scroll to next page.  Rollover to first page if at end.
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        // Scroll to previous page.
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
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
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
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

  private static void MoveReceiptRefund(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.Taxid = source.Taxid;
    target.Amount = source.Amount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.RequestDate = source.RequestDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.OffsetClosed = source.OffsetClosed;
  }

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text4 = source.Text4;
    target.Text10 = source.Text10;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.CsePerson.Number = useImport.CsePerson.Number;
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

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Advancements.Update.DetailCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
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

  private bool ReadCashReceiptDetailCashReceiptCashReceiptEvent()
  {
    entities.CashReceiptType.Populated = false;
    entities.Detail.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptCashReceiptEvent",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.SelectedReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.Detail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Detail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Detail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 5);
        entities.CashReceiptType.Populated = true;
        entities.Detail.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
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
        entities.PaymentRequest.Classification = db.GetString(reader, 1);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 2);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 3);
        entities.PaymentRequest.Type1 = db.GetString(reader, 4);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 7);
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
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaymentStatus.Code = db.GetString(reader, 4);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund1()
  {
    return ReadEach("ReadReceiptRefund1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.CsePerson.Number);
        db.SetDate(command, "date1", export.InputFrom.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.InputTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Advancements.IsFull)
        {
          return false;
        }

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
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 12);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 13);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 14);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 16);
        entities.ReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund2()
  {
    return ReadEach("ReadReceiptRefund2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Advancements.IsFull)
        {
          return false;
        }

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
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 12);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 13);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 14);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 16);
        entities.ReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefundCsePerson()
  {
    return ReadEach("ReadReceiptRefundCsePerson",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.InputFrom.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.InputTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Advancements.IsFull)
        {
          return false;
        }

        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 12);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 13);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 14);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 16);
        entities.CsePerson.Populated = true;
        entities.ReceiptRefund.Populated = true;
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
    /// <summary>A AdvancementsGroup group.</summary>
    [Serializable]
    public class AdvancementsGroup
    {
      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetail")]
      public CashReceiptDetail DetailCashReceiptDetail
      {
        get => detailCashReceiptDetail ??= new();
        set => detailCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of DetailName.
      /// </summary>
      [JsonPropertyName("detailName")]
      public TextWorkArea DetailName
      {
        get => detailName ??= new();
        set => detailName = value;
      }

      /// <summary>
      /// A value of DetailOffset.
      /// </summary>
      [JsonPropertyName("detailOffset")]
      public TextWorkArea DetailOffset
      {
        get => detailOffset ??= new();
        set => detailOffset = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DetailReceiptRefund.
      /// </summary>
      [JsonPropertyName("detailReceiptRefund")]
      public ReceiptRefund DetailReceiptRefund
      {
        get => detailReceiptRefund ??= new();
        set => detailReceiptRefund = value;
      }

      /// <summary>
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private CollectionType detailCollectionType;
      private CashReceiptDetail detailCashReceiptDetail;
      private TextWorkArea detailName;
      private TextWorkArea detailOffset;
      private PaymentStatus detailPaymentStatus;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CsePerson detailCsePerson;
      private Common detailCommon;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private ReceiptRefund detailReceiptRefund;
      private PaymentRequest detailPaymentRequest;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Common Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public ReceiptRefund Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of SearchCsePersonNumber.
    /// </summary>
    [JsonPropertyName("searchCsePersonNumber")]
    public Common SearchCsePersonNumber
    {
      get => searchCsePersonNumber ??= new();
      set => searchCsePersonNumber = value;
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
    /// Gets a value of Advancements.
    /// </summary>
    [JsonIgnore]
    public Array<AdvancementsGroup> Advancements => advancements ??= new(
      AdvancementsGroup.Capacity);

    /// <summary>
    /// Gets a value of Advancements for json serialization.
    /// </summary>
    [JsonPropertyName("advancements")]
    [Computed]
    public IList<AdvancementsGroup> Advancements_Json
    {
      get => advancements;
      set => Advancements.Assign(value);
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
    /// A value of OffsetType.
    /// </summary>
    [JsonPropertyName("offsetType")]
    public Standard OffsetType
    {
      get => offsetType ??= new();
      set => offsetType = value;
    }

    private CollectionType fromFlow;
    private CollectionType collectionType;
    private DateWorkArea to;
    private DateWorkArea from;
    private Common closed;
    private ReceiptRefund selected;
    private CsePerson csePerson;
    private Common searchCsePersonNumber;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<AdvancementsGroup> advancements;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard offsetType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AdvancementsGroup group.</summary>
    [Serializable]
    public class AdvancementsGroup
    {
      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetail")]
      public CashReceiptDetail DetailCashReceiptDetail
      {
        get => detailCashReceiptDetail ??= new();
        set => detailCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of DetailName.
      /// </summary>
      [JsonPropertyName("detailName")]
      public TextWorkArea DetailName
      {
        get => detailName ??= new();
        set => detailName = value;
      }

      /// <summary>
      /// A value of DetailOffset.
      /// </summary>
      [JsonPropertyName("detailOffset")]
      public TextWorkArea DetailOffset
      {
        get => detailOffset ??= new();
        set => detailOffset = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DetailReceiptRefund.
      /// </summary>
      [JsonPropertyName("detailReceiptRefund")]
      public ReceiptRefund DetailReceiptRefund
      {
        get => detailReceiptRefund ??= new();
        set => detailReceiptRefund = value;
      }

      /// <summary>
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private CollectionType detailCollectionType;
      private CashReceiptDetail detailCashReceiptDetail;
      private TextWorkArea detailName;
      private TextWorkArea detailOffset;
      private PaymentStatus detailPaymentStatus;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CsePerson detailCsePerson;
      private Common detailCommon;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private ReceiptRefund detailReceiptRefund;
      private PaymentRequest detailPaymentRequest;
    }

    /// <summary>
    /// A value of SelectedCashReceiptType.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptType")]
    public CashReceiptType SelectedCashReceiptType
    {
      get => selectedCashReceiptType ??= new();
      set => selectedCashReceiptType = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptSourceType")]
    public CashReceiptSourceType SelectedCashReceiptSourceType
    {
      get => selectedCashReceiptSourceType ??= new();
      set => selectedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptEvent")]
    public CashReceiptEvent SelectedCashReceiptEvent
    {
      get => selectedCashReceiptEvent ??= new();
      set => selectedCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of SelectedCashReceipt.
    /// </summary>
    [JsonPropertyName("selectedCashReceipt")]
    public CashReceipt SelectedCashReceipt
    {
      get => selectedCashReceipt ??= new();
      set => selectedCashReceipt = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptDetail")]
    public CashReceiptDetail SelectedCashReceiptDetail
    {
      get => selectedCashReceiptDetail ??= new();
      set => selectedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Input.
    /// </summary>
    [JsonPropertyName("input")]
    public CollectionType Input
    {
      get => input ??= new();
      set => input = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InputTo.
    /// </summary>
    [JsonPropertyName("inputTo")]
    public DateWorkArea InputTo
    {
      get => inputTo ??= new();
      set => inputTo = value;
    }

    /// <summary>
    /// A value of InputFrom.
    /// </summary>
    [JsonPropertyName("inputFrom")]
    public DateWorkArea InputFrom
    {
      get => inputFrom ??= new();
      set => inputFrom = value;
    }

    /// <summary>
    /// A value of InputClosed.
    /// </summary>
    [JsonPropertyName("inputClosed")]
    public Common InputClosed
    {
      get => inputClosed ??= new();
      set => inputClosed = value;
    }

    /// <summary>
    /// A value of SelectedReceiptRefund.
    /// </summary>
    [JsonPropertyName("selectedReceiptRefund")]
    public ReceiptRefund SelectedReceiptRefund
    {
      get => selectedReceiptRefund ??= new();
      set => selectedReceiptRefund = value;
    }

    /// <summary>
    /// A value of SearchCsePersonNumber.
    /// </summary>
    [JsonPropertyName("searchCsePersonNumber")]
    public Common SearchCsePersonNumber
    {
      get => searchCsePersonNumber ??= new();
      set => searchCsePersonNumber = value;
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
    /// Gets a value of Advancements.
    /// </summary>
    [JsonIgnore]
    public Array<AdvancementsGroup> Advancements => advancements ??= new(
      AdvancementsGroup.Capacity);

    /// <summary>
    /// Gets a value of Advancements for json serialization.
    /// </summary>
    [JsonPropertyName("advancements")]
    [Computed]
    public IList<AdvancementsGroup> Advancements_Json
    {
      get => advancements;
      set => Advancements.Assign(value);
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
    /// A value of OffsetType.
    /// </summary>
    [JsonPropertyName("offsetType")]
    public Standard OffsetType
    {
      get => offsetType ??= new();
      set => offsetType = value;
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

    private CashReceiptType selectedCashReceiptType;
    private CashReceiptSourceType selectedCashReceiptSourceType;
    private CashReceiptEvent selectedCashReceiptEvent;
    private CashReceipt selectedCashReceipt;
    private CashReceiptDetail selectedCashReceiptDetail;
    private CollectionType input;
    private CsePerson selectedCsePerson;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private DateWorkArea inputTo;
    private DateWorkArea inputFrom;
    private Common inputClosed;
    private ReceiptRefund selectedReceiptRefund;
    private Common searchCsePersonNumber;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<AdvancementsGroup> advancements;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard offsetType;
    private Code code;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NoOfMonthsAfter.
    /// </summary>
    [JsonPropertyName("noOfMonthsAfter")]
    public Common NoOfMonthsAfter
    {
      get => noOfMonthsAfter ??= new();
      set => noOfMonthsAfter = value;
    }

    /// <summary>
    /// A value of NoOfMonthsPrior.
    /// </summary>
    [JsonPropertyName("noOfMonthsPrior")]
    public Common NoOfMonthsPrior
    {
      get => noOfMonthsPrior ??= new();
      set => noOfMonthsPrior = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of SelectionCounter.
    /// </summary>
    [JsonPropertyName("selectionCounter")]
    public Common SelectionCounter
    {
      get => selectionCounter ??= new();
      set => selectionCounter = value;
    }

    private Common noOfMonthsAfter;
    private Common noOfMonthsPrior;
    private Common common;
    private DateWorkArea null1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common selectionCounter;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Detail.
    /// </summary>
    [JsonPropertyName("detail")]
    public CashReceiptSourceType Detail
    {
      get => detail ??= new();
      set => detail = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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

    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType detail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CollectionType collectionType;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private ReceiptRefund receiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
    private PaymentRequest paymentRequest;
  }
#endregion
}

// Program: FN_PAYR_LST_COLLS_BY_AP_PYR, ID: 371742374, model: 746.
// Short name: SWEPAYRP
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
/// A program: FN_PAYR_LST_COLLS_BY_AP_PYR.
/// </para>
/// <para>
/// RESP: FINANCE
/// This is a list screen that shows collection information for an obligor.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPayrLstCollsByApPyr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PAYR_LST_COLLS_BY_AP_PYR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPayrLstCollsByApPyr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPayrLstCollsByApPyr.
  /// </summary>
  public FnPayrLstCollsByApPyr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------
    // CHANGE LOG
    // Date	  Programmer		#	Description
    // 06/13/96  Holly Kennedy-MTW		Change Prompt command to list -
    // 					was not hitting the code.
    // 					Added command and flow to List
    // 					Collection Activity by AP/Payor.
    // 					Brought screen up to standard
    // 					Add Logic to utilize the screen
    // 					owed amounts calculation CAB
    // 					Added error message field
    // 					beneath the owed amounts for
    // 					Unavailable and 			Processed Transaction error message
    // 					Changed code to display the creation
    // 					date in the Reference if the
    // 					source type is FDSO, SDSO, UI, or
    // 					KPERS.  This is a change imposed at
    // 					the screen signoff meetings.
    // 11/05/96  Holly Kennedy-MTW		Changed the List command
    // 					to flow to the correct screen.
    // 11/27/96  Holly Kennedy-MTW		Added data level Security
    // 01/20/97  Holly Kennedy-MTW		Fix display problem.  User had to
    // 					Press PF2 twice to get a display.
    // 					Read was being performed on the import
    // 					CSE Person not the export CSE Person
    // 					The export value was the one that had
    // 					the zero fill process performed to
    // 					it
    // 03/26/98	Siraj Konkader		ZDEL cleanup
    // --------------------------------------------------------------------------
    // 03/05/97   A.Kinney
    // Added logic for return from MCOL, COLL and COLA.  Also added logic for 
    // return from LIST (original data was not being displayed when nothing was
    // selected from AP Name list).
    // 04/11/97   Sumanta - MTW
    // Made the following changes :-
    //   - deleted reference # column
    //   - added status column and added read logic to get the status
    //   - added PF key to CRRC
    //   - moved Undistributed Amount up one line below Total Owed
    //   - ADDED a look up logic to CRUC ...
    // 9/26/97	A Samuels	IDCR 362
    // 10/1/98    E. Parker		Added logic to check for selection on CASE COLA & 
    // CRRC, set local date to CURRENT DATE and changed logic to use local,
    // restructured Read Each on Cash Receipt Detail for performance, changed
    // Receipt Date field on screen from Received Date to Receipt Date, remove
    // logic to calculate undistributed amount and added use stmt for
    // fn_compute_undistributed_amount, added logic to allow + in the list
    // prompts, changed owed amounts to show zero when zero, change return from
    // CRUC to execute with command retcruc.
    // 2/4/1999   E. Parker 		Added Collection Type to screen.
    // --------------------------------------------------------------------------
    // : 03/30/1999  A Doty    Modified the logic to support the calculation of 
    // summary totals real-time.  Removed the use of the Summary Entities.
    // ---------------------------------------------------------------------------------------
    // 6/16/99 - bud adams  -  reviewed Read actions: set properties, combined 
    // Reads, and combined Read Each actions.
    // 11/5/99 - E. Parker - PR #76060 Changed Receipt Date to Received Date.
    // 12/2/99 - E. Parker - PR #80974 Change Screen to display Cash Receipt 
    // Event Received Date instead of Cash Receipt Received Date.  Cleaned up
    // commented code.
    // 4/3/2000 - K. Doshi - PR# 92247 Set From/To Dates correctly so all 
    // payments in entered months are displayed.
    // ---------------------------------------------------------------------------------------
    // 8/3/2000 - E. Lyman - PR# 92247 Fix Next Tran
    // ---------------------------------------------------------------------------------------
    // 1/16/2001 - M. Brown - PR# 108289 AP number not being populated in some 
    // cases on a next tran in.
    // ---------------------------------------------------------------------------------------
    // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's deleted by 
    // REIP.
    // *****************************************************************
    // 07/02/02    M. Brown     PR0140491  Populate court order number on a next
    // tran out of this screen.
    // *****************************************************************
    // 09/13/12    GVandy       CQ35548  Implement FTIE and FTIR security 
    // profile restrictions.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Move all IMPORTs to EXPORTs.
    export.PromptAmt.Text1 = import.PromptAmt.Text1;
    export.NextTransaction.Command = import.NextTransaction.Command;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.HiddenCsePersonsWorkSet.FormattedName =
      import.HiddenCsePersonsWorkSet.FormattedName;
    export.CsePerson.Number = import.CsePerson.Number;
    export.Prompt.Text1 = import.Prompt.Text1;
    export.SearchFrom.Date = import.SearchFrom.Date;
    export.SearchTo.Date = import.SearchTo.Date;
    export.CurrentOwed.TotalCurrency = import.CurrentOwed.TotalCurrency;
    export.ArrearsOwed.TotalCurrency = import.ArrearsOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = import.InterestOwed.TotalCurrency;
    export.TotalOwed.TotalCurrency = import.TotalOwed.TotalCurrency;
    export.TotalUndistAmt.TotalCurrency = import.TotalUndistAmt.TotalCurrency;
    export.SelCourtOrderNo.CourtOrderNumber =
      import.SelCourtOrderNo.CourtOrderNumber;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // *** Return from LIST, if key is not populated (i.e. Spaces) refresh payor
    // name and escape after populating export views, else move new key value
    // to export view for read and reset Command to DISPLAY. ***
    if (Equal(global.Command, "PRMPTRET"))
    {
      if (IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.FormattedName =
          import.HiddenCsePersonsWorkSet.FormattedName;
      }
      else
      {
        export.CsePerson.Number = import.CsePersonsWorkSet.Number;
        global.Command = "DISPLAY";
      }
    }

    // ***************************************************************
    // Set local_date to minimize usage of CURRENT_DATE -- E. Parker  9/23/98
    // ***************************************************************
    local.CurrentDate.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();

    // *** If returning from COLL, if key is not populated (i.e. Spaces) escape 
    // after populating export views, else move new key value to export view for
    // read and reset Command to DISPLAY. ***
    if (Equal(global.Command, "RETCOLL"))
    {
      if (IsEmpty(import.FlowSelected.Number))
      {
      }
      else
      {
        export.CsePerson.Number = import.FlowSelected.Number;
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "RETCAJR"))
    {
      if (IsEmpty(import.FlowSelected.Number))
      {
      }
      else
      {
        export.CsePerson.Number = import.FlowSelected.Number;
        global.Command = "DISPLAY";
      }
    }

    // *** check for, and validate, selection
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      local.ItemSelected.Flag = "N";

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        if (Equal(global.Command, "PRMPTRET") || Equal
          (global.Command, "RETCOLL") || Equal(global.Command, "RETCAJR"))
        {
          export.Export1.Update.Common.SelectChar = "";
        }
        else
        {
          export.Export1.Update.Common.SelectChar =
            import.Import1.Item.Common.SelectChar;
        }

        MoveCollectionType(import.Import1.Item.CollectionType,
          export.Export1.Update.CollectionType);
        MoveCashReceiptSourceType(import.Import1.Item.CashReceiptSourceType,
          export.Export1.Update.CashReceiptSourceType);
        export.Export1.Update.CashReceipt.
          Assign(import.Import1.Item.CashReceipt);
        export.Export1.Update.CashReceiptDetail.Assign(
          import.Import1.Item.CashReceiptDetail);
        export.Export1.Update.FnReferenceNumber.ReferenceNumber =
          import.Import1.Item.FnReferenceNumber.ReferenceNumber;
        MoveCashReceiptEvent(import.Import1.Item.DetailCashReceiptEvent,
          export.Export1.Update.DetailCashReceiptEvent);
        MoveCashReceiptType(import.Import1.Item.DetailCashReceiptType,
          export.Export1.Update.DetailCashReceiptType);
        export.Export1.Update.Status.Code = import.Import1.Item.Status.Code;

        if (IsEmpty(export.Export1.Item.Common.SelectChar))
        {
        }
        else if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          // Check for multiple select
          if (AsChar(local.ItemSelected.Flag) == 'Y')
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            export.Export1.Next();

            continue;
          }

          local.ItemSelected.Flag = "Y";

          // Set export to selected item
          export.Selected.Assign(export.Export1.Item.CashReceiptDetail);
          MoveCashReceiptSourceType(export.Export1.Item.CashReceiptSourceType,
            export.CashReceiptSourceType);
          export.CashReceiptEvent.SystemGeneratedIdentifier =
            export.Export1.Item.DetailCashReceiptEvent.
              SystemGeneratedIdentifier;
          MoveCashReceiptType(export.Export1.Item.DetailCashReceiptType,
            export.CashReceiptType);
          export.CashReceipt.SequentialNumber =
            export.Export1.Item.CashReceipt.SequentialNumber;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "PRMPTRET") || Equal
      (global.Command, "RETCOLL") || Equal(global.Command, "RETCAJR") || Equal
      (global.Command, "RETCRUC") || Equal(global.Command, "RETLCDA"))
    {
      // ***  Nothing was selected from LIST or COLL and we have just populated 
      // the export views, so Escape to show original data that was displayed on
      // PAYR screen before link. ***
      return;
    }

    // *** if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.HiddenNextTranInfo.CsePersonNumberAp = import.CsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumberObligor =
        import.CsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumber = import.CsePerson.Number;

      // 07/02/02    M. Brown     PR0140491  Populate court order number on a 
      // next tran out of this screen.
      export.HiddenNextTranInfo.StandardCrtOrdNumber =
        export.SelCourtOrderNo.CourtOrderNumber ?? "";
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

    // the attribute will only be set here if the user was linking to another 
    // procudure an was not authorized for that transaction.  the called
    // procedure will return to here with the command set to notauth.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      // ---------------------------------
      // 1/16/2001 - M. Brown - PR# 108289: changed order of fields being 
      // checked
      //  in following stmt.  Before, ap number was checked first.
      // ---------------------------------
      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberAp))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligor))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberObligor ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // to validate action level security
    // Added data level security
    if (!Equal(global.Command, "COLA") && !Equal(global.Command, "COLL") && !
      Equal(global.Command, "CRRC") && !Equal(global.Command, "LIST") && !
      Equal(global.Command, "RETCAJR") && !Equal(global.Command, "LCDA"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // 09/13/12  GVandy  CQ35548  Check for FTIE and FTIR security profile 
      // restrictions.
      if (Equal(global.Command, "DISPLAY"))
      {
        UseScCheckProfileRestrictions();

        if (Equal(local.Profile.RestrictionCode1, "FTIR") || Equal
          (local.Profile.RestrictionCode2, "FTIR") || Equal
          (local.Profile.RestrictionCode3, "FTIR"))
        {
          local.FtirRestriction.Flag = "Y";
        }

        if (Equal(local.Profile.RestrictionCode1, "FTIE") || Equal
          (local.Profile.RestrictionCode2, "FTIE") || Equal
          (local.Profile.RestrictionCode3, "FTIE"))
        {
          local.FtieRestriction.Flag = "Y";
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }

    // Validation CASE structure
    switch(TrimEnd(global.Command))
    {
      case "MCOL":
        if (export.Selected.SequentialIdentifier == 0 || export
          .CashReceipt.SequentialNumber == 0 || export
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        ExitState = "ECO_LNK_TO_MCOL";

        break;
      case "LIST":
        switch(AsChar(import.Prompt.Text1))
        {
          case 'S':
            export.HiddenCsePersonsWorkSet.FormattedName =
              import.CsePersonsWorkSet.FormattedName;
            export.Prompt.Text1 = "+";
            local.PromptCount.Count = 1;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.Prompt, "text1");

            field.Error = true;

            return;
        }

        switch(AsChar(import.PromptAmt.Text1))
        {
          case 'S':
            export.PromptAmt.Text1 = "+";
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.PromptAmt, "text1");

            field.Error = true;

            return;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        if (AsChar(import.Prompt.Text1) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }
        else
        {
          ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";
        }

        break;
      case "CRRC":
        // ****************************************************************
        // Added logic to check for selection -- E. Parker  9/23/98
        // ****************************************************************
        if (export.Selected.SequentialIdentifier == 0 || export
          .CashReceipt.SequentialNumber == 0 || export
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else
        {
          ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";
        }

        break;
      case "COLA":
        // ****************************************************************
        // Added logic to check for selection -- E. Parker  9/23/98
        // ****************************************************************
        if (export.Selected.SequentialIdentifier == 0 || export
          .CashReceipt.SequentialNumber == 0 || export
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else
        {
          ExitState = "ECO_LNK_TO_REC_COLL_ADJMNT";
        }

        break;
      case "COLL":
        if (export.Selected.SequentialIdentifier == 0 || export
          .CashReceipt.SequentialNumber == 0 || export
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else
        {
          ExitState = "ECO_LNK_TO_LST_COL_ACT_BY_AP_PYR";
        }

        break;
      case "LCDA":
        if (export.Selected.SequentialIdentifier == 0 || export
          .CashReceipt.SequentialNumber == 0 || export
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          export.LcdaPassFrom.Date = export.SearchFrom.Date;
          export.LcdaPassTo.Date = export.SearchTo.Date;
        }
        else
        {
          export.LcdaPassFrom.Date = export.Selected.CollectionDate;
          export.LcdaPassTo.Date = export.Selected.CollectionDate;
        }

        ExitState = "ECO_LNK_TO_LCDA";

        break;
      case "PAYR":
        // *** If command is "PAYR", this is firstime thru.
        // Set command to DISPLAY and default dates ***
        global.Command = "DISPLAY";

        // --------------------------------------------------------
        // 4/3/2000 - K. Doshi - PR# 92247 Set From/To Dates correctly so all 
        // payments in entered months are displayed.
        // --------------------------------------------------------
        UseCabFirstAndLastDateOfMonth4();
        export.SearchFrom.Date = AddMonths(export.SearchFrom.Date, -1);

        break;
      case "DISPLAY":
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          ExitState = "NO_SELECT_WITH_DISPLAY_OPTION";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          break;
        }

        // ----------------------------------------------------------
        // *** From and To Dates ***
        // When blank, default From Date to first day of prev month
        // and To Date to last day of current month.
        // When From Date is supplied, it will be reset to first day of entered 
        // month.
        // When To Date is supplied, it will be reset to last day of
        // entered month.
        // -----------------------------------------------------------
        if (Equal(export.SearchFrom.Date, local.BlankDate.Date))
        {
          UseCabFirstAndLastDateOfMonth3();
          export.SearchFrom.Date = AddMonths(export.SearchFrom.Date, -1);
        }
        else
        {
          UseCabFirstAndLastDateOfMonth1();
        }

        if (Equal(export.SearchTo.Date, local.BlankDate.Date))
        {
          UseCabFirstAndLastDateOfMonth5();
        }
        else
        {
          UseCabFirstAndLastDateOfMonth2();
        }

        // *** Validate Dates ***
        if (Lt(export.SearchTo.Date, export.SearchFrom.Date))
        {
          ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

          var field1 = GetField(export.SearchFrom, "date");

          field1.Error = true;

          var field2 = GetField(export.SearchTo, "date");

          field2.Error = true;

          break;
        }

        // *** CSE person number is mandatory ***
        if (IsEmpty(export.CsePerson.Number))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        break;
      case "CLEAR":
        break;
      case "RETURN":
        break;
      case "BYPASS":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
        {
          if (AsChar(local.ItemSelected.Flag) == 'Y')
          {
            ExitState = "DO_NOT_MAKE_SELECTION_WITH_OPT";

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
              {
                var field = GetField(export.Export1.Item.Common, "selectChar");

                field.Error = true;
              }
            }
          }
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }

        break;
    }

    // Set command to BYPASS if edit errors detected
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "BYPASS":
        break;
      case "DISPLAY":
        // *** call CAB to get CSE person name and SSN ***
        local.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Continue Processing
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          return;
        }
        else
        {
          // : ADABAS is not available or has an error - Continue Processing.
        }

        if (!ReadCsePerson())
        {
          ExitState = "CSE_PERSON_NF";

          return;
        }

        UseFnHardcodedDebtDistribution();
        UseFnComputeSummaryTotals();
        export.CurrentOwed.TotalCurrency =
          export.ScreenOwedAmounts.CurrentAmountOwed;
        export.ArrearsOwed.TotalCurrency =
          export.ScreenOwedAmounts.ArrearsAmountOwed;
        export.InterestOwed.TotalCurrency =
          export.ScreenOwedAmounts.InterestAmountOwed;
        export.TotalOwed.TotalCurrency =
          export.ScreenOwedAmounts.TotalAmountOwed;

        if (IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Intensity = Intensity.Dark;
          field.Protected = true;
        }
        else
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        // READ EACH for selection list.
        // ****************************************************************
        // Restructured Read Each to be better qualified.  It was reading a 
        // large number of records and causing a performance issue for this
        // screen --
        // E. Parker  9/24/98
        // ****************************************************************
        if (!IsEmpty(export.SelCourtOrderNo.CourtOrderNumber))
        {
          // ***--- Sumanta Mahapatra - MTW
          //        Added read to find the status ..
          // ***--- Bud Adams - not mtw - combined that Read with the Read Each 
          // of History,
          // ***--- and then combined that with this R/E - 6/16/99
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailCashReceiptEventCashReceiptType1())
            
          {
            // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
            // deleted by REIP.
            // *****************************************************************
            if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
              "REIPDELETE"))
            {
              export.Export1.Next();

              continue;
            }

            // *****************************************************************
            if (ReadCollectionType())
            {
              // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if 
              // security profile restriction is FTIE.
              if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
                (entities.CollectionType.Code, "F"))
              {
                export.Export1.Next();

                continue;
              }

              MoveCollectionType(entities.CollectionType,
                export.Export1.Update.CollectionType);

              // 09/13/12 GVandy  CQ35548  Do not display Collection Type if 
              // security profile restriction is FTIR.
              if (AsChar(local.FtirRestriction.Flag) == 'Y')
              {
                export.Export1.Update.CollectionType.Code = "";
              }
            }
            else
            {
              // *** Collection Type is not mandatory
            }

            // ***---  combined these two into 1 Read - b adams  -  6/10/99
            if (ReadCashReceiptCashReceiptSourceType1())
            {
              // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if 
              // security profile restriction is FTIE.
              if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
                (entities.CashReceiptSourceType.Code, "FDSO"))
              {
                export.Export1.Next();

                continue;
              }

              MoveCashReceiptDetail2(entities.CashReceiptDetail,
                export.Export1.Update.CashReceiptDetail);
              export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
              MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                export.Export1.Update.CashReceiptSourceType);

              // 09/13/12 GVandy  CQ35548  Do not display Cash Receipt Source 
              // Type if security profile restriction is FTIR.
              if (AsChar(local.FtirRestriction.Flag) == 'Y')
              {
                export.Export1.Update.CashReceiptSourceType.Code = "";
              }

              MoveCashReceiptEvent(entities.CashReceiptEvent,
                export.Export1.Update.DetailCashReceiptEvent);
              MoveCashReceiptType(entities.CashReceiptType,
                export.Export1.Update.DetailCashReceiptType);

              // *** Call AB to set reference number on screen ***
              UseFnSetReferenceNumber();

              // *** If the distributed amount is zero, add net
              // collected amount to undistributed amount. ***
              // ***Removed this logic.  fn_compute_undistributed_amount will 
              // now take its place -- E. Parker 10/1/98***
            }
            else
            {
              ExitState = "FN0084_CASH_RCPT_NF";
              export.Export1.Next();

              return;
            }

            export.Export1.Update.Status.Code =
              entities.CashReceiptDetailStatus.Code;
            export.Export1.Next();
          }
        }
        else
        {
          // ***--- Sumanta Mahapatra - MTW
          //        Added read to find the status ..
          // ***--- Bud Adams - not mtw - combined that Read with the Read Each 
          // of History,
          // ***--- and then combined that with this R/E  -  6/16/99
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailCashReceiptEventCashReceiptType3())
            
          {
            // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
            // deleted by REIP.
            // *****************************************************************
            if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
              "REIPDELETE"))
            {
              export.Export1.Next();

              continue;
            }

            // *****************************************************************
            if (ReadCollectionType())
            {
              // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if 
              // security profile restriction is FTIE.
              if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
                (entities.CollectionType.Code, "F"))
              {
                export.Export1.Next();

                continue;
              }

              MoveCollectionType(entities.CollectionType,
                export.Export1.Update.CollectionType);

              // 09/13/12 GVandy  CQ35548  Do not display Collection Type if 
              // security profile restriction is FTIR.
              if (AsChar(local.FtirRestriction.Flag) == 'Y')
              {
                export.Export1.Update.CollectionType.Code = "";
              }
            }
            else
            {
              // *** Collection Type is not mandatory
            }

            // ***---  included Read of Cash_Receipt_Source_Type here &
            // ***---  removed separate Read.
            if (ReadCashReceiptCashReceiptSourceType2())
            {
              // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if 
              // security profile restriction is FTIE.
              if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
                (entities.CashReceiptSourceType.Code, "FDSO"))
              {
                export.Export1.Next();

                continue;
              }

              MoveCashReceiptDetail2(entities.CashReceiptDetail,
                export.Export1.Update.CashReceiptDetail);
              export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
              MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                export.Export1.Update.CashReceiptSourceType);

              // 09/13/12 GVandy  CQ35548  Do not display Cash Receipt Source 
              // Type if security profile restriction is FTIR.
              if (AsChar(local.FtirRestriction.Flag) == 'Y')
              {
                export.Export1.Update.CashReceiptSourceType.Code = "";
              }

              MoveCashReceiptEvent(entities.CashReceiptEvent,
                export.Export1.Update.DetailCashReceiptEvent);
              MoveCashReceiptType(entities.CashReceiptType,
                export.Export1.Update.DetailCashReceiptType);

              // *** Call AB to set reference number on screen ***
              UseFnSetReferenceNumber();

              // *** If the distributed amount is zero, add net
              // collected amount to undistributed amount. ***
              // ***Removed this logic.  fn_compute_undistributed_amount will 
              // now take its place -- E. Parker 10/1/98***
            }
            else
            {
              ExitState = "FN0084_CASH_RCPT_NF";
              export.Export1.Next();

              return;
            }

            export.Export1.Update.Status.Code =
              entities.CashReceiptDetailStatus.Code;
            export.Export1.Next();
          }

          // ***--- Sumanta Mahapatra - MTW
          //        Added read to find the status ..
          // ***--- Bud Adams - not mtw - combined that Read with the Read Each 
          // of History,
          // ***--- and then combined that with this R/E  -  6/16/99
          // ***---  combined 2 Read Eaches into 1: Legal_Action with
          // ***---  C_R_Detail, C_R_Event, C_R_Type
          // ***---  Uncombined 2 Read Eaches.  Had to
          // ***---  read Legal_Action separate from
          // ***---  C_R_Detail, C_R_Event, C_R_Type
          // ***---  to avoid duplicates.  E. Parker 1/5/2000
          foreach(var item in ReadLegalAction())
          {
            if (Equal(local.Prev.StandardNumber,
              entities.LegalAction.StandardNumber))
            {
              continue;
            }

            local.Prev.StandardNumber = entities.LegalAction.StandardNumber;

            export.Export1.Index = export.Export1.Count;
            export.Export1.CheckIndex();

            foreach(var item1 in ReadCashReceiptDetailCashReceiptEventCashReceiptType2())
              
            {
              // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
              // deleted by REIP.
              // *****************************************************************
              if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
                "REIPDELETE"))
              {
                export.Export1.Next();

                continue;
              }

              // *****************************************************************
              if (ReadCollectionType())
              {
                // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if 
                // security profile restriction is FTIE.
                if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
                  (entities.CollectionType.Code, "F"))
                {
                  export.Export1.Next();

                  continue;
                }

                MoveCollectionType(entities.CollectionType,
                  export.Export1.Update.CollectionType);

                // 09/13/12 GVandy  CQ35548  Do not display Collection Type if 
                // security profile restriction is FTIR.
                if (AsChar(local.FtirRestriction.Flag) == 'Y')
                {
                  export.Export1.Update.CollectionType.Code = "";
                }
              }
              else
              {
                // *** Collection Type is not mandatory
              }

              // ***---  combined 2 Reads into 1
              if (ReadCashReceiptCashReceiptSourceType2())
              {
                // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if 
                // security profile restriction is FTIE.
                if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
                  (entities.CashReceiptSourceType.Code, "FDSO"))
                {
                  export.Export1.Next();

                  continue;
                }

                MoveCashReceiptDetail2(entities.CashReceiptDetail,
                  export.Export1.Update.CashReceiptDetail);
                export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
                MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                  export.Export1.Update.CashReceiptSourceType);

                // 09/13/12 GVandy  CQ35548  Do not display Cash Receipt Source 
                // Type if security profile restriction is FTIR.
                if (AsChar(local.FtirRestriction.Flag) == 'Y')
                {
                  export.Export1.Update.CashReceiptSourceType.Code = "";
                }

                MoveCashReceiptEvent(entities.CashReceiptEvent,
                  export.Export1.Update.DetailCashReceiptEvent);
                MoveCashReceiptType(entities.CashReceiptType,
                  export.Export1.Update.DetailCashReceiptType);

                // *** Call AB to set reference number on screen ***
                UseFnSetReferenceNumber();

                // *** If the distributed amount is zero, add net
                // collected amount to undistributed amount. ***
                // ***Removed this logic.  fn_compute_undistributed_amount will 
                // now take its place -- E. Parker 10/1/98***
              }
              else
              {
                ExitState = "FN0084_CASH_RCPT_NF";
                export.Export1.Next();

                return;
              }

              export.Export1.Update.Status.Code =
                entities.CashReceiptDetailStatus.Code;
              export.Export1.Next();
            }
          }
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        if (export.Export1.IsEmpty)
        {
          export.TotalUndistAmt.TotalCurrency = 0;
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "COLA":
        break;
      case "COLL":
        break;
      case "RETURN":
        if (IsEmpty(import.CsePerson.Number))
        {
        }
        else
        {
          export.FlowToKdmv.Number = import.CsePerson.Number;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
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

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
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

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = import.SearchFrom.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchFrom.Date = useExport.First.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = import.SearchTo.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchTo.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth3()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.CurrentDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchFrom.Date = useExport.First.Date;
  }

  private void UseCabFirstAndLastDateOfMonth4()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.CurrentDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchFrom.Date = useExport.First.Date;
    export.SearchTo.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth5()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.CurrentDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.SearchTo.Date = useExport.Last.Date;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = export.CsePerson.Number;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.TotalUndistAmt.TotalCurrency = useExport.UndistAmt.TotalCurrency;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void UseFnSetReferenceNumber()
  {
    var useImport = new FnSetReferenceNumber.Import();
    var useExport = new FnSetReferenceNumber.Export();

    useImport.CashReceiptSourceType.Code = entities.CashReceiptSourceType.Code;
    useImport.CashReceipt.CheckNumber = entities.CashReceipt.CheckNumber;
    MoveCashReceiptDetail1(entities.CashReceiptDetail,
      useImport.CashReceiptDetail);

    Call(FnSetReferenceNumber.Execute, useImport, useExport);

    export.Export1.Update.FnReferenceNumber.ReferenceNumber =
      useExport.FnReferenceNumber.ReferenceNumber;
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScCheckProfileRestrictions()
  {
    var useImport = new ScCheckProfileRestrictions.Import();
    var useExport = new ScCheckProfileRestrictions.Export();

    Call(ScCheckProfileRestrictions.Execute, useImport, useExport);

    local.Profile.Assign(useExport.Profile);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePerson.Number = useExport.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCashReceiptCashReceiptSourceType1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 5);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 6);
        entities.CashReceipt.ReferenceNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 9);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 10);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptCashReceiptSourceType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 5);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 6);
        entities.CashReceipt.ReferenceNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 9);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 10);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptEventCashReceiptType1()
  {
    return ReadEach("ReadCashReceiptDetailCashReceiptEventCashReceiptType1",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", export.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", export.SearchTo.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "courtOrderNumber",
          export.SelCourtOrderNo.CourtOrderNumber ?? "");
        db.SetDate(
          command, "collectionDate", local.BlankDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 15);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 17);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptType.Code = db.GetString(reader, 19);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 20);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 23);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 25);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 26);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 27);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptEventCashReceiptType2()
  {
    return ReadEach("ReadCashReceiptDetailCashReceiptEventCashReceiptType2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber", entities.LegalAction.StandardNumber ?? ""
          );
        db.
          SetDate(command, "date1", export.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", export.SearchTo.Date.GetValueOrDefault());
        db.SetDate(
          command, "collectionDate", local.BlankDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 15);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 17);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptType.Code = db.GetString(reader, 19);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 20);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 23);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 25);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 26);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 27);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptEventCashReceiptType3()
  {
    return ReadEach("ReadCashReceiptDetailCashReceiptEventCashReceiptType3",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", export.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", export.SearchTo.Date.GetValueOrDefault());
        db.
          SetNullableString(command, "oblgorSsn", export.CsePersonsWorkSet.Ssn);
          
        db.SetNullableString(command, "oblgorPrsnNbr", export.CsePerson.Number);
        db.SetDate(
          command, "collectionDate", local.BlankDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 15);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 17);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptType.Code = db.GetString(reader, 19);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 20);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 23);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 25);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 26);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 27);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
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
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;

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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
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
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
      }

      /// <summary>
      /// A value of FnReferenceNumber.
      /// </summary>
      [JsonPropertyName("fnReferenceNumber")]
      public FnReferenceNumber FnReferenceNumber
      {
        get => fnReferenceNumber ??= new();
        set => fnReferenceNumber = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
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
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CollectionType collectionType;
      private CashReceiptDetailStatus status;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceiptType detailCashReceiptType;
      private FnReferenceNumber fnReferenceNumber;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
    }

    /// <summary>
    /// A value of SelCourtOrderNo.
    /// </summary>
    [JsonPropertyName("selCourtOrderNo")]
    public CashReceiptDetail SelCourtOrderNo
    {
      get => selCourtOrderNo ??= new();
      set => selCourtOrderNo = value;
    }

    /// <summary>
    /// A value of PromptAmt.
    /// </summary>
    [JsonPropertyName("promptAmt")]
    public TextWorkArea PromptAmt
    {
      get => promptAmt ??= new();
      set => promptAmt = value;
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
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of CurrentOwed.
    /// </summary>
    [JsonPropertyName("currentOwed")]
    public Common CurrentOwed
    {
      get => currentOwed ??= new();
      set => currentOwed = value;
    }

    /// <summary>
    /// A value of ArrearsOwed.
    /// </summary>
    [JsonPropertyName("arrearsOwed")]
    public Common ArrearsOwed
    {
      get => arrearsOwed ??= new();
      set => arrearsOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
    }

    /// <summary>
    /// A value of TotalUndistAmt.
    /// </summary>
    [JsonPropertyName("totalUndistAmt")]
    public Common TotalUndistAmt
    {
      get => totalUndistAmt ??= new();
      set => totalUndistAmt = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FlowSelected.
    /// </summary>
    [JsonPropertyName("flowSelected")]
    public CsePerson FlowSelected
    {
      get => flowSelected ??= new();
      set => flowSelected = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private CashReceiptDetail selCourtOrderNo;
    private TextWorkArea promptAmt;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common obligorPrompt;
    private Common nextTransaction;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common currentOwed;
    private Common arrearsOwed;
    private Common interestOwed;
    private Common totalOwed;
    private Common totalUndistAmt;
    private Array<ImportGroup> import1;
    private DateWorkArea dateWorkArea;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private CsePerson flowSelected;
    private TextWorkArea prompt;
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
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
      }

      /// <summary>
      /// A value of FnReferenceNumber.
      /// </summary>
      [JsonPropertyName("fnReferenceNumber")]
      public FnReferenceNumber FnReferenceNumber
      {
        get => fnReferenceNumber ??= new();
        set => fnReferenceNumber = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
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
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CollectionType collectionType;
      private CashReceiptDetailStatus status;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceiptType detailCashReceiptType;
      private FnReferenceNumber fnReferenceNumber;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
    }

    /// <summary>
    /// A value of SelCourtOrderNo.
    /// </summary>
    [JsonPropertyName("selCourtOrderNo")]
    public CashReceiptDetail SelCourtOrderNo
    {
      get => selCourtOrderNo ??= new();
      set => selCourtOrderNo = value;
    }

    /// <summary>
    /// A value of PromptAmt.
    /// </summary>
    [JsonPropertyName("promptAmt")]
    public TextWorkArea PromptAmt
    {
      get => promptAmt ??= new();
      set => promptAmt = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CashReceiptDetail Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of CurrentOwed.
    /// </summary>
    [JsonPropertyName("currentOwed")]
    public Common CurrentOwed
    {
      get => currentOwed ??= new();
      set => currentOwed = value;
    }

    /// <summary>
    /// A value of ArrearsOwed.
    /// </summary>
    [JsonPropertyName("arrearsOwed")]
    public Common ArrearsOwed
    {
      get => arrearsOwed ??= new();
      set => arrearsOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
    }

    /// <summary>
    /// A value of TotalUndistAmt.
    /// </summary>
    [JsonPropertyName("totalUndistAmt")]
    public Common TotalUndistAmt
    {
      get => totalUndistAmt ??= new();
      set => totalUndistAmt = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of LcdaPassFrom.
    /// </summary>
    [JsonPropertyName("lcdaPassFrom")]
    public DateWorkArea LcdaPassFrom
    {
      get => lcdaPassFrom ??= new();
      set => lcdaPassFrom = value;
    }

    /// <summary>
    /// A value of LcdaPassTo.
    /// </summary>
    [JsonPropertyName("lcdaPassTo")]
    public DateWorkArea LcdaPassTo
    {
      get => lcdaPassTo ??= new();
      set => lcdaPassTo = value;
    }

    /// <summary>
    /// A value of FlowToKdmv.
    /// </summary>
    [JsonPropertyName("flowToKdmv")]
    public CsePersonsWorkSet FlowToKdmv
    {
      get => flowToKdmv ??= new();
      set => flowToKdmv = value;
    }

    private CashReceiptDetail selCourtOrderNo;
    private TextWorkArea promptAmt;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private Obligation obligation;
    private ScreenOwedAmounts screenOwedAmounts;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common obligorPrompt;
    private CashReceiptDetail selected;
    private Common nextTransaction;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common currentOwed;
    private Common arrearsOwed;
    private Common interestOwed;
    private Common totalOwed;
    private Common totalUndistAmt;
    private Array<ExportGroup> export1;
    private DateWorkArea dateWorkArea;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private TextWorkArea prompt;
    private DateWorkArea lcdaPassFrom;
    private DateWorkArea lcdaPassTo;
    private CsePersonsWorkSet flowToKdmv;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public LegalAction Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of BlankDate.
    /// </summary>
    [JsonPropertyName("blankDate")]
    public DateWorkArea BlankDate
    {
      get => blankDate ??= new();
      set => blankDate = value;
    }

    /// <summary>
    /// A value of ItemSelected.
    /// </summary>
    [JsonPropertyName("itemSelected")]
    public Common ItemSelected
    {
      get => itemSelected ??= new();
      set => itemSelected = value;
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
    /// A value of Undistributed.
    /// </summary>
    [JsonPropertyName("undistributed")]
    public Common Undistributed
    {
      get => undistributed ??= new();
      set => undistributed = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of FtirRestriction.
    /// </summary>
    [JsonPropertyName("ftirRestriction")]
    public Common FtirRestriction
    {
      get => ftirRestriction ??= new();
      set => ftirRestriction = value;
    }

    /// <summary>
    /// A value of FtieRestriction.
    /// </summary>
    [JsonPropertyName("ftieRestriction")]
    public Common FtieRestriction
    {
      get => ftieRestriction ??= new();
      set => ftieRestriction = value;
    }

    private LegalAction prev;
    private DateWorkArea maxDate;
    private DateWorkArea currentDate;
    private AbendData abendData;
    private CsePersonAccount hardcodeObligor;
    private DateWorkArea blankDate;
    private Common itemSelected;
    private DateWorkArea dateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common undistributed;
    private Common work;
    private Common promptCount;
    private Profile profile;
    private Common ftirRestriction;
    private Common ftieRestriction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private CollectionType collectionType;
    private LegalAction legalAction;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private ObligationType obligationType;
    private CashReceiptType cashReceiptType;
    private Obligation obligation;
    private MonthlyObligorSummary monthlyObligorSummary;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}

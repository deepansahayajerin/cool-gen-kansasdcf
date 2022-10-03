// Program: FN_DEBT_LST_DBT_ACTVTY_BY_AP_PYR, ID: 372115849, model: 746.
// Short name: SWEDEBTP
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
/// A program: FN_DEBT_LST_DBT_ACTVTY_BY_AP_PYR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDebtLstDbtActvtyByApPyr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DEBT_LST_DBT_ACTVTY_BY_AP_PYR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDebtLstDbtActvtyByApPyr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDebtLstDbtActvtyByApPyr.
  /// </summary>
  public FnDebtLstDbtActvtyByApPyr(IContext context, Import import,
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
    // *************************************************************************
    // MOD DOC
    // DATE	  DEVELOPER		REF #	DESCIPTION
    // -------------------------------------------------------------------------
    //           R.B.Mohapatra - MTW            Initial Dev. thru Unit Testing
    // 10/21/96  H. Kennedy - MTW		Cosmetic surgery to the screen.
    // 					Added Data level security.
    // 0/04/1997 C. Dasgupta - MTW
    // Changes made to to make search criteria from 'From Date to To Date' to 
    // month and year
    // 07/22/1997  A Samuels MTW		Put "from date and to date back in
    // *************************************************************************
    // 03/17/97   A.Kinney
    // Various fixes to transaction.  Data not being returned on return from 
    // LIST.  Prompts using permitted values.  Show Debt/Coll were using
    // permitted values.  Second line of screen data was not being populated.
    // Total owed and individual owed amts not correct.  Some flow problems.
    // ****************************************************************************
    // *=====================================================================================*
    // RBM  08/26/97  Changed the Logic not to display the Collections which are
    // Adjusted.
    //                The default for the 'Show Collection Adjustments' screen 
    // field is
    //                changed to "N"
    //      10/08/97  Included Filter to show Only Debts having amounts owed. 
    // Def - 'Y'
    //                Changed display logic to list Debt Details By supported 
    // person, Due date
    //                and Obligation Type.
    //                added the Flow to COLA and necessary logic to accomplish 
    // this.
    // *=====================================================================================*
    // ****************************************************************
    // 09/22/98    E. Parker		Removed logic to bypass security, added exit state
    // when no person # entered, removed "DA" from 1st CASE substr, and cleared
    // 'S' from select_char after returning from COLA.
    // 09/28/98    E. Parker 		Fixed dialog flow to OVOL so it would pass data, 
    // changed flow from LINT to new POFF screen, added flow to OFEE.
    // 11/12/98    E. Parker		Added export_flow_to_cola flag and added 
    // collection system_generated_identifier to group view so it could be
    // passed to COLA.
    // 2/4/1999    E. Parker		Changed logic to derive Debt_Detail Program using 
    // fn_determine_pgm_for_debt_detail instead of using
    // zdel_fn_cab_get_dist_prog_type.
    // 2/18/1999  E. Parker		Changed logic to prevent Voluntary Debt Details 
    // from going to DBAJ for adjustment.  Also made Program field 3 chars long
    // instead of 2.
    // 03/30/99    A. Doty
    // 
    // Converted the Prad to use the new
    // real-time summary AB's.
    // 06/04/99    E. Parker		Added logic to reset exit state if it is 
    // no_case_rl_found_for_supp_person.
    // 06/15/99 M. Brown
    // 
    // Added logic to allow flow to PACC.
    // Need to make sure the selected
    // row is a collection type record,
    // find the payee for the collection,
    // and send the payee and collection
    // date to PACC.  The flow is not
    // allowed if the payee is the state
    // (supported ind = "N").
    // ***************************************************************
    // ****************************************************************
    // 11/19/99 K. Doshi PR#80753
    // Amend data passed via dialog flow to PACC.
    // 3/18/00  - b adams  -  PR# 82600: Unlimited viewing of data;
    //   changed from implicit subscripting to explicit.  Several
    //   'export_temp' views exist for the purpose of debugging.
    //   They were placed on the screen whereever there was room
    //   so values such as subscripts, etc., could be viewed at
    //   various points in the scrolling process.  TSO Trace does
    //   not work properly with EABs here so this was necessary.
    // 3/24/00 - b adams  -  PR# 81829: Performance enhancement.
    // 04/26/2000  Vithal Madhira    PR# 92839    :        Fixed the problem to 
    // display the selected  record  when returning from other screens(OREC,
    // OACC, ONAC, OFEE, OVOL, CRRC)  to DEBT.
    // 09/13/00   Vithal Madhira     WR# 000202        Default 'Show Collection 
    // Adj.' & ' Show Debt Adj.' to  " N ".
    // 09/14/00    Vitha Madhira      WR# 000200    Add Y/N indicator to DEBT 
    // and set to Y if there are future collections. Also set 'To' date to
    // Highest Debt Detail ' Due Date '.
    // 09/20/00   Vithal Madhira      Fixed the PF4(LIST) key. Screen will now 
    // display correct messages and flow to the correct screen only.
    // 09/20/00   Vithal Madhira     Fixed the edits for ' Show Debt Adj.' ,  '
    // Show Coll Adj.'  and  'Show  only debts with amount owed' fields. Now the
    // screen will display error messages for invalid values in these fields.
    // October, 2000, M. Brown, pr# 106234 - Updated NEXT TRAN logic.
    // January, 2001, M. Brown, Work Order# 228 - Changed print processing (pf21
    // ) to flow to POPT.
    // November, 2001, M. Ashworth, PR# 127419 - PAYR to COLL to DEBT to PACC 
    // was displaying wrong obligee.
    // ***************************************************************
    // 5/25/07 M. Fan  PR# 202233 Added codes to qulify read collection 
    // statement to fix the problem of displaying wrong payee account. Also
    // added the entity view of obligation_type
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    MoveNextTranInfo(import.Hidden, export.Hidden);

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    export.CsePersonsWorkSet.Number = import.SearchCsePerson.Number;
    local.KansasObligee.Number = "000000017O";
    local.LinesToFill.Count = 71;
    local.PreviousGroupsToSave.Count = 13;
    local.WrongAcct.SystemGeneratedIdentifier = 5;
    local.Max.Timestamp =
      AddMicroseconds(new DateTime(2099, 12, 31, 23, 59, 59), 999999);
    export.SearchCsePerson.Assign(import.SearchCsePerson);
    export.CsePersonsWorkSet.FormattedName =
      import.CsePersonsWorkSet.FormattedName;
    export.ApPayorPrompt.Text1 = import.ApPayorPrompt.Text1;
    MoveObligationType(import.ObligationType, export.ObligationType);
    export.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    export.CourtOrderPrompt.Text1 = import.CourtOrderPrompt.Text1;
    export.NextTransaction.Command = import.NextTransaction.Command;
    export.SearchFrom.Date = import.SearchFrom.Date;
    export.SearchTo.Date = import.SearchTo.Date;
    MoveLegalAction2(import.SearchLegalAction, export.SearchLegalAction);
    export.UndistributedAmount.TotalCurrency =
      import.UndistributedAmount.TotalCurrency;
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.PromptForCruc.Text1 = import.PromptForCruc.Text1;
    export.SearchShowCollAdj.Text1 = import.SearchShowCollAdj.Text1;
    export.SearchShowDebtAdj.Text1 = import.SearchShowDebtAdj.Text1;
    export.PrinterId.Text8 = import.PrinterId.Text8;
    MoveCommon(import.PagesInGroupView, export.FullPagesInGroupView);
    export.PageNumberOnScreen.Count = import.PageNumberOnScreen.Count;
    export.FromColl.SystemGeneratedIdentifier =
      import.FromColl.SystemGeneratedIdentifier;
    export.HidListDebtsWithAmtOwed.SelectChar =
      import.HidListDebtsWithAmtOwed.SelectChar;
    export.HidSearchFrom.Date = import.HidSearchFrom.Date;
    export.HidSearchTo.Date = import.HidSearchTo.Date;
    MoveCsePerson(import.HidSearchCsePerson, export.HidSearchCsePerson);
    export.HidSearchLegalAction.StandardNumber =
      import.HidSearchLegalAction.StandardNumber;
    export.HidSearchShowCollAdj.Text1 = import.HidSearchShowCollAdj.Text1;
    export.HidSearchShowDebtAdj.Text1 = import.HidSearchShowDebtAdj.Text1;
    export.ScrollIndicator.Text3 = import.ScrollIndicator.Text3;
    export.GroupViewRetrieved.Count = import.GroupViewRetrieved.Count;
    export.FutureCollection.Flag = import.FutureCollection.Flag;

    if (IsEmpty(import.ListDebtsWithAmtOwed.SelectChar))
    {
      export.ListDebtsWithAmtOwed.SelectChar = "Y";
    }
    else
    {
      export.ListDebtsWithAmtOwed.SelectChar =
        import.ListDebtsWithAmtOwed.SelectChar;
    }

    // =================================================
    // 3/3/00 - B Adams  -  PR# 82600: Explicit scrolling
    // =================================================
    if (Equal(global.Command, "DISPLAY"))
    {
      local.NextPageCollection.SystemGeneratedIdentifier = 0;
      local.NextPageObligation.SystemGeneratedIdentifier =
        export.Obligation.SystemGeneratedIdentifier;
      local.NextPageObligationTransaction.SystemGeneratedIdentifier =
        export.FromColl.SystemGeneratedIdentifier;
      local.NextPageObligationTransactionRln.CreatedTmst = local.Max.Timestamp;
      local.NextPageObligationType.Code = export.ObligationType.Code;
      local.NextPageSearchTo.Date = export.SearchTo.Date;

      for(import.Xxx.Index = 0; import.Xxx.Index < import.Xxx.Count; ++
        import.Xxx.Index)
      {
        if (!import.Xxx.CheckSize())
        {
          break;
        }

        export.X.Update.DtlXCommon.SelectChar =
          local.Blank.Item.DtlBlankCommon.SelectChar;
        export.X.Update.DtlXListScreenWorkArea.TextLine76 =
          local.Blank.Item.DtlBlankListScreenWorkArea.TextLine76;
        export.X.Update.DtlHiddenXCsePersonsWorkSet.Number =
          local.Blank.Item.DtlBlankCsePersonsWorkSet.Number;
        MoveLegalAction1(local.Blank.Item.DtlBlankLegalAction,
          export.X.Update.DtlHiddenXLegalAction);
        export.X.Update.DtlHiddnDisplayLineIndX.Flag =
          local.Blank.Item.DtlBlankInd.Flag;
      }

      import.Xxx.CheckIndex();

      for(import.PrevStartingValue.Index = 0; import.PrevStartingValue.Index < import
        .PrevStartingValue.Count; ++import.PrevStartingValue.Index)
      {
        if (!import.PrevStartingValue.CheckSize())
        {
          break;
        }

        export.PrevStartingValue.Update.DtlPreviousCollection.
          SystemGeneratedIdentifier =
            local.Blank.Item.DtlBlankCollection.SystemGeneratedIdentifier;
        export.PrevStartingValue.Update.DtlPreviousDebtDetail.DueDt =
          local.Blank.Item.DtlBlankDebtDetail.DueDt;
        export.PrevStartingValue.Update.DtlPreviousObligation.
          SystemGeneratedIdentifier =
            local.Blank.Item.DtlBlankObligation.SystemGeneratedIdentifier;
        MoveObligationTransaction(local.Blank.Item.
          DtlBlankObligationTransaction,
          export.PrevStartingValue.Update.DtlPreviousObligationTransaction);

        export.PrevStartingValue.Update.DtlPreviousAdjusted.CreatedTmst =
          local.Blank.Item.DtlBlankObligationTransactionRln.CreatedTmst;
        export.PrevStartingValue.Update.DtlPreviousDispLinInd.Flag =
          local.Blank.Item.DtlBlankInd.Flag;
      }

      import.PrevStartingValue.CheckIndex();
    }
    else
    {
      for(import.Xxx.Index = 0; import.Xxx.Index < import.Xxx.Count; ++
        import.Xxx.Index)
      {
        if (!import.Xxx.CheckSize())
        {
          break;
        }

        export.Xxx.Index = import.Xxx.Index;
        export.Xxx.CheckSize();

        export.Xxx.Update.DtlHiddnDispLineInd.Flag =
          import.Xxx.Item.DtlHiddenDispLinInd.Flag;
        export.Xxx.Update.DtlHiddenCashReceipt.SequentialNumber =
          import.Xxx.Item.DtlHiddenCashReceipt.SequentialNumber;
        export.Xxx.Update.DtlHiddenCashReceiptDetail.SequentialIdentifier =
          import.Xxx.Item.DtlHiddenCashReceiptDetail.SequentialIdentifier;
        export.Xxx.Update.DtlHiddenCashReceiptEvent.SystemGeneratedIdentifier =
          import.Xxx.Item.DtlHiddenCashReceiptEvent.SystemGeneratedIdentifier;
        export.Xxx.Update.DtlHiddenCashReceiptSourceType.
          SystemGeneratedIdentifier =
            import.Xxx.Item.DtlHiddenCashReceiptSourceType.
            SystemGeneratedIdentifier;
        export.Xxx.Update.DtlHiddenCashReceiptType.SystemGeneratedIdentifier =
          import.Xxx.Item.DtlHiddenCashReceiptType.SystemGeneratedIdentifier;
        export.Xxx.Update.DtlHiddenCollection.SystemGeneratedIdentifier =
          import.Xxx.Item.DtlHiddenCollection.SystemGeneratedIdentifier;
        export.Xxx.Update.DtlHiddenCsePersonsWorkSet.Number =
          import.Xxx.Item.DtlHiddenCsePersonsWorkSet.Number;
        export.Xxx.Update.DtlHiddenDebtDetail.DueDt =
          import.Xxx.Item.DtlHiddenDebtDetail.DueDt;
        MoveLegalAction1(import.Xxx.Item.DtlHiddenLegalAction,
          export.Xxx.Update.DtlHiddenLegalAction);
        MoveObligation(import.Xxx.Item.DtlHiddenObligation,
          export.Xxx.Update.DtlHiddenObligation);
        MoveObligationTransaction(import.Xxx.Item.
          DtlHiddenObligationTransaction,
          export.Xxx.Update.DtlHiddenObligationTransaction);
        export.Xxx.Update.DtlHiddenObligationType.Assign(
          import.Xxx.Item.DtlHiddenObligationType);
        export.Xxx.Update.DtlHiddenCommon.SelectChar =
          import.Xxx.Item.DtlHiddenCommon.SelectChar;
        export.Xxx.Update.DtlHiddenListScreenWorkArea.TextLine76 =
          import.Xxx.Item.DtlHiddenListScreenWorkArea.TextLine76;
        export.Xxx.Update.DtlHiddenAdjusted.CreatedTmst =
          import.Xxx.Item.DtlHiddenAdjusted.CreatedTmst;
      }

      import.Xxx.CheckIndex();

      for(import.PrevStartingValue.Index = 0; import.PrevStartingValue.Index < import
        .PrevStartingValue.Count; ++import.PrevStartingValue.Index)
      {
        if (!import.PrevStartingValue.CheckSize())
        {
          break;
        }

        export.PrevStartingValue.Index = import.PrevStartingValue.Index;
        export.PrevStartingValue.CheckSize();

        export.PrevStartingValue.Update.DtlPreviousCollection.
          SystemGeneratedIdentifier =
            import.PrevStartingValue.Item.DtlPreviousCollection.
            SystemGeneratedIdentifier;
        export.PrevStartingValue.Update.DtlPreviousDebtDetail.DueDt =
          import.PrevStartingValue.Item.DtlPreviousDebtDetail.DueDt;
        export.PrevStartingValue.Update.DtlPreviousObligation.
          SystemGeneratedIdentifier =
            import.PrevStartingValue.Item.DtlPreviousObligation.
            SystemGeneratedIdentifier;
        MoveObligationTransaction(import.PrevStartingValue.Item.
          DtlPreviousObligationTransaction,
          export.PrevStartingValue.Update.DtlPreviousObligationTransaction);
        export.PrevStartingValue.Update.DtlPreviousObligationType.Code =
          import.PrevStartingValue.Item.DtlPreviousObligationType.Code;
        export.PrevStartingValue.Update.DtlPreviousAdjusted.CreatedTmst =
          import.PrevStartingValue.Item.DtlPreviousAdjusted.CreatedTmst;
        export.PrevStartingValue.Update.DtlPreviousDispLinInd.Flag =
          import.PrevStartingValue.Item.DtlPreviousDispLinInd.Flag;
      }

      import.PrevStartingValue.CheckIndex();
    }

    if (AsChar(export.ListDebtsWithAmtOwed.SelectChar) == AsChar
      (export.HidListDebtsWithAmtOwed.SelectChar) && Equal
      (export.SearchCsePerson.Number, export.HidSearchCsePerson.Number) && Equal
      (export.SearchLegalAction.StandardNumber,
      export.HidSearchLegalAction.StandardNumber) && Equal
      (export.SearchFrom.Date, export.HidSearchFrom.Date) && AsChar
      (export.SearchShowCollAdj.Text1) == AsChar
      (export.HidSearchShowCollAdj.Text1) && AsChar
      (export.SearchShowDebtAdj.Text1) == AsChar
      (export.HidSearchShowDebtAdj.Text1) && Equal
      (export.SearchTo.Date, export.HidSearchTo.Date))
    {
      local.KeyChange.Flag = "N";
    }
    else
    {
      local.KeyChange.Flag = "Y";
    }

    if (Equal(global.Command, "NEXT"))
    {
      if (AsChar(local.KeyChange.Flag) == 'Y')
      {
        ExitState = "ACO_NEXT_INVALID_WITH_KEY_CHANGE";

        return;
      }

      if (export.PageNumberOnScreen.Count == export
        .FullPagesInGroupView.Count && export.Xxx.Count == local
        .LinesToFill.Count)
      {
        global.Command = "DISPLAY";

        export.Xxx.Index = local.LinesToFill.Count - 1;
        export.Xxx.CheckSize();

        local.NextPageObligation.SystemGeneratedIdentifier =
          export.Xxx.Item.DtlHiddenObligation.SystemGeneratedIdentifier;
        local.NextPageObligationTransaction.SystemGeneratedIdentifier =
          export.Xxx.Item.DtlHiddenObligationTransaction.
            SystemGeneratedIdentifier;
        local.NextPageObligationTransaction.Type1 =
          export.Xxx.Item.DtlHiddenObligationTransaction.Type1;
        local.NextPageObligationTransactionRln.CreatedTmst =
          export.Xxx.Item.DtlHiddenAdjusted.CreatedTmst;
        local.NextPageObligationType.Code =
          export.Xxx.Item.DtlHiddenObligationType.Code;
        local.NextPageCollection.SystemGeneratedIdentifier =
          export.Xxx.Item.DtlHiddenCollection.SystemGeneratedIdentifier;
        local.NextPageSearchTo.Date = export.Xxx.Item.DtlHiddenDebtDetail.DueDt;
        local.NextPageDisplayLineInd.Flag =
          export.Xxx.Item.DtlHiddnDispLineInd.Flag;

        export.Xxx.Index = 0;
        export.Xxx.CheckSize();

        // =================================================
        // User did not change the input keys, but it's time to get the
        //   xxx_group_export view filled again.
        // =================================================
        local.KeyChange.Flag = "Y";
        local.PfkeyPressed.Text4 = "NEXT";
      }
    }

    if (Equal(global.Command, "PREV"))
    {
      if (AsChar(local.KeyChange.Flag) == 'Y')
      {
        ExitState = "ACO_PREV_INVALID_WITH_KEY_CHANGE";

        return;
      }

      if (export.PageNumberOnScreen.Count == 1 && export
        .GroupViewRetrieved.Count > 1 && export.GroupViewRetrieved.Count < local
        .PreviousGroupsToSave.Count + 1)
      {
        global.Command = "DISPLAY";

        import.PrevStartingValue.Index = export.GroupViewRetrieved.Count - 2;
        import.PrevStartingValue.CheckSize();

        local.NextPageObligation.SystemGeneratedIdentifier =
          import.PrevStartingValue.Item.DtlPreviousObligation.
            SystemGeneratedIdentifier;
        local.NextPageObligationTransaction.SystemGeneratedIdentifier =
          import.PrevStartingValue.Item.DtlPreviousObligationTransaction.
            SystemGeneratedIdentifier;
        local.NextPageObligationTransaction.Type1 =
          import.PrevStartingValue.Item.DtlPreviousObligationTransaction.Type1;
        local.NextPageObligationTransactionRln.CreatedTmst =
          import.PrevStartingValue.Item.DtlPreviousAdjusted.CreatedTmst;
        local.NextPageObligationType.Code =
          import.PrevStartingValue.Item.DtlPreviousObligationType.Code;
        local.NextPageCollection.SystemGeneratedIdentifier =
          import.PrevStartingValue.Item.DtlPreviousCollection.
            SystemGeneratedIdentifier;
        local.NextPageSearchTo.Date =
          import.PrevStartingValue.Item.DtlPreviousDebtDetail.DueDt;
        local.NextPageDisplayLineInd.Flag =
          import.PrevStartingValue.Item.DtlPreviousDispLinInd.Flag;

        // =================================================
        // User did not change the input keys, but it's time to get the
        //   xxx_group_export view filled again.
        // =================================================
        local.KeyChange.Flag = "Y";
        local.PfkeyPressed.Text4 = "PREV";
      }
    }

    // =================================================
    //     end of PR# 82600 additions
    // =================================================
    if (Equal(global.Command, "RETLAPS"))
    {
      // *** On return from LIST, populate screen Court Order field. ***
      export.CourtOrderPrompt.Text1 = "";

      if (import.SearchLegalAction.Identifier > 0)
      {
        if (ReadLegalAction())
        {
          export.SearchLegalAction.StandardNumber =
            entities.LegalAction.StandardNumber;
        }
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "FROMCOLL"))
    {
      export.SearchShowCollAdj.Text1 = "Y";
      export.ListDebtsWithAmtOwed.SelectChar = "N";
      export.HidSearchCsePerson.Number = export.SearchCsePerson.Number;
      export.HidSearchLegalAction.StandardNumber =
        export.SearchLegalAction.StandardNumber ?? "";
      global.Command = "DISPLAY";
    }

    if (IsEmpty(global.Command) || Equal(global.Command, "RETLCDA"))
    {
      global.Command = "DISPLAY";
    }

    // ***************************************************************
    // Deleted IF COMMAND IS EQUAL TO retlint because this screen no longer 
    // flows to and from LINT -- E. Parker  9/28/98
    // ***************************************************************
    if (Equal(global.Command, "RETCRUC"))
    {
      export.PromptForCruc.Text1 = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETNAME"))
    {
      export.ApPayorPrompt.Text1 = "";

      if (!IsEmpty(import.FromList.Number))
      {
        MoveCsePersonsWorkSet(import.FromList, export.CsePersonsWorkSet);
        export.SearchCsePerson.Number = import.FromList.Number;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // *** Do nothing, this will clear the screen for a fresh display ***
      export.ApPayorPrompt.Text1 = "";
      export.CourtOrderPrompt.Text1 = "";
      export.PromptForCruc.Text1 = "";
      local.KeyChange.Flag = "Y";
    }
    else
    {
      // =================================================
      // 3/3/00 - b adams  -  PR#82600: Changed structure of how
      //   this is done to support explicit scrolling.
      // =================================================
      export.X.Index = -1;

      for(import.X.Index = 0; import.X.Index < import.X.Count; ++import.X.Index)
      {
        if (!import.X.CheckSize())
        {
          break;
        }

        export.X.Index = import.X.Index;
        export.X.CheckSize();

        export.X.Update.DtlXCommon.SelectChar =
          import.X.Item.DtlXCommon.SelectChar;
        export.X.Update.DtlHiddnDisplayLineIndX.Flag =
          import.X.Item.DtlHiddnDisplayLineIndX.Flag;
        export.X.Update.DtlXListScreenWorkArea.TextLine76 =
          import.X.Item.DtlXListScreenWorkArea.TextLine76;
        export.X.Update.DtlHiddenXObligationType.Classification =
          import.X.Item.DtlHiddenXObligationType.Classification;
        export.X.Update.DtlHiddenXObligationTransaction.Type1 =
          import.X.Item.DtlHiddenXObligationTransaction.Type1;

        switch(AsChar(export.X.Item.DtlXCommon.SelectChar))
        {
          case ' ':
            // : Not selected, continue processing.
            break;
          case 'S':
            // ****************************************************************
            // Added retcola to If -- E. Parker  9/22/98
            // ****************************************************************
            if (Equal(global.Command, "RETCOLA") || Equal
              (global.Command, "RETDBAJ"))
            {
              export.Xxx.Update.DtlHiddenCommon.SelectChar = "";
            }
            else
            {
              ++local.Counter.Count;
            }

            if (local.Counter.Count > 1)
            {
              var field1 = GetField(export.X.Item.DtlXCommon, "selectChar");

              field1.Error = true;

              local.Counter.Flag = "E";
              ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN3";
            }
            else
            {
              import.Xxx.Index = (int)((export.PageNumberOnScreen.Count - 1) * (
                long)10 + import.X.Index + 1 - 1);
              import.Xxx.CheckSize();

              export.ToNextTransactionObligation.SystemGeneratedIdentifier =
                import.Xxx.Item.DtlHiddenObligation.SystemGeneratedIdentifier;
              MoveObligationTransaction(import.Xxx.Item.
                DtlHiddenObligationTransaction,
                export.ToNextTransactionObligationTransaction);
              export.ToNextTransactionObligationType.Assign(
                import.Xxx.Item.DtlHiddenObligationType);
              local.ObligationType.Classification =
                import.Xxx.Item.DtlHiddenObligationType.Classification;
              MoveLegalAction1(import.Xxx.Item.DtlHiddenLegalAction,
                export.ToNextTransactionLegalAction);
              export.PassSupported.Number =
                import.Xxx.Item.DtlHiddenCsePersonsWorkSet.Number;
            }

            break;
          default:
            var field = GetField(export.X.Item.DtlXCommon, "selectChar");

            field.Error = true;

            local.Counter.Flag = "E";
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        // ****************************************************************
        // Removed CASE "DA"  --  E. Parker  9/22/98
        // ****************************************************************
        switch(TrimEnd(Substring(
          export.X.Item.DtlXListScreenWorkArea.TextLine76, 1, 2)))
        {
          case "DE":
            break;
          case "CO":
            var field1 =
              GetField(export.X.Item.DtlXListScreenWorkArea, "textLine76");

            field1.Color = "cyan";
            field1.Intensity = Intensity.High;
            field1.Protected = true;

            break;
          default:
            var field2 = GetField(export.X.Item.DtlXCommon, "selectChar");

            field2.Intensity = Intensity.Dark;
            field2.Protected = true;

            break;
        }
      }

      import.X.CheckIndex();
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    // *** If it is Return from DBAJ screen, escape
    // ------------------------------------------------------------------------------
    // PR# 92839  :    New COMMANDS are added to the IF statement to fix this 
    // problem report. User can select a DE or CO record and PF16 to flow to
    // OACC, OREC, ONAC, OFEE, OVOL or can flow to CRRC, PACC, COLA, DBAJ .
    // Whenever user returns to DEBT screen from OACC, OREC, ONAC, OFEE, OVOL,
    // CRRC, PACC, COLA, DBAJ by pressing PF9 on these screens; need to display
    // the selected record instead of returning to top of the list. Also after
    // returning to DEBT screen the 'S' in selection field must be removed.
    //                                                                
    // Vithal (04/26/2000)
    // -------------------------------------------------------------------------
    if (Equal(global.Command, "RETDBAJ") || Equal
      (global.Command, "RETCOLA") || Equal(global.Command, "RETOACC") || Equal
      (global.Command, "RETOFEE") || Equal(global.Command, "RETONAC") || Equal
      (global.Command, "RETOREC") || Equal(global.Command, "RETOVOL") || Equal
      (global.Command, "RETCRRC") || Equal(global.Command, "RETPACC"))
    {
      for(export.X.Index = 0; export.X.Index < Export.XGroup.Capacity; ++
        export.X.Index)
      {
        if (!export.X.CheckSize())
        {
          break;
        }

        if (AsChar(export.X.Item.DtlXCommon.SelectChar) == 'S')
        {
          export.X.Update.DtlXCommon.SelectChar = "";
        }
      }

      export.X.CheckIndex();

      // ***---  Light up the 'transaction information line' when returning.  b 
      // adams - 5/6/00
      if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
      {
        var field = GetField(export.ScreenOwedAmounts, "errorInformationLine");

        field.Color = "red";
        field.Intensity = Intensity.High;
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = true;
      }

      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // : User requested NEXT TRAN - now validate.
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // set the next_tran_info attributes to the import view attributes for the
      // data to be passed to the next transaction
      // ****
      // : M. Brown, Oct 2000, pr# 106234
      if (!Equal(export.SearchCsePerson.Number, export.HidSearchCsePerson.Number)
        && !IsEmpty(export.SearchCsePerson.Number))
      {
        // : New cse person number entered, but pf2 not pressed, so set person #
        //   to new number and initialize all other next tran keys.
        local.PadCsePersonNum.Text10 = export.SearchCsePerson.Number;
        UseEabPadLeftWithZeros();
        export.Hidden.CsePersonNumberAp = local.PadCsePersonNum.Text10;
        export.Hidden.CsePersonNumber = local.PadCsePersonNum.Text10;
        export.Hidden.CsePersonNumberObligor = local.PadCsePersonNum.Text10;
        export.Hidden.StandardCrtOrdNumber = "";
        export.Hidden.LegalActionIdentifier = 0;
        export.Hidden.MiscNum1 = 0;
        export.Hidden.CsePersonNumberObligee = "";
        export.Hidden.CaseNumber = "";
        export.Hidden.CourtOrderNumber = "";
        export.Hidden.CourtCaseNumber = "";
        export.Hidden.ObligationId = 0;
        export.Hidden.MiscNum2 = 0;
      }
      else if (!Equal(export.HidSearchCsePerson.Number,
        export.Hidden.CsePersonNumber) && !
        IsEmpty(export.HidSearchCsePerson.Number))
      {
        export.Hidden.CsePersonNumberAp = export.HidSearchCsePerson.Number;
        export.Hidden.CsePersonNumber = export.HidSearchCsePerson.Number;
        export.Hidden.CsePersonNumberObligor = export.HidSearchCsePerson.Number;
        export.Hidden.StandardCrtOrdNumber =
          export.HidSearchLegalAction.StandardNumber ?? "";
        export.Hidden.LegalActionIdentifier = 0;
        export.Hidden.MiscNum1 = 0;
        export.Hidden.CsePersonNumberObligee = "";
        export.Hidden.CaseNumber = "";
        export.Hidden.CourtOrderNumber = "";
        export.Hidden.CourtCaseNumber = "";
        export.Hidden.ObligationId = 0;
        export.Hidden.MiscNum2 = 0;

        // : Populate next tran with obligation info if available.
        if (export.Obligation.SystemGeneratedIdentifier > 0)
        {
          export.Hidden.ObligationId =
            export.Obligation.SystemGeneratedIdentifier;
          export.Hidden.MiscNum2 =
            export.ObligationType.SystemGeneratedIdentifier;

          export.Xxx.Index = 0;
          export.Xxx.CheckSize();

          export.Hidden.LegalActionIdentifier =
            export.Xxx.Item.DtlHiddenLegalAction.Identifier;
        }
      }

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

      // : M. Brown, Oct 2000, pr# 106234
      export.SearchCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);
      export.SearchLegalAction.StandardNumber =
        export.Hidden.StandardCrtOrdNumber ?? "";
      export.Obligation.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      export.ObligationType.SystemGeneratedIdentifier =
        (int)export.Hidden.MiscNum2.GetValueOrDefault();
      global.Command = "DISPLAY";
    }

    // *****************************************************************
    // ----> User_id "If" condition temporarily added. To be removed later.
    //      -Syed
    // Removed Userid If Statement.  E. Parker 9/22/98
    // *****************************************************************
    if (Equal(global.Command, "CRRC") || Equal(global.Command, "DBAJ") || Equal
      (global.Command, "OBLGM") || Equal(global.Command, "LCDA") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "COLA") || Equal
      (global.Command, "PACC"))
    {
      // * continue *
    }
    else
    {
      // *****
      // data level security added 10/21/96
      // *****
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // * continue *
      }
      else
      {
        return;
      }
    }

    // ***** Left Padding the cse_Person Number *****
    if (IsEmpty(export.SearchCsePerson.Number))
    {
      // ***** No need of Padding
    }
    else
    {
      local.PadCsePersonNum.Text10 = export.SearchCsePerson.Number;

      // #################################################
      // ##
      // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
      // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
      // ##
      // #################################################
      UseEabPadLeftWithZeros();
      export.SearchCsePerson.Number = local.PadCsePersonNum.Text10;
      export.CsePersonsWorkSet.Number = local.PadCsePersonNum.Text10;
    }

    if (local.Counter.Count > 1)
    {
      ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN3";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "POFF":
        // ****************************************************************
        // Changed command from LINK to POFF -- E. Parker 9/28/98
        // ****************************************************************
        // This command should cause a flow to the POFF(Interest) screen.
        // ***Disabled flow to POFF for now as it is a future enhancement.  E. 
        // Parker - 06/08/99
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        return;
      case "DISPLAY":
        // ***************************************************************
        // Added the following code to reset the values.
        //                                                  
        // Vithal Madhira (09/14/00)
        // ***************************************************************
        if (!Equal(export.SearchCsePerson.Number,
          export.HidSearchCsePerson.Number) && !
          IsEmpty(export.HidSearchCsePerson.Number))
        {
          export.SearchFrom.Date = local.Zero.Date;
          export.SearchTo.Date = local.Zero.Date;
          export.FutureCollection.Flag = "";
        }

        if (!Equal(export.SearchLegalAction.StandardNumber,
          export.HidSearchLegalAction.StandardNumber) && !
          IsEmpty(export.HidSearchLegalAction.StandardNumber))
        {
          export.SearchFrom.Date = local.Zero.Date;
          export.SearchTo.Date = local.Zero.Date;
          export.FutureCollection.Flag = "";
        }

        // ***************************************************************
        // Added exit state -- E. Parker  9/22/98
        // ***************************************************************
        if (IsEmpty(export.SearchCsePerson.Number))
        {
          var field = GetField(export.SearchCsePerson, "number");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = false;
          field.Focused = true;

          // ***************************************************************
          // Added the following code to reset the values.
          //                                                  
          // Vithal Madhira (09/14/00)
          // ***************************************************************
          export.SearchFrom.Date = local.Zero.Date;
          export.SearchTo.Date = local.Zero.Date;
          export.CsePersonsWorkSet.FormattedName = "";
          export.FutureCollection.Flag = "";
          ExitState = "CSE_PERSON_NO_REQUIRED";

          return;
        }

        if (IsEmpty(import.SearchShowDebtAdj.Text1))
        {
          // --------------------------------------------------------------------------------
          // Per WR# 000202  the default value for 'Debt Adj' is changed to N.
          //                                                            
          // ----- Vithal ( 09/13/00)
          // --------------------------------------------------------------------------------
          export.SearchShowDebtAdj.Text1 = "N";
        }
        else if (AsChar(import.SearchShowDebtAdj.Text1) == 'Y' || AsChar
          (export.SearchShowDebtAdj.Text1) == 'N')
        {
          // * continue *
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          var field = GetField(export.SearchShowDebtAdj, "text1");

          field.Error = true;
        }

        // ------------------------------------------------------------------------------------
        // The Default for Coll. Adj Indicator is changed to "N" and for Debt. 
        // Adj. Indicator is "Y". The user can choose to display either one or
        // both of the adjustments.
        // ------------------------------------------------------------------------------------
        if (IsEmpty(import.SearchShowCollAdj.Text1))
        {
          export.SearchShowCollAdj.Text1 = "N";
        }
        else if (AsChar(import.SearchShowCollAdj.Text1) == 'Y' || AsChar
          (import.SearchShowCollAdj.Text1) == 'N')
        {
          // * continue *
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          var field = GetField(export.SearchShowCollAdj, "text1");

          field.Error = true;
        }

        // =============================================================================
        // Added this code to check if the user entered only Y or N. All other 
        // values are invalid and give an error message.
        // Also deleted the exitstate " ZD_aco_ne0000_invalid_indicator"  and 
        // added  "aco_ne0000_invalid_indicator_yn".
        //                                        
        // ---- Vithal Madhira  (09/20/00)
        // ========================================================================
        if (IsEmpty(export.ListDebtsWithAmtOwed.SelectChar))
        {
          export.ListDebtsWithAmtOwed.SelectChar = "Y";
        }
        else if (AsChar(export.ListDebtsWithAmtOwed.SelectChar) == 'Y' || AsChar
          (export.ListDebtsWithAmtOwed.SelectChar) == 'N')
        {
          // * continue *
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          var field = GetField(export.ListDebtsWithAmtOwed, "selectChar");

          field.Error = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // =================================================
          //                     Continue.................
          // =================================================
        }
        else
        {
          return;
        }

        // ---------------------------------------------------------------------
        // The following code is to set the TO date.
        // --------------------------------------------------------------------
        local.Current.Date = Now().Date;

        if (Equal(export.SearchTo.Date, local.Zero.Date))
        {
          // ---------------------------------------------------------------------
          // The following code is to get the last date of current month.
          // --------------------------------------------------------------------
          local.Month.TotalInteger = Month(local.Current.Date);
          local.Year.TotalInteger = Year(local.Current.Date);

          if (local.Month.TotalInteger == 12)
          {
            local.Month.TotalInteger = 1;
            ++local.Year.TotalInteger;
          }
          else
          {
            ++local.Month.TotalInteger;
          }

          local.EndOfMonth.Date =
            AddDays(IntToDate((int)((decimal)local.Year.TotalInteger * 10000 + local
            .Month.TotalInteger * 100 + 1)), -1);

          // --------------------------------------------------------------------------------
          // Per WR#000200  the following code is added:
          // 1. to set the TO date to Highest Debt Detail  Due Date.
          // 2. to set the " Future Coll "  flag.
          // -------------------------------------------------------------------------------
          if (export.Obligation.SystemGeneratedIdentifier == 0)
          {
            local.Temp.SystemGeneratedIdentifier = 999;
            export.FutureCollection.Flag = "N";

            if (IsEmpty(export.SearchLegalAction.StandardNumber))
            {
              foreach(var item in ReadDebtDetailCollection4())
              {
                if (!Lt(local.EndOfMonth.Date, entities.DebtDetail.DueDt))
                {
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "N";

                  goto Test1;
                }
                else
                {
                  local.Month.TotalInteger = Month(entities.DebtDetail.DueDt);
                  local.Year.TotalInteger = Year(entities.DebtDetail.DueDt);

                  if (local.Month.TotalInteger == 12)
                  {
                    local.Month.TotalInteger = 1;
                    ++local.Year.TotalInteger;
                  }
                  else
                  {
                    ++local.Month.TotalInteger;
                  }

                  local.EndOfMonth.Date =
                    AddDays(IntToDate((int)((decimal)local.Year.TotalInteger * 10000
                    + local.Month.TotalInteger * 100 + 1)), -1);
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "Y";

                  goto Test1;
                }
              }
            }
            else
            {
              foreach(var item in ReadDebtDetailCollection2())
              {
                if (!Lt(local.EndOfMonth.Date, entities.DebtDetail.DueDt))
                {
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "N";

                  goto Test1;
                }
                else
                {
                  local.Month.TotalInteger = Month(entities.DebtDetail.DueDt);
                  local.Year.TotalInteger = Year(entities.DebtDetail.DueDt);

                  if (local.Month.TotalInteger == 12)
                  {
                    local.Month.TotalInteger = 1;
                    ++local.Year.TotalInteger;
                  }
                  else
                  {
                    ++local.Month.TotalInteger;
                  }

                  local.EndOfMonth.Date =
                    AddDays(IntToDate((int)((decimal)local.Year.TotalInteger * 10000
                    + local.Month.TotalInteger * 100 + 1)), -1);
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "Y";

                  goto Test1;
                }
              }
            }
          }
          else
          {
            local.Temp.SystemGeneratedIdentifier =
              export.Obligation.SystemGeneratedIdentifier;
            export.FutureCollection.Flag = "N";

            if (IsEmpty(export.SearchLegalAction.StandardNumber))
            {
              foreach(var item in ReadDebtDetailCollection3())
              {
                if (!Lt(local.EndOfMonth.Date, entities.DebtDetail.DueDt))
                {
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "N";

                  goto Test1;
                }
                else
                {
                  local.Month.TotalInteger = Month(entities.DebtDetail.DueDt);
                  local.Year.TotalInteger = Year(entities.DebtDetail.DueDt);

                  if (local.Month.TotalInteger == 12)
                  {
                    local.Month.TotalInteger = 1;
                    ++local.Year.TotalInteger;
                  }
                  else
                  {
                    ++local.Month.TotalInteger;
                  }

                  local.EndOfMonth.Date =
                    AddDays(IntToDate((int)((decimal)local.Year.TotalInteger * 10000
                    + local.Month.TotalInteger * 100 + 1)), -1);
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "Y";

                  goto Test1;
                }
              }
            }
            else
            {
              foreach(var item in ReadDebtDetailCollection1())
              {
                if (!Lt(local.EndOfMonth.Date, entities.DebtDetail.DueDt))
                {
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "N";

                  goto Test1;
                }
                else
                {
                  local.Month.TotalInteger = Month(entities.DebtDetail.DueDt);
                  local.Year.TotalInteger = Year(entities.DebtDetail.DueDt);

                  if (local.Month.TotalInteger == 12)
                  {
                    local.Month.TotalInteger = 1;
                    ++local.Year.TotalInteger;
                  }
                  else
                  {
                    ++local.Month.TotalInteger;
                  }

                  local.EndOfMonth.Date =
                    AddDays(IntToDate((int)((decimal)local.Year.TotalInteger * 10000
                    + local.Month.TotalInteger * 100 + 1)), -1);
                  export.SearchTo.Date = local.EndOfMonth.Date;
                  export.FutureCollection.Flag = "Y";

                  goto Test1;
                }
              }
            }
          }

Test1:

          if (Equal(export.SearchTo.Date, local.Zero.Date))
          {
            export.SearchTo.Date = local.EndOfMonth.Date;
          }

          local.NextPageSearchTo.Date = export.SearchTo.Date;
        }

        // ----------------------------------------------------------------------
        // The following code is to set the FROM date.
        // ------------------------------------------------------------------------
        if (Equal(export.SearchFrom.Date, local.Zero.Date) && Lt
          (local.Zero.Date, export.SearchTo.Date))
        {
          local.Month.TotalInteger = Month(local.Current.Date);
          local.Year.TotalInteger = Year(local.Current.Date);

          if (local.Month.TotalInteger == 1)
          {
            local.Month.TotalInteger = 12;
            --local.Year.TotalInteger;
          }
          else
          {
            --local.Month.TotalInteger;
          }

          export.SearchFrom.Date =
            IntToDate((int)((decimal)local.Year.TotalInteger * 10000 + local
            .Month.TotalInteger * 100 + 1));
        }

        // ----------------------------------------------------------------------
        // The following code is to reset the FROM date if it happen to be 
        // greater than the TO date.
        // ------------------------------------------------------------------------
        if (Lt(export.SearchTo.Date, export.SearchFrom.Date))
        {
          local.Month.TotalInteger = Month(export.SearchTo.Date);
          local.Year.TotalInteger = Year(export.SearchTo.Date);

          if (local.Month.TotalInteger == 1)
          {
            local.Month.TotalInteger = 12;
            --local.Year.TotalInteger;
          }
          else
          {
            --local.Month.TotalInteger;
          }

          export.SearchFrom.Date =
            IntToDate((int)((decimal)local.Year.TotalInteger * 10000 + local
            .Month.TotalInteger * 100 + 1));
        }

        if (Lt(export.SearchTo.Date, export.SearchFrom.Date))
        {
          ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

          return;
        }

        // =================================================
        // 3/3/00 - b adams  -  PR# 82600: Explicit scrolling support.
        // =================================================
        if (AsChar(local.KeyChange.Flag) == 'N')
        {
          return;
        }

        if (Equal(local.NextPageSearchTo.Date, local.Zero.Date))
        {
          local.NextPageSearchTo.Date = export.SearchTo.Date;
        }

        // ===============================================
        // 5/12/00 - bud adams  -  PR# 82600: When retrieving debt
        //   adjustments they come most recent for each obligation.
        // ===============================================
        if (Equal(local.NextPageObligationTransactionRln.CreatedTmst,
          local.Zero.Timestamp))
        {
          local.NextPageObligationTransactionRln.CreatedTmst =
            local.Max.Timestamp;
        }

        // =================================================
        // 3/24/00 - b adams  -  PR# 81829: Performance enhancement.
        //   Eliminated overhead from this CAB for when a specific
        //   Obligation was not selected by making a copy and deleting
        //   selection, sort, etc., logic having to do with Obligation and
        //   Obligation_Type.
        // =================================================
        if (export.Obligation.SystemGeneratedIdentifier == 0)
        {
          UseFnReadDebtActivityForApPyr();
        }
        else
        {
          UseFnReadOneDebtActvtyForPyr();
        }

        // =================================================
        // 3/3/00 - b adams  -  PR# 82600: Explicit scrolling.
        // =================================================
        export.FullPagesInGroupView.Count = export.Xxx.Count / 10;

        if (export.Xxx.Count - (long)export.FullPagesInGroupView.Count * 10 > 0)
        {
          // ***--- Last page in group view is 'P'artially filled.
          export.FullPagesInGroupView.Flag = "P";
        }
        else
        {
          // ***--- Last page in group view is 'F'ull.
          export.FullPagesInGroupView.Flag = "F";
        }

        if (Equal(local.PfkeyPressed.Text4, "NEXT"))
        {
          if (export.GroupViewRetrieved.Count < local
            .PreviousGroupsToSave.Count)
          {
            ++export.GroupViewRetrieved.Count;
            export.PageNumberOnScreen.Count = 0;

            // ***---  Save these values for when the user presses the
            // ***---  PREV pfkey.
            export.PrevStartingValue.Index = export.GroupViewRetrieved.Count - 1
              ;
            export.PrevStartingValue.CheckSize();

            export.PrevStartingValue.Update.DtlPreviousCollection.
              SystemGeneratedIdentifier =
                local.NextPageCollection.SystemGeneratedIdentifier;
            export.PrevStartingValue.Update.DtlPreviousObligation.
              SystemGeneratedIdentifier =
                local.NextPageObligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.NextPageObligationTransaction,
              export.PrevStartingValue.Update.DtlPreviousObligationTransaction);
              
            export.PrevStartingValue.Update.DtlPreviousObligationType.Code =
              local.NextPageObligationType.Code;
            export.PrevStartingValue.Update.DtlPreviousAdjusted.CreatedTmst =
              local.NextPageObligationTransactionRln.CreatedTmst;
            export.PrevStartingValue.Update.DtlPreviousDebtDetail.DueDt =
              local.NextPageSearchTo.Date;
            export.PrevStartingValue.Update.DtlPreviousDispLinInd.Flag =
              local.NextPageDisplayLineInd.Flag;
          }

          // ***---  If we're at the 13th group retrieved backwards scrolling
          // ***---  will only be able to go back to the 12th group no matter
          // ***---  how many groups we scroll forward.
          if (export.GroupViewRetrieved.Count == local
            .PreviousGroupsToSave.Count)
          {
            export.PageNumberOnScreen.Count = 0;
          }
        }
        else if (Equal(local.PfkeyPressed.Text4, "PREV"))
        {
          if (export.GroupViewRetrieved.Count < local
            .PreviousGroupsToSave.Count + 1)
          {
            --export.GroupViewRetrieved.Count;

            // ***---  if the xxx group view has 61 elements, this value is 5
            export.PageNumberOnScreen.Count = (local.LinesToFill.Count - 1) / 10
              - 1;
          }
        }
        else
        {
          export.GroupViewRetrieved.Count = 1;
          export.PageNumberOnScreen.Count = 0;

          // ***---  Save these values for when the user presses the
          // ***---  PREV pfkey.
          export.PrevStartingValue.Index = export.GroupViewRetrieved.Count - 1;
          export.PrevStartingValue.CheckSize();

          export.PrevStartingValue.Update.DtlPreviousCollection.
            SystemGeneratedIdentifier =
              local.NextPageCollection.SystemGeneratedIdentifier;
          export.PrevStartingValue.Update.DtlPreviousObligation.
            SystemGeneratedIdentifier =
              local.NextPageObligation.SystemGeneratedIdentifier;
          MoveObligationTransaction(local.NextPageObligationTransaction,
            export.PrevStartingValue.Update.DtlPreviousObligationTransaction);
          export.PrevStartingValue.Update.DtlPreviousObligationType.Code =
            local.NextPageObligationType.Code;
          export.PrevStartingValue.Update.DtlPreviousAdjusted.CreatedTmst =
            local.NextPageObligationTransactionRln.CreatedTmst;
          export.PrevStartingValue.Update.DtlPreviousDebtDetail.DueDt =
            local.NextPageSearchTo.Date;
          export.PrevStartingValue.Update.DtlPreviousDispLinInd.Flag =
            local.NextPageDisplayLineInd.Flag;
        }

        export.HidListDebtsWithAmtOwed.SelectChar =
          export.ListDebtsWithAmtOwed.SelectChar;
        MoveCsePerson(export.SearchCsePerson, export.HidSearchCsePerson);
        export.HidSearchLegalAction.StandardNumber =
          export.SearchLegalAction.StandardNumber;
        export.HidSearchFrom.Date = export.SearchFrom.Date;
        export.HidSearchTo.Date = export.SearchTo.Date;
        export.HidSearchShowCollAdj.Text1 = export.SearchShowCollAdj.Text1;
        export.HidSearchShowDebtAdj.Text1 = export.SearchShowDebtAdj.Text1;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Xxx.IsFull)
          {
          }
          else if (export.Xxx.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }

        break;
      case "LIST":
        // =================================================================================
        //    I am not sure why the  " eab_cursor_position "  is used here. The 
        // screen is not working correctly when PF4 (LIST) is pressed with
        // values in the prompt fields. It is not displaying the proper error
        // messages. I commented out some code below and at the end of this  "
        // CASE list "  and added new code.
        //                                            
        // Vithal Madhira (09/20/00)
        // =================================================================================
        // =================================================================================
        //                       Start of new code..........
        //                                            
        // Vithal Madhira (09/20/00)
        // =================================================================================
        local.CountPromptsSelected.Count = 0;

        if (!IsEmpty(export.ApPayorPrompt.Text1))
        {
          ++local.CountPromptsSelected.Count;
        }

        if (!IsEmpty(export.CourtOrderPrompt.Text1))
        {
          ++local.CountPromptsSelected.Count;
        }

        if (!IsEmpty(export.PromptForCruc.Text1))
        {
          ++local.CountPromptsSelected.Count;
        }

        if (local.CountPromptsSelected.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          if (!IsEmpty(export.ApPayorPrompt.Text1))
          {
            var field = GetField(export.ApPayorPrompt, "text1");

            field.Error = true;
          }

          if (!IsEmpty(export.CourtOrderPrompt.Text1))
          {
            var field = GetField(export.CourtOrderPrompt, "text1");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptForCruc.Text1))
          {
            var field = GetField(export.PromptForCruc, "text1");

            field.Error = true;
          }
        }
        else if (local.CountPromptsSelected.Count == 0)
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          var field1 = GetField(export.ApPayorPrompt, "text1");

          field1.Error = true;

          var field2 = GetField(export.CourtOrderPrompt, "text1");

          field2.Error = true;

          var field3 = GetField(export.PromptForCruc, "text1");

          field3.Error = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ==========================================
          //        Continue processing..........
          // ==========================================
        }
        else
        {
          // ==========================================
          //       Escape this CASE........
          // ==========================================
          break;
        }

        // =================================================================================
        //                End  of new code..........
        //                                            
        // Vithal Madhira (09/20/00)
        // =================================================================================
        switch(AsChar(export.ApPayorPrompt.Text1))
        {
          case ' ':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          default:
            var field = GetField(export.ApPayorPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.CourtOrderPrompt.Text1))
        {
          case ' ':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_LST_LGL_ACT_BY_CSE_P";

            return;
          default:
            var field = GetField(export.CourtOrderPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.PromptForCruc.Text1))
        {
          case ' ':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";

            return;
          default:
            var field = GetField(export.PromptForCruc, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "PREV":
        if (export.PageNumberOnScreen.Count == 1)
        {
          if (export.FullPagesInGroupView.Count == 0)
          {
          }
          else
          {
            export.ScrollIndicator.Text3 = "+";
          }

          ExitState = "ACO_NI0000_TOP_OF_LIST";

          break;
        }

        export.PageNumberOnScreen.Count -= 2;

        break;
      case "NEXT":
        // =================================================
        // Page number in group view is number of 'F'ull pages.  If
        //   there are more, but not a full page, the flag value will be
        //   'P'artial.
        // =================================================
        if (export.PageNumberOnScreen.Count == export
          .FullPagesInGroupView.Count && AsChar
          (export.FullPagesInGroupView.Flag) == 'F' || export
          .PageNumberOnScreen.Count == export.FullPagesInGroupView.Count + 1)
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "CRRC":
        // **** Pf15 entered
        if (local.Counter.Count == 1)
        {
          for(export.X.Index = 0; export.X.Index < export.X.Count; ++
            export.X.Index)
          {
            if (!export.X.CheckSize())
            {
              break;
            }

            if (AsChar(export.X.Item.DtlXCommon.SelectChar) == 'S')
            {
              export.Xxx.Index = (int)((export.PageNumberOnScreen.Count - 1) * (
                long)10 + export.X.Index + 1 - 1);
              export.Xxx.CheckSize();

              if (Equal(export.X.Item.DtlXListScreenWorkArea.TextLine76, 1, 2,
                "CO"))
              {
                // ---------
                // This is good!
                // ---------
                export.DlgflwSelectedCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    export.Xxx.Item.DtlHiddenCashReceiptSourceType.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    export.Xxx.Item.DtlHiddenCashReceiptEvent.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceiptType.SystemGeneratedIdentifier =
                  export.Xxx.Item.DtlHiddenCashReceiptType.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceipt.SequentialNumber =
                  export.Xxx.Item.DtlHiddenCashReceipt.SequentialNumber;
                export.DlgflwSelectedCashReceiptDetail.SequentialIdentifier =
                  export.Xxx.Item.DtlHiddenCashReceiptDetail.
                    SequentialIdentifier;
                ExitState = "ECO_XFR_TO_RECORD_COLLECTION";
              }
              else
              {
                var field = GetField(export.X.Item.DtlXCommon, "selectChar");

                field.Error = true;

                local.Counter.Flag = "E";
                ExitState = "FN0000_DEBT_COL_TO_BE_SEL_F_CRRC";
              }

              goto Test3;
            }
          }

          export.X.CheckIndex();
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "COLA":
        // **** Pf19 entered
        if (local.Counter.Count == 1)
        {
          export.CsePersonsWorkSet.Number = export.SearchCsePerson.Number;

          for(export.X.Index = 0; export.X.Index < export.X.Count; ++
            export.X.Index)
          {
            if (!export.X.CheckSize())
            {
              break;
            }

            if (AsChar(export.X.Item.DtlXCommon.SelectChar) == 'S')
            {
              export.Xxx.Index = (int)((export.PageNumberOnScreen.Count - 1) * (
                long)10 + export.X.Index + 1 - 1);
              export.Xxx.CheckSize();

              if (Equal(export.X.Item.DtlXListScreenWorkArea.TextLine76, 1, 2,
                "CO"))
              {
                // ---------
                // This is good!
                // ---------
                export.DlgflwSelectedCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    export.Xxx.Item.DtlHiddenCashReceiptSourceType.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    export.Xxx.Item.DtlHiddenCashReceiptEvent.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceiptType.SystemGeneratedIdentifier =
                  export.Xxx.Item.DtlHiddenCashReceiptType.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceipt.SequentialNumber =
                  export.Xxx.Item.DtlHiddenCashReceipt.SequentialNumber;
                export.DlgflwSelectedCashReceiptDetail.SequentialIdentifier =
                  export.Xxx.Item.DtlHiddenCashReceiptDetail.
                    SequentialIdentifier;
                export.DlgflwSelectedCollection.SystemGeneratedIdentifier =
                  export.Xxx.Item.DtlHiddenCollection.SystemGeneratedIdentifier;
                  
                export.FlowToCola.Flag = "Y";
                ExitState = "ECO_LNK_TO_MTN_COLL_ADJMNTS";
              }
              else
              {
                var field = GetField(export.X.Item.DtlXCommon, "selectChar");

                field.Error = true;

                local.Counter.Flag = "E";
                ExitState = "FN0000_DEBT_TO_COLA_INVALID_SEL";
              }

              goto Test3;
            }
          }

          export.X.CheckIndex();
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "OBLGM":
        // **** Pf16 entered
        if (local.Counter.Count == 1)
        {
          for(export.X.Index = 0; export.X.Index < export.X.Count; ++
            export.X.Index)
          {
            if (!export.X.CheckSize())
            {
              break;
            }

            if (AsChar(export.X.Item.DtlXCommon.SelectChar) == 'S')
            {
              export.Xxx.Index = (int)((export.PageNumberOnScreen.Count - 1) * (
                long)10 + export.X.Index + 1 - 1);
              export.Xxx.CheckSize();

              if (Equal(export.X.Item.DtlHiddenXObligationTransaction.Type1,
                "DE"))
              {
                switch(AsChar(export.X.Item.DtlHiddenXObligationType.
                  Classification))
                {
                  case 'A':
                    ExitState = "ECO_LNK_TO_MTN_ACCRUING_OBLIG";

                    break;
                  case 'R':
                    ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";

                    break;
                  case 'M':
                    ExitState = "ECO_LNK_TO_MTN_NON_ACCRUING_OBLG";

                    break;
                  case 'N':
                    ExitState = "ECO_LNK_TO_MTN_NON_ACCRUING_OBLG";

                    break;
                  case 'F':
                    ExitState = "ECO_LNK_TO_OFEE";

                    break;
                  case 'V':
                    ExitState = "ECO_LNK_TO_MTN_VOLUNTARY_OBLIG";

                    break;
                  default:
                    ExitState = "FN0000_OBLIG_TYPE_CLASS_INVALID";

                    break;
                }
              }
              else
              {
                var field = GetField(export.X.Item.DtlXCommon, "selectChar");

                field.Error = true;

                local.Counter.Flag = "E";
                ExitState = "FN0000_DEBT_DEBT_TO_BE_SEL";
              }

              goto Test3;
            }
          }

          export.X.CheckIndex();
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "DBAJ":
        // **** Pf17 entered
        if (local.Counter.Count == 1)
        {
          for(export.X.Index = 0; export.X.Index < export.X.Count; ++
            export.X.Index)
          {
            if (!export.X.CheckSize())
            {
              break;
            }

            if (AsChar(export.X.Item.DtlXCommon.SelectChar) == 'S')
            {
              export.Xxx.Index = (int)((export.PageNumberOnScreen.Count - 1) * (
                long)10 + export.X.Index + 1 - 1);
              export.Xxx.CheckSize();

              switch(TrimEnd(export.Xxx.Item.DtlHiddenObligationTransaction.
                Type1))
              {
                case "DA":
                  ExitState = "ECO_LNK_TO_REC_IND_DEBT_ADJ";

                  break;
                case "DE":
                  if (AsChar(export.Xxx.Item.DtlHiddenObligationType.
                    Classification) == 'V')
                  {
                    ExitState = "FN0000_CANNOT_ADJ_VOL_DEBT_DTL";

                    goto Test3;
                  }
                  else
                  {
                  }

                  ExitState = "ECO_LNK_TO_REC_IND_DEBT_ADJ";

                  break;
                default:
                  var field = GetField(export.X.Item.DtlXCommon, "selectChar");

                  field.Error = true;

                  local.Counter.Flag = "E";
                  ExitState = "FN0000_DEBT_R_DEBT_ADJ_TO_BE_SEL";

                  break;
              }

              goto Test3;
            }
          }

          export.X.CheckIndex();
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "PACC":
        if (local.Counter.Count != 1)
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        export.X.Index = 0;

        for(var limit = export.X.Count; export.X.Index < limit; ++
          export.X.Index)
        {
          if (!export.X.CheckSize())
          {
            break;
          }

          if (AsChar(export.X.Item.DtlXCommon.SelectChar) == 'S')
          {
            if (Equal(export.X.Item.DtlXListScreenWorkArea.TextLine76, 1, 2,
              "CO"))
            {
              // ---------
              // This is good!
              // ---------
            }
            else
            {
              var field = GetField(export.X.Item.DtlXCommon, "selectChar");

              field.Error = true;

              local.Counter.Flag = "E";
              ExitState = "FN0000_MUST_SEL_COLL_REC_4_PACC";

              goto Test3;
            }

            if (AsChar(export.Xxx.Item.DtlHiddenObligationType.
              SupportedPersonReqInd) == 'N')
            {
              var field = GetField(export.X.Item.DtlXCommon, "selectChar");

              field.Error = true;

              local.Counter.Flag = "E";
              ExitState = "FN0000_KS_IS_PAYEE_NO_PACC_FLO";

              goto Test3;
            }

            export.Xxx.Index = (int)((export.PageNumberOnScreen.Count - 1) * (
              long)10 + export.X.Index + 1 - 1);
            export.Xxx.CheckSize();

            // PR202233 05/25/07 M. Fan Added codes to qualify following read.
            if (!ReadCollection())
            {
              ExitState = "FN0000_COLLECTION_NF";

              goto Test3;
            }

            if (AsChar(entities.Collection.AdjustedInd) == 'Y')
            {
              if (ReadCollectionAdjustmentReason())
              {
                // : Collection Adjustment reason is 'Wrong Account'  -
                //   set an exitstate and don't flow.
                var field = GetField(export.X.Item.DtlXCommon, "selectChar");

                field.Error = true;

                local.Counter.Flag = "E";
                ExitState = "FN0000_COLL_APPL_WRONG_ACCT";

                goto Test3;
              }
              else
              {
                // OK
              }
            }

            // ****************************************************************
            // 11/19/99 K. Doshi PR#80753
            // Amend data passed via dialog flow to PACC. Set End date ro
            // collection_date of selected record and start date to first date
            // of the previous month.
            // ***************************************************************
            export.PaccEndDate.Date = entities.Collection.CollectionDt;
            export.PaccStartDate.Date =
              AddMonths(entities.Collection.CollectionDt, -1);
            UseCabFirstAndLastDateOfMonth();

            // **********************************************************************
            // November, 2001, M. Ashworth, PR# 127419 - PAYR to COLL to DEBT to
            // PACC was displaying wrong obligee.
            // Changed read to qualify on current collection instead of "SOME"
            // collection.
            // ***********************************************************************
            if (ReadDisbursementTransactionCsePerson())
            {
              local.Found.Flag = "Y";

              if (Equal(entities.Obligee2.Number, local.KansasObligee.Number))
              {
                var field = GetField(export.X.Item.DtlXCommon, "selectChar");

                field.Error = true;

                local.Counter.Flag = "E";
                ExitState = "FN0000_KS_IS_PAYEE_NO_PACC_FLO";

                goto Test3;
              }

              export.FlowToPacc.Number = entities.Obligee2.Number;
            }

            if (AsChar(local.Found.Flag) != 'Y')
            {
              var field = GetField(export.X.Item.DtlXCommon, "selectChar");

              field.Error = true;

              local.Counter.Flag = "E";
              ExitState = "FN0000_NO_DISB_TRAN_FOR_COLL";

              goto Test3;
            }

            ExitState = "ECO_LNK_TO_LST_APACC";

            goto Test3;
          }
        }

        export.X.CheckIndex();

        break;
      case "LCDA":
        if (local.Counter.Count == 1)
        {
          export.CsePersonsWorkSet.Number = export.SearchCsePerson.Number;

          for(export.X.Index = 0; export.X.Index < export.X.Count; ++
            export.X.Index)
          {
            if (!export.X.CheckSize())
            {
              break;
            }

            if (AsChar(export.X.Item.DtlXCommon.SelectChar) == 'S')
            {
              export.Xxx.Index = (int)((export.PageNumberOnScreen.Count - 1) * (
                long)10 + export.X.Index + 1 - 1);
              export.Xxx.CheckSize();

              if (Equal(export.X.Item.DtlXListScreenWorkArea.TextLine76, 1, 2,
                "CO"))
              {
                // ---------
                // This is good!
                // ---------
                export.DlgflwSelectedCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    export.Xxx.Item.DtlHiddenCashReceiptSourceType.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    export.Xxx.Item.DtlHiddenCashReceiptEvent.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceiptType.SystemGeneratedIdentifier =
                  export.Xxx.Item.DtlHiddenCashReceiptType.
                    SystemGeneratedIdentifier;
                export.DlgflwSelectedCashReceipt.SequentialNumber =
                  export.Xxx.Item.DtlHiddenCashReceipt.SequentialNumber;
                export.DlgflwSelectedCashReceiptDetail.SequentialIdentifier =
                  export.Xxx.Item.DtlHiddenCashReceiptDetail.
                    SequentialIdentifier;
                export.DlgflwSelectedCollection.SystemGeneratedIdentifier =
                  export.Xxx.Item.DtlHiddenCollection.SystemGeneratedIdentifier;
                  

                goto Test2;
              }
              else
              {
                var field = GetField(export.X.Item.DtlXCommon, "selectChar");

                field.Error = true;

                local.Counter.Flag = "E";
                ExitState = "FN0000_DEBT_TO_LCDA_INVALID_SEL";

                goto Test3;
              }
            }
          }

          export.X.CheckIndex();
        }

Test2:

        ExitState = "ECO_LNK_TO_LCDA";

        break;
      case "PRINT":
        // **** PF 21 entered  ****
        // January, 2001, M. Brown, Work Order# 228 - Changed print processing (
        // pf21)
        // to flow to POPT.
        // Removed edits for legal action standard number, and printer id.
        // Removed call to cab that submitted print job, since now POPT does 
        // that.
        if (IsEmpty(export.SearchCsePerson.Number))
        {
          var field = GetField(export.SearchCsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        local.FormattedFromDate.Text15 =
          NumberToString(DateToInt(export.SearchFrom.Date), 15);
        local.FormattedFromDate.Text10 =
          Substring(local.FormattedFromDate.Text15, WorkArea.Text15_MaxLength,
          8, 4) + "-" + Substring
          (local.FormattedFromDate.Text15, WorkArea.Text15_MaxLength, 12, 2) + "-"
          + Substring
          (local.FormattedFromDate.Text15, WorkArea.Text15_MaxLength, 14, 2);
        local.FormattedToDate.Text15 =
          NumberToString(DateToInt(export.SearchTo.Date), 15);
        local.FormattedToDate.Text10 =
          Substring(local.FormattedToDate.Text15, WorkArea.Text15_MaxLength, 8,
          4) + "-" + Substring
          (local.FormattedToDate.Text15, WorkArea.Text15_MaxLength, 12, 2) + "-"
          + Substring
          (local.FormattedToDate.Text15, WorkArea.Text15_MaxLength, 14, 2);
        export.ToPoptJob.TranId = global.TranCode;
        export.ToPoptJobRun.ParmInfo = export.SearchCsePerson.Number + " " + (
          export.SearchLegalAction.StandardNumber ?? "") + " " + NumberToString
          (export.Obligation.SystemGeneratedIdentifier, 13, 3) + " " + NumberToString
          (export.ObligationType.SystemGeneratedIdentifier, 13, 3) + " " + local
          .FormattedFromDate.Text10 + " " + local.FormattedToDate.Text10 + " " +
          export.SearchShowDebtAdj.Text1 + " " + export
          .SearchShowCollAdj.Text1 + " " + export
          .ListDebtsWithAmtOwed.SelectChar + " " + export
          .FutureCollection.Flag;
        ExitState = "CO_LINK_TO_POPT";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test3:

    // =================================================
    // 3/3/00 - b adams  -  PR# 82600: Added for explicit scrolling.
    // =================================================
    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_DISPLAY_SUCCESSFUL"))
    {
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
        (global.Command, "PREV"))
      {
        export.X.Index = -1;

        if (export.PageNumberOnScreen.Count < export
          .FullPagesInGroupView.Count || export.PageNumberOnScreen.Count == export
          .FullPagesInGroupView.Count && AsChar
          (export.FullPagesInGroupView.Flag) == 'F')
        {
          // =================================================
          // This means that either there's at least another page of data
          // in the group view (meaning we have a screenful to display
          // in this page), or this is the last page - and it's full.
          // =================================================
          export.Xxx.Index = (int)((long)export.PageNumberOnScreen.Count * 10);

          for(var limit =
            (int)((export.PageNumberOnScreen.Count + 1) * (long)10); export
            .Xxx.Index < limit; ++export.Xxx.Index)
          {
            if (!export.Xxx.CheckSize())
            {
              break;
            }

            // =================================================
            // If this is the first member of the Group View, keep the key
            // values so if the user goes past six pages and repopulates
            // the group view, we have starting values saved for if they
            // want to PREV back to this data.  >>>BUT<<< there is only
            // enough room to save 13 groups of these keys.
            // =================================================
            ++export.X.Index;
            export.X.CheckSize();

            export.X.Update.DtlHiddenXObligationType.Classification =
              export.Xxx.Item.DtlHiddenObligationType.Classification;
            export.X.Update.DtlHiddenXObligationTransaction.Type1 =
              export.Xxx.Item.DtlHiddenObligationTransaction.Type1;
            export.X.Update.DtlXCommon.SelectChar =
              export.Xxx.Item.DtlHiddenCommon.SelectChar;
            export.X.Update.DtlXListScreenWorkArea.TextLine76 =
              export.Xxx.Item.DtlHiddenListScreenWorkArea.TextLine76;
            export.X.Update.DtlHiddenXCsePersonsWorkSet.Number =
              export.Xxx.Item.DtlHiddenCsePersonsWorkSet.Number;
            MoveLegalAction1(export.Xxx.Item.DtlHiddenLegalAction,
              export.X.Update.DtlHiddenXLegalAction);
            export.X.Update.DtlHiddnDisplayLineIndX.Flag =
              export.Xxx.Item.DtlHiddnDispLineInd.Flag;
          }

          export.Xxx.CheckIndex();
        }
        else
        {
          // =================================================
          // We don't want the value of SUBSCRIPT of xxx_group_export
          // to be incremented beyond its actual value.
          // =================================================
          if (export.FullPagesInGroupView.Count == 0 && AsChar
            (export.FullPagesInGroupView.Flag) == 'F')
          {
            // ***---  This will be the case when only a parital page and no 
            // full pages are returned.
            export.ScrollIndicator.Text3 = "";
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            goto Test4;
          }
          else
          {
            // =================================================
            // So this is the last page - and it's not Full.
            // =================================================
            for(export.Xxx.Index =
              (int)((long)export.PageNumberOnScreen.Count * 10); export
              .Xxx.Index < export.Xxx.Count; ++export.Xxx.Index)
            {
              if (!export.Xxx.CheckSize())
              {
                break;
              }

              ++export.X.Index;
              export.X.CheckSize();

              export.X.Update.DtlHiddenXObligationType.Classification =
                export.Xxx.Item.DtlHiddenObligationType.Classification;
              export.X.Update.DtlHiddenXObligationTransaction.Type1 =
                export.Xxx.Item.DtlHiddenObligationTransaction.Type1;
              export.X.Update.DtlXCommon.SelectChar =
                export.Xxx.Item.DtlHiddenCommon.SelectChar;
              export.X.Update.DtlXListScreenWorkArea.TextLine76 =
                export.Xxx.Item.DtlHiddenListScreenWorkArea.TextLine76;
              export.X.Update.DtlHiddenXCsePersonsWorkSet.Number =
                export.Xxx.Item.DtlHiddenCsePersonsWorkSet.Number;
              MoveLegalAction1(export.Xxx.Item.DtlHiddenLegalAction,
                export.X.Update.DtlHiddenXLegalAction);
              export.X.Update.DtlHiddnDisplayLineIndX.Flag =
                export.Xxx.Item.DtlHiddnDispLineInd.Flag;
            }

            export.Xxx.CheckIndex();
          }

          // =================================================
          // Make sure the rest of the screen is blanked out.
          // =================================================
          for(++export.X.Index; export.X.Index < 10; ++export.X.Index)
          {
            if (!export.X.CheckSize())
            {
              break;
            }

            export.X.Update.DtlHiddenXObligationType.Classification =
              local.Blank.Item.DtlBlankObligationType.Classification;
            export.X.Update.DtlHiddenXObligationTransaction.Type1 =
              local.Blank.Item.DtlBlankObligationTransaction.Type1;
            export.X.Update.DtlXCommon.SelectChar =
              local.Blank.Item.DtlBlankCommon.SelectChar;
            export.X.Update.DtlXListScreenWorkArea.TextLine76 =
              local.Blank.Item.DtlBlankListScreenWorkArea.TextLine76;
            export.X.Update.DtlHiddenXCsePersonsWorkSet.Number =
              local.Blank.Item.DtlBlankCsePersonsWorkSet.Number;
            MoveLegalAction1(local.Blank.Item.DtlBlankLegalAction,
              export.X.Update.DtlHiddenXLegalAction);
            export.X.Update.DtlHiddnDisplayLineIndX.Flag =
              local.Blank.Item.DtlBlankInd.Flag;
          }

          export.X.CheckIndex();
        }

        ++export.PageNumberOnScreen.Count;
      }

      if (export.PageNumberOnScreen.Count <= export.FullPagesInGroupView.Count)
      {
        if (export.PageNumberOnScreen.Count == 1 && export
          .GroupViewRetrieved.Count == 1)
        {
          export.ScrollIndicator.Text3 = "+";

          if (export.GroupViewRetrieved.Count == 1)
          {
            ExitState = "ACO_NI0000_TOP_OF_LIST";
          }
        }
        else
        {
          export.ScrollIndicator.Text3 = "+/-";
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      if (export.PageNumberOnScreen.Count == export
        .FullPagesInGroupView.Count && export.Xxx.Index + 1 != local
        .LinesToFill.Count && AsChar(export.FullPagesInGroupView.Flag) == 'F'
        || export.PageNumberOnScreen.Count == export
        .FullPagesInGroupView.Count + 1)
      {
        if (export.PageNumberOnScreen.Count == 1)
        {
          export.ScrollIndicator.Text3 = "";
        }
        else
        {
          export.ScrollIndicator.Text3 = "  -";
        }

        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
      }
      else if (export.FullPagesInGroupView.Count == 0)
      {
        export.ScrollIndicator.Text3 = "";
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else
      {
      }
    }

Test4:

    // =================================================
    // 3/3/00 - bud adams  -  PR# 82600: Moved from DISPLAY to
    //   support explicit scrolling.
    // =================================================
    for(export.X.Index = 0; export.X.Index < Export.XGroup.Capacity; ++
      export.X.Index)
    {
      if (!export.X.CheckSize())
      {
        break;
      }

      // ***---  This shouldn't be necessary, but after changing over
      // ***---  to explicit scrolling, these things were sometimes not
      // ***---  being unprotected on the PREV function.
      if (AsChar(local.Counter.Flag) == 'E')
      {
        // ***---  this is being highlighted as an Error.
      }
      else
      {
        var field = GetField(export.X.Item.DtlXCommon, "selectChar");

        field.Color = "green";
        field.Intensity = Intensity.Normal;
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
      }

      if (IsEmpty(export.X.Item.DtlXListScreenWorkArea.TextLine76))
      {
        var field1 = GetField(export.X.Item.DtlXCommon, "selectChar");

        field1.Intensity = Intensity.Dark;
        field1.Protected = true;

        var field2 =
          GetField(export.X.Item.DtlXListScreenWorkArea, "textLine76");

        field2.Intensity = Intensity.Dark;
        field2.Protected = true;
      }

      switch(TrimEnd(Substring(
        export.X.Item.DtlXListScreenWorkArea.TextLine76, 1, 2)))
      {
        case "DE":
          break;
        case "DA":
          var field1 =
            GetField(export.X.Item.DtlXListScreenWorkArea, "textLine76");

          field1.Color = "cyan";
          field1.Intensity = Intensity.High;
          field1.Protected = true;

          break;
        case "CO":
          var field2 =
            GetField(export.X.Item.DtlXListScreenWorkArea, "textLine76");

          field2.Color = "cyan";
          field2.Intensity = Intensity.High;
          field2.Protected = true;

          break;
        case "CA":
          var field3 =
            GetField(export.X.Item.DtlXListScreenWorkArea, "textLine76");

          field3.Color = "cyan";
          field3.Intensity = Intensity.High;
          field3.Protected = true;

          break;
        default:
          break;
      }

      if (AsChar(export.X.Item.DtlHiddnDisplayLineIndX.Flag) == 'N')
      {
        var field = GetField(export.X.Item.DtlXCommon, "selectChar");

        field.Intensity = Intensity.Dark;
        field.Protected = true;
      }
    }

    export.X.CheckIndex();

    if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
    {
      var field = GetField(export.ScreenOwedAmounts, "errorInformationLine");

      field.Color = "red";
      field.Intensity = Intensity.High;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.Flag = source.Flag;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToXxx1(FnReadDebtActivityForApPyr.Export.
    ExportGroup source, Export.XxxGroup target)
  {
    target.DtlHiddenDebtDetail.DueDt = source.DtlDebtDetail.DueDt;
    target.DtlHiddenCollection.SystemGeneratedIdentifier =
      source.DtlCollection.SystemGeneratedIdentifier;
    target.DtlHiddenCommon.SelectChar = source.DtlCommon.SelectChar;
    target.DtlHiddnDispLineInd.Flag = source.DtlDisplayLineInd.Flag;
    target.DtlHiddenListScreenWorkArea.TextLine76 =
      source.DtlListScreenWorkArea.TextLine76;
    target.DtlHiddenCsePersonsWorkSet.Number = source.DtlSupported.Number;
    MoveObligation(source.DtlObligation, target.DtlHiddenObligation);
    target.DtlHiddenObligationType.Assign(source.DtlObligationType);
    MoveObligationTransaction(source.DtlObligationTransaction,
      target.DtlHiddenObligationTransaction);
    target.DtlHiddenAdjusted.CreatedTmst = source.DtlAdjusted.CreatedTmst;
    target.DtlHiddenCashReceiptSourceType.SystemGeneratedIdentifier =
      source.DtlCashReceiptSourceType.SystemGeneratedIdentifier;
    target.DtlHiddenCashReceiptEvent.SystemGeneratedIdentifier =
      source.DtlCashReceiptEvent.SystemGeneratedIdentifier;
    target.DtlHiddenCashReceiptType.SystemGeneratedIdentifier =
      source.DtlCashReceiptType.SystemGeneratedIdentifier;
    target.DtlHiddenCashReceipt.SequentialNumber =
      source.DtlCashReceipt.SequentialNumber;
    target.DtlHiddenCashReceiptDetail.SequentialIdentifier =
      source.DtlCashReceiptDetail.SequentialIdentifier;
    MoveLegalAction1(source.DtlLegalAction, target.DtlHiddenLegalAction);
  }

  private static void MoveExport1ToXxx2(FnReadOneDebtActvtyForPyr.Export.
    ExportGroup source, Export.XxxGroup target)
  {
    target.DtlHiddenDebtDetail.DueDt = source.DtlDebtDetail.DueDt;
    target.DtlHiddenCollection.SystemGeneratedIdentifier =
      source.DtlCollection.SystemGeneratedIdentifier;
    target.DtlHiddenCommon.SelectChar = source.DtlCommon.SelectChar;
    target.DtlHiddnDispLineInd.Flag = source.DtlDisplayLineInd.Flag;
    target.DtlHiddenListScreenWorkArea.TextLine76 =
      source.DtlListScreenWorkArea.TextLine76;
    target.DtlHiddenCsePersonsWorkSet.Number = source.DtlSupported.Number;
    MoveObligation(source.DtlObligation, target.DtlHiddenObligation);
    target.DtlHiddenObligationType.Assign(source.DtlObligationType);
    MoveObligationTransaction(source.DtlObligationTransaction,
      target.DtlHiddenObligationTransaction);
    target.DtlHiddenAdjusted.CreatedTmst = source.DtlAdjusted.CreatedTmst;
    target.DtlHiddenCashReceiptSourceType.SystemGeneratedIdentifier =
      source.DtlCashReceiptSourceType.SystemGeneratedIdentifier;
    target.DtlHiddenCashReceiptEvent.SystemGeneratedIdentifier =
      source.DtlCashReceiptEvent.SystemGeneratedIdentifier;
    target.DtlHiddenCashReceiptType.SystemGeneratedIdentifier =
      source.DtlCashReceiptType.SystemGeneratedIdentifier;
    target.DtlHiddenCashReceipt.SequentialNumber =
      source.DtlCashReceipt.SequentialNumber;
    target.DtlHiddenCashReceiptDetail.SequentialIdentifier =
      source.DtlCashReceiptDetail.SequentialIdentifier;
    MoveLegalAction1(source.DtlLegalAction, target.DtlHiddenLegalAction);
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
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

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = export.PaccStartDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.PaccStartDate.Date = useExport.First.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.PadCsePersonNum.Text10;
    useExport.TextWorkArea.Text10 = local.PadCsePersonNum.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.PadCsePersonNum.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnReadDebtActivityForApPyr()
  {
    var useImport = new FnReadDebtActivityForApPyr.Import();
    var useExport = new FnReadDebtActivityForApPyr.Export();

    useImport.ListDebtsWithAmtOwed.SelectChar =
      export.ListDebtsWithAmtOwed.SelectChar;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.FromColl.SystemGeneratedIdentifier;
    useImport.SearchYear.TotalInteger = local.Year.TotalInteger;
    useImport.SearchMonth.TotalInteger = local.Month.TotalInteger;
    MoveCsePerson(export.SearchCsePerson, useImport.SearchCsePerson);
    useImport.SearchLegalAction.StandardNumber =
      export.SearchLegalAction.StandardNumber;
    useImport.SearchFrom.Date = export.SearchFrom.Date;
    useImport.SearchTo.Date = export.SearchTo.Date;
    useImport.SearchShowDebtAdj.Text1 = export.SearchShowDebtAdj.Text1;
    useImport.SearchShowCollAdj.Text1 = export.SearchShowCollAdj.Text1;
    useImport.NextPageCollection.SystemGeneratedIdentifier =
      local.NextPageCollection.SystemGeneratedIdentifier;
    useImport.NextPageObligationTransactionRln.CreatedTmst =
      local.NextPageObligationTransactionRln.CreatedTmst;
    MoveObligationTransaction(local.NextPageObligationTransaction,
      useImport.NextPageObligationTransaction);
    MoveObligationType(local.NextPageObligationType,
      useImport.NextPageObligationType);
    useImport.NextPageObligation.SystemGeneratedIdentifier =
      local.NextPageObligation.SystemGeneratedIdentifier;
    useImport.NextPageSearchTo.Date = local.NextPageSearchTo.Date;
    useImport.Max.Timestamp = local.Max.Timestamp;
    useImport.Current.Date = local.Current.Date;
    useImport.NextPageDisplayLinInd.Flag = local.NextPageDisplayLineInd.Flag;

    Call(FnReadDebtActivityForApPyr.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.UndistributedAmount.TotalCurrency =
      useExport.UndistributedAmt.TotalCurrency;
    useExport.Export1.CopyTo(export.Xxx, MoveExport1ToXxx1);
  }

  private void UseFnReadOneDebtActvtyForPyr()
  {
    var useImport = new FnReadOneDebtActvtyForPyr.Import();
    var useExport = new FnReadOneDebtActvtyForPyr.Export();

    useImport.Max.Timestamp = local.Max.Timestamp;
    useImport.NextPageCollection.SystemGeneratedIdentifier =
      local.NextPageCollection.SystemGeneratedIdentifier;
    useImport.NextPageObligationTransactionRln.CreatedTmst =
      local.NextPageObligationTransactionRln.CreatedTmst;
    useImport.NextPageObligationTransaction.SystemGeneratedIdentifier =
      local.NextPageObligationTransaction.SystemGeneratedIdentifier;
    useImport.NextPageSearchTo.Date = local.NextPageSearchTo.Date;
    useImport.SearchYear.TotalInteger = local.Year.TotalInteger;
    useImport.SearchMonth.TotalInteger = local.Month.TotalInteger;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.FromColl.SystemGeneratedIdentifier;
    useImport.ListDebtsWithAmtOwed.SelectChar =
      export.ListDebtsWithAmtOwed.SelectChar;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    MoveCsePerson(export.SearchCsePerson, useImport.SearchCsePerson);
    useImport.SearchLegalAction.StandardNumber =
      export.SearchLegalAction.StandardNumber;
    useImport.SearchFrom.Date = export.SearchFrom.Date;
    useImport.SearchTo.Date = export.SearchTo.Date;
    useImport.SearchShowCollAdj.Text1 = export.SearchShowCollAdj.Text1;
    useImport.Current.Date = local.Current.Date;
    useImport.SearchShowDebtAdj.Text1 = export.SearchShowDebtAdj.Text1;

    Call(FnReadOneDebtActvtyForPyr.Execute, useImport, useExport);

    export.ObligationType.Code = useExport.ObligationType.Code;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.UndistributedAmount.TotalCurrency =
      useExport.UndistributedAmt.TotalCurrency;
    useExport.Export1.CopyTo(export.Xxx, MoveExport1ToXxx2);
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

    useImport.NextTranInfo.Assign(export.Hidden);
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

    useImport.CsePerson.Number = export.SearchCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveLegalAction2(export.SearchLegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId",
          export.Xxx.Item.DtlHiddenCollection.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crdId",
          export.Xxx.Item.DtlHiddenCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "otrId",
          export.Xxx.Item.DtlHiddenObligationTransaction.
            SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgId",
          export.Xxx.Item.DtlHiddenObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId",
          export.Xxx.Item.DtlHiddenObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.CarId = db.GetNullableInt32(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId1",
          entities.Collection.CarId.GetValueOrDefault());
        db.SetInt32(
          command, "obTrnRlnRsnId2", local.WrongAcct.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailCollection1()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailCollection1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId", local.Temp.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.SearchCsePerson.Number);
        db.SetNullableString(
          command, "standardNo", export.SearchLegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CollectionDt = db.GetDate(reader, 8);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 9);
        entities.Collection.CrtType = db.GetInt32(reader, 10);
        entities.Collection.CstId = db.GetInt32(reader, 11);
        entities.Collection.CrvId = db.GetInt32(reader, 12);
        entities.Collection.CrdId = db.GetInt32(reader, 13);
        entities.Collection.ObgId = db.GetInt32(reader, 14);
        entities.Collection.CspNumber = db.GetString(reader, 15);
        entities.Collection.CpaType = db.GetString(reader, 16);
        entities.Collection.OtrId = db.GetInt32(reader, 17);
        entities.Collection.OtrType = db.GetString(reader, 18);
        entities.Collection.CarId = db.GetNullableInt32(reader, 19);
        entities.Collection.OtyId = db.GetInt32(reader, 20);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailCollection2()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId", local.Temp.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.SearchCsePerson.Number);
        db.SetNullableString(
          command, "standardNo", export.SearchLegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CollectionDt = db.GetDate(reader, 8);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 9);
        entities.Collection.CrtType = db.GetInt32(reader, 10);
        entities.Collection.CstId = db.GetInt32(reader, 11);
        entities.Collection.CrvId = db.GetInt32(reader, 12);
        entities.Collection.CrdId = db.GetInt32(reader, 13);
        entities.Collection.ObgId = db.GetInt32(reader, 14);
        entities.Collection.CspNumber = db.GetString(reader, 15);
        entities.Collection.CpaType = db.GetString(reader, 16);
        entities.Collection.OtrId = db.GetInt32(reader, 17);
        entities.Collection.OtrType = db.GetString(reader, 18);
        entities.Collection.CarId = db.GetNullableInt32(reader, 19);
        entities.Collection.OtyId = db.GetInt32(reader, 20);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailCollection3()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailCollection3",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId", local.Temp.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CollectionDt = db.GetDate(reader, 8);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 9);
        entities.Collection.CrtType = db.GetInt32(reader, 10);
        entities.Collection.CstId = db.GetInt32(reader, 11);
        entities.Collection.CrvId = db.GetInt32(reader, 12);
        entities.Collection.CrdId = db.GetInt32(reader, 13);
        entities.Collection.ObgId = db.GetInt32(reader, 14);
        entities.Collection.CspNumber = db.GetString(reader, 15);
        entities.Collection.CpaType = db.GetString(reader, 16);
        entities.Collection.OtrId = db.GetInt32(reader, 17);
        entities.Collection.OtrType = db.GetString(reader, 18);
        entities.Collection.CarId = db.GetNullableInt32(reader, 19);
        entities.Collection.OtyId = db.GetInt32(reader, 20);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailCollection4()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailCollection4",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId", local.Temp.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CollectionDt = db.GetDate(reader, 8);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 9);
        entities.Collection.CrtType = db.GetInt32(reader, 10);
        entities.Collection.CstId = db.GetInt32(reader, 11);
        entities.Collection.CrvId = db.GetInt32(reader, 12);
        entities.Collection.CrdId = db.GetInt32(reader, 13);
        entities.Collection.ObgId = db.GetInt32(reader, 14);
        entities.Collection.CspNumber = db.GetString(reader, 15);
        entities.Collection.CpaType = db.GetString(reader, 16);
        entities.Collection.OtrId = db.GetInt32(reader, 17);
        entities.Collection.OtrType = db.GetString(reader, 18);
        entities.Collection.CarId = db.GetNullableInt32(reader, 19);
        entities.Collection.OtyId = db.GetInt32(reader, 20);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadDisbursementTransactionCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.DisbursementTransaction.Populated = false;
    entities.Obligee2.Populated = false;

    return Read("ReadDisbursementTransactionCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", entities.Collection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", entities.Collection.OtyId);
        db.SetNullableInt32(command, "obgId", entities.Collection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", entities.Collection.CspNumber);
        db.
          SetNullableString(command, "cpaTypeDisb", entities.Collection.CpaType);
          
        db.SetNullableInt32(command, "otrId", entities.Collection.OtrId);
        db.
          SetNullableString(command, "otrTypeDisb", entities.Collection.OtrType);
          
        db.SetNullableInt32(command, "crtId", entities.Collection.CrtType);
        db.SetNullableInt32(command, "cstId", entities.Collection.CstId);
        db.SetNullableInt32(command, "crvId", entities.Collection.CrvId);
        db.SetNullableInt32(command, "crdId", entities.Collection.CrdId);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligee2.Number = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.OtyId = db.GetNullableInt32(reader, 4);
        entities.DisbursementTransaction.OtrTypeDisb =
          db.GetNullableString(reader, 5);
        entities.DisbursementTransaction.OtrId = db.GetNullableInt32(reader, 6);
        entities.DisbursementTransaction.CpaTypeDisb =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransaction.CspNumberDisb =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransaction.ObgId = db.GetNullableInt32(reader, 9);
        entities.DisbursementTransaction.CrdId =
          db.GetNullableInt32(reader, 10);
        entities.DisbursementTransaction.CrvId =
          db.GetNullableInt32(reader, 11);
        entities.DisbursementTransaction.CstId =
          db.GetNullableInt32(reader, 12);
        entities.DisbursementTransaction.CrtId =
          db.GetNullableInt32(reader, 13);
        entities.DisbursementTransaction.ColId =
          db.GetNullableInt32(reader, 14);
        entities.DisbursementTransaction.Populated = true;
        entities.Obligee2.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbursementTransaction.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbursementTransaction.CpaTypeDisb);
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", import.SearchLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
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
    /// <summary>A XGroup group.</summary>
    [Serializable]
    public class XGroup
    {
      /// <summary>
      /// A value of DtlXCommon.
      /// </summary>
      [JsonPropertyName("dtlXCommon")]
      public Common DtlXCommon
      {
        get => dtlXCommon ??= new();
        set => dtlXCommon = value;
      }

      /// <summary>
      /// A value of DtlHiddnDisplayLineIndX.
      /// </summary>
      [JsonPropertyName("dtlHiddnDisplayLineIndX")]
      public Common DtlHiddnDisplayLineIndX
      {
        get => dtlHiddnDisplayLineIndX ??= new();
        set => dtlHiddnDisplayLineIndX = value;
      }

      /// <summary>
      /// A value of DtlXListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("dtlXListScreenWorkArea")]
      public ListScreenWorkArea DtlXListScreenWorkArea
      {
        get => dtlXListScreenWorkArea ??= new();
        set => dtlXListScreenWorkArea = value;
      }

      /// <summary>
      /// A value of DtlHiddenXCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlHiddenXCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlHiddenXCsePersonsWorkSet
      {
        get => dtlHiddenXCsePersonsWorkSet ??= new();
        set => dtlHiddenXCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlHiddenXLegalAction.
      /// </summary>
      [JsonPropertyName("dtlHiddenXLegalAction")]
      public LegalAction DtlHiddenXLegalAction
      {
        get => dtlHiddenXLegalAction ??= new();
        set => dtlHiddenXLegalAction = value;
      }

      /// <summary>
      /// A value of DtlHiddenXObligationType.
      /// </summary>
      [JsonPropertyName("dtlHiddenXObligationType")]
      public ObligationType DtlHiddenXObligationType
      {
        get => dtlHiddenXObligationType ??= new();
        set => dtlHiddenXObligationType = value;
      }

      /// <summary>
      /// A value of DtlHiddenXObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlHiddenXObligationTransaction")]
      public ObligationTransaction DtlHiddenXObligationTransaction
      {
        get => dtlHiddenXObligationTransaction ??= new();
        set => dtlHiddenXObligationTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common dtlXCommon;
      private Common dtlHiddnDisplayLineIndX;
      private ListScreenWorkArea dtlXListScreenWorkArea;
      private CsePersonsWorkSet dtlHiddenXCsePersonsWorkSet;
      private LegalAction dtlHiddenXLegalAction;
      private ObligationType dtlHiddenXObligationType;
      private ObligationTransaction dtlHiddenXObligationTransaction;
    }

    /// <summary>A XxxGroup group.</summary>
    [Serializable]
    public class XxxGroup
    {
      /// <summary>
      /// A value of DtlHiddenDebtDetail.
      /// </summary>
      [JsonPropertyName("dtlHiddenDebtDetail")]
      public DebtDetail DtlHiddenDebtDetail
      {
        get => dtlHiddenDebtDetail ??= new();
        set => dtlHiddenDebtDetail = value;
      }

      /// <summary>
      /// A value of DtlHiddenCollection.
      /// </summary>
      [JsonPropertyName("dtlHiddenCollection")]
      public Collection DtlHiddenCollection
      {
        get => dtlHiddenCollection ??= new();
        set => dtlHiddenCollection = value;
      }

      /// <summary>
      /// A value of DtlHiddenCommon.
      /// </summary>
      [JsonPropertyName("dtlHiddenCommon")]
      public Common DtlHiddenCommon
      {
        get => dtlHiddenCommon ??= new();
        set => dtlHiddenCommon = value;
      }

      /// <summary>
      /// A value of DtlHiddenDispLinInd.
      /// </summary>
      [JsonPropertyName("dtlHiddenDispLinInd")]
      public Common DtlHiddenDispLinInd
      {
        get => dtlHiddenDispLinInd ??= new();
        set => dtlHiddenDispLinInd = value;
      }

      /// <summary>
      /// A value of DtlHiddenListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("dtlHiddenListScreenWorkArea")]
      public ListScreenWorkArea DtlHiddenListScreenWorkArea
      {
        get => dtlHiddenListScreenWorkArea ??= new();
        set => dtlHiddenListScreenWorkArea = value;
      }

      /// <summary>
      /// A value of DtlHiddenCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlHiddenCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlHiddenCsePersonsWorkSet
      {
        get => dtlHiddenCsePersonsWorkSet ??= new();
        set => dtlHiddenCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlHiddenObligation.
      /// </summary>
      [JsonPropertyName("dtlHiddenObligation")]
      public Obligation DtlHiddenObligation
      {
        get => dtlHiddenObligation ??= new();
        set => dtlHiddenObligation = value;
      }

      /// <summary>
      /// A value of DtlHiddenObligationType.
      /// </summary>
      [JsonPropertyName("dtlHiddenObligationType")]
      public ObligationType DtlHiddenObligationType
      {
        get => dtlHiddenObligationType ??= new();
        set => dtlHiddenObligationType = value;
      }

      /// <summary>
      /// A value of DtlHiddenObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlHiddenObligationTransaction")]
      public ObligationTransaction DtlHiddenObligationTransaction
      {
        get => dtlHiddenObligationTransaction ??= new();
        set => dtlHiddenObligationTransaction = value;
      }

      /// <summary>
      /// A value of DtlHiddenAdjusted.
      /// </summary>
      [JsonPropertyName("dtlHiddenAdjusted")]
      public ObligationTransactionRln DtlHiddenAdjusted
      {
        get => dtlHiddenAdjusted ??= new();
        set => dtlHiddenAdjusted = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptSourceType")]
      public CashReceiptSourceType DtlHiddenCashReceiptSourceType
      {
        get => dtlHiddenCashReceiptSourceType ??= new();
        set => dtlHiddenCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptEvent")]
      public CashReceiptEvent DtlHiddenCashReceiptEvent
      {
        get => dtlHiddenCashReceiptEvent ??= new();
        set => dtlHiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptType")]
      public CashReceiptType DtlHiddenCashReceiptType
      {
        get => dtlHiddenCashReceiptType ??= new();
        set => dtlHiddenCashReceiptType = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceipt.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceipt")]
      public CashReceipt DtlHiddenCashReceipt
      {
        get => dtlHiddenCashReceipt ??= new();
        set => dtlHiddenCashReceipt = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptDetail")]
      public CashReceiptDetail DtlHiddenCashReceiptDetail
      {
        get => dtlHiddenCashReceiptDetail ??= new();
        set => dtlHiddenCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of DtlHiddenLegalAction.
      /// </summary>
      [JsonPropertyName("dtlHiddenLegalAction")]
      public LegalAction DtlHiddenLegalAction
      {
        get => dtlHiddenLegalAction ??= new();
        set => dtlHiddenLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 71;

      private DebtDetail dtlHiddenDebtDetail;
      private Collection dtlHiddenCollection;
      private Common dtlHiddenCommon;
      private Common dtlHiddenDispLinInd;
      private ListScreenWorkArea dtlHiddenListScreenWorkArea;
      private CsePersonsWorkSet dtlHiddenCsePersonsWorkSet;
      private Obligation dtlHiddenObligation;
      private ObligationType dtlHiddenObligationType;
      private ObligationTransaction dtlHiddenObligationTransaction;
      private ObligationTransactionRln dtlHiddenAdjusted;
      private CashReceiptSourceType dtlHiddenCashReceiptSourceType;
      private CashReceiptEvent dtlHiddenCashReceiptEvent;
      private CashReceiptType dtlHiddenCashReceiptType;
      private CashReceipt dtlHiddenCashReceipt;
      private CashReceiptDetail dtlHiddenCashReceiptDetail;
      private LegalAction dtlHiddenLegalAction;
    }

    /// <summary>A PrevStartingValueGroup group.</summary>
    [Serializable]
    public class PrevStartingValueGroup
    {
      /// <summary>
      /// A value of DtlPreviousObligationType.
      /// </summary>
      [JsonPropertyName("dtlPreviousObligationType")]
      public ObligationType DtlPreviousObligationType
      {
        get => dtlPreviousObligationType ??= new();
        set => dtlPreviousObligationType = value;
      }

      /// <summary>
      /// A value of DtlPreviousCollection.
      /// </summary>
      [JsonPropertyName("dtlPreviousCollection")]
      public Collection DtlPreviousCollection
      {
        get => dtlPreviousCollection ??= new();
        set => dtlPreviousCollection = value;
      }

      /// <summary>
      /// A value of DtlPreviousObligation.
      /// </summary>
      [JsonPropertyName("dtlPreviousObligation")]
      public Obligation DtlPreviousObligation
      {
        get => dtlPreviousObligation ??= new();
        set => dtlPreviousObligation = value;
      }

      /// <summary>
      /// A value of DtlPreviousDebtDetail.
      /// </summary>
      [JsonPropertyName("dtlPreviousDebtDetail")]
      public DebtDetail DtlPreviousDebtDetail
      {
        get => dtlPreviousDebtDetail ??= new();
        set => dtlPreviousDebtDetail = value;
      }

      /// <summary>
      /// A value of DtlPreviousAdjusted.
      /// </summary>
      [JsonPropertyName("dtlPreviousAdjusted")]
      public ObligationTransactionRln DtlPreviousAdjusted
      {
        get => dtlPreviousAdjusted ??= new();
        set => dtlPreviousAdjusted = value;
      }

      /// <summary>
      /// A value of DtlPreviousObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlPreviousObligationTransaction")]
      public ObligationTransaction DtlPreviousObligationTransaction
      {
        get => dtlPreviousObligationTransaction ??= new();
        set => dtlPreviousObligationTransaction = value;
      }

      /// <summary>
      /// A value of DtlPreviousDispLinInd.
      /// </summary>
      [JsonPropertyName("dtlPreviousDispLinInd")]
      public Common DtlPreviousDispLinInd
      {
        get => dtlPreviousDispLinInd ??= new();
        set => dtlPreviousDispLinInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private ObligationType dtlPreviousObligationType;
      private Collection dtlPreviousCollection;
      private Obligation dtlPreviousObligation;
      private DebtDetail dtlPreviousDebtDetail;
      private ObligationTransactionRln dtlPreviousAdjusted;
      private ObligationTransaction dtlPreviousObligationTransaction;
      private Common dtlPreviousDispLinInd;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of HidListDebtsWithAmtOwed.
    /// </summary>
    [JsonPropertyName("hidListDebtsWithAmtOwed")]
    public Common HidListDebtsWithAmtOwed
    {
      get => hidListDebtsWithAmtOwed ??= new();
      set => hidListDebtsWithAmtOwed = value;
    }

    /// <summary>
    /// A value of HidSearchCsePerson.
    /// </summary>
    [JsonPropertyName("hidSearchCsePerson")]
    public CsePerson HidSearchCsePerson
    {
      get => hidSearchCsePerson ??= new();
      set => hidSearchCsePerson = value;
    }

    /// <summary>
    /// A value of HidSearchLegalAction.
    /// </summary>
    [JsonPropertyName("hidSearchLegalAction")]
    public LegalAction HidSearchLegalAction
    {
      get => hidSearchLegalAction ??= new();
      set => hidSearchLegalAction = value;
    }

    /// <summary>
    /// A value of HidSearchFrom.
    /// </summary>
    [JsonPropertyName("hidSearchFrom")]
    public DateWorkArea HidSearchFrom
    {
      get => hidSearchFrom ??= new();
      set => hidSearchFrom = value;
    }

    /// <summary>
    /// A value of HidSearchTo.
    /// </summary>
    [JsonPropertyName("hidSearchTo")]
    public DateWorkArea HidSearchTo
    {
      get => hidSearchTo ??= new();
      set => hidSearchTo = value;
    }

    /// <summary>
    /// A value of HidSearchShowDebtAdj.
    /// </summary>
    [JsonPropertyName("hidSearchShowDebtAdj")]
    public TextWorkArea HidSearchShowDebtAdj
    {
      get => hidSearchShowDebtAdj ??= new();
      set => hidSearchShowDebtAdj = value;
    }

    /// <summary>
    /// A value of HidSearchShowCollAdj.
    /// </summary>
    [JsonPropertyName("hidSearchShowCollAdj")]
    public TextWorkArea HidSearchShowCollAdj
    {
      get => hidSearchShowCollAdj ??= new();
      set => hidSearchShowCollAdj = value;
    }

    /// <summary>
    /// A value of PageNumberOnScreen.
    /// </summary>
    [JsonPropertyName("pageNumberOnScreen")]
    public Common PageNumberOnScreen
    {
      get => pageNumberOnScreen ??= new();
      set => pageNumberOnScreen = value;
    }

    /// <summary>
    /// A value of PagesInGroupView.
    /// </summary>
    [JsonPropertyName("pagesInGroupView")]
    public Common PagesInGroupView
    {
      get => pagesInGroupView ??= new();
      set => pagesInGroupView = value;
    }

    /// <summary>
    /// A value of PrinterId.
    /// </summary>
    [JsonPropertyName("printerId")]
    public TextWorkArea PrinterId
    {
      get => printerId ??= new();
      set => printerId = value;
    }

    /// <summary>
    /// A value of LastDocSection.
    /// </summary>
    [JsonPropertyName("lastDocSection")]
    public Common LastDocSection
    {
      get => lastDocSection ??= new();
      set => lastDocSection = value;
    }

    /// <summary>
    /// A value of GroupViewRetrieved.
    /// </summary>
    [JsonPropertyName("groupViewRetrieved")]
    public Common GroupViewRetrieved
    {
      get => groupViewRetrieved ??= new();
      set => groupViewRetrieved = value;
    }

    /// <summary>
    /// A value of DocSectionIndicator.
    /// </summary>
    [JsonPropertyName("docSectionIndicator")]
    public Common DocSectionIndicator
    {
      get => docSectionIndicator ??= new();
      set => docSectionIndicator = value;
    }

    /// <summary>
    /// A value of FromColl.
    /// </summary>
    [JsonPropertyName("fromColl")]
    public ObligationTransaction FromColl
    {
      get => fromColl ??= new();
      set => fromColl = value;
    }

    /// <summary>
    /// A value of ListDebtsWithAmtOwed.
    /// </summary>
    [JsonPropertyName("listDebtsWithAmtOwed")]
    public Common ListDebtsWithAmtOwed
    {
      get => listDebtsWithAmtOwed ??= new();
      set => listDebtsWithAmtOwed = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
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
    /// A value of FromList.
    /// </summary>
    [JsonPropertyName("fromList")]
    public CsePersonsWorkSet FromList
    {
      get => fromList ??= new();
      set => fromList = value;
    }

    /// <summary>
    /// A value of SearchShowDebtAdj.
    /// </summary>
    [JsonPropertyName("searchShowDebtAdj")]
    public TextWorkArea SearchShowDebtAdj
    {
      get => searchShowDebtAdj ??= new();
      set => searchShowDebtAdj = value;
    }

    /// <summary>
    /// A value of SearchShowCollAdj.
    /// </summary>
    [JsonPropertyName("searchShowCollAdj")]
    public TextWorkArea SearchShowCollAdj
    {
      get => searchShowCollAdj ??= new();
      set => searchShowCollAdj = value;
    }

    /// <summary>
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public TextWorkArea CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
    }

    /// <summary>
    /// A value of ApPayorPrompt.
    /// </summary>
    [JsonPropertyName("apPayorPrompt")]
    public TextWorkArea ApPayorPrompt
    {
      get => apPayorPrompt ??= new();
      set => apPayorPrompt = value;
    }

    /// <summary>
    /// A value of PromptForCruc.
    /// </summary>
    [JsonPropertyName("promptForCruc")]
    public TextWorkArea PromptForCruc
    {
      get => promptForCruc ??= new();
      set => promptForCruc = value;
    }

    /// <summary>
    /// Gets a value of X.
    /// </summary>
    [JsonIgnore]
    public Array<XGroup> X => x ??= new(XGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of X for json serialization.
    /// </summary>
    [JsonPropertyName("x")]
    [Computed]
    public IList<XGroup> X_Json
    {
      get => x;
      set => X.Assign(value);
    }

    /// <summary>
    /// Gets a value of Xxx.
    /// </summary>
    [JsonIgnore]
    public Array<XxxGroup> Xxx => xxx ??= new(XxxGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Xxx for json serialization.
    /// </summary>
    [JsonPropertyName("xxx")]
    [Computed]
    public IList<XxxGroup> Xxx_Json
    {
      get => xxx;
      set => Xxx.Assign(value);
    }

    /// <summary>
    /// Gets a value of PrevStartingValue.
    /// </summary>
    [JsonIgnore]
    public Array<PrevStartingValueGroup> PrevStartingValue =>
      prevStartingValue ??= new(PrevStartingValueGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrevStartingValue for json serialization.
    /// </summary>
    [JsonPropertyName("prevStartingValue")]
    [Computed]
    public IList<PrevStartingValueGroup> PrevStartingValue_Json
    {
      get => prevStartingValue;
      set => PrevStartingValue.Assign(value);
    }

    /// <summary>
    /// A value of TempCurrentSearchTo.
    /// </summary>
    [JsonPropertyName("tempCurrentSearchTo")]
    public DateWorkArea TempCurrentSearchTo
    {
      get => tempCurrentSearchTo ??= new();
      set => tempCurrentSearchTo = value;
    }

    /// <summary>
    /// A value of FutureCollection.
    /// </summary>
    [JsonPropertyName("futureCollection")]
    public Common FutureCollection
    {
      get => futureCollection ??= new();
      set => futureCollection = value;
    }

    private WorkArea scrollIndicator;
    private Common hidListDebtsWithAmtOwed;
    private CsePerson hidSearchCsePerson;
    private LegalAction hidSearchLegalAction;
    private DateWorkArea hidSearchFrom;
    private DateWorkArea hidSearchTo;
    private TextWorkArea hidSearchShowDebtAdj;
    private TextWorkArea hidSearchShowCollAdj;
    private Common pageNumberOnScreen;
    private Common pagesInGroupView;
    private TextWorkArea printerId;
    private Common lastDocSection;
    private Common groupViewRetrieved;
    private Common docSectionIndicator;
    private ObligationTransaction fromColl;
    private Common listDebtsWithAmtOwed;
    private ObligationType obligationType;
    private Obligation obligation;
    private Common nextTransaction;
    private CsePerson searchCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction searchLegalAction;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistributedAmount;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePersonsWorkSet fromList;
    private TextWorkArea searchShowDebtAdj;
    private TextWorkArea searchShowCollAdj;
    private TextWorkArea courtOrderPrompt;
    private TextWorkArea apPayorPrompt;
    private TextWorkArea promptForCruc;
    private Array<XGroup> x;
    private Array<XxxGroup> xxx;
    private Array<PrevStartingValueGroup> prevStartingValue;
    private DateWorkArea tempCurrentSearchTo;
    private Common futureCollection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A XGroup group.</summary>
    [Serializable]
    public class XGroup
    {
      /// <summary>
      /// A value of DtlXCommon.
      /// </summary>
      [JsonPropertyName("dtlXCommon")]
      public Common DtlXCommon
      {
        get => dtlXCommon ??= new();
        set => dtlXCommon = value;
      }

      /// <summary>
      /// A value of DtlHiddnDisplayLineIndX.
      /// </summary>
      [JsonPropertyName("dtlHiddnDisplayLineIndX")]
      public Common DtlHiddnDisplayLineIndX
      {
        get => dtlHiddnDisplayLineIndX ??= new();
        set => dtlHiddnDisplayLineIndX = value;
      }

      /// <summary>
      /// A value of DtlXListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("dtlXListScreenWorkArea")]
      public ListScreenWorkArea DtlXListScreenWorkArea
      {
        get => dtlXListScreenWorkArea ??= new();
        set => dtlXListScreenWorkArea = value;
      }

      /// <summary>
      /// A value of DtlHiddenXCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlHiddenXCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlHiddenXCsePersonsWorkSet
      {
        get => dtlHiddenXCsePersonsWorkSet ??= new();
        set => dtlHiddenXCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlHiddenXLegalAction.
      /// </summary>
      [JsonPropertyName("dtlHiddenXLegalAction")]
      public LegalAction DtlHiddenXLegalAction
      {
        get => dtlHiddenXLegalAction ??= new();
        set => dtlHiddenXLegalAction = value;
      }

      /// <summary>
      /// A value of DtlHiddenXObligationType.
      /// </summary>
      [JsonPropertyName("dtlHiddenXObligationType")]
      public ObligationType DtlHiddenXObligationType
      {
        get => dtlHiddenXObligationType ??= new();
        set => dtlHiddenXObligationType = value;
      }

      /// <summary>
      /// A value of DtlHiddenXObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlHiddenXObligationTransaction")]
      public ObligationTransaction DtlHiddenXObligationTransaction
      {
        get => dtlHiddenXObligationTransaction ??= new();
        set => dtlHiddenXObligationTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common dtlXCommon;
      private Common dtlHiddnDisplayLineIndX;
      private ListScreenWorkArea dtlXListScreenWorkArea;
      private CsePersonsWorkSet dtlHiddenXCsePersonsWorkSet;
      private LegalAction dtlHiddenXLegalAction;
      private ObligationType dtlHiddenXObligationType;
      private ObligationTransaction dtlHiddenXObligationTransaction;
    }

    /// <summary>A XxxGroup group.</summary>
    [Serializable]
    public class XxxGroup
    {
      /// <summary>
      /// A value of DtlHiddenDebtDetail.
      /// </summary>
      [JsonPropertyName("dtlHiddenDebtDetail")]
      public DebtDetail DtlHiddenDebtDetail
      {
        get => dtlHiddenDebtDetail ??= new();
        set => dtlHiddenDebtDetail = value;
      }

      /// <summary>
      /// A value of DtlHiddenCollection.
      /// </summary>
      [JsonPropertyName("dtlHiddenCollection")]
      public Collection DtlHiddenCollection
      {
        get => dtlHiddenCollection ??= new();
        set => dtlHiddenCollection = value;
      }

      /// <summary>
      /// A value of DtlHiddenCommon.
      /// </summary>
      [JsonPropertyName("dtlHiddenCommon")]
      public Common DtlHiddenCommon
      {
        get => dtlHiddenCommon ??= new();
        set => dtlHiddenCommon = value;
      }

      /// <summary>
      /// A value of DtlHiddnDispLineInd.
      /// </summary>
      [JsonPropertyName("dtlHiddnDispLineInd")]
      public Common DtlHiddnDispLineInd
      {
        get => dtlHiddnDispLineInd ??= new();
        set => dtlHiddnDispLineInd = value;
      }

      /// <summary>
      /// A value of DtlHiddenListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("dtlHiddenListScreenWorkArea")]
      public ListScreenWorkArea DtlHiddenListScreenWorkArea
      {
        get => dtlHiddenListScreenWorkArea ??= new();
        set => dtlHiddenListScreenWorkArea = value;
      }

      /// <summary>
      /// A value of DtlHiddenCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlHiddenCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlHiddenCsePersonsWorkSet
      {
        get => dtlHiddenCsePersonsWorkSet ??= new();
        set => dtlHiddenCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlHiddenObligation.
      /// </summary>
      [JsonPropertyName("dtlHiddenObligation")]
      public Obligation DtlHiddenObligation
      {
        get => dtlHiddenObligation ??= new();
        set => dtlHiddenObligation = value;
      }

      /// <summary>
      /// A value of DtlHiddenObligationType.
      /// </summary>
      [JsonPropertyName("dtlHiddenObligationType")]
      public ObligationType DtlHiddenObligationType
      {
        get => dtlHiddenObligationType ??= new();
        set => dtlHiddenObligationType = value;
      }

      /// <summary>
      /// A value of DtlHiddenObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlHiddenObligationTransaction")]
      public ObligationTransaction DtlHiddenObligationTransaction
      {
        get => dtlHiddenObligationTransaction ??= new();
        set => dtlHiddenObligationTransaction = value;
      }

      /// <summary>
      /// A value of DtlHiddenAdjusted.
      /// </summary>
      [JsonPropertyName("dtlHiddenAdjusted")]
      public ObligationTransactionRln DtlHiddenAdjusted
      {
        get => dtlHiddenAdjusted ??= new();
        set => dtlHiddenAdjusted = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptSourceType")]
      public CashReceiptSourceType DtlHiddenCashReceiptSourceType
      {
        get => dtlHiddenCashReceiptSourceType ??= new();
        set => dtlHiddenCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptEvent")]
      public CashReceiptEvent DtlHiddenCashReceiptEvent
      {
        get => dtlHiddenCashReceiptEvent ??= new();
        set => dtlHiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptType")]
      public CashReceiptType DtlHiddenCashReceiptType
      {
        get => dtlHiddenCashReceiptType ??= new();
        set => dtlHiddenCashReceiptType = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceipt.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceipt")]
      public CashReceipt DtlHiddenCashReceipt
      {
        get => dtlHiddenCashReceipt ??= new();
        set => dtlHiddenCashReceipt = value;
      }

      /// <summary>
      /// A value of DtlHiddenCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("dtlHiddenCashReceiptDetail")]
      public CashReceiptDetail DtlHiddenCashReceiptDetail
      {
        get => dtlHiddenCashReceiptDetail ??= new();
        set => dtlHiddenCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of DtlHiddenLegalAction.
      /// </summary>
      [JsonPropertyName("dtlHiddenLegalAction")]
      public LegalAction DtlHiddenLegalAction
      {
        get => dtlHiddenLegalAction ??= new();
        set => dtlHiddenLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 71;

      private DebtDetail dtlHiddenDebtDetail;
      private Collection dtlHiddenCollection;
      private Common dtlHiddenCommon;
      private Common dtlHiddnDispLineInd;
      private ListScreenWorkArea dtlHiddenListScreenWorkArea;
      private CsePersonsWorkSet dtlHiddenCsePersonsWorkSet;
      private Obligation dtlHiddenObligation;
      private ObligationType dtlHiddenObligationType;
      private ObligationTransaction dtlHiddenObligationTransaction;
      private ObligationTransactionRln dtlHiddenAdjusted;
      private CashReceiptSourceType dtlHiddenCashReceiptSourceType;
      private CashReceiptEvent dtlHiddenCashReceiptEvent;
      private CashReceiptType dtlHiddenCashReceiptType;
      private CashReceipt dtlHiddenCashReceipt;
      private CashReceiptDetail dtlHiddenCashReceiptDetail;
      private LegalAction dtlHiddenLegalAction;
    }

    /// <summary>A PrevStartingValueGroup group.</summary>
    [Serializable]
    public class PrevStartingValueGroup
    {
      /// <summary>
      /// A value of DtlPreviousDebtDetail.
      /// </summary>
      [JsonPropertyName("dtlPreviousDebtDetail")]
      public DebtDetail DtlPreviousDebtDetail
      {
        get => dtlPreviousDebtDetail ??= new();
        set => dtlPreviousDebtDetail = value;
      }

      /// <summary>
      /// A value of DtlPreviousCollection.
      /// </summary>
      [JsonPropertyName("dtlPreviousCollection")]
      public Collection DtlPreviousCollection
      {
        get => dtlPreviousCollection ??= new();
        set => dtlPreviousCollection = value;
      }

      /// <summary>
      /// A value of DtlPreviousObligation.
      /// </summary>
      [JsonPropertyName("dtlPreviousObligation")]
      public Obligation DtlPreviousObligation
      {
        get => dtlPreviousObligation ??= new();
        set => dtlPreviousObligation = value;
      }

      /// <summary>
      /// A value of DtlPreviousObligationType.
      /// </summary>
      [JsonPropertyName("dtlPreviousObligationType")]
      public ObligationType DtlPreviousObligationType
      {
        get => dtlPreviousObligationType ??= new();
        set => dtlPreviousObligationType = value;
      }

      /// <summary>
      /// A value of DtlPreviousObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlPreviousObligationTransaction")]
      public ObligationTransaction DtlPreviousObligationTransaction
      {
        get => dtlPreviousObligationTransaction ??= new();
        set => dtlPreviousObligationTransaction = value;
      }

      /// <summary>
      /// A value of DtlPreviousAdjusted.
      /// </summary>
      [JsonPropertyName("dtlPreviousAdjusted")]
      public ObligationTransactionRln DtlPreviousAdjusted
      {
        get => dtlPreviousAdjusted ??= new();
        set => dtlPreviousAdjusted = value;
      }

      /// <summary>
      /// A value of DtlPreviousDispLinInd.
      /// </summary>
      [JsonPropertyName("dtlPreviousDispLinInd")]
      public Common DtlPreviousDispLinInd
      {
        get => dtlPreviousDispLinInd ??= new();
        set => dtlPreviousDispLinInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private DebtDetail dtlPreviousDebtDetail;
      private Collection dtlPreviousCollection;
      private Obligation dtlPreviousObligation;
      private ObligationType dtlPreviousObligationType;
      private ObligationTransaction dtlPreviousObligationTransaction;
      private ObligationTransactionRln dtlPreviousAdjusted;
      private Common dtlPreviousDispLinInd;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of HidListDebtsWithAmtOwed.
    /// </summary>
    [JsonPropertyName("hidListDebtsWithAmtOwed")]
    public Common HidListDebtsWithAmtOwed
    {
      get => hidListDebtsWithAmtOwed ??= new();
      set => hidListDebtsWithAmtOwed = value;
    }

    /// <summary>
    /// A value of HidSearchCsePerson.
    /// </summary>
    [JsonPropertyName("hidSearchCsePerson")]
    public CsePerson HidSearchCsePerson
    {
      get => hidSearchCsePerson ??= new();
      set => hidSearchCsePerson = value;
    }

    /// <summary>
    /// A value of HidSearchLegalAction.
    /// </summary>
    [JsonPropertyName("hidSearchLegalAction")]
    public LegalAction HidSearchLegalAction
    {
      get => hidSearchLegalAction ??= new();
      set => hidSearchLegalAction = value;
    }

    /// <summary>
    /// A value of HidSearchFrom.
    /// </summary>
    [JsonPropertyName("hidSearchFrom")]
    public DateWorkArea HidSearchFrom
    {
      get => hidSearchFrom ??= new();
      set => hidSearchFrom = value;
    }

    /// <summary>
    /// A value of HidSearchTo.
    /// </summary>
    [JsonPropertyName("hidSearchTo")]
    public DateWorkArea HidSearchTo
    {
      get => hidSearchTo ??= new();
      set => hidSearchTo = value;
    }

    /// <summary>
    /// A value of HidSearchShowDebtAdj.
    /// </summary>
    [JsonPropertyName("hidSearchShowDebtAdj")]
    public TextWorkArea HidSearchShowDebtAdj
    {
      get => hidSearchShowDebtAdj ??= new();
      set => hidSearchShowDebtAdj = value;
    }

    /// <summary>
    /// A value of HidSearchShowCollAdj.
    /// </summary>
    [JsonPropertyName("hidSearchShowCollAdj")]
    public TextWorkArea HidSearchShowCollAdj
    {
      get => hidSearchShowCollAdj ??= new();
      set => hidSearchShowCollAdj = value;
    }

    /// <summary>
    /// A value of PageNumberOnScreen.
    /// </summary>
    [JsonPropertyName("pageNumberOnScreen")]
    public Common PageNumberOnScreen
    {
      get => pageNumberOnScreen ??= new();
      set => pageNumberOnScreen = value;
    }

    /// <summary>
    /// A value of FullPagesInGroupView.
    /// </summary>
    [JsonPropertyName("fullPagesInGroupView")]
    public Common FullPagesInGroupView
    {
      get => fullPagesInGroupView ??= new();
      set => fullPagesInGroupView = value;
    }

    /// <summary>
    /// A value of PrinterId.
    /// </summary>
    [JsonPropertyName("printerId")]
    public TextWorkArea PrinterId
    {
      get => printerId ??= new();
      set => printerId = value;
    }

    /// <summary>
    /// A value of PaccStartDate.
    /// </summary>
    [JsonPropertyName("paccStartDate")]
    public DateWorkArea PaccStartDate
    {
      get => paccStartDate ??= new();
      set => paccStartDate = value;
    }

    /// <summary>
    /// A value of FlowToPacc.
    /// </summary>
    [JsonPropertyName("flowToPacc")]
    public CsePerson FlowToPacc
    {
      get => flowToPacc ??= new();
      set => flowToPacc = value;
    }

    /// <summary>
    /// A value of PaccEndDate.
    /// </summary>
    [JsonPropertyName("paccEndDate")]
    public DateWorkArea PaccEndDate
    {
      get => paccEndDate ??= new();
      set => paccEndDate = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCollection.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCollection")]
    public Collection DlgflwSelectedCollection
    {
      get => dlgflwSelectedCollection ??= new();
      set => dlgflwSelectedCollection = value;
    }

    /// <summary>
    /// A value of FlowToCola.
    /// </summary>
    [JsonPropertyName("flowToCola")]
    public Common FlowToCola
    {
      get => flowToCola ??= new();
      set => flowToCola = value;
    }

    /// <summary>
    /// A value of LastDocSection.
    /// </summary>
    [JsonPropertyName("lastDocSection")]
    public Common LastDocSection
    {
      get => lastDocSection ??= new();
      set => lastDocSection = value;
    }

    /// <summary>
    /// A value of GroupViewRetrieved.
    /// </summary>
    [JsonPropertyName("groupViewRetrieved")]
    public Common GroupViewRetrieved
    {
      get => groupViewRetrieved ??= new();
      set => groupViewRetrieved = value;
    }

    /// <summary>
    /// A value of DocSectionIndicator.
    /// </summary>
    [JsonPropertyName("docSectionIndicator")]
    public Common DocSectionIndicator
    {
      get => docSectionIndicator ??= new();
      set => docSectionIndicator = value;
    }

    /// <summary>
    /// A value of PassSupported.
    /// </summary>
    [JsonPropertyName("passSupported")]
    public CsePersonsWorkSet PassSupported
    {
      get => passSupported ??= new();
      set => passSupported = value;
    }

    /// <summary>
    /// A value of FromColl.
    /// </summary>
    [JsonPropertyName("fromColl")]
    public ObligationTransaction FromColl
    {
      get => fromColl ??= new();
      set => fromColl = value;
    }

    /// <summary>
    /// A value of ListDebtsWithAmtOwed.
    /// </summary>
    [JsonPropertyName("listDebtsWithAmtOwed")]
    public Common ListDebtsWithAmtOwed
    {
      get => listDebtsWithAmtOwed ??= new();
      set => listDebtsWithAmtOwed = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ToNextTransactionObligationType.
    /// </summary>
    [JsonPropertyName("toNextTransactionObligationType")]
    public ObligationType ToNextTransactionObligationType
    {
      get => toNextTransactionObligationType ??= new();
      set => toNextTransactionObligationType = value;
    }

    /// <summary>
    /// A value of ToNextTransactionLegalAction.
    /// </summary>
    [JsonPropertyName("toNextTransactionLegalAction")]
    public LegalAction ToNextTransactionLegalAction
    {
      get => toNextTransactionLegalAction ??= new();
      set => toNextTransactionLegalAction = value;
    }

    /// <summary>
    /// A value of ToNextTransactionObligationTransaction.
    /// </summary>
    [JsonPropertyName("toNextTransactionObligationTransaction")]
    public ObligationTransaction ToNextTransactionObligationTransaction
    {
      get => toNextTransactionObligationTransaction ??= new();
      set => toNextTransactionObligationTransaction = value;
    }

    /// <summary>
    /// A value of ToNextTransactionObligation.
    /// </summary>
    [JsonPropertyName("toNextTransactionObligation")]
    public Obligation ToNextTransactionObligation
    {
      get => toNextTransactionObligation ??= new();
      set => toNextTransactionObligation = value;
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
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
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
    /// A value of SearchShowDebtAdj.
    /// </summary>
    [JsonPropertyName("searchShowDebtAdj")]
    public TextWorkArea SearchShowDebtAdj
    {
      get => searchShowDebtAdj ??= new();
      set => searchShowDebtAdj = value;
    }

    /// <summary>
    /// A value of SearchShowCollAdj.
    /// </summary>
    [JsonPropertyName("searchShowCollAdj")]
    public TextWorkArea SearchShowCollAdj
    {
      get => searchShowCollAdj ??= new();
      set => searchShowCollAdj = value;
    }

    /// <summary>
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public TextWorkArea CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
    }

    /// <summary>
    /// A value of ApPayorPrompt.
    /// </summary>
    [JsonPropertyName("apPayorPrompt")]
    public TextWorkArea ApPayorPrompt
    {
      get => apPayorPrompt ??= new();
      set => apPayorPrompt = value;
    }

    /// <summary>
    /// A value of PromptForCruc.
    /// </summary>
    [JsonPropertyName("promptForCruc")]
    public TextWorkArea PromptForCruc
    {
      get => promptForCruc ??= new();
      set => promptForCruc = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCashReceiptSourceType")]
    public CashReceiptSourceType DlgflwSelectedCashReceiptSourceType
    {
      get => dlgflwSelectedCashReceiptSourceType ??= new();
      set => dlgflwSelectedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCashReceiptEvent")]
    public CashReceiptEvent DlgflwSelectedCashReceiptEvent
    {
      get => dlgflwSelectedCashReceiptEvent ??= new();
      set => dlgflwSelectedCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCashReceiptType.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCashReceiptType")]
    public CashReceiptType DlgflwSelectedCashReceiptType
    {
      get => dlgflwSelectedCashReceiptType ??= new();
      set => dlgflwSelectedCashReceiptType = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCashReceipt.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCashReceipt")]
    public CashReceipt DlgflwSelectedCashReceipt
    {
      get => dlgflwSelectedCashReceipt ??= new();
      set => dlgflwSelectedCashReceipt = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCashReceiptDetail")]
    public CashReceiptDetail DlgflwSelectedCashReceiptDetail
    {
      get => dlgflwSelectedCashReceiptDetail ??= new();
      set => dlgflwSelectedCashReceiptDetail = value;
    }

    /// <summary>
    /// Gets a value of X.
    /// </summary>
    [JsonIgnore]
    public Array<XGroup> X => x ??= new(XGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of X for json serialization.
    /// </summary>
    [JsonPropertyName("x")]
    [Computed]
    public IList<XGroup> X_Json
    {
      get => x;
      set => X.Assign(value);
    }

    /// <summary>
    /// Gets a value of Xxx.
    /// </summary>
    [JsonIgnore]
    public Array<XxxGroup> Xxx => xxx ??= new(XxxGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Xxx for json serialization.
    /// </summary>
    [JsonPropertyName("xxx")]
    [Computed]
    public IList<XxxGroup> Xxx_Json
    {
      get => xxx;
      set => Xxx.Assign(value);
    }

    /// <summary>
    /// Gets a value of PrevStartingValue.
    /// </summary>
    [JsonIgnore]
    public Array<PrevStartingValueGroup> PrevStartingValue =>
      prevStartingValue ??= new(PrevStartingValueGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrevStartingValue for json serialization.
    /// </summary>
    [JsonPropertyName("prevStartingValue")]
    [Computed]
    public IList<PrevStartingValueGroup> PrevStartingValue_Json
    {
      get => prevStartingValue;
      set => PrevStartingValue.Assign(value);
    }

    /// <summary>
    /// A value of TempXSubFirst.
    /// </summary>
    [JsonPropertyName("tempXSubFirst")]
    public Common TempXSubFirst
    {
      get => tempXSubFirst ??= new();
      set => tempXSubFirst = value;
    }

    /// <summary>
    /// A value of TempXSubLast.
    /// </summary>
    [JsonPropertyName("tempXSubLast")]
    public Common TempXSubLast
    {
      get => tempXSubLast ??= new();
      set => tempXSubLast = value;
    }

    /// <summary>
    /// A value of TempXxxSubFirst.
    /// </summary>
    [JsonPropertyName("tempXxxSubFirst")]
    public Common TempXxxSubFirst
    {
      get => tempXxxSubFirst ??= new();
      set => tempXxxSubFirst = value;
    }

    /// <summary>
    /// A value of TempXxxSubLast.
    /// </summary>
    [JsonPropertyName("tempXxxSubLast")]
    public Common TempXxxSubLast
    {
      get => tempXxxSubLast ??= new();
      set => tempXxxSubLast = value;
    }

    /// <summary>
    /// A value of TempXxxLastInGrp.
    /// </summary>
    [JsonPropertyName("tempXxxLastInGrp")]
    public Common TempXxxLastInGrp
    {
      get => tempXxxLastInGrp ??= new();
      set => tempXxxLastInGrp = value;
    }

    /// <summary>
    /// A value of TempFullPgsInGrpView.
    /// </summary>
    [JsonPropertyName("tempFullPgsInGrpView")]
    public Common TempFullPgsInGrpView
    {
      get => tempFullPgsInGrpView ??= new();
      set => tempFullPgsInGrpView = value;
    }

    /// <summary>
    /// A value of TempPgOnScreen.
    /// </summary>
    [JsonPropertyName("tempPgOnScreen")]
    public Common TempPgOnScreen
    {
      get => tempPgOnScreen ??= new();
      set => tempPgOnScreen = value;
    }

    /// <summary>
    /// A value of TempPreviousSubscript.
    /// </summary>
    [JsonPropertyName("tempPreviousSubscript")]
    public Common TempPreviousSubscript
    {
      get => tempPreviousSubscript ??= new();
      set => tempPreviousSubscript = value;
    }

    /// <summary>
    /// A value of TempMoveLocation.
    /// </summary>
    [JsonPropertyName("tempMoveLocation")]
    public WorkArea TempMoveLocation
    {
      get => tempMoveLocation ??= new();
      set => tempMoveLocation = value;
    }

    /// <summary>
    /// A value of Temp2DigitSubscript.
    /// </summary>
    [JsonPropertyName("temp2DigitSubscript")]
    public CursorPosition Temp2DigitSubscript
    {
      get => temp2DigitSubscript ??= new();
      set => temp2DigitSubscript = value;
    }

    /// <summary>
    /// A value of TempCurrentSearchTo.
    /// </summary>
    [JsonPropertyName("tempCurrentSearchTo")]
    public DateWorkArea TempCurrentSearchTo
    {
      get => tempCurrentSearchTo ??= new();
      set => tempCurrentSearchTo = value;
    }

    /// <summary>
    /// A value of TempXxxSubCurrent.
    /// </summary>
    [JsonPropertyName("tempXxxSubCurrent")]
    public Common TempXxxSubCurrent
    {
      get => tempXxxSubCurrent ??= new();
      set => tempXxxSubCurrent = value;
    }

    /// <summary>
    /// A value of TempFromXxxGroupView.
    /// </summary>
    [JsonPropertyName("tempFromXxxGroupView")]
    public Collection TempFromXxxGroupView
    {
      get => tempFromXxxGroupView ??= new();
      set => tempFromXxxGroupView = value;
    }

    /// <summary>
    /// A value of FutureCollection.
    /// </summary>
    [JsonPropertyName("futureCollection")]
    public Common FutureCollection
    {
      get => futureCollection ??= new();
      set => futureCollection = value;
    }

    /// <summary>
    /// A value of ToPoptJob.
    /// </summary>
    [JsonPropertyName("toPoptJob")]
    public Job ToPoptJob
    {
      get => toPoptJob ??= new();
      set => toPoptJob = value;
    }

    /// <summary>
    /// A value of ToPoptJobRun.
    /// </summary>
    [JsonPropertyName("toPoptJobRun")]
    public JobRun ToPoptJobRun
    {
      get => toPoptJobRun ??= new();
      set => toPoptJobRun = value;
    }

    private WorkArea scrollIndicator;
    private Common hidListDebtsWithAmtOwed;
    private CsePerson hidSearchCsePerson;
    private LegalAction hidSearchLegalAction;
    private DateWorkArea hidSearchFrom;
    private DateWorkArea hidSearchTo;
    private TextWorkArea hidSearchShowDebtAdj;
    private TextWorkArea hidSearchShowCollAdj;
    private Common pageNumberOnScreen;
    private Common fullPagesInGroupView;
    private TextWorkArea printerId;
    private DateWorkArea paccStartDate;
    private CsePerson flowToPacc;
    private DateWorkArea paccEndDate;
    private Collection dlgflwSelectedCollection;
    private Common flowToCola;
    private Common lastDocSection;
    private Common groupViewRetrieved;
    private Common docSectionIndicator;
    private CsePersonsWorkSet passSupported;
    private ObligationTransaction fromColl;
    private Common listDebtsWithAmtOwed;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationType toNextTransactionObligationType;
    private LegalAction toNextTransactionLegalAction;
    private ObligationTransaction toNextTransactionObligationTransaction;
    private Obligation toNextTransactionObligation;
    private Common nextTransaction;
    private CsePerson searchCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction searchLegalAction;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistributedAmount;
    private NextTranInfo hidden;
    private Standard standard;
    private TextWorkArea searchShowDebtAdj;
    private TextWorkArea searchShowCollAdj;
    private TextWorkArea courtOrderPrompt;
    private TextWorkArea apPayorPrompt;
    private TextWorkArea promptForCruc;
    private CashReceiptSourceType dlgflwSelectedCashReceiptSourceType;
    private CashReceiptEvent dlgflwSelectedCashReceiptEvent;
    private CashReceiptType dlgflwSelectedCashReceiptType;
    private CashReceipt dlgflwSelectedCashReceipt;
    private CashReceiptDetail dlgflwSelectedCashReceiptDetail;
    private Array<XGroup> x;
    private Array<XxxGroup> xxx;
    private Array<PrevStartingValueGroup> prevStartingValue;
    private Common tempXSubFirst;
    private Common tempXSubLast;
    private Common tempXxxSubFirst;
    private Common tempXxxSubLast;
    private Common tempXxxLastInGrp;
    private Common tempFullPgsInGrpView;
    private Common tempPgOnScreen;
    private Common tempPreviousSubscript;
    private WorkArea tempMoveLocation;
    private CursorPosition temp2DigitSubscript;
    private DateWorkArea tempCurrentSearchTo;
    private Common tempXxxSubCurrent;
    private Collection tempFromXxxGroupView;
    private Common futureCollection;
    private Job toPoptJob;
    private JobRun toPoptJobRun;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>A BlankGroup group.</summary>
    [Serializable]
    public class BlankGroup
    {
      /// <summary>
      /// A value of DtlBlankDebtDetail.
      /// </summary>
      [JsonPropertyName("dtlBlankDebtDetail")]
      public DebtDetail DtlBlankDebtDetail
      {
        get => dtlBlankDebtDetail ??= new();
        set => dtlBlankDebtDetail = value;
      }

      /// <summary>
      /// A value of DtlBlankCollection.
      /// </summary>
      [JsonPropertyName("dtlBlankCollection")]
      public Collection DtlBlankCollection
      {
        get => dtlBlankCollection ??= new();
        set => dtlBlankCollection = value;
      }

      /// <summary>
      /// A value of DtlBlankCommon.
      /// </summary>
      [JsonPropertyName("dtlBlankCommon")]
      public Common DtlBlankCommon
      {
        get => dtlBlankCommon ??= new();
        set => dtlBlankCommon = value;
      }

      /// <summary>
      /// A value of DtlBlankInd.
      /// </summary>
      [JsonPropertyName("dtlBlankInd")]
      public Common DtlBlankInd
      {
        get => dtlBlankInd ??= new();
        set => dtlBlankInd = value;
      }

      /// <summary>
      /// A value of DtlBlankListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("dtlBlankListScreenWorkArea")]
      public ListScreenWorkArea DtlBlankListScreenWorkArea
      {
        get => dtlBlankListScreenWorkArea ??= new();
        set => dtlBlankListScreenWorkArea = value;
      }

      /// <summary>
      /// A value of DtlBlankCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlBlankCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlBlankCsePersonsWorkSet
      {
        get => dtlBlankCsePersonsWorkSet ??= new();
        set => dtlBlankCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlBlankObligation.
      /// </summary>
      [JsonPropertyName("dtlBlankObligation")]
      public Obligation DtlBlankObligation
      {
        get => dtlBlankObligation ??= new();
        set => dtlBlankObligation = value;
      }

      /// <summary>
      /// A value of DtlBlankObligationType.
      /// </summary>
      [JsonPropertyName("dtlBlankObligationType")]
      public ObligationType DtlBlankObligationType
      {
        get => dtlBlankObligationType ??= new();
        set => dtlBlankObligationType = value;
      }

      /// <summary>
      /// A value of DtlBlankObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlBlankObligationTransaction")]
      public ObligationTransaction DtlBlankObligationTransaction
      {
        get => dtlBlankObligationTransaction ??= new();
        set => dtlBlankObligationTransaction = value;
      }

      /// <summary>
      /// A value of DtlBlankObligationTransactionRln.
      /// </summary>
      [JsonPropertyName("dtlBlankObligationTransactionRln")]
      public ObligationTransactionRln DtlBlankObligationTransactionRln
      {
        get => dtlBlankObligationTransactionRln ??= new();
        set => dtlBlankObligationTransactionRln = value;
      }

      /// <summary>
      /// A value of DtlBlankLegalAction.
      /// </summary>
      [JsonPropertyName("dtlBlankLegalAction")]
      public LegalAction DtlBlankLegalAction
      {
        get => dtlBlankLegalAction ??= new();
        set => dtlBlankLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private DebtDetail dtlBlankDebtDetail;
      private Collection dtlBlankCollection;
      private Common dtlBlankCommon;
      private Common dtlBlankInd;
      private ListScreenWorkArea dtlBlankListScreenWorkArea;
      private CsePersonsWorkSet dtlBlankCsePersonsWorkSet;
      private Obligation dtlBlankObligation;
      private ObligationType dtlBlankObligationType;
      private ObligationTransaction dtlBlankObligationTransaction;
      private ObligationTransactionRln dtlBlankObligationTransactionRln;
      private LegalAction dtlBlankLegalAction;
    }

    /// <summary>
    /// A value of PreviousGroupsToSave.
    /// </summary>
    [JsonPropertyName("previousGroupsToSave")]
    public Common PreviousGroupsToSave
    {
      get => previousGroupsToSave ??= new();
      set => previousGroupsToSave = value;
    }

    /// <summary>
    /// A value of PfkeyPressed.
    /// </summary>
    [JsonPropertyName("pfkeyPressed")]
    public WorkArea PfkeyPressed
    {
      get => pfkeyPressed ??= new();
      set => pfkeyPressed = value;
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
    /// A value of LinesToFill.
    /// </summary>
    [JsonPropertyName("linesToFill")]
    public Common LinesToFill
    {
      get => linesToFill ??= new();
      set => linesToFill = value;
    }

    /// <summary>
    /// A value of KeyChange.
    /// </summary>
    [JsonPropertyName("keyChange")]
    public Common KeyChange
    {
      get => keyChange ??= new();
      set => keyChange = value;
    }

    /// <summary>
    /// A value of NextPageCollection.
    /// </summary>
    [JsonPropertyName("nextPageCollection")]
    public Collection NextPageCollection
    {
      get => nextPageCollection ??= new();
      set => nextPageCollection = value;
    }

    /// <summary>
    /// A value of NextPageObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("nextPageObligationTransactionRln")]
    public ObligationTransactionRln NextPageObligationTransactionRln
    {
      get => nextPageObligationTransactionRln ??= new();
      set => nextPageObligationTransactionRln = value;
    }

    /// <summary>
    /// A value of NextPageObligationTransaction.
    /// </summary>
    [JsonPropertyName("nextPageObligationTransaction")]
    public ObligationTransaction NextPageObligationTransaction
    {
      get => nextPageObligationTransaction ??= new();
      set => nextPageObligationTransaction = value;
    }

    /// <summary>
    /// A value of NextPageObligationType.
    /// </summary>
    [JsonPropertyName("nextPageObligationType")]
    public ObligationType NextPageObligationType
    {
      get => nextPageObligationType ??= new();
      set => nextPageObligationType = value;
    }

    /// <summary>
    /// A value of NextPageObligation.
    /// </summary>
    [JsonPropertyName("nextPageObligation")]
    public Obligation NextPageObligation
    {
      get => nextPageObligation ??= new();
      set => nextPageObligation = value;
    }

    /// <summary>
    /// A value of NextPageSearchTo.
    /// </summary>
    [JsonPropertyName("nextPageSearchTo")]
    public DateWorkArea NextPageSearchTo
    {
      get => nextPageSearchTo ??= new();
      set => nextPageSearchTo = value;
    }

    /// <summary>
    /// A value of NextPageDisplayLineInd.
    /// </summary>
    [JsonPropertyName("nextPageDisplayLineInd")]
    public Common NextPageDisplayLineInd
    {
      get => nextPageDisplayLineInd ??= new();
      set => nextPageDisplayLineInd = value;
    }

    /// <summary>
    /// Gets a value of Blank.
    /// </summary>
    [JsonIgnore]
    public Array<BlankGroup> Blank => blank ??= new(BlankGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Blank for json serialization.
    /// </summary>
    [JsonPropertyName("blank")]
    [Computed]
    public IList<BlankGroup> Blank_Json
    {
      get => blank;
      set => Blank.Assign(value);
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
    /// A value of KansasObligee.
    /// </summary>
    [JsonPropertyName("kansasObligee")]
    public CsePerson KansasObligee
    {
      get => kansasObligee ??= new();
      set => kansasObligee = value;
    }

    /// <summary>
    /// A value of WrongAcct.
    /// </summary>
    [JsonPropertyName("wrongAcct")]
    public CollectionAdjustmentReason WrongAcct
    {
      get => wrongAcct ??= new();
      set => wrongAcct = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public Common Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public Common Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePersonsWorkSet Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of PadCsePersonNum.
    /// </summary>
    [JsonPropertyName("padCsePersonNum")]
    public TextWorkArea PadCsePersonNum
    {
      get => padCsePersonNum ??= new();
      set => padCsePersonNum = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of CursorPosition.
    /// </summary>
    [JsonPropertyName("cursorPosition")]
    public CursorPosition CursorPosition
    {
      get => cursorPosition ??= new();
      set => cursorPosition = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Obligation Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of EndOfMonth.
    /// </summary>
    [JsonPropertyName("endOfMonth")]
    public DateWorkArea EndOfMonth
    {
      get => endOfMonth ??= new();
      set => endOfMonth = value;
    }

    /// <summary>
    /// A value of CountPromptsSelected.
    /// </summary>
    [JsonPropertyName("countPromptsSelected")]
    public Common CountPromptsSelected
    {
      get => countPromptsSelected ??= new();
      set => countPromptsSelected = value;
    }

    /// <summary>
    /// A value of FormattedFromDate.
    /// </summary>
    [JsonPropertyName("formattedFromDate")]
    public WorkArea FormattedFromDate
    {
      get => formattedFromDate ??= new();
      set => formattedFromDate = value;
    }

    /// <summary>
    /// A value of FormattedToDate.
    /// </summary>
    [JsonPropertyName("formattedToDate")]
    public WorkArea FormattedToDate
    {
      get => formattedToDate ??= new();
      set => formattedToDate = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      previousGroupsToSave = null;
      pfkeyPressed = null;
      max = null;
      linesToFill = null;
      keyChange = null;
      nextPageCollection = null;
      nextPageObligationTransactionRln = null;
      nextPageObligationTransaction = null;
      nextPageObligationType = null;
      nextPageObligation = null;
      nextPageSearchTo = null;
      nextPageDisplayLineInd = null;
      blank = null;
      legalAction = null;
      found = null;
      obligationType = null;
      zero = null;
      current = null;
      year = null;
      month = null;
      pass = null;
      padCsePersonNum = null;
      counter = null;
      cursorPosition = null;
      temp = null;
      endOfMonth = null;
      countPromptsSelected = null;
      formattedFromDate = null;
      formattedToDate = null;
    }

    private Common previousGroupsToSave;
    private WorkArea pfkeyPressed;
    private DateWorkArea max;
    private Common linesToFill;
    private Common keyChange;
    private Collection nextPageCollection;
    private ObligationTransactionRln nextPageObligationTransactionRln;
    private ObligationTransaction nextPageObligationTransaction;
    private ObligationType nextPageObligationType;
    private Obligation nextPageObligation;
    private DateWorkArea nextPageSearchTo;
    private Common nextPageDisplayLineInd;
    private Array<BlankGroup> blank;
    private LegalAction legalAction;
    private CsePerson kansasObligee;
    private CollectionAdjustmentReason wrongAcct;
    private Common found;
    private ObligationType obligationType;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Common year;
    private Common month;
    private CsePersonsWorkSet pass;
    private TextWorkArea padCsePersonNum;
    private Common counter;
    private CursorPosition cursorPosition;
    private Obligation temp;
    private DateWorkArea endOfMonth;
    private Common countPromptsSelected;
    private WorkArea formattedFromDate;
    private WorkArea formattedToDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
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

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private CsePersonAccount obligee1;
    private DisbursementTransaction disbursementTransaction;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private LegalAction legalAction;
    private Collection collection;
    private CsePerson obligee2;
    private ObligationTransaction obligationTransaction;
    private CsePerson obligor;
    private LegalActionPerson legalActionPerson;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CashReceiptDetail cashReceiptDetail;
    private ObligationType obligationType;
  }
#endregion
}

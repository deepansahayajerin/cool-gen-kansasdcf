// Program: FN_COLL_LST_COLL_ACTVY_BY_AP_PYR, ID: 371741345, model: 746.
// Short name: SWECOLLP
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
/// A program: FN_COLL_LST_COLL_ACTVY_BY_AP_PYR.
/// </para>
/// <para>
/// RESP: FINANCE
/// This is a list screen that shows an obligor's collections, and activity 
/// related to those collections.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCollLstCollActvyByApPyr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COLL_LST_COLL_ACTVY_BY_AP_PYR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCollLstCollActvyByApPyr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCollLstCollActvyByApPyr.
  /// </summary>
  public FnCollLstCollActvyByApPyr(IContext context, Import import,
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
    // -----------------------------------------------------------------
    // Date	  Programmer		Description
    // 06/16/96  Holly Kennedy-MTW	Made changes from the screen signoff 
    // meetings. Added Selection criteria for Court Order Number. Added fields
    // to the group view.
    // 12/03/96  R. Marchman		Add new security and next tran
    // ------------------------------------------------------------------
    // 03/17/97    A.Kinney		Various fixes to transaction.  Payor prompt uses 
    // permitted values.  Court order does not display an error message.  Flow
    // from menu, cursor on Nexttran.  Only one collection displayed even when
    // more exist.
    // 04/28/97     JF. Caillouet	Change Current Date
    // 10/12/98     E. Parker		Made owed amts display zero; removed list to and 
    // from fields; clear 'S' select char upon return; adjusted screen fields to
    // line up properly and moved fields at top of screen to be less cluttered;
    // added exit state if user tries to come directly to this screen; removed
    // MCOL logic; removed PF2 and PF4 logic; added logic to require selection
    // of CR Detail on flow to CRRC, CRUC, or COLA; added logic to avoid select
    // lines on blank lines or collection adjustments; changed logic to use
    // fn_check_unprocessed_transactions instead of
    // fn_cab_unproc_tran_by_obligor; moved total owed logic so that it would be
    // calculated even when monthly_obligor_summary is not found; removed use
    // of fn_hardcoded_cash_receipting as it is no longer needed; moved location
    // of next tran logic so error wouldn't wipe out screen.
    // 11/10/98      E. Parker		Added logic to save Collection 
    // system_generated_id for passing to COLA, added logic to pass flag to
    // COLA, changed logic to force selection of an activity line on flow to
    // COLA.
    // 03/30/99      A. Doty
    // 
    // Removed logic to compute summary
    // & undist amts with a call to an
    // AB (the new AB does not use the
    // summary entities).
    // 04/19/99     E. Parker		Changed logic to allow selection of either CRD or
    // Coll to flow to COLA; Removed sort of Supp Person and added sort by Coll
    // Created Tmst;  Added hidden Debt Detail Due Date to group view to pass
    // to DEBT screen if Activity line is selected; Added Obligation
    // Primary_Secondary_Code to Activity Line; Corrected logic to avoid
    // displaying supp person on 'no supp person required' type obligations;
    // Added logic to look at Ob .
    // -----------------------------------------------------------------
    // ****************************************************************
    // 04/19/99     E. Parker		Changed logic to allow selection of either CRD or
    // Coll to flow to COLA; Removed sort of Supp Person and added sort by Coll
    // Created Tmst;  Added hidden Debt Detail Due Date to group view to pass
    // to DEBT screen if Activity line is selected; Added Obligation
    // Primary_Secondary_Code to Activity Line; Corrected logic to avoid
    // displaying supp person on no supp person required type Obligations;
    // Added logic to look at Ob Assignment for no supp person required type
    // Obligations to retrieve Worker ID assigned.
    // 05/14/99     E. Parker		Added logic to provide error message if a 
    // Collection is selected that was applied to a different obligor than the
    // one displayed on the screen.  Also set both from and to date to Debt
    // Detail Due Date if a Collection is selected to flow to DEBT.
    // 06/05/99     E. Parker		Changed attributes on Error Information Line so 
    // it would not show up red when there was not a message.  Also added logic
    // to move spaces to local service provider if exit state was set in
    // fn_read_case_no_and_worker_id (otherwise, previous service provider will
    // remain set).
    // 06/22/99 - B Adams  -  Read properties set; first full day of Summer
    // 11/18/99 - K Doshi - PR80025 - Add flow to PACC; first cold day of Winter
    // 12/03/99 - E. Parker - PR#80974  Changed screen to use Cash Receipt Event
    // Received Date instead of Cash Receipt Received Date.  Cleaned up
    // commented code.
    // 03/15/2000  V.Madhira  --  PR# 79177    Added flow to COMN.
    // *****************************************************************
    // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's deleted by 
    // REIP.
    // *****************************************************************
    // 05/08/07    M. Fan       PR00202233  Modified to Qualify the read 
    // collection for PACC with cash receipt detail, cash receipt, cash receipt
    // event, cash receipt source type and cash receipt type.
    // *****************************************************************
    // 09/13/12    GVandy       CQ35548  Implement FTIE and FTIR security 
    // profile restrictions.
    // *****************************************************************
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *** Set hardcode values
    UseFnHardcodedDebtDistribution();
    local.HardcodeActivityLineType.Flag = "A";
    local.HardcodeCollectionLineType.Flag = "C";

    // *** Move all IMPORTs to EXPORTs.
    export.NextTransaction.Command = import.NextTransaction.Command;
    local.CsePersonNumber.Text10 = import.Obligor.Number;
    UseEabPadLeftWithZeros();
    export.Obligor.Number = local.CsePersonNumber.Text10;
    export.ObligorPrompt.PromptField = import.ObligorPrompt.PromptField;
    export.ShowColl.Flag = import.ShowColl.Flag;
    export.CurrentOwed.TotalCurrency = import.CurrentOwed.TotalCurrency;
    export.ArrearsOwed.TotalCurrency = import.ArrearsOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = import.InterestOwed.TotalCurrency;
    export.TotalOwed.TotalCurrency = import.TotalOwed.TotalCurrency;
    export.TotalUndistAmt.TotalCurrency = import.TotalUndistAmt.TotalCurrency;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.ScreenOwedAmounts.ErrorInformationLine =
      import.ScreenOwedAmounts.ErrorInformationLine;
    export.CourtOrderPrompt.PromptField = import.CourtOrderPrompt.PromptField;
    export.HiddenDlgflwCashReceipt.Assign(import.HiddenDlgflwCashReceipt);
    export.HiddenDlgflwCashReceiptDetail.Assign(
      import.HiddenDlgflwCashReceiptDetail);
    export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier;
    export.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier =
      import.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier;
    export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier =
      import.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier;
    export.HiddenMoreDataInDb.Flag = import.HiddenMoreDataInDb.Flag;
    export.HiddenColl.Date = import.HiddenColl.Date;
    MoveStandard(import.Standard, export.Standard);
    export.Minus.Text1 = import.Minus.Text1;
    export.Plus.Text1 = import.Plus.Text1;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsEmpty(export.Obligor.Number))
      {
        ExitState = "FN0000_CANNOT_NEXTTRAN_TO_COLL";
      }

      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else
    {
    }

    // *** Check for, and validate, selection ***
    for(import.PageKey.Index = 0; import.PageKey.Index < import.PageKey.Count; ++
      import.PageKey.Index)
    {
      if (!import.PageKey.CheckSize())
      {
        break;
      }

      export.PageKey.Index = import.PageKey.Index;
      export.PageKey.CheckSize();

      export.PageKey.Update.KeyCashReceipt.SequentialNumber =
        import.PageKey.Item.KeyCashReceipt.SequentialNumber;
      MoveCashReceiptDetail3(import.PageKey.Item.KeyCashReceiptDetail,
        export.PageKey.Update.KeyCashReceiptDetail);
      export.PageKey.Update.KeyCashReceiptSourceType.SystemGeneratedIdentifier =
        import.PageKey.Item.KeyCashReceiptSourceType.SystemGeneratedIdentifier;
      export.PageKey.Update.KeyCashReceiptEvent.SystemGeneratedIdentifier =
        import.PageKey.Item.KeyCashReceiptEvent.SystemGeneratedIdentifier;
      export.PageKey.Update.KeyCashReceiptType.SystemGeneratedIdentifier =
        import.PageKey.Item.KeyCashReceiptType.SystemGeneratedIdentifier;
      MoveCollection3(import.PageKey.Item.KeyCollection,
        export.PageKey.Update.KeyCollection);
      export.PageKey.Update.AdjustentLine.Flag =
        import.PageKey.Item.AdjustmentLine.Flag;
    }

    import.PageKey.CheckIndex();
    local.ItemSelected.Flag = "N";

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = import.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
        import.Group.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
      export.Group.Update.HiddenCashReceiptSourceType.
        SystemGeneratedIdentifier =
          import.Group.Item.HiddenCashReceiptSourceType.
          SystemGeneratedIdentifier;
      export.Group.Update.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
        import.Group.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
      export.Group.Update.HiddenCashReceiptDetail.Assign(
        import.Group.Item.HiddenCashReceiptDetail);
      export.Group.Update.HiddenCashReceipt.SequentialNumber =
        import.Group.Item.HiddenCashReceipt.SequentialNumber;
      export.Group.Update.HiddenObligation.SystemGeneratedIdentifier =
        import.Group.Item.HiddenObligation.SystemGeneratedIdentifier;
      export.Group.Update.HiddenObligationTransaction.
        SystemGeneratedIdentifier =
          import.Group.Item.HiddenObligationTransaction.
          SystemGeneratedIdentifier;
      export.Group.Update.HiddenObligationType.SystemGeneratedIdentifier =
        import.Group.Item.HiddenObligationType.SystemGeneratedIdentifier;
      export.Group.Update.HiddenCsePerson.Number =
        import.Group.Item.HiddenCsePerson.Number;
      export.Group.Update.HiddenColl.Date = import.Group.Item.HiddenColl.Date;
      MoveCollection3(import.Group.Item.HiddenCollection,
        export.Group.Update.HiddenCollection);
      export.Group.Update.HiddenLineType.Flag =
        import.Group.Item.HiddenLineType.Flag;
      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.ListScreenWorkArea.TextLine76 =
        import.Group.Item.ListScreenWorkArea.TextLine76;
      export.Group.Update.HiddenDebtDetail.DueDt =
        import.Group.Item.HiddenDebtDetail.DueDt;

      if (AsChar(export.Group.Item.HiddenLineType.Flag) != 'A' && AsChar
        (export.Group.Item.HiddenLineType.Flag) != 'C')
      {
        var field = GetField(export.Group.Item.Common, "selectChar");

        field.Intensity = Intensity.Dark;
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.Group.Item.Common, "selectChar");

        field.Intensity = Intensity.Normal;
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
      }

      if (IsEmpty(import.Group.Item.Common.SelectChar))
      {
        // **OK**
      }
      else if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
      {
        // *** Check for multiple select ***
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          continue;
        }

        local.ItemSelected.Flag = "Y";
        local.LinetypeOfSelectedRow.Flag =
          import.Group.Item.HiddenLineType.Flag;

        // *** Set export views to selected item ***
        if (AsChar(import.Group.Item.HiddenLineType.Flag) == AsChar
          (local.HardcodeActivityLineType.Flag))
        {
          MoveObligation(import.Group.Item.HiddenObligation,
            export.SelectedObligation);
          export.SelectedObligationType.SystemGeneratedIdentifier =
            import.Group.Item.HiddenObligationType.SystemGeneratedIdentifier;
          export.SelectedObligationTransaction.SystemGeneratedIdentifier =
            import.Group.Item.HiddenObligationTransaction.
              SystemGeneratedIdentifier;
          export.SelectedChild.Number =
            import.Group.Item.HiddenCsePerson.Number;
          export.SelectedCollection.SystemGeneratedIdentifier =
            import.Group.Item.HiddenCollection.SystemGeneratedIdentifier;
          export.SelectedCashReceiptType.SystemGeneratedIdentifier =
            import.Group.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
          export.SelectedCashReceiptSourceType.SystemGeneratedIdentifier =
            import.Group.Item.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier;
          export.SelectedCashReceiptEvent.SystemGeneratedIdentifier =
            import.Group.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
          export.SelectedCashReceiptDetail.SequentialIdentifier =
            import.Group.Item.HiddenCashReceiptDetail.SequentialIdentifier;
          export.SelectedCashReceipt.SequentialNumber =
            import.Group.Item.HiddenCashReceipt.SequentialNumber;
          export.PassToDebtActFrom.Date =
            import.Group.Item.HiddenDebtDetail.DueDt;
          export.PassToDebtActTo.Date =
            import.Group.Item.HiddenDebtDetail.DueDt;
        }
        else
        {
          MoveObligation(import.Group.Item.HiddenObligation,
            export.SelectedObligation);
          export.SelectedObligationTransaction.SystemGeneratedIdentifier =
            import.Group.Item.HiddenObligationTransaction.
              SystemGeneratedIdentifier;
          export.SelectedObligationType.SystemGeneratedIdentifier =
            import.Group.Item.HiddenObligationType.SystemGeneratedIdentifier;
          export.SelectedChild.Number =
            import.Group.Item.HiddenCsePerson.Number;
          export.SelectedCashReceiptType.SystemGeneratedIdentifier =
            import.Group.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
          export.SelectedCashReceiptSourceType.SystemGeneratedIdentifier =
            import.Group.Item.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier;
          export.SelectedCashReceiptEvent.SystemGeneratedIdentifier =
            import.Group.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
          export.SelectedCashReceiptDetail.SequentialIdentifier =
            import.Group.Item.HiddenCashReceiptDetail.SequentialIdentifier;
          export.SelectedCashReceipt.SequentialNumber =
            import.Group.Item.HiddenCashReceipt.SequentialNumber;
        }
      }
      else
      {
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        var field = GetField(export.Group.Item.Common, "selectChar");

        field.Error = true;
      }
    }

    import.Group.CheckIndex();

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      export.Hidden.CsePersonNumber = export.Obligor.Number;
      export.Hidden.LegalActionIdentifier = export.LegalAction.Identifier;
      export.Hidden.ObligationId =
        export.SelectedObligation.SystemGeneratedIdentifier;
      export.Hidden.CourtCaseNumber = export.LegalAction.StandardNumber ?? "";
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

    if (Equal(global.Command, "RETCOLA") || Equal
      (global.Command, "RETCRRC") || Equal(global.Command, "RETCRUC") || Equal
      (global.Command, "RETDEBT") || Equal(global.Command, "RETPACC") || Equal
      (global.Command, "RETLCDA"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        export.Group.Update.Common.SelectChar = "";
      }

      export.Group.CheckIndex();

      if (AsChar(local.ItemSelected.Flag) == 'Y')
      {
        local.ItemSelected.Flag = "N";
      }

      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "COLA") || Equal(global.Command, "CRUC") || Equal
      (global.Command, "CRRC") || Equal(global.Command, "DEBT") || Equal
      (global.Command, "PACC") || Equal(global.Command, "COMN") || Equal
      (global.Command, "LCDA"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // 09/13/12  GVandy  CQ35548  Check for FTIE and FTIR security profile 
      // restrictions.
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "REFRESH"))
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

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (IsEmpty(import.ShowColl.Flag) || AsChar(import.ShowColl.Flag) == 'Y'
      || AsChar(import.ShowColl.Flag) == 'N')
    {
      // *** Show Collection Indicator is valid ***
    }
    else
    {
      ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
      global.Command = "BYPASS";
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        break;
      case "DISPLAY":
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          ExitState = "NO_SELECT_WITH_DISPLAY_OPTION";

          break;
        }

        if (IsEmpty(import.ShowColl.Flag))
        {
          export.ShowColl.Flag = "N";
        }

        break;
      case "REFRESH":
        // ****SET PERSON NUMBER TO PREVIOUS ONE ENTERED
        global.Command = "DISPLAY";

        break;
      case "COLA":
        break;
      case "CRUC":
        break;
      case "CRRC":
        break;
      case "DEBT":
        // ---------------------------
        // PR80025 - 11/19/99
        // Add new flow to PACC
        // ---------------------------
        break;
      case "PACC":
        break;
      case "LCDA":
        break;
      default:
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          ExitState = "DO_NOT_MAKE_SELECTION_WITH_OPT";
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
      case "BYPASS":
        break;
      case "DISPLAY":
        // *** Call EAB to get CSE person name and SSN ***
        export.CsePersonsWorkSet.Number = export.Obligor.Number;
        local.CsePersonsWorkSet.Number = export.Obligor.Number;
        UseSiReadCsePerson2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.Obligor.Number = export.CsePersonsWorkSet.Number;
        MoveCsePersonsWorkSet(export.CsePersonsWorkSet, local.CsePersonsWorkSet);
          
        UseFnComputeSummaryTotals();
        export.CurrentOwed.TotalCurrency =
          local.ScreenOwedAmounts.CurrentAmountOwed;
        export.ArrearsOwed.TotalCurrency =
          local.ScreenOwedAmounts.ArrearsAmountOwed;
        export.InterestOwed.TotalCurrency =
          local.ScreenOwedAmounts.InterestAmountOwed;
        export.TotalOwed.TotalCurrency =
          local.ScreenOwedAmounts.TotalAmountOwed;
        export.ScreenOwedAmounts.ErrorInformationLine =
          local.ScreenOwedAmounts.ErrorInformationLine;

        if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        export.PageKey.Index = -1;
        export.PageKey.Count = 0;

        export.PageKey.Index = 0;
        export.PageKey.CheckSize();

        export.Standard.PageNumber = 1;

        // *** Initialize subscript of local group view that will
        // be populated with detail lines via an EAB ***
        // Madhu made this to 0 earlier it was 1
        local.Group.Index = -1;
        local.Group.Count = 0;
        export.PageKey.Update.KeyCashReceipt.SequentialNumber =
          export.HiddenDlgflwCashReceipt.SequentialNumber;
        MoveCashReceiptDetail3(export.HiddenDlgflwCashReceiptDetail,
          export.PageKey.Update.KeyCashReceiptDetail);
        export.PageKey.Update.KeyCashReceiptEvent.SystemGeneratedIdentifier =
          export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier;
        export.PageKey.Update.KeyCashReceiptSourceType.
          SystemGeneratedIdentifier =
            export.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier;
        export.PageKey.Update.KeyCashReceiptType.SystemGeneratedIdentifier =
          export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier;

        // *** READ EACH for selection list. ***
        foreach(var item in ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt())
          
        {
          // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if security 
          // profile restriction is FTIE.
          if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
            (entities.CashReceiptSourceType.Code, "FDSO"))
          {
            continue;
          }

          export.HiddenColl.Date = entities.CashReceiptDetail.CollectionDate;

          // Madhu added this line on 3rd of August.
          if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
          {
            break;
          }

          // *****************************************************************
          // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
          // deleted by REIP.
          ReadCashReceiptDetailStatHistory();

          if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
            "REIPDELETE"))
          {
            continue;
          }

          // *****************************************************************
          ++local.Group.Index;
          local.Group.CheckSize();

          export.HiddenDlgflwCashReceipt.Assign(entities.CashReceipt);
          MoveCashReceiptDetail4(entities.CashReceiptDetail,
            local.CollectionLine.CcashReceiptDetail);
          MoveCashReceiptDetail1(entities.CashReceiptDetail,
            export.HiddenDlgflwCashReceiptDetail);
          local.CollectionLine.CcashReceiptEvent.ReceivedDate =
            entities.CashReceiptEvent.ReceivedDate;
          export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier =
            entities.CashReceiptType.SystemGeneratedIdentifier;
          local.Group.Update.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          MoveCashReceiptDetail1(entities.CashReceiptDetail,
            export.HiddenDlgflwCashReceiptDetail);
          local.Group.Update.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          local.Group.Update.HiddenCashReceiptSourceType.
            SystemGeneratedIdentifier =
              entities.CashReceiptSourceType.SystemGeneratedIdentifier;
          MoveCashReceiptDetail2(entities.CashReceiptDetail,
            local.Group.Update.HiddenCashReceiptDetail);
          local.Group.Update.HiddenCashReceipt.SequentialNumber =
            entities.CashReceipt.SequentialNumber;
          local.Group.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
            entities.CashReceiptType.SystemGeneratedIdentifier;

          // *** Save identifier ***
          MoveCashReceiptDetail2(entities.CashReceiptDetail,
            local.Group.Update.HiddenCashReceiptDetail);
          local.CollectionLine.CcashReceiptSourceType.Code =
            entities.CashReceiptSourceType.Code;

          // 09/13/12 GVandy  CQ35548  Do not display Cash Receipt Source Type 
          // if security profile restriction is FTIR.
          if (AsChar(local.FtirRestriction.Flag) == 'Y')
          {
            local.CollectionLine.CcashReceiptSourceType.Code = "";
          }

          // *** Save identifier ***
          local.Group.Update.HiddenCashReceiptSourceType.
            SystemGeneratedIdentifier =
              entities.CashReceiptSourceType.SystemGeneratedIdentifier;

          // *** Calculate Undistributed Amount ***
          if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) !=
            'Y')
          {
            local.CollectionLine.Cundst.TotalCurrency =
              entities.CashReceiptDetail.CollectionAmount - (
                entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() +
              entities
              .CashReceiptDetail.DistributedAmount.GetValueOrDefault());

            // *****************************************************************
            // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
            // deleted by REIP.
            local.CollectionLine.CcashReceiptDetailStatHistory.ReasonCodeId =
              entities.CashReceiptDetailStatHistory.ReasonCodeId;

            // *****************************************************************
          }

          // *** Call EAB to format detail line.  Set
          // local work field to indicate type of line. ***
          local.Group.Update.HiddenLineType.Flag =
            local.HardcodeCollectionLineType.Flag;

          // collection line
          UseFnEabCollFormatDetailLine1();

          // *** If Show Collection Only Indicator = Y, read next item ***
          if (AsChar(export.ShowColl.Flag) == 'Y')
          {
            // *** Read next cash receipt ***
          }
          else
          {
            // Madhu moved it inside the else statement.
            if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
            {
              break;
            }

            // Madhu changed this subscript  to one to obtain a blank  line 
            // here.
            // ***---  included Obligation_Type and Debt_Detail in Read Each - b
            // adams  -  6/21/99
            // *** Get activity for each Cash Receipt.  Read DISTINCT on 
            // Collection. ***
            foreach(var item1 in ReadCollectionObligationTransactionObligationObligationType())
              
            {
              // Madhu added this line here.
              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                goto ReadEach1;
              }

              ++local.Group.Index;
              local.Group.CheckSize();

              local.Group.Update.HiddenCashReceiptEvent.
                SystemGeneratedIdentifier =
                  entities.CashReceiptEvent.SystemGeneratedIdentifier;
              local.Group.Update.HiddenCashReceiptSourceType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptSourceType.SystemGeneratedIdentifier;
              MoveCashReceiptDetail2(entities.CashReceiptDetail,
                local.Group.Update.HiddenCashReceiptDetail);
              local.Group.Update.HiddenCashReceipt.SequentialNumber =
                entities.CashReceipt.SequentialNumber;
              local.Group.Update.HiddenCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;

              // *** Save identifiers ***
              MoveObligation(entities.Obligation,
                local.Group.Update.HiddenObligation);
              local.ActivityLine.Aobligation.PrimarySecondaryCode =
                entities.Obligation.PrimarySecondaryCode;
              local.Group.Update.HiddenObligationTransaction.
                SystemGeneratedIdentifier =
                  entities.ObligationTransaction.SystemGeneratedIdentifier;
              MoveCollection1(entities.Collection,
                local.ActivityLine.Acollection);
              MoveCollection2(entities.Collection,
                local.Group.Update.HiddenCollection);
              local.ActivityLine.AobligationType.Code =
                entities.ObligationType.Code;
              MoveObligationType(entities.ObligationType,
                local.Group.Update.HiddenObligationType);
              local.ActivityLine.AdebtDetail.DueDt = entities.DebtDetail.DueDt;
              local.Group.Update.HiddenDebtDetail.DueDt =
                entities.DebtDetail.DueDt;
              local.ActivityLine.AautoManualDist.Flag =
                entities.Collection.DistributionMethod;
              local.ActivityLine.Acollection.AppliedToCode =
                entities.Collection.AppliedToOrderTypeCode;
              local.ActivityLine.AdetailProcessDat.Date =
                Date(entities.Collection.CreatedTmst);
              local.Group.Update.HiddenCollDate.Date =
                local.ActivityLine.AdetailProcessDat.Date;

              // *** Set Amount Applied ***
              local.ActivityLine.Acollection.Amount =
                entities.Collection.Amount;
              local.ActivityLine.Acollection.AppliedToCode =
                entities.Collection.AppliedToCode;
              local.ActivityLine.Acollection.ProgramAppliedTo =
                entities.Collection.ProgramAppliedTo;

              if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'N')
              {
                // *** If the supported indicator on the Obligation Type is "N",
                // there is no supported person related to this obligation.  
                // Need to blank out any previous supported person date.  Also
                // need to read for Ob Assignment. ***
                local.ActivityLine.Achild.FormattedName = "";

                if (ReadObligationAssignmentServiceProviderOfficeServiceProvider())
                  
                {
                  local.ActivityLine.AserviceProvider.UserId =
                    entities.ServiceProvider.UserId;
                }
                else
                {
                  local.ActivityLine.AserviceProvider.UserId = "";
                }
              }
              else if (ReadCsePerson1())
              {
                local.CsePersonsWorkSet.Number = entities.Child.Number;
                local.Group.Update.HiddenChild.Number = entities.Child.Number;
                local.Pass.Number = entities.Child.Number;
                UseSiReadCsePerson1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                // ***  Call Action Block to get Worker ID
                UseFnDetCaseNoAndWrkrForDbt();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.ActivityLine.AserviceProvider.UserId = "";
                  ExitState = "ACO_NN0000_ALL_OK";
                }
              }
              else
              {
                ExitState = "OE0056_NF_CHILD_CSE_PERSON";

                // ******	NOT FOUND condition will result in program abort. 
                // ******
                return;
              }

              local.ActivityLine.Aprogram.Code =
                entities.Collection.ProgramAppliedTo;

              // *** Call EAB to format detail line.  Set
              // local work field to indicate type of line. ***
              local.Group.Update.HiddenLineType.Flag =
                local.HardcodeActivityLineType.Flag;

              // activity line
              UseFnEabCollFormatDetailLine2();

              if (AsChar(entities.Collection.AdjustedInd) == 'Y')
              {
                // Moved into the if by Madhu
                if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                {
                  goto ReadEach1;
                }

                ++local.Group.Index;
                local.Group.CheckSize();

                if (ReadCollectionAdjustmentReason())
                {
                  local.AdjLineAdjRsn.Code =
                    entities.CollectionAdjustmentReason.Code;
                }

                local.AdjLineCollection.Amount = -entities.Collection.Amount;
                local.AdjLineCollection.CollectionAdjustmentDt =
                  entities.Collection.CollectionAdjustmentDt;
                local.AdjLineCollection.LastUpdatedBy =
                  entities.Collection.LastUpdatedBy;

                // *** Call EAB to format detail line.  Set
                // local work field to indicate type of line. ***
                local.Group.Update.AdjustmentLine.Flag = "Y";
                local.Group.Update.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptEvent.SystemGeneratedIdentifier;
                local.Group.Update.HiddenCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptSourceType.SystemGeneratedIdentifier;
                MoveCashReceiptDetail2(entities.CashReceiptDetail,
                  local.Group.Update.HiddenCashReceiptDetail);
                local.Group.Update.HiddenCashReceipt.SequentialNumber =
                  entities.CashReceipt.SequentialNumber;
                local.Group.Update.HiddenCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;
                local.Group.Update.HiddenLineType.Flag = "D";
                MoveObligation(entities.Obligation,
                  local.Group.Update.HiddenObligation);
                local.Group.Update.HiddenObligationTransaction.
                  SystemGeneratedIdentifier =
                    entities.ObligationTransaction.SystemGeneratedIdentifier;
                MoveCollection2(entities.Collection,
                  local.Group.Update.HiddenCollection);
                MoveObligationType(entities.ObligationType,
                  local.Group.Update.HiddenObligationType);
                local.Group.Update.HiddenDebtDetail.DueDt =
                  entities.DebtDetail.DueDt;
                UseFnEabCollFormatDetailLine3();

                // Madhu commented this out  ...
              }
            }
          }
        }

ReadEach1:

        export.Group.Index = -1;
        export.Group.Count = 0;

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          export.Group.Index = local.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.ListScreenWorkArea.TextLine76 =
            local.Group.Item.ListScreenWorkArea.TextLine76;
          export.Group.Update.HiddenLineType.Flag =
            local.Group.Item.HiddenLineType.Flag;
          export.Group.Update.HiddenCashReceipt.SequentialNumber =
            local.Group.Item.HiddenCashReceipt.SequentialNumber;
          export.Group.Update.HiddenCashReceiptDetail.Assign(
            local.Group.Item.HiddenCashReceiptDetail);
          export.Group.Update.HiddenCashReceiptSourceType.
            SystemGeneratedIdentifier =
              local.Group.Item.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier;
          export.Group.Update.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
            local.Group.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
          export.Group.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
            local.Group.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
          export.Group.Update.HiddenCsePerson.Number =
            local.Group.Item.HiddenChild.Number;
          export.Group.Update.HiddenObligation.SystemGeneratedIdentifier =
            local.Group.Item.HiddenObligation.SystemGeneratedIdentifier;
          export.Group.Update.HiddenObligationType.SystemGeneratedIdentifier =
            local.Group.Item.HiddenObligationType.SystemGeneratedIdentifier;
          export.Group.Update.HiddenObligationTransaction.
            SystemGeneratedIdentifier =
              local.Group.Item.HiddenObligationTransaction.
              SystemGeneratedIdentifier;
          export.Group.Update.HiddenColl.Date =
            local.Group.Item.HiddenCollDate.Date;
          MoveCollection3(local.Group.Item.HiddenCollection,
            export.Group.Update.HiddenCollection);
          export.Group.Update.HiddenDebtDetail.DueDt =
            local.Group.Item.HiddenDebtDetail.DueDt;

          if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
          {
            ++export.PageKey.Index;
            export.PageKey.CheckSize();

            export.PageKey.Update.KeyCashReceipt.SequentialNumber =
              export.HiddenDlgflwCashReceipt.SequentialNumber;
            MoveCashReceiptDetail3(export.HiddenDlgflwCashReceiptDetail,
              export.PageKey.Update.KeyCashReceiptDetail);
            MoveCashReceiptDetail3(local.Group.Item.HiddenCashReceiptDetail,
              export.PageKey.Update.KeyCashReceiptDetail);
            export.PageKey.Update.KeyCashReceiptSourceType.
              SystemGeneratedIdentifier =
                export.HiddenDlgflwCashReceiptSourceType.
                SystemGeneratedIdentifier;
            export.PageKey.Update.KeyCashReceiptEvent.
              SystemGeneratedIdentifier =
                export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier;
            export.PageKey.Update.KeyCashReceiptType.SystemGeneratedIdentifier =
              export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier;
            MoveCollection3(local.Group.Item.HiddenCollection,
              export.PageKey.Update.KeyCollection);
            export.PageKey.Update.AdjustentLine.Flag =
              local.Group.Item.AdjustmentLine.Flag;
          }
        }

        local.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Minus.Text1 = "";

          if (export.Group.IsFull)
          {
            export.Plus.Text1 = "+";
            export.HiddenMoreDataInDb.Flag = "Y";
          }
          else
          {
            export.Plus.Text1 = "";
          }

          if (export.Group.IsEmpty)
          {
            export.HiddenMoreDataInDb.Flag = "";
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
          }
        }

        break;
      case "NEXT":
        if (IsEmpty(export.Plus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        if (export.Standard.PageNumber == Export.PageKeyGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_DISPLAY_CAPAB";

          return;
        }

        ++export.Standard.PageNumber;

        if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        export.PageKey.Index = export.Standard.PageNumber - 1;
        export.PageKey.CheckSize();

        MoveCashReceiptDetail3(export.PageKey.Item.KeyCashReceiptDetail,
          export.HiddenDlgflwCashReceiptDetail);
        export.HiddenDlgflwCashReceipt.SequentialNumber =
          export.PageKey.Item.KeyCashReceipt.SequentialNumber;
        export.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier =
          export.PageKey.Item.KeyCashReceiptSourceType.
            SystemGeneratedIdentifier;
        export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier =
          export.PageKey.Item.KeyCashReceiptEvent.SystemGeneratedIdentifier;
        export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier =
          export.PageKey.Item.KeyCashReceiptType.SystemGeneratedIdentifier;

        // *** Initialize subscript of local group view that will
        // be populated with detail lines via an EAB ***
        // Madhu made this to 0 earlier it was 1
        local.Group.Index = -1;
        local.Group.Count = 0;
        local.SkipProcessing.Flag = "Y";

        // *** READ EACH for selection list. ***
        foreach(var item in ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt())
          
        {
          // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if security 
          // profile restriction is FTIE.
          if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
            (entities.CashReceiptSourceType.Code, "FDSO"))
          {
            continue;
          }

          export.HiddenColl.Date = entities.CashReceiptDetail.CollectionDate;

          // Madhu added this line on 3rd of August.
          if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
          {
            break;
          }

          // *****************************************************************
          // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
          // deleted by REIP.
          ReadCashReceiptDetailStatHistory();

          if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
            "REIPDELETE"))
          {
            continue;
          }

          // *****************************************************************
          if (!Equal(entities.CashReceiptDetail.CreatedTmst,
            export.PageKey.Item.KeyCashReceiptDetail.CreatedTmst) || entities
            .CashReceipt.SequentialNumber != export
            .PageKey.Item.KeyCashReceipt.SequentialNumber || entities
            .CashReceiptEvent.SystemGeneratedIdentifier != export
            .PageKey.Item.KeyCashReceiptEvent.SystemGeneratedIdentifier || entities
            .CashReceiptSourceType.SystemGeneratedIdentifier != export
            .PageKey.Item.KeyCashReceiptSourceType.SystemGeneratedIdentifier ||
            entities.CashReceiptType.SystemGeneratedIdentifier != export
            .PageKey.Item.KeyCashReceiptType.SystemGeneratedIdentifier)
          {
            local.SkipProcessing.Flag = "N";
          }

          if (AsChar(local.SkipProcessing.Flag) == 'N')
          {
            ++local.Group.Index;
            local.Group.CheckSize();

            export.HiddenDlgflwCashReceipt.Assign(entities.CashReceipt);
            MoveCashReceiptDetail4(entities.CashReceiptDetail,
              local.CollectionLine.CcashReceiptDetail);
            MoveCashReceiptDetail1(entities.CashReceiptDetail,
              export.HiddenDlgflwCashReceiptDetail);
            local.CollectionLine.CcashReceiptEvent.ReceivedDate =
              entities.CashReceiptEvent.ReceivedDate;
            export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier =
              entities.CashReceiptType.SystemGeneratedIdentifier;
            local.Group.Update.HiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.CashReceiptEvent.SystemGeneratedIdentifier;
            export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier =
              entities.CashReceiptEvent.SystemGeneratedIdentifier;
            MoveCashReceiptDetail1(entities.CashReceiptDetail,
              export.HiddenDlgflwCashReceiptDetail);
            local.Group.Update.HiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.CashReceiptEvent.SystemGeneratedIdentifier;
            local.Group.Update.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier =
                entities.CashReceiptSourceType.SystemGeneratedIdentifier;
            MoveCashReceiptDetail2(entities.CashReceiptDetail,
              local.Group.Update.HiddenCashReceiptDetail);
            local.Group.Update.HiddenCashReceipt.SequentialNumber =
              entities.CashReceipt.SequentialNumber;
            local.Group.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
              entities.CashReceiptType.SystemGeneratedIdentifier;

            // *** Save identifier ***
            MoveCashReceiptDetail2(entities.CashReceiptDetail,
              local.Group.Update.HiddenCashReceiptDetail);
            local.CollectionLine.CcashReceiptSourceType.Code =
              entities.CashReceiptSourceType.Code;

            // 09/13/12 GVandy  CQ35548  Do not display Cash Receipt Source Type
            // if security profile restriction is FTIR.
            if (AsChar(local.FtirRestriction.Flag) == 'Y')
            {
              local.CollectionLine.CcashReceiptSourceType.Code = "";
            }

            // *** Save identifier ***
            local.Group.Update.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier =
                entities.CashReceiptSourceType.SystemGeneratedIdentifier;

            // *** Calculate Undistributed Amount ***
            if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) !=
              'Y')
            {
              local.CollectionLine.Cundst.TotalCurrency =
                entities.CashReceiptDetail.CollectionAmount - (
                  entities.CashReceiptDetail.RefundedAmount.
                  GetValueOrDefault() + entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault());

              // *****************************************************************
              // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
              // deleted by REIP.
              local.CollectionLine.CcashReceiptDetailStatHistory.ReasonCodeId =
                entities.CashReceiptDetailStatHistory.ReasonCodeId;

              // *****************************************************************
            }

            // *** Call EAB to format detail line.  Set
            // local work field to indicate type of line. ***
            local.Group.Update.HiddenLineType.Flag =
              local.HardcodeCollectionLineType.Flag;

            // collection
            UseFnEabCollFormatDetailLine1();
          }

          // *** If Show Collection Only Indicator = Y, read next item ***
          if (AsChar(export.ShowColl.Flag) == 'Y')
          {
            // *** Read next cash receipt ***
          }
          else
          {
            // Madhu moved it inside the else statement.
            if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
            {
              break;
            }

            // Madhu changed this subscript  to one to obtain a blank  line 
            // here.
            // ***---  included Obligation_Type and Debt_Detail in Read Each - b
            // adams  -  6/21/99
            // *** Get activity for each Cash Receipt.  Read DISTINCT on 
            // Collection. ***
            if (local.Group.Index == -1)
            {
              local.AdjustLine.Flag = export.PageKey.Item.AdjustentLine.Flag;
            }

            foreach(var item1 in ReadCollectionObligationTransactionObligationObligationType())
              
            {
              // Madhu added this line here.
              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                goto ReadEach2;
              }

              if (Lt(export.PageKey.Item.KeyCollection.CreatedTmst,
                entities.Collection.CreatedTmst))
              {
                continue;
              }

              if (Equal(entities.Collection.CreatedTmst,
                export.PageKey.Item.KeyCollection.CreatedTmst) && entities
                .Collection.SystemGeneratedIdentifier > export
                .PageKey.Item.KeyCollection.SystemGeneratedIdentifier)
              {
                continue;
              }

              if (AsChar(local.AdjustLine.Flag) != 'Y')
              {
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptEvent.SystemGeneratedIdentifier;
                local.Group.Update.HiddenCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptSourceType.SystemGeneratedIdentifier;
                MoveCashReceiptDetail2(entities.CashReceiptDetail,
                  local.Group.Update.HiddenCashReceiptDetail);
                local.Group.Update.HiddenCashReceipt.SequentialNumber =
                  entities.CashReceipt.SequentialNumber;
                local.Group.Update.HiddenCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;

                // *** Save identifiers ***
                MoveObligation(entities.Obligation,
                  local.Group.Update.HiddenObligation);
                local.ActivityLine.Aobligation.PrimarySecondaryCode =
                  entities.Obligation.PrimarySecondaryCode;
                local.Group.Update.HiddenObligationTransaction.
                  SystemGeneratedIdentifier =
                    entities.ObligationTransaction.SystemGeneratedIdentifier;
                MoveCollection1(entities.Collection,
                  local.ActivityLine.Acollection);
                MoveCollection2(entities.Collection,
                  local.Group.Update.HiddenCollection);
                local.ActivityLine.AobligationType.Code =
                  entities.ObligationType.Code;
                MoveObligationType(entities.ObligationType,
                  local.Group.Update.HiddenObligationType);
                local.ActivityLine.AdebtDetail.DueDt =
                  entities.DebtDetail.DueDt;
                local.Group.Update.HiddenDebtDetail.DueDt =
                  entities.DebtDetail.DueDt;
                local.ActivityLine.AautoManualDist.Flag =
                  entities.Collection.DistributionMethod;
                local.ActivityLine.Acollection.AppliedToCode =
                  entities.Collection.AppliedToOrderTypeCode;
                local.ActivityLine.AdetailProcessDat.Date =
                  Date(entities.Collection.CreatedTmst);
                local.Group.Update.HiddenCollDate.Date =
                  local.ActivityLine.AdetailProcessDat.Date;

                // *** Set Amount Applied ***
                local.ActivityLine.Acollection.Amount =
                  entities.Collection.Amount;
                local.ActivityLine.Acollection.AppliedToCode =
                  entities.Collection.AppliedToCode;
                local.ActivityLine.Acollection.ProgramAppliedTo =
                  entities.Collection.ProgramAppliedTo;

                if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'N'
                  )
                {
                  // *** If the supported indicator on the Obligation Type is "
                  // N",
                  // there is no supported person related to this obligation.  
                  // Need to blank out any previous supported person date.  Also
                  // need to read for Ob Assignment. ***
                  local.ActivityLine.Achild.FormattedName = "";

                  if (ReadObligationAssignmentServiceProviderOfficeServiceProvider())
                    
                  {
                    local.ActivityLine.AserviceProvider.UserId =
                      entities.ServiceProvider.UserId;
                  }
                  else
                  {
                    local.ActivityLine.AserviceProvider.UserId = "";
                  }
                }
                else if (ReadCsePerson1())
                {
                  local.CsePersonsWorkSet.Number = entities.Child.Number;
                  local.Group.Update.HiddenChild.Number = entities.Child.Number;
                  local.Pass.Number = entities.Child.Number;
                  UseSiReadCsePerson1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  // ***  Call Action Block to get Worker ID
                  UseFnDetCaseNoAndWrkrForDbt();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    local.ActivityLine.AserviceProvider.UserId = "";
                    ExitState = "ACO_NN0000_ALL_OK";
                  }
                }
                else
                {
                  ExitState = "OE0056_NF_CHILD_CSE_PERSON";

                  // ******	NOT FOUND condition will result in program abort. 
                  // ******
                  return;
                }

                local.ActivityLine.Aprogram.Code =
                  entities.Collection.ProgramAppliedTo;

                // *** Call EAB to format detail line.  Set
                // local work field to indicate type of line. ***
                local.Group.Update.HiddenLineType.Flag =
                  local.HardcodeActivityLineType.Flag;

                // activity
                UseFnEabCollFormatDetailLine2();
                local.AdjustLine.Flag = "";
              }

              if (AsChar(entities.Collection.AdjustedInd) == 'Y')
              {
                // Moved into the if by Madhu
                if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                {
                  goto ReadEach2;
                }

                ++local.Group.Index;
                local.Group.CheckSize();

                if (ReadCollectionAdjustmentReason())
                {
                  local.AdjLineAdjRsn.Code =
                    entities.CollectionAdjustmentReason.Code;
                }

                MoveObligation(entities.Obligation,
                  local.Group.Update.HiddenObligation);
                local.Group.Update.HiddenObligationTransaction.
                  SystemGeneratedIdentifier =
                    entities.ObligationTransaction.SystemGeneratedIdentifier;
                MoveCollection2(entities.Collection,
                  local.Group.Update.HiddenCollection);
                MoveObligationType(entities.ObligationType,
                  local.Group.Update.HiddenObligationType);
                local.Group.Update.HiddenDebtDetail.DueDt =
                  entities.DebtDetail.DueDt;
                local.Group.Update.HiddenCollDate.Date =
                  local.ActivityLine.AdetailProcessDat.Date;
                local.AdjLineCollection.Amount = -entities.Collection.Amount;
                local.AdjLineCollection.CollectionAdjustmentDt =
                  entities.Collection.CollectionAdjustmentDt;
                local.AdjLineCollection.LastUpdatedBy =
                  entities.Collection.LastUpdatedBy;

                // *** Call EAB to format detail line.  Set
                // local work field to indicate type of line. ***
                local.Group.Update.HiddenLineType.Flag = "D";
                local.Group.Update.AdjustmentLine.Flag = "Y";
                local.Group.Update.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptEvent.SystemGeneratedIdentifier;
                local.Group.Update.HiddenCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptSourceType.SystemGeneratedIdentifier;
                MoveCashReceiptDetail2(entities.CashReceiptDetail,
                  local.Group.Update.HiddenCashReceiptDetail);
                local.Group.Update.HiddenCashReceipt.SequentialNumber =
                  entities.CashReceipt.SequentialNumber;
                local.Group.Update.HiddenCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;
                UseFnEabCollFormatDetailLine3();
                local.AdjustLine.Flag = "";
              }

              local.SkipProcessing.Flag = "N";
            }
          }
        }

ReadEach2:

        export.Group.Index = -1;
        export.Group.Count = 0;

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          export.Group.Index = local.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.ListScreenWorkArea.TextLine76 =
            local.Group.Item.ListScreenWorkArea.TextLine76;
          export.Group.Update.HiddenLineType.Flag =
            local.Group.Item.HiddenLineType.Flag;
          export.Group.Update.HiddenCashReceipt.SequentialNumber =
            local.Group.Item.HiddenCashReceipt.SequentialNumber;
          export.Group.Update.HiddenCashReceiptDetail.Assign(
            local.Group.Item.HiddenCashReceiptDetail);
          export.Group.Update.HiddenCashReceiptSourceType.
            SystemGeneratedIdentifier =
              local.Group.Item.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier;
          export.Group.Update.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
            local.Group.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
          export.Group.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
            local.Group.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
          export.Group.Update.HiddenCsePerson.Number =
            local.Group.Item.HiddenChild.Number;
          export.Group.Update.HiddenObligation.SystemGeneratedIdentifier =
            local.Group.Item.HiddenObligation.SystemGeneratedIdentifier;
          export.Group.Update.HiddenObligationType.SystemGeneratedIdentifier =
            local.Group.Item.HiddenObligationType.SystemGeneratedIdentifier;
          export.Group.Update.HiddenObligationTransaction.
            SystemGeneratedIdentifier =
              local.Group.Item.HiddenObligationTransaction.
              SystemGeneratedIdentifier;
          export.Group.Update.HiddenColl.Date =
            local.Group.Item.HiddenCollDate.Date;
          MoveCollection3(local.Group.Item.HiddenCollection,
            export.Group.Update.HiddenCollection);
          export.Group.Update.HiddenDebtDetail.DueDt =
            local.Group.Item.HiddenDebtDetail.DueDt;

          if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
          {
            if (export.PageKey.Index + 1 == Export.PageKeyGroup.Capacity)
            {
              ExitState = "ACO_NE0000_MAX_DISPLAY_CAPAB";

              break;
            }

            ++export.PageKey.Index;
            export.PageKey.CheckSize();

            export.PageKey.Update.KeyCashReceipt.SequentialNumber =
              local.Group.Item.HiddenCashReceipt.SequentialNumber;
            MoveCashReceiptDetail3(local.Group.Item.HiddenCashReceiptDetail,
              export.PageKey.Update.KeyCashReceiptDetail);
            export.PageKey.Update.KeyCashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.Group.Item.HiddenCashReceiptSourceType.
                SystemGeneratedIdentifier;
            export.PageKey.Update.KeyCashReceiptEvent.
              SystemGeneratedIdentifier =
                local.Group.Item.HiddenCashReceiptEvent.
                SystemGeneratedIdentifier;
            export.PageKey.Update.KeyCashReceiptType.SystemGeneratedIdentifier =
              local.Group.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
            MoveCollection3(local.Group.Item.HiddenCollection,
              export.Group.Update.HiddenCollection);
            MoveCashReceiptDetail3(local.Group.Item.HiddenCashReceiptDetail,
              export.PageKey.Update.KeyCashReceiptDetail);
            MoveCollection3(local.Group.Item.HiddenCollection,
              export.PageKey.Update.KeyCollection);
            export.PageKey.Update.AdjustentLine.Flag =
              local.Group.Item.AdjustmentLine.Flag;

            break;
          }
        }

        local.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Minus.Text1 = "-";

          if (export.Group.IsFull)
          {
            export.Plus.Text1 = "+";
            export.HiddenMoreDataInDb.Flag = "Y";

            if (Equal(global.Command, "1099"))
            {
            }
          }
          else
          {
            export.Plus.Text1 = "";
          }
        }

        break;
      case "PREV":
        if (IsEmpty(export.Minus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Standard.PageNumber;

        if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        export.PageKey.Index = export.Standard.PageNumber - 1;
        export.PageKey.CheckSize();

        MoveCashReceiptDetail3(export.PageKey.Item.KeyCashReceiptDetail,
          export.HiddenDlgflwCashReceiptDetail);
        export.HiddenDlgflwCashReceipt.SequentialNumber =
          export.PageKey.Item.KeyCashReceipt.SequentialNumber;
        export.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier =
          export.PageKey.Item.KeyCashReceiptSourceType.
            SystemGeneratedIdentifier;
        export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier =
          export.PageKey.Item.KeyCashReceiptEvent.SystemGeneratedIdentifier;
        export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier =
          export.PageKey.Item.KeyCashReceiptType.SystemGeneratedIdentifier;

        // *** Initialize subscript of local group view that will
        // be populated with detail lines via an EAB ***
        local.Group.Index = -1;
        local.Group.Count = 0;
        local.SkipProcessing.Flag = "Y";

        // *** READ EACH for selection list. ***
        foreach(var item in ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt())
          
        {
          // 09/13/12 GVandy  CQ35548  Do not display FDSO payments if security 
          // profile restriction is FTIE.
          if (AsChar(local.FtieRestriction.Flag) == 'Y' && Equal
            (entities.CashReceiptSourceType.Code, "FDSO"))
          {
            continue;
          }

          export.HiddenColl.Date = entities.CashReceiptDetail.CollectionDate;

          // Madhu added this line on 3rd of August.
          if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
          {
            break;
          }

          // *****************************************************************
          // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
          // deleted by REIP.
          ReadCashReceiptDetailStatHistory();

          if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
            "REIPDELETE"))
          {
            continue;
          }

          // *****************************************************************
          if (!Equal(entities.CashReceiptDetail.CreatedTmst,
            export.PageKey.Item.KeyCashReceiptDetail.CreatedTmst) || entities
            .CashReceipt.SequentialNumber != export
            .PageKey.Item.KeyCashReceipt.SequentialNumber || entities
            .CashReceiptEvent.SystemGeneratedIdentifier != export
            .PageKey.Item.KeyCashReceiptEvent.SystemGeneratedIdentifier || entities
            .CashReceiptSourceType.SystemGeneratedIdentifier != export
            .PageKey.Item.KeyCashReceiptSourceType.SystemGeneratedIdentifier ||
            entities.CashReceiptType.SystemGeneratedIdentifier != export
            .PageKey.Item.KeyCashReceiptType.SystemGeneratedIdentifier || !
            Lt(local.BlankDate.Timestamp,
            export.PageKey.Item.KeyCashReceiptDetail.CreatedTmst))
          {
            local.SkipProcessing.Flag = "N";
          }

          if (AsChar(local.SkipProcessing.Flag) == 'N')
          {
            ++local.Group.Index;
            local.Group.CheckSize();

            export.HiddenDlgflwCashReceipt.Assign(entities.CashReceipt);
            MoveCashReceiptDetail4(entities.CashReceiptDetail,
              local.CollectionLine.CcashReceiptDetail);
            MoveCashReceiptDetail1(entities.CashReceiptDetail,
              export.HiddenDlgflwCashReceiptDetail);
            local.CollectionLine.CcashReceiptEvent.ReceivedDate =
              entities.CashReceiptEvent.ReceivedDate;
            export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier =
              entities.CashReceiptType.SystemGeneratedIdentifier;
            local.Group.Update.HiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.CashReceiptEvent.SystemGeneratedIdentifier;
            export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier =
              entities.CashReceiptEvent.SystemGeneratedIdentifier;
            MoveCashReceiptDetail1(entities.CashReceiptDetail,
              export.HiddenDlgflwCashReceiptDetail);
            local.Group.Update.HiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.CashReceiptEvent.SystemGeneratedIdentifier;
            local.Group.Update.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier =
                entities.CashReceiptSourceType.SystemGeneratedIdentifier;
            MoveCashReceiptDetail2(entities.CashReceiptDetail,
              local.Group.Update.HiddenCashReceiptDetail);
            local.Group.Update.HiddenCashReceipt.SequentialNumber =
              entities.CashReceipt.SequentialNumber;
            local.Group.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
              entities.CashReceiptType.SystemGeneratedIdentifier;

            // *** Save identifier ***
            MoveCashReceiptDetail2(entities.CashReceiptDetail,
              local.Group.Update.HiddenCashReceiptDetail);
            local.CollectionLine.CcashReceiptSourceType.Code =
              entities.CashReceiptSourceType.Code;

            // 09/13/12 GVandy  CQ35548  Do not display Cash Receipt Source Type
            // if security profile restriction is FTIR.
            if (AsChar(local.FtirRestriction.Flag) == 'Y')
            {
              local.CollectionLine.CcashReceiptSourceType.Code = "";
            }

            // *** Save identifier ***
            local.Group.Update.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier =
                entities.CashReceiptSourceType.SystemGeneratedIdentifier;

            // *** Calculate Undistributed Amount ***
            if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) !=
              'Y')
            {
              local.CollectionLine.Cundst.TotalCurrency =
                entities.CashReceiptDetail.CollectionAmount - (
                  entities.CashReceiptDetail.RefundedAmount.
                  GetValueOrDefault() + entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault());

              // *****************************************************************
              // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
              // deleted by REIP.
              local.CollectionLine.CcashReceiptDetailStatHistory.ReasonCodeId =
                entities.CashReceiptDetailStatHistory.ReasonCodeId;

              // *****************************************************************
            }

            // *** Call EAB to format detail line.  Set
            // local work field to indicate type of line. ***
            local.Group.Update.HiddenLineType.Flag =
              local.HardcodeCollectionLineType.Flag;

            // collection
            UseFnEabCollFormatDetailLine1();
          }

          // *** If Show Collection Only Indicator = Y, read next item ***
          if (AsChar(export.ShowColl.Flag) == 'Y')
          {
            // *** Read next cash receipt ***
          }
          else
          {
            // Madhu moved it inside the else statement.
            if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
            {
              break;
            }

            // Madhu changed this subscript  to one to obtain a blank  line 
            // here.
            // ***---  included Obligation_Type and Debt_Detail in Read Each - b
            // adams  -  6/21/99
            // *** Get activity for each Cash Receipt.  Read DISTINCT on 
            // Collection. ***
            if (local.Group.Index == -1)
            {
              local.AdjustLine.Flag = export.PageKey.Item.AdjustentLine.Flag;
            }

            foreach(var item1 in ReadCollectionObligationTransactionObligationObligationType())
              
            {
              if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
              {
                goto ReadEach3;
              }

              if (Lt(export.PageKey.Item.KeyCollection.CreatedTmst,
                entities.Collection.CreatedTmst) && Lt
                (local.BlankDate.Timestamp,
                export.PageKey.Item.KeyCollection.CreatedTmst))
              {
                continue;
              }

              if (Equal(entities.Collection.CreatedTmst,
                export.PageKey.Item.KeyCollection.CreatedTmst) && Lt
                (local.BlankDate.Timestamp,
                export.PageKey.Item.KeyCollection.CreatedTmst) && entities
                .Collection.SystemGeneratedIdentifier > export
                .PageKey.Item.KeyCollection.SystemGeneratedIdentifier)
              {
                continue;
              }

              if (AsChar(local.AdjustLine.Flag) != 'Y')
              {
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptEvent.SystemGeneratedIdentifier;
                local.Group.Update.HiddenCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptSourceType.SystemGeneratedIdentifier;
                MoveCashReceiptDetail2(entities.CashReceiptDetail,
                  local.Group.Update.HiddenCashReceiptDetail);
                local.Group.Update.HiddenCashReceipt.SequentialNumber =
                  entities.CashReceipt.SequentialNumber;
                local.Group.Update.HiddenCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;

                // *** Save identifiers ***
                MoveObligation(entities.Obligation,
                  local.Group.Update.HiddenObligation);
                local.ActivityLine.Aobligation.PrimarySecondaryCode =
                  entities.Obligation.PrimarySecondaryCode;
                local.Group.Update.HiddenObligationTransaction.
                  SystemGeneratedIdentifier =
                    entities.ObligationTransaction.SystemGeneratedIdentifier;
                MoveCollection1(entities.Collection,
                  local.ActivityLine.Acollection);
                MoveCollection2(entities.Collection,
                  local.Group.Update.HiddenCollection);
                local.ActivityLine.AobligationType.Code =
                  entities.ObligationType.Code;
                MoveObligationType(entities.ObligationType,
                  local.Group.Update.HiddenObligationType);
                local.ActivityLine.AdebtDetail.DueDt =
                  entities.DebtDetail.DueDt;
                local.Group.Update.HiddenDebtDetail.DueDt =
                  entities.DebtDetail.DueDt;
                local.ActivityLine.AautoManualDist.Flag =
                  entities.Collection.DistributionMethod;
                local.ActivityLine.Acollection.AppliedToCode =
                  entities.Collection.AppliedToOrderTypeCode;
                local.ActivityLine.AdetailProcessDat.Date =
                  Date(entities.Collection.CreatedTmst);
                local.Group.Update.HiddenCollDate.Date =
                  local.ActivityLine.AdetailProcessDat.Date;

                // *** Set Amount Applied ***
                local.ActivityLine.Acollection.Amount =
                  entities.Collection.Amount;
                local.ActivityLine.Acollection.AppliedToCode =
                  entities.Collection.AppliedToCode;
                local.ActivityLine.Acollection.ProgramAppliedTo =
                  entities.Collection.ProgramAppliedTo;

                if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'N'
                  )
                {
                  // *** If the supported indicator on the Obligation Type is "
                  // N",
                  // there is no supported person related to this obligation.  
                  // Need to blank out any previous supported person date.  Also
                  // need to read for Ob Assignment. ***
                  local.ActivityLine.Achild.FormattedName = "";

                  if (ReadObligationAssignmentServiceProviderOfficeServiceProvider())
                    
                  {
                    local.ActivityLine.AserviceProvider.UserId =
                      entities.ServiceProvider.UserId;
                  }
                  else
                  {
                    local.ActivityLine.AserviceProvider.UserId = "";
                  }
                }
                else if (ReadCsePerson1())
                {
                  local.CsePersonsWorkSet.Number = entities.Child.Number;
                  local.Group.Update.HiddenChild.Number = entities.Child.Number;
                  local.Pass.Number = entities.Child.Number;
                  UseSiReadCsePerson1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  // ***  Call Action Block to get Worker ID
                  UseFnDetCaseNoAndWrkrForDbt();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    local.ActivityLine.AserviceProvider.UserId = "";
                    ExitState = "ACO_NN0000_ALL_OK";
                  }
                }
                else
                {
                  ExitState = "OE0056_NF_CHILD_CSE_PERSON";

                  // ******	NOT FOUND condition will result in program abort. 
                  // ******
                  return;
                }

                local.ActivityLine.Aprogram.Code =
                  entities.Collection.ProgramAppliedTo;

                // *** Call EAB to format detail line.  Set
                // local work field to indicate type of line. ***
                local.Group.Update.HiddenLineType.Flag =
                  local.HardcodeActivityLineType.Flag;

                // activity
                UseFnEabCollFormatDetailLine2();
                local.AdjustLine.Flag = "";
              }

              if (AsChar(entities.Collection.AdjustedInd) == 'Y')
              {
                // Moved into the if by Madhu
                if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
                {
                  goto ReadEach3;
                }

                ++local.Group.Index;
                local.Group.CheckSize();

                if (ReadCollectionAdjustmentReason())
                {
                  local.AdjLineAdjRsn.Code =
                    entities.CollectionAdjustmentReason.Code;
                }

                MoveObligation(entities.Obligation,
                  local.Group.Update.HiddenObligation);
                local.Group.Update.HiddenObligationTransaction.
                  SystemGeneratedIdentifier =
                    entities.ObligationTransaction.SystemGeneratedIdentifier;
                MoveCollection2(entities.Collection,
                  local.Group.Update.HiddenCollection);
                MoveObligationType(entities.ObligationType,
                  local.Group.Update.HiddenObligationType);
                local.Group.Update.HiddenDebtDetail.DueDt =
                  entities.DebtDetail.DueDt;
                local.Group.Update.HiddenCollDate.Date =
                  local.ActivityLine.AdetailProcessDat.Date;
                local.AdjLineCollection.Amount = -entities.Collection.Amount;
                local.AdjLineCollection.CollectionAdjustmentDt =
                  entities.Collection.CollectionAdjustmentDt;
                local.AdjLineCollection.LastUpdatedBy =
                  entities.Collection.LastUpdatedBy;

                // *** Call EAB to format detail line.  Set
                // local work field to indicate type of line. ***
                local.Group.Update.HiddenLineType.Flag = "D";
                local.Group.Update.AdjustmentLine.Flag = "Y";
                local.Group.Update.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptEvent.SystemGeneratedIdentifier;
                local.Group.Update.HiddenCashReceiptSourceType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptSourceType.SystemGeneratedIdentifier;
                MoveCashReceiptDetail2(entities.CashReceiptDetail,
                  local.Group.Update.HiddenCashReceiptDetail);
                local.Group.Update.HiddenCashReceipt.SequentialNumber =
                  entities.CashReceipt.SequentialNumber;
                local.Group.Update.HiddenCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;

                // *** Call EAB to format detail line.  Set
                // local work field to indicate type of line. ***
                local.Group.Update.HiddenLineType.Flag = "D";
                UseFnEabCollFormatDetailLine3();
                local.AdjustLine.Flag = "";
              }

              local.SkipProcessing.Flag = "N";
            }
          }
        }

ReadEach3:

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          export.Group.Index = local.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.ListScreenWorkArea.TextLine76 =
            local.Group.Item.ListScreenWorkArea.TextLine76;
          export.Group.Update.HiddenLineType.Flag =
            local.Group.Item.HiddenLineType.Flag;
          export.Group.Update.HiddenCashReceipt.SequentialNumber =
            local.Group.Item.HiddenCashReceipt.SequentialNumber;
          export.Group.Update.HiddenCashReceiptDetail.Assign(
            local.Group.Item.HiddenCashReceiptDetail);
          export.Group.Update.HiddenCashReceiptSourceType.
            SystemGeneratedIdentifier =
              local.Group.Item.HiddenCashReceiptSourceType.
              SystemGeneratedIdentifier;
          export.Group.Update.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
            local.Group.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
          export.Group.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
            local.Group.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
          export.Group.Update.HiddenCsePerson.Number =
            local.Group.Item.HiddenChild.Number;
          export.Group.Update.HiddenObligation.SystemGeneratedIdentifier =
            local.Group.Item.HiddenObligation.SystemGeneratedIdentifier;
          export.Group.Update.HiddenObligationType.SystemGeneratedIdentifier =
            local.Group.Item.HiddenObligationType.SystemGeneratedIdentifier;
          export.Group.Update.HiddenObligationTransaction.
            SystemGeneratedIdentifier =
              local.Group.Item.HiddenObligationTransaction.
              SystemGeneratedIdentifier;
          export.Group.Update.HiddenColl.Date =
            local.Group.Item.HiddenCollDate.Date;
          MoveCollection3(local.Group.Item.HiddenCollection,
            export.Group.Update.HiddenCollection);
          export.Group.Update.HiddenDebtDetail.DueDt =
            local.Group.Item.HiddenDebtDetail.DueDt;

          if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
          {
            break;
          }
        }

        local.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Standard.PageNumber > 1)
          {
            export.Minus.Text1 = "-";
          }
          else
          {
            export.Minus.Text1 = "";
          }

          if (export.PageKey.Count > export.Standard.PageNumber)
          {
            export.Plus.Text1 = "+";
          }
          else
          {
            export.Plus.Text1 = "";
          }
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "COLA":
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          if (AsChar(local.LinetypeOfSelectedRow.Flag) == AsChar
            (local.HardcodeCollectionLineType.Flag))
          {
            export.SelectedCashReceiptDetail.ObligorPersonNumber =
              export.Obligor.Number;
            export.CsePersonsWorkSet.Number = export.Obligor.Number;
            export.FlowToCola.Flag = "N";
            ExitState = "ECO_LNK_TO_MTN_COLL_ADJMNTS";
          }
          else
          {
            export.SelectedCashReceiptDetail.ObligorPersonNumber =
              export.Obligor.Number;
            export.CsePersonsWorkSet.Number = export.Obligor.Number;
            export.FlowToCola.Flag = "Y";
            ExitState = "ECO_LNK_TO_MTN_COLL_ADJMNTS";
          }
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "CRRC":
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          if (AsChar(local.LinetypeOfSelectedRow.Flag) == AsChar
            (local.HardcodeCollectionLineType.Flag))
          {
            export.SelectedCashReceiptDetail.ObligorPersonNumber =
              export.Obligor.Number;
            ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";
          }
          else
          {
            ExitState = "FN0000_MUST_SELECT_CASH_RECEIPT";
          }
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "CRUC":
        // *** Can flow to CRUC without Selection ***
        ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";

        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          if (AsChar(local.LinetypeOfSelectedRow.Flag) == AsChar
            (local.HardcodeCollectionLineType.Flag))
          {
            export.SelectedCashReceiptDetail.ObligorPersonNumber =
              export.Obligor.Number;
            ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";
          }
          else
          {
            ExitState = "FN0000_MUST_SELECT_CASH_RECEIPT";
          }
        }

        break;
      case "DEBT":
        // <<< Will Flow to DEBT with or without selection >>>
        // <<< If a Selection is made, then it must have line_type of "Activity 
        // Type" >>>
        ExitState = "ECO_LNK_LST_DBT_ACT_BY_AP_PYR";

        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          if (AsChar(local.LinetypeOfSelectedRow.Flag) != AsChar
            (local.HardcodeActivityLineType.Flag))
          {
            ExitState = "FN0000_MUST_BE_DEBT_ACTIVITY";
          }
          else
          {
            // ******************************************************************
            //     The lines below have been commented solution
            //  to problem report  100145.
            // ******************************************************************
            // PR00202233 - 5/8/07 mFan
            // Changed to qulify the collection record with cash receipt detail,
            // cash receipt, cash receipt event, and cash receipt source type.
            if (ReadCollectionCsePerson())
            {
              if (Equal(entities.Obligor.Number, export.Obligor.Number))
              {
              }
              else
              {
                ExitState = "FN0000_COLL_IS_CONCURRENT";
              }
            }
          }
        }

        break;
      case "LCDA":
        ExitState = "ECO_LNK_TO_LCDA";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PACC":
        // ---------------------------
        // PR80025 - 11/19/99
        // Add new flow to PACC
        // ---------------------------
        // <<< Must select a line_type of "Activity Type" >>>
        if (AsChar(local.LinetypeOfSelectedRow.Flag) != AsChar
          (local.HardcodeActivityLineType.Flag) || AsChar
          (local.ItemSelected.Flag) != 'Y')
        {
          ExitState = "FN0000_MUST_SELECT_DIST_ACTIVITY";

          break;
        }

        // PR00202233 - 5/8/07 mFan
        // Changed to qulify the collection record with cash receipt detail, 
        // cash receipt, cash receipt event, and cash receipt source type.
        if (!ReadCollection())
        {
          ExitState = "FN0000_COLLECTION_NF";

          break;
        }

        foreach(var item in ReadCsePerson2())
        {
          if (IsEmpty(export.FlowToPacc.Number))
          {
            export.FlowToPacc.Number = entities.CsePerson.Number;
          }
          else if (!Equal(entities.CsePerson.Number, export.FlowToPacc.Number))
          {
            // ------------------------------------------------------
            // PR80025 - 11/19/99
            // More than one obligee found on this collection. User must
            // flow to DEBT first to select obligee and then flow to PACC
            // from there.
            // -----------------------------------------------------
            ExitState = "FN0000_FLOW_TO_DEBT_FIRST";

            goto Test;
          }
        }

        if (IsEmpty(export.FlowToPacc.Number))
        {
          // ------------------------------------------------------
          // If collection was not disbursed, do not flow to PACC.
          // -----------------------------------------------------
          ExitState = "FN0000_COLL_NOT_DISB_FLOW_TO_DBT";

          break;
        }

        export.FlowPaccEndDate.Date = entities.Collection.CollectionDt;
        export.FlowPaccStartDate.Date =
          AddMonths(entities.Collection.CollectionDt, -1);
        UseCabFirstAndLastDateOfMonth();
        ExitState = "ECO_LNK_TO_LST_APACC";

        break;
      case "COMN":
        // -----------------------------------------------------------------
        // PR# 79177: New flow to COMN.
        //                                                 
        // ----Vithal Madhira (03/15/2000)
        // -----------------------------------------------------------------
        export.HidForComn.Number = export.Obligor.Number;
        export.HidForComn.FormattedName =
          export.CsePersonsWorkSet.FormattedName;
        ExitState = "ECO_LNK_TO_COMN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test:

    // Add any common logic that must occur at
    // the end of every pass.
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (AsChar(export.Group.Item.HiddenLineType.Flag) != 'A' && AsChar
        (export.Group.Item.HiddenLineType.Flag) != 'C')
      {
        var field = GetField(export.Group.Item.Common, "selectChar");

        field.Intensity = Intensity.Dark;
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.Group.Item.Common, "selectChar");

        field.Intensity = Intensity.Normal;
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
      }
    }

    export.Group.CheckIndex();
  }

  private static void MoveActivityLine(Local.ActivityLineGroup source,
    FnEabCollFormatDetailLine.Import.ActivityLineGroup target)
  {
    target.Obligation.PrimarySecondaryCode =
      source.Aobligation.PrimarySecondaryCode;
    target.ObligationType.Code = source.AobligationType.Code;
    target.CsePersonsWorkSet.FormattedName = source.Achild.FormattedName;
    target.Program.Code = source.Aprogram.Code;
    target.ServiceProvider.UserId = source.AserviceProvider.UserId;
    target.Collection.Assign(source.Acollection);
    target.DateWorkArea.Date = source.AdetailProcessDat.Date;
    target.AutoManualDist.Flag = source.AautoManualDist.Flag;
    target.DebtDetail.DueDt = source.AdebtDetail.DueDt;
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptDate = source.ReceiptDate;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionDate = source.CollectionDate;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionDate = source.CollectionDate;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCashReceiptDetail3(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCashReceiptDetail4(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.RefundedAmount = source.RefundedAmount;
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveCollection1(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CollectionDt = source.CollectionDt;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCollection3(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCollectionLine(Local.CollectionLineGroup source,
    FnEabCollFormatDetailLine.Import.CollectionLineGroup target)
  {
    target.CcashReceiptEvent.ReceivedDate =
      source.CcashReceiptEvent.ReceivedDate;
    MoveCashReceiptDetailStatHistory(source.CcashReceiptDetailStatHistory,
      target.CcashReceiptDetailStatHistory);
    target.CcashReceiptSourceType.Code = source.CcashReceiptSourceType.Code;
    target.Ccommon.TotalCurrency = source.Cundst.TotalCurrency;
    MoveCashReceipt(source.CcashReceipt, target.CcashReceipt);
    target.CcashReceiptDetail.Assign(source.CcashReceiptDetail);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
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
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = export.FlowPaccStartDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.FlowPaccStartDate.Date = useExport.First.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.CsePersonNumber.Text10;
    useExport.TextWorkArea.Text10 = local.CsePersonNumber.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.CsePersonNumber.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = export.Obligor.Number;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    local.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.TotalUndistAmt.TotalCurrency = useExport.UndistAmt.TotalCurrency;
  }

  private void UseFnDetCaseNoAndWrkrForDbt()
  {
    var useImport = new FnDetCaseNoAndWrkrForDbt.Import();
    var useExport = new FnDetCaseNoAndWrkrForDbt.Export();

    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.Supported.Number = entities.Child.Number;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.Obligor.Number = export.Obligor.Number;

    Call(FnDetCaseNoAndWrkrForDbt.Execute, useImport, useExport);

    local.ActivityLine.AserviceProvider.UserId =
      useExport.ServiceProvider.UserId;
  }

  private void UseFnEabCollFormatDetailLine1()
  {
    var useImport = new FnEabCollFormatDetailLine.Import();
    var useExport = new FnEabCollFormatDetailLine.Export();

    useImport.LineType.Flag = local.Group.Item.HiddenLineType.Flag;
    MoveCollectionLine(local.CollectionLine, useImport.CollectionLine);
    useExport.ListScreenWorkArea.TextLine76 =
      local.Group.Item.ListScreenWorkArea.TextLine76;

    Call(FnEabCollFormatDetailLine.Execute, useImport, useExport);

    local.Group.Update.ListScreenWorkArea.TextLine76 =
      useExport.ListScreenWorkArea.TextLine76;
  }

  private void UseFnEabCollFormatDetailLine2()
  {
    var useImport = new FnEabCollFormatDetailLine.Import();
    var useExport = new FnEabCollFormatDetailLine.Export();

    useImport.LineType.Flag = local.Group.Item.HiddenLineType.Flag;
    MoveActivityLine(local.ActivityLine, useImport.ActivityLine);
    useExport.ListScreenWorkArea.TextLine76 =
      local.Group.Item.ListScreenWorkArea.TextLine76;

    Call(FnEabCollFormatDetailLine.Execute, useImport, useExport);

    local.Group.Update.ListScreenWorkArea.TextLine76 =
      useExport.ListScreenWorkArea.TextLine76;
  }

  private void UseFnEabCollFormatDetailLine3()
  {
    var useImport = new FnEabCollFormatDetailLine.Import();
    var useExport = new FnEabCollFormatDetailLine.Export();

    useImport.AdjLineAdjRsn.Code = local.AdjLineAdjRsn.Code;
    useImport.AdjLineCollAdj.Assign(local.AdjLineCollection);
    useImport.LineType.Flag = local.Group.Item.HiddenLineType.Flag;
    useExport.ListScreenWorkArea.TextLine76 =
      local.Group.Item.ListScreenWorkArea.TextLine76;

    Call(FnEabCollFormatDetailLine.Execute, useImport, useExport);

    local.Group.Update.ListScreenWorkArea.TextLine76 =
      useExport.ListScreenWorkArea.TextLine76;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.Hardcode.Type1 = useExport.CpaObligor.Type1;
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

    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    useImport.CsePerson.Number = import.Obligor.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScCheckProfileRestrictions()
  {
    var useImport = new ScCheckProfileRestrictions.Import();
    var useExport = new ScCheckProfileRestrictions.Export();

    Call(ScCheckProfileRestrictions.Execute, useImport, useExport);

    local.Profile.Assign(useExport.Profile);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Pass.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ActivityLine.Achild.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt()
  {
    entities.CashReceiptType.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          export.HiddenDlgflwCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId",
          export.HiddenDlgflwCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crtIdentifier",
          export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crSrceTypeId",
          export.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "creventId",
          export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 8);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 9);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 11);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 12);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 13);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 14);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 15);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 16);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 17);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 18);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 20);
        entities.CashReceiptType.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistory",
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
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId",
          export.SelectedCollection.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crdId",
          export.HiddenDlgflwCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId",
          export.HiddenDlgflwCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.Hardcode.Type1);
        db.SetString(command, "cspNumber", export.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.CarId = db.GetNullableInt32(reader, 16);
        entities.Collection.OtyId = db.GetInt32(reader, 17);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 18);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Collection.Amount = db.GetDecimal(reader, 21);
        entities.Collection.DistributionMethod = db.GetString(reader, 22);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 23);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 24);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 25);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
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
          command, "obTrnRlnRsnId",
          entities.Collection.CarId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCollectionCsePerson()
  {
    entities.Obligor.Populated = false;
    entities.Collection.Populated = false;

    return Read("ReadCollectionCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId",
          export.SelectedCollection.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crdId",
          export.HiddenDlgflwCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId",
          export.HiddenDlgflwCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          export.HiddenDlgflwCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.HiddenDlgflwCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          export.HiddenDlgflwCashReceiptType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.Hardcode.Type1);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Obligor.Number = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.CarId = db.GetNullableInt32(reader, 16);
        entities.Collection.OtyId = db.GetInt32(reader, 17);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 18);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Collection.Amount = db.GetDecimal(reader, 21);
        entities.Collection.DistributionMethod = db.GetString(reader, 22);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 23);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 24);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 25);
        entities.Obligor.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
      });
  }

  private IEnumerable<bool>
    ReadCollectionObligationTransactionObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.Collection.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach(
      "ReadCollectionObligationTransactionObligationObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 12);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 15);
        entities.DebtDetail.OtrType = db.GetString(reader, 15);
        entities.Collection.CarId = db.GetNullableInt32(reader, 16);
        entities.Collection.OtyId = db.GetInt32(reader, 17);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 17);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 17);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 17);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 18);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Collection.Amount = db.GetDecimal(reader, 21);
        entities.Collection.DistributionMethod = db.GetString(reader, 22);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 23);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 24);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 25);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 26);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 27);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 28);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 29);
        entities.ObligationType.Code = db.GetString(reader, 30);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 31);
        entities.DebtDetail.DueDt = db.GetDate(reader, 32);
        entities.DebtDetail.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.Collection.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.ObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Child.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ObligationTransaction.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.Child.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
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
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadObligationAssignmentServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignmentServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 4);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 7);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 8);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 9);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 10);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 11);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 11);
        entities.ServiceProvider.UserId = db.GetString(reader, 12);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
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
      /// A value of HiddenDebtDetail.
      /// </summary>
      [JsonPropertyName("hiddenDebtDetail")]
      public DebtDetail HiddenDebtDetail
      {
        get => hiddenDebtDetail ??= new();
        set => hiddenDebtDetail = value;
      }

      /// <summary>
      /// A value of HiddenCollection.
      /// </summary>
      [JsonPropertyName("hiddenCollection")]
      public Collection HiddenCollection
      {
        get => hiddenCollection ??= new();
        set => hiddenCollection = value;
      }

      /// <summary>
      /// A value of HiddenObligationType.
      /// </summary>
      [JsonPropertyName("hiddenObligationType")]
      public ObligationType HiddenObligationType
      {
        get => hiddenObligationType ??= new();
        set => hiddenObligationType = value;
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
      /// A value of HiddenCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptSourceType")]
      public CashReceiptSourceType HiddenCashReceiptSourceType
      {
        get => hiddenCashReceiptSourceType ??= new();
        set => hiddenCashReceiptSourceType = value;
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
      /// A value of HiddenCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptDetail")]
      public CashReceiptDetail HiddenCashReceiptDetail
      {
        get => hiddenCashReceiptDetail ??= new();
        set => hiddenCashReceiptDetail = value;
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
      /// A value of HiddenObligation.
      /// </summary>
      [JsonPropertyName("hiddenObligation")]
      public Obligation HiddenObligation
      {
        get => hiddenObligation ??= new();
        set => hiddenObligation = value;
      }

      /// <summary>
      /// A value of HiddenObligationTransaction.
      /// </summary>
      [JsonPropertyName("hiddenObligationTransaction")]
      public ObligationTransaction HiddenObligationTransaction
      {
        get => hiddenObligationTransaction ??= new();
        set => hiddenObligationTransaction = value;
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
      /// A value of HiddenLineType.
      /// </summary>
      [JsonPropertyName("hiddenLineType")]
      public Common HiddenLineType
      {
        get => hiddenLineType ??= new();
        set => hiddenLineType = value;
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
      /// A value of HiddenColl.
      /// </summary>
      [JsonPropertyName("hiddenColl")]
      public DateWorkArea HiddenColl
      {
        get => hiddenColl ??= new();
        set => hiddenColl = value;
      }

      /// <summary>
      /// A value of ListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("listScreenWorkArea")]
      public ListScreenWorkArea ListScreenWorkArea
      {
        get => listScreenWorkArea ??= new();
        set => listScreenWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private DebtDetail hiddenDebtDetail;
      private Collection hiddenCollection;
      private ObligationType hiddenObligationType;
      private CashReceiptType hiddenCashReceiptType;
      private CashReceiptSourceType hiddenCashReceiptSourceType;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CashReceiptDetail hiddenCashReceiptDetail;
      private CashReceipt hiddenCashReceipt;
      private Obligation hiddenObligation;
      private ObligationTransaction hiddenObligationTransaction;
      private CsePerson hiddenCsePerson;
      private Common hiddenLineType;
      private Common common;
      private DateWorkArea hiddenColl;
      private ListScreenWorkArea listScreenWorkArea;
    }

    /// <summary>A PageKeyGroup group.</summary>
    [Serializable]
    public class PageKeyGroup
    {
      /// <summary>
      /// A value of KeyCollection.
      /// </summary>
      [JsonPropertyName("keyCollection")]
      public Collection KeyCollection
      {
        get => keyCollection ??= new();
        set => keyCollection = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptType.
      /// </summary>
      [JsonPropertyName("keyCashReceiptType")]
      public CashReceiptType KeyCashReceiptType
      {
        get => keyCashReceiptType ??= new();
        set => keyCashReceiptType = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("keyCashReceiptSourceType")]
      public CashReceiptSourceType KeyCashReceiptSourceType
      {
        get => keyCashReceiptSourceType ??= new();
        set => keyCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("keyCashReceiptEvent")]
      public CashReceiptEvent KeyCashReceiptEvent
      {
        get => keyCashReceiptEvent ??= new();
        set => keyCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("keyCashReceiptDetail")]
      public CashReceiptDetail KeyCashReceiptDetail
      {
        get => keyCashReceiptDetail ??= new();
        set => keyCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of KeyCashReceipt.
      /// </summary>
      [JsonPropertyName("keyCashReceipt")]
      public CashReceipt KeyCashReceipt
      {
        get => keyCashReceipt ??= new();
        set => keyCashReceipt = value;
      }

      /// <summary>
      /// A value of AdjustmentLine.
      /// </summary>
      [JsonPropertyName("adjustmentLine")]
      public Common AdjustmentLine
      {
        get => adjustmentLine ??= new();
        set => adjustmentLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 190;

      private Collection keyCollection;
      private CashReceiptType keyCashReceiptType;
      private CashReceiptSourceType keyCashReceiptSourceType;
      private CashReceiptEvent keyCashReceiptEvent;
      private CashReceiptDetail keyCashReceiptDetail;
      private CashReceipt keyCashReceipt;
      private Common adjustmentLine;
    }

    /// <summary>
    /// A value of HiddenMoreDataInDb.
    /// </summary>
    [JsonPropertyName("hiddenMoreDataInDb")]
    public Common HiddenMoreDataInDb
    {
      get => hiddenMoreDataInDb ??= new();
      set => hiddenMoreDataInDb = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptEvent")]
    public CashReceiptEvent HiddenDlgflwCashReceiptEvent
    {
      get => hiddenDlgflwCashReceiptEvent ??= new();
      set => hiddenDlgflwCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptType")]
    public CashReceiptType HiddenDlgflwCashReceiptType
    {
      get => hiddenDlgflwCashReceiptType ??= new();
      set => hiddenDlgflwCashReceiptType = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptSourceType")]
    public CashReceiptSourceType HiddenDlgflwCashReceiptSourceType
    {
      get => hiddenDlgflwCashReceiptSourceType ??= new();
      set => hiddenDlgflwCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptDetail")]
    public CashReceiptDetail HiddenDlgflwCashReceiptDetail
    {
      get => hiddenDlgflwCashReceiptDetail ??= new();
      set => hiddenDlgflwCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceipt.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceipt")]
    public CashReceipt HiddenDlgflwCashReceipt
    {
      get => hiddenDlgflwCashReceipt ??= new();
      set => hiddenDlgflwCashReceipt = value;
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
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public Standard CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of ShowColl.
    /// </summary>
    [JsonPropertyName("showColl")]
    public Common ShowColl
    {
      get => showColl ??= new();
      set => showColl = value;
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
    /// A value of CurrentOwed.
    /// </summary>
    [JsonPropertyName("currentOwed")]
    public Common CurrentOwed
    {
      get => currentOwed ??= new();
      set => currentOwed = value;
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
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Standard ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of HidForComn.
    /// </summary>
    [JsonPropertyName("hidForComn")]
    public CsePersonsWorkSet HidForComn
    {
      get => hidForComn ??= new();
      set => hidForComn = value;
    }

    /// <summary>
    /// A value of HiddenColl.
    /// </summary>
    [JsonPropertyName("hiddenColl")]
    public DateWorkArea HiddenColl
    {
      get => hiddenColl ??= new();
      set => hiddenColl = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// Gets a value of PageKey.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeyGroup> PageKey => pageKey ??= new(
      PageKeyGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKey for json serialization.
    /// </summary>
    [JsonPropertyName("pageKey")]
    [Computed]
    public IList<PageKeyGroup> PageKey_Json
    {
      get => pageKey;
      set => PageKey.Assign(value);
    }

    private Common hiddenMoreDataInDb;
    private CashReceiptEvent hiddenDlgflwCashReceiptEvent;
    private CashReceiptType hiddenDlgflwCashReceiptType;
    private CashReceiptSourceType hiddenDlgflwCashReceiptSourceType;
    private CashReceiptDetail hiddenDlgflwCashReceiptDetail;
    private CashReceipt hiddenDlgflwCashReceipt;
    private ScreenOwedAmounts screenOwedAmounts;
    private Standard courtOrderPrompt;
    private LegalAction legalAction;
    private CsePerson obligor;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common nextTransaction;
    private Common showColl;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common arrearsOwed;
    private Common interestOwed;
    private Common currentOwed;
    private Common totalOwed;
    private Common totalUndistAmt;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard obligorPrompt;
    private CsePersonsWorkSet hidForComn;
    private DateWorkArea hiddenColl;
    private WorkArea plus;
    private WorkArea minus;
    private Array<PageKeyGroup> pageKey;
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
      /// A value of HiddenDebtDetail.
      /// </summary>
      [JsonPropertyName("hiddenDebtDetail")]
      public DebtDetail HiddenDebtDetail
      {
        get => hiddenDebtDetail ??= new();
        set => hiddenDebtDetail = value;
      }

      /// <summary>
      /// A value of HiddenCollection.
      /// </summary>
      [JsonPropertyName("hiddenCollection")]
      public Collection HiddenCollection
      {
        get => hiddenCollection ??= new();
        set => hiddenCollection = value;
      }

      /// <summary>
      /// A value of HiddenObligationType.
      /// </summary>
      [JsonPropertyName("hiddenObligationType")]
      public ObligationType HiddenObligationType
      {
        get => hiddenObligationType ??= new();
        set => hiddenObligationType = value;
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
      /// A value of HiddenCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptSourceType")]
      public CashReceiptSourceType HiddenCashReceiptSourceType
      {
        get => hiddenCashReceiptSourceType ??= new();
        set => hiddenCashReceiptSourceType = value;
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
      /// A value of HiddenCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptDetail")]
      public CashReceiptDetail HiddenCashReceiptDetail
      {
        get => hiddenCashReceiptDetail ??= new();
        set => hiddenCashReceiptDetail = value;
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
      /// A value of HiddenObligation.
      /// </summary>
      [JsonPropertyName("hiddenObligation")]
      public Obligation HiddenObligation
      {
        get => hiddenObligation ??= new();
        set => hiddenObligation = value;
      }

      /// <summary>
      /// A value of HiddenObligationTransaction.
      /// </summary>
      [JsonPropertyName("hiddenObligationTransaction")]
      public ObligationTransaction HiddenObligationTransaction
      {
        get => hiddenObligationTransaction ??= new();
        set => hiddenObligationTransaction = value;
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
      /// A value of HiddenLineType.
      /// </summary>
      [JsonPropertyName("hiddenLineType")]
      public Common HiddenLineType
      {
        get => hiddenLineType ??= new();
        set => hiddenLineType = value;
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
      /// A value of HiddenColl.
      /// </summary>
      [JsonPropertyName("hiddenColl")]
      public DateWorkArea HiddenColl
      {
        get => hiddenColl ??= new();
        set => hiddenColl = value;
      }

      /// <summary>
      /// A value of ListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("listScreenWorkArea")]
      public ListScreenWorkArea ListScreenWorkArea
      {
        get => listScreenWorkArea ??= new();
        set => listScreenWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private DebtDetail hiddenDebtDetail;
      private Collection hiddenCollection;
      private ObligationType hiddenObligationType;
      private CashReceiptType hiddenCashReceiptType;
      private CashReceiptSourceType hiddenCashReceiptSourceType;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CashReceiptDetail hiddenCashReceiptDetail;
      private CashReceipt hiddenCashReceipt;
      private Obligation hiddenObligation;
      private ObligationTransaction hiddenObligationTransaction;
      private CsePerson hiddenCsePerson;
      private Common hiddenLineType;
      private Common common;
      private DateWorkArea hiddenColl;
      private ListScreenWorkArea listScreenWorkArea;
    }

    /// <summary>A PageKeyGroup group.</summary>
    [Serializable]
    public class PageKeyGroup
    {
      /// <summary>
      /// A value of KeyCollection.
      /// </summary>
      [JsonPropertyName("keyCollection")]
      public Collection KeyCollection
      {
        get => keyCollection ??= new();
        set => keyCollection = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptType.
      /// </summary>
      [JsonPropertyName("keyCashReceiptType")]
      public CashReceiptType KeyCashReceiptType
      {
        get => keyCashReceiptType ??= new();
        set => keyCashReceiptType = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("keyCashReceiptSourceType")]
      public CashReceiptSourceType KeyCashReceiptSourceType
      {
        get => keyCashReceiptSourceType ??= new();
        set => keyCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("keyCashReceiptEvent")]
      public CashReceiptEvent KeyCashReceiptEvent
      {
        get => keyCashReceiptEvent ??= new();
        set => keyCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of KeyCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("keyCashReceiptDetail")]
      public CashReceiptDetail KeyCashReceiptDetail
      {
        get => keyCashReceiptDetail ??= new();
        set => keyCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of KeyCashReceipt.
      /// </summary>
      [JsonPropertyName("keyCashReceipt")]
      public CashReceipt KeyCashReceipt
      {
        get => keyCashReceipt ??= new();
        set => keyCashReceipt = value;
      }

      /// <summary>
      /// A value of AdjustentLine.
      /// </summary>
      [JsonPropertyName("adjustentLine")]
      public Common AdjustentLine
      {
        get => adjustentLine ??= new();
        set => adjustentLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 190;

      private Collection keyCollection;
      private CashReceiptType keyCashReceiptType;
      private CashReceiptSourceType keyCashReceiptSourceType;
      private CashReceiptEvent keyCashReceiptEvent;
      private CashReceiptDetail keyCashReceiptDetail;
      private CashReceipt keyCashReceipt;
      private Common adjustentLine;
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
    /// A value of FlowPaccStartDate.
    /// </summary>
    [JsonPropertyName("flowPaccStartDate")]
    public DateWorkArea FlowPaccStartDate
    {
      get => flowPaccStartDate ??= new();
      set => flowPaccStartDate = value;
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
    /// A value of PassToDebtActTo.
    /// </summary>
    [JsonPropertyName("passToDebtActTo")]
    public DateWorkArea PassToDebtActTo
    {
      get => passToDebtActTo ??= new();
      set => passToDebtActTo = value;
    }

    /// <summary>
    /// A value of SelectedCollection.
    /// </summary>
    [JsonPropertyName("selectedCollection")]
    public Collection SelectedCollection
    {
      get => selectedCollection ??= new();
      set => selectedCollection = value;
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
    /// A value of HiddenMoreDataInDb.
    /// </summary>
    [JsonPropertyName("hiddenMoreDataInDb")]
    public Common HiddenMoreDataInDb
    {
      get => hiddenMoreDataInDb ??= new();
      set => hiddenMoreDataInDb = value;
    }

    /// <summary>
    /// A value of PassToDebtActFrom.
    /// </summary>
    [JsonPropertyName("passToDebtActFrom")]
    public DateWorkArea PassToDebtActFrom
    {
      get => passToDebtActFrom ??= new();
      set => passToDebtActFrom = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptEvent")]
    public CashReceiptEvent HiddenDlgflwCashReceiptEvent
    {
      get => hiddenDlgflwCashReceiptEvent ??= new();
      set => hiddenDlgflwCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptType")]
    public CashReceiptType HiddenDlgflwCashReceiptType
    {
      get => hiddenDlgflwCashReceiptType ??= new();
      set => hiddenDlgflwCashReceiptType = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptSourceType")]
    public CashReceiptSourceType HiddenDlgflwCashReceiptSourceType
    {
      get => hiddenDlgflwCashReceiptSourceType ??= new();
      set => hiddenDlgflwCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceiptDetail")]
    public CashReceiptDetail HiddenDlgflwCashReceiptDetail
    {
      get => hiddenDlgflwCashReceiptDetail ??= new();
      set => hiddenDlgflwCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of HiddenDlgflwCashReceipt.
    /// </summary>
    [JsonPropertyName("hiddenDlgflwCashReceipt")]
    public CashReceipt HiddenDlgflwCashReceipt
    {
      get => hiddenDlgflwCashReceipt ??= new();
      set => hiddenDlgflwCashReceipt = value;
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
    /// A value of CourtOrderPrompt.
    /// </summary>
    [JsonPropertyName("courtOrderPrompt")]
    public Standard CourtOrderPrompt
    {
      get => courtOrderPrompt ??= new();
      set => courtOrderPrompt = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of ShowColl.
    /// </summary>
    [JsonPropertyName("showColl")]
    public Common ShowColl
    {
      get => showColl ??= new();
      set => showColl = value;
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
    /// A value of CurrentOwed.
    /// </summary>
    [JsonPropertyName("currentOwed")]
    public Common CurrentOwed
    {
      get => currentOwed ??= new();
      set => currentOwed = value;
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
    /// A value of SelectedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptDetail")]
    public CashReceiptDetail SelectedCashReceiptDetail
    {
      get => selectedCashReceiptDetail ??= new();
      set => selectedCashReceiptDetail = value;
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
    /// A value of SelectedObligation.
    /// </summary>
    [JsonPropertyName("selectedObligation")]
    public Obligation SelectedObligation
    {
      get => selectedObligation ??= new();
      set => selectedObligation = value;
    }

    /// <summary>
    /// A value of SelectedObligationType.
    /// </summary>
    [JsonPropertyName("selectedObligationType")]
    public ObligationType SelectedObligationType
    {
      get => selectedObligationType ??= new();
      set => selectedObligationType = value;
    }

    /// <summary>
    /// A value of SelectedObligationTransaction.
    /// </summary>
    [JsonPropertyName("selectedObligationTransaction")]
    public ObligationTransaction SelectedObligationTransaction
    {
      get => selectedObligationTransaction ??= new();
      set => selectedObligationTransaction = value;
    }

    /// <summary>
    /// A value of SelectedChild.
    /// </summary>
    [JsonPropertyName("selectedChild")]
    public CsePerson SelectedChild
    {
      get => selectedChild ??= new();
      set => selectedChild = value;
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
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Standard ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of HidForComn.
    /// </summary>
    [JsonPropertyName("hidForComn")]
    public CsePersonsWorkSet HidForComn
    {
      get => hidForComn ??= new();
      set => hidForComn = value;
    }

    /// <summary>
    /// A value of HiddenColl.
    /// </summary>
    [JsonPropertyName("hiddenColl")]
    public DateWorkArea HiddenColl
    {
      get => hiddenColl ??= new();
      set => hiddenColl = value;
    }

    /// <summary>
    /// Gets a value of PageKey.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeyGroup> PageKey => pageKey ??= new(
      PageKeyGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKey for json serialization.
    /// </summary>
    [JsonPropertyName("pageKey")]
    [Computed]
    public IList<PageKeyGroup> PageKey_Json
    {
      get => pageKey;
      set => PageKey.Assign(value);
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    private DateWorkArea flowPaccEndDate;
    private DateWorkArea flowPaccStartDate;
    private CsePerson flowToPacc;
    private DateWorkArea passToDebtActTo;
    private Collection selectedCollection;
    private Common flowToCola;
    private Common hiddenMoreDataInDb;
    private DateWorkArea passToDebtActFrom;
    private CashReceiptEvent hiddenDlgflwCashReceiptEvent;
    private CashReceiptType hiddenDlgflwCashReceiptType;
    private CashReceiptSourceType hiddenDlgflwCashReceiptSourceType;
    private CashReceiptDetail hiddenDlgflwCashReceiptDetail;
    private CashReceipt hiddenDlgflwCashReceipt;
    private ScreenOwedAmounts screenOwedAmounts;
    private Standard courtOrderPrompt;
    private LegalAction legalAction;
    private CsePerson obligor;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common nextTransaction;
    private Common showColl;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common arrearsOwed;
    private Common interestOwed;
    private Common currentOwed;
    private Common totalOwed;
    private Common totalUndistAmt;
    private Array<GroupGroup> group;
    private CashReceiptType selectedCashReceiptType;
    private CashReceiptSourceType selectedCashReceiptSourceType;
    private CashReceiptEvent selectedCashReceiptEvent;
    private CashReceiptDetail selectedCashReceiptDetail;
    private CashReceipt selectedCashReceipt;
    private Obligation selectedObligation;
    private ObligationType selectedObligationType;
    private ObligationTransaction selectedObligationTransaction;
    private CsePerson selectedChild;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard obligorPrompt;
    private CsePersonsWorkSet hidForComn;
    private DateWorkArea hiddenColl;
    private Array<PageKeyGroup> pageKey;
    private WorkArea plus;
    private WorkArea minus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CollectionLineGroup group.</summary>
    [Serializable]
    public class CollectionLineGroup
    {
      /// <summary>
      /// A value of CcashReceiptEvent.
      /// </summary>
      [JsonPropertyName("ccashReceiptEvent")]
      public CashReceiptEvent CcashReceiptEvent
      {
        get => ccashReceiptEvent ??= new();
        set => ccashReceiptEvent = value;
      }

      /// <summary>
      /// A value of CcashReceiptDetailStatHistory.
      /// </summary>
      [JsonPropertyName("ccashReceiptDetailStatHistory")]
      public CashReceiptDetailStatHistory CcashReceiptDetailStatHistory
      {
        get => ccashReceiptDetailStatHistory ??= new();
        set => ccashReceiptDetailStatHistory = value;
      }

      /// <summary>
      /// A value of CcashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("ccashReceiptSourceType")]
      public CashReceiptSourceType CcashReceiptSourceType
      {
        get => ccashReceiptSourceType ??= new();
        set => ccashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of Cundst.
      /// </summary>
      [JsonPropertyName("cundst")]
      public Common Cundst
      {
        get => cundst ??= new();
        set => cundst = value;
      }

      /// <summary>
      /// A value of CcashReceipt.
      /// </summary>
      [JsonPropertyName("ccashReceipt")]
      public CashReceipt CcashReceipt
      {
        get => ccashReceipt ??= new();
        set => ccashReceipt = value;
      }

      /// <summary>
      /// A value of CcashReceiptDetail.
      /// </summary>
      [JsonPropertyName("ccashReceiptDetail")]
      public CashReceiptDetail CcashReceiptDetail
      {
        get => ccashReceiptDetail ??= new();
        set => ccashReceiptDetail = value;
      }

      private CashReceiptEvent ccashReceiptEvent;
      private CashReceiptDetailStatHistory ccashReceiptDetailStatHistory;
      private CashReceiptSourceType ccashReceiptSourceType;
      private Common cundst;
      private CashReceipt ccashReceipt;
      private CashReceiptDetail ccashReceiptDetail;
    }

    /// <summary>A ActivityLineGroup group.</summary>
    [Serializable]
    public class ActivityLineGroup
    {
      /// <summary>
      /// A value of Aobligation.
      /// </summary>
      [JsonPropertyName("aobligation")]
      public Obligation Aobligation
      {
        get => aobligation ??= new();
        set => aobligation = value;
      }

      /// <summary>
      /// A value of AobligationType.
      /// </summary>
      [JsonPropertyName("aobligationType")]
      public ObligationType AobligationType
      {
        get => aobligationType ??= new();
        set => aobligationType = value;
      }

      /// <summary>
      /// A value of Achild.
      /// </summary>
      [JsonPropertyName("achild")]
      public CsePersonsWorkSet Achild
      {
        get => achild ??= new();
        set => achild = value;
      }

      /// <summary>
      /// A value of Aprogram.
      /// </summary>
      [JsonPropertyName("aprogram")]
      public Program Aprogram
      {
        get => aprogram ??= new();
        set => aprogram = value;
      }

      /// <summary>
      /// A value of AserviceProvider.
      /// </summary>
      [JsonPropertyName("aserviceProvider")]
      public ServiceProvider AserviceProvider
      {
        get => aserviceProvider ??= new();
        set => aserviceProvider = value;
      }

      /// <summary>
      /// A value of Acollection.
      /// </summary>
      [JsonPropertyName("acollection")]
      public Collection Acollection
      {
        get => acollection ??= new();
        set => acollection = value;
      }

      /// <summary>
      /// A value of AdetailProcessDat.
      /// </summary>
      [JsonPropertyName("adetailProcessDat")]
      public DateWorkArea AdetailProcessDat
      {
        get => adetailProcessDat ??= new();
        set => adetailProcessDat = value;
      }

      /// <summary>
      /// A value of AautoManualDist.
      /// </summary>
      [JsonPropertyName("aautoManualDist")]
      public Common AautoManualDist
      {
        get => aautoManualDist ??= new();
        set => aautoManualDist = value;
      }

      /// <summary>
      /// A value of AdebtDetail.
      /// </summary>
      [JsonPropertyName("adebtDetail")]
      public DebtDetail AdebtDetail
      {
        get => adebtDetail ??= new();
        set => adebtDetail = value;
      }

      private Obligation aobligation;
      private ObligationType aobligationType;
      private CsePersonsWorkSet achild;
      private Program aprogram;
      private ServiceProvider aserviceProvider;
      private Collection acollection;
      private DateWorkArea adetailProcessDat;
      private Common aautoManualDist;
      private DebtDetail adebtDetail;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of HiddenDebtDetail.
      /// </summary>
      [JsonPropertyName("hiddenDebtDetail")]
      public DebtDetail HiddenDebtDetail
      {
        get => hiddenDebtDetail ??= new();
        set => hiddenDebtDetail = value;
      }

      /// <summary>
      /// A value of HiddenCollection.
      /// </summary>
      [JsonPropertyName("hiddenCollection")]
      public Collection HiddenCollection
      {
        get => hiddenCollection ??= new();
        set => hiddenCollection = value;
      }

      /// <summary>
      /// A value of HiddenObligationType.
      /// </summary>
      [JsonPropertyName("hiddenObligationType")]
      public ObligationType HiddenObligationType
      {
        get => hiddenObligationType ??= new();
        set => hiddenObligationType = value;
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
      /// A value of HiddenCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptSourceType")]
      public CashReceiptSourceType HiddenCashReceiptSourceType
      {
        get => hiddenCashReceiptSourceType ??= new();
        set => hiddenCashReceiptSourceType = value;
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
      /// A value of HiddenCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptDetail")]
      public CashReceiptDetail HiddenCashReceiptDetail
      {
        get => hiddenCashReceiptDetail ??= new();
        set => hiddenCashReceiptDetail = value;
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
      /// A value of HiddenObligation.
      /// </summary>
      [JsonPropertyName("hiddenObligation")]
      public Obligation HiddenObligation
      {
        get => hiddenObligation ??= new();
        set => hiddenObligation = value;
      }

      /// <summary>
      /// A value of HiddenObligationTransaction.
      /// </summary>
      [JsonPropertyName("hiddenObligationTransaction")]
      public ObligationTransaction HiddenObligationTransaction
      {
        get => hiddenObligationTransaction ??= new();
        set => hiddenObligationTransaction = value;
      }

      /// <summary>
      /// A value of HiddenChild.
      /// </summary>
      [JsonPropertyName("hiddenChild")]
      public CsePerson HiddenChild
      {
        get => hiddenChild ??= new();
        set => hiddenChild = value;
      }

      /// <summary>
      /// A value of HiddenLineType.
      /// </summary>
      [JsonPropertyName("hiddenLineType")]
      public Common HiddenLineType
      {
        get => hiddenLineType ??= new();
        set => hiddenLineType = value;
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
      /// A value of HiddenCollDate.
      /// </summary>
      [JsonPropertyName("hiddenCollDate")]
      public DateWorkArea HiddenCollDate
      {
        get => hiddenCollDate ??= new();
        set => hiddenCollDate = value;
      }

      /// <summary>
      /// A value of ListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("listScreenWorkArea")]
      public ListScreenWorkArea ListScreenWorkArea
      {
        get => listScreenWorkArea ??= new();
        set => listScreenWorkArea = value;
      }

      /// <summary>
      /// A value of AdjustmentLine.
      /// </summary>
      [JsonPropertyName("adjustmentLine")]
      public Common AdjustmentLine
      {
        get => adjustmentLine ??= new();
        set => adjustmentLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private DebtDetail hiddenDebtDetail;
      private Collection hiddenCollection;
      private ObligationType hiddenObligationType;
      private CashReceiptType hiddenCashReceiptType;
      private CashReceiptSourceType hiddenCashReceiptSourceType;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CashReceiptDetail hiddenCashReceiptDetail;
      private CashReceipt hiddenCashReceipt;
      private Obligation hiddenObligation;
      private ObligationTransaction hiddenObligationTransaction;
      private CsePerson hiddenChild;
      private Common hiddenLineType;
      private Common common;
      private DateWorkArea hiddenCollDate;
      private ListScreenWorkArea listScreenWorkArea;
      private Common adjustmentLine;
    }

    /// <summary>
    /// A value of AdjustLine.
    /// </summary>
    [JsonPropertyName("adjustLine")]
    public Common AdjustLine
    {
      get => adjustLine ??= new();
      set => adjustLine = value;
    }

    /// <summary>
    /// A value of SkipProcessing.
    /// </summary>
    [JsonPropertyName("skipProcessing")]
    public Common SkipProcessing
    {
      get => skipProcessing ??= new();
      set => skipProcessing = value;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of LinetypeOfSelectedRow.
    /// </summary>
    [JsonPropertyName("linetypeOfSelectedRow")]
    public Common LinetypeOfSelectedRow
    {
      get => linetypeOfSelectedRow ??= new();
      set => linetypeOfSelectedRow = value;
    }

    /// <summary>
    /// A value of AdjLineAdjRsn.
    /// </summary>
    [JsonPropertyName("adjLineAdjRsn")]
    public CollectionAdjustmentReason AdjLineAdjRsn
    {
      get => adjLineAdjRsn ??= new();
      set => adjLineAdjRsn = value;
    }

    /// <summary>
    /// A value of AdjLineCollection.
    /// </summary>
    [JsonPropertyName("adjLineCollection")]
    public Collection AdjLineCollection
    {
      get => adjLineCollection ??= new();
      set => adjLineCollection = value;
    }

    /// <summary>
    /// A value of CsePersonNumber.
    /// </summary>
    [JsonPropertyName("csePersonNumber")]
    public TextWorkArea CsePersonNumber
    {
      get => csePersonNumber ??= new();
      set => csePersonNumber = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePersonsWorkSet Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of EabReturn.
    /// </summary>
    [JsonPropertyName("eabReturn")]
    public AbendData EabReturn
    {
      get => eabReturn ??= new();
      set => eabReturn = value;
    }

    /// <summary>
    /// A value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public CsePersonAccount Hardcode
    {
      get => hardcode ??= new();
      set => hardcode = value;
    }

    /// <summary>
    /// A value of HardcodeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDebt")]
    public ObligationTransaction HardcodeDebt
    {
      get => hardcodeDebt ??= new();
      set => hardcodeDebt = value;
    }

    /// <summary>
    /// A value of HardcodeActivityLineType.
    /// </summary>
    [JsonPropertyName("hardcodeActivityLineType")]
    public Common HardcodeActivityLineType
    {
      get => hardcodeActivityLineType ??= new();
      set => hardcodeActivityLineType = value;
    }

    /// <summary>
    /// A value of HardcodeCollectionLineType.
    /// </summary>
    [JsonPropertyName("hardcodeCollectionLineType")]
    public Common HardcodeCollectionLineType
    {
      get => hardcodeCollectionLineType ??= new();
      set => hardcodeCollectionLineType = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
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
    /// Gets a value of CollectionLine.
    /// </summary>
    [JsonPropertyName("collectionLine")]
    public CollectionLineGroup CollectionLine
    {
      get => collectionLine ?? (collectionLine = new());
      set => collectionLine = value;
    }

    /// <summary>
    /// Gets a value of ActivityLine.
    /// </summary>
    [JsonPropertyName("activityLine")]
    public ActivityLineGroup ActivityLine
    {
      get => activityLine ?? (activityLine = new());
      set => activityLine = value;
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
    /// A value of ItemSelected.
    /// </summary>
    [JsonPropertyName("itemSelected")]
    public Common ItemSelected
    {
      get => itemSelected ??= new();
      set => itemSelected = value;
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
    /// A value of Calc.
    /// </summary>
    [JsonPropertyName("calc")]
    public Common Calc
    {
      get => calc ??= new();
      set => calc = value;
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
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public Common DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
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

    private Common adjustLine;
    private Common skipProcessing;
    private Common ftieRestriction;
    private Common ftirRestriction;
    private Profile profile;
    private DateWorkArea max;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common linetypeOfSelectedRow;
    private CollectionAdjustmentReason adjLineAdjRsn;
    private Collection adjLineCollection;
    private TextWorkArea csePersonNumber;
    private DateWorkArea current;
    private CsePersonsWorkSet pass;
    private AbendData eabReturn;
    private CsePersonAccount hardcode;
    private ObligationTransaction hardcodeDebt;
    private Common hardcodeActivityLineType;
    private Common hardcodeCollectionLineType;
    private CashReceiptDetailStatus suspended;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CollectionLineGroup collectionLine;
    private ActivityLineGroup activityLine;
    private DateWorkArea dateWorkArea;
    private Common itemSelected;
    private DateWorkArea blankDate;
    private Common calc;
    private Common undistributed;
    private Common promptCount;
    private Common delMe;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private DisbursementTransaction disbursementTransaction;
    private CsePerson csePerson;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private ObligationAssignment obligationAssignment;
    private DebtDetail debtDetail;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CsePerson child;
    private CsePerson obligor;
    private CsePersonAccount csePersonAccount;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private MonthlyObligorSummary monthlyObligorSummary;
    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}

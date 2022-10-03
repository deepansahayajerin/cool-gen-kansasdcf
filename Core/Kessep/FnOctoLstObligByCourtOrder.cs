// Program: FN_OCTO_LST_OBLIG_BY_COURT_ORDER, ID: 371739387, model: 746.
// Short name: SWEOCTOP
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
/// A program: FN_OCTO_LST_OBLIG_BY_COURT_ORDER.
/// </para>
/// <para>
/// RESP: FINANCE
/// this prad displays the obligations of a court order.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOctoLstObligByCourtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCTO_LST_OBLIG_BY_COURT_ORDER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOctoLstObligByCourtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOctoLstObligByCourtOrder.
  /// </summary>
  public FnOctoLstObligByCourtOrder(IContext context, Import import,
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
    // : List Obligations by court order
    //   Developer	Mod Date
    //   D.M. NILSEN	07/10/95
    //   H. Kennedy	09/16/96	Fixed the Return command.  the command was
    // 				being reset to display and the actual command
    // 				was never getting executed.
    // 				Changed the code so that the Legal Action
    // 				identifier could be passed to the Maintenance
    // 				screens.
    //   R. Welborn  	09/17/96      	Flow to PREL.
    //   H. Kennedy	10/14/960	Took out validation of Legal action before the 
    // display
    // 				command is hit.
    //   H. Kennedy	10/16/96	Retrofitted Data level Security
    //   H. Kennedy	01/20/97	Fixed reads. Not populating with all
    // 				Active Obligations.  Also not populating
    // 				Frequency amount.  Added logic to include
    // 				New infrasture in obtaining the Case Worker
    // 				assigned to the obligation.
    //   H. Kennedy    04/02/97        Fixes:
    //                                 
    // Added logic to disallow flow for
    //                                 
    // commands REIP RERE and PREL
    //                                 
    // when no selection is made
    //                                 
    // Protected Undist Amount field in
    //                                 
    // the group view.
    //                                 
    // Caused a more specific message
    //                                 
    // to be displayed when wrong Court
    //                                 
    // Order number is entered.
    //                                 
    // Caused Payor number to flow to
    //                                 
    // REIP and RERE (03/28/97).
    //                                 
    // Fixed so that data passes to the
    //                                 
    // primary section of PREL.
    // Siraj Konkader	10/3/97		Fix display of Screen Due Amounts which was never
    // being populated despite being painted on the screen.
    // E. Parker	10/26/98		Made undistributed amt and detail owed amts blank 
    // when zero; made PF6, PF5, PF10, and ENTER invalid commands.
    // B Adams - 7/6/99  -  Read properties set for this and all cabs
    //   packaged in OCTO.
    // *****************************************************************
    // Oct 12, 1999, pr# 75885, mbrown - Deleted code that read legal action if 
    // group view is empty and replaced it with 'no data found' message.
    // : 10/25/99, pr# 77622, M Brown - Add current owed to OCTO screen.
    // : July, 2002, PR# 140491, M. Brown
    //   On Next Tran into this screen, should not escape when there
    //   is a court order number available to use for display.
    // : 11/25/02, pr# 148011, K Doshi - Fix Screen Help
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // : Move imports to exports.
    export.AmtPrompt.Text1 = import.AmtPrompt.Text1;
    export.ScreenDueAmounts.Assign(import.ScreenDueAmounts);
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveLegalAction2(import.HiddenLegalAction, export.HiddenLegalAction);
    export.Search.Assign(import.Search);
    export.LegalActionPrompt.PromptField = import.LegalActionPrompt.PromptField;
    MoveCommon(import.ShowDeactivedObligation, export.ShowDeactivedObligation);
    export.UndistributedAmount.TotalCurrency =
      import.UndistributedAmount.TotalCurrency;
    export.Multi.Flag = import.Multi.Flag;

    if (Equal(global.Command, "PRMPTRET"))
    {
      UseFnCabValidateLegalAction();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Search, "standardNumber");

        field.Error = true;

        return;
      }

      global.Command = "DISPLAY";
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      if (IsEmpty(import.Group.Item.CsePerson.Number))
      {
        export.Group.Next();

        break;
      }

      export.Group.Update.LegalAction.Identifier =
        import.Group.Item.LegalAction.Identifier;
      export.Group.Update.ScreenObMutliSvcPrvdr.MultiServiceProviderInd =
        import.Group.Item.ScreenObMutliSvcPrvdr.MultiServiceProviderInd;
      export.Group.Update.DetailConcatInds.Text8 =
        import.Group.Item.DetailConcatInds.Text8;
      export.Group.Update.CsePerson.Number = import.Group.Item.CsePerson.Number;
      MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
        export.Group.Update.CsePersonsWorkSet);
      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.ObligationPaymentSchedule.FrequencyCode =
        import.Group.Item.ObligationPaymentSchedule.FrequencyCode;
      export.Group.Update.ObligationType.
        Assign(import.Group.Item.ObligationType);
      export.Group.Update.ScreenOwedAmounts.Assign(
        import.Group.Item.ScreenOwedAmounts);
      export.Group.Update.ServiceProvider.UserId =
        import.Group.Item.ServiceProvider.UserId;
      export.Group.Update.Obligation.Assign(import.Group.Item.Obligation);
      export.Group.Update.HiddenAmtOwedUnavl.Flag =
        import.Group.Item.HiddenAmtOwedUnavl.Flag;
      export.Group.Update.ScreenObligationStatus.ObligationStatus =
        import.Group.Item.ScreenObligationStatus.ObligationStatus;
      export.Group.Update.ScreenDueAmounts.TotalAmountDue =
        import.Group.Item.ScreenDueAmounts.TotalAmountDue;

      switch(AsChar(import.Group.Item.Common.SelectChar))
      {
        case 'S':
          ++local.SelectFound.Count;

          if (local.SelectFound.Count > 1)
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;
          }
          else
          {
            // : Move only the first one to the select field.
            export.SelectedHiddenCsePerson.Number =
              export.Group.Item.CsePerson.Number;
            export.Pass.Number = import.Group.Item.CsePerson.Number;
            export.Pass.FormattedName =
              import.Group.Item.CsePersonsWorkSet.FormattedName;
            local.Selected.Classification =
              export.Group.Item.ObligationType.Classification;
            export.SelectedHiddenObligationType.Assign(
              export.Group.Item.ObligationType);
            export.SelectedHiddenObligation.SystemGeneratedIdentifier =
              export.Group.Item.Obligation.SystemGeneratedIdentifier;
            export.HiddenLegalAction.Identifier =
              export.Group.Item.LegalAction.Identifier;
          }

          export.Group.Update.Common.SelectChar = "";

          break;
        case ' ':
          // : Spaces are OK - Continue Processing.
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          break;
      }

      export.Group.Next();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (local.SelectFound.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    if (Equal(global.Command, "BYPASS"))
    {
      return;
    }

    switch(AsChar(export.ShowDeactivedObligation.SelectChar))
    {
      case 'Y':
        break;
      case 'N':
        break;
      case ' ':
        export.ShowDeactivedObligation.SelectChar = "N";

        break;
      default:
        var field = GetField(export.ShowDeactivedObligation, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

        return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.Search.StandardNumber =
        export.HiddenNextTranInfo.StandardCrtOrdNumber ?? "";

      // : July, 2002, PR# 140491, M. Brown
      //   Should not escape when there is a court order number available to use
      // for display.
      if (IsEmpty(export.Search.StandardNumber))
      {
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

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.HiddenNextTranInfo.StandardCrtOrdNumber =
        export.Search.StandardNumber ?? "";

      if (!IsEmpty(export.SelectedHiddenCsePerson.Number))
      {
        export.HiddenNextTranInfo.CsePersonNumber =
          export.SelectedHiddenCsePerson.Number;
      }
      else
      {
        // --- Pick the first obligor
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          export.HiddenNextTranInfo.CsePersonNumber =
            export.Group.Item.CsePerson.Number;

          goto Test;
        }
      }

Test:

      export.HiddenNextTranInfo.CsePersonNumberObligor =
        export.HiddenNextTranInfo.CsePersonNumber ?? "";
      export.HiddenNextTranInfo.CsePersonNumberAp =
        export.HiddenNextTranInfo.CsePersonNumber ?? "";
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

    if (Equal(global.Command, "PRMPTRET"))
    {
    }
    else if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "RETLACN"))
    {
      if (IsEmpty(export.Search.StandardNumber))
      {
        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "OSUM") || Equal
      (global.Command, "DBWR") || Equal(global.Command, "DEBT") || Equal
      (global.Command, "COLL") || Equal(global.Command, "OCOL") || Equal
      (global.Command, "REIP") || Equal(global.Command, "RERE") || Equal
      (global.Command, "CSPM") || Equal(global.Command, "MAINT") || Equal
      (global.Command, "PREL"))
    {
    }
    else
    {
      // *****
      // to validate action level security
      // Logic to validate data level security added 10/15/96
      // *****
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ***** MAINLINE *****
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "PREL":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_PREL";

        return;
      case "REIP":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_REC_IND_PYMNT_HIST";

        return;
      case "RERE":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_REC_RCRNG_PYMNT_HIST";

        return;
      case "DISPLAY":
        if (IsEmpty(export.Search.StandardNumber) && IsEmpty
          (export.Search.CourtCaseNumber))
        {
          var field = GetField(export.Search, "standardNumber");

          field.Error = true;

          ExitState = "FN0000_MUST_ENTER_COURT_ORDER";

          return;
        }

        if (!Equal(export.HiddenLegalAction.StandardNumber,
          export.Search.StandardNumber))
        {
          export.Search.Identifier = 0;
          export.Search.CourtCaseNumber = "";
        }

        UseFnDisplayObligByCourtOrder();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveLegalAction2(export.Search, export.HiddenLegalAction);

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!IsEmpty(export.Group.Item.ScreenOwedAmounts.
              ErrorInformationLine))
            {
              var field =
                GetField(export.Group.Item.ScreenOwedAmounts,
                "errorInformationLine");

              field.Color = "red";
              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
          }

          if (export.Group.IsEmpty)
          {
            // Oct 12, 1999, pr# 75885, mbrown - Deleted code that read legal 
            // action if group view
            // is empty and replaced it with 'no data found' message. ( If 
            // exitstate is all ok, then
            // the legal action was already successfully read in the above cab).
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            return;
          }
          else if (export.Group.IsFull)
          {
            ExitState = "FN0000_MORE_DATA_EXISTS";

            return;
          }
        }
        else if (IsExitState("FN0000_COURT_ORDER_NF"))
        {
          var field = GetField(export.Search, "standardNumber");

          field.Error = true;

          return;
        }
        else if (IsExitState("LEGAL_ACTION_NF"))
        {
          var field = GetField(export.Search, "standardNumber");

          field.Error = true;

          return;
        }
        else
        {
          MoveLegalAction2(export.Search, export.HiddenLegalAction);

          return;
        }

        break;
      case "LIST":
        switch(AsChar(export.LegalActionPrompt.PromptField))
        {
          case ' ':
            break;
          case 'S':
            local.PromptCount.Count = 1;

            break;
          case '+':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.LegalActionPrompt, "promptField");

            field.Error = true;

            return;
        }

        switch(AsChar(export.AmtPrompt.Text1))
        {
          case ' ':
            break;
          case 'S':
            ++local.PromptCount.Count;

            break;
          case '+':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.AmtPrompt, "text1");

            field.Error = true;

            return;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          case 1:
            if (AsChar(export.LegalActionPrompt.PromptField) == 'S')
            {
              export.LegalActionPrompt.PromptField = "+";
              export.Search.StandardNumber = "";
              export.Search.CourtCaseNumber = "";
              export.Search.Identifier = 0;
              ExitState = "ECO_LNK_TO_LACN";
            }
            else
            {
              if (IsEmpty(export.Multi.Flag))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  export.SelectedHiddenCsePerson.Number =
                    export.Group.Item.CsePerson.Number;

                  break;
                }
              }

              export.AmtPrompt.Text1 = "+";
              ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";
            }

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        break;
      case "NEXT":
        if (!Equal(export.Search.StandardNumber,
          import.HiddenLegalAction.StandardNumber))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        break;
      case "PREV":
        if (!Equal(export.Search.StandardNumber,
          import.HiddenLegalAction.StandardNumber))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        break;
      case "MAINT":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        switch(AsChar(local.Selected.Classification))
        {
          case 'F':
            ExitState = "ECO_LNK_TO_OFEE";

            break;
          case 'V':
            ExitState = "ECO_LNK_TO_MTN_VOLUNTARY_OBLIG";

            break;
          case 'R':
            ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";

            break;
          case 'A':
            ExitState = "ECO_LNK_TO_MTN_ACCRUING_OBLIG";

            break;
          default:
            ExitState = "ECO_LNK_TO_MTN_NON_ACCRUING_OBLG";

            break;
        }

        return;
      case "OCOL":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_LST_COLL_BY_OBLIG";

        return;
      case "COLL":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_LST_COL_ACT_BY_AP_PYR";

        return;
      case "DEBT":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_LST_DBT_ACT_BY_AP_PYR";

        return;
      case "OSUM":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_DSPLY_OBLIG_SUM";

        return;
      case "DBWR":
        if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }
        else if (AsChar(export.SelectedHiddenObligationType.Classification) != 'A'
          )
        {
          ExitState = "FN0000_SEL_INVALID_FOR_DBWR";

          return;
        }

        ExitState = "ECO_LNK_TO_REC_ACCRUED_ARR_ADJ_2";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "CSPM":
        if (local.SelectFound.Count == 1)
        {
          ExitState = "ECO_LNK_MTN_OBLG_CPN_SUPRESON";

          return;
        }
        else if (local.SelectFound.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }
        else if (local.SelectFound.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (AsChar(export.Group.Item.HiddenAmtOwedUnavl.Flag) == 'Y')
      {
        var field1 =
          GetField(export.Group.Item.ScreenOwedAmounts, "currentAmountOwed");

        field1.Color = "yellow";
        field1.Intensity = Intensity.High;
        field1.Highlighting = Highlighting.ReverseVideo;
        field1.Protected = true;

        var field2 = GetField(export.Group.Item.ObligationType, "code");

        field2.Color = "yellow";
        field2.Intensity = Intensity.High;
        field2.Highlighting = Highlighting.ReverseVideo;
        field2.Protected = true;

        var field3 =
          GetField(export.Group.Item.ScreenOwedAmounts, "arrearsAmountOwed");

        field3.Color = "yellow";
        field3.Intensity = Intensity.High;
        field3.Highlighting = Highlighting.ReverseVideo;
        field3.Protected = true;

        var field4 =
          GetField(export.Group.Item.ScreenOwedAmounts, "currentAmountOwed");

        field4.Color = "yellow";
        field4.Intensity = Intensity.High;
        field4.Highlighting = Highlighting.ReverseVideo;
        field4.Protected = true;

        var field5 =
          GetField(export.Group.Item.ScreenOwedAmounts, "interestAmountOwed");

        field5.Color = "yellow";
        field5.Intensity = Intensity.High;
        field5.Highlighting = Highlighting.ReverseVideo;
        field5.Protected = true;

        var field6 =
          GetField(export.Group.Item.ScreenOwedAmounts, "totalAmountOwed");

        field6.Color = "yellow";
        field6.Intensity = Intensity.High;
        field6.Highlighting = Highlighting.ReverseVideo;
        field6.Protected = true;

        var field7 =
          GetField(export.Group.Item.ScreenDueAmounts, "totalAmountDue");

        field7.Color = "yellow";
        field7.Intensity = Intensity.High;
        field7.Highlighting = Highlighting.ReverseVideo;
        field7.Protected = true;

        ExitState = "FN0000_MOS_NOT_AVAIL_FOR_OBLIG";
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroup(FnDisplayObligByCourtOrder.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.DetailConcatInds.Text8 = source.DetailConcatInds.Text8;
    target.LegalAction.Identifier = source.LegalAction.Identifier;
    target.Common.SelectChar = source.Common.SelectChar;
    target.CsePerson.Number = source.CsePerson.Number;
    MoveCsePersonsWorkSet(source.CsePersonsWorkSet, target.CsePersonsWorkSet);
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.ScreenObligationStatus.ObligationStatus =
      source.ScreenObligationStatus.ObligationStatus;
    target.ObligationPaymentSchedule.FrequencyCode =
      source.ObligationPaymentSchedule.FrequencyCode;
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
    target.ScreenObMutliSvcPrvdr.MultiServiceProviderInd =
      source.ScreenObMutliSvcPrvdr.MultiServiceProviderInd;
    target.ScreenOwedAmounts.Assign(source.ScreenOwedAmounts);
    target.HiddenAmtOwedUnavl.Flag = source.HiddenAmtOwedUnavl.Flag;
    target.ScreenDueAmounts.TotalAmountDue =
      source.ScreenDueAmounts.TotalAmountDue;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
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

  private void UseFnCabValidateLegalAction()
  {
    var useImport = new FnCabValidateLegalAction.Import();
    var useExport = new FnCabValidateLegalAction.Export();

    useImport.LegalAction.Assign(import.Search);

    Call(FnCabValidateLegalAction.Execute, useImport, useExport);

    export.Search.Assign(useExport.LegalAction);
  }

  private void UseFnDisplayObligByCourtOrder()
  {
    var useImport = new FnDisplayObligByCourtOrder.Import();
    var useExport = new FnDisplayObligByCourtOrder.Export();

    MoveCommon(import.ShowDeactivedObligation, useImport.ShowDeactivedObligation);
      
    useImport.Search.Assign(export.Search);

    Call(FnDisplayObligByCourtOrder.Execute, useImport, useExport);

    export.Multi.Flag = useExport.MultiPayor.Flag;
    export.Search.Assign(useExport.LegalAction);
    export.ScreenDueAmounts.Assign(useExport.ScreenDueAmounts);
    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.UndistributedAmount.TotalCurrency =
      useExport.Undistributed.TotalCurrency;
    useExport.Group.CopyTo(export.Group, MoveGroup);
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

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
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

    MoveLegalAction1(import.Search, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
      /// A value of DetailConcatInds.
      /// </summary>
      [JsonPropertyName("detailConcatInds")]
      public TextWorkArea DetailConcatInds
      {
        get => detailConcatInds ??= new();
        set => detailConcatInds = value;
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
      /// A value of ScreenObligationStatus.
      /// </summary>
      [JsonPropertyName("screenObligationStatus")]
      public ScreenObligationStatus ScreenObligationStatus
      {
        get => screenObligationStatus ??= new();
        set => screenObligationStatus = value;
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
      /// A value of ObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("obligationPaymentSchedule")]
      public ObligationPaymentSchedule ObligationPaymentSchedule
      {
        get => obligationPaymentSchedule ??= new();
        set => obligationPaymentSchedule = value;
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
      /// A value of ScreenObMutliSvcPrvdr.
      /// </summary>
      [JsonPropertyName("screenObMutliSvcPrvdr")]
      public ScreenObMutliSvcPrvdr ScreenObMutliSvcPrvdr
      {
        get => screenObMutliSvcPrvdr ??= new();
        set => screenObMutliSvcPrvdr = value;
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
      /// A value of HiddenAmtOwedUnavl.
      /// </summary>
      [JsonPropertyName("hiddenAmtOwedUnavl")]
      public Common HiddenAmtOwedUnavl
      {
        get => hiddenAmtOwedUnavl ??= new();
        set => hiddenAmtOwedUnavl = value;
      }

      /// <summary>
      /// A value of ScreenDueAmounts.
      /// </summary>
      [JsonPropertyName("screenDueAmounts")]
      public ScreenDueAmounts ScreenDueAmounts
      {
        get => screenDueAmounts ??= new();
        set => screenDueAmounts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private TextWorkArea detailConcatInds;
      private LegalAction legalAction;
      private ScreenObligationStatus screenObligationStatus;
      private Common common;
      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ServiceProvider serviceProvider;
      private ScreenObMutliSvcPrvdr screenObMutliSvcPrvdr;
      private ScreenOwedAmounts screenOwedAmounts;
      private Common hiddenAmtOwedUnavl;
      private ScreenDueAmounts screenDueAmounts;
    }

    /// <summary>
    /// A value of Multi.
    /// </summary>
    [JsonPropertyName("multi")]
    public Common Multi
    {
      get => multi ??= new();
      set => multi = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of LegalActionPrompt.
    /// </summary>
    [JsonPropertyName("legalActionPrompt")]
    public Standard LegalActionPrompt
    {
      get => legalActionPrompt ??= new();
      set => legalActionPrompt = value;
    }

    /// <summary>
    /// A value of ShowDeactivedObligation.
    /// </summary>
    [JsonPropertyName("showDeactivedObligation")]
    public Common ShowDeactivedObligation
    {
      get => showDeactivedObligation ??= new();
      set => showDeactivedObligation = value;
    }

    /// <summary>
    /// A value of ScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("screenDueAmounts")]
    public ScreenDueAmounts ScreenDueAmounts
    {
      get => screenDueAmounts ??= new();
      set => screenDueAmounts = value;
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
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Common multi;
    private TextWorkArea amtPrompt;
    private Standard standard;
    private LegalAction search;
    private Standard legalActionPrompt;
    private Common showDeactivedObligation;
    private ScreenDueAmounts screenDueAmounts;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistributedAmount;
    private LegalAction hiddenLegalAction;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of DetailConcatInds.
      /// </summary>
      [JsonPropertyName("detailConcatInds")]
      public TextWorkArea DetailConcatInds
      {
        get => detailConcatInds ??= new();
        set => detailConcatInds = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of ScreenObligationStatus.
      /// </summary>
      [JsonPropertyName("screenObligationStatus")]
      public ScreenObligationStatus ScreenObligationStatus
      {
        get => screenObligationStatus ??= new();
        set => screenObligationStatus = value;
      }

      /// <summary>
      /// A value of ObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("obligationPaymentSchedule")]
      public ObligationPaymentSchedule ObligationPaymentSchedule
      {
        get => obligationPaymentSchedule ??= new();
        set => obligationPaymentSchedule = value;
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
      /// A value of ScreenObMutliSvcPrvdr.
      /// </summary>
      [JsonPropertyName("screenObMutliSvcPrvdr")]
      public ScreenObMutliSvcPrvdr ScreenObMutliSvcPrvdr
      {
        get => screenObMutliSvcPrvdr ??= new();
        set => screenObMutliSvcPrvdr = value;
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
      /// A value of HiddenAmtOwedUnavl.
      /// </summary>
      [JsonPropertyName("hiddenAmtOwedUnavl")]
      public Common HiddenAmtOwedUnavl
      {
        get => hiddenAmtOwedUnavl ??= new();
        set => hiddenAmtOwedUnavl = value;
      }

      /// <summary>
      /// A value of ScreenDueAmounts.
      /// </summary>
      [JsonPropertyName("screenDueAmounts")]
      public ScreenDueAmounts ScreenDueAmounts
      {
        get => screenDueAmounts ??= new();
        set => screenDueAmounts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private TextWorkArea detailConcatInds;
      private LegalAction legalAction;
      private Common common;
      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private ObligationType obligationType;
      private Obligation obligation;
      private ScreenObligationStatus screenObligationStatus;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ServiceProvider serviceProvider;
      private ScreenObMutliSvcPrvdr screenObMutliSvcPrvdr;
      private ScreenOwedAmounts screenOwedAmounts;
      private Common hiddenAmtOwedUnavl;
      private ScreenDueAmounts screenDueAmounts;
    }

    /// <summary>
    /// A value of Multi.
    /// </summary>
    [JsonPropertyName("multi")]
    public Common Multi
    {
      get => multi ??= new();
      set => multi = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePersonsWorkSet Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of LegalActionPrompt.
    /// </summary>
    [JsonPropertyName("legalActionPrompt")]
    public Standard LegalActionPrompt
    {
      get => legalActionPrompt ??= new();
      set => legalActionPrompt = value;
    }

    /// <summary>
    /// A value of ShowDeactivedObligation.
    /// </summary>
    [JsonPropertyName("showDeactivedObligation")]
    public Common ShowDeactivedObligation
    {
      get => showDeactivedObligation ??= new();
      set => showDeactivedObligation = value;
    }

    /// <summary>
    /// A value of ScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("screenDueAmounts")]
    public ScreenDueAmounts ScreenDueAmounts
    {
      get => screenDueAmounts ??= new();
      set => screenDueAmounts = value;
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
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// A value of SelectedHiddenCsePerson.
    /// </summary>
    [JsonPropertyName("selectedHiddenCsePerson")]
    public CsePerson SelectedHiddenCsePerson
    {
      get => selectedHiddenCsePerson ??= new();
      set => selectedHiddenCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedHiddenObligationType.
    /// </summary>
    [JsonPropertyName("selectedHiddenObligationType")]
    public ObligationType SelectedHiddenObligationType
    {
      get => selectedHiddenObligationType ??= new();
      set => selectedHiddenObligationType = value;
    }

    /// <summary>
    /// A value of SelectedHiddenObligation.
    /// </summary>
    [JsonPropertyName("selectedHiddenObligation")]
    public Obligation SelectedHiddenObligation
    {
      get => selectedHiddenObligation ??= new();
      set => selectedHiddenObligation = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Common multi;
    private TextWorkArea amtPrompt;
    private CsePersonsWorkSet pass;
    private Standard standard;
    private LegalAction search;
    private Standard legalActionPrompt;
    private Common showDeactivedObligation;
    private ScreenDueAmounts screenDueAmounts;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistributedAmount;
    private LegalAction hiddenLegalAction;
    private CsePerson selectedHiddenCsePerson;
    private ObligationType selectedHiddenObligationType;
    private Obligation selectedHiddenObligation;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SelectFound.
    /// </summary>
    [JsonPropertyName("selectFound")]
    public Common SelectFound
    {
      get => selectFound ??= new();
      set => selectFound = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public ObligationType Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    private Common selectFound;
    private ObligationType selected;
    private Common promptCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }
#endregion
}

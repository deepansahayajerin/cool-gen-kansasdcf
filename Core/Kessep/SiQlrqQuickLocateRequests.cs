// Program: SI_QLRQ_QUICK_LOCATE_REQUESTS, ID: 372392822, model: 746.
// Short name: SWEQLRQP
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
/// A program: SI_QLRQ_QUICK_LOCATE_REQUESTS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists and makes all of the requests to other states for 
/// information on an AP for a specified CASE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiQlrqQuickLocateRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QLRQ_QUICK_LOCATE_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQlrqQuickLocateRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQlrqQuickLocateRequests.
  /// </summary>
  public SiQlrqQuickLocateRequests(IContext context, Import import,
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
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 	  ?????????????		Initial Development
    // 01/26/1999   Carl Ott		(1) Add 'update' functionality to allow
    //                                     
    // entry of Return Date.
    //                                 
    // (2) Modified to allow locate
    // requests on in-
    //                                     
    // coming Interstate cases.
    //                                 
    // (3) Modified to refresh screen
    // after 'Send'
    //                                     
    // or 'update' actions.
    // ------------------------------------------------------------
    // ***************************************************************
    // 03/15/1999    C. Ott   Call CAB to read CSENet State Table to verify that
    // the selected state accepts Quick Locate requests.
    // ***************************************************************
    // ***************************************************************
    // 03/24/1999    C. Ott   Display all information on closed cases.
    // ***************************************************************
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // *********************************************
    // 08/16/00 M. Ashworth            Added Exit State SSN Required when 
    // sending a
    //                                 
    // Quick Locate.
    //                                 
    // PR126931
    // *********************************************
    // 9/26/01 M. Ashworth PR115337- Monitored activity not closing.  By setting
    // the denorm numeric 12 to the state Fips., we will be able to distinguish
    // between requests sent out to different states for the same AP. MCA 9-26-
    // 01
    // PR133601. L. Bachura 12-22-2001. Fix udpate portion of the security cab 
    // call.
    //  PR136902 adds the send command to the security cab call. Done 1-22-02.
    // ***********************************************************************
    // PR189075. B. Lee    10-21-20013.  Added the display command to the 
    // security cab call.
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    UseOeCabSetMnemonics();

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    export.DisplayOnly.Number = import.DisplayOnly.Number;
    export.OspServiceProvider.LastName = import.OspServiceProvider.LastName;
    MoveOffice(import.OspOffice, export.OspOffice);
    export.ApCsePersonsWorkSet.Assign(import.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.Next.Number = import.Next.Number;
    export.Prompt.Count = import.Prompt.Count;
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;

    export.Details.Index = 0;
    export.Details.Clear();

    for(import.Details.Index = 0; import.Details.Index < import.Details.Count; ++
      import.Details.Index)
    {
      if (export.Details.IsFull)
      {
        break;
      }

      export.Details.Update.Hidden.IntHGeneratedId =
        import.Details.Item.Hidden.IntHGeneratedId;
      export.Details.Update.State.State = import.Details.Item.State.State;
      MoveInterstateRequestHistory1(import.Details.Item.Send,
        export.Details.Update.Send);
      export.Details.Update.Return1.TransactionDate =
        import.Details.Item.Return1.TransactionDate;

      if (export.Details.Item.Return1.TransactionDate != null)
      {
        var field = GetField(export.Details.Item.Return1, "transactionDate");

        field.Color = "cyan";
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.Details.Item.Return1, "transactionDate");

        field.Protected = false;
      }

      export.Details.Next();
    }

    import.Header.Index = 0;
    import.Header.CheckSize();

    export.Header.Index = -1;

    for(import.Header.Index = 0; import.Header.Index < Import
      .HeaderGroup.Capacity; ++import.Header.Index)
    {
      if (!import.Header.CheckSize())
      {
        break;
      }

      ++export.Header.Index;
      export.Header.CheckSize();

      export.Header.Update.HeaderPrompt.SelectChar =
        import.Header.Item.HeaderPrompt.SelectChar;
      export.Header.Update.HeaderState.State =
        import.Header.Item.HeaderState.State;
      export.Header.Update.HeaderSend.TransactionDate =
        import.Header.Item.HeaderSend.TransactionDate;
      export.Header.Update.HeaderReturn.TransactionDate =
        import.Header.Item.HeaderReturn.TransactionDate;

      if (!IsEmpty(export.Header.Item.HeaderPrompt.SelectChar))
      {
        ++local.NoOfPrompts.Count;
      }

      switch(import.Header.Index + 1)
      {
        case 1:
          local.SubscriptOne.State = export.Header.Item.HeaderState.State;

          break;
        case 2:
          local.SubscriptTwo.State = export.Header.Item.HeaderState.State;

          break;
        case 3:
          local.SubscriptThree.State = export.Header.Item.HeaderState.State;

          break;
        default:
          break;
      }
    }

    import.Header.CheckIndex();

    // ------------------------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ------------------------------------------------------------
    export.HiddenGroupExportDetails.Index = -1;

    for(import.HiddenGroupImportDetails.Index = 0; import
      .HiddenGroupImportDetails.Index < import.HiddenGroupImportDetails.Count; ++
      import.HiddenGroupImportDetails.Index)
    {
      if (!import.HiddenGroupImportDetails.CheckSize())
      {
        break;
      }

      ++export.HiddenGroupExportDetails.Index;
      export.HiddenGroupExportDetails.CheckSize();

      export.HiddenGroupExportDetails.Update.HiddenGroupExportState.State =
        import.HiddenGroupImportDetails.Item.HiddenGroupImportState.State;
      export.HiddenGroupExportDetails.Update.HiddenGroupExportSend.
        TransactionDate =
          import.HiddenGroupImportDetails.Item.HiddenGroupImportSend.
          TransactionDate;
      export.HiddenGroupExportDetails.Update.HiddenGroupExportReturn.
        TransactionDate =
          import.HiddenGroupImportDetails.Item.HiddenGroupImportReturn.
          TransactionDate;
    }

    import.HiddenGroupImportDetails.CheckIndex();

    if (!IsEmpty(export.Next.Number))
    {
      local.ZeroFill.Text10 = export.Next.Number;
      UseEabPadLeftWithZeros();
      export.Next.Number = local.ZeroFill.Text10;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.ApCsePersonsWorkSet.Assign(import.Ap);
      MoveCsePersonsWorkSet(import.Ar, export.Ar);

      return;
    }

    // ---------------------------------------------
    // When the control is returned from the Case
    // Composition screen, populate the AP and AR
    // with the returned values, and COMMAND is set
    // to DISPLAY.
    // ---------------------------------------------
    if (Equal(global.Command, "RETCOMP"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        MoveCsePersonsWorkSet(import.SelectedCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
        export.ApCsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
      }

      export.PromptPerson.SelectChar = "";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // When the control is returned from the Code
    // Value Screen.
    // ---------------------------------------------
    if (export.Prompt.Count != 0)
    {
      export.Header.Index = export.Prompt.Count - 1;
      export.Header.CheckSize();

      export.Header.Update.HeaderState.State = import.SelectedCodeValue.Cdvalue;
      export.Prompt.Count = 0;
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      for(export.Header.Index = 0; export.Header.Index < Export
        .HeaderGroup.Capacity; ++export.Header.Index)
      {
        if (!export.Header.CheckSize())
        {
          break;
        }

        if (AsChar(export.Header.Item.HeaderPrompt.SelectChar) == 'S')
        {
          export.Header.Update.HeaderState.State =
            import.SelectedCodeValue.Cdvalue;
        }

        export.Header.Update.HeaderPrompt.SelectChar = "";
      }

      export.Header.CheckIndex();
      export.ApCsePersonsWorkSet.Assign(import.Ap);
      MoveCsePersonsWorkSet(import.Ar, export.Ar);

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Next.Number;
      export.Hidden.CsePersonNumberAp = export.ApCsePersonsWorkSet.Number;
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
      UseScCabNextTranGet();
      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
      export.ApCsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
        (10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      export.Next.Number = export.DisplayOnly.Number;
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "SEND") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "SEND":
        export.ApCsePersonsWorkSet.Assign(import.Ap);
        MoveCsePersonsWorkSet(import.Ar, export.Ar);

        if (IsEmpty(export.ApCsePersonsWorkSet.Ssn) || Equal
          (export.ApCsePersonsWorkSet.Ssn, "000000000") || Equal
          (export.ApCsePersonsWorkSet.Ssn, "0"))
        {
          var field1 = GetField(export.ApCsePersonsWorkSet, "formattedName");

          field1.Error = true;

          var field2 = GetField(export.ApCsePersonsWorkSet, "number");

          field2.Error = true;

          ExitState = "SI0000_SSN_REQUIRED_WHEN_SENDING";

          return;
        }

        export.HiddenGroupExportDetails.Index = -1;

        for(export.Details.Index = 0; export.Details.Index < export
          .Details.Count; ++export.Details.Index)
        {
          ++export.HiddenGroupExportDetails.Index;
          export.HiddenGroupExportDetails.CheckSize();

          if (!Equal(export.Details.Item.Return1.TransactionDate,
            export.HiddenGroupExportDetails.Item.HiddenGroupExportReturn.
              TransactionDate))
          {
            var field =
              GetField(export.Details.Item.Return1, "transactionDate");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          UseOeCabCheckCaseMember();

          if (!IsEmpty(local.Error.Flag))
          {
            ExitState = "CASE_NF";

            return;
          }
        }

        if (!Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field1 = GetField(export.DisplayOnly, "number");

          field1.Error = true;

          var field2 = GetField(export.Next, "number");

          field2.Error = true;

          ExitState = "SI0000_DISPLAY_BEFORE_SEND";

          return;
        }

        export.Lo1SentTodayCount.Count = 0;
        export.Header.Index = 0;

        for(var limit = export.Header.Count; export.Header.Index < limit; ++
          export.Header.Index)
        {
          if (!export.Header.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Header.Item.HeaderState.State))
          {
            if (Equal(export.Header.Item.HeaderState.State, "KS"))
            {
              var field = GetField(export.Header.Item.HeaderState, "state");

              field.Error = true;

              ExitState = "SI0000_LO1R_CANNOT_BE_SENT_TO_KS";

              continue;
            }

            local.Error.Flag = "";
            UseSiValidateStateFips2();

            if (AsChar(local.Error.Flag) == 'Y')
            {
              var field = GetField(export.Header.Item.HeaderState, "state");

              field.Error = true;
            }
            else
            {
              // ***************************************************************
              // 03/15/1999    C. Ott   Call CAB to read CSENet State Table to 
              // verify that the selected state accepts Quick Locate requests.
              // ***************************************************************
              local.CsenetStateTable.StateCode =
                export.Header.Item.HeaderState.State;
              UseSiReadCsenetStateTable();

              if (IsExitState("CO0000_CSENET_STATE_NF"))
              {
                var field = GetField(export.Header.Item.HeaderState, "state");

                field.Error = true;
              }
              else if (AsChar(local.CsenetStateTable.RecStateInd) != 'Y')
              {
                ExitState = "CO0000_CSENET_NOT_SENT_TO_STATE";

                var field = GetField(export.Header.Item.HeaderState, "state");

                field.Error = true;
              }
              else if (AsChar(local.CsenetStateTable.QuickLocate) != 'Y')
              {
                ExitState = "CO0000_STATE_DOES_ACCEPT_LO1";

                var field = GetField(export.Header.Item.HeaderState, "state");

                field.Error = true;
              }
              else
              {
                ++export.Lo1SentTodayCount.Count;
              }
            }
          }
        }

        export.Header.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.Lo1SentTodayCount.Count == 0)
        {
          ExitState = "SI0000_ENTER_STATES_FOR_LO1R";

          return;
        }

        for(export.Header.Index = 0; export.Header.Index < Export
          .HeaderGroup.Capacity; ++export.Header.Index)
        {
          if (!export.Header.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Header.Item.HeaderState.State))
          {
            switch(export.Header.Index + 1)
            {
              case 2:
                if (Equal(export.Header.Item.HeaderState.State,
                  local.SubscriptOne.State))
                {
                  var field = GetField(export.Header.Item.HeaderState, "state");

                  field.Error = true;

                  ExitState = "CO0000_STATE_ALREADY_SELECTED";
                }

                break;
              case 3:
                if (Equal(export.Header.Item.HeaderState.State,
                  local.SubscriptTwo.State) || Equal
                  (export.Header.Item.HeaderState.State,
                  local.SubscriptOne.State))
                {
                  var field = GetField(export.Header.Item.HeaderState, "state");

                  field.Error = true;

                  ExitState = "CO0000_STATE_ALREADY_SELECTED";
                }

                break;
              default:
                break;
            }
          }
        }

        export.Header.CheckIndex();

        if (IsExitState("CO0000_STATE_ALREADY_SELECTED"))
        {
          return;
        }

        for(export.Header.Index = 0; export.Header.Index < Export
          .HeaderGroup.Capacity; ++export.Header.Index)
        {
          if (!export.Header.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Header.Item.HeaderState.State))
          {
            UseSiQlrqSendQuickLocateRqsts();

            if (IsExitState("SI0000_NO_CSENET_OUT_CLOSED_CASE"))
            {
              var field = GetField(export.Next, "number");

              field.Error = true;
            }
            else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              // ***************************************************************
              // Create Monitored Activity fo each Quick Locate Request sent.
              // ***************************************************************
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.BusinessObjectCd = "CAU";
              local.Infrastructure.CaseNumber = export.Next.Number;
              local.Infrastructure.CreatedBy = global.UserId;
              local.Infrastructure.CreatedTimestamp = Now();
              local.Infrastructure.CsePersonNumber =
                export.ApCsePersonsWorkSet.Number;
              local.Infrastructure.CsenetInOutCode = "O";
              local.Infrastructure.DenormDate =
                local.MatchToInfrastructureInterstateCase.TransactionDate;

              // ***************************************************************
              // PR115337- Monitored activity not closing.  By setting the 
              // denorm numeric 12 to the state abbr., we will be able to
              // distinguish between requests sent out to different states for
              // the same AP. MCA 9-26-01
              // ***************************************************************
              ReadFips();
              local.Infrastructure.DenormNumeric12 = entities.Fips.State;
              local.Infrastructure.DenormTimestamp =
                local.MatchToInfrastructureInterstateRequestHistory.
                  CreatedTimestamp;
              local.Infrastructure.Detail = "Quick Locate Request sent to " + export
                .Header.Item.HeaderState.State + " for " + TrimEnd
                (export.ApCsePersonsWorkSet.FormattedName) + " on Case # " + export
                .Next.Number;
              local.Infrastructure.EventId = 10;
              local.Infrastructure.InitiatingStateCode = "KS";
              local.Infrastructure.UserId = "QLRQ";
              local.Infrastructure.SituationNumber = 0;
              local.Infrastructure.ReasonCode = "QUICKLOCATESENT";
              local.Infrastructure.ReferenceDate = Now().Date;
              UseSpCabCreateInfrastructure();
              export.Header.Update.HeaderState.State = "";
              export.Header.Update.HeaderPrompt.SelectChar = "";

              if (IsExitState("SP0000_EVENT_DETAIL_NF"))
              {
                return;
              }
            }
            else if (IsExitState("SI0000_LO1R_REQUIRES_1_MNTH_WAIT"))
            {
              if (Equal(local.ErrorState.State,
                export.Header.Item.HeaderState.State))
              {
                local.Lo1RRequires1MnthWait.Flag = "Y";

                var field = GetField(export.Header.Item.HeaderState, "state");

                field.Error = true;

                ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              }
            }
            else if (IsExitState("SI0000_LO1R_REQUIRES_3_MNTH_WAIT"))
            {
              if (Equal(local.ErrorState.State,
                export.Header.Item.HeaderState.State))
              {
                local.Lo1RRequires3MnthWait.Flag = "Y";

                var field = GetField(export.Header.Item.HeaderState, "state");

                field.Error = true;

                ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              }
            }
            else if (IsExitState("SI0000_ONLY_3_LO1_ALLOWED"))
            {
              if (Equal(local.ErrorState.State,
                export.Header.Item.HeaderState.State))
              {
                local.Only3Lo1Allowed.Flag = "Y";

                var field = GetField(export.Header.Item.HeaderState, "state");

                field.Error = true;

                ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              }
            }
            else
            {
              return;
            }
          }
        }

        export.Header.CheckIndex();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          if (AsChar(local.Lo1RRequires1MnthWait.Flag) == 'Y' || AsChar
            (local.Lo1RRequires3MnthWait.Flag) == 'Y' || AsChar
            (local.Only3Lo1Allowed.Flag) == 'Y')
          {
          }
          else
          {
            local.DisplayAfterAdd.Flag = "Y";
          }

          ExitState = "ACO_NN0000_ALL_OK";
          global.Command = "DISPLAY";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        switch(AsChar(export.PromptPerson.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.NoOfPrompts.Count;

            break;
          default:
            ++local.NoOfPrompts.Count;

            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        if (local.NoOfPrompts.Count > 1)
        {
          for(export.Header.Index = 0; export.Header.Index < Export
            .HeaderGroup.Capacity; ++export.Header.Index)
          {
            if (!export.Header.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Header.Item.HeaderPrompt.SelectChar))
            {
              var field =
                GetField(export.Header.Item.HeaderPrompt, "selectChar");

              field.Error = true;
            }
          }

          export.Header.CheckIndex();

          if (!IsEmpty(export.PromptPerson.SelectChar))
          {
            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        if (local.NoOfPrompts.Count == 0)
        {
          for(export.Header.Index = 0; export.Header.Index < Export
            .HeaderGroup.Capacity; ++export.Header.Index)
          {
            if (!export.Header.CheckSize())
            {
              break;
            }

            var field1 =
              GetField(export.Header.Item.HeaderPrompt, "selectChar");

            field1.Error = true;
          }

          export.Header.CheckIndex();

          var field = GetField(export.PromptPerson, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.ApCsePersonsWorkSet.Assign(import.Ap);
          MoveCsePersonsWorkSet(import.Ar, export.Ar);

          return;
        }

        if (!IsEmpty(export.PromptPerson.SelectChar))
        {
          if (AsChar(export.PromptPerson.SelectChar) == 'S')
          {
            if (IsEmpty(export.Next.Number))
            {
              export.ApCsePersonsWorkSet.Assign(import.Ap);
              MoveCsePersonsWorkSet(import.Ar, export.Ar);

              var field = GetField(export.Next, "number");

              field.Error = true;

              ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";

              return;
            }

            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
          else
          {
            export.ApCsePersonsWorkSet.Assign(import.Ap);
            MoveCsePersonsWorkSet(import.Ar, export.Ar);

            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }
        }

        for(export.Header.Index = 0; export.Header.Index < Export
          .HeaderGroup.Capacity; ++export.Header.Index)
        {
          if (!export.Header.CheckSize())
          {
            break;
          }

          export.ApCsePersonsWorkSet.Assign(import.Ap);
          MoveCsePersonsWorkSet(import.Ar, export.Ar);

          if (AsChar(export.Header.Item.HeaderPrompt.SelectChar) == 'S')
          {
            // for the cases where you link from 1 procedure to another 
            // procedure, you must set the export_hidden security link_indicator
            // to "L".
            // this will tell the called procedure that we are on a link and not
            // a transfer.  Don't forget to do the view matching on the dialog
            // design screen.
            // ****
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
          else if (IsEmpty(export.Header.Item.HeaderPrompt.SelectChar) || AsChar
            (import.Header.Item.HeaderPrompt.SelectChar) == '+')
          {
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.Header.Item.HeaderPrompt, "selectChar");

            field.Error = true;

            return;
          }
        }

        export.Header.CheckIndex();

        break;
      case "DISPLAY":
        break;
      case "UPDATE":
        export.ApCsePersonsWorkSet.Assign(import.Ap);
        MoveCsePersonsWorkSet(import.Ar, export.Ar);

        for(export.Header.Index = 0; export.Header.Index < Export
          .HeaderGroup.Capacity; ++export.Header.Index)
        {
          if (!export.Header.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Header.Item.HeaderState.State))
          {
            var field = GetField(export.Header.Item.HeaderState, "state");

            field.Error = true;

            ExitState = "INVALID_UPDATE_THIS_ENTRY";
          }
        }

        export.Header.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.HiddenGroupExportDetails.Index = -1;
        local.Updates.Count = 0;

        for(export.Details.Index = 0; export.Details.Index < export
          .Details.Count; ++export.Details.Index)
        {
          ++export.HiddenGroupExportDetails.Index;
          export.HiddenGroupExportDetails.CheckSize();

          if (!Equal(export.Details.Item.Return1.TransactionDate,
            export.HiddenGroupExportDetails.Item.HiddenGroupExportReturn.
              TransactionDate))
          {
            if (Lt(export.Details.Item.Return1.TransactionDate,
              export.Details.Item.Send.TransactionDate))
            {
              var field =
                GetField(export.Details.Item.Return1, "transactionDate");

              field.Error = true;

              ExitState = "CO0000_RET_DTE_LESS_THAN_SEND_DT";

              return;
            }

            if (Lt(Now().Date, export.Details.Item.Return1.TransactionDate))
            {
              var field =
                GetField(export.Details.Item.Return1, "transactionDate");

              field.Error = true;

              ExitState = "CO0000_RET_DTE_GREATER_THAN_CURR";

              return;
            }

            UseSiValidateStateFips1();

            if (AsChar(local.Error.Flag) == 'Y')
            {
              var field = GetField(export.Details.Item.State, "state");

              field.Error = true;

              return;
            }

            // ***************************************************************
            // 03/15/1999    C. Ott   Call CAB to read CSENet State Table to 
            // verify that the selected state accepts Quick Locate requests.
            // ***************************************************************
            local.CsenetStateTable.StateCode = export.Details.Item.State.State;
            UseSiReadCsenetStateTable();

            if (IsExitState("CO0000_CSENET_STATE_NF"))
            {
              var field = GetField(export.Details.Item.State, "state");

              field.Error = true;

              return;
            }
            else if (AsChar(local.CsenetStateTable.QuickLocate) != 'Y')
            {
              ExitState = "CO0000_STATE_DOES_ACCEPT_LO1";

              var field = GetField(export.Details.Item.State, "state");

              field.Error = true;

              return;
            }
            else
            {
            }

            local.InterstateRequest.IntHGeneratedId =
              export.Details.Item.Hidden.IntHGeneratedId;
            local.InterstateRequest.OtherStateFips = local.Fips.State;
            local.Ap.Number = export.ApCsePersonsWorkSet.Number;
            local.InterstateRequestHistory.FunctionalTypeCode = "LO1";
            local.InterstateRequestHistory.ActionCode = "P";
            local.InterstateRequestHistory.TransactionDirectionInd = "I";
            local.InterstateRequestHistory.TransactionDate = Now().Date;
            local.InterstateRequestHistory.ActionResolutionDate =
              export.Details.Item.Return1.TransactionDate;
            local.InterstateRequestHistory.CreatedTimestamp =
              export.Details.Item.Send.CreatedTimestamp;
            UseSiQlrqUpdateQuickLocRequest();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              var field =
                GetField(export.Details.Item.Return1, "transactionDate");

              field.Error = true;

              return;
            }

            ++local.Updates.Count;
          }
        }

        if (local.Updates.Count == 0)
        {
          ExitState = "FN0000_NO_UPDATES_MADE";

          return;
        }

        local.DisplayAfterUpdate.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.Details.Index = 0;
      export.Details.Clear();

      for(import.Details.Index = 0; import.Details.Index < import
        .Details.Count; ++import.Details.Index)
      {
        if (export.Details.IsFull)
        {
          break;
        }

        var field = GetField(export.Details.Item.Return1, "transactionDate");

        field.Protected = false;

        export.Details.Next();
      }

      if (IsEmpty(export.Next.Number))
      {
        export.DisplayOnly.Number = local.RefreshCase.Number;
        MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
        MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);

        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }
      else
      {
        if (!Equal(export.DisplayOnly.Number, export.Next.Number) && !
          IsEmpty(export.DisplayOnly.Number))
        {
          export.DisplayOnly.Number = local.RefreshCase.Number;
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);
        }

        UseSiReadCaseHeaderInformation();

        if (IsExitState("CASE_NF"))
        {
          export.DisplayOnly.Number = local.RefreshCase.Number;
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);

          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }

        export.DisplayOnly.Number = export.Next.Number;
      }

      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (!Equal(export.Next.Number, export.DisplayOnly.Number) && !
        IsEmpty(export.DisplayOnly.Number))
      {
        export.ApCsePerson.Number = "";
        export.ApCsePersonsWorkSet.Number = "";
        export.ApCsePersonsWorkSet.FormattedName = "";
      }

      if (AsChar(local.Lo1RRequires1MnthWait.Flag) == 'Y' || AsChar
        (local.Lo1RRequires3MnthWait.Flag) == 'Y' || AsChar
        (local.Only3Lo1Allowed.Flag) == 'Y')
      {
      }
      else
      {
        for(export.Header.Index = 0; export.Header.Index < Export
          .HeaderGroup.Capacity; ++export.Header.Index)
        {
          if (!export.Header.CheckSize())
          {
            break;
          }

          export.Header.Update.HeaderState.State = "";
          export.Header.Update.HeaderPrompt.SelectChar = "";
        }

        export.Header.CheckIndex();
      }

      UseSiQlrqReadQuickLocateRqsts();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
      {
        export.Details.Index = 0;
        export.Details.Clear();

        for(import.Details.Index = 0; import.Details.Index < import
          .Details.Count; ++import.Details.Index)
        {
          if (export.Details.IsFull)
          {
            break;
          }

          export.Details.Next();
        }

        return;
      }
      else if (AsChar(local.DisplayAfterAdd.Flag) == 'Y')
      {
        ExitState = "SI0000_LO1R_SUCCESSFUL";
      }
      else if (AsChar(local.DisplayAfterUpdate.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      else if (AsChar(local.Lo1RRequires1MnthWait.Flag) == 'Y')
      {
        ExitState = "SI0000_LO1R_REQUIRES_1_MNTH_WAIT";
      }
      else if (AsChar(local.Lo1RRequires3MnthWait.Flag) == 'Y')
      {
        ExitState = "SI0000_LO1R_REQUIRES_3_MNTH_WAIT";
      }
      else if (AsChar(local.Only3Lo1Allowed.Flag) == 'Y')
      {
        ExitState = "SI0000_ONLY_3_LO1_ALLOWED";
      }
      else if (AsChar(local.CaseOpen.Flag) == 'N')
      {
        ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
      }

      export.HiddenGroupExportDetails.Index = -1;

      for(export.Details.Index = 0; export.Details.Index < export
        .Details.Count; ++export.Details.Index)
      {
        ++export.HiddenGroupExportDetails.Index;
        export.HiddenGroupExportDetails.CheckSize();

        export.HiddenGroupExportDetails.Update.HiddenGroupExportState.State =
          export.Details.Item.State.State;
        export.HiddenGroupExportDetails.Update.HiddenGroupExportReturn.
          TransactionDate = export.Details.Item.Return1.TransactionDate;
        export.HiddenGroupExportDetails.Update.HiddenGroupExportSend.
          TransactionDate = export.Details.Item.Send.TransactionDate;

        if (export.Details.Item.Return1.TransactionDate != null)
        {
          var field = GetField(export.Details.Item.Return1, "transactionDate");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.Details.Item.Return1, "transactionDate");

          field.Protected = false;
        }
      }
    }
    else
    {
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToDetails(SiQlrqReadQuickLocateRqsts.Export.
    ExportGroup source, Export.DetailsGroup target)
  {
    target.State.State = source.State.State;
    MoveInterstateRequestHistory1(source.Send, target.Send);
    target.Return1.TransactionDate = source.Return1.TransactionDate;
    target.Hidden.IntHGeneratedId = source.Hidden.IntHGeneratedId;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
  }

  private static void MoveInterstateRequestHistory1(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateRequestHistory2(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ActionResolutionDate = source.ActionResolutionDate;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Error.Flag = useExport.Work.Flag;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    export.State.CodeName = useExport.State.CodeName;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiQlrqReadQuickLocateRqsts()
  {
    var useImport = new SiQlrqReadQuickLocateRqsts.Import();
    var useExport = new SiQlrqReadQuickLocateRqsts.Export();

    useImport.CaseOpen.Flag = local.CaseOpen.Flag;
    useImport.Case1.Number = export.Next.Number;
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);

    Call(SiQlrqReadQuickLocateRqsts.Execute, useImport, useExport);

    export.DisplayOnly.Number = useExport.Case1.Number;
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    useExport.Export1.CopyTo(export.Details, MoveExport1ToDetails);
  }

  private void UseSiQlrqSendQuickLocateRqsts()
  {
    var useImport = new SiQlrqSendQuickLocateRqsts.Import();
    var useExport = new SiQlrqSendQuickLocateRqsts.Export();

    useImport.Lo1SentTodayCount.Count = export.Lo1SentTodayCount.Count;
    useImport.Case1.Number = export.Next.Number;
    useImport.State.State = export.Header.Item.HeaderState.State;
    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiQlrqSendQuickLocateRqsts.Execute, useImport, useExport);

    local.ErrorState.State = useExport.Common.State;
    MoveInterstateCase(useExport.InterstateCase,
      local.MatchToInfrastructureInterstateCase);
    export.Lo1SentTodayCount.Count = useExport.Lo1SentTodayCount.Count;
    local.MatchToInfrastructureInterstateRequestHistory.Assign(useExport.Send);
  }

  private void UseSiQlrqUpdateQuickLocRequest()
  {
    var useImport = new SiQlrqUpdateQuickLocRequest.Import();
    var useExport = new SiQlrqUpdateQuickLocRequest.Export();

    MoveInterstateRequestHistory2(local.InterstateRequestHistory,
      useImport.InterstateRequestHistory);
    MoveInterstateRequest(local.InterstateRequest, useImport.InterstateRequest);

    Call(SiQlrqUpdateQuickLocRequest.Execute, useImport, useExport);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.CaseOpen.Flag = useExport.CaseOpen.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    export.ApCsePersonsWorkSet.Assign(useExport.Ap);
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    local.CsenetStateTable.Assign(useExport.CsenetStateTable);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.OspServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.OspOffice);
  }

  private void UseSiValidateStateFips1()
  {
    var useImport = new SiValidateStateFips.Import();
    var useExport = new SiValidateStateFips.Export();

    useImport.Common.State = export.Details.Item.State.State;

    Call(SiValidateStateFips.Execute, useImport, useExport);

    local.Error.Flag = useExport.Error.Flag;
    MoveFips(useExport.Fips, local.Fips);
  }

  private void UseSiValidateStateFips2()
  {
    var useImport = new SiValidateStateFips.Import();
    var useExport = new SiValidateStateFips.Export();

    useImport.Common.State = export.Header.Item.HeaderState.State;

    Call(SiValidateStateFips.Execute, useImport, useExport);

    local.Error.Flag = useExport.Error.Flag;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.Header.Item.HeaderState.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
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
    /// <summary>A HiddenGroupImportDetailsGroup group.</summary>
    [Serializable]
    public class HiddenGroupImportDetailsGroup
    {
      /// <summary>
      /// A value of HiddenGroupImportState.
      /// </summary>
      [JsonPropertyName("hiddenGroupImportState")]
      public Common HiddenGroupImportState
      {
        get => hiddenGroupImportState ??= new();
        set => hiddenGroupImportState = value;
      }

      /// <summary>
      /// A value of HiddenGroupImportSend.
      /// </summary>
      [JsonPropertyName("hiddenGroupImportSend")]
      public InterstateRequestHistory HiddenGroupImportSend
      {
        get => hiddenGroupImportSend ??= new();
        set => hiddenGroupImportSend = value;
      }

      /// <summary>
      /// A value of HiddenGroupImportReturn.
      /// </summary>
      [JsonPropertyName("hiddenGroupImportReturn")]
      public InterstateRequestHistory HiddenGroupImportReturn
      {
        get => hiddenGroupImportReturn ??= new();
        set => hiddenGroupImportReturn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common hiddenGroupImportState;
      private InterstateRequestHistory hiddenGroupImportSend;
      private InterstateRequestHistory hiddenGroupImportReturn;
    }

    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of HeaderState.
      /// </summary>
      [JsonPropertyName("headerState")]
      public Common HeaderState
      {
        get => headerState ??= new();
        set => headerState = value;
      }

      /// <summary>
      /// A value of HeaderPrompt.
      /// </summary>
      [JsonPropertyName("headerPrompt")]
      public Common HeaderPrompt
      {
        get => headerPrompt ??= new();
        set => headerPrompt = value;
      }

      /// <summary>
      /// A value of HeaderSend.
      /// </summary>
      [JsonPropertyName("headerSend")]
      public InterstateRequestHistory HeaderSend
      {
        get => headerSend ??= new();
        set => headerSend = value;
      }

      /// <summary>
      /// A value of HeaderReturn.
      /// </summary>
      [JsonPropertyName("headerReturn")]
      public InterstateRequestHistory HeaderReturn
      {
        get => headerReturn ??= new();
        set => headerReturn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common headerState;
      private Common headerPrompt;
      private InterstateRequestHistory headerSend;
      private InterstateRequestHistory headerReturn;
    }

    /// <summary>A DetailsGroup group.</summary>
    [Serializable]
    public class DetailsGroup
    {
      /// <summary>
      /// A value of State.
      /// </summary>
      [JsonPropertyName("state")]
      public Common State
      {
        get => state ??= new();
        set => state = value;
      }

      /// <summary>
      /// A value of Send.
      /// </summary>
      [JsonPropertyName("send")]
      public InterstateRequestHistory Send
      {
        get => send ??= new();
        set => send = value;
      }

      /// <summary>
      /// A value of Return1.
      /// </summary>
      [JsonPropertyName("return1")]
      public InterstateRequestHistory Return1
      {
        get => return1 ??= new();
        set => return1 = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public InterstateRequest Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common state;
      private InterstateRequestHistory send;
      private InterstateRequestHistory return1;
      private InterstateRequest hidden;
    }

    /// <summary>
    /// Gets a value of HiddenGroupImportDetails.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroupImportDetailsGroup> HiddenGroupImportDetails =>
      hiddenGroupImportDetails ??= new(HiddenGroupImportDetailsGroup.Capacity, 0);
      

    /// <summary>
    /// Gets a value of HiddenGroupImportDetails for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGroupImportDetails")]
    [Computed]
    public IList<HiddenGroupImportDetailsGroup> HiddenGroupImportDetails_Json
    {
      get => hiddenGroupImportDetails;
      set => HiddenGroupImportDetails.Assign(value);
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
    }

    /// <summary>
    /// Gets a value of Details.
    /// </summary>
    [JsonIgnore]
    public Array<DetailsGroup> Details =>
      details ??= new(DetailsGroup.Capacity);

    /// <summary>
    /// Gets a value of Details for json serialization.
    /// </summary>
    [JsonPropertyName("details")]
    [Computed]
    public IList<DetailsGroup> Details_Json
    {
      get => details;
      set => Details.Assign(value);
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Array<HiddenGroupImportDetailsGroup> hiddenGroupImportDetails;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Common promptPerson;
    private Common prompt;
    private CodeValue selectedCodeValue;
    private Array<HeaderGroup> header;
    private Array<DetailsGroup> details;
    private Case1 next;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Case1 displayOnly;
    private NextTranInfo hidden;
    private Standard standard;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenGroupExportDetailsGroup group.</summary>
    [Serializable]
    public class HiddenGroupExportDetailsGroup
    {
      /// <summary>
      /// A value of HiddenGroupExportState.
      /// </summary>
      [JsonPropertyName("hiddenGroupExportState")]
      public Common HiddenGroupExportState
      {
        get => hiddenGroupExportState ??= new();
        set => hiddenGroupExportState = value;
      }

      /// <summary>
      /// A value of HiddenGroupExportSend.
      /// </summary>
      [JsonPropertyName("hiddenGroupExportSend")]
      public InterstateRequestHistory HiddenGroupExportSend
      {
        get => hiddenGroupExportSend ??= new();
        set => hiddenGroupExportSend = value;
      }

      /// <summary>
      /// A value of HiddenGroupExportReturn.
      /// </summary>
      [JsonPropertyName("hiddenGroupExportReturn")]
      public InterstateRequestHistory HiddenGroupExportReturn
      {
        get => hiddenGroupExportReturn ??= new();
        set => hiddenGroupExportReturn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common hiddenGroupExportState;
      private InterstateRequestHistory hiddenGroupExportSend;
      private InterstateRequestHistory hiddenGroupExportReturn;
    }

    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of HeaderState.
      /// </summary>
      [JsonPropertyName("headerState")]
      public Common HeaderState
      {
        get => headerState ??= new();
        set => headerState = value;
      }

      /// <summary>
      /// A value of HeaderPrompt.
      /// </summary>
      [JsonPropertyName("headerPrompt")]
      public Common HeaderPrompt
      {
        get => headerPrompt ??= new();
        set => headerPrompt = value;
      }

      /// <summary>
      /// A value of HeaderSend.
      /// </summary>
      [JsonPropertyName("headerSend")]
      public InterstateRequestHistory HeaderSend
      {
        get => headerSend ??= new();
        set => headerSend = value;
      }

      /// <summary>
      /// A value of HeaderReturn.
      /// </summary>
      [JsonPropertyName("headerReturn")]
      public InterstateRequestHistory HeaderReturn
      {
        get => headerReturn ??= new();
        set => headerReturn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common headerState;
      private Common headerPrompt;
      private InterstateRequestHistory headerSend;
      private InterstateRequestHistory headerReturn;
    }

    /// <summary>A DetailsGroup group.</summary>
    [Serializable]
    public class DetailsGroup
    {
      /// <summary>
      /// A value of State.
      /// </summary>
      [JsonPropertyName("state")]
      public Common State
      {
        get => state ??= new();
        set => state = value;
      }

      /// <summary>
      /// A value of Send.
      /// </summary>
      [JsonPropertyName("send")]
      public InterstateRequestHistory Send
      {
        get => send ??= new();
        set => send = value;
      }

      /// <summary>
      /// A value of Return1.
      /// </summary>
      [JsonPropertyName("return1")]
      public InterstateRequestHistory Return1
      {
        get => return1 ??= new();
        set => return1 = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public InterstateRequest Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common state;
      private InterstateRequestHistory send;
      private InterstateRequestHistory return1;
      private InterstateRequest hidden;
    }

    /// <summary>
    /// Gets a value of HiddenGroupExportDetails.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroupExportDetailsGroup> HiddenGroupExportDetails =>
      hiddenGroupExportDetails ??= new(HiddenGroupExportDetailsGroup.Capacity, 0);
      

    /// <summary>
    /// Gets a value of HiddenGroupExportDetails for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGroupExportDetails")]
    [Computed]
    public IList<HiddenGroupExportDetailsGroup> HiddenGroupExportDetails_Json
    {
      get => hiddenGroupExportDetails;
      set => HiddenGroupExportDetails.Assign(value);
    }

    /// <summary>
    /// A value of Lo1SentTodayCount.
    /// </summary>
    [JsonPropertyName("lo1SentTodayCount")]
    public Common Lo1SentTodayCount
    {
      get => lo1SentTodayCount ??= new();
      set => lo1SentTodayCount = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
    }

    /// <summary>
    /// Gets a value of Details.
    /// </summary>
    [JsonIgnore]
    public Array<DetailsGroup> Details =>
      details ??= new(DetailsGroup.Capacity);

    /// <summary>
    /// Gets a value of Details for json serialization.
    /// </summary>
    [JsonPropertyName("details")]
    [Computed]
    public IList<DetailsGroup> Details_Json
    {
      get => details;
      set => Details.Assign(value);
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
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
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Array<HiddenGroupExportDetailsGroup> hiddenGroupExportDetails;
    private Common lo1SentTodayCount;
    private Common promptPerson;
    private Common prompt;
    private Code state;
    private Array<HeaderGroup> header;
    private Array<DetailsGroup> details;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Case1 next;
    private Case1 displayOnly;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson apCsePerson;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Only3Lo1Allowed.
    /// </summary>
    [JsonPropertyName("only3Lo1Allowed")]
    public Common Only3Lo1Allowed
    {
      get => only3Lo1Allowed ??= new();
      set => only3Lo1Allowed = value;
    }

    /// <summary>
    /// A value of Lo1RRequires3MnthWait.
    /// </summary>
    [JsonPropertyName("lo1RRequires3MnthWait")]
    public Common Lo1RRequires3MnthWait
    {
      get => lo1RRequires3MnthWait ??= new();
      set => lo1RRequires3MnthWait = value;
    }

    /// <summary>
    /// A value of Lo1RRequires1MnthWait.
    /// </summary>
    [JsonPropertyName("lo1RRequires1MnthWait")]
    public Common Lo1RRequires1MnthWait
    {
      get => lo1RRequires1MnthWait ??= new();
      set => lo1RRequires1MnthWait = value;
    }

    /// <summary>
    /// A value of ErrorState.
    /// </summary>
    [JsonPropertyName("errorState")]
    public Common ErrorState
    {
      get => errorState ??= new();
      set => errorState = value;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    /// <summary>
    /// A value of MatchToInfrastructureInterstateCase.
    /// </summary>
    [JsonPropertyName("matchToInfrastructureInterstateCase")]
    public InterstateCase MatchToInfrastructureInterstateCase
    {
      get => matchToInfrastructureInterstateCase ??= new();
      set => matchToInfrastructureInterstateCase = value;
    }

    /// <summary>
    /// A value of MatchToInfrastructureInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("matchToInfrastructureInterstateRequestHistory")]
    public InterstateRequestHistory MatchToInfrastructureInterstateRequestHistory
      
    {
      get => matchToInfrastructureInterstateRequestHistory ??= new();
      set => matchToInfrastructureInterstateRequestHistory = value;
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
    /// A value of SubscriptThree.
    /// </summary>
    [JsonPropertyName("subscriptThree")]
    public Common SubscriptThree
    {
      get => subscriptThree ??= new();
      set => subscriptThree = value;
    }

    /// <summary>
    /// A value of SubscriptTwo.
    /// </summary>
    [JsonPropertyName("subscriptTwo")]
    public Common SubscriptTwo
    {
      get => subscriptTwo ??= new();
      set => subscriptTwo = value;
    }

    /// <summary>
    /// A value of SubscriptOne.
    /// </summary>
    [JsonPropertyName("subscriptOne")]
    public Common SubscriptOne
    {
      get => subscriptOne ??= new();
      set => subscriptOne = value;
    }

    /// <summary>
    /// A value of NoOfPrompts.
    /// </summary>
    [JsonPropertyName("noOfPrompts")]
    public Common NoOfPrompts
    {
      get => noOfPrompts ??= new();
      set => noOfPrompts = value;
    }

    /// <summary>
    /// A value of Updates.
    /// </summary>
    [JsonPropertyName("updates")]
    public Common Updates
    {
      get => updates ??= new();
      set => updates = value;
    }

    /// <summary>
    /// A value of DisplayAfterUpdate.
    /// </summary>
    [JsonPropertyName("displayAfterUpdate")]
    public Common DisplayAfterUpdate
    {
      get => displayAfterUpdate ??= new();
      set => displayAfterUpdate = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of DisplayAfterAdd.
    /// </summary>
    [JsonPropertyName("displayAfterAdd")]
    public Common DisplayAfterAdd
    {
      get => displayAfterAdd ??= new();
      set => displayAfterAdd = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of RefreshCase.
    /// </summary>
    [JsonPropertyName("refreshCase")]
    public Case1 RefreshCase
    {
      get => refreshCase ??= new();
      set => refreshCase = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    private Common only3Lo1Allowed;
    private Common lo1RRequires3MnthWait;
    private Common lo1RRequires1MnthWait;
    private Common errorState;
    private CsenetStateTable csenetStateTable;
    private InterstateCase matchToInfrastructureInterstateCase;
    private InterstateRequestHistory matchToInfrastructureInterstateRequestHistory;
      
    private Infrastructure infrastructure;
    private Common subscriptThree;
    private Common subscriptTwo;
    private Common subscriptOne;
    private Common noOfPrompts;
    private Common updates;
    private Common displayAfterUpdate;
    private InterstateRequest interstateRequest;
    private CsePerson ap;
    private InterstateRequestHistory interstateRequestHistory;
    private Common displayAfterAdd;
    private Common caseOpen;
    private Case1 refreshCase;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private TextWorkArea zeroFill;
    private Fips fips;
    private Common error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips fips;
  }
#endregion
}

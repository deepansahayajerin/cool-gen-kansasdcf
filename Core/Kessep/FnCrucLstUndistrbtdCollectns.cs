// Program: FN_CRUC_LST_UNDISTRBTD_COLLECTNS, ID: 372062698, model: 746.
// Short name: SWECRUCP
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
/// A program: FN_CRUC_LST_UNDISTRBTD_COLLECTNS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This screen will list undistributed collections(those with collection amount
/// fully supplied = blank) for a receivables worker id.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrucLstUndistrbtdCollectns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRUC_LST_UNDISTRBTD_COLLECTNS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrucLstUndistrbtdCollectns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrucLstUndistrbtdCollectns.
  /// </summary>
  public FnCrucLstUndistrbtdCollectns(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    //                                 
    // M A I N T E N A N C E    L O G
    // -----------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ----------	-----------	
    // -------------------------------------------------------
    // 02/21/96  H. Kennedy			Retrofits
    // 12/16/96  R. Marchman			Add new security/next tran
    // 04/11/97  Sumanta			Added cse person on the screen as a search criteria 
    // and made
    // 					necessary code changes. Also added flow for look up to person NAME.
    // 10/03/97  S. Konkader			Added attrs to EXPORT_OUT CRD, needed by CRRU. 
    // refunded_amount,
    // 					distributed_amount, offset_taxid
    // 11/13/97  S. Hardy			Added logic to read for cash receipt on the SSN of 
    // the person
    // 					number that was enter as a search value.
    // 12/11/98  S. Newman			Increased view sizes to full pages, removed release
    // logic
    // 					from procedure & screen, added edit to only allow the flow
    // 					to MCOL if pend/suspend reason = 'manual dist'.
    // 12/12/98  S. Newman			Add logic to prevent multiple prompt selections, 
    // invalid command
    // 					for enter, increased screen size of receipt number & combined
    // 					receipt number, removed zdel exit states.
    // 12/14/98  S. Newman			Added logic to display upon return  from NAME, 
    // LAPS, and CRSL.
    // 					Added logic so that when you return from NAME, LAPS, and
    // 					CRSL without making a selection, that the original display
    // 					criteria shows.  Added logic to remove CSE Person Name
    // 					when you blank out the CSE Person Number and display by
    // 					another filter.  Added logic so that when you filter by court order
    // 					number and person number, you get undistributed records.
    // 					Added logic so that when you return from CRSL and CDVL
    // 					without making a selection, that the original display criteria 
    // shows.
    // 12/21/98  S. Newman			Changed filter date to received Date rather than 
    // Receipt Date,
    // 					revised logic to display FDSO, made CRUC undistributed
    // 					calculations the same as CRDL
    // 01/12/00  Sunya Sharp	PR# 84516	Change color of exit st
    // 12/06/99  PDP		H00079312	Added FILTER of SSN and Source Type Code
    // 12/08/00  PDP		H00079312-Re	Changed order so check by SSN is done first.
    // 08/14/00  PDP		H0098148	If releasing INVCTORDER - try to get the Person 
    // Number if it is
    // 					NOT supplied
    // 06/06/03  GVandy	PR179707	Require user to enter some criteria before 
    // displaying.  Also
    // 					performance enhancements in the display cab.
    // 12/10/10  RMathews      CQ22192         Screen change to expand 
    // collection amount and undistributed amount
    //                                         
    // from 5.2 to 6.2
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Security.Userid = import.Security.Userid;
    export.UserInputCashReceiptSourceType.Code =
      import.UserCashReceiptSourceType.Code;
    export.HiddenUserInput.Code = import.HiddenUser.Code;
    export.PromptPerson.Text1 = import.PromptPerson.Text1;
    export.FilterCsePerson.Number = import.CsePerson.Number;
    export.FilterCsePersonsWorkSet.FormattedName = import.Filter.FormattedName;
    export.PromptCourtOrder.PromptField = import.PromptCourtOrder.PromptField;
    export.Starting.SequentialIdentifier = import.Starting.SequentialIdentifier;
    export.UserInputServiceProvider.UserId = import.UserServiceProvider.UserId;
    export.PromptCashReceipt.SelectChar = import.PromptCashReceipt.SelectChar;
    export.PromptSourceCode.SelectChar = import.PromptSourceCode.SelectChar;
    export.CashReceiptDetailStatHistory1.ReasonCodeId =
      import.CashReceiptDetailStatHistory.ReasonCodeId;
    export.HiddenCashReceiptDetailStatHistory.ReasonCodeId =
      import.HiddenCashReceiptDetailStatHistory.ReasonCodeId;
    export.CashReceiptDetailStatus1.Code = import.CashReceiptDetailStatus.Code;
    export.HiddenCashReceiptDetailStatus.Code =
      import.HiddenCashReceiptDetailStatus.Code;
    export.PromptCollStat.PromptField = import.PromptCollStat.PromptField;
    export.PromptRsnCode.PromptField = import.PromptRsnCode.PromptField;
    export.CourtOrderFilter.StandardNumber =
      import.CourtOrderFilter.StandardNumber;
    export.HiddenCourtOrderFilter.StandardNumber =
      import.HiddenCourtOrderFilter.StandardNumber;
    export.UserInputCashReceiptEvent.ReceivedDate =
      import.UserCashReceiptEvent.ReceivedDate;
    export.PayHistoryIndicator.Flag = import.PayHistoryIndicator.Flag;

    // H00079312    12/06/99 PDP  Added FILTER of SSN and Source Type Code
    export.FilterSsn.ObligorSocialSecurityNumber =
      import.FilterSsn.ObligorSocialSecurityNumber;
    export.PromtCollType.SelectChar = import.PromptCollType.SelectChar;
    export.Select.Code = import.Select.Code;
    export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber =
      import.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber;
    export.HiddenFilterCsePerson.Number = import.HiddenFilterCsePerson.Number;

    if (IsEmpty(import.PayHistoryIndicator.Flag))
    {
      export.PayHistoryIndicator.Flag = "N";
    }

    if (!IsEmpty(import.CsePerson.Number))
    {
      local.PadCsePersonNumber.Text10 = import.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.FilterCsePerson.Number = local.PadCsePersonNumber.Text10;
    }

    export.UserInputCashReceipt.Assign(import.UserCashReceipt);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      if (!Equal(global.Command, "RTLIST"))
      {
        export.Export1.Update.Sel.SelectChar =
          import.Import1.Item.Sel.SelectChar;
      }

      export.Export1.Update.Detail.CrdCrCombo =
        import.Import1.Item.Detail.CrdCrCombo;
      export.Export1.Update.CashReceipt.Assign(import.Import1.Item.CashReceipt);
      export.Export1.Update.CashReceiptDetail.Assign(
        import.Import1.Item.CashReceiptDetail);
      export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
        import.Import1.Item.CashReceiptDetailStatHistory.ReasonCodeId;
      export.Export1.Update.CashReceiptDetailStatus.Assign(
        import.Import1.Item.CashReceiptDetailStatus);
      MoveCashReceiptSourceType(import.Import1.Item.CashReceiptSourceType,
        export.Export1.Update.CashReceiptSourceType);
      export.Export1.Update.UndistAmt.TotalCurrency =
        import.Import1.Item.UndistAmt.TotalCurrency;
      export.Export1.Update.ScreenDisplay.CourtOrderNumber =
        import.Import1.Item.ScreenDisplay.CourtOrderNumber;
      MoveCashReceiptEvent(import.Import1.Item.HiddenCashReceiptEvent,
        export.Export1.Update.HiddenCashReceiptEvent);
      export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
        import.Import1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
      export.Export1.Update.Filter.Code = import.Import1.Item.Filter.Code;

      switch(AsChar(export.Export1.Item.Sel.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.Common.Count;

          if (Equal(global.Command, "RELEASE"))
          {
            // ************Okay to have multiple selections*************
          }
          else if (local.Common.Count == 1)
          {
            MoveCashReceipt(import.Import1.Item.CashReceipt, export.CashReceipt);
              
            MoveCashReceiptDetail1(import.Import1.Item.CashReceiptDetail,
              export.CashReceiptDetail1);
            export.CashReceiptDetailStatHistory2.ReasonCodeId =
              import.Import1.Item.CashReceiptDetailStatHistory.ReasonCodeId;
            MoveCashReceiptDetailStatus(import.Import1.Item.
              CashReceiptDetailStatus, export.CashReceiptDetailStatus2);
            MoveCashReceiptSourceType(import.Import1.Item.CashReceiptSourceType,
              export.CashReceiptSourceType);
            export.CashReceiptEvent.SystemGeneratedIdentifier =
              import.Import1.Item.HiddenCashReceiptEvent.
                SystemGeneratedIdentifier;
            export.CashReceiptType.SystemGeneratedIdentifier =
              import.Import1.Item.HiddenCashReceiptType.
                SystemGeneratedIdentifier;
          }
          else
          {
            var field1 = GetField(export.Export1.Item.Sel, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }

          break;
        default:
          var field = GetField(export.Export1.Item.Sel, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }

      export.Export1.Next();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
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
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "RTLIST"))
    {
      return;
    }

    if (!Equal(global.Command, "REFUND") && !
      Equal(global.Command, "RETCRSL") && !Equal(global.Command, "DETAIL") && !
      Equal(global.Command, "MCOL") && !Equal(global.Command, "COLA") && !
      Equal(global.Command, "RETLAPS") && !
      Equal(global.Command, "PRMPTRET") && !
      Equal(global.Command, "RETCDVL") && !Equal(global.Command, "RETRSDL") && !
      Equal(global.Command, "ENTER"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "RETCREL"))
    {
      global.Command = "DISPLAY";

      if (IsEmpty(export.UserInputServiceProvider.UserId))
      {
        export.UserInputServiceProvider.UserId =
          import.UserCashReceipt.CreatedBy;
      }
    }

    if (Equal(global.Command, "PRMPTRET"))
    {
      // ***---
      // ***--- Coming back from person name list ..
      // ***--- Use the selected person number .. if exists..
      // ***---
      if (!IsEmpty(import.Name.Number))
      {
        export.FilterCsePersonsWorkSet.FormattedName =
          import.Name.FormattedName;
        export.FilterCsePerson.Number = import.Name.Number;
      }
      else
      {
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCRSL"))
    {
      export.PromptSourceCode.SelectChar = "";

      if (IsEmpty(import.UserCashReceiptSourceType.Code))
      {
        export.UserInputCashReceiptSourceType.Code =
          export.HiddenUserInput.Code;
      }
      else
      {
        export.UserInputCashReceiptSourceType.Code =
          import.UserCashReceiptSourceType.Code;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETLAPS"))
    {
      export.PromptCourtOrder.PromptField = "";

      if (IsEmpty(import.CourtOrderFilter.StandardNumber))
      {
        export.CourtOrderFilter.StandardNumber =
          export.HiddenCourtOrderFilter.StandardNumber;
      }
      else
      {
        export.CourtOrderFilter.StandardNumber =
          import.CourtOrderFilter.StandardNumber;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETRSDL"))
    {
      export.PromptCollStat.PromptField = "";

      if (IsEmpty(import.CashReceiptDetailStatus.Code))
      {
        export.CashReceiptDetailStatus1.Code =
          export.HiddenCashReceiptDetailStatus.Code;
      }
      else
      {
        export.CashReceiptDetailStatus1.Code =
          import.CashReceiptDetailStatus.Code;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      export.PromptRsnCode.PromptField = "";

      if (IsEmpty(import.Dlgflow.Cdvalue))
      {
        export.CashReceiptDetailStatHistory1.ReasonCodeId =
          export.HiddenCashReceiptDetailStatHistory.ReasonCodeId;
      }
      else
      {
        export.CashReceiptDetailStatHistory1.ReasonCodeId =
          import.Dlgflow.Cdvalue;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "MCOL"))
    {
      if (Equal(export.CashReceiptDetailStatus2.Code, "REL") || Equal
        (export.CashReceiptDetailStatus2.Code, "DIST") || Equal
        (export.CashReceiptDetailStatus2.Code, "REF"))
      {
        ExitState = "ECO_LNK_TO_MANUAL_DIST_OF_COLL";

        return;
      }
      else if (Equal(export.CashReceiptDetailStatus2.Code, "SUSP") && (
        Equal(export.CashReceiptDetailStatHistory2.ReasonCodeId, "MANUALDIST") ||
        Equal
        (export.CashReceiptDetailStatHistory2.ReasonCodeId, "MANREAPPLY")))
      {
        local.FlowToMcol.Flag = "Y";
        global.Command = "RELEASE";
      }
      else
      {
        ExitState = "FN0000_CANT_FLOW_TO_MCOL";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "COLA":
        ExitState = "ECO_LNK_TO_REC_COLL_ADJMNT";

        break;
      case "":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        // -- Blank out any data that might be in the export group.
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(local.Blank.Index = 0; local.Blank.Index < local.Blank.Count; ++
          local.Blank.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Next();
        }

        // 06/06/03 GVandy  PR179707 Require user to enter some criteria before 
        // displaying.
        if (IsEmpty(export.FilterCsePerson.Number) && IsEmpty
          (export.FilterSsn.ObligorSocialSecurityNumber) && IsEmpty
          (export.CourtOrderFilter.StandardNumber) && IsEmpty
          (export.UserInputCashReceiptSourceType.Code) && IsEmpty
          (export.Select.Code) && IsEmpty
          (export.CashReceiptDetailStatHistory1.ReasonCodeId) && Equal
          (export.UserInputCashReceiptEvent.ReceivedDate, local.Null1.Date) && IsEmpty
          (export.UserInputServiceProvider.UserId) && export
          .UserInputCashReceipt.SequentialNumber == 0)
        {
          var field1 = GetField(export.FilterCsePerson, "number");

          field1.Error = true;

          var field2 =
            GetField(export.FilterSsn, "obligorSocialSecurityNumber");

          field2.Error = true;

          var field3 = GetField(export.CourtOrderFilter, "standardNumber");

          field3.Error = true;

          var field4 = GetField(export.UserInputCashReceiptSourceType, "code");

          field4.Error = true;

          var field5 = GetField(export.Select, "code");

          field5.Error = true;

          var field6 =
            GetField(export.CashReceiptDetailStatHistory1, "reasonCodeId");

          field6.Error = true;

          var field7 =
            GetField(export.UserInputCashReceiptEvent, "receivedDate");

          field7.Error = true;

          var field8 = GetField(export.UserInputServiceProvider, "userId");

          field8.Error = true;

          var field9 =
            GetField(export.UserInputCashReceipt, "sequentialNumber");

          field9.Error = true;

          ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

          return;
        }

        if (import.Starting.SequentialIdentifier > 0 && export
          .UserInputCashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.UserInputCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "SP0000_REQUIRED_FIELD_MISSING";

          return;
        }

        // ***--- Call CAB to get CSE person name ...
        //        OK to continue if person not found..
        // H00079312    12/06/99 PDP  Added FILTER of SSN and Source Type Code
        if (!IsEmpty(export.Select.Code) && IsEmpty
          (export.UserInputCashReceiptSourceType.Code))
        {
          var field1 = GetField(export.UserInputCashReceiptSourceType, "code");

          field1.Error = true;

          var field2 = GetField(export.Select, "code");

          field2.Error = true;

          ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";

          return;
        }

        if (IsEmpty(export.FilterCsePerson.Number) && IsEmpty
          (export.FilterSsn.ObligorSocialSecurityNumber))
        {
          export.FilterCsePersonsWorkSet.FormattedName = "";
          export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber = "";
          export.HiddenFilterCsePerson.Number = "";
        }

        if (!IsEmpty(export.FilterCsePerson.Number))
        {
          if (Equal(export.FilterCsePerson.Number,
            export.HiddenFilterCsePerson.Number))
          {
            goto Test1;
          }

          local.CsePersonsWorkSet.Number = export.FilterCsePerson.Number;
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.FilterSsn.ObligorSocialSecurityNumber = "";
            export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber =
              "";

            // H00079312-Re 02/08/00 PDP Changed order so check by SSN is done 
            // first.
            ExitState = "ACO_NN0000_ALL_OK";
            export.HiddenFilterCsePerson.Number = export.FilterCsePerson.Number;
            export.FilterCsePersonsWorkSet.FormattedName =
              "CSE Person NOT Found";

            goto Test1;
          }

          export.FilterCsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          if (Verify(local.CsePersonsWorkSet.Ssn, " 0") == 0)
          {
            local.CsePersonsWorkSet.Ssn = "";
          }

          export.FilterSsn.ObligorSocialSecurityNumber =
            local.CsePersonsWorkSet.Ssn;
          export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber =
            local.CsePersonsWorkSet.Ssn;
          export.HiddenFilterCsePerson.Number = export.FilterCsePerson.Number;
        }

Test1:

        // H00079312    12/06/99 PDP  Added FILTER of SSN and Source Type Code
        if (!IsEmpty(export.FilterSsn.ObligorSocialSecurityNumber))
        {
          if (Equal(export.FilterSsn.ObligorSocialSecurityNumber,
            export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber))
          {
            goto Test2;
          }

          local.CsePersonsWorkSet.Ssn =
            export.FilterSsn.ObligorSocialSecurityNumber ?? Spaces(9);
          UseFnReadCsePersonUsingSsnO();

          if (!IsEmpty(local.CsePersonsWorkSet.Number))
          {
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.FilterCsePerson.Number = "";
              export.HiddenFilterCsePerson.Number = "";
              export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber =
                "";
              export.FilterCsePersonsWorkSet.FormattedName = "";

              var field =
                GetField(export.FilterSsn, "obligorSocialSecurityNumber");

              field.Error = true;

              return;
            }

            export.FilterCsePersonsWorkSet.FormattedName =
              local.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.FilterCsePerson.Number = "";
            export.HiddenFilterCsePerson.Number = "";
            export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber =
              export.FilterSsn.ObligorSocialSecurityNumber ?? "";

            // H00079312-Re 02/08/00 PDP Changed order so check by SSN is done 
            // first.
            export.FilterCsePersonsWorkSet.FormattedName =
              "CSE Person NOT on ADABAS";

            goto Test2;
          }

          if (Verify(local.CsePersonsWorkSet.Ssn, " 0") == 0)
          {
            local.CsePersonsWorkSet.Ssn = "";
          }

          export.HiddenFilterCashReceiptDetail.ObligorSocialSecurityNumber =
            export.FilterSsn.ObligorSocialSecurityNumber ?? "";
          export.FilterCsePerson.Number = local.CsePersonsWorkSet.Number;
          export.HiddenFilterCsePerson.Number = local.CsePersonsWorkSet.Number;
        }

Test2:

        UseFnBuildListOfUndistCrds();

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        // *** PR# 84516 - Add new exitstate to make this message a warning.  It
        // is getting lost and causing details to be missed.  Sunya Sharp 01/12
        // /2000 ***
        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NW0000_LST_RETURNED_FULL";

          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST":
        switch(AsChar(export.PromptPerson.Text1))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptPerson, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptCourtOrder.PromptField))
        {
          case 'S':
            export.Dlgflw.Number = export.FilterCsePerson.Number;
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptCourtOrder, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptCashReceipt.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptCashReceipt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptSourceCode.SelectChar))
        {
          case '+':
            break;
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptSourceCode, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptCollStat.PromptField))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptCollStat, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptRsnCode.PromptField))
        {
          case '+':
            break;
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptRsnCode, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        // H00079312    12/06/99 PDP  Added FILTER of SSN and Source Type Code
        // H00079312    12/06/99 PDP  Link to retrive Source Type Code
        switch(AsChar(export.PromtCollType.SelectChar))
        {
          case '+':
            break;
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromtCollType, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

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

        if (AsChar(export.PromptPerson.Text1) == 'S')
        {
          // **** Link to list person name screen
          export.PromptPerson.Text1 = "+";
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

          return;
        }

        // H00079312    12/06/99 PDP  Added FILTER of SSN and Source Type Code
        // H00079312    12/06/99 PDP  Link to retrive Source Type Code
        if (AsChar(export.PromtCollType.SelectChar) == 'S')
        {
          // **** Link to CLCT screen
          export.PromtCollType.SelectChar = "+";
          ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";

          return;
        }

        if (AsChar(export.PromptCourtOrder.PromptField) == 'S')
        {
          // **** Link to LAPS screen
          export.PromptCourtOrder.PromptField = "+";
          export.HiddenCourtOrderFilter.StandardNumber =
            export.CourtOrderFilter.StandardNumber;
          ExitState = "ECO_LNK_TO_LAPS";

          return;
        }

        if (AsChar(export.PromptSourceCode.SelectChar) == 'S')
        {
          // **** Link to list Cash Receipt Source Code screen
          export.PromptSourceCode.SelectChar = "+";
          export.HiddenUserInput.Code =
            export.UserInputCashReceiptSourceType.Code;
          ExitState = "ECO_LNK_TO_CASH_RECEIPT_SRC_TYPE";

          return;
        }
        else
        {
        }

        if (AsChar(export.PromptCashReceipt.SelectChar) == 'S')
        {
          // **** Link to list Cash Receipt Number screen
          export.PromptCashReceipt.SelectChar = "+";
          ExitState = "ECO_LNK_TO_LST_CASH_RECEIPT";

          return;
        }
        else
        {
        }

        if (AsChar(export.PromptCollStat.PromptField) == 'S')
        {
          // **** Link to list Cash Receipt Number screen
          export.PromptCollStat.PromptField = "+";
          export.HiddenCashReceiptDetailStatus.Code =
            export.CashReceiptDetailStatus1.Code;
          ExitState = "ECO_LNK_TO_LST_CASH_RCPT_DTL_ST";

          return;
        }
        else
        {
        }

        if (AsChar(export.PromptRsnCode.PromptField) == 'S')
        {
          // **** Link to list Cash Receipt Number screen
          export.Dlgflow.CodeName = "PEND/SUSP REASON";
          export.PromptRsnCode.PromptField = "+";
          export.HiddenCashReceiptDetailStatHistory.ReasonCodeId =
            export.CashReceiptDetailStatHistory1.ReasonCodeId;
          ExitState = "ECO_LNK_TO_CDVL1";
        }
        else
        {
        }

        break;
      case "DETAIL":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            export.Export1.Update.Sel.SelectChar = "";
            ExitState = "ECO_XFR_TO_RECORD_COLLECTION";

            return;
          }
        }

        ExitState = "OE0000_NO_RECORD_SELECTED";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "REFUND":
        if (Equal(export.CashReceiptDetailStatus2.Code, "PEND"))
        {
          ExitState = "FN0000_CANT_FLOW_TO_CRRU_PEND";
        }
        else
        {
          // **** Transfer control to Refund screen with display first
          ExitState = "ECO_XFR_TO_REFUND_MAIN_SCREEN";
        }

        break;
      case "RELEASE":
        // *******************************************************************
        // *This
        // function will implement the Release Collection action block *
        // 
        // *for processing each line that is marked with an "s".             *
        // 
        // *******************************************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            if (Equal(export.Export1.Item.CashReceiptDetailStatus.Code, "SUSP"))
            {
              if (!Equal(export.Export1.Item.CashReceiptDetailStatHistory.
                ReasonCodeId, "CTORSNN") && !
                Equal(export.Export1.Item.CashReceiptDetailStatHistory.
                  ReasonCodeId, "INVCOLTYPE") && !
                Equal(export.Export1.Item.CashReceiptDetailStatHistory.
                  ReasonCodeId, "INVMPIND") && !
                Equal(export.Export1.Item.CashReceiptDetailStatHistory.
                  ReasonCodeId, "INVSSN") && !
                Equal(export.Export1.Item.CashReceiptDetailStatHistory.
                  ReasonCodeId, "MULTIPAYOR"))
              {
                // ********Okay to Release********
              }
              else
              {
                var field1 =
                  GetField(export.Export1.Item.CashReceiptDetailStatHistory,
                  "reasonCodeId");

                field1.Error = true;

                var field2 = GetField(export.Export1.Item.Sel, "selectChar");

                field2.Error = true;

                ExitState = "FN0000_NO_REL_WITH_STAT_OR_REAS";

                return;
              }
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.CashReceiptDetailStatHistory,
                "reasonCodeId");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Sel, "selectChar");

              field2.Error = true;

              ExitState = "FN0000_NO_REL_WITH_STAT_OR_REAS";

              return;
            }
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            local.CashReceipt.SequentialNumber =
              export.Export1.Item.CashReceipt.SequentialNumber;

            if (ReadCashReceiptEvent())
            {
              local.CashReceiptEvent.SystemGeneratedIdentifier =
                entities.CashReceiptEvent.SystemGeneratedIdentifier;
            }
            else
            {
              ExitState = "FN0000_CASH_RCPT_EVENT_NF";

              return;
            }

            if (ReadCashReceiptType())
            {
              local.CashReceiptType.SystemGeneratedIdentifier =
                entities.CashReceiptType.SystemGeneratedIdentifier;
            }
            else
            {
              ExitState = "FN0113_CASH_RCPT_TYPE_NF";

              return;
            }

            // *************************************************************
            // 
            // Edit Check Before Release
            // *************************************************************
            if (IsEmpty(export.Export1.Item.CashReceiptDetail.CaseNumber) && IsEmpty
              (export.Export1.Item.CashReceiptDetail.CourtOrderNumber) && IsEmpty
              (export.Export1.Item.CashReceiptDetail.ObligorPersonNumber))
            {
              var field = GetField(export.Export1.Item.Sel, "selectChar");

              field.Error = true;

              ExitState = "FN0000_COLL_RELEASE_INFO_MISSING";

              return;
            }

            if (!IsEmpty(export.Export1.Item.CashReceiptDetail.CaseNumber))
            {
              local.Case1.Number =
                export.Export1.Item.CashReceiptDetail.CaseNumber ?? Spaces(10);

              // *************Use FN_READ_CASE_WITH_NUMBER********************
              UseFnReadCaseWithNumber();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                ExitState = "FN0000_INVALID_CASE_NUMBR_ON_COL";

                var field = GetField(export.Export1.Item.Sel, "selectChar");

                field.Error = true;

                return;
              }
            }

            if (!IsEmpty(export.Export1.Item.CashReceiptDetail.CourtOrderNumber))
              
            {
              local.LegalAction.StandardNumber =
                export.Export1.Item.CashReceiptDetail.CourtOrderNumber ?? "";

              // ****************Use FN_READ_STANDARD_COURT_ORDER**************
              UseFnReadStandardCourtOrder();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                ExitState = "FN0000_INVALID_LA_FOR_COLL";

                var field = GetField(export.Export1.Item.Sel, "selectChar");

                field.Error = true;

                return;
              }

              // H0098148 HERE V V V V V
              if (!IsEmpty(export.Export1.Item.CashReceiptDetail.
                ObligorPersonNumber))
              {
              }
              else
              {
                UseFnAbObligorListForCtOrder();

                if (local.WorkNumberOfObligors.Count > 1)
                {
                  var field = GetField(export.Export1.Item.Sel, "selectChar");

                  field.Error = true;

                  ExitState = "FN0000_MULTIPAYOR_NEEDS_CSE_NBR";

                  return;
                }

                if (local.WorkNumberOfObligors.Count == 0)
                {
                  if (!IsEmpty(export.Export1.Item.CashReceiptDetail.
                    ObligorPersonNumber))
                  {
                    goto Test3;
                  }

                  var field = GetField(export.Export1.Item.Sel, "selectChar");

                  field.Error = true;

                  ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

                  return;
                }

                for(local.ObligorList.Index = 0; local.ObligorList.Index < local
                  .ObligorList.Count; ++local.ObligorList.Index)
                {
                  local.UpdateName.FirstName =
                    local.ObligorList.Item.GrpsWork.FirstName;
                  local.UpdateName.LastName =
                    local.ObligorList.Item.GrpsWork.LastName;
                  local.UpdateName.MiddleInitial =
                    local.ObligorList.Item.GrpsWork.MiddleInitial;
                  local.UpdateName.Sex = local.ObligorList.Item.GrpsWork.Sex;

                  if (!IsEmpty(local.ObligorList.Item.GrpsWork.Number))
                  {
                    export.Export1.Update.CashReceiptDetail.
                      ObligorPersonNumber =
                        local.ObligorList.Item.GrpsWork.Number;
                    export.Export1.Update.CashReceiptDetail.
                      ObligorSocialSecurityNumber =
                        local.ObligorList.Item.GrpsWork.Ssn;

                    break;
                  }

                  if (!IsEmpty(local.ObligorList.Item.Grps.Number))
                  {
                    export.Export1.Update.CashReceiptDetail.
                      ObligorPersonNumber = local.ObligorList.Item.Grps.Number;
                    export.Export1.Update.CashReceiptDetail.
                      ObligorSocialSecurityNumber =
                        local.ObligorList.Item.Grps.TaxId ?? "";

                    break;
                  }
                }
              }
            }

Test3:

            // H0098148 HERE ^ ^ ^ ^ ^
            if (!IsEmpty(export.Export1.Item.CashReceiptDetail.
              ObligorPersonNumber))
            {
              local.CsePerson.Number =
                export.Export1.Item.CashReceiptDetail.ObligorPersonNumber ?? Spaces
                (10);

              if (ReadCsePerson())
              {
                local.UpdateNamePhone.ObligorPhoneNumber = "";

                if (Lt(0, entities.CsePerson.HomePhone))
                {
                  if (Lt(0, entities.CsePerson.HomePhoneAreaCode))
                  {
                    local.UpdateNamePhone.ObligorPhoneNumber =
                      NumberToString(entities.CsePerson.HomePhoneAreaCode.
                        GetValueOrDefault(), 15) + NumberToString
                      (entities.CsePerson.HomePhone.GetValueOrDefault(), 15);

                    goto Read;
                  }

                  local.UpdateNamePhone.ObligorPhoneNumber =
                    NumberToString(entities.CsePerson.HomePhone.
                      GetValueOrDefault(), 12);
                }
              }
              else
              {
                var field = GetField(export.Export1.Item.Sel, "selectChar");

                field.Error = true;

                ExitState = "FN0000_CSE_PERSON_NF_ON_CRDTL";

                return;
              }

Read:

              if (!IsEmpty(export.Export1.Item.CashReceiptDetail.CaseNumber))
              {
                if (!ReadCaseCaseRole())
                {
                  ExitState = "FN0000_AP_CASE_MISMATCH";

                  var field = GetField(export.Export1.Item.Sel, "selectChar");

                  field.Error = true;

                  return;
                }
              }

              if (!IsEmpty(export.Export1.Item.CashReceiptDetail.
                CourtOrderNumber))
              {
                if (!ReadLegalActionPerson())
                {
                  ExitState = "FN0000_CRT_ORDR_CSE_PRSN_MSMTCH";

                  var field = GetField(export.Export1.Item.Sel, "selectChar");

                  field.Error = true;

                  return;
                }
              }
            }

            // H0098148 HERE V V V V V
            if (Equal(export.Export1.Item.CashReceiptDetailStatHistory.
              ReasonCodeId, "INVCTORDER") || Equal
              (export.Export1.Item.CashReceiptDetailStatHistory.ReasonCodeId,
              "INVPERSNBR"))
            {
              if (ReadCashReceiptDetail())
              {
                try
                {
                  UpdateCashReceiptDetail();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      var field1 =
                        GetField(export.Export1.Item.Sel, "selectChar");

                      field1.Error = true;

                      ExitState = "FN0000_COLLECTION_NU_RB";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      var field2 =
                        GetField(export.Export1.Item.Sel, "selectChar");

                      field2.Error = true;

                      ExitState = "FN0000_COLLECTION_PV_RB";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            // H0098148 HERE ^ ^ ^ ^ ^
            // **************Use FN_AB_DETERMINE_COLL_AMT_APPLIED***************
            UseFnAbDetermineCollAmtApplied();

            if (local.CollAmtApplied.TotalCurrency + local
              .TotalRefunded.TotalCurrency + local
              .TotalAdjusted.TotalCurrency < export
              .Export1.Item.CashReceipt.ReceiptAmount)
            {
              ExitState = "FN0000_UNACCOUNTD_MONEY_REMAINNG";

              var field = GetField(export.Export1.Item.Sel, "selectChar");

              field.Error = true;

              return;
            }

            // *********Add Use RELEASE_COLLECTION***************
            if (Equal(export.Export1.Item.CashReceiptDetailStatus.Code, "SUSP"))
            {
              export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
                "";
            }

            UseReleaseCollection();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (AsChar(local.FlowToMcol.Flag) == 'Y')
              {
                export.Export1.Update.Sel.SelectChar = "";
                local.FlowToMcol.Flag = "";
                ExitState = "ECO_LNK_TO_MANUAL_DIST_OF_COLL";

                return;
              }
              else
              {
                export.Export1.Update.Sel.SelectChar = "";
                export.Export1.Update.CashReceiptDetailStatHistory.
                  ReasonCodeId = "";
              }
            }
            else
            {
              // *******Release Collection Failed********
              var field = GetField(export.Export1.Item.Sel, "selectChar");

              field.Error = true;
            }
          }
        }

        ExitState = "FN0000_COLL_RELEASE_SUCCESSFUL";

        break;
      default:
        if (Equal(global.Command, "ENTER"))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }

        break;
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.ReceivedDate = source.ReceivedDate;
    target.CreatedBy = source.CreatedBy;
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
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.UnemploymentInd = source.UnemploymentInd;
    target.FederalInd = source.FederalInd;
    target.TaxIdSuffix = source.TaxIdSuffix;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(FnBuildListOfUndistCrds.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Sel.SelectChar = source.Common.SelectChar;
    target.Detail.CrdCrCombo = source.Detail.CrdCrCombo;
    MoveCashReceiptSourceType(source.HiddenCashReceiptSourceType,
      target.CashReceiptSourceType);
    target.CashReceiptDetailStatus.Assign(source.HiddenCashReceiptDetailStatus);
    target.CashReceiptDetailStatHistory.ReasonCodeId =
      source.CashReceiptDetailStatHistory.ReasonCodeId;
    target.CashReceipt.Assign(source.CashReceipt);
    target.UndistAmt.TotalCurrency = source.UndistAmt.TotalCurrency;
    target.CashReceiptDetail.Assign(source.CashReceiptDetail);
    target.ScreenDisplay.CourtOrderNumber =
      source.ScreenDisplay.CourtOrderNumber;
    target.HiddenCashReceiptType.SystemGeneratedIdentifier =
      source.HiddenCashReceiptType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(source.HiddenCashReceiptEvent,
      target.HiddenCashReceiptEvent);
    target.Filter.Code = source.CollectionType.Code;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
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

  private static void MoveObligorList(FnAbObligorListForCtOrder.Export.
    ObligorListGroup source, Local.ObligorListGroup target)
  {
    MoveCsePerson(source.Grps, target.Grps);
    target.GrpsWork.Assign(source.GrpsWork);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.PadCsePersonNumber.Text10;
    useExport.TextWorkArea.Text10 = local.PadCsePersonNumber.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.PadCsePersonNumber.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnAbDetermineCollAmtApplied()
  {
    var useImport = new FnAbDetermineCollAmtApplied.Import();
    var useExport = new FnAbDetermineCollAmtApplied.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.Export1.Item.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptDetail2(export.Export1.Item.CashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;

    Call(FnAbDetermineCollAmtApplied.Execute, useImport, useExport);

    local.TotalRefunded.TotalCurrency = useExport.TotalRefunded.TotalCurrency;
    local.TotalAdjusted.TotalCurrency = useExport.TotalAdjusted.TotalCurrency;
    local.CollAmtApplied.TotalCurrency = useExport.CollAmtApplied.TotalCurrency;
  }

  private void UseFnAbObligorListForCtOrder()
  {
    var useImport = new FnAbObligorListForCtOrder.Import();
    var useExport = new FnAbObligorListForCtOrder.Export();

    useImport.CashReceiptDetail.CourtOrderNumber =
      export.Export1.Item.CashReceiptDetail.CourtOrderNumber;

    Call(FnAbObligorListForCtOrder.Execute, useImport, useExport);

    local.WorkNumberOfObligors.Count = useExport.WorkNoOfObligors.Count;
    useExport.ObligorList.CopyTo(local.ObligorList, MoveObligorList);
  }

  private void UseFnBuildListOfUndistCrds()
  {
    var useImport = new FnBuildListOfUndistCrds.Import();
    var useExport = new FnBuildListOfUndistCrds.Export();

    useImport.UserInputStarting.SequentialIdentifier =
      import.Starting.SequentialIdentifier;
    useImport.DelMe.Ssn = local.CsePersonsWorkSet.Ssn;
    useImport.PayHistoryIndicator.Flag = export.PayHistoryIndicator.Flag;
    useImport.UserCashReceiptEvent.ReceivedDate =
      export.UserInputCashReceiptEvent.ReceivedDate;
    useImport.UserLegalAction.StandardNumber =
      export.CourtOrderFilter.StandardNumber;
    useImport.UserCsePerson.Number = export.FilterCsePerson.Number;
    useImport.UserCashReceiptDetailStatHistory.ReasonCodeId =
      export.CashReceiptDetailStatHistory1.ReasonCodeId;
    useImport.UserCashReceiptDetailStatus.Code =
      export.CashReceiptDetailStatus1.Code;
    useImport.UserServiceProvider.UserId =
      export.UserInputServiceProvider.UserId;
    useImport.UserCashReceipt.SequentialNumber =
      export.UserInputCashReceipt.SequentialNumber;
    useImport.UserCashReceiptSourceType.Code =
      export.UserInputCashReceiptSourceType.Code;
    useImport.UserInputFilterSsn.ObligorSocialSecurityNumber =
      export.FilterSsn.ObligorSocialSecurityNumber;
    useImport.SelectedFilter.Code = export.Select.Code;

    Call(FnBuildListOfUndistCrds.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseFnReadCaseWithNumber()
  {
    var useImport = new FnReadCaseWithNumber.Import();
    var useExport = new FnReadCaseWithNumber.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(FnReadCaseWithNumber.Execute, useImport, useExport);
  }

  private void UseFnReadCsePersonUsingSsnO()
  {
    var useImport = new FnReadCsePersonUsingSsnO.Import();
    var useExport = new FnReadCsePersonUsingSsnO.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);

    Call(FnReadCsePersonUsingSsnO.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseFnReadStandardCourtOrder()
  {
    var useImport = new FnReadStandardCourtOrder.Import();
    var useExport = new FnReadStandardCourtOrder.Export();

    MoveLegalAction(local.LegalAction, useImport.LegalAction);

    Call(FnReadStandardCourtOrder.Execute, useImport, useExport);
  }

  private void UseReleaseCollection()
  {
    var useImport = new ReleaseCollection.Import();
    var useExport = new ReleaseCollection.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.Export1.Item.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      export.Export1.Item.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatHistory.ReasonCodeId =
      export.Export1.Item.CashReceiptDetailStatHistory.ReasonCodeId;
    useImport.CashReceipt.SequentialNumber =
      export.Export1.Item.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.Export1.Item.CashReceiptDetail.SequentialIdentifier;

    Call(ReleaseCollection.Execute, useImport, useExport);

    MoveCashReceiptDetailStatus(useExport.CashReceiptDetailStatus,
      export.Export1.Update.CashReceiptDetailStatus);
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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCaseCaseRole()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CourtOrder.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          export.Export1.Item.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId",
          export.Export1.Item.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CourtOrder.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CourtOrder.CstIdentifier = db.GetInt32(reader, 1);
        entities.CourtOrder.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CourtOrder.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.CourtOrder.CourtOrderNumber = db.GetNullableString(reader, 4);
        entities.CourtOrder.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
        entities.CourtOrder.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 6);
        entities.CourtOrder.ObligorFirstName = db.GetNullableString(reader, 7);
        entities.CourtOrder.ObligorLastName = db.GetNullableString(reader, 8);
        entities.CourtOrder.ObligorMiddleName = db.GetNullableString(reader, 9);
        entities.CourtOrder.ObligorPhoneNumber =
          db.GetNullableString(reader, 10);
        entities.CourtOrder.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.CourtOrder.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.CourtOrder.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.
          SetInt32(command, "cashReceiptId", local.CashReceipt.SequentialNumber);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.
          SetInt32(command, "cashReceiptId", local.CashReceipt.SequentialNumber);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo",
          export.Export1.Item.CashReceiptDetail.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CourtOrder.Populated);

    var courtOrderNumber =
      export.Export1.Item.CashReceiptDetail.CourtOrderNumber ?? "";
    var obligorPersonNumber =
      export.Export1.Item.CashReceiptDetail.ObligorPersonNumber ?? "";
    var obligorSocialSecurityNumber =
      export.Export1.Item.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
    var obligorFirstName = local.UpdateName.FirstName;
    var obligorLastName = local.UpdateName.LastName;
    var obligorMiddleName = local.UpdateName.MiddleInitial;
    var obligorPhoneNumber = local.UpdateNamePhone.ObligorPhoneNumber ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.CourtOrder.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "oblgorPrsnNbr", obligorPersonNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMidNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", obligorPhoneNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetInt32(command, "crvIdentifier", entities.CourtOrder.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CourtOrder.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CourtOrder.CrtIdentifier);
          
        db.SetInt32(command, "crdId", entities.CourtOrder.SequentialIdentifier);
      });

    entities.CourtOrder.CourtOrderNumber = courtOrderNumber;
    entities.CourtOrder.ObligorPersonNumber = obligorPersonNumber;
    entities.CourtOrder.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CourtOrder.ObligorFirstName = obligorFirstName;
    entities.CourtOrder.ObligorLastName = obligorLastName;
    entities.CourtOrder.ObligorMiddleName = obligorMiddleName;
    entities.CourtOrder.ObligorPhoneNumber = obligorPhoneNumber;
    entities.CourtOrder.LastUpdatedBy = lastUpdatedBy;
    entities.CourtOrder.LastUpdatedTmst = lastUpdatedTmst;
    entities.CourtOrder.Populated = true;
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
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CrdCrComboNo Detail
      {
        get => detail ??= new();
        set => detail = value;
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
      /// A value of UndistAmt.
      /// </summary>
      [JsonPropertyName("undistAmt")]
      public Common UndistAmt
      {
        get => undistAmt ??= new();
        set => undistAmt = value;
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
      /// A value of ScreenDisplay.
      /// </summary>
      [JsonPropertyName("screenDisplay")]
      public CashReceiptDetail ScreenDisplay
      {
        get => screenDisplay ??= new();
        set => screenDisplay = value;
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
      /// A value of HiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptEvent")]
      public CashReceiptEvent HiddenCashReceiptEvent
      {
        get => hiddenCashReceiptEvent ??= new();
        set => hiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of Filter.
      /// </summary>
      [JsonPropertyName("filter")]
      public CollectionType Filter
      {
        get => filter ??= new();
        set => filter = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 55;

      private Common sel;
      private CrdCrComboNo detail;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptDetailStatus cashReceiptDetailStatus;
      private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
      private CashReceipt cashReceipt;
      private Common undistAmt;
      private CashReceiptDetail cashReceiptDetail;
      private CashReceiptDetail screenDisplay;
      private CashReceiptType hiddenCashReceiptType;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CollectionType filter;
    }

    /// <summary>
    /// A value of PayHistoryIndicator.
    /// </summary>
    [JsonPropertyName("payHistoryIndicator")]
    public Common PayHistoryIndicator
    {
      get => payHistoryIndicator ??= new();
      set => payHistoryIndicator = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory HiddenCashReceiptDetailStatHistory
    {
      get => hiddenCashReceiptDetailStatHistory ??= new();
      set => hiddenCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    /// <summary>
    /// A value of UserCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("userCashReceiptEvent")]
    public CashReceiptEvent UserCashReceiptEvent
    {
      get => userCashReceiptEvent ??= new();
      set => userCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDetailStatus")]
    public CashReceiptDetailStatus HiddenCashReceiptDetailStatus
    {
      get => hiddenCashReceiptDetailStatus ??= new();
      set => hiddenCashReceiptDetailStatus = value;
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
    /// A value of HiddenCourtOrderFilter.
    /// </summary>
    [JsonPropertyName("hiddenCourtOrderFilter")]
    public LegalAction HiddenCourtOrderFilter
    {
      get => hiddenCourtOrderFilter ??= new();
      set => hiddenCourtOrderFilter = value;
    }

    /// <summary>
    /// A value of HiddenUser.
    /// </summary>
    [JsonPropertyName("hiddenUser")]
    public CashReceiptSourceType HiddenUser
    {
      get => hiddenUser ??= new();
      set => hiddenUser = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceiptDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of PromptCourtOrder.
    /// </summary>
    [JsonPropertyName("promptCourtOrder")]
    public Standard PromptCourtOrder
    {
      get => promptCourtOrder ??= new();
      set => promptCourtOrder = value;
    }

    /// <summary>
    /// A value of CourtOrderFilter.
    /// </summary>
    [JsonPropertyName("courtOrderFilter")]
    public LegalAction CourtOrderFilter
    {
      get => courtOrderFilter ??= new();
      set => courtOrderFilter = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public CsePersonsWorkSet Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public TextWorkArea PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of Dlgflow.
    /// </summary>
    [JsonPropertyName("dlgflow")]
    public CodeValue Dlgflow
    {
      get => dlgflow ??= new();
      set => dlgflow = value;
    }

    /// <summary>
    /// A value of PromptRsnCode.
    /// </summary>
    [JsonPropertyName("promptRsnCode")]
    public Standard PromptRsnCode
    {
      get => promptRsnCode ??= new();
      set => promptRsnCode = value;
    }

    /// <summary>
    /// A value of PromptCollStat.
    /// </summary>
    [JsonPropertyName("promptCollStat")]
    public Standard PromptCollStat
    {
      get => promptCollStat ??= new();
      set => promptCollStat = value;
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
    /// A value of UserServiceProvider.
    /// </summary>
    [JsonPropertyName("userServiceProvider")]
    public ServiceProvider UserServiceProvider
    {
      get => userServiceProvider ??= new();
      set => userServiceProvider = value;
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
    /// A value of UserCashReceipt.
    /// </summary>
    [JsonPropertyName("userCashReceipt")]
    public CashReceipt UserCashReceipt
    {
      get => userCashReceipt ??= new();
      set => userCashReceipt = value;
    }

    /// <summary>
    /// A value of UserCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userCashReceiptSourceType")]
    public CashReceiptSourceType UserCashReceiptSourceType
    {
      get => userCashReceiptSourceType ??= new();
      set => userCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PromptCashReceipt.
    /// </summary>
    [JsonPropertyName("promptCashReceipt")]
    public Common PromptCashReceipt
    {
      get => promptCashReceipt ??= new();
      set => promptCashReceipt = value;
    }

    /// <summary>
    /// A value of PromptSourceCode.
    /// </summary>
    [JsonPropertyName("promptSourceCode")]
    public Common PromptSourceCode
    {
      get => promptSourceCode ??= new();
      set => promptSourceCode = value;
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
    /// A value of FilterSsn.
    /// </summary>
    [JsonPropertyName("filterSsn")]
    public CashReceiptDetail FilterSsn
    {
      get => filterSsn ??= new();
      set => filterSsn = value;
    }

    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public CollectionType Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of PromptCollType.
    /// </summary>
    [JsonPropertyName("promptCollType")]
    public Common PromptCollType
    {
      get => promptCollType ??= new();
      set => promptCollType = value;
    }

    /// <summary>
    /// A value of HiddenFilterCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("hiddenFilterCashReceiptDetail")]
    public CashReceiptDetail HiddenFilterCashReceiptDetail
    {
      get => hiddenFilterCashReceiptDetail ??= new();
      set => hiddenFilterCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of HiddenFilterCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenFilterCsePerson")]
    public CsePerson HiddenFilterCsePerson
    {
      get => hiddenFilterCsePerson ??= new();
      set => hiddenFilterCsePerson = value;
    }

    private Common payHistoryIndicator;
    private CashReceiptDetailStatHistory hiddenCashReceiptDetailStatHistory;
    private Security2 security;
    private CashReceiptEvent userCashReceiptEvent;
    private CashReceiptDetailStatus hiddenCashReceiptDetailStatus;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private LegalAction hiddenCourtOrderFilter;
    private CashReceiptSourceType hiddenUser;
    private CashReceiptDetail starting;
    private Standard promptCourtOrder;
    private LegalAction courtOrderFilter;
    private CsePersonsWorkSet name;
    private CsePersonsWorkSet filter;
    private TextWorkArea promptPerson;
    private CsePerson csePerson;
    private CodeValue dlgflow;
    private Standard promptRsnCode;
    private Standard promptCollStat;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private ServiceProvider userServiceProvider;
    private Array<ImportGroup> import1;
    private CashReceipt userCashReceipt;
    private CashReceiptSourceType userCashReceiptSourceType;
    private Common promptCashReceipt;
    private Common promptSourceCode;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CashReceiptDetail filterSsn;
    private CollectionType select;
    private Common promptCollType;
    private CashReceiptDetail hiddenFilterCashReceiptDetail;
    private CsePerson hiddenFilterCsePerson;
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
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CrdCrComboNo Detail
      {
        get => detail ??= new();
        set => detail = value;
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
      /// A value of UndistAmt.
      /// </summary>
      [JsonPropertyName("undistAmt")]
      public Common UndistAmt
      {
        get => undistAmt ??= new();
        set => undistAmt = value;
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
      /// A value of ScreenDisplay.
      /// </summary>
      [JsonPropertyName("screenDisplay")]
      public CashReceiptDetail ScreenDisplay
      {
        get => screenDisplay ??= new();
        set => screenDisplay = value;
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
      /// A value of HiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptEvent")]
      public CashReceiptEvent HiddenCashReceiptEvent
      {
        get => hiddenCashReceiptEvent ??= new();
        set => hiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of Filter.
      /// </summary>
      [JsonPropertyName("filter")]
      public CollectionType Filter
      {
        get => filter ??= new();
        set => filter = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 55;

      private Common sel;
      private CrdCrComboNo detail;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptDetailStatus cashReceiptDetailStatus;
      private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
      private CashReceipt cashReceipt;
      private Common undistAmt;
      private CashReceiptDetail cashReceiptDetail;
      private CashReceiptDetail screenDisplay;
      private CashReceiptType hiddenCashReceiptType;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CollectionType filter;
    }

    /// <summary>
    /// A value of PayHistoryIndicator.
    /// </summary>
    [JsonPropertyName("payHistoryIndicator")]
    public Common PayHistoryIndicator
    {
      get => payHistoryIndicator ??= new();
      set => payHistoryIndicator = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory HiddenCashReceiptDetailStatHistory
    {
      get => hiddenCashReceiptDetailStatHistory ??= new();
      set => hiddenCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    /// <summary>
    /// A value of UserInputCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("userInputCashReceiptEvent")]
    public CashReceiptEvent UserInputCashReceiptEvent
    {
      get => userInputCashReceiptEvent ??= new();
      set => userInputCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDetailStatus")]
    public CashReceiptDetailStatus HiddenCashReceiptDetailStatus
    {
      get => hiddenCashReceiptDetailStatus ??= new();
      set => hiddenCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of HiddenCourtOrderFilter.
    /// </summary>
    [JsonPropertyName("hiddenCourtOrderFilter")]
    public LegalAction HiddenCourtOrderFilter
    {
      get => hiddenCourtOrderFilter ??= new();
      set => hiddenCourtOrderFilter = value;
    }

    /// <summary>
    /// A value of HiddenUserInput.
    /// </summary>
    [JsonPropertyName("hiddenUserInput")]
    public CashReceiptSourceType HiddenUserInput
    {
      get => hiddenUserInput ??= new();
      set => hiddenUserInput = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceiptDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public CsePersonsWorkSet Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// A value of PromptCourtOrder.
    /// </summary>
    [JsonPropertyName("promptCourtOrder")]
    public Standard PromptCourtOrder
    {
      get => promptCourtOrder ??= new();
      set => promptCourtOrder = value;
    }

    /// <summary>
    /// A value of CourtOrderFilter.
    /// </summary>
    [JsonPropertyName("courtOrderFilter")]
    public LegalAction CourtOrderFilter
    {
      get => courtOrderFilter ??= new();
      set => courtOrderFilter = value;
    }

    /// <summary>
    /// A value of FilterCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("filterCsePersonsWorkSet")]
    public CsePersonsWorkSet FilterCsePersonsWorkSet
    {
      get => filterCsePersonsWorkSet ??= new();
      set => filterCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public TextWorkArea PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of FilterCsePerson.
    /// </summary>
    [JsonPropertyName("filterCsePerson")]
    public CsePerson FilterCsePerson
    {
      get => filterCsePerson ??= new();
      set => filterCsePerson = value;
    }

    /// <summary>
    /// A value of Dlgflow.
    /// </summary>
    [JsonPropertyName("dlgflow")]
    public Code Dlgflow
    {
      get => dlgflow ??= new();
      set => dlgflow = value;
    }

    /// <summary>
    /// A value of PromptRsnCode.
    /// </summary>
    [JsonPropertyName("promptRsnCode")]
    public Standard PromptRsnCode
    {
      get => promptRsnCode ??= new();
      set => promptRsnCode = value;
    }

    /// <summary>
    /// A value of PromptCollStat.
    /// </summary>
    [JsonPropertyName("promptCollStat")]
    public Standard PromptCollStat
    {
      get => promptCollStat ??= new();
      set => promptCollStat = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory1.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory1")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory1
    {
      get => cashReceiptDetailStatHistory1 ??= new();
      set => cashReceiptDetailStatHistory1 = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus1.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus1")]
    public CashReceiptDetailStatus CashReceiptDetailStatus1
    {
      get => cashReceiptDetailStatus1 ??= new();
      set => cashReceiptDetailStatus1 = value;
    }

    /// <summary>
    /// A value of UserInputServiceProvider.
    /// </summary>
    [JsonPropertyName("userInputServiceProvider")]
    public ServiceProvider UserInputServiceProvider
    {
      get => userInputServiceProvider ??= new();
      set => userInputServiceProvider = value;
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
    /// A value of UserInputCashReceipt.
    /// </summary>
    [JsonPropertyName("userInputCashReceipt")]
    public CashReceipt UserInputCashReceipt
    {
      get => userInputCashReceipt ??= new();
      set => userInputCashReceipt = value;
    }

    /// <summary>
    /// A value of UserInputCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userInputCashReceiptSourceType")]
    public CashReceiptSourceType UserInputCashReceiptSourceType
    {
      get => userInputCashReceiptSourceType ??= new();
      set => userInputCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail1.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail1")]
    public CashReceiptDetail CashReceiptDetail1
    {
      get => cashReceiptDetail1 ??= new();
      set => cashReceiptDetail1 = value;
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
    /// A value of CashReceiptDetailStatus2.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus2")]
    public CashReceiptDetailStatus CashReceiptDetailStatus2
    {
      get => cashReceiptDetailStatus2 ??= new();
      set => cashReceiptDetailStatus2 = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory2.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory2")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory2
    {
      get => cashReceiptDetailStatHistory2 ??= new();
      set => cashReceiptDetailStatHistory2 = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of PromptSourceCode.
    /// </summary>
    [JsonPropertyName("promptSourceCode")]
    public Common PromptSourceCode
    {
      get => promptSourceCode ??= new();
      set => promptSourceCode = value;
    }

    /// <summary>
    /// A value of PromptCashReceipt.
    /// </summary>
    [JsonPropertyName("promptCashReceipt")]
    public Common PromptCashReceipt
    {
      get => promptCashReceipt ??= new();
      set => promptCashReceipt = value;
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
    /// A value of CashReceiptDetail2.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail2")]
    public CashReceiptDetail CashReceiptDetail2
    {
      get => cashReceiptDetail2 ??= new();
      set => cashReceiptDetail2 = value;
    }

    /// <summary>
    /// A value of FilterSsn.
    /// </summary>
    [JsonPropertyName("filterSsn")]
    public CashReceiptDetail FilterSsn
    {
      get => filterSsn ??= new();
      set => filterSsn = value;
    }

    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public CollectionType Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of PromtCollType.
    /// </summary>
    [JsonPropertyName("promtCollType")]
    public Common PromtCollType
    {
      get => promtCollType ??= new();
      set => promtCollType = value;
    }

    /// <summary>
    /// A value of HiddenFilterCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("hiddenFilterCashReceiptDetail")]
    public CashReceiptDetail HiddenFilterCashReceiptDetail
    {
      get => hiddenFilterCashReceiptDetail ??= new();
      set => hiddenFilterCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of HiddenFilterCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenFilterCsePerson")]
    public CsePerson HiddenFilterCsePerson
    {
      get => hiddenFilterCsePerson ??= new();
      set => hiddenFilterCsePerson = value;
    }

    private Common payHistoryIndicator;
    private CashReceiptDetailStatHistory hiddenCashReceiptDetailStatHistory;
    private Security2 security;
    private CashReceiptEvent userInputCashReceiptEvent;
    private CashReceiptDetailStatus hiddenCashReceiptDetailStatus;
    private LegalAction hiddenCourtOrderFilter;
    private CashReceiptSourceType hiddenUserInput;
    private CashReceiptDetail starting;
    private CsePersonsWorkSet dlgflw;
    private Standard promptCourtOrder;
    private LegalAction courtOrderFilter;
    private CsePersonsWorkSet filterCsePersonsWorkSet;
    private TextWorkArea promptPerson;
    private CsePerson filterCsePerson;
    private Code dlgflow;
    private Standard promptRsnCode;
    private Standard promptCollStat;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory1;
    private CashReceiptDetailStatus cashReceiptDetailStatus1;
    private ServiceProvider userInputServiceProvider;
    private Array<ExportGroup> export1;
    private CashReceipt userInputCashReceipt;
    private CashReceiptSourceType userInputCashReceiptSourceType;
    private CashReceiptDetail cashReceiptDetail1;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetailStatus cashReceiptDetailStatus2;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory2;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private Common promptSourceCode;
    private Common promptCashReceipt;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CashReceiptDetail cashReceiptDetail2;
    private CashReceiptDetail filterSsn;
    private CollectionType select;
    private Common promtCollType;
    private CashReceiptDetail hiddenFilterCashReceiptDetail;
    private CsePerson hiddenFilterCsePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A BlankGroup group.</summary>
    [Serializable]
    public class BlankGroup
    {
      /// <summary>
      /// A value of BlankGrpSel.
      /// </summary>
      [JsonPropertyName("blankGrpSel")]
      public Common BlankGrpSel
      {
        get => blankGrpSel ??= new();
        set => blankGrpSel = value;
      }

      /// <summary>
      /// A value of BlankGrpDetail.
      /// </summary>
      [JsonPropertyName("blankGrpDetail")]
      public CrdCrComboNo BlankGrpDetail
      {
        get => blankGrpDetail ??= new();
        set => blankGrpDetail = value;
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
      /// A value of BlankCashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("blankCashReceiptDetailStatus")]
      public CashReceiptDetailStatus BlankCashReceiptDetailStatus
      {
        get => blankCashReceiptDetailStatus ??= new();
        set => blankCashReceiptDetailStatus = value;
      }

      /// <summary>
      /// A value of BlankCashReceiptDetailStatHistory.
      /// </summary>
      [JsonPropertyName("blankCashReceiptDetailStatHistory")]
      public CashReceiptDetailStatHistory BlankCashReceiptDetailStatHistory
      {
        get => blankCashReceiptDetailStatHistory ??= new();
        set => blankCashReceiptDetailStatHistory = value;
      }

      /// <summary>
      /// A value of BlankCashReceipt.
      /// </summary>
      [JsonPropertyName("blankCashReceipt")]
      public CashReceipt BlankCashReceipt
      {
        get => blankCashReceipt ??= new();
        set => blankCashReceipt = value;
      }

      /// <summary>
      /// A value of BlankGrpUndistAmt.
      /// </summary>
      [JsonPropertyName("blankGrpUndistAmt")]
      public Common BlankGrpUndistAmt
      {
        get => blankGrpUndistAmt ??= new();
        set => blankGrpUndistAmt = value;
      }

      /// <summary>
      /// A value of BlankCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("blankCashReceiptDetail")]
      public CashReceiptDetail BlankCashReceiptDetail
      {
        get => blankCashReceiptDetail ??= new();
        set => blankCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of BlankGrpScreenDisplay.
      /// </summary>
      [JsonPropertyName("blankGrpScreenDisplay")]
      public CashReceiptDetail BlankGrpScreenDisplay
      {
        get => blankGrpScreenDisplay ??= new();
        set => blankGrpScreenDisplay = value;
      }

      /// <summary>
      /// A value of BlankHidden.
      /// </summary>
      [JsonPropertyName("blankHidden")]
      public CashReceiptType BlankHidden
      {
        get => blankHidden ??= new();
        set => blankHidden = value;
      }

      /// <summary>
      /// A value of HiddenLocalBlank.
      /// </summary>
      [JsonPropertyName("hiddenLocalBlank")]
      public CashReceiptEvent HiddenLocalBlank
      {
        get => hiddenLocalBlank ??= new();
        set => hiddenLocalBlank = value;
      }

      /// <summary>
      /// A value of BlankGrpFilter.
      /// </summary>
      [JsonPropertyName("blankGrpFilter")]
      public CollectionType BlankGrpFilter
      {
        get => blankGrpFilter ??= new();
        set => blankGrpFilter = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2;

      private Common blankGrpSel;
      private CrdCrComboNo blankGrpDetail;
      private CashReceiptSourceType blankCashReceiptSourceType;
      private CashReceiptDetailStatus blankCashReceiptDetailStatus;
      private CashReceiptDetailStatHistory blankCashReceiptDetailStatHistory;
      private CashReceipt blankCashReceipt;
      private Common blankGrpUndistAmt;
      private CashReceiptDetail blankCashReceiptDetail;
      private CashReceiptDetail blankGrpScreenDisplay;
      private CashReceiptType blankHidden;
      private CashReceiptEvent hiddenLocalBlank;
      private CollectionType blankGrpFilter;
    }

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
    /// Gets a value of Blank.
    /// </summary>
    [JsonIgnore]
    public Array<BlankGroup> Blank => blank ??= new(BlankGroup.Capacity);

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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of FlowToMcol.
    /// </summary>
    [JsonPropertyName("flowToMcol")]
    public Common FlowToMcol
    {
      get => flowToMcol ??= new();
      set => flowToMcol = value;
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
    /// A value of TotalAdjusted.
    /// </summary>
    [JsonPropertyName("totalAdjusted")]
    public Common TotalAdjusted
    {
      get => totalAdjusted ??= new();
      set => totalAdjusted = value;
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
    /// A value of CollAmtApplied.
    /// </summary>
    [JsonPropertyName("collAmtApplied")]
    public Common CollAmtApplied
    {
      get => collAmtApplied ??= new();
      set => collAmtApplied = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of NoOfFiltersSelected.
    /// </summary>
    [JsonPropertyName("noOfFiltersSelected")]
    public Common NoOfFiltersSelected
    {
      get => noOfFiltersSelected ??= new();
      set => noOfFiltersSelected = value;
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
    /// A value of PreviousCashReceipt.
    /// </summary>
    [JsonPropertyName("previousCashReceipt")]
    public CashReceipt PreviousCashReceipt
    {
      get => previousCashReceipt ??= new();
      set => previousCashReceipt = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of PadCsePersonNumber.
    /// </summary>
    [JsonPropertyName("padCsePersonNumber")]
    public TextWorkArea PadCsePersonNumber
    {
      get => padCsePersonNumber ??= new();
      set => padCsePersonNumber = value;
    }

    /// <summary>
    /// A value of WorkNumberOfObligors.
    /// </summary>
    [JsonPropertyName("workNumberOfObligors")]
    public Common WorkNumberOfObligors
    {
      get => workNumberOfObligors ??= new();
      set => workNumberOfObligors = value;
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
    /// A value of UpdateName.
    /// </summary>
    [JsonPropertyName("updateName")]
    public CsePersonsWorkSet UpdateName
    {
      get => updateName ??= new();
      set => updateName = value;
    }

    /// <summary>
    /// A value of UpdateNamePhone.
    /// </summary>
    [JsonPropertyName("updateNamePhone")]
    public CashReceiptDetail UpdateNamePhone
    {
      get => updateNamePhone ??= new();
      set => updateNamePhone = value;
    }

    private Array<BlankGroup> blank;
    private DateWorkArea null1;
    private Common flowToMcol;
    private Common totalRefunded;
    private Common totalAdjusted;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common collAmtApplied;
    private CsePerson csePerson;
    private Case1 case1;
    private LegalAction legalAction;
    private Common noOfFiltersSelected;
    private Common promptCount;
    private CashReceipt previousCashReceipt;
    private CashReceiptSourceType previousCashReceiptSourceType;
    private Common common;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private TextWorkArea padCsePersonNumber;
    private Common workNumberOfObligors;
    private Array<ObligorListGroup> obligorList;
    private CsePersonsWorkSet updateName;
    private CashReceiptDetail updateNamePhone;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    public CashReceiptDetail CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    /// <summary>
    /// A value of TbdGetPerson.
    /// </summary>
    [JsonPropertyName("tbdGetPerson")]
    public CashReceiptDetail TbdGetPerson
    {
      get => tbdGetPerson ??= new();
      set => tbdGetPerson = value;
    }

    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole caseRole;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CashReceiptDetail courtOrder;
    private CashReceiptDetail tbdGetPerson;
  }
#endregion
}

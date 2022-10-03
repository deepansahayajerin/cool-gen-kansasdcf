// Program: FN_LCDA_LIST_COLL_DIST_ACTIVITY, ID: 373394518, model: 746.
// Short name: SWELCDAP
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
/// A program: FN_LCDA_LIST_COLL_DIST_ACTIVITY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnLcdaListCollDistActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LCDA_LIST_COLL_DIST_ACTIVITY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLcdaListCollDistActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLcdaListCollDistActivity.
  /// </summary>
  public FnLcdaListCollDistActivity(IContext context, Import import,
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
    // Date    Developer    Request #   Description
    // --------------------------------------------------------------------
    // 08/23/01  Alan Doty              Initial Development
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.TraceIndHidden.Flag = import.TraceIndHidden.Flag;

    // : Check for quick exits...
    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        break;
    }

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.LstDbtWAmtOwedDlgflw.SelectChar = "N";
    local.ObligeeStateOfKansas.Number = "000000017O";

    // *****************************************************************
    // Move Imports to Exports
    // *****************************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      export.ShowHistory.Text1 = "N";
      export.CollectionFrom.Date = Now().Date.AddMonths(-1);
      export.CollectionTo.Date = Now().Date;

      return;
    }

    export.ObligorCsePerson.Number = import.ObligorCsePerson.Number;
    MoveCsePersonsWorkSet(import.ObligorCsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.CollectionFrom.Date = import.CollectionFrom.Date;
    export.CollectionTo.Date = import.CollectionTo.Date;
    export.ShowHistory.Text1 = import.ShowHistory.Text1;
    export.PromptToLaps.SelectChar = import.PromptToLaps.SelectChar;
    export.PromptToName.SelectChar = import.PromptToName.SelectChar;
    export.PromptToCdvl.SelectChar = import.PromptToCdvl.SelectChar;
    export.FlowTo.NextTransaction = import.FlowTo.NextTransaction;

    if (IsEmpty(export.HiddenCsePerson.Number))
    {
      if (Lt(Now().Date, export.CollectionTo.Date))
      {
        export.CollectionTo.Date = Now().Date;
      }
    }

    if (Equal(export.ObligorCsePerson.Number, export.HiddenCsePerson.Number) &&
      !IsEmpty(export.HiddenCsePerson.Number))
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.LineType.Text2 = import.Group.Item.LineType.Text2;
        export.Group.Update.CashReceipt.SequentialNumber =
          import.Group.Item.CashReceipt.SequentialNumber;
        export.Group.Update.CashReceiptDetail.Assign(
          import.Group.Item.CashReceiptDetail);
        export.Group.Update.Collection.Assign(import.Group.Item.Collection);
        export.Group.Update.CrdCrComboNo.CrdCrCombo =
          import.Group.Item.CrdCrComboNo.CrdCrCombo;
        MoveObligation(import.Group.Item.Obligation,
          export.Group.Update.Obligation);
        export.Group.Update.ObligationType.Assign(
          import.Group.Item.ObligationType);
        export.Group.Update.Debt.SystemGeneratedIdentifier =
          import.Group.Item.Debt.SystemGeneratedIdentifier;
        export.Group.Update.DebtDetail.DueDt =
          import.Group.Item.DebtDetail.DueDt;
        MoveLegalAction(import.Group.Item.LegalAction,
          export.Group.Update.LegalAction);
        export.Group.Update.Amt.TotalCurrency =
          import.Group.Item.Amt.TotalCurrency;
        export.Group.Update.PayeeRsn.Text10 = import.Group.Item.PayeeRsn.Text10;
        export.Group.Update.Pgm.Text6 = import.Group.Item.Pgm.Text6;
        export.Group.Update.Process.Date = import.Group.Item.Process.Date;
        export.Group.Update.RcvryExists.Text1 =
          import.Group.Item.RcvryExists.Text1;
        export.Group.Update.DtlRecId.TotalInteger =
          import.Group.Item.DtlRecId.TotalInteger;

        switch(TrimEnd(export.Group.Item.LineType.Text2))
        {
          case "CR":
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Protected = false;

            var field2 =
              GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

            field2.Protected = true;

            var field3 = GetField(export.Group.Item.Process, "date");

            field3.Protected = true;

            break;
          case "BL":
            var field4 = GetField(export.Group.Item.Common, "selectChar");

            field4.Intensity = Intensity.Dark;
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

            field5.Intensity = Intensity.Dark;
            field5.Protected = true;

            var field6 = GetField(export.Group.Item.Process, "date");

            field6.Protected = true;

            break;
          case "CF":
            var field7 = GetField(export.Group.Item.Common, "selectChar");

            field7.Intensity = Intensity.Dark;
            field7.Protected = true;

            var field8 =
              GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

            field8.Intensity = Intensity.Dark;
            field8.Protected = true;

            var field9 = GetField(export.Group.Item.Process, "date");

            field9.Intensity = Intensity.Dark;
            field9.Protected = true;

            break;
          default:
            var field10 = GetField(export.Group.Item.Common, "selectChar");

            field10.Protected = false;

            var field11 =
              GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

            field11.Intensity = Intensity.Dark;
            field11.Protected = true;

            var field12 = GetField(export.Group.Item.Process, "date");

            field12.Intensity = Intensity.Normal;
            field12.Protected = true;

            break;
        }

        export.Group.Next();
      }

      if (Equal(global.Command, "DISPLAY"))
      {
        export.PgPos.Count = 1;

        export.PgCntl.Index = 0;
        export.PgCntl.CheckSize();

        export.PgCntl.Update.PgCntlCashReceipt.SequentialNumber = 999999999;
        export.PgCntl.Update.PgCntlCashReceiptDetail.SequentialIdentifier =
          9999;
        export.PgCntl.Update.PgCntlCashReceiptDetail.CollectionDate =
          UseCabSetMaximumDiscontinueDate();
        export.PgCntl.Update.PgCntlDtlRecId.TotalInteger = 0;

        goto Test1;
      }

      export.PgPos.Count = import.PgPos.Count;

      for(import.PgCntl.Index = 0; import.PgCntl.Index < import.PgCntl.Count; ++
        import.PgCntl.Index)
      {
        if (!import.PgCntl.CheckSize())
        {
          break;
        }

        export.PgCntl.Index = import.PgCntl.Index;
        export.PgCntl.CheckSize();

        export.PgCntl.Update.PgCntlCashReceipt.SequentialNumber =
          import.PgCntl.Item.PgCntlCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(import.PgCntl.Item.PgCntlCashReceiptDetail,
          export.PgCntl.Update.PgCntlCashReceiptDetail);
        export.PgCntl.Update.PgCntlDtlRecId.TotalInteger =
          import.PgCntl.Item.PgCntlDtlRecId.TotalInteger;
      }

      import.PgCntl.CheckIndex();
    }
    else
    {
      export.PgPos.Count = 1;

      export.PgCntl.Index = 0;
      export.PgCntl.CheckSize();

      export.PgCntl.Update.PgCntlCashReceipt.SequentialNumber = 999999999;
      export.PgCntl.Update.PgCntlCashReceiptDetail.SequentialIdentifier = 9999;
      export.PgCntl.Update.PgCntlCashReceiptDetail.CollectionDate =
        UseCabSetMaximumDiscontinueDate();
      export.PgCntl.Update.PgCntlDtlRecId.TotalInteger = 0;
    }

Test1:

    // : Set Defaults...
    switch(AsChar(export.ShowHistory.Text1))
    {
      case 'Y':
        break;
      case 'N':
        break;
      case ' ':
        export.ShowHistory.Text1 = "N";

        break;
      default:
        var field = GetField(export.ShowHistory, "text1");

        field.Error = true;

        ExitState = "ACO_NI0000_ENTER_Y_OR_N";

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (import.DlgflwFromCollection.SystemGeneratedIdentifier != 0)
      {
        if (ReadCollection())
        {
          export.CollectionFrom.Date = entities.ExistingCollection.CollectionDt;
          export.CollectionTo.Date = entities.ExistingCollection.CollectionDt;
        }
        else
        {
          // : Did not flow from DEBT - Continue Processing.
        }
      }
      else if (!Equal(import.DlgflwFromCashReceiptDetail.CollectionDate,
        local.NullDateWorkArea.Date))
      {
        export.CollectionFrom.Date =
          import.DlgflwFromCashReceiptDetail.CollectionDate;
        export.CollectionTo.Date =
          import.DlgflwFromCashReceiptDetail.CollectionDate;
      }
    }

    if (Equal(export.CollectionFrom.Date, local.NullDateWorkArea.Date))
    {
      export.CollectionFrom.Date = Now().Date.AddMonths(-1);
    }

    if (Equal(export.CollectionTo.Date, local.NullDateWorkArea.Date))
    {
      export.CollectionTo.Date = Now().Date;
    }

    if (Lt(export.CollectionTo.Date, export.CollectionFrom.Date))
    {
      var field1 = GetField(export.CollectionFrom, "date");

      field1.Error = true;

      var field2 = GetField(export.CollectionTo, "date");

      field2.Error = true;

      ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

      return;
    }

    if (Lt(Now().Date, export.CollectionTo.Date))
    {
      var field = GetField(export.CollectionTo, "date");

      field.Error = true;

      ExitState = "FN0000_TO_DATE_GREATER_THAN_CURR";

      return;
    }

    if (!Equal(global.Command, "ENTER"))
    {
      export.Standard.NextTransaction = "";
    }

    if (!Equal(global.Command, "LIST"))
    {
      export.PromptToCdvl.SelectChar = "";
      export.PromptToLaps.SelectChar = "";
      export.PromptToName.SelectChar = "";
    }

    // : Handle the return from a prompt.
    switch(TrimEnd(global.Command))
    {
      case "RETNAME":
        if (IsEmpty(import.ObligorDlgflw.Number))
        {
          if (IsEmpty(export.ObligorCsePerson.Number))
          {
            var field1 = GetField(export.ObligorCsePerson, "number");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
          }
        }
        else
        {
          export.ObligorCsePerson.Number = import.ObligorDlgflw.Number;
        }

        global.Command = "DISPLAY";

        break;
      case "RETLAPS":
        if (!IsEmpty(import.ObligorDlgflw.Number))
        {
          export.ObligorCsePerson.Number = import.ObligorDlgflw.Number;
        }

        if (IsEmpty(export.ObligorCsePerson.Number))
        {
          var field1 = GetField(export.ObligorCsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        if (!IsEmpty(import.DlgflwLegalAction.StandardNumber))
        {
          export.LegalAction.StandardNumber =
            import.DlgflwLegalAction.StandardNumber ?? "";
        }

        global.Command = "DISPLAY";

        break;
      case "RETCDVL":
        if (IsEmpty(import.DlgflwCodeValue.Cdvalue))
        {
          var field1 = GetField(export.FlowTo, "nextTransaction");

          field1.Error = true;

          ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

          return;
        }

        export.FlowTo.NextTransaction = import.DlgflwCodeValue.Cdvalue;
        global.Command = "FLOWSTO";

        break;
      case "RETLINK":
        export.FlowTo.NextTransaction = "";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          export.Group.Update.Common.SelectChar = "";
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        return;
      case "":
        var field = GetField(export.ObligorCsePerson, "number");

        field.Error = true;

        ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

        return;
      default:
        break;
    }

    if (Equal(global.Command, "FLOWSTO") || Equal(global.Command, "OBLGM"))
    {
      // : Verify Selection of one and only one row.
      local.Common.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;
            local.LineType.Text2 = export.Group.Item.LineType.Text2;
            export.DlgflwCashReceipt.SequentialNumber =
              export.Group.Item.CashReceipt.SequentialNumber;
            export.DlgflwCashReceiptDetail.Assign(
              export.Group.Item.CashReceiptDetail);
            MoveObligation(export.Group.Item.Obligation, export.DlgflwObligation);
              
            export.DlgflwObligationType.
              Assign(export.Group.Item.ObligationType);
            export.DlgflwDebt.SystemGeneratedIdentifier =
              export.Group.Item.Debt.SystemGeneratedIdentifier;
            MoveLegalAction(export.Group.Item.LegalAction,
              export.DlgflwLegalAction);
            export.DlgflwCollection.Assign(export.Group.Item.Collection);

            if (Equal(local.LineType.Text2, "CR"))
            {
              export.DlgflwFrom.Date = export.CollectionFrom.Date;
              export.DlgflwTo.Date = export.CollectionTo.Date;
              export.DlgflwLegalAction.StandardNumber =
                export.DlgflwCashReceiptDetail.CourtOrderNumber ?? "";
            }
            else
            {
              export.DlgflwFrom.Date = export.Group.Item.DebtDetail.DueDt;
              export.DlgflwTo.Date = export.Group.Item.DebtDetail.DueDt;
            }

            break;
          case ' ':
            break;
          case '*':
            break;
          default:
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Color = "red";
            field.Protected = false;
            field.Focused = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }
      }

      if (local.Common.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }
    else
    {
      export.FlowTo.NextTransaction = "";

      if (Equal(global.Command, "LIST"))
      {
        goto Test2;
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        export.Group.Update.Common.SelectChar = "";
      }
    }

Test2:

    // : Check for Next Tran.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // : User entered this screen from another screen
      UseScCabNextTranGet();

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligor))
      {
        export.ObligorCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberObligor ?? Spaces(10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberAp))
      {
        export.ObligorCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.ObligorCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      }
      else
      {
        var field = GetField(export.ObligorCsePerson, "number");

        field.Error = true;

        ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

        return;
      }

      export.LegalAction.StandardNumber =
        export.HiddenNextTranInfo.StandardCrtOrdNumber ?? "";
      global.Command = "DISPLAY";
    }

    // : Handle Flow on Next Tran...
    if (Equal(global.Command, "ENTER"))
    {
      if (IsEmpty(import.Standard.NextTransaction))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        ExitState = "SP0000_REQUIRED_FIELD_MISSING";

        return;
      }

      export.HiddenNextTranInfo.CsePersonNumber =
        export.ObligorCsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumberAp =
        export.ObligorCsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumberObligor =
        export.ObligorCsePerson.Number;
      export.HiddenNextTranInfo.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_ACTION";

        return;
      }
    }

    // : Handle PF9 - Return
    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    // : Verify the security for the User to be able to execute specific 
    // commands.
    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.TraceIndHidden.Flag) == 'Y')
      {
        goto Test3;
      }

      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

Test3:

    // : Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // : Prompt Check
        switch(AsChar(export.PromptToName.SelectChar))
        {
          case 'S':
            export.PromptToName.SelectChar = "";
            ExitState = "ECO_LNK_TO_NAME";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field1 = GetField(export.PromptToName, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptToLaps.SelectChar))
        {
          case 'S':
            export.PromptToLaps.SelectChar = "";
            ExitState = "ECO_LNK_TO_LAPS";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field1 = GetField(export.PromptToLaps, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptToCdvl.SelectChar))
        {
          case 'S':
            export.PromptToCdvl.SelectChar = "";
            export.DlgflwCode.CodeName = "FLOW FROM LCDA";
            ExitState = "ECO_LNK_TO_CDVL";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field1 = GetField(export.PromptToCdvl, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        // : PF4 - List pressed with no acceptable prompt entered.
        var field = GetField(export.PromptToName, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      case "FLOWSTO":
        switch(TrimEnd(export.FlowTo.NextTransaction))
        {
          case "DEBT":
            ExitState = "ECO_LNK_TO_LST_DBT_ACT_BY_APPYR";

            return;
          case "PAYR":
            ExitState = "ECO_LNK_TO_LST_OBLIG_BY_AP_PYR";

            return;
          case "OCOL":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (Equal(export.Group.Item.LineType.Text2, "CO") || Equal
                  (export.Group.Item.LineType.Text2, "CA"))
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CO":
                break;
              case "CA":
                break;
              default:
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

                goto Test4;
            }

            ExitState = "ECO_LNK_TO_LST_COLL_BY_OBLIG";

            return;
          case "COLL":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (Equal(export.Group.Item.LineType.Text2, "CO") || Equal
                  (export.Group.Item.LineType.Text2, "CA"))
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CO":
                break;
              case "CA":
                break;
              default:
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

                goto Test4;
            }

            if (ReadCashReceiptSourceTypeCashReceiptTypeCashReceiptEvent())
            {
              export.DlgflwCashReceiptSourceType.SystemGeneratedIdentifier =
                entities.ExistingCashReceiptSourceType.
                  SystemGeneratedIdentifier;
              export.DlgflwCashReceiptEvent.SystemGeneratedIdentifier =
                entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
              export.DlgflwCashReceiptType.SystemGeneratedIdentifier =
                entities.ExistingCashReceiptType.SystemGeneratedIdentifier;
            }
            else
            {
              ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

              break;
            }

            ExitState = "ECO_LNK_LST_COLLECTIONS";

            return;
          case "COMN":
            ExitState = "ECO_LNK_TO_COMN";

            return;
          case "OPAY":
            ExitState = "ECO_LNK_LST_OPAY_OBLG_BY_AP";

            return;
          case "REIP":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!IsEmpty(export.Group.Item.CashReceiptDetail.
                  CourtOrderNumber))
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CO":
                if (export.DlgflwLegalAction.Identifier == 0)
                {
                  for(export.Group.Index = 0; export.Group.Index < export
                    .Group.Count; ++export.Group.Index)
                  {
                    if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                    {
                      var field3 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field3.Error = true;
                    }
                  }

                  ExitState = "FN0000_NO_CO_COLL_INVALID_SELECT";

                  goto Test4;
                }

                break;
              case "CA":
                if (export.DlgflwLegalAction.Identifier == 0)
                {
                  for(export.Group.Index = 0; export.Group.Index < export
                    .Group.Count; ++export.Group.Index)
                  {
                    if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                    {
                      var field3 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field3.Error = true;
                    }
                  }

                  ExitState = "FN0000_NO_CO_COLL_INVALID_SELECT";

                  goto Test4;
                }

                break;
              default:
                if (IsEmpty(export.DlgflwCashReceiptDetail.CourtOrderNumber))
                {
                  for(export.Group.Index = 0; export.Group.Index < export
                    .Group.Count; ++export.Group.Index)
                  {
                    if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                    {
                      var field3 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field3.Error = true;
                    }
                  }

                  ExitState = "FN0000_NO_CO_INVALID_SELECTION";

                  goto Test4;
                }

                if (ReadLegalAction1())
                {
                  MoveLegalAction(entities.ExistingLegalAction,
                    export.DlgflwLegalAction);
                }
                else
                {
                  ExitState = "FN0000_COURT_ORDER_NF";

                  goto Test4;
                }

                break;
            }

            ExitState = "ECO_LNK_TO_REIP";

            return;
          case "APSM":
            ExitState = "ECO_LNK_TO_APSM";

            return;
          case "OSUM":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (Equal(export.Group.Item.LineType.Text2, "CO") || Equal
                  (export.Group.Item.LineType.Text2, "CA"))
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CO":
                break;
              case "CA":
                break;
              default:
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

                goto Test4;
            }

            ExitState = "ECO_LNK_TO_OSUM";

            return;
          case "PSUM":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if ((Equal(export.Group.Item.LineType.Text2, "CO") || Equal
                  (export.Group.Item.LineType.Text2, "CA")) && AsChar
                  (export.Group.Item.Obligation.PrimarySecondaryCode) != 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            if (AsChar(export.DlgflwObligation.PrimarySecondaryCode) == 'S')
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_NO_FLOW_FOR_SECONDARY";

              break;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CO":
                break;
              case "CA":
                break;
              default:
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

                goto Test4;
            }

            if (ReadCsePerson1())
            {
              if (Equal(entities.ExistingObligee1.Number,
                local.ObligeeStateOfKansas.Number))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "FN0000_FLOW_NOT_ALLOWED_FOR_KS";

                break;
              }

              export.ObligeeDlgflwCsePerson.Number =
                entities.ExistingObligee1.Number;
              export.ObligeeDlgflwCsePersonsWorkSet.Number =
                entities.ExistingObligee1.Number;
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_PAYEE_NF";

              break;
            }

            if (Equal(export.DlgflwCollection.CollectionDt,
              local.NullDateWorkArea.Date))
            {
              export.DlgflwMonthlyObligeeSummary.Month =
                Month(export.CollectionFrom.Date);
              export.DlgflwMonthlyObligeeSummary.Year =
                Year(export.CollectionFrom.Date);
            }
            else
            {
              export.DlgflwMonthlyObligeeSummary.Month =
                Month(export.DlgflwCollection.CollectionDt);
              export.DlgflwMonthlyObligeeSummary.Year =
                Year(export.DlgflwCollection.CollectionDt);
            }

            ExitState = "ECO_LNK_TO_PSUM";

            return;
          case "PACC":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if ((Equal(export.Group.Item.LineType.Text2, "CO") || Equal
                  (export.Group.Item.LineType.Text2, "CA")) && AsChar
                  (export.Group.Item.Obligation.PrimarySecondaryCode) != 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CO":
                break;
              case "CA":
                break;
              default:
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

                goto Test4;
            }

            if (AsChar(export.DlgflwObligation.PrimarySecondaryCode) == 'S')
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_NO_FLOW_FOR_SECONDARY";

              break;
            }

            if (ReadCsePerson1())
            {
              if (Equal(entities.ExistingObligee1.Number,
                local.ObligeeStateOfKansas.Number))
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "FN0000_FLOW_NOT_ALLOWED_FOR_KS";

                break;
              }

              export.ObligeeDlgflwCsePerson.Number =
                entities.ExistingObligee1.Number;
              export.ObligeeDlgflwCsePersonsWorkSet.Number =
                entities.ExistingObligee1.Number;
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_PAYEE_NF";

              break;
            }

            export.DlgflwFrom.Date =
              AddMonths(AddDays(
                export.DlgflwCollection.CollectionDt,
              -(Day(export.DlgflwCollection.CollectionDt) - 1)), -1);
            export.DlgflwTo.Date = export.DlgflwCollection.CollectionDt;
            ExitState = "ECO_LNK_TO_LST_APACC";

            return;
          case "WARR":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if ((Equal(export.Group.Item.LineType.Text2, "CO") || Equal
                  (export.Group.Item.LineType.Text2, "CA")) && Equal
                  (export.Group.Item.Collection.ProgramAppliedTo, "NA") && AsChar
                  (export.Group.Item.Obligation.PrimarySecondaryCode) != 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            if (AsChar(export.DlgflwObligation.PrimarySecondaryCode) == 'S')
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_NO_FLOW_FOR_SECONDARY";

              break;
            }

            if (!Equal(export.DlgflwCollection.ProgramAppliedTo, "NA"))
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

              break;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CO":
                break;
              case "CA":
                break;
              default:
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

                goto Test4;
            }

            if (ReadCsePerson1())
            {
              export.ObligeeDlgflwCsePerson.Number =
                entities.ExistingObligee1.Number;
              export.ObligeeDlgflwCsePersonsWorkSet.Number =
                entities.ExistingObligee1.Number;
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_PAYEE_NF";

              break;
            }

            if (Equal(export.DlgflwCollection.DisbursementDt,
              local.NullDateWorkArea.Date))
            {
              export.DlgflwFrom.Date =
                AddMonths(AddDays(
                  export.DlgflwCollection.CollectionDt,
                -(Day(export.DlgflwCollection.CollectionDt) - 1)), -1);
            }
            else
            {
              export.DlgflwFrom.Date =
                AddMonths(AddDays(
                  export.DlgflwCollection.DisbursementDt,
                -(Day(export.DlgflwCollection.DisbursementDt) - 1)), -1);
            }

            export.DlgflwTo.Date = Now().Date;
            ExitState = "ECO_LNK_TO_LST_WARRANTS";

            return;
          case "LACN":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (Equal(export.Group.Item.LineType.Text2, "CO") || Equal
                  (export.Group.Item.LineType.Text2, "CA") || Equal
                  (export.Group.Item.LineType.Text2, "CR"))
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            switch(TrimEnd(local.LineType.Text2))
            {
              case "CR":
                if (IsEmpty(export.DlgflwCashReceiptDetail.CourtOrderNumber))
                {
                  for(export.Group.Index = 0; export.Group.Index < export
                    .Group.Count; ++export.Group.Index)
                  {
                    if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                    {
                      var field3 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field3.Error = true;
                    }
                  }

                  ExitState = "FN0000_NO_CO_COLL_INVALID_SELECT";

                  goto Test4;
                }

                if (ReadLegalAction1())
                {
                  MoveLegalAction(entities.ExistingLegalAction,
                    export.DlgflwLegalAction);
                }
                else
                {
                  for(export.Group.Index = 0; export.Group.Index < export
                    .Group.Count; ++export.Group.Index)
                  {
                    if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                    {
                      var field3 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field3.Error = true;
                    }
                  }

                  ExitState = "FN0000_COURT_ORDER_NF";

                  goto Test4;
                }

                break;
              case "CO":
                if (export.DlgflwLegalAction.Identifier == 0)
                {
                  for(export.Group.Index = 0; export.Group.Index < export
                    .Group.Count; ++export.Group.Index)
                  {
                    if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                    {
                      var field3 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field3.Error = true;
                    }
                  }

                  ExitState = "FN0000_NO_CO_COLL_INVALID_SELECT";

                  goto Test4;
                }

                break;
              case "CA":
                if (export.DlgflwLegalAction.Identifier == 0)
                {
                  for(export.Group.Index = 0; export.Group.Index < export
                    .Group.Count; ++export.Group.Index)
                  {
                    if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                    {
                      var field3 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field3.Error = true;
                    }
                  }

                  ExitState = "FN0000_NO_CO_COLL_INVALID_SELECT";

                  goto Test4;
                }

                break;
              default:
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    var field3 =
                      GetField(export.Group.Item.Common, "selectChar");

                    field3.Error = true;
                  }
                }

                ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

                goto Test4;
            }

            ExitState = "ECO_LNK_TO_LACN";

            return;
          case "OREL":
            if (local.Common.Count == 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (Equal(export.Group.Item.LineType.Text2, "CA") && AsChar
                  (export.Group.Item.RcvryExists.Text1) == 'Y' && AsChar
                  (export.Group.Item.Obligation.PrimarySecondaryCode) != 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  break;
                }
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            if (!Equal(local.LineType.Text2, "CA"))
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

              break;
            }

            if (AsChar(export.DlgflwObligation.PrimarySecondaryCode) == 'S')
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_NO_FLOW_FOR_SECONDARY";

              break;
            }

            if (ReadCsePerson1())
            {
              export.ObligeeDlgflwCsePerson.Number =
                entities.ExistingObligee1.Number;
              export.ObligeeDlgflwCsePersonsWorkSet.Number =
                entities.ExistingObligee1.Number;
              export.DlgflwMonthlyObligeeSummary.Month =
                Month(export.DlgflwFrom.Date);
              export.DlgflwMonthlyObligeeSummary.Year =
                Year(export.DlgflwFrom.Date);
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                {
                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;
                }
              }

              ExitState = "FN0000_NO_OBLIGEE_NO_FLOW";

              break;
            }

            export.DlgflwFirstTimeInd.Flag = "N";
            ExitState = "ECO_LNK_LST_POTNTL_RCVRY_OBLG";

            return;
          case "":
            var field1 = GetField(export.FlowTo, "nextTransaction");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          default:
            var field2 = GetField(export.FlowTo, "nextTransaction");

            field2.Error = true;

            ExitState = "ACO_NE0000_FLOW_TO_NOT_SUPPORTED";

            return;
        }

Test4:

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;
          }
        }

        return;
      case "OBLGM":
        if (local.Common.Count == 0)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (Equal(export.Group.Item.LineType.Text2, "CO") || Equal
              (export.Group.Item.LineType.Text2, "CA"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              break;
            }
          }

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        switch(TrimEnd(local.LineType.Text2))
        {
          case "CO":
            break;
          case "CA":
            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

            return;
        }

        switch(AsChar(export.DlgflwObligationType.Classification))
        {
          case 'A':
            ExitState = "ECO_LNK_TO_MTN_ACCRUING_OBLIG";

            break;
          case 'R':
            ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";

            break;
          case 'F':
            ExitState = "ECO_LNK_TO_OFEE";

            break;
          case 'V':
            ExitState = "ECO_LNK_TO_MTN_VOLUNTARY_OBLIG";

            break;
          default:
            ExitState = "ECO_LNK_TO_MTN_NON_ACCRUING_OBLG";

            break;
        }

        return;
      case "DISPLAY":
        if (IsEmpty(export.ObligorCsePerson.Number))
        {
          export.HiddenCsePerson.Number = local.NullCsePerson.Number;
          MoveCsePersonsWorkSet(local.NullCsePersonsWorkSet,
            export.ObligorCsePersonsWorkSet);

          var field1 = GetField(export.ObligorCsePerson, "number");

          field1.Error = true;

          ExitState = "CSE_PERSON_NF";

          return;
        }

        export.ObligorCsePersonsWorkSet.Number = export.ObligorCsePerson.Number;
        UseCabZeroFillNumber();

        if (ReadCsePerson2())
        {
          export.HiddenCsePerson.Number = export.ObligorCsePerson.Number;
        }
        else
        {
          export.HiddenCsePerson.Number = local.NullCsePerson.Number;
          MoveCsePersonsWorkSet(local.NullCsePersonsWorkSet,
            export.ObligorCsePersonsWorkSet);

          var field1 = GetField(export.ObligorCsePerson, "number");

          field1.Error = true;

          ExitState = "CSE_PERSON_NF";

          return;
        }

        if (AsChar(export.TraceIndHidden.Flag) == 'Y')
        {
          export.ObligorCsePersonsWorkSet.FormattedName = "TRACE MODE";
        }
        else
        {
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
            export.ObligorCsePersonsWorkSet.FormattedName =
              "ADABAS Error: Name Unavailable";
          }
        }

        if (!IsEmpty(export.LegalAction.StandardNumber))
        {
          if (ReadLegalActionLegalActionPerson())
          {
            MoveLegalAction(entities.ExistingLegalAction, export.LegalAction);
          }
          else if (ReadLegalAction2())
          {
            var field1 = GetField(export.LegalAction, "standardNumber");

            field1.Error = true;

            var field2 = GetField(export.ObligorCsePerson, "number");

            field2.Error = true;

            ExitState = "FN0000_OBLIGOR_NOT_A_PART_OF_CT";

            return;
          }
          else
          {
            var field1 = GetField(export.LegalAction, "standardNumber");

            field1.Error = true;

            ExitState = "LEGAL_ACTION_NF";

            return;
          }
        }

        break;
      case "PREV":
        if (export.PgPos.Count == 1)
        {
          if (!export.Group.IsFull)
          {
            export.ScrollInd.Text10 = "More:";
          }
          else
          {
            export.ScrollInd.Text10 = "More: +";
          }

          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        --export.PgPos.Count;

        break;
      case "NEXT":
        if (export.PgPos.Count == export.PgCntl.Count)
        {
          export.ScrollInd.Text10 = "More:   -";
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }
        else
        {
          ++export.PgPos.Count;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // : Load the Export Group View with the Activity
    export.PgCntl.Index = export.PgPos.Count - 1;
    export.PgCntl.CheckSize();

    UseFnLcdaBuildDisplayList();
    export.PgCntlCashReceipt.SequentialNumber =
      local.PgCntlCashReceipt.SequentialNumber;
    MoveCashReceiptDetail(local.PgCntlCashReceiptDetail,
      export.PgCntlCashReceiptDetail);
    export.PgCntlDtlRecId.TotalInteger = local.PgCntlDtlRecId.TotalInteger;

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      switch(TrimEnd(export.Group.Item.LineType.Text2))
      {
        case "CR":
          var field1 = GetField(export.Group.Item.Common, "selectChar");

          field1.Protected = false;

          var field2 =
            GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

          field2.Intensity = Intensity.Normal;
          field2.Protected = true;

          var field3 = GetField(export.Group.Item.Process, "date");

          field3.Protected = true;

          break;
        case "BL":
          var field4 = GetField(export.Group.Item.Common, "selectChar");

          field4.Intensity = Intensity.Dark;
          field4.Protected = true;

          var field5 =
            GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

          field5.Intensity = Intensity.Dark;
          field5.Protected = true;

          var field6 = GetField(export.Group.Item.Process, "date");

          field6.Protected = true;

          break;
        case "CF":
          var field7 = GetField(export.Group.Item.Common, "selectChar");

          field7.Intensity = Intensity.Dark;
          field7.Protected = true;

          var field8 =
            GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

          field8.Intensity = Intensity.Dark;
          field8.Protected = true;

          var field9 = GetField(export.Group.Item.Process, "date");

          field9.Intensity = Intensity.Dark;
          field9.Protected = true;

          break;
        default:
          var field10 = GetField(export.Group.Item.Common, "selectChar");

          field10.Protected = false;

          var field11 =
            GetField(export.Group.Item.CashReceiptDetail, "collectionDate");

          field11.Intensity = Intensity.Dark;
          field11.Protected = true;

          var field12 = GetField(export.Group.Item.Process, "date");

          field12.Protected = true;

          break;
      }
    }

    if (export.Group.IsEmpty)
    {
      if (Equal(global.Command, "NEXT"))
      {
        export.ScrollInd.Text10 = "More:   -";
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
      }
      else
      {
        export.ScrollInd.Text10 = "More:";
        ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
      }

      var field = GetField(export.ObligorCsePerson, "number");

      field.Protected = false;
      field.Focused = true;

      return;
    }

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (Equal(export.Group.Item.LineType.Text2, "BL"))
      {
        continue;
      }

      var field = GetField(export.Group.Item.Common, "selectChar");

      field.Protected = false;
      field.Focused = true;

      break;
    }

    if (!export.Group.IsFull)
    {
      if (Equal(global.Command, "NEXT"))
      {
        export.ScrollInd.Text10 = "More:   -";
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
      }
      else
      {
        export.ScrollInd.Text10 = "More:";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      return;
    }

    if (export.PgPos.Count == export.PgCntl.Count && Equal
      (global.Command, "NEXT") || export.PgPos.Count == 1 && Equal
      (global.Command, "DISPLAY"))
    {
      export.PgCntl.Index = export.PgPos.Count;
      export.PgCntl.CheckSize();

      export.PgCntl.Update.PgCntlCashReceipt.SequentialNumber =
        local.PgCntlCashReceipt.SequentialNumber;
      MoveCashReceiptDetail(local.PgCntlCashReceiptDetail,
        export.PgCntl.Update.PgCntlCashReceiptDetail);
      export.PgCntl.Update.PgCntlDtlRecId.TotalInteger =
        local.PgCntlDtlRecId.TotalInteger;
    }

    switch(TrimEnd(global.Command))
    {
      case "PREV":
        if (export.PgPos.Count == 1)
        {
          export.ScrollInd.Text10 = "More: +";
        }
        else
        {
          export.ScrollInd.Text10 = "More: + -";
        }

        break;
      case "NEXT":
        export.ScrollInd.Text10 = "More: + -";

        break;
      default:
        export.ScrollInd.Text10 = "More: +";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroup(FnLcdaBuildDisplayList.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.LineType.Text2 = source.LineType.Text2;
    target.CashReceipt.SequentialNumber = source.CashReceipt.SequentialNumber;
    target.CashReceiptDetail.Assign(source.CashReceiptDetail);
    target.CrdCrComboNo.CrdCrCombo = source.CrdCrComboNo.CrdCrCombo;
    target.Collection.Assign(source.Collection);
    target.Process.Date = source.Process.Date;
    target.PayeeRsn.Text10 = source.PayeeRsn.Text10;
    MoveObligation(source.Obligation, target.Obligation);
    target.ObligationType.Assign(source.ObligationType);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    target.DebtDetail.DueDt = source.DebtDetail.DueDt;
    MoveLegalAction(source.LegalAction, target.LegalAction);
    target.Amt.TotalCurrency = source.Amt.TotalCurrency;
    target.Pgm.Text6 = source.Pgm.Text6;
    target.RcvryExists.Text1 = source.RcvryExists.Text1;
    target.DtlRecId.TotalInteger = source.DtlRecId.TotalInteger;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.ObligorCsePerson.Number = useImport.CsePerson.Number;
    export.ObligorCsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseFnLcdaBuildDisplayList()
  {
    var useImport = new FnLcdaBuildDisplayList.Import();
    var useExport = new FnLcdaBuildDisplayList.Export();

    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.CollectionFrom.Date = export.CollectionFrom.Date;
    useImport.CollectionTo.Date = export.CollectionTo.Date;
    useImport.ShowHistory.Text1 = export.ShowHistory.Text1;
    useImport.TraceInd.Flag = export.TraceIndHidden.Flag;
    useImport.PgCntlCashReceipt.SequentialNumber =
      export.PgCntl.Item.PgCntlCashReceipt.SequentialNumber;
    MoveCashReceiptDetail(export.PgCntl.Item.PgCntlCashReceiptDetail,
      useImport.PgCntlCashReceiptDetail);
    useImport.PgCntlDtlRecId.TotalInteger =
      export.PgCntl.Item.PgCntlDtlRecId.TotalInteger;

    Call(FnLcdaBuildDisplayList.Execute, useImport, useExport);

    export.ObligorCsePerson.Number = useImport.Obligor.Number;
    local.PgCntlCashReceipt.SequentialNumber =
      useExport.PgCntlCashReceipt.SequentialNumber;
    MoveCashReceiptDetail(useExport.PgCntlCashReceiptDetail,
      local.PgCntlCashReceiptDetail);
    local.PgCntlDtlRecId.TotalInteger = useExport.PgCntlDtlRecId.TotalInteger;
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
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.ObligorCsePerson.Number = useExport.CsePerson.Number;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
  }

  private bool ReadCashReceiptSourceTypeCashReceiptTypeCashReceiptEvent()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;
    entities.ExistingCashReceiptType.Populated = false;
    entities.ExistingCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptSourceTypeCashReceiptTypeCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", export.DlgflwCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crdId",
          export.DlgflwCashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;
        entities.ExistingCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    entities.ExistingCollection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          import.DlgflwFromDebt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          import.DlgflwFromCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.DlgflwFromCashReceiptType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cashReceiptId",
          import.DlgflwFromCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crdId",
          import.DlgflwFromCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "collId",
          import.DlgflwFromCollection.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 1);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 2);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 3);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 5);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 6);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 7);
        entities.ExistingCollection.CpaType = db.GetString(reader, 8);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 9);
        entities.ExistingCollection.OtrType = db.GetString(reader, 10);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 11);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingObligee1.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", export.DlgflwCollection.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "otrId", export.DlgflwDebt.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "obgId", export.DlgflwObligation.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "otyId",
          export.DlgflwObligationType.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "cspNumberDisb", export.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligee1.Number = db.GetString(reader, 0);
        entities.ExistingObligee1.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingObligor1.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingObligor1.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          export.DlgflwCashReceiptDetail.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionLegalActionPerson()
  {
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;

    return Read("ReadLegalActionLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
        db.SetNullableString(
          command, "cspNumber", entities.ExistingObligor1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 2);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of LineType.
      /// </summary>
      [JsonPropertyName("lineType")]
      public TextWorkArea LineType
      {
        get => lineType ??= new();
        set => lineType = value;
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
      /// A value of CrdCrComboNo.
      /// </summary>
      [JsonPropertyName("crdCrComboNo")]
      public CrdCrComboNo CrdCrComboNo
      {
        get => crdCrComboNo ??= new();
        set => crdCrComboNo = value;
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
      /// A value of Process.
      /// </summary>
      [JsonPropertyName("process")]
      public DateWorkArea Process
      {
        get => process ??= new();
        set => process = value;
      }

      /// <summary>
      /// A value of PayeeRsn.
      /// </summary>
      [JsonPropertyName("payeeRsn")]
      public TextWorkArea PayeeRsn
      {
        get => payeeRsn ??= new();
        set => payeeRsn = value;
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
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>
      /// A value of Amt.
      /// </summary>
      [JsonPropertyName("amt")]
      public Common Amt
      {
        get => amt ??= new();
        set => amt = value;
      }

      /// <summary>
      /// A value of Pgm.
      /// </summary>
      [JsonPropertyName("pgm")]
      public WorkArea Pgm
      {
        get => pgm ??= new();
        set => pgm = value;
      }

      /// <summary>
      /// A value of RcvryExists.
      /// </summary>
      [JsonPropertyName("rcvryExists")]
      public TextWorkArea RcvryExists
      {
        get => rcvryExists ??= new();
        set => rcvryExists = value;
      }

      /// <summary>
      /// A value of DtlRecId.
      /// </summary>
      [JsonPropertyName("dtlRecId")]
      public Common DtlRecId
      {
        get => dtlRecId ??= new();
        set => dtlRecId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common common;
      private TextWorkArea lineType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private CrdCrComboNo crdCrComboNo;
      private Collection collection;
      private DateWorkArea process;
      private TextWorkArea payeeRsn;
      private Obligation obligation;
      private ObligationType obligationType;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private Common amt;
      private WorkArea pgm;
      private TextWorkArea rcvryExists;
      private Common dtlRecId;
    }

    /// <summary>A PgCntlGroup group.</summary>
    [Serializable]
    public class PgCntlGroup
    {
      /// <summary>
      /// A value of PgCntlCashReceipt.
      /// </summary>
      [JsonPropertyName("pgCntlCashReceipt")]
      public CashReceipt PgCntlCashReceipt
      {
        get => pgCntlCashReceipt ??= new();
        set => pgCntlCashReceipt = value;
      }

      /// <summary>
      /// A value of PgCntlCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("pgCntlCashReceiptDetail")]
      public CashReceiptDetail PgCntlCashReceiptDetail
      {
        get => pgCntlCashReceiptDetail ??= new();
        set => pgCntlCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of PgCntlDtlRecId.
      /// </summary>
      [JsonPropertyName("pgCntlDtlRecId")]
      public Common PgCntlDtlRecId
      {
        get => pgCntlDtlRecId ??= new();
        set => pgCntlDtlRecId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 350;

      private CashReceipt pgCntlCashReceipt;
      private CashReceiptDetail pgCntlCashReceiptDetail;
      private Common pgCntlDtlRecId;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptToName.
    /// </summary>
    [JsonPropertyName("promptToName")]
    public Common PromptToName
    {
      get => promptToName ??= new();
      set => promptToName = value;
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
    /// A value of PromptToLaps.
    /// </summary>
    [JsonPropertyName("promptToLaps")]
    public Common PromptToLaps
    {
      get => promptToLaps ??= new();
      set => promptToLaps = value;
    }

    /// <summary>
    /// A value of CollectionFrom.
    /// </summary>
    [JsonPropertyName("collectionFrom")]
    public DateWorkArea CollectionFrom
    {
      get => collectionFrom ??= new();
      set => collectionFrom = value;
    }

    /// <summary>
    /// A value of CollectionTo.
    /// </summary>
    [JsonPropertyName("collectionTo")]
    public DateWorkArea CollectionTo
    {
      get => collectionTo ??= new();
      set => collectionTo = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public TextWorkArea ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of ScrollInd.
    /// </summary>
    [JsonPropertyName("scrollInd")]
    public TextWorkArea ScrollInd
    {
      get => scrollInd ??= new();
      set => scrollInd = value;
    }

    /// <summary>
    /// A value of FlowTo.
    /// </summary>
    [JsonPropertyName("flowTo")]
    public Standard FlowTo
    {
      get => flowTo ??= new();
      set => flowTo = value;
    }

    /// <summary>
    /// A value of PromptToCdvl.
    /// </summary>
    [JsonPropertyName("promptToCdvl")]
    public Common PromptToCdvl
    {
      get => promptToCdvl ??= new();
      set => promptToCdvl = value;
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
    /// A value of PgPos.
    /// </summary>
    [JsonPropertyName("pgPos")]
    public Common PgPos
    {
      get => pgPos ??= new();
      set => pgPos = value;
    }

    /// <summary>
    /// Gets a value of PgCntl.
    /// </summary>
    [JsonIgnore]
    public Array<PgCntlGroup> PgCntl => pgCntl ??= new(PgCntlGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PgCntl for json serialization.
    /// </summary>
    [JsonPropertyName("pgCntl")]
    [Computed]
    public IList<PgCntlGroup> PgCntl_Json
    {
      get => pgCntl;
      set => PgCntl.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwCodeValue.
    /// </summary>
    [JsonPropertyName("dlgflwCodeValue")]
    public CodeValue DlgflwCodeValue
    {
      get => dlgflwCodeValue ??= new();
      set => dlgflwCodeValue = value;
    }

    /// <summary>
    /// A value of ObligorDlgflw.
    /// </summary>
    [JsonPropertyName("obligorDlgflw")]
    public CsePersonsWorkSet ObligorDlgflw
    {
      get => obligorDlgflw ??= new();
      set => obligorDlgflw = value;
    }

    /// <summary>
    /// A value of DlgflwLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwLegalAction")]
    public LegalAction DlgflwLegalAction
    {
      get => dlgflwLegalAction ??= new();
      set => dlgflwLegalAction = value;
    }

    /// <summary>
    /// A value of DlgflwFromDebt.
    /// </summary>
    [JsonPropertyName("dlgflwFromDebt")]
    public CashReceiptSourceType DlgflwFromDebt
    {
      get => dlgflwFromDebt ??= new();
      set => dlgflwFromDebt = value;
    }

    /// <summary>
    /// A value of DlgflwFromCashReceiptType.
    /// </summary>
    [JsonPropertyName("dlgflwFromCashReceiptType")]
    public CashReceiptType DlgflwFromCashReceiptType
    {
      get => dlgflwFromCashReceiptType ??= new();
      set => dlgflwFromCashReceiptType = value;
    }

    /// <summary>
    /// A value of DlgflwFromCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("dlgflwFromCashReceiptEvent")]
    public CashReceiptEvent DlgflwFromCashReceiptEvent
    {
      get => dlgflwFromCashReceiptEvent ??= new();
      set => dlgflwFromCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of DlgflwFromCashReceipt.
    /// </summary>
    [JsonPropertyName("dlgflwFromCashReceipt")]
    public CashReceipt DlgflwFromCashReceipt
    {
      get => dlgflwFromCashReceipt ??= new();
      set => dlgflwFromCashReceipt = value;
    }

    /// <summary>
    /// A value of DlgflwFromCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("dlgflwFromCashReceiptDetail")]
    public CashReceiptDetail DlgflwFromCashReceiptDetail
    {
      get => dlgflwFromCashReceiptDetail ??= new();
      set => dlgflwFromCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of DlgflwFromCollection.
    /// </summary>
    [JsonPropertyName("dlgflwFromCollection")]
    public Collection DlgflwFromCollection
    {
      get => dlgflwFromCollection ??= new();
      set => dlgflwFromCollection = value;
    }

    /// <summary>
    /// A value of TraceIndHidden.
    /// </summary>
    [JsonPropertyName("traceIndHidden")]
    public Common TraceIndHidden
    {
      get => traceIndHidden ??= new();
      set => traceIndHidden = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Standard standard;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Common promptToName;
    private LegalAction legalAction;
    private Common promptToLaps;
    private DateWorkArea collectionFrom;
    private DateWorkArea collectionTo;
    private TextWorkArea showHistory;
    private TextWorkArea scrollInd;
    private Standard flowTo;
    private Common promptToCdvl;
    private Array<GroupGroup> group;
    private Common pgPos;
    private Array<PgCntlGroup> pgCntl;
    private CodeValue dlgflwCodeValue;
    private CsePersonsWorkSet obligorDlgflw;
    private LegalAction dlgflwLegalAction;
    private CashReceiptSourceType dlgflwFromDebt;
    private CashReceiptType dlgflwFromCashReceiptType;
    private CashReceiptEvent dlgflwFromCashReceiptEvent;
    private CashReceipt dlgflwFromCashReceipt;
    private CashReceiptDetail dlgflwFromCashReceiptDetail;
    private Collection dlgflwFromCollection;
    private Common traceIndHidden;
    private CsePerson hiddenCsePerson;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of LineType.
      /// </summary>
      [JsonPropertyName("lineType")]
      public TextWorkArea LineType
      {
        get => lineType ??= new();
        set => lineType = value;
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
      /// A value of CrdCrComboNo.
      /// </summary>
      [JsonPropertyName("crdCrComboNo")]
      public CrdCrComboNo CrdCrComboNo
      {
        get => crdCrComboNo ??= new();
        set => crdCrComboNo = value;
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
      /// A value of Process.
      /// </summary>
      [JsonPropertyName("process")]
      public DateWorkArea Process
      {
        get => process ??= new();
        set => process = value;
      }

      /// <summary>
      /// A value of PayeeRsn.
      /// </summary>
      [JsonPropertyName("payeeRsn")]
      public TextWorkArea PayeeRsn
      {
        get => payeeRsn ??= new();
        set => payeeRsn = value;
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
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>
      /// A value of Amt.
      /// </summary>
      [JsonPropertyName("amt")]
      public Common Amt
      {
        get => amt ??= new();
        set => amt = value;
      }

      /// <summary>
      /// A value of Pgm.
      /// </summary>
      [JsonPropertyName("pgm")]
      public WorkArea Pgm
      {
        get => pgm ??= new();
        set => pgm = value;
      }

      /// <summary>
      /// A value of RcvryExists.
      /// </summary>
      [JsonPropertyName("rcvryExists")]
      public TextWorkArea RcvryExists
      {
        get => rcvryExists ??= new();
        set => rcvryExists = value;
      }

      /// <summary>
      /// A value of DtlRecId.
      /// </summary>
      [JsonPropertyName("dtlRecId")]
      public Common DtlRecId
      {
        get => dtlRecId ??= new();
        set => dtlRecId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common common;
      private TextWorkArea lineType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private CrdCrComboNo crdCrComboNo;
      private Collection collection;
      private DateWorkArea process;
      private TextWorkArea payeeRsn;
      private Obligation obligation;
      private ObligationType obligationType;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private Common amt;
      private WorkArea pgm;
      private TextWorkArea rcvryExists;
      private Common dtlRecId;
    }

    /// <summary>A PgCntlGroup group.</summary>
    [Serializable]
    public class PgCntlGroup
    {
      /// <summary>
      /// A value of PgCntlCashReceipt.
      /// </summary>
      [JsonPropertyName("pgCntlCashReceipt")]
      public CashReceipt PgCntlCashReceipt
      {
        get => pgCntlCashReceipt ??= new();
        set => pgCntlCashReceipt = value;
      }

      /// <summary>
      /// A value of PgCntlCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("pgCntlCashReceiptDetail")]
      public CashReceiptDetail PgCntlCashReceiptDetail
      {
        get => pgCntlCashReceiptDetail ??= new();
        set => pgCntlCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of PgCntlDtlRecId.
      /// </summary>
      [JsonPropertyName("pgCntlDtlRecId")]
      public Common PgCntlDtlRecId
      {
        get => pgCntlDtlRecId ??= new();
        set => pgCntlDtlRecId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 350;

      private CashReceipt pgCntlCashReceipt;
      private CashReceiptDetail pgCntlCashReceiptDetail;
      private Common pgCntlDtlRecId;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptToName.
    /// </summary>
    [JsonPropertyName("promptToName")]
    public Common PromptToName
    {
      get => promptToName ??= new();
      set => promptToName = value;
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
    /// A value of PromptToLaps.
    /// </summary>
    [JsonPropertyName("promptToLaps")]
    public Common PromptToLaps
    {
      get => promptToLaps ??= new();
      set => promptToLaps = value;
    }

    /// <summary>
    /// A value of CollectionFrom.
    /// </summary>
    [JsonPropertyName("collectionFrom")]
    public DateWorkArea CollectionFrom
    {
      get => collectionFrom ??= new();
      set => collectionFrom = value;
    }

    /// <summary>
    /// A value of CollectionTo.
    /// </summary>
    [JsonPropertyName("collectionTo")]
    public DateWorkArea CollectionTo
    {
      get => collectionTo ??= new();
      set => collectionTo = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public TextWorkArea ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of ScrollInd.
    /// </summary>
    [JsonPropertyName("scrollInd")]
    public TextWorkArea ScrollInd
    {
      get => scrollInd ??= new();
      set => scrollInd = value;
    }

    /// <summary>
    /// A value of FlowTo.
    /// </summary>
    [JsonPropertyName("flowTo")]
    public Standard FlowTo
    {
      get => flowTo ??= new();
      set => flowTo = value;
    }

    /// <summary>
    /// A value of PromptToCdvl.
    /// </summary>
    [JsonPropertyName("promptToCdvl")]
    public Common PromptToCdvl
    {
      get => promptToCdvl ??= new();
      set => promptToCdvl = value;
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
    /// A value of PgPos.
    /// </summary>
    [JsonPropertyName("pgPos")]
    public Common PgPos
    {
      get => pgPos ??= new();
      set => pgPos = value;
    }

    /// <summary>
    /// Gets a value of PgCntl.
    /// </summary>
    [JsonIgnore]
    public Array<PgCntlGroup> PgCntl => pgCntl ??= new(PgCntlGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PgCntl for json serialization.
    /// </summary>
    [JsonPropertyName("pgCntl")]
    [Computed]
    public IList<PgCntlGroup> PgCntl_Json
    {
      get => pgCntl;
      set => PgCntl.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwCode.
    /// </summary>
    [JsonPropertyName("dlgflwCode")]
    public Code DlgflwCode
    {
      get => dlgflwCode ??= new();
      set => dlgflwCode = value;
    }

    /// <summary>
    /// A value of DlgflwObligation.
    /// </summary>
    [JsonPropertyName("dlgflwObligation")]
    public Obligation DlgflwObligation
    {
      get => dlgflwObligation ??= new();
      set => dlgflwObligation = value;
    }

    /// <summary>
    /// A value of DlgflwObligationType.
    /// </summary>
    [JsonPropertyName("dlgflwObligationType")]
    public ObligationType DlgflwObligationType
    {
      get => dlgflwObligationType ??= new();
      set => dlgflwObligationType = value;
    }

    /// <summary>
    /// A value of DlgflwDebt.
    /// </summary>
    [JsonPropertyName("dlgflwDebt")]
    public ObligationTransaction DlgflwDebt
    {
      get => dlgflwDebt ??= new();
      set => dlgflwDebt = value;
    }

    /// <summary>
    /// A value of DlgflwCollection.
    /// </summary>
    [JsonPropertyName("dlgflwCollection")]
    public Collection DlgflwCollection
    {
      get => dlgflwCollection ??= new();
      set => dlgflwCollection = value;
    }

    /// <summary>
    /// A value of DlgflwFrom.
    /// </summary>
    [JsonPropertyName("dlgflwFrom")]
    public DateWorkArea DlgflwFrom
    {
      get => dlgflwFrom ??= new();
      set => dlgflwFrom = value;
    }

    /// <summary>
    /// A value of DlgflwTo.
    /// </summary>
    [JsonPropertyName("dlgflwTo")]
    public DateWorkArea DlgflwTo
    {
      get => dlgflwTo ??= new();
      set => dlgflwTo = value;
    }

    /// <summary>
    /// A value of DlgflwLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwLegalAction")]
    public LegalAction DlgflwLegalAction
    {
      get => dlgflwLegalAction ??= new();
      set => dlgflwLegalAction = value;
    }

    /// <summary>
    /// A value of DlgflwCashReceipt.
    /// </summary>
    [JsonPropertyName("dlgflwCashReceipt")]
    public CashReceipt DlgflwCashReceipt
    {
      get => dlgflwCashReceipt ??= new();
      set => dlgflwCashReceipt = value;
    }

    /// <summary>
    /// A value of DlgflwCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("dlgflwCashReceiptDetail")]
    public CashReceiptDetail DlgflwCashReceiptDetail
    {
      get => dlgflwCashReceiptDetail ??= new();
      set => dlgflwCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of DlgflwCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("dlgflwCashReceiptSourceType")]
    public CashReceiptSourceType DlgflwCashReceiptSourceType
    {
      get => dlgflwCashReceiptSourceType ??= new();
      set => dlgflwCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of DlgflwCashReceiptType.
    /// </summary>
    [JsonPropertyName("dlgflwCashReceiptType")]
    public CashReceiptType DlgflwCashReceiptType
    {
      get => dlgflwCashReceiptType ??= new();
      set => dlgflwCashReceiptType = value;
    }

    /// <summary>
    /// A value of DlgflwCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("dlgflwCashReceiptEvent")]
    public CashReceiptEvent DlgflwCashReceiptEvent
    {
      get => dlgflwCashReceiptEvent ??= new();
      set => dlgflwCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ObligeeDlgflwCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeDlgflwCsePerson")]
    public CsePerson ObligeeDlgflwCsePerson
    {
      get => obligeeDlgflwCsePerson ??= new();
      set => obligeeDlgflwCsePerson = value;
    }

    /// <summary>
    /// A value of LstDbtWAmtOwedDlgflw.
    /// </summary>
    [JsonPropertyName("lstDbtWAmtOwedDlgflw")]
    public Common LstDbtWAmtOwedDlgflw
    {
      get => lstDbtWAmtOwedDlgflw ??= new();
      set => lstDbtWAmtOwedDlgflw = value;
    }

    /// <summary>
    /// A value of ObligeeDlgflwCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligeeDlgflwCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligeeDlgflwCsePersonsWorkSet
    {
      get => obligeeDlgflwCsePersonsWorkSet ??= new();
      set => obligeeDlgflwCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DlgflwMonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("dlgflwMonthlyObligeeSummary")]
    public MonthlyObligeeSummary DlgflwMonthlyObligeeSummary
    {
      get => dlgflwMonthlyObligeeSummary ??= new();
      set => dlgflwMonthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of DlgflwForDisb.
    /// </summary>
    [JsonPropertyName("dlgflwForDisb")]
    public DateWorkArea DlgflwForDisb
    {
      get => dlgflwForDisb ??= new();
      set => dlgflwForDisb = value;
    }

    /// <summary>
    /// A value of DlgflwFirstTimeInd.
    /// </summary>
    [JsonPropertyName("dlgflwFirstTimeInd")]
    public Common DlgflwFirstTimeInd
    {
      get => dlgflwFirstTimeInd ??= new();
      set => dlgflwFirstTimeInd = value;
    }

    /// <summary>
    /// A value of PgCntlCashReceipt.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceipt")]
    public CashReceipt PgCntlCashReceipt
    {
      get => pgCntlCashReceipt ??= new();
      set => pgCntlCashReceipt = value;
    }

    /// <summary>
    /// A value of PgCntlCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceiptDetail")]
    public CashReceiptDetail PgCntlCashReceiptDetail
    {
      get => pgCntlCashReceiptDetail ??= new();
      set => pgCntlCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PgCntlDtlRecId.
    /// </summary>
    [JsonPropertyName("pgCntlDtlRecId")]
    public Common PgCntlDtlRecId
    {
      get => pgCntlDtlRecId ??= new();
      set => pgCntlDtlRecId = value;
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
    /// A value of TraceIndHidden.
    /// </summary>
    [JsonPropertyName("traceIndHidden")]
    public Common TraceIndHidden
    {
      get => traceIndHidden ??= new();
      set => traceIndHidden = value;
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

    private Standard standard;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Common promptToName;
    private LegalAction legalAction;
    private Common promptToLaps;
    private DateWorkArea collectionFrom;
    private DateWorkArea collectionTo;
    private TextWorkArea showHistory;
    private TextWorkArea scrollInd;
    private Standard flowTo;
    private Common promptToCdvl;
    private Array<GroupGroup> group;
    private Common pgPos;
    private Array<PgCntlGroup> pgCntl;
    private Code dlgflwCode;
    private Obligation dlgflwObligation;
    private ObligationType dlgflwObligationType;
    private ObligationTransaction dlgflwDebt;
    private Collection dlgflwCollection;
    private DateWorkArea dlgflwFrom;
    private DateWorkArea dlgflwTo;
    private LegalAction dlgflwLegalAction;
    private CashReceipt dlgflwCashReceipt;
    private CashReceiptDetail dlgflwCashReceiptDetail;
    private CashReceiptSourceType dlgflwCashReceiptSourceType;
    private CashReceiptType dlgflwCashReceiptType;
    private CashReceiptEvent dlgflwCashReceiptEvent;
    private CsePerson obligeeDlgflwCsePerson;
    private Common lstDbtWAmtOwedDlgflw;
    private CsePersonsWorkSet obligeeDlgflwCsePersonsWorkSet;
    private MonthlyObligeeSummary dlgflwMonthlyObligeeSummary;
    private DateWorkArea dlgflwForDisb;
    private Common dlgflwFirstTimeInd;
    private CashReceipt pgCntlCashReceipt;
    private CashReceiptDetail pgCntlCashReceiptDetail;
    private Common pgCntlDtlRecId;
    private CsePerson hiddenCsePerson;
    private Common traceIndHidden;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LineType.
    /// </summary>
    [JsonPropertyName("lineType")]
    public TextWorkArea LineType
    {
      get => lineType ??= new();
      set => lineType = value;
    }

    /// <summary>
    /// A value of PgCntlCashReceipt.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceipt")]
    public CashReceipt PgCntlCashReceipt
    {
      get => pgCntlCashReceipt ??= new();
      set => pgCntlCashReceipt = value;
    }

    /// <summary>
    /// A value of PgCntlCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceiptDetail")]
    public CashReceiptDetail PgCntlCashReceiptDetail
    {
      get => pgCntlCashReceiptDetail ??= new();
      set => pgCntlCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PgCntlDtlRecId.
    /// </summary>
    [JsonPropertyName("pgCntlDtlRecId")]
    public Common PgCntlDtlRecId
    {
      get => pgCntlDtlRecId ??= new();
      set => pgCntlDtlRecId = value;
    }

    /// <summary>
    /// A value of NullCashReceipt.
    /// </summary>
    [JsonPropertyName("nullCashReceipt")]
    public CashReceipt NullCashReceipt
    {
      get => nullCashReceipt ??= new();
      set => nullCashReceipt = value;
    }

    /// <summary>
    /// A value of NullCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("nullCashReceiptDetail")]
    public CashReceiptDetail NullCashReceiptDetail
    {
      get => nullCashReceiptDetail ??= new();
      set => nullCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of NullCollection.
    /// </summary>
    [JsonPropertyName("nullCollection")]
    public Collection NullCollection
    {
      get => nullCollection ??= new();
      set => nullCollection = value;
    }

    /// <summary>
    /// A value of NullCsePerson.
    /// </summary>
    [JsonPropertyName("nullCsePerson")]
    public CsePerson NullCsePerson
    {
      get => nullCsePerson ??= new();
      set => nullCsePerson = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of ObligeeStateOfKansas.
    /// </summary>
    [JsonPropertyName("obligeeStateOfKansas")]
    public CsePerson ObligeeStateOfKansas
    {
      get => obligeeStateOfKansas ??= new();
      set => obligeeStateOfKansas = value;
    }

    private TextWorkArea lineType;
    private CashReceipt pgCntlCashReceipt;
    private CashReceiptDetail pgCntlCashReceiptDetail;
    private Common pgCntlDtlRecId;
    private CashReceipt nullCashReceipt;
    private CashReceiptDetail nullCashReceiptDetail;
    private Collection nullCollection;
    private CsePerson nullCsePerson;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private DateWorkArea current;
    private Common common;
    private DateWorkArea nullDateWorkArea;
    private CsePerson obligeeStateOfKansas;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingLegalActionPerson")]
    public LegalActionPerson ExistingLegalActionPerson
    {
      get => existingLegalActionPerson ??= new();
      set => existingLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingObligee1.
    /// </summary>
    [JsonPropertyName("existingObligee1")]
    public CsePerson ExistingObligee1
    {
      get => existingObligee1 ??= new();
      set => existingObligee1 = value;
    }

    /// <summary>
    /// A value of ExistingObligee2.
    /// </summary>
    [JsonPropertyName("existingObligee2")]
    public CsePersonAccount ExistingObligee2
    {
      get => existingObligee2 ??= new();
      set => existingObligee2 = value;
    }

    /// <summary>
    /// A value of ExistingDisbCollection.
    /// </summary>
    [JsonPropertyName("existingDisbCollection")]
    public DisbursementTransaction ExistingDisbCollection
    {
      get => existingDisbCollection ??= new();
      set => existingDisbCollection = value;
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

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
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
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private ObligationTransaction existingDebt;
    private LegalAction existingLegalAction;
    private LegalActionPerson existingLegalActionPerson;
    private CsePerson existingObligee1;
    private CsePersonAccount existingObligee2;
    private DisbursementTransaction existingDisbCollection;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptType existingCashReceiptType;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private Collection existingCollection;
  }
#endregion
}

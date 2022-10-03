// Program: FN_CRBI_LST_CR_INTERFACE_BALANCE, ID: 372339635, model: 746.
// Short name: SWECRBIP
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
/// A program: FN_CRBI_LST_CR_INTERFACE_BALANCE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrbiLstCrInterfaceBalance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRBI_LST_CR_INTERFACE_BALANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrbiLstCrInterfaceBalance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrbiLstCrInterfaceBalance.
  /// </summary>
  public FnCrbiLstCrInterfaceBalance(IContext context, Import import,
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
    // ----------------------------------------------------------------------
    // Procedure:  Balance CR to Interface
    // Developer : Bryan Fristrup, MTW
    // Start Date: 03/01/1996
    // Change Log :
    // ----------------------------------------------------------------------
    // Date	  Programmer	   Reference #	Description
    // ----------------------------------------------------------------------
    // 04/05/96  Holly kennedy-MTW		Retrofit to bring screen up to
    // 					Standard.
    // 12/03/96  R. Marchman			Add new security and next tran.
    // 09/24/97  Siraj Konkader		Added PF18 CRAJ and PF19 CRIA
    // 					to the screen and to Case Of
    // 					Command. Dialog flows were
    // 					already defined...completely!
    // 01/26/99  J Katz			Allow the flow to CREC
    // 					regardless of whether the
    // 					interface receipt has been
    // 					matched to a cash receipt.
    // 03/09/99  J Katz			Add flow from CRBI to CRDA
    // 					with supporting logic.
    // 06/08/99  J Katz			Analyze READ statements and
    // 					set read property to Select
    // 					Only where appropriate.
    // ----------------------------------------------------------------------
    // 10/20/99  P Phinney    H00077900        Add two New
    // Cash_Receipt_Rln_Rsn
    // PROCCSTFEE and NETINTFERR
    // for use with creating
    // Receipt Amount Adjustments
    // on CRIA
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "FROMCRAJ":
        export.CallingPrad.Command = global.Command;
        global.Command = "DISPLAY";

        break;
      case "FROMCRDA":
        export.CallingPrad.Command = global.Command;
        global.Command = "DISPLAY";

        break;
      case "FROMCRIA":
        export.CallingPrad.Command = global.Command;
        global.Command = "DISPLAY";

        break;
      default:
        export.CallingPrad.Command = import.CallingPrad.Command;

        break;
    }

    // ----------------------------------------------------------------
    // If the next tran info is not equal to spaces, the user
    // requested a next tran action.  Now validate.
    // ----------------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // Set the export hidden next_tran_info attributes to the import
      // view attributes for the data to be passed to the next
      // ransaction.
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
      // Set the export value to the export hidden next tran values if
      // the procedure is accessed by a next tran action.
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    // ----------------------------------------------------------------
    // Validate action level security.  If the command is to link to
    // another screen, the security validation will be done in the
    // called procedures.
    // Only the Display command requires a security check.
    // JLK  01/20/99
    // ----------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ----------------------------------------------------------------
    // Move imports to exports.
    // ----------------------------------------------------------------
    if (IsEmpty(import.PassArea.Code))
    {
      MoveCashReceiptSourceType(import.CashReceiptSourceType,
        export.CashReceiptSourceType);
    }
    else
    {
      MoveCashReceiptSourceType(import.PassArea, export.CashReceiptSourceType);
      global.Command = "DISPLAY";
    }

    if (IsEmpty(import.OutOfBalanceOnly.OneChar))
    {
      export.OfBalanceOnly.OneChar = "Y";
    }
    else
    {
      export.OfBalanceOnly.OneChar = import.OutOfBalanceOnly.OneChar;
    }

    export.FromDate.Date = import.FromDate.Date;
    export.ToDate.Date = import.ToDate.Date;
    export.HiddenCashReceiptSourceType.Code =
      import.HiddenCashReceiptSourceType.Code;
    export.HiddenFromDate.Date = import.HiddenFromDate.Date;
    export.HiddenToDate.Date = import.HiddenToDate.Date;
    export.HiddenOutOfBalance.OneChar = import.HiddenOutOfBalance.OneChar;
    local.Select.Count = 0;

    if (Equal(export.CashReceiptSourceType.Code,
      export.HiddenCashReceiptSourceType.Code) && Equal
      (export.FromDate.Date, export.HiddenFromDate.Date) && Equal
      (export.ToDate.Date, export.HiddenToDate.Date) && AsChar
      (export.OfBalanceOnly.OneChar) == AsChar
      (export.HiddenOutOfBalance.OneChar))
    {
      // ----------------------------------------------------------------
      // If the command is DISPLAY, do not move imports to
      // exports to initialize the group view.
      // ----------------------------------------------------------------
      if (!Equal(global.Command, "DISPLAY"))
      {
        // *** Move IMPORT VIEWS TO EXPORT VIEWS ***
        export.TotalBalDue.TotalCurrency = import.TotalBalDue.TotalCurrency;
        export.TotalDispBalDue.TotalCurrency =
          import.TotalDispBalDue.TotalCurrency;

        export.List.Index = 0;
        export.List.Clear();

        for(import.List.Index = 0; import.List.Index < import.List.Count; ++
          import.List.Index)
        {
          if (export.List.IsFull)
          {
            break;
          }

          export.List.Update.MbrCommon.SelectChar =
            import.List.Item.MbrCommon.SelectChar;
          MoveCashReceiptEvent(import.List.Item.MbrCashReceiptEvent,
            export.List.Update.MbrCashReceiptEvent);
          export.List.Update.MbrCashReceipt.Assign(
            import.List.Item.MbrCashReceipt);
          export.List.Update.MbrIntfAdjAmt.TotalCurrency =
            import.List.Item.MbrIntfAdjAmt.TotalCurrency;
          export.List.Update.MbrRcptAdjAmt.TotalCurrency =
            import.List.Item.MbrRcptAdjAmt.TotalCurrency;
          export.List.Update.MbrNetRcptAmt.TotalCurrency =
            import.List.Item.MbrNetRcptAmt.TotalCurrency;
          MoveCashReceiptType(import.List.Item.MbrHidden,
            export.List.Update.MbrHidden);

          // ----------------------------------------------------------------
          // Validate the selection characters.
          // ----------------------------------------------------------------
          switch(AsChar(export.List.Item.MbrCommon.SelectChar))
          {
            case 'S':
              ++local.Select.Count;

              // -----------------------------------------------------------
              // If the net interface amount [Cash Receipt Cash Due] is
              // greater than or equal to zero and the Receipt Amount is
              // zero, do not allow the flow to CRIA.   JLK  10/28/99
              // -----------------------------------------------------------
              if (Equal(global.Command, "CRIA") || Equal
                (global.Command, "CRAJ"))
              {
                if (import.List.Item.MbrCashReceipt.CashDue.
                  GetValueOrDefault() >= 0 && import
                  .List.Item.MbrCashReceipt.ReceiptAmount == 0)
                {
                  ExitState = "FN0000_RCPT_AMT_MUST_BE_GT_ZERO";

                  var field1 =
                    GetField(export.List.Item.MbrCommon, "selectChar");

                  field1.Error = true;
                }
              }

              if (local.Select.Count == 1)
              {
                export.PassAreaCashReceiptSourceType.SystemGeneratedIdentifier =
                  export.CashReceiptSourceType.SystemGeneratedIdentifier;
                export.PassAreaFirstCashReceiptEvent.SystemGeneratedIdentifier =
                  export.List.Item.MbrCashReceiptEvent.
                    SystemGeneratedIdentifier;
                export.PassAreaFirstCashReceipt.Assign(
                  export.List.Item.MbrCashReceipt);
              }
              else if (local.Select.Count == 2)
              {
                export.PassAreaSecond.Assign(export.List.Item.MbrCashReceipt);
              }

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.List.Item.MbrCommon, "selectChar");

              field.Error = true;

              break;
          }

          export.List.Next();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          switch(local.Select.Count)
          {
            case 0:
              break;
            case 1:
              if (Equal(global.Command, "RETURN"))
              {
                switch(TrimEnd(export.CallingPrad.Command))
                {
                  case "FROMCRAJ":
                    if (export.PassAreaFirstCashReceipt.ReceiptAmount == 0)
                    {
                      ExitState = "FN0000_RCPT_AMT_MUST_BE_GT_ZERO";

                      return;
                    }

                    break;
                  case "FROMCRIA":
                    if (export.PassAreaFirstCashReceipt.ReceiptAmount == 0 && export
                      .PassAreaFirstCashReceipt.CashDue.GetValueOrDefault() >= 0
                      )
                    {
                      ExitState = "FN0000_RCPT_AMT_MUST_BE_GT_ZERO";

                      return;
                    }

                    if (export.PassAreaSecond.ReceiptAmount == 0 && export
                      .PassAreaSecond.CashDue.GetValueOrDefault() >= 0)
                    {
                      ExitState = "FN0000_RCPT_AMT_MUST_BE_GT_ZERO";

                      return;
                    }

                    break;
                  default:
                    break;
                }
              }

              break;
            case 2:
              if (Equal(global.Command, "CRAJ") || Equal
                (global.Command, "CRDA") || Equal(global.Command, "CRDL") || Equal
                (global.Command, "CREC") || Equal(global.Command, "RMSR"))
              {
                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }
              else if (Equal(global.Command, "RETURN") && !
                Equal(export.CallingPrad.Command, "CRIA"))
              {
                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }

              break;
            default:
              if (Equal(global.Command, "CRIA"))
              {
                ExitState = "FN0000_ONLY_SELECT_2";

                return;
              }
              else
              {
                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }

              return;
          }
        }
        else
        {
          return;
        }
      }
    }
    else
    {
      local.SearchCriteriaChanged.Flag = "Y";
    }

    // ----------------------------------------------------------------
    // Main CASE OF command structure.
    // ----------------------------------------------------------------
    if (Equal(global.Command, "CREC") || Equal(global.Command, "CRDL") || Equal
      (global.Command, "CRAJ") || Equal(global.Command, "CRIA") || Equal
      (global.Command, "CRDA") || Equal(global.Command, "RETURN"))
    {
      // ------------------------------------------------------------
      // Determine if the search criteria has changed.
      // If it has, the user must first press PF2 Display.
      // ------------------------------------------------------------
      if (AsChar(local.SearchCriteriaChanged.Flag) == 'Y')
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ------------------------------------------------------------
        // PF2 Display
        // Validate that a Source Type Code has been entered.
        // ------------------------------------------------------------
        if (IsEmpty(export.CashReceiptSourceType.Code))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field1 = GetField(export.CashReceiptSourceType, "code");

          field1.Error = true;

          return;
        }

        // ------------------------------------------------------------
        // Validate the "Out-of-Balance Only" flag.
        // ------------------------------------------------------------
        switch(AsChar(export.OfBalanceOnly.OneChar))
        {
          case 'Y':
            break;
          case 'N':
            break;
          default:
            var field1 = GetField(export.OfBalanceOnly, "oneChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

            return;
        }

        // ------------------------------------------------------------
        // Populate the group view.
        // ------------------------------------------------------------
        if (Equal(export.FromDate.Date, new DateTime(1, 1, 1)))
        {
          export.FromDate.Date = local.Null1.Date;
        }

        if (Equal(export.ToDate.Date, new DateTime(1, 1, 1)))
        {
          export.ToDate.Date = local.Null1.Date;
        }

        UseFnCabBuildIntfBalRcptList();

        // ----------------------------------------------------------------
        // Check for processing errors.
        // ----------------------------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else if (IsExitState("FN0097_CASH_RCPT_SOURCE_TYPE_NF"))
        {
          var field1 = GetField(export.CashReceiptSourceType, "code");

          field1.Error = true;

          return;
        }
        else if (IsExitState("FN0000_CR_SRC_TYPE_NOT_INTERFACE"))
        {
          var field1 = GetField(export.CashReceiptSourceType, "code");

          field1.Error = true;

          return;
        }
        else if (IsExitState("FROM_DATE_GREATER_THAN_TO_DATE"))
        {
          var field1 = GetField(export.FromDate, "date");

          field1.Error = true;

          var field2 = GetField(export.ToDate, "date");

          field2.Error = true;

          return;
        }
        else
        {
          return;
        }

        export.HiddenCashReceiptSourceType.Code =
          export.CashReceiptSourceType.Code;
        export.HiddenFromDate.Date = export.FromDate.Date;
        export.HiddenToDate.Date = export.ToDate.Date;
        export.HiddenOutOfBalance.OneChar = export.OfBalanceOnly.OneChar;

        // ----------------------------------------------------------------
        // Display informational message regarding status of display action.
        // ----------------------------------------------------------------
        if (export.List.IsEmpty)
        {
          ExitState = "FN0000_CR_SEARCH_CRITERIA_NF";
        }
        else if (export.List.IsFull)
        {
          ExitState = "FN0000_CR_GROUP_VIEW_OVERFLOW";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST":
        // ------------------------------------------------------------
        // PF4 List   Displays CRSL - List Cash Receipt Sources.
        // ------------------------------------------------------------
        if (AsChar(import.CrSrcType.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_LST_CASH_SOURCES";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.CrSrcType, "promptField");

          field1.Error = true;
        }

        break;
      case "RETLINK":
        // ----------------------------------------------------------------
        // Logic processed upon returning from PF4 List action.
        // ----------------------------------------------------------------
        var field = GetField(export.FromDate, "date");

        field.Protected = false;
        field.Focused = true;

        break;
      case "PREV":
        // ----------------------------------------------------------------
        // PF7 Prev
        // ----------------------------------------------------------------
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        // ----------------------------------------------------------------
        // PF8 Next
        // ----------------------------------------------------------------
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        // ----------------------------------------------------------------
        // PF9 Return
        // Command is valid when screen is accessed from CRAJ or
        // CRIA.
        // Selection is optional, but only one Receipt can be
        // selected.
        // ----------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_RETURN";

            break;
          case 1:
            ExitState = "ACO_NE0000_RETURN";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "SIGNOFF":
        // ----------------------------------------------------------------
        // PF12 Signoff
        // ----------------------------------------------------------------
        UseScCabSignoff();

        break;
      case "CREC":
        // ----------------------------------------------------------------
        // PF16 CREC
        // Link to CASH RECEIPTING Procedure.
        // Only one Receipt can be selected.
        // ----------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_CASH_RECEIPTING";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "CRDL":
        // ----------------------------------------------------------------
        // PF17 CRDL
        // Link to Cash Receipt Detail List.
        // Only one Receipt can be selected.
        // ----------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_CRDL";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "CRAJ":
        // ----------------------------------------------------------------
        // PF18 CRAJ
        // Link to Cash Receipt Balance Adjustment List.
        // Only one Receipt can be selected.
        // ----------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            ExitState = "ECO_LNK_LST_CR_BAL_ADJ";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "CRIA":
        // ----------------------------------------------------------------
        // PF19 CRIA
        // Link to Adjust Receipted Interface.
        // One or two Receipts may be selected.
        // ----------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            // 10/20/99  P Phinney    H00077900
            // Due to NEW Rln_Rsn codes
            // can NO LONGER Default with ONE selection!!!
            export.PassAreaCashReceiptRlnRsn.Code = "";
            ExitState = "ECO_LNK_TO_ADJUST_REC_INTERFACE";

            break;
          case 2:
            export.PassAreaCashReceiptRlnRsn.Code = "NETPMT";
            ExitState = "ECO_LNK_TO_ADJUST_REC_INTERFACE";

            break;
          default:
            ExitState = "FN0000_ONLY_SELECT_2";

            break;
        }

        break;
      case "CRDA":
        // ----------------------------------------------------------------
        // PF20  CRDA       JLK 03/09/99
        // Link to List Cash Receipt Detail Adjustments.
        // Only one Receipt can be selected.
        // ----------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_CRDA";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "RMSR":
        // ----------------------------------------------------------------
        // PF21 RMSR
        // Link to Request Miscellaneous Refunds screen.
        // One Receipt may be selected.
        // ----------------------------------------------------------------
        ExitState = "ECO_LNK_TO_RMSR";

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
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

  private static void MoveList(FnCabBuildIntfBalRcptList.Export.
    ListGroup source, Export.ListGroup target)
  {
    target.MbrCommon.SelectChar = source.MbrCommon.SelectChar;
    MoveCashReceiptEvent(source.MbrCashReceiptEvent, target.MbrCashReceiptEvent);
      
    target.MbrCashReceipt.Assign(source.MbrCashReceipt);
    target.MbrIntfAdjAmt.TotalCurrency = source.MbrIntfAdjAmt.TotalCurrency;
    target.MbrRcptAdjAmt.TotalCurrency = source.MbrRcptAdjAmt.TotalCurrency;
    target.MbrNetRcptAmt.TotalCurrency = source.MbrNetRcptAmt.TotalCurrency;
    MoveCashReceiptType(source.MbrHidden, target.MbrHidden);
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

  private void UseFnCabBuildIntfBalRcptList()
  {
    var useImport = new FnCabBuildIntfBalRcptList.Import();
    var useExport = new FnCabBuildIntfBalRcptList.Export();

    useImport.CashReceiptSourceType.Code = export.CashReceiptSourceType.Code;
    useImport.From.Date = export.FromDate.Date;
    useImport.To.Date = export.ToDate.Date;
    useImport.OutOfBalanceOnly.OneChar = export.OfBalanceOnly.OneChar;

    Call(FnCabBuildIntfBalRcptList.Execute, useImport, useExport);

    export.TotalBalDue.TotalCurrency = useExport.TotalBalDue.TotalCurrency;
    export.TotalDispBalDue.TotalCurrency =
      useExport.TotalDispBalDue.TotalCurrency;
    useExport.List.CopyTo(export.List, MoveList);
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of MbrCommon.
      /// </summary>
      [JsonPropertyName("mbrCommon")]
      public Common MbrCommon
      {
        get => mbrCommon ??= new();
        set => mbrCommon = value;
      }

      /// <summary>
      /// A value of MbrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrCashReceiptEvent")]
      public CashReceiptEvent MbrCashReceiptEvent
      {
        get => mbrCashReceiptEvent ??= new();
        set => mbrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrCashReceipt")]
      public CashReceipt MbrCashReceipt
      {
        get => mbrCashReceipt ??= new();
        set => mbrCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrIntfAdjAmt.
      /// </summary>
      [JsonPropertyName("mbrIntfAdjAmt")]
      public Common MbrIntfAdjAmt
      {
        get => mbrIntfAdjAmt ??= new();
        set => mbrIntfAdjAmt = value;
      }

      /// <summary>
      /// A value of MbrRcptAdjAmt.
      /// </summary>
      [JsonPropertyName("mbrRcptAdjAmt")]
      public Common MbrRcptAdjAmt
      {
        get => mbrRcptAdjAmt ??= new();
        set => mbrRcptAdjAmt = value;
      }

      /// <summary>
      /// A value of MbrNetRcptAmt.
      /// </summary>
      [JsonPropertyName("mbrNetRcptAmt")]
      public Common MbrNetRcptAmt
      {
        get => mbrNetRcptAmt ??= new();
        set => mbrNetRcptAmt = value;
      }

      /// <summary>
      /// A value of MbrHidden.
      /// </summary>
      [JsonPropertyName("mbrHidden")]
      public CashReceiptType MbrHidden
      {
        get => mbrHidden ??= new();
        set => mbrHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common mbrCommon;
      private CashReceiptEvent mbrCashReceiptEvent;
      private CashReceipt mbrCashReceipt;
      private Common mbrIntfAdjAmt;
      private Common mbrRcptAdjAmt;
      private Common mbrNetRcptAmt;
      private CashReceiptType mbrHidden;
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
    /// A value of CrSrcType.
    /// </summary>
    [JsonPropertyName("crSrcType")]
    public Standard CrSrcType
    {
      get => crSrcType ??= new();
      set => crSrcType = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DateWorkArea FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of ToDate.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DateWorkArea ToDate
    {
      get => toDate ??= new();
      set => toDate = value;
    }

    /// <summary>
    /// A value of OutOfBalanceOnly.
    /// </summary>
    [JsonPropertyName("outOfBalanceOnly")]
    public Standard OutOfBalanceOnly
    {
      get => outOfBalanceOnly ??= new();
      set => outOfBalanceOnly = value;
    }

    /// <summary>
    /// A value of TotalBalDue.
    /// </summary>
    [JsonPropertyName("totalBalDue")]
    public Common TotalBalDue
    {
      get => totalBalDue ??= new();
      set => totalBalDue = value;
    }

    /// <summary>
    /// A value of TotalDispBalDue.
    /// </summary>
    [JsonPropertyName("totalDispBalDue")]
    public Common TotalDispBalDue
    {
      get => totalDispBalDue ??= new();
      set => totalDispBalDue = value;
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
    /// A value of HiddenFromDate.
    /// </summary>
    [JsonPropertyName("hiddenFromDate")]
    public DateWorkArea HiddenFromDate
    {
      get => hiddenFromDate ??= new();
      set => hiddenFromDate = value;
    }

    /// <summary>
    /// A value of HiddenToDate.
    /// </summary>
    [JsonPropertyName("hiddenToDate")]
    public DateWorkArea HiddenToDate
    {
      get => hiddenToDate ??= new();
      set => hiddenToDate = value;
    }

    /// <summary>
    /// A value of HiddenOutOfBalance.
    /// </summary>
    [JsonPropertyName("hiddenOutOfBalance")]
    public Standard HiddenOutOfBalance
    {
      get => hiddenOutOfBalance ??= new();
      set => hiddenOutOfBalance = value;
    }

    /// <summary>
    /// A value of CallingPrad.
    /// </summary>
    [JsonPropertyName("callingPrad")]
    public Common CallingPrad
    {
      get => callingPrad ??= new();
      set => callingPrad = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public CashReceiptSourceType PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private Standard crSrcType;
    private DateWorkArea fromDate;
    private DateWorkArea toDate;
    private Standard outOfBalanceOnly;
    private Common totalBalDue;
    private Common totalDispBalDue;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private DateWorkArea hiddenFromDate;
    private DateWorkArea hiddenToDate;
    private Standard hiddenOutOfBalance;
    private Common callingPrad;
    private Array<ListGroup> list;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CashReceiptSourceType passArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of MbrCommon.
      /// </summary>
      [JsonPropertyName("mbrCommon")]
      public Common MbrCommon
      {
        get => mbrCommon ??= new();
        set => mbrCommon = value;
      }

      /// <summary>
      /// A value of MbrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrCashReceiptEvent")]
      public CashReceiptEvent MbrCashReceiptEvent
      {
        get => mbrCashReceiptEvent ??= new();
        set => mbrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrCashReceipt")]
      public CashReceipt MbrCashReceipt
      {
        get => mbrCashReceipt ??= new();
        set => mbrCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrIntfAdjAmt.
      /// </summary>
      [JsonPropertyName("mbrIntfAdjAmt")]
      public Common MbrIntfAdjAmt
      {
        get => mbrIntfAdjAmt ??= new();
        set => mbrIntfAdjAmt = value;
      }

      /// <summary>
      /// A value of MbrRcptAdjAmt.
      /// </summary>
      [JsonPropertyName("mbrRcptAdjAmt")]
      public Common MbrRcptAdjAmt
      {
        get => mbrRcptAdjAmt ??= new();
        set => mbrRcptAdjAmt = value;
      }

      /// <summary>
      /// A value of MbrNetRcptAmt.
      /// </summary>
      [JsonPropertyName("mbrNetRcptAmt")]
      public Common MbrNetRcptAmt
      {
        get => mbrNetRcptAmt ??= new();
        set => mbrNetRcptAmt = value;
      }

      /// <summary>
      /// A value of MbrHidden.
      /// </summary>
      [JsonPropertyName("mbrHidden")]
      public CashReceiptType MbrHidden
      {
        get => mbrHidden ??= new();
        set => mbrHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common mbrCommon;
      private CashReceiptEvent mbrCashReceiptEvent;
      private CashReceipt mbrCashReceipt;
      private Common mbrIntfAdjAmt;
      private Common mbrRcptAdjAmt;
      private Common mbrNetRcptAmt;
      private CashReceiptType mbrHidden;
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
    /// A value of CrSrcType.
    /// </summary>
    [JsonPropertyName("crSrcType")]
    public Standard CrSrcType
    {
      get => crSrcType ??= new();
      set => crSrcType = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DateWorkArea FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of ToDate.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DateWorkArea ToDate
    {
      get => toDate ??= new();
      set => toDate = value;
    }

    /// <summary>
    /// A value of OfBalanceOnly.
    /// </summary>
    [JsonPropertyName("ofBalanceOnly")]
    public Standard OfBalanceOnly
    {
      get => ofBalanceOnly ??= new();
      set => ofBalanceOnly = value;
    }

    /// <summary>
    /// A value of TotalBalDue.
    /// </summary>
    [JsonPropertyName("totalBalDue")]
    public Common TotalBalDue
    {
      get => totalBalDue ??= new();
      set => totalBalDue = value;
    }

    /// <summary>
    /// A value of TotalDispBalDue.
    /// </summary>
    [JsonPropertyName("totalDispBalDue")]
    public Common TotalDispBalDue
    {
      get => totalDispBalDue ??= new();
      set => totalDispBalDue = value;
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
    /// A value of HiddenFromDate.
    /// </summary>
    [JsonPropertyName("hiddenFromDate")]
    public DateWorkArea HiddenFromDate
    {
      get => hiddenFromDate ??= new();
      set => hiddenFromDate = value;
    }

    /// <summary>
    /// A value of HiddenToDate.
    /// </summary>
    [JsonPropertyName("hiddenToDate")]
    public DateWorkArea HiddenToDate
    {
      get => hiddenToDate ??= new();
      set => hiddenToDate = value;
    }

    /// <summary>
    /// A value of HiddenOutOfBalance.
    /// </summary>
    [JsonPropertyName("hiddenOutOfBalance")]
    public Standard HiddenOutOfBalance
    {
      get => hiddenOutOfBalance ??= new();
      set => hiddenOutOfBalance = value;
    }

    /// <summary>
    /// A value of CallingPrad.
    /// </summary>
    [JsonPropertyName("callingPrad")]
    public Common CallingPrad
    {
      get => callingPrad ??= new();
      set => callingPrad = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of PassAreaCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("passAreaCashReceiptSourceType")]
    public CashReceiptSourceType PassAreaCashReceiptSourceType
    {
      get => passAreaCashReceiptSourceType ??= new();
      set => passAreaCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PassAreaCashReceiptType.
    /// </summary>
    [JsonPropertyName("passAreaCashReceiptType")]
    public CashReceiptType PassAreaCashReceiptType
    {
      get => passAreaCashReceiptType ??= new();
      set => passAreaCashReceiptType = value;
    }

    /// <summary>
    /// A value of PassAreaFirstCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("passAreaFirstCashReceiptEvent")]
    public CashReceiptEvent PassAreaFirstCashReceiptEvent
    {
      get => passAreaFirstCashReceiptEvent ??= new();
      set => passAreaFirstCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of PassAreaFirstCashReceipt.
    /// </summary>
    [JsonPropertyName("passAreaFirstCashReceipt")]
    public CashReceipt PassAreaFirstCashReceipt
    {
      get => passAreaFirstCashReceipt ??= new();
      set => passAreaFirstCashReceipt = value;
    }

    /// <summary>
    /// A value of PassAreaSecond.
    /// </summary>
    [JsonPropertyName("passAreaSecond")]
    public CashReceipt PassAreaSecond
    {
      get => passAreaSecond ??= new();
      set => passAreaSecond = value;
    }

    /// <summary>
    /// A value of PassAreaCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("passAreaCashReceiptRlnRsn")]
    public CashReceiptRlnRsn PassAreaCashReceiptRlnRsn
    {
      get => passAreaCashReceiptRlnRsn ??= new();
      set => passAreaCashReceiptRlnRsn = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private Standard crSrcType;
    private DateWorkArea fromDate;
    private DateWorkArea toDate;
    private Standard ofBalanceOnly;
    private Common totalBalDue;
    private Common totalDispBalDue;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private DateWorkArea hiddenFromDate;
    private DateWorkArea hiddenToDate;
    private Standard hiddenOutOfBalance;
    private Common callingPrad;
    private Array<ListGroup> list;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CashReceiptSourceType passAreaCashReceiptSourceType;
    private CashReceiptType passAreaCashReceiptType;
    private CashReceiptEvent passAreaFirstCashReceiptEvent;
    private CashReceipt passAreaFirstCashReceipt;
    private CashReceipt passAreaSecond;
    private CashReceiptRlnRsn passAreaCashReceiptRlnRsn;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of SearchCriteriaChanged.
    /// </summary>
    [JsonPropertyName("searchCriteriaChanged")]
    public Common SearchCriteriaChanged
    {
      get => searchCriteriaChanged ??= new();
      set => searchCriteriaChanged = value;
    }

    private DateWorkArea null1;
    private Common select;
    private Common searchCriteriaChanged;
  }
#endregion
}

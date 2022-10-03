// Program: FN_CRAJ_LST_CR_BALANCE_ADJSTMTS, ID: 372338432, model: 746.
// Short name: SWECRAJP
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
/// A program: FN_CRAJ_LST_CR_BALANCE_ADJSTMTS.
/// </para>
/// <para>
/// This procedure lists all of the Cash Receipt Balance Adjustments for a given
/// Cash Receipt.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrajLstCrBalanceAdjstmts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRAJ_LST_CR_BALANCE_ADJSTMTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrajLstCrBalanceAdjstmts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrajLstCrBalanceAdjstmts.
  /// </summary>
  public FnCrajLstCrBalanceAdjstmts(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Procedure:  Balance CR to Interface
    // Developer : Bryan Fristrup, MTW
    // Start Date: 03/01/1996
    // -------------------------------------------------------------------
    // Change Log :
    // -------------------------------------------------------------------
    // 12/03/96  R. Marchman	Add new security and next tran.
    // 01/18/99  J. Katz	Modify security validation to only act
    // 			on command Display.
    // 			Removed logic for Exit command.
    // 			(Autoflow handles exit flow.)
    // 02/26/99  J. Katz	Redesign screen and logic to only display
    // 			adjustments that apply to the receipt amount.
    // 06/08/99  J. Katz	Analyze READ statements and change read
    // 			property to Select Only where appropriate.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    switch(TrimEnd(global.Command))
    {
      case "CRBI":
        global.Command = "DISPLAY";

        break;
      case "CRIA":
        global.Command = "DISPLAY";

        break;
      case "CLEAR":
        ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

        return;
      case "XXFMMENU":
        return;
      default:
        break;
    }

    // ---------------------------------------------------------------------
    // Move imports to exports.
    // ---------------------------------------------------------------------
    if (import.PassArea.SequentialNumber == 0)
    {
      if (import.CashReceipt.SequentialNumber == import
        .HiddenCashReceipt.SequentialNumber && import
        .HiddenCashReceipt.SequentialNumber > 0 && !
        Equal(global.Command, "DISPLAY"))
      {
        // ---------------------------------------------------------------
        // Move imports to exports.
        // ---------------------------------------------------------------
        MoveCashReceiptSourceType(import.CashReceiptSourceType,
          export.CashReceiptSourceType);
        export.CashReceiptEvent.Assign(import.CashReceiptEvent);
        export.CashReceipt.Assign(import.CashReceipt);
        export.NetReceiptAmt.TotalCurrency = import.NetReceiptAmt.TotalCurrency;
        export.HiddenCashReceipt.SequentialNumber =
          import.HiddenCashReceipt.SequentialNumber;

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          // ---------------------------------------------------------------
          // Place the cursor on the first occurrence in the group view.
          // When targeting a group view, the group view is considered
          // "EMPTY" until the first occurrence is complete.
          // ---------------------------------------------------------------
          export.Export1.Update.DetailCommon.SelectChar =
            import.Import1.Item.DetailCommon.SelectChar;
          MoveCashReceiptRlnRsn(import.Import1.Item.DetailCashReceiptRlnRsn,
            export.Export1.Update.DetailCashReceiptRlnRsn);
          MoveCashReceiptEvent(import.Import1.Item.DetailIncrCashReceiptEvent,
            export.Export1.Update.DetailIncrCashReceiptEvent);
          export.Export1.Update.DetailIncrCashReceipt.Assign(
            import.Import1.Item.DetailIncrCashReceipt);
          MoveCashReceiptBalanceAdjustment(import.Import1.Item.
            DetailIncrCashReceiptBalanceAdjustment,
            export.Export1.Update.DetailIncrCashReceiptBalanceAdjustment);
          MoveCashReceiptEvent(import.Import1.Item.DetailDecrCashReceiptEvent,
            export.Export1.Update.DetailDecrCashReceiptEvent);
          export.Export1.Update.DetailDecrCashReceipt.Assign(
            import.Import1.Item.DetailDecrCashReceipt);
          MoveCashReceiptBalanceAdjustment(import.Import1.Item.
            DetailDecrCashReceiptBalanceAdjustment,
            export.Export1.Update.DetailDecrCashReceiptBalanceAdjustment);
          export.Export1.Next();
        }
      }
      else
      {
        // ---------------------------------------------------------------
        // Clear the screen by not moving the imports to the exports.
        // Populate the screen with the new Cash Receipt entered by
        // the user.
        // ---------------------------------------------------------------
        export.CashReceipt.SequentialNumber =
          import.CashReceipt.SequentialNumber;
        export.HiddenCashReceipt.SequentialNumber = 0;
        local.SearchCriteriaChanged.Flag = "Y";
      }
    }
    else
    {
      // ------------------------------------------------------------------
      // Clear the screen by NOT moving the imports to the exports.
      // Populate the screen with the Cash Receipt passed to this procedure.
      // ------------------------------------------------------------------
      export.CashReceipt.SequentialNumber = import.PassArea.SequentialNumber;
    }

    // ---------------------------------------------------------------------
    // Next Tran logic.
    // ---------------------------------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ----------------------------------------------------------------
    // If the next tran info is not equal to spaces, the user requested
    // a next tran action.  Now validate.
    // ----------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
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
      global.Command = "DISPLAY";

      return;
    }

    // ---------------------------------------------------------------------
    // Security Logic
    // Validate action level security.  If the command is to link to
    // another screen, the security validation will be done in the
    // called procedures.
    // Only the Display command requires security validation.
    // JLK  01/18/99
    // ---------------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ----------------------------------------------------------------
    // Validate the selection characters.
    // ----------------------------------------------------------------
    local.Select.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
      {
        case 'S':
          ++local.Select.Count;

          var field1 = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field1.Error = true;

          if (local.Select.Count == 1)
          {
            export.PassAreaCashReceiptBalanceAdjustment.CreatedTimestamp =
              export.Export1.Item.DetailIncrCashReceiptBalanceAdjustment.
                CreatedTimestamp;
            MoveCashReceiptRlnRsn(export.Export1.Item.DetailCashReceiptRlnRsn,
              export.PassAreaCashReceiptRlnRsn);
            export.PassAreaIncrease.SequentialNumber =
              export.Export1.Item.DetailIncrCashReceipt.SequentialNumber;
            export.PassAreaDecrease.SequentialNumber =
              export.Export1.Item.DetailDecrCashReceipt.SequentialNumber;
          }

          break;
        case ' ':
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field2 = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field2.Error = true;

          break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------------------------------------------------------
    // Main CASE OF command structure.
    // ----------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // --------------------------------------------------------------------
        // PF2  Display
        // Validate that a Cash Receipt has been entered and that it is valid.
        // --------------------------------------------------------------------
        if (export.CashReceipt.SequentialNumber == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }

        // -----------------------------------------------------------
        // Populate the group view.
        // -----------------------------------------------------------
        UseFnDisplayRcptAmtAdjForCr();

        // -----------------------------------------------------------------
        // Display an informational message based on contents of group view.
        // -----------------------------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenCashReceipt.SequentialNumber =
            export.CashReceipt.SequentialNumber;

          if (export.Export1.IsFull)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Protected = false;
              field.Focused = true;

              break;
            }

            ExitState = "FN0000_CR_ADJ_GRP_VIEW_OVERFLOW";
          }
          else if (export.Export1.IsEmpty)
          {
            ExitState = "FN0000_CR_ADJ_SEARCH_CRITERIA_NF";
          }
          else
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Protected = false;
              field.Focused = true;

              break;
            }

            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else if (IsExitState("FN0084_CASH_RCPT_NF"))
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0097_CASH_RCPT_SOURCE_TYPE_NF"))
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0000_CR_SRC_TYPE_NOT_INTERFACE"))
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "LIST":
        // ---------------------------------------------------------------
        // PF4  List
        // ---------------------------------------------------------------
        export.CashRcpt.PromptField = import.CashRcpt.PromptField;

        if (AsChar(import.CashRcpt.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_BALANCE_INTERFACE";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.CashRcpt, "promptField");

          field.Error = true;
        }

        break;
      case "PREV":
        // ---------------------------------------------------------------
        // PF7  Previous
        // ---------------------------------------------------------------
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        // ---------------------------------------------------------------
        // PF8  Next
        // ---------------------------------------------------------------
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        // ---------------------------------------------------------------
        // PF9 Return
        // Make sure the search criteria has not changed.
        // If it has, the user must first press PF2 Display.
        // ---------------------------------------------------------------
        if (export.CashReceipt.SequentialNumber == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }

        if (AsChar(local.SearchCriteriaChanged.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // ---------------------------------------------------------------
        // A selection is NOT required, but only one selection is allowed.
        // ---------------------------------------------------------------
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
        // ---------------------------------------------------------------
        // PF12 Signoff
        // ---------------------------------------------------------------
        UseScCabSignoff();

        break;
      case "LNK_CRIA":
        // ---------------------------------------------------------------
        // PF18 CRIA    Adjust Receipted Interface
        // Make sure the search criteria has not changed.
        // If it has, the user must first press PF2 Display.
        // ---------------------------------------------------------------
        if (export.CashReceipt.SequentialNumber == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }

        if (AsChar(local.SearchCriteriaChanged.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // ---------------------------------------------------------------
        // A selection is required, and only one selection is allowed.
        // ---------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_ADJUST_REC_INTERFACE";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

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

  private static void MoveCashReceiptBalanceAdjustment(
    CashReceiptBalanceAdjustment source, CashReceiptBalanceAdjustment target)
  {
    target.AdjustmentAmount = source.AdjustmentAmount;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SourceCreationDate = source.SourceCreationDate;
  }

  private static void MoveCashReceiptRlnRsn(CashReceiptRlnRsn source,
    CashReceiptRlnRsn target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveExport1(FnDisplayRcptAmtAdjForCr.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.MemberCommon.SelectChar;
    MoveCashReceiptEvent(source.MemberIncrCashReceiptEvent,
      target.DetailIncrCashReceiptEvent);
    target.DetailIncrCashReceipt.Assign(source.MemberIncrCashReceipt);
    MoveCashReceiptBalanceAdjustment(source.
      MemberIncrCashReceiptBalanceAdjustment,
      target.DetailIncrCashReceiptBalanceAdjustment);
    MoveCashReceiptEvent(source.MemberDecrCashReceiptEvent,
      target.DetailDecrCashReceiptEvent);
    target.DetailDecrCashReceipt.Assign(source.MemberDecrCashReceipt);
    MoveCashReceiptBalanceAdjustment(source.
      MemberDecrCashReceiptBalanceAdjustment,
      target.DetailDecrCashReceiptBalanceAdjustment);
    MoveCashReceiptRlnRsn(source.MemberCashReceiptRlnRsn,
      target.DetailCashReceiptRlnRsn);
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

  private void UseFnDisplayRcptAmtAdjForCr()
  {
    var useImport = new FnDisplayRcptAmtAdjForCr.Import();
    var useExport = new FnDisplayRcptAmtAdjForCr.Export();

    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;

    Call(FnDisplayRcptAmtAdjForCr.Execute, useImport, useExport);

    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      export.CashReceiptSourceType);
    export.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
    export.CashReceipt.Assign(useExport.CashReceipt);
    export.NetReceiptAmt.TotalCurrency = useExport.NetReceiptAmt.TotalCurrency;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
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
      /// A value of DetailIncrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailIncrCashReceiptEvent")]
      public CashReceiptEvent DetailIncrCashReceiptEvent
      {
        get => detailIncrCashReceiptEvent ??= new();
        set => detailIncrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailIncrCashReceipt.
      /// </summary>
      [JsonPropertyName("detailIncrCashReceipt")]
      public CashReceipt DetailIncrCashReceipt
      {
        get => detailIncrCashReceipt ??= new();
        set => detailIncrCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailIncrCashReceiptBalanceAdjustment.
      /// </summary>
      [JsonPropertyName("detailIncrCashReceiptBalanceAdjustment")]
      public CashReceiptBalanceAdjustment DetailIncrCashReceiptBalanceAdjustment
      {
        get => detailIncrCashReceiptBalanceAdjustment ??= new();
        set => detailIncrCashReceiptBalanceAdjustment = value;
      }

      /// <summary>
      /// A value of DetailDecrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailDecrCashReceiptEvent")]
      public CashReceiptEvent DetailDecrCashReceiptEvent
      {
        get => detailDecrCashReceiptEvent ??= new();
        set => detailDecrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailDecrCashReceipt.
      /// </summary>
      [JsonPropertyName("detailDecrCashReceipt")]
      public CashReceipt DetailDecrCashReceipt
      {
        get => detailDecrCashReceipt ??= new();
        set => detailDecrCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailDecrCashReceiptBalanceAdjustment.
      /// </summary>
      [JsonPropertyName("detailDecrCashReceiptBalanceAdjustment")]
      public CashReceiptBalanceAdjustment DetailDecrCashReceiptBalanceAdjustment
      {
        get => detailDecrCashReceiptBalanceAdjustment ??= new();
        set => detailDecrCashReceiptBalanceAdjustment = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptRlnRsn.
      /// </summary>
      [JsonPropertyName("detailCashReceiptRlnRsn")]
      public CashReceiptRlnRsn DetailCashReceiptRlnRsn
      {
        get => detailCashReceiptRlnRsn ??= new();
        set => detailCashReceiptRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private CashReceiptEvent detailIncrCashReceiptEvent;
      private CashReceipt detailIncrCashReceipt;
      private CashReceiptBalanceAdjustment detailIncrCashReceiptBalanceAdjustment;
        
      private CashReceiptEvent detailDecrCashReceiptEvent;
      private CashReceipt detailDecrCashReceipt;
      private CashReceiptBalanceAdjustment detailDecrCashReceiptBalanceAdjustment;
        
      private CashReceiptRlnRsn detailCashReceiptRlnRsn;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashRcpt.
    /// </summary>
    [JsonPropertyName("cashRcpt")]
    public Standard CashRcpt
    {
      get => cashRcpt ??= new();
      set => cashRcpt = value;
    }

    /// <summary>
    /// A value of NetReceiptAmt.
    /// </summary>
    [JsonPropertyName("netReceiptAmt")]
    public Common NetReceiptAmt
    {
      get => netReceiptAmt ??= new();
      set => netReceiptAmt = value;
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
    public CashReceipt PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private Standard cashRcpt;
    private Common netReceiptAmt;
    private CashReceipt hiddenCashReceipt;
    private Array<ImportGroup> import1;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CashReceipt passArea;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailIncrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailIncrCashReceiptEvent")]
      public CashReceiptEvent DetailIncrCashReceiptEvent
      {
        get => detailIncrCashReceiptEvent ??= new();
        set => detailIncrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailIncrCashReceipt.
      /// </summary>
      [JsonPropertyName("detailIncrCashReceipt")]
      public CashReceipt DetailIncrCashReceipt
      {
        get => detailIncrCashReceipt ??= new();
        set => detailIncrCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailIncrCashReceiptBalanceAdjustment.
      /// </summary>
      [JsonPropertyName("detailIncrCashReceiptBalanceAdjustment")]
      public CashReceiptBalanceAdjustment DetailIncrCashReceiptBalanceAdjustment
      {
        get => detailIncrCashReceiptBalanceAdjustment ??= new();
        set => detailIncrCashReceiptBalanceAdjustment = value;
      }

      /// <summary>
      /// A value of DetailDecrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailDecrCashReceiptEvent")]
      public CashReceiptEvent DetailDecrCashReceiptEvent
      {
        get => detailDecrCashReceiptEvent ??= new();
        set => detailDecrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailDecrCashReceipt.
      /// </summary>
      [JsonPropertyName("detailDecrCashReceipt")]
      public CashReceipt DetailDecrCashReceipt
      {
        get => detailDecrCashReceipt ??= new();
        set => detailDecrCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailDecrCashReceiptBalanceAdjustment.
      /// </summary>
      [JsonPropertyName("detailDecrCashReceiptBalanceAdjustment")]
      public CashReceiptBalanceAdjustment DetailDecrCashReceiptBalanceAdjustment
      {
        get => detailDecrCashReceiptBalanceAdjustment ??= new();
        set => detailDecrCashReceiptBalanceAdjustment = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptRlnRsn.
      /// </summary>
      [JsonPropertyName("detailCashReceiptRlnRsn")]
      public CashReceiptRlnRsn DetailCashReceiptRlnRsn
      {
        get => detailCashReceiptRlnRsn ??= new();
        set => detailCashReceiptRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private CashReceiptEvent detailIncrCashReceiptEvent;
      private CashReceipt detailIncrCashReceipt;
      private CashReceiptBalanceAdjustment detailIncrCashReceiptBalanceAdjustment;
        
      private CashReceiptEvent detailDecrCashReceiptEvent;
      private CashReceipt detailDecrCashReceipt;
      private CashReceiptBalanceAdjustment detailDecrCashReceiptBalanceAdjustment;
        
      private CashReceiptRlnRsn detailCashReceiptRlnRsn;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashRcpt.
    /// </summary>
    [JsonPropertyName("cashRcpt")]
    public Standard CashRcpt
    {
      get => cashRcpt ??= new();
      set => cashRcpt = value;
    }

    /// <summary>
    /// A value of NetReceiptAmt.
    /// </summary>
    [JsonPropertyName("netReceiptAmt")]
    public Common NetReceiptAmt
    {
      get => netReceiptAmt ??= new();
      set => netReceiptAmt = value;
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
    /// A value of PassAreaCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("passAreaCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment PassAreaCashReceiptBalanceAdjustment
    {
      get => passAreaCashReceiptBalanceAdjustment ??= new();
      set => passAreaCashReceiptBalanceAdjustment = value;
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

    /// <summary>
    /// A value of PassAreaIncrease.
    /// </summary>
    [JsonPropertyName("passAreaIncrease")]
    public CashReceipt PassAreaIncrease
    {
      get => passAreaIncrease ??= new();
      set => passAreaIncrease = value;
    }

    /// <summary>
    /// A value of PassAreaDecrease.
    /// </summary>
    [JsonPropertyName("passAreaDecrease")]
    public CashReceipt PassAreaDecrease
    {
      get => passAreaDecrease ??= new();
      set => passAreaDecrease = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private Standard cashRcpt;
    private Common netReceiptAmt;
    private CashReceipt hiddenCashReceipt;
    private Array<ExportGroup> export1;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CashReceiptBalanceAdjustment passAreaCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn passAreaCashReceiptRlnRsn;
    private CashReceipt passAreaIncrease;
    private CashReceipt passAreaDecrease;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SearchCriteriaChanged.
    /// </summary>
    [JsonPropertyName("searchCriteriaChanged")]
    public Common SearchCriteriaChanged
    {
      get => searchCriteriaChanged ??= new();
      set => searchCriteriaChanged = value;
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

    private Common searchCriteriaChanged;
    private Common select;
  }
#endregion
}

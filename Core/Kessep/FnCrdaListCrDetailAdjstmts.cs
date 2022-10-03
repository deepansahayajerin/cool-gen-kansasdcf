// Program: FN_CRDA_LIST_CR_DETAIL_ADJSTMTS, ID: 372341132, model: 746.
// Short name: SWECRDAP
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
/// A program: FN_CRDA_LIST_CR_DETAIL_ADJSTMTS.
/// </para>
/// <para>
/// This procedure lists all of the Cash Receipt Balance Adjustments for a given
/// Cash Receipt.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrdaListCrDetailAdjstmts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRDA_LIST_CR_DETAIL_ADJSTMTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrdaListCrDetailAdjstmts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrdaListCrDetailAdjstmts.
  /// </summary>
  public FnCrdaListCrDetailAdjstmts(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // Procedure:  List Cash Receipt Detail Adjustments
    // ------------------------------------------------------------------------
    // Every initial development and change to that development needs to
    // be documented below.
    // ------------------------------------------------------------------------
    // Date 	  Developer Name	Description
    // ------------------------------------------------------------------------
    // 03/08/99  J. Katz		Copied CRAJ screen and modified the
    // 				logic to list cash receipt detail
    // 				balance adjustments.
    // 05/26/99  J. Katz		Added list prompt to flow to CLBR and
    // 				display list of detail balance
    // 				adjustment reasons.
    // ------------------------------------------------------------------------
    // 11/18/99   P. Phinney      H00080606    Views being lost on PF8 PF9
    // ------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
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
          export.Export1.Update.MbrCommon.SelectChar =
            import.Import1.Item.MbrCommon.SelectChar;
          MoveCashReceiptDetailRlnRsn(import.Import1.Item.
            MbrCashReceiptDetailRlnRsn,
            export.Export1.Update.MbrCashReceiptDetailRlnRsn);
          export.Export1.Update.MbrAdjustedCrdCrComboNo.CrdCrCombo =
            import.Import1.Item.MbrAdjustedCrdCrComboNo.CrdCrCombo;
          MoveCashReceiptEvent(import.Import1.Item.MbrAdjustedCashReceiptEvent,
            export.Export1.Update.MbrAdjustedCashReceiptEvent);
          export.Export1.Update.MbrAdjustedCashReceipt.SequentialNumber =
            import.Import1.Item.MbrAdjustedCashReceipt.SequentialNumber;
          export.Export1.Update.MbrAdjustedCashReceiptDetail.Assign(
            import.Import1.Item.MbrAdjustedCashReceiptDetail);
          export.Export1.Update.MbrAdjustingCrdCrComboNo.CrdCrCombo =
            import.Import1.Item.MbrAdjustingCrdCrComboNo.CrdCrCombo;
          MoveCashReceiptEvent(import.Import1.Item.MbrAdjustingCashReceiptEvent,
            export.Export1.Update.MbrAdjustingCashReceiptEvent);
          export.Export1.Update.MbrAdjustingCashReceipt.SequentialNumber =
            import.Import1.Item.MbrAdjustingCashReceipt.SequentialNumber;

          // 11/18/99   P. Phinney      H00080606    Views being lost on PF8 PF9
          // Export was being moved to export
          // 
          // changed to move import to export
          export.Export1.Update.MbrAdjustingCashReceiptDetail.Assign(
            import.Import1.Item.MbrAdjustingCashReceiptDetail);
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
      switch(AsChar(export.Export1.Item.MbrCommon.SelectChar))
      {
        case 'S':
          ++local.Select.Count;

          var field1 = GetField(export.Export1.Item.MbrCommon, "selectChar");

          field1.Error = true;

          if (local.Select.Count == 1)
          {
            export.PassCashReceiptEvent.SystemGeneratedIdentifier =
              export.Export1.Item.MbrAdjustedCashReceiptEvent.
                SystemGeneratedIdentifier;
            export.PassCashReceipt.SequentialNumber =
              export.Export1.Item.MbrAdjustedCashReceipt.SequentialNumber;
            export.PassCashReceiptDetail.SequentialIdentifier =
              export.Export1.Item.MbrAdjustedCashReceiptDetail.
                SequentialIdentifier;
          }

          break;
        case ' ':
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field2 = GetField(export.Export1.Item.MbrCommon, "selectChar");

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
        UseFnDisplayCrDetailAdj();

        // -----------------------------------------------------------------
        // Display an informational message based on contents of group view.
        // -----------------------------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenCashReceipt.SequentialNumber =
            export.CashReceipt.SequentialNumber;

          if (local.CseDtlAdj.Count < export
            .CashReceipt.TotalDetailAdjustmentCount.GetValueOrDefault())
          {
            ExitState = "FN0000_ADJ_COUNT_GT_DTL_DISPLAY";
          }
          else if (local.CseDtlAdj.Count == export
            .CashReceipt.TotalDetailAdjustmentCount.GetValueOrDefault())
          {
            if (export.Export1.IsFull)
            {
              ExitState = "FN0000_CR_ADJ_GRP_VIEW_OVERFLOW";
            }
            else if (export.Export1.IsEmpty)
            {
              ExitState = "FN0000_CR_ADJ_SEARCH_CRITERIA_NF";
            }
            else
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
          else
          {
            ExitState = "FN0000_INVALID_ADJ_COUNT";
          }

          if (!export.Export1.IsEmpty)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              var field = GetField(export.Export1.Item.MbrCommon, "selectChar");

              field.Protected = false;
              field.Focused = true;

              break;
            }
          }
        }
        else if (IsExitState("FN0084_CASH_RCPT_NF"))
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0097_CASH_RCPT_SOURCE_TYPE_NF"))
        {
          var field1 = GetField(export.CashReceiptSourceType, "code");

          field1.Error = true;

          var field2 = GetField(export.CashReceipt, "sequentialNumber");

          field2.Error = true;
        }
        else if (IsExitState("FN0000_INTF_SOURCE_CD_REQ"))
        {
          var field1 = GetField(export.CashReceiptSourceType, "code");

          field1.Error = true;

          var field2 = GetField(export.CashReceipt, "sequentialNumber");

          field2.Error = true;
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
          MoveCashReceiptSourceType(export.CashReceiptSourceType,
            export.PassCashReceiptSourceType);
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
        if (export.CashReceipt.SequentialNumber != export
          .HiddenCashReceipt.SequentialNumber)
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

        MoveCashReceiptSourceType(export.CashReceiptSourceType,
          export.PassCashReceiptSourceType);

        break;
      case "SIGNOFF":
        // ---------------------------------------------------------------
        // PF12 Signoff
        // ---------------------------------------------------------------
        UseScCabSignoff();

        break;
      case "CRRC":
        // ---------------------------------------------------------------
        // PF17 CRRC
        // Display Record Cash Receipt Collection Details screen.
        // A single selection from the list is required.
        // ---------------------------------------------------------------
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            if (export.PassCashReceipt.SequentialNumber > 0)
            {
              if (!ReadCashReceipt())
              {
                ExitState = "FN0000_CASH_RECEIPT_NF";

                return;
              }

              if (!ReadCashReceiptSourceType())
              {
                ExitState = "FN0000_CASH_RCPT_SOURCE_TYPE_NF";

                return;
              }

              if (!ReadCashReceiptType())
              {
                ExitState = "FN0000_CASH_RECEIPT_TYPE_NF";

                return;
              }

              MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                export.PassCashReceiptSourceType);
              export.PassCashReceiptType.SystemGeneratedIdentifier =
                entities.CashReceiptType.SystemGeneratedIdentifier;
              ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";
            }
            else
            {
              ExitState = "FN0000_EXPORT_PASS_CR_NF";
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "CLDR":
        // ---------------------------------------------------------------
        // PF18  Adj Reasons
        // Display CLDR - List Cash Receipt Detail Balance Reasons screen.
        // CLDR provides the user with a list of the NAMES that apply to
        // the Adjustment Reason Codes used on CRDA.  JLK  05/26/99
        // ---------------------------------------------------------------
        ExitState = "ECO_LNK_TO_CLDR";

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

  private static void MoveCashReceiptDetailRlnRsn(
    CashReceiptDetailRlnRsn source, CashReceiptDetailRlnRsn target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
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

  private static void MoveExport1(FnDisplayCrDetailAdj.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.MbrCommon.SelectChar = source.MbrCommon.SelectChar;
    MoveCashReceiptDetailRlnRsn(source.MbrCashReceiptDetailRlnRsn,
      target.MbrCashReceiptDetailRlnRsn);
    target.MbrAdjustedCrdCrComboNo.CrdCrCombo =
      source.MbrAdjustedCrdCrComboNo.CrdCrCombo;
    MoveCashReceiptEvent(source.MbrAdjustedCashReceiptEvent,
      target.MbrAdjustedCashReceiptEvent);
    target.MbrAdjustedCashReceipt.SequentialNumber =
      source.MbrAdjustedCashReceipt.SequentialNumber;
    target.MbrAdjustedCashReceiptDetail.Assign(
      source.MbrAdjustedCashReceiptDetail);
    target.MbrAdjustingCrdCrComboNo.CrdCrCombo =
      source.MbrAdjustingCrdCrComboNo.CrdCrCombo;
    MoveCashReceiptEvent(source.MbrAdjustingCashReceiptEvent,
      target.MbrAdjustingCashReceiptEvent);
    target.MbrAdjustingCashReceipt.SequentialNumber =
      source.MbrAdjustingCashReceipt.SequentialNumber;
    target.MbrAdjustingCashReceiptDetail.Assign(
      source.MbrAdjustingCashReceiptDetail);
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

  private void UseFnDisplayCrDetailAdj()
  {
    var useImport = new FnDisplayCrDetailAdj.Import();
    var useExport = new FnDisplayCrDetailAdj.Export();

    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;

    Call(FnDisplayCrDetailAdj.Execute, useImport, useExport);

    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      export.CashReceiptSourceType);
    export.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
    export.CashReceipt.Assign(useExport.CashReceipt);
    export.NetReceiptAmt.TotalCurrency = useExport.NetRcptAmt.TotalCurrency;
    local.CseDtlAdj.Count = useExport.CseDtlAdj.Count;
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

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", export.PassCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetInt32(command, "crSrceTypeId", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
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
      /// A value of MbrCommon.
      /// </summary>
      [JsonPropertyName("mbrCommon")]
      public Common MbrCommon
      {
        get => mbrCommon ??= new();
        set => mbrCommon = value;
      }

      /// <summary>
      /// A value of MbrCashReceiptDetailRlnRsn.
      /// </summary>
      [JsonPropertyName("mbrCashReceiptDetailRlnRsn")]
      public CashReceiptDetailRlnRsn MbrCashReceiptDetailRlnRsn
      {
        get => mbrCashReceiptDetailRlnRsn ??= new();
        set => mbrCashReceiptDetailRlnRsn = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCrdCrComboNo")]
      public CrdCrComboNo MbrAdjustedCrdCrComboNo
      {
        get => mbrAdjustedCrdCrComboNo ??= new();
        set => mbrAdjustedCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceiptEvent")]
      public CashReceiptEvent MbrAdjustedCashReceiptEvent
      {
        get => mbrAdjustedCashReceiptEvent ??= new();
        set => mbrAdjustedCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceipt")]
      public CashReceipt MbrAdjustedCashReceipt
      {
        get => mbrAdjustedCashReceipt ??= new();
        set => mbrAdjustedCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceiptDetail")]
      public CashReceiptDetail MbrAdjustedCashReceiptDetail
      {
        get => mbrAdjustedCashReceiptDetail ??= new();
        set => mbrAdjustedCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCrdCrComboNo")]
      public CrdCrComboNo MbrAdjustingCrdCrComboNo
      {
        get => mbrAdjustingCrdCrComboNo ??= new();
        set => mbrAdjustingCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceiptEvent")]
      public CashReceiptEvent MbrAdjustingCashReceiptEvent
      {
        get => mbrAdjustingCashReceiptEvent ??= new();
        set => mbrAdjustingCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceipt")]
      public CashReceipt MbrAdjustingCashReceipt
      {
        get => mbrAdjustingCashReceipt ??= new();
        set => mbrAdjustingCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceiptDetail")]
      public CashReceiptDetail MbrAdjustingCashReceiptDetail
      {
        get => mbrAdjustingCashReceiptDetail ??= new();
        set => mbrAdjustingCashReceiptDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common mbrCommon;
      private CashReceiptDetailRlnRsn mbrCashReceiptDetailRlnRsn;
      private CrdCrComboNo mbrAdjustedCrdCrComboNo;
      private CashReceiptEvent mbrAdjustedCashReceiptEvent;
      private CashReceipt mbrAdjustedCashReceipt;
      private CashReceiptDetail mbrAdjustedCashReceiptDetail;
      private CrdCrComboNo mbrAdjustingCrdCrComboNo;
      private CashReceiptEvent mbrAdjustingCashReceiptEvent;
      private CashReceipt mbrAdjustingCashReceipt;
      private CashReceiptDetail mbrAdjustingCashReceiptDetail;
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
      /// A value of MbrCommon.
      /// </summary>
      [JsonPropertyName("mbrCommon")]
      public Common MbrCommon
      {
        get => mbrCommon ??= new();
        set => mbrCommon = value;
      }

      /// <summary>
      /// A value of MbrCashReceiptDetailRlnRsn.
      /// </summary>
      [JsonPropertyName("mbrCashReceiptDetailRlnRsn")]
      public CashReceiptDetailRlnRsn MbrCashReceiptDetailRlnRsn
      {
        get => mbrCashReceiptDetailRlnRsn ??= new();
        set => mbrCashReceiptDetailRlnRsn = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCrdCrComboNo")]
      public CrdCrComboNo MbrAdjustedCrdCrComboNo
      {
        get => mbrAdjustedCrdCrComboNo ??= new();
        set => mbrAdjustedCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceiptEvent")]
      public CashReceiptEvent MbrAdjustedCashReceiptEvent
      {
        get => mbrAdjustedCashReceiptEvent ??= new();
        set => mbrAdjustedCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceipt")]
      public CashReceipt MbrAdjustedCashReceipt
      {
        get => mbrAdjustedCashReceipt ??= new();
        set => mbrAdjustedCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceiptDetail")]
      public CashReceiptDetail MbrAdjustedCashReceiptDetail
      {
        get => mbrAdjustedCashReceiptDetail ??= new();
        set => mbrAdjustedCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCrdCrComboNo")]
      public CrdCrComboNo MbrAdjustingCrdCrComboNo
      {
        get => mbrAdjustingCrdCrComboNo ??= new();
        set => mbrAdjustingCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceiptEvent")]
      public CashReceiptEvent MbrAdjustingCashReceiptEvent
      {
        get => mbrAdjustingCashReceiptEvent ??= new();
        set => mbrAdjustingCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceipt")]
      public CashReceipt MbrAdjustingCashReceipt
      {
        get => mbrAdjustingCashReceipt ??= new();
        set => mbrAdjustingCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceiptDetail")]
      public CashReceiptDetail MbrAdjustingCashReceiptDetail
      {
        get => mbrAdjustingCashReceiptDetail ??= new();
        set => mbrAdjustingCashReceiptDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common mbrCommon;
      private CashReceiptDetailRlnRsn mbrCashReceiptDetailRlnRsn;
      private CrdCrComboNo mbrAdjustedCrdCrComboNo;
      private CashReceiptEvent mbrAdjustedCashReceiptEvent;
      private CashReceipt mbrAdjustedCashReceipt;
      private CashReceiptDetail mbrAdjustedCashReceiptDetail;
      private CrdCrComboNo mbrAdjustingCrdCrComboNo;
      private CashReceiptEvent mbrAdjustingCashReceiptEvent;
      private CashReceipt mbrAdjustingCashReceipt;
      private CashReceiptDetail mbrAdjustingCashReceiptDetail;
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
    /// A value of PassCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("passCashReceiptSourceType")]
    public CashReceiptSourceType PassCashReceiptSourceType
    {
      get => passCashReceiptSourceType ??= new();
      set => passCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PassCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("passCashReceiptEvent")]
    public CashReceiptEvent PassCashReceiptEvent
    {
      get => passCashReceiptEvent ??= new();
      set => passCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of PassCashReceiptType.
    /// </summary>
    [JsonPropertyName("passCashReceiptType")]
    public CashReceiptType PassCashReceiptType
    {
      get => passCashReceiptType ??= new();
      set => passCashReceiptType = value;
    }

    /// <summary>
    /// A value of PassCashReceipt.
    /// </summary>
    [JsonPropertyName("passCashReceipt")]
    public CashReceipt PassCashReceipt
    {
      get => passCashReceipt ??= new();
      set => passCashReceipt = value;
    }

    /// <summary>
    /// A value of PassCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("passCashReceiptDetail")]
    public CashReceiptDetail PassCashReceiptDetail
    {
      get => passCashReceiptDetail ??= new();
      set => passCashReceiptDetail = value;
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
    private CashReceiptSourceType passCashReceiptSourceType;
    private CashReceiptEvent passCashReceiptEvent;
    private CashReceiptType passCashReceiptType;
    private CashReceipt passCashReceipt;
    private CashReceiptDetail passCashReceiptDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CseDtlAdj.
    /// </summary>
    [JsonPropertyName("cseDtlAdj")]
    public Common CseDtlAdj
    {
      get => cseDtlAdj ??= new();
      set => cseDtlAdj = value;
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
    /// A value of ListPrompt.
    /// </summary>
    [JsonPropertyName("listPrompt")]
    public Common ListPrompt
    {
      get => listPrompt ??= new();
      set => listPrompt = value;
    }

    private Common cseDtlAdj;
    private Common select;
    private Common listPrompt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
  }
#endregion
}

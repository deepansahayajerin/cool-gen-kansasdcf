// Program: FN_CRDE_DELETE_CASH_RECEIPT, ID: 371721462, model: 746.
// Short name: SWECRDEP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CRDE_DELETE_CASH_RECEIPT.
/// </para>
/// <para>
/// Resp: Finance	
/// This Procedure will allow a Cash Receipt to be deleted from the system.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrdeDeleteCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRDE_DELETE_CASH_RECEIPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrdeDeleteCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrdeDeleteCashReceipt.
  /// </summary>
  public FnCrdeDeleteCashReceipt(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // Date 	  Developer Name	Description
    // 02/12/96  Holly Kennedy-MTW 	Retrofits
    // 04/03/96  Holly Kennedy-MTW 	When setting the Delete Status
    // 				History the current status
    // 				was not being discontinued.
    // 04/03/96  Holly Kennedy-MTW	When setting the Status to delete,
    // 				the discontinue date on the
    // 				Stat History was not being
    // 				set to max.
    // 12/13/96  R. Marchman		Add new security/next tran
    // 01/14/97  H. Kennedy-MTW	Added logic to check for the command
    // 				bypass before checking security.
    // 				This Command should not be secured.
    // 02/27/97  T.O.Redmond 		Removed PF15 Key
    // 				Removed Bypass Logic
    // 09/17/98  J. Katz - SRG		Removed PF11 Clear logic.
    // 				Add validation logic for non-cash
    // 				receipts.
    // 				Protect entry and prompt fields
    // 				if active status is DEL.
    // 				Added logic to support BYPASS
    // 				command that is returned from CRDR
    // 				link.
    // 				Removed unused link from REIP to
    // 				CRDE.
    // 				Removed logic for command
    // 				XXFMMENU which does not
    // 				apply to this procedure.
    // 11/07/98  J. Katz - SRG		Add Undelete logic.
    // 06/07/99  J. Katz - SRG		Analyzed READ statements and
    // 				changed read property to Select
    // 				Only where appropriate.
    // 06/24/99  J. Katz - SRG		Modified protection logic to ensure
    // 				that protected fields stay protected
    // 				when an incorrect function key or
    // 				the enter key is pressed in error.
    // ----------------------------------------------------------------
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // *** Move all IMPORTs to EXPORTs. ***
    MoveCashReceiptSourceType(import.CashReceiptSourceType,
      export.CashReceiptSourceType);
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceipt.Assign(import.CashReceipt);
    export.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    MoveCashReceiptDeleteReason(import.CashReceiptDeleteReason,
      export.CashReceiptDeleteReason);
    export.Prompt.SelectChar = import.Prompt.SelectChar;
    export.CashReceiptStatusHistory.Assign(import.CashReceiptStatusHistory);
    MoveCashReceiptDeleteReason(import.HiddenCashReceiptDeleteReason,
      export.HiddenCashReceiptDeleteReason);

    // *** Next Tran Logic ***
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
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
      if (import.CashReceipt.SequentialNumber < 1)
      {
        ExitState = "FN0000_CASH_RCPT_SELECT_MISSING";

        return;
      }

      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    // ---------------------------------------------------------------
    // Logic for Command XXFMMENU was removed.  This command is
    // generally set on the flow from the CAMM Menu to a procedure.
    // This command is not valid for CRDE since there is no direct
    // flow from CAMM.     JLK  09/18/98
    // ---------------------------------------------------------------
    // ************************************************
    // *Validate action level security                *
    // ************************************************
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UNDELETE"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------------------------------------------------
    // Set up local views/             JLK   12/19/98
    // ----------------------------------------------------------
    UseFnHardcodedCashReceipting();

    // ----------------------------------------------------------
    // Protect Delete Reason Code, Delete Reason List Prompt,
    // and Delete Note if receipt has already been deleted.
    // JLK   06/24/99
    // ----------------------------------------------------------
    if (IsEmpty(export.CashReceiptStatusHistory.CreatedBy))
    {
      // ---------------------------------------------------------
      // Active Cash Receipt Status is not DEL.
      // Allow data entry on Delete Reason Code and Delete Note.
      // ---------------------------------------------------------
      var field1 = GetField(export.CashReceiptDeleteReason, "code");

      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.Prompt, "selectChar");

      field2.Protected = false;

      var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

      field3.Protected = false;
    }
    else
    {
      // ---------------------------------------------------
      // Active Cash Receipt Status is DEL.
      // Do not allow Delete Reason Code and Delete Note
      // to be changed.
      // ---------------------------------------------------
      var field1 = GetField(export.CashReceiptDeleteReason, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Prompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

      field3.Color = "cyan";
      field3.Protected = true;
    }

    // ----------------------------------------------------------
    // Main Case of Command
    // ----------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "LIST":
        // ----------------------------------------------------------
        //             DATA VALIDATION EDITS
        // The Cash Receipt Identifier must be passed to CRDE to
        // perform any action.        JLK  09/18/98
        // ----------------------------------------------------------
        if (import.CashReceiptSourceType.SystemGeneratedIdentifier == 0 || import
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || import
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_CASH_RCPT_SELECT_MISSING";

          return;
        }

        // ----------------------------------------------------------
        // Validate that the list prompt is Selected.
        // Set up flow to CRDR.
        // ----------------------------------------------------------
        if (!IsEmpty(import.Prompt.SelectChar))
        {
          if (AsChar(import.Prompt.SelectChar) == 'S')
          {
            MoveCashReceiptDeleteReason(import.CashReceiptDeleteReason,
              export.HiddenCashReceiptDeleteReason);
            ExitState = "LIST_CASH_RECEIPT_DELETE";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case "BYPASS":
        // ----------------------------------------------------------
        // If no Delete Reason Code was selected from CRDR,
        // reset the reason code value to the held prior to the
        // flow.             JLK   09/18/98
        // ----------------------------------------------------------
        if (IsEmpty(import.CashReceiptDeleteReason.Code))
        {
          MoveCashReceiptDeleteReason(export.HiddenCashReceiptDeleteReason,
            export.CashReceiptDeleteReason);
        }

        var field = GetField(export.CashReceiptStatusHistory, "reasonText");

        field.Protected = false;
        field.Focused = true;

        export.Prompt.SelectChar = "";

        break;
      case "DISPLAY":
        // ----------------------------------------------------------
        //             DATA VALIDATION EDITS
        // The Cash Receipt Identifier must be passed to CRDE to
        // perform any action.
        // ----------------------------------------------------------
        if (import.CashReceiptSourceType.SystemGeneratedIdentifier == 0 || import
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || import
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_CASH_RCPT_SELECT_MISSING";

          return;
        }

        // ----------------------------------------------------------
        // Determine if Cash Receipt is in DEL status.
        // If it is, retrieve the Delete Reason Code and Delete Note.
        // JLK    09/18/98
        // ----------------------------------------------------------
        UseReadCashReceiptDeleteReason();

        // ------------------------------------------------------------
        // If the Cash Receipt is in DEL status, do not allow the
        // Delete Reason Code and Delete Note to be changed.
        // ------------------------------------------------------------
        if (IsEmpty(export.CashReceiptDeleteReason.Code))
        {
          // ---------------------------------------------------------
          // Active Cash Receipt Status is not DEL.
          // Allow data entry on Delete Reason Code and Delete Note.
          // ---------------------------------------------------------
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Protected = false;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Protected = false;
        }
        else
        {
          // ---------------------------------------------------
          // Active Cash Receipt Status is DEL.
          // Do not allow Delete Reason Code and Delete Note
          // to be changed.
          // ---------------------------------------------------
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "DELETE":
        // -----------------------------------------------------------
        //                    DATA VALIDATION SECTION
        // Cannot delete payment history records on CRDE.  JLK 12/18/98
        // -----------------------------------------------------------
        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
        {
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Color = "cyan";
          field3.Protected = true;

          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          return;
        }

        // -----------------------------------------------------------
        // If the Cash Receipt identifier, the Delete Reason Code, or
        // the Delete Note are zero or spaces, the DELETE action
        // cannot be processed.         JLK  09/18/98
        // -----------------------------------------------------------
        if (import.CashReceiptSourceType.SystemGeneratedIdentifier == 0 || import
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || import
          .CashReceiptType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_CASH_RCPT_SELECT_MISSING";

          return;
        }

        if (IsEmpty(import.CashReceiptStatusHistory.ReasonText))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field1 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field1.Error = true;
        }

        if (IsEmpty(import.CashReceiptDeleteReason.Code))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -----------------------------------------------
        // All mandatory data are available.  Continue.
        // -----------------------------------------------
        UseFnDeleteCashRcpt();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Color = "cyan";
          field3.Protected = true;

          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else if (IsExitState("FN0000_CASH_RECEIPT_ALREADY_DEL"))
        {
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Color = "cyan";
          field3.Protected = true;
        }
        else if (IsExitState("FN0035_CASH_RCPT_DEL_RSN_NF"))
        {
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Error = true;
        }

        break;
      case "UNDELETE":
        // -------------------------------------------------------------
        // PF15 Undelete
        // Restore a Cash Receipt back to its active status prior to
        // the Delete action.  To perform this action, the Cash Receipt
        // must have an active status of Delete.  The active status
        // prior to Delete should be either REC or INTF.
        // A deleted Cash Receipt can only be Undeleted on the same
        // day that it was deleted.    JLK  11/07/98
        // -------------------------------------------------------------
        if (export.CashReceipt.SequentialNumber == 0)
        {
          ExitState = "FN0000_CASH_RCPT_SELECT_MISSING";

          return;
        }

        if (IsEmpty(export.CashReceiptDeleteReason.Code))
        {
          ExitState = "FN0000_CASH_RCPT_NOT_DELETED";

          return;
        }

        if (export.CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFcrtRec.SystemGeneratedIdentifier || export
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
        {
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Color = "cyan";
          field3.Protected = true;

          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          return;
        }

        if (!Equal(Date(export.CashReceiptStatusHistory.CreatedTimestamp),
          Now().Date))
        {
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Color = "cyan";
          field3.Protected = true;

          ExitState = "FN0000_UNDELETE_NOT_ALLOWED";

          return;
        }

        UseFnCabUndeleteCashReceipt();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CashReceiptStatusHistory.Assign(
            local.ClearCashReceiptStatusHistory);
          MoveCashReceiptDeleteReason(local.ClearCashReceiptDeleteReason,
            export.CashReceiptDeleteReason);
          MoveCashReceiptDeleteReason(local.ClearCashReceiptDeleteReason,
            export.HiddenCashReceiptDeleteReason);
          ExitState = "FN0000_UNDELETE_SUCCESSFUL";
        }
        else if (!IsEmpty(export.CashReceiptDeleteReason.Code))
        {
          var field1 = GetField(export.CashReceiptDeleteReason, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Prompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CashReceiptStatusHistory, "reasonText");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCashReceiptDeleteReason(
    CashReceiptDeleteReason source, CashReceiptDeleteReason target)
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

  private void UseFnCabUndeleteCashReceipt()
  {
    var useImport = new FnCabUndeleteCashReceipt.Import();
    var useExport = new FnCabUndeleteCashReceipt.Export();

    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;

    Call(FnCabUndeleteCashReceipt.Execute, useImport, useExport);
  }

  private void UseFnDeleteCashRcpt()
  {
    var useImport = new FnDeleteCashRcpt.Import();
    var useExport = new FnDeleteCashRcpt.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDeleteReason.Code =
      import.CashReceiptDeleteReason.Code;
    useImport.CashReceiptStatusHistory.ReasonText =
      import.CashReceiptStatusHistory.ReasonText;

    Call(FnDeleteCashRcpt.Execute, useImport, useExport);

    export.CashReceiptStatusHistory.Assign(useExport.CashReceiptStatusHistory);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedCrtFcrtRec.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
  }

  private void UseReadCashReceiptDeleteReason()
  {
    var useImport = new ReadCashReceiptDeleteReason.Import();
    var useExport = new ReadCashReceiptDeleteReason.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;

    Call(ReadCashReceiptDeleteReason.Execute, useImport, useExport);

    MoveCashReceiptDeleteReason(useExport.CashReceiptDeleteReason,
      export.CashReceiptDeleteReason);
    export.CashReceiptStatusHistory.Assign(useExport.CashReceiptStatusHistory);
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("cashReceiptDeleteReason")]
    public CashReceiptDeleteReason CashReceiptDeleteReason
    {
      get => cashReceiptDeleteReason ??= new();
      set => cashReceiptDeleteReason = value;
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
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDeleteReason")]
    public CashReceiptDeleteReason HiddenCashReceiptDeleteReason
    {
      get => hiddenCashReceiptDeleteReason ??= new();
      set => hiddenCashReceiptDeleteReason = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
    private Common prompt;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptDeleteReason hiddenCashReceiptDeleteReason;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("cashReceiptDeleteReason")]
    public CashReceiptDeleteReason CashReceiptDeleteReason
    {
      get => cashReceiptDeleteReason ??= new();
      set => cashReceiptDeleteReason = value;
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
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptDeleteReason")]
    public CashReceiptDeleteReason HiddenCashReceiptDeleteReason
    {
      get => hiddenCashReceiptDeleteReason ??= new();
      set => hiddenCashReceiptDeleteReason = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
    private Common prompt;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptDeleteReason hiddenCashReceiptDeleteReason;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ClearCashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("clearCashReceiptDeleteReason")]
    public CashReceiptDeleteReason ClearCashReceiptDeleteReason
    {
      get => clearCashReceiptDeleteReason ??= new();
      set => clearCashReceiptDeleteReason = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("clearCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ClearCashReceiptStatusHistory
    {
      get => clearCashReceiptStatusHistory ??= new();
      set => clearCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFdirPmt")]
    public CashReceiptType HardcodedCrtFdirPmt
    {
      get => hardcodedCrtFdirPmt ??= new();
      set => hardcodedCrtFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFcrtRec.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFcrtRec")]
    public CashReceiptType HardcodedCrtFcrtRec
    {
      get => hardcodedCrtFcrtRec ??= new();
      set => hardcodedCrtFcrtRec = value;
    }

    private CashReceiptDeleteReason clearCashReceiptDeleteReason;
    private CashReceiptStatusHistory clearCashReceiptStatusHistory;
    private CashReceiptType hardcodedCrtFdirPmt;
    private CashReceiptType hardcodedCrtFcrtRec;
  }
#endregion
}

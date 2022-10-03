// Program: FN_CMBR_MTN_CASH_RCPT_BAL_RSNS, ID: 373529623, model: 746.
// Short name: SWECMBRP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CMBR_MTN_CASH_RCPT_BAL_RSNS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCmbrMtnCashRcptBalRsns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CMBR_MTN_CASH_RCPT_BAL_RSNS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCmbrMtnCashRcptBalRsns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCmbrMtnCashRcptBalRsns.
  /// </summary>
  public FnCmbrMtnCashRcptBalRsns(IContext context, Import import, Export export)
    :
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
    // Set initial EXIT STATE.
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ------------------------------------------------------------
    // Set up local views.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // ------------------------------------------------------------
    // Handle clear screen action.
    // ------------------------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // ------------------------------------------------------------
    // Move all IMPORTs to EXPORTs.
    // ------------------------------------------------------------
    export.Prompt.Text1 = import.Prompt.Text1;
    export.CashReceiptRlnRsn.Assign(import.CashReceiptRlnRsn);
    export.HiddenCashReceiptRlnRsn.Assign(import.HiddenCashReceiptRlnRsn);
    export.TypeStatusAudit.Assign(import.TypeStatusAudit);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ------------------------------------------------------------
    // NEXT TRAN LOGIC
    // CMBR is being accessed via nexttran action.
    // ------------------------------------------------------------
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    // ------------------------------------------------------------
    // NEXT TRAN LOGIC
    // Note if the next tran info is not equal to spaces, the user
    // requested a next tran action.  Now validate.
    // ------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // Set the local next_tran_info attributes to the import view
      // attributes for the data to be passed to the next transaction.
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      global.Command = "";
    }

    // ------------------------------------------------------------
    // SECURITY LOGIC
    // If the command is Add, Display, Update, or Delete
    // check security for current user id.
    // ------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        return;
      }
    }

    // ------------------------------------------------------------
    // A record cannot be UPDATEd or DELETEd without first being
    // displayed.
    // ------------------------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (export.HiddenCashReceiptRlnRsn.SystemGeneratedIdentifier == 0 || export
        .CashReceiptRlnRsn.SystemGeneratedIdentifier != export
        .HiddenCashReceiptRlnRsn.SystemGeneratedIdentifier)
      {
        export.CashReceiptRlnRsn.Assign(local.Clear);
        export.HiddenCashReceiptRlnRsn.Assign(local.Clear);
        export.CashReceiptRlnRsn.Code = import.CashReceiptRlnRsn.Code;
        ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

        var field = GetField(export.CashReceiptRlnRsn, "code");

        field.Error = true;

        return;
      }
    }

    // ------------------------------------------------------------
    // Data validation common to Add and Update command.
    // ------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.CashReceiptRlnRsn.Code))
      {
        var field = GetField(export.CashReceiptRlnRsn, "code");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(export.CashReceiptRlnRsn.Name))
      {
        var field = GetField(export.CashReceiptRlnRsn, "name");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(export.CashReceiptRlnRsn.Description))
      {
        var field = GetField(export.CashReceiptRlnRsn, "description");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (Equal(export.CashReceiptRlnRsn.EffectiveDate,
        local.Clear.EffectiveDate))
      {
        var field = GetField(export.CashReceiptRlnRsn, "effectiveDate");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ------------------------------------------------------------
    // Check data returned from CLBR.
    // ------------------------------------------------------------
    if (Equal(global.Command, "RETCLBR"))
    {
      if (!IsEmpty(import.Pass.Code))
      {
        MoveCashReceiptRlnRsn2(import.Pass, export.CashReceiptRlnRsn);
      }

      export.Prompt.Text1 = "";
      global.Command = "DISPLAY";
    }

    // ============================================================
    //       Main CASE OF COMMAND.
    // ============================================================
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ------------------------------------------------------------
        // PF2 DISPLAY
        // ------------------------------------------------------------
        if (IsEmpty(export.CashReceiptRlnRsn.Code))
        {
          ExitState = "FN0000_MANDATORY_FIELDS";

          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;

          break;
        }

        UseFnDisplayCrRlnRsnCode();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (Equal(export.CashReceiptRlnRsn.DiscontinueDate, local.Max.Date))
          {
            export.CashReceiptRlnRsn.DiscontinueDate =
              local.Clear.DiscontinueDate;
          }

          export.HiddenCashReceiptRlnRsn.Assign(export.CashReceiptRlnRsn);
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          export.HiddenCashReceiptRlnRsn.Assign(local.Clear);
        }

        break;
      case "EXIT":
        // -------------------------------------------------------------
        // PF3 EXIT  Displays CSAM - Cash Management Administration Menu
        // -------------------------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // ------------------------------------------------------------
        // PF4 LIST  Flows to CLBR to allow the user to list and select
        // a Cash Receipt Relationship Reason Code.
        // ------------------------------------------------------------
        switch(AsChar(export.Prompt.Text1))
        {
          case 'S':
            ExitState = "ECO_LINK_TO_CLBR";

            break;
          case ' ':
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            var field1 = GetField(export.Prompt, "text1");

            field1.Error = true;

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field2 = GetField(export.Prompt, "text1");

            field2.Error = true;

            break;
        }

        break;
      case "ADD":
        // ------------------------------------------------------------
        // PF5 ADD
        // Data Validation Edits:
        // *  Effective Date must be greater than or equal to current date.
        // *  If Discontinue date is not entered, default to 2099-12-31.
        // *  Discontinue Date must be greater than current date.
        // *  Discontinue Date must be greater than Effective Date.
        // ------------------------------------------------------------
        if (Lt(export.CashReceiptRlnRsn.EffectiveDate, local.Current.Date))
        {
          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          var field = GetField(export.CashReceiptRlnRsn, "effectiveDate");

          field.Error = true;

          break;
        }

        if (Equal(export.CashReceiptRlnRsn.DiscontinueDate,
          local.Clear.DiscontinueDate))
        {
          export.CashReceiptRlnRsn.DiscontinueDate = local.Max.Date;
        }
        else if (!Lt(local.Current.Date,
          export.CashReceiptRlnRsn.DiscontinueDate))
        {
          ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

          var field = GetField(export.CashReceiptRlnRsn, "discontinueDate");

          field.Error = true;

          break;
        }

        if (!Lt(export.CashReceiptRlnRsn.EffectiveDate,
          export.CashReceiptRlnRsn.DiscontinueDate))
        {
          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          var field1 = GetField(export.CashReceiptRlnRsn, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptRlnRsn, "discontinueDate");

          field2.Error = true;

          break;
        }

        UseFnCreateCrRlnRsnCode();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (Equal(export.CashReceiptRlnRsn.DiscontinueDate, local.Max.Date))
          {
            export.CashReceiptRlnRsn.DiscontinueDate =
              local.Clear.DiscontinueDate;
          }

          export.HiddenCashReceiptRlnRsn.Assign(export.CashReceiptRlnRsn);
          ExitState = "ACO_NI0000_CREATE_OK";
        }
        else if (IsExitState("FN0092_CASH_RCPT_RLN_RSN_AE"))
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;
        }
        else if (IsExitState("FN0094_CASH_RCPT_RLN_RSN_PV"))
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;
        }
        else
        {
          // --->  no special action required
        }

        break;
      case "UPDATE":
        // ------------------------------------------------------------
        // PF6 UPDATE
        // If the code value is effective with a future discontinue date,
        // only the Discontinue Date and Description can be changed.
        // ------------------------------------------------------------
        if (!Lt(local.Current.Date, export.HiddenCashReceiptRlnRsn.EffectiveDate))
          
        {
          if (!Equal(export.CashReceiptRlnRsn.Code,
            export.HiddenCashReceiptRlnRsn.Code))
          {
            ExitState = "CANNOT_CHANGE_EFFECTIVE_RECORD";

            var field = GetField(export.CashReceiptRlnRsn, "code");

            field.Error = true;
          }

          if (!Equal(export.CashReceiptRlnRsn.Name,
            export.HiddenCashReceiptRlnRsn.Name))
          {
            ExitState = "CANNOT_CHANGE_EFFECTIVE_RECORD";

            var field = GetField(export.CashReceiptRlnRsn, "name");

            field.Error = true;
          }

          if (!Equal(export.CashReceiptRlnRsn.EffectiveDate,
            export.HiddenCashReceiptRlnRsn.EffectiveDate))
          {
            ExitState = "CANNOT_CHANGE_EFFECTIVE_DATE";

            var field = GetField(export.CashReceiptRlnRsn, "effectiveDate");

            field.Error = true;

            break;
          }
        }

        // ------------------------------------------------------------
        // Future dated Discontinue Dates can be changed as long as
        // the new date is greater than the current date and greater
        // than the Effective Date.
        // If a Discontinue Date is not entered, it is defaulted to
        // 2099-12-31.
        // ------------------------------------------------------------
        if (Lt(local.Current.Date,
          export.HiddenCashReceiptRlnRsn.DiscontinueDate) || Equal
          (export.HiddenCashReceiptRlnRsn.DiscontinueDate,
          local.Clear.DiscontinueDate))
        {
          if (Equal(export.CashReceiptRlnRsn.DiscontinueDate,
            local.Clear.DiscontinueDate))
          {
            export.CashReceiptRlnRsn.DiscontinueDate = local.Max.Date;

            if (Equal(export.HiddenCashReceiptRlnRsn.DiscontinueDate,
              local.Clear.DiscontinueDate))
            {
              export.HiddenCashReceiptRlnRsn.DiscontinueDate = local.Max.Date;
            }
          }
          else if (!Lt(local.Current.Date,
            export.CashReceiptRlnRsn.DiscontinueDate))
          {
            ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

            var field = GetField(export.CashReceiptRlnRsn, "discontinueDate");

            field.Error = true;

            break;
          }

          if (!Lt(export.CashReceiptRlnRsn.EffectiveDate,
            export.CashReceiptRlnRsn.DiscontinueDate))
          {
            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            var field1 = GetField(export.CashReceiptRlnRsn, "effectiveDate");

            field1.Error = true;

            var field2 = GetField(export.CashReceiptRlnRsn, "discontinueDate");

            field2.Error = true;

            break;
          }
        }
        else if (!Equal(export.CashReceiptRlnRsn.DiscontinueDate,
          export.HiddenCashReceiptRlnRsn.DiscontinueDate))
        {
          ExitState = "CANNOT_CHANGE_A_DISCONTINUED_REC";

          var field = GetField(export.CashReceiptRlnRsn, "discontinueDate");

          field.Error = true;

          break;
        }

        // ------------------------------------------------------------
        // Determine if any data has been changed prior to executing
        // the update logic.
        // ------------------------------------------------------------
        if (Equal(export.CashReceiptRlnRsn.Code,
          export.HiddenCashReceiptRlnRsn.Code) && Equal
          (export.CashReceiptRlnRsn.Name, export.HiddenCashReceiptRlnRsn.Name) &&
          Equal
          (export.CashReceiptRlnRsn.EffectiveDate,
          export.HiddenCashReceiptRlnRsn.EffectiveDate) && Equal
          (export.CashReceiptRlnRsn.DiscontinueDate,
          export.HiddenCashReceiptRlnRsn.DiscontinueDate) && Equal
          (export.CashReceiptRlnRsn.Description,
          export.HiddenCashReceiptRlnRsn.Description))
        {
          ExitState = "FN0000_NO_CHANGE_TO_UPDATE";

          break;
        }

        // ------------------------------------------------------------
        // Update the Cash Receipt Rln Rsn record.
        // ------------------------------------------------------------
        UseFnUpdateCrRlnRsnCode();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (Equal(export.CashReceiptRlnRsn.DiscontinueDate, local.Max.Date))
          {
            export.CashReceiptRlnRsn.DiscontinueDate =
              local.Clear.DiscontinueDate;
          }

          export.HiddenCashReceiptRlnRsn.Assign(export.CashReceiptRlnRsn);
          ExitState = "FN0000_UPDATE_SUCCESSFUL";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DELETE":
        // ------------------------------------------------------------
        // PF10 DELETE
        // A record can only be deleted on the same day it is added or when the 
        // Effective Date is greater than the current date.
        // ------------------------------------------------------------
        if (!Lt(Now().Date, export.CashReceiptRlnRsn.EffectiveDate) && Lt
          (Date(export.CashReceiptRlnRsn.CreatedTimestamp), local.Current.Date))
        {
          ExitState = "CANNOT_DELETE_EFFECTIVE_RECORD";

          break;
        }

        UseFnDeleteCrRlnRsnCode();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CashReceiptRlnRsn.Assign(local.Clear);
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          export.HiddenCashReceiptRlnRsn.Assign(local.Clear);
        }

        break;
      case "SIGNOFF":
        // ------------------------------------------------------------
        // PF12 SIGNOFF
        // ------------------------------------------------------------
        UseScEabSignoff();

        return;
      case "":
        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // ------------------------------------------------------------
    // Protection Logic.
    // ------------------------------------------------------------
    if (Lt(local.Clear.EffectiveDate,
      export.HiddenCashReceiptRlnRsn.EffectiveDate) && !
      Lt(local.Current.Date, export.HiddenCashReceiptRlnRsn.EffectiveDate))
    {
      var field1 = GetField(export.CashReceiptRlnRsn, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.CashReceiptRlnRsn, "name");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.CashReceiptRlnRsn, "effectiveDate");

      field3.Color = "cyan";
      field3.Protected = true;
    }

    if (Lt(local.Clear.DiscontinueDate,
      export.HiddenCashReceiptRlnRsn.DiscontinueDate) && !
      Lt(local.Current.Date, export.HiddenCashReceiptRlnRsn.DiscontinueDate))
    {
      var field = GetField(export.CashReceiptRlnRsn, "discontinueDate");

      field.Color = "cyan";
      field.Protected = true;
    }
  }

  private static void MoveCashReceiptRlnRsn1(CashReceiptRlnRsn source,
    CashReceiptRlnRsn target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCashReceiptRlnRsn2(CashReceiptRlnRsn source,
    CashReceiptRlnRsn target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreateCrRlnRsnCode()
  {
    var useImport = new FnCreateCrRlnRsnCode.Import();
    var useExport = new FnCreateCrRlnRsnCode.Export();

    useImport.CashReceiptRlnRsn.Assign(export.CashReceiptRlnRsn);

    Call(FnCreateCrRlnRsnCode.Execute, useImport, useExport);

    MoveCashReceiptRlnRsn1(useExport.CashReceiptRlnRsn, export.CashReceiptRlnRsn);
      
  }

  private void UseFnDeleteCrRlnRsnCode()
  {
    var useImport = new FnDeleteCrRlnRsnCode.Import();
    var useExport = new FnDeleteCrRlnRsnCode.Export();

    useImport.CashReceiptRlnRsn.SystemGeneratedIdentifier =
      export.CashReceiptRlnRsn.SystemGeneratedIdentifier;

    Call(FnDeleteCrRlnRsnCode.Execute, useImport, useExport);
  }

  private void UseFnDisplayCrRlnRsnCode()
  {
    var useImport = new FnDisplayCrRlnRsnCode.Import();
    var useExport = new FnDisplayCrRlnRsnCode.Export();

    useImport.CashReceiptRlnRsn.Code = export.CashReceiptRlnRsn.Code;

    Call(FnDisplayCrRlnRsnCode.Execute, useImport, useExport);

    export.CashReceiptRlnRsn.Assign(useExport.CashReceiptRlnRsn);
  }

  private void UseFnUpdateCrRlnRsnCode()
  {
    var useImport = new FnUpdateCrRlnRsnCode.Import();
    var useExport = new FnUpdateCrRlnRsnCode.Export();

    useImport.CashReceiptRlnRsn.Assign(export.CashReceiptRlnRsn);

    Call(FnUpdateCrRlnRsnCode.Execute, useImport, useExport);

    export.CashReceiptRlnRsn.Assign(useExport.CashReceiptRlnRsn);
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScEabSignoff()
  {
    var useImport = new ScEabSignoff.Import();
    var useExport = new ScEabSignoff.Export();

    Call(ScEabSignoff.Execute, useImport, useExport);
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptRlnRsn")]
    public CashReceiptRlnRsn HiddenCashReceiptRlnRsn
    {
      get => hiddenCashReceiptRlnRsn ??= new();
      set => hiddenCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CashReceiptRlnRsn Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    private TextWorkArea prompt;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptRlnRsn hiddenCashReceiptRlnRsn;
    private TypeStatusAudit typeStatusAudit;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CashReceiptRlnRsn pass;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptRlnRsn")]
    public CashReceiptRlnRsn HiddenCashReceiptRlnRsn
    {
      get => hiddenCashReceiptRlnRsn ??= new();
      set => hiddenCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
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

    private TextWorkArea prompt;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptRlnRsn hiddenCashReceiptRlnRsn;
    private TypeStatusAudit typeStatusAudit;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CashReceiptRlnRsn Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private CashReceiptRlnRsn clear;
  }
#endregion
}

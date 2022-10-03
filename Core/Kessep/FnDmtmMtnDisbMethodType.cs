// Program: FN_DMTM_MTN_DISB_METHOD_TYPE, ID: 371827972, model: 746.
// Short name: SWEDMTMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DMTM_MTN_DISB_METHOD_TYPE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDmtmMtnDisbMethodType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DMTM_MTN_DISB_METHOD_TYPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDmtmMtnDisbMethodType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDmtmMtnDisbMethodType.
  /// </summary>
  public FnDmtmMtnDisbMethodType(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // PR's 133601, 133602, & 133583. Provide for installation of the security 
    // cab. L. Bachura 12-22-2001
    // *** Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // *** Move all IMPORTs to EXPORTs.
    export.HiddenId.Assign(import.HiddenId);
    export.PromptTextWorkArea.Text1 = import.PromptTextWorkArea.Text1;
    export.PaymentMethodType.Assign(import.PaymentMethodType);
    export.TypeStatusAudit.Assign(import.TypeStatusAudit);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "RETDMTL"))
    {
      if (IsEmpty(import.FlowSelected.Code))
      {
        MovePaymentMethodType2(export.HiddenId, export.PaymentMethodType);
      }
      else
      {
        MovePaymentMethodType2(import.FlowSelected, export.PaymentMethodType);
      }

      global.Command = "DISPLAY";
    }

    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
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
      // *** This is where you set your export value to the export hidden next 
      // tran values if the user is coming into this procedure on a next tran
      // action. ***
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ***  The logic assumes that a record cannot be
    // UPDATEd or DELETEd without first being displayed.
    // Therefore, a key change with either command is invalid. ***
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      !Equal(import.PaymentMethodType.Code, export.HiddenId.Code))
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field = GetField(export.PaymentMethodType, "code");

      field.Error = true;

      return;
    }

    // ------------------------
    // If the key field is blank, certain commands are not allowed.
    // ------------------------
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE")) && IsEmpty
      (export.PaymentMethodType.Code))
    {
      export.PaymentMethodType.Description =
        Spaces(PaymentMethodType.Description_MaxLength);
      export.PaymentMethodType.Name = "";
      export.PaymentMethodType.EffectiveDate = null;
      export.PaymentMethodType.DiscontinueDate = null;
      export.PromptTextWorkArea.Text1 = "+";
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.PaymentMethodType, "code");

      field.Error = true;

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(import.PaymentMethodType.Code))
      {
        var field = GetField(export.PaymentMethodType, "code");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      if (IsEmpty(import.PaymentMethodType.Name))
      {
        var field = GetField(export.PaymentMethodType, "name");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }

    // Security Logic. Added 12-22-2001, per PR's noted above.
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ************************************
    //        Main CASE OF COMMAND.
    // ************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // -----------------
        // Calls the display module.
        // -----------------
        UseFnReadPaymentMethodType();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------
          // Set the hidden key field to that of the new record.
          // ------------------
          export.HiddenId.Assign(export.PaymentMethodType);

          // If discontinue date contains maximum date then display blank 
          // instead
          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          if (Equal(export.PaymentMethodType.DiscontinueDate, null))
          {
          }
          else if (Lt(export.PaymentMethodType.DiscontinueDate, Now().Date))
          {
            var field2 = GetField(export.PaymentMethodType, "name");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentMethodType, "discontinueDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.PaymentMethodType, "description");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (Equal(export.PaymentMethodType.EffectiveDate, null))
          {
          }
          else if (Lt(export.PaymentMethodType.EffectiveDate, Now().Date))
          {
            var field2 = GetField(export.PaymentMethodType, "name");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (Equal(export.PaymentMethodType.DiscontinueDate, local.Max.Date))
          {
            export.PaymentMethodType.DiscontinueDate = local.Min.Date;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (IsExitState("FN0000_PYMNT_MTHD_TYPE_NF"))
        {
          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Error = true;

          export.PaymentMethodType.Description =
            Spaces(PaymentMethodType.Description_MaxLength);
          export.PaymentMethodType.Name = "";
          export.PaymentMethodType.DiscontinueDate = null;
          export.PaymentMethodType.EffectiveDate = null;
          export.HiddenId.SystemGeneratedIdentifier = 0;
        }
        else
        {
        }

        export.PromptTextWorkArea.Text1 = "+";

        break;
      case "ADD":
        if (IsEmpty(export.PaymentMethodType.Name))
        {
          var field1 = GetField(export.PaymentMethodType, "name");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (Equal(export.PaymentMethodType.DiscontinueDate, null))
        {
          export.PaymentMethodType.DiscontinueDate = new DateTime(2099, 12, 31);
        }

        if (Equal(export.PaymentMethodType.EffectiveDate, null))
        {
          export.PaymentMethodType.EffectiveDate = Now().Date;
        }

        if (Lt(export.PaymentMethodType.EffectiveDate, Now().Date))
        {
          var field1 = GetField(export.PaymentMethodType, "effectiveDate");

          field1.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (Lt(export.PaymentMethodType.DiscontinueDate, Now().Date))
        {
          var field1 = GetField(export.PaymentMethodType, "discontinueDate");

          field1.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

          return;
        }

        if (!Lt(export.PaymentMethodType.EffectiveDate,
          export.PaymentMethodType.DiscontinueDate))
        {
          var field1 = GetField(export.PaymentMethodType, "discontinueDate");

          field1.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          return;
        }

        // *** Calls the create module.
        UseFnCreatePaymentMethodType();

        if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          if (export.PaymentMethodType.SystemGeneratedIdentifier != 0)
          {
            var field3 = GetField(export.PaymentMethodType, "code");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          var field1 = GetField(export.PaymentMethodType, "discontinueDate");

          field1.Error = true;

          var field2 = GetField(export.PaymentMethodType, "effectiveDate");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------
          // Set the hidden key field to that of the new record.
          // ---------------
          export.HiddenId.Assign(export.PaymentMethodType);

          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
        }

        break;
      case "UPDATE":
        // *** Calls the update module.***
        if (Equal(export.PaymentMethodType.EffectiveDate, null))
        {
          export.PaymentMethodType.EffectiveDate = Now().Date;
        }

        if (Equal(export.PaymentMethodType.DiscontinueDate, null))
        {
          export.PaymentMethodType.DiscontinueDate = new DateTime(2099, 12, 31);
        }

        if (Equal(export.PaymentMethodType.DiscontinueDate,
          export.HiddenId.DiscontinueDate) && Lt
          (export.PaymentMethodType.DiscontinueDate, Now().Date))
        {
          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.PaymentMethodType, "name");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.PaymentMethodType, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.PaymentMethodType, "discontinueDate");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.PaymentMethodType, "description");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.PaymentMethodType.DiscontinueDate,
          export.HiddenId.DiscontinueDate) || !
          Equal(export.PaymentMethodType.EffectiveDate,
          export.HiddenId.EffectiveDate))
        {
          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          if (Equal(export.PaymentMethodType.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.PaymentMethodType.EffectiveDate, Now().Date))
          {
            var field2 = GetField(export.PaymentMethodType, "name");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!Equal(export.PaymentMethodType.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.PaymentMethodType.EffectiveDate, Now().Date))
          {
            var field2 = GetField(export.PaymentMethodType, "effectiveDate");

            field2.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.PaymentMethodType.DiscontinueDate, Now().Date))
          {
            var field2 = GetField(export.PaymentMethodType, "discontinueDate");

            field2.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.PaymentMethodType.EffectiveDate,
            export.PaymentMethodType.DiscontinueDate))
          {
            var field2 = GetField(export.PaymentMethodType, "discontinueDate");

            field2.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        UseFnUpdatePaymentMethodType();

        if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field1 = GetField(export.PaymentMethodType, "discontinueDate");

          field1.Error = true;

          var field2 = GetField(export.PaymentMethodType, "effectiveDate");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          if (Lt(export.PaymentMethodType.EffectiveDate, Now().Date))
          {
            var field2 = GetField(export.PaymentMethodType, "name");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
        }

        break;
      case "DELETE":
        // *** Calls the delete module.
        var field = GetField(export.PaymentMethodType, "code");

        field.Color = "cyan";
        field.Protected = true;

        if (Equal(export.PaymentMethodType.DiscontinueDate, null))
        {
        }
        else if (Lt(export.PaymentMethodType.DiscontinueDate, Now().Date))
        {
          var field1 = GetField(export.PaymentMethodType, "name");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.PaymentMethodType, "effectiveDate");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.PaymentMethodType, "discontinueDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.PaymentMethodType, "description");

          field4.Color = "cyan";
          field4.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        if (Equal(export.PaymentMethodType.EffectiveDate, null))
        {
        }
        else if (Lt(export.PaymentMethodType.EffectiveDate, Now().Date))
        {
          var field1 = GetField(export.PaymentMethodType, "name");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.PaymentMethodType, "effectiveDate");

          field2.Color = "cyan";
          field2.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        UseFnDeletePaymentMethodType();

        if (IsExitState("FN0000_PYMNT_MTHD_TYPE_NF"))
        {
          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ***
          // Set the hidden key field to spaces or zero.
          export.PaymentMethodType.Assign(local.Blank);
          export.HiddenId.Assign(local.Blank);

          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Color = "";
          field1.Protected = false;

          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else
        {
        }

        break;
      case "LIST":
        if (AsChar(export.PromptTextWorkArea.Text1) == 'S')
        {
          export.HiddenId.Assign(export.PaymentMethodType);
          export.PromptTextWorkArea.Text1 = "+";
          ExitState = "ECO_LNK_TO_LST_DISB_METHOD_TYPES";
        }
        else
        {
          if (Lt(export.PaymentMethodType.EffectiveDate, Now().Date) && export
            .PaymentMethodType.SystemGeneratedIdentifier != 0)
          {
            var field2 = GetField(export.PaymentMethodType, "code");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentMethodType, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (Lt(export.PaymentMethodType.DiscontinueDate, Now().Date) && export
            .PaymentMethodType.SystemGeneratedIdentifier != 0 && !
            Equal(export.PaymentMethodType.DiscontinueDate, null))
          {
            var field2 = GetField(export.PaymentMethodType, "code");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentMethodType, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.PaymentMethodType, "discontinueDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.PaymentMethodType, "description");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          var field1 = GetField(export.PromptTextWorkArea, "text1");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

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
      default:
        if (export.PaymentMethodType.SystemGeneratedIdentifier != 0)
        {
          var field1 = GetField(export.PaymentMethodType, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          if (Lt(export.PaymentMethodType.DiscontinueDate, Now().Date) && !
            Equal(export.PaymentMethodType.DiscontinueDate, null))
          {
            var field2 = GetField(export.PaymentMethodType, "name");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentMethodType, "discontinueDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.PaymentMethodType, "description");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (Lt(export.PaymentMethodType.EffectiveDate, Now().Date))
          {
            var field2 = GetField(export.PaymentMethodType, "name");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PaymentMethodType, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MovePaymentMethodType1(PaymentMethodType source,
    PaymentMethodType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePaymentMethodType2(PaymentMethodType source,
    PaymentMethodType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveTypeStatusAudit(TypeStatusAudit source,
    TypeStatusAudit target)
  {
    target.StringOfOthers = source.StringOfOthers;
    target.TableName = source.TableName;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreatePaymentMethodType()
  {
    var useImport = new FnCreatePaymentMethodType.Import();
    var useExport = new FnCreatePaymentMethodType.Export();

    useImport.PaymentMethodType.Assign(export.PaymentMethodType);

    Call(FnCreatePaymentMethodType.Execute, useImport, useExport);

    MovePaymentMethodType1(useExport.PaymentMethodType, export.PaymentMethodType);
      
  }

  private void UseFnDeletePaymentMethodType()
  {
    var useImport = new FnDeletePaymentMethodType.Import();
    var useExport = new FnDeletePaymentMethodType.Export();

    useImport.PaymentMethodType.Assign(export.PaymentMethodType);

    Call(FnDeletePaymentMethodType.Execute, useImport, useExport);

    MoveTypeStatusAudit(useExport.TypeStatusAudit, export.TypeStatusAudit);
  }

  private void UseFnReadPaymentMethodType()
  {
    var useImport = new FnReadPaymentMethodType.Import();
    var useExport = new FnReadPaymentMethodType.Export();

    useImport.Flag.Flag = import.Flag.Flag;
    useImport.PaymentMethodType.Assign(export.PaymentMethodType);

    Call(FnReadPaymentMethodType.Execute, useImport, useExport);

    export.PaymentMethodType.Assign(useExport.PaymentMethodType);
  }

  private void UseFnUpdatePaymentMethodType()
  {
    var useImport = new FnUpdatePaymentMethodType.Import();
    var useExport = new FnUpdatePaymentMethodType.Export();

    useImport.PaymentMethodType.Assign(export.PaymentMethodType);

    Call(FnUpdatePaymentMethodType.Execute, useImport, useExport);

    MovePaymentMethodType1(useExport.PaymentMethodType, export.PaymentMethodType);
      
    export.TypeStatusAudit.Assign(useExport.TypeStatusAudit);
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of PromptTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptTextWorkArea")]
    public TextWorkArea PromptTextWorkArea
    {
      get => promptTextWorkArea ??= new();
      set => promptTextWorkArea = value;
    }

    /// <summary>
    /// A value of FlowSelected.
    /// </summary>
    [JsonPropertyName("flowSelected")]
    public PaymentMethodType FlowSelected
    {
      get => flowSelected ??= new();
      set => flowSelected = value;
    }

    /// <summary>
    /// A value of PromptCommon.
    /// </summary>
    [JsonPropertyName("promptCommon")]
    public Common PromptCommon
    {
      get => promptCommon ??= new();
      set => promptCommon = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
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
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PaymentMethodType HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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

    private Common flag;
    private TextWorkArea promptTextWorkArea;
    private PaymentMethodType flowSelected;
    private Common promptCommon;
    private PaymentMethodType paymentMethodType;
    private TypeStatusAudit typeStatusAudit;
    private PaymentMethodType hiddenId;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PromptTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptTextWorkArea")]
    public TextWorkArea PromptTextWorkArea
    {
      get => promptTextWorkArea ??= new();
      set => promptTextWorkArea = value;
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
    /// A value of PromptCommon.
    /// </summary>
    [JsonPropertyName("promptCommon")]
    public Common PromptCommon
    {
      get => promptCommon ??= new();
      set => promptCommon = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
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
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PaymentMethodType HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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

    private TextWorkArea promptTextWorkArea;
    private NextTranInfo hidden;
    private Common promptCommon;
    private PaymentMethodType paymentMethodType;
    private TypeStatusAudit typeStatusAudit;
    private PaymentMethodType hiddenId;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public PaymentMethodType Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of Min.
    /// </summary>
    [JsonPropertyName("min")]
    public DateWorkArea Min
    {
      get => min ??= new();
      set => min = value;
    }

    private PaymentMethodType blank;
    private DateWorkArea max;
    private DateWorkArea min;
  }
#endregion
}

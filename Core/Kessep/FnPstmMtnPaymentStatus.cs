// Program: FN_PSTM_MTN_PAYMENT_STATUS, ID: 371839187, model: 746.
// Short name: SWEPSTMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PSTM_MTN_PAYMENT_STATUS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPstmMtnPaymentStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PSTM_MTN_PAYMENT_STATUS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPstmMtnPaymentStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPstmMtnPaymentStatus.
  /// </summary>
  public FnPstmMtnPaymentStatus(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // MAINTENANCE LOG
    // DATE	  AUTHOR        CHG REQ#     DESCRIPTION
    // 12/18/96  R. Marchman 			Add data level security
    // 03/10/97  JF. Caillouet			Display / Dialog Design changes.
    // *********************************************
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // *****  Move all IMPORTs to EXPORTs.
    export.PaymentStatus.Assign(import.PaymentStatus);
    export.HiddenId.Assign(import.HiddenId);
    export.HiddenDisplayOk.Flag = import.HiddenDisplayOk.Flag;
    export.PromptTextWorkArea.Text1 = import.PromptTextWorkArea.Text1;

    // *** Check Return Data ***
    if (Equal(global.Command, "RETPSTL"))
    {
      if (IsEmpty(import.FlowSelection.Code))
      {
        MovePaymentStatus3(export.HiddenId, export.PaymentStatus);
      }
      else
      {
        MovePaymentStatus2(import.FlowSelection, export.PaymentStatus);
      }

      global.Command = "DISPLAY";
    }

    // *****  The logic assumes that a record cannot be UPDATEd or DELETEd 
    // without first being displayed. Therefore, a key change with either
    // command is invalid.
    // *****
    if (IsEmpty(global.Command))
    {
      var field = GetField(export.PaymentStatus, "code");

      field.Error = true;

      ExitState = "KEY_FIELD_IS_BLANK";

      return;
    }

    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      !Equal(import.PaymentStatus.Code, import.HiddenId.Code))
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field = GetField(export.PaymentStatus, "code");

      field.Error = true;

      return;
    }

    // ---------------------
    // If the key field is blank, certain commands are not allowed.
    // ---------------------
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE")) && IsEmpty
      (export.PaymentStatus.Code))
    {
      export.PaymentStatus.EffectiveDate = null;
      export.PaymentStatus.DiscontinueDate = null;
      export.PaymentStatus.Description =
        Spaces(PaymentStatus.Description_MaxLength);
      export.PaymentStatus.Name = "";
      export.PromptTextWorkArea.Text1 = "+";
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.PaymentStatus, "code");

      field.Error = true;

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
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

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        UseScCabNextTranGet();
        global.Command = "DISPLAY";
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        global.Command = "DISPLAY";
      }
    }
    else
    {
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "PSTL"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** Edit for display/add/modify/delete ****
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.PaymentStatus.Name))
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field = GetField(export.PaymentStatus, "name");

        field.Error = true;

        return;
      }

      if (Equal(export.PaymentStatus.EffectiveDate, null))
      {
        export.PaymentStatus.EffectiveDate = Now().Date;
      }

      if (Equal(export.PaymentStatus.DiscontinueDate, null))
      {
        export.PaymentStatus.DiscontinueDate = new DateTime(2099, 12, 31);
      }

      if (Equal(global.Command, "ADD"))
      {
        if (Lt(export.PaymentStatus.EffectiveDate, Now().Date))
        {
          var field = GetField(export.PaymentStatus, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (Lt(export.PaymentStatus.DiscontinueDate, Now().Date))
        {
          var field = GetField(export.PaymentStatus, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }

        if (!Lt(export.PaymentStatus.EffectiveDate,
          export.PaymentStatus.DiscontinueDate))
        {
          var field = GetField(export.PaymentStatus, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          return;
        }
      }
    }

    // ******************************
    //      Main CASE OF COMMAND.
    // ******************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        UseFnReadPaymentStatus();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenDisplayOk.Flag = "Y";
          export.HiddenId.Assign(export.PaymentStatus);

          var field = GetField(export.PaymentStatus, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Equal(export.PaymentStatus.DiscontinueDate, local.Max.Date))
          {
          }
          else if (Lt(export.PaymentStatus.DiscontinueDate, Now().Date))
          {
            var field3 = GetField(export.PaymentStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.PaymentStatus, "discontinueDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.PaymentStatus, "description");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (Lt(export.PaymentStatus.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.PaymentStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (Equal(export.PaymentStatus.DiscontinueDate, local.Max.Date))
          {
            export.PaymentStatus.DiscontinueDate = null;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          export.HiddenDisplayOk.Flag = "N";
          export.HiddenId.SystemGeneratedIdentifier = 0;
          export.HiddenId.Code = "";

          var field = GetField(export.PaymentStatus, "code");

          field.Error = true;

          export.PaymentStatus.Name = "";
          export.PaymentStatus.EffectiveDate = null;
          export.PaymentStatus.DiscontinueDate = null;
          export.PaymentStatus.Description =
            Spaces(PaymentStatus.Description_MaxLength);
        }

        export.PromptTextWorkArea.Text1 = "+";

        break;
      case "ADD":
        UseFnCreatePaymentStatus();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *****  Set the hidden key field to that of the new record.
          export.HiddenId.Assign(export.PaymentStatus);

          var field = GetField(export.PaymentStatus, "code");

          field.Color = "cyan";
          field.Protected = true;

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
          if (export.PaymentStatus.SystemGeneratedIdentifier != 0)
          {
            var field = GetField(export.PaymentStatus, "code");

            field.Color = "cyan";
            field.Protected = true;
          }

          var field3 = GetField(export.PaymentStatus, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.PaymentStatus, "discontinueDate");

          field4.Error = true;
        }

        break;
      case "UPDATE":
        if (AsChar(export.HiddenDisplayOk.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        var field1 = GetField(export.PaymentStatus, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        if (Equal(export.PaymentStatus.DiscontinueDate,
          export.HiddenId.DiscontinueDate) && Lt
          (export.PaymentStatus.DiscontinueDate, Now().Date))
        {
          var field3 = GetField(export.PaymentStatus, "name");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.PaymentStatus, "description");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.PaymentStatus, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.PaymentStatus, "discontinueDate");

          field6.Color = "cyan";
          field6.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.PaymentStatus.DiscontinueDate,
          export.HiddenId.DiscontinueDate) || !
          Equal(export.PaymentStatus.EffectiveDate,
          export.HiddenId.EffectiveDate))
        {
          if (Equal(export.PaymentStatus.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.PaymentStatus.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.PaymentStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!Equal(export.PaymentStatus.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.PaymentStatus.EffectiveDate, Now().Date))
          {
            var field = GetField(export.PaymentStatus, "effectiveDate");

            field.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.PaymentStatus.DiscontinueDate, Now().Date))
          {
            var field = GetField(export.PaymentStatus, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.PaymentStatus.EffectiveDate,
            export.PaymentStatus.DiscontinueDate))
          {
            var field = GetField(export.PaymentStatus, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        UseFnUpdatePaymentStatus();

        var field2 = GetField(export.PaymentStatus, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        if (IsExitState("FN0000_PYMNT_STAT_NF"))
        {
          var field = GetField(export.PaymentStatus, "code");

          field.Error = true;
        }
        else if (IsExitState("FN0000_PYMNT_STAT_PV"))
        {
          var field3 = GetField(export.PaymentStatus, "name");

          field3.Error = true;

          var field4 = GetField(export.PaymentStatus, "code");

          field4.Error = true;

          var field5 = GetField(export.PaymentStatus, "description");

          field5.Error = true;
        }
        else if (IsExitState("FN0000_PYMNT_STAT_NU"))
        {
          var field = GetField(export.PaymentStatus, "code");

          field.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenId.Assign(export.PaymentStatus);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          var field3 = GetField(export.PaymentStatus, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.PaymentStatus, "discontinueDate");

          field4.Error = true;

          // Continue Processing
        }

        if (Lt(export.PaymentStatus.EffectiveDate, Now().Date) && Equal
          (export.PaymentStatus.EffectiveDate, export.HiddenId.EffectiveDate))
        {
          var field3 = GetField(export.PaymentStatus, "name");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.PaymentStatus, "effectiveDate");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        break;
      case "DELETE":
        if (AsChar(export.HiddenDisplayOk.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        if (Equal(export.PaymentStatus.DiscontinueDate, null))
        {
        }
        else if (Lt(export.PaymentStatus.DiscontinueDate, Now().Date))
        {
          var field3 = GetField(export.PaymentStatus, "code");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.PaymentStatus, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.PaymentStatus, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.PaymentStatus, "discontinueDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.PaymentStatus, "description");

          field7.Color = "cyan";
          field7.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        if (Equal(export.PaymentStatus.EffectiveDate, null))
        {
        }
        else if (Lt(export.PaymentStatus.EffectiveDate, Now().Date))
        {
          var field3 = GetField(export.PaymentStatus, "code");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.PaymentStatus, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.PaymentStatus, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        UseFnDeletePaymentStatus();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.PaymentStatus.Name = "";
          export.PaymentStatus.Description =
            Spaces(PaymentStatus.Description_MaxLength);
          export.PaymentStatus.EffectiveDate = null;
          export.PaymentStatus.DiscontinueDate = null;

          // *****  Set the hidden key field to spaces or zero.
          export.HiddenDisplayOk.Flag = "N";
          export.PaymentStatus.Assign(local.Blank);
          export.HiddenId.Assign(local.Blank);
          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else
        {
        }

        break;
      case "LIST":
        if (AsChar(export.PromptTextWorkArea.Text1) == 'S')
        {
          export.PromptTextWorkArea.Text1 = "+";
          ExitState = "ECO_LNK_TO_LST_PAYMENT_STATUSES";

          return;
        }
        else
        {
          if (export.PaymentStatus.SystemGeneratedIdentifier != 0)
          {
            var field3 = GetField(export.PaymentStatus, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            if (Lt(export.PaymentStatus.DiscontinueDate, Now().Date) && !
              Equal(export.PaymentStatus.DiscontinueDate, null))
            {
              var field4 = GetField(export.PaymentStatus, "name");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.PaymentStatus, "effectiveDate");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.PaymentStatus, "discontinueDate");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.PaymentStatus, "description");

              field7.Color = "cyan";
              field7.Protected = true;
            }

            if (Lt(export.PaymentStatus.EffectiveDate, Now().Date))
            {
              var field4 = GetField(export.PaymentStatus, "name");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.PaymentStatus, "effectiveDate");

              field5.Color = "cyan";
              field5.Protected = true;
            }
          }

          var field = GetField(export.PromptTextWorkArea, "text1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PSTL":
        ExitState = "ECO_XFR_TO_LST_PAYMENT_STAT";

        break;
      default:
        if (export.PaymentStatus.SystemGeneratedIdentifier != 0)
        {
          var field = GetField(export.PaymentStatus, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.PaymentStatus.DiscontinueDate, Now().Date) && !
            Equal(export.PaymentStatus.DiscontinueDate, null))
          {
            var field3 = GetField(export.PaymentStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.PaymentStatus, "discontinueDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.PaymentStatus, "description");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (Lt(export.PaymentStatus.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.PaymentStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PaymentStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

  private static void MovePaymentStatus1(PaymentStatus source,
    PaymentStatus target)
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

  private static void MovePaymentStatus2(PaymentStatus source,
    PaymentStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MovePaymentStatus3(PaymentStatus source,
    PaymentStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreatePaymentStatus()
  {
    var useImport = new FnCreatePaymentStatus.Import();
    var useExport = new FnCreatePaymentStatus.Export();

    useImport.PaymentStatus.Assign(export.PaymentStatus);

    Call(FnCreatePaymentStatus.Execute, useImport, useExport);

    MovePaymentStatus1(useExport.PaymentStatus, export.PaymentStatus);
  }

  private void UseFnDeletePaymentStatus()
  {
    var useImport = new FnDeletePaymentStatus.Import();
    var useExport = new FnDeletePaymentStatus.Export();

    useImport.PaymentStatus.Assign(export.PaymentStatus);

    Call(FnDeletePaymentStatus.Execute, useImport, useExport);

    export.PaymentStatus.Assign(useExport.PaymentStatus);
  }

  private void UseFnReadPaymentStatus()
  {
    var useImport = new FnReadPaymentStatus.Import();
    var useExport = new FnReadPaymentStatus.Export();

    useImport.Flag.Flag = import.Flag.Flag;
    useImport.PaymentStatus.Assign(export.PaymentStatus);

    Call(FnReadPaymentStatus.Execute, useImport, useExport);

    export.PaymentStatus.Assign(useExport.PaymentStatus);
  }

  private void UseFnUpdatePaymentStatus()
  {
    var useImport = new FnUpdatePaymentStatus.Import();
    var useExport = new FnUpdatePaymentStatus.Export();

    useImport.PaymentStatus.Assign(export.PaymentStatus);

    Call(FnUpdatePaymentStatus.Execute, useImport, useExport);

    export.PaymentStatus.Assign(useExport.PaymentStatus);
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
    /// A value of PromptTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptTextWorkArea")]
    public TextWorkArea PromptTextWorkArea
    {
      get => promptTextWorkArea ??= new();
      set => promptTextWorkArea = value;
    }

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
    /// A value of HiddenDisplayOk.
    /// </summary>
    [JsonPropertyName("hiddenDisplayOk")]
    public Common HiddenDisplayOk
    {
      get => hiddenDisplayOk ??= new();
      set => hiddenDisplayOk = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PaymentStatus HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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
    /// A value of FlowSelection.
    /// </summary>
    [JsonPropertyName("flowSelection")]
    public PaymentStatus FlowSelection
    {
      get => flowSelection ??= new();
      set => flowSelection = value;
    }

    private TextWorkArea promptTextWorkArea;
    private Common flag;
    private Common hiddenDisplayOk;
    private Common promptCommon;
    private PaymentStatus paymentStatus;
    private PaymentStatus hiddenId;
    private NextTranInfo hidden;
    private Standard standard;
    private PaymentStatus flowSelection;
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
    /// A value of HiddenDisplayOk.
    /// </summary>
    [JsonPropertyName("hiddenDisplayOk")]
    public Common HiddenDisplayOk
    {
      get => hiddenDisplayOk ??= new();
      set => hiddenDisplayOk = value;
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
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public Common Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PaymentStatus HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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

    private TextWorkArea promptTextWorkArea;
    private Common hiddenDisplayOk;
    private Common promptCommon;
    private Common flow;
    private PaymentStatus paymentStatus;
    private PaymentStatus hiddenId;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
    }

    /// <summary>
    /// A value of Effective.
    /// </summary>
    [JsonPropertyName("effective")]
    public DateWorkArea Effective
    {
      get => effective ??= new();
      set => effective = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public PaymentStatus Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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

    private DateWorkArea dateWorkArea;
    private DateWorkArea discontinue;
    private DateWorkArea effective;
    private PaymentStatus blank;
    private DateWorkArea zero;
    private DateWorkArea max;
  }
#endregion
}

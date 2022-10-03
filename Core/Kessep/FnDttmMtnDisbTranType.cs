// Program: FN_DTTM_MTN_DISB_TRAN_TYPE, ID: 371837284, model: 746.
// Short name: SWEDTTMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DTTM_MTN_DISB_TRAN_TYPE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDttmMtnDisbTranType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DTTM_MTN_DISB_TRAN_TYPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDttmMtnDisbTranType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDttmMtnDisbTranType.
  /// </summary>
  public FnDttmMtnDisbTranType(IContext context, Import import, Export export):
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
    // Date	  Author	ChgReq#	Discription
    // 03/10/97  JF. Caillouet		Display / Dialog Design changes.
    // *********************************************
    // *** Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    // Clear screen on command CLEAR - PF Key 11.
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // *** Move all IMPORTs to EXPORTs.
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.DisbursementTransactionType.
      Assign(import.DisbursementTransactionType);
    export.HiddenId.Assign(import.HiddenId);
    export.PromptTextWorkArea.Text1 = import.PromptTextWorkArea.Text1;

    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETDTTL"))
    {
      if (IsEmpty(import.DisbursementTransactionType.Code))
      {
        MoveDisbursementTransactionType2(export.HiddenId,
          export.DisbursementTransactionType);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    // Set command when coming in from a menu.
    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // Note if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. Now validate.
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****    NEXT TRAN Logic    ****
      // set the local next_tran_info attributes to the import view attribues 
      // for the data to be passed to the next transaction.
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

    // ---------------------------------------------------
    // The logic assumes that a record cannot be UPDATEd or DELETEd
    // without first being displayed. Therefore, a key change with
    // either command is invalid.
    // ----------------------------------------------------
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      !Equal(import.DisbursementTransactionType.Code, import.HiddenId.Code))
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field = GetField(export.DisbursementTransactionType, "code");

      field.Error = true;

      return;
    }

    // If the key field is blank, certain commands are not allowed.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE")) && IsEmpty
      (export.DisbursementTransactionType.Code))
    {
      var field = GetField(export.DisbursementTransactionType, "code");

      field.Error = true;

      // ----------------
      // Clear all fields being displayed on the screen
      // ----------------
      export.HiddenId.Code = "";
      export.HiddenId.SystemGeneratedIdentifier = 0;
      export.DisbursementTransactionType.Description =
        Spaces(DisbursementTransactionType.Description_MaxLength);
      export.DisbursementTransactionType.Name = "";
      export.DisbursementTransactionType.DiscontinueDate = null;
      export.DisbursementTransactionType.EffectiveDate = null;
      export.PromptTextWorkArea.Text1 = "+";
      ExitState = "KEY_FIELD_IS_BLANK";

      return;
    }

    if (Equal(global.Command, "DTTL"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.DisbursementTransactionType.Name))
      {
        var field = GetField(export.DisbursementTransactionType, "name");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (Equal(export.DisbursementTransactionType.EffectiveDate, null))
      {
        export.DisbursementTransactionType.EffectiveDate = Now().Date;
      }

      if (Equal(export.DisbursementTransactionType.DiscontinueDate, null))
      {
        export.DisbursementTransactionType.DiscontinueDate =
          new DateTime(2099, 12, 31);
      }

      if (Equal(global.Command, "ADD"))
      {
        if (Lt(export.DisbursementTransactionType.DiscontinueDate, Now().Date))
        {
          var field =
            GetField(export.DisbursementTransactionType, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }

        if (Lt(export.DisbursementTransactionType.EffectiveDate, Now().Date))
        {
          var field =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (!Lt(export.DisbursementTransactionType.EffectiveDate,
          export.DisbursementTransactionType.DiscontinueDate))
        {
          var field =
            GetField(export.DisbursementTransactionType, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          return;
        }
      }
    }

    // ---------------------------------------------
    //            Main CASE OF COMMAND.
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.DisbursementTransactionType.Code =
          export.DisbursementTransactionType.Code;
        UseFnReadDisbTransactionType();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.DisbursementTransactionType, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementTransactionType.DiscontinueDate, Now().Date) &&
            !Equal(export.DisbursementTransactionType.DiscontinueDate, null))
          {
            var field4 =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTransactionType, "discontinueDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.DisbursementTransactionType, "description");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.DisbursementTransactionType, "name");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          if (Lt(export.DisbursementTransactionType.EffectiveDate, Now().Date))
          {
            var field4 = GetField(export.DisbursementTransactionType, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          // Set the hidden key field to that of the new record.
          export.HiddenId.Assign(export.DisbursementTransactionType);

          // If Discontinue date contain maximum date then display blank 
          // instead.
          if (Equal(export.DisbursementTransactionType.DiscontinueDate,
            local.Max.Date))
          {
            export.DisbursementTransactionType.DiscontinueDate = local.Min.Date;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          export.DisbursementTransactionType.Code =
            local.DisbursementTransactionType.Code;

          var field = GetField(export.DisbursementTransactionType, "code");

          field.Error = true;

          export.DisbursementTransactionType.Description =
            Spaces(DisbursementTransactionType.Description_MaxLength);
          export.DisbursementTransactionType.Name = "";
          export.DisbursementTransactionType.DiscontinueDate = null;
          export.DisbursementTransactionType.EffectiveDate = null;

          // Set the hidden key field to spaces or zero.
          export.HiddenId.SystemGeneratedIdentifier = 0;
        }

        export.PromptTextWorkArea.Text1 = "+";

        return;
      case "ADD":
        UseFnCreateDisbursementTranType();

        if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field4 =
            GetField(export.DisbursementTransactionType, "discontinueDate");

          field4.Error = true;

          var field5 =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field5.Error = true;

          if (export.DisbursementTransactionType.SystemGeneratedIdentifier != 0)
          {
            var field = GetField(export.DisbursementTransactionType, "code");

            field.Color = "cyan";
            field.Protected = true;
          }

          return;
        }
        else if (IsExitState("EFFECTIVE_DATE_PRIOR_TO_CURRENT"))
        {
          var field =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field.Error = true;

          return;
        }
        else if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
        {
          var field4 = GetField(export.DisbursementTransactionType, "code");

          field4.Error = true;

          var field5 = GetField(export.DisbursementTransactionType, "name");

          field5.Error = true;

          return;
        }
        else if (IsExitState("FN0000_DISB_TRANS_TYP_AE"))
        {
          var field = GetField(export.DisbursementTransactionType, "code");

          field.Error = true;

          return;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenId.Assign(export.DisbursementTransactionType);

          var field = GetField(export.DisbursementTransactionType, "code");

          field.Color = "cyan";
          field.Protected = true;

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
        }

        return;
      case "UPDATE":
        var field1 = GetField(export.DisbursementTransactionType, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        if (Equal(export.DisbursementTransactionType.DiscontinueDate,
          export.HiddenId.DiscontinueDate) && Lt
          (export.DisbursementTransactionType.DiscontinueDate, Now().Date))
        {
          var field4 = GetField(export.DisbursementTransactionType, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.DisbursementTransactionType, "description");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 =
            GetField(export.DisbursementTransactionType, "discontinueDate");

          field7.Color = "cyan";
          field7.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.DisbursementTransactionType.DiscontinueDate,
          export.HiddenId.DiscontinueDate) || !
          Equal(export.DisbursementTransactionType.EffectiveDate,
          export.HiddenId.EffectiveDate))
        {
          if (Equal(export.DisbursementTransactionType.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.DisbursementTransactionType.EffectiveDate, Now().Date))
          {
            var field4 = GetField(export.DisbursementTransactionType, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (!Equal(export.DisbursementTransactionType.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.DisbursementTransactionType.EffectiveDate, Now().Date))
          {
            var field =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.DisbursementTransactionType.DiscontinueDate, Now().Date))
            
          {
            var field =
              GetField(export.DisbursementTransactionType, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.DisbursementTransactionType.EffectiveDate,
            export.DisbursementTransactionType.DiscontinueDate))
          {
            var field =
              GetField(export.DisbursementTransactionType, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        UseFnUpdateDisbursementTranType();

        var field2 = GetField(export.DisbursementTransactionType, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field4 =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field4.Error = true;

          var field5 =
            GetField(export.DisbursementTransactionType, "discontinueDate");

          field5.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
        }

        if (Lt(export.DisbursementTransactionType.EffectiveDate, Now().Date) &&
          Equal
          (export.DisbursementTransactionType.EffectiveDate,
          export.HiddenId.EffectiveDate))
        {
          var field4 = GetField(export.DisbursementTransactionType, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        return;
      case "DELETE":
        var field3 = GetField(export.DisbursementTransactionType, "code");

        field3.Color = "cyan";
        field3.Protected = true;

        if (Equal(export.DisbursementTransactionType.DiscontinueDate, null))
        {
        }
        else if (Lt(export.DisbursementTransactionType.DiscontinueDate,
          Now().Date))
        {
          var field4 = GetField(export.DisbursementTransactionType, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.DisbursementTransactionType, "discontinueDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 =
            GetField(export.DisbursementTransactionType, "description");

          field7.Color = "cyan";
          field7.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        if (Equal(export.DisbursementTransactionType.EffectiveDate, null))
        {
        }
        else if (Lt(export.DisbursementTransactionType.EffectiveDate, Now().Date))
          
        {
          var field4 = GetField(export.DisbursementTransactionType, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.DisbursementTransactionType, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        UseFnDeleteDisbursementTranType();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -------------------------
          // Set the hidden key field to spaces and blank out the screen.
          // -------------------------
          export.DisbursementTransactionType.Assign(local.Blank);
          export.HiddenId.Assign(local.Blank);

          var field = GetField(export.DisbursementTransactionType, "code");

          field.Color = "";
          field.Protected = false;

          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else
        {
        }

        return;
      case "LIST":
        if (AsChar(export.PromptTextWorkArea.Text1) == 'S')
        {
          export.HiddenId.Assign(export.DisbursementTransactionType);
          export.PromptTextWorkArea.Text1 = "+";
          ExitState = "ECO_LNK_TO_LST_DISB_TRAN_TYPE";

          return;
        }
        else
        {
          if (Lt(export.DisbursementTransactionType.EffectiveDate, Now().Date) &&
            export.DisbursementTransactionType.SystemGeneratedIdentifier != 0)
          {
            var field4 = GetField(export.DisbursementTransactionType, "code");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementTransactionType, "name");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (Lt(export.DisbursementTransactionType.DiscontinueDate, Now().Date) &&
            !
            Equal(export.DisbursementTransactionType.DiscontinueDate, null) && export
            .DisbursementTransactionType.SystemGeneratedIdentifier != 0)
          {
            var field4 = GetField(export.DisbursementTransactionType, "code");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementTransactionType, "name");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.DisbursementTransactionType, "discontinueDate");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.DisbursementTransactionType, "description");

            field8.Color = "cyan";
            field8.Protected = true;
          }

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          var field = GetField(export.PromptTextWorkArea, "text1");

          field.Error = true;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        if (export.DisbursementTransactionType.SystemGeneratedIdentifier != 0)
        {
          var field = GetField(export.DisbursementTransactionType, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementTransactionType.DiscontinueDate, Now().Date) &&
            !Equal(export.DisbursementTransactionType.DiscontinueDate, null))
          {
            var field4 = GetField(export.DisbursementTransactionType, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.DisbursementTransactionType, "discontinueDate");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.DisbursementTransactionType, "description");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          if (Lt(export.DisbursementTransactionType.EffectiveDate, Now().Date))
          {
            var field4 = GetField(export.DisbursementTransactionType, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTransactionType, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(export.DisbursementTransactionType.DiscontinueDate, local.Max.Date))
      
    {
      export.DisbursementTransactionType.DiscontinueDate = local.Min.Date;
    }
  }

  private static void MoveDisbursementTransactionType1(
    DisbursementTransactionType source, DisbursementTransactionType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveDisbursementTransactionType2(
    DisbursementTransactionType source, DisbursementTransactionType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveTypeStatusAudit1(TypeStatusAudit source,
    TypeStatusAudit target)
  {
    target.StringOfOthers = source.StringOfOthers;
    target.TableName = source.TableName;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private static void MoveTypeStatusAudit2(TypeStatusAudit source,
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

  private void UseFnCreateDisbursementTranType()
  {
    var useImport = new FnCreateDisbursementTranType.Import();
    var useExport = new FnCreateDisbursementTranType.Export();

    useImport.DisbursementTransactionType.Assign(
      export.DisbursementTransactionType);

    Call(FnCreateDisbursementTranType.Execute, useImport, useExport);

    MoveDisbursementTransactionType1(useExport.DisbursementTransactionType,
      export.DisbursementTransactionType);
  }

  private void UseFnDeleteDisbursementTranType()
  {
    var useImport = new FnDeleteDisbursementTranType.Import();
    var useExport = new FnDeleteDisbursementTranType.Export();

    useImport.DisbursementTransactionType.Assign(
      import.DisbursementTransactionType);

    Call(FnDeleteDisbursementTranType.Execute, useImport, useExport);

    MoveTypeStatusAudit2(useExport.TypeStatusAudit, local.TypeStatusAudit);
  }

  private void UseFnReadDisbTransactionType()
  {
    var useImport = new FnReadDisbTransactionType.Import();
    var useExport = new FnReadDisbTransactionType.Export();

    useImport.Flag.Flag = import.Flag.Flag;
    useImport.DisbursementTransactionType.Assign(
      export.DisbursementTransactionType);

    Call(FnReadDisbTransactionType.Execute, useImport, useExport);

    export.DisbursementTransactionType.Assign(
      useExport.DisbursementTransactionType);
  }

  private void UseFnUpdateDisbursementTranType()
  {
    var useImport = new FnUpdateDisbursementTranType.Import();
    var useExport = new FnUpdateDisbursementTranType.Export();

    useImport.DisbursementTransactionType.Assign(
      import.DisbursementTransactionType);

    Call(FnUpdateDisbursementTranType.Execute, useImport, useExport);

    MoveTypeStatusAudit1(useExport.TypeStatusAudit, local.TypeStatusAudit);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.ExportHidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.ExportHidden);

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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public CsePerson Hidden
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
    /// A value of PromptCommon.
    /// </summary>
    [JsonPropertyName("promptCommon")]
    public Common PromptCommon
    {
      get => promptCommon ??= new();
      set => promptCommon = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public DisbursementTransactionType HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    /// <summary>
    /// A value of FlowSelection.
    /// </summary>
    [JsonPropertyName("flowSelection")]
    public DisbursementTransactionType FlowSelection
    {
      get => flowSelection ??= new();
      set => flowSelection = value;
    }

    private Common flag;
    private TextWorkArea promptTextWorkArea;
    private CsePerson hidden;
    private Standard standard;
    private Common promptCommon;
    private DisbursementTransactionType disbursementTransactionType;
    private DisbursementTransactionType hiddenId;
    private DisbursementTransactionType flowSelection;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public DisbursementTransactionType HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    private TextWorkArea promptTextWorkArea;
    private Standard standard;
    private Common promptCommon;
    private DisbursementTransactionType disbursementTransactionType;
    private DisbursementTransactionType hiddenId;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    /// <summary>
    /// A value of ExportHidden.
    /// </summary>
    [JsonPropertyName("exportHidden")]
    public NextTranInfo ExportHidden
    {
      get => exportHidden ??= new();
      set => exportHidden = value;
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
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DisbursementTransactionType Blank
    {
      get => blank ??= new();
      set => blank = value;
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

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
    private NextTranInfo exportHidden;
    private Standard standard;
    private TypeStatusAudit typeStatusAudit;
    private DisbursementTransactionType blank;
    private DateWorkArea min;
    private DateWorkArea max;
  }
#endregion
}

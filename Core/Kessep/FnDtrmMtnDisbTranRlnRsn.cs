// Program: FN_DTRM_MTN_DISB_TRAN_RLN_RSN, ID: 371834720, model: 746.
// Short name: SWEDTRMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DTRM_MTN_DISB_TRAN_RLN_RSN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDtrmMtnDisbTranRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DTRM_MTN_DISB_TRAN_RLN_RSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDtrmMtnDisbTranRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDtrmMtnDisbTranRlnRsn.
  /// </summary>
  public FnDtrmMtnDisbTranRlnRsn(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // PR 133601 added the update check for the security cab. 12-22-2001. L. 
    // Bachura
    // ---------------------
    // Set initial EXIT STATE.
    // ---------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // ------------------------
    // Move all IMPORTs to EXPORTs.
    // ------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.PromptTextWorkArea.Text1 = import.PromptTextWorkArea.Text1;
    export.Standard.NextTransaction = import.Standard1.NextTransaction;
    export.DisbursementTranRlnRsn.Assign(import.DisbursementTranRlnRsn);
    export.TypeStatusAudit.Assign(import.TypeStatusAudit);
    export.HiddenId.Assign(import.HiddenId);

    // ------------------------------------
    // Check return data.
    // ------------------------------------
    if (Equal(global.Command, "RETDTRL"))
    {
      if (IsEmpty(import.DisbursementTranRlnRsn.Code))
      {
        MoveDisbursementTranRlnRsn1(export.HiddenId,
          export.DisbursementTranRlnRsn);
      }

      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // The logic assumes that a record cannot be UPDATEd or DELETEd without
    // first being displayed. Therefore, a key change with either command is 
    // invalid.
    // ---------------------------------------------
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

    // -----------------
    // Note if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. Now validate.
    // -----------------
    if (IsEmpty(import.Standard1.NextTransaction))
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

    if ((Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE")) &&
      !Equal(import.DisbursementTranRlnRsn.Code, import.HiddenId.Code))
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field = GetField(export.DisbursementTranRlnRsn, "code");

      field.Error = true;

      return;
    }

    // If the key field is blank, certain commands are not allowed.
    if ((Equal(global.Command, "DELETE") || IsEmpty(global.Command) || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE")) && IsEmpty
      (export.DisbursementTranRlnRsn.Code))
    {
      var field = GetField(export.DisbursementTranRlnRsn, "code");

      field.Error = true;

      export.HiddenId.Code = "";
      export.HiddenId.SystemGeneratedIdentifier = 0;
      export.DisbursementTranRlnRsn.Description =
        Spaces(DisbursementTranRlnRsn.Description_MaxLength);
      export.DisbursementTranRlnRsn.Name = "";
      export.DisbursementTranRlnRsn.DiscontinueDate = null;
      export.DisbursementTranRlnRsn.EffectiveDate = null;
      export.PromptTextWorkArea.Text1 = "+";
      ExitState = "KEY_FIELD_IS_BLANK";

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // =================================
    //       Main CASE OF COMMAND.
    // =================================
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // Calls the display module.
        local.Blank.Code = export.DisbursementTranRlnRsn.Code;
        UseFnReadDisbTranRlnRsn();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.DisbursementTranRlnRsn.Code = local.Blank.Code;

          // Set the hidden key field to that of the new record.
          export.HiddenId.Assign(export.DisbursementTranRlnRsn);

          var field = GetField(export.DisbursementTranRlnRsn, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementTranRlnRsn.EffectiveDate, Now().Date))
          {
            var field4 = GetField(export.DisbursementTranRlnRsn, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (Lt(export.DisbursementTranRlnRsn.DiscontinueDate, Now().Date))
          {
            var field4 = GetField(export.DisbursementTranRlnRsn, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementTranRlnRsn, "description");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.DisbursementTranRlnRsn, "discontinueDate");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          // If Discontinue date contains maximum date then display blank 
          // instead
          if (Equal(export.DisbursementTranRlnRsn.DiscontinueDate,
            local.Max.Date))
          {
            export.DisbursementTranRlnRsn.DiscontinueDate = local.Min.Date;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenId.Code = "";
          export.DisbursementTranRlnRsn.Code = local.Blank.Code;

          var field = GetField(export.DisbursementTranRlnRsn, "code");

          field.Error = true;

          export.DisbursementTranRlnRsn.Description =
            Spaces(DisbursementTranRlnRsn.Description_MaxLength);
          export.DisbursementTranRlnRsn.Name = "";
          export.DisbursementTranRlnRsn.DiscontinueDate = null;
          export.DisbursementTranRlnRsn.EffectiveDate = null;
        }

        export.PromptTextWorkArea.Text1 = "+";

        break;
      case "ADD":
        if (IsEmpty(export.DisbursementTranRlnRsn.Name))
        {
          var field = GetField(export.DisbursementTranRlnRsn, "name");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (Equal(export.DisbursementTranRlnRsn.EffectiveDate, null))
        {
          export.DisbursementTranRlnRsn.EffectiveDate = Now().Date;
        }

        if (Equal(export.DisbursementTranRlnRsn.DiscontinueDate, null))
        {
          export.DisbursementTranRlnRsn.DiscontinueDate =
            new DateTime(2099, 12, 31);
        }

        if (Lt(export.DisbursementTranRlnRsn.EffectiveDate, Now().Date))
        {
          var field = GetField(export.DisbursementTranRlnRsn, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (!Lt(Now().Date, export.DisbursementTranRlnRsn.DiscontinueDate))
        {
          var field =
            GetField(export.DisbursementTranRlnRsn, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

          return;
        }

        if (!Lt(export.DisbursementTranRlnRsn.EffectiveDate,
          export.DisbursementTranRlnRsn.DiscontinueDate))
        {
          var field =
            GetField(export.DisbursementTranRlnRsn, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          return;
        }

        // Calls the create module.
        UseFnCreDisbursementTranRlnRsn();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenId.Assign(export.DisbursementTranRlnRsn);

          var field = GetField(export.DisbursementTranRlnRsn, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Equal(export.DisbursementTranRlnRsn.DiscontinueDate,
            local.Max.Date))
          {
            export.DisbursementTranRlnRsn.DiscontinueDate = null;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          if (export.DisbursementTranRlnRsn.SystemGeneratedIdentifier != 0)
          {
            var field = GetField(export.DisbursementTranRlnRsn, "code");

            field.Color = "cyan";
            field.Protected = true;
          }

          var field4 = GetField(export.DisbursementTranRlnRsn, "effectiveDate");

          field4.Error = true;

          var field5 =
            GetField(export.DisbursementTranRlnRsn, "discontinueDate");

          field5.Error = true;
        }
        else
        {
        }

        break;
      case "UPDATE":
        if (Equal(export.DisbursementTranRlnRsn.DiscontinueDate, null))
        {
          export.DisbursementTranRlnRsn.DiscontinueDate =
            new DateTime(2099, 12, 31);
        }

        if (Equal(export.DisbursementTranRlnRsn.EffectiveDate, null))
        {
          export.DisbursementTranRlnRsn.EffectiveDate = Now().Date;
        }

        var field1 = GetField(export.DisbursementTranRlnRsn, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        if (Equal(export.DisbursementTranRlnRsn.DiscontinueDate,
          export.HiddenId.DiscontinueDate) && Lt
          (export.DisbursementTranRlnRsn.DiscontinueDate, Now().Date))
        {
          var field4 = GetField(export.DisbursementTranRlnRsn, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementTranRlnRsn, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.DisbursementTranRlnRsn, "discontinueDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.DisbursementTranRlnRsn, "description");

          field7.Color = "cyan";
          field7.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.DisbursementTranRlnRsn.DiscontinueDate,
          export.HiddenId.DiscontinueDate) || !
          Equal(export.DisbursementTranRlnRsn.EffectiveDate,
          export.HiddenId.EffectiveDate))
        {
          if (Equal(export.DisbursementTranRlnRsn.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.DisbursementTranRlnRsn.EffectiveDate, Now().Date))
          {
            var field4 = GetField(export.DisbursementTranRlnRsn, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (!Equal(export.DisbursementTranRlnRsn.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.DisbursementTranRlnRsn.EffectiveDate, Now().Date))
          {
            var field =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (Lt(export.DisbursementTranRlnRsn.DiscontinueDate, Now().Date))
          {
            var field =
              GetField(export.DisbursementTranRlnRsn, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.DisbursementTranRlnRsn.EffectiveDate,
            export.DisbursementTranRlnRsn.DiscontinueDate))
          {
            var field =
              GetField(export.DisbursementTranRlnRsn, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        // ------------------------------
        // Calls the update module.
        // ------------------------------
        UseFnUpdDisbursementTranRlnRsn();

        var field2 = GetField(export.DisbursementTranRlnRsn, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // --------------------------
          // Set the hidden key field to that of the new record.
          // --------------------------
          export.HiddenId.Assign(export.DisbursementTranRlnRsn);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field4 = GetField(export.DisbursementTranRlnRsn, "effectiveDate");

          field4.Error = true;

          var field5 =
            GetField(export.DisbursementTranRlnRsn, "discontinueDate");

          field5.Error = true;
        }
        else
        {
        }

        if (Lt(export.DisbursementTranRlnRsn.EffectiveDate, Now().Date) && Equal
          (export.DisbursementTranRlnRsn.EffectiveDate,
          export.HiddenId.EffectiveDate))
        {
          var field4 = GetField(export.DisbursementTranRlnRsn, "effectiveDate");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementTranRlnRsn, "name");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        break;
      case "DELETE":
        var field3 = GetField(export.DisbursementTranRlnRsn, "code");

        field3.Color = "cyan";
        field3.Protected = true;

        if (Equal(export.DisbursementTranRlnRsn.DiscontinueDate, null))
        {
        }
        else if (Lt(export.DisbursementTranRlnRsn.DiscontinueDate, Now().Date))
        {
          var field4 = GetField(export.DisbursementTranRlnRsn, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementTranRlnRsn, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.DisbursementTranRlnRsn, "discontinueDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.DisbursementTranRlnRsn, "description");

          field7.Color = "cyan";
          field7.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        if (Equal(export.DisbursementTranRlnRsn.EffectiveDate, null))
        {
        }
        else if (Lt(export.DisbursementTranRlnRsn.EffectiveDate, Now().Date))
        {
          var field4 = GetField(export.DisbursementTranRlnRsn, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementTranRlnRsn, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        // -----------------
        // Calls the delete module.
        // -----------------
        UseFnDelDisbursementTranRlnRsn();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -----------------------------------------
          // Set the hidden key field to spaces or zero.
          // -----------------------------------------
          export.DisbursementTranRlnRsn.Assign(local.Blank);
          export.HiddenId.Assign(local.Blank);

          // -----------------------------------------
          // Set the hidden key field to spaces or zero.
          // -----------------------------------------
          export.DisbursementTranRlnRsn.Description =
            Spaces(DisbursementTranRlnRsn.Description_MaxLength);
          export.DisbursementTranRlnRsn.Name = "";
          export.DisbursementTranRlnRsn.DiscontinueDate = null;
          export.DisbursementTranRlnRsn.EffectiveDate = null;

          var field = GetField(export.DisbursementTranRlnRsn, "code");

          field.Color = "";
          field.Protected = false;

          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else
        {
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        if (AsChar(export.PromptTextWorkArea.Text1) == 'S')
        {
          export.PromptTextWorkArea.Text1 = "+";
          ExitState = "ECO_LNK_TO_LST_DISB_TRAN_REL_RSN";
        }
        else
        {
          if (Lt(export.DisbursementTranRlnRsn.EffectiveDate, Now().Date) && !
            Equal(export.DisbursementTranRlnRsn.EffectiveDate, null))
          {
            var field4 = GetField(export.DisbursementTranRlnRsn, "code");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementTranRlnRsn, "name");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (Lt(export.DisbursementTranRlnRsn.DiscontinueDate, Now().Date) && !
            Equal(export.DisbursementTranRlnRsn.DiscontinueDate, null))
          {
            var field4 = GetField(export.DisbursementTranRlnRsn, "code");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementTranRlnRsn, "name");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementTranRlnRsn, "description");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.DisbursementTranRlnRsn, "discontinueDate");

            field8.Color = "cyan";
            field8.Protected = true;
          }

          var field = GetField(export.PromptTextWorkArea, "text1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        if (export.DisbursementTranRlnRsn.SystemGeneratedIdentifier != 0)
        {
          var field = GetField(export.DisbursementTranRlnRsn, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementTranRlnRsn.DiscontinueDate, Now().Date) && !
            Equal(export.DisbursementTranRlnRsn.DiscontinueDate, null))
          {
            var field4 = GetField(export.DisbursementTranRlnRsn, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementTranRlnRsn, "description");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.DisbursementTranRlnRsn, "discontinueDate");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          if (Lt(export.DisbursementTranRlnRsn.EffectiveDate, Now().Date))
          {
            var field4 = GetField(export.DisbursementTranRlnRsn, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.DisbursementTranRlnRsn, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveDisbursementTranRlnRsn1(DisbursementTranRlnRsn source,
    DisbursementTranRlnRsn target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveDisbursementTranRlnRsn2(DisbursementTranRlnRsn source,
    DisbursementTranRlnRsn target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
    target.Description = source.Description;
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

  private void UseFnCreDisbursementTranRlnRsn()
  {
    var useImport = new FnCreDisbursementTranRlnRsn.Import();
    var useExport = new FnCreDisbursementTranRlnRsn.Export();

    useImport.DisbursementTranRlnRsn.Assign(export.DisbursementTranRlnRsn);

    Call(FnCreDisbursementTranRlnRsn.Execute, useImport, useExport);

    MoveDisbursementTranRlnRsn2(useExport.DisbursementTranRlnRsn,
      export.DisbursementTranRlnRsn);
  }

  private void UseFnDelDisbursementTranRlnRsn()
  {
    var useImport = new FnDelDisbursementTranRlnRsn.Import();
    var useExport = new FnDelDisbursementTranRlnRsn.Export();

    useImport.DisbursementTranRlnRsn.Assign(export.DisbursementTranRlnRsn);

    Call(FnDelDisbursementTranRlnRsn.Execute, useImport, useExport);

    MoveTypeStatusAudit(useExport.TypeStatusAudit, export.TypeStatusAudit);
  }

  private void UseFnReadDisbTranRlnRsn()
  {
    var useImport = new FnReadDisbTranRlnRsn.Import();
    var useExport = new FnReadDisbTranRlnRsn.Export();

    useImport.Flag.Flag = import.Flag.Flag;
    useImport.DisbursementTranRlnRsn.Assign(export.DisbursementTranRlnRsn);

    Call(FnReadDisbTranRlnRsn.Execute, useImport, useExport);

    export.DisbursementTranRlnRsn.Assign(useExport.DisbursementTranRlnRsn);
  }

  private void UseFnUpdDisbursementTranRlnRsn()
  {
    var useImport = new FnUpdDisbursementTranRlnRsn.Import();
    var useExport = new FnUpdDisbursementTranRlnRsn.Export();

    useImport.DisbursementTranRlnRsn.Assign(export.DisbursementTranRlnRsn);

    Call(FnUpdDisbursementTranRlnRsn.Execute, useImport, useExport);

    export.TypeStatusAudit.Assign(useExport.TypeStatusAudit);
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

    useImport.Standard.NextTransaction = import.Standard1.NextTransaction;
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
    /// A value of Standard1.
    /// </summary>
    [JsonPropertyName("standard1")]
    public Standard Standard1
    {
      get => standard1 ??= new();
      set => standard1 = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Common Hidden
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
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
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
    public DisbursementTranRlnRsn HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    /// <summary>
    /// A value of Standard2.
    /// </summary>
    [JsonPropertyName("standard2")]
    public Standard Standard2
    {
      get => standard2 ??= new();
      set => standard2 = value;
    }

    private TextWorkArea promptTextWorkArea;
    private Common flag;
    private Standard standard1;
    private Common hidden;
    private Common promptCommon;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
    private TypeStatusAudit typeStatusAudit;
    private DisbursementTranRlnRsn hiddenId;
    private Standard standard2;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Common Hidden
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
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
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
    public DisbursementTranRlnRsn HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    private TextWorkArea promptTextWorkArea;
    private Standard standard;
    private Common hidden;
    private Common promptCommon;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
    private TypeStatusAudit typeStatusAudit;
    private DisbursementTranRlnRsn hiddenId;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DisbursementTranRlnRsn Blank
    {
      get => blank ??= new();
      set => blank = value;
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

    private DateWorkArea max;
    private DateWorkArea min;
    private DisbursementTranRlnRsn blank;
    private NextTranInfo exportHidden;
  }
#endregion
}
